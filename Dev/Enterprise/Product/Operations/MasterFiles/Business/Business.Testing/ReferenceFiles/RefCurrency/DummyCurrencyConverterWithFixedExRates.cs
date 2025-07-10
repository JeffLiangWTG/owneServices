using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class DummyCurrencyConverterWithFixedExRates : DummyCurrencyConverterDataProvider, ICurrencyConverterDataProviderWithFixedExRates
	{
		public DummyCurrencyConverterWithFixedExRates()
		{
		}

		public DummyCurrencyConverterWithFixedExRates(ZDateTime dateOfValuation, ExchangeRateType rateType, int maximumDaysToFallback)
			: base(dateOfValuation, rateType, maximumDaysToFallback)
		{
		}

		public string FixedExchangeRateCurrencyCodeExposed = "";
		public string FixedExchangeRateCurrencyCode
		{
			get { return FixedExchangeRateCurrencyCodeExposed; }
		}

		public decimal FixedExchangeRateExposed = decimal.Zero;
		public decimal FixedExchangeRate
		{
			get { return FixedExchangeRateExposed; }
		}
	}
}
