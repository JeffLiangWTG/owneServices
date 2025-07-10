using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCurrencyCurrencyConverter_Test : TestCaseWithFactory
	{
		public void TestGetExchangeRateCachesValueOnlyWhenAppropriate()
		{
			RefCurrencyCurrencyConverter converter = new RefCurrencyCurrencyConverter(Factory, ZDateTime.Today, ExchangeRateType.Customs, 0);
			RefCurrency currency = RefCurrency.New(Factory);
			RefExchangeRate rate = currency.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate.RE_SellRate = 1.5m;
			rate.RE_ExRateType = "CUS";
			AssertEquals("Rate after initial setup", 1.5m, converter.GetExchangeRate(currency));
			AssertEquals("Found Cached Value", false, converter.HasFoundCachedValue);
			AssertEquals("Rate after First Cache Hit", 1.5m, converter.GetExchangeRate(currency));
			AssertEquals("Found Cached Value", true, converter.HasFoundCachedValue);
			rate.RE_SellRate = 1.6m;
			AssertEquals("Rate after rate change", 1.6m, converter.GetExchangeRate(currency));
			rate.RE_StartDate = ZDateTime.Today.AddDays(1);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			AssertEquals("Rate after date invalidation", 0m, converter.GetExchangeRate(currency));
		}

		public void TestCachedExchangeRatesAreRefreshed()
		{
			TestCurrencyConverter converter = new TestCurrencyConverter(Factory);
			converter.RefreshCachedExchangeRatesfIfNeeded();
			AssertEquals("CachedRates is refreshed", null, converter.cachedPropertyExposed);
		}

		class TestCurrencyConverter : RefCurrencyCurrencyConverter
		{
			public TestCurrencyConverter(BusinessObjectFactory factory) : base(factory)
			{
			}

			public CachedProperty<Dictionary<string, ExchangeRate>> cachedPropertyExposed
			{
				get { return cachedRates; }
			}

			protected override bool NeedToRefreshCachedExchangeRates
			{
				get
				{
					return true;
				}
			}
		}
	}
}
