using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using WTG.OpenIDConnect.Token;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Enterprise.Services.ServiceHost
{
	//
	// Ensure compliance with RFC 6750 "The OAuth 2.0 Authorization Framework: Bearer Token Usage".
	//
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public abstract class OAuth2AuthorizationAttribute : AuthorizationFilterAttribute
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web authentication message.")]
		const string AuthenticatonScheme = "Bearer";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web exception message.")]
		const string InvalidAccessToken = "Invalid access token";

		public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			var logger = new TokenValidationLogger(new SimpleLogger());
			string failureReason = string.Empty;

			try
			{
				if (!TryGetAccessToken(actionContext, out string accessToken))
				{
					failureReason = (NoResString)"Invalid authorization data"; // Web exception message.
				}
				else if (!ValidateCliendIdAndAuthorityUrl(logger, accessToken, out string clientId, out string authorityUrl))
				{
					failureReason = InvalidAccessToken;
				}
				else
				{
					cancellationToken.ThrowIfCancellationRequested();

					var validatedtoken = await TokenValidator.ValidateAccessToken(authorityUrl, clientId, accessToken, ConfigurationHelper.ConfigurationManagerCache, logger, cancellationToken).ConfigureAwait(false);
					if (validatedtoken != null)
					{
						await base.OnAuthorizationAsync(actionContext, cancellationToken).ConfigureAwait(false);
						var claimsIdentity = new ClaimsIdentity(validatedtoken.Claims, AuthenticatonScheme);
						actionContext.RequestContext.Principal = new ClaimsPrincipal(new[] { claimsIdentity });
						return;
					}
					failureReason = InvalidAccessToken;
				}
			}
			catch (ValidateTokenException ex) when (ex.Type == ValidateTokenExceptionType.TokenExpired)
			{
				failureReason = (NoResString)"Access token expired"; // Web exception message.
			}
			catch (ValidateTokenException ex) when (ex.Type == ValidateTokenExceptionType.TokenValidation)
			{
				failureReason = InvalidAccessToken;
			}
			catch (OperationCanceledException)
			{
				actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (NoResString)"Operation Cancelled"); // Web exception message.
				return;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, (NoResString)"Unable to validate access token"); // Web exception message.
				ErrorReporter.ReportOnce("EAdaptorAuthenticationUnhandledError", logger.ToString(), ex);
				return;
			}

			actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
			actionContext.Response.Content = new StringContent(failureReason);
			actionContext.Response.ReasonPhrase = failureReason;
			actionContext.Response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue((NoResString)"Bearer", (NoResString)"realm=\"WiseTech Global\"")); // Web authentication message.
		}

		bool TryGetAccessToken(HttpActionContext actionContext, out string accessToken)
		{
			accessToken = null;
			var authHeader = actionContext.Request.Headers?.Authorization;

			if (AuthenticatonScheme.Equals(authHeader?.Scheme, StringComparison.OrdinalIgnoreCase))
			{
				accessToken = authHeader.Parameter;
			}

			return !string.IsNullOrEmpty(accessToken);
		}

		bool ValidateCliendIdAndAuthorityUrl(ILogger logger, string accessToken, out string clientID, out string authorityUrl)
		{
			clientID = null;
			authorityUrl = null;
			var token = TokenValidator.ReadJwtToken(accessToken, logger);

			if (token?.Audiences?.Count() == 1)
			{
				clientID = token.Audiences.First();
				authorityUrl = token.Issuer;

				using (Db.DisposableActionForDbConnection())
				{
					if (!string.IsNullOrEmpty(clientID)
						&& !string.IsNullOrEmpty(authorityUrl)
						&& ValidateClientIdAndAuthUrl(clientID, authorityUrl, token))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected abstract bool ValidateClientIdAndAuthUrl(string clientID, string authorityUrl, JwtSecurityToken token);
	}
}
