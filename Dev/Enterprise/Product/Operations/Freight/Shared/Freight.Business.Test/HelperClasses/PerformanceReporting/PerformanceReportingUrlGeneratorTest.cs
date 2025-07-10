using System;
using System.Collections.Generic;
using System.Threading;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PerformanceReportingUrlGeneratorTest : TestCaseWithFactory
	{
		const string MockUrl = "https://whatever.cargowise.com:8080";
		PerformanceReportingUrlGenerator generator;
		IPerformanceReportingAuthToken token;
		protected override void SetUp()
		{
			base.SetUp();
			generator = new PerformanceReportingUrlGenerator();
			token = new PerformanceReportingAuthToken("authToken");
		}

		IDisposable RegisterTokenProvider(string token = "authToken", string errorMessage = null, LoginInfo loginInfo = null)
		{
			var tokenProviderMock = new Mock<IAuthTokenProvider>();
			tokenProviderMock
				.Setup(m => m.GetToken(It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<Action<LoginInfo>>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<bool>()))
				.Returns((token, errorMessage))
				.Callback((string _, int _, Action<LoginInfo> overrideLoginInfo, CancellationToken _, bool _) =>
					{
						if (loginInfo != null)
						{
							overrideLoginInfo(loginInfo);
						}
					});
			return ObjectFactory.Substitute(tokenProviderMock.Object);
		}

		void AssertUrlEquals(Uri expectedUrl, Uri actualUrl)
		{
			AssertEquals("URL must match scheme, port, domain and path", expectedUrl.GetLeftPart(UriPartial.Path), actualUrl.GetLeftPart(UriPartial.Path));

			IEnumerable<string> ToKeyValuePairStrings(string query)
			{
				if (query.Length == 0)
				{
					yield break;
				}

				var qs = new QueryString(query.Substring(1));

				foreach (var k in qs.AllKeys)
				{
					yield return k + "=" + qs[k];
				}
			}

			var expectedQuery = ToKeyValuePairStrings(expectedUrl.Query);
			var actualQuery = ToKeyValuePairStrings(actualUrl.Query);

			AssertContainsExactElementsInAnyOrder("URL query must match by key value pair, in any order", expectedQuery, actualQuery);
		}

		void AssertGenerate_ReturnsUrl(string path, IPerformanceReportingAuthToken token, bool isLayoutHidden, Uri expectedUrl)
		{
			using (RegisterTokenProvider())
			using (FreightDataRegistry.Instance.ReportingUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MockUrl))
			{
				var (url, errorMessage) = generator.Generate(path, token, isLayoutHidden);

				CombineAssertions(() =>
				{
					AssertNull(errorMessage);
					AssertUrlEquals(expectedUrl, url);
				});
			}
		}

		void AssertGenerate_ReturnsError(string path, IPerformanceReportingAuthToken token, bool isLayoutHidden, string expectedErrorMessage, string testUrl)
		{
			using (RegisterTokenProvider())
			using (FreightDataRegistry.Instance.ReportingUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testUrl))
			{
				var (url, errorMessage) = generator.Generate(path, token, isLayoutHidden);

				CombineAssertions(() =>
				{
					AssertEquals(expectedErrorMessage, errorMessage);
					AssertNull(url);
				});
			}
		}

		public void TestGetToken_Succeeds()
		{
			using (RegisterTokenProvider())
			{
				var tokenResult = generator.GetToken(ZGuid.NewZGuid().ToString());

				CombineAssertions(() =>
				{
					AssertEquals("authToken", tokenResult.Token.Value);
					AssertNull(tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetTokenForOrgContact_Succeeds()
		{
			var loginInfo = new LoginInfo();
			using (RegisterTokenProvider(loginInfo: loginInfo))
			{
				var parentOrganisation = Factory.New<OrgHeader>();
				parentOrganisation.OH_Code = "PCO";
				parentOrganisation.OH_FullName = "Parent Company Org";

				var orgContact = Factory.New<OrgContact>();
				orgContact.OC_ContactName = "User Name";
				orgContact.OC_Email = "user.name@mail.me";
				orgContact.OC_OH = parentOrganisation.PK;

				var tokenResult = generator.GetToken(ZGuid.NewZGuid().ToString(), orgContact);

				CombineAssertions(() =>
				{
					AssertEquals("authToken", tokenResult.Token.Value);
					AssertNull(tokenResult.ErrorMessage);
					AssertEquals("User Name", loginInfo.UserFullName);
					AssertEquals("user.name@mail.me", loginInfo.UserEmail);
					AssertEquals("PCO", loginInfo.ClientCompanyCode);
					AssertEquals("Parent Company Org", loginInfo.ClientCompanyName);
				});
			}
		}

		public void TestGetToken_ReturnsError_When_NoAuthToken()
		{
			using (RegisterTokenProvider(null, "internal error message here"))
			{
				var tokenResult = generator.GetToken(ZGuid.NewZGuid().ToString());

				CombineAssertions(() =>
				{
					AssertNull(tokenResult.Token);
					AssertEquals("Unable to get permission to show the report: internal error message here", tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGenerate_ReturnsLandingPageUrlByDefault()
		{
			var expectedUrl = new Uri($"{MockUrl}");
			AssertGenerate_ReturnsUrl(null, null, false, expectedUrl);
		}

		public void TestGenerate_ReturnsUrlWithPath()
		{
			var expectedUrl = new Uri($"{MockUrl}/login");
			AssertGenerate_ReturnsUrl("login", new PerformanceReportingAuthToken(null), false, expectedUrl);
		}

		public void TestGenerate_AddsNoLayoutQueryParameter()
		{
			var expectedUrl = new Uri($"{MockUrl}/reports/on-time-performance?token=authToken&nolayout=true");
			AssertGenerate_ReturnsUrl("reports/on-time-performance", token, true, expectedUrl);
		}

		public void TestGenerate_ReturnsError()
		{
			AssertGenerate_ReturnsError(null, token, false, "Invalid registry item: Freight > Global Tracking > Performance Reporting > Reporting URL.", "not a url");
		}
	}
}
