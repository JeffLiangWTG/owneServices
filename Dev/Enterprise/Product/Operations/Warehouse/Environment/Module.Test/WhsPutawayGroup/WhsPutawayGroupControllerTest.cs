using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsPutawayGroupController))]
	class WhsPutawayGroupControllerTest : ZControllerBasherTest
	{
		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsConfigPutawayGroup, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsConfigPutawayGroup, controller.ModuleID);
		}

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WhsConfigPutawayGroupDelete, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsConfigPutawayGroupEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.WhsConfigPutawayGroupNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsConfigPutawayGroupView, Controller.GetCheckPointForView(null));
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsPutawayGroup), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigPutawayGroup;
		}
	}
}
