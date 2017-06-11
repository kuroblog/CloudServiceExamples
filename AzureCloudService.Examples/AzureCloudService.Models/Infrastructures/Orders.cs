
namespace AzureCloudService.Utils.Infrastructures
{
    using DTOs;

    public interface IOrderWorker
    {
        bool CreateOrder(SubOrderDto subOrder);

        bool UpdateOrder(SubOrderDto subOrder);

        bool CancelOrder(SubOrderDto subOrder);
    }

    public interface IOrderCommonWorker
    {
        bool SplitOrder(OrderDto order);
    }
}
