using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationService.Client.Models;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Microsoft.Owin;
using WTG.OpenIDConnect.Token;

namespace Enterprise.Rating.Web.Authentication
{
	public class OIDCValidationMiddleware
	{
		readonly ILoggerExtended logger;

		public OIDCValidationMiddleware(ILoggerExtended logger)
		{
			this.logger = logger;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key value")]
		public Task Invoke(IOwinContext context, Func<Task> next)
		{
			if (context.Authentication.User is object)
			{
				// Already authenticated. Continue
				return next();
			}

			// Try to authenticate using this middleware
			if (context.Request.Headers.TryGetValue("Authorization", out var authArray))
			{
				var authHeaderString = authArray[0];
				var accessToken = AuthenticationHeaderValue.Parse(authHeaderString).Parameter;

				var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

				if (jwtSecurityTokenHandler.CanReadToken(accessToken))
				{
					TokenValidationCore(context, jwtSecurityTokenHandler, accessToken);
				}
			}

			// Move to next middleware (ie existing OAuth2 authentication)
			return next();
		}

		void TokenValidationCore(IOwinContext context, JwtSecurityTokenHandler jwtSecurityTokenHandler, string accessToken)
		{
			var jwtToken = jwtSecurityTokenHandler.ReadJwtToken(accessToken);
			var usernameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "user_name");
			var clientId = jwtToken.Payload.Azp;

			if (usernameClaim != null)
			{
				var oidcConfig = CargoWise.Application.ObjectFactory.Get<IOIDCConfig>();

				if (oidcConfig.IsOIDCEnabled)
				{
					if (ValidateUserAccessToken(accessToken, oidcConfig, logger))
					{
						SetUserContext(context, usernameClaim.Value);
					}
				}
				else
				{
					logger.Warning((NoResString)"OpenID Connect is not enabled.");
				}
			}
			else if (TryGetRatingTokenAuthentication(clientId, logger, out var ratingTokenAuthentication))
			{
				if (ValidateClientAccessToken(accessToken, ratingTokenAuthentication, logger))
				{
					using (Db.DisposableActionForDbConnection())
					{
						var loginName = ratingTokenAuthentication.GetLoginNameFromStaffCode();

						if (!string.IsNullOrEmpty(loginName))
						{
							SetUserContext(context, loginName);
						}
					}
				}
			}
		}

		bool TryGetRatingTokenAuthentication(string clientId, ILoggerExtended logger, out RatingTokenAuthentication ratingTokenAuthentication)
		{
			ratingTokenAuthentication = null;
			var ratingTokenAuthenticationCollection = RatingDataRegistry.Instance.RatingTokenAuthentication.Value.OfType<RatingTokenAuthentication>();
			if (!ratingTokenAuthenticationCollection.Any())
			{
				logger.Warning($"Registry '{RatingDataRegistry.Instance.RatingTokenAuthentication.GetLocation()}' is not defined.");
			}
			else
			{
				ratingTokenAuthentication = ratingTokenAuthenticationCollection.FirstOrDefault(tokenAuthentication => tokenAuthentication.ClientId == clientId);
				if (ratingTokenAuthentication == null)
				{
					logger.Warning($"No matched '{RatingDataRegistry.Instance.RatingTokenAuthentication.GetLocation()}' item for client id '{clientId}'.");
				}
			}
			return ratingTokenAuthentication != null;
		}

		public bool ValidateUserAccessToken(string accessToken, IOIDCConfig config, ILoggerExtended logger)
		{
			return ValidateCore(accessToken, config.AuthorityURL, config.ClientIdentifier, logger);
		}

		public bool ValidateClientAccessToken(string accessToken, RatingTokenAuthentication ratingTokenAuthentication, ILoggerExtended logger)
		{
			return ValidateCore(accessToken, ratingTokenAuthentication.Endpoint, ratingTokenAuthentication.ClientId, logger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging message")]
		bool ValidateCore(string accessToken, string url, string clientId, ILoggerExtended logger)
		{
			try
			{
				var tokenValidationLogger = new TokenValidationLogger(logger);
				var securityToken = TokenValidator.ValidateAccessToken(url, clientId, accessToken, ConfigurationHelper.ConfigurationManagerCache, tokenValidationLogger, CancellationToken.None).Result;
				return securityToken != null;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error("Token Validation failed:\r\n" + ex.Message);
				return false;
			}
		}

		bool SetUserContext(IOwinContext context, string username)
		{
			var identity = new ClaimsIdentity("JwtBearer");
			identity.AddClaim(new Claim(WTGClaimTypes.UserCode, username));
			context.Authentication.User = new ClaimsPrincipal(identity);
			return true;
		}
	}
}
