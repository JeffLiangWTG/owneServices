using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgLandedCostingPrefChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBrokerageOnlyIncludedInIncotermChargeGroups()
		{
			OrgLandedCostingPrefCharges charge = Factory.New<OrgLandedCostingPrefCharges>();
			AssertEquals(true, charge.Lookups.IncoTermChargeGroups.ContainsCode(ChargeCodeGroupList.Codes.BrokerageOnly));
		}
	}
}
