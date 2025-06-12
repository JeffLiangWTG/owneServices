using System.Web.Http;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService
{
    public static class WebApiConfig
    {
		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{controller}/{action}/{id}",
				defaults: new { id = RouteParameter.Optional });
		}
	}
}
