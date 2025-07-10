using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICurrencyProvider
	{
		ZString CurrencyCode { get; }
		void SetExchangeRateIfNotUserOverridden();
		void ValidateCurrencyCode();
	}

	public static class ICurrencyProviderExtensionMethods
	{
		public static bool IsForeignCurrency(this ICurrencyProvider currencyProvider, ZString localCurrency)
		{
			return !currencyProvider.CurrencyCode.IsEmpty && currencyProvider.CurrencyCode != localCurrency;
		}
	}
}
