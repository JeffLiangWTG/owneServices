using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(OrderLineEntryForm))]
	class OrderLineEntryFormTest : ZFormBasherTest
	{
		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);

			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new OrderLineEntryForm(order.Lines[0]));
		}

		#endregion

		#region TestOrderDetailsAreReadOnly

		public void TestOrderDetailsAreReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderLineEntryForm(orderLine))
			{
				form.Show();

				var clientGuidFindBox = GUITestHelper.FindControl<ZGuidFindBox>(form.Controls, "ClientGuidFindBox");
				var warehouseGuidFindBox = GUITestHelper.FindControl<ZGuidFindBox>(form.Controls, "WarehouseGuidFindBox");
				var externalReferenceTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "ExternalReferenceTextBox");
				AssertEquals("Client FindBox should be Read Only on Order Line Form.", true, clientGuidFindBox.ReadOnly);
				AssertEquals("Warehouse FindBox should be Read Only on Order Line Form.", true, warehouseGuidFindBox.ReadOnly);
				AssertEquals("Order Number TextBox should be Read Only on Order Line Form.", true, externalReferenceTextBox.ReadOnly);
			}
		}

		#endregion

		#region TestShowOrderClick

		public void TestShowOrderClick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			Factory.Save();

			using (var form = new OrderLineEntryForm(orderLine))
			{
				form.Show();

				var controller = ZControllerFactory.Create(ControllerIDs.WhsOrder);
				AssertNull(controller.GetOpenedForm(order));

				orderLine.WE_LineComment = "Set Has Changes";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var showOrderButton = GUITestHelper.FindControl<ZButton>(form.Controls, "ShowOrderButton");
				showOrderButton.PerformClick();
				AssertEquals("Save this form first before going to the Order.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have been an error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(controller.GetOpenedForm(order));

				Factory.Save();
				AssertEquals("Precondition", false, orderLine.HasChanges);

				showOrderButton.PerformClick();
				using (var orderForm = controller.GetOpenedForm(order))
				{
					AssertNotNull(orderForm);
					AssertEquals("Order Form should be modal to OrderLine Form.", form, orderForm.Owner);
				}
			}
		}

		#endregion

		#region TestGetFormCaption

		public void TestGetFormCaption_WhenPickableDocketIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			orderLine.WE_WD = ZGuid.Empty;

			using (var orderLineEntryForm = new OrderLineEntryForm(orderLine))
			{
				AssertNoExceptionThrown(() => { var caption = orderLineEntryForm.FormCaption; });
			}
		}

		public void TestGetFormCaption_WhenBusinessEntityIsNull()
		{
			using (var orderLineEntryForm = new OrderLineEntryForm(null))
			{
				AssertNoExceptionThrown(() => { var caption = orderLineEntryForm.FormCaption; });
			}
		}

		#endregion

		#region TestFormShouldNotAllowNewOrderLines

		public void TestFormShouldNotAllowNewOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			Factory.Save();

			using (var form = new OrderLineEntryForm(orderLine))
			{
				form.Show();
				AssertEquals("Display mode should be NewSaved", ODisplayMode.NewSaved, form.DisplayMode);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var order = Factory.New<WhsOrder>();
			var line = order.Lines.AddNew();
			line.HasChanges = false;

			return new OrderLineEntryForm(line);
		}

		#endregion
	}
}
