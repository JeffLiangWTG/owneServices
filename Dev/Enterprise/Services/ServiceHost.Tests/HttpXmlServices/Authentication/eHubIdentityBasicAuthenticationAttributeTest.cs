using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Test
{
	[UseSnapshotProtection]
	public class eHubIdentityBasicAuthenticationAttributeTest : TestCase
	{
		public void TestAuthenticateAsyncInThread()
		{
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test");

			var authentication = new eHubIdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes("test1:test1")));
			request.Headers.Authorization = authorization;

			var thread = new Thread(() =>
			{
				((IRegistryItemInternals)eAdaptorRegistry.Instance.eAdaptorInboundAuthentications).ClearCache();
				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
			});

			thread.Start();
			thread.Join();

			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("ClientID or Password invalid.", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncCorruptedBase64()
		{
			// Arrange
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test");

			var authentication = new eHubIdentityBasicAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var context = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", "pretendToBeBase64String");
			request.Headers.Authorization = authorization;

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Corrupted authentication data.", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncNullBase64()
		{
			// Arrange
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test");

			var authentication = new eHubIdentityBasicAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var context = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", null);
			request.Headers.Authorization = authorization;

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Unauthorized", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncInvalidAmountOfStrings()
		{
			// Arrange
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test");

			var authentication = new eHubIdentityBasicAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var context = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue(
				"Basic",
				Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes("test1:test2:test3")));
			request.Headers.Authorization = authorization;

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Invalid authentication data.", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncTimeout()
		{
			// Arrange
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test");

			var authentication = new eHubIdentityBasicAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var actionContext = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(actionContext, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes("test:test")));
			request.Headers.Authorization = authorization;

			using var resetHook = IdentityBasicAuthenticationAttribute.SetOnAuthenticateHookForTest((context) => { throw new InvalidOperationException("Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool."); });

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Failed to connect to database. Please try again later.", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncErrorMeesageIsInHttpContext()
		{
			// Arrange
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test");

			var authentication = new eHubIdentityBasicAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var context = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue(
				"Basic",
				Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes("test1:test2")));
			request.Headers.Authorization = authorization;

			using var resetContext = HttpContextHelper.SetUp(new HttpContext(
				new HttpRequest("", "http://tempuri.org", ""),
				new HttpResponse(new StringWriter())));

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			AssertNotNull(errorResult);
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("ClientID or Password invalid.", response.ReasonPhrase);
			AssertEquals("ClientID or Password invalid.", HttpContext.Current.Items[eHubUserNamePasswordValidator.ErrorMessageKeyInContextItems]);
		}
	}

	class IdentityBasicAuthenticationAttributeTransactionedTest : TransactionedTestCase
	{
		public void TestAuthenticateAsync()
		{
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test");

			var authentication = new eHubIdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			AssertNull(authenticationContext.Principal);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes("test1:test1")));
			request.Headers.Authorization = authorization;

			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("ClientID or Password invalid.", response.ReasonPhrase);

			authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes("test:test")));
			request.Headers.Authorization = authorization;
			authenticationContext.ErrorResult = null;

			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			AssertEquals(
				$"Failure reason should be empty, but it is {(authenticationContext.ErrorResult as AuthenticationFailureResult)?.ReasonPhrase}.",
				null,
				authenticationContext.ErrorResult);

			var principal = authenticationContext.Principal;
			AssertNotNull(principal);
			Assert(principal.Identity.IsAuthenticated);
		}

		public void TestChallengeAsync()
		{
			var authentication = new eHubIdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var challengeContext = new HttpAuthenticationChallengeContext(context, new AuthenticationFailureResult("Invalid username or password", request));

			authentication.ChallengeAsync(challengeContext, new CancellationToken());
			var challengeAction = challengeContext.Result;
			var result = challengeAction.ExecuteAsync(new CancellationToken()).Result;
			Assert(result.Headers.WwwAuthenticate.Any(x => x.Scheme == IdentityBasicAuthenticationAttribute.BasicAuthenticationType));
		}

		public void TestIdentityBasicAuthenticationAttributeeAdaptorController()
		{
			AssertEquals(true, Attribute.IsDefined(typeof(eAdaptorController), typeof(IdentityBasicAuthenticationAttribute)));
			AssertEquals(true, Attribute.IsDefined(typeof(eAdaptorController), typeof(AuthorizeAttribute)));
		}
	}
}
