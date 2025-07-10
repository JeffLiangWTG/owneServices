using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public static class RefCurrencyExtensionMethods
	{
		public static void SetUpExchangeRates(this RefCurrency currency, ZDateTime date, ZDecimal rate)
		{
			SetUpExchangeRates(currency, date, date, rate);
		}

		public static void SetUpExchangeRates(this RefCurrency currency, ZDateTime date, ZDateTime endDate, ZDecimal rate)
		{
			if (currency != null)
			{
				if (!currency.ExchangeRates.Cast<RefExchangeRate>().Any(r => r.RE_StartDate >= date && r.RE_ExpiryDate <= date))
				{
					var exRate = currency.ExchangeRates.AddNew();
					exRate.RE_StartDate = date;
					exRate.RE_ExpiryDate = endDate.IsValid ? endDate : date;
					exRate.RE_ExRateType = "CUS";
					exRate.RE_SellRate = rate;
				}
			}
		}
	}
}
