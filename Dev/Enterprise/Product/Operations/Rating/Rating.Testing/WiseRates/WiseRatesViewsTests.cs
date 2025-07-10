using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Rating.GUI;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static WiseRates.Api.Model.RatesSearchRequest;
using APIRefContainer = WiseRates.Api.Model.RefContainer;
using Constants = Enterprise.Core.Constants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.RatingTests.WiseRates
{
	[TestedType(typeof(WiseRatingHeaderView))]
	public class WiseRatingHeaderViewTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			return new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);
		}

		public void TestClearGridWhenNoResponseReceived()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			Factory.Save();

			var costing = new Rate
			{
				Carrier = "WiseCarrier",
				Charges = new List<Charge>(),
				ContainerMode = "FCL",
				Destination = "USLAX",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = "AUSYD",
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = "SEA",
				Provider = WRConstants.RateProviders.CargoSphere
			};
			costing.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 8m });

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "WiseCarrier", SCACCode = "SCAC", Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "FRT" } },
			};

			var noResponseException = new Exception("pretend error on getting rates");
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.SetupSequence(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesSearchResponse))
				.Throws(noResponseException);

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(It.IsAny<string>(), It.IsAny<int>(),It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery() });

			// On the first search, there was a response and it displayed one value.
			AssertEquals(1, wiseRatingHeaderView.WiseEntryViews.Count);
			AssertEquals(true, wiseRatingHeaderView.WiseEntryViews[0].TI_OH_TransportProvider.IsEmpty);

			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery() });
			AssertEquals(0, wiseRatingHeaderView.WiseEntryViews.Count);
			AssertEquals(noResponseException, ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestRecalculateWiseEntryViewsFromLastResponse()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			Factory.Save();

			var costing = new Rate
			{
				Carrier = "WiseCarrier",
				Charges = new List<Charge>(),
				ContainerMode = "FCL",
				Destination = "USLAX",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = "AUSYD",
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = "SEA",
				Provider = WRConstants.RateProviders.CargoSphere
			};
			costing.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 8m });

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "WiseCarrier", SCACCode = "SCAC", Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "FRT" } },
			};

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesSearchResponse));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery() });

			AssertEquals(1, wiseRatingHeaderView.WiseEntryViews.Count);
			AssertEquals(true, wiseRatingHeaderView.WiseEntryViews[0].TI_OH_TransportProvider.IsEmpty);

			var factory2 = new BusinessObjectFactory();
			var orgForEdit = (OrgHeader)factory2.ImportFromAnotherFactory(carrier);
			AddShippingLineToCarrier(factory2, orgForEdit, "SCAC");

			factory2.Save();

			wiseRatingHeaderView.RecalculateWiseEntryViewsFromLastResponse();
			AssertEquals(false, wiseRatingHeaderView.WiseEntryViews[0].TI_OH_TransportProvider.IsEmpty);
		}

		void AddShippingLineToCarrier(BusinessObjectFactory factory, OrgHeader carrier, string scac)
		{
			var shippingLine = factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = scac;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
		}
	}

	[TestedType(typeof(WiseEntryView))]
	public class WiseEntryViewTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewClientRate(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m);
			return new WiseEntryView(rateEntry, new RatesSearchResponse());
		}

		public void TestProperties_ShouldHaveCGReference_WhenItsAvailable_ForCargoGuideRate()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var wiseEntry = new WiseEntry(new Rate(), Factory);
			wiseEntry.CustomFields = new[] { new CustomField { Code = Rate.CustomFields.Cargoguide.Reference, Value = "REF1", Description = "Reference" } };

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = carrier.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("REF1", view.CGReference);

			// When it is not available. Expect empty string.
			wiseEntry.CustomFields = new[] { new CustomField { Code = Rate.CustomFields.Cargoguide.ProductName, Value = "meh", Description = "Something not a reference" } };
			view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals(string.Empty, view.CGReference);
		}

		public void TestUniversalCarrierServiceLevel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var rate = new Rate() { ServiceLevel = "UN1" };
			var wiseEntry = new WiseEntry(rate, Factory);

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = carrier.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("UN1", view.UniversalCarrierServiceLevel);
		}

		public void TestContainerQuality_Specified()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var rate = new Rate()
			{
				ProviderCustomFields = new[]
				{
					new CustomField { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "GOH" }
				},
			};
			var wiseEntry = new WiseEntry(rate, Factory);
			wiseEntry.CustomFields = rate.ProviderCustomFields;

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = carrier.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("GOH", view.ContainerQuality);
		}

		public void TestContainerQuality_NotSpecified()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var wiseEntry = new WiseEntry(new Rate(), Factory);
			wiseEntry.CustomFields = Array.Empty<CustomField>(); // There is no ContainerQuality specified

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = carrier.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("", view.ContainerQuality);
		}

		public void TestCargoguideProductCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var wiseEntry = new WiseEntry(new Rate(), Factory);
			wiseEntry.CustomFields = new[] { new CustomField { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "PROD1", Description = "Product1" } };

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = carrier.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("PROD1", view.CargoguideProductCode);
		}

		public void TestCommodities_ShouldBeEmpty_WhenWiseEntryCommoditiesIsNull()
		{
			var wiseEntry = new WiseEntry(new Rate(), Factory);

			AssertNull("Precondition: WiseEntry Commodities", wiseEntry.Commodities);

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertNullOrEmpty(
				"GIVEN rateEntry has null Commodities WHEN get WiseEntryView.Commodities THEN it should not throw ArgumentNullException",
				view.Commodities
			);
		}

		public void TestProperties_ShouldProxyValuesFromWiseEntry()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var wiseEntry = new WiseEntry(new Rate(), Factory);
			wiseEntry.TI_Mode = "AIR";
			wiseEntry.TI_RateCategory = "LSE";
			wiseEntry.TI_OriginLRC = "AUSYD";
			wiseEntry.TI_DestinationLRC = "UAIEV";
			wiseEntry.TI_RC = gp20.PK;
			wiseEntry.RateProvider = WRConstants.RateProviders.CargoGuide;
			wiseEntry.TI_RateStartDate = new ZDate(2020, 06, 06);
			wiseEntry.TI_RateEndDate = new ZDate(2030, 06, 06);
			wiseEntry.TI_RH_NKCommodityCode = "MCLAREN";
			wiseEntry.TI_PL_NKCarrierServiceLevel = "GOD";
			wiseEntry.TI_ContractNumber = "ZXC02192";
			wiseEntry.CommodityGroup = "CM1";

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = carrier.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			Factory.Save();

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("TI_Mode should be properly proxied", "AIR", view.TI_Mode);
			AssertEquals("TI_RateCategory should be properly proxied", "LSE", view.TI_RateCategory);
			AssertEquals("TI_OriginLRC should be properly proxied", "AUSYD", view.TI_OriginLRC);
			AssertEquals("TI_DestinationLRC should be properly proxied", "UAIEV", view.TI_DestinationLRC);
			AssertEquals("TI_RC should be properly proxied", gp20.PK, view.TI_RC);
			AssertEquals("RateProvider should be properly proxied", "Cargoguide", view.RateProvider);
			AssertEquals("TI_RateStartDate should be properly proxied", new ZDate(2020, 06, 06), view.TI_RateStartDate);
			AssertEquals("TI_RateEndDate should be properly proxied", new ZDate(2030, 06, 06), view.TI_RateEndDate);
			AssertEquals("TI_RH_NKCommodityCode should be properly proxied", "MCLAREN", view.TI_RH_NKCommodityCode);
			AssertEquals("TI_PL_NKCarrierServiceLevel should be properly proxied", "GOD", view.TI_PL_NKCarrierServiceLevel);
			AssertEquals("TI_ContractNumber should be properly proxied", "ZXC02192", view.TI_ContractNumber);
			AssertEquals("CommodityGroup should be properly proxied", "CM1", view.CommodityGroup);
		}

		public void TestCarrierOrgHeaderPK_UnderlyingRateHasCarrier_ReturnPK()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var wiseEntry = new WiseEntry(new Rate(), Factory);
			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = carrier.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("The CarrierOrgHeaderPK should match the carrier's PK.", carrier.PK, ((INeedCodeMappings)view).CarrierOrgHeaderPK);
		}

		public void TestCarrierOrgHeaderPK_UnderlyingRateHasNoCarrier_ReturnEmptyGuid()
		{
			var wiseEntry = new WiseEntry(new Rate(), Factory);
			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = ZGuid.Empty;
			wiseHeader.ChildRateEntries = new[] { wiseEntry };

			var view = new WiseEntryView(wiseEntry, new RatesSearchResponse());
			AssertEquals("The CarrierOrgHeaderPK should be ZGuid.Empty when there is no underlying carrier", ZGuid.Empty, ((INeedCodeMappings)view).CarrierOrgHeaderPK);
		}

		public void TestContainerPayload()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gp20.RC_GrossWeight = 500m;
			gp20.RC_TareWeight = 50m;
			gp20.RC_CubicCapacity = 2m;

			var wiseEntry = new WiseEntry(new Rate(), Factory);
			AssertEquals("Initial container payload weight should be 0.", 0m, wiseEntry.ContainerPayloadWeight);
			AssertEquals("Initial container payload volume should be 0.", 0m, wiseEntry.ContainerPayloadVolume);

			wiseEntry.TI_RC = gp20.PK;
			AssertEquals("Container payload weight after assigning GP20 should be 450.", 450m, wiseEntry.ContainerPayloadWeight);
			AssertEquals("Container payload volume after assigning GP20 should be 2.", 2m, wiseEntry.ContainerPayloadVolume);

			wiseEntry.ContainerPayloadWeightOverride = 666m;
			wiseEntry.ContainerPayloadVolumeOverride = 1.5m;
			AssertEquals("Overridden container payload weight should be 666.", 666m, wiseEntry.ContainerPayloadWeight);
			AssertEquals("Overridden container payload volume should be 1.5.", 1.5m, wiseEntry.ContainerPayloadVolume);
		}
	}

	[TestedType(typeof(WiseLineView))]
	public class WiseLineViewTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewClientRate(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m);
			return new WiseLineView(rateEntry.RateLines[0]);
		}
	}

	[TestedType(typeof(WiseLineItemView))]
	public class WiseLineItemViewTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewClientRate(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m);
			var rateLine = rateEntry.RateLines[0];
			return new WiseLineItemView(rateLine.RateLineItems[0]);
		}
	}

	[TestedType(typeof(WiseEntryViewsCollection))]
	public class WiseEntryViewsCollectionTests : NonPersistentBusinessObjectCollectionTestCase<WiseEntryViewsCollection>
	{
		protected override WiseEntryViewsCollection GetCollectionToTest()
		{
			return new WiseEntryViewsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewClientRate(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m);
			return new WiseEntryView(rateEntry, new RatesSearchResponse());
		}
	}

	[TestedType(typeof(WiseLineViewsCollection))]
	public class WiseLineViewsCollectionTests : NonPersistentBusinessObjectCollectionTestCase<WiseLineViewsCollection>
	{
		protected override WiseLineViewsCollection GetCollectionToTest()
		{
			return new WiseLineViewsCollection(Enumerable.Empty<IRateLine>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewClientRate(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m);
			return new WiseLineView(rateEntry.RateLines[0]);
		}
	}

	[TestedType(typeof(WiseLineItemViewsCollection))]
	public class WiseLineItemViewsCollectionTests : NonPersistentBusinessObjectCollectionTestCase<WiseLineItemViewsCollection>
	{
		protected override WiseLineItemViewsCollection GetCollectionToTest()
		{
			return new WiseLineItemViewsCollection(Enumerable.Empty<IRateLineItem>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewClientRate(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR", "LCL", "AUSYD", "USLAX");
			var rateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, "KG", "AUD");
			return new WiseLineItemView(rateLine.RateLineItems[0]);
		}
	}

	public class WiseRatesSearchTests : BaseRatingIntegrationTest
	{
		[GuiTest]
		public void TestSendRatesSearchRequest()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var costing = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			costing.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 8));

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = GetChargeCodesFromRates(costing),
			};

			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();

				var filterStripBizo = form.FilterControlForTest.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = new ZDateTime(2018, 3, 25);

				var originDestStrip1 = (ModuleLocationFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				var containerTypeStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.ContainerType];
				containerTypeStrip1.IsActive = true;
				containerTypeStrip1.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				var containerModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.ContainerMode];
				containerModeStrip1.IsActive = true;
				containerModeStrip1.Property = "LSE";

				var transportModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.TransportMode];
				transportModeStrip1.IsActive = true;
				transportModeStrip1.Property = "AIR";

				var carrierStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierStrip1.IsActive = true;
				carrierStrip1.Property = carrier.PK;

				form.FilterControlForTest.Find();

				Assert("Should load rates", wiseRatingHeaderView.WiseEntryViews.Count > 0);
			}
		}

		public void TestContextOperationShouldBeWiseRateSearchWithWiseRatesSearch()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.Is<RatesSearchRequest>(p => p.ContextOperation == Operation.WiseRateSearch), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);

			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);

			ErrorReporter.Instance.Clear();
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery { Origin = new[] { "AUSYD" } } });

			AssertEquals("We should not log an error if we search false", string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();

			wiseRatesClientMock
				.Verify(c =>
					c.SearchAsync(It.Is<RatesSearchRequest>(p => p.ContextOperation == Operation.WiseRateSearch),
						It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestProviderAccountsSecurityDenied()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var costing = CreateTestRate("XXX", "YYY", "ABCDE", "XYYXX", "BBB", "CFG", "UFC", "Fanta", "XXA", "AAX");
			costing.Charges.Add(CreatePerUnitCharge("ZZZX", "OO", "BTC", 8));
			costing.Provider = WRConstants.RateProviders.CargoGuide;
			var costing2 = CreateTestRate("XXX", "YYY", "ABCDE", "XYYXX", "BBB", "CFG", "UFC", "Fanta", "XXA", "AAX");
			costing2.Charges.Add(CreatePerUnitCharge("ZZZX", "OO", "BTC", 8));
			costing2.Provider = WRConstants.RateProviders.CargoSphere;

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing, costing2 },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "ZZZX" } },
			};

			Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = false;
			Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = false;
			SetupWiseRatesResponse
			(
				ratesSearchResponse,
				req => !(req.RatesQuery.AcceptedProviders.Contains(WRConstants.RateProviders.CargoSphere) || req.RatesQuery.AcceptedProviders.Contains(WRConstants.RateProviders.CargoGuide)),
				withRatesServiceClient: true
			);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
				wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery>
				{
					new RatesQuery
					{
						TransportMode = new[] { WRConstants.TransportModes.AIR },
						ContainerMode = new[] { Constants.ContainerModes.Loose, Constants.ContainerModes.FCL }
					},
					new RatesQuery
					{
						TransportMode = new[] { WRConstants.TransportModes.SEA },
						ContainerMode = new[] { Constants.ContainerModes.LCL, Constants.ContainerModes.FCL }
					}
				});

				Assert(!(wiseRatingHeaderView.WiseEntryViews.Count > 0));
			}
		}

		public void TestProviderAccountsSecurityAllowed()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var costing = CreateTestRate("XXX", "YYY", "ABCDE", "XYYXX", "BBB", "CFG", "UFC", "Fanta", "XXA", "AAX");
			costing.Charges.Add(CreatePerUnitCharge("ZZZX", "OO", "BTC", 8));
			costing.Provider = WRConstants.RateProviders.CargoGuide;
			var costing2 = CreateTestRate("XXX", "YYY", "ABCDE", "XYYXX", "BBB", "CFG", "UFC", "Fanta", "XXA", "AAX");
			costing2.Charges.Add(CreatePerUnitCharge("ZZZX", "OO", "BTC", 8));
			costing2.Provider = WRConstants.RateProviders.CargoSphere;

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing, costing2 },
				Carriers = new[]
				{
					new RefCarrier { Code = "Fanta", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "ZZZX" } },
			};

			Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = true;
			Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = true;
			SetupWiseRatesResponse
			(
				ratesSearchResponse,
				req => req.RatesQuery.AcceptedProviders.Contains(WRConstants.RateProviders.CargoSphere) || req.RatesQuery.AcceptedProviders.Contains(WRConstants.RateProviders.CargoGuide),
				withRatesServiceClient: true
			);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
				wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery>
				{
					new RatesQuery
					{
						TransportMode = new[] { WRConstants.TransportModes.AIR },
						ContainerMode = new[] { Constants.ContainerModes.Loose, Constants.ContainerModes.FCL }
					},
					new RatesQuery
					{
						TransportMode = new[] { WRConstants.TransportModes.SEA },
						ContainerMode = new[] { Constants.ContainerModes.LCL, Constants.ContainerModes.FCL }
					}
				});

				AssertEquals(4, wiseRatingHeaderView.WiseEntryViews.Count);
			}
		}

		public void TestINeedsCodeMapping()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var costing = CreateTestRate
			(
				category: "LSE",
				mode: "AIR",
				origin: "ABCDE",
				destination: "XYYXX",
				serviceLevel: "BBB",
				commodityCode: "CFG",
				containerType: "UFC",
				carrier: "Fanta",
				client: "XXA",
				controllingCustomer: "AAX"
			);
			costing.Container = new APIRefContainer { Code = "UFC" };
			costing.Charges.Add(CreatePerUnitCharge("ZZZX", "OO", "BTC", 8));

			var line = new WiseLine(Factory, costing.Charges[0]);
			line.Errors[RateLinesSchema.TL_AC] = "Invalid";
			line.Errors[RateLinesSchema.TL_RX_NKCurrency] = "Invalid";

			var entry = new WiseEntry(costing, Factory);
			entry.Errors[RateEntrySchema.TI_OriginLRC] = "Invalid";
			entry.Errors[RateEntrySchema.TI_DestinationLRC] = "Invalid";
			entry.Errors[RateEntrySchema.TI_PL_NKCarrierServiceLevel] = "Invalid";
			entry.Errors[RateEntrySchema.TI_RH_NKCommodityCode] = "Invalid";
			entry.Errors[RateEntrySchema.TI_RC] = "Invalid";
			entry.Errors[RateEntrySchema.TI_OH_ControllingCustomer] = "Invalid";
			entry.ChildRateLines = new[] { line };

			var header = new WiseHeader(Factory);
			header.Errors[RatingHeaderSchema.TH_OH] = "Invalid";
			header.ChildRateEntries = new[] { entry };

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "Fanta", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "ZZZX" } },
			};

			var view = new WiseEntryView(entry, ratesSearchResponse);
			view.Validation.ValidateAll();

			var expectedCodes = new[]
			{
				new UnmappedForeignCode("CFG", Constants.OrgPatternMatchOverrideRelationships.Commodities),
				new UnmappedForeignCode("AAX", Constants.OrgPatternMatchOverrideRelationships.Organisation),
				new UnmappedForeignCode("ABCDE", Constants.OrgPatternMatchOverrideRelationships.Port),
				new UnmappedForeignCode("XYYXX", Constants.OrgPatternMatchOverrideRelationships.Port),
				new UnmappedForeignCode("BBB", Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel),
				new UnmappedForeignCode("UFC", Constants.OrgPatternMatchOverrideRelationships.ContainerType),
				new UnmappedForeignCode("Fanta", Constants.OrgPatternMatchOverrideRelationships.Organisation)
			};

			Assert("Expected NeedsCodeMapping to be true", ((INeedCodeMappings)view).NeedsCodeMapping);
			AssertContainsExactElementsInAnyOrder(
				"Unmapped codes must match the expected codes",
				expectedCodes,
				((INeedCodeMappings)view).UnmappedCodes
			);

			var wiseLineView = view.ChildWiseRateLineViews[0] as INeedCodeMappings;
			expectedCodes = new[]
			{
				new UnmappedForeignCode("ZZZX", Constants.OrgPatternMatchOverrideRelationships.ChargeCodes),
				new UnmappedForeignCode("BTC", Constants.OrgPatternMatchOverrideRelationships.Currency)
			};

			Assert("Expected NeedsCodeMapping to be true for WiseLineView", wiseLineView.NeedsCodeMapping);
			AssertContainsExactElementsInAnyOrder(
				"Unmapped codes for WiseLineView must match the expected codes",
				expectedCodes,
				wiseLineView.UnmappedCodes
			);

			var validCosting = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			validCosting.Charges.Add(CreateFlatCharge("FRT", "AUD", 8));

			ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { validCosting },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "FRT" } },
			};
		}

		public void TestSearchResponseCache_CacheBypassedForWiderQuery()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var costing1 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "20GP", "Emirates", "", "");
			costing1.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 8));
			costing1.Provider = WRConstants.RateProviders.CargoGuide;

			var costing2 = CreateTestRate("AIR", "LCL", "AUMEL", "USLAX", "", "", "40GP", "Emirates", "", "");
			costing2.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 8));
			costing2.Provider = WRConstants.RateProviders.WTG;

			var costing3 = CreateTestRate("AIR", "LCL", "NZAKL", "USLAX", "", "", "UL8", "Emirates", "", "");
			costing3.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 8));
			costing3.Provider = WRConstants.RateProviders.CargoSphere;

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing1, costing2, costing3 },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName },
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "CARR_SPECZZZX" } },
			};

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesSearchResponse));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);

			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery { Origin = new[] { "AUSYD" } } });
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery() }); //Wider Query - should trigger service call

			AssertNoExceptionThrown("Rates Service should be called twice", () => wiseRatesClientMock.VerifyAll());

			wiseRatesClientMock
				.Verify(
					c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(),
						It.IsAny<CancellationToken>()), Times.Exactly(2));
		}

		public void TestMinimumFreightCostAndRatePerChargeableUnit()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var costing1 = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			costing1.Charges.Add(CreateFlatCharge("FSC", "AUD", 10m));
			costing1.Charges.Add(CreateFlatCharge("BAF", "AUD", 30m));
			costing1.Charges.Add(CreateFlatCharge("CAF", "HKD", 30m));
			costing1.Charges.Add(CreateFlatCharge("WAR", "HKD", 40m));
			costing1.Charges.Add(CreatePerUnitCharge("FRT", "CN", "AUD", 8m));
			costing1.Charges.Add(CreatePerUnitCharge("OFORW", "CN", "AUD", 2m));
			costing1.Charges.Add(CreatePerUnitCharge("EFAF", "CN", "USD", 7m));
			costing1.Charges.Add(CreatePerUnitCharge("PSS", "CN", "USD", 8m));

			var costing2 = CreateTestRate("SEA", "FCL", "AUSYD", "NZAKL", "", "", "", "Emirates", "", "");
			costing2.Charges.Add(CreateFlatCharge("FRT", "AUD", 10m));
			costing2.Charges.Add(CreatePerUnitCharge("BAF", "CN", "AUD", 8m));
			costing2.Charges.Add(CreateMinCharge("CAF", "AUD", "", 15m));
			costing2.Charges.Add(CreateMinCharge("WAR", "AUD", "", 25m));
			costing2.Charges.Add(CreateMinCharge("PSS", "USD", "", 10m));
			costing2.Charges.Add(CreateMinCharge("OFORW", "USD", "", 13m));

			var costing3 = CreateTestRate("SEA", "FCL", "AUSYD", "HKHKG", "", "", "", "Emirates", "", "");
			costing3.Charges.Add(CreatePerUnitCharge("FRT", "CN", "AUD", 10m));
			costing3.Charges.Add(CreatePerUnitCharge("FRT", "PLT", "AUD", 10m));
			costing3.Charges.Add(CreatePerUnitCharge("FRT", "BOX", "AUD", 20m));

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing1, costing2, costing3 },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName },
				},
				ChargeCodes = GetChargeCodesFromRates(costing1, costing2, costing3),
			};

			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery() });

			Assert(wiseRatingHeaderView.WiseEntryViews.Count > 0);

			var wiseEntryView1 = wiseRatingHeaderView.WiseEntryViews[0];
			var wiseEntryView2 = wiseRatingHeaderView.WiseEntryViews[1];
			var wiseEntryView3 = wiseRatingHeaderView.WiseEntryViews[2];

			AssertEquals("AUD 50.0000 + HKD 70.0000 + USD 15.0000", wiseEntryView1.AllInCost);
			AssertEquals("AUD 8.0000/CN", wiseEntryView1.FreightRatePerChargeableUnit);

			AssertEquals("AUD 58.0000 + USD 23.0000", wiseEntryView2.AllInCost);
			AssertEquals("", wiseEntryView2.FreightRatePerChargeableUnit);

			AssertEquals("AUD 40.0000", wiseEntryView3.AllInCost);
			AssertEquals("AUD 10.0000/CN + AUD 10.0000/PLT + AUD 20.0000/BOX", wiseEntryView3.FreightRatePerChargeableUnit);
		}

		public void TestAllInCost_OriginalRateToRateFactorIsUsedIfCurrencyIsUSD()
		{
			var costing1 = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "", "", "");
			// charges with valid custom fields
			AddFlatCharge(costing1, "FSC", "AUD", "USD", 10m, 0.75m);
			AddFlatCharge(costing1, "BAF", "HKD", "USD", 12m, 0.5m);
			AddFlatCharge(costing1, "CAF", "INR", "USD", 14m, 0.25m);
			// charges with invalid custom fields
			AddFlatCharge(costing1, "WAR", "AUD", "AUD", 16m, 0.75m); // targetCurrency is AUD
			AddFlatCharge(costing1, "BAF", "AUD", "", 17m, 0.75m); // targetCurrency is blank
			AddFlatCharge(costing1, "CAF", "AUD", null, 18m, 0.75m); // targetCurrency is null
			AddFlatCharge(costing1, "FRT", "HKD", "USD", 18m, 0m); // originalRateToRateFactor is zero
			AddFlatCharge(costing1, "OFORW", "INR", "USD", 20m, null); // originalRateToRateFactor is null
																	   // charges with no custom fields
			costing1.Charges.Add(CreateFlatCharge("FRT", "USD", 22m)); // with no custom fields but with USD
			costing1.Charges.Add(CreateFlatCharge("OFORW", "AUD", 24m)); // with no custom fields but with AUD

			var costing2 = CreateTestRate("SEA", "FCL", "AUSYD", "NZAKL", "", "", "", "", "", "");
			AddFlatCharge(costing2, "CAF", "INR", "USD", 14m, 0.25m);
			AddPerUnitCharge(costing2, "WAR", "INR", "USD", 14m, 0.5m);
			costing2.Charges.Add(CreateFlatCharge("FSC", "INR", 22m));
			costing2.Charges.Add(CreatePerUnitCharge("BAF", "KG", "INR", 24m));

			var costing3 = CreateTestRate("SEA", "FCL", "AUSYD", "NZAKL", "", "", "", "", "", "");
			AddMinCharge(costing3, "CAF", "AUD", "USD", 15m, 0.5m);
			AddMinCharge(costing3, "FSC", "INR", "USD", 10m, 0.25m);
			costing3.Charges.Add(CreateMinCharge("WAR", "AUD", "", 25m));
			costing3.Charges.Add(CreateMinCharge("PSS", "USD", "", 10m));

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing1, costing2, costing3 },
				ChargeCodes = GetChargeCodesFromRates(costing1),
			};
			SetupWiseRatesResponse(ratesSearchResponse);

			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery() });
			Assert(wiseRatingHeaderView.WiseEntryViews.Count > 0);

			//Total
			//in USD = 7.5(FSC) + 6(BAF) + 3.5(CAF) + 22(FRT) = 39
			//in AUD = 16(WAR) + 17(BAF) + 18(CAF) + 24(OFORW) = 75
			//in HKD = 18(FRT)
			//in INR = 20(OFORW)
			AssertEquals("USD 39.0000 + AUD 75.0000 + HKD 18.0000 + INR 20.0000", wiseRatingHeaderView.WiseEntryViews[0].AllInCost);

			//Total
			//in USD = 3.5(CAF) + 7(WAR) = 10.5
			//in INR = 22(FSC) + 24(BAF)
			AssertEquals("USD 10.5000 + INR 46.0000", wiseRatingHeaderView.WiseEntryViews[1].AllInCost);

			//Total
			//in USD = 7.5(CAF) + 2.5(FSC) + 10(PSS) = 20
			//in AUD = 25(WAR)
			AssertEquals("USD 20.0000 + AUD 25.0000", wiseRatingHeaderView.WiseEntryViews[2].AllInCost);

			void AddFlatCharge(Rate rate, string chargeCode, string originalCurrency, string targetCurrency, decimal originalRate, decimal? originalRateToRateFactor)
			{
				var charge = CreateFlatCharge(chargeCode, originalCurrency, originalRate);
				charge.ProviderCustomFields = GetCustomFieldWithOriginalRateToRateFactor(originalRateToRateFactor, targetCurrency);
				rate.Charges.Add(charge);
			}

			void AddPerUnitCharge(Rate rate, string chargeCode, string originalCurrency, string targetCurrency, decimal originalRate, decimal? originalRateToRateFactor)
			{
				var charge = CreatePerUnitCharge(chargeCode, "KG", originalCurrency, originalRate);
				charge.ProviderCustomFields = GetCustomFieldWithOriginalRateToRateFactor(originalRateToRateFactor, targetCurrency);
				rate.Charges.Add(charge);
			}

			void AddMinCharge(Rate rate, string chargeCode, string originalCurrency, string targetCurrency, decimal originalRate, decimal? originalRateToRateFactor)
			{
				var charge = CreateMinCharge(chargeCode, originalCurrency, "", originalRate);
				charge.ProviderCustomFields = GetCustomFieldWithOriginalRateToRateFactor(originalRateToRateFactor, targetCurrency);
				rate.Charges.Add(charge);
			}

			IEnumerable<CustomField> GetCustomFieldWithOriginalRateToRateFactor(decimal? originalRateToRateFactor, string currency)
			{
				return new[] {
					new CustomField() {
						Code = Rate.CustomFields.CargoSphere.CurrencyCode,
						Value = currency
					},
					new CustomField() {
						Code = Rate.CustomFields.CargoSphere.OriginalRateToRateFactor,
						Value = originalRateToRateFactor
					}
				};
			}
		}

		public void TestFreightInclusiveCalculator_GetChargeCodes()
		{
			var chargeCodeBAF = Helper.ChargeCodes["BAF"];

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var costing = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			costing.Charges.Add(CreateFlatCharge("FSC", "AUD", 10m));
			var inclusiveCharge = CreateInclusiveCharge(chargeCodeBAF.AC_Code, Constants.CurrencyCodes.Australia, "FSC");
			costing.Charges.Add(inclusiveCharge);

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName },
				},
				ChargeCodes = GetChargeCodesFromRates(costing),
			};

			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery() });

			Assert(wiseRatingHeaderView.WiseEntryViews.Count > 0);

			var wiseRateLine = wiseRatingHeaderView.WiseEntryViews[0].ChildRateLines.Single(l => l.TL_AC == chargeCodeBAF.PK);
			var freightInclusiveCalculator = wiseRateLine.GetCalculator<FreightInclusiveCalculator>();
			var chargeCodesForFreightInclusiveCalculator = freightInclusiveCalculator.ChargeCodes;

			chargeCodesForFreightInclusiveCalculator.Load();
			Assert(chargeCodesForFreightInclusiveCalculator.Any());
		}

		public void TestGivenEmptyWiseRatesClient_WhenSendRatesRequest_NoExceptionBeThrowed()
		{
			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((null,"No Client"));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);

			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);
			AssertNoExceptionThrown("Should not throw any exception", () => wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { new RatesQuery { Origin = new[] { "AUSYD" } } }));
		}

		[GuiTest]
		public void TestSlidingCalculator()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 1, minRate: 50));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 45, 2, minRate: 50));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 100, 3, minRate: 50));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 150, 4, minRate: 50));

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = GetChargeCodesFromRates(costing),
			};

			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();

				var filterStripBizo = form.FilterControlForTest.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = new ZDateTime(2018, 3, 25);

				var originDestStrip1 = (ModuleLocationFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				var containerTypeStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.ContainerType];
				containerTypeStrip1.IsActive = true;
				containerTypeStrip1.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				var containerModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.ContainerMode];
				containerModeStrip1.IsActive = true;
				containerModeStrip1.Property = "LSE";

				var transportModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.TransportMode];
				transportModeStrip1.IsActive = true;
				transportModeStrip1.Property = "AIR";

				var carrierStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierStrip1.IsActive = true;
				carrierStrip1.Property = carrier.PK;

				form.FilterControlForTest.Find();

				Assert("Should load rates", wiseRatingHeaderView.WiseEntryViews.Count > 0);
			}
		}

		[GuiTest]
		public void TestConversionFactorAndUnitMultiple()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];
			var costing = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");

			var charge = CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 1, minRate: 50);
			charge.ConversionFactor = 100;
			charge.ConversionFactorUnit = "KG";
			charge.ConversionFactorDenominatorUnit = "CC";
			charge.UnitMultiplier = 800;

			costing.Charges.Add(charge);

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = GetChargeCodesFromRates(costing),
			};

			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);
			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();

				var filterStripBizo = form.FilterControlForTest.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = new ZDateTime(2018, 3, 25);

				var originDestStrip1 = (ModuleLocationFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				var containerTypeStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.ContainerType];
				containerTypeStrip1.IsActive = true;
				containerTypeStrip1.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				var containerModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.ContainerMode];
				containerModeStrip1.IsActive = true;
				containerModeStrip1.Property = "LSE";

				var transportModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.TransportMode];
				transportModeStrip1.IsActive = true;
				transportModeStrip1.Property = "AIR";

				var carrierStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierStrip1.IsActive = true;
				carrierStrip1.Property = carrier.PK;

				form.FilterControlForTest.Find();

				Assert("Should load rates", wiseRatingHeaderView.WiseEntryViews.Count > 0);
				var wiseRateLine = wiseRatingHeaderView.WiseEntryViews[0].ChildRateLines.Single(l => l.TL_AC == chargeCodeFRT.PK);
				AssertEquals("Should have value for Unit Multiple", "800", wiseRateLine.UnitMultipleAsString);
				AssertEquals("Should have value for ConversionFactor", "100 KG/CC", wiseRateLine.ConversionFactorForBinding.ConversionFactorString);
			}
		}

		public void TestRateEntryViewValidation_UnderlyingRateLinesHaveErrors_AddErrorsToEntry()
		{
			var costing = CreateTestRate("AIR", "FCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");

			var entry = new WiseEntry(costing, Factory);
			var response = new RatesSearchResponse { Rates = new[] { costing } };

			var wiseEntryView = new WiseEntryView(entry, response);
			wiseEntryView.RunPreSaveValidation();
			AssertEquals("[Precondition] Entry has errors if rate line is fine", false, wiseEntryView.HasErrors);

			entry.Errors[RateEntrySchema.TI_OH_TransportProvider] = "Some error";
			wiseEntryView = new WiseEntryView(entry, response);
			wiseEntryView.RunPreSaveValidation();
			AssertEquals("Entry has errors if rate line has errors", true, wiseEntryView.HasErrors);
		}

		[GuiTest]
		public void TestRatesServiceSearchWarnings_NotDuplicate()
		{
			var carrierSCAC = Factory.NewWithValidTestData<OrgHeader>();
			carrierSCAC.OH_Code = "Emirates";
			carrierSCAC.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrierSCAC, "SCAC");

			var carrierFNTA = Factory.NewWithValidTestData<OrgHeader>();
			carrierFNTA.OH_Code = "Fanta";
			carrierFNTA.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrierFNTA, "FNTA");

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "UFC";
			container.RC_ISOType = "22GO";

			Factory.Save();

			var costingA = CreateTestRate("XXX", "YYY", "AUSYD", "USLAX", "BBB", "CFG", "UFC", "Fanta", "XXA", "AAX");
			costingA.Charges.Add(CreatePerUnitCharge("UNM", "OO", "BTC", 8));

			var costingB = CreateTestRate("XXX", "YYY", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			costingB.Charges.Add(CreatePerUnitCharge("UNM", "OO", "BTC", 8));

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costingA, costingB },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrierSCAC.SCACCode, Name = carrierSCAC.OH_FullName },
					new RefCarrier { Code = "Fanta", SCACCode = carrierFNTA.SCACCode , Name = carrierFNTA.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "UNM", Group = "ORG" } },
			};

			SetupWiseRatesResponse(ratesSearchResponse, withRatesServiceClient: true);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();

				var filterStripBizo = form.FilterControlForTest.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = ZDate.Today;

				var originDestStrip1 = (ModuleLocationFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				form.FilterControlForTest.Find();

				var expectedWarningSea = "Warning:Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription";
				AssertNotNull("Logs should not have any duplicated warning line", wiseRatingHeaderView.Logger.Warnings.SingleOrDefault(w => w.Contains(expectedWarningSea)));

				var expectedWarningAir = "Warning:Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service Subscription";
				AssertNotNull("Logs should not have any duplicated warning line", wiseRatingHeaderView.Logger.Warnings.SingleOrDefault(w => w.Contains(expectedWarningAir)));

				form.FilterControlForTest.Find();

				AssertNotNull("Logs should be still the same as there was no change in criteria and there should not be any duplicated lines", wiseRatingHeaderView.Logger.Warnings.SingleOrDefault(w => w.Contains(expectedWarningSea)));
				AssertNotNull("Logs should be still the same as there was no change in criteria and there should not be any duplicated lines", wiseRatingHeaderView.Logger.Warnings.SingleOrDefault(w => w.Contains(expectedWarningAir)));

				var carrierStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierStrip1.IsActive = true;
				carrierStrip1.Property = carrierSCAC.PK;

				form.FilterControlForTest.Find();

				AssertNotNull("Costing A has been filtered. The warnings should remain the same", wiseRatingHeaderView.Logger.Warnings.SingleOrDefault(w => w.Contains(expectedWarningSea)));
				AssertNotNull("Costing A has been filtered. The warnings should remain the same", wiseRatingHeaderView.Logger.Warnings.SingleOrDefault(w => w.Contains(expectedWarningAir)));
			}
		}

		[GuiTest]
		public void TestValidation_ValidAirLSE_InvalidAirLCL_ShowOnlyWarning()
		{
			TransportContainerModePair invalidPair = new TransportContainerModePair("AIR", "LCL");
			TransportContainerModePair validPair = new TransportContainerModePair("AIR", "LSE");

			AssertTransportAndContainerModeValidationWarning(validPair, invalidPair);
		}

		[GuiTest]
		public void TestValidation_ValidSeaFCL_InvalidAirFCL_ShowOnlyWarning()
		{
			TransportContainerModePair invalidPair = new TransportContainerModePair("AIR", "FCL");
			TransportContainerModePair validPair = new TransportContainerModePair("SEA", "FCL");

			AssertTransportAndContainerModeValidationWarning(validPair, invalidPair);
		}

		[GuiTest]
		public void TestValidation_ValidSeaLCL_InvalidSeaLSE_ShowOnlyWarning()
		{
			TransportContainerModePair invalidPair = new TransportContainerModePair("SEA", "LSE");
			TransportContainerModePair validPair = new TransportContainerModePair("SEA", "LCL");

			AssertTransportAndContainerModeValidationWarning(validPair, invalidPair);
		}

		[GuiTest]
		public void TestValidation_AllTransportAndContainerModeOptionsSelected_ShowOnlyWarning()
		{
			var invalidPairs = new List<TransportContainerModePair>();
			var validPairs = new List<TransportContainerModePair>();

			invalidPairs.Add(new TransportContainerModePair("SEA", "LSE"));
			invalidPairs.Add(new TransportContainerModePair("AIR", "FCL"));
			invalidPairs.Add(new TransportContainerModePair("AIR", "LCL"));

			validPairs.Add(new TransportContainerModePair("SEA", "LCL"));
			validPairs.Add(new TransportContainerModePair("AIR", "LSE"));
			validPairs.Add(new TransportContainerModePair("SEA", "FCL"));
			AssertAllTransportAndContainerModeValidationWarnings(validPairs, invalidPairs);
		}

		[GuiTest]
		public void TestValidation_AllValidTransportAndContainerModeOptionsDisabledInRegistry_ShowOnlyWarning()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var validPairs = new List<TransportContainerModePair>();
			validPairs.Add(new TransportContainerModePair("SEA", "LCL"));
			validPairs.Add(new TransportContainerModePair("AIR", "LSE"));
			validPairs.Add(new TransportContainerModePair("SEA", "FCL"));

			var ratesSearchResponse = new RatesSearchResponse();
			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);

			AssertEquals("Precondition: Current user is CWSupport", ZBool.True, Env.CurrentUser.IsSupportUser);

			var registrySettings = new RatesServiceRegistrySettingsCollection();

			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Sea, Constants.ContainerModes.FCL, false));
			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Sea, Constants.ContainerModes.LCL, false));
			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Air, Constants.ContainerModes.Loose, false));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettings))
			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();

				var filterStripBizo = form.FilterControlForTest.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = new ZDateTime(2018, 3, 25);

				var originDestStrip1 = (ModuleLocationFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				var errorMessages = new List<string>();

				foreach (var validMode in validPairs)
				{
					filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.TransportMode, validMode.transportMode);
					filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.ContainerMode, validMode.containerMode);
					errorMessages.Add(String.Format("Warning:Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for {0}-{1}: AutoRating -> Rates Service -> Rates Service Subscription", validMode.transportMode, validMode.containerMode));
				}

				var carrierStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierStrip1.IsActive = true;
				carrierStrip1.Property = carrier.PK;

				form.FilterControlForTest.Find();

				var notifiedMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("No notifications should exist", true, notifiedMessage.WasNone);

				foreach (var message in errorMessages)
				{
					AssertCollectionContains("Expected warning messages should match the logger's warnings", message, Logger.Warnings);
				}
			}
		}

		void AssertTransportAndContainerModeValidationWarning(TransportContainerModePair validModes, TransportContainerModePair invalidModes)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var ratesSearchResponse = new RatesSearchResponse();
			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);

			AssertEquals("Precondition: Current user is CWSupport", ZBool.True, Env.CurrentUser.IsSupportUser);

			var registrySettings = new RatesServiceRegistrySettingsCollection();

			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Sea, Constants.ContainerModes.FCL, true));
			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Sea, Constants.ContainerModes.LCL, true));
			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Air, Constants.ContainerModes.Loose, true));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettings))
			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();

				var filterStripBizo = form.FilterControlForTest.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = new ZDateTime(2018, 3, 25);

				var originDestStrip1 = (ModuleLocationFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				var containerModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.ContainerMode];
				containerModeStrip1.IsActive = true;
				containerModeStrip1.Property = invalidModes.containerMode;

				var transportModeStrip1 = (ModuleTextFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.TransportMode];
				transportModeStrip1.IsActive = true;
				transportModeStrip1.Property = invalidModes.transportMode;

				filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.TransportMode, validModes.transportMode);
				filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.ContainerMode, validModes.containerMode);

				var carrierStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierStrip1.IsActive = true;
				carrierStrip1.Property = carrier.PK;

				form.FilterControlForTest.Find();
				var errorMessage = String.Format("Warning:Request will not be sent to Rates Service because {0}-{1} is not a valid combination", invalidModes.transportMode, invalidModes.containerMode);

				AssertContainsExactElementsInAnyOrder(
					"Logger warnings should match the expected warning message",
					new[] { errorMessage },
					Logger.Warnings
				);

				var notifiedMessage = UnitTestUserNotification.Instance.LastMessage;
				Assert(notifiedMessage.WasNone);
			}
		}

		void AssertAllTransportAndContainerModeValidationWarnings(List<TransportContainerModePair> validModes, List<TransportContainerModePair> invalidModes)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "SCAC");

			Factory.Save();

			var ratesSearchResponse = new RatesSearchResponse();
			SetupWiseRatesResponse(ratesSearchResponse);
			var wiseRatingHeaderView = new WiseRatingHeaderView(TestWiseRatesProvider, Factory, Logger);

			AssertEquals("Precondition: Current user is CWSupport", true, Env.CurrentUser.IsSupportUser == ZBool.True);
			var registrySettings = new RatesServiceRegistrySettingsCollection();

			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Sea, Constants.ContainerModes.FCL, true));
			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Sea, Constants.ContainerModes.LCL, true));
			registrySettings.Add(new RatesServiceRegistrySettings(Constants.TransportModes.Air, Constants.ContainerModes.Loose, true));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettings))
			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();

				var filterStripBizo = form.FilterControlForTest.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = new ZDateTime(2018, 3, 25);

				var originDestStrip1 = (ModuleLocationFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				var errorMessages = new List<string>();

				foreach (var validMode in validModes)
				{
					filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.TransportMode, validMode.transportMode);
					filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.ContainerMode, validMode.containerMode);
				}

				foreach (var invalidMode in invalidModes)
				{
					filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.TransportMode, invalidMode.transportMode);
					filterStripBizo.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.ContainerMode, invalidMode.containerMode);
					errorMessages.Add(String.Format("Warning:Request will not be sent to Rates Service because {0}-{1} is not a valid combination", invalidMode.transportMode, invalidMode.containerMode));
				}

				var carrierStrip1 = (ModuleGuidFilter)filterStripBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierStrip1.IsActive = true;
				carrierStrip1.Property = carrier.PK;

				form.FilterControlForTest.Find();

				var notifiedMessage = UnitTestUserNotification.Instance.LastMessage;
				Assert("Notified message should indicate no errors.", notifiedMessage.WasNone);

				var expectedWarnings = Logger.Warnings.Select(w => w).ToArray();
				var actualWarnings = errorMessages.ToArray();
				AssertContainsExactElementsInAnyOrder("The logged warnings should match the expected warnings.", expectedWarnings, actualWarnings);
			}
		}

		protected void SetupWiseRatesResponse(RatesSearchResponse wiseRatesResponse, System.Linq.Expressions.Expression<Predicate<RatesSearchRequest>> requestMatchPredicate = null, bool withRatesServiceClient = false)
		{
			requestMatchPredicate = requestMatchPredicate ?? (x => true);

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(It.Is<RatesSearchRequest>(p => requestMatchPredicate.Compile().Invoke(p)),
					It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(wiseRatesResponse));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory> { CallBase = true };

			if (withRatesServiceClient)
			{
				clientFactoryMock
					.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
					.Returns((new RatesServiceClient(clientMock.Object), string.Empty));
			}
			else
			{
				clientFactoryMock
					.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
					.Returns((clientMock.Object, string.Empty));
			}

			TestWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, Logger);
		}

		protected WiseRatesProvider TestWiseRatesProvider;
		protected TestLogger Logger;

		void AddShippingLineToCarrier(OrgHeader carrier, string scac)
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = scac;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new TestLogger();
		}

		class TransportContainerModePair
		{
			internal TransportContainerModePair(string transportMode, string containerMode)
			{
				this.transportMode = transportMode;
				this.containerMode = containerMode;
			}

			internal readonly string transportMode;
			internal readonly string containerMode;
		}
	}

	[TestClass]
	class WiseRatesFormForTest : WiseRatesForm
	{
		public WiseRatesFormForTest(WiseRatingHeaderView wiseRatingHeaderView) : base(wiseRatingHeaderView)
		{
		}

		public WiseRatesFilterStripControl FilterControlForTest => FilterControl;
	}

	#region Form Basher

	[TestedType(typeof(WiseRatesForm))]
	public class WiseRatesFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			shippingLine.RSL_CarrierName = carrier.OH_Code;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			Factory.Save();

			var costing = new Rate
			{
				Carrier = "Emirates",
				Charges = new List<Charge>(),
				ContainerMode = "LCL",
				Destination = "USLAX",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = "AUSYD",
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = "AIR"
			};

			costing.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 8m });

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "FRT", Group = "FRT" } },
			};

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesSearchResponse));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((clientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);

			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);
			return new WiseRatesForm(wiseRatingHeaderView);
		}
	}

	#endregion
}
