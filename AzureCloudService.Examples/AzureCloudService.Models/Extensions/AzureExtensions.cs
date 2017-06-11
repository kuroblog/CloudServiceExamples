
namespace AzureCloudService.Utils.Extensions
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Models;
    using System;
    using System.Diagnostics;

    public static class AzureExtensions
    {
        public static QueueClient GetQueueClient(this NamespaceManager namespaceManager, string serviceBusConnectionString, string queueName)
        {
            if (namespaceManager.QueueExists(queueName) == false)
            {
                namespaceManager.CreateQueue(queueName);
            }

            return QueueClient.CreateFromConnectionString(serviceBusConnectionString, queueName, ReceiveMode.ReceiveAndDelete);
        }

        public static CloudTable GetCloudTable(this CloudStorageAccount cloudStorageAccount, string storageLogsTableName)
        {
            var tableClient = cloudStorageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference(storageLogsTableName);
            table.CreateIfNotExists();

            return table;
        }

        public static void Create(this CloudTable table, string content, string message = "", string owner = "Unknow")
        {
            var log = new SaLogTable(Guid.NewGuid(), owner)
            {
                Content = content,
                Message = message
            };
            var operation = TableOperation.Insert(log);

            //var table = cloudStorageAccount.GetCloudTable(storageLogsTableName);

            try
            {
                var executeResult = table.Execute(operation);
                Debug.WriteLine(executeResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
