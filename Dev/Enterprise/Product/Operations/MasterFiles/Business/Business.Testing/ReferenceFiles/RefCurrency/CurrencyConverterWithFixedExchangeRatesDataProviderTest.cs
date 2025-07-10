using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class CurrencyConverterWithFixedExchangeRatesDataProviderTest : CurrencyConverterWithDataProviderTest
	{
		public void TestRefreshCachedExchangeRatesWhenFixedExchangeRatesIsChanged()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZZZ";
			RefExchangeRate rateCus1 = currency.ExchangeRates.AddNew();
			rateCus1.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateCus1.RE_ExpiryDate = new ZDateTime(2005, 1, 5);
			rateCus1.RE_SellRate = 1.5m;
			rateCus1.RE_ExRateType = "CUS";

			DummyProvider.DateOfValuationExposed = rateCus1.RE_StartDate;
			DummyProvider.MaximumDaysToFallbackExposed = 0;
			DummyProvider.RateTypeExposed = Enterprise.ZArchitecture.Core.ExchangeRateType.Customs;

			CurrencyConverterWithFixedExchangeRatesDataProvider currencyConverter = (CurrencyConverterWithFixedExchangeRatesDataProvider)GetCurrencyConverter();
			ZDecimal result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("Exchange rate with 2005/1/1", 1.5m, result);

			DummyProvider.FixedExchangeRateCurrencyCodeExposed = currency.RX_Code;
			DummyProvider.FixedExchangeRateExposed = 1.6m;
			result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("rate of fixed exchange", 1.6m, result);

			DummyProvider.FixedExchangeRateExposed = 1.55m;
			result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("rate of fixed exchange", 1.55m, result);

			DummyProvider.DateOfValuationExposed = rateCus1.RE_StartDate.AddDays(1);
			result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("rate of fixed exchange", 1.55m, result);

			DummyProvider.FixedExchangeRateCurrencyCodeExposed = "";
			result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("rate of fixed exchange", 1.5m, result);

			result = currencyConverter.GetExchangeRate(null);
			AssertEquals("rate of fixed exchange", 0m, result);
		}

		public void TestGetExchangeRateToDefault()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZZZ";
			RefExchangeRate rateCus1 = currency.ExchangeRates.AddNew();
			rateCus1.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateCus1.RE_ExpiryDate = new ZDateTime(2005, 1, 5);
			rateCus1.RE_SellRate = 1.5m;
			rateCus1.RE_ExRateType = "CUS";

			DummyProvider.DateOfValuationExposed = rateCus1.RE_StartDate;
			DummyProvider.MaximumDaysToFallbackExposed = 0;
			DummyProvider.RateTypeExposed = Enterprise.ZArchitecture.Core.ExchangeRateType.Customs;
			DummyProvider.FixedExchangeRateCurrencyCodeExposed = currency.RX_Code;
			DummyProvider.FixedExchangeRateExposed = 0m;

			CurrencyConverterWithFixedExchangeRatesDataProvider currencyConverter = (CurrencyConverterWithFixedExchangeRatesDataProvider)GetCurrencyConverter();
			ZDecimal result = currencyConverter.GetExchangeRateToDefault(currency);
			AssertEquals("Exchange rate with 2005/1/1 is used to default to exchange rate field", 1.5m, result);
		}

		#region Implementation

		protected new DummyCurrencyConverterWithFixedExRates DummyProvider
		{
			get { return (DummyCurrencyConverterWithFixedExRates)base.DummyProvider; }
		}

		protected override DummyCurrencyConverterDataProvider GetNewDummyProvider()
		{
			return new DummyCurrencyConverterWithFixedExRates();
		}

		protected override CurrencyConverterWithDataProvider GetCurrencyConverter()
		{
			return new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, DummyProvider);
		}

		#endregion
	}
}
