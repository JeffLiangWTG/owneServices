using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class InventoryBOMComponentUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestGrid_ProperlyDisplayProductAndAttributes

		public void TestGrid_ProperlyDisplayProductAndAttributes()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 2);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var component1 = Helper.CreateProduct(client, "component1");
			var component2 = Helper.CreateProduct(client, "component2");

			Factory.Save();

			Helper.SetClientAllAttributeType(client, true);
			Helper.SetClientAttributeType(client, AttributeNumber.One, true, "Size");
			Helper.SetClientAttributeType(client, AttributeNumber.Two, true, "Shape");
			Helper.SetClientAttributeType(client, AttributeNumber.Three, true, "Colour");
			Helper.SetProductAllAttributeUse(client, component1, true);

			Helper.CreateProductBOM(parentProduct1, component1);
			Helper.CreateProductBOM(parentProduct1, component2);

			var today = ZDate.Today;
			var receive1 = Helper.CreateWhsReceive(client, whs, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive1, component1, 1m);
			inventory1.WE_PartAttrib1 = "A1";
			inventory1.WE_PartAttrib2 = "A2";
			inventory1.WE_PartAttrib3 = "A3";
			inventory1.WE_SerialNumber = "S1";
			inventory1.WE_ExpiryDate = today.AddDays(7);
			inventory1.WE_PackingDate = today.AddDays(-7);

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();

			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs, "R2", component2, 1m);

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder);
			workOrder.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory = workOrder.Receive.Inventory[0];

			using (var form = new TestInventoryForm(woInventory.InDocketLine))
			{
				form.Show();

				var userControl = form.GetInventoryComponentUserControl();
				var grid = userControl.FindSingle<ZGrid>();
				AssertEquals("Precondition - should have 2 component info lines.", 2, grid.VisibleRowCount);

				grid.Sort = WhsDocketLine.Schema.ProductCode;

				for (var i = 0; i < grid.Columns.Count; i++)
				{
					var column = grid.Columns[i];
					if (column.ColumnStyle.HeaderText == "Product Code")
					{
						AssertEquals(true, column.IsVisible);
						AssertColumnValues(grid, column, i, "COMPONENT1", "COMPONENT2");
					}
					else if (column.ColumnStyle.HeaderText == "Size")
					{
						AssertEquals(true, column.IsVisible);
						AssertColumnValues(grid, column, i, "A1", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Shape")
					{
						AssertEquals(true, column.IsVisible);
						AssertColumnValues(grid, column, i, "A2", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Colour")
					{
						AssertEquals(true, column.IsVisible);
						AssertColumnValues(grid, column, i, "A3", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Serial #")
					{
						AssertEquals(true, column.IsVisible);
						AssertColumnValues(grid, column, i, "S1", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Expiry Date")
					{
						AssertEquals(true, column.IsVisible);
						AssertDateColumnValues(grid, column, i, today.AddDays(7), ZDate.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Packing Date")
					{
						AssertEquals(true, column.IsVisible);
						AssertDateColumnValues(grid, column, i, today.AddDays(-7), ZDate.Empty);
					}
					else
					{
						AssertEquals($"Non Customs inventory {column.ColumnStyle.HeaderText} should hide customs details.", false, column.IsVisible);
					}
				}
			}
		}

		public void TestGrid_Customs_ProperlyDisplayProductAndAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var otherComponent = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(data.Part2, data.Part1);
			Helper.CreateProductBOM(data.Part2, otherComponent);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].WE_WL = row.Locations[0].PK;
			receive.Lines[0].WE_BondedEntryKey = "KEY";

			var today = ZDate.Today;
			var customsData = receive.Lines[0].CustomsData;
			customsData.WB_EntryKey = "ENTRYNUM";
			customsData.WB_DeclarationReference = "DEC";
			customsData.WB_InwardStyle = "STY";
			customsData.WB_InwardProcedure = "PRO";
			customsData.WB_AddInfo = "ADD";
			customsData.WB_CustomsUnitOfQty = "UNT";
			customsData.WB_EntryLineNo = 1;
			customsData.WB_CustomsQty = 5m;
			customsData.WB_ValueForDuty = 30m;
			customsData.WB_TILV = 3m;
			customsData.WB_EntryDate = today;
			customsData.WB_CustomsDeadline = today;
			customsData.WB_RN_NKCountryOfOrigin = "NZ";

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", otherComponent, 5m, allocateLocations: false, finalise: false);
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM";
			receive2.Lines[0].WE_WL = row.Locations[0].PK;
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var woInventory = workOrder.Receive.Inventory[0];

			using (var form = new TestInventoryForm(woInventory.InDocketLine))
			{
				form.Show();

				var userControl = form.GetInventoryComponentUserControl();
				var grid = userControl.FindSingle<ZGrid>();
				AssertEquals("Precondition - should have 2 component info lines.", 2, grid.VisibleRowCount);

				grid.Sort = WhsDocketLine.Schema.ProductCode;

				for (var i = 0; i < grid.Columns.Count; i++)
				{
					var column = grid.Columns[i];

					if (column.ColumnStyle.HeaderText == "Product Code")
					{
						AssertColumnValues(grid, column, i, "P1", "P3");
					}
					else if (column.ColumnStyle.HeaderText == "Inwards Entry")
					{
						AssertColumnValues(grid, column, i, "KEY", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Entry Number")
					{
						AssertColumnValues(grid, column, i, "ENTRYNUM", "ENTRYNUM");
					}
					else if (column.ColumnStyle.HeaderText == "Declaration Reference")
					{
						AssertColumnValues(grid, column, i, "DEC", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Inward Style")
					{
						AssertColumnValues(grid, column, i, "STY", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Inward Procedure")
					{
						AssertColumnValues(grid, column, i, "PRO", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Customs Add Info")
					{
						AssertColumnValues(grid, column, i, "ADD", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Customs Unit Quantity (WRU)")
					{
						AssertColumnValues(grid, column, i, "UNT", string.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Entry Line No")
					{
						AssertColumnValues(grid, column, i, "1", "0");
					}
					else if (column.ColumnStyle.HeaderText == "Customs Quantity")
					{
						AssertDecimalColumnValues(grid, column, i, 5m, 0m);
					}
					else if (column.ColumnStyle.HeaderText == "Value For Duty")
					{
						AssertDecimalColumnValues(grid, column, i, 30m, 0m);
					}
					else if (column.ColumnStyle.HeaderText == "T&I Line Value")
					{
						AssertDecimalColumnValues(grid, column, i, 3m, 0m);
					}
					else if (column.ColumnStyle.HeaderText == "Entry Declaration Date")
					{
						AssertDateColumnValues(grid, column, i, today, ZDate.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Customs Deadline")
					{
						AssertDateColumnValues(grid, column, i, today, ZDate.Empty);
					}
					else if (column.ColumnStyle.HeaderText == "Country/Region of Origin")
					{
						AssertColumnValues(grid, column, i, "NZ", string.Empty);
					}
					else
					{
						AssertEquals($"Attribute column {column.ColumnStyle.HeaderText} should not be visible.", false, column.IsVisible);
					}
				}
			}
		}

		void AssertColumnValues(ZGrid grid, ZGridColumn column, int columnIndex, string firstLineValue, string secondLineValue)
		{
			grid.CurrentCell = new DataGridCell(0, columnIndex);
			grid.BeginEdit(column.ColumnStyle, 0);
			AssertEquals(firstLineValue, grid.LastFocusedColumn.TextBox.Text);

			grid.CurrentCell = new DataGridCell(1, columnIndex);
			grid.BeginEdit(column.ColumnStyle, 1);
			AssertEquals(secondLineValue, grid.LastFocusedColumn.TextBox.Text);
		}

		void AssertDecimalColumnValues(ZGrid grid, ZGridColumn column, int columnIndex, decimal firstLineValue, decimal secondLineValue)
		{
			grid.CurrentCell = new DataGridCell(0, columnIndex);
			grid.BeginEdit(column.ColumnStyle, 0);
			AssertDecimalField(firstLineValue);

			grid.CurrentCell = new DataGridCell(1, columnIndex);
			grid.BeginEdit(column.ColumnStyle, 1);
			AssertDecimalField(secondLineValue);

			void AssertDecimalField(decimal expectedDecimalValue)
			{
				if (decimal.TryParse(grid.LastFocusedColumn.TextBox.Text, out var actualValue1))
				{
					AssertEquals(expectedDecimalValue, actualValue1);
				}
				else
				{
					Assert($"Decimal value {grid.LastFocusedColumn.TextBox.Text} could not be parsed.", false);
				}
			}
		}

		void AssertDateColumnValues(ZGrid grid, ZGridColumn column, int columnIndex, ZDate firstLineValue, ZDate secondLineValue)
		{
			grid.CurrentCell = new DataGridCell(0, columnIndex);
			grid.BeginEdit(column.ColumnStyle, 0);
			AssertDateField(firstLineValue);

			grid.CurrentCell = new DataGridCell(1, columnIndex);
			grid.BeginEdit(column.ColumnStyle, 1);
			AssertDateField(secondLineValue);

			void AssertDateField(ZDate expectedDate)
			{
				if (DateTime.TryParse(grid.LastFocusedColumn.TextBox.Text, out var actualValue1))
				{
					AssertEquals(expectedDate, actualValue1);
				}
				else
				{
					AssertEquals(string.Empty, grid.LastFocusedColumn.TextBox.Text);
				}
			}
		}

		#endregion

		#region TestUserControl_DoubleClick

		public void TestUserControl_DoubleClick_Receive()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 2);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var component1 = Helper.CreateProduct(client, "component1");

			Factory.Save();

			Helper.CreateProductBOM(parentProduct1, component1);

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", component1, 1m);

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder);
			workOrder.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory = workOrder.Receive.Inventory[0];

			using (var form = new TestInventoryForm(woInventory.InDocketLine))
			{
				form.Show();

				var userControl = form.GetInventoryComponentUserControl();
				var grid = userControl.FindSingle<ZGrid>();
				AssertEquals("Precondition - should have single component info line.", 1, grid.VisibleRowCount);

				InventoryHelper.lastShowForm = null;
				grid.Select(0);
				grid.PerformMouseDoubleClickForTest(0);

				var lastForm = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
				AssertEquals(receive.PK, ((BusinessObject)lastForm.BusinessEntity).PK);
				lastForm.Dispose();
				InventoryHelper.lastShowForm = null;
			}
		}

		public void TestUserControl_DoubleClick_AdjustmentIn()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 2);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var component1 = Helper.CreateProduct(client, "component1");

			Factory.Save();

			Helper.CreateProductBOM(parentProduct1, component1);

			var adjustment = Helper.CreateWhsAdjustment(client, whs, "A1");
			Helper.CreateWhsAdjustmentLine(adjustment, component1, 1m, whs.FindLocation("A-1"));
			adjustment.FinaliseDocket();

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder);
			workOrder.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory = workOrder.Receive.Inventory[0];

			using (var form = new TestInventoryForm(woInventory.InDocketLine))
			{
				form.Show();

				var userControl = form.GetInventoryComponentUserControl();
				var grid = userControl.FindSingle<ZGrid>();
				AssertEquals("Precondition - should have single component info line.", 1, grid.VisibleRowCount);

				InventoryHelper.lastShowForm = null;
				grid.Select(0);
				grid.PerformMouseDoubleClickForTest(0);

				var lastForm = InventoryHelper.lastShowForm[0] as AdjustmentEntryForm;
				AssertEquals(adjustment.PK, ((BusinessObject)lastForm.BusinessEntity).PK);
				lastForm.Dispose();
				InventoryHelper.lastShowForm = null;
			}
		}

		public void TestUserControl_DoubleClick_Transfer()
		{
			var whs1 = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var component1 = Helper.CreateProduct(client, "component1");

			Factory.Save();

			Helper.CreateProductBOM(parentProduct1, component1);

			var receive = Helper.CreateWhsReceive(client, whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, component1, 1m, whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			Factory.Save();

			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(client, whs2, "1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, component1, 1m, "A-1", whs1.PK, "A-2");
			transfer.FinaliseDocket();

			transfer.FinaliseDocket();

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(client, whs2, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder);
			workOrder.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory = workOrder.Receive.Inventory[0];

			using (var form = new TestInventoryForm(woInventory.InDocketLine))
			{
				form.Show();

				var userControl = form.GetInventoryComponentUserControl();
				var grid = userControl.FindSingle<ZGrid>();
				AssertEquals("Precondition - should have single component info line.", 1, grid.VisibleRowCount);

				InventoryHelper.lastShowForm = null;
				grid.Select(0);
				grid.PerformMouseDoubleClickForTest(0);

				var lastForm = InventoryHelper.lastShowForm[0] as TransferEntryForm;
				AssertEquals(transfer.PK, ((BusinessObject)lastForm.BusinessEntity).PK);
				lastForm.Dispose();
				InventoryHelper.lastShowForm = null;
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

			public InventoryBOMComponentsUserControl GetInventoryComponentUserControl()
			{
				MainTabControl.SelectTab(base.ComponentTabPage);
				return base.inventoryBOMComponentsUserControl;
			}
		}

		#endregion
	}
}
