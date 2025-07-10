using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Rating.GUI.Test
{
	public class RateChooserFilterStripBusinessObjectTest : TestCaseWithFactory
	{
		#region Effective On

		public void TestEffectiveOnFilter_ReadOnly_GivenCanUpdateDateFalse() => TestEffectiveOnFilter_ReadOnly(canUpdateDate: false, expectedReadOnly: true);

		public void TestEffectiveOnFilter_ReadOnly_GivenCanUpdateDateTrue() => TestEffectiveOnFilter_ReadOnly(canUpdateDate: true, expectedReadOnly: false);

		void TestEffectiveOnFilter_ReadOnly(bool canUpdateDate, bool expectedReadOnly)
		{
			var autoRatingMock = new Mock<IAutoRating>();
			var mockJobDataUpdater = autoRatingMock.As<IJobDataUpdater>();
			mockJobDataUpdater.Setup(m => m.CanUpdateDate).Returns(canUpdateDate);

			TestEffectiveOnFilter_ReadOnly(autoRatingMock.Object, expectedReadOnly);
		}

		public void TestEffectiveOnFilter_ReadOnly_GivenJobConsolidation()
			=> TestEffectiveOnFilter_ReadOnly
			(
				autoRating: ChooserHelper.CreateConsol().RatingAdapter,
				expectedReadOnly: false
			);

		public void TestEffectiveOnFilter_ReadOnly_GivenJobShipment()
			=> TestEffectiveOnFilter_ReadOnly
			(
				autoRating: Factory.NewWithValidTestData<ForwardingShipment>().RatingAdapter,
				expectedReadOnly: true
			);

		public void TestEffectiveOnFilter_ReadOnly_GivenJobQuote()
		{
			var testHelper = new TestHelper(Factory);
			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking
			(
				Factory,
				transportMode: Constants.TransportModes.Sea,
				containerMode: Constants.ContainerModes.FCL,
				paymentTerms: ZString.Empty,
				client: testHelper.NewOrgHeader(),
				consignor: testHelper.NewOrgHeader(),
				consignee: testHelper.NewOrgHeader(),
				carrier: testHelper.NewOrgHeader(),
				origin: "USLAX",
				destination: "HKHKG",
				weight: 10m,
				volume: 1m,
				quotedBookingState: QuotedBookingState.QuoteOnly
			);
			TestEffectiveOnFilter_ReadOnly(autoRating: quotedBooking.Quote.CurrentOneOffQuote.RatingAdapter, expectedReadOnly: true);
		}

		void TestEffectiveOnFilter_ReadOnly(IAutoRating autoRating, bool expectedReadOnly)
		{
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var rateChooserFilterStripBusinessObject = new RateChooserFilterStripBusinessObject(criteria);
			var effectiveOnFilter = rateChooserFilterStripBusinessObject.GetFilters<ModuleSingleDateFilter>(RateEntryFilterUtility.Constants.Codes.EffectiveOn).Single();
			AssertEquals("EffectiveOn filter ReadOnly", expected: expectedReadOnly, effectiveOnFilter.ReadOnly);
		}

		public void TestEffectiveOnFilter_Consolidation()
		{
			var consol = ChooserHelper.CreateConsol();
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var rateChooserFilterStripBusinessObject = new RateChooserFilterStripBusinessObject(criteria);
			var effectiveOnFilter = rateChooserFilterStripBusinessObject.GetFilters<ModuleSingleDateFilter>(RateEntryFilterUtility.Constants.Codes.EffectiveOn).Single();
			AssertEquals("EffectiveOn filter ReadOnly", expected: false, effectiveOnFilter.ReadOnly);

			effectiveOnFilter.Property1 = ZDateTime.Empty;
			effectiveOnFilter.Validation.ValidateProperty1();
			AssertHasError("EffectiveOn filter Property1 error", effectiveOnFilter.Property1Info, "Please enter a value.");

			AssertEquals("EffectiveOn visibility", FilterVisibility.AlwaysVisible, effectiveOnFilter.Visibility);

			effectiveOnFilter.Property1 = new ZDateTime(2023, 1, 1);
			var (ratesQuery, _) = rateChooserFilterStripBusinessObject.BuildRatesQuery(new ElementaryLogger());
			AssertEquals("EffectiveOn Query", new ZDateTime(2023, 1, 1), ratesQuery.EffectiveDate);
		}

		#endregion

		public void TestLocationFilter_ReadOnly()
		{
			var consol = ChooserHelper.CreateConsol();
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testObject = new RateChooserFilterStripBusinessObject(criteria);
				var locationFilter = testObject.GetFilters<ModuleLocationFilter>(RateEntryFilterUtility.Constants.Codes.OriginDestination).First();

				Assert("Location filter should be readonly", locationFilter.ReadOnly);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testObject = new RateChooserFilterStripBusinessObject(criteria);
				var locationFilter = testObject.GetFilters<ModuleLocationFilter>(RateEntryFilterUtility.Constants.Codes.OriginDestination).First();

				Assert("Location filter should not be read only", !locationFilter.ReadOnly);
			}
		}

		public void TestBuildRatesQuery_GivenMultiRouteIsOn_RouteCreditorAndCarrierAreNotSet_QueryShouldBeCreated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "INVALID";
			carrier.OH_IsShippingProvider = true;
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var creditor = ChooserHelper.NewOrgHeader();

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.SetDefaultShippingLineAddress(carrier);
			consol.SetDefaultSendingForwarderAddress(ChooserHelper.NewOrgHeader());
			consol.SetDefaultReceivingForwarderAddress(ChooserHelper.NewOrgHeader());
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var route = consol.Transports[0];
			route.JW_IsLinked = false;
			consol.Transports[0].CreditorPK = ZGuid.Empty;
			consol.Transports[0].CarrierPK = ZGuid.Empty;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var filter = new RateChooserFilterStripBusinessObject(criteria);

			var testDate = ZDateTime.Today;
			var effectiveOnFilter = (ModuleSingleDateFilter)filter[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
			effectiveOnFilter.IsActive = true;
			effectiveOnFilter.Property1 = testDate;

			var locationFilter = (ModuleLocationFilter)filter[RateEntryFilterUtility.Constants.Codes.OriginDestination];
			locationFilter.IsActive = true;
			locationFilter.Property1 = "UAIEV";
			locationFilter.Property2 = "AUSYD";

			var carrierFilter = (ModuleGuidFilter)filter[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier.PK;

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testLogger = new ElementaryLogger();
				var (ratesQuery, _) = filter.BuildRatesQuery(testLogger);

				AssertNotNull(nameof(ratesQuery), ratesQuery);
				AssertEquals("SCAC", ratesQuery.Carrier.Single().SCACCode);
			}
		}

		public void TestBuildRatesQuery_GivenDisabledRateServiceRegistry_ShouldHaveWarningInLogger()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var filter = new RateChooserFilterStripBusinessObject(criteria);

			var testLogger = new ElementaryLogger();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (_, validForRatesService) = filter.BuildRatesQuery(testLogger);

				AssertEquals("Expected validForRatesService to be false when service is disabled.", false, validForRatesService);
				AssertCollectionContains(
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription",
					testLogger.Warnings
				);
			}

			testLogger.ClearLogs();
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (_, validForRatesService) = filter.BuildRatesQuery(testLogger);

				AssertEquals("Expected validForRatesService to be true when service is enabled.", true, validForRatesService);
				AssertCollectionNotContains(
					"Request will not be sent to Rates Service for SEA-FCL job as it is disabled in registry",
					testLogger.Warnings
				);
				AssertCollectionNotContains(
					"Request won't be sending to Rates Service for CGSP-SEA-FCL job as it is disabled in registry.",
					testLogger.Warnings
				);
			}
		}

		public void TestContainerTypeFilter_OnlyForFCL()
		{
			var consol = ChooserHelper.CreateConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var containerTypeFiler = filter[RateEntryFilterUtility.Constants.Codes.ContainerType];
			AssertNotNull("Container type should Not be null for FCL", containerTypeFiler);
		}

		public void TestNoContainerTypeFilter_ForLCL()
		{
			var consol = ChooserHelper.CreateConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var containerTypeFiler = filter[RateEntryFilterUtility.Constants.Codes.ContainerType];
			AssertNull("Container type should be null for LCL", containerTypeFiler);
		}

		public void TestBCNConsolWithEnabledBCNRegistry()
		{
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creditor = ChooserHelper.NewOrgHeader();

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
				consol.SetDefaultSendingForwarderAddress(ChooserHelper.NewOrgHeader());
				consol.SetDefaultReceivingForwarderAddress(ChooserHelper.NewOrgHeader());
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
				var container = consol.Containers.AddNew();

				container.JC_ContainerNum = "TEST1111117";
				container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

				Factory.Save();

				var ratingAdapter = consol.RatingAdapter;
				var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
				var criteria = new RatingCriteria(autoRatingProxy, Factory);

				var filter = new RateChooserFilterStripBusinessObject(criteria);

				var effectiveOnFilter = (ModuleSingleDateFilter)filter[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveOnFilter.IsActive = true;
				effectiveOnFilter.Property1 = ZDateTime.Today;

				var locationFilter = (ModuleLocationFilter)filter[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				locationFilter.IsActive = true;
				locationFilter.Property1 = "UAIEV";
				locationFilter.Property2 = "AUSYD";

				GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";

				var registrySettings = new RatesServiceRegistrySettingsCollection
				{
					new RatesServiceRegistrySettings(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol, true)
				};

				using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettings))
				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Container mode must be BCN", "BCN", criteria.ContainerMode);

					var testLogger = new ElementaryLogger();
					var (ratesQuery, isValidForRatesService) = filter.BuildRatesQuery(testLogger);

					AssertNotNull("Rates query cannot be null", ratesQuery);
					AssertEquals("Validation for rate service must succeed", true, isValidForRatesService);
					AssertEquals("We log build query errors as warnings", 0, testLogger.Warnings.Count);
					AssertCollectionContains("Transport mode must be SEA", "SEA", ratesQuery.TransportMode);
					AssertCollectionContains("Container mode must be FCL", "FCL", ratesQuery.ContainerMode);
				}
			}
		}

		public void TestBuildRatesQuery_MeasuresContainChargeableVolume()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_ConsolChargeable = 5;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var testLogger = new ElementaryLogger();
			var (ratesQuery, _) = filter.BuildRatesQuery(testLogger);
			var volumeMeasure = ratesQuery.Measures?.FirstOrDefault(x => x.Type == WiseRates.Api.Model.MeasureType.Volume);
			AssertNotNull(nameof(volumeMeasure), volumeMeasure);

			var expectedMeasure = $"Volume|5|M3";
			var actual = $"{volumeMeasure.Type}|{volumeMeasure.Amount}|{volumeMeasure.Unit}";

			AssertEquals("The volume measure should match the expected measure.", expectedMeasure, actual);
		}

		public void TestRateChooserFilterStripBusinessObject_WhenEffectiveOnFieldIsEmpty_ThenNoExceptionInGettingContractNumberFilterList()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filter = new RateChooserFilterStripBusinessObject(criteria);
				var effectiveOnFilter = (ModuleSingleDateFilter)filter[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveOnFilter.IsActive = true;
				effectiveOnFilter.Property1 = ZDateTime.Empty;

				var contractNumberFilter = (WiseRatesModuleTextFilter)filter[RateEntryFilterUtility.Constants.Description.CarrierContractNumber];
				contractNumberFilter.IsActive = true;
				AssertNoExceptionThrown("should load filter list without exception", () => getFilterList(contractNumberFilter));
			}

			IList getFilterList(WiseRatesModuleTextFilter filter)
			{
				return filter.List;
			}
		}

		protected RateChooserTestHelper ChooserHelper
		{
			get { return chooserHelper ?? (chooserHelper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper chooserHelper;
	}
}
