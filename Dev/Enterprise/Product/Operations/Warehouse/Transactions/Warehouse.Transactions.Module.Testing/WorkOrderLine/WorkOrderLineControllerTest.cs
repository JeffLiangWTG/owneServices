using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WorkOrderLineController))]
	class WorkOrderLineControllerTest : ZControllerBasherTest
	{
		#region TestGetForm

		public void TestGetForm()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var workOrder = helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			helper.CreateProductBOM(data.Part1, data.Part2);
			var workOrderLine = helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 10m);
			Factory.Save();

			using (var form = Controller.ShowEditForm(workOrderLine))
			{
				AssertEquals(typeof(OrderLineEntryForm), form.GetType());
			}
		}

		#endregion

		#region TestNewForm

		public override void TestNewForm()
		{
			// Creating New WorkOrderLines should not be done with a controller, this test is irrelevant
			Assert(true);
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(WhsWorkOrderLine), Controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WhsWorkOrderDelete, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsWorkOrderEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.None, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsWorkOrderView, Controller.GetCheckPointForView(null));
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsWorkOrderLine;
		}

		#endregion
	}
}
