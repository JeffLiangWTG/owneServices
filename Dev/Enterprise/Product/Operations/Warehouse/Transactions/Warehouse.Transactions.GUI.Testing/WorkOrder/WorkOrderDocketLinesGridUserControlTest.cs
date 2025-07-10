using System;
using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class WorkOrderDocketLinesGridUserControlTest : DocketLinesGridUserControlTest<WorkOrderDocketLinesGridUserControl>
	{
		public void TestFindAttributesCopiesCorrectFields()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateInventoryWithHeldCode();
			data.Line111.InDocketLine.WE_F3_NKPackType = "PLT";
			var product = Factory.Load<OrgSupplierPart>(data.Line111.InDocketLine.WE_OP);
			AssertEquals("Precondition - WE_StockOnHand", 100m, data.Line111.InDocketLine.WE_StockOnHand);
			AssertEquals("Precondition - OP_Desc", "P1", product.OP_Desc);
			AssertEquals("Precondition - WE_OriginalInventoryStatus", "HEL", data.Line111.InDocketLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - WE_CurrentInventoryStatus", "HEL", data.Line111.InDocketLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition - WE_DocketLineType", "INW", data.Line111.InDocketLine.WE_DocketLineType);
			AssertEquals("Precondition - WE_F3_NKPackType", "PLT", data.Line111.InDocketLine.WE_F3_NKPackType);
			AssertEquals("Precondition - WB_EntryKey", "ABC", data.Line111.CustomsData.WB_EntryKey);
			AssertEquals("Precondition - WB_EntryLineNo", (short)10, data.Line111.CustomsData.WB_EntryLineNo);
			AssertEquals("Precondition - WE_PackageGroupId", "123", data.Line111.InDocketLine.WE_PackageGroupId);
			AssertEquals("Precondition - WE_PerPackageQty", 2m, data.Line111.InDocketLine.WE_PerPackageQty);
			AssertEquals("Precondition - WE_PartAttrib1", "PA1", data.Line111.InDocketLine.WE_PartAttrib1);
			AssertEquals("Precondition - WE_CustomAttrib2", "CA2", data.Line111.InDocketLine.WE_CustomAttrib2);

			var workOrder = GetNewDocket();
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_OH_Client = data.Org1.PK;

			using (var form = GetNewDocketLinesTestForm(workOrder))
			{
				form.Show();

				var userControl = form.UserControl;
				userControl.LinesGrid.Select(0);
				userControl.FireFindAttributesMenuItemClickForTesting();

				var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				var module = popup.Module_ForTest;
				((BusinessObjectCollection)module.GridCollection).Load();

				for (var i = 0; i < module.GridCollection.Count; i++)
				{
					if (!((WhsInventoryView)module.GridCollection[i]).WI_PartAttrib1.IsEmpty)
					{
						((ZDisplayGrid)module.DisplayGrid).Select(i);
						break;
					}
				}

				popup.ExposedOKButtonForTesting.PerformClick();

				var docketLine = workOrder.Lines[0];
				AssertEquals("Order WE_PartAttrib1 should match the selected Inventory WE_PartAttrib1.", "PA1", docketLine.WE_PartAttrib1);
				AssertEquals("Order WE_CustomAttrib2 should match the selected Inventory WE_CustomAttrib2.", "CA2", docketLine.WE_CustomAttrib2);
				AssertEquals("Order WE_OP should match the selected Inventory WE_OP.", data.Line111.InDocketLine.WE_OP, docketLine.WE_OP);
				AssertEquals("Pallet ID should be empty.", string.Empty, docketLine.WE_PalletID);
				AssertEquals("Original Hold Code should be empty.", string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("Current Hold Code should be empty.", string.Empty, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("WE_StockOnHand should be 0.", 0m, docketLine.WE_StockOnHand);
				AssertEquals("WE_OriginalInventoryStatus should be empty.", string.Empty, docketLine.WE_OriginalInventoryStatus);
				AssertEquals("WE_CurrentInventoryStatus should be empty.", string.Empty, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("WE_F3_NKPackType should match the selected Inventory WE_F3_NKPackType.", "UNT", docketLine.WE_F3_NKPackType);
				AssertEquals("WB_EntryKey should be empty.", string.Empty, docketLine.CustomsData.WB_EntryKey);
				AssertEquals("WB_EntryLineNo should should be 0.", (short)0, docketLine.CustomsData.WB_EntryLineNo);
				AssertEquals("WE_PackageGroupId should be empty.", string.Empty, docketLine.WE_PackageGroupId);
				AssertEquals("WE_PerPackageQty should be empty.", 0m, docketLine.WE_PerPackageQty);
				AssertEquals("WOR", docketLine.WE_DocketLineType);
			}
		}

		#region TestColumnLayoutContextForLinesGrid

		public void TestColumnLayoutContextForLinesGrid()
		{
			using (var control = new WorkOrderDocketLinesGridUserControl())
			{
				AssertEquals("layout context is defined", nameof(DocketLinesGridContext.WorkOrder), control.LinesGrid.ColumnLayoutContext);
			}
		}

		#endregion

		#region TestBOMMenuItemCheckedAndEnabledStateIsUpdatedOnPopup

		public void TestBOMMenuItemCheckedAndEnabledStateIsUpdatedOnPopup()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			workOrder.BOM.ExpandAllLines();

			// two separate forms because we need to dispose of the Context menu after each popup.
			using (var form = new WorkOrderLinesTestForm(workOrder))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var bomMenuItem = form.FindBomMenuItem();

				bool? bomMenuItemEnabled = null;
				bool? bomMenuItemChecked = null;

				grid.ContextMenu.Popup += delegate
				{
					bomMenuItemEnabled = bomMenuItem.Enabled;
					bomMenuItemChecked = bomMenuItem.Checked;
					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};

				grid.ContextMenu.Show(grid, new Point(10, 10));
				AssertEquals("No lines selected.", false, bomMenuItemEnabled);
				AssertEquals("No lines selected.", false, bomMenuItemChecked);
			}

			using (var form = new WorkOrderLinesTestForm(workOrder))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var bomMenuItem = form.FindBomMenuItem();

				bool? bomMenuItemEnabled = null;
				bool? bomMenuItemChecked = null;

				grid.ContextMenu.Popup += delegate
				{
					bomMenuItemEnabled = bomMenuItem.Enabled;
					bomMenuItemChecked = bomMenuItem.Checked;
					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};

				grid.Select(0);
				grid.ContextMenu.Show(grid, new Point(10, 10));
				AssertEquals("No lines selected.", true, bomMenuItemEnabled);
				AssertEquals("No lines selected.", true, bomMenuItemChecked);
			}
		}

		#endregion

		#region TestAllocationKeyColumn

		public void TestAllocationKeyColumn_Assembly_InwardProcessingEnabled() => TestAllocationKeyColumn(WorkOrderType.Codes.Assemble, inwardProcessingEnabled: true);

		public void TestAllocationKeyColumn_Disassembly_InwardProcessingEnabled() => TestAllocationKeyColumn(WorkOrderType.Codes.Disassemble, inwardProcessingEnabled: true);

		public void TestAllocationKeyColumn_Assembly_InwardProcessingDisabled() => TestAllocationKeyColumn(WorkOrderType.Codes.Assemble, inwardProcessingEnabled: false);

		public void TestAllocationKeyColumn_Disassembly_InwardProcessingDisabled() => TestAllocationKeyColumn(WorkOrderType.Codes.Disassemble, inwardProcessingEnabled: false);

		void TestAllocationKeyColumn(string type, bool inwardProcessingEnabled)
		{
			var whs1 = Helper.CreateWarehouse("1");
			whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

			var docket = Factory.New<WhsWorkOrder>();
			docket.WD_DocketSubType = type;
			docket.WD_WW_Whs = whs1.PK;

			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(inwardProcessingEnabled);

			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var form = GetNewDocketLinesTestForm(docket))
				{
					form.Show();
					var userControl = form.UserControl;
					AssertEquals("WE_AllocationKey availability.",
						type == WorkOrderType.Codes.Disassemble && inwardProcessingEnabled,
						userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.WE_AllocationKey.Name));
				}
			}
		}

		#endregion

		#region Implementation

		protected override bool TestDocketStatusChanged => false;

		protected override bool TestDocketSubTypeChanged => false;

		protected override void SetTransactionAsBondedCore(WhsDocket docket)
		{
			docket.WD_DocketSubType = OrderType.Codes.Customs;
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsWorkOrder>();
		}

		protected override WorkOrderDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
		{
			return new WorkOrderDocketLinesGridUserControl();
		}

		protected override DocketLinesTestForm GetNewDocketLinesTestForm(WhsDocket docket)
		{
			return new WorkOrderLinesTestForm((WhsWorkOrder)docket);
		}

		class WorkOrderLinesTestForm : DocketLinesTestForm
		{
			public WorkOrderLinesTestForm(WhsWorkOrder workOrder)
				: base(workOrder)
			{
			}

			protected override WorkOrderDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
			{
				return new WorkOrderDocketLinesGridUserControl();
			}

			public BOMMenuItem FindBomMenuItem()
			{
				foreach (var item in UserControl.LinesGrid.ContextMenu.MenuItems)
				{
					var bomItem = item as BOMMenuItem;
					if (bomItem != null)
					{
						return bomItem;
					}
				}

				throw new Exception("BOM Menu Item wasn't found in the Grid's ContextMenu.");
			}
		}

		#endregion
	}
}
