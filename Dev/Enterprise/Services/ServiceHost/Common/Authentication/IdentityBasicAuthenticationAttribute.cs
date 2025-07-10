using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	public abstract class IdentityBasicAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		public virtual Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			var request = context.Request;

			try
			{
#if DEBUG
				onAuthenticate.Value?.Invoke(context);
#endif
				if (request.Headers.Authorization != null && request.Headers.Authorization.Scheme.Equals(BasicAuthenticationType, StringComparison.OrdinalIgnoreCase))
				{
					var errorMessage = string.Empty;
					if (TryGetNameAndPassword(request.Headers.Authorization.Parameter, out var userName, out var password, out errorMessage)
						&& IsOK(userName, password, out errorMessage))
					{
						var nameClaim = new Claim(ClaimTypes.Name, userName);
						var identity = new ClaimsIdentity(new[] { nameClaim }, BasicAuthenticationType);
						context.Principal = new ClaimsPrincipal(new[] { identity });
						return Task.FromResult(0);
					}
					else
					{
						context.ErrorResult = new AuthenticationFailureResult(errorMessage, request);
					}
				}
			}
			catch (InvalidOperationException ex) when (ex.Message.StartsWith((NoResString)"Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.", StringComparison.Ordinal)) // Exception message
			{
				var message = Res.GetString("2b812cd2-9866-4e58-877e-b6eadacb33aa", "Failed to connect to database. Please try again later.");
				context.ErrorResult = new AuthenticationFailureResult(message, request);
			}

			if (context.ErrorResult == null)
			{
				context.ErrorResult = new AuthenticationFailureResult((NoResString)"Unauthorized", request);
			}

			return Task.FromResult(0);
		}

		protected abstract bool IsOK(string userName, string password, out string message);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Basic authentication type, i.e. username:password")]
		public const string BasicAuthenticationType = "Basic";

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			var challenge = new AuthenticationHeaderValue(BasicAuthenticationType);
			context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
			return Task.FromResult(0);
		}

		public bool AllowMultiple
		{
			get { return false; }
		}

		protected bool TryGetNameAndPassword(string authorisationParameter, out string userName, out string password, out string errorMessage)
		{
			userName = password = errorMessage = null;
			try
			{
				if (authorisationParameter == null)
				{
					return false;
				}

				var credentials = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(authorisationParameter));
				var userNameAndPassword = credentials.Split(':');
				if (userNameAndPassword.Length != 2)
				{
					errorMessage = Res.GetString("{1696D95F-D82F-4E88-8C6F-8B7530A219FA}", "Invalid authentication data.");
					return false;
				}

				userName = userNameAndPassword[0];
				password = userNameAndPassword[1];
				return true;
			}
			catch (FormatException)
			{
				errorMessage = Res.GetString("{CED6CC4C-70C2-4048-9CCE-1EBE0206E91C}", "Corrupted authentication data.");
				return false;
			}
		}

#if DEBUG
		protected static Overridable<Action<HttpAuthenticationContext>> onAuthenticate = new Overridable<Action<HttpAuthenticationContext>>(null);

		public static IDisposable SetOnAuthenticateHookForTest(Action<HttpAuthenticationContext> hook)
		{
			onAuthenticate.Value = hook;
			return new DisposableAction(onAuthenticate.ResetValue);
		}
#endif
	}
}
