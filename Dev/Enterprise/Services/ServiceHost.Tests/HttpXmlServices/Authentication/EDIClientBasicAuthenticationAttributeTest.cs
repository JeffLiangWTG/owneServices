using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices.Authentication
{
	class EDIClientBasicAuthenticationAttributeTest : TransactionedTestCase
	{
		IEDICommunicationPartyConfig config;
		readonly string eAdaptorNextApplicationCode = eAdaptorNextApplicationDescriptor.ApplicationCode;

		protected override void SetUp()
		{
			config = AuthenticationTestHelper.SetUpBasicAuthenticationUser("JaJaBinks", "Naboo", "Test");
			AuthenticationTestHelper.SetUpBasicAuthenticationUser("C3PO", "Tatooine", "Test2", false);
			AuthenticationTestHelper.SetUpBasicAuthenticationUser("R2D2", "Tatooine", "Test3", true, false);
			base.SetUp();
		}

		public void TestAuthenticateIncorrectPassword() => TestAuthenticateInvalidUser("JaJaBinks", "tatooine");
		public void TestAuthenticateNoUser() => TestAuthenticateInvalidUser("CadBane", "Duro");
		public void TestAuthenticateInactiveConfig() => TestAuthenticateInvalidUser("C3PO", "Tatooine");
		public void TestAuthenticateInactiveClient() => TestAuthenticateInvalidUser("R2D2", "Tatooine");

		void TestAuthenticateInvalidUser(string username, string password)
		{
			var authenticationContext = GetAuthenticationContext(username, password);

			var authenticate = new EDIClientBasicAuthenticationAttribute(eAdaptorNextApplicationCode);
			authenticate.AuthenticateAsync(authenticationContext, new CancellationToken());
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Invalid Credentials.", response.ReasonPhrase);
		}

		HttpAuthenticationContext GetAuthenticationContext(string username, string password)
		{
			var request = new HttpRequestMessage();
			var actionContect = AuthenticationTestHelper.CreateHttpActionContext(request);
			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}")));
			request.Headers.Authorization = authorization;
			return new HttpAuthenticationContext(actionContect, null);
		}

		public void TestAuthenticateValidUser()
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				var authenticationContext = GetAuthenticationContext("JaJaBinks", "Naboo");

				var authenticate = new EDIClientBasicAuthenticationAttribute(eAdaptorNextApplicationCode);
				authenticate.AuthenticateAsync(authenticationContext, new CancellationToken());
				var principal = authenticationContext.Principal;
				AssertNotNull(principal);
				Assert(principal.Identity.IsAuthenticated);
				var inboundConfig = ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig;
				AssertEquals(config.PK, inboundConfig?.PK);
				AssertEquals(eAdaptorNextApplicationCode, inboundConfig?.Party.ECP_ApplicationCode);
			}
		}

		public async void TesteAdaptorAuthenticationDeciderAttribute()
		{
			var authenticationContext = GetAuthenticationContext("JaJaBinks", "Naboo");

			var authenticate = new eAdaptorNextEDIClientAuthenticationDeciderAttribute();
			await authenticate.AuthenticateAsync(authenticationContext, new CancellationToken());
			var principal = authenticationContext.Principal;
			AssertNotNull(principal);
			Assert(principal.Identity.IsAuthenticated);
		}

		public void TesteAdaptorAuthenticationDeciderAttributeInAnotherThread()
		{
			System.Threading.Tasks.Task.Run(() =>
			{
				var authenticationContext = GetAuthenticationContext("JaJaBinks", "Naboo");
				var authenticate = new eAdaptorNextEDIClientAuthenticationDeciderAttribute();
				authenticate.AuthenticateAsync(authenticationContext, new CancellationToken());
				AssertEquals("No errors should be reported.", 0, ErrorReporter.TotalErrorCount);
			}).GetAwaiter().GetResult();
		}

		public void TestApplicationCodeDoesNotExist()
		{
			var username = "Ahsoka";
			var password = "Shili";
			AuthenticationTestHelper.SetUpBasicAuthenticationUser(username, password, "Test4", applicationCode: "000");
			var authenticationContext = GetAuthenticationContext(username, password);

			var authenticate = new EDIClientBasicAuthenticationAttribute(eAdaptorNextApplicationCode);
			authenticate.AuthenticateAsync(authenticationContext, new CancellationToken());
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Invalid Credentials.", response.ReasonPhrase);
		}
	}
}
