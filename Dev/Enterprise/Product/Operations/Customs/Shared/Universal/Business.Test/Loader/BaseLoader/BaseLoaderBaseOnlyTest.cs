using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(BaseLoaderTestHelper.BaseLoaderForTest))]
	class BaseLoaderBaseOnlyTest : BaseLoaderAbstractTest<BaseLoaderTestHelper.DummyBizoForTest>
	{
		public void TestGetEmptyAdditionalCodesCriteriaTable()
		{
			var tvpDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptyAdditionalCodesParameterTable();

			CombineAssertions(() =>
			{
				AssertNotNull("Created TVP_AdditionalCodes Data Table", tvpDataTable);
				AssertEquals("TVP Row Count", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(new[] { TvpAdditionalCodes.Columns.Id, TvpAdditionalCodes.Columns.CriteriaId, TvpAdditionalCodes.Columns.AdditionalCode }, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		public void TestGetEmptySecondTradeGroupsParameterTable()
		{
			var tvpDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptySecondTradeGroupsParameterTable();

			CombineAssertions(() =>
			{
				AssertNotNull("Created TVP_SecondTradeGroup Data Table", tvpDataTable);
				AssertEquals("TVP Row Count", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(new[] { TvpSecondTradeGroup.Columns.Id, TvpSecondTradeGroup.Columns.CriteriaId, TvpSecondTradeGroup.Columns.SecondTradeGroup }, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		public void TestLoadDataForMultipleCriteriaSets_UsesDistinctCriteriaSets()
		{
			PrepareTestData();
			var tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet1_1, tariffCriteriaSet1_2, tariffCriteriaSet2_1, tariffCriteriaSet2_2 };
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory);
			var dictionary = loader.GetDictionaryForTesting();
			CombineAssertions(() =>
			{
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P2  (Set1)", tariffCriteriaSet2_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P2  (Set2)", tariffCriteriaSet2_2, dictionary, false);
				AssertEquals("[PRE-CONDITION] Before Loading: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[PRE-CONDITION] Before Loading: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);

				var loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P2  (Set1)", tariffCriteriaSet2_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P2  (Set2)", tariffCriteriaSet2_2, dictionary, true);
				AssertEquals("[1st Load Results] After Loading: MockDbHitCount", 1, loader.MockDbHitCount);
				AssertEquals("[1st Load Results] After Loading: LastDbLoadBatchSize: Only Distinct Criteria Sets", 2, loader.LastDbLoadBatchSize);
				AssertEquals("[1st Load Results] After Loading: Loaded count", 2, loadedData.Count());
				AssertLoadedBizosCount("[1st Load Results] bizo1", loadedData, bizoPk1, 1);
				AssertLoadedBizosCount("[1st Load Results] bizo2", loadedData, bizoPk2, 1);

				loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory);
				loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("[2nd Load Results] From Cache: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[2nd Load Results] From Cache: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);
				AssertEquals("[2nd Load Results] From Cache: Loaded count", 2, loadedData.Count());
				AssertLoadedBizosCount("[2nd Load Results] bizo1", loadedData, bizoPk1, 1);
				AssertLoadedBizosCount("[2nd Load Results] bizo2", loadedData, bizoPk2, 1);

				tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet1_1, tariffCriteriaSet1_2 };
				loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("[3rd Load Results] From Cache: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[3rd Load Results] From Cache: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);
				AssertEquals("[3rd Load Results] From Cache: Loaded count", 1, loadedData.Count());
				AssertLoadedBizosCount("[3rd Load Results] bizo1", loadedData, bizoPk1, 1);
				AssertLoadedBizosCount("[3rd Load Results] bizo2", loadedData, bizoPk2, 0);
			});
		}

		public void TestLoadDataForMultipleCriteriaSets_SplitsLoadInMultipleBatches()
		{
			PrepareTestData();
			var tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest>();

			for (var i = 0; i < 21; i++)
			{
				var selectionCriteria3 = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPk2, "ZA", "P" + i, new HashSet<ZString>(), "", CurrentCountry);
				var tariffCriteriaSet3 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria3);
				tariffCriteriaSetsList.Add(tariffCriteriaSet3);
			}

			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 5);
			var dictionary = loader.GetDictionaryForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] Before Loading: tariffCriteriaSet count", 21, tariffCriteriaSetsList.Count);
				AssertEquals("[PRE-CONDITION] Before Loading: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[PRE-CONDITION] Before Loading: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);

				var loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("[1st Load Results] After Loading: MockDbHitCount", 5, loader.MockDbHitCount);
				AssertEquals("[1st Load Results] After Loading: LastDbLoadBatchSize: Only one in a batch", 1, loader.LastDbLoadBatchSize);

				loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 5);
				loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("[2nd Load Results] From Cache: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[2nd Load Results] From Cache: LastDbLoadBatchSize: Only one in a batch", 0, loader.LastDbLoadBatchSize);
			});
		}

		public void TestLoadDataForMultipleCriteriaSets_SplitsLoadInBatches()
		{
			PrepareTestData();
			var selectionCriteria3 = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPk2, "ZA", "P3", new HashSet<ZString>(), "", CurrentCountry);
			var tariffCriteriaSet3 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria3);
			var tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet1_1, tariffCriteriaSet1_2, tariffCriteriaSet2_1, tariffCriteriaSet3 };
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 1);
			var dictionary = loader.GetDictionaryForTesting();
			CombineAssertions(() =>
			{
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P2", tariffCriteriaSet2_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P3", tariffCriteriaSet3, dictionary, false);
				AssertEquals("[PRE-CONDITION] Before Loading: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[PRE-CONDITION] Before Loading: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);

				var loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P2", tariffCriteriaSet2_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P3", tariffCriteriaSet3, dictionary, true);
				AssertEquals("[1st Load Results] After Loading: MockDbHitCount", 3, loader.MockDbHitCount);
				AssertEquals("[1st Load Results] After Loading: LastDbLoadBatchSize: Only one in a batch", 1, loader.LastDbLoadBatchSize);
				AssertEquals("[1st Load Results] After Loading: Loaded count", 2, loadedData.Count());
				AssertLoadedBizosCount("bizo1", loadedData, bizoPk1, 1);   // match to selectionCriteria1
				AssertLoadedBizosCount("bizo2", loadedData, bizoPk2, 1);  // match to selectionCriteria2 & selectionCriteria3

				loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 1);
				loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("[2nd Load Results] From Cache: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[2nd Load Results] From Cache: LastDbLoadBatchSize: Only one in a batch", 0, loader.LastDbLoadBatchSize);
				AssertEquals("[2nd Load Results] From Cache: Loaded count", 2, loadedData.Count());
				AssertLoadedBizosCount("bizo1", loadedData, bizoPk1, 1);   // match to selectionCriteria1
				AssertLoadedBizosCount("bizo2", loadedData, bizoPk2, 1);  // match to selectionCriteria2 & selectionCriteria3
			});
		}

		public void TestCacheDataForMultipleCriteriaSets_UsesDistinctCriteriaSets()
		{
			PrepareTestData();
			var tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet1_1, tariffCriteriaSet1_2, tariffCriteriaSet2_1, tariffCriteriaSet2_2 };
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory);
			var dictionary = loader.GetDictionaryForTesting();
			CombineAssertions(() =>
			{
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P2  (Set1)", tariffCriteriaSet2_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P2  (Set2)", tariffCriteriaSet2_2, dictionary, false);
				AssertEquals("[PRE-CONDITION] Before Loading: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[PRE-CONDITION] Before Loading: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);

				loader.CacheDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P2  (Set1)", tariffCriteriaSet2_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P2  (Set2)", tariffCriteriaSet2_2, dictionary, true);
				AssertEquals("[1st Load Results] After Loading: MockDbHitCount", 1, loader.MockDbHitCount);
				AssertEquals("[1st Load Results] After Loading: LastDbLoadBatchSize: Only Distinct Criteria Sets", 2, loader.LastDbLoadBatchSize);

				loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory);
				var loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("[2nd Load Results] From Cache: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[2nd Load Results] From Cache: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);
				AssertEquals("[2nd Load Results] From Cache: Loaded count", 2, loadedData.Count());
				AssertLoadedBizosCount("bizoPk1", loadedData, bizoPk1, 1);
				AssertLoadedBizosCount("bizoPk2", loadedData, bizoPk2, 1);
			});
		}

		public void TestCacheDataForMultipleCriteriaSets_SplitsLoadInBatches()
		{
			PrepareTestData();
			var selectionCriteria3 = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPk2, "ZA", "P3", new HashSet<ZString>(), "", CurrentCountry);
			var tariffCriteriaSet3 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria3);
			var tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet1_1, tariffCriteriaSet1_2, tariffCriteriaSet2_1, tariffCriteriaSet3 };
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 1);
			var dictionary = loader.GetDictionaryForTesting();
			CombineAssertions(() =>
			{
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P2", tariffCriteriaSet2_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Before Loading: Preference-P3", tariffCriteriaSet3, dictionary, false);
				AssertEquals("[PRE-CONDITION] Before Loading: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[PRE-CONDITION] Before Loading: LastDbLoadBatchSize", 0, loader.LastDbLoadBatchSize);

				loader.CacheDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1 (Set1)", tariffCriteriaSet1_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P1  (Set2)", tariffCriteriaSet1_2, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P2", tariffCriteriaSet2_1, dictionary, true);
				AssertCriteriaSetCached("[1st Load Results] After Loading: Preference-P3", tariffCriteriaSet3, dictionary, true);
				AssertEquals("[1st Load Results] After Loading: MockDbHitCount", 3, loader.MockDbHitCount);
				AssertEquals("[1st Load Results] After Loading: LastDbLoadBatchSize: Only one in a batch", 1, loader.LastDbLoadBatchSize);

				loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 1);
				var loadedData = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("[2nd Load Results] From Cache: MockDbHitCount", 0, loader.MockDbHitCount);
				AssertEquals("[2nd Load Results] From Cache: LastDbLoadBatchSize: Only one in a batch", 0, loader.LastDbLoadBatchSize);
				AssertEquals("[2nd Load Results] From Cache: Loaded count", 2, loadedData.Count());
				AssertLoadedBizosCount("bizo1", loadedData, bizoPk1, 1);   // match to selectionCriteria1
				AssertLoadedBizosCount("bizo2", loadedData, bizoPk2, 1);  // match to selectionCriteria2 & selectionCriteria3
			});
		}

		public void TestLoadDataForSingleCriteriaSet()
		{
			PrepareTestData();
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 1);

			CombineAssertions(() =>
			{
				var selectedBizos = loader.LoadDataForSingleCriteriaSet(tariffCriteriaSet1_1);
				AssertCriteriaSetCached("tariffCriteriaSet cached", tariffCriteriaSet1_1, loader.GetDictionaryForTesting(), true);
				AssertEquals("Loaded selectedBizos count", 1, selectedBizos.Count());
				AssertLoadedBizosCount("bizoPk1", selectedBizos, bizoPk1, 1);
			});
		}

		public void TestCacheDataForMultipleCriteriaSets()
		{
			PrepareTestData();
			var selectionCriteria3 = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPk2, "ZA", "P3", new HashSet<ZString>(), "", CurrentCountry);
			var tariffCriteriaSet3 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria3);

			var tariffCriteriaSetsList =
				new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet1_1, tariffCriteriaSet2_1 };
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 1);
			var dictionary = loader.GetDictionaryForTesting();
			CombineAssertions(() =>
			{
				AssertCriteriaSetCached("[PRE-CONDITION] Is tariffCriteriaSet1_1 cached before 1st loading?", tariffCriteriaSet1_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Is tariffCriteriaSet2_1 cached before 1st loading?", tariffCriteriaSet2_1, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Is tariffCriteriaSet2_2 cached before 1st loading?", tariffCriteriaSet2_2, dictionary, false);
				AssertCriteriaSetCached("[PRE-CONDITION] Is tariffCriteriaSet3 cached before 1st loading?", tariffCriteriaSet3, dictionary, false);

				loader.CacheDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("tariffCriteriaSet1_1 cached", tariffCriteriaSet1_1, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet2_1 cached", tariffCriteriaSet2_1, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet2_2 cached as tariffCriteriaSet2_2 has the same selectionCriteria2 as tariffCriteriaSet2_1", tariffCriteriaSet2_2, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet3 not cached yet", tariffCriteriaSet3, dictionary, false);

				tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet3 };
				loader.CacheDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("tariffCriteriaSet1_1 already cached", tariffCriteriaSet1_1, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet2_1 already cached", tariffCriteriaSet2_1, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet2_2 already cached: ", tariffCriteriaSet2_2, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet3 already cached", tariffCriteriaSet3, dictionary, true);
			});
		}

		public void TestLoadDataForMultipleCriteriaSets_LoadOnlyOnceIfMatchMultiCriteriaSet()
		{
			PrepareTestData();
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory, 1);
			var selectionCriteria3 = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPk2, "ZA", "P2",
				new HashSet<ZString>(), "", CurrentCountry);
			var tariffCriteriaSet3 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria3);

			var tariffCriteriaSetsList = new List<BaseLoaderTestHelper.TariffCriteriaSetForTest> { tariffCriteriaSet1_1, tariffCriteriaSet2_1, tariffCriteriaSet3 };
			CombineAssertions(() =>
			{
				var bizos = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("Loaded count: loaded", 2, bizos.Count());
				AssertLoadedBizosCount("bizoPk1", bizos, bizoPk1, 1);  // Match selectionCriteria1
				AssertLoadedBizosCount("bizoPk2", bizos, bizoPk2, 1);  // Match selectionCriteria2 & selectionCriteria2 but load once

				bizos = loader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("Loaded Condition count: Cached", 2, bizos.Count());
				AssertLoadedBizosCount("bizoPk1", bizos, bizoPk1, 1);  // Match selectionCriteria1
				AssertLoadedBizosCount("bizoPk2", bizos, bizoPk2, 1);  // Match selectionCriteria2 & selectionCriteria2 but load once
			});
		}

		void PrepareTestData()
		{
			tariff = Factory.New<TariffView>();
			bizoPk1 = Factory.New<BaseLoaderTestHelper.DummyBizoForTest>().PK;
			bizoPk2 = Factory.New<BaseLoaderTestHelper.DummyBizoForTest>().PK;
			var selectionCriteria1 = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPk1, "DE", "P1", new HashSet<ZString>(), "", CurrentCountry);
			tariffCriteriaSet1_1 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria1);
			tariffCriteriaSet1_2 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria1);
			var selectionCriteria2 = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPk2, "ZA", "P2", new HashSet<ZString>(), "", CurrentCountry);
			tariffCriteriaSet2_1 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria2);
			tariffCriteriaSet2_2 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff, selectionCriteria2);
		}

		protected override string[] ExpectedCriteriaTableColumns => new[]
		{
			TvpSelectionCriteria.Columns.CriteriaId, TvpSelectionCriteria.Columns.TariffPK, TvpSelectionCriteria.Columns.EffectiveDate, TvpSelectionCriteria.Columns.TradeGroupCountry, TvpSelectionCriteria.Columns.DataGrouping,
			TvpSelectionCriteria.Columns.Preference, TvpSelectionCriteria.Columns.OrderNumber, BaseLoaderTestHelper.BizoPkColumn
		};

		protected override BaseLoader<BaseLoaderTestHelper.DummyBizoForTest> BaseLoaderForTest => new BaseLoaderTestHelper.BaseLoaderForTest(Factory);

		TariffView tariff;
		ZGuid bizoPk1, bizoPk2;
		BaseLoaderTestHelper.TariffCriteriaSetForTest tariffCriteriaSet1_1, tariffCriteriaSet1_2, tariffCriteriaSet2_1, tariffCriteriaSet2_2;
		ZString CurrentCountry => GlbCompany.CurrentCompany.Country.Code;
	}
}
