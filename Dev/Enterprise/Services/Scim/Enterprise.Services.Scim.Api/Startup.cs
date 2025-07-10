using System;
using System.Net.Http.Headers;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Api.Auth;
using Enterprise.Services.Scim.Api.Config;
using Enterprise.Services.Scim.Api.Filters;
using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Api.Middlewares;
using Enterprise.Services.Scim.Business;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Owin;
using SimpleIdServer.Scim;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.Scim.Api
{
	/// <summary>
	/// Owin startup class.
	/// </summary>
	[CodeAlive("Using OwinStartup see web.config appSettings")]
	public class Startup
	{
		protected readonly IServiceCollection Services;
		protected IServiceCollection GetServices() => Services;

		public Startup()
		{
			Services = new ServiceCollection();
			DependencyInjectionConfig.RegisterTypes(Services);
		}

		/// <summary>
		/// Configuration method of owin startup.
		/// </summary>
		/// <param name="app"></param>
		public void Configuration(IAppBuilder app)
		{
			Argument.NotNull(app, nameof(app));
			var serviceProvider = CreateServiceProvider();
			var configurations = serviceProvider.GetRequiredService<IAppSettings>();

			bool.TryParse(System.Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat);
			if (!onDat)
			{
				CW1ApplicationInitialiser.Initialise(configurations);
			}

			IdentityModelEventSource.ShowPII = configurations.ShowPii;
			app.Use(typeof(ThrottlingMiddleware), configurations);

			try
			{
				if (SystemDataRegistry.Instance.ScimAuthenticationMethod.Value.Equals(ScimConstants.AuthenticationCodes.ApiToken))
				{
					app.Use(typeof(ApiTokenMiddleware), new TokenAuthenticationOptions());
				}
				else // If not using token authentication, use Azure Entra by default
				{
					ConfigureOAuthTokenConsumption(app, configurations);
				}
			}
			catch (DatabaseUpgradedException)
			{
			}

			if (!onDat && SystemDataRegistry.Instance.ScimSafeListType.Value.Equals(ScimConstants.SafelistTypes.AzureEntra))
			{
				var db = serviceProvider.GetRequiredService<IIPSafelistDBStore>();
				db.UpdateSafelistToDB();
			}

			app.UseWebApi(CreateWebApiConfig(serviceProvider));
			NLogConfig.ConfigNLog();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Route")]
		HttpConfiguration CreateWebApiConfig(IServiceProvider serviceProvider)
		{
			var config = new HttpConfiguration { DependencyResolver = new DefaultDependencyResolver(serviceProvider) };
			if (!Env.Instance.IsProductionSystem)
			{
				config.ConfigureSwagger();
			}

			config.Routes.MapHttpRoute(
				name: "ScimServiceApi",
				routeTemplate: "scim/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			config.MapHttpAttributeRoutes();

			var validator = serviceProvider.GetRequiredService<ISafelistValidator>();
			var appSettings = serviceProvider.GetRequiredService<IAppSettings>();

			config.Filters.Add(new ValidateIPAttribute(validator, appSettings));
			config.Filters.Add(new AuthorizeAttribute());
			config.Filters.Add(new ScimEnabledAttribute());
			config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(SCIMConstants.STANDARD_SCIM_CONTENT_TYPE));

			return config;
		}

		protected virtual IServiceProvider CreateServiceProvider()
		{
			return Services.BuildServiceProvider();
		}

		void ConfigureOAuthTokenConsumption(IAppBuilder app, IAppSettings configurations)
		{
#if DEBUG
			app.ConfigureDebugAuth();
#else
			var registry = Enterprise.Environment.Env.Registry;
			app.ConfigureAzureAdJwtAuth(registry.ScimIssuer, registry.ScimAudienceId, registry.ScimKnownEndpointPath);
#endif
		}
	}
}
