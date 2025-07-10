using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class RateEntryQueriesTest : RatingTestCase
	{
		public void TestCreateRateEntryQuery_MatchTI_OH_TransportProvider()
		{
			var helper = new TestHelper(Factory);

			var localClient = helper.NewOrgHeader();
			var carrier1 = helper.NewOrgHeader();
			var carrier2 = helper.NewOrgHeader();
			var carrier3 = helper.NewOrgHeader();

			var rate = helper.NewClientRate(localClient);

			// no carrier
			rate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "", "20GP");

			// with carrier 1..3
			rate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "", "20GP")
				.TI_OH_TransportProvider = carrier1.PK;
			rate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "", "20GP")
				.TI_OH_TransportProvider = carrier2.PK;
			rate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "", "20GP")
				.TI_OH_TransportProvider = carrier3.PK;

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP20, null);

			// Need to ensure that the SQL filter and the CW1-based filter provide the same result.
			var query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			var queriedEntries = Factory.Load<RateEntry>(query);
			AssertEquals("There should be 1 rate with empty carrier", ZGuid.Empty, queriedEntries.Single().TI_OH_TransportProvider);
			TestRateLoading(rate, criteria, queriedEntries);

			criteria.Carrier = carrier1;
			query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			queriedEntries = Factory.Load<RateEntry>(query);
			AssertEquals("There should be 2 rates", 2, queriedEntries.Length);
			AssertEquals("There should be 1 rate with empty carrier", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == ZGuid.Empty));
			AssertEquals("There should be 1 rate with carrier1", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == carrier1.PK));
			TestRateLoading(rate, criteria, queriedEntries);

			criteria.Carrier = carrier2;
			query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			queriedEntries = Factory.Load<RateEntry>(query);
			AssertEquals("There should be 2 rates", 2, queriedEntries.Length);
			AssertEquals("There should be 1 rate with empty carrier", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == ZGuid.Empty));
			AssertEquals("There should be 1 rate with carrier2", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == carrier2.PK));
			TestRateLoading(rate, criteria, queriedEntries);

			criteria.Carrier = carrier1;
			criteria.PossibleCarriers = new[] { carrier1, carrier2 };
			query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			queriedEntries = Factory.Load<RateEntry>(query);
			AssertEquals("rate count", 3, queriedEntries.Length);
			AssertEquals("There should be 1 rate with empty carrier", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == ZGuid.Empty));
			AssertEquals("There should be 1 rate with carrier1", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == carrier1.PK));
			AssertEquals("There should be 1 rate with carrier2", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == carrier2.PK));
			TestRateLoading(rate, criteria, queriedEntries);

			criteria.Carrier = null;
			criteria.PossibleCarriers = new[] { carrier2, carrier3 };
			query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			queriedEntries = Factory.Load<RateEntry>(query);
			AssertEquals("rate count", 3, queriedEntries.Length);
			AssertEquals("There should be 1 rate with empty carrier", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == ZGuid.Empty));
			AssertEquals("There should be 1 rate with carrier2", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == carrier2.PK));
			AssertEquals("There should be 1 rate with carrier3", 1, queriedEntries.Count(x => x.TI_OH_TransportProvider == carrier3.PK));
			TestRateLoading(rate, criteria, queriedEntries);

			// Checks that three ways of loading the rates are equivalent.
			// It cheats a bit because you'd never load client rate to costings, but, its the
			// rate loading that we're testing and not the rate header loading
			void TestRateLoading(ClientRate rate, TestRatingCriteria criteria, IEnumerable<RateEntry> rateEntries)
			{
				var costEntries = new CostRatesLoader(Factory, Logger).Load(criteria, [rate]);
				// revenue rates run with less SQL filters due to possible-matches functionality
				// so they need to be filtered by CW1 application filter.
				var revenueEntries = new RevenueRatesLoader(Factory, Logger).Load(criteria, [rate]);
				var filteredRevenueEntries = RateEntryFilter.Filter(criteria, false, revenueEntries, Factory, Logger);

				AssertEachIsSame(rateEntries, costEntries, filteredRevenueEntries);
			}
		}

		public void TestCreateRateEntryQuery_IsCrossTrade_PlannedLoadDischarge()
		{
			var helper = new TestHelper(Factory);
			var clientRate = helper.NewClientRate(NewClient);

			var crossTradeEntry = clientRate.AddRateEntry("ORG");
			crossTradeEntry.TI_IsCrossTrade = true;

			var crossTradeEntryWithNonMatchingPlannedLoad = clientRate.AddRateEntry("ORG");
			crossTradeEntryWithNonMatchingPlannedLoad.TI_IsCrossTrade = true;
			crossTradeEntryWithNonMatchingPlannedLoad.TI_PlannedLoadLRC = "IR";

			var crossTradeEntryWithMatchingPlannedLoad = clientRate.AddRateEntry("ORG");
			crossTradeEntryWithMatchingPlannedLoad.TI_IsCrossTrade = true;
			crossTradeEntryWithMatchingPlannedLoad.TI_PlannedLoadLRC = "CN";

			var crossTradeEntryWithNonMatchingPlannedDischarge = clientRate.AddRateEntry("ORG");
			crossTradeEntryWithNonMatchingPlannedDischarge.TI_IsCrossTrade = true;
			crossTradeEntryWithNonMatchingPlannedDischarge.TI_PlannedDischargeLRC = "MY";

			var crossTradeEntryWithMatchingPlannedDischarge = clientRate.AddRateEntry("ORG");
			crossTradeEntryWithMatchingPlannedDischarge.TI_IsCrossTrade = true;
			crossTradeEntryWithMatchingPlannedDischarge.TI_PlannedDischargeLRC = "NZAKL";

			var matchingPlannedLoadEntry = clientRate.AddRateEntry("ORG", "LCL", "USLAX", "GBLON");
			matchingPlannedLoadEntry.TI_PlannedLoadLRC = "CN";

			var matchingPlannedDischargeEntry = clientRate.AddRateEntry("ORG", "LCL", "US", "GB");
			matchingPlannedDischargeEntry.TI_PlannedDischargeLRC = "NZAKL";

			var nonMatchingPlannedLoadEntry = clientRate.AddRateEntry("ORG", "LCL", "USLAX", "GBLON");
			nonMatchingPlannedLoadEntry.TI_PlannedLoadLRC = "IR";

			var nonMatchingPlannedDischargeEntry = clientRate.AddRateEntry("ORG", "LCL", "US", "GB");
			nonMatchingPlannedDischargeEntry.TI_PlannedDischargeLRC = "MXCAN";

			Factory.Save();

			var criteria = new TestRatingCriteria("USLAX", "GBLON", 0, null, null);
			criteria.PlannedLoadForTest = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CNSHA");
			criteria.PlannedDischargeForTest = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			var query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			var queriedEntries = Factory.Load<RateEntry>(query);

			var message = "CrossTrade Condition is applied to Origin/Destination. PlannedLoad and PlannedDischarge should not be filtered by CrossTrade";
			var expectedRates = new[]
				{
					crossTradeEntry.PK,
					crossTradeEntryWithMatchingPlannedLoad.PK,
					crossTradeEntryWithMatchingPlannedDischarge.PK,
					matchingPlannedLoadEntry.PK,
					matchingPlannedDischargeEntry.PK
				};

			AssertContainsExactElementsInAnyOrder(message, expectedRates, queriedEntries.Select(x => x.PK));
		}

		public void TestCreateRateEntryQuery_IsRevenue_ShouldLoadRateEntriesMatchingRateOriginDestination()
		{
			var helper = new TestHelper(Factory);

			var costing = helper.NewCosting(null);
			var entryA = costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 50m);
			var entryB = costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUMEL", "CNSHA", "FRT", 50m);

			Factory.Save();

			var criteria = new TestRatingCriteria("AUMEL", "CNSHA", 0, null, null);
			criteria.RateOrigin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			criteria.RateDestination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var query = RateEntryQueries.GetQuery(criteria, isCosting: true, isInterCompanyTariff: false);
			var rateEntries = Factory.Load<RateEntry>(query);

			AssertEquals("Only the entry matching Criteria Origin/Destination should be loaded", 1, rateEntries.Length);
			AssertEquals(entryB.PK, rateEntries.Single().PK);

			query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			rateEntries = Factory.Load<RateEntry>(query);

			AssertEquals("Both entry should be loaded", 2, rateEntries.Length);
			AssertContainsExactElementsInAnyOrder(new[] { entryA.PK, entryB.PK }, rateEntries.Select(x => x.PK));
		}

		#region Contract number

		public void TestCreateContractNumberQuery()
		{
			var criteria = new TestRatingCriteria();
			criteria.SetClientContractNumbers(new ZString("CLC-Test"));
			var expectedQuery = "TI_ContractNumber in ('', 'CLC-Test')";

			criteria.ShouldReturnContractNumberConfiguration = true;
			criteria.ShouldAddContractNumberQueryFilter = false;

			var contractNumberQuery = RateEntryQueries.GetQuery(criteria, isCosting: false, whichQueries: RateEntryQueries.Kind.ContractNumber);
			AssertEquals(true, contractNumberQuery.IsEmpty);

			criteria.ShouldAddContractNumberQueryFilter = true;
			contractNumberQuery = RateEntryQueries.GetQuery(criteria, isCosting: false, whichQueries: RateEntryQueries.Kind.ContractNumber);
			AssertContains(expectedQuery, contractNumberQuery.FilterPartsHashKey, false);

			criteria.ShouldReturnContractNumberConfiguration = false;
			contractNumberQuery = RateEntryQueries.GetQuery(criteria, isCosting: false, whichQueries: RateEntryQueries.Kind.ContractNumber);
			AssertContains(expectedQuery, contractNumberQuery.FilterPartsHashKey, false);
		}

		public void TestCreateContractNumberQuery_ManualCostSelect_ShouldAlwaysAdd()
		{
			var criteria = new TestRatingCriteria();
			var expectedQuery = "TI_ContractNumber in ('', 'NUM1', 'NUM2')";

			criteria.IsManualCostSelectMode = true;
			criteria.ShouldReturnContractNumberConfiguration = true;
			criteria.SetCarrierContractNumber("NUM1", "NUM2");

			criteria.ShouldAddContractNumberQueryFilter = false;
			var contractNumberQuery = RateEntryQueries.GetQuery(criteria, isCosting: true, whichQueries: RateEntryQueries.Kind.ContractNumber);
			AssertContains(expectedQuery, contractNumberQuery.FilterPartsHashKey);

			criteria.ShouldAddContractNumberQueryFilter = true;
			contractNumberQuery = RateEntryQueries.GetQuery(criteria, isCosting: true, whichQueries: RateEntryQueries.Kind.ContractNumber);
			AssertContains(expectedQuery, contractNumberQuery.FilterPartsHashKey);
		}

		public void TestCreateContractNumberQuery_IsNotCalledForCostsWhenNoContractNumbers()
		{
			var criteria = new TestRatingCriteria();
			criteria.ShouldReturnContractNumberConfiguration = true;
			criteria.ShouldAddContractNumberQueryFilter = true;

			var contractNumberQuery = RateEntryQueries.GetQuery(criteria, isCosting: true, whichQueries: RateEntryQueries.Kind.ContractNumber);
			AssertEquals(true, contractNumberQuery.IsEmpty);
		}

		#endregion

		#region Service Level queries

		public void TestCreateClientServiceLevelQuery_ServiceLevelInJob()
		{
			var criteria = new TestRatingCriteria();
			criteria.ServiceLevel = new ServiceLevelRatingInformation(
				new ServiceLevelInfo("AAA", ServiceLevelType.Client),
				new ServiceLevelInfo("BBB", ServiceLevelType.Carrier)
			);

			var query = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.ClientServiceLevel);
			var expectedQuery = @"TI_RS_NKServiceLevel_NI = '' or TI_RS_NKServiceLevel_NI = 'AAA'";
			AssertContains(expectedQuery, query.FilterPartsHashKey);
		}

		public void TestCreateClientServiceLevelQuery_ServiceLevelNotInJob()
		{
			var criteria = new TestRatingCriteria();
			criteria.ServiceLevel = new ServiceLevelRatingInformation(
				new ServiceLevelInfo("BBB", ServiceLevelType.Carrier)
			);

			var query = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.ClientServiceLevel);
			var expectedQuery = "TI_RS_NKServiceLevel_NI = '' or TI_RS_NKServiceLevel_NI = 'STD'";
			AssertContains(expectedQuery, query.FilterPartsHashKey);
		}

		public void TestCreateCarrierServiceLevelQuery_CarrierServiceLevelInJob()
		{
			var criteria = new TestRatingCriteria();
			criteria.ServiceLevel = new ServiceLevelRatingInformation(
				new ServiceLevelInfo("AAA", ServiceLevelType.Client),
				new ServiceLevelInfo("BBB", ServiceLevelType.Carrier)
			);

			var query = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.CarrierServiceLevel);
			var expectedQuery = "TI_PL_NKCarrierServiceLevel = '' or TI_PL_NKCarrierServiceLevel = 'BBB'";
			AssertContains(expectedQuery, query.FilterPartsHashKey);

			using (criteria.TemporarilyAllowCriteriaChanging())
			{
				Logger = new TestLogger();
				criteria.CarrierServiceLevelOverride = (new ZString[] { "CCC", "DDD" }).ToList();
				query = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.CarrierServiceLevel);
				expectedQuery = "TI_PL_NKCarrierServiceLevel = '' or (TI_PL_NKCarrierServiceLevel in ('CCC', 'DDD'))";
				AssertContains(expectedQuery, query.FilterPartsHashKey);
			}
		}

		public void TestCreateCarrierServiceLevelQuery_CarrierServiceLevelNotInJob()
		{
			var criteria = new TestRatingCriteria();
			criteria.ServiceLevel = new ServiceLevelRatingInformation(
				new ServiceLevelInfo("AAA", ServiceLevelType.Client)
			);

			var query = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.CarrierServiceLevel);
			var expectedQuery = "TI_PL_NKCarrierServiceLevel = '' or TI_PL_NKCarrierServiceLevel = 'STD'";
			AssertContains(expectedQuery, query.FilterPartsHashKey);
		}

		#endregion Service level queries

		#region ApplyBBK_BLK_RORRateModeFilter

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryDisable_BBK()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: false, FreightMode.BBK, expectedQuery: "(\r\n\tTI_Mode not in \r\n\t(\r\n\t\t'BLK', 'BBK', 'ROR'\r\n\t)\r\n)\r\n");

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryDisable_BLK()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: false, FreightMode.BLK, expectedQuery: "(\r\n\tTI_Mode not in \r\n\t(\r\n\t\t'BLK', 'BBK', 'ROR'\r\n\t)\r\n)\r\n");

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryDisable_ROR()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: false, FreightMode.ROR, expectedQuery: "(\r\n\tTI_Mode not in \r\n\t(\r\n\t\t'BLK', 'BBK', 'ROR'\r\n\t)\r\n)\r\n");

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryDisable_AIR()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: false, FreightMode.AIR, string.Empty);

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryEnable_BBK()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: true, FreightMode.BBK, expectedQuery: "TI_Mode = 'BBK'\r\n");

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryEnable_BLK()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: true, FreightMode.BLK, expectedQuery: "TI_Mode = 'BLK'\r\n");

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryEnable_ROR()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: true, FreightMode.ROR, expectedQuery: "TI_Mode = 'ROR'\r\n");

		public void TestApplyBBK_BLK_RORRateModeFilter_RegistryEnable_AIR()
			=> AssertApplyBBK_BLK_RORRateModeFilter(isRegistryEnabled: false, FreightMode.AIR, string.Empty);

		void AssertApplyBBK_BLK_RORRateModeFilter(bool isRegistryEnabled, FreightMode freightMode, string expectedQuery)
		{
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled))
			{
				var criteria = new TestRatingCriteria() { FreightMode = freightMode };
				var query = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.RateModesSpecialFreightModes);
				AssertEquals($"GIVEN isRegistryEnabled={isRegistryEnabled}, FreightMode={freightMode}", expectedQuery, query.LiteralTextSqlFormatted);
			}
		}

		#endregion

		void AssertEachIsSame(params IEnumerable<IRateEntry>[] entries)
		{
			var firstPks = entries.First().Select(x => x.PK).Distinct().ToList();

			foreach (var other in entries.Skip(1))
			{
				var otherPks = other.Select(x => x.PK).Distinct();
				AssertSequencesEqual(otherPks, firstPks);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Logger = new TestLogger();
			RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		TestLogger Logger { get; set; }
	}
}
