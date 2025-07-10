using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;

namespace Enterprise.Services.Scim.Api.Auth
{
	static class AuthConfigExtensions
	{
#if DEBUG
		public static void ConfigureDebugAuth(this IAppBuilder app)
		{
			app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
			{
				AuthenticationMode = AuthenticationMode.Active,
				TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = false,
					ValidateIssuerSigningKey = false,
					ValidIssuer = "Microsoft.Security.Bearer",
					ValidAudience = "Microsoft.Security.Bearer",
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("A1B2C3D4E5F6A1B2C3D4E5F6"))
				}
			});
		}
#endif

		public static void ConfigureAzureAdJwtAuth(this IAppBuilder app, string issuer, string audienceId, string knownEndpointPath)
		{
			app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
			{
				AuthenticationMode = AuthenticationMode.Active,
				AllowedAudiences = new[] { audienceId },
				IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[]
				{
					new OpenIdConnectSecurityKeyProvider($"{issuer}{knownEndpointPath}")
				}
			});
		}
	}
}
