using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices
{
	public class HttpXmlServiceConfigTest : TestCase
	{
		public void TestGlowDocumentRequestHandlerShouldNotMatchAnyHttpRoutes()
		{
			var config = new HttpConfiguration();
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
			WebApiServiceConfig.Register(config);
			config.EnsureInitialized();
			foreach (var httpRoute in config.Routes)
			{
				var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/GlowDocumentRequestHandler.axd");
				request.Properties[HttpPropertyKeys.HttpRouteDataKey] = httpRoute.GetRouteData("/", request);
				request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

				var dispatcher = new HttpControllerDispatcher(config);
				var invoker = new HttpMessageInvoker(dispatcher);

				var responseMessage = invoker.SendAsync(request, CancellationToken.None).Result.Content.ReadAsStringAsync().Result;
				Assert(@"If response contains this message, it means GlowDocumentRequestHandler matches urltemplate of one of httpRoute, it shouldn't", !responseMessage.Contains("No type was found that matches the controller named"));
			}
		}
	}
}
