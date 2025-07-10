using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(LocationTypeController))]
	class LocationTypeControllerBasherTest : WhsControllerBaseBasherTest
	{
		#region TestID

		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsConfigLocationType, controller.ID);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsConfigLocationType, controller.ModuleID);
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsLocationType), controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigLocationType;
		}

		#endregion
	}
}
