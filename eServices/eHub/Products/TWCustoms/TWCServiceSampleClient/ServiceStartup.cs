using Owin;
using System.Web.Http;


//[assembly: OwinStartup(typeof(OwinAPI.Startup))]
namespace CargoWise.eHub.Products.TWCustoms.TWCServiceSampleClient
{
	class ServiceStartup
    {

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }

    }
}


