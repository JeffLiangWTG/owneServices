using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class FinaliseJobFormHelperTest : TestCaseWithFactory
	{
		#region TestFinaliseDocket

		public void TestFinaliseDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10);
			receive.AllocateLocationsWithMock();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var notify = new TestNotificationBuffer();
				FinaliseJobFormHelper.FinaliseDocket(receive, form, notify);

				AssertEquals(true, receive.IsFinalised);
				AssertEquals(notify, receive.NotificationManager.LastPopped);
			}
		}

		#endregion

		#region TestFinaliseDocket_WithOptionalAction

		public void TestFinaliseDocket_WithOptionalAction()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);

			using (var form = new WorkOrderEntryForm(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var notify = new TestNotificationBuffer();
				FinaliseJobFormHelper.FinaliseDocket(workOrder, form, notify, w => w.FinaliseDocketAlwaysFinalisingPick());

				AssertEquals(true, workOrder.IsFinalised);
				AssertEquals(notify, workOrder.NotificationManager.LastPopped);
			}
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
