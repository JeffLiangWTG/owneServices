using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public class TokenAuthenticationHandler : IAuthenticationHandler
	{
		public TokenAuthenticationHandler(ILogWrapper logWrapper, IEnumerable<ITokenValidationHelper> tokenValidationHelpers)
		{
			logger = new AuthenticationLogger(logWrapper);
			this.tokenValidationHelpers = tokenValidationHelpers;
		}

		HttpContext context;
		readonly ILogger logger;
		readonly IEnumerable<ITokenValidationHelper> tokenValidationHelpers;

		string schemeName;
		const string Realm = "Wisetech Global";
		const string FailureMessage = "Unauthorized: Invalid access token";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public async Task<AuthenticateResult> AuthenticateAsync()
		{
			var request = context.Request;
			var authHeader = request.Headers.Authorization;
			var accessToken = AccessTokenResolver.ResolveAccessToken(authHeader);
			if (string.IsNullOrEmpty(accessToken))
			{
				return AuthenticateResult.Fail(FailureMessage);
			}

			foreach (var tokenValidationHelper in tokenValidationHelpers)
			{
				if (tokenValidationHelper.ShouldHandle(accessToken))
				{
					try
					{
						var securityToken = await tokenValidationHelper.VerifyAccessTokenAsync(accessToken, ConfigurationHelper.ConfigurationManagerCache, logger, CancellationToken.None);
						if (securityToken != null)
						{
							var principal = new ClaimsPrincipal(new[] { new ClaimsIdentity(securityToken.Claims, schemeName) });
							context.User = principal;
							var authTicket = new AuthenticationTicket(principal, schemeName);
							return AuthenticateResult.Success(authTicket);
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, FailureMessage);
					}
				}
			}

			return AuthenticateResult.Fail(FailureMessage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public Task ChallengeAsync(AuthenticationProperties properties)
		{
			try
			{
				WriteToResponse();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, FailureMessage);
			}
			return Task.CompletedTask;
		}

		public Task ForbidAsync(AuthenticationProperties properties)
		{
			return Task.CompletedTask;
		}

		public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
		{
			this.context = context;
			schemeName = scheme.Name;
			return Task.CompletedTask;
		}

		void WriteToResponse()
		{
			var response = context.Response;
			if (!response.HasStarted && response.StatusCode != (int)HttpStatusCode.Unauthorized)
			{
				response.StatusCode = (int)HttpStatusCode.Unauthorized;
				if (!response.Headers.ContainsKey("WWW-Authenticate"))
				{
					response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Realm}\"";
				}
				var message = Encoding.UTF8.GetBytes(FailureMessage);
				response.Body.Write(message);
			}
		}
	}
}
