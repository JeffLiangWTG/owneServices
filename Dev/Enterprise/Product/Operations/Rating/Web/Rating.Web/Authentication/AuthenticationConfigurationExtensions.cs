using System;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace Enterprise.Rating.Web.Authentication
{
	static class AuthenticationConfigurationExtensions
	{
		public static void ConfigureRatesApiAuthentication(this IAppBuilder app, IRatesAPIsAppSettings configurations)
		{
			app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions()
			{
				TokenEndpointPath = new Microsoft.Owin.PathString("/api/rating/token"),
				AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(configurations.TokenExpiryDurationInSeconds),
				Provider = new AuthenticationServerProvider(),
				AllowInsecureHttp = true,
			});

			app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
		}

		public static void UseOIDCAuthentication(this IAppBuilder app)
		{
			var serviceProvider = DependencyInjectionConfig.CreateServiceProvider();

			app.Use((context, next) =>
			{
				using (var scope = serviceProvider.CreateScope())
				{
					var scopedLogger = serviceProvider.GetRequiredService<ILoggerExtended>();
					var mw = new OIDCValidationMiddleware(scopedLogger);

					return mw.Invoke(context, next);
				}
			});
			// Run this during the authenticate stage of the IIS Integrated
			// Pipeline
			app.UseStageMarker(PipelineStage.Authenticate);
		}
	}
}
