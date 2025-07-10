using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public class CurrencyConverterWithFixedExchangeRatesDataProvider : CurrencyConverterWithDataProvider
	{
		public CurrencyConverterWithFixedExchangeRatesDataProvider(BusinessObjectFactory factory, ICurrencyConverterDataProviderWithFixedExRates dataProvider) : base(factory, dataProvider)
		{
		}

		public CurrencyConverterWithFixedExchangeRatesDataProvider(BusinessObjectFactory factory, ICurrencyConverterDataProviderWithFixedExRates dataProvider, bool roundToTargetCurrencyDecimals = true)
			: base(factory, dataProvider, roundToTargetCurrencyDecimals)
		{
		}

		public sealed override ZDecimal GetExchangeRate(ICurrency currency)
		{
			ZDateTime dontCareDateFallBack;
			return GetExchangeRate(currency, out dontCareDateFallBack);
		}

		public sealed override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate) => GetExchangeRateCore(currency, out foundRateDate);

		protected virtual ZDecimal GetExchangeRateCore(ICurrency currency, out ZDateTime foundRateDate)
		{
			decimal lookupRate = decimal.Zero;
			foundRateDate = ZDateTime.Empty;
			if (currency != null && !string.IsNullOrEmpty(currency.Code) && DataProvider.FixedExchangeRateCurrencyCode.Equals(currency.Code, StringComparison.OrdinalIgnoreCase))
			{
				lookupRate = DataProvider.FixedExchangeRate;
			}
			else
			{
				lookupRate = base.GetExchangeRate(currency, out foundRateDate);
			}
			return lookupRate;
		}

		public ZDecimal GetExchangeRateToDefault(RefCurrency currency)
		{
			ZDateTime date;
			return base.GetExchangeRate(currency, out date);
		}

		protected new ICurrencyConverterDataProviderWithFixedExRates DataProvider
		{
			get { return (ICurrencyConverterDataProviderWithFixedExRates)base.DataProvider; }
		}
	}
}
