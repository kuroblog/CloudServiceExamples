
namespace AzureCloudService.GettingStarted.WebRole.Controllers
{
    using System.Configuration;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    public partial class DefaultController : ApiController
    {
        private int version = 0;

        public DefaultController()
        {
#if DEBUG
            var verValue = ConfigurationManager.AppSettings["DefaultControllerVersion"];
            int.TryParse(verValue, out version);
#else
#endif
        }

        [HttpGet]
        public async Task<IHttpActionResult> Hello()
        {
            switch (version)
            {
                case 2: return await HelloV2();
                case 1: return await HelloV1();
                default: return Content(HttpStatusCode.NoContent, $"{nameof(HttpStatusCode.NoContent)}");
            }
        }
    }
}
