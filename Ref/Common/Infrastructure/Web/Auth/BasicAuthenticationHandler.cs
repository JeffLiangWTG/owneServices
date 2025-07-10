using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public class BasicAuthenticationHandler : IAuthenticationHandler
	{
		HttpContext _context;
		ILogger _logger;
		IAuthenticationHelper _authenticationHelper;

		string _schemeName;
		const string Realm = "Wisetech Global";
		const string FailureMessage = "Unauthorized request";

		public BasicAuthenticationHandler(ILogWrapper logWrapper, IAuthenticationHelper authenticationHelper)
		{
			_logger = new AuthenticationLogger(logWrapper);
			_authenticationHelper = authenticationHelper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public Task<AuthenticateResult> AuthenticateAsync()
		{
			AuthenticateResult result = AuthenticateResult.Fail(FailureMessage);
			try
			{
				var request = _context.Request;
				var authHeader = request.Headers.Authorization;
				var principal = _authenticationHelper.GetClaimsPrincipal(authHeader);
				if (principal != null)
				{
					_context.User = principal;
					var authenticateTicket = new AuthenticationTicket(principal, _schemeName);
					result = AuthenticateResult.Success(authenticateTicket);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, FailureMessage);
			}
			return Task.FromResult(result);
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
				_logger.LogError(ex, FailureMessage);
			}
			return Task.CompletedTask;
		}

		public Task ForbidAsync(AuthenticationProperties properties)
		{
			return Task.CompletedTask;
		}

		public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
		{
			_schemeName = scheme.Name;
			_context = context;
			return Task.CompletedTask;
		}

		void WriteToResponse()
		{
			var response = _context.Response;
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
