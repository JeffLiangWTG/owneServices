using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentActionAssemblyDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DtbConsignmentAction), new DtbConsignmenActionAssemblyData().BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertNull("This is only used for the Allocate eDocs functionality, which we've disabled for Consignment Actions.", new DtbConsignmenActionAssemblyData().GetBusinessObjectCollection(Factory));
		}

		public void TestModuleID()
		{
			AssertNull("There isn't a module for Actions which is fine since it won't work in Manage > DocManager > Allocate eDocs", new DtbConsignmenActionAssemblyData().ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, new DtbConsignmenActionAssemblyData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Land Transport Consignment Action", new DtbConsignmenActionAssemblyData().HumanReadableName.ToString());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals("This will prevent Actions from appearing in Manage > DocManager > Allocate eDocs.", false, new DtbConsignmenActionAssemblyData().IsAllowedForUnallocatedeDocs);
		}
	}
}
