using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	//
	// This filter attributes relies on The ApiController Processsing Model
	// Where Authentication Filter is considered first (AuthenticateAsync) then Authorization second (OnAuthorizationAsync) 
	//
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Class is inhertied by eAdaptorNextEDIClientAuthenticationDeciderAttribute class")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class EDIClientAuthenticationDeciderAttribute : AuthorizationFilterAttribute, IAuthenticationFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Auth type name")]
		const string OAuthAuthenticationType = "Bearer";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Auth type name")]
		const string BasicAuthenticationType = "Basic";

		readonly string ApplicationCode;

		public EDIClientAuthenticationDeciderAttribute(string applicationCode)
		{
			ApplicationCode = applicationCode;
		}

		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var authHeader = GetAuthHeaderScheme(context.Request);

				if (authHeader == OAuthAuthenticationType)
				{
					return Task.CompletedTask;
				}
				else if (authHeader == BasicAuthenticationType)
				{
					var basicAuthAttribute = new EDIClientBasicAuthenticationAttribute(ApplicationCode);
					basicAuthAttribute.AuthenticateAsync(context, cancellationToken);
				}
				else
				{
					context.ErrorResult = new AuthenticationFailureResult((NoResString)"Invalid authorization data", context.Request);
				}
			}
			return Task.CompletedTask;
		}

		public override async Task OnAuthorizationAsync(HttpActionContext context, CancellationToken cancellationToken)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (GetAuthHeaderScheme(context.Request) == OAuthAuthenticationType)
				{
					var oauthAttribute = new EDIClientOAuth2AuthorizationAttribute(ApplicationCode);
					await oauthAttribute.OnAuthorizationAsync(context, cancellationToken);
					return;
				}
			}
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			var challenge = new AuthenticationHeaderValue(BasicAuthenticationType);
			context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
			return Task.CompletedTask;
		}

		string GetAuthHeaderScheme(HttpRequestMessage request)
		{
			return request.Headers?.Authorization?.Scheme ?? string.Empty;
		}
	}
}
