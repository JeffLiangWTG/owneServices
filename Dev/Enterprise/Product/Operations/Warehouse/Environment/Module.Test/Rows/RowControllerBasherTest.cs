using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(RowController))]
	class RowControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestID()
		{
			var controller = new RowController();
			AssertEquals(ControllerIDs.WhsConfigRow, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = new RowController();
			AssertEquals(ModuleIDs.WhsConfigRow, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new RowController();
			AssertEquals(typeof(WhsRow), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigRow;
		}
	}
}
