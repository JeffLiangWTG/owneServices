using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class OrderLineEntryUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestSerialNumberColumnInitialised

		public void TestSerialNumberColumnInitialised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient();
			var order = Helper.CreateWhsOrder(client.PK, whs.PK);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1.PK, 1);

			using (var form = new ZForm(orderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();

				var serialNoColumnName = nameof(WhsPickLine.Inventory) + "+" + nameof(WhsInventoryView.WI_SerialNumber);
				AssertEquals("Serial Number Column Availability.", false,
					control.CrossDockedInventoryModuleButtonGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == serialNoColumnName).IsUnavailable);
			}
		}

		#endregion

		#region TestSerialNumberControl

		public void TestSerialNumberControl()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient();
			var order = Helper.CreateWhsOrder(client.PK, whs.PK);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1.PK, 1);

			using (var form = new ZForm(orderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();

				var serialNumberTextBox = GUITestHelper.FindControl<ZTextBox>(control.Controls, "SerialNumberTextBox");
				AssertEquals("Serial Number Text Box Visiblity.", true, serialNumberTextBox.Visible);
			}
		}

		#endregion

		#region TestCrossDockedInventoryModuleButtonGrid_Attaching

		public void TestCrossDockedInventoryModuleButtonGrid_Attaching()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			Factory.Save();

			using (var form = new ZForm(orderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();

				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(control.CrossDockedInventoryModuleButtonGrid.Controls, "toolStrip");
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).FirstOrDefault();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				attachButton.PerformClick();
				AssertEquals("Order Line is fully reserved.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(control.CrossDockedInventoryModuleButtonGrid.LastShownAttachPopupForTesting);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				orderLine.WE_TransactionQuantity = 11m;
				attachButton.PerformClick();
				AssertEquals("Save Order Line before Attaching.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(control.CrossDockedInventoryModuleButtonGrid.LastShownAttachPopupForTesting);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				attachButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNotNull(control.CrossDockedInventoryModuleButtonGrid.LastShownAttachPopupForTesting);
				AssertEquals(ModuleIDs.WhsPickLine, control.CrossDockedInventoryModuleButtonGrid.LastShownAttachPopupForTesting.CurrentModule.ModuleID);
				control.CrossDockedInventoryModuleButtonGrid.LastShownAttachPopupForTesting = null; // clean-up

				var pick = Helper.CreatePickNew(order);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				attachButton.PerformClick();
				AssertEquals("Cannot Cross Dock a Picked Order.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(control.CrossDockedInventoryModuleButtonGrid.LastShownAttachPopupForTesting);
			}
		}

		public void TestCrossDocketInventoryModuleButtonGrid_PickableDocketDeletedInOtherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient();
			var order = Helper.CreateWhsOrder(client.PK, whs.PK);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1.PK, 1);
			Factory.Save();
			using (var form = new ZForm(orderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();
				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(control.CrossDockedInventoryModuleButtonGrid.Controls, "toolStrip");
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).FirstOrDefault();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var otherFactory = new BusinessObjectFactory();
				otherFactory.Load<WhsPickableDocket>(orderLine.WE_WD).Delete();
				otherFactory.Save();
				AssertNoExceptionThrown(() => attachButton.PerformClick());
			}
		}

		#endregion

		#region TestCrossDockedInventoryModuleButtonGrid_Detaching

		public void TestCrossDockedInventoryModuleButtonGrid_Detaching()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			Factory.Save();

			using (var form = new ZForm(orderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();

				orderLine.WE_LineComment = "make changes";
				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(control.CrossDockedInventoryModuleButtonGrid.Controls, "toolStrip");
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).FirstOrDefault();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				detachButton.PerformClick();
				AssertEquals("Save Order Line before Detaching.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(1, orderLine.ReservedPickLines.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Factory.Save();
				detachButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(0, orderLine.ReservedPickLines.Count);
			}
		}

		#endregion

		#region TestCrossDockedInventoryModuleButtonGrid_Visibility

		public void TestCrossDockedInventoryModuleButtonGrid_Visibility()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreateProductBOM(data.Part1, data.Part2);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 10m);
			Factory.Save();

			using (var form = new ZForm(orderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Cross Dock Grid should be visible for Order Lines.", true, control.CrossDockedInventoryModuleButtonGrid.Visible);
			}

			using (var form = new ZForm(workOrderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Cross Dock Grid should not be visible for Work Order Lines.", false, control.CrossDockedInventoryModuleButtonGrid.Visible);
			}
		}

		#endregion

		#region TestCrossDockedInventoryModuleButtonGrid_DoesNotHaveOperationActions

		public void TestCrossDockedInventoryModuleButtonGrid_DoesNotHaveOperationActions()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			using (var form = new ZForm(orderLine))
			{
				var control = new OrderLineEntryUserControl();
				form.Controls.Add(control);
				form.Show();

				var crossDockGridForm = GUITestHelper.FindControl<CrossDockedInventoryAttachedToOrderLineGrid>(control.Controls, "CrossDockedInventoryModuleButtonGrid");
				var crossDockGrid = GUITestHelper.FindControl<ZGridWithoutColumnStylesSerialisation>(crossDockGridForm.Controls, "Grid");
				var actionMenuItems = crossDockGrid.ContextMenu.MenuItems.FindByText("Operational Actions");
				AssertNull("Cross-Dock grid should not have Operational Actions", actionMenuItems);
			}
		}

		#endregion

		#region TestCrossDockGridAndButtonsReadOnlyIfOrderIsCancelledOrPickFinalised

		public void TestCrossDockGridAndButtonsReadOnlyIfOrderIsCancelledOrPickFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.CancelReactivateDocket(); // Cancel Order
			AssertGridAndButtonsReadOnly(orderLine, true);

			order.CancelReactivateDocket(); // Reactivate Order
			AssertGridAndButtonsReadOnly(orderLine, false);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			AssertEquals(true, order.IsFinalised);
			AssertGridAndButtonsReadOnly(orderLine, false);

			pick.FinalisePick();
			AssertEquals(true, pick.IsFinalised);
			AssertGridAndButtonsReadOnly(orderLine, true);
		}

		void AssertGridAndButtonsReadOnly(WhsOrderLine orderLine, bool readOnly)
		{
			using (var form = new ZForm(orderLine))
			{
				var orderLineControl = new OrderLineEntryUserControl();
				form.Controls.Add(orderLineControl);
				form.Show();

				AssertEquals(readOnly, orderLineControl.CrossDockedInventoryModuleButtonGrid.InnerGrid.ReadOnly);
				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(orderLineControl.CrossDockedInventoryModuleButtonGrid.Controls, "toolStrip");
				AssertEquals(!readOnly, toolStrip.Items.Find("NewButton", true)[0].Enabled);
				AssertEquals(!readOnly, toolStrip.Items.Find("AttachButton", true)[0].Enabled);
				AssertEquals(!readOnly, toolStrip.Items.Find("DetachButton", true)[0].Enabled);
			}
		}

		#endregion
	}
}
