namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface ISupplyChainSecurityConfigurationHelper
		{
			ISupplyChainSecurityConfiguration GetConfiguration();
			ISupplyChainSecurityConfiguration GetConfigurationForCountry(string countryCode);
		}
	}
}
