using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.Business.Testing.CurrencyConverterTestHelper.StrategyIfExchangeRateAlreadyExists;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CurrencyConverterTestHelper))]
	sealed class CurrencyConverterTestHelperTest : TestCaseWithFactory
	{
		public void TestGetCurrency()
		{
			var testCurrency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, testCurrency.Code);
		}

		[TestDate(2016, 01, 01)]
		public void TestSetExchangeRate() => CombineAssertions(() =>
		{
			var today = new ZDateTime(2016, 01, 01);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 10m, today, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 20m, today, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.Sweden, 30m);
			var currency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.Denmark);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 40m);
			currency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.Iceland);
			CurrencyConverterTestHelper.SetExchangeRateWithAsPublished(Factory, currency, 30m, ZDate.Today, Core.Constants.ExchangeRateTypes.Code.CustomsRate, "123,45");
			Factory.Save();

			var converter = new RefCurrencyCurrencyConverter(Factory, today, ExchangeRateType.Customs, 0);
			var testCurrency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var rate = converter.GetExchangeRate(testCurrency);
			AssertEquals("USD, called with all params", 10m, rate);

			converter = new RefCurrencyCurrencyConverter(Factory, today, ExchangeRateType.CustomsSecondary, 0);
			testCurrency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.EuropeanUnion);
			rate = converter.GetExchangeRate(testCurrency);
			AssertEquals("EUR as secondary customs-rate, called with all params", 20m, rate);

			converter = new RefCurrencyCurrencyConverter(Factory, today, ExchangeRateType.Customs, 0);
			testCurrency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.Sweden);
			rate = converter.GetExchangeRate(testCurrency);
			AssertEquals("SEK, called with currency and rate", 30m, rate);

			converter = new RefCurrencyCurrencyConverter(Factory, today, ExchangeRateType.Customs, 0);
			testCurrency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.Denmark);
			rate = converter.GetExchangeRate(testCurrency);
			AssertEquals("DKK, called with RefCurrency and rate", 40m, rate);

			converter = new RefCurrencyCurrencyConverter(Factory, today, ExchangeRateType.Customs, 0);
			testCurrency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.Iceland);
			var exchangeRate = converter.GetExchangeRateObject(testCurrency);
			AssertEquals("ISK, called with AsPublished", "123,45", exchangeRate.RE_AsPublished);
		});

		public void TestSetExchangeRate_StrategySplitTimePeriod() => CombineAssertions(() =>
		{
			var currency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.UnitedKingdom);
			ZDecimal rate1 = 10m;
			ZDecimal rate2 = 12m;
			var startDate = new ZDateTime(1980, 1, 1);
			var splitDate = new ZDateTime(1985, 7, 1);
			var expiryDate = new ZDateTime(1990, 12, 31);
			ZString rateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, rate1, startDate, expiryDate, rateType);
			AssertContainsExactElementsInAnyOrder("before exchange rate is split", [
				"Between [1980-01-01T00:00:00] and [1990-12-31T23:59:00] [CUS] sell rate is [10]",
			], GetExchangeRatesAsString(currency, startDate, expiryDate));

			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, rate2, splitDate, rateType, strategy: SplitTimePeriod);
			AssertContainsExactElementsInAnyOrder("after exchange rate is split", [
				"Between [1980-01-01T00:00:00] and [1985-06-30T23:59:00] [CUS] sell rate is [10]",
				"Between [1985-07-01T00:00:00] and [1985-07-01T23:59:00] [CUS] sell rate is [12]",
				"Between [1985-07-02T00:00:00] and [1990-12-31T23:59:00] [CUS] sell rate is [10]",
			], GetExchangeRatesAsString(currency, startDate, expiryDate));
		});

		public void TestSetExchangeRate_StrategyUpdateRateButKeepTimePeriod() => CombineAssertions(() =>
		{
			var currency = CurrencyConverterTestHelper.GetCurrency(Factory, Core.Constants.CurrencyCodes.UnitedKingdom);
			ZDecimal rate1 = 10m;
			ZDecimal rate2 = 12m;
			var startDate = new ZDateTime(1980, 1, 1);
			var effectiveDate = new ZDateTime(1985, 7, 1);
			var expiryDate = new ZDateTime(1990, 12, 31);
			ZString rateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, rate1, startDate, expiryDate, rateType);
			AssertContainsExactElementsInAnyOrder("before exchange rate is split", [
				"Between [1980-01-01T00:00:00] and [1990-12-31T23:59:00] [CUS] sell rate is [10]",
			], GetExchangeRatesAsString(currency, startDate, expiryDate));

			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, rate2, effectiveDate, rateType, strategy: UpdateRateButKeepTimePeriod);
			AssertContainsExactElementsInAnyOrder("after exchange rate is split", [
				"Between [1980-01-01T00:00:00] and [1990-12-31T23:59:00] [CUS] sell rate is [12]",
			], GetExchangeRatesAsString(currency, startDate, expiryDate));
		});

		static string[] GetExchangeRatesAsString(RefCurrency currency, ZDateTime startDate, ZDateTime expiryDate) =>
			currency.ExchangeRates
				.Where(x => x.RE_StartDate >= startDate)
				.Where(x => x.RE_ExpiryDate <= expiryDate.AddDays(1).AddMinutes(-1))
				.Select(x => $"Between [{x.RE_StartDate.ToISO8601String()}] and [{x.RE_ExpiryDate.ToISO8601String()}] [{x.RE_ExRateType}] sell rate is [{x.RE_SellRate}]")
				.ToArray();
	}
}
