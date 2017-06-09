
namespace AzureCloudService.GettingStarted.WebRole.Controllers
{
    using System.Configuration;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    public partial class LogsController : ApiController
    {
        private int version = 0;

        public LogsController()
        {
#if DEBUG
            var verValue = ConfigurationManager.AppSettings["LogsControllerVersion"];
            int.TryParse(verValue, out version);
#else
#endif
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateLog()
        {
            switch (version)
            {
                //case 2: return await CreateLogV2();
                case 1: return await CreateLogV1();
                default: return Content(HttpStatusCode.NoContent, $"{nameof(HttpStatusCode.NoContent)}");
            }
        }
    }
}
