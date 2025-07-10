using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public static class CurrencyHelper
	{
		public static ZDecimal GetExchangeRate(CurrencyConverter converter, ZString currencyCode)
		{
			var currency = RefCurrency.LoadFromCurrencyCode(converter.Factory, currencyCode);
			return converter.GetExchangeRate(currency);
		}

		public static ZDecimal ConvertUsingCustomsRate(ZDateTime dateForRate, ZDecimal amount, ZString originalCurrencyCode, ZString destinationCurrencyCode, GlbCompany company, BusinessObjectFactory factory, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var result = ZDecimal.Zero;
			if (originalCurrencyCode == destinationCurrencyCode)
			{
				result = amount;
			}
			else if (dateForRate.IsValid)
			{
				var originalCurrency = RefCurrency.LoadFromCurrencyCode(factory, originalCurrencyCode);
				if (originalCurrency != null)
				{
					var destinationCurrency = RefCurrency.LoadFromCurrencyCode(factory, destinationCurrencyCode);
					if (destinationCurrency != null && company != null)
					{
						var originalAmount = new Money(amount, originalCurrency);
						var cC = new RefCurrencyCurrencyConverter(company, factory, dateForRate, rateType, 0);
						result = cC.ConvertRounded(originalAmount, destinationCurrency).Amount;
					}
				}
			}
			return result;
		}
	}
}
