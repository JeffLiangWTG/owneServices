using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Web.Configuration
{
	/// <summary>
	/// Startup feature common to OWIN SelfHost and IIS hosting. They are invoked at separate locations respectively
	/// </summary>
	/// <remarks>Source: https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/configuring-aspnet-web-api</remarks>
	public static class SharedStartup
	{
		/// <summary>
		/// Core startup functions. Not specifically OWIN pipeline configuration
		/// </summary>
		/// <remarks>Invoked inline in the case of SelfHost, or via Global.asax.cs in IIS</remarks>
		public static void ConfigureAtStartup()
		{
			DbConnection.ApplicationName = "RatingWebServices";

			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
		}

		/// <summary>
		/// Configure WebApi
		/// </summary>
		/// <remarks>Invoked inline in the case of SelfHost, or via Global.asax.cs in IIS</remarks>
		public static void ConfigureWebApi(HttpConfiguration config, IRatesAPIsAppSettings configurations)
		{
			var serviceProvider = DependencyInjectionConfig.CreateServiceProvider();
			config.DependencyResolver = new DefaultDependencyResolver(serviceProvider);

			config.Services.Replace(typeof(IHttpControllerTypeResolver), new RatingWebControllerTypeResolver());

			config.MapHttpAttributeRoutes();
			config.ConfigureSwagger();

			/*
			 * Formatters are going to be used based on their order.  
			 * As Swagger examples can only presented in JSON, we have decided to set JSON as default media type (When enabled in registry).
			 */
			config.Formatters.Clear();

			if (configurations.SupportJsonMediaType)
			{
				config.Formatters.Add(new JsonMediaTypeFormatter());
			}

			config.Formatters.Add(new XmlMediaTypeFormatter());
		}
	}
}
