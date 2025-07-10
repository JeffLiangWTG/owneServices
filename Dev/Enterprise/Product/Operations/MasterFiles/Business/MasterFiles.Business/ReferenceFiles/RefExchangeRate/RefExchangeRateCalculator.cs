using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business;

public class RefExchangeRateCalculator : AutoRefExchangeRateCalculator
{
	public RefExchangeRateCalculator(RefExchangeRate refExchangeRate)
	{
		this.refExchangeRate = Argument.NotNull(refExchangeRate, nameof(refExchangeRate));

		QuoteCurrency = refExchangeRate.RE_RX_NKExCurrency;
		BaseCurrency = refExchangeRate.Company.GC_RX_NKLocalCurrency;
		if (refExchangeRate.TryGetCurrencyMultiplier(out var multiplier))
		{
			QuoteCurrencyValue = multiplier;
			BaseCurrencyValue = refExchangeRate.RE_SellRate * multiplier;
		}
	}

	readonly RefExchangeRate refExchangeRate;

	public void CalculateExchangeRate(int decimalPlaces)
	{
		if (QuoteCurrencyValue != ZDecimal.Zero)
		{
			refExchangeRate.RE_SellRate = ZArchitecture.Core.Utilities.Round(BaseCurrencyValue / QuoteCurrencyValue, decimalPlaces);
		}
	}
}
