using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class CrossDockedOrderLineAttachedToInventoryGridTest : WhsGuiTestCaseWithFactory
	{
		#region TestEditOnModuleButtonGrid

		public void TestEditOnModuleButtonGrid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			Factory.Save();

			var orderController = ZControllerFactory.Create(ControllerIDs.WhsOrder);
			using (var orderForm = orderController.ShowEditForm(order))
			using (var form = new ZForm(inventory))
			{
				var grid = new CrossDockedOrderLineAttachedToInventoryGrid();

				// sets up binding source and members
				grid.ModuleID = ModuleIDs.WhsOrderLine;
				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(WhsInventoryView);
				bindingSourceProvider.BindingSource.SetBindingMember(grid, "ReservedPickLines");

				// grid must have at least one column
				var calcEditColumn = new ZCalcEditColumnStyleInfo();
				calcEditColumn.ColumnName = "WZ_OriginalReservedQty";
				grid.ColumnStyles.Add(calcEditColumn);

				// exception is thrown in binding if 'Attach' or 'Detach' is visible and no BindToList has been set
				grid.ShowAttachButton = false;
				grid.ShowDetachButton = false;
				form.Controls.Add(grid);

				form.Show();

				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(grid.Controls, "toolStrip");
				var editButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).FirstOrDefault();
				AssertNull(grid.LastShownZForm);

				editButton.PerformClick();
				using (var lastShownForm = (ZForm)grid.LastShownZForm)
				{
					AssertNotNull(lastShownForm);
					AssertEquals(typeof(OrderLineEntryForm), lastShownForm.GetType());
					AssertEquals(typeof(WhsOrderLine), lastShownForm.BusinessEntity.GetType());
					AssertEquals(orderLine.PK, lastShownForm.BusinessEntity.Identifier);
					AssertEquals("Form should be modal to the order form if it is open.", orderForm, lastShownForm.Owner);
				}
			}
		}

		#endregion
	}

	[TestedType(typeof(CrossDockedOrderLineAttachedToInventoryGrid))]
	class CrossDockedOrderLineAttachedToInventoryGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
