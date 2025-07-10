using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsSalesChannelController))]
	class WhsSalesChannelControllerTest : ZControllerBasherTest
	{
		public void TestControllerID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsSalesChannel, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsSalesChannel, controller.ModuleID);
		}

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WhsConfigSalesChannelDelete, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsConfigSalesChannelEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.WhsConfigSalesChannelNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsConfigSalesChannelView, Controller.GetCheckPointForView(null));
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsSalesChannel), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.WhsSalesChannel;
	}
}
