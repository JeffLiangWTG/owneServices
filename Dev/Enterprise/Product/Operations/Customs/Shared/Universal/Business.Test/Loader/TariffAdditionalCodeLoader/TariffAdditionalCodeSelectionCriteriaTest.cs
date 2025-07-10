using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing.Helper;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class TariffAdditionalCodeSelectionCriteriaTest : TestCase
	{
		public void TestZZConditionSelectionCriteria()
		{
			var dateTime = new ZDateTime(2024, 2, 29);
			var critieria = new TariffAdditionalCodeSelectionCriteria("SEP", dateTime, "AU", "FR");
			CombineAssertions(() =>
			{
				AssertEquals("Category", "SEP", critieria.Category);
				AssertEquals("EffectiveDate", dateTime, critieria.EffectiveDate);
				AssertEquals("TradeGroupCountry", "AU", critieria.TradeGroupCountry);
				AssertEquals("DataGroupîng", "FR", critieria.DataGrouping);
			});
		}

		public void TestTypesMatch_TVP_TariffAdditionalCodeSelectionCriteria_ColumnDefinition()
		{
			var columnInfos = TVPTestHelper.GetTableTypesColumnInfoFromDb(TvpTariffAdditionalCodeSelectionCriteria.QualifiedName).ToDictionary(x => x.ColumnName);

			CombineAssertions(() =>
			{
				AssertEquals("TVP column Count", 6, columnInfos.Count);

				var columnCategory = columnInfos[TvpTariffAdditionalCodeSelectionCriteria.Columns.Category];
				var zzColumnCategory = RefCusTariffAdditionalCodeSchema.ZY2_ZY3_NKCategory;
				AssertEquals("Category.DataType", zzColumnCategory.SqlDbType.ToString().ToUpper(), columnCategory.DataType.ToUpper());
				AssertEquals("Category.MaxLength", zzColumnCategory.MaxLength, columnCategory.MaxLength);

				var columnTradeGroupCountry = columnInfos[TvpTariffAdditionalCodeSelectionCriteria.Columns.TradeGroupCountry];
				var zzColumnTradeGroupCountry = RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode;
				AssertEquals("TradeGroupCountry.DataType", zzColumnTradeGroupCountry.SqlDbType.ToString().ToUpper(), columnTradeGroupCountry.DataType.ToUpper());
				AssertEquals("TradeGroupCountry.MaxLength", zzColumnTradeGroupCountry.MaxLength, columnTradeGroupCountry.MaxLength);

				var columnDataGrouping = columnInfos[TvpTariffAdditionalCodeSelectionCriteria.Columns.DataGrouping];
				var zzColumnDataGrouping = RefDataGroupingSchema.ZZZ_DataGrouping;
				AssertEquals("DataGrouping.DataType", zzColumnDataGrouping.SqlDbType.ToString().ToUpper(), columnDataGrouping.DataType.ToUpper());
				AssertEquals("DataGrouping.MaxLength", zzColumnDataGrouping.MaxLength, columnDataGrouping.MaxLength);
			});
		}
		public void TestValidEffectiveDate()
		{
			CombineAssertions(() =>
			{
				var validDate = ZDateTime.Today.AddMonths(-1);
				var criteriaSetWithValidDate = new TariffAdditionalCodeSelectionCriteria("", validDate, "", "");
				AssertEquals("ValidEffectiveDate with a valid date", validDate, criteriaSetWithValidDate.ValidEffectiveDate());
				AssertEquals("EffectiveDate with a valid date", validDate, criteriaSetWithValidDate.EffectiveDate);
				var invalidDate = ZDateTime.Invalid;
				var criteriaSetWithInvalidDate = new TariffAdditionalCodeSelectionCriteria("", invalidDate, "", "");
				AssertEquals("ValidEffectiveDate with an invalid date", ZDateTime.Today, criteriaSetWithInvalidDate.ValidEffectiveDate());
				AssertEquals("EffectiveDate with an invalid date", invalidDate, criteriaSetWithInvalidDate.EffectiveDate);
			});
		}
	}
}
