using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class CrossDockedInventoryAttachedToOrderLineGridTest : WhsGuiTestCaseWithFactory
	{
		public void TestOpenEditForm_InventoryHasNoChange()
		{
			TestOpenEditFormCore(false);
		}

		public void TestOpenEditForm_InventoryHasChange()
		{
			TestOpenEditFormCore(true);
		}

		void TestOpenEditFormCore(bool isInventoryChanged)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			Factory.Save();

			if (isInventoryChanged)
			{
				inventory.WI_InDocketLineUnits = 15m;

				AssertEquals("Precondition: ", true, inventory.InDocketLine.HasChanges);
			}

			var orderController = ZControllerFactory.Create(ControllerIDs.WhsOrder);
			using (var orderForm = orderController.ShowEditForm(order))
			using (var form = new ZForm(inventory))
			{
				var grid = new CrossDockedInventoryAttachedToOrderLineGrid();

				// sets up binding source and members
				grid.ModuleID = ModuleIDs.WhsInventory;
				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(WhsInventoryView);
				bindingSourceProvider.BindingSource.SetBindingMember(grid, "ReservedPickLines");

				// grid must have at least one column
				var calcEditColumn = new ZCalcEditColumnStyleInfo();
				calcEditColumn.ColumnName = "WI_OP_PartNum";
				grid.ColumnStyles.Add(calcEditColumn);

				// exception is thrown in binding if 'Attach' or 'Detach' is visible and no BindToList has been set
				grid.ShowAttachButton = false;
				grid.ShowDetachButton = false;
				form.Controls.Add(grid);

				form.Show();

				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(grid.Controls, "toolStrip");
				var editButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).FirstOrDefault();
				AssertNull(grid.LastShownZForm);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(() =>
				{
					editButton.PerformClick();

					if (isInventoryChanged)
					{
						AssertEquals("Should show message to user if Inventory has changes",
							"The form must be saved before a e.g, Order can be edited or a new e.g, Order can be created. Do you wish to save the form?", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertNull("Should not show message to user if Inventory has no changes", UnitTestUserNotification.Instance.LastMessage.Text);
					}

					using (var lastShownForm = (ZForm)grid.LastShownZForm)
					{
						AssertNotNull(lastShownForm);
						AssertEquals(typeof(InventoryForm), lastShownForm.GetType());
					}
				});
			}
		}
	}

	[TestedType(typeof(CrossDockedInventoryAttachedToOrderLineGrid))]
	class CrossDockedInventoryAttachedToOrderLineGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
