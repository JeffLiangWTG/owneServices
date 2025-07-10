using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public interface IRateChooserServices
	{
		Money ConvertToDefaultCurrency(decimal amount, string currency);
		string ConvertToCurrentCompanyFormat(decimal amount);
	}

	public class RateChooserServices : IRateChooserServices
	{
		public RateChooserServices(BusinessObjectFactory factory, ZDateTime dateForCurrencyConvert, string defaultCurrencyCode)
		{
			Factory = factory;
			this.dateForCurrencyConvert = dateForCurrencyConvert;
			this.defaultCurrencyCode = defaultCurrencyCode;
		}
		readonly BusinessObjectFactory Factory;
		readonly ZDateTime dateForCurrencyConvert;
		readonly string defaultCurrencyCode;

		/// <summary>
		/// Converts the given 'amount' in 'currency' currency to an amount in
		/// the DefaultCurrency.
		/// </summary>
		/// <returns>
		/// If there is no exchange rate, the returned Money is not valid.
		/// </returns>
		public Money ConvertToDefaultCurrency(decimal amount, string currency)
		{
			if (currency == defaultCurrencyCode)
			{
				return new Money(amount, DefaultCurrency);
			}

			var sourceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);

			var result = CurrencyConvert.ConvertExact(new Money(amount, sourceCurrency), DefaultCurrency);
			// Make sure failed conversion produces invalid Money.
			// This can happen when there is no exchange rate
			if (result.IsValid && result.Amount == 0 && amount != 0)
			{
				result = new Money(amount, sourceCurrency, overrideIsValid: false);
			}
			return result;
		}

		/// <summary>
		/// Applies company-culture formatting to the given amount. The amount is expected
		/// to be in the local currency. 
		/// </summary>
		public string ConvertToCurrentCompanyFormat(decimal amount)
		{
			return amount.ToString("C", Culture.CurrentCompanyCountryCulture.NumberFormat);
		}

		RefCurrency DefaultCurrency => defaultCurrency ?? (defaultCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, defaultCurrencyCode));
		RefCurrency defaultCurrency;

		CurrencyConverter CurrencyConvert => currencyConvert ?? (currencyConvert = CurrencyConverter.New(Factory, dateForCurrencyConvert, ZArchitecture.Core.ExchangeRateType.Buy, 0));
		CurrencyConverter currencyConvert;
	}
}
