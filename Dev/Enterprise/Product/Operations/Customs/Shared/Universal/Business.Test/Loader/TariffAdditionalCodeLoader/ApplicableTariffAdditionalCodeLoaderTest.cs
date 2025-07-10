using System;

using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class ApplicableTariffAdditionalCodeLoaderTest : TestCaseWithFactory
	{
		public void TestGetEmptyCriteriaTable()
		{
			var expectedCriteriaTableColumns = new[]
			{
				TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId,
				TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK,
				TvpTariffAdditionalCodeSelectionCriteria.Columns.Category,
				TvpTariffAdditionalCodeSelectionCriteria.Columns.EffectiveDate,
				TvpTariffAdditionalCodeSelectionCriteria.Columns.TradeGroupCountry,
				TvpTariffAdditionalCodeSelectionCriteria.Columns.DataGrouping,
			};

			var tvpDataTable = ApplicableTariffAdditionalCodeLoader.GetEmptyCriteriaTable();

			CombineAssertions(() =>
			{
				AssertNotNull("Created TVP_TariffAdditionalCodeSelectionCriteria Data Table should not be null.", tvpDataTable);
				AssertEquals("TVP Row Count should be 0 for an empty table.", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(expectedCriteriaTableColumns, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		public void TestAddNewCriteriaSetRow()
		{
			var tvpCriteriaTable = ApplicableTariffAdditionalCodeLoader.GetEmptyCriteriaTable();

			AssertEquals("TVP initial row count", 0, tvpCriteriaTable.Rows.Count);
			var criteriaId = Guid.NewGuid();
			var tariffPk = ZGuid.NewZGuid();
			var selectionCriteria = new TariffAdditionalCodeSelectionCriteria("SEP", new ZDateTime(2024, 6, 30), "AU", "FR");
			ApplicableTariffAdditionalCodeLoader.AddNewCriteriaSetRow(tvpCriteriaTable, criteriaId, tariffPk, selectionCriteria);

			CombineAssertions("Row 0", () =>
			{
				AssertEquals("tvpCriteriaTable row count", 1, tvpCriteriaTable.Rows.Count);

				var row = tvpCriteriaTable.Rows[0];
				AssertEquals("CriteriaId", criteriaId, (Guid)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("TariffPK", tariffPk, (Guid)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK]);
				AssertEquals("Category", "SEP", row[TvpTariffAdditionalCodeSelectionCriteria.Columns.Category].ToString());
				AssertEquals("EffectiveDate", new DateTime(2024, 6, 30), (DateTime)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.EffectiveDate]);
				AssertEquals("TradeGroupCountry", "AU", row[TvpTariffAdditionalCodeSelectionCriteria.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", "FR", row[TvpTariffAdditionalCodeSelectionCriteria.Columns.DataGrouping].ToString());
			});
		}

		[TestDate(2024, 6, 30, 15, 24, 33)]
		public void TestAddNewCriteriaSetRow_EmptyCriteria()
		{
			var tvpCriteriaTable = ApplicableTariffAdditionalCodeLoader.GetEmptyCriteriaTable();
			var selectionCriteria = new TariffAdditionalCodeSelectionCriteria(ZString.Empty, ZDateTime.Empty, "", "");
			ApplicableTariffAdditionalCodeLoader.AddNewCriteriaSetRow(tvpCriteriaTable, Guid.Empty, ZGuid.BrettsGuid, selectionCriteria);

			CombineAssertions("Row 1", () =>
			{
				AssertEquals("TVP row count", 1, tvpCriteriaTable.Rows.Count);

				var row = tvpCriteriaTable.Rows[0];
				AssertEquals("CriteriaId", Guid.Empty, (Guid)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("TariffPK", ZGuid.BrettsGuid, (Guid)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK]);
				AssertEquals("Category", ZString.Empty, row[TvpTariffAdditionalCodeSelectionCriteria.Columns.Category].ToString());
				AssertEquals("EffectiveDate", new DateTime(2024, 6, 30, 00, 00, 00), (DateTime)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.EffectiveDate]);
				AssertEquals("TradeGroupCountry", ZString.Empty, row[TvpTariffAdditionalCodeSelectionCriteria.Columns.TradeGroupCountry].ToString());
				AssertEquals("DataGrouping", ZString.Empty, row[TvpTariffAdditionalCodeSelectionCriteria.Columns.DataGrouping].ToString());
			});
		}

		public void TestLoadTariffAdditionalCodesForSingleCriteriaSet()
		{
			SetupTestData();

			var selectionCriteria = new TariffAdditionalCodeSelectionCriteria("SEP", new DateTime(2024, 6, 30), "AU", "FR");
			var tariffCriteriaSet = new TariffAdditionalCodeLoadTariffCriteriaSet(tariff, selectionCriteria);
			AssertEquals("[PRE-CONDITION] Criteria set should not be cached before 1st loading.", false, tariffCriteriaSet.IsCached());

			var loader = new ApplicableTariffAdditionalCodeLoader(Factory);
			var loadedTariffAdditionalCodes = loader.LoadTariffAdditionalCodesForSingleCriteriaSet(tariffCriteriaSet);

			CombineAssertions("[After 1st Load]", () =>
			{
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet.IsCached());
				AssertEquals("Loaded tariff additional codes count", 2, loadedTariffAdditionalCodes.Count());
				AssertContainsExactElementsInAnyOrder("Loaded tariff additional codes", new string[] { "SEP1", "SEP2" }, loadedTariffAdditionalCodes.Select(x => x.ZY2_AdditionalCode));
			});
		}

		public void TestLoadRatesForMultipleCriteriaSets()
		{
			SetupTestData();
			var selectionCriteria1 = new TariffAdditionalCodeSelectionCriteria("SEP", new DateTime(2024, 6, 30), "AU", "FR");
			var selectionCriteria2 = new TariffAdditionalCodeSelectionCriteria("SEP", new DateTime(1900, 6, 30), "AU", "FR");
			var selectionCriteria3 = new TariffAdditionalCodeSelectionCriteria("SEP", new DateTime(2024, 6, 30), "LV", "FR");
			var selectionCriteria4 = new TariffAdditionalCodeSelectionCriteria("SIP", new DateTime(2024, 6, 30), "AU", "FR");
			var selectionCriteria5 = new TariffAdditionalCodeSelectionCriteria("SEP", new DateTime(2024, 6, 30), "ER", "IT");
			var tariffCriteriaSet1 = new TariffAdditionalCodeLoadTariffCriteriaSet(tariff, selectionCriteria1);
			var tariffCriteriaSet2 = new TariffAdditionalCodeLoadTariffCriteriaSet(tariff, selectionCriteria2);
			var tariffCriteriaSet3 = new TariffAdditionalCodeLoadTariffCriteriaSet(tariff, selectionCriteria3);
			var tariffCriteriaSet4 = new TariffAdditionalCodeLoadTariffCriteriaSet(tariff, selectionCriteria4);
			var tariffCriteriaSet5 = new TariffAdditionalCodeLoadTariffCriteriaSet(tariff, selectionCriteria5);
			CombineAssertions("[PRE-CONDITION] Before Loading", () =>
			{
				AssertEquals("Criteria set should not be cached before 1st loading.", false, tariffCriteriaSet1.IsCached());
				AssertEquals("Criteria set should not be cached before 1st loading", false, tariffCriteriaSet2.IsCached());
				AssertEquals("Criteria set should not be cached before 1st loading", false, tariffCriteriaSet3.IsCached());
				AssertEquals("Criteria set should not be cached before 1st loading", false, tariffCriteriaSet4.IsCached());
				AssertEquals("Criteria set should not be cached before 1st loading", false, tariffCriteriaSet5.IsCached());
			});

			var loader = new ApplicableTariffAdditionalCodeLoader(Factory);
			var loadedTariffAdditionalCodes = loader.LoadTariffAdditionalCodesForMultipleCriteriaSets(new TariffAdditionalCodeLoadTariffCriteriaSet[] { tariffCriteriaSet1, tariffCriteriaSet2, tariffCriteriaSet3, tariffCriteriaSet4 });
			CombineAssertions("[After 1st Load]", () =>
			{
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet1.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet2.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet3.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet4.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", false, tariffCriteriaSet5.IsCached());
				AssertContainsExactElementsInAnyOrder("Loaded tariff additional codes (SEP5 and SEP6 skipped - SEP5 has no matching criteria; SEP6 can't have matching criteria because it has no applicability).", new string[] { "SEP1", "SEP2", "SEP3", "SEP4", "SIP1" }, loadedTariffAdditionalCodes.Select(x => x.ZY2_AdditionalCode));
			});

			loadedTariffAdditionalCodes = loadedTariffAdditionalCodes = loader.LoadTariffAdditionalCodesForMultipleCriteriaSets(new TariffAdditionalCodeLoadTariffCriteriaSet[] { tariffCriteriaSet1, tariffCriteriaSet2, tariffCriteriaSet3, tariffCriteriaSet4, tariffCriteriaSet5 });
			CombineAssertions("[After 2nd Load]", () =>
			{
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet1.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet2.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet3.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet4.IsCached());
				AssertEquals("Criteria set should be cached after 1st loading.", true, tariffCriteriaSet5.IsCached());
				AssertContainsExactElementsInAnyOrder("Loaded tariff additional codes (SEP6 can't have matching criteria because it has no applicability).", new string[] { "SEP1", "SEP2", "SEP3", "SEP4", "SIP1", "SEP5" }, loadedTariffAdditionalCodes.Select(x => x.ZY2_AdditionalCode));
			});
		}

		void SetupTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var startDate = new DateTime(2024, 1, 1);
			var endDate = new DateTime(2024, 12, 31);

			var eunDataGroupingPK = helper.CreateNewOrGetExistingDataGrouping("EUN", "Eun");
			helper.CreateNewOrGetExistingDataGrouping("FR", "France", eunDataGroupingPK);
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", eunDataGroupingPK);

			var tariffType = helper.CreateNewOrGetExistingTariffType("EUN", "IMP");
			Factory.Save();

			tariff = helper.CreateTariff("EUN", tariffType.PK, "0000000001", startDate, endDate);

			var fr01tradeGroup = helper.LoadOrCreateTradeGroup("FR", "FR01");
			var fr02tradeGroup = helper.LoadOrCreateTradeGroup("FR", "FR02");
			var it01tradeGroup = helper.LoadOrCreateTradeGroup("IT", "IT01");

			helper.AddCountry(fr01tradeGroup, "AU");
			helper.AddCountry(fr02tradeGroup, "LV");
			helper.AddCountry(it01tradeGroup, "ER");

			var sepCategory = helper.CreateNewOrGetExistingTariffAdditionalCodeCategory("FR", "SEP", "Subdivision statistique à l'exportation");
			var sipCategory = helper.CreateNewOrGetExistingTariffAdditionalCodeCategory("FR", "SIP", "Subdivision statistique à l'importation");

			var tariffAdditionalCode1  = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(Factory, false, tariff.PK, "FR", sepCategory.ZY3_Category, "SEP1", "SEP/FR/FR01/Effective in 2024");
			var tariffAdditionalCode2 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(Factory, false, tariff.PK, "FR", sepCategory.ZY3_Category, "SEP2", "SEP/FR/FR01/Effective in 2024");
			var tariffAdditionalCode3 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(Factory, false, tariff.PK, "FR", sepCategory.ZY3_Category, "SEP3", "SEP/FR/FR01/Effective in 1900");
			var tariffAdditionalCode4 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(Factory, false, tariff.PK, "FR", sepCategory.ZY3_Category, "SEP4", "SEP/FR/FR02/Effective in 2024");
			var tariffAdditionalCode5 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(Factory, false, tariff.PK, "FR", sipCategory.ZY3_Category, "SIP1", "SIP/FR/FR01/Effective in 2024");
			var tariffAdditionalCode6 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(Factory, false, tariff.PK, "IT", sepCategory.ZY3_Category, "SEP5", "SEP/IT/IT01/Effective in 2024");
			var tariffAdditionalCode7 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(Factory, false, tariff.PK, "FR", sepCategory.ZY3_Category, "SEP6", "SEP/FR/FR01/No applicability");
			Factory.Save();

			helper.CreateCusApplicability(tariffAdditionalCode1, fr01tradeGroup, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
			helper.CreateCusApplicability(tariffAdditionalCode2, fr01tradeGroup, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
			helper.CreateCusApplicability(tariffAdditionalCode3, fr01tradeGroup, new DateTime(1900, 1, 1), new DateTime(1900, 12, 31));
			helper.CreateCusApplicability(tariffAdditionalCode4, fr02tradeGroup, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
			helper.CreateCusApplicability(tariffAdditionalCode5, fr01tradeGroup, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
			helper.CreateCusApplicability(tariffAdditionalCode6, it01tradeGroup, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
			Factory.Save();
		}

		TariffView tariff;
	}
}
