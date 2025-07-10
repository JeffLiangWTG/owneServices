using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(VASOrderController))]
	public class VASOrderControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsVASOrder, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsVASOrder, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsVASOrder), controller.TypeOfTopLevelBusinessObject);
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsVASOrder;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();
			return vasOrder;
		}

		#endregion
	}
}
