
namespace AzureCloudService.CastleWindsor.WebRole.Controllers
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Utils.DTOs;
    using Utils.Extensions;
    using Utils.Types;

    public class DeliverController : ApiController
    {
        private NamespaceManager namespaceManager = null;
        private string serviceBusConnectionString = string.Empty;
        private string queueName = string.Empty;

        public DeliverController()
        {
#if DEBUG
            serviceBusConnectionString = ConfigurationManager.AppSettings["serviceBusConnectionString"];
            queueName = ConfigurationManager.AppSettings["queueName"];

            //storageConnectionString = @"DefaultEndpointsProtocol=https;AccountName=satest1;AccountKey=msu5juE/elGuN5CzMyduVw+3Rl6CqSQT8bZIfTy1Af2FNhU1JJwdaANsOicmeEbDf0eR3+d0l1S3nSoyCDH5MQ==;EndpointSuffix=core.chinacloudapi.cn";
            //storageAccount = "satest1";
            //storageLogsTableName = "logs";
#else
            serviceBusConnectionString = Microsoft.Azure.CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            queueName = Microsoft.Azure.CloudConfigurationManager.GetSetting("QueueName");
            
            //storageConnectionString = Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString");
            //storageAccount = Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageAccount");
            //storageLogsTableName = Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageLogsTableName");
#endif

            namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
            //cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Create()
        {
            var content = await Request.Content.ReadAsStringAsync();

            var order = new OrderDto
            {
                Content = content,
                SubOrders = new SubOrderDto[] {
                    new SubOrderDto { Type = OrderTypes.Pickup },
                    new SubOrderDto { Type = OrderTypes.Deliver }
                }
            };

            var verifyResults = order.VerifyAll().Where(p => p.IsValid == false);
            if (verifyResults != null && verifyResults.Count() > 0)
            {
                return Content(HttpStatusCode.BadRequest, verifyResults);
            }

            var queueClient = namespaceManager.GetQueueClient(serviceBusConnectionString, queueName);

            var jsonString = JsonConvert.SerializeObject(order);
            var jsonDto = new BrokeredMessage(jsonString);
            queueClient.SendAsync(jsonDto).Wait();

            return Content(HttpStatusCode.Created, order);
        }
    }
}
