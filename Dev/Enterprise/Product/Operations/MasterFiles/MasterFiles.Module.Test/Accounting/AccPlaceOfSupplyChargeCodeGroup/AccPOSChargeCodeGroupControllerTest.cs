using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccPOSChargeCodeGroupController))]
	sealed class AccPOSChargeCodeGroupControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.AccPlaceOfSupplyChargeCodeGroup;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			Factory.Save();
			return group;
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new AccPOSChargeCodeGroupControllerForTest();
			AssertEquals("For New", Env.Security.POSChargeCodeGroupsNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.POSChargeCodeGroups, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.POSChargeCodeGroupsModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.POSChargeCodeGroupsDelete, controller.CheckPointForDelete);
		}
	}
}
