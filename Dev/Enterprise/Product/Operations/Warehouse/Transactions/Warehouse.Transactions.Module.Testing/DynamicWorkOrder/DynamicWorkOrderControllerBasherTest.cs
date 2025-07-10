using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(DynamicWorkOrderController))]
	internal class DynamicWorkOrderControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestID()
		{
			var controller = new DynamicWorkOrderController();
			AssertEquals(ControllerIDs.WhsDynamicWorkOrder, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = new DynamicWorkOrderController();
			AssertEquals(ModuleIDs.WhsDynamicWorkOrder, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new DynamicWorkOrderController();
			AssertEquals(typeof(WhsDynamicWorkOrder), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsDynamicWorkOrder;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var docket = helper.CreateWhsDynamicWorkOrder(helper.CreateClient(), helper.CreateWarehouse("WHS1"));
			Factory.Save();
			return docket;
		}
	}
}
