using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccExchangeRateConfigurationWrapper
	{
		public AccExchangeRateConfigurationWrapper(AccExchangeRateConfiguration exRateConfiguration, ExchangeRateCurrencyConfiguration currencyConfiguration )
		{
			ExchangeRateConfiguration = exRateConfiguration;
			CurrencyConfiguration = currencyConfiguration;
		}

		public ZGuid PK => ExchangeRateConfiguration.PK;

		public ExchangeRateType ExchangeRateType => CurrencyConfiguration.ExchangeRateType;

		public ZString ExchangeRateTypeAsZString => CurrencyConfiguration.JCT_ExRateType;

		public ZBool Prompt => ExchangeRateConfiguration.JCE_Prompt;

		public ZString Preference => ExchangeRateConfiguration.JCE_Preference;

		public ZInt OffSet => ExchangeRateConfiguration.JCE_Offset;

		public ZString Ledger => ExchangeRateConfiguration.JCE_Ledger;

		public ZString JobType => ExchangeRateConfiguration.JCE_JobType;

		public ZString ServiceDirection => ExchangeRateConfiguration.JCE_ServiceDirection;

		public ZString TransportMode => ExchangeRateConfiguration.JCE_TransportMode;

		readonly AccExchangeRateConfiguration ExchangeRateConfiguration;
		readonly ExchangeRateCurrencyConfiguration CurrencyConfiguration;
	}
}
