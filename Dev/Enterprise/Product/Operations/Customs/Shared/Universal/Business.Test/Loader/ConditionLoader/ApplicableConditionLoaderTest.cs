using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(BaseLoader<RefCusCondition>))]
	class ApplicableConditionLoaderTest : BaseLoaderAbstractTest<RefCusCondition>
	{
		public void TestLoadConditionsForSingleCriteriaSet()
		{
			CreateReferenceData();
			var conditionLoader = new ApplicableConditionLoader(Factory);
			var selectionCriteria = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria);

			CombineAssertions(() =>
			{
				var conditions = conditionLoader.LoadDataForSingleCriteriaSet(tariffCriteriaSet);
				var dictionary = GetDictionaryForTesting(Factory);
				AssertCriteriaSetCached("tariffCriteriaSet cached", tariffCriteriaSet, dictionary, true);
				AssertEquals("Loaded Condition count", 1, conditions.Count());
				AssertLoadedBizosCount("condition1", conditions, condition1.PK, 1);
			});
		}

		public void TestLoadConditionsForSingleCriteriaSet_SecondTradeGroup()
		{
			CreateReferenceData();
			var conditionLoader = new ApplicableConditionLoader(Factory);
			var selectionCriteria = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P2", new HashSet<ZString>(), "",
				EunGroupCode, ConditionChecker.ConditionDirection.Export, "CTRL", "TSTC1", new HashSet<ZString> { "STANDARD" });
			var tariffCriteriaSet = new ConditionLoadTariffCriteriaSet(tariff3, selectionCriteria);

			CombineAssertions(() =>
			{
				var conditions = conditionLoader.LoadDataForSingleCriteriaSet(tariffCriteriaSet);
				var dictionary = GetDictionaryForTesting(Factory);
				AssertCriteriaSetCached("tariffCriteriaSet cached", tariffCriteriaSet, dictionary, true);
				AssertEquals("Loaded Condition count", 1, conditions.Count());
				AssertLoadedBizosCount("condition9", conditions, condition9.PK, 1);
			});
		}

		public void TestCacheConditionsForMultipleCriteriaSets()
		{
			CreateReferenceData();
			var conditionLoader = new ApplicableConditionLoader(Factory);
			var selectionCriteria1 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet1 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria1);
			var selectionCriteria2 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet2 = new ConditionLoadTariffCriteriaSet(tariff2, selectionCriteria2);
			var selectionCriteria3 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P2",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet3 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria3);

			var tariffCriteriaSetsList =
				new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet1, tariffCriteriaSet2 };
			CombineAssertions(() =>
			{
				var dictionary = GetDictionaryForTesting(Factory);
				conditionLoader.CacheDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("tariffCriteriaSet1 cached", tariffCriteriaSet1, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet2 cached", tariffCriteriaSet2, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet3 not cached yet", tariffCriteriaSet3, dictionary, false);

				tariffCriteriaSetsList = new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet3 };
				conditionLoader.CacheDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertCriteriaSetCached("tariffCriteriaSet1 already cached", tariffCriteriaSet1, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet2 already cached", tariffCriteriaSet2, dictionary, true);
				AssertCriteriaSetCached("tariffCriteriaSet3 cached", tariffCriteriaSet3, dictionary, true);
			});
		}

		public void TestLoadConditionsForMultipleCriteriaSets_LoadConditionOnlyOnceIfMatchMultiCriteriaSet()
		{
			CreateReferenceData();
			var conditionLoader = new ApplicableConditionLoader(Factory);
			var selectionCriteria1 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet1 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria1);
			var selectionCriteria2 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "", "");
			var tariffCriteriaSet2 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria2);

			var tariffCriteriaSetsList = new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet1, tariffCriteriaSet2 };
			CombineAssertions(() =>
			{
				var conditions = conditionLoader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("Loaded Condition count: loaded", 3, conditions.Count());
				AssertLoadedBizosCount("condition1", conditions, condition1.PK, 1);  // Match selectionCriteria1 & selectionCriteria2 but load once
				AssertLoadedBizosCount("condition5", conditions, condition5.PK, 1);  // Match selectionCriteria2 only
				AssertLoadedBizosCount("condition7", conditions, condition7.PK, 1);  // Match selectionCriteria2 only

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(tariffCriteriaSetsList);
				AssertEquals("Loaded Condition count: Cached", 3, conditions.Count());
				AssertLoadedBizosCount("condition1", conditions, condition1.PK, 1);  // Match selectionCriteria1 & selectionCriteria2 but load once
				AssertLoadedBizosCount("condition5", conditions, condition5.PK, 1);  // Match selectionCriteria2 only
				AssertLoadedBizosCount("condition7", conditions, condition7.PK, 1);  // Match selectionCriteria2 only
			});
		}

		public void TestLoadConditionsForMultipleCriteriaSets()
		{
			CreateReferenceData();
			var conditionLoader = new ApplicableConditionLoader(Factory);
			var selectionCriteria1 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet1 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria1);
			var selectionCriteria2 = new ZZConditionSelectionCriteria(ZDateTime.Today, "DE", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet2 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria2);
			var selectionCriteria3 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>() { "AC2" }, "1", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1");
			var tariffCriteriaSet3 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria3);
			var selectionCriteria4 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString> { "AC1", "AC2" }, "1", EunGroupCode, ConditionChecker.ConditionDirection.Import,
				"RATE", "TSTR1");
			var tariffCriteriaSet4 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria4);
			var selectionCriteria5 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "CTRL", "TSTC1");
			var tariffCriteriaSet5 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria5);
			var selectionCriteria6 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P2",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "CTRL", "TSTC1");
			var tariffCriteriaSet6 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria6);
			var selectionCriteria7 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Either, "", "");
			var tariffCriteriaSet7 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria7);
			var selectionCriteria8 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P2",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Export, "CTRL", "TSTC1");
			var tariffCriteriaSet8 = new ConditionLoadTariffCriteriaSet(tariff2, selectionCriteria8);

			CombineAssertions(() =>
			{
				var conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet1 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: ZA, Preference: P1, Direction: Import, ConditionClass: RATE, ConditionType: TSTR1]", 1, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet1: condition1", conditions, condition1.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet2 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: DE, Preference: P1, Direction: Import, ConditionClass: RATE, ConditionType: TSTR1]", 1, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet2: condition2", conditions, condition2.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet3 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: ZA, Preference: P1, Direction: Import, ConditionClass: RATE, ConditionType: TSTR1, OderNumber: 1]", 1, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet3: condition3", conditions, condition3.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet4 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: ZA, Preference: P1, Direction: Import, ConditionClass: RATE, ConditionType: TSTR1, AdditionalCode: AC1,AC2]", 2, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet4: condition4", conditions, condition4.PK, 1);
				AssertLoadedBizosCount("tariffCriteriaSet4: condition3", conditions, condition3.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet5 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: ZA, Preference: P1, Direction: Import, ConditionClass: CTRL, ConditionType: TSTC1]", 2, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet5: condition5", conditions, condition5.PK, 1);
				AssertLoadedBizosCount("tariffCriteriaSet5: condition7", conditions, condition7.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet6 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: ZA, Preference: P2, Direction: Import, ConditionClass: CTRL, ConditionType: TSTC1]", 1, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet6: condition6", conditions, condition6.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet7 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: ZA, Preference: P1, Direction: Import", 3, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet7: condition7", conditions, condition7.PK, 1);
				AssertLoadedBizosCount("tariffCriteriaSet7: condition1", conditions, condition1.PK, 1);
				AssertLoadedBizosCount("tariffCriteriaSet7: condition5", conditions, condition5.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet8 });
				AssertEquals("Loaded Condition count: [Tariff: tariff2, TradeGroupCountry: ZA, Preference: P2, Direction: Import, ConditionClass: CTRL, ConditionType: TSTC1]", 1, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet8: condition8", conditions, condition8.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet1, tariffCriteriaSet2, tariffCriteriaSet3, tariffCriteriaSet4, tariffCriteriaSet5, tariffCriteriaSet6, tariffCriteriaSet7, tariffCriteriaSet8 });
				AssertEquals("Loaded Condition count: All tariffCriteriaSet", 8, conditions.Count());
				AssertLoadedBizosCount("All tariffCriteriaSet: condition1", conditions, condition1.PK, 1);
				AssertLoadedBizosCount("All tariffCriteriaSet: condition2", conditions, condition2.PK, 1);
				AssertLoadedBizosCount("All tariffCriteriaSet: condition3", conditions, condition3.PK, 1);
				AssertLoadedBizosCount("All tariffCriteriaSet: condition4", conditions, condition4.PK, 1);
				AssertLoadedBizosCount("All tariffCriteriaSet: condition5", conditions, condition5.PK, 1);
				AssertLoadedBizosCount("All tariffCriteriaSet: condition6", conditions, condition6.PK, 1);
				AssertLoadedBizosCount("All tariffCriteriaSet: condition7", conditions, condition7.PK, 1);
				AssertLoadedBizosCount("All tariffCriteriaSet: condition8", conditions, condition8.PK, 1);
			});
		}

		public void TestLoadConditionsForMultipleCriteriaSets_SecondTradeGroup()
		{
			CreateReferenceData();
			var conditionLoader = new ApplicableConditionLoader(Factory);
			var selectionCriteria1 = new ZZConditionSelectionCriteria(ZDateTime.Today, "ZA", "P2", new HashSet<ZString>(), "",
				EunGroupCode, ConditionChecker.ConditionDirection.Export, "CTRL", "TSTC1", new HashSet<ZString> { "STANDARD", "XX" });
			var tariffCriteriaSet1 = new ConditionLoadTariffCriteriaSet(tariff3, selectionCriteria1);
			var selectionCriteria2 = new ZZConditionSelectionCriteria(ZDateTime.Today, "DE", "P1",
				new HashSet<ZString>(), "", EunGroupCode, ConditionChecker.ConditionDirection.Import, "RATE", "TSTR1", new HashSet<ZString> { "XX" });
			var tariffCriteriaSet2 = new ConditionLoadTariffCriteriaSet(tariff, selectionCriteria2);

			CombineAssertions(() =>
			{
				var conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet1 });
				AssertEquals("Loaded Condition count: [Tariff: tariff3, TradeGroupCountry: ZA, Preference: P2, Direction: Export, ConditionClass: CTRL, ConditionType: TSTC1, SecondTradeGroup: STANDARD]", 1, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet1: condition9", conditions, condition9.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet2 });
				AssertEquals("Loaded Condition count: [Tariff: tariff, TradeGroupCountry: DE, Preference: P1, Direction: Import, ConditionClass: RATE, ConditionType: TSTR1, SecondTradeGroup: NULL]", 1, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet2: condition2", conditions, condition2.PK, 1);

				conditions = conditionLoader.LoadDataForMultipleCriteriaSets(new List<ConditionLoadTariffCriteriaSet> { tariffCriteriaSet1, tariffCriteriaSet2 });
				AssertEquals("Loaded Condition count: Both", 2, conditions.Count());
				AssertLoadedBizosCount("tariffCriteriaSet1: condition1", conditions, condition9.PK, 1);
				AssertLoadedBizosCount("tariffCriteriaSet2: condition2", conditions, condition2.PK, 1);
			});
		}

		protected override string[] ExpectedCriteriaTableColumns => new[]
		{
			TvpSelectionCriteria.Columns.CriteriaId, TvpSelectionCriteria.Columns.TariffPK, TvpSelectionCriteria.Columns.EffectiveDate, TvpSelectionCriteria.Columns.TradeGroupCountry, TvpSelectionCriteria.Columns.DataGrouping, TvpSelectionCriteria.Columns.Preference,
			TvpSelectionCriteria.Columns.OrderNumber, TvpConditionSelectionCriteria.Columns.ConditionType, TvpConditionSelectionCriteria.Columns.ConditionClass, TvpConditionSelectionCriteria.Columns.IsExport, TvpConditionSelectionCriteria.Columns.IsImport
		};

		protected override BaseLoader<RefCusCondition> BaseLoaderForTest => new ApplicableConditionLoader(Factory);

		void CreateReferenceData()
		{
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(EunGroupCode);
			var tariffType = helper.CreateTariffType(EunGroupCode, "T1");
			var stdTradeGroup = helper.CreateTradeGroup(EunGroupCode, "STANDARD", startDate, endDate);
			var othTradeGroup = helper.CreateTradeGroup(EunGroupCode, "OTHER", startDate, endDate);
			conditionType1 = helper.CreateOrGetExistingRefCusConditionType(EunGroupCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "TSTR1", "Test Rate Condition Type 1");
			conditionType2 = helper.CreateOrGetExistingRefCusConditionType(EunGroupCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
			preference1 = helper.CreatePreferenceForCountry("P1", "TestPreference1", EunGroupCode);
			preference2 = helper.CreatePreferenceForCountry("P2", "TestPreference2", EunGroupCode);
			Factory.Save();

			tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			tariff2 = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC2", startDate, endDate);
			tariff3 = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC3", startDate, endDate);
			helper.AddCountry(stdTradeGroup, "ZA");
			helper.AddCountry(othTradeGroup, "DE");

			condition1 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType1.PK, tariff.PK, "C1", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			helper.CreateCusApplicabilityInternal(condition1, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "");
			condition2 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType1.PK, tariff.PK, "C2", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			helper.CreateCusApplicabilityInternal(condition2, othTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "");
			condition3 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType1.PK, tariff.PK, "C3", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			helper.CreateCusApplicabilityInternal(condition3, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "AC2", orderNumber: "1");
			condition4 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType1.PK, tariff.PK, "C4", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			helper.CreateCusApplicabilityInternal(condition4, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "AC1", orderNumber: "1");
			condition5 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType2.PK, tariff.PK, "C5", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			helper.CreateCusApplicabilityInternal(condition5, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "");
			condition6 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType2.PK, tariff.PK, "C6", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference2.PK);
			helper.CreateCusApplicabilityInternal(condition6, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "");
			condition7 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType2.PK, tariff.PK, "C7", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			condition8 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType2.PK, tariff2.PK, "C8", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference2.PK);
			helper.CreateCusApplicabilityInternal(condition8, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "");
			condition9 = helper.CreateOrGetExistingRefCusCondition(EunGroupCode, conditionType2.PK, tariff3.PK, "C9", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference2.PK);
			helper.CreateCusApplicabilityInternal(condition9, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "", secondTradeGroup: stdTradeGroup);
			Factory.Save();
		}

		Dictionary<string, RefCusCondition[]> GetDictionaryForTesting(BusinessObjectFactory factory) => factory.GetCachedValue("UniversalCachedData", () => new Dictionary<string, RefCusCondition[]>());

		string EunGroupCode => Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		TariffView tariff, tariff2, tariff3;
		CusRefPreferenceView preference1, preference2;
		RefCusConditionType conditionType1, conditionType2;
		RefCusCondition condition1, condition2, condition3, condition4, condition5, condition6, condition7, condition8, condition9;
	}
}
