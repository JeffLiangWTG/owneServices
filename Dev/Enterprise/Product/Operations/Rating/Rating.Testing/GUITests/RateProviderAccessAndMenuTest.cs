using System;
using System.Threading;
using AuthenticationService.Client.Models;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.Testing
{
	using CargoWise.Types;
	using Enterprise.Integration.Rating;
	using Enterprise.Rating.Business.Testing;
	using Enterprise.ZArchitecture.Core;

	public class RateProviderAccessAndMenuTest : RatingTestCase
	{
		public void TestSubscriptionRegistryItems_DisabledShouldShowSubscriptionMessage()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new RatingFormForTest(costing, null))
			{
				var cargoSphereMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("CargoSphere - Ocean Rates");
				var cargoSphereRateSearchMenu = cargoSphereMenu.MenuItems.FindByText("Rate Search");
				var cargoSphereContractManagement = cargoSphereMenu.MenuItems.FindByText("SUDS");

				var expectedEnableCargoSphereSubscriptionMessage = @"CargoSphere ocean rates management integration is disabled in the registry:
AutoRating -> Rates Service -> Third Party Rate Providers -> CargoSphere -> Enable Ocean Rate Management Integration";

				var expectedEnableCargoguideSubscriptionMessage = @"Cargoguide air rates management integration is disabled in the registry:
AutoRating -> Rates Service -> Third Party Rate Providers -> Cargoguide -> Enable Air Rate Management Integration";

				using (DataRegistryRating.Instance.CargoSphereRateSearchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl"))
				using (DataRegistryRating.Instance.CargoSphereSUDSUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl"))
				using (DataRegistryRating.Instance.CargoguideRateSearchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl"))
				{
					using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						cargoSphereRateSearchMenu.PerformClick();
						AssertEquals("Enable integration message should show", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedEnableCargoSphereSubscriptionMessage));
					}

					using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						cargoSphereRateSearchMenu.PerformClick();
						AssertEquals("Enable integration message should not show", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedEnableCargoSphereSubscriptionMessage));
					}

					using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						cargoSphereContractManagement.PerformClick();
						AssertEquals("Enable integration message should show", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedEnableCargoSphereSubscriptionMessage));
					}

					using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						cargoSphereContractManagement.PerformClick();
						AssertEquals("Enable integration message should not show", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedEnableCargoSphereSubscriptionMessage));
					}

					var cargoguideRateSearchMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("Cargoguide - Air Rates");

					using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						cargoguideRateSearchMenu.PerformClick();
						AssertEquals("Enable integration message should show", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedEnableCargoguideSubscriptionMessage));
					}

					using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						cargoguideRateSearchMenu.PerformClick();
						AssertEquals("Enable integration message should not show", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedEnableCargoguideSubscriptionMessage));
					}
				}
			}
		}

		public void TestSecurityCheckingForMenuItems_CargoSphereRateSearch()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new RatingFormForTest(costing, null))
			{
				var cargoSphereMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("CargoSphere - Ocean Rates");
				var cargoSphereRateSearchMenu = cargoSphereMenu.MenuItems.FindByText("Rate Search");

				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.CargoSphereRateSearchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://someurl/"))
				{
					var checkpoint = Env.Security.WiseRatesCargoSphereRateSearch;
					var expectedMessage = checkpoint.ErrorMessageForNotAllowed;

					checkpoint.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereRateSearchMenu.PerformClick();
					AssertEquals("Providers and Settings message should show", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

					checkpoint.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereRateSearchMenu.PerformClick();
					AssertEquals("Providers and Settings message should show", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

					GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereRateSearchMenu.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Email address of current user in staff details is mandatory for accessing CargoSphere"));
				}
			}
		}

		public void TestSecurityCheckingForMenuItems_CargoSphereContractManagement()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new RatingFormForTest(costing, null))
			{
				var cargoSphereMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("CargoSphere - Ocean Rates");
				var cargoSphereContractManagementMenu = cargoSphereMenu.MenuItems.FindByText("SUDS");

				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.CargoSphereSUDSUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://someurl/"))
				{
					var checkpoint = Env.Security.WiseRatesCargoSphereContractManagement;
					var expectedMessage = checkpoint.ErrorMessageForNotAllowed;

					checkpoint.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereContractManagementMenu.PerformClick();
					AssertEquals("Providers and Settings message should show", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

					checkpoint.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereContractManagementMenu.PerformClick();
					AssertEquals("Providers and Settings message should show", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

					GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereContractManagementMenu.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Email address of current user in staff details is mandatory for accessing CargoSphere"));
				}
			}
		}

		public void TestSecurityCheckingForMenuItems_CargoguideRateSearch()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new RatingFormForTest(costing, null))
			{
				var cargoguideRateSearchMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("Cargoguide - Air Rates");

				using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.CargoguideRateSearchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://someurl/"))
				{
					var checkpoint = Env.Security.WiseRatesCargoguideRateSearch;
					var expectedMessage = checkpoint.ErrorMessageForNotAllowed;

					checkpoint.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoguideRateSearchMenu.PerformClick();
					AssertEquals("Providers and Settings message should show", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

					checkpoint.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoguideRateSearchMenu.PerformClick();
					AssertEquals("Providers and Settings message should show", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

					GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
					UnitTestUserNotification.Instance.ClearMessages();
					cargoguideRateSearchMenu.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Email address of current user in staff details is mandatory for accessing Cargoguide"));
				}
			}
		}

		public void TestSUDSSiteOpens()
		{
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereSUDSUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://someurl/"))
			{
				var notificationMsgText = "Cannot access CargoSphere SUDS page due to validation errors";

				var costing = Helper.NewCosting(Helper.NewOrgHeader());

				using (var form = new RatingFormForTest(costing, null, "validation errors"))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					WebUrlLauncher.ClearLastUrlLaunched();

					var cargoSphereMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("CargoSphere - Ocean Rates");
					var cargoSphereSUDSMenuItem = cargoSphereMenu.MenuItems.FindByText("SUDS");
					cargoSphereSUDSMenuItem.PerformClick();

					AssertEquals("Information Message should have been shown", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(notificationMsgText));
					AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);
				}

				var testToken = "123";

				using (var form = new RatingFormForTest(costing, testToken))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					WebUrlLauncher.ClearLastUrlLaunched();

					var cargoSphereMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("CargoSphere - Ocean Rates");
					var cargoSphereSUDSMenuItem = cargoSphereMenu.MenuItems.FindByText("SUDS");
					cargoSphereSUDSMenuItem.PerformClick();

					AssertEquals("Information Message should not have been shown", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(notificationMsgText));
					AssertEquals($"{DataRegistryRating.Instance.CargoSphereSUDSUrl.Value}?{RateProviderAccessAndMenu.UrlQueryKey.token}={testToken}", WebUrlLauncher.LastUrlLaunched);
				}
			}
		}

		public void TestSUDSSiteOpens_WhenOrganizationHasSCACCode()
		{
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereSUDSUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://website.com/page.jsp"))
			{
				var notificationMsgText = "Automatic login to CargoSphere SUDS page is currently unavailable.";

				var costing = Helper.NewCosting(Helper.NewOrgHeader());
				var testToken = "123";
				var scacCode = "CODD";

				costing.Header.SetCustomsCode(
					OrgCusCode.CodeTypes.CarrierCode,
					RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates),
					scacCode);

				using (var form = new RatingFormForTest(costing, testToken))
				{
					var cargoSphereMenu = form.Menu.MenuItems.FindByText("Rates Service").MenuItems.FindByText("CargoSphere - Ocean Rates");
					var cargoSphereSUDSMenuItem = cargoSphereMenu.MenuItems.FindByText("SUDS");
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereSUDSMenuItem.PerformClick();

					AssertEquals("Information Message should not have been shown", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(notificationMsgText));
					AssertEquals($"{DataRegistryRating.Instance.CargoSphereSUDSUrl.Value}?{RateProviderAccessAndMenu.UrlQueryKey.token}={testToken}&{RateProviderAccessAndMenu.UrlQueryKey.scac}={scacCode}", WebUrlLauncher.LastUrlLaunched);
				}
			}
		}

		public void TestGetConfigFromRatesService_EmptyValuesShouldShowMessageBoxes()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var testToken = "123";
			var scacCode = "CODD";

			costing.Header.SetCustomsCode(
				OrgCusCode.CodeTypes.CarrierCode,
				RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates),
				scacCode);

			using (var form = new RatingFormForTest(costing, testToken))
			{
				var wiseRatesClientMock = new Mock<IWiseRatesClient>();
				wiseRatesClientMock
					.Setup(x => x.GetConfiguration(It.IsAny<string>()))
					.Returns(new RatesServiceConfiguration
					{
						Providers = new ProvidersConfig
						{
							Cargoguide = new CargoguideConfig { ServiceUrl = "has a value", SiteSearchUrl = string.Empty },
							CargoSphere = new CargoSphereConfig { ServiceUrl = "has a value", SiteSearchUrl = string.Empty, SiteSudsUrl = string.Empty }
						}
					});

				var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
				clientFactoryMock
					.Setup(x => x.TryCreate(
						It.IsAny<string>(),
						It.IsAny<int>(),
						It.IsAny<CancellationToken>(),
						It.IsAny<ILogger>()))
					.Returns((wiseRatesClientMock.Object, string.Empty));

				form.RateProviderAccess.ClientFactoryMock = clientFactoryMock;

				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var menuItems = form.Menu.MenuItems.FindByText("Rates Service").MenuItems;
					var cargoSphereMenu = menuItems.FindByText("CargoSphere - Ocean Rates");

					var cargoSphereRateSearchMenu = cargoSphereMenu.MenuItems.FindByText("Rate Search");
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereRateSearchMenu.PerformClick();

					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("CargoSphere Rate Search URL setting could not be found. Please raise an eRequest."));

					var cargoSphereSUDSMenuItem = cargoSphereMenu.MenuItems.FindByText("SUDS");
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereSUDSMenuItem.PerformClick();

					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("CargoSphere SUDS URL setting could not be found. Please raise an eRequest."));

					var cargoguideRateSearchMenu = menuItems.FindByText("Cargoguide - Air Rates");
					UnitTestUserNotification.Instance.ClearMessages();
					cargoguideRateSearchMenu.PerformClick();

					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Cargoguide Rate Search URL setting could not be found. Please raise an eRequest."));
				}
			}
		}

		public void TestGetConfigFromRatesService_ValuesPresent_ShouldNotShowMessageBoxes()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var testToken = "123";
			var scacCode = "CODD";

			costing.Header.SetCustomsCode(
				OrgCusCode.CodeTypes.CarrierCode,
				RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates),
				scacCode);

			using (var form = new RatingFormForTest(costing, testToken))
			{
				var wiseRatesClientMock = new Mock<IWiseRatesClient>();
				wiseRatesClientMock
					.Setup(x => x.GetConfiguration(It.IsAny<string>()))
					.Returns(new RatesServiceConfiguration
					{
						Providers = new ProvidersConfig
						{
							Cargoguide = new CargoguideConfig { ServiceUrl = "has a value", SiteSearchUrl = "http://url.com" },
							CargoSphere = new CargoSphereConfig { ServiceUrl = "has a value", SiteSearchUrl = "http://url.com", SiteSudsUrl = "http://url.com" }
						}
					});

				var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
				clientFactoryMock
					.Setup(x => x.TryCreate(
						It.IsAny<string>(),
						It.IsAny<int>(),
						It.IsAny<CancellationToken>(),
						It.IsAny<ILogger>()))
					.Returns((wiseRatesClientMock.Object, string.Empty));

				form.RateProviderAccess.ClientFactoryMock = clientFactoryMock;

				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var menuItems = form.Menu.MenuItems.FindByText("Rates Service").MenuItems;
					var cargoSphereMenu = menuItems.FindByText("CargoSphere - Ocean Rates");

					var cargoSphereRateSearchMenu = cargoSphereMenu.MenuItems.FindByText("Rate Search");
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereRateSearchMenu.PerformClick();

					Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("CargoSphere Rate Search URL setting could not be found. Please raise an eRequest."));

					var cargoSphereSUDSMenuItem = cargoSphereMenu.MenuItems.FindByText("SUDS");
					UnitTestUserNotification.Instance.ClearMessages();
					cargoSphereSUDSMenuItem.PerformClick();

					Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("CargoSphere SUDS URL setting could not be found. Please raise an eRequest."));

					var cargoguideRateSearchMenu = menuItems.FindByText("Cargoguide - Air Rates");
					UnitTestUserNotification.Instance.ClearMessages();
					cargoguideRateSearchMenu.PerformClick();

					Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Cargoguide Rate Search URL setting could not be found. Please raise an eRequest."));
				}
			}
		}

		protected override void SetUp()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			base.SetUp();
		}
	}

	class RatingFormForTest : RatingForm
	{
		public RatingFormForTest(RatingHeader ratingHeader, string token, string validationMessage = "")
			: base(ratingHeader)
		{
			this.token = token;
			this.validationMessage = validationMessage;

			InitializeRatesServiceMenuItems();
		}

		protected new void InitializeRatesServiceMenuItems()
		{
			var ratesServiceMenu = new ZMenuItem((NoResString)"Rates Service");
			MainMenu.MenuItems.Add(ratesServiceMenu);

			RateProviderAccess = new RateProviderAccessAndMenuForTest(ratesServiceMenu, CurrentHeader, token, validationMessage);
		}

		public RateProviderAccessAndMenuForTest RateProviderAccess;

		readonly string token;
		readonly string validationMessage;
	}

	class RateProviderAccessAndMenuForTest : RateProviderAccessAndMenu
	{
		public RateProviderAccessAndMenuForTest(ZMenuItem parentMenu, RatingHeader ratingHeader, string token, string validationMessage)
			: base(parentMenu, ratingHeader)
		{
			this.authTokenProvider = new DummyAuthProvider(token, validationMessage);
		}

		protected override IWiseRatesClient CreateWiseRatesClient(string traceID)
		{
			if (ClientFactoryMock == null)
			{
				return base.CreateWiseRatesClient(traceID);
			}

			var (client, _) = ClientFactoryMock.Object.TryCreate(traceID);
			return client;
		}

		public Mock<IWiseRatesClientFactory> ClientFactoryMock;
	}

	class DummyAuthProvider : IAuthTokenProvider
	{
		readonly string testToken;

		public DummyAuthProvider(string testToken, string validationMessage)
		{
			this.testToken = testToken;
			this.validationMessage = validationMessage;
		}

		public string ClientID => "";

		public string AuthServiceURL => "";

		readonly string validationMessage;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public (string Token, string ValidationMessage) GetToken(string correlationID, int secondsBeforeTokenExpiry, Action<LoginInfo> overrideLoginInfo = null, CancellationToken cancellationToken = default(CancellationToken), bool useEnvironmentLoginInfoFromConstructor = false)
		{
			return (testToken, validationMessage);
		}
	}
}
