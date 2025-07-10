#nullable enable
using System;
using System.Threading;
using AuthenticationService.Client.Models;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class UrsRatesClientFactoryTest : TestCaseWithFactory
	{
		public void TestTryCreate_TokenInvalid_ReturnNull()
		{
			using (RatingDataRegistry.Instance.RatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost:7200"))
			{
				var correlationID = "12345";
				var logger = new ElementaryLogger();
				var seconds = TimeSpan.FromSeconds(30);
				var cts = new CancellationTokenSource(seconds);

				var providerMock = new Mock<IAuthTokenProvider>();
				providerMock
					.Setup(s => s.GetToken(correlationID, 30, It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
					.Returns((string.Empty, "Email Required"))
					.Verifiable();

				// Setup temporary current user
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "test";
				staff.GS_EmailAddress = "Test@email.com";

				using var ursIntegration = RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using var ratesServiceSubscription = DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled());
				using var cargoSphereIntegration = DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using var userContext = Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK, factory: Factory));

				Env.Security.WiseRatesCargoSphereRateSearch.SetPropertyValue("IsAllowed", true);

				var client = UrsRatesClientFactory.TryCreate("SEA", "FCL", correlationID, logger, seconds, cts.Token, providerMock.Object);

				AssertNull(client);
				AssertContainsExactElementsInAnyOrder(["Unable to access Universal Rates Service because 'Email Required'"], logger.Errors);
				AssertContainsExactElementsInAnyOrder([], logger.Warnings);

				providerMock.VerifyAll();
			}
		}

		public void TestTryCreate_DifferentConfigurations()
		{
			// Setup temporary current user
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "test";
			var context = new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK, factory: Factory);
			using var userContext = Env.SetTemporaryUserContext(context);

			// Rates Service Subscription Disabled for SEA-FCL
			TestTryCreate(
				subscriptions: RatesServiceRegistrySettingsCollection.GetDisabled(),
				expectedWarnings: ["Unable to access Universal Rates Service because 'Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription'"]
			);

			// Urs Integration Disabled
			TestTryCreate(
				ursEnabled: false,
				expectedWarnings: ["Unable to access Universal Rates Service because 'The Universal Rates Service in the registry is disabled.'"]
			);

			// No email for current user
			TestTryCreate(
				staffEmailAddress: null,
				expectedErrors: ["Unable to access Universal Rates Service because 'Email address of current user in staff details is mandatory for accessing URS rates.'"]
			);

			// Trying to get SEA rates when CargoSphere is disabled
			TestTryCreate(
				cargoSphereEnabled: false,
				expectedWarnings: ["Unable to access Universal Rates Service because 'Request will not be sent to Rates Service because CGSP integration is disabled in the registry.'"]
			);

			// Trying to get SEA rates when CargoGuide is disabled, should be no issue
			TestTryCreate(
				cargoGuideEnabled: false,
				expectedWarnings: null
			);

			// Trying to get AIR rates when CargoGuide is disabled
			TestTryCreate(
				transportMode: "AIR",
				containerMode: "LSE",
				cargoGuideEnabled: false,
				expectedWarnings: ["Unable to access Universal Rates Service because 'Request will not be sent to Rates Service because CGGD integration is disabled in the registry.'"]
			);

			// Trying to get SEA rates when CargoSphere is enabled but user is not allowed
			TestTryCreate(
				userAllowedCargoSphereAccess: false,
				expectedWarnings: ["Unable to access Universal Rates Service because 'CGSP won't be accessed as access for current user is denied'"]
			);

			// Trying to get AIR rates when CargoGuide is enabled but user is not allowed
			TestTryCreate(
				transportMode: "AIR",
				containerMode: "LSE",
				userAllowedCargoGuideAccess: false,
				expectedWarnings: ["Unable to access Universal Rates Service because 'CGGD won't be accessed as access for current user is denied'"]
			);

			// No url
			TestTryCreate(
				ursUrl: string.Empty,
				expectedErrors: ["Unable to access Universal Rates Service because 'The Universal Rates Service URL is not configured'"]
			);

			// Correct configuration SEA rates
			TestTryCreate(
				transportMode: "SEA",
				containerMode: "FCL",
				expectedWarnings: null
			);

			// Correct configuration AIR rates
			TestTryCreate(
				transportMode: "AIR",
				containerMode: "LSE",
				expectedWarnings: null
			);

			// Support user no email for current user
			TestTryCreate(
				staffEmailAddress: null,
				isCWSupport: true,
				expectedWarnings: null
			);
		}

		void TestTryCreate(
			string transportMode = "SEA",
			string containerMode = "FCL",
			RatesServiceRegistrySettingsCollection? subscriptions = null,
			bool ursEnabled = true,
			string? staffEmailAddress = "test@email.com",
			bool cargoSphereEnabled = true,
			bool cargoGuideEnabled = true,
			bool userAllowedCargoSphereAccess = true,
			bool userAllowedCargoGuideAccess = true,
			string ursUrl = "https://default-url.com",
			bool isCWSupport = false,
			string[]? expectedWarnings = null,
			string[]? expectedErrors = null)
		{
			// Registry settings
			subscriptions ??= RatesServiceRegistrySettingsCollection.GetEnabled();
			using var ursIntegration = RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ursEnabled);
			using var ratesServiceSubscription = DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, subscriptions);
			using var cargoSphereIntegration = DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cargoSphereEnabled);
			using var cargoGuide = DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cargoGuideEnabled);
			using var ursUrlSetting = RatingDataRegistry.Instance.UniversalRatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ursUrl);

			// User setup
			GlbStaff.CurrentUser.GS_EmailAddress = staffEmailAddress ?? string.Empty;
			GlbStaff.CurrentUser.GS_LoginName = isCWSupport ? User.SupportUserName : "test";

			// Security setup
			Env.Security.WiseRatesCargoSphereRateSearch.SetPropertyValue("IsAllowed", userAllowedCargoSphereAccess);
			Env.Security.WiseRatesCargoguideRateSearch.SetPropertyValue("IsAllowed", userAllowedCargoGuideAccess);

			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock
				.Setup(s => s.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns(("McLaren", ""))
				.Verifiable();

			var logger = new ElementaryLogger();
			var client = UrsRatesClientFactory.TryCreate(transportMode, containerMode, "correlationId", logger, TimeSpan.FromSeconds(30), CancellationToken.None, authTokenProviderMock.Object);
			if (expectedWarnings != null)
			{
				AssertContainsExactElementsInAnyOrder(expectedWarnings, logger.Warnings);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder([], logger.Warnings);
			}

			if (expectedErrors != null)
			{
				AssertContainsExactElementsInAnyOrder(expectedErrors, logger.Errors);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder([], logger.Errors);
			}

			if (expectedErrors == null && expectedWarnings == null)
			{
				AssertNotNull(client);
			}
		}

		protected override void SetUp()
		{
			Globals.IsTest_ForTest.Value = false;
		}

		protected override void TearDown()
		{
			Globals.IsTest_ForTest.Value = true;
		}
	}
}
