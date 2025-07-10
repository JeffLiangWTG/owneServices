using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(AllocationRouteModule))]
	class AllocationRouteModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ContractAllocationRoutes;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		public void TestCannotCreateNewAllocationRouteFromPopUp()
		{
			using (var allocationRouteModule = new AllocationRouteModule())
			{
				AssertEquals("'New' button should be hidden", false, allocationRouteModule.AllowNew);
			}
		}

		public void TestCannotDeleteAllocationRouteFromPopUp()
		{
			using (var allocationRouteModule = new AllocationRouteModule())
			{
				AssertEquals("'Delete' button should be hidden", false, allocationRouteModule.AllowDelete);
			}
		}
	}
}
