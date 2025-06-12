using System.Web.Http;

namespace CargoWise.eHub.Products.GBCustoms.TL.InboundMessageWebService
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
		}
	}
}
