using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactAttributeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAllocationTypes()
		{
			var orgContactAllocation = Factory.New<OrgContactAllocation>();
			var allocationTypes = orgContactAllocation.Lookups.AllocationTypes;
			Assert(allocationTypes.ContainsCode(OrgConstants.ContactAllocationType.CAPGA));
		}
	}
}
