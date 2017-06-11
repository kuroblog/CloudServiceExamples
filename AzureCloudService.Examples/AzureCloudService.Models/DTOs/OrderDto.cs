
namespace AzureCloudService.Utils.DTOs
{
    using Infrastructures;
    using System;
    using System.ComponentModel.DataAnnotations;
    using Types;

    public class OrderDto : IBaseModel
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Content { get; set; }

        public SubOrderDto[] SubOrders { get; set; } = new SubOrderDto[] { };
    }

    public class SubOrderDto : IBaseModel
    {
        public string Id { get; set; }

        [Required]
        public OrderTypes Type { get; set; } = OrderTypes.Unknow;

        public string Content { get; set; }
    }
}
