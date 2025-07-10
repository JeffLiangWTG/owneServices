using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine.ExchangeRates
{
	public static class CustomsExchangeArgumentsParser
	{
		public static bool TryParse(string[] cmdLineArguments,
			out ICurrencyRateSearchParam currencyRateSearchParam,
			out string messageError
			)
		{
			currencyRateSearchParam = null;
			messageError = null;

			if (cmdLineArguments == null || cmdLineArguments.Length > 1)
			{
				messageError =
@"Invalid arguments.
Here's the correct format you should use:
CUSTOMS_EXCHANGE_RATE_UPDATE
Please note:
- Use this command to update all exchange rates for today and for the upcoming week.";
				return false;
			}

			currencyRateSearchParam = new CurrencyRateSearchParamWrapper(DateTimeUtil.GetNow, DateTimeUtil.GetNow.Date, DateTimeUtil.GetNow.Date.AddDays(7), null);

			return true;
		}
	}
}

