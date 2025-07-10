using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class OrderLinesGridUserControlTest : DocketLinesGridUserControlTest<OrderDocketLinesGridUserControl>
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

			var order = GetNewDocket();
			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_OH_Client = data.Org1.PK;

			using (var form = GetNewDocketLinesTestForm(order))
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

				var docketLine = ((WhsOrder)order).ParentLines[0];
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
				AssertEquals("ORD", docketLine.WE_DocketLineType);
			}
		}

		#region TestDoubleClickOnEmptyGrid

		public void TestDoubleClickOnEmptyGrid()
		{
			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				// double click the grid when it has no rows
				const int noClicks = 2;
				var onMouseDownMethod = typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertNoExceptionThrown("", () => onMouseDownMethod.Invoke(form.LinesGrid, new object[] { new MouseEventArgs(MouseButtons.Left, noClicks, 50, 30, 0) }));
			}
		}

		#endregion

		#region TestBondedEntryKeyColumns

		public void TestBondedEntryKeyColumns()
		{
			var whs1 = Helper.CreateWarehouse("1");
			Helper.EnableWarehouseForBond(whs1, true);

			var docket = Factory.New<WhsOrder>();
			docket.WD_WW_Whs = whs1.PK;
			SetTransactionAsBondedCore(docket);
			AssertEquals(true, docket.IsCustomsTransaction);

			using (var form = GetNewDocketLinesTestForm(docket))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertEquals("WE_BondedEntryKey visibility incorrect", true, userControl.LinesGrid.Columns[WhsDocketLineSchema.WE_BondedEntryKey.Name].IsVisible);
				AssertEquals("WE_BondedEntryKey caption incorrect", "Inwards Entry Key", userControl.LinesGrid.GetColumnStyle(WhsDocketLineSchema.WE_BondedEntryKey.Name).CaptionResourceString.Caption);
				AssertEquals("CustomsData+WB_EntryKey caption incorrect", "Outwards Entry No.", userControl.LinesGrid.GetColumnStyle("CustomsData+WB_EntryKey").CaptionResourceString.Caption);
				AssertEquals("CustomsData+WB_EntryLineNo caption incorrect", "Outwards Entry Line No.", userControl.LinesGrid.GetColumnStyle("CustomsData+WB_EntryLineNo").CaptionResourceString.Caption);
			}
		}

		#endregion

		#region TestAllocationKeyColumn

		public void TestAllocationKeyColumn_InwardProcessingEnabled() => TestAllocationKeyColumn(inwardProcessingEnabled: true);

		public void TestAllocationKeyColumn_InwardProcessingDisabled() => TestAllocationKeyColumn(inwardProcessingEnabled: false);

		void TestAllocationKeyColumn(bool inwardProcessingEnabled)
		{
			var whs1 = Helper.CreateWarehouse("1");
			whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

			var docket = Factory.New<WhsOrder>();
			docket.WD_WW_Whs = whs1.PK;

			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(inwardProcessingEnabled);

			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var form = GetNewDocketLinesTestForm(docket))
				{
					form.Show();
					var userControl = form.UserControl;
					AssertEquals("WE_AllocationKey availability.", inwardProcessingEnabled, userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.WE_AllocationKey.Name));
				}
			}
		}

		#endregion

		#region TestColumnsAreInitialisedCorrectlyEvenIfPickHasNoOrders

		public void TestColumnsAreInitialisedCorrectlyEvenIfPickHasNoOrders()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new PickTestForm(pick))
			{
				form.Show();
				var grid = form.UserControl.LinesGrid;
				AssertAttributeVisibility(grid, false, false, false, false, false, false);
				AssertPricingFieldsVisibility(grid, false, false, false, true, true, true);
			}
		}

		void AssertPricingFieldsVisibility(ZGrid grid, bool recUnitPrice, bool unitDiscAmount, bool unitDiscPercent, bool unitPriceAfterDisc, bool extendedPrice, bool currency)
		{
			AssertEquals("RecUnitPrice visibility incorrect", recUnitPrice, grid.Columns[WhsDocketLineSchema.WE_RecommendedUnitPrice.Name].IsVisible);
			AssertEquals("UnitDiscAmount visibility incorrect", unitDiscAmount, grid.Columns[WhsDocketLineSchema.WE_UnitDiscountAmount.Name].IsVisible);
			AssertEquals("UnitDiscPercent visibility incorrect", unitDiscPercent, grid.Columns[WhsDocketLineSchema.WE_UnitDiscountPercent.Name].IsVisible);
			AssertEquals("UnitPriceAfterDisc visibility incorrect", unitPriceAfterDisc, grid.Columns[WhsDocketLineSchema.WE_UnitPriceAfterDiscount.Name].IsVisible);
			AssertEquals("ExtendedPrice visibility incorrect", extendedPrice, grid.Columns[WhsDocketLineSchema.WE_ExtendedLinePrice.Name].IsVisible);
			AssertEquals("Currency visibility incorrect", currency, grid.Columns[WhsDocketLineSchema.WE_RX_NKUnitPriceCurrency.Name].IsVisible);
		}

		#endregion

		#region TestPackIDColumn

		public void TestPackIDColumnIsDisplayedCorrectly()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				AssertEquals("Pallet ID column is visible", true, grid.Columns[WhsDocketLineSchema.WE_PalletID.Name].IsVisible);
			}
		}

		#endregion

		#region TestOnDragDrop

		public void TestOnDragDrop()
		{
			var order = (WhsOrder)GetNewDocket();
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();
				AssertEquals("Precondition no lines in order", 0, order.Lines.Count);

				order.WD_WW_Whs = data.Whs1.PK;
				order.WD_OH_Client = data.Org1.PK;
				AssertEquals("Precondition collection allows new", true, ((IBindingList)order.Lines).AllowNew);

				form.UserControl.LinesGridDragAndDropManagerForTesting.OnDragDropCore(new BusinessObject[] { data.Line111 });
				AssertEquals("New line should be generated for order", 1, order.Lines.Count);
			}
		}

		#endregion

		#region TestDelete

		public void TestDelete_PickingLine_ReducesUnitsToZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			Factory.Save(); // reset to 0 only occurs if the line is in the DB.

			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				// select the line in the grid and simulate the user pressing the DEL key
				form.UserControl.LinesGrid.Select(0);
				KeySender.PostKeyDown(form.UserControl.LinesGrid, Keys.Delete);
				Application.DoEvents();
			}

			AssertEquals("The Order is Picking -- the selected line should not have been deleted.", false, line.IsDeleted);
			AssertEquals("The Order is Picking -- instead of deleting the line, the units should have been reduced to 0.", 0m, line.WE_TransactionQuantity);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(
				"This Order is already Picked. Qty Ordered was reduced to 0, and the empty line will be automatically deleted when you save the Order."));
		}

		public void TestDelete_PickingLine_DONOTReduceUnits_WhenIsAMultiOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD1");
			var line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 20m);

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);

			Factory.Save(); // reset to 0 only occurs if the line is in the DB.

			using (var form = new OrderLinesTestForm(order1))
			{
				form.Show();

				// select the line in the grid and simulate the user pressing the DEL key
				form.UserControl.LinesGrid.Select(0);
				KeySender.PostKeyDown(form.UserControl.LinesGrid, Keys.Delete);
				Application.DoEvents();
			}

			AssertEquals("The Order is Picking -- the selected line should not have been deleted.", false, line1.IsDeleted);
			AssertEquals("The Order is Picking -- units should have NOT been reduced to 0.", 10m, line1.WE_TransactionQuantity);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(
				"This order is attached to multi-order pick, no changes to ordered content is allowed."));
		}

		public void TestDelete_PickedLine_DoesNothing()
		{
			TestDelete_PickedLine_DoesNothing_Core(usingInTransitTransfer: false);
		}

		public void TestDelete_PickedLine_DoesNothing_InTransit()
		{
			TestDelete_PickedLine_DoesNothing_Core(usingInTransitTransfer: true);
		}

		void TestDelete_PickedLine_DoesNothing_Core(bool usingInTransitTransfer)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine_Picked = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine_NotPicked = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 20m, totalPickLineQuantity);

			var pickLine = orderLine_Picked.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			// Mark pickline as picked
			if (usingInTransitTransfer)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				Factory.Save();
			}
			else
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					Factory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine_Picked.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			}

			Factory.Save(); // reset to 0 only occurs if the line is in the DB.

			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				// select the line in the grid and simulate the user pressing the DEL key
				form.UserControl.LinesGrid.SelectElements(orderLine_NotPicked);
				KeySender.PostKeyDown(form.UserControl.LinesGrid, Keys.Delete);
				Application.DoEvents();
				AssertEquals("The selected line should not have been deleted.", false, orderLine_NotPicked.IsDeleted);
				AssertEquals("The selected line units should have been reduced to 0.", 0m, orderLine_NotPicked.WE_TransactionQuantity);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(
					"This Order is already Picked. Qty Ordered was reduced to 0, and the empty line will be automatically deleted when you save the Order."));

				// select the line in the grid and simulate the user pressing the DEL key
				form.UserControl.LinesGrid.SelectElements(orderLine_Picked);
				KeySender.PostKeyDown(form.UserControl.LinesGrid, Keys.Delete);
				Application.DoEvents();
				AssertEquals("The selected line should not have been deleted.", false, orderLine_Picked.IsDeleted);
				AssertEquals("The selected line units should not be changed.", 10m, orderLine_Picked.WE_TransactionQuantity);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(
					"This line has been partially or fully picked, it cannot be deleted."));
			}
		}

		public void TestDelete_PickedFTZLine_DoesNothing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				var pick = Helper.CreatePickNew(order);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				// select the line in the grid and simulate the user pressing the DEL key
				form.UserControl.LinesGrid.SelectElements(orderLine);
				KeySender.PostKeyDown(form.UserControl.LinesGrid, Keys.Delete);
				Application.DoEvents();
				AssertEquals("The selected line should not have been deleted.", false, orderLine.IsDeleted);
				AssertEquals("The selected line units should not be changed.", 5m, orderLine.WE_TransactionQuantity);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(
					"This order is allocated to a Pick for an FTZ Warehouse in a Country/Region that uses Permits, it cannot be deleted."));
			}
		}

		#endregion

		#region TestDuplicateContextMenu

		public void TestDuplicateContextMenu()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Precondition - Pick failed.", false, order.IsAttachedToPickButNotFinalised);
			AssertDuplicateMenuAvailablity("Should be Available because Order is not Picking.", order, true);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			Factory.Save();
			AssertDuplicateMenuAvailablity("Should be Available because Order is linked to a single-order Pick.", order, true);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD1");
			var line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 20m);
			var pick2 = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);
			Factory.Save();
			AssertDuplicateMenuAvailablity("Should be Disabled because Order1 is linked to a multi-order Pick.", order1, false);
		}

		void AssertDuplicateMenuAvailablity(string message, WhsOrder order, bool expectedEnabled)
		{
			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var duplicateMenuItem = form.FindDuplicateMenuItem();

				bool? menuItemEnabled = null;

				grid.ContextMenu.Popup += delegate
				{
					menuItemEnabled = duplicateMenuItem.Enabled;
					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};

				grid.Select(0);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(message, expectedEnabled, menuItemEnabled);
			}
		}

		#endregion

		#region TestRMAForOrderLinesContextMenu

		public void TestRMAForOrderLinesContextMenu()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;

				grid.Select(0);
				grid.ContextMenu.ShowPopupMenu();
				AssertNull("Precondition - Pick is null.", order.Pick);
				AssertEquals("RMA for Order Lines menu item should not be added into ContextMenu", 0, grid.ContextMenu.MenuItems.Find("RMA For Order Lines", false).Length);

				var pick = Helper.CreatePickNew(order);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Precondition - Pick not finalized.", false, order.Pick.IsFinalised);
				AssertEquals("RMA for Order Lines menu item should not be added into ContextMenu", 0, grid.ContextMenu.MenuItems.Find("RMA For Order Lines", false).Length);

				order.FinaliseDocketWithoutUserConfirmation();
				order.Pick.FinalisePick();
				AssertIsFinalisedPrecondition(order.Pick);

				grid.UnSelect(0);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("No rows selected, RMA for Order Lines menu item should not be added into ContextMenu", 0, grid.ContextMenu.MenuItems.Find("RMA For Order Lines", false).Length);

				grid.Select(0);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("RMA for Order Lines menu item should be added into ContextMenu", 1, grid.ContextMenu.MenuItems.Find("RMA For Order Lines", false).Length);

				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("RMA for Order Lines menu item should not be added into ContextMenu again", 1, grid.ContextMenu.MenuItems.Find("RMA For Order Lines", false).Length);
			}
		}

		#endregion

		#region TestRMAForOrderLinesContextMenu_Click

		public void TestRMAForOrderLinesContextMenu_Click()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			order.Pick.FinalisePick();
			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				grid.Select(0);
				form.UserControl.RMAForOrderLineMenuClickForTest();

				AssertType<RMAForOrderLinesForm>("Should shown RMAFOrOrderLinesForm.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#endregion

		#region TestGetGridModuleFiltersWhenGridOnceHaveRows

		public void TestGetGridModuleFiltersWhenGridOnceHaveRows()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				form.UserControl.LinesGrid.Select(0);
				KeySender.PostKeyDown(form.UserControl.LinesGrid, Keys.Delete);
				Application.DoEvents();

				var gridFilterStrip = new GridFilterStripBusinessObject(form.UserControl.LinesGrid);

				AssertNoExceptionThrown(() => { var filter = gridFilterStrip.ModuleFilters; });
			}
		}

		#endregion

		public void TestShowOrderLineEditForm()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var whs2 = Helper.CreateWarehouse("WH2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			using (var form = new OrderLinesTestForm(order))
			{
				form.Show();

				form.UserControl.ShowOrderLineEditForm(null);
				AssertEquals("Please select an order line to edit.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.UserControl.ShowOrderLineEditForm(line);
				AssertEquals("Please save this Order before going to the Order Line Form.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.FireSaveButton();

				order.WD_DocketSubType = OrderType.Codes.BackOrder;
				form.UserControl.ShowOrderLineEditForm(line);
				AssertEquals("The Order Type has changed. Please save this Order before going to the Order Line Form.", UnitTestUserNotification.Instance.LastMessage.Text);

				order.WD_DocketSubType = OrderType.Codes.Order;
				form.FireSaveButton();

				order.WD_WW_Whs = whs2.PK;
				form.UserControl.ShowOrderLineEditForm(line);
				AssertEquals("The Warehouse has changed. Please save this Order before going to the Order Line Form.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Should not show any forms.", ZFormModaliser.LastFormShownForTest);

				form.FireSaveButton();

				form.UserControl.ShowOrderLineEditForm(line);
				AssertType<OrderLineEntryForm>("Should have shown Order Line Form.", ZFormModaliser.LastFormShownForTest);
			}
		}

		#region TestWE_WHC_NKOrderedHeldCode

		public void TestWE_WHC_NKOrderedHeldCode_HeldGoodsForOrdersEnabled() => TestWE_WHC_NKOrderedHeldCode(true);

		public void TestWE_WHC_NKOrderedHeldCode_HeldGoodsForOrdersDisabled() => TestWE_WHC_NKOrderedHeldCode(false);

		void TestWE_WHC_NKOrderedHeldCode(bool heldGoodsForOrders)
		{
			var whs1 = Helper.CreateWarehouse("1");
			var docket = Factory.New<WhsOrder>();
			docket.WD_WW_Whs = whs1.PK;

			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, heldGoodsForOrders))
			{
				using (var form = GetNewDocketLinesTestForm(docket))
				{
					form.Show();
					var userControl = form.UserControl;
					AssertEquals("WE_WHC_NKOrderedHeldCode availability.", heldGoodsForOrders, userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.WE_WHC_NKOrderedHeldCode.Name));
				}
			}
		}

		#endregion

		#region Implementation

		protected override string ControlBindTo => "ParentLines";

		protected override bool IsPerPackageQtyColumnVisible => false;

		protected override void SetTransactionAsBondedCore(WhsDocket docket)
		{
			docket.WD_DocketSubType = OrderType.Codes.Customs;
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		protected override OrderDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
		{
			return new OrderDocketLinesGridUserControl();
		}

		protected override DocketLinesTestForm GetNewDocketLinesTestForm(WhsDocket docket)
		{
			return new OrderLinesTestForm(docket);
		}

		class OrderLinesTestForm : DocketLinesTestForm
		{
			public OrderLinesTestForm(WhsDocket docket)
				: base(docket)
			{
			}

			protected override OrderDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
			{
				return new OrderDocketLinesGridUserControl();
			}

			public new OrderDocketLinesGridUserControl UserControl => base.UserControl;

			public ZMenuItem FindDuplicateMenuItem()
			{
				foreach (ZMenuItem item in UserControl.LinesGrid.ContextMenu.MenuItems)
				{
					if (item.Name == "Duplicate")
					{
						return item;
					}
				}

				throw new Exception("Duplicate Menu Item wasn't found in the Grid's ContextMenu.");
			}
		}

		class PickTestForm : ZForm
		{
			public PickTestForm(WhsPick pick)
				: base(pick)
			{
			}

			public DocketLinesGridUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = new OrderDocketLinesGridUserControl();
				this.UserControl.BindTo = "Orders.Lines";
				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsPick";
			}
		}

		#endregion
	}

	#region ReleaseOrderLineMenuItemsTest

	class QtyLostReleaseOrderLineMenuItemsTest : ReleaseOrderLineMenuItemsTest
	{
		protected override ZString MenuItemName => "Adjust Out &Quantity Met";

		protected override ZString FunctionDescription => "Adjust Out Quantity Met";

		protected override ReduceStockReason ReduceStockReason => ReduceStockReason.Lost;

		protected override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsReleaseAdjustOutQtyMet;

		protected override ZString ExpectedDocketTypeCreated => DocketType.Codes.Adjustment;
	}

	class ReturnStockReleaseOrderLineMenuItemsTest : ReleaseOrderLineMenuItemsTest
	{
		protected override ZString MenuItemName => "&Return Items to Stock";

		protected override ZString FunctionDescription => "Return Items to Stock";

		protected override ReduceStockReason ReduceStockReason => ReduceStockReason.Returned;

		protected override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsReleaseReturnItemsToStock;

		protected override ZString ExpectedDocketTypeCreated => DocketType.Codes.Transfer;
	}

	abstract class ReleaseOrderLineMenuItemsTest : OrderLinesGridUserControlTest
	{
		#region TestReduceStockMenuItem_NotShownByDefault

		public void TestReduceStockMenuItem_NotShownByDefault()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.Show();

				var menuItem = form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText(MenuItemName);
				AssertNull(menuItem);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_ShownAfterSetVisibility

		public void TestReduceStockMenuItem_ShownAfterSetVisibility()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var menuItem = form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText(MenuItemName);
				form.UserControl.ReduceStockOptionsAreVisible = true;
				AssertNotNull(menuItem);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_EnabledWhenSelected

		public void TestReduceStockMenuItem_EnabledWhenSelected()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText(MenuItemName);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Precondition:", 0, grid.SelectedElements.Length);
				AssertEquals("When no item selected - menu item should be disabled.", false, menuItem.Enabled);

				grid.SelectElements(order.Lines[0]);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When one or more items selected - menu item should be enabled.", true, menuItem.Enabled);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_DisabledWhenPickIsFinalized

		public void TestReduceStockMenuItem_DisabledWhenPickIsFinalized()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText(MenuItemName);
				AssertEquals("Precondition: Pick is not finalized", false, pick.IsFinalised);
				grid.SelectElements(order.Lines[0]);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When Pick is not Finalized - menu item should be enabled.", true, menuItem.Enabled);

				pick.FinaliseAllOrders();
				pick.FinalisePick();
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When Pick is Finalized - menu item should be disabled.", false, menuItem.Enabled);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_Clicked

		public void TestReduceStockMenuItem_ShowWindowWhenClicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "AAA", "", "", "");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "BBB", "", "", "");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			SecurityCheckpoint.IsAllowed = true;

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText(MenuItemName);
				form.UserControl.LinesGrid.SelectElements(order.Lines[0]);
				menuItem.PerformClick();

				AssertEquals($"1 Lines were selected. Are you sure you want to fully bulk {FunctionDescription}?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_DoesNothingIfUserClickNo

		public void TestReduceStockMenuItem_DoesNothingIfUserClickNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "AAA", "", "", "");
			receive1.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			SecurityCheckpoint.IsAllowed = true;

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText(MenuItemName);
				form.UserControl.LinesGrid.SelectElements(order.Lines[0]);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();

				AssertEquals("Should still have 1 release line", 1, order.Lines[0].ReleaseLines.Count);
				AssertEquals("Should not change qty of release line", 10m, order.Lines[0].ReleaseLines[0].Quantity);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_ReduceStockIfUserClickYes

		public void TestReduceStockMenuItem_ReduceStockIfUserClickYes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "AAA", "", "", "");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "BBB", "", "", "");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = "E";
				pl.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});
			Factory.Save();

			SecurityCheckpoint.IsAllowed = true;

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText(MenuItemName);
				AssertNotNull(menuItem);
				form.UserControl.LinesGrid.SelectElements(order.Lines[0]);
				UnitTestUserNotification.Instance.AddYesAnswer();
				menuItem.PerformClick();
				AssertEquals($"1 Lines were selected. Are you sure you want to fully bulk {FunctionDescription}?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("ReleaseLines should be empty", 0, order.Lines[0].ReleaseLines.Count);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_ShowsErrorForUnsavedChanges

		public void TestReduceStockMenuItem_ShowsErrorForUnsavedChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "AAA", "", "", "");
			receive1.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			SecurityCheckpoint.IsAllowed = true;

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				pick.WP_PercentageComplete = 8;
				Assert("Precondition:", pick.HasChanges);

				var grid = form.UserControl.LinesGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText(MenuItemName);
				grid.SelectElements(order.Lines[0]);
				UnitTestUserNotification.Instance.AddYesAnswer();
				menuItem.PerformClick();
				AssertEquals($"Save pick before attempting to '{FunctionDescription}'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_SuccessfullyReducesBOMKits

		public void TestReduceStockMenuItem_SuccessfullyReducesBOMKits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			Factory.Save();

			SecurityCheckpoint.IsAllowed = true;

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var menuItem = form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText(MenuItemName);
				form.UserControl.LinesGrid.SelectElements(order.Lines[0]);
				UnitTestUserNotification.Instance.AddYesAnswer();
				AssertEquals("Precondition: Should have full Release Line Quantity.", 10m, order.Lines[0].ReleaseLines[0].Quantity);

				menuItem.PerformClick();
				AssertEquals($"1 Lines were selected. Are you sure you want to fully bulk {FunctionDescription}?", UnitTestUserNotification.Instance.LastMessage.Text);

				_ = order.Lines[0].ReleaseLines.Count; // poke for rebuilding ReleaseLines
				AssertEquals("Should have Reduced Release Line Quantity.", 4m, order.Lines[0].ReleaseLines[0].Quantity);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_ReducesStockOnOriginal

		public void TestReduceStockMenuItem_ReducesStockOnOriginal()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines[0];
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			var originalPickInventoryLine = pickLine.WZ_WE_OriginalPickedInventoryLine;

			SecurityCheckpoint.IsAllowed = true;

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var menuItem = form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText(MenuItemName);
				form.UserControl.LinesGrid.SelectElements(order.Lines[0]);
				AssertEquals("Precondition:", 0m, receive.Inventory[0].WI_TotalUnits);
				UnitTestUserNotification.Instance.AddYesAnswer();
				menuItem.PerformClick();

				AssertEquals("Stock should not change.", 0m, receive.Lines[0].WE_StockOnHand);
				var docket = (WhsDocket)order.RelatedJobs.Single();
				AssertEquals(ExpectedDocketTypeCreated, docket.WD_DocketType);
				var docketPickLine = docket.Lines.Single().PickLines.Single();
				var splitTransferLine = pick.Transfers.Single().Lines.Single(l => l.IsFinalised);
				AssertEquals("Adjustment/Transfer should have adjusted the correct Inventory.", splitTransferLine.PK, docketPickLine.WZ_WE_InventoryLine);
				AssertEquals("Adjustment/Transfer should have adjusted the correct Inventory.", originalPickInventoryLine, splitTransferLine.PickLines.Single().WZ_WE_InventoryLine);
				AssertEquals("Adjustment/Transfer should have adjusted the correct Amount.", 10m, docketPickLine.WZ_Units);
			}
		}

		#endregion

		#region TestReduceStockMenuItem_DisplaysErrorIfUserNotAuthorized

		public void TestReduceStockMenuItem_DisplaysErrorIfUserNotAuthorized()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			SecurityCheckpoint.IsAllowed = false;

			using (var form = GetNewDocketLinesTestForm(order))
			{
				form.UserControl.ReduceStockOptionsAreVisible = true;
				form.Show();

				var menuItem = form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText(MenuItemName);
				form.UserControl.LinesGrid.SelectElements(order.Lines[0]);
				UnitTestUserNotification.Instance.AddYesAnswer();
				menuItem.PerformClick();

				AssertEquals("Expected security warning as user does not have permission to reduce stock", SecurityCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected abstract ZString MenuItemName { get; }
		protected abstract ZString FunctionDescription { get; }
		protected abstract ReduceStockReason ReduceStockReason { get; }
		protected abstract SecurityCheckpoint SecurityCheckpoint { get; }

		protected abstract ZString ExpectedDocketTypeCreated { get; }

		protected override void SetUp()
		{
			base.SetUp();

			UnitTestUserNotification.Instance.ClearUserResponses();
		}

		#endregion
	}

	#endregion
}
