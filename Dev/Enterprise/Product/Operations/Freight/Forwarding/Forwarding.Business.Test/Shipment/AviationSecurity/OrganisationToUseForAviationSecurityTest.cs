using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class OrganisationToUseForAviationSecurityTest : TestCaseWithFactory
	{
		public void TestDescription()
		{
			var org = new OrganisationToUseForAviationSecurity(SupplyChainSecurityOrganisationTypes.Consignor, "YES");
			AssertEquals("Consignor", org.OrganisationDescription);

			org = new OrganisationToUseForAviationSecurity(SupplyChainSecurityOrganisationTypes.ConsolAirline, "YES");
			AssertEquals("Consol Airline", org.OrganisationDescription);
		}
	}
}
