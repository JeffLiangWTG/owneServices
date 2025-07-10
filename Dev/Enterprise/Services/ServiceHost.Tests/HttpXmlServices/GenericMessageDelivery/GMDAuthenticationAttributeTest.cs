using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Enterprise.Customs.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices.GenericMessageDelivery
{
	class GMDAuthenticationAttributeTest : TransactionedTestCase
	{
		public void TestDefaultProperties()
		{
			var authentication = new GMDAuthenticationAttribute();
			AssertEquals(false, authentication.AllowMultiple);
		}

		public async void TestAuthenticateAsync()
		{
			using (CustomsDataRegistry.Instance.EnableGenericMessageDeliveryWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var authenticationContext = new HttpAuthenticationContext(GetDefaultActionContext(), null);
				var authentication = new GMDAuthenticationAttribute();
				await authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				var result = authenticationContext.ErrorResult as AuthenticationFailureResult;
				AssertEquals("Generic Message Delivery Service is disabled. You can enable it in Maintain -> System -> Registry -> Customs -> Gate Booking -> Enable Facilities Gate Web Service", result.ReasonPhrase);
			}

			using (CustomsDataRegistry.Instance.EnableGenericMessageDeliveryWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var authenticationContext = new HttpAuthenticationContext(GetDefaultActionContext(), null);
				var authentication = new GMDAuthenticationAttribute();
				await authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				AssertNull(authenticationContext.ErrorResult);
			}
		}

		public async void TestChallengeAsync()
		{
			var context = GetDefaultActionContext();
			var authenticationChallengeContext = new HttpAuthenticationChallengeContext(context, new AuthenticationFailureResult("Invalid username or password", context.Request));
			var authentication = new GMDAuthenticationAttribute();
			await authentication.ChallengeAsync(authenticationChallengeContext, new CancellationToken());
			var challengeAction = authenticationChallengeContext.Result;
			var result = await challengeAction.ExecuteAsync(new CancellationToken());

			Assert(result.Headers.WwwAuthenticate.Any(x => x.Scheme == IdentityBasicAuthenticationAttribute.BasicAuthenticationType));
			AssertEquals("Invalid username or password", result.ReasonPhrase);
		}

		HttpActionContext GetDefaultActionContext()
		{
			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			return context;
		}
	}
}
