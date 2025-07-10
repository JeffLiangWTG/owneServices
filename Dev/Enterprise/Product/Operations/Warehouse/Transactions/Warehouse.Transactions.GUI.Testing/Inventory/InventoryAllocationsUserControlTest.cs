using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.Warehouse.Transactions.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Warehouse.Transactions.GUI.Testing
{
	public class InventoryAllocationsUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestPartAttributeColumnNameChanges

		public void TestPartAttributeColumnNameChanges()
		{
			var data = new TestDataForInventory(Factory);

			data.Whs1 = Helper.CreateWarehouse("CP1", "A", 2, 1);
			data.Whs2 = Helper.CreateWarehouse("CP2", "A");
			data.Org1 = Helper.CreateClient("1");
			data.Org2 = Helper.CreateClient("2");
			data.Product1 = WhsProduct.GetWhsProduct(data.Part1 = Helper.CreateProduct(data.Org1, "P1"));
			data.Product2 = WhsProduct.GetWhsProduct(data.Part2 = Helper.CreateProduct(data.Org2, "P2"));
			Helper.CreateProductClientRelationShip(data.Org2, data.Part1);
			Helper.CreateProductClientRelationShip(data.Org1, data.Part2);

			data.Org2.MiscServ.OM_IMPartAttrib1Name = "Batch #";
			data.Org2.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			data.Org2.MiscServ.OM_IMPartAttrib2Name = "Vehicle #";
			data.Org2.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			data.Org2.MiscServ.OM_IMPartAttrib3Name = "Colour";
			data.Org2.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			data.Org2.MiscServ.OM_IMUseExpiryDate = true;
			data.Org2.MiscServ.OM_IMUsePackingDate = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals(true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org2, data.Whs2, "R2", data.Part2, 10m);
			AssertEquals(true, receive2.IsFinalised);
			Factory.Save();

			var serialNumberColumnName = nameof(WhsPickLine.DocketLine) + "+" + WhsDocketLineSchema.Constants.WE_SerialNumber;
			using (var form = new TestInventoryForm(receive2.Lines[0]))
			{
				form.Show();

				var userControl = form.GetInventoryAllocationsUserControl();
				var grid = userControl.FindSingle<CrossDockedOrderLineAttachedToInventoryGrid>().InnerGrid;
				AssertAttributeVisibility(grid, true, true, true, true, true);
				AssertAttributeTitles(grid, "Batch #", "Vehicle #", "Colour");

				AssertEquals(false,
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == serialNumberColumnName).IsUnavailable);
			}
		}

		void AssertAttributeVisibility(ZGrid grid, bool expiryDate, bool packingDate, bool partAttribute1, bool partAttribute2, bool partAttribute3)
		{
			AssertEquals("Expiry visibility incorrect", expiryDate, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_ExpiryDate.Name].IsVisible);
			AssertEquals("Packing visibility incorrect", packingDate, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_PackingDate.Name].IsVisible);
			AssertEquals("Part1 visibility incorrect", partAttribute1, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_PartAttrib1.Name].IsVisible);
			AssertEquals("Part2 visibility incorrect", partAttribute2, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_PartAttrib2.Name].IsVisible);
			AssertEquals("Part3 visibility incorrect", partAttribute3, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_PartAttrib3.Name].IsVisible);
		}

		void AssertAttributeTitles(ZGrid grid, string partAttribute1, string partAttribute2, string partAttribute3)
		{
			if (!string.IsNullOrEmpty(partAttribute1))
			{
				AssertEquals("Part1 title incorrect", partAttribute1, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_PartAttrib1.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(partAttribute2))
			{
				AssertEquals("Part2 title incorrect", partAttribute2, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_PartAttrib2.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(partAttribute3))
			{
				AssertEquals("Part3 title incorrect", partAttribute3, grid.Columns["DocketLine+" + WhsDocketLineSchema.WE_PartAttrib3.Name].ColumnStyle.HeaderText);
			}
		}

		#endregion

		#region TestCrossDockedOrdersAndWorkOrdersDontBlowUp

		public void TestCrossDockedOrdersAndWorkOrdersDontBlowUp()
		{
			// create bom product and inventory

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bomProduct = data.Part1;
			Helper.CreateProductBOM(bomProduct, data.Part2, 1m, "UNT");

			// create receive
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 5m, false, false);
			Factory.Save();
			AssertEquals("Precondition - receive should not be finalised", false, receive.IsFinalised);

			// create work order and cross dock
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			Helper.CreateReservePickLine(workOrderLine.ChildComponentLines.ElementAt(0), receive.Inventory[0], 1m);

			// create order and cross dock
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, data.Org1.PK, "W2");
			var orderLine = Helper.CreateWhsOrderLine(order, bomProduct, 1m);
			Helper.CreateReservePickLine(orderLine, receive.Inventory[0], 1m);
			Factory.Save();

			AssertEquals("Precondition", 2, receive.Inventory[0].ReservedPickLines.Count);
			using (var form = new TestInventoryForm(receive.Lines[0]))
			{
				form.Show();
				form.GetInventoryAllocationsUserControl();
				Application.DoEvents(); // IMPORTANT: Test wouldn't blow up due to incorrect List attribute without this part
			}
		}

		#endregion

		#region TestGrid_ProperlyDisplayConsignee

		public void TestGrid_ProperlyDisplayConsignee()
		{
			const string ConsigneeColumnName = "Consignee";

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var consignee1 = Helper.CreateClient("CONSIGNEE_1");
			var consignee2 = Helper.CreateClient("CONSIGNEE_2");

			var order1 = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee1.PK, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 6m);
			orderLine1.ReserveStockIfAbleTo(inventory, 6m);

			var order2 = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee2.PK, "O2");
			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			orderLine2.ReserveStockIfAbleTo(inventory, 4m);
			AssertEquals("Precondition", 10m, inventory.WI_CrossDockQuantity);

			using (var form = new TestInventoryForm(inventory.InDocketLine))
			{
				form.Show();

				var userControl = form.GetInventoryAllocationsUserControl();
				var grid = userControl.FindSingle<CrossDockedOrderLineAttachedToInventoryGrid>().InnerGrid;
				AssertEquals("Precondition - should have 2 cross-dock lines.", 2, grid.VisibleRowCount);

				grid.Sort = WhsPickLine.Schema.ConsigneeNameOrPK;

				for (int i = 0; i < grid.Columns.Count; i++)
				{
					var column = grid.Columns[i];
					if (column.ColumnStyle.HeaderText == ConsigneeColumnName)
					{
						grid.CurrentCell = new DataGridCell(0, i);
						grid.BeginEdit(column.ColumnStyle, 0);
						AssertEquals("Consignee", grid.LastFocusedColumn.HeaderText);
						AssertEquals("CONSIGNEE_1", grid.LastFocusedColumn.TextBox.Text);

						grid.CurrentCell = new DataGridCell(1, i);
						grid.BeginEdit(column.ColumnStyle, 1);
						AssertEquals("Consignee", grid.LastFocusedColumn.HeaderText);
						AssertEquals("CONSIGNEE_2", grid.LastFocusedColumn.TextBox.Text);

						break;
					}
				}
			}
		}

		#endregion

		#region TestInventoryForm class

		internal class TestInventoryForm : InventoryForm
		{
			public TestInventoryForm(WhsDocketLine inventoryLine)
				: base(inventoryLine)
			{
			}

			public InventoryAllocationsUserControl GetInventoryAllocationsUserControl()
			{
				MainTabControl.SelectTab(base.AllocationsTabPage);
				return base.inventoryAllocationsUserControl1;
			}
		}

		#endregion
	}
}
