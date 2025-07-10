using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(BaseLoaderTestHelper.TariffCriteriaSetForTest))]
	class TariffCriteriaSetBaseOnlyTest : TariffCriteriaSetAbstractTest<BaseLoaderTestHelper.DummyBizoForTest>
	{
		public void TestConstruction()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Attempt to instantiate TariffCriteriaSet with a null selectionCriteria", typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: selectionCriteria", () => new BaseLoaderTestHelper.TariffCriteriaSetForTest(Tariff, selectionCriteria: null));
				AssertExceptionThrown("Attempt to instantiate TariffCriteriaSet with a null tariff", typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: tariff", () => new BaseLoaderTestHelper.TariffCriteriaSetForTest(tariff: null, Criteria));
				AssertNoExceptionThrown("Instantiating TariffCriteriaSet with valid tariff and selectionCriteria objects", () => new BaseLoaderTestHelper.TariffCriteriaSetForTest(Tariff, Criteria));
			});
		}

		public void TestCriteriaId()
		{
			CombineAssertions(() =>
			{
				var tariffCriteriaSet1 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(Tariff, Criteria);
				AssertNotNull(nameof(tariffCriteriaSet1.CriteriaId), tariffCriteriaSet1.CriteriaId);
				var tariffCriteriaSet2 = new BaseLoaderTestHelper.TariffCriteriaSetForTest(Tariff, Criteria);
				AssertNotNull(nameof(tariffCriteriaSet2.CriteriaId), tariffCriteriaSet2.CriteriaId);
				AssertNotEquals("Different instances of TariffCriteriaSet must have different CriteriaId", tariffCriteriaSet1.CriteriaId, tariffCriteriaSet2.CriteriaId);
			});
		}

		public void TestCacheKeyEqualWithGetCriteriaSetCacheKey()
		{
			AssertEquals("CacheKey from GetCriteriaSetCacheKey", TariffCriteriaSetForTest.CacheKey, TariffCriteriaSetForTest.GetCriteriaSetCacheKey(Tariff.PK, Criteria));
		}

		public void TestAddApplicableTableValueParameterRows()
		{
			AssertAddApplicableTableValueParameterRows("Added rows when Tariff is not National Code", 1, ZGuid.Empty);
		}

		public void TestAddApplicableTableValueParameterRows_NationalCode()
		{
			Tariff.ZZ1_TableType = RefCusTariffNationalCodeSchema.Constants.Prefix;
			Tariff.ZZ1_ZZ1_Tariff = ZGuid.NewZGuid();
			AssertAddApplicableTableValueParameterRows("Added rows when Tariff is National Code", 2, Tariff.ZZ1_ZZ1_Tariff);
		}

		[TestDate(2021, 8, 17, 7, 53, 15)]
		public override void TestCacheKeyWhenCriteriaSetHasNullValues()
		{
			var testCriteriaSetWithNullValues = new BaseLoaderTestHelper.SelectionCriteriaForTest(ZGuid.Empty, null, null, null, null, null, null);
			var tariffCriteriaSet = new BaseLoaderTestHelper.TariffCriteriaSetForTest(Tariff, testCriteriaSetWithNullValues);
			AssertEquals("CacheKey with null criteria values", $"TariffCriteriaSetForTest_{Tariff.PK}____2021-08-17T00:00:00___", tariffCriteriaSet.CacheKey);
		}

		[TestDate(2021, 8, 20)]
		public override void TestGetCriteriaSetCacheKeyWhenCriteriaSetHasEmptyValues()
		{
			var emptyTariffPk = ZGuid.Empty;
			var emptyTestCriteria = new BaseLoaderTestHelper.SelectionCriteriaForTest(ZGuid.Empty, "", "", new HashSet<ZString>(), "", "", new HashSet<ZString>());
			AssertEquals("Cache Key with empty criteria values", $"TariffCriteriaSetForTest_00000000-0000-0000-0000-000000000000____2021-08-20T00:00:00___", TariffCriteriaSetForTest.GetCriteriaSetCacheKey(emptyTariffPk, emptyTestCriteria));
		}

		[TestDate(2021, 8, 12)]
		public override void TestAddNewCriteriaSetRow()
		{
			var tvpCriteriaDataTable = new BaseLoaderTestHelper.BaseLoaderForTest(Factory).GetEmptyCriteriaTable();
			var tvpAdditionalCodesDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptyAdditionalCodesParameterTable();
			var secondTradeGroupsDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptySecondTradeGroupsParameterTable();
			AssertEquals("TVP initial row count", 0, tvpCriteriaDataTable.Rows.Count);
			AssertEquals("TVP AdditionalCodes row count", 0, tvpAdditionalCodesDataTable.Rows.Count);

			var tariffPk = ZGuid.NewZGuid();
			var criteriaId = Guid.NewGuid();
			var bizoPK = Guid.NewGuid();
			var dateTime = ZDateTime.Today;
			var selectionCriteria = new BaseLoaderTestHelper.SelectionCriteriaForTest(bizoPK, "CA", "PRE", new HashSet<ZString> { "AC1", "AC2" }, "ORD", GlbCompany.CurrentCompany.Country.Code, new HashSet<ZString> { "TR1", "TR2", "TR1" });
			TariffCriteriaSetForTest.AddNewCriteriaSetRow(tvpCriteriaDataTable, tvpAdditionalCodesDataTable, secondTradeGroupsDataTable, criteriaId, tariffPk, selectionCriteria);

			CombineAssertions(() =>
			{
				AssertEquals("TVP condition row count", 1, tvpCriteriaDataTable.Rows.Count);
				var criteriaRow = tvpCriteriaDataTable.Rows[0];
				AssertEquals("CriteriaId", criteriaId, (Guid)criteriaRow[TvpSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("TariffPK", tariffPk, (Guid)criteriaRow[TvpSelectionCriteria.Columns.TariffPK]);
				AssertEquals("TradeGroupCountry", "CA",
					criteriaRow[TvpSelectionCriteria.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code,
					criteriaRow[TvpSelectionCriteria.Columns.DataGrouping].ToString());
				AssertEquals("Preference", "PRE",
					criteriaRow[TvpSelectionCriteria.Columns.Preference].ToString());
				AssertEquals("OrderNumber", "ORD",
					criteriaRow[TvpSelectionCriteria.Columns.OrderNumber].ToString());
				AssertEquals("EffectiveDate", dateTime.ToDateTime(),
					(DateTime)criteriaRow[TvpSelectionCriteria.Columns.EffectiveDate]);
				AssertEquals("TariffPK", bizoPK, (Guid)criteriaRow[BaseLoaderTestHelper.BizoPkColumn]);

				AssertEquals("TVP AdditionalCodes row count", 2, tvpAdditionalCodesDataTable.Rows.Count);
				var additionalCodeRow = tvpAdditionalCodesDataTable.Rows[0];
				AssertEquals("Row 0 - TVP AdditionalCodes: CriteriaId", criteriaId, (Guid)additionalCodeRow[TvpAdditionalCodes.Columns.CriteriaId]);
				AssertEquals("Row 0 - TVP AdditionalCodes: AdditionalCodes", "AC1", additionalCodeRow[TvpAdditionalCodes.Columns.AdditionalCode].ToString());
				var additionalCodeRow1 = tvpAdditionalCodesDataTable.Rows[1];
				AssertEquals("Row 1 - TVP AdditionalCodes: CriteriaId", criteriaId, (Guid)additionalCodeRow1[TvpAdditionalCodes.Columns.CriteriaId]);
				AssertEquals("Row 1 - TVP AdditionalCodes: AdditionalCodes", "AC2", additionalCodeRow1[TvpAdditionalCodes.Columns.AdditionalCode].ToString());

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
			var tvpCriteriaDataTable = new BaseLoaderTestHelper.BaseLoaderForTest(Factory).GetEmptyCriteriaTable();
			var tvpAdditionalCodesDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptyAdditionalCodesParameterTable();
			var secondTradeGroupsDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptySecondTradeGroupsParameterTable();
			TariffCriteriaSetForTest.AddNewCriteriaSetRow(tvpCriteriaDataTable, tvpAdditionalCodesDataTable, secondTradeGroupsDataTable, Guid.Empty, ZGuid.BrettsGuid,
				new BaseLoaderTestHelper.SelectionCriteriaForTest(ZGuid.BrettsGuid, "", "", null, "", ""));

			CombineAssertions(() =>
			{
				AssertEquals("TVP condition row count", 1, tvpCriteriaDataTable.Rows.Count);
				var criteriaRow = tvpCriteriaDataTable.Rows[0];
				AssertEquals("CriteriaId", Guid.Empty, (Guid)criteriaRow[TvpSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("TariffPK", ZGuid.BrettsGuid, (Guid)criteriaRow[TvpSelectionCriteria.Columns.TariffPK]);
				AssertEquals("TradeGroupCountry", "", criteriaRow[TvpSelectionCriteria.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", "", criteriaRow[TvpSelectionCriteria.Columns.DataGrouping].ToString());
				AssertEquals("Preference", "", criteriaRow[TvpSelectionCriteria.Columns.Preference].ToString());
				AssertEquals("OrderNumber", "", criteriaRow[TvpSelectionCriteria.Columns.OrderNumber].ToString());
				AssertEquals("EffectiveDate", new DateTime(2021, 8, 17, 0, 0, 0), (DateTime)criteriaRow[TvpSelectionCriteria.Columns.EffectiveDate]);
				AssertEquals("TariffPK", ZGuid.BrettsGuid, (Guid)criteriaRow[BaseLoaderTestHelper.BizoPkColumn]);

				AssertEquals("TVP AdditionalCodes row count", 0, tvpAdditionalCodesDataTable.Rows.Count);
				AssertEquals("TVP SecondTradeGroups row count", 0, secondTradeGroupsDataTable.Rows.Count);
			});
		}

		protected override string ExpectedCriteriaSetCacheKey => $"TariffCriteriaSetForTest_{Tariff.PK}_CN_TR1.TR2_ZZ_2021-08-12T00:00:00_PRE_AC1.AC2_ORD";

		protected override TariffCriteriaSet<BaseLoaderTestHelper.DummyBizoForTest> TariffCriteriaSetForTest => new BaseLoaderTestHelper.TariffCriteriaSetForTest(Tariff, Criteria);

		void AssertAddApplicableTableValueParameterRows(ZString message, int expectedCriteriaRowCount, ZGuid nationalTariffPk)
		{
			var loader = new BaseLoaderTestHelper.BaseLoaderForTest(Factory);
			var tvpCriteriaDataTable = loader.GetEmptyCriteriaTable();
			var tvpAdditionalCodesDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptyAdditionalCodesParameterTable();
			var secondTradeGroupsDataTable = BaseLoaderTestHelper.BaseLoaderForTest.GetEmptySecondTradeGroupsParameterTable();
			var tariffCriteriaSet = TariffCriteriaSetForTest;
			tariffCriteriaSet.AddApplicableTableValueParameterRows(tvpCriteriaDataTable, tvpAdditionalCodesDataTable, secondTradeGroupsDataTable);
			var criteriaId = tariffCriteriaSet.CriteriaId;

			CombineAssertions(message, () =>
			{
				AssertEquals("[Tariff is a National Code - should add 2 rows] TVP Criteria Row Count", expectedCriteriaRowCount, tvpCriteriaDataTable.Rows.Count);
				var criteriaRow = tvpCriteriaDataTable.Rows[0];
				AssertEquals("Row 1 - CriteriaId", criteriaId, (Guid)criteriaRow[TvpSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("Row 1 - TariffPK", Tariff.PK, (Guid)criteriaRow[TvpSelectionCriteria.Columns.TariffPK]);

				if (nationalTariffPk.IsValid)
				{
					var criteriaRow1 = tvpCriteriaDataTable.Rows[1];
					AssertEquals("Row 2 - CriteriaId", criteriaId, (Guid)criteriaRow1[TvpSelectionCriteria.Columns.CriteriaId]);
					AssertEquals("Row 2 - TariffPK", nationalTariffPk, (Guid)criteriaRow1[TvpSelectionCriteria.Columns.TariffPK]);
				}

				AssertEquals("[Two AdditionalCodes] TVP AdditionalCodes Row Count", 2, tvpAdditionalCodesDataTable.Rows.Count);
				var additionalCodeRow = tvpAdditionalCodesDataTable.Rows[0];
				AssertEquals("Row 0 - CriteriaId", criteriaId, (Guid)additionalCodeRow[TvpAdditionalCodes.Columns.CriteriaId]);
				AssertEquals("Row 0 - AdditionalCode", "AC1", additionalCodeRow[TvpAdditionalCodes.Columns.AdditionalCode].ToString());
				var additionalCodeRow1 = tvpAdditionalCodesDataTable.Rows[1];
				AssertEquals("Row 1 - CriteriaId", criteriaId, (Guid)additionalCodeRow1[TvpAdditionalCodes.Columns.CriteriaId]);
				AssertEquals("Row 1 - AdditionalCode", "AC2", additionalCodeRow1[TvpAdditionalCodes.Columns.AdditionalCode].ToString());

				AssertEquals("[Two AdditionalCodes] TVP AdditionalCodes Row Count", 2, secondTradeGroupsDataTable.Rows.Count);
				var secondTradeGroupRow = secondTradeGroupsDataTable.Rows[0];
				AssertEquals("Row 0 - CriteriaId", criteriaId, (Guid)secondTradeGroupRow[TvpSecondTradeGroup.Columns.CriteriaId]);
				AssertEquals("Row 0 - SecondTradeGroup", "TR1", secondTradeGroupRow[TvpSecondTradeGroup.Columns.SecondTradeGroup].ToString());
				var secondTradeGroupRow1 = secondTradeGroupsDataTable.Rows[1];
				AssertEquals("Row 1 - CriteriaId", criteriaId, (Guid)secondTradeGroupRow1[TvpSecondTradeGroup.Columns.CriteriaId]);
				AssertEquals("Row 1 - SecondTradeGroup", "TR2", secondTradeGroupRow1[TvpSecondTradeGroup.Columns.SecondTradeGroup].ToString());
			});
		}

		TariffView Tariff => _tariff ?? (_tariff = Factory.New<TariffView>());
		TariffView _tariff;

		IZZApplicabilitySelectionCriteria Criteria => _criteria ?? (_criteria = new BaseLoaderTestHelper.SelectionCriteriaForTest(ZGuid.NewZGuid(), "CN", "PRE", new HashSet<ZString> { "AC1", "AC2", "AC1" }, "ORD", "ZZ", new HashSet<ZString> { "TR1", "TR2", "TR1" }));
		IZZApplicabilitySelectionCriteria _criteria;
	}
}
