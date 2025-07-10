using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ConsolCurrencyConverterHelper
	{
		public static ZDecimal? TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(this ForwardingConsol consol, ZDecimal originalAmount, ZString originalCurrencyCode, ZString targetCurrencyCode)
		{
			ZDecimal? result = null;

			if (originalCurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				var targetCurrencyExRate = consol.GetExchangeRateFromFreightCostsOrSchedule(targetCurrencyCode);
				result = Env.CurrentCompany.ExchangeRate.LocalToForeign(originalAmount, targetCurrencyExRate, targetCurrencyCode);
			}
			else if (targetCurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				var originalCurrencyExRate = consol.GetExchangeRateFromFreightCostsOrSchedule(originalCurrencyCode);
				if (originalCurrencyExRate > 0m)
				{
					result = Env.CurrentCompany.ExchangeRate.ForeignToLocal(originalAmount, originalCurrencyExRate);
				}
			}
			else
			{
				var originalCurrencyExRate = consol.GetExchangeRateFromFreightCostsOrSchedule(originalCurrencyCode);
				var targetCurrencyExRate = consol.GetExchangeRateFromFreightCostsOrSchedule(targetCurrencyCode);
				if (originalCurrencyExRate != 0 && targetCurrencyExRate != 0)
				{
					result = Env.CurrentCompany.ExchangeRate.ForeignToForeign(originalAmount, originalCurrencyExRate, targetCurrencyExRate, targetCurrencyCode);
				}
			}

			return result;
		}
	}
}
