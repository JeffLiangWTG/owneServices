namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Integration.Accounting;
	using Enterprise.Integration.ZArchitecture;
	using Enterprise.ZArchitecture.Core;

	public class NonOrgSpecificExRateCurrencyConverter : RefCurrencyCurrencyConverter, IJobExRateCurrencyConverter
	{
		public NonOrgSpecificExRateCurrencyConverter(GlbCompany company, BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, int maximumDaysToFallback)
			: base(company, factory, dateForRate, rateType, maximumDaysToFallback)
		{
		}

		public NonOrgSpecificExRateCurrencyConverter(BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, int maximumDaysToFallback)
			: base(factory, dateForRate, rateType, maximumDaysToFallback)
		{
		}

		public NonOrgSpecificExRateCurrencyConverter(GlbCompany company, BusinessObjectFactory factory)
			: base(company, factory)
		{
		}

		public NonOrgSpecificExRateCurrencyConverter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public virtual ZDecimal GetExchangeRate(ICurrency currency, ZGuid orgPK, CostSell costOrSell) =>
			GetExchangeRate(currency);

		public virtual Money ConvertExact(Money monetaryAmount, ICurrency destinationCurrency, ZGuid orgPK, CostSell costOrSell, bool roundToDestinationCurrencyDecimals = true) =>
			ConvertExact(monetaryAmount, destinationCurrency, currency => GetExchangeRate(currency, orgPK, costOrSell), roundToDestinationCurrencyDecimals);

		public static NonOrgSpecificExRateCurrencyConverter Default(BusinessObjectFactory factory) =>
			new NonOrgSpecificExRateCurrencyConverter(factory, ZDateTime.Today, ExchangeRateType.Sell, 7);
	}
}
