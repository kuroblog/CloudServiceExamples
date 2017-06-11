
namespace AzureCloudService.CastleWindsor.WebRole.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Utils.DTOs;
    using Utils.Extensions;
    using Utils.Types;

    public class DeliverController : ApiController
    {
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

            return Content(HttpStatusCode.Created, order);
        }
    }
}
