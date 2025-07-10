using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobExRateCurrencyConverter
	{
		ExchangeRateType RateType { get; set; }
		ZDecimal GetExchangeRate(ICurrency currency, ZGuid orgPK, CostSell costOrSell);
		Money ConvertExact(Money monetaryAmount, ICurrency destinationCurrency, ZGuid orgPK, CostSell costOrSell, bool roundToDestinationCurrencyDecimals = true);
	}
}
