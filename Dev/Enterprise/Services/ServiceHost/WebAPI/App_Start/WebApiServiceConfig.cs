using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Enterprise.Services.ServiceHost
{
	public static class WebApiServiceConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();
			config.MessageHandlers.Add(new FeatureToggleHandler());
			config.MessageHandlers.Add(new InstanceIdHandler());
			config.Services.Add(typeof(IExceptionLogger), new WebApiServiceExceptionReporter());
		}
	}
}
