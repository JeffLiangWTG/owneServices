using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickManagerForWhsPickTest : WhsGuiTestCaseWithFactory
	{
		public void TestPickOrders()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var manager = new PickManagerForWhsPick(form, pick);
				manager.PickOrders();
				AssertEquals("Pick P00000001 has been created.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			pick.PickOrders();
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		#region TestPickOrders_AutoPickWhenPartiallyPicked

		public void TestPickOrders_AutoPickWhenPartiallyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);

				manager.PickOrders();

				AssertNull("When is already in pick form it should not try to load pick page again.", manager.LastUsedPickControllerForTest);
			}
		}

		#endregion

	}
}
