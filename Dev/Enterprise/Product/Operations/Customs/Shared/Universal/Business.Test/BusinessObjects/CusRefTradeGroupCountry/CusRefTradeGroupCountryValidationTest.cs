using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class CusRefTradeGroupCountryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCRA_RN_NKTradeGroupCountryCode_DuplicateTradeGroupCountryError()
		{
			this.tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			var tradeGroupCountry = Factory.New<CusRefTradeGroupCountry>();
			tradeGroupCountry.CRA_CR9_TradeGroup = tradeGroup.PK;
			var tradeGroupCountryCodeInfo = tradeGroupCountry.CRA_RN_NKTradeGroupCountryCodeInfo;

			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			AssertNoErrors("No error when startDate or EndDate is empty", tradeGroupCountryCodeInfo);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 17);
			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 27);
			tradeGroupCountry.Validation.ValidateCRA_RN_NKTradeGroupCountryCode();
			AssertHasError("Has error when duplicate", tradeGroupCountryCodeInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "AU";
			AssertNoErrors("No error when not duplicate", tradeGroupCountryCodeInfo);
		}

		public void TestCheckCRA_StartDate_DuplicateTradeGroupCountryError()
		{
			this.tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			this.tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 20);
			this.tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 22);

			var tradeGroupCountry = Factory.New<CusRefTradeGroupCountry>();
			tradeGroupCountry.CRA_CR9_TradeGroup = tradeGroup.PK;
			var startDateInfo = tradeGroupCountry.CRA_StartDateInfo;

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 20);
			AssertNoErrors("No error when TradeGroupCountryCode or EndDate is empty", startDateInfo);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 22);
			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			tradeGroupCountry.Validation.ValidateCRA_StartDate();
			AssertHasError("Has error when duplicate", startDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 27);
			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 17);
			AssertHasError("Has error when overlap", startDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 23);
			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 21);
			AssertHasError("Has error when date overlapped 1", startDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 21);
			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 19);
			AssertHasError("Has error when date overlapped 2", startDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 25);
			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 23);
			AssertNoError("No error when not duplicate and overlap", startDateInfo, duplicateTradeGroupCountryError);
		}

		public void TestCheckCRA_EndDate_DuplicateTradeGroupCountryError()
		{
			this.tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			this.tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 20);
			this.tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 22);

			var tradeGroupCountry = Factory.New<CusRefTradeGroupCountry>();
			tradeGroupCountry.CRA_CR9_TradeGroup = tradeGroup.PK;
			var endDateInfo = tradeGroupCountry.CRA_EndDateInfo;

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 22);
			AssertNoErrors("No error when TradeGroupCountryCode or StartDate is empty", endDateInfo);

			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 20);
			tradeGroupCountry.Validation.ValidateCRA_EndDate();
			AssertHasError("Has error when duplicate", endDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 17);
			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 27);
			AssertHasError("Has error when overlap", endDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 21);
			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 23);
			AssertHasError("Has error when date overlapped 1", endDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 19);
			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 21);
			AssertHasError("Has error when date overlapped 2", endDateInfo, duplicateTradeGroupCountryError);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 23);
			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 24);
			AssertNoError("No error when not duplicate and overlap", endDateInfo, duplicateTradeGroupCountryError);
		}

		static readonly string duplicateTradeGroupCountryError = "The trade group country should be unique in the trade group within date range.";

		public void TestCheckCRA_RN_NKTradeGroupCountryCode()
		{
			var info = tradeGroupCountry.CRA_RN_NKTradeGroupCountryCodeInfo;

			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "";
			AssertHasErrorContaining(info, MandatoryValidation.MustBeEntered);

			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "C";
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);
			AssertNoError(info, MandatoryValidation.MustBeEntered);

			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			AssertNoError(info, MandatoryValidation.MustBeEntered);
			AssertNoError(info, ListValidation.InvalidCodeError);
		}

		public void TestCheckCRA_StartDate()
		{
			var info = tradeGroupCountry.CRA_StartDateInfo;
			var errorMsg = "The start date of the trade group country must not be earlier than trade group start date and later than its end date.";
			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 16);
			AssertHasError(info, errorMsg);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 28);
			AssertHasError(info, errorMsg);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 20);
			AssertNoError(info, errorMsg);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 19);
			tradeGroupCountry.Validation.ValidateCRA_StartDate();
			AssertHasError(info, errorMsg);

			tradeGroupCountry.CRA_EndDate = ZDate.Empty;
			tradeGroupCountry.Validation.ValidateCRA_StartDate();
			AssertNoError(info, errorMsg);

			tradeGroup.CR9_StartDate = ZDate.Today.AddYears(-11);
			tradeGroupCountry.CRA_EndDate = ZDate.Today.AddYears(6);
			tradeGroupCountry.CRA_StartDate = ZDate.Today.AddYears(5).AddDays(1);
			AssertNoErrors(tradeGroupCountry.CRA_StartDateInfo);

			tradeGroupCountry.CRA_StartDate = ZDate.Today.AddYears(1).AddDays(1);
			AssertNoWarnings(tradeGroupCountry.CRA_StartDateInfo);

			tradeGroupCountry.CRA_StartDate = ZDate.Today.AddYears(-10).AddDays(-1);
			AssertNoErrors(tradeGroupCountry.CRA_StartDateInfo);

			tradeGroupCountry.CRA_StartDate = ZDate.Today.AddYears(-1).AddDays(-1);
			AssertNoWarnings(tradeGroupCountry.CRA_StartDateInfo);
		}

		public void TestCheckCRA_EndDate()
		{
			var info = tradeGroupCountry.CRA_EndDateInfo;
			var errorMsg = "The end date of the trade group country must not be later than trade group end date and earlier than its start date.";
			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 16);
			AssertHasError(info, errorMsg);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 28);
			AssertHasError(info, errorMsg);

			tradeGroupCountry.CRA_EndDate = new ZDate(2020, 11, 20);
			AssertNoError(info, errorMsg);

			tradeGroupCountry.CRA_StartDate = new ZDate(2020, 11, 21);
			tradeGroupCountry.Validation.ValidateCRA_EndDate();
			AssertHasError(info, errorMsg);

			tradeGroupCountry.CRA_StartDate = ZDate.Empty;
			tradeGroupCountry.Validation.ValidateCRA_EndDate();
			AssertNoError(info, errorMsg);

			tradeGroup.CR9_EndDate = ZDate.Today.AddYears(6);
			tradeGroupCountry.CRA_StartDate = ZDate.Today.AddYears(-11);
			tradeGroupCountry.CRA_EndDate = ZDate.Today.AddYears(5).AddDays(1);
			AssertNoErrors(tradeGroupCountry.CRA_EndDateInfo);

			tradeGroupCountry.CRA_EndDate = ZDate.Today.AddYears(1).AddDays(1);
			AssertNoWarnings(tradeGroupCountry.CRA_EndDateInfo);

			tradeGroupCountry.CRA_EndDate = ZDate.Today.AddYears(-10).AddDays(-1);
			AssertNoErrors(tradeGroupCountry.CRA_EndDateInfo);

			tradeGroupCountry.CRA_EndDate = ZDate.Today.AddYears(-1).AddDays(-1);
			AssertNoWarnings(tradeGroupCountry.CRA_EndDateInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_StartDate = new ZDate(2020, 11, 17);
			tradeGroup.CR9_EndDate = new ZDate(2020, 11, 27);

			tradeGroupCountry = tradeGroup.TradeGroupCountries.AddNew();
		}

		CusRefTradeGroup tradeGroup;
		CusRefTradeGroupCountry tradeGroupCountry;
	}
}
