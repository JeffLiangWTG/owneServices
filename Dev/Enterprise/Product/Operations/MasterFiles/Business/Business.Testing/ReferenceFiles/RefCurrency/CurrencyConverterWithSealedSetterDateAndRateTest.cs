using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class CurrencyConverterWithSealedSetterDateAndRateTest : RefCurrencyCurrencyConverterTest
	{
		protected abstract CurrencyConverterWithDataProvider GetCurrencyConverter();
	}

	public class CurrencyConverterWithDataProviderTest : CurrencyConverterWithSealedSetterDateAndRateTest
	{
		public void TestOverrideLocalCurrencyCode()
		{
			var currencyConverter = GetCurrencyConverter();
			AssertEquals("default local currency", GlbCompany.CurrentCompany.LocalCurrency.PK, currencyConverter.LocalCurrency.PK);
			DummyProvider.LocalCurrencyCodeOverrideExposed = "NZD";
			AssertNotEquals("NOT default local currency", GlbCompany.CurrentCompany.LocalCurrency.PK, currencyConverter.LocalCurrency.PK);
			AssertEquals("is NZD", "NZD", currencyConverter.LocalCurrency.RX_Code);

			AssertEquals("default isreciprocal", GlbCompany.CurrentCompany.GC_IsReciprocal, currencyConverter.IsReciprocal);
			DummyProvider.IsReciprocalOverrideExposed = !GlbCompany.CurrentCompany.GC_IsReciprocal;
			AssertNotEquals("is now reversed", GlbCompany.CurrentCompany.GC_IsReciprocal, currencyConverter.IsReciprocal);
			DummyProvider.IsReciprocalOverrideExposed = GlbCompany.CurrentCompany.GC_IsReciprocal;
			AssertEquals("is now same", GlbCompany.CurrentCompany.GC_IsReciprocal, currencyConverter.IsReciprocal);
		}

		public void TestDateRateAndMaximumDaysToFallBack()
		{
			SetDataForDummyProvider(new ZDateTime(2005, 1, 1), ExchangeRateType.Customs, 3);
			CurrencyConverterWithDataProvider currencyConverter = GetCurrencyConverter();
			AssertEquals("CurrencyConverter.DateForRate", new ZDateTime(2005, 1, 1), currencyConverter.DateForRate);
			AssertEquals("CurrencyConverter.RateType", ExchangeRateType.Customs, currencyConverter.RateType);
			AssertEquals("CurrencyConverter.MaximumDaysToFallback", 3, currencyConverter.MaximumDaysToFallback);

			SetDataForDummyProvider(new ZDateTime(2005, 1, 2), ExchangeRateType.Buy, 4);
			AssertEquals("CurrencyConverter.DateForRate", new ZDateTime(2005, 1, 2), currencyConverter.DateForRate);
			AssertEquals("CurrencyConverter.RateType", ExchangeRateType.Buy, currencyConverter.RateType);
			AssertEquals("CurrencyConverter.MaximumDaysToFallback", 4, currencyConverter.MaximumDaysToFallback);
		}

		public void TestRefreshCachedExchangeRatesWhenDateIsChanged()
		{
			RefCurrency currency = RefCurrency.New(Factory);
			RefExchangeRate rateCus1 = currency.ExchangeRates.AddNew();
			rateCus1.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateCus1.RE_ExpiryDate = new ZDateTime(2005, 1, 1);
			rateCus1.RE_SellRate = 1.5m;
			rateCus1.RE_ExRateType = "CUS";

			RefExchangeRate rateCus2 = currency.ExchangeRates.AddNew();
			rateCus2.RE_StartDate = new ZDateTime(2005, 1, 2);
			rateCus2.RE_ExpiryDate = new ZDateTime(2005, 1, 2);
			rateCus2.RE_SellRate = 1.6m;
			rateCus2.RE_ExRateType = "CUS";

			SetDataForDummyProvider(new ZDateTime(2005, 1, 1), ExchangeRateType.Customs, 0);
			CurrencyConverterWithDataProvider currencyConverter = GetCurrencyConverter();
			ZDecimal result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("Exchange rate with 2005/1/1", 1.5m, result);

			SetDataForDummyProvider(new ZDateTime(2005, 1, 2), ExchangeRateType.Customs, 0);
			result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("Exchange rate with 2005/1/2", 1.6m, result);
		}

		public void TestRefreshCachedExchangeWhenRateTypeIsChanged()
		{
			RefCurrency currency = RefCurrency.New(Factory);
			RefExchangeRate rateBuy1 = currency.ExchangeRates.AddNew();
			rateBuy1.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateBuy1.RE_ExpiryDate = new ZDateTime(2005, 1, 1);
			rateBuy1.RE_SellRate = 1.5m;
			rateBuy1.RE_ExRateType = "CUS";

			RefExchangeRate rateBuy2 = currency.ExchangeRates.AddNew();
			rateBuy2.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateBuy2.RE_ExpiryDate = new ZDateTime(2005, 1, 1);
			rateBuy2.RE_SellRate = 1.6m;
			rateBuy2.RE_ExRateType = "BUY";

			SetDataForDummyProvider(new ZDateTime(2005, 1, 1), ExchangeRateType.Customs, 0);
			CurrencyConverterWithDataProvider currencyConverter = GetCurrencyConverter();
			ZDecimal result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("Exchange rate with 2005/1/1", 1.5m, result);

			SetDataForDummyProvider(new ZDateTime(2005, 1, 1), ExchangeRateType.Buy, 0);
			result = currencyConverter.GetExchangeRate(currency);
			AssertEquals("Buy Exchange rate with 2005/1/1", 1.6m, result);
		}

		public void TestConverterProviderValidity()
		{
			var currencyConverter = GetCurrencyConverter();
			Assert("The default value of IsConverterValid should be true", currencyConverter.IsConverterValid);
		}

		#region Implementation

		protected DummyCurrencyConverterDataProvider DummyProvider;

		protected override void SetUp()
		{
			base.SetUp();
			DummyProvider = GetNewDummyProvider();
		}

		protected virtual DummyCurrencyConverterDataProvider GetNewDummyProvider()
		{
			return new DummyCurrencyConverterDataProvider();
		}

		void SetDataForDummyProvider(ZDateTime dateForRate, ExchangeRateType rateType, int maximumDaysToFallback)
		{
			DummyProvider.DateOfValuationExposed = dateForRate;
			DummyProvider.MaximumDaysToFallbackExposed = maximumDaysToFallback;
			DummyProvider.RateTypeExposed = rateType;
		}

		protected override CurrencyConverterWithDataProvider GetCurrencyConverter()
		{
			return new CurrencyConverterWithDataProvider(Factory, DummyProvider);
		}

		#endregion
	}
}
