namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationCNTest : SupplyChainSecurityConfigurationTest
	{
		public void TestRelatedCountriesForSupplyChainSecurity()
		{
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.CountryCodes.HongKong }, SupplyChainSecurityConfiguration.RelatedCountriesForSupplyChainSecurity);
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationCN();
		}

		#endregion
	}
}
