using CargoWise.eHub.Portal.HealthCheck;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace CargoWise.eHub.Portal.Controllers
{
	[Authorize]
    public class WtgController : ApiController
    {
        [HttpGet]
		[HttpHead]
		[AllowAnonymous]
        [Route("wtg/status")]
        public HttpResponseMessage Status()
        {
            var documentText = new PortalHealthCheckHttpTaskAsyncHandler().GetDocumentText().Result;
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent(documentText, Encoding.UTF8, "text/plain");

            return httpResponseMessage;
        }
    }
}
