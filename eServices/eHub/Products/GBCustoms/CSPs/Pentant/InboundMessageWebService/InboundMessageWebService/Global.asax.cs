using System.Web;
using System.Web.Http;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService
{
	public class WebApiApplication : HttpApplication
	{
		protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
		}
	}
}
