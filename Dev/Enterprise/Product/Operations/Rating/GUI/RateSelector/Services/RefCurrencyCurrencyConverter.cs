using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI.RateSelector.Services
{
	public class RefCurrenciesCurrencyConverter : ICurrencyConverter
	{
		public RefCurrenciesCurrencyConverter(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		public Money Convert(Money amount, RefCurrency targetCurrency)
		{
			if (amount.Currency.Code == targetCurrency.Code)
			{
				return new Money(amount.Amount, targetCurrency);
			}

			var result = CurrencyConvert.ConvertExact(amount, targetCurrency);
			if (result.IsValid && result.Amount == 0 && amount.Amount != 0)
			{
				result = new Money(amount.Amount, targetCurrency, false);
			}

			return result;
		}

		CurrencyConverter CurrencyConvert => currencyConvert ?? (currencyConvert = CurrencyConverter.New(factory, ZDateTime.Today, ExchangeRateType.Buy, 0));
		CurrencyConverter currencyConvert;
		readonly BusinessObjectFactory factory;
	}
}
