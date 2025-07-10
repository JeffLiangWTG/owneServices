using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvTypeDeferredChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeGroupList()
		{
			OrgInvTypeDeferredCharges defcharge = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			OrgInvTypeDeferredChargesLookups lookups = new OrgInvTypeDeferredChargesLookups(defcharge);
			Assert(lookups.ChargeGroupList is ChargeCodeGroupList);
		}
	}
}
