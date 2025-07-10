using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class ReceiveDocketLinesGridUserControlTest : DocketLinesGridUserControlTest<ReceiveDocketLinesGridUserControl>
	{
		#region Constructors

		public void TestContructor()
		{
			using (var userControl = GetNewDocketLinesGridUserControl())
			{
				AssertNotNull("Duplicate MenuItem should be added", FindMenuItem(userControl.LinesGrid, "Du&plicate"));
				AssertNotNull("Find Attributes MenuItem should be added", FindMenuItem(userControl.LinesGrid, "Find &Attributes"));
				AssertNotNull("Add Override Consignee MenuItem should be added", FindMenuItem(userControl.LinesGrid, "A&dd Override Consignee"));
			}
		}

		public void TestContructor_SerialNumberSplitter()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var userControl = GetNewDocketLinesGridUserControl())
			{
				AssertNotNull("Split Serial Product Lines MenuItem should be added", FindMenu(userControl));
			}

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var userControl = GetNewDocketLinesGridUserControl())
			{
				AssertNull("Split Serial Product Lines MenuItem should **not** be added.", FindMenu(userControl));
			}

			MenuItem FindMenu(ReceiveDocketLinesGridUserControl userControl) => FindMenuItem(userControl.LinesGrid, "S&plit Lines with Serial Numbers");
		}

		#endregion

		#region TestAllocationKeyVisibility_InwardProcessingSupported

		public void TestAllocationKeyVisibility_InwardProcessingSupported()
		{
			TestAllocationKeyVisibilityCore(inwardProcessingSupported: true);
		}

		public void TestAllocationKeyVisibility_InwardProcessingNotSupported()
		{
			TestAllocationKeyVisibilityCore(inwardProcessingSupported: false);
		}

		void TestAllocationKeyVisibilityCore(bool inwardProcessingSupported)
		{
			var whs = Helper.CreateWarehouse("Bond");
			whs.WarehouseAddress.OA_RN_NKCountryCode = "FR";

			var client = Helper.CreateClient();
			var receive = Helper.CreateWhsReceive(client, whs);
			var inventoryLine = receive.Lines.AddNew();

			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(inwardProcessingSupported);

			using (ObjectFactory.Substitute(mock.Object))
			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();
				var userControl = form.UserControl;
				var grid = userControl.LinesGrid;
				var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals(!inwardProcessingSupported, columnStyles.Single(i => i.ColumnName == "WE_AllocationKey").IsUnavailable);
			}
		}

		#endregion

		#region TestBondedColumnVisibility

		public void TestBondedColumnVisibility()
		{
			var whs = Helper.CreateWarehouse("Bond");
			var client = Helper.CreateClient();
			var receive = Helper.CreateWhsReceive(client, whs);
			var inventoryLine = receive.Lines.AddNew();

			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertBondedVisibility(userControl, false);

				Helper.EnableWarehouseForBond(whs, true);
				receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				AssertBondedVisibility(userControl, true);
			}
		}

		void AssertBondedVisibility(DocketLinesGridUserControl userControl, bool visible)
		{
			var grid = userControl.LinesGrid;
			AssertEquals(visible, grid.Columns.Contains("CustomsTariffLookup"));
			AssertEquals(visible, grid.Columns.Contains("CustomsTariffItem"));
			AssertEquals(visible, grid.Columns.Contains("CustomsTariffDesc"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_EntryKey"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_EntryLineNo"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_EntryDate"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_DeclarationReference"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_CustomsDeadline"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_InwardStyle"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_InwardProcedure"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_RN_NKCountryOfOrigin"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_CustomsQty"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_CustomsUnitOfQty"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_ValueForDuty"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_TILV"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_AddInfo"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_CustomsSecondQuantity"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_CustomsSecondUnitQty"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_Tariff"));
			AssertEquals(visible, grid.Columns.Contains("CustomsData+WB_PrimaryPreference"));
		}

		#endregion

		#region TestSerialNumberColumnGrid_SerialMenu

		public void TestSerialNumberColumnGrid_SerialMenu()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();

				AssertEquals("Precondition", false, form.UserControl.SerialNumberControl.Visible);

				var serialNumberMenu = FindMenu(form.UserControl);
				serialNumberMenu.PerformClick();

				AssertEquals(true, form.UserControl.SerialNumberControl.Visible);
				AssertEquals("Hide &Serials", serialNumberMenu.Text);

				serialNumberMenu.PerformClick();
				AssertEquals(false, form.UserControl.SerialNumberControl.Visible);
			}
			MenuItem FindMenu(ReceiveDocketLinesGridUserControl userControl) => FindMenuItem(userControl.LinesGrid, "Show &Serials");
		}

		#endregion

		#region TestSelectedInventoryLines

		public void TestSelectedInventoryLines()
		{
			var docket = Factory.New<WhsReceive>();
			using (var form = new ReceiveLinesTestForm(docket))
			{
				form.Show();

				var userControl = form.UserControl;
				var inventoryLine1 = docket.Lines.AddNew();
				var inventoryLine2 = docket.Lines.AddNew();
				var inventoryLine3 = docket.Lines.AddNew();

				AssertEquals(3, userControl.SelectedInventoryLines.Count);

				userControl.LinesGrid.Select(0);
				AssertEquals(1, userControl.SelectedInventoryLines.Count);
				AssertEquals(inventoryLine1.PK, userControl.SelectedInventoryLines[0].PK);

				userControl.LinesGrid.SelectAllElements();
				AssertEquals(3, userControl.SelectedInventoryLines.Count);
				AssertEquals(inventoryLine1.PK, userControl.SelectedInventoryLines[0].PK);
				AssertEquals(inventoryLine2.PK, userControl.SelectedInventoryLines[1].PK);
				AssertEquals(inventoryLine3.PK, userControl.SelectedInventoryLines[2].PK);

				userControl.LinesGrid.UnSelect(0);
				userControl.LinesGrid.UnSelect(2);
				AssertEquals(1, userControl.SelectedInventoryLines.Count);
				AssertEquals(inventoryLine2.PK, userControl.SelectedInventoryLines[0].PK);
			}
		}

		#endregion

		#region TestGrid_ColourDeciding

		public void TestGrid_ColourDeciding()
		{
			//the arhitecure assures that the ColourDeciding attached event is run on each paint event
			//so the test is just assuring the event sets correct colours and the event is attached to the control

			var receive = Factory.New<WhsReceive>();
			var inventoryLine = receive.Lines.AddNew();
			inventoryLine.WE_ClientOrderedUnits = 10m;

			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertGridLineColorSyncronized(userControl, inventoryLine, 10m, Color.Empty);
				AssertGridLineColorSyncronized(userControl, inventoryLine, 5m, Color.LightSalmon);
				AssertGridLineColorSyncronized(userControl, inventoryLine, 20m, Color.LightGreen);
				AssertGridLineColorSyncronized(userControl, inventoryLine, 10m, Color.Empty);
			}
		}

		void AssertGridLineColorSyncronized(ReceiveDocketLinesGridUserControl userControl, WhsReceiveLine inventoryLine, ZDecimal quantity, Color color)
		{
			inventoryLine.WE_TransactionQuantity = quantity;
			var args = new ColourDecidingEventArgs(inventoryLine);
			userControl.OnColourDecidingForTest(args);
			AssertEquals(color, args.Colour);
		}

		#endregion

		#region TestGrid_CustomColourScheme_FilterBusinessObject_IsCorrect

		public void TestGrid_CustomColourScheme_FilterBusinessObject_IsCorrect()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();
				var userControl = form.UserControl;
				var colorManager = new GridColourSchemeManagerForTest(userControl.LinesGrid);
				var filter = colorManager.FilterBusinessObject;
				AssertEquals("Filter type of colour manager should match receiveline grid filter", "GridFilterStripBusinessObject", filter.GetType().Name);
			}
		}

		#endregion

		#region TestSelectRow

		public void TestSelectRow()
		{
			var receive = Factory.New<WhsReceive>();
			var inv1 = receive.Lines.AddNew();
			var inv2 = receive.Lines.AddNew();

			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();

				var userControl = form.UserControl;
				AssertEquals(userControl.LinesGrid.SelectedRowCount, 0);

				userControl.SelectRow(inv2.PK);
				AssertEquals(userControl.LinesGrid.SelectedRowCount, 1);
				AssertEquals(inv2, userControl.LinesGrid.SelectedElements[0]);

				userControl.SelectRow(inv1.PK);
				AssertEquals(userControl.LinesGrid.SelectedRowCount, 2);
				AssertEquals(inv1.PK, userControl.LinesGrid.SelectedElements[0].PK);
			}
		}

		#endregion

		#region TestShowInventoryEditForm

		public void TestShowInventoryEditForm()
		{
			var whs = Helper.CreateWarehouse("Bond");
			var client = Helper.CreateClient();
			var receive = Helper.CreateWhsReceive(client, whs);
			var part = Helper.CreateProduct(client, "TESTPART");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			Factory.Save();

			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();

				var userControl = form.UserControl;
				userControl.LinesGrid.SelectSingleElement(inv1);
				userControl.ShowInventoryEditForm();

				using (var invForm = (InventoryForm)ZControllerFactory.Create(ControllerIDs.WhsInventory).GetOpenedForm(inv1))
				{
					AssertEquals(inv1.WI_WE_InDocketLine, ((WhsDocketLine)invForm.BusinessEntity).PK);
					AssertEquals(true, ((WhsDocketLine)invForm.BusinessEntity).IsInventoryEditForm);
					AssertEquals(true, ((WhsDocketLine)invForm.BusinessEntity).Inventory[0].IsInventoryEditForm);
				}
			}
		}

		public void TestShowInventoryEditFormWithNullInventoryView()
		{
			var whs = Helper.CreateWarehouse("Bond");
			var client = Helper.CreateClient();
			var receive = Helper.CreateWhsReceive(client, whs);
			var part = Helper.CreateProduct(client, "TESTPART");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var receive2 = factory2.Load<WhsReceive>(receive.PK);

			using (var form = new ReceiveLinesTestForm(receive2))
			{
				form.Show();

				var userControl = form.UserControl;
				userControl.LinesGrid.SelectSingleElement(receive2.Lines[0]);

				receive.Lines[0].Inventory.RemoveAndDeleteAll();
				Factory.Save();

				AssertNoExceptionThrown("No exception should be thrown", () => userControl.ShowInventoryEditForm());
				AssertEquals("The selected record has been deleted by another user. It cannot be displayed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Events

		#region TestConsigneeDocAddressControl_OrgChanged

		public void TestConsigneeDocAddressControl_OrgChanged()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			using (var form = new ReceiveLinesTestForm(data.Receive11))
			{
				form.Show();

				var userControl = form.UserControl;
				var grid = userControl.LinesGrid;
				userControl.ConsigneeDocAddressControl.SetDataBinding(data.Receive11, "Lines.ConsigneeDocAddress");

				TestConsigneeDocAddressControl_OrgChangedCore(data, form, grid);

				Factory.Save(); // To Generate Docket Lines

				TestConsigneeDocAddressControl_OrgChangedCore(data, form, grid);
			}
		}

		void TestConsigneeDocAddressControl_OrgChangedCore(TestDataForInventory data, ReceiveLinesTestForm form, ZGrid grid)
		{
			var receiveLine = (WhsReceiveLine)data.Line111.InDocketLine;
			grid.Focus();
			grid.SetAllColumnsVisible(true);

			// set invalid value into memory cache.
			grid.CurrentCell = new DataGridCell(0, 0);
			SetGridCellFocus(grid, WhsReceiveLine.Schema.ConsigneeNameOrPK);
			var multiCombinationControl1 = grid.LastFocusedColumn.EditControl as ZMultiCombinationControl;
			multiCombinationControl1.CurrentEditor.Text = "INVALID";
			SendKeyToEditControl(grid, Keys.Tab);
			AssertEquals("No Organisation with such code exist, so GUI memory cache should save this value.", "INVALID", FieldInvalidTextMemory.GetInvalidText(receiveLine, WhsReceiveLine.Schema.ConsigneeNameOrPK));

			// check correct org doesn't clear the invalid value cache.
			receiveLine.ConsigneeDocAddress.OrganisationPK = data.Org1.PK;
			AssertEquals("The correct Organisation was entered, but GUI memory cache should not be cleared.", "INVALID", FieldInvalidTextMemory.GetInvalidText(receiveLine, WhsReceiveLine.Schema.ConsigneeNameOrPK));

			// check empty org clear the invalid value cache.
			receiveLine.ConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("When empty OrganisationPK is entered the GUI memory cache should be cleared of wrong organisation code.", "", FieldInvalidTextMemory.GetInvalidText(receiveLine, WhsReceiveLine.Schema.ConsigneeNameOrPK));
		}

		void SetGridCellFocus(ZGrid grid, ZString columnName)
		{
			for (int i = 0; i < grid.Columns.Count; i++)
			{
				if (grid.LastFocusedColumn.MappingName == columnName)
				{
					return;
				}
				SendKeyToEditControl(grid, Keys.Tab);
			}
		}

		void SendKeyToEditControl(ZGrid grid, Keys key)
		{
			var findBox = grid.LastFocusedColumn.EditControl as ZGridFindBox;
			if (findBox != null)
			{
				KeySender.PostKeyDown(findBox.CodeBox, findBox.CodeBox.Handle, key);
			}
			else
			{
				var dropEdit = grid.LastFocusedColumn.EditControl as ZGridDropEdit;
				if (dropEdit != null)
				{
					KeySender.PostKeyDown(dropEdit.CodeBox, dropEdit.CodeBox.Handle, key);
				}
				else
				{
					var dateEdit = grid.LastFocusedColumn.EditControl as ZDateEdit;
					if (dateEdit != null)
					{
						KeySender.PostKeyDown(dateEdit.DateTextBox, dateEdit.DateTextBox.Handle, key);
					}
					else
					{
						KeySender.PostKeyDown(grid.LastFocusedColumn.TextBox.Text, grid.LastFocusedColumn.TextBox.Handle, key);
					}
				}
			}
			Application.DoEvents();
		}

		#endregion

		#region TestAddConsigneeOverrideMenuItemClick

		public void TestAddConsigneeOverrideMenuItemClick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (var form = new ReceiveLinesTestForm(data.Receive11))
			{
				form.Show();

				var userControl = form.UserControl;
				AssertEquals(false, userControl.ConsigneeDocAddressControl.Visible);

				var consigneeOverrideMenuItem = userControl.LinesGrid.ContextMenu.MenuItems.FindByText("Add Override Consignee");
				consigneeOverrideMenuItem.PerformClick();
				AssertEquals(true, userControl.ConsigneeDocAddressControl.Visible);

				consigneeOverrideMenuItem.PerformClick();
				AssertEquals(false, userControl.ConsigneeDocAddressControl.Visible);
			}
		}

		#endregion

		#region TestList_ListChanged

		public void TestList_ListChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = receive.Lines.AddNew();
			receiveLine.WE_OP = ZGuid.Invalid;
			receiveLine.ProductCode = "TEMP";

			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();

				AssertEquals("Precondition", "", FieldInvalidTextMemory.GetInvalidText(receiveLine, WhsDocketLineSchema.WE_OP.Name));

				var clone = (WhsReceiveLine)receiveLine.Clone();
				receive.Lines.Add(clone); // it's done automatically at Grid when you use Duplicate Popup Menu item.
				AssertEquals("TEMP", FieldInvalidTextMemory.GetInvalidText(clone, WhsDocketLineSchema.WE_OP.Name));
			}
		}

		#endregion

		#region TestList_ListChanged_Duplicate

		public void TestList_ListChanged_Duplicate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();

				var userControl = form.UserControl;
				var grid = userControl.LinesGrid;
				grid.Select(0);
				var duplicateMenu = grid.ContextMenu.MenuItems.FindByText("Duplicate");
				AssertNoExceptionThrown(() => duplicateMenu.PerformClick());
			}
		}

		#endregion

		#region TestOnProductChange_ChangesProductCode_WhenSameWE_OPIsSet

		public void TestOnProductChange_ChangesProductCode_WhenSameWE_OPIsSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = receive.Lines.AddNew();
			receiveLine.WE_OP = ZGuid.Invalid;
			receiveLine.ProductCode = "TEMP";

			AssertEquals("Precondition: ProductCode original value at start.", "TEMP", receiveLine.ProductCode);
			using (var form = new ReceiveLinesTestForm(receive))
			{
				form.Show();

				// Simulate GUI setting new temporary product code value to Framework storage.
				FieldInvalidTextMemory.SetInvalidText(receiveLine, WhsDocketLineSchema.WE_OP.Name, "ChangedProductCode");
				receiveLine.WE_OP = ZGuid.Invalid;
				AssertEquals("ProductCode changed on WE_OP being set.", "ChangedProductCode", receiveLine.ProductCode);
			}
		}

		#endregion

		#region TestSerialNumberMenuItemClick

		public void TestSerialNumberMenuItemClick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ReceiveLinesTestForm(data.Receive11))
			{
				form.Show();

				AssertNull("Should not add menu if EnableSchemaRedesignChanges is false.", form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText("Show Serials"));
			}

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ReceiveLinesTestForm(data.Receive11))
			{
				form.Show();

				var userControl = form.UserControl;
				AssertEquals(false, userControl.SerialNumberControl.Visible);

				var serialNumberMenuItem = userControl.LinesGrid.ContextMenu.MenuItems.FindByText("Show Serials");
				serialNumberMenuItem.PerformClick();
				AssertEquals(true, userControl.SerialNumberControl.Visible);
				AssertNull("Should change the text.", form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText("Show Serials"));
				AssertNotNull("Should change the text.", form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText("Hide Serials"));

				serialNumberMenuItem.PerformClick();
				AssertEquals(false, userControl.SerialNumberControl.Visible);
				AssertNotNull("Should change the text.", form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText("Show Serials"));
				AssertNull("Should change the text.", form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText("Hide Serials"));
			}
		}

		#endregion

		#endregion

		#region TestLocationAutoComplete

		public void TestLocationAutoCompleteDisabled()
		{
			using (var control = GetNewDocketLinesGridUserControl())
			{
				var style = (ZCodeFindBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle("WE_WL");
				Assert("Location style should have auto complete disabled", style.AutoCompleteDisabled);
			}
		}

		#endregion

		#region TestDestLocationColumnAvailability

		public void TestDestLocationColumnAvailability()
		{
			using (var control = GetNewDocketLinesGridUserControl())
			{
				var destLocationColumn = (ZTextBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle(WhsReceiveLine.Schema.DestLocation);
				AssertNotNull("DestLocation column should exist.", destLocationColumn);
				AssertEquals("DestLocation should be available.", false, destLocationColumn.IsUnavailable);
			}
		}

		#endregion

		#region TestExpectedQuantityColumnNextToTransactionQuantity

		public void TestExpectedQuantityColumnNextToTransactionQuantity()
		{
			using (var receiveLinesUserControl = GetNewDocketLinesGridUserControl())
			{
				var columns = new ZStringBuilder();
				foreach (var column in receiveLinesUserControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Where(column => column.IsVisible))
				{
					columns.Append(column.ColumnName + ",");
				}

				AssertEquals("The lines grid on the receive lines user control is set to default, no column layout context is assigned.", true, string.IsNullOrEmpty(receiveLinesUserControl.LinesGrid.ColumnLayoutContext));
				AssertEquals("Expected quantity column is inserted next to the transaction quantity column.", true, columns.ToString().Contains("WE_PackQuantity,WE_F3_NKPackType,WE_ClientOrderedUnits,WE_TransactionQuantity,ProductUQ"));
			}
		}

		#endregion

		#region Implementation

		protected override void SetTransactionAsBondedCore(WhsDocket docket)
		{
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsReceive>();
		}

		protected override ReceiveDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
		{
			return new ReceiveDocketLinesGridUserControl();
		}

		protected override DocketLinesTestForm GetNewDocketLinesTestForm(WhsDocket docket)
		{
			return new ReceiveLinesTestForm(docket);
		}

		protected class ReceiveLinesTestForm : DocketLinesTestForm
		{
			public ReceiveLinesTestForm(WhsDocket docket)
				: base(docket)
			{
			}

			protected override ReceiveDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
			{
				return new ReceiveDocketLinesGridUserControl();
			}
		}

		#endregion
	}
}
