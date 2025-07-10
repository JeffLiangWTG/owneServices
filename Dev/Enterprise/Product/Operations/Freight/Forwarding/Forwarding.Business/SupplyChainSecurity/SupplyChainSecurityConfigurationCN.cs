using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationCN : SupplyChainSecurityConfiguration
	{
		#region RelatedCountriesForSupplyChainSecurity

		protected override IEnumerable<string> GetRelatedCountriesForSupplyChainSecurityCore()
		{
			yield return Core.Constants.CountryCodes.HongKong;
		}

		#endregion
	}
}
