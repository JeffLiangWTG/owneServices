using System.Web.Http;

namespace CargoWise.eHub.Portal
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			config.MapHttpAttributeRoutes();
		}
    }
}
