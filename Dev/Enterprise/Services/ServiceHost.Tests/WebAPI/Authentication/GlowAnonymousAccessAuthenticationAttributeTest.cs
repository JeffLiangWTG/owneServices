using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GlowAnonymousAccessAuthenticationAttributeTest : TestCaseWithFactory
	{
		public void TestOnAuthorization_NoAuthorizationHeader()
		{
			var response = ExecuteRequest();
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.TokenNotProvided.ToString("G"));
		}

		public void TestOnAuthorization_InalidToken()
		{
			var token = "this is not a token";

			var response = ExecuteRequest(token);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
		}

		public void TestOnAuthorization_ValidToken()
		{
			var token = CreateLimitedToken();
			var actionContext = CreateActionContext(token);

			var attribute = new GlowAnonymousAccessAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute, actionContext);

			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals(actionContext.Response, response);
		}

		public void TestOnAuthorization_ConsumedToken()
		{
			var token = CreateLimitedToken();
			var actionContext = CreateActionContext(token);

			var attribute = new GlowAnonymousAccessAuthenticationAttribute();

			ExecuteFilterAndReturnResponse(attribute, actionContext);

			var response = ExecuteRequest(token);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
		}

		public void TestShouldUseDbCorrectly()
		{
			var lastError = string.Empty;
			var token = CreateLimitedToken();

			var thread = new Thread(() =>
			{
				var actionContext = CreateActionContext(token);
				var attribute = new GlowAnonymousAccessAuthenticationAttribute();

				ExecuteFilterAndReturnResponse(attribute, actionContext);

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});
			thread.Start();
			thread.Join(1000);

			// Should not be "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			AssertEquals(string.Empty, lastError);
		}

		HttpResponseMessage ExecuteRequest(string token = null)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			var attribute = new GlowAnonymousAccessAuthenticationAttribute();

			if (token != null)
			{
				request.Headers.Authorization = new AuthenticationHeaderValue(nameof(AccessTokenTypes.GlowAnonymousEnterpriseAccess), token);
			}

			return ExecuteFilterAndReturnResponse(attribute, request);
		}

		static HttpResponseMessage ExecuteFilterAndReturnResponse(IAuthenticationFilter filter, HttpRequestMessage request)
		{
			return ExecuteFilterAndReturnResponse(
				filter,
				new HttpActionContext
				{
					ControllerContext = new HttpControllerContext { Request = request },
					Response = new HttpResponseMessage(HttpStatusCode.OK)
				});
		}

		static HttpResponseMessage ExecuteFilterAndReturnResponse(IAuthenticationFilter filter, HttpActionContext actionContext)
		{
			var authenticationContext = new HttpAuthenticationContext(actionContext, null);
			filter.AuthenticateAsync(authenticationContext, new CancellationToken()).GetAwaiter().GetResult();
			actionContext.RequestContext.Principal = authenticationContext.Principal;
			if (authenticationContext.ErrorResult != null)
			{
				return authenticationContext.ErrorResult.ExecuteAsync(new CancellationToken()).GetAwaiter().GetResult();
			}
			return authenticationContext.ActionContext.Response;
		}

		static string GetHeaderValue(HttpResponseMessage response, string headerName) => response.Headers.GetValues(headerName).FirstOrDefault();

		static string CreateLimitedToken()
		{
			var accessControl = new TokenizedAccessControl();
			return accessControl.CreateLimitedToken(AccessTokenTypes.GlowAnonymousEnterpriseAccess, new AccessTokenInfo(string.Empty, Guid.NewGuid(), "OC"), TimeSpan.FromMinutes(5), 1);
		}

		static HttpActionContext CreateActionContext(string token)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue(nameof(AccessTokenTypes.GlowAnonymousEnterpriseAccess), token);

			var actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };
			actionContext.Response = new HttpResponseMessage(HttpStatusCode.OK);

			return actionContext;
		}
	}
}
