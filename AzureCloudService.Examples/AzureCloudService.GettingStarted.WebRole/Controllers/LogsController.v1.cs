
namespace AzureCloudService.GettingStarted.WebRole.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Utils.DTOs;
    using Utils.Extensions;

    public partial class LogsController
    {
        [HttpPost, Route("~/api/v1/logs")]
        public async Task<IHttpActionResult> CreateLogV1()
        {
            var content = await Request.Content.ReadAsStringAsync();

            var log = new LogDto
            {
                Owner = "Unknow",
                Content = content
            };

            var verifyResult = log.VerifyAll();
            if (verifyResult != null)
            {
                return Content(HttpStatusCode.BadRequest, log.VerifyAll().ToList().Where(p => p.IsValid == false));
            }

            return Content(HttpStatusCode.Created, $"{nameof(HttpStatusCode.Created)}");
        }
    }
}