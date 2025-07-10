using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Services.ServiceHost.Tests.AuthenticationTestHelper;

namespace Enterprise.Services.ServiceHost.Tests
{
	class OAuth2ViaCommunicationPartyConfigCacheAuthorizationAttributeTest : OAuth2AuthorizationMockOpenIDIdentityServerTestCase
	{
		class TestMessagingContext : IMessagingContext
		{
			public IEDICommunicationPartyConfig CurrentInboundConfig { get; set; }
			public void ResetCurrentInboundConfig() { }
		}

		HttpActionContext Authenticate(string scheme, string accessToken, CancellationToken cancellationToken)
		{
			var authorization = new AuthenticationHeaderValue(scheme, accessToken);
			return AuthenticationTestHelper.Authenticate(authorization, cancellationToken, () => new EDIClientOAuth2AuthorizationAttribute(eAdaptorNextApplicationDescriptor.ApplicationCode));
		}

		void DoWithEAdaptorNextEnabled(Action action)
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None))
				.Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				action();
			}
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestPartyConfigCache()
		{
			DoWithEAdaptorNextEnabled(() =>
			{
				Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (ObjectFactory.Substitute<IMessagingContext>(new TestMessagingContext()))
					{
						var config = SetUpOAuthAuthentication(authServer);

						var cache = ObjectFactory.Get<ICommunicationPartyConfigCache>();

						var token = GetAccessTokenFromMockServer(authServer);
						var httpActionContext = Authenticate("Bearer", token, CancellationToken.None);

						AssertNull(httpActionContext.Response);
						Assert(httpActionContext.RequestContext.Principal.Identity.IsAuthenticated);
						AssertEquals(config.PK, ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig?.PK);
					}
				}).GetAwaiter().GetResult();
			});
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestInactiveParty()
		{
			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					SetUpOAuthAuthentication(authServer, true, false);

					var token = GetAccessTokenFromMockServer(authServer);
					var httpActionContext = Authenticate("Bearer", token, CancellationToken.None);

					AssertEquals("Invalid access token", httpActionContext.Response.ReasonPhrase);
				}
			}).GetAwaiter().GetResult();
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestInactivePartyConfig()
		{
			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					SetUpOAuthAuthentication(authServer, false);

					var token = GetAccessTokenFromMockServer(authServer);
					var httpActionContext = Authenticate("Bearer", token, CancellationToken.None);

					AssertEquals("Invalid access token", httpActionContext.Response.ReasonPhrase);
				}
			}).GetAwaiter().GetResult();
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TesteAdaptorAuthenticationDeciderAttributeWithHTTPS()
		{
			VerifyAuthenticationDeciderAttribute("https://www.galaxyfarfaraway.com");
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TesteAdaptorAuthenticationDeciderAttributeWithHTTP()
		{
			VerifyAuthenticationDeciderAttribute("http://www.galaxyfarfaraway.com");
		}

		void VerifyAuthenticationDeciderAttribute(string uri)
		{
			async Task<HttpActionContext> AuthenticateWithDecider(AuthenticationHeaderValue authHeader, CancellationToken cancellationToken)
			{
				var request = CreateAuthRequest(authHeader, uri);
				var actionContext = CreateHttpActionContext(request);

				var authentication = new EDIClientAuthenticationDeciderAttribute(eAdaptorNextApplicationDescriptor.ApplicationCode);
				await authentication.OnAuthorizationAsync(actionContext, cancellationToken);
				return actionContext;
			}

			DoWithEAdaptorNextEnabled(() =>
			{
				Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						SetUpOAuthAuthentication(authServer);

						var token = GetAccessTokenFromMockServer(authServer);
						var authorization = new AuthenticationHeaderValue("Bearer", token);

						var httpActionContext = AuthenticateWithDecider(authorization, CancellationToken.None).Result;
						AssertValidResponse(httpActionContext);
					}
				}).GetAwaiter().GetResult();
			});
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TesteAdaptorAuthenticationDeciderAttributeWithoutDisposableActionWithHTTPS()
		{
			VerifyAuthenticationDeciderAttributeWithoutDisposableAction("https://www.galaxyfarfaraway.com");
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TesteAdaptorAuthenticationDeciderAttributeWithoutDisposableActionWithHTTP()
		{
			VerifyAuthenticationDeciderAttributeWithoutDisposableAction("http://www.galaxyfarfaraway.com");
		}

		void VerifyAuthenticationDeciderAttributeWithoutDisposableAction(string uri)
		{
			async Task<HttpActionContext> AuthenticateWithDecider(AuthenticationHeaderValue authHeader, CancellationToken cancellationToken)
			{
				var request = CreateAuthRequest(authHeader, uri);
				var actionContext = CreateHttpActionContext(request);

				var authentication = new EDIClientAuthenticationDeciderAttribute(eAdaptorNextApplicationDescriptor.ApplicationCode);
				await authentication.OnAuthorizationAsync(actionContext, cancellationToken);
				return actionContext;
			}

			DoWithEAdaptorNextEnabled(() =>
			{
				Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						SetUpOAuthAuthentication(authServer);
					}
					var token = GetAccessTokenFromMockServer(authServer);
					var authorization = new AuthenticationHeaderValue("Bearer", token);

					var httpActionContext = AuthenticateWithDecider(authorization, CancellationToken.None).Result;
					AssertValidResponse(httpActionContext);
					AssertEquals("No errors should be reported.", 0, ErrorReporter.TotalErrorCount);
				}).GetAwaiter().GetResult();
			});
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestApplicationCodeDoesNotExist()
		{
			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					SetUpOAuthAuthentication(authServer, applicationCode: "000");

					var token = GetAccessTokenFromMockServer(authServer);
					var httpActionContext = Authenticate("Bearer", token, CancellationToken.None);

					AssertEquals("Invalid access token", httpActionContext.Response.ReasonPhrase);
				}
			}).GetAwaiter().GetResult();
		}
	}
}
