using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccExchangeRateConfigurationWrapperTest : TestCaseWithFactory
	{
		public void TestExchangeRateConfigurationWrapperFields()
		{
			var exRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig = exRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty);
			var wrapper = new AccExchangeRateConfigurationWrapper(exRateConfig, currencyConfig);

			AssertEquals(currencyConfig.ExchangeRateType, wrapper.ExchangeRateType);
			AssertEquals(currencyConfig.JCT_ExRateType, wrapper.ExchangeRateTypeAsZString);
			AssertEquals(exRateConfig.PK, wrapper.PK);
			AssertEquals(exRateConfig.JCE_Prompt, wrapper.Prompt);
			AssertEquals(exRateConfig.JCE_Preference, wrapper.Preference);
			AssertEquals(exRateConfig.JCE_Ledger, wrapper.Ledger);
			AssertEquals(exRateConfig.JCE_JobType, wrapper.JobType);
			AssertEquals(exRateConfig.JCE_ServiceDirection, wrapper.ServiceDirection);
			AssertEquals(exRateConfig.JCE_TransportMode, wrapper.TransportMode);
		}
	}
}
