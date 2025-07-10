using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefTradeGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCR9_RN_NKCountryCode()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_RN_NKCountryCode = "T";
			AssertHasErrorContaining(tradeGroup.CR9_RN_NKCountryCodeInfo, ListValidation.InvalidCodeError);
			tradeGroup.CR9_RN_NKCountryCode = "XX";
			AssertHasErrorContaining(tradeGroup.CR9_RN_NKCountryCodeInfo, ListValidation.InvalidCodeError);
			tradeGroup.CR9_RN_NKCountryCode = "CN";
			AssertNoErrors(tradeGroup.CR9_RN_NKCountryCodeInfo);
		}

		public void TestCheckCR9_TradeGroup()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_TradeGroup = "";
			AssertHasErrorContaining(tradeGroup.CR9_TradeGroupInfo, MandatoryValidation.MustBeEntered);
			tradeGroup.CR9_TradeGroup = "T012345678";
			AssertNoError(tradeGroup.CR9_TradeGroupInfo, MandatoryValidation.MustBeEntered);
			var msgTradeGroupIncludeOneTradeGroupCountryAtLeast = "The trade group must have one trade group country at least.";
			AssertHasErrorContaining(tradeGroup.CR9_TradeGroupInfo, msgTradeGroupIncludeOneTradeGroupCountryAtLeast);
			tradeGroup.TradeGroupCountries.AddNew();
			tradeGroup.Validation.ValidateCR9_TradeGroup();
			AssertNoError(tradeGroup.CR9_TradeGroupInfo, msgTradeGroupIncludeOneTradeGroupCountryAtLeast);
		}

		public void TestCheckCR9_Description()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_Description = "";
			AssertHasErrorContaining(tradeGroup.CR9_DescriptionInfo, MandatoryValidation.MustBeEntered);
			tradeGroup.CR9_Description = new string('T', 100);
			AssertNoErrors(tradeGroup.CR9_DescriptionInfo);
		}

		public void TestCheckCR9_EndDate()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_StartDate = new ZDate(2020, 11, 17);
			tradeGroup.CR9_EndDate = new ZDate(2020, 11, 16);
			AssertHasError(tradeGroup.CR9_EndDateInfo, "The end date of trade group must not be earlier than its start date.");
			tradeGroup.CR9_EndDate = new ZDate(2020, 11, 18);
			AssertNoErrors(tradeGroup.CR9_EndDateInfo);
			tradeGroup.CR9_EndDate = ZDate.Empty;
			AssertHasError(tradeGroup.CR9_EndDateInfo, "Please enter an End Date.");
			tradeGroup.CR9_StartDate = ZDate.Empty;
			tradeGroup.CR9_EndDate = ZDate.Today.AddYears(5).AddDays(1);
			AssertNoErrors(tradeGroup.CR9_EndDateInfo);
			tradeGroup.CR9_EndDate = ZDate.Today.AddYears(1).AddDays(1);
			AssertNoWarnings(tradeGroup.CR9_EndDateInfo);
			tradeGroup.CR9_EndDate = ZDate.Today.AddYears(-10).AddDays(-1);
			AssertNoErrors(tradeGroup.CR9_EndDateInfo);
			tradeGroup.CR9_EndDate = ZDate.Today.AddYears(-1).AddDays(-1);
			AssertNoWarnings(tradeGroup.CR9_EndDateInfo);
		}

		public void TestCheckCR9_StartDate()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_StartDate = ZDate.Today.AddYears(5).AddDays(1);
			AssertNoErrors(tradeGroup.CR9_StartDateInfo);
			tradeGroup.CR9_StartDate = ZDate.Today.AddYears(1).AddDays(1);
			AssertNoWarnings(tradeGroup.CR9_StartDateInfo);
			tradeGroup.CR9_StartDate = ZDate.Today.AddYears(-10).AddDays(-1);
			AssertNoErrors(tradeGroup.CR9_StartDateInfo);
			tradeGroup.CR9_StartDate = ZDate.Today.AddYears(-1).AddDays(-1);
			AssertNoWarnings(tradeGroup.CR9_StartDateInfo);
		}
	}
}
