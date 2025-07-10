using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsCartonGroupController))]
	class WhsCartonGroupControllerBasherTest : ZControllerBasherTest
	{
		#region TestID

		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsCartonGroup, controller.ID);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsCartonGroup, controller.ModuleID);
		}

		#endregion

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WhsConfigCartonGroupDelete, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsConfigCartonGroupEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.WhsConfigCartonGroupNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsConfigCartonGroupView, Controller.GetCheckPointForView(null));
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsCartonGroup), controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsCartonGroup;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var cartonGroup = Factory.NewWithValidTestData<WhsCartonGroup>();
			Factory.Save();
			return cartonGroup;
		}

		#endregion
	}
}
