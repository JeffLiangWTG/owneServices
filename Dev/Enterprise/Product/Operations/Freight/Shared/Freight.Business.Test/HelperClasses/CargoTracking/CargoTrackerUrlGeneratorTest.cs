using System;
using System.Collections.Generic;
using System.Threading;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CargoTrackerUrlGeneratorTest : TestCaseWithFactory
	{
		public void TestGetToken_ReturnsError_When_NoAuthToken()
		{
			using (RegisterTokenProvider(null, "no token for you"))
			{
				var tokenResult = generator.GetToken(ZGuid.NewZGuid().ToString());

				CombineAssertions(() =>
				{
					AssertNull(tokenResult.Token);
					AssertNull(tokenResult.RedirectUrl);
					AssertEquals("Unable to get permission to show Cargo Tracker: no token for you", tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_WhenRatingSuccess()
		{
			using (RegisterTokenProvider())
			{
				var tokenResult = generator.GetToken(ZGuid.NewZGuid().ToString());

				CombineAssertions(() =>
				{
					AssertEquals(CargoTrackerAuthTokenType.Rating, tokenResult.Token.Type);
					AssertEquals("authToken", tokenResult.Token.Value);
					AssertNull(tokenResult.RedirectUrl);
					AssertNull(tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_WithContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG1";
			org.OH_FullName = "Organization Name";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			contact.OC_Email = "test@test.com";

			var loginInfo = new LoginInfo();
			var tokenProviderMock = new Mock<IAuthTokenProvider>();
			tokenProviderMock
				.Setup(x => x.GetToken(It.IsAny<string>(), 600, It.IsAny<Action<LoginInfo>>(), CancellationToken.None, It.IsAny<bool>()))
				.Callback<string, int, Action<LoginInfo>, CancellationToken, bool>((_, _, action, _, _) =>
			{
				action(loginInfo);
			}).Returns(("authToken", null));

			using (ObjectFactory.Substitute(tokenProviderMock.Object))
			{
				var tokenResult = generator.GetToken(ZGuid.NewZGuid().ToString(), contact, CancellationToken.None);

				CombineAssertions(() =>
				{
					AssertEquals(contact.OC_ContactName, loginInfo.UserFullName);
					AssertEquals(contact.OC_Email, loginInfo.UserEmail);
					AssertEquals(org.OH_Code, loginInfo.ClientCompanyCode);
					AssertEquals(org.OH_FullName, loginInfo.ClientCompanyName);
				});
			}
		}

		public void TestGenerate_ReturnsError_When_CargoTrackerUrlRegistryIsInvalid()
		{
			using (FreightDataRegistry.Instance.CargoTrackerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "not a url"))
			{
				var (url, errorMessage) = generator.Generate(ratingToken, "WISGLOSYD", "C00123456");

				CombineAssertions(() =>
				{
					AssertNull(url);
					AssertEquals("Invalid registry item: Freight > Global Tracking > Cargo Tracker > Cargo Tracker URL.", errorMessage);
				});
			}
		}

		public void TestGenerate_ReturnsUrl_WithRatingToken()
		{
			var model = new CargoTrackerUrlModel
			{
				ClientCode = "WISGLOSYD",
				ConsignmentNumber = "C00123456",
			};

			var expectedUrl = new Uri($"{CargoTrackerUrl}clients/WISSYD/consignments/C00123456/tracking?token=authToken");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		void AssertGenerate_ReturnsError(CargoTrackerAuthToken token, CargoTrackerUrlModel model, string expectedErrorMessage)
		{
			using (RegisterTokenProvider())
			using (FreightDataRegistry.Instance.CargoTrackerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoTrackerUrl))
			{
				var (url, errorMessage) = generator.Generate(token, model.ClientCode, model.ConsignmentNumber);

				CombineAssertions(() =>
				{
					AssertEquals(expectedErrorMessage, errorMessage);
					AssertNull(url);
				});
			}
		}

		void AssertGenerate_ReturnsUrl(CargoTrackerAuthToken token, CargoTrackerUrlModel model, Uri expectedUrl)
		{
			using (RegisterTokenProvider())
			using (FreightDataRegistry.Instance.CargoTrackerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoTrackerUrl))
			{
				var (url, errorMessage) = generator.Generate(token, model.ClientCode, model.ConsignmentNumber);

				CombineAssertions(() =>
				{
					AssertNull(errorMessage);
					AssertUrlEquals(expectedUrl, url);
				});
			}
		}

		public void TestGenerate_ReturnsError_When_NoClientCode()
		{
			var model = new CargoTrackerUrlModel
			{
				ConsignmentNumber = "C00123456",
			};

			var expectedErrorMessage = $"{BrandingFactory.Instance.ProductName} application must have a valid license code.";

			AssertGenerate_ReturnsError(ratingToken, model, expectedErrorMessage);
		}

		public void TestGetSelfSignedSystemToSystemToken_ReturnsError_When_SystemDataRegistryIsInvalid()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var result = generator.GetSelfSignedSystemToSystemToken("EDIWTGDAT");

				CombineAssertions(() =>
				{
					AssertNull(result.Token);
					AssertEquals($"{nameof(SystemDataRegistry.SystemToSystemCertificate)} is not valid. Please ensure System to System Trust configuration has been setup and the TCM task has run successfully", result.ErrorMessage);
				});
			}
		}

		public void TestGetSelfSignedSystemToSystemToken_ReturnsError_WhenLicenseCodeIsInvalid()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				var result = generator.GetSelfSignedSystemToSystemToken("ABC");

				CombineAssertions(() =>
				{
					AssertNull(result.Token);
					AssertEndsWith("Error message should end with \"application must have a valid license code.\"", "application must have a valid license code.", result.ErrorMessage);
				});
			}
		}

		public void TestGetSelfSignedSystemToSystemToken_ReturnsError_WhenCargoTrackerApiAudienceIdIsInvalid()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				using (FreightDataRegistry.Instance.CargoTrackerApiAudienceId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					var result = generator.GetSelfSignedSystemToSystemToken("EDIWTGDAT");

					CombineAssertions(() =>
					{
						AssertNull(result.Token);
						AssertEquals("Invalid registry item value: Freight > Global Tracking > Cargo Tracker > Cargo Tracker API Audience ID.", result.ErrorMessage);
					});
				}
			}
		}

		public void TestGetSelfSignedSystemToSystemToken_ReturnsError_WhenAisApiAudienceIdIsInvalid()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				using (FreightDataRegistry.Instance.AisApiAudienceId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					var result = generator.GetSelfSignedSystemToSystemToken("EDIWTGDAT");

					CombineAssertions(() =>
					{
						AssertNull(result.Token);
						AssertEquals("Invalid registry item value: Freight > Global Tracking > Cargo Tracker > AIS API Audience ID.", result.ErrorMessage);
					});
				}
			}
		}

		public void TestGetSelfSignedSystemToSystemToken_ReturnsToken()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				var result = generator.GetSelfSignedSystemToSystemToken("EDIWTGDAT");

				CombineAssertions(() =>
				{
					AssertNull(result.ErrorMessage);
					AssertEquals(true, IsValidJwt(result.Token?.Value));
				});
			}
		}

		public void TestGenerate_ReturnsError_When_NoConsignmentNumber()
		{
			var model = new CargoTrackerUrlModel
			{
				ClientCode = "WISGLOSYD",
			};

			var expectedErrorMessage = "Consol form must have a valid job number.";

			AssertGenerate_ReturnsError(ratingToken, model, expectedErrorMessage);
		}

		void AssertUrlEquals(Uri expectedUrl, Uri actualUrl)
		{
			AssertEquals("URL must match scheme, port, domain and path", expectedUrl.GetLeftPart(UriPartial.Path), actualUrl.GetLeftPart(UriPartial.Path));

			IEnumerable<string> ToKeyValuePairStrings(QueryString qs)
			{
				foreach (var k in qs.AllKeys)
				{
					yield return k + "=" + qs[k];
				}
			}

			var expectedQuery = ToKeyValuePairStrings(new QueryString(expectedUrl.Query.Substring(1)));
			var actualQuery = ToKeyValuePairStrings(new QueryString(actualUrl.Query.Substring(1)));

			AssertContainsExactElementsInAnyOrder("URL query must match by key value pair, in any order", expectedQuery, actualQuery);
		}

		IDisposable RegisterTokenProvider(string token = "authToken", string errorMessage = null)
		{
			var tokenProviderMock = new Mock<IAuthTokenProvider>();
			tokenProviderMock
				.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(),
					It.IsAny<CancellationToken>(), It.IsAny<bool>())).Returns((token, errorMessage));
			return ObjectFactory.Substitute(tokenProviderMock.Object);
		}

		const string CargoTrackerUrl = "https://abd991.cargowise.com:8443/";

		protected override void SetUp()
		{
			base.SetUp();

			generator = new CargoTrackerUrlGenerator();
			ratingToken = new CargoTrackerAuthToken(CargoTrackerAuthTokenType.Rating, "authToken");
		}

		CargoTrackerUrlGenerator generator;
		CargoTrackerAuthToken ratingToken;

		bool IsValidJwt(string token)
		{
			if (string.IsNullOrWhiteSpace(token))
			{
				return false;
			}

			var parts = token.Split('.');
			{
				if (parts.Length != 3)
				{
					return false;
				}
			}

			return true;
		}
	}
}
