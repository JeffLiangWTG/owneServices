using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class GlobalCreditGroupHelper
	{
		public static decimal GetGlobalExchangeRate(BusinessObjectFactory factory, string globalCreditCurrencyCode)
		{
			var rate = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == globalCreditCurrencyCode ? 1m
				: Env.CurrentCompany.ExchangeRate.TodaysRate(globalCreditCurrencyCode, ExchangeRateType.GlobalCreditControl);

			if (rate == decimal.Zero)
			{
				var lastDayForCurrentPeriod = new AccountingPeriodCalculator(factory).GetLastDayForPeriod(ZDateTime.Today);
				if (lastDayForCurrentPeriod.IsValid)
				{
					rate = Env.CurrentCompany.ExchangeRate.GetRate(globalCreditCurrencyCode, ExchangeRateType.PeriodEnd, lastDayForCurrentPeriod.ToDateTime());
				}
			}

			return rate;
		}
	}
}
