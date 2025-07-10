using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Filters = Enterprise.Rating.GUI.RateEntryFilterUtility.Constants.Codes;
using MeasureInfo = Enterprise.MasterFiles.Business.MeasureInfo;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	[TestedType(typeof(RateSelectorFilterStripBusinessObject))]
	public class RateSelectorFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filter getters

		public void TestEffectiveDateGetter_ReturnEffectiveDateFilterValue()
		{
			((ModuleSingleDateFilter)Filter[Filters.EffectiveOn]).Property1 = new ZDateTime(2066, 01, 01);

			AssertEquals("Effective date should match the assigned value in the filter.", new ZDateTime(2066, 01, 01), Filter.EffectiveDate);
		}

		public void TestEffectiveOnFilter()
		{
			var criteria = new TestRatingCriteria("XXXXX", "YYYYY", FreightMode.LSE, 10, 1, null);
			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
			var effectiveOnFilter = filter.GetFilters<ModuleSingleDateFilter>(Filters.EffectiveOn).Single();
			AssertEquals("EffectiveOn filter ReadOnly", expected: false, effectiveOnFilter.ReadOnly);

			effectiveOnFilter.Property1 = ZDateTime.Empty;
			effectiveOnFilter.Validation.ValidateProperty1();
			AssertHasError("EffectiveOn filter Property1 error", effectiveOnFilter.Property1Info, "Please enter a value.");

			AssertEquals("EffectiveOn visibility", FilterVisibility.AlwaysVisible, effectiveOnFilter.Visibility);

			effectiveOnFilter.Property1 = new ZDateTime(2023, 1, 1);
			var (ratesQuery, _) = filter.BuildRatesQuery(new ElementaryLogger());
			AssertEquals("EffectiveOn Query", new ZDateTime(2023, 1, 1), ratesQuery.EffectiveDate);
		}

		public void TestLocationGetter_ReturnLocationFilterValue()
		{
			((ModuleLocationFilter)Filter[Filters.OriginDestination]).IsActive = true;
			((ModuleLocationFilter)Filter[Filters.OriginDestination]).Property1 = "UAIEV";
			((ModuleLocationFilter)Filter[Filters.OriginDestination]).Property2 = "AUSYD";

			var expectedOrigins = new[] { "", "IEV", "UA", "MEDG", "UAIEV" };
			var expectedDestinations = new[] { "", "AUSYD", "SYD", "AU", "OCEG" };

			AssertContainsExactElementsInAnyOrder(expectedOrigins, Filter.Origins);
			AssertContainsExactElementsInAnyOrder(expectedDestinations, Filter.Destinations);
		}

		public void TestCarrierGetter_ReturnCarrierFilterValue()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var filterStrip1 = Filter.AddFilterStrip<ModuleGuidFilter>(Filters.CarrierTransportProvider);
			filterStrip1.Property = carrier1.PK;
			var filterStrip2 = Filter.AddFilterStrip<ModuleGuidFilter>(Filters.CarrierTransportProvider);
			filterStrip2.Property = carrier2.PK;

			var expectedCarriers = new[] { carrier1.PK, carrier2.PK };
			var actualCarriers = Filter.Carriers.Select(x => x.Org.PK).ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Carriers filter should match the expected values.",
				expectedCarriers,
				actualCarriers
			);
		}

		public void TestCarrierServiceLevelsFromFiltersGetter_ReturnCarrierServiceLevel()
		{
			var filterStrip1 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.CarrierServiceLevel);
			filterStrip1.Property = "SV1";
			var filterStrip2 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.CarrierServiceLevel);
			filterStrip2.Property = "SV2";

			AssertContainsExactElementsInAnyOrder(
				"Expected CarrierServiceLevelsFromFilters to contain 'SV1' and 'SV2'",
				new[] { "SV1", "SV2" },
				Filter.CarrierServiceLevelsFromFilters
			);
		}

		public void TestPaymentTermsFromFiltersGetter_ReturnPaymentTermValue()
		{
			var filterStrip1 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.PaymentTerm);
			filterStrip1.Property = Constants.PaymentType.Prepaid;

			var filterStrip2 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.PaymentTerm);
			filterStrip2.Property = Constants.PaymentType.Collect;

			var expected = new[] { Constants.PaymentType.Prepaid, Constants.PaymentType.Collect };

			AssertContainsExactElementsInAnyOrder(
				"Payment terms from filters should match the expected values.",
				expected,
				Filter.PaymentTermsFromFilters
			);
		}

		public void TestCommodityGroupsFromFiltersGetter_ReturnCommodityGroups()
		{
			var filterStrip1 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.UniversalCommodityGroup);
			filterStrip1.Property = "COMMODITY1";
			var filterStrip2 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.UniversalCommodityGroup);
			filterStrip2.Property = "COMMODITY2";

			var expected = new[] { "COMMODITY1", "COMMODITY2" };
			AssertContainsExactElementsInAnyOrder("The UniversalCommodityGroupsFromFilters should return the correct commodities", expected, Filter.UniversalCommodityGroupsFromFilters);
		}

		public void TestCommodityCodesFromFiltersGetter_ReturnCommodityCodes()
		{
			var filterStrip1 = Filter.AddFilterStrip<ModuleNkFilter>(Filters.CommodityCode);
			filterStrip1.Property = "XYZ";

			AssertEquals(
				"Commodity codes from the filter should match the provided value.",
				"XYZ",
				Filter.CommodityCodeFromFilter);

			// Removed the placeholder assertion as it's not necessary.
		}

		public void TestContractNumbersFromFiltersGetter_ReturnContractNumbers()
		{
			var filterStrip1 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.CarrierContractNumber);
			filterStrip1.Property = "CONTRACT1";
			var filterStrip2 = Filter.AddFilterStrip<ModuleTextFilter>(Filters.CarrierContractNumber);
			filterStrip2.Property = "CONTRACT2";

			var expectedContractNumbers = new[] { "CONTRACT1", "CONTRACT2" };
			AssertContainsExactElementsInAnyOrder(expectedContractNumbers, Filter.ContractNumbersFromFilters);
		}

		#endregion

		#region Job getters

		public void TestPossibleServiceProvidersFromJobGetter_SingleRoute()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var creditor = Helper.NewOrgHeader();
			var creditorOnRoute = Helper.NewOrgHeader();
			var carrierOnRoute = Helper.NewOrgHeader();

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(carrier);
			consol.SetDefaultSendingForwarderAddress(Helper.NewOrgHeader());
			consol.SetDefaultReceivingForwarderAddress(Helper.NewOrgHeader());
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_PackDepotAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.Transports[0].CarrierPK = carrierOnRoute.PK;
			consol.Transports[0].CreditorPK = creditorOnRoute.PK;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
			var actualServiceProviders = filter.JobServiceProviders.Select(o => o.PK).ToArray();
			var expectedServiceProviders = new[] { carrier.PK, creditor.PK, carrierOnRoute.PK, creditorOnRoute.PK };

			AssertContainsExactElementsInAnyOrder(
				"Expected the possible service providers from the job getter to match the list of service providers",
				expectedServiceProviders,
				actualServiceProviders
			);
		}

		public void TestUniversalCarrierServiceLevelsFromJobGetter_ReturnMappedServiceLevelCodes()
		{
			var carrier = CreateCarrierOrg("SCAC");
			MapUniversalCarrierServiceLevel(carrier, "EXP", "ABC", "ABC mapped to EXP");

			var creditor = Helper.NewOrgHeader();
			MapUniversalCarrierServiceLevel(creditor, "EXP", "DEF", "DEF mapped to EXP");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(carrier);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			consol.JK_AWBServiceLevel = "EXP";

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
			var expectedServiceLevels = new[] { "ABC", "DEF", "EXP" };
			AssertContainsExactElementsInAnyOrder(
				"The UniversalCarrierServiceLevelsFromJob should match the mapped service levels",
				expectedServiceLevels,
				filter.UniversalCarrierServiceLevelsFromJob
			);
		}

		public void TestUniversalCarrierServiceLevelsFromJobGetter_WithCommaSeparatedCodes_ReturnMappedServiceLevelCodes()
		{
			var carrier = CreateCarrierOrg("SCAC");
			MapUniversalCarrierServiceLevel(carrier, "EXP", "ABC", "ABC mapped to EXP");

			var creditor = Helper.NewOrgHeader();
			MapUniversalCarrierServiceLevel(creditor, "EXP", "D,E", "D,E mapped to EXP");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(carrier);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			consol.JK_AWBServiceLevel = "EXP";

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);

			var actual = filter.UniversalCarrierServiceLevelsFromJob.ToArray();
			var expected = new[] { "ABC", "D", "E", "EXP" };

			AssertContainsExactElementsInAnyOrder("Unexpected service level mapping.", expected, actual);
		}

		public void TestContractNumbersFromJobGetter_ReturnContractNumbers()
		{
			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			criteria.CarrierContractNumbers = new ZString[] { "CONTRACT1", "CONTRACT2", "CONTRACT3" };

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);

			var expectedContractNumbers = new[] { "CONTRACT1", "CONTRACT2", "CONTRACT3" };
			AssertContainsExactElementsInAnyOrder(
				"The contract numbers from the job should match the expected values",
				expectedContractNumbers,
				filter.ContractNumbersFromJob
			);
		}

		public void TestPaymentTermsFromJobGetter_ReturnPaymentTermsOfTypePrepaidCollect()
		{
			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			criteria.PaymentTerm = new PaymentTermInfos();
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Cost, Constants.PaymentType.Prepaid));
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Revenue, Constants.PaymentType.Prepaid));

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
			AssertEquals("Expected PaymentTermsFromJob to be Prepaid", Constants.PaymentType.Prepaid, filter.PaymentTermsFromJob);

			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Cost, Constants.IncoTerms.FreeOnBoard));
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, Constants.IncoTerms.FreeOnBoard));
			AssertNullOrEmpty("Incoterm is not for searching CG rates", filter.PaymentTermsFromJob);
		}

		public void TestCommoditiesFromJobGetter()
		{
			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity1.RH_Code = "COM1";
			commodity1.RH_UniversalCommodityGroup = "UCG1";
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity2.RH_Code = "COM2";
			commodity2.RH_UniversalCommodityGroup = "UCG2";
			var commodity3 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity3.RH_Code = "COM3";
			commodity3.RH_UniversalCommodityGroup = "UCG3";
			Factory.Save();

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var measures = criteria.RateableMeasures;
			measures.AddContainerGroup(ZGuid.Empty, "COM1", new[] { new MeasureInfo.ContainerInfo() });

			var parts = new RateablePartList { HasCommodity = true };
			parts.AddPart(new RateablePart { CommodityCode = "COM2" });
			parts.AddPart(new RateablePart { CommodityCode = "COM3" });
			measures.AddPartList(MeasureType.Package, parts);

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);

			AssertContainsExactElementsInAnyOrder("Commodity codes from job containers should match", new[] { "COM1" }, filter.CommodityCodesFromJobContainers);

			AssertContainsExactElementsInAnyOrder("Commodity codes from job packlines should match", new[] { "COM2", "COM3" }, filter.CommodityCodesFromJobPacklines);

			AssertContainsExactElementsInAnyOrder("Universal commodity groups from job should match", new[] { "UCG1" }, filter.UniversalCommodityGroupsFromJob);
		}

		public void TestUniversalCommodityGroupsFromJobPlusGeneralAndNotClassifiedGetter_ReturnCommodityGroupsIncludingGenericOnes()
		{
			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity1.RH_UniversalCommodityGroup = "CMMGROUP1";
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity2.RH_UniversalCommodityGroup = "CMMGROUP2";
			Factory.Save();

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var measures = criteria.RateableMeasures;
			measures.AddContainerGroup(ZGuid.Empty, commodity1.RH_Code, new[] { new MeasureInfo.ContainerInfo() });
			measures.AddContainerGroup(ZGuid.Empty, commodity2.RH_Code, new[] { new MeasureInfo.ContainerInfo() });

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);

			var expectedGroups = new[]
			{
				"CMMGROUP1",
				"CMMGROUP2",
				RefCommodityCode.UniversalGroups.General,
				RefCommodityCode.UniversalGroups.NotClassified
			};

			AssertContainsExactElementsInAnyOrder(
				"Filter should return expected universal commodity groups.",
				expectedGroups,
				filter.UniversalCommodityGroupsFromJobPlusGeneralAndNotClassified
			);
		}

		public void TestCheckboxCommodityGroupsFromJobGetter_ReturnCheckboxCommodityCodes()
		{
			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity1.RH_UniversalCommodityGroup = "CMMGROUP1";
			commodity1.RH_IsFlammable = true;
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity2.RH_IsFlammable = true;
			commodity2.RH_IsTimber = true;
			commodity2.RH_IsPerishable = true;
			commodity2.RH_IsHazardous = true;
			commodity2.RH_ContainerVentRequired = true;
			Factory.Save();

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var measures = criteria.RateableMeasures;
			measures.AddContainerGroup(ZGuid.Empty, commodity1.RH_Code, new[] { new MeasureInfo.ContainerInfo() });
			measures.AddContainerGroup(ZGuid.Empty, commodity2.RH_Code, new[] { new MeasureInfo.ContainerInfo() });

			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
			var expectedGroups = new[]
			{
				"CMMGROUP1",
				RefCommodityCode.FLAM,
				RefCommodityCode.CNVT,
				RefCommodityCode.TIMB,
				RefCommodityCode.PERS,
				RefCommodityCode.HAZD
			};

			AssertContainsExactElementsInAnyOrder(
				"UniversalCommodityGroupsFromJob should contain the expected groups",
				expectedGroups,
				filter.UniversalCommodityGroupsFromJob
			);
		}

		#endregion

		#region Query building

		public void TestBuildQuery_MandatoryFiltersOverwriteJobFields()
		{
			var sq = RefAirline.LoadFromAirline2LetterCode(Factory, "SQ");
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "SQ";
			carrier.OH_FullName = "Singapore Airlines";
			carrier.OH_IsShippingProvider = true;
			carrier.MiscServ.OM_RM_Airline = sq.PK;

			Factory.Save();

			// doesn't matter of the initial criteria, expect individual filters to overwrite them later
			var criteria = new TestRatingCriteria("XXXXX", "YYYYY", FreightMode.LSE, 10, 1, null);
			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);

			var testDate = ZDateTime.Today;
			var effectiveOnFilter = (ModuleSingleDateFilter)filter[Filters.EffectiveOn];
			effectiveOnFilter.IsActive = true;
			effectiveOnFilter.Property1 = testDate;

			var locationFilter = (ModuleLocationFilter)filter[Filters.OriginDestination];
			locationFilter.IsActive = true;
			locationFilter.Property1 = "UAIEV";
			locationFilter.Property2 = "AUSYD";

			var carrierFilter = (ModuleGuidFilter)filter[Filters.CarrierTransportProvider];
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier.PK;

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (ratesQuery, _) = filter.BuildRatesQuery(TestLogger);

				AssertCollectionContains("Origin should contain 'UAIEV'", "UAIEV", ratesQuery.Origin);
				AssertCollectionContains("Destination should contain 'AUSYD'", "AUSYD", ratesQuery.Destination);
				AssertEquals("EffectiveDate should match testDate", testDate.ToDateTime(), ratesQuery.EffectiveDate);
				AssertCollectionContains(
					"Carrier should contain an entry with IATACode 'SQ'",
					"SQ",
					ratesQuery.Carrier.Select(x => x.IATACode)
				);
			}
		}

		public void TestFilter_CGReference()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var referenceTextFilter = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.CGReference);
				AssertEquals("Default value of Effective On filter", string.Empty, referenceTextFilter.Property);
				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);
				AssertEquals("CargoGuide filters should initially have no references.", 0, ratesQuery.CargoGuideFilters?.References?.Count() ?? 0);

				referenceTextFilter.Property = "Potato";
				(ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				var expectedReferences = new[] { "Potato" };
				AssertContainsExactElementsInAnyOrder("CargoGuide filters should contain the correct reference after being set.", expectedReferences, ratesQuery.CargoGuideFilters.References);
			}
		}

		public void TestBuildQuery_NoServiceLevel()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: No filter added", 0, Filter.CarrierServiceLevelsFromFilters.Count());
				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				AssertEquals(0, ratesQuery.ServiceLevel.Count());
			}
		}

		public void TestBuildQuery_WithServiceLevel()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filterStrip1 = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.CarrierServiceLevel);
				filterStrip1.Property = "ABC";
				var filterStrip2 = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.CarrierServiceLevel);
				filterStrip2.Property = "DEF";

				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				AssertContainsExactElementsInAnyOrder("Expected service levels to match the filter properties",
					new[] { "ABC", "DEF" }, ratesQuery.ServiceLevel);
			}
		}

		public void TestBuildQuery_NoCommodityGroup()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: No filter added.", 0, Filter.UniversalCommodityGroupsFromFilters.Count());
				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				AssertEquals(0, ratesQuery.Commodities.Count());
			}
		}

		public void TestBuildQuery_WithCommodityGroup()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filterStrip1 = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.UniversalCommodityGroup);
				filterStrip1.Property = "ABC";
				var filterStrip2 = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.UniversalCommodityGroup);
				filterStrip2.Property = "DEF";

				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				AssertContainsExactElementsInAnyOrder(
					"The commodities should contain the expected properties.",
					new[] { "ABC", "DEF" },
					ratesQuery.Commodities
				);
			}
		}

		public void TestBuildQuery_WithCommodityCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 10, 1, carrier);
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(ZGuid.NewZGuid(), "ABC", "1234", new MeasureInfo.ContainerInfo());
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(ZGuid.NewZGuid(), "XYZ", "1234", new MeasureInfo.ContainerInfo());

			Filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
			var preconditionCommodities = Filter.CreateCriteria().RateableMeasures.GetCommodities();
			AssertContainsExactElementsInAnyOrder(
				"Precondition: Ensure the commodities match the expected values",
				new[] { "ABC", "XYZ" },
				preconditionCommodities
			);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filterStrip1 = Filter.AddFilterStrip<ModuleNkFilter>(Filters.CommodityCode);
				filterStrip1.Property = "WART";

				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				AssertEquals(
					"Commodity code filter only affects CW1 rates",
					0,
					ratesQuery.Commodities.Count()
				);

				var updatedCommodities = Filter.CreateCriteria().RateableMeasures.GetCommodities();
				AssertContainsExactElementsInAnyOrder(
					"Ensure the commodities match the updated value",
					new[] { "WART" },
					updatedCommodities
				);
			}
		}

		//TODO: uncomment this test when we added ULD to registry for rates Service Subscription
		//We do not support ULD at the moment
		//public void TestBuildQuery_NoContainerType()
		//{
		//	var container = Factory.NewWithValidTestData<RefContainer>();
		//	container.RC_ShippingMode = "AIR";
		//	container.RC_IsActive = true;
		//	container.RC_Code = "LD7";
		//	container.RC_ISOType = "LD7";
		//	container.RC_Description = "AIR LD7 Container";
		//	Factory.Save();

		//	var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.ULD, 0, null, 1700M, "KG", 1M, "M3", null, ZString.Empty);
		//	var logger = new ElementaryLogger();
		//	var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);

		//	GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
		//	using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
		//	using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		//	{
		//		filter.ContainerTypeFilters.Should().BeEmpty("Precondition: No filter added.");

		//		var (ratesQuery, _) = filter.BuildRatesQuery(TestLogger);

		//		ratesQuery.Container.Should().BeNullOrEmpty();
		//	}

		//	Assert("This test uses FluentAssertions", true);
		//}

		//TODO: uncomment this test when we added ULD to registry for rates Service Subscription
		//We do not support ULD at the moment
		//public void TestBuildQuery_WithContainerType()
		//{
		//	var container = Factory.NewWithValidTestData<RefContainer>();
		//	container.RC_ShippingMode = "AIR";
		//	container.RC_IsActive = true;
		//	container.RC_Code = "LD7";
		//	container.RC_ISOType = "LD7";
		//	container.RC_Description = "AIR LD7 Container";
		//	Factory.Save();

		//	var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.ULD, 0, null, 1700M, "KG", 1M, "M3", null, ZString.Empty);
		//	var logger = new ElementaryLogger();
		//	var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);

		//	GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
		//	using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
		//	using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		//	{
		//		var filterStrip1 = filter.AddFilterStrip<ModuleTextFilter>(Filters.ContainerType);
		//		filterStrip1.Property = "LD7";

		//		filter.ContainerTypeFilters.Should().BeEquivalentTo(new string[] { "LD7" });

		//		var (ratesQuery, _) = filter.BuildRatesQuery(TestLogger);

		//		ratesQuery.Container.Should().BeEquivalentTo(new[] { new WiseRatesModel.RatesQueryContainer { ISOType = "LD7" } });
		//	}

		//	Assert("This test uses FluentAssertions", true);
		//}

		public void TestBuildQuery_NoContractNumbers()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: no filter added", 0, Filter.ContractNumbersFromFilters.Count());
				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				AssertEquals(0, ratesQuery.Contract.Count());
			}
		}

		public void TestBuildQuery_WithContractNumbers()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filterStrip1 = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.CarrierContractNumber);
				filterStrip1.Property = "ABC";
				var filterStrip2 = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.CarrierContractNumber);
				filterStrip2.Property = "DEF";
				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				var expected = new[] { "ABC", "DEF" };
				var actual = ratesQuery.Contract.Select(x => x.ContractNumber);

				AssertContainsExactElementsInAnyOrder("The contract numbers should match the expected values.", expected, actual);
			}
		}

		public void TestBuildQuery_WithPaymentTerm()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filterStrip = Filter.AddFilterStrip<WiseRatesModuleTextFilter>(Filters.PaymentTerm);
				filterStrip.Property = Constants.PaymentType.Prepaid;

				var (ratesQuery, _) = Filter.BuildRatesQuery(TestLogger);

				AssertContainsExactElementsInAnyOrder("PaymentTerm should match the expected value",
					new[] { Constants.PaymentType.Prepaid },
					ratesQuery.PaymentTerm);
			}
		}

		public void TestBuildQuery_GivenInvalidCarrier_WhenRemovingFromFilter_ThenQueryShouldBuildWithoutCarrier()
		{
			var query = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, SQLComparisonOperator.IsBlank, "");
			var airlineWithoutAirline2LetterCode = Factory.LoadTop1<RefAirline>(query);
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "INVALID";
			carrier.OH_FullName = "Test Org no IATA";
			carrier.OH_IsShippingProvider = true;
			carrier.MiscServ.OM_RM_Airline = airlineWithoutAirline2LetterCode.PK;

			Factory.Save();

			var criteria = new TestRatingCriteria("XXXXX", "YYYYY", FreightMode.LSE, 10, 1, null);
			criteria.Creditors = Creditors.New(OrgWithSource.New(carrier, new List<string> { "TransportProvider" }));
			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);

			var testDate = ZDateTime.Today;
			var effectiveOnFilter = (ModuleSingleDateFilter)filter[Filters.EffectiveOn];
			effectiveOnFilter.IsActive = true;
			effectiveOnFilter.Property1 = testDate;

			var locationFilter = (ModuleLocationFilter)filter[Filters.OriginDestination];
			locationFilter.IsActive = true;
			locationFilter.Property1 = "UAIEV";
			locationFilter.Property2 = "AUSYD";

			var carrierFilter = (ModuleGuidFilter)filter[Filters.CarrierTransportProvider];
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier.PK;

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (ratesQuery, _) = filter.BuildRatesQuery(TestLogger);

				AssertNull("Selected carrier is not a valid air carrier", ratesQuery);
				AssertCollectionContains("None of Job's Service Providers/Creditors can be used for rates search",
					"None of Job's Service Providers/Creditors can be used for rates search", TestLogger.Warnings);

				TestLogger.ClearLogs();
				carrierFilter.IsActive = false;
				(ratesQuery, _) = filter.BuildRatesQuery(TestLogger);

				AssertNotNull(ratesQuery);
				AssertEquals("The carrier has been removed from filter strip", 0, ratesQuery.Carrier.Count());
				AssertEquals("The carrier should not in carrier list for query building hence should not cause error", 0, TestLogger.Warnings.Count);
			}
		}

		public void TestBuildQuery_GivenDisabledRateServiceRegistry_ShouldHaveWarningInLogger()
		{
			var criteria = new TestRatingCriteria("XXXXX", "YYYYY", FreightMode.LSE, 10, 1, null);
			var filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (ratesQuery, validForRatesService) = filter.BuildRatesQuery(TestLogger);

				AssertEquals(false, validForRatesService);
				AssertNull("ratesQuery", ratesQuery);

				AssertCollectionContains(
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service Subscription",
					TestLogger.Warnings
				);
			}

			TestLogger.ClearLogs();
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (ratesQuery, validForRatesService) = filter.BuildRatesQuery(TestLogger);

				AssertEquals(true, validForRatesService);
				AssertNotNull("ratesQuery", ratesQuery);

				AssertCollectionNotContains(
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service Subscription",
					TestLogger.Warnings
				);
				AssertCollectionNotContains(
					"Request will not be sent to Rates Service because CGGD integration is disabled in the registry",
					TestLogger.Warnings
				);
			}
		}

		#endregion

		public void TestModuleFilters_ContainerTypeFilterShouldBeAddedOnlyOnULDContainerMode()
		{
			var criteriaWithULDContainerMode = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.ULD, 0, null, 1700M, "KG", 1M, "M3", null, ZString.Empty);
			var logger = new ElementaryLogger();
			var filterWithContainerType = new RateSelectorFilterStripBusinessObject(criteriaWithULDContainerMode, logger);
			AssertNotNull(filterWithContainerType.ModuleFilters.FirstOrDefault(f => f.OriginalCode == Filters.ContainerType));

			var criteriaWithNonULDContainerMode = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 0, null, 1700M, "KG", 1M, "M3", null, ZString.Empty);
			var filterWithoutContainerType = new RateSelectorFilterStripBusinessObject(criteriaWithNonULDContainerMode, logger);
			AssertNull(filterWithoutContainerType.ModuleFilters.FirstOrDefault(f => f.OriginalCode == Filters.ContainerType));
		}

		#region Overrides and helpers

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var carrier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MCLAREN");
			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 10, 1, carrier);
			return new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestLogger = new ElementaryLogger();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 10, 1, carrier);

			Filter = new RateSelectorFilterStripBusinessObject(criteria, TestLogger);
		}

		static void MapUniversalCarrierServiceLevel(OrgHeader orgHeader, string localCode, string foreignCode, string description)
		{
			var mapping = orgHeader.MiscServ.CarrierServiceLevels.AddNew();
			mapping.PL_Code = localCode;
			mapping.PL_CarrierServiceCode = foreignCode;
			mapping.PL_CarrierServiceLevelDescription = description;
		}

		OrgHeader CreateCarrierOrg(string scac = "SCAC")
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = scac + "CARRIER";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsCreditor = true;
			carrier.CompanyData.SetAPTaxApplicable(false);

			if (string.IsNullOrWhiteSpace(scac))
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_StandardCarrierAlphaCode = scac;
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
			}

			return carrier;
		}

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		#endregion

		RateSelectorFilterStripBusinessObject Filter { get; set; }
		ElementaryLogger TestLogger;
	}
}
