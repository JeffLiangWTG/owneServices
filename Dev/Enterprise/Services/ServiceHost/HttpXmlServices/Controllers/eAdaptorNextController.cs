using System.Net.Http;
using System.Web.Http;
using Enterprise.Integration;

namespace Enterprise.Services.ServiceHost
{
	[eAdaptorNextEDIClientAuthenticationDecider]
	[RoutePrefix("eAdaptorNext")]
	public class eAdaptorNextController : eAdaptorControllerBase, IeAdaptorController
	{
		internal override IeAdaptorConfig DefaultConfig => eAdaptorNextConfig.Instance;

		[Route("")]
		[HttpGet]
		public HttpResponseMessage Get() => eAdaptorRequestProcessorCore.GetPlainTextResponse("<html><body><h1>Welcome to the eAdaptor Next Service</h1></body></html>");

		[Route("")]
		[HttpPost]
		public HttpResponseMessage Post()
		{
			return eAdaptorRequestProcessor.Post(Request, Config);
		}

		[Route("{messageType}")]
		[HttpPost]
		public HttpResponseMessage PostWithMessageType(string messageType)
		{
			if (eAdaptorHandlerFactory.handlers.TryGetValue(messageType, out var handlerName))
			{
				return eAdaptorRequestProcessor.Post(Request, Config, handlerName);
			}

			return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"{messageType} is not a valid message type") };
		}

		[Route("Async")]
		[HttpGet]
		public HttpResponseMessage GetAsynchronous() => eAdaptorRequestProcessorCore.GetPlainTextResponse("<html><body><h1>Welcome to the eAdaptor Next Async Service</h1></body></html>");

		[Route("Async")]
		[HttpPost]
		public HttpResponseMessage PostAsynchronous() => eAdaptorAsyncRequestProcessor.Post(Request, Config);
	}
}
