
namespace AzureCloudService.Utils.Models
{
    using Microsoft.WindowsAzure.Storage.Table;
    using System;

    public class SaLogTable : TableEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Owner { get; set; }

        public SaLogTable(Guid id, string owner)
        {
            PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd");
            RowKey = id.ToString();

            Id = id;
            Owner = owner;
        }
    }
}
