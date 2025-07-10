using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationHelper : ISupplyChainSecurityConfigurationHelper
	{
		public ISupplyChainSecurityConfiguration GetConfiguration()
		{
			return GetConfigurationForCountry(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public ISupplyChainSecurityConfiguration GetConfigurationForCountry(string countryCode)
		{
			if (supplyChainSecurityConfiguration != null && supplyChainSecurityConfigurationCountryCode == countryCode)
			{
				return supplyChainSecurityConfiguration;
			}

			supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New(countryCode);
			supplyChainSecurityConfigurationCountryCode = countryCode;

			return supplyChainSecurityConfiguration;
		}

		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;
		ZString supplyChainSecurityConfigurationCountryCode;
	}
}
