
namespace AzureCloudService.CastleWindsor.WorkerRole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using Utils.Infrastructures;
    using Castle.MicroKernel.Registration;
    using Utils.Types;
    using Utils.DTOs;
    using Utils.Extensions;
    using Utils;
    using Newtonsoft.Json;
    using Microsoft.ServiceBus;
    using Utils.Models;
    using Microsoft.WindowsAzure.Storage.Table;

    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("AzureCloudService.CastleWindsor.WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            //设置最大并发连接数
            ServicePointManager.DefaultConnectionLimit = 12;

            // 有关处理配置更改的信息，
            // 请在 https://go.microsoft.com/fwlink/?LinkId=166357 参阅 MSDN 主题。

            bool result = base.OnStart();

            Trace.TraceInformation("AzureCloudService.CastleWindsor.WorkerRole has been started");

            bootstarp.InstallComponents();

            AzureComponentInitialize();

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("AzureCloudService.CastleWindsor.WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("AzureCloudService.CastleWindsor.WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: 将以下逻辑替换为你自己的逻辑。
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");

                Parallel.Invoke(async () =>
                {
                    while (true)
                    {
                        var order = await ReceiveMessageFromQueue(queueName);
                        if (order != null)
                        {
                            var hasComponent = bootstarp.Container.Kernel.HasComponent(typeof(IOrderCommonWorker));
                            if (hasComponent)
                            {
                                var worker = bootstarp.Container.Resolve<IOrderCommonWorker>();
                                worker.SplitOrder(order);
                            }

                            order.SubOrders.ToList().ForEach(p =>
                            {
                                var typeName = p.Type.ToString();
                                hasComponent = bootstarp.Container.Kernel.HasComponent(typeName);
                                if (hasComponent)
                                {
                                    var worker = bootstarp.Container.Resolve<IOrderWorker>(typeName);
                                    worker.CreateOrder(p);
                                }
                            });
                        }

                        await Task.Delay(100);
                    }
                });

                await Task.Delay(1000);
            }
        }

        private readonly Bootstarp bootstarp = new Bootstarp();

        private string serviceBusConnectionString = string.Empty;
        private string queueName = string.Empty;

        private string storageConnectionString = string.Empty;
        //private string storageAccount = string.Empty;
        private string storageLogsTableName = string.Empty;

        private NamespaceManager namespaceManager = null;
        private CloudStorageAccount cloudStorageAccount = null;

        private CloudTable logTable = null;

        public void AzureComponentInitialize()
        {
#if DEBUG
            serviceBusConnectionString = @"Endpoint=sb://sb-test.servicebus.chinacloudapi.cn/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=MPY8Stj6vlIg2oSvN6VaAFy0Y25egKNg/s1qFQLjOXM=";
            queueName = "castle_windsor_test";

            storageConnectionString = @"DefaultEndpointsProtocol=https;AccountName=satest1;AccountKey=msu5juE/elGuN5CzMyduVw+3Rl6CqSQT8bZIfTy1Af2FNhU1JJwdaANsOicmeEbDf0eR3+d0l1S3nSoyCDH5MQ==;EndpointSuffix=core.chinacloudapi.cn";
            //storageAccount = "satest1";
            storageLogsTableName = "logs";
#else
            serviceBusConnectionString = Microsoft.Azure.CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            queueName = Microsoft.Azure.CloudConfigurationManager.GetSetting("QueueName");
            
            storageConnectionString = Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString");
            //storageAccount = Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageAccount");
            storageLogsTableName = Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageLogsTableName");
#endif

            namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
            cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);

            logTable = cloudStorageAccount.GetCloudTable(storageLogsTableName);
        }

        public async Task<OrderDto> ReceiveMessageFromQueue(string queueName)
        {
            OrderDto order = null;

            var count = namespaceManager.GetQueue(queueName).MessageCount;
            if (count <= 0)
            {
                return await Task.FromResult(order);
            }

            var queueClient = namespaceManager.GetQueueClient(serviceBusConnectionString, queueName);
            var queueMessage = queueClient.Receive();
            if (queueMessage == null)
            {
                return await Task.FromResult(order);
            }

            var jsonString = queueMessage.GetBody<string>();
            if (string.IsNullOrEmpty(jsonString))
            {
                return await Task.FromResult(order);
            }

            logTable.Create(jsonString, "Receive a message from queue.", "Worker");

            order = JsonConvert.DeserializeObject<OrderDto>(jsonString);

            return await Task.FromResult(order);
        }
    }

    public class Bootstarp
    {
        public WindsorContainer Container { get; } = new WindsorContainer();

        public Bootstarp()
        {
            Container.Install(FromAssembly.This());
        }

        public void InstallComponents()
        {
            Container.Register(Component.For<IRunner>().ImplementedBy<Runner>().LifestylePerThread());

            Container.Register(Component.For<IOrderCommonWorker>().ImplementedBy<OrderCommonWorker>().LifestylePerThread());

            Container.Register(Component.For<IOrderWorker>().ImplementedBy<PickupOrderWorker>().LifestylePerThread().Named($"{nameof(OrderTypes.Pickup)}"));
            Container.Register(Component.For<IOrderWorker>().ImplementedBy<DeliverOrderWorker>().LifestylePerThread().Named($"{nameof(OrderTypes.Deliver)}"));
            Container.Register(Component.For<IOrderWorker>().ImplementedBy<ViolationOrderWorker>().LifestylePerThread().Named($"{nameof(OrderTypes.Violation)}"));
        }
    }

    public class OrderCommonWorker : IOrderCommonWorker
    {
        private readonly IRunner runner = null;

        public OrderCommonWorker(IRunner runner)
        {
            this.runner = runner;
        }

        public bool SplitOrder(OrderDto order)
        {
            return runner.Execute(() =>
            {
                for (var i = 0; i < order.SubOrders.Length; i++)
                {
                    order.SubOrders[i].Id = $"{order.Id.ToString("N")}-{i + 1}";
                }

                Debug.WriteLine(JsonConvert.SerializeObject(order));

                return true;
            }, GetType().Name, nameof(SplitOrder));
        }
    }

    public class PickupOrderWorker : IOrderWorker
    {
        private readonly IRunner runner = null;

        public PickupOrderWorker(IRunner runner)
        {
            this.runner = runner;
        }

        public bool CreateOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(CreateOrder));
        }

        public bool UpdateOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(UpdateOrder));
        }

        public bool CancelOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(CancelOrder));
        }
    }

    public class DeliverOrderWorker : IOrderWorker
    {
        private readonly IRunner runner = null;

        public DeliverOrderWorker(IRunner runner)
        {
            this.runner = runner;
        }

        public bool CreateOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(CreateOrder));
        }

        public bool UpdateOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(UpdateOrder));
        }

        public bool CancelOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(CancelOrder));
        }
    }

    public class ViolationOrderWorker : IOrderWorker
    {
        private readonly IRunner runner = null;

        public ViolationOrderWorker(IRunner runner)
        {
            this.runner = runner;
        }

        public bool CreateOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(CreateOrder));
        }

        public bool UpdateOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(UpdateOrder));
        }

        public bool CancelOrder(SubOrderDto subOrder)
        {
            return runner.Execute(() =>
            {
                Thread.Sleep(100);
                return true;
            }, GetType().Name, nameof(CancelOrder));
        }
    }
}
