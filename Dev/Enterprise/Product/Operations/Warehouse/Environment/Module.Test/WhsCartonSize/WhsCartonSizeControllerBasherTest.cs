using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsCartonSizeController))]
	class WhsCartonSizeControllerBasherTest : ZControllerBasherTest
	{
		#region TestID

		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsCartonSize, controller.ID);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsCartonSize, controller.ModuleID);
		}

		#endregion

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WhsConfigCartonSizeDelete, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsConfigCartonSizeEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.WhsConfigCartonSizeNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsConfigCartonSizeView, Controller.GetCheckPointForView(null));
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsCartonSize), controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsCartonSize;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var cartonSize = new WhsTestHelperFunctionsEnv(Factory).CreateWhsCartonSize("S1");
			Factory.Save();
			return cartonSize;
		}

		#endregion
	}
}
