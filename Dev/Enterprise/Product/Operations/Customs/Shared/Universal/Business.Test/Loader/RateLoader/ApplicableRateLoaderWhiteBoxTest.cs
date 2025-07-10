using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class ApplicableRateLoaderWhiteBoxTest : BaseApplicableRateLoaderAbstractTest
	{
		public void TestLoadRatesForMultipleCriteriaSets_UsesDistinctCriteriaSets()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType = helper.CreateCusRateType(EunGroupCode, "RT1");
			CreateRate(tariff, rateType, "RC111");
			CreateRate(tariff, rateType, "RC112");
			Factory.Save();
			var selectionCriteriaRateType1 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var selectionCriteriaRateCode112 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "RC112");
			var tariffCriteriaSetRateType1_1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateType1_2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateCode112_1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateCode112);
			var tariffCriteriaSetRateCode112_2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateCode112);

			var rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory);
			CombineAssertions("[PRE-CONDITION] Before Loading", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSetRateType1_1, false);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSetRateType1_2, false);
				AssertCriteriaSetCached("RateCode112 (Set1)", tariffCriteriaSetRateCode112_1, false);
				AssertCriteriaSetCached("RateCode112 (Set2)", tariffCriteriaSetRateCode112_2, false);
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
			});

			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetRateType1_1, tariffCriteriaSetRateType1_2, tariffCriteriaSetRateCode112_1, tariffCriteriaSetRateCode112_2, });
			CombineAssertions("[1st Load Results]", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSetRateType1_1, true);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSetRateType1_2, true);
				AssertCriteriaSetCached("RateCode112 (Set1)", tariffCriteriaSetRateCode112_1, true);
				AssertCriteriaSetCached("RateCode112 (Set2)", tariffCriteriaSetRateCode112_2, true);
				AssertEquals("MockDbHitCount", 1, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize (Only Distinct Criteria Sets)", 2, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});

			rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory);
			loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetRateType1_1, tariffCriteriaSetRateType1_2, tariffCriteriaSetRateCode112_1, tariffCriteriaSetRateCode112_2, });
			CombineAssertions("[Results From Loading Cached Rates]", () =>
			{
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});

			loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSetRateType1_1);
			CombineAssertions("Criteria [RateType: RT1 (Set1)]", () =>
			{
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});

			loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSetRateType1_2);
			CombineAssertions("Criteria [RateType: RT1 (Set2)]", () =>
			{
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});

			loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSetRateCode112_1);
			CombineAssertions("Criteria [RateCode: RC112 (Set1)]", () =>
			{
				AssertEquals("Loaded rate count", 1, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 0);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});

			loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSetRateCode112_2);
			CombineAssertions("Criteria [RateCode: RC112 (Set2)]", () =>
			{
				AssertEquals("Loaded rate count", 1, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 0);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});
		}

		public void TestLoadRatesForMultipleCriteriaSets_SplitsLoadInBatches()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType = helper.CreateCusRateType(EunGroupCode, "RT1");
			CreateRate(tariff, rateType, "RC111");
			CreateRate(tariff, rateType, "RC112");
			Factory.Save();
			var selectionCriteriaRateType1 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var selectionCriteriaRateCode112 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "RC112");
			var selectionCriteriaAnyRate = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "");
			var tariffCriteriaSetRateType1_1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateType1_2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateType1_3 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateCode112_1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateCode112);
			var tariffCriteriaSetRateCode112_2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateCode112);
			var tariffCriteriaSetAnyRate = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaAnyRate);

			var rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory, 2);
			CombineAssertions("[PRE-CONDITION] Before Loading", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSetRateType1_1, false);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSetRateType1_2, false);
				AssertCriteriaSetCached("RateType1 (Set3)", tariffCriteriaSetRateType1_3, false);
				AssertCriteriaSetCached("RateCode112 (Set1)", tariffCriteriaSetRateCode112_1, false);
				AssertCriteriaSetCached("RateCode112 (Set2)", tariffCriteriaSetRateCode112_2, false);
				AssertCriteriaSetCached("Any Rate", tariffCriteriaSetAnyRate, false);
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
			});

			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetRateType1_1, tariffCriteriaSetRateType1_2, tariffCriteriaSetRateType1_3, tariffCriteriaSetRateCode112_1, tariffCriteriaSetRateCode112_2, tariffCriteriaSetAnyRate, });
			CombineAssertions("[1st Load Results]", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSetRateType1_1, true);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSetRateType1_2, true);
				AssertCriteriaSetCached("RateType1 (Set3)", tariffCriteriaSetRateType1_3, true);
				AssertCriteriaSetCached("RateCode112 (Set1)", tariffCriteriaSetRateCode112_1, true);
				AssertCriteriaSetCached("RateCode112 (Set2)", tariffCriteriaSetRateCode112_2, true);
				AssertCriteriaSetCached("Any Rate", tariffCriteriaSetAnyRate, true);
				AssertEquals("MockDbHitCount (3 distinct sets in 2 batches)", 2, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 1, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});

			rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory, 2);
			loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetRateType1_1, tariffCriteriaSetRateType1_2, tariffCriteriaSetRateType1_3, tariffCriteriaSetRateCode112_1, tariffCriteriaSetRateCode112_2, tariffCriteriaSetAnyRate, });
			CombineAssertions("[Results From Loading Cached Rates]", () =>
			{
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});
		}

		public void TestCacheRatesForMultipleCriteriaSets_UsesDistinctCriteriaSets()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType = helper.CreateCusRateType(EunGroupCode, "RT1");
			CreateRate(tariff, rateType, "RC111");
			Factory.Save();
			var selectionCriteria = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var tariffCriteriaSet1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteria);
			var tariffCriteriaSet2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteria);

			var rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory);
			CombineAssertions("[PRE-CONDITION] Before Caching", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSet1, false);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSet1, false);
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
			});

			rateLoader.CacheRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSet1, tariffCriteriaSet2, });
			CombineAssertions("[Caching outcome]", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSet1, true);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSet1, true);
				AssertEquals("MockDbHitCount", 1, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize (Only Distinct Criteria Sets)", 1, rateLoader.LastDbLoadBatchSize);
			});

			rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory);
			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSet1, tariffCriteriaSet2, });
			CombineAssertions("[Results From Loading Cached Rates]", () =>
			{
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 1, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
			});

			loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet1);
			CombineAssertions("Criteria [RateType: RT1 (Set1)]", () =>
			{
				AssertEquals("Loaded rate count", 1, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
			});

			loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet2);
			CombineAssertions("Criteria [RateType: RT1 (Set2)]", () =>
			{
				AssertEquals("Loaded rate count", 1, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
			});
		}

		public void TestCacheRatesForMultipleCriteriaSets_SplitsLoadInBatches()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType1 = helper.CreateCusRateType(EunGroupCode, "RT1");
			var rateType2 = helper.CreateCusRateType(EunGroupCode, "RT2");
			CreateRate(tariff, rateType1, "RC111");
			CreateRate(tariff, rateType1, "RC112");
			CreateRate(tariff, rateType2, "RC121");
			Factory.Save();

			var selectionCriteriaRateType1 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var selectionCriteriaRateCode112 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "RC112");
			var selectionCriteriaAnyRate = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "");
			var tariffCriteriaSetRateType1_1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateType1_2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateType1_3 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateType1);
			var tariffCriteriaSetRateCode112_1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateCode112);
			var tariffCriteriaSetRateCode112_2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaRateCode112);
			var tariffCriteriaSetAnyRate = new RateLoadTariffCriteriaSet(tariff, selectionCriteriaAnyRate);

			var rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory, 1);
			CombineAssertions("[PRE-CONDITION] Before Caching", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSetRateType1_1, false);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSetRateType1_2, false);
				AssertCriteriaSetCached("RateType1 (Set3)", tariffCriteriaSetRateType1_3, false);
				AssertCriteriaSetCached("RateCode112 (Set1)", tariffCriteriaSetRateCode112_1, false);
				AssertCriteriaSetCached("RateCode112 (Set2)", tariffCriteriaSetRateCode112_2, false);
				AssertCriteriaSetCached("Any Rate", tariffCriteriaSetAnyRate, false);
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
			});

			rateLoader.CacheRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetRateType1_1, tariffCriteriaSetRateType1_2, tariffCriteriaSetRateType1_3, tariffCriteriaSetRateCode112_1, tariffCriteriaSetRateCode112_2, tariffCriteriaSetAnyRate, });
			CombineAssertions("[1st Load Results]", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSetRateType1_1, true);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSetRateType1_2, true);
				AssertCriteriaSetCached("RateType1 (Set3)", tariffCriteriaSetRateType1_3, true);
				AssertCriteriaSetCached("RateCode112 (Set1)", tariffCriteriaSetRateCode112_1, true);
				AssertCriteriaSetCached("RateCode112 (Set2)", tariffCriteriaSetRateCode112_2, true);
				AssertCriteriaSetCached("Any Rate", tariffCriteriaSetAnyRate, true);
				AssertEquals("MockDbHitCount (3 distinct sets in 3 batches)", 3, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 1, rateLoader.LastDbLoadBatchSize);
			});

			rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory, 1);
			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetRateType1_1, tariffCriteriaSetRateType1_2, tariffCriteriaSetRateType1_3, tariffCriteriaSetRateCode112_1, tariffCriteriaSetRateCode112_2, tariffCriteriaSetAnyRate, });
			CombineAssertions("[Results From Loading Cached Rates]", () =>
			{
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 3, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
				AssertLoadedRateCount(loadedRates, "RC121", 1);
			});
		}

		public void TestLoadRatesForSingleCriteriaSet_DatabaseLoadCount()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType = helper.CreateCusRateType(EunGroupCode, "RT1");
			CreateRate(tariff, rateType, "RC111");
			CreateRate(tariff, rateType, "RC112");
			Factory.Save();

			var selectionCriteria = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var tariffCriteriaSet1 = new RateLoadTariffCriteriaSet(tariff, selectionCriteria);
			var tariffCriteriaSet2 = new RateLoadTariffCriteriaSet(tariff, selectionCriteria);

			var rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory);
			CombineAssertions("[PRE-CONDITION] Before Loading", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSet1, false);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSet1, false);
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
			});

			var loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet1);
			CombineAssertions("[Results from Loading Criteria Set 1]", () =>
			{
				AssertCriteriaSetCached("RateType1 (Set1)", tariffCriteriaSet1, true);
				AssertCriteriaSetCached("RateType1 (Set2)", tariffCriteriaSet1, true);
				AssertEquals("MockDbHitCount", 1, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 1, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});

			rateLoader = new ApplicableRateLoaderForCriteriaSetBatchTesting(Factory);
			loadedRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet2);
			CombineAssertions("[Results from Loading Criteria Set 1, after an equal Set2 had already been loaded]", () =>
			{
				AssertEquals("MockDbHitCount", 0, rateLoader.MockDbHitCount);
				AssertEquals("LastDbLoadBatchSize", 0, rateLoader.LastDbLoadBatchSize);
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
			});
		}

		class ApplicableRateLoaderForCriteriaSetBatchTesting : ApplicableRateLoader
		{
			public ApplicableRateLoaderForCriteriaSetBatchTesting(BusinessObjectFactory factory, int? batchSizeOverride = null) : base(factory)
			{
				BatchSizeOverride = batchSizeOverride;
			}

			public int? BatchSizeOverride
			{
				get;
			}

			public int MockDbHitCount
			{
				get;
				private set;
			}

			public int LastDbLoadBatchSize
			{
				get;
				private set;
			}

			protected override IEnumerable<RateView> LoadRatesForBatchOfCriteriaSets(IEnumerable<RateLoadTariffCriteriaSet> criteriaSetBatch)
			{
				MockDbHitCount++;
				LastDbLoadBatchSize = criteriaSetBatch.Count();
				return base.LoadRatesForBatchOfCriteriaSets(criteriaSetBatch);
			}

			protected override int CriteriaSetBatchSize => BatchSizeOverride ?? base.CriteriaSetBatchSize;
		}
	}
}
