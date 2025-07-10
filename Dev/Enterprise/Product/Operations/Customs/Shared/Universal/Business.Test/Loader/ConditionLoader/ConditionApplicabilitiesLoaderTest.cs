using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class ConditionApplicabilitiesLoaderTest : TestCase
	{
		public void TestGetEmptyConditionSelectionCriteriaTable()
		{
			var expectedCriteriaTableColumns = new[]
			{
				TvpConditionSelectionCriteria.Columns.CriteriaId , TvpConditionSelectionCriteria.Columns.TariffPK, TvpConditionSelectionCriteria.Columns.TradeGroupCountry, TvpConditionSelectionCriteria.Columns.DataGrouping, TvpConditionSelectionCriteria.Columns.EffectiveDate,
				TvpConditionSelectionCriteria.Columns.IsImport, TvpConditionSelectionCriteria.Columns.IsExport, TvpConditionSelectionCriteria.Columns.ConditionClass, TvpConditionSelectionCriteria.Columns.ConditionType, TvpConditionSelectionCriteria.Columns.Preference,
				TvpConditionSelectionCriteria.Columns.OrderNumber
			};
			var tvpDataTable = SetUpEmptyConditionSelectionCriteriaTable();
			CombineAssertions(() =>
			{
				AssertEquals("TVP Row Count", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(expectedCriteriaTableColumns, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		public void TestGetEmptySecondTradeGroupsParameterTable()
		{
			var expectedCriteriaTableColumns = new[]
			{
				TvpSecondTradeGroup.Columns.Id, TvpSecondTradeGroup.Columns.CriteriaId, TvpSecondTradeGroup.Columns.SecondTradeGroup
			};
			var tvpDataTable = SetUpEmptySecondTradeGroupsParameterTable();
			CombineAssertions(() =>
			{
				AssertEquals("TVP Row Count", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(expectedCriteriaTableColumns, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		public void TestAddNewCriteriaSetRow_NeedSecondTradeGroup() => AssertAddNewCriteriaSetRow(true);

		public void TestAddNewCriteriaSetRow_NoNeedSecondTradeGroup() => AssertAddNewCriteriaSetRow(false);

		static void AssertAddNewCriteriaSetRow(bool needSecondTradeGroup)
		{
			var tvpDataTable = SetUpEmptyConditionSelectionCriteriaTable();
			var secondTradeGroupsDataTable = SetUpEmptySecondTradeGroupsParameterTable();
			var criteriaId = Guid.NewGuid();
			var tariffPk = ZGuid.NewZGuid();
			var selectionCriteria = new ZZConditionSelectionCriteria(new ZDateTime(2020, 8, 12, 23, 40, 21), "TG", "PP",
				new HashSet<ZString> { "AC1", "AC2" }, "CO", "DG", ConditionChecker.ConditionDirection.Import, "CC", "CT",
				new HashSet<ZString> { "TR1", "TR2", "TR1" });

			ConditionApplicabilitiesLoader.AddNewCriteriaSetRow(tvpDataTable, secondTradeGroupsDataTable, criteriaId, tariffPk, selectionCriteria, needSecondTradeGroup);

			CombineAssertions(() =>
			{
				AssertEquals("TVP row count", 1, tvpDataTable.Rows.Count);
				var row = tvpDataTable.Rows[0];
				AssertEquals("CriteriaId", criteriaId, (Guid)row[TvpConditionSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("TariffPK", tariffPk, (Guid)row[TvpConditionSelectionCriteria.Columns.TariffPK]);
				AssertEquals("TradeGroupCountry", "TG", row[TvpConditionSelectionCriteria.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", "DG", row[TvpConditionSelectionCriteria.Columns.DataGrouping].ToString());
				AssertEquals("EffectiveDate", new DateTime(2020, 8, 12, 23, 40, 21), (DateTime)row[TvpConditionSelectionCriteria.Columns.EffectiveDate]);
				AssertEquals("IsImport", true, (bool)row[TvpConditionSelectionCriteria.Columns.IsImport]);
				AssertEquals("IsExport", false, (bool)row[TvpConditionSelectionCriteria.Columns.IsExport]);
				AssertEquals("ConditionClass", "CC", row[TvpConditionSelectionCriteria.Columns.ConditionClass].ToString());
				AssertEquals("ConditionType", "CT", row[TvpConditionSelectionCriteria.Columns.ConditionType].ToString());
				AssertEquals("Preference", "PP", row[TvpConditionSelectionCriteria.Columns.Preference].ToString());
				AssertEquals("OrderNumber", "CO", row[TvpConditionSelectionCriteria.Columns.OrderNumber].ToString());

				if (needSecondTradeGroup)
				{
					AssertEquals("[Two SecondTradeGroups] TVP SecondTradeGroup Row Count", 2, secondTradeGroupsDataTable.Rows.Count);
					var secondTradeGroupRow = secondTradeGroupsDataTable.Rows[0];
					AssertEquals("Row 0 - CriteriaId", criteriaId, (Guid)secondTradeGroupRow[TvpSecondTradeGroup.Columns.CriteriaId]);
					AssertEquals("Row 0 - SecondTradeGroup", "TR1", secondTradeGroupRow[TvpSecondTradeGroup.Columns.SecondTradeGroup].ToString());
					var secondTradeGroupRow1 = secondTradeGroupsDataTable.Rows[1];
					AssertEquals("Row 1 - CriteriaId", criteriaId, (Guid)secondTradeGroupRow1[TvpSecondTradeGroup.Columns.CriteriaId]);
					AssertEquals("Row 1 - SecondTradeGroup", "TR2", secondTradeGroupRow1[TvpSecondTradeGroup.Columns.SecondTradeGroup].ToString());
				}
				else
				{
					AssertEquals("Empty TVP SecondTradeGroup", 0, secondTradeGroupsDataTable.Rows.Count);
				}
			});
		}

		static DataTable SetUpEmptyConditionSelectionCriteriaTable() => ConditionApplicabilitiesLoader.GetEmptyConditionSelectionCriteriaTable();

		static DataTable SetUpEmptySecondTradeGroupsParameterTable() => BaseLoaderTestHelper.BaseLoaderForTest.GetEmptySecondTradeGroupsParameterTable();
	}
}
