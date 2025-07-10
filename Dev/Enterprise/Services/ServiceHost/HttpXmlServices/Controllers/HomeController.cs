using System.Net.Http;
using System.Web.Http;

namespace Enterprise.Services.ServiceHost
{
	public class HomeController : ApiController
	{
		[Route]
		[Route("Home")]
		public HttpResponseMessage Get()
		{
			return eAdaptorRequestProcessorCore.GetPlainTextResponse("<html><body><h1>Welcome to CargoWiseOne Services</h1></body></html>");
		}
	}
}
