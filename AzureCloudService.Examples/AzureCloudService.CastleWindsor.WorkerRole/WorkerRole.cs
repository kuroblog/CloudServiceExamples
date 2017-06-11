
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

                var order = new OrderDto
                {
                    Content = "test",
                    SubOrders = new SubOrderDto[] {
                        new SubOrderDto { Type = OrderTypes.Pickup },
                        new SubOrderDto { Type = OrderTypes.Deliver },
                        new SubOrderDto { Type = OrderTypes.Violation },
                        new SubOrderDto { Type = OrderTypes.Unknow }
                    }
                };

                order.SubOrders.ToList().ForEach(p =>
                {
                    var hasComponent = bootstarp.Container.Kernel.HasComponent(nameof(p.Type));
                    if (hasComponent)
                    {
                        var worker = bootstarp.Container.Resolve<IOrderWorker>(nameof(p.Type));
                        if (worker != null)
                        {
                            worker.CreateOrder(p);
                        }
                    }
                });

                await Task.Delay(1000);
            }
        }

        private readonly Bootstarp bootstarp = new Bootstarp();
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
            Container.Register(Component.For<IOrderWorker>().ImplementedBy<PickupOrderWorker>().LifestylePerThread().Named($"{nameof(OrderTypes.Pickup)}"));
            Container.Register(Component.For<IOrderWorker>().ImplementedBy<DeliverOrderWorker>().LifestylePerThread().Named($"{nameof(OrderTypes.Deliver)}"));
            Container.Register(Component.For<IOrderWorker>().ImplementedBy<ViolationOrderWorker>().LifestylePerThread().Named($"{nameof(OrderTypes.Violation)}"));
        }
    }

    public class PickupOrderWorker : IOrderWorker
    {
        public bool SplitOrder(SubOrderDto subOrder)
        {
            return this.Execute(SplitOrder, subOrder);
        }

        public bool CreateOrder(SubOrderDto subOrder)
        {
            return this.Execute(CreateOrder, subOrder);
        }

        public bool UpdateOrder(SubOrderDto subOrder)
        {
            return this.Execute(UpdateOrder, subOrder);
        }

        public bool CancelOrder(SubOrderDto subOrder)
        {
            return this.Execute(CancelOrder, subOrder);
        }
    }

    public class DeliverOrderWorker : IOrderWorker
    {
        public bool SplitOrder(SubOrderDto subOrder)
        {
            return this.Execute(SplitOrder, subOrder);
        }

        public bool CreateOrder(SubOrderDto subOrder)
        {
            return this.Execute(CreateOrder, subOrder);
        }

        public bool UpdateOrder(SubOrderDto subOrder)
        {
            return this.Execute(UpdateOrder, subOrder);
        }

        public bool CancelOrder(SubOrderDto subOrder)
        {
            return this.Execute(CancelOrder, subOrder);
        }
    }

    public class ViolationOrderWorker : IOrderWorker
    {
        public bool SplitOrder(SubOrderDto subOrder)
        {
            return this.Execute(SplitOrder, subOrder);
        }

        public bool CreateOrder(SubOrderDto subOrder)
        {
            return this.Execute(CreateOrder, subOrder);
        }

        public bool UpdateOrder(SubOrderDto subOrder)
        {
            return this.Execute(UpdateOrder, subOrder);
        }

        public bool CancelOrder(SubOrderDto subOrder)
        {
            return this.Execute(CancelOrder, subOrder);
        }
    }
}
