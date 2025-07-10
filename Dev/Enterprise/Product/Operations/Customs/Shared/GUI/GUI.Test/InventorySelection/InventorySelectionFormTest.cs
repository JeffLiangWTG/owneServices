using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(InventorySelectionForm))]
	sealed class InventorySelectionFormTest : ZFormBasherTest
	{
		public void TestModuleControlMainPanel()
		{
			using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeader(Declaration)))
			{
				form.Show();
				var header = form.BusinessEntity;
				var controls = form.Controls.Find("MainPanel", true);
				AssertEquals(1, controls.Length);
				var mainPanel = (ZPanel)controls[0];
				controls = form.Controls.Find("InventoryFilterControl", true);
				AssertEquals(1, controls.Length);
				var embeddedControl = (ZFilterStripControl)controls[0];
				AssertEquals(true, embeddedControl.Controls.Contains(mainPanel));
			}
		}

		public void TestChangeDataLayoutWhenGroupByIsChanged()
		{
			using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeader(Declaration)))
			{
				form.Show();
				var header = form.BusinessEntity;
				var controls = form.Controls.Find("DataGrid", true);
				AssertEquals(1, controls.Length);
				var dataGrid = (ZGrid)controls[0];

				AssertArrayEqualsByElements(new[]
				{
					InventorySelectionLine.Schema.US_Product,
					InventorySelectionLine.Schema.US_Description,
					InventorySelectionLine.Schema.US_OriginalBondedQty,
					InventorySelectionLine.Schema.US_ProductQtyToDraw,
					InventorySelectionLine.Schema.US_ProductQtyOnHand,
					InventorySelectionLine.Schema.US_CustomsEntryKey,
					InventorySelectionLine.Schema.US_ArrivalDate,
					InventorySelectionLine.Schema.US_OriginalPackType,
					InventorySelectionLine.Schema.US_GroupingID,
					InventorySelectionLine.Schema.US_Attribute1,
					InventorySelectionLine.Schema.US_Attribute2,
					InventorySelectionLine.Schema.US_Attribute3,
					InventorySelectionLine.Schema.US_SerialNumber,
					InventorySelectionLine.Schema.US_CustomsDeadline,
					InventorySelectionLine.Schema.US_ProductQtyPerCarton,
					InventorySelectionLine.Schema.US_Warehouse,
					InventorySelectionLine.Schema.US_DeclarantsReference
				}, dataGrid.Columns.Select(x => x.ColumnName).ToArray());
				AssertStartsWith("dataGrid.GridId", "I", dataGrid.GridId);

				header.IsGroupByProduct = true;
				AssertArrayEqualsByElements(new[]
				{
					InventorySelectionLine.Schema.US_Product,
					InventorySelectionLine.Schema.US_Description,
					InventorySelectionLine.Schema.US_OriginalBondedQty,
					InventorySelectionLine.Schema.US_ProductQtyToDraw,
					InventorySelectionLine.Schema.US_ProductQtyOnHand,
					InventorySelectionLine.Schema.US_OriginalPackType,
					InventorySelectionLine.Schema.US_Attribute1,
					InventorySelectionLine.Schema.US_Attribute2,
					InventorySelectionLine.Schema.US_Attribute3,
					InventorySelectionLine.Schema.US_SerialNumber,
					InventorySelectionLine.Schema.US_CustomsDeadline,
					InventorySelectionLine.Schema.US_Warehouse
				}, dataGrid.Columns.Select(x => x.ColumnName).ToArray());

				AssertStartsWith("dataGrid.GridId", "P", dataGrid.GridId);

				header.IsGroupByCarton = true;
				AssertArrayEqualsByElements(new[]
				{
					InventorySelectionLine.Schema.US_Product,
					InventorySelectionLine.Schema.US_Description,
					InventorySelectionLine.Schema.US_OriginalBondedQty,
					InventorySelectionLine.Schema.US_CartonQtyOnHand,
					InventorySelectionLine.Schema.US_CartonQtytoDraw,
					InventorySelectionLine.Schema.US_ProductQtyToDraw,
					InventorySelectionLine.Schema.US_ProductQtyOnHand,
					InventorySelectionLine.Schema.US_CustomsEntryKey,
					InventorySelectionLine.Schema.US_ArrivalDate,
					InventorySelectionLine.Schema.US_OriginalPackType,
					InventorySelectionLine.Schema.US_GroupingID,
					InventorySelectionLine.Schema.US_SerialNumber,
					InventorySelectionLine.Schema.US_CustomsDeadline,
					InventorySelectionLine.Schema.US_ProductQtyPerCarton,
					InventorySelectionLine.Schema.US_Warehouse
				}, dataGrid.Columns.Select(x => x.ColumnName).ToArray());
				AssertStartsWith("dataGrid.GridId", "C", dataGrid.GridId);
			}
		}

		public void TestSelectedProductGridLayout()
		{
			using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeader(Declaration)))
			{
				form.Show();
				var header = form.BusinessEntity;
				var controls = form.Controls.Find("SelectedProductGrid", true);
				AssertEquals(1, controls.Length);
				var dataGrid = (ZGrid)controls[0];
				AssertArrayEqualsByElements(new[]
				{
					WhsInventoryWrapper.Schema.Product,
					WhsInventoryWrapper.Schema.ProductDescription,
					WhsInventoryWrapper.Schema.CalculatedOriginalBondedQty,
					WhsInventoryWrapper.Schema.QuantityToDraw,
					WhsInventoryWrapper.Schema.CalculatedQuantityOnHand,
					WhsInventoryWrapper.Schema.CustomsEntryKey,
					WhsInventoryWrapper.Schema.ArrivalDate,
					WhsInventoryWrapper.Schema.OriginalPackType,
					WhsInventoryWrapper.Schema.SupplierName,
					WhsInventoryWrapper.Schema.WarehouseName,
					WhsInventoryWrapper.Schema.Attribute1,
					WhsInventoryWrapper.Schema.Attribute2,
					WhsInventoryWrapper.Schema.Attribute3,
					WhsInventoryWrapper.Schema.GroupingID
				}, dataGrid.Columns.Select(x => x.ColumnName).ToArray());
			}
		}

		public void TestSelectButton_Click()
		{
			var whsWarehouse1 = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse1.PK, Helper.Importer.PK, "RCV1");
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, ZString.Empty, ZDecimal.Zero, 1000m, 1000m, "PK", bondedEntryKey: "ENS934232-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory1.WI_WE_InDocketLine, 10000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "ENS934232", (ZShort)1);

			Helper.Warehouse2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var whsWarehouse2 = Helper.GetNewWhsWarehouse(Helper.Warehouse2.MainAddress.PK, true, "N20");
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse2.PK, Helper.Importer.PK, "RCV2");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part2, ZString.Empty, ZDecimal.Zero, 1000m, 1000m, "PK", bondedEntryKey: "ENS934233-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory2.WI_WE_InDocketLine, 10000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "ENS934233", (ZShort)1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Declaration.JE_OH_Importer = Helper.Importer.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.Invoices.DeleteAll();
			Factory.Save();

			using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeader(Declaration)))
			{
				form.Show();
				var header = form.BusinessEntity;
				var controls = form.Controls.Find("InventoryFilterControl", true);
				AssertEquals(1, controls.Length);
				var filterStripControl = (ZFilterStripControl)controls[0];

				controls = form.Controls.Find("SelectButton", true);
				AssertEquals(1, controls.Length);
				var selectButton = (ZButton)controls[0];
				AssertEquals(false, selectButton.Enabled);

				filterStripControl.FirePerformSearch();
				AssertEquals(2, filterStripControl.GridCollection.Count);
				AssertEquals(2, header.SelectionLines.Count);
				AssertEquals(true, selectButton.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				selectButton.PerformClick();
				AssertEquals("No inventory has been selected to draw from warehouse; please enter the draw qty on the inventory you want to draw from warehouse.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, Declaration.Invoices.Count);

				var line1 = header.SelectionLines[0];
				line1.US_ProductQtyToDraw = 800m;
				var line2 = header.SelectionLines[1];
				line2.US_ProductQtyToDraw = 800m;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				selectButton.PerformClick();
				AssertEquals("Inventories belonging to different warehouses have been selected; please only select inventories from same warehouse.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, Declaration.Invoices.Count);

				line2.US_ProductQtyToDraw = 0m;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				selectButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, Declaration.Invoices.Count);
				var invoice = Declaration.Invoices[0];
				AssertEquals(8000m, invoice.JZ_InvoiceAmount);
				AssertEquals(1, Declaration.InvoiceLines.Count);
				var invoiceLine = Declaration.InvoiceLines[0];
				AssertEquals(800m, invoiceLine.JI_InvoiceQuantity);
				AssertEquals(8000m, invoiceLine.JI_LinePrice);
			}
		}

		public void TestAutoFillOutDrawQuantities()
		{
			var whsWarehouse1 = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse1.PK, Helper.Importer.PK, "RCV1");
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, ZString.Empty, ZDecimal.Zero, 1000m, 1000m, "PK", bondedEntryKey: "ENS934232-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory1.WI_WE_InDocketLine, 10000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "ENS934232", (ZShort)1);

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Declaration.JE_OH_Importer = Helper.Importer.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.Invoices.DeleteAll();
			Factory.Save();

			using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeaderForTest(Declaration, false)))
			{
				form.Show();
				var header = form.BusinessEntity;
				var controls = form.Controls.Find("InventoryFilterControl", true);
				var filterStripControl = (ZFilterStripControl)controls[0];

				filterStripControl.FirePerformSearch();
				AssertEquals(1, filterStripControl.GridCollection.Count);

				var line1 = header.SelectionLines[0];
				AssertEquals("AutoFillOutDrawQuantities false - line1 QtyToDraw is 0", 0m, line1.US_ProductQtyToDraw);
			}

			using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeaderForTest(Declaration, true)))
			{
				form.Show();
				var header = form.BusinessEntity;
				var controls = form.Controls.Find("InventoryFilterControl", true);
				var filterStripControl = (ZFilterStripControl)controls[0];

				filterStripControl.FirePerformSearch();
				AssertEquals(1, filterStripControl.GridCollection.Count);

				var line1 = header.SelectionLines[0];
				AssertEquals("AutoFillOutDrawQuantities true - line1 QtyToDraw is filled", 1000m, line1.US_ProductQtyToDraw);
			}
		}

		public void TestRadioButtonsVisibilitiesAndFillAllDrawQtyButtonText()
		{
			using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeader(Declaration)))
			{
				form.Show();
				var cargoRadioButton = form.Controls.Find("CartonRadioButton", true)[0];
				var productRadioButton = form.Controls.Find("ProductRadioButton", true)[0];
				var groupByGroupBox = form.Controls.Find("GroupByGroupBox", true)[0];

				AssertEquals("CartonRadioButton should be shown", true, cargoRadioButton.Visible);
				AssertEquals("ProductRadioButton should be shown", true, productRadioButton.Visible);
				AssertEquals("GroupByGroupBox should be shown", true, groupByGroupBox.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new InventorySelectionForm(new DeclarationInventorySelectionHeader(Declaration));

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration => declaration ?? (declaration = Helper.GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "BEXW000001", "EN00012312", 10m));

		WhsDataTestHelper helper;
		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));

		class DeclarationInventorySelectionHeaderForTest : DeclarationInventorySelectionHeader
		{
			public DeclarationInventorySelectionHeaderForTest(BaseJobDeclaration declaration, bool autoFillOutDrawQuantities) : base(declaration)
			{
				this.autoFillOutDrawQuantities = autoFillOutDrawQuantities;
			}

			public override bool AutoFillOutDrawQuantities => autoFillOutDrawQuantities;
			readonly bool autoFillOutDrawQuantities;
		}
	}
}
