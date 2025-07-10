using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Test
{
	public class RateChooserServicesTest : TestCaseWithFactory
	{
		public void TestConvertToDefaultCurrency()
		{
			var today = ZDateTime.Today;
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefExchangeRate buyExchangeRate = usdCurrency.ExchangeRates.AddNew();
			buyExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyExchangeRate.RE_SellRate = 0.8;
			buyExchangeRate.RE_StartDate = today.AddMonths(-1);
			buyExchangeRate.RE_ExpiryDate = today.AddMonths(1);

			RefExchangeRate sellExchangeRate = usdCurrency.ExchangeRates.AddNew();
			sellExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellExchangeRate.RE_SellRate = 0.85;
			sellExchangeRate.RE_StartDate = today.AddMonths(-1);
			sellExchangeRate.RE_ExpiryDate = today.AddMonths(1);
			Factory.Save();

			var servicesAUD = new RateChooserServices(Factory, today, "AUD");
			var servicesUSD = new RateChooserServices(Factory, today, "USD");

			AssertEquals("Value should not change for local currency", 1.23m, (decimal)servicesAUD.ConvertToDefaultCurrency(1.23m, "AUD").Amount);
			AssertEquals("Value should change for foreign currency", 1.54m, (decimal)servicesAUD.ConvertToDefaultCurrency(1.23m, "USD").Amount);

			AssertEquals("Value should not change for local currency", 1.23m, (decimal)servicesUSD.ConvertToDefaultCurrency(1.23m, "USD").Amount);
			AssertEquals("Value should change for foreign currency", 0.98m, (decimal)servicesUSD.ConvertToDefaultCurrency(1.23m, "AUD").Amount);

			sellExchangeRate.Delete();
			buyExchangeRate.Delete();
			Factory.Save();

			servicesAUD = new RateChooserServices(Factory, today, "AUD");
			servicesUSD = new RateChooserServices(Factory, today, "USD");
			AssertEquals("Value should not change for local currency", 1.23m, (decimal)servicesAUD.ConvertToDefaultCurrency(1.23m, "AUD").Amount);
			AssertEquals("Invalid for foreign currency with no exchange rate", false, servicesAUD.ConvertToDefaultCurrency(1.23m, "USD").IsValid);

			AssertEquals("Value should not change for local currency", 1.23m, (decimal)servicesUSD.ConvertToDefaultCurrency(1.23m, "USD").Amount);
			AssertEquals("Invalid for foreign currency with no exchange rate", false, servicesUSD.ConvertToDefaultCurrency(1.23m, "AUD").IsValid);
		}

		public void TestFormattedDefaultCurrencyAmount()
		{
			AssertFormattedDefaultCurrencyAmount(Core.Constants.CountryCodes.India, 123456, "₹1,23,456.00");
			AssertFormattedDefaultCurrencyAmount(Core.Constants.CountryCodes.Australia, 1234.56, "$1,234.56");
#if NET
			AssertFormattedDefaultCurrencyAmount(Core.Constants.CountryCodes.VietNam, 1234.56, "1.235 ₫");
#else
			AssertFormattedDefaultCurrencyAmount(Core.Constants.CountryCodes.VietNam, 1234.56, "1.234,56 ₫");
#endif

			void AssertFormattedDefaultCurrencyAmount(string countryCode, ZDecimal originalValue, string expectedFormattedValue)
			{
				using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(countryCode)))
				{
					var services = new RateChooserServices(Factory, ZDateTime.Today, "AUD");
					AssertEquals(expectedFormattedValue, services.ConvertToCurrentCompanyFormat(originalValue));
				}
			}
		}
	}
}
