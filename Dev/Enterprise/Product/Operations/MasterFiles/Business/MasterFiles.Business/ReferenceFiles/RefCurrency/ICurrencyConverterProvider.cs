using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface ICurrencyConverterProvider
	{
		CurrencyConverter CurrencyConverter { get; }
	}

	public interface ICurrencyConverterDataProvider
	{
		ZDateTime DateOfValuation { get; }
		ExchangeRateType RateType { get; }
		int MaximumDaysToFallback { get; }
		GlbCompany Company { get; }
		ZString LocalCurrencyCodeOverride { get; }
		ZBool? IsReciprocalOverride { get; }
	}

	public interface ICurrencyConverterDataProviderWithFixedExRates : ICurrencyConverterDataProvider
	{
		string FixedExchangeRateCurrencyCode { get; }
		decimal FixedExchangeRate { get; }
	}
}
