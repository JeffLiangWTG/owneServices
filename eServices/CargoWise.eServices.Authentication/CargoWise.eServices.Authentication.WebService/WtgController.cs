using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace CargoWise.eServices.Authentication.WebService
{
	public class WtgController : ApiController
	{
		[HttpGet]
		[HttpHead]
		[Route("wtg/status")]
		public HttpResponseMessage Status()
		{
			var documentText = new AuthenticationServiceHealthCheckHttpTaskAsyncHandler().GetDocumentText().Result;
			var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
			httpResponseMessage.Content = new StringContent(documentText, Encoding.UTF8, "text/plain");

			return httpResponseMessage;
		}
	}
}