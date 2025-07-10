using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;
using RateCriteriaTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpRateSelectionCriteria_V2;
using SecondTradeGroupTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpSecondTradeGroup;

namespace Enterprise.Customs.Universal.Testing
{
	class ApplicableRateLoaderStatelessTest : TestCase
	{
		public void TestGetEmptyRateCriteriaTable()
		{
			var expectedCriteriaTableColumns = new[]
			{
				RateCriteriaTvp.Columns.CriteriaId , RateCriteriaTvp.Columns.TariffPK, RateCriteriaTvp.Columns.EffectiveDate, RateCriteriaTvp.Columns.TradeGroupCountry,
				RateCriteriaTvp.Columns.DataGrouping, RateCriteriaTvp.Columns.Preference, RateCriteriaTvp.Columns.AdditionalCodesXml,
				RateCriteriaTvp.Columns.OrderNumber, RateCriteriaTvp.Columns.RateType, RateCriteriaTvp.Columns.RateCode, RateCriteriaTvp.Columns.Direction
			};

			var tvpDataTable = ApplicableRateLoader.GetEmptyRateCriteriaTable();

			CombineAssertions(() =>
			{
				AssertNotNull("Created TVP_RateSelectionCriteria_V2 Data Table", tvpDataTable);
				AssertEquals("TVP Row Count", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(expectedCriteriaTableColumns, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		public void TestGetEmptySecondTradeGroupsParameterTable()
		{
			var expectedCriteriaTableColumns = new[]
			{
				SecondTradeGroupTvp.Columns.Id, SecondTradeGroupTvp.Columns.CriteriaId, SecondTradeGroupTvp.Columns.SecondTradeGroup
			};

			var tvpDataTable = ApplicableRateLoader.GetEmptySecondTradeGroupsParameterTable();

			CombineAssertions(() =>
			{
				AssertNotNull("Created TVP_SecondTradeGroup Data Table", tvpDataTable);
				AssertEquals("TVP Row Count", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(expectedCriteriaTableColumns, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		public void TestAddNewCriteriaSetRow()
		{
			var tvpRateCriteriaTable = ApplicableRateLoader.GetEmptyRateCriteriaTable();
			var tvpSecondTradeGroupsParameterTable = ApplicableRateLoader.GetEmptySecondTradeGroupsParameterTable();
			AssertEquals("TVP initial row count", 0, tvpRateCriteriaTable.Rows.Count);
			var criteriaId = Guid.NewGuid();
			var tariffPk = ZGuid.NewZGuid();
			var selectionCriteria = new SpecificRateSelectionCriteria("XX", "YYY", "PP", "CO",
				new HashSet<ZString> { "AC1", "AC2" }, new ZDateTime(2020, 8, 12, 23, 40, 21), "RT", "RC",
				new HashSet<ZString> { "TR1", "TR2" });
			ApplicableRateLoader.AddNewCriteriaSetRow(tvpRateCriteriaTable, tvpSecondTradeGroupsParameterTable,
				criteriaId, tariffPk, selectionCriteria);

			CombineAssertions("Row 0", () =>
			{
				AssertEquals("tvpRateCriteriaTable row count", 1, tvpRateCriteriaTable.Rows.Count);

				var row = tvpRateCriteriaTable.Rows[0];
				AssertEquals("CriteriaId", criteriaId, (Guid)row[RateCriteriaTvp.Columns.CriteriaId]);
				AssertEquals("TariffPK", tariffPk, (Guid)row[RateCriteriaTvp.Columns.TariffPK]);
				AssertEquals("TradeGroupCountry", "XX", row[RateCriteriaTvp.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", "YYY", row[RateCriteriaTvp.Columns.DataGrouping].ToString());
				AssertEquals("Preference", "PP", row[RateCriteriaTvp.Columns.Preference].ToString());
				AssertEquals("AdditionalCodesXml", "<v>AC1</v><v>AC2</v>", row[RateCriteriaTvp.Columns.AdditionalCodesXml].ToString());
				AssertEquals("OrderNumber", "CO", row[RateCriteriaTvp.Columns.OrderNumber].ToString());
				AssertEquals("EffectiveDate", new DateTime(2020, 8, 12, 23, 40, 21), (DateTime)row[RateCriteriaTvp.Columns.EffectiveDate]);
				AssertEquals("RateType", "RT", row[RateCriteriaTvp.Columns.RateType].ToString());
				AssertEquals("RateCode", "RC", row[RateCriteriaTvp.Columns.RateCode].ToString());

				AssertEquals("tvpSecondTradeGroupsTable row count", 2, tvpSecondTradeGroupsParameterTable.Rows.Count);
				var secondTradeGroupRow1 = tvpSecondTradeGroupsParameterTable.Rows[0];
				AssertEquals("CriteriaId", criteriaId, (Guid)secondTradeGroupRow1[SecondTradeGroupTvp.Columns.CriteriaId]);
				AssertEquals("SecondTradeGroup", "TR1", secondTradeGroupRow1[SecondTradeGroupTvp.Columns.SecondTradeGroup]);
				var secondTradeGroupRow2 = tvpSecondTradeGroupsParameterTable.Rows[1];
				AssertEquals("CriteriaId", criteriaId, (Guid)secondTradeGroupRow2[SecondTradeGroupTvp.Columns.CriteriaId]);
				AssertEquals("SecondTradeGroup", "TR2", secondTradeGroupRow2[SecondTradeGroupTvp.Columns.SecondTradeGroup]);
			});
		}

		[TestDate(2020, 6, 13, 15, 24, 33)]
		public void TestAddNewCriteriaSetRow_EmptyCriteria()
		{
			var tvpRateCriteriaTable = ApplicableRateLoader.GetEmptyRateCriteriaTable();
			var tvpSecondTradeGroupsParameterTable = ApplicableRateLoader.GetEmptySecondTradeGroupsParameterTable();
			var selectionCriteria = new SpecificRateSelectionCriteria("", "", "", "", null, ZDateTime.Empty, "", "");
			ApplicableRateLoader.AddNewCriteriaSetRow(tvpRateCriteriaTable, tvpSecondTradeGroupsParameterTable, Guid.Empty, ZGuid.BrettsGuid, selectionCriteria);

			CombineAssertions("Row 1", () =>
			{
				AssertEquals("TVP row count", 1, tvpRateCriteriaTable.Rows.Count);

				var row1 = tvpRateCriteriaTable.Rows[0];
				AssertEquals("CriteriaId", Guid.Empty, (Guid)row1[RateCriteriaTvp.Columns.CriteriaId]);
				AssertEquals("TariffPK", ZGuid.BrettsGuid, (Guid)row1[RateCriteriaTvp.Columns.TariffPK]);
				AssertEquals("TradeGroupCountry", "", row1[RateCriteriaTvp.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", "", row1[RateCriteriaTvp.Columns.DataGrouping].ToString());
				AssertEquals("Preference", "", row1[RateCriteriaTvp.Columns.Preference].ToString());
				AssertEquals("AdditionalCodesXml", "<v></v>", row1[RateCriteriaTvp.Columns.AdditionalCodesXml].ToString());
				AssertEquals("OrderNumber", "", row1[RateCriteriaTvp.Columns.OrderNumber].ToString());
				AssertEquals("EffectiveDate", new DateTime(2020, 6, 13, 0, 0, 0), (DateTime)row1[RateCriteriaTvp.Columns.EffectiveDate]);
				AssertEquals("RateType", "", row1[RateCriteriaTvp.Columns.RateType].ToString());
				AssertEquals("RateCode", "", row1[RateCriteriaTvp.Columns.RateCode].ToString());

				AssertEquals("tvpSecondTradeGroupsTable row count", 0, tvpSecondTradeGroupsParameterTable.Rows.Count);
			});
		}
	}
}
