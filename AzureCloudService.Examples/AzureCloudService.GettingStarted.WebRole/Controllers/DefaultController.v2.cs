
namespace AzureCloudService.GettingStarted.WebRole.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    public partial class DefaultController
    {
        [HttpGet, Route("~/api/v2/default")]
        public async Task<IHttpActionResult> HelloV2()
        {
            return await Task.FromResult(Content(HttpStatusCode.OK, "Hello, V2!"));
        }
    }
}