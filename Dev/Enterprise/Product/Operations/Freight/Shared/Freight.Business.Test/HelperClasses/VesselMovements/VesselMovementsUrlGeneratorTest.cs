using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VesselMovementsUrlGeneratorTest : TestCaseWithFactory
	{
		public void TestGetToken_ReturnsError_When_MyAccountServiceReturnsError()
		{
			using (RegisterUserPortalClient(false))
			using (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RouteVisualizerUrl))
			{
				var tokenResult = generator.GetTokenAsync(ZGuid.NewZGuid().ToString()).Result;

				CombineAssertions(() =>
				{
					AssertNull(tokenResult.Token);
					AssertNull(tokenResult.RedirectUrl);
					AssertEquals("2000: something went wrong", tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_ReturnsError_When_MyAccountServiceReturnsError_ReturnsMyAccountPortalUrl_WhenMyAccountReturnsSuccessWithRedirection()
		{
			const string portalRedirectUrl = "https://myaccount-portal.cargowise.com/myaccount/Login/LoginLite.aspx?TabId=600&language=en-US&returnurl=%2f";

			using (RegisterUserPortalClient(true, portalRedirectUrl))
			using (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RouteVisualizerUrl))
			{
				var tokenResult = generator.GetTokenAsync(ZGuid.NewZGuid().ToString()).Result;

				CombineAssertions(() =>
				{
					AssertNull(tokenResult.Token);
					AssertUrlEquals(new Uri(portalRedirectUrl), tokenResult.RedirectUrl);
					AssertNull(tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_ReturnsError_When_NoAuthToken()
		{
			using (RegisterTokenProvider(null, "no token for you"))
			{
				var tokenResult = generator.GetTokenAsync(ZGuid.NewZGuid().ToString()).Result;

				CombineAssertions(() =>
				{
					AssertNull(tokenResult.Token);
					AssertNull(tokenResult.RedirectUrl);
					AssertEquals("Unable to get permission to show map: no token for you", tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_ReturnsError_When_RouteVisualizerUrlRegistryIsInvalid()
		{
			using (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "not a url"))
			{
				var tokenResult = generator.GetTokenAsync(ZGuid.NewZGuid().ToString()).Result;

				CombineAssertions(() =>
				{
					AssertNull(tokenResult.Token);
					AssertNull(tokenResult.RedirectUrl);
					AssertEquals("Invalid registry item: Freight > Global Tracking > Route Visualizer > Route Visualizer URL.", tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_WhenMyAccountReturnsSuccess()
		{
			using (RegisterUserPortalClient(true))
			using (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RouteVisualizerUrl))
			{
				var tokenResult = generator.GetTokenAsync(ZGuid.NewZGuid().ToString()).Result;

				CombineAssertions(() =>
				{
					AssertEquals(VesselMovementsAuthTokenType.MyAccount, tokenResult.Token.Type);
					AssertEquals("123456", tokenResult.Token.Value);
					AssertNull(tokenResult.RedirectUrl);
					AssertNull(tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_WhenRatingSuccess()
		{
			using (RegisterTokenProvider())
			{
				var tokenResult = generator.GetTokenAsync(ZGuid.NewZGuid().ToString()).Result;

				CombineAssertions(() =>
				{
					AssertEquals(VesselMovementsAuthTokenType.Rating, tokenResult.Token.Type);
					AssertEquals("authToken", tokenResult.Token.Value);
					AssertNull(tokenResult.RedirectUrl);
					AssertNull(tokenResult.ErrorMessage);
				});
			}
		}

		public void TestGetToken_UsesUseEnvironmentLoginInfoFromConstructor()
		{
			var branchWhenConstructingTokenProvider = new List<IBranch>();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "B01";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B02";
			Factory.Save();
			var tokenProviderMock = new Mock<IAuthTokenProvider>();

			ObjectFactory.Substitute(() =>
			{
				branchWhenConstructingTokenProvider.Add(Env.CurrentBranch);

				tokenProviderMock
					.Setup(m => m.GetToken(
						It.IsAny<string>(),
						It.IsAny<int>(),
						It.IsAny<Action<LoginInfo>>(),
						It.IsAny<CancellationToken>(),
						It.IsAny<bool>()))
					.Returns(() => ("mytoken", null));
				return tokenProviderMock.Object;
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var myGenerator = new VesselMovementsUrlGenerator();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var tokenResult = myGenerator.GetTokenAsync(ZGuid.NewZGuid().ToString()).Result;
				}
			}

			AssertSequencesEqual("the outer branch is active whewn constructing the token provider", new[] { branch1.PK.ToGuid() }, branchWhenConstructingTokenProvider.Select(t => t.PK));
			tokenProviderMock
				.Verify(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), true), Times.Once());
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
				.Callback((string _, int _, Action<LoginInfo> action, CancellationToken _, bool _) =>
				{
					action(loginInfo);
				})
				.Returns(("authToken", null));

			using (SubstituteTokenProvider(tokenProviderMock))
			{
				var tokenResult = generator.GetTokenAsync(ZGuid.NewZGuid().ToString(), contact, CancellationToken.None).Result;

				CombineAssertions(() =>
				{
					AssertEquals(contact.OC_ContactName, loginInfo.UserFullName);
					AssertEquals(contact.OC_Email, loginInfo.UserEmail);
					AssertEquals(org.OH_Code, loginInfo.ClientCompanyCode);
					AssertEquals(org.OH_FullName, loginInfo.ClientCompanyName);
				});
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGenerate_Throws_When_NoSupporter()
		{
			var token = new VesselMovementsAuthToken(VesselMovementsAuthTokenType.Rating, "authToken");
			generator.Generate(token, null, VesselMovementsUrlGeneratorOptions.None);
		}

		public void TestGenerate_ReturnsError_When_RouteVisualizerUrlRegistryIsInvalid()
		{
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "not a url"))
			{
				var (url, errorMessage) = generator.Generate(ratingToken, mockSupporter.Object, VesselMovementsUrlGeneratorOptions.None);

				CombineAssertions(() =>
				{
					AssertNull(url);
					AssertEquals("Invalid registry item: Freight > Global Tracking > Route Visualizer > Route Visualizer URL.", errorMessage);
				});
			}
		}

		public void TestGenerate_ReturnsError_When_SupporterReturnsError()
		{
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RouteVisualizerUrl))
			{
				mockSupporter.Setup(x => x.GetVesselMovementsUrlModel()).Returns((null, "Error message from supporter"));

				var (url, errorMessage) = generator.Generate(ratingToken, mockSupporter.Object, VesselMovementsUrlGeneratorOptions.None);

				CombineAssertions(() =>
				{
					AssertNull(url);
					AssertEquals("Error message from supporter", errorMessage);
				});
			}
		}

		public void TestGenerate_ReturnsError_When_NoVessel()
		{
			var model = new VesselMovementsUrlModel();

			var expectedErrorMessage = "Routing leg must have a vessel with valid IMO Number.";

			AssertGenerate_ReturnsError(ratingToken, model, expectedErrorMessage);
		}

		public void TestGenerate_ReturnsError_When_NoVesselIMO()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = string.Empty,
			};

			var expectedErrorMessage = "Routing leg must have a vessel with valid IMO Number.";

			AssertGenerate_ReturnsError(ratingToken, model, expectedErrorMessage);
		}

		public void TestGenerate_ReturnsError_When_InvalidVesselIMO()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "1111111"
			};

			var expectedErrorMessage = "Routing leg must have a vessel with valid IMO Number.";

			AssertGenerate_ReturnsError(ratingToken, model, expectedErrorMessage);
		}

		public void TestGenerate_ReturnsError_When_NoDepartureTimeAndNoArrivalTime()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652"
			};

			var expectedErrorMessage = "Routing leg must have a valid departure or arrival time.";

			AssertGenerate_ReturnsError(ratingToken, model, expectedErrorMessage);
		}

		public void TestGenerate_ReturnsError_When_ArrivalTimeIsBeforeDepartureTime()
		{
			var now = ZDateTime.Now;
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				ArrivalTime = now.AddSeconds(-1),
				DepartureTime = now,
			};

			var expectedErrorMessage = "Routing leg must have a valid departure or arrival time.";

			AssertGenerate_ReturnsError(ratingToken, model, expectedErrorMessage);
		}

		public void TestGenerate_ReturnsUrl_For_DepartureTimeWithoutArrivalTime()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T0523");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_ArrivalTimeWithoutDepartureTime()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				ArrivalTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&arrivalTime=20141031T0523");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_DepartureTimeAndArrivalTime()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T0523&arrivalTime=20141101T0624");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_DepartureTimeAndArrivalTimeWithOnlyDate()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31),
				ArrivalTime = new ZDateTime(2014, 11, 01),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031&arrivalTime=20141101");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_DepartureTimeAndArrivalTimeOneWithDateOnly()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031&arrivalTime=20141101T0624");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_DepartureTimeAndArrivalTimeWithSeconds()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 41),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 5),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T052341&arrivalTime=20141101T062405");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_CarrierCodeWithoutVoyageNumber()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
				CarrierCode = "MAEL",
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T0523&arrivalTime=20141101T0624");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_VoyageNumberWithoutCarrierCode()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
				VoyageNumber = "74N",
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T0523&arrivalTime=20141101T0624");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_CarrierCodeAndVoyageNumber()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
				CarrierCode = "MAEL",
				VoyageNumber = "74N",
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T0523&arrivalTime=20141101T0624&carrierCode=MAEL&voyageNumber=74N");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_ArrivalPortAndDeparturePort()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
				ArrivalPortUnloco = "NZAKL",
				DeparturePortUnloco = "AUPBT",
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T0523&arrivalTime=20141101T0624&arrivalPortUnloco=NZAKL&departurePortUnloco=AUPBT");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl);
		}

		public void TestGenerate_ReturnsUrl_For_CollapseInfoPanelOption()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 41),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 5),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T052341&arrivalTime=20141101T062405&collapseInfoPanel=true");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl, VesselMovementsUrlGeneratorOptions.CollapseInfoPanel);
		}

		public void TestGenerate_ReturnsUrl_For_CollapsePortCallsPanelOption()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 41),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 5),
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T052341&arrivalTime=20141101T062405&collapsePortCallsPanel=true");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl, VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel);
		}

		public void TestGenerate_ReturnsUrl_For_AllParameters()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
				CarrierCode = "MAEL",
				VoyageNumber = "74N",
				ArrivalPortUnloco = "NZAKL",
				DeparturePortUnloco = "AUPBT",
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?token=authToken&departureTime=20141031T0523&arrivalTime=20141101T0624&carrierCode=MAEL&voyageNumber=74N&arrivalPortUnloco=NZAKL&departurePortUnloco=AUPBT&collapseInfoPanel=true&collapsePortCallsPanel=true");

			AssertGenerate_ReturnsUrl(ratingToken, model, expectedUrl, VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel);
		}

		public void TestGenerate_ReturnsUrl_For_AllParameters_WithMyAccountToken()
		{
			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = "8507652",
				DepartureTime = new ZDateTime(2014, 10, 31, 5, 23, 0),
				ArrivalTime = new ZDateTime(2014, 11, 01, 6, 24, 0),
				CarrierCode = "MAEL",
				VoyageNumber = "74N",
				ArrivalPortUnloco = "NZAKL",
				DeparturePortUnloco = "AUPBT",
			};

			var expectedUrl = new Uri($"{RouteVisualizerUrl}movements/8507652?myaccount_token=123456&departureTime=20141031T0523&arrivalTime=20141101T0624&carrierCode=MAEL&voyageNumber=74N&arrivalPortUnloco=NZAKL&departurePortUnloco=AUPBT&collapseInfoPanel=true&collapsePortCallsPanel=true");

			AssertGenerate_ReturnsUrl(myAccountToken, model, expectedUrl, VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel);
		}

		void AssertGenerate_ReturnsError(VesselMovementsAuthToken token, VesselMovementsUrlModel model, string expectedErrorMessage)
		{
			using (RegisterTokenProvider())
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RouteVisualizerUrl))
			{
				mockSupporter.Setup(x => x.GetVesselMovementsUrlModel()).Returns((model, null));

				var (url, errorMessage) = generator.Generate(token, mockSupporter.Object, VesselMovementsUrlGeneratorOptions.None);

				CombineAssertions(() =>
				{
					AssertEquals(expectedErrorMessage, errorMessage);
					AssertNull(url);
				});
			}
		}

		void AssertGenerate_ReturnsUrl(VesselMovementsAuthToken token, VesselMovementsUrlModel model, Uri expectedUrl, VesselMovementsUrlGeneratorOptions options = VesselMovementsUrlGeneratorOptions.None)
		{
			using (RegisterTokenProvider())
			using (FreightDataRegistry.Instance.RouteVisualizerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RouteVisualizerUrl))
			{
				mockSupporter.Setup(x => x.GetVesselMovementsUrlModel()).Returns((model, null));

				var (url, errorMessage) = generator.Generate(token, mockSupporter.Object, options);

				CombineAssertions(() =>
				{
					AssertNull(errorMessage);
					AssertUrlEquals(expectedUrl, url);
				});
			}
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
				.Setup(m => m.GetToken(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<Action<LoginInfo>>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<bool>()))
				.Returns((token, errorMessage));

			return SubstituteTokenProvider(tokenProviderMock);
		}

		IDisposable SubstituteTokenProvider(Mock<IAuthTokenProvider> tokenProviderMock)
		{
			var subs = ObjectFactory.Substitute(tokenProviderMock.Object);
			generator = new VesselMovementsUrlGenerator();
			return subs;
		}

		IDisposable RegisterUserPortalClient(bool isSuccess, string url = null)
		{
			TrustedResponse<OAuthLoginResponse> response;
			if (!isSuccess)
			{
				response = new TrustedResponse<OAuthLoginResponse>("2000", "something went wrong");
			}
			else if (url is null)
			{
				response = new TrustedResponse<OAuthLoginResponse>(true, $"{{\"url\":\"{RouteVisualizerUrl}?token={myAccountToken.Value}\",\"token\":\"{myAccountToken.Value}\"}}");
			}
			else
			{
				response = new TrustedResponse<OAuthLoginResponse>(true, $"{{\"url\":\"{url}\"}}");
			}

			var clientMock = new Mock<IUserPortalClient>();
			clientMock.Setup(m => m.OAuthAutoLoginAsync(It.IsAny<Uri>())).Returns(Task.FromResult(response));

			return ObjectFactory.Substitute(clientMock.Object);
		}

		const string RouteVisualizerUrl = "https://abd991.cargowise.com:8443/";

		protected override void SetUp()
		{
			base.SetUp();

			mockSupporter = new Mock<IVesselMovementsUrlSupporter>();
			generator = new VesselMovementsUrlGenerator();
			ratingToken = new VesselMovementsAuthToken(VesselMovementsAuthTokenType.Rating, "authToken");
			myAccountToken = new VesselMovementsAuthToken(VesselMovementsAuthTokenType.MyAccount, "123456");
		}

		Mock<IVesselMovementsUrlSupporter> mockSupporter;
		VesselMovementsUrlGenerator generator;
		VesselMovementsAuthToken ratingToken;
		VesselMovementsAuthToken myAccountToken;
	}
}
