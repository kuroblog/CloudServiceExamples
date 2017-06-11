
namespace AzureCloudService.Utils.Infrastructures
{
    using DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IOrderWorker
    {
        bool CreateOrder(SubOrderDto subOrder);

        bool SplitOrder(SubOrderDto subOrder);

        bool UpdateOrder(SubOrderDto subOrder);

        bool CancelOrder(SubOrderDto subOrder);
    }
}
