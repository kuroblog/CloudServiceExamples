
namespace AzureCloudService.GettingStarted.WebRole.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Utils.DTOs;
    using Utils.Extensions;

    public partial class LogsController
    {
        //[HttpPost, Route("~/api/v2/logs")]
        //public async Task<IHttpActionResult> CreateLogV2([FromBody]LogDto log)
        //{
        //    var verifyResult = log.VerifyAll();
        //    if (verifyResult != null)
        //    {
        //        return Content(HttpStatusCode.BadRequest, log.VerifyAll().ToList().Where(p => p.IsValid == false));
        //    }

        //    return await Task.FromResult(Content(HttpStatusCode.Created, $"{nameof(HttpStatusCode.Created)}"));
        //}
    }
}