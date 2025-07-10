using System.Net.Http;
using System.Web.Http;
using Enterprise.Integration;

namespace Enterprise.Services.ServiceHost
{
	[Authorize]
	[eHubIdentityBasicAuthentication]
	[RoutePrefix("eAdaptor")]
	public class eAdaptorController : eAdaptorControllerBase, IeAdaptorController
	{
		internal override IeAdaptorConfig DefaultConfig => eAdaptorConfig.Instance;

		[Route("")]
		[HttpGet]
		public HttpResponseMessage Get() => eAdaptorRequestProcessorCore.GetPlainTextResponse("<html><body><h1>Welcome to the eAdaptor Service</h1></body></html>");

		[Route("")]
		[HttpPost]
		public HttpResponseMessage Post() => eAdaptorRequestProcessor.Post(Request, Config);
	}
}
