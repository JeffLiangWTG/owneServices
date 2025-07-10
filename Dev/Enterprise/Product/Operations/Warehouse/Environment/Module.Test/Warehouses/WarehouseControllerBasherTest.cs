using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WarehouseController))]
	class WarehouseControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestID()
		{
			var controller = new WarehouseController();
			AssertEquals(ControllerIDs.WhsConfigWarehouse, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = new WarehouseController();
			AssertEquals(ModuleIDs.WhsConfigWarehouse, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new WarehouseController();
			AssertEquals(typeof(WhsWarehouse), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigWarehouse;
		}
	}
}
