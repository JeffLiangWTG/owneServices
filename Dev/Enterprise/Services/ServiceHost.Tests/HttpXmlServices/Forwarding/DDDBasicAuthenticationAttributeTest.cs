using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices.Forwarding
{
	class DDDBasicAuthenticationAttributeTest : TransactionedTestCase
	{
		public void TestAuthenticateAsync()
		{
			using (FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test"))
			{
				var authentication = new DDDBasicAuthenticationAttribute();

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
				AssertEquals("Username or Password invalid.", response.ReasonPhrase);

				authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
					Encoding.GetEncoding("iso-8859-1").GetBytes($"test:test")));
				request.Headers.Authorization = authorization;
				authenticationContext.ErrorResult = null;

				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				var principal = authenticationContext.Principal;
				AssertNotNull(principal);
				Assert(principal.Identity.IsAuthenticated);
			}
		}

		public void TestChallengeAsync()
		{
			var authentication = new DDDBasicAuthenticationAttribute();

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
	}
}
