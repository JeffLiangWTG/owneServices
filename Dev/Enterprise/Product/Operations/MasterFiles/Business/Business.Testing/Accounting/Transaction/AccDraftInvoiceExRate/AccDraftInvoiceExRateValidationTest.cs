using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccDraftInvoiceExRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAIE_ExchangeRate()
		{
			var exRate = this.CreateParent();
			exRate.Validation.ValidateAIE_ExchangeRate();
			AssertNoErrors(exRate.AIE_ExchangeRateInfo);

			exRate.AIE_ExchangeRate = 0m;
			AssertHasError(exRate.AIE_ExchangeRateInfo, "Exchange Rate must be greater than zero.");

			exRate.AIE_ExchangeRate = 1.5m;
			AssertNoErrors(exRate.AIE_ExchangeRateInfo);

			exRate.AIE_ExchangeRate = -1m;
			AssertHasError(exRate.AIE_ExchangeRateInfo, "Exchange Rate must be greater than zero.");
		}

		public void TestCheckAIE_RX_NKRateCurrency()
		{
			var exRate = this.CreateParent();
			exRate.Validation.ValidateAIE_RX_NKRateCurrency();
			AssertHasError(exRate.AIE_RX_NKRateCurrencyInfo, "Please enter a value.");

			exRate.AIE_RX_NKRateCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertNoErrors(exRate.AIE_RX_NKRateCurrencyInfo);

			var header = Factory.New<AccDraftInvoiceHeader>();
			header.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			exRate.AIE_AIH_Header = header.PK;
			exRate.Validation.ValidateAIE_RX_NKRateCurrency();
			AssertHasError(exRate.AIE_RX_NKRateCurrencyInfo, "Please select a foreign currency from the list.");

			exRate.AIE_RX_NKRateCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, exRate.AIE_RX_NKRateCurrency);
		}

		public void TestCheckAIE_RX_NKRateCurrency_NoDuplicateCurrencies()
		{
			var header = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			header.AIH_TransactionType = TransactionTypes.Invoice;
			header.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			var exRate1 = this.CreateParent();
			exRate1.AIE_AIH_Header = header.PK;
			exRate1.AIE_RX_NKRateCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			var exRate2 = this.CreateParent();
			exRate2.AIE_AIH_Header = header.PK;
			exRate2.AIE_RX_NKRateCurrency = Enterprise.Core.Constants.CurrencyCodes.NewZealand;
			var exRate3 = this.CreateParent();
			exRate3.AIE_AIH_Header = header.PK;
			exRate3.AIE_RX_NKRateCurrency = Enterprise.Core.Constants.CurrencyCodes.Brazil;
			Factory.Save();
			header.ExchangeRates.Reload(true);

			AssertNoErrors(exRate1.AIE_RX_NKRateCurrencyInfo);
			AssertNoErrors(exRate2.AIE_RX_NKRateCurrencyInfo);
			AssertNoErrors(exRate3.AIE_RX_NKRateCurrencyInfo);

			exRate3.AIE_RX_NKRateCurrency = Enterprise.Core.Constants.CurrencyCodes.NewZealand;

			AssertNoErrors(exRate1.AIE_RX_NKRateCurrencyInfo);
			AssertNoErrors(exRate2.AIE_RX_NKRateCurrencyInfo);
			AssertHasError(exRate3.AIE_RX_NKRateCurrencyInfo, "At least one more record already sets exchange rate for the same currency.");

			exRate1.Validation.ValidateAIE_RX_NKRateCurrency();
			exRate2.Validation.ValidateAIE_RX_NKRateCurrency();
			exRate3.Validation.ValidateAIE_RX_NKRateCurrency();

			AssertNoErrors(exRate1.AIE_RX_NKRateCurrencyInfo);
			AssertHasError(exRate2.AIE_RX_NKRateCurrencyInfo, "At least one more record already sets exchange rate for the same currency.");
			AssertHasError(exRate3.AIE_RX_NKRateCurrencyInfo, "At least one more record already sets exchange rate for the same currency.");

			exRate3.AIE_RX_NKRateCurrency = Enterprise.Core.Constants.CurrencyCodes.Brazil;
			exRate1.Validation.ValidateAIE_RX_NKRateCurrency();
			exRate2.Validation.ValidateAIE_RX_NKRateCurrency();
			exRate3.Validation.ValidateAIE_RX_NKRateCurrency();

			AssertNoErrors(exRate1.AIE_RX_NKRateCurrencyInfo);
			AssertNoErrors(exRate2.AIE_RX_NKRateCurrencyInfo);
			AssertNoErrors(exRate3.AIE_RX_NKRateCurrencyInfo);
		}

		AccDraftInvoiceExRate CreateParent() => Factory.New<AccDraftInvoiceExRate>();
	}
}
