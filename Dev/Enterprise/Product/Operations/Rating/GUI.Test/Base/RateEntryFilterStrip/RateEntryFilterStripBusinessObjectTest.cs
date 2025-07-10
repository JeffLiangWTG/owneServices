using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(RateEntryFilterStripBusinessObject))]
	public class RateEntryFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TACTRatesFilter Strips

		public void TestIsTACTRatesFilter_StandardCostRate_AreLoaded()
		{
			var expectedFilterStripDescriptions = new List<string>
			{
				RateEntryFilterUtility.Constants.Codes.IncludeTACTRates,
				RateEntryFilterUtility.Constants.Codes.EffectiveOn,
				RateEntryFilterUtility.Constants.Codes.OriginDestination,
				RateEntryFilterUtility.Constants.Codes.CommodityCode
			};

			var standardCostRateCategories = new[]
			{
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.SummaryRatesCategory
			};

			var stdCosting = Factory.NewWithValidTestData<Costing>();
			stdCosting.TH_OH = Guid.Empty;

			foreach (var filterCategory in standardCostRateCategories)
			{
				var collection = stdCosting.EntryCollections[filterCategory].LazyLoadingCollection;
				var filterStripBusinessObject = new RateEntryFilterStripBusinessObject(collection);
				Assert_ExpectedFilterStrips_AreLoaded($"Rating_{filterCategory}_StandardCostRate", filterStripBusinessObject, expectedFilterStripDescriptions);
			}
		}

		public void TestIsTACTRatesFilter_NonStandardCostRate_NotLoaded()
		{
			var expectedFilterStripDescriptions = new List<string>
			{
				"Effective On",
				"Origin / Destination",
				"Commodity Code"
			};

			var nonStandardCostRateCategories = new[]
			{
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.CST,
				RatingConstants.RateCategory.DST,
				RatingConstants.RateCategory.FCL,
				RatingConstants.RateCategory.LCL,
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.PAC,
				RatingConstants.RateCategory.SCO,
				RatingConstants.RateCategory.SDE,
				RatingConstants.RateCategory.SED,
				RatingConstants.RateCategory.SID,
				RatingConstants.RateCategory.SummaryRatesCategory,
				RatingConstants.RateCategory.SNC,
				RatingConstants.RateCategory.SOR,
				RatingConstants.RateCategory.TBC,
				RatingConstants.RateCategory.TRN,
				RatingConstants.RateCategory.UNP,
				RatingConstants.RateCategory.WHS
			};

			var costing = Factory.NewWithValidTestData<Costing>();
			foreach (var filterCategory in nonStandardCostRateCategories)
			{
				var collection = costing.EntryCollections[filterCategory].LazyLoadingCollection;
				var filterStripBusinessObject = new RateEntryFilterStripBusinessObject(collection);
				Assert_ExpectedFilterStrips_AreLoaded($"Rating_{filterCategory}", filterStripBusinessObject, expectedFilterStripDescriptions);
			}
		}

		void Assert_ExpectedFilterStrips_AreLoaded(string moduleId, RateEntryFilterStripBusinessObject filterStripBusinessObject, List<string> expectedFilterStripsDescriptions)
		{
			var layoutContext = ((IFilterStripBusinessObjectInternals)filterStripBusinessObject).LayoutContext;
			AssertEquals("Module layout context", moduleId, layoutContext);

			var loader = new StmModuleFilter.Loader(Factory);
			var filterLayout = loader.FindTop1ByID(moduleId);
			var filterStrips = filterStripBusinessObject.GetFilterStrips(filterLayout);

			AssertContainsExactElementsInAnyOrder
			(
				"Loaded filter strips",
				expectedFilterStripsDescriptions,
				filterStrips.Cast<FilterStrip>().Select(x => x.FilterDescription)
			);

			Assert
			(
				"This filter strip should not be included by default",
				filterStrips.Cast<FilterStrip>().All(x => x.FilterDescription != RateEntryFilterUtility.Constants.Codes.ShowPublished)
			);
		}

		#endregion

		#region Include Global Rates

		public void TestIncludeGlobalRatesQuery_ClientRate()
		{
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "ABC";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);

			var helper = new TestHelper(Factory);
			var client = helper.NewOrgHeader();
			var globalClientRate = helper.NewGlobalClientRate(client);
			var rateCategory = RatingConstants.RateCategory.ORG;
			var globalEntry1 = globalClientRate.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "GBLON", "AUSYD");
			var globalEntry2 = globalClientRate.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "CNSHA", "AUSYD");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var anotherCompanyPublishedEntry = globalClientRate.AddRateEntry(rateCategory, Core.Constants.RateMode.SEA, "DEHAM", "");
				AssertEquals("Pre-condition", company2.PK, anotherCompanyPublishedEntry.TI_GC_Publisher);
			}

			Factory.Save();

			var localClientRate = helper.NewClientRate(client);
			var localEntry1 = localClientRate.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "GB", "AU");
			var localEntry2 = localClientRate.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "GBLON", "AUSYD");

			AssertEquals("Pre-condition", globalClientRate.PK, localClientRate.GlobalRatingHeader.PK);

			var collection = localClientRate.EntryCollections[rateCategory].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			collection.SetUserFilter(filterBizO);

			var message = "Expected to only see the local rates as global rates should not be shown by default";
			var expected = new[] { localEntry1, localEntry2 };
			var actual = localClientRate.ORGRateEntriesForBinding;

			AssertContainsExactElementsInAnyOrder(message, expected, actual);

			var filter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowPublished];
			filter.IsActive = true;
			filter.Property0 = true;

			message = "Once the filter is enabled, expecting only to see the global rates where the publisher is the company.";
			expected = new[] { localEntry1, localEntry2, globalEntry1, globalEntry2 };
			actual = localClientRate.ORGRateEntriesForBinding;

			AssertContainsExactElementsInAnyOrder(message, expected, actual);

			filter.Property0 = false;

			message = "If filter is false then we expect only local rates again.";
			expected = new[] { localEntry1, localEntry2 };
			actual = localClientRate.ORGRateEntriesForBinding;

			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		public void TestIncludeGlobalRatesQuery_Costings()
		{
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "ABC";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);

			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out _, out _, true, false, "FRT", "FRT");

			var helper = new TestHelper(Factory);
			var supplier = helper.CreateCreditor();
			var globalCosting = helper.NewGlobalCosting(supplier);
			var rateCategory = RatingConstants.RateCategory.LCL;
			var globalEntry1 = globalCosting.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "GBLON", "AUSYD");
			var globalEntry2 = globalCosting.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "CNSHA", "AUSYD");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var anotherCompanyPublishedEntry = globalCosting.AddRateEntry(rateCategory, Core.Constants.RateMode.LRO, "DEHAM", "");
				AssertEquals("Pre-condition", company2.PK, anotherCompanyPublishedEntry.TI_GC_Publisher);
			}

			Factory.Save();

			var localCosting = helper.NewCosting(supplier);
			var localEntry1 = localCosting.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "GB", "AU");
			var localEntry2 = localCosting.AddRateEntry(rateCategory, Core.Constants.RateMode.LCL, "GBLON", "AUSYD");

			AssertEquals("Pre-condition", globalCosting.PK, localCosting.GlobalRatingHeader.PK);

			var collection = localCosting.EntryCollections[rateCategory].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			collection.SetUserFilter(filterBizO);

			var message = "Expected to only see the local costs as global costs should not be shown by default";
			var expected = new[] { localEntry1, localEntry2 };
			var actual = localCosting.LCLRateEntriesForBinding;

			AssertContainsExactElementsInAnyOrder(message, expected, actual);

			var filter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowPublished];
			filter.IsActive = true;
			filter.Property0 = true;

			message = "Once the filter is enabled, expecting only to see the global costs where the publisher is the company.";
			expected = new[] { localEntry1, localEntry2, globalEntry1, globalEntry2 };
			actual = localCosting.LCLRateEntriesForBinding;

			AssertContainsExactElementsInAnyOrder(message, expected, actual);

			filter.Property0 = false;

			message = "If filter is false then we expect only local costs again.";
			expected = new[] { localEntry1, localEntry2 };
			actual = localCosting.LCLRateEntriesForBinding;

			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		public void TestShowPublishedFilter_DoesNotFilterPricingPageResultsForRelatedEntries()
		{
			var helper = new TestHelper(Factory);
			helper.ChargeCodes.CreateGlobalCharge("GLBFRT", FlatCalculator.Code);
			helper.ChargeCodes.CreateGlobalCharge("GLBORG", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var client = helper.NewOrgHeader(1);
			client.OH_IsConsignee = true;
			Factory.Save();

			var companyTariff = Factory.New<CompanyTariff>();
			var companyTariffEntry1 = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "NZ", "WAR", 950);
			var companyTariffEntry2 = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "NZ", "OAQF", 500);

			var globalClientRate = helper.NewGlobalClientRate(client);
			var globalClientRateEntry1 = globalClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "NZ", "GLBFRT", 920);
			var globalClientRateEntry2 = globalClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "NZ", "GLBORG", 200);

			var localClientRate = helper.NewClientRate(client);
			var localClientRateEntry1 = localClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "NZ", "FRT", 820);
			var localClientRateEntry2 = localClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "NZ", "ODOC", 820);

			var collection = localClientRate.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var filter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowPublished];
			filter.IsActive = true;
			filter.Property0 = false;

			collection.SetUserFilter(filterBizO);
			collection.Load();

			var page = new PricingPage(localClientRateEntry1, Factory, PricingPageStyle.Standard);
			var loader = new RelatedRateEntriesLoader(page, Factory);

			var actualResults = loader.GetRelatedEntries(localClientRateEntry1);
			var actualFreightResults = actualResults.FreightRateEntries.Select(x => x.PK);

			var message = "Global Client Rate should be excluded by filter strip";
			var expected = new ZGuid[] { localClientRateEntry1.PK, companyTariffEntry1.PK };

			AssertContainsExactElementsInAnyOrder(message, expected, actualFreightResults);

			filter.Property0 = true;
			collection.Load();

			message = "Should still exclude the Global Client Rate";
			AssertContainsExactElementsInAnyOrder(message, expected, actualFreightResults);
		}

		#endregion

		#region Publisher

		public void TestPublisherFilter()
		{
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_Code = "ABC";
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherCompany.Branches.Add(anotherBranch);

			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out _, out _, true, false, "FRT", "FRT");

			var globalCosting = Factory.NewWithValidTestData<Costing>();
			globalCosting.TH_GC = ZGuid.Empty;
			var currentCompanyRateEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LRO, "AU", "NZ");

			Factory.Save();

			RateEntry anotherCompanyRateEntry = null;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				anotherCompanyRateEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "CN");
				Factory.Save();
			}

			var collection = globalCosting.EntryCollections[RatingConstants.RateCategory.LCL];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var publisherFilter = (ModuleGuidFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.Publisher];
			publisherFilter.IsActive = true;

			var asserter = new Asserter(collection, filterBizO);

			asserter.AssertFiltering("Empty filter should restrict results", new[] { currentCompanyRateEntry, anotherCompanyRateEntry });

			publisherFilter.Property = Env.CurrentCompanyPK;
			asserter.AssertFiltering("Publisher Company Filter", new[] { currentCompanyRateEntry });

			publisherFilter.Property = anotherCompany.PK;
			asserter.AssertFiltering("20RE", new[] { anotherCompanyRateEntry });
		}

		public void TestPublisherFilter_LocalRatingHeaders_ExcludesPublisher()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var localRatingHeaders = new RatingHeader[] { costing, companyTariff, clientRate };
			foreach (var ratingHeader in localRatingHeaders)
			{
				Assert("Pre-condition", !ratingHeader.IsGlobal());
				foreach (var filterCategory in RatingConstants.RateCategory.RateCategories)
				{
					var collection = ratingHeader.EntryCollections[filterCategory].LazyLoadingCollection;
					var filterStripBusinessObject = new RateEntryFilterStripBusinessObject(collection);
					var publisherFilter = filterStripBusinessObject.FirstOrDefault(f => f.Description == RateEntryFilterUtility.Constants.Codes.Publisher);
					var message = "Publisher Filter should not be included for local rates. filterCategory: " + filterCategory;

					AssertNull(message, publisherFilter);
				}
			}
		}

		public void TestPublisherFilter_GlobalRatingHeaders_IncludesPublisher()
		{
			var globalCosting = Factory.NewWithValidTestData<Costing>();
			globalCosting.TH_GC = ZGuid.Empty;

			var globalTariff = Factory.NewWithValidTestData<CompanyTariff>();
			globalTariff.TH_GC = ZGuid.Empty;

			var globalClientRate = Factory.NewWithValidTestData<ClientRate>();
			globalClientRate.TH_GC = ZGuid.Empty;
			globalClientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var globalRatingHeaders = new RatingHeader[] { globalCosting, globalTariff, globalClientRate };

			foreach (var ratingHeader in globalRatingHeaders)
			{
				Assert("Pre-condition", ratingHeader.IsGlobal());
				foreach (var filterCategory in RatingConstants.RateCategory.RateCategories)
				{
					var collection = ratingHeader.EntryCollections[filterCategory].LazyLoadingCollection;
					var filterStripBusinessObject = new RateEntryFilterStripBusinessObject(collection);
					var publisherFilter = filterStripBusinessObject.FirstOrDefault(f => f.Description == RateEntryFilterUtility.Constants.Codes.Publisher);
					var message = "Publisher Filter should be included for global rates. filterCategory: " + filterCategory;

					AssertNotNull(message, publisherFilter);
				}
			}
		}

		#endregion

		#region Show Published

		public void TestShowPublishedFilterForAllCompanyTariffLevels()
		{
			const int numberOfCompanyTariffLevel = 4;

			var testCompanyTariffs = new List<CompanyTariff>();
			var moduleFilterDescriptionList = new List<string[]>();

			for (var i = 0; i < numberOfCompanyTariffLevel; i++)
			{
				var companyTariff = Factory.New<CompanyTariff>();

				var collection = companyTariff.LCLRateEntriesForBinding;
				var filterBizO = new RateEntryFilterStripBusinessObject(collection);
				var moduleFilterShowPublishedDescriptions = filterBizO
					.ModuleFilters
					.Where(f => f.Description.EqualsIgnoringCase(RateEntryFilterUtility.Constants.Codes.ShowPublished))
					.Select(f => f.Description.ToString())
					.ToArray();

				testCompanyTariffs.Add(companyTariff);
				moduleFilterDescriptionList.Add(moduleFilterShowPublishedDescriptions);
			}

			CombineAssertions("ShowPublished filter for Company Tariff", () =>
			{
				Assert("Company Tariff of all levels should be able to add ShowPublished filter", testCompanyTariffs.All(x => x.SupportsPublishedFilter()));
				Assert("Company Tariff of all levels should have 1 ShowPublished filter", moduleFilterDescriptionList.All(x => x.Length == 1));
			});
		}

		#endregion

		public void TestIntercompanyTariffFilters()
		{
			var expected = new string[]
			{
				RateEntryFilterUtility.Constants.Codes.ShowExpired,
				RateEntryFilterUtility.Constants.Codes.StartDate,
				RateEntryFilterUtility.Constants.Codes.EndDate,
				RateEntryFilterUtility.Constants.Codes.EffectiveOn,
				RateEntryFilterUtility.Constants.Codes.ControllingCustomer,
				RateEntryFilterUtility.Constants.Codes.Consignee,
				RateEntryFilterUtility.Constants.Codes.Consignor,
				RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider,
				RateEntryFilterUtility.Constants.Codes.ProductWarehouse,
				RateEntryFilterUtility.Constants.Codes.TransitWarehouse,
				RateEntryFilterUtility.Constants.Codes.FromToOrganization,
				RateEntryFilterUtility.Constants.Codes.OriginDestination,
				RateEntryFilterUtility.Constants.Codes.FirstLoad,
				RateEntryFilterUtility.Constants.Codes.LastDischarge,
				RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad,
				RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge,
				RateEntryFilterUtility.Constants.Codes.TransitTime,
				RateEntryFilterUtility.Constants.Codes.FromToSuburb,
				RateEntryFilterUtility.Constants.Codes.FromPostcode,
				RateEntryFilterUtility.Constants.Codes.ToPostcode,
				RateEntryFilterUtility.Constants.Codes.FromToZone,
				RateEntryFilterUtility.Constants.Codes.FromLocationDescription,
				RateEntryFilterUtility.Constants.Codes.ToLocationDescription,
				RateEntryFilterUtility.Constants.Codes.FromLocationType,
				RateEntryFilterUtility.Constants.Codes.ToLocationType,
				RateEntryFilterUtility.Constants.Codes.CommodityCode,
				RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel,
				RateEntryFilterUtility.Constants.Codes.GatewayServiceLevel,
				RateEntryFilterUtility.Constants.Codes.ShipmentGatewayServiceLevel,
				RateEntryFilterUtility.Constants.Codes.TransportMode,
				RateEntryFilterUtility.Constants.Codes.ContainerType,
				RateEntryFilterUtility.Constants.Codes.ServiceLevel,
				RateEntryFilterUtility.Constants.Codes.AircraftType,
				RateEntryFilterUtility.Constants.Codes.GatewayAgentType,
				RateEntryFilterUtility.Constants.Codes.Currency,
				RateEntryFilterUtility.Constants.Codes.IsNonOperatingReefer,
				RateEntryFilterUtility.Constants.Codes.ShowAllRateLines,
				RateLineModuleFilters.Constants.Codes.ActualPercentage,
				RateLineModuleFilters.Constants.Codes.UseOnlyActualWeightMeasure,
				RateLineModuleFilters.Constants.Codes.Condition,
				RateLineModuleFilters.Constants.Codes.ContainerOwnership,
				RateLineModuleFilters.Constants.Codes.ConversionFactor,
				RateLineModuleFilters.Constants.Codes.Currency,
				RateLineModuleFilters.Constants.Codes.ChargeCode,
				RateLineModuleFilters.Constants.Codes.FeeChargeType,
				RateLineModuleFilters.Constants.Codes.FeeChargeLevel,
				RateLineModuleFilters.Constants.Codes.Rounding,
				RateLineModuleFilters.Constants.Codes.HasOverrideChargeDescription,
				RateLineModuleFilters.Constants.Codes.IsJobLevelCharge,
				RateLineModuleFilters.Constants.Codes.UnitFactor,
				RateLineModuleFilters.Constants.Codes.UnitMultiple,
				RateLineModuleFilters.Constants.Codes.Units,
				RateLineModuleFilters.Constants.Codes.StartDate,
				RateLineModuleFilters.Constants.Codes.EndDate,
				RateLineModuleFilters.Constants.Codes.EffectiveOn,
				RateLineModuleFilters.Constants.Codes.ShowExpired,
				"Creating User",
				"Created Time",
				"Last Edit User",
				"Last Edit Time",
				"Created On Web/Internal"
			};

			var helper = new TestHelper(Factory);
			var intercompanyTariff = helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			var collection = intercompanyTariff.LCLRateEntriesForBinding;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var actual = filterBizO
				.ModuleFilters
				.ToSortedArrayWithIsExclusiveLast()
				.Select(f => f.Description.ToString())
				.ToArray();

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestOriginDestinationSearchesWithinInternationalZonesByCountryOrUnloco()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var helper = new TestHelper(Factory);
			var cnZone = helper.NewInternationalZone("CNZM", null, "CN", "CNSHA");
			var gbZone = helper.NewInternationalZone("GBZD", null, "GB", "GBLON", "GBSUN");

			var clientRate = helper.NewClientRate(client);

			var category = RatingConstants.RateCategory.LCL;
			var entry1 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "CNZM", "GBZD");
			var entry2 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "CNZM", "GB");
			var entry3 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "CNZM", "GBSUN");

			var entry4 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "CN", "GBZD");
			var entry5 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "CNSHA", "GBZD");

			var entry6 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "CNZMU", "GBZDA");
			var entry7 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "GB", "GBSUN");
			var entry8 = clientRate.AddRateEntry(category, Core.Constants.RateMode.LCL, "AU", "US");

			Factory.Save();

			var collection = clientRate.EntryCollections[category].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var filter = (ModuleLocationFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.OriginDestination];
			filter.IsActive = true;

			var asserter = new Asserter(collection, filterBizO);

			filter.Property1 = "CN";
			asserter.AssertFiltering("Should match origin country which matches all zones", new[] { entry1, entry2, entry3, entry4, entry5, entry6 });

			filter.Property2 = "GB";
			asserter.AssertFiltering("Should match by destination as well", new[] { entry1, entry2, entry3, entry4, entry5, entry6 });

			filter.Property2 = "GBSUN";
			asserter.AssertFiltering("Should match destination by zone and UNLOCO", new[] { entry1, entry3, entry4, entry5 });

			filter.Property2 = "GBLON";
			asserter.AssertFiltering("Should match by zone as no rate entry uses this UNLOCO", new[] { entry1, entry4, entry5 });

			filter.Property1 = "CNSHA";
			asserter.AssertFiltering("Should match by either zone and UNLOCO", new[] { entry1, entry5 });

			filter.Property1 = "";
			filter.Property2 = "GB";
			asserter.AssertFiltering("", new[] { entry1, entry2, entry3, entry4, entry5, entry6, entry7 });

			filter.Property1 = "CNZMU";
			asserter.AssertFiltering("Should match by zone and unloco, should not confuse UNLOCO for smiliar code", new[] { entry6 });

			filter.Property1 = "CNZM";
			filter.Property2 = "GBZD";
			asserter.AssertFiltering("Should ONLY match the international zones, and not include entry7 with similar UNLOCO", new[] { entry1 });
		}

		public void TestLocationTypes()
		{
			var costing = Factory.New<Costing>();

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "NZAKL");
			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AU", "NZ");
			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AURZ", "NZDR");
			var entry4 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "SYD", "LON");

			Factory.Save();

			var collection = costing.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var fromLocationTypeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FromLocationType];
			var toLocationTypeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ToLocationType];

			var asserter = new Asserter(collection, filterBizO);

			Action<ModuleTextFilter> assertLocationTypes = filter =>
			{
				filter.IsActive = true;
				filter.Property = Core.Constants.LocationTypes.Codes.Port;
				asserter.AssertFiltering("Port", new[] { entry1 });

				filter.Property = Core.Constants.LocationTypes.Codes.Country;
				asserter.AssertFiltering("Country", new[] { entry2 });

				filter.Property = Core.Constants.LocationTypes.Codes.Zone;
				asserter.AssertFiltering("Zone", new[] { entry3 });

				filter.Property = Core.Constants.LocationTypes.Codes.IATARegion;
				asserter.AssertFiltering("IATA Region", new[] { entry4 });

				filter.IsActive = false;
			};

			assertLocationTypes(fromLocationTypeFilter);
			assertLocationTypes(toLocationTypeFilter);
		}

		public void TestFromToLocationTypeFilterShouldNotSupportIATARegionForNonStandardCostingRatingHeaders()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var quote = Factory.NewWithValidTestData<Quote>();
			var nonStandardCosting = Factory.NewWithValidTestData<Costing>();

			var ratingHeaders = new RatingHeader[] { companyTariff, clientRate, quote, nonStandardCosting };

			foreach (var ratingHeader in ratingHeaders)
			{
				Assert("Non Standard Costing Rate", !ratingHeader.IsStandardCostRate());

				var filterBizO = new RateEntryFilterStripBusinessObject(ratingHeader.AIRRateEntriesForBinding);
				var fromLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FromLocationType];
				var toLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ToLocationType];

				var hasIATARegionItem = fromLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
				Assert("'From Location Type' filter should not support 'IATA Region' for non-standard costing rating headers.", !hasIATARegionItem);

				hasIATARegionItem = toLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
				Assert("'To Location Type' filter should not support 'IATA Region' for non-standard costing rating headers.", !hasIATARegionItem);
			}
		}

		public void TestFromToLocationTypeFilterShouldSupportIATARegionOnAirRateCategory()
		{
			var costing = Factory.New<Costing>();

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.AIRRateEntriesForBinding);
			var fromLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FromLocationType];
			var toLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ToLocationType];

			var hasIATARegionItem = fromLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
			Assert("'From Location Type' filter should support 'IATA Region' on Air tab of standard costing rating headers.", hasIATARegionItem);

			hasIATARegionItem = toLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
			Assert("'To Location Type' filter should support 'IATA Region' on Air tab of standard costing rating headers.", hasIATARegionItem);
		}

		public void TestFromToLocationTypeFilterShouldSupportIATARegionOnSummaryRateCategory()
		{
			var costing = Factory.New<Costing>();

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.SummaryRateEntries);
			var fromLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FromLocationType];
			var toLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ToLocationType];

			var hasIATARegionItem = fromLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
			Assert("'From Location Type' filter should support 'IATA Region' on Summary tab of standard costing rating headers.", hasIATARegionItem);

			hasIATARegionItem = toLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
			Assert("'To Location Type' filter should support 'IATA Region' on Summary tab of standard costing rating headers.", hasIATARegionItem);
		}

		public void TestFromToLocationTypeFilterShouldNotSupportIATARegionForOtherRateCategories()
		{
			var costing = Factory.New<Costing>();

			foreach (var rateCategory in RatingConstants.RateCategory.RateCategories.Where(x => x != RatingConstants.RateCategory.AIR && x != RatingConstants.RateCategory.CAI && x != RatingConstants.RateCategory.SummaryRatesCategory))
			{
				var filterBizO = new RateEntryFilterStripBusinessObject(costing.EntryCollections[rateCategory].LazyLoadingCollection);
				var fromLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FromLocationType];
				var toLocationType = (ModuleTextFilter)filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ToLocationType];

				var hasIATARegionItem = fromLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
				Assert("'From Location Type' filter should not support 'IATA Region' on other tabs than AIR or Summary.", !hasIATARegionItem);

				hasIATARegionItem = toLocationType.List.Cast<ICodeDescription>().Any(x => x.Code == Core.Constants.LocationTypes.Codes.IATARegion);
				Assert("'To Location Type' filter should not support 'IATA Region' on other tabs than AIR or Summary.", !hasIATARegionItem);
			}
		}

		public void TestCrossTrade()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "NZAKL");
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "NLAMS", "USLAX");
			var entry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AU", "NZ");
			var entry4 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "CN", "MY");
			var entry5 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUEC", "");
			var entry6 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "USEC", "");
			var entry7 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "", "");
			entry7.TI_IsCrossTrade = true;

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.AIR];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var crossTradeFilter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.CrossTrade];

			var asserter = new Asserter(collection, filterBizO);

			crossTradeFilter.IsActive = true;
			crossTradeFilter.Property0 = false;
			asserter.AssertFiltering("Non Cross Trade", new[] { entry1, entry3, entry5 });

			crossTradeFilter.Property0 = true;
			asserter.AssertFiltering("Cross Trade", new[] { entry2, entry4, entry6, entry7 });
		}

		public void TestTransitTime()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "NZAKL");
			entry1.TI_TransitTime = "SMD";
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "NZAKL");
			entry2.TI_TransitTime = "6";

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.AIR];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var transitTimeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.TransitTime];

			var asserter = new Asserter(collection, filterBizO);

			transitTimeFilter.IsActive = true;
			transitTimeFilter.Property = "SMD";
			asserter.AssertFiltering("SMD", new[] { entry1 });

			transitTimeFilter.Property = "6";
			asserter.AssertFiltering("6 days", new[] { entry2 });
		}

		public void TestProductWarehouse()
		{
			AssertWarehouseFilter(WarehouseTypes.Codes.Product, RatingConstants.RateCategory.WHS,
				RateEntryFilterUtility.Constants.Codes.ProductWarehouse);
		}

		public void TestTransitWarehouse()
		{
			AssertWarehouseFilter(WarehouseTypes.Codes.Transit, RatingConstants.RateCategory.TRW,
				RateEntryFilterUtility.Constants.Codes.TransitWarehouse);
		}

		void AssertWarehouseFilter(string warehouseType, string ratingCategory, string filterName)
		{
			var whs1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			whs1[WhsWarehouseSchema.WW_WarehouseCode] = "WR1";
			whs1[WhsWarehouseSchema.WW_WarehouseName] = "Warehouse1";
			whs1[WhsWarehouseSchema.WW_WarehouseType] = warehouseType;

			var whs2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			whs2[WhsWarehouseSchema.WW_WarehouseCode] = "WR2";
			whs2[WhsWarehouseSchema.WW_WarehouseName] = "Warehouse2";
			whs2[WhsWarehouseSchema.WW_WarehouseType] = warehouseType;

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(ratingCategory);
			entry1.TI_WW_Warehouse = whs1.PK;
			var entry2 = clientRate.AddRateEntry(ratingCategory);
			entry2.TI_WW_Warehouse = whs2.PK;
			clientRate.AddRateEntry(ratingCategory);

			Factory.Save();

			var collection = clientRate.EntryCollections[ratingCategory];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var whsFilter = (ModuleGuidFilter)filterBizO[filterName];

			var asserter = new Asserter(collection, filterBizO);

			whsFilter.IsActive = true;
			whsFilter.Property = whs1.PK;
			asserter.AssertFiltering("whs1", new[] { entry1 });

			whsFilter.Property = whs2.PK;
			asserter.AssertFiltering("whs2", new[] { entry2 });
		}

		public void TestContainerType()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP");
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20RE");

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.FCL];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var containerTypeFilter = (ModuleGuidFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ContainerType];

			var asserter = new Asserter(collection, filterBizO);

			containerTypeFilter.IsActive = true;
			containerTypeFilter.Property = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			asserter.AssertFiltering("20GP", new[] { entry1 });

			containerTypeFilter.Property = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20RE")).PK;
			asserter.AssertFiltering("20RE", new[] { entry2 });
		}

		public void TestServiceLevel()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "D2D", "");
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "DIR", "");

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.FCL];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var serviceLevelFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ServiceLevel];

			var asserter = new Asserter(collection, filterBizO);

			serviceLevelFilter.IsActive = true;
			serviceLevelFilter.Property = "D2D";
			asserter.AssertFiltering("D2D", new[] { entry1 });

			serviceLevelFilter.Property = "DIR";
			asserter.AssertFiltering("DIR", new[] { entry2 });
		}

		public void TestCarrierServiceLevel()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ");
			entry1.TI_PL_NKCarrierServiceLevel = "STD";
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ");
			entry2.TI_PL_NKCarrierServiceLevel = "XYZ";

			Factory.Save();

			var filterBizO = new RateEntryFilterStripBusinessObject(clientRate.FCLRateEntriesForBinding);
			var carrierServiceLevelFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel];

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.FCL];
			var asserter = new Asserter(collection, filterBizO);

			carrierServiceLevelFilter.IsActive = true;
			carrierServiceLevelFilter.Property = "STD";
			asserter.AssertFiltering("STD", new[] { entry1 });

			carrierServiceLevelFilter.Property = "XYZ";
			asserter.AssertFiltering("XYZ", new[] { entry2 });
		}

		public void TestDateAndLocationFilters()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var clientRate2 = Factory.New<ClientRate>();
			clientRate2.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry1.TI_RateStartDate = new ZDate(2013, 1, 1);
			entry1.TI_RateEndDate = ZDate.Empty;

			var expiredEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUBNE", "SGSIN");
			expiredEntry.TI_RateStartDate = new ZDate(2012, 1, 1);
			expiredEntry.TI_RateEndDate = new ZDate(2013, 12, 31);

			var entryWithVia = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "GB");
			entryWithVia.TI_ViaLRC = "SGSIN";
			entryWithVia.TI_RateStartDate = new ZDate(2012, 1, 1);
			entryWithVia.TI_RateEndDate = ZDate.Empty;

			var entryWithFirstLoad = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "HKHKG", "US");
			entryWithFirstLoad.TI_FirstLoadLRC = "AUMEL";
			entryWithFirstLoad.TI_RateStartDate = new ZDate(2012, 1, 1);
			entryWithFirstLoad.TI_RateEndDate = ZDate.Empty;

			var entryWithLastDischarge = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "HKHKG", "US");
			entryWithLastDischarge.TI_LastDischargeLRC = "AUMEL";
			entryWithLastDischarge.TI_RateStartDate = new ZDate(2012, 1, 1);
			entryWithLastDischarge.TI_RateEndDate = ZDate.Empty;

			var entryWithFirstRouteSetLoad = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "HKHKG", "US");
			entryWithFirstRouteSetLoad.TI_FirstRouteSetLoadPortLRC = "AUMEL";
			entryWithFirstRouteSetLoad.TI_RateStartDate = new ZDate(2012, 1, 1);
			entryWithFirstRouteSetLoad.TI_RateEndDate = ZDate.Empty;

			var entryWithLastRouteSetDischarge = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "HKHKG", "US");
			entryWithLastRouteSetDischarge.TI_LastRouteSetDischargePortLRC = "AUMEL";
			entryWithLastRouteSetDischarge.TI_RateStartDate = new ZDate(2012, 1, 1);
			entryWithLastRouteSetDischarge.TI_RateEndDate = ZDate.Empty;

			var entryDifferentCategory = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			entryDifferentCategory.TI_RateStartDate = new ZDate(2013, 1, 1);
			entryDifferentCategory.TI_RateEndDate = ZDate.Empty;

			var entryDifferentRate = clientRate2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entryDifferentRate.TI_RateStartDate = new ZDate(2013, 1, 1);
			entryDifferentRate.TI_RateEndDate = ZDate.Empty;

			Factory.Save();

			var filterBizO = new RateEntryFilterStripBusinessObject(clientRate.AIRRateEntriesForBinding);
			var startDateFilter = (ModuleDateFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.StartDate];
			var endDateFilter = (ModuleDateFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.EndDate];
			var effectiveOnFilter = (ModuleSingleDateFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
			var showExpiredFilter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowExpired];
			var odFilter = (ModuleLocationFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.OriginDestination];
			var viaFilter = (ModuleNkFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.Via];
			var firstLoadFilter = (ModuleNkFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FirstLoad];
			var lastDischargeFilter = (ModuleNkFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.LastDischarge];
			var firstRouteSetLoadFilter = (ModuleNkFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad];
			var lastRouteSetDischargeFilter = (ModuleNkFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge];

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.AIR];
			var asserter = new Asserter(collection, filterBizO);

			asserter.AssertFiltering("Default state with no filters active (expired not shown)", new[] { entry1, entryWithVia, entryWithFirstLoad, entryWithLastDischarge, entryWithFirstRouteSetLoad, entryWithLastRouteSetDischarge });

			startDateFilter.IsActive = true;
			startDateFilter.Property1 = new ZDateTime(2011, 1, 1);
			startDateFilter.Property2 = new ZDateTime(2012, 2, 1);
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.StartDate, new[] { entryWithVia, entryWithFirstLoad, entryWithLastDischarge, entryWithFirstRouteSetLoad, entryWithLastRouteSetDischarge, expiredEntry });
			filterBizO.FilterStrips.ClearValues();
			startDateFilter.IsActive = false;

			endDateFilter.IsActive = true;
			endDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.EndDate, new[] { expiredEntry });
			filterBizO.FilterStrips.ClearValues();
			endDateFilter.IsActive = false;

			effectiveOnFilter.IsActive = true;
			effectiveOnFilter.Property1 = new ZDateTime(2012, 3, 1);
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.EffectiveOn, new[] { entryWithVia, entryWithFirstLoad, entryWithLastDischarge, entryWithFirstRouteSetLoad, entryWithLastRouteSetDischarge, expiredEntry });
			filterBizO.FilterStrips.ClearValues();
			effectiveOnFilter.IsActive = false;

			showExpiredFilter.IsActive = true;
			showExpiredFilter.Property0 = true;
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.ShowExpired, new[] { entry1, entryWithVia, entryWithFirstLoad, entryWithLastDischarge, entryWithFirstRouteSetLoad, entryWithLastRouteSetDischarge, expiredEntry });
			filterBizO.FilterStrips.ClearValues();
			showExpiredFilter.IsActive = false;

			odFilter.IsActive = true;
			odFilter.Property1 = "AU";
			asserter.AssertFiltering("Origin", new[] { entry1, entryWithVia });
			odFilter.Property2 = "GB";
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.OriginDestination, new[] { entryWithVia });
			filterBizO.FilterStrips.ClearValues();
			odFilter.IsActive = false;

			viaFilter.IsActive = true;
			viaFilter.Property = "SGSIN";
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.Via, new[] { entryWithVia });
			filterBizO.FilterStrips.ClearValues();
			viaFilter.IsActive = false;

			firstLoadFilter.IsActive = true;
			firstLoadFilter.Property = "AUMEL";
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.FirstLoad, new[] { entryWithFirstLoad });
			filterBizO.FilterStrips.ClearValues();
			firstLoadFilter.IsActive = false;

			lastDischargeFilter.IsActive = true;
			lastDischargeFilter.Property = "AUMEL";
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.LastDischarge, new[] { entryWithLastDischarge });
			filterBizO.FilterStrips.ClearValues();
			lastDischargeFilter.IsActive = false;

			firstRouteSetLoadFilter.IsActive = true;
			firstRouteSetLoadFilter.Property = "AUMEL";
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad, new[] { entryWithFirstRouteSetLoad });
			filterBizO.FilterStrips.ClearValues();
			firstRouteSetLoadFilter.IsActive = false;

			lastRouteSetDischargeFilter.IsActive = true;
			lastRouteSetDischargeFilter.Property = "AUMEL";
			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge, new[] { entryWithLastRouteSetDischarge });
			filterBizO.FilterStrips.ClearValues();
			lastRouteSetDischargeFilter.IsActive = false;

			filterBizO.ClearRateEntryFilterStrips();
			asserter.AssertFiltering("Reloaded with no filter (expired shown)", new[] { entry1, entryWithVia, entryWithFirstLoad, entryWithLastDischarge, entryWithFirstRouteSetLoad, entryWithLastRouteSetDischarge, expiredEntry });
		}

		public void TestFromToDescriptionFilters()
		{
			var helper = new TestHelper(Factory);

			var clientRate = helper.NewClientRate(helper.NewOrgHeader());

			var sydZone = helper.NewInternationalZone("GSYD", null, "AUSYD", "AUNSY", "AUPRM");
			sydZone.FZ_Description = "Greater Sydney";

			var ausZone = helper.NewInternationalZone("AUST", null, "AT", "AU");
			ausZone.FZ_Description = "Aust Zones";

			var unlocoEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AUSYD", "AUMEL");
			var countryEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AU", "AT");
			var zoneEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "GSYD", "AUST");

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.TBC].LazyLoadingCollection;
			var filter = new RateEntryFilterStripBusinessObject(collection);
			var fromFilter = (ModuleTextFilter)filter[RateEntryFilterUtility.Constants.Codes.FromLocationDescription];

			var asserter = new Asserter(collection, filter);

			fromFilter.IsActive = true;
			fromFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			fromFilter.Property = "Sydney";

			asserter.AssertFiltering("Only the UNLOCO begins with 'Sydney'", new[] { unlocoEntry });

			fromFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;

			asserter.AssertFiltering("Both the UNLOCO and the zone have descriptions containing 'Sydney'", new[] { unlocoEntry, zoneEntry });

			fromFilter.Property = "stralia";
			asserter.AssertFiltering("Should match by country description", new[] { countryEntry });

			fromFilter.Property = "";
			var toFilter = (ModuleTextFilter)filter[RateEntryFilterUtility.Constants.Codes.ToLocationDescription];
			toFilter.IsActive = true;
			toFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			toFilter.Property = "Melb";

			asserter.AssertFiltering("Should match 'Melbourne'", new[] { unlocoEntry });

			toFilter.Property = "Aust";

			asserter.AssertFiltering("Should match both 'Austria' and 'Aust Zones'", new[] { countryEntry, zoneEntry });
		}

		public void TestFromToDescriptionFiltersShouldFindIATARegionCodes()
		{
			var costing = Factory.New<Costing>();

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "SYD", "LAX");
			entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "MEL");
			entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry2.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUBNE", "AUMEL");
			entry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry3.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			Factory.Save();

			var collection = costing.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var asserter = new Asserter(collection, filterBizO);

			filterBizO.FilterStrips.ClearValues();
			var fromFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FromLocationDescription];
			fromFilter.IsActive = true;
			fromFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			fromFilter.Property = "SYDNEY";

			asserter.AssertFiltering("Both the UNLOCO and the zone have descriptions containing 'Sydney'", new[] { entry1, entry2 });

			fromFilter.Property = "";
			var toFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ToLocationDescription];
			toFilter.IsActive = true;
			toFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			toFilter.Property = "Melb";

			asserter.AssertFiltering("Should match both 'Austria' and 'Aust Zones'", new[] { entry2, entry3 });
		}

		public void TestFromToZoneFilters()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var zoneTo = Factory.NewWithValidTestData<RateTransportZone>();
			var zoneFrom = Factory.NewWithValidTestData<RateTransportZone>();

			var entryZones = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AUSYD", "GB");
			entryZones.TI_TZ_OriginZone = zoneFrom.PK;
			entryZones.TI_TZ_DestinationZone = zoneTo.PK;
			entryZones.TI_RateStartDate = new ZDate(2013, 1, 1);
			entryZones.TI_RateEndDate = ZDate.Empty;

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.TBC].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var fromToZoneFilter = (ModuleGuidsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FromToZone];

			var asserter = new Asserter(collection, filterBizO);

			fromToZoneFilter.IsActive = true;
			fromToZoneFilter.Property1 = zoneFrom.PK;
			fromToZoneFilter.Property2 = zoneTo.PK;

			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.FromToZone, new[] { entryZones });
		}

		public void TestFromToSuburbFilters()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var suburbTo = Factory.NewWithValidTestData<RefCityTown>();
			var suburbFrom = Factory.NewWithValidTestData<RefCityTown>();

			var entrySuburbs = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AUSYD", "USLAX");
			entrySuburbs.TI_RateStartDate = new ZDate(2013, 1, 1);
			entrySuburbs.TI_RateEndDate = ZDate.Empty;
			entrySuburbs.TI_R9_FromSuburb = suburbFrom.PK;
			entrySuburbs.TI_R9_ToSuburb = suburbTo.PK;

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.TBC].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var fromToSuburbFilter = (ModuleGuidsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FromToSuburb];

			var asserter = new Asserter(collection, filterBizO);

			fromToSuburbFilter.IsActive = true;
			fromToSuburbFilter.Property1 = suburbFrom.PK;
			fromToSuburbFilter.Property2 = suburbTo.PK;

			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.FromToSuburb, new[] { entrySuburbs });
		}

		public void TestFromToPostcodeFilters()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var postcodeTo = "2205";
			var postcodeFrom = "2015";

			var entryPostCode = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AUBNE", "SGSIN");
			entryPostCode.TI_RateStartDate = new ZDate(2013, 1, 1);
			entryPostCode.TI_RateEndDate = ZDate.Empty;
			entryPostCode.TI_CartagePickupAddressPostCode = postcodeFrom;
			entryPostCode.TI_CartageDeliveryAddressPostCode = postcodeTo;

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.TBC];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var fromPostcodeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FromPostcode];
			var toPostcodeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ToPostcode];

			var asserter = new Asserter(collection, filterBizO);

			fromPostcodeFilter.IsActive = true;
			fromPostcodeFilter.Property = postcodeFrom;

			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.FromPostcode, new[] { entryPostCode });

			filterBizO.FilterStrips.ClearValues();
			fromPostcodeFilter.IsActive = false;

			toPostcodeFilter.IsActive = true;
			toPostcodeFilter.Property = postcodeTo;

			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.ToPostcode, new[] { entryPostCode });
		}

		public void TestFromToOrganizationFilters()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var entryConsignorConsignee = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AUSYD", "USLAX");
			entryConsignorConsignee.TI_OH_Consignor = consignor.PK;
			entryConsignorConsignee.TI_OH_Consignee = consignee.PK;
			entryConsignorConsignee.TI_RateStartDate = new ZDate(2013, 1, 1);
			entryConsignorConsignee.TI_RateEndDate = ZDate.Empty;

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.TBC];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var fromToOrganizationFilter = (ModuleGuidsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FromToOrganization];

			var asserter = new Asserter(collection, filterBizO);

			fromToOrganizationFilter.IsActive = true;
			fromToOrganizationFilter.Property1 = consignor.PK;
			fromToOrganizationFilter.Property2 = consignee.PK;

			asserter.AssertFiltering(RateEntryFilterUtility.Constants.Codes.FromToOrganization, new[] { entryConsignorConsignee });
		}

		public void TestTransportModeFiltersForFreight()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var airEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			var seaEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			var roadEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ROA, "AUSYD", "USLAX");
			var railEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.RAI, "AUSYD", "USLAX");

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.ORG];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var transportModeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.TransportMode];

			var asserter = new Asserter(collection, filterBizO);

			transportModeFilter.IsActive = true;
			transportModeFilter.Property = RateEntryFilterUtility.TransportModes.AIF;
			asserter.AssertFiltering("Air", new[] { airEntry });

			transportModeFilter.Property = RateEntryFilterUtility.TransportModes.SEF;
			asserter.AssertFiltering("Sea", new[] { seaEntry });

			transportModeFilter.Property = RateEntryFilterUtility.TransportModes.ROF;
			asserter.AssertFiltering("Road", new[] { roadEntry });

			transportModeFilter.Property = RateEntryFilterUtility.TransportModes.RAF;
			asserter.AssertFiltering("Rail", new[] { railEntry });
		}

		public void TestTransportModeFilter()
		{
			var ratingHeader = SetupRatesForTestTransportModeFilter();

			AssertTransportModeFilter(ratingHeader, "TRN", "LRO", "LRO");
			AssertTransportModeFilter(ratingHeader, "TBC", "LRA", "LRA");
			AssertTransportModeFilter(ratingHeader, "CST", "RAI", "RAI");
			AssertTransportModeFilter(ratingHeader, "UNP", "LRO", "LRO");
			AssertTransportModeFilter(ratingHeader, "PAC", "LRA", "LRA");
			AssertTransportModeFilter(ratingHeader, "SDE", "FCL", "FCL");
			AssertTransportModeFilter(ratingHeader, "SOR", "LCL", "LCL");
			AssertTransportModeFilter(ratingHeader, "DST", "BCN", "BCN");
			AssertTransportModeFilter(ratingHeader, "ORG", "BBK", "BBK");
			AssertTransportModeFilter(ratingHeader, "LCL", "FTL", "FTL");
			AssertTransportModeFilter(ratingHeader, "FCL", "ROA", "ROA");
			AssertTransportModeFilter(ratingHeader, "AIR", "ULD", "ULD");
		}

		void AssertTransportModeFilter(RatingHeader ratingHeader, string rateCategory, string filterValue, params string[] expectedRateModes)
		{
			var collection = ratingHeader.EntryCollections[rateCategory];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var transportModeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.TransportMode];

			var asserter = new Asserter(collection, filterBizO);

			transportModeFilter.IsActive = true;
			transportModeFilter.Property = filterValue;

			collection.LoadedCollection.LoadAndSortForGUI();
			var expectedRateEntries = collection.LoadedCollection.Where(e => expectedRateModes.Any(x => string.IsNullOrWhiteSpace(x)) || expectedRateModes.ToList().Contains(e.TI_Mode));
			Assert("pre condition", expectedRateEntries.Any());

			asserter.AssertFiltering($"Rate Category: {rateCategory} - Filter Value: {filterValue}", expectedRateEntries);
		}

		RatingHeader SetupRatesForTestTransportModeFilter()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			(string category, string[] modes)[] rates = new[]
			{
				("AIR", new[] { "LSE", "ULD", "BCN" }),
				("FCL", new[] { "SEA", "ROA", "RAI", "BCN" }),
				("LCL", new[] { "LCL", "LRO", "FTL", "LRA", "FWL", "BCN", "BBK", "BLK", "ROR" }),
				("ORG", new[] { "ALL", "AIR", "ULD", "LSE", "SEA", "LCL", "FCL", "ROA", "LRO", "FRO", "FTL", "RAI", "LRA", "FRA", "FWL", "MAI", "BCN", "BBK", "BLK", "ROR" }),
				("DST", new[] { "ALL", "AIR", "ULD", "LSE", "SEA", "LCL", "FCL", "ROA", "LRO", "FRO", "FTL", "RAI", "LRA", "FRA", "FWL", "MAI", "BCN", "BBK", "BLK", "ROR" }),
				("SOR", new[] { "ALL", "LCL", "FCL" }),
				("SDE", new[] { "ALL", "LCL", "FCL" }),
				("PAC", new[] { "ALL", "AIR", "ULD", "LSE", "SEA", "LCL", "FCL", "ROA", "LRO", "FRO", "FTL", "RAI", "LRA", "FRA", "FWL" }),
				("UNP", new[] { "ALL", "AIR", "ULD", "LSE", "SEA", "LCL", "FCL", "ROA", "LRO", "FRO", "FTL", "RAI", "LRA", "FRA", "FWL" }),
				("CST", new[] { "ALL", "AIR", "SEA", "ROA", "RAI" }),
				("TRN", new[] { "ALL", "AIR", "ROA", "LRO", "FRO" }),
				("TBC", new[] { "ALL", "AIR", "ROA", "LRO", "FRO", "FTL", "RAI", "LRA", "FRA", "FWL" }),
			};

			foreach (var rate in rates)
			{
				foreach (var mode in rate.modes)
				{
					clientRate.AddRateEntry(rate.category, mode, origin: "AUSYD");
				}
			}

			Factory.Save();

			return clientRate;
		}

		public void TestShowExpiredFilterWorksCorrectlyWithPricingPageCollection()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry1.RateLines[0].GetCalculator<CombinedCalculator>().BaseRate = 150m;
			entry1.TI_RateStartDate = new ZDate(2013, 1, 1);
			entry1.TI_RateEndDate = ZDate.Empty;

			var expiredEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUBNE", "SGSIN");
			expiredEntry.RateLines[0].GetCalculator<CombinedCalculator>().BaseRate = 150m;
			expiredEntry.TI_RateStartDate = new ZDate(2012, 1, 1);
			expiredEntry.TI_RateEndDate = new ZDate(2013, 12, 31);

			var entryDifferentCategory = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			entryDifferentCategory.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 150m;
			entryDifferentCategory.TI_RateStartDate = new ZDate(2013, 1, 1);
			entryDifferentCategory.TI_RateEndDate = ZDate.Empty;

			Factory.Save();

			var collection = clientRate.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var showExpiredFilter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowExpired];
			collection.Load();

			var collection1 = new PricingPageCollection(clientRate);
			collection1.Load(PricingPaginationStrategy.StandardStyle);
			var expected = new[] { entry1.PK, entryDifferentCategory.PK };
			var actual = collection1.Cast<PricingPage>().SelectMany(x => x.RateEntries).Select(e => e.PK);

			AssertContainsExactElementsInAnyOrder("Should include all but the expired rate by default", expected, actual);

			filterBizO.FilterStrips.ClearValues();
			showExpiredFilter.IsActive = true;
			showExpiredFilter.Property0 = true;
			collection.Load();

			var collection2 = new PricingPageCollection(clientRate);
			collection2.Load(PricingPaginationStrategy.StandardStyle);
			expected = new[] { entry1.PK, expiredEntry.PK, entryDifferentCategory.PK };
			actual = collection2.Cast<PricingPage>().SelectMany(x => x.RateEntries).Select(e => e.PK);

			AssertContainsExactElementsInAnyOrder("Should now include the expired rate as we've enabled the filter", expected, actual);
		}

		public void TestExpiredQuotesAreNotFilteredOut()
		{
			var quote = Factory.NewWithValidTestData<Quote>();

			var expiredEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUBNE", "SGSIN");
			expiredEntry.TI_RateStartDate = new ZDate(2015, 1, 1);
			expiredEntry.TI_RateEndDate = new ZDate(2015, 12, 31);

			var expiredEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			expiredEntry2.TI_RateStartDate = new ZDate(2015, 1, 1);
			expiredEntry2.TI_RateEndDate = new ZDate(2015, 12, 31);

			Factory.Save();

			var collection = quote.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			Assert("Should not have expired rates filter by default", !filterBizO.Filter.LiteralTextADO.Contains("TI_RateEndDate"));

			var showExpiredFilter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowExpired];
			Assert("Show Expired should be on by default for Quotes", showExpiredFilter.Property0);

			collection.LoadAndSortForGUI();
			AssertEquals("Should load both rates", 2, collection.Count);
		}

		[TestDate(2023, 11, 15)]
		public void TestCommodityCodeFilter()
		{
			var commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";

			var tariff1 = Factory.NewWithValidTestData<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.TI_RH_NKCommodityCode = commodityCode1.RH_Code;

			var entry2 = tariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry2.TI_RH_NKCommodityCode = "";

			Factory.Save();

			var collection = tariff1.EntryCollections[RatingConstants.RateCategory.FCL].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			AssertEquals("Should not have Commodity Code filter by default", "TI_RateEndDate >= #2023-11-15 00:00:00.000# or TI_RateEndDate is null", filterBizO.Filter.LiteralTextADO);
			var commodityCodeFilter = (ModuleNkFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.CommodityCode];

			commodityCodeFilter.IsActive = true;
			collection.Load();

			var actual1 = collection
				.Select(s => s.TI_RH_NKCommodityCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"If property is empty, we will load all tariffs.",
				new ZString[] { "COM1", "" },
				actual1
			);

			commodityCodeFilter.Property = commodityCode1.RH_Code;
			collection.Load();

			var actual2 = collection
				.Select(s => s.TI_RH_NKCommodityCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Only entry1 has an entry with commodityCode1.",
				new ZString[] { "COM1" },
				actual2
			);
		}

		public void TestFMCTariffIDFilter()
		{
			var tariff1 = Factory.NewWithValidTestData<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.TI_FMCTariffID = "ABC";
			var entry2 = tariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry2.TI_FMCTariffID = "";

			Factory.Save();

			var collection = tariff1.EntryCollections[RatingConstants.RateCategory.FCL].LazyLoadingCollection;
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			Assert("Should not have FMC Tariff ID filter by default", !filterBizO.Filter.LiteralTextADO.Contains("TI_FMCTariffID"));

			var fmcTariffIDFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.FMCTariffID];

			fmcTariffIDFilter.IsActive = true;
			fmcTariffIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("FMCTariffID MaxLength", 4, fmcTariffIDFilter.MaxLength);
			collection.Load();

			var actual1 = collection.Select(s => s.PK).ToArray();
			var expected1 = new[] { entry2.PK };
			AssertContainsExactElementsInAnyOrder(
				"When comparison operator is blank, only entry2 is in Collection",
				expected1,
				actual1
			);

			fmcTariffIDFilter.Property = "A";
			fmcTariffIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			collection.Load();
			var actual2 = collection.Select(s => s.PK).ToArray();
			var expected2 = new[] { entry1.PK };
			AssertContainsExactElementsInAnyOrder(
				"Only entry1 starts with 'A'.",
				expected2,
				actual2
			);
		}

		public void TestOnlyCompanyTariffAndClientRateAndQuotationIncludeFMCTariffIDFilter()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var quote = Factory.NewWithValidTestData<Quote>();
			var costing = Factory.NewWithValidTestData<Costing>();
			var interCompanyTariff = Factory.NewWithValidTestData<IntercompanyTariff>();
			var ratingHeadersNotIncludeFMCTariffIDFilter = new RatingHeader[] { interCompanyTariff, costing };

			foreach (var ratingHeader in ratingHeadersNotIncludeFMCTariffIDFilter)
			{
				var filterBizO = new RateEntryFilterStripBusinessObject(ratingHeader.AIRRateEntriesForBinding);
				AssertNull("Non-Company Tariff/Client Rate/Quotation rate should not contain 'FMC Tariff ID' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FMCTariffID]);
			}
			var companyTariffFilter = new RateEntryFilterStripBusinessObject(companyTariff.AIRRateEntriesForBinding);
			var clientRateFilter = new RateEntryFilterStripBusinessObject(clientRate.AIRRateEntriesForBinding);
			var quotationFilter = new RateEntryFilterStripBusinessObject(quote.AIRRateEntriesForBinding);
			AssertNotNull("Company Tariff rate should contain 'FMC Tariff ID' filter", companyTariffFilter.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FMCTariffID]);
			AssertNotNull("Client Rate should contain 'FMC Tariff ID' filter", clientRateFilter.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FMCTariffID]);
			AssertNotNull("Quotation rate should contain 'FMC Tariff ID' filter", quotationFilter.ModuleFilters[RateEntryFilterUtility.Constants.Codes.FMCTariffID]);
		}

		#region Test Include TACT Rates Filter

		public void TestIncludeTactRatesFilterNotApplicableForNonCostingRatingHeaders()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var quote = Factory.NewWithValidTestData<Quote>();
			var ratingHeaders = new RatingHeader[] { companyTariff, clientRate, quote };

			foreach (var ratingHeader in ratingHeaders)
			{
				var filterBizO = new RateEntryFilterStripBusinessObject(ratingHeader.AIRRateEntriesForBinding);
				AssertNull("Non-costing rate should not contain 'Include TACT Rates' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates]);

				var filterText = filterBizO.Filter.LiteralTextSqlFormatted;
				Assert("Generated query does not filter for tact rates", !filterText.Contains("TI_IsTact"));
			}
		}

		public void TestIncludeTactRatesFilterDefaultIsSTD()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = Guid.Empty;

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.AIRRateEntriesForBinding);
			var includeTactRatesFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates];
			AssertEquals("AIR tab contains 'Include TACT Rates' filter with STD as the default", RateEntryFilterStripBusinessObject.IncludeTACTRatesConstants.Code.STD, includeTactRatesFilter.DefaultProperty);

			filterBizO = new RateEntryFilterStripBusinessObject(costing.CAIRateEntriesForBinding);
			includeTactRatesFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates];
			AssertEquals("CAI tab contains 'Include TACT Rates' filter with STD as the default", RateEntryFilterStripBusinessObject.IncludeTACTRatesConstants.Code.STD, includeTactRatesFilter.DefaultProperty);
		}

		public void TestIncludeTactRatesFilterAvailableToAirRateCategory()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = Guid.Empty;

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.AIRRateEntriesForBinding);
			AssertNotNull("AIR tab contains 'Include TACT Rates' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates]);

			filterBizO = new RateEntryFilterStripBusinessObject(costing.CAIRateEntriesForBinding);
			AssertNotNull("CAI tab contains 'Include TACT Rates' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates]);
		}

		public void TestIncludeTactRatesFilterAvailableToSummaryRateCategory()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = Guid.Empty;

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.SummaryRateEntries);
			AssertNotNull("Summary tab contains 'Include TACT Rates' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates]);
		}

		public void TestIncludeTactRatesFilterNotAvailableToOtherRateCategories()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = Guid.Empty;

			foreach (var rateCategory in RatingConstants.RateCategory.RateCategories.Where(x => x != RatingConstants.RateCategory.AIR && x != RatingConstants.RateCategory.CAI && x != RatingConstants.RateCategory.SummaryRatesCategory))
			{
				var collection = costing.EntryCollections[rateCategory].LazyLoadingCollection;
				var filterBizO = new RateEntryFilterStripBusinessObject(collection);
				AssertNull("Other tabs than AIR or Summary should not contain 'Include TACT Rates' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates]);
			}
		}

		public void TestIncludeTactRatesFilter_ALL_WorksCorrectlyWithAirRateEntryCollection()
		{
			var costing = Factory.New<Costing>();

			var collection = CreateRateEntryCollectionForIncludeTactRatesFilter(costing, RatingConstants.RateCategory.AIR, costing.AIRRateEntriesForBinding, "ALL");
			collection.LoadAndSortForGUI();
			AssertEquals("Collection loaded to include TACT contains both records of TACT rates and none TACT rates", 2, collection.Count);
			Assert("Rate entry collection contains TACT Rates", collection.Cast<RateEntry>().Any(x => x.TI_IsTact));
			Assert("Rate entry collection contains none TACT Rates", collection.Cast<RateEntry>().Any(x => !x.TI_IsTact));

			collection = CreateRateEntryCollectionForIncludeTactRatesFilter(costing, RatingConstants.RateCategory.CAI, costing.CAIRateEntriesForBinding, "ALL");
			collection.LoadAndSortForGUI();
			AssertEquals("Collection loaded to include TACT contains both records of TACT rates and none TACT rates", 2, collection.Count);
			Assert("Rate entry collection contains TACT Rates", collection.Cast<RateEntry>().Any(x => x.TI_IsTact));
			Assert("Rate entry collection contains none TACT Rates", collection.Cast<RateEntry>().Any(x => !x.TI_IsTact));
		}

		public void TestIncludeTactRatesFilter_STD_WorksCorrectlyWithAirRateEntryCollection()
		{
			var costing = Factory.New<Costing>();

			var collection = CreateRateEntryCollectionForIncludeTactRatesFilter(costing, RatingConstants.RateCategory.AIR, costing.AIRRateEntriesForBinding, "STD");
			collection.LoadAndSortForGUI();
			AssertEquals("Collection loaded not to include TACT contains only records of none TACT rates", 1, collection.Count);
			Assert("Rate entry collection contains only none TACT Rates", collection.Cast<RateEntry>().All(x => !x.TI_IsTact));

			collection = CreateRateEntryCollectionForIncludeTactRatesFilter(costing, RatingConstants.RateCategory.CAI, costing.CAIRateEntriesForBinding, "STD");
			collection.LoadAndSortForGUI();
			AssertEquals("Collection loaded not to include TACT contains only records of none TACT rates", 1, collection.Count);
			Assert("Rate entry collection contains only none TACT Rates", collection.Cast<RateEntry>().All(x => !x.TI_IsTact));
		}

		public void TestIncludeTactRatesFilter_TAC_WorksCorrectlyWithAirRateEntryCollection()
		{
			var costing = Factory.New<Costing>();

			var collection = CreateRateEntryCollectionForIncludeTactRatesFilter(costing, RatingConstants.RateCategory.AIR, costing.AIRRateEntriesForBinding, "TAC");
			collection.LoadAndSortForGUI();
			AssertEquals("Collection loaded to include TACT contains only records of TACT rates", 1, collection.Count);
			Assert("Rate entry collection contains only TACT Rates", collection.Cast<RateEntry>().All(x => x.TI_IsTact));

			collection = CreateRateEntryCollectionForIncludeTactRatesFilter(costing, RatingConstants.RateCategory.CAI, costing.CAIRateEntriesForBinding, "TAC");
			collection.LoadAndSortForGUI();
			AssertEquals("Collection loaded to include TACT contains only records of TACT rates", 1, collection.Count);
			Assert("Rate entry collection contains only TACT Rates", collection.Cast<RateEntry>().All(x => x.TI_IsTact));
		}

		RateEntryCollection CreateRateEntryCollectionForIncludeTactRatesFilter(Costing costing, string category, RateEntryCollection rateEntriesForBinding, string includeTactRates)
		{
			var entry1 = costing.AddRateEntry(category, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry1.TI_IsTact = true;
			entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry2 = costing.AddRateEntry(category, Core.Constants.RateMode.ULD, "AUBNE", "SGSIN");
			entry2.TI_IsTact = false;
			entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry2.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			Factory.Save();

			var filterBizO = new RateEntryFilterStripBusinessObject(rateEntriesForBinding);
			filterBizO.FilterStrips.ClearValues();

			var showExpiredFilter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowExpired];
			showExpiredFilter.IsActive = true;
			showExpiredFilter.Property0 = true;

			var includeTactRatesFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates];
			includeTactRatesFilter.IsActive = true;
			includeTactRatesFilter.DefaultProperty = includeTactRates;

			var collection = costing.EntryCollections[category];
			return collection.LazyLoadingCollection;
		}

		#endregion

		#region Test Origin/Destination/Via IATA Region Filter

		public void TestIATARegionFiltersNotApplicableForNonStandardCostingRatingHeaders()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var quote = Factory.NewWithValidTestData<Quote>();
			var nonStandardCosting = Factory.NewWithValidTestData<Costing>();

			var ratingHeaders = new RatingHeader[] { companyTariff, clientRate, quote, nonStandardCosting };

			foreach (var ratingHeader in ratingHeaders)
			{
				Assert("Non Standard Costing Rate", !ratingHeader.IsStandardCostRate());

				var filterBizO = new RateEntryFilterStripBusinessObject(ratingHeader.AIRRateEntriesForBinding);
				AssertNull("Non-costing rate should not contain 'Origin IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.OriginIATARegion]);
				AssertNull("Non-costing rate should not contain 'Destination IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.DestinationIATARegion]);
				AssertNull("Non-costing rate should not contain 'Via IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ViaIATARegion]);

				var filterText = filterBizO.Filter.LiteralTextSqlFormatted;
				Assert("Generated query does not filter for tact rates", !filterText.Contains(RefUNLOCOSchema.RL_IATARegionCode.Name));

				filterBizO = new RateEntryFilterStripBusinessObject(ratingHeader.CAIRateEntriesForBinding);
				AssertNull("Non-costing rate should not contain 'Origin IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.OriginIATARegion]);
				AssertNull("Non-costing rate should not contain 'Destination IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.DestinationIATARegion]);
				AssertNull("Non-costing rate should not contain 'Via IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ViaIATARegion]);

				filterText = filterBizO.Filter.LiteralTextSqlFormatted;
				Assert("Generated query does not filter for tact rates", !filterText.Contains(RefUNLOCOSchema.RL_IATARegionCode.Name));
			}
		}

		public void TestIATARegionFiltersAvailableToAirRateCategory()
		{
			var costing = Factory.New<Costing>();

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.AIRRateEntriesForBinding);
			AssertNotNull("AIR tab contains 'Origin IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.OriginIATARegion]);
			AssertNotNull("AIR tab contains 'Destination IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.DestinationIATARegion]);
			AssertNotNull("AIR tab contains 'Via IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ViaIATARegion]);
		}

		public void TestIATARegionFiltersAvailableToCAIRateCategory()
		{
			var costing = Factory.New<Costing>();

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.CAIRateEntriesForBinding);
			AssertNotNull("AIR tab contains 'Origin IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.OriginIATARegion]);
			AssertNotNull("AIR tab contains 'Destination IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.DestinationIATARegion]);
			AssertNotNull("AIR tab contains 'Via IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ViaIATARegion]);
		}

		public void TestIATARegionFiltersAvailableToSummaryRateCategory()
		{
			var costing = Factory.New<Costing>();

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.SummaryRateEntries);
			AssertNotNull("Summary tab contains 'Origin IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.OriginIATARegion]);
			AssertNotNull("Summary tab contains 'Destination IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.DestinationIATARegion]);
			AssertNotNull("Summary tab contains 'Via IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ViaIATARegion]);
		}

		public void TestIATARegionFiltersNotAvailableToOtherRateCategories()
		{
			var costing = Factory.New<Costing>();

			foreach (var rateCategory in RatingConstants.RateCategory.RateCategories.Where(x => x != RatingConstants.RateCategory.AIR && x != RatingConstants.RateCategory.CAI && x != RatingConstants.RateCategory.SummaryRatesCategory))
			{
				var collection = costing.EntryCollections[rateCategory].LazyLoadingCollection;
				var filterBizO = new RateEntryFilterStripBusinessObject(collection);
				AssertNull("Other tabs than AIR/CAI/Summary should not contain 'Origin IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.OriginIATARegion]);
				AssertNull("Other tabs than AIR/CAI/Summary should not contain 'Destination IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.DestinationIATARegion]);
				AssertNull("Other tabs than AIR/CAI/Summary should not contain 'Via IATA Region' filter", filterBizO.ModuleFilters[RateEntryFilterUtility.Constants.Codes.ViaIATARegion]);
			}
		}

		[StressTest]
		public void TestOriginIATARegionFilterWorksCorrectlyWithAirRateEntryCollection()
		{
			var collection = CreateRateEntryCollectionForIATARegionFilters(RateEntryFilterUtility.Constants.Codes.OriginIATARegion, "SYD");
			collection.LoadAndSortForGUI();

			AssertEquals("Collection loaded with iata region as origin should contains 3-letter codes", 2, collection.Count);
			Assert("Rate entry collection contains only IATA Region codes as origin", collection.Cast<RateEntry>().All(x => x.TI_OriginLRC == "SYD"));
		}

		[StressTest]
		public void TestDestinationIATARegionFilterWorksCorrectlyWithAirRateEntryCollection()
		{
			var collection = CreateRateEntryCollectionForIATARegionFilters(RateEntryFilterUtility.Constants.Codes.DestinationIATARegion, "SIN");
			collection.LoadAndSortForGUI();

			AssertEquals("Collection loaded with iata region as destination should contains 3-letter codes", 1, collection.Count);
			Assert("Rate entry collection contains only IATA Region codes as destination", collection.Cast<RateEntry>().All(x => x.TI_DestinationLRC == "SIN"));
		}

		[StressTest]
		public void TestViaIATARegionFilterWorksCorrectlyWithAirRateEntryCollection()
		{
			var collection = CreateRateEntryCollectionForIATARegionFilters(RateEntryFilterUtility.Constants.Codes.ViaIATARegion, "SYD");
			collection.LoadAndSortForGUI();

			AssertEquals("Collection loaded with iata region as via should contains 3-letter codes", 2, collection.Count);
			Assert("Rate entry collection contains only IATA Region codes as via", collection.Cast<RateEntry>().All(x => x.TI_ViaLRC == "SYD"));
		}

		RateEntryCollection CreateRateEntryCollectionForIATARegionFilters(string filterKey, string iataRegion)
		{
			var costing = Factory.New<Costing>();

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "SYD", "LAX");
			entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "BNE", "SIN");
			entry2.TI_ViaLRC = "SYD";
			entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry2.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "SYD", "");
			entry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry3.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry4 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "", "LAX");
			entry4.TI_ViaLRC = "SIN";
			entry4.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry4.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry5 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry5.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry5.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry6 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUBNE", "SGSIN");
			entry6.TI_ViaLRC = "AUSYD";
			entry6.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry6.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry7 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "");
			entry7.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry7.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var entry8 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "", "USLAX");
			entry8.TI_ViaLRC = "SYD";
			entry8.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			entry8.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			Factory.Save();

			var filterBizO = new RateEntryFilterStripBusinessObject(costing.AIRRateEntriesForBinding);
			filterBizO.FilterStrips.ClearValues();

			var showExpiredFilter = (ModuleFlagsFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.ShowExpired];
			showExpiredFilter.IsActive = true;
			showExpiredFilter.Property0 = true;

			var iataRegionFilter = (ModuleTextFilter)filterBizO[filterKey];
			iataRegionFilter.IsActive = true;
			iataRegionFilter.DefaultProperty = iataRegion;

			var collection = costing.EntryCollections[RatingConstants.RateCategory.AIR];
			return collection.LazyLoadingCollection;
		}

		#endregion

		public void TestShowAllRateLinesFilter()
		{
			// Setup rate entries and rate lines
			var helper = new TestHelper(Factory);
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "NTC1";
			client.OH_FullName = "New Test Client";
			client.OH_RL_NKClosestPort = "AUSYD";
			client.OH_IsDebtor = true;
			client.OH_IsConsignor = true;
			client.OH_IsConsignee = true;
			var address = client.MainAddress;
			address.OA_Address1 = "123 Fake Street";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "2000";

			var rate = helper.NewGlobalClientRate(client);

			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var frtChargeCode = helper.ChargeCodes.CreateGlobalCharge("FRT");
			var bafChargeCode = helper.ChargeCodes.CreateGlobalCharge("BAF");
			var cafChargeCode = helper.ChargeCodes.CreateGlobalCharge("CAF");

			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, origin: "AU", destination: "NZ", commodity: "GEN", removeLines: true);
			var rateLine1A = rateEntry1.AddRateLine("FRT", "UNT", QuantityUnit.CN, "AUD");
			rateLine1A.TL_RateStartDate = new ZDate(2024, 1, 1);
			rateLine1A.TL_RateEndDate = new ZDate(2024, 1, 31);

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, origin: "AU", destination: "NZ", commodity: "AABT", removeLines: true);
			var rateLine2A = rateEntry2.AddRateLine("FRT", "UNT", QuantityUnit.CN, "AUD");
			rateLine2A.TL_RateStartDate = new ZDate(2024, 1, 1);
			rateLine2A.TL_RateEndDate = new ZDate(2024, 1, 31);
			var rateLine2B = rateEntry2.AddRateLine("BAF", "UNT", QuantityUnit.CN, "AUD");
			rateLine2B.TL_RateStartDate = new ZDate(2024, 1, 1);
			rateLine2B.TL_RateEndDate = new ZDate(2024, 1, 15);

			var rateEntry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, origin: "AU", destination: "NZAKL", commodity: "AABT", removeLines: true);
			var rateLine3A = rateEntry3.AddRateLine("CAF", "UNT", QuantityUnit.CN, "AUD");
			rateLine3A.TL_RateStartDate = new ZDate(2024, 1, 1);
			rateLine3A.TL_RateEndDate = new ZDate(2024, 1, 31);
			var rateLine3B = rateEntry3.AddRateLine("BAF", "UNT", QuantityUnit.CN, "AUD");
			rateLine3B.TL_RateStartDate = new ZDate(2024, 1, 15);
			rateLine3B.TL_RateEndDate = new ZDate(2024, 1, 31);

			Factory.Save();

			var filterCollection = new ModuleFilterCollection();
			var rateLineFilters = new RateLineModuleFilters(Factory, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.AIR, false);
			rateLineFilters.AddForRateEntryFilter(filterCollection);
			var collection = rate.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var filterBiz = new RateEntryFilterStripBusinessObject(collection);

			var commodityCodeFilter = (ModuleNkFilter)filterBiz[RateEntryFilterUtility.Constants.Codes.CommodityCode];
			var startDateFilter = (ModuleDateFilter)filterBiz[RateEntryFilterUtility.Constants.Codes.StartDate];
			var showAllRateLinesFilter = (ModuleFlagsFilter)filterBiz[RateEntryFilterUtility.Constants.Codes.ShowAllRateLines];
			var chargeCodeFilter = (ModuleGuidFilter)filterBiz[RateLineModuleFilters.Constants.Codes.ChargeCode];
			chargeCodeFilter.Property = frtChargeCode.PK;
			showAllRateLinesFilter.Property0 = true;
			startDateFilter.Property1 = new ZDate(2024, 1, 1);
			commodityCodeFilter.Property = "AABT";

			// Show All Rate Lines NOT Checked AND Charge Code = ‘FRT’
			chargeCodeFilter.IsActive = true;
			showAllRateLinesFilter.IsActive = false;
			startDateFilter.IsActive = false;
			commodityCodeFilter.IsActive = false;
			collection.LoadAndSortForGUI();
			// Expect rateLine1A and rateLine2A
			AssertCollectionContains(rateEntry1, collection);
			AssertCollectionContains(rateEntry2, collection);
			AssertCollectionNotContains(rateEntry3, collection);
			var actualLines1 = rateEntry1.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals(actualLines1.Count, 1);
			AssertCollectionContains(rateLine1A, actualLines1);
			var actualLines2 = rateEntry2.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals(actualLines2.Count, 1);
			AssertCollectionContains(rateLine2A, actualLines2);

			// Show All Rate Lines Checked AND Charge Code = ‘FRT’
			showAllRateLinesFilter.IsActive = true;
			collection.LoadAndSortForGUI();
			// Expect rateLine1A, rateLine2A and rateLine2B
			AssertCollectionContains(rateEntry1, collection);
			AssertCollectionContains(rateEntry2, collection);
			AssertCollectionNotContains(rateEntry3, collection);
			actualLines1 = rateEntry1.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals(actualLines1.Count, 1);
			AssertCollectionContains(rateLine1A, actualLines1);
			actualLines2 = rateEntry2.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals(actualLines2.Count, 2);
			AssertCollectionContains(rateLine2A, actualLines2);
			AssertCollectionContains(rateLine2B, actualLines2);

			// Show All Rate Lines NOT Checked AND Charge Code = ‘FRT’ AND Commodity = ‘AABT’
			showAllRateLinesFilter.IsActive = false;
			chargeCodeFilter.IsActive = true;
			commodityCodeFilter.IsActive = true;
			startDateFilter.IsActive = false;
			collection.LoadAndSortForGUI();
			// Expect rateLine2A
			AssertCollectionNotContains(rateEntry1, collection);
			AssertCollectionContains(rateEntry2, collection);
			AssertCollectionNotContains(rateEntry3, collection);
			actualLines2 = rateEntry2.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals(actualLines2.Count, 1);
			AssertCollectionContains(rateLine2A, actualLines2);

			// Show All Rate Lines Checked AND Start Date = ’01-Jan-24’ AND Commodity = ‘AABT’
			chargeCodeFilter.IsActive = false;
			showAllRateLinesFilter.IsActive = true;
			startDateFilter.IsActive = true;
			commodityCodeFilter.IsActive = true;
			collection.LoadAndSortForGUI();
			// Assert Expect rateLine2A, rateLine2B, rateLine3A, rateLine3B
			AssertCollectionNotContains(rateEntry1, collection);
			AssertCollectionContains(rateEntry2, collection);
			AssertCollectionContains(rateEntry3, collection);
			actualLines2 = rateEntry2.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals(actualLines2.Count, 2);
			AssertCollectionContains(rateLine2A, actualLines2);
			AssertCollectionContains(rateLine2B, actualLines2);
			var actualLines3 = rateEntry3.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals(actualLines3.Count, 2);
			AssertCollectionContains(rateLine3A, actualLines3);
			AssertCollectionContains(rateLine3B, actualLines3);
		}

		public void TestLayoutContext()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();

			var filterBizO = new RateEntryFilterStripBusinessObject(clientRate.FCLRateEntriesForBinding);
			AssertEquals("Filter should have correct Layout Context", string.Format("Rating_{0}", RatingConstants.RateCategory.FCL), ((IFilterStripBusinessObjectInternals)filterBizO).LayoutContext);
		}

		public void TestNewEntriesShouldNotBeMissingWhileRunningPreSaveValidation()
		{
			var localCosting = Factory.NewWithValidTestData<Costing>();
			localCosting.TH_OH = ZGuid.Empty;

			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var collection = localCosting.EntryCollections[category].LazyLoadingCollection;
				var filterStrip = new RateEntryFilterStripBusinessObject(collection);
				filterStrip.LoadModuleFilters();

				var entry = localCosting.AddRateEntry(category, "ALL", "AU", "US");
				entry.AddRateLine("FRT", FlatCalculator.Code);
			}

			localCosting.RunPreSaveValidation();

			CombineAssertions("Each category should have 1 entry", () =>
			{
				foreach (var category in RatingConstants.RateCategory.RateCategories)
				{
					var entriesCollection = localCosting.GetRateEntryCollectionForCategory(category);
					AssertEquals($"Category {category} ", 1, entriesCollection.Count);
				}
			});

			var provider = Factory.NewWithValidTestData<OrgHeader>();
			localCosting.TH_OH = provider.PK;
			localCosting.RunPreSaveValidation();

			CombineAssertions("Each category should have 1 entry", () =>
			{
				foreach (var category in RatingConstants.RateCategory.RateCategories)
				{
					var entriesCollection = localCosting.GetRateEntryCollectionForCategory(category);
					AssertEquals($"Category {category} ", 1, entriesCollection.Count);
				}
			});
		}

		class Asserter
		{
			public Asserter(RateEntryCollection collection, RateEntryFilterStripBusinessObject filterBizO)
			{
				this.collection = collection;
				this.filterBizO = filterBizO;
			}

			readonly RateEntryCollection collection;
			readonly RateEntryFilterStripBusinessObject filterBizO;

			public void AssertFiltering(string message, IEnumerable<RateEntry> expected)
			{
				collection.LoadAndSortForGUI();
				//ideally collection should have been reloaded automatically.
				//But, unfortunately, changing values on FilterSripBusinessObject doesn't trigger any event which I cood hook for to reload the collection.
				//In production the RateEntryStripControl control does trigger FilterChanged event though and reloads this collection. But I don't use this control in this test.

				AssertContainsExactElementsInAnyOrder(message + System.Environment.NewLine + filterBizO.Filter,
					BusinessObjectEqualityComparer<RateEntry>.InstanceComparer,
					GetEntryDescription,
					expected,
					collection.Cast<RateEntry>());
			}

			static string GetEntryDescription(RateEntry entry)
			{
				var sb = new ZStringBuilder();
				sb.Append(entry.TI_RateCategory);
				sb.Append(entry.TI_Mode);
				sb.Append(entry.TI_OriginLRC);
				sb.Append(entry.TI_ViaLRC);
				sb.Append(entry.TI_DestinationLRC);
				sb.Append(entry.TI_RateStartDate.ToShortDateString());
				sb.Append(entry.TI_RateEndDate.ToShortDateString());

				return "{" + sb.ToStringWithDelimiterBetweenAppends("}-{") + "}";
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var collection = clientRate.AIRRateEntriesForBinding;

			return new RateEntryFilterStripBusinessObject(collection);
		}
	}
}
