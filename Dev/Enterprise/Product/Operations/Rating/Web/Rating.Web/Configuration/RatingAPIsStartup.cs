using System.Web.Http;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Rating.Web.Authentication;
using Owin;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Configuration
{
	/// <summary>
	/// Owin startup class.
	/// </summary>
	[CodeAlive("Using OwinStartup see web.config appSettings")]
	public class RatingAPIsStartup
	{
		/// <summary>
		/// Configuration method of owin startup.
		/// </summary>
		public void Configuration(IAppBuilder app)
		{
			Argument.NotNull(app, nameof(app));

			if (!CW1ApplicationInitialiser.IsHostedInIIS())
			{
				SharedStartup.ConfigureAtStartup();
			}

			var configurations = ObjectFactory.New<IRatesAPIsAppSettings>();

			if (!CW1ApplicationInitialiser.IsHostedInIIS())
			{
				// This is handled by ZEnterpriseGlobal
				CW1ApplicationInitialiser.Initialise(configurations);

				/*
				 * To handle exceptions during Owin pipelines, we need a middleware.
				 * Our APIControllersPipelineExceptionHandler is not helpful here, because it is only useful during API controller lifecycle.
				 */
				app.Use<OwinPipelineExceptionHandler>(configurations);
			}

			/*
			MediaTypeValidationMiddleware is used because ASP.NET was ignoring Accept header
			when requested media type was not supported. Currenlty we just support application/xml (as default) and
			application/json (if configured) for main APIs (other than health-check or auth token endpoints).
			We wanted the client to get a clear message when requested media-type is not supported.

			We also tried to use DefaultContentNegotiator but when application/json is supported, this negotiator
			matches almost every media type with it, so for example even when requested media type was 'text/csv' we
			were returning json.

			You can change MediaTypeValidationMiddleware to extend content negotiation for Rates APIs.
			 */
			app.Use<MediaTypeValidationMiddleware>(configurations);

			// Authenticate first with Azure B2C (ie OpenID Connect)
			app.UseOIDCAuthentication();
			// If that fails, flow through to API-specific authentication (OAuth 2 Bearer token)
			app.ConfigureRatesApiAuthentication(configurations);

			if (!CW1ApplicationInitialiser.IsHostedInIIS())
			{
				// In IIS hosting, we use GlobalConfiguration.Configure in global.asax.cs
				// Source: https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/configuring-aspnet-web-api
				app.UseWebApi(CreateWebApiConfig(configurations));
			}
		}

		HttpConfiguration CreateWebApiConfig(IRatesAPIsAppSettings configurations)
		{
			var config = new HttpConfiguration();

			SharedStartup.ConfigureWebApi(config, configurations);

			config.ConfigureWebApplicationUpgrade();

			return config;
		}
	}
}
