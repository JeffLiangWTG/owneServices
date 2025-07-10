using System;
using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffCriteriaSet<RefCusCondition>))]
	class ConditionLoadTariffCriteriaSetTest : TariffCriteriaSetAbstractTest<RefCusCondition>
	{
		[TestDate(2021, 8, 17, 7, 53, 15)]
		public override void TestCacheKeyWhenCriteriaSetHasNullValues()
		{
			var testCriteriaSetWithNullValues = new ZZConditionSelectionCriteria(ZDate.Empty, null, null, null, null, null, ConditionChecker.ConditionDirection.Import, null, null);
			var tariffCriteriaSet = new ConditionLoadTariffCriteriaSet(Tariff, testCriteriaSetWithNullValues);
			AssertEquals("CacheKey with null criteria values", $"ConditionLoadTariffCriteriaSet_{Tariff.PK}____2021-08-17T00:00:00____Import__", tariffCriteriaSet.CacheKey);
		}

		[TestDate(2021, 8, 20)]
		public override void TestGetCriteriaSetCacheKeyWhenCriteriaSetHasEmptyValues()
		{
			var emptyTariffPk = ZGuid.Empty;
			var emptyTestCriteria = new ZZConditionSelectionCriteria(ZDate.Empty, "", "", null, "", "", ConditionChecker.ConditionDirection.Import, "", "");
			AssertEquals("Cache Key with empty criteria values", $"ConditionLoadTariffCriteriaSet_00000000-0000-0000-0000-000000000000____2021-08-20T00:00:00____Import__", TariffCriteriaSetForTest.GetCriteriaSetCacheKey(emptyTariffPk, emptyTestCriteria));
		}

		public override void TestAddNewCriteriaSetRow()
		{
			var tvpConditionDataTable = new ApplicableConditionLoader(Factory).GetEmptyCriteriaTable();
			var tvpAdditionalCodesDataTable = ApplicableConditionLoader.GetEmptyAdditionalCodesParameterTable();
			var secondTradeGroupsDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptySecondTradeGroupsParameterTable();
			AssertEquals("TVP initial row count", 0, tvpConditionDataTable.Rows.Count);
			AssertEquals("TVP AdditionalCodes row count", 0, tvpAdditionalCodesDataTable.Rows.Count);

			var criteriaId = Guid.NewGuid();
			var tariffPk = ZGuid.NewZGuid();
			var dateTime = new ZDateTime(2020, 8, 12, 23, 40, 21);
			var selectionCriteria = new ZZConditionSelectionCriteria(dateTime, "CA", "PRE", new HashSet<ZString> { "AC1", "AC2" }, "ORD", "ZZ", ConditionChecker.ConditionDirection.Import, "CC", "CT", new HashSet<ZString> { "TR1", "TR2", "TR1" });
			TariffCriteriaSetForTest.AddNewCriteriaSetRow(tvpConditionDataTable, tvpAdditionalCodesDataTable, secondTradeGroupsDataTable, criteriaId, tariffPk, selectionCriteria);

			CombineAssertions(() =>
			{
				AssertEquals("TVP condition row count", 1, tvpConditionDataTable.Rows.Count);
				var row = tvpConditionDataTable.Rows[0];
				AssertEquals("CriteriaId", criteriaId, (Guid)row[TvpConditionSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("TariffPK", tariffPk, (Guid)row[TvpConditionSelectionCriteria.Columns.TariffPK]);
				AssertEquals("TradeGroupCountry", "CA", row[TvpConditionSelectionCriteria.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", "ZZ", row[TvpConditionSelectionCriteria.Columns.DataGrouping].ToString());
				AssertEquals("Preference", "PRE", row[TvpConditionSelectionCriteria.Columns.Preference].ToString());
				AssertEquals("OrderNumber", "ORD", row[TvpConditionSelectionCriteria.Columns.OrderNumber].ToString());
				AssertEquals("EffectiveDate", dateTime.ToDateTime(),
					(DateTime)row[TvpConditionSelectionCriteria.Columns.EffectiveDate]);
				AssertEquals("ConditionClass", "CC", row[TvpConditionSelectionCriteria.Columns.ConditionClass].ToString());
				AssertEquals("ConditionType", "CT", row[TvpConditionSelectionCriteria.Columns.ConditionType].ToString());
				AssertEquals("IsImport", true, (bool)row[TvpConditionSelectionCriteria.Columns.IsImport]);
				AssertEquals("IsExport", false, (bool)row[TvpConditionSelectionCriteria.Columns.IsExport]);

				AssertEquals("TVP AdditionalCodes row count", 2, tvpAdditionalCodesDataTable.Rows.Count);
				var additionalCodeRow = tvpAdditionalCodesDataTable.Rows[0];
				AssertEquals("TVP AdditionalCodes: CriteriaId", criteriaId, (Guid)additionalCodeRow[TvpAdditionalCodes.Columns.CriteriaId]);
				AssertEquals("TVP AdditionalCodes: AdditionalCodes", "AC1", additionalCodeRow[TvpAdditionalCodes.Columns.AdditionalCode].ToString());
				var additionalCodeRow1 = tvpAdditionalCodesDataTable.Rows[1];
				AssertEquals("TVP AdditionalCodes: CriteriaId", criteriaId, (Guid)additionalCodeRow1[TvpAdditionalCodes.Columns.CriteriaId]);
				AssertEquals("TVP AdditionalCodes: AdditionalCodes", "AC2", additionalCodeRow1[TvpAdditionalCodes.Columns.AdditionalCode].ToString());

				AssertEquals("[Two SecondTradeGroups] TVP SecondTradeGroup Row Count", 2, secondTradeGroupsDataTable.Rows.Count);
				var secondTradeGroupRow = secondTradeGroupsDataTable.Rows[0];
				AssertEquals("Row 0 - CriteriaId", criteriaId, (Guid)secondTradeGroupRow[TvpSecondTradeGroup.Columns.CriteriaId]);
				AssertEquals("Row 0 - SecondTradeGroup", "TR1", secondTradeGroupRow[TvpSecondTradeGroup.Columns.SecondTradeGroup].ToString());
				var secondTradeGroupRow1 = secondTradeGroupsDataTable.Rows[1];
				AssertEquals("Row 1 - CriteriaId", criteriaId, (Guid)secondTradeGroupRow1[TvpSecondTradeGroup.Columns.CriteriaId]);
				AssertEquals("Row 1 - SecondTradeGroup", "TR2", secondTradeGroupRow1[TvpSecondTradeGroup.Columns.SecondTradeGroup].ToString());
			});
		}

		[TestDate(2021, 8, 17)]
		public override void TestAddNewCriteriaSetRow_Default()
		{
			var tvpConditionDataTable = new ApplicableConditionLoader(Factory).GetEmptyCriteriaTable();
			var tvpAdditionalCodesDataTable = ApplicableConditionLoader.GetEmptyAdditionalCodesParameterTable();
			var secondTradeGroupsDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptySecondTradeGroupsParameterTable();
			TariffCriteriaSetForTest.AddNewCriteriaSetRow(tvpConditionDataTable, tvpAdditionalCodesDataTable, secondTradeGroupsDataTable, Guid.Empty, ZGuid.BrettsGuid, new ZZConditionSelectionCriteria(ZDateTime.Empty, "", "", null, "", "", ConditionChecker.ConditionDirection.Either, "", ""));

			CombineAssertions(() =>
			{
				AssertEquals("TVP condition row count", 1, tvpConditionDataTable.Rows.Count);
				var row = tvpConditionDataTable.Rows[0];
				AssertEquals("CriteriaId", Guid.Empty, (Guid)row[TvpConditionSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("TariffPK", ZGuid.BrettsGuid, (Guid)row[TvpConditionSelectionCriteria.Columns.TariffPK]);
				AssertEquals("TradeGroupCountry", "", row[TvpConditionSelectionCriteria.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", "", row[TvpConditionSelectionCriteria.Columns.DataGrouping].ToString());
				AssertEquals("Preference", "", row[TvpConditionSelectionCriteria.Columns.Preference].ToString());
				AssertEquals("OrderNumber", "", row[TvpConditionSelectionCriteria.Columns.OrderNumber].ToString());
				AssertEquals("EffectiveDate", new DateTime(2021, 8, 17, 0, 0, 0), (DateTime)row[TvpConditionSelectionCriteria.Columns.EffectiveDate]);
				AssertEquals("ConditionClass", "", row[TvpConditionSelectionCriteria.Columns.ConditionClass].ToString());
				AssertEquals("ConditionType", "", row[TvpConditionSelectionCriteria.Columns.ConditionType].ToString());
				AssertEquals("IsImport", true, (bool)row[TvpConditionSelectionCriteria.Columns.IsImport]);
				AssertEquals("IsExport", true, (bool)row[TvpConditionSelectionCriteria.Columns.IsExport]);

				AssertEquals("TVP AdditionalCodes row count", 0, tvpAdditionalCodesDataTable.Rows.Count);
				AssertEquals("TVP SecondTradeGroups row count", 0, secondTradeGroupsDataTable.Rows.Count);
			});
		}

		protected override string ExpectedCriteriaSetCacheKey => $"ConditionLoadTariffCriteriaSet_{Tariff.PK}_CN_TR1.TR2_ZZ_2021-08-12T00:00:00_PRE_AC1.AC2_ORD_Import_CC_CT";

		protected override TariffCriteriaSet<RefCusCondition> TariffCriteriaSetForTest => new ConditionLoadTariffCriteriaSet(Tariff, testCriteria);

		TariffView Tariff => _tariff ?? (_tariff = Factory.New<TariffView>());
		TariffView _tariff;

		ZZConditionSelectionCriteria testCriteria => _testCriteria ?? (_testCriteria = new ZZConditionSelectionCriteria(new ZDateTime(2021, 8, 12), "CN", "PRE", new HashSet<ZString> { "AC1", "AC2" }, "ORD", "ZZ", ConditionChecker.ConditionDirection.Import, "CC", "CT", new HashSet<ZString> { "TR1", "TR2" }));
		ZZConditionSelectionCriteria _testCriteria;
	}
}
