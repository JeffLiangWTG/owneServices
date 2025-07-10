using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefExchangeRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWarningSystemRateTypes()
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM RefDatabase_RefExchangeRateZZ");
			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO RefDatabase_RefExchangeRateZZ (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished)
VALUES (newid(), 'CUS', '2017-01-11 00:00:00', '2017-01-11 00:00:00', 0.072090000, 'USD', '{0}', '')", GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var exRate = RefExchangeRate.New(Factory);
			exRate.RE_RX_NKExCurrency = "USD";
			exRate.RE_ExRateType = "CUS";
			AssertHasWarning(exRate.RE_ExRateTypeInfo, "Rate Type 'CUS' is being managed by the system. It might be replaced by the system rate once the system is updated with new rates.");
			exRate.RE_ExRateType = "BUY";
			AssertNoWarnings(exRate.RE_ExRateTypeInfo);
		}

		[TestDate(2004, 11, 30)]
		public void TestValidateRE_ExpiryDate()
		{
			ExchangeRate.RE_ExpiryDate = ZDateTime.Empty;
			ExchangeRate.RE_StartDate = new ZDateTime(2003, 11, 1);
			Assert("Expecting RE_ExpiryDate to be empty and have errors.", ExchangeRate.RE_ExpiryDateInfo.HasErrors());
			ExchangeRate.RE_StartDate = new ZDateTime(2003, 12, 1);
			ExchangeRate.RE_ExpiryDate = new ZDateTime(2003, 11, 30);
			Assert("Expecting RE_ExpiryDate to have errors because the expiry date occurs before the start date.", ExchangeRate.RE_ExpiryDateInfo.HasErrors());
			ExchangeRate.RE_StartDate = new ZDateTime(2003, 12, 1);
			ExchangeRate.RE_ExpiryDate = new ZDateTime(2003, 12, 1);
			Assert("Expecting RE_ExpiryDate should be correct, not expecting errors.", !ExchangeRate.RE_ExpiryDateInfo.HasNotifications());

			ExchangeRate.RE_ExpiryDate = new ZDateTime(2003, 12, 31);
			Assert("RE_ExpiryDate should be correct, not expecting errors.", !ExchangeRate.RE_ExpiryDateInfo.HasNotifications());

			RefCurrency currency = Factory.New<RefCurrency>();

			AssertEquals("Currency should have 0 exchange rates.", 0, currency.ExchangeRates.Count);
			currency.RX_Code = "XXX";

			RefExchangeRate rate1 = currency.ExchangeRates.AddNew();
			rate1.RE_StartDate = new ZDateTime(2003, 12, 1);
			rate1.RE_ExpiryDate = new ZDateTime(2003, 12, 1);
			rate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate1.RE_SellRate = 1.2003m;

			RefExchangeRate rate2 = currency.ExchangeRates.AddNew();
			rate2.RE_StartDate = new ZDateTime(2003, 12, 2);
			rate2.RE_ExpiryDate = new ZDateTime(2003, 12, 4);
			rate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate2.RE_SellRate = 1.2654m;

			RefExchangeRate rate3 = currency.ExchangeRates.AddNew();
			rate3.RE_StartDate = new ZDateTime(2003, 12, 5);
			rate3.RE_ExpiryDate = new ZDateTime(2003, 12, 6);
			rate3.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate3.RE_SellRate = 1.315m;

			RefExchangeRate rate4 = currency.ExchangeRates.AddNew();
			rate4.RE_StartDate = new ZDateTime(2003, 12, 1);
			rate4.RE_ExpiryDate = new ZDateTime(2003, 12, 8);
			rate4.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rate4.RE_SellRate = 1.315m;

			AssertEquals("Currency should have 4 exchange rates.", 4, currency.ExchangeRates.Count);

			RefExchangeRate rate5 = currency.ExchangeRates.AddNew();
			rate5.RE_StartDate = new ZDateTime(2003, 12, 3);
			rate5.RE_ExpiryDate = new ZDateTime(2003, 12, 8);
			rate5.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate5.RE_SellRate = 1.2890m;

			Assert("RE_ExpiryDate overlaps with other dates, expecting errors.", rate5.RE_ExpiryDateInfo.HasErrors());

			rate5.RE_StartDate = new ZDateTime(2003, 12, 7);
			rate5.RE_ExpiryDate = new ZDateTime(2003, 12, 10);
			rate5.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			Assert("RE_ExpiryDate overlaps with other dates, expecting errors.", rate5.RE_ExpiryDateInfo.HasErrors());

			rate4.RE_IsSystem = true;
			rate5.Validation.ValidateRE_ExpiryDate();
			Assert("RE_ExpiryDate is before start date and overlaps with other dates, expecting errors.", !rate5.RE_ExpiryDateInfo.HasErrors());

			rate5.RE_StartDate = new ZDateTime(2003, 12, 7);
			rate5.RE_ExpiryDate = new ZDateTime(2003, 12, 3);
			Assert("RE_ExpiryDate is before start date and overlaps with other dates, expecting errors.", rate5.RE_ExpiryDateInfo.HasErrors());

			rate4.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate5.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate5.RE_StartDate = new ZDateTime(2003, 12, 7);
			rate5.RE_ExpiryDate = new ZDateTime(2003, 12, 10);
			Assert("RE_ExpiryDate is before start date and overlaps with other dates, expecting errors.", rate5.RE_ExpiryDateInfo.HasErrors());

			rate5.RE_StartDate = new ZDateTime(2003, 12, 7);
			rate5.RE_ExpiryDate = new ZDateTime(2003, 12, 8);
			rate5.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			Assert("RE_ExpiryDate should be correct, not expecting errors.", !rate5.RE_ExpiryDateInfo.HasErrors());
		}

		public void TestValidateRE_SellRate()
		{
			ExchangeRate.RE_SellRate = 0;
			Assert("Expecting RE_SellRate to be empty and have errors.", ExchangeRate.RE_SellRateInfo.HasErrors());
			ExchangeRate.RE_SellRate = new ZDecimal(2.003);
			Assert("RE_SellRate should be correct, not expecting errors.", !ExchangeRate.RE_SellRateInfo.HasNotifications());
		}

		public void TestValidateRE_ExRateTypeForGlobalCreditWhenUserCanEdit()
		{
			Env.Security.GCBExchangeRateUpdate.IsAllowed = true;
			RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			AssertNoErrors("User should be able to set rate type to GCB when they have security", exchangeRate.RE_ExRateTypeInfo);
		}

		public void TestValidateRE_ExRateTypeForGlobalCreditWhenUserCannotEdit()
		{
			Env.Security.GCBExchangeRateUpdate.IsAllowed = false;
			RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			AssertHasErrors("User should not be able to set rate type to GCB when they don't have security", exchangeRate.RE_ExRateTypeInfo);
		}

		public void TestValidateRE_ExRateTypeWhenWhenUserCannotEditCustomsExchangeRates()
		{
			Env.Security.CustomsExchangeRateUpdate.IsAllowed = true;
			RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			AssertNoErrors(exchangeRate.RE_ExRateTypeInfo);
		}

		public void TestValidateRE_ExRateTypeWhenWhenUserCanEditCustomsExchangeRates()
		{
			Env.Security.CustomsExchangeRateUpdate.IsAllowed = false;
			RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			AssertHasErrors(exchangeRate.RE_ExRateTypeInfo);
		}

		public void TestValidateRE_ExRateTypeWhenLoadingExistingObjectAndUserCannotEditCustomsExchangeRates()
		{
			Env.Security.CustomsExchangeRateUpdate.IsAllowed = false;
			RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.OnLoaded();
			exchangeRate.Validation.ValidateRE_ExRateType();
			AssertNoErrors(exchangeRate.RE_ExRateTypeInfo);
		}

		public void TestNoDateRangeValidationOnCustomsRateExpiryDates()
		{
			RefExchangeRate rate = RefExchangeRate.New(Factory);
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rate.RE_ExpiryDate = ZDateTime.Now.AddYears(20);
			AssertHasErrors("Should be an error on expiry date because it is user input", rate.RE_ExpiryDateInfo);

			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.Validation.ValidateRE_ExpiryDate();
			AssertNoErrors("Shouldn't be an error on expiry date for customs rates because they come from customs and are perpetual until the next update", rate.RE_ExpiryDateInfo);
		}

		public void TestCheckRE_RX_NKExCurrency_MandatoryValidation()
		{
			var exRate = Factory.New<RefExchangeRate>();

			exRate.RE_RX_NKExCurrency = ZString.Empty;
			AssertHasErrorContaining(exRate.RE_RX_NKExCurrencyInfo, MandatoryValidation.MustBeEntered);

			exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Ethiopia;
			AssertNoErrorContaining(exRate.RE_RX_NKExCurrencyInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckRE_RX_NKExCurrency_ListValidation()
		{
			var exRate = Factory.New<RefExchangeRate>();

			exRate.RE_RX_NKExCurrency = "XXX";
			AssertHasErrorContaining(exRate.RE_RX_NKExCurrencyInfo, ListValidation.InvalidCodeError);

			exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Ethiopia;
			AssertNoErrorContaining(exRate.RE_RX_NKExCurrencyInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckRE_RX_NKExCurrency_SameAsLoggedInCompanyCurrency()
		{
			var exRate = Factory.New<RefExchangeRate>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var validationMessage = "Currency for exchange rate should not be the same as Currency for logged in Company";
				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertHasWarning(exRate.RE_RX_NKExCurrencyInfo, validationMessage);

				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Ethiopia;
				AssertNoWarning(exRate.RE_RX_NKExCurrencyInfo, validationMessage);

				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertHasError(exRate.RE_RX_NKExCurrencyInfo, validationMessage);

				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Ethiopia;
				AssertNoError(exRate.RE_RX_NKExCurrencyInfo, validationMessage);

				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertHasError(exRate.RE_RX_NKExCurrencyInfo, validationMessage);

				exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Ethiopia;
				AssertNoError(exRate.RE_RX_NKExCurrencyInfo, validationMessage);
			}
		}

		public void TestCheckRE_OH_Client()
		{
			CombineAssertions(() =>
			{
				ExchangeRate.RE_OH_Client = ZGuid.Empty;
				AssertNoNotifications(ExchangeRate.RE_OH_ClientInfo);

				ExchangeRate.RE_OH_Client = ZGuid.BrettsGuid;
				Factory.ClearCachedValue<bool>("IsLocalClientExchangeRatefieldNeeded");

				var exRate = RefExchangeRate.New(Factory);
				exRate.Validation.ValidateAll();
				AssertHasWarningContaining(exRate.RE_OH_ClientInfo, "This functionality is being phased out. Instead, record the client’s preferred Ex Rate Type on the AR > Invoicing > Job Billing Exchange Rates tab. Additional exchange rate types can be created in the Registry: Accounting > Custom Exchange Rate Types.");
			});
		}

		public void TestCheckRE_AsPublished()
		{
			var plFormatErrorMessage = "Invalid value. Please insert correct decimal value or leave this field empty.\r\nMake sure using ',' (COMMA) instead of '.' (DOT) for decimal separator.";
			RefExchangeRate rate = RefExchangeRate.New(Factory);
			rate.Company.SetCountry(Core.Constants.CountryCodes.Poland);
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_SellRate = new ZDecimal(2.003m);

			rate.RE_AsPublished = "ABCDEFGH";
			AssertHasMessageErrors(plFormatErrorMessage, rate.RE_AsPublishedInfo);
			rate.RE_AsPublished = "ABCD,0432";
			AssertHasMessageErrors(plFormatErrorMessage, rate.RE_AsPublishedInfo);
			rate.RE_AsPublished = "A,0432";
			AssertHasMessageErrors(plFormatErrorMessage, rate.RE_AsPublishedInfo);

			rate.RE_AsPublished = "2,0";
			AssertHasWarnings("Invalid As Published value. Correct value should contain multiplied exchange rate by 1/100/10000", rate.RE_AsPublishedInfo);
			rate.RE_AsPublished = "0,2003";
			AssertHasWarnings("Invalid As Published value. Correct value should contain multiplied exchange rate by 1/100/10000", rate.RE_AsPublishedInfo);
			rate.RE_AsPublished = "20,03";
			AssertHasWarnings("Invalid As Published value. Correct value should contain multiplied exchange rate by 1/100/10000", rate.RE_AsPublishedInfo);
			rate.RE_AsPublished = "2003,0";
			AssertHasWarnings("Invalid As Published value. Correct value should contain multiplied exchange rate by 1/100/10000", rate.RE_AsPublishedInfo);

			rate.RE_AsPublished = "2,003";
			AssertNoNotifications(rate.RE_AsPublishedInfo);
			rate.RE_AsPublished = "200,3";
			AssertNoNotifications(rate.RE_AsPublishedInfo);
			rate.RE_AsPublished = "20030,0";
			AssertNoNotifications(rate.RE_AsPublishedInfo);

			rate.RE_AsPublished = string.Empty;
			AssertNoNotifications(rate.RE_AsPublishedInfo);

			rate.Company.SetCountry(Core.Constants.CountryCodes.Denmark);
			rate.RE_AsPublished = "ABCDEFGH";
			AssertNoNotifications(rate.RE_AsPublishedInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ExchangeRate = RefExchangeRate.New(Factory);
		}
		RefExchangeRate ExchangeRate;

		#endregion
	}
}
