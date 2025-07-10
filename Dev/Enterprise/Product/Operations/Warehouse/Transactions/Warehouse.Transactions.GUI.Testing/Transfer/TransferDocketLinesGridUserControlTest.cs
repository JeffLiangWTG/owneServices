using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class TransferDocketLinesGridUserControlTest : DocketLinesGridUserControlTest<TransferDocketLinesGridUserControl>
	{
		#region Constructor

		protected override void TestConstructorCore(TransferDocketLinesGridUserControl userControl)
		{
			base.TestConstructorCore(userControl);

			AssertNotNull("Find Destination Location MenuItem should be added", FindMenuItem(userControl.LinesGrid, "Find Destination Location"));
			AssertEquals("ColumnLayoutContext has been defined", nameof(DocketLinesGridContext.Transfer), userControl.LinesGrid.ColumnLayoutContext);

			// Columns for the Qty's witout matching lines should *NOT* be in the list
			AssertEquals(false, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Any(i => i.ColumnName == WhsTransferLine.Schema.WE_TransactionQuantity));
			AssertEquals(false, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Any(i => i.ColumnName == WhsTransferLine.Schema.WE_PackQuantity));

			// Columns for the Qty's including Matching lines should be in the list.
			userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Single(i => i.ColumnName == WhsTransferLine.Schema.QtyToMoveIncludingMatchingLines);
			userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Single(i => i.ColumnName == WhsTransferLine.Schema.PackQtyIncludingMatchingLines);
			userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Single(i => i.ColumnName == WhsTransferLine.Schema.QtyCommittedIncludingMatchingLines);
		}

		#endregion

		#region TestOnDocketSubTypeChanged

		protected override void TestOnDocketSubTypeChangedCore()
		{
			base.TestOnDocketSubTypeChangedCore();

			var docket = GetNewDocket();
			using (var form = GetNewDocketLinesTestForm(docket))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertEquals(TransferType.Codes.Internal, docket.WD_DocketSubType);
				AssertEquals(false, userControl.LinesGrid.Columns["TransferFromWarehousePK"].IsVisible);
				AssertEquals(true, userControl.LinesGrid.Columns["TransferFromWarehousePK"].IsUnavailable);
				AssertEquals(userControl.LinesGrid.Columns["TransferFromWarehousePK"].ToString() + " column is only available for Inter-Warehouse Destination Transfers", userControl.LinesGrid.Columns["TransferFromWarehousePK"].ErrorMessageWhenUnavailable);
				AssertEquals(false, userControl.LinesGrid.Columns["DestinationWarehousePK"].IsVisible);
				AssertEquals(true, userControl.LinesGrid.Columns["DestinationWarehousePK"].IsUnavailable);
				AssertEquals(userControl.LinesGrid.Columns["DestinationWarehousePK"].ToString() + " column is only available for Inter-Warehouse Source Transfers", userControl.LinesGrid.Columns["DestinationWarehousePK"].ErrorMessageWhenUnavailable);

				docket.WD_DocketSubType = TransferType.Codes.InterWhsSource;
				AssertEquals(false, userControl.LinesGrid.Columns["TransferFromWarehousePK"].IsVisible);
				AssertEquals(true, userControl.LinesGrid.Columns["TransferFromWarehousePK"].IsUnavailable);
				AssertEquals(true, userControl.LinesGrid.Columns["DestinationWarehousePK"].IsVisible);
				AssertEquals(false, userControl.LinesGrid.Columns["DestinationWarehousePK"].IsUnavailable);

				docket.WD_DocketSubType = TransferType.Codes.InterWhsDest;
				AssertEquals(true, userControl.LinesGrid.Columns["TransferFromWarehousePK"].IsVisible);
				AssertEquals(false, userControl.LinesGrid.Columns["TransferFromWarehousePK"].IsUnavailable);
				AssertEquals(false, userControl.LinesGrid.Columns["DestinationWarehousePK"].IsVisible);
				AssertEquals(true, userControl.LinesGrid.Columns["DestinationWarehousePK"].IsUnavailable);
			}
		}

		#endregion

		#region TestFindAttributes

		protected override void TestFindAttributes_Core(WhsDocketLine line, MenuItem menuItem, ContextMenu contextMenu)
		{
			var transferLine = (WhsTransferLine)line;
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Now;
			contextMenu.ShowPopupMenu();
			Assert("Should be disabled with finalised lines.", !menuItem.Enabled);

			// Undo finalised docket line, for finalise docket test part of the test
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Empty;
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
			contextMenu.ShowPopupMenu();
			Assert("Precondition.", menuItem.Enabled);

			transferLine.Docket.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			contextMenu.ShowPopupMenu();
			Assert("Should be disabled with Outbound Dock Door Transfers.", !menuItem.Enabled);
		}

		#endregion

		#region TestFindDestinationLocation

		public void TestFindDestinationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				form.UserControl.LinesGrid.Select(0);
				var menuItem = form.UserControl.LinesGrid.ContextMenu.MenuItems.FindByText("Find Destination Location");
				menuItem.PerformClick();
				var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				var module = popup.Module_ForTest;
				((BusinessObjectCollection)module.GridCollection).Load();
				((ZDisplayGrid)module.DisplayGrid).Select(0);
				popup.ExposedOKButtonForTesting.PerformClick();

				var transferLine = transfer.Lines[0];
				AssertEquals("A-2", transferLine.LocationString);

				form.UserControl.LinesGrid.ContextMenu.ShowPopupMenu();
				Assert("Precondition: enabled.", menuItem.Enabled);

				using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
				{
					transferLine.WE_PutawayTime = ZDateTimeOffset.Now;
				}
				Assert("Precondition", transferLine.LocationStringInfo.ReadOnly);
				form.UserControl.LinesGrid.ContextMenu.ShowPopupMenu();
				AssertEquals("Should not be enabled.", false, menuItem.Enabled);

				using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
				{
					transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
				}
				form.UserControl.LinesGrid.ContextMenu.ShowPopupMenu();
				Assert("Precondition: enabled.", menuItem.Enabled);

				transferLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(transferLine);

				form.UserControl.LinesGrid.ContextMenu.ShowPopupMenu();
				AssertEquals("Should not be enabled.", false, menuItem.Enabled);
			}
		}

		#endregion

		#region TestFinaliseLinesMenuItem

		public void TestFinaliseLinesMenuItem()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var finaliseLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Finalize Line(s)");
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When transfer is not finalised or canceled Finalise Lines functionality should be enabled.", true, finaliseLinesMenuItem.Enabled);

				transfer.FinaliseDocket();
				AssertIsFinalisedPrecondition(transfer);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When transfer is finalised, then Finalise Lines functionality should be disabled.", false, finaliseLinesMenuItem.Enabled);

				transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When transfer is Cancelled, then Finalise Lines functionality should be disabled.", false, finaliseLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestFinaliseLinesMenuItem_OutboundDockDoorTransfer

		public void TestFinaliseLinesMenuItem_OutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var finaliseLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Finalize Line(s)");
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When transfer is not an Outbound Dock Door transfer, Finalise Lines menu Item should be enabled.", true, finaliseLinesMenuItem.Enabled);

				transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When transfer is an Outbound Dock Door transfer, Finalise Lines menu Item should be disabled.", false, finaliseLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestFinaliseLinesMenuItem_InterWhs

		public void TestFinaliseLinesMenuItem_InterWhs_Dest()
		{
			TestFinaliseLinesMenuItem_InterWhs_Core(TransferType.Codes.InterWhsDest);
		}

		public void TestFinaliseLinesMenuItem_InterWhs_Source()
		{
			TestFinaliseLinesMenuItem_InterWhs_Core(TransferType.Codes.InterWhsSource);
		}

		void TestFinaliseLinesMenuItem_InterWhs_Core(string parentType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, parentType == TransferType.Codes.InterWhsDest ? whs2 : data.Whs1);
			transfer.DocketSubType = parentType;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", parentType == TransferType.Codes.InterWhsDest ? data.Whs1.PK : whs2.PK, "A");
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertNotNull("Precondition: Child Transfer created.", transferLine.ChildTransferLine);

			// Parent
			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var finaliseLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Finalize Line(s)");
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("When transfer is not finalised or canceled Finalise Lines functionality should be enabled on the parent.", true, finaliseLinesMenuItem.Enabled);
			}

			// Child
			using (var form = GetNewDocketLinesTestForm(transfer.ChildTransfers.Single()))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var finaliseLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Finalize Line(s)");
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Finalise Lines should *not* be enabled on the child.", false, finaliseLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region AssignSelectedLinesToUser

		#region TestAssignSelectedLinesToUserMenuItems

		public void TestAssignSelectedLinesToUserMenuItems()
		{
			var transfer = Factory.New<WhsTransfer>();
			transfer.Lines.AddNew();

			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var assignAllLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Assign selected lines to User");
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(false, assignAllLinesMenuItem.Enabled);

				grid.Select(0);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(true, assignAllLinesMenuItem.Enabled);

				transfer.WD_FinalisedDate = DateTime.Now;
				form.UserControl.LinesGrid.ContextMenu.ShowPopupMenu();
				AssertEquals(false, assignAllLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestAssignPickOnly

		public void TestAssignPickOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			var activeStaff2 = Helper.CreateGlbStaff("02", "T2", true);
			var inactiveStaff = Helper.CreateGlbStaff("04", "T4", false);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = (WhsInventoryView)receive.Inventory.Single();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			var selectedLineWithEmptyPickTimeAndEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			var selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");

			using (new SemaphoreManager(selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.FinaliseDocketLineSemaphore))
			{
				selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy = ""; // Clear default

			var selectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			selectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			using (new SemaphoreManager(selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.FinaliseDocketLineSemaphore))
			{
				selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy = ""; // Clear default

			var nonSelectedLineWithEmptyPickTimeAndEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			var nonSelectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			using (new SemaphoreManager(nonSelectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.FinaliseDocketLineSemaphore))
			{
				nonSelectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			nonSelectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy = ""; // Clear default

			var nonSelectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			nonSelectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			using (new SemaphoreManager(nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.FinaliseDocketLineSemaphore))
			{
				nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy = ""; // Clear default

			selectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.GS_NKPickedBy = ""; // clear default 
			selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy = ""; // clear default 
			nonSelectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.GS_NKPickedBy = ""; // clear default 
			nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy = ""; // clear default

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;

				AssertEquals("Pre-condition", 8, grid.List.Count);

				var assignSelectedLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Assign selected lines to User");
				var pickOnlyMenuItem = assignSelectedLinesMenuItem.MenuItems.FindByText("Pick Only");

				grid.SelectElements(new BusinessObject[] { selectedLineWithEmptyPickTimeAndEmptyPutAwayTime, selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime, selectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime, selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime });
				pickOnlyMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
				var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(form.UserControl.TransferLineAttacherForTesting);

				AssertCollectionContains("Collection should contain active staff.", activeStaff1, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff2, staffCollection);
				AssertCollectionNotContains("Collection should not contain inactive staff.", inactiveStaff, staffCollection);

				form.UserControl.LastShownTransferLineAttachPopupForTesting.SelectStaffForEmbeddedModuleSelection(activeStaff1);

				AssertEquals("", selectedLineWithEmptyPickTimeAndEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals(activeStaff1.GS_Code, selectedLineWithEmptyPickTimeAndEmptyPutAwayTime.GS_NKPickedBy);

				AssertEquals("", selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals(activeStaff1.GS_Code, selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy);

				AssertEquals("", selectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals("", selectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.GS_NKPickedBy);

				AssertEquals("", selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals("", selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy);

				// Non selected lines should not be allocated

				AssertEquals("", nonSelectedLineWithEmptyPickTimeAndEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals("", nonSelectedLineWithEmptyPickTimeAndEmptyPutAwayTime.GS_NKPickedBy);
				AssertEquals("", nonSelectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals("", nonSelectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy);
				AssertEquals("", nonSelectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals("", nonSelectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime.GS_NKPickedBy);
				AssertEquals("", nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.WE_GS_NKPutawayBy);
				AssertEquals("", nonSelectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy);

				grid.SelectElements(new[] { selectedLineWithNonEmptyPickTimeAndNonEmptyPutAwayTime });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should be disabled, since selected line is with non-emtpy pick and putaway times.", false, pickOnlyMenuItem.Enabled);

				grid.SelectElements(new[] { selectedLineWithEmptyPickTimeAndEmptyPutAwayTime });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should be enabled, since selected line is with emtpy pick and putaway times.", true, pickOnlyMenuItem.Enabled);

				grid.SelectElements(new[] { selectedLineWithEmptyPickTimeAndNonEmptyPutAwayTime });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should be enabled, since selected line is with emtpy pick time.", true, pickOnlyMenuItem.Enabled);

				grid.SelectElements(new[] { selectedLineWithNonEmptyPickTimeAndEmptyPutAwayTime });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should be enabled, since selected line is with emtpy putaway time.", false, pickOnlyMenuItem.Enabled);

				grid.SelectElements(Array.Empty<BusinessObject>());
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since no element is selected, Selected Lines menu item should be disabled.", false, assignSelectedLinesMenuItem.Enabled);

				grid.SelectElements(new[] { selectedLineWithEmptyPickTimeAndEmptyPutAwayTime });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Precondition: Enabled.", true, pickOnlyMenuItem.Enabled);

				selectedLineWithEmptyPickTimeAndEmptyPutAwayTime.WE_TransactionQuantity = 1;
				selectedLineWithEmptyPickTimeAndEmptyPutAwayTime.LocationString = inventory.LocationString;
				selectedLineWithEmptyPickTimeAndEmptyPutAwayTime.TransferFromLocationString = inventory.LocationString;
				selectedLineWithEmptyPickTimeAndEmptyPutAwayTime.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(selectedLineWithEmptyPickTimeAndEmptyPutAwayTime);

				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should not be enabled, since selected line is finalised.", false, pickOnlyMenuItem.Enabled);
			}
		}

		#endregion

		#region TestAssignPickOnly_CannotChangeWhenIsPicking

		public void TestAssignPickOnly_CannotChangeWhenIsPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var ben = Helper.CreateGlbStaff("Ben", "Ben");
			var eli = Helper.CreateGlbStaff("Eli", "Eli");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, loc1, loc2);
			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(ben);
			transfer.RunPreSaveValidation(); // to commit inventory.
			Factory.Save();

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;

				AssertEquals("Pre-condition", 1, grid.List.Count);
				AssertEquals("Pre-condition", false, transferLine.PickLines[0].WZ_IsPicking);

				var assignSelectedLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Assign selected lines to User");
				var pickOnlyMenuItem = assignSelectedLinesMenuItem.MenuItems.FindByText("Pick Only");

				grid.SelectElements(new BusinessObject[] { transferLine });
				pickOnlyMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				form.UserControl.LastShownTransferLineAttachPopupForTesting.SelectStaffForEmbeddedModuleSelection(eli);

				AssertEquals(eli.GS_Code, transferLine.GS_NKPickedBy);
				AssertEquals("Should be able to assign the line.", true, ((ILineStaffAssigner)transferLine).CanAssignOrUnAssignLine());
				AssertNoErrors("Can change the picker if is not picking", transferLine.GS_NKPickedByInfo);

				transferLine.PickLines[0].WZ_IsPicking = true;

				form.UserControl.LastShownTransferLineAttachPopupForTesting.SelectStaffForEmbeddedModuleSelection(ben);
				AssertNoErrors("Should not have error.", transferLine.GS_NKPickedByInfo);
				AssertEquals("Cannot assign the line", false, ((ILineStaffAssigner)transferLine).CanAssignOrUnAssignLine());
				AssertEquals("Should not change picker", eli.GS_Code, transferLine.GS_NKPickedBy);
			}
		}

		#endregion

		#endregion

		#region TestUnAssignSelectedLines

		#region TestUnAssignSelectedLinesMenuItem

		public void TestUnAssignSelectedLinesMenuItem()
		{
			var transfer = Factory.New<WhsTransfer>();
			transfer.Lines.AddNew();

			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var assignAllLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Un-assign selected lines");
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(false, assignAllLinesMenuItem.Enabled);

				grid.Select(0);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(true, assignAllLinesMenuItem.Enabled);

				transfer.WD_FinalisedDate = DateTime.Now;
				form.UserControl.LinesGrid.ContextMenu.ShowPopupMenu();
				AssertEquals(false, assignAllLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestUnAssignPickOnly

		public void TestUnAssignPickOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("01", "T1", true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			var lineWithEmptyPickTime1 = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			lineWithEmptyPickTime1.GS_NKPickedBy = staff.GS_Code;

			var lineWithEmptyPickTime2 = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			lineWithEmptyPickTime2.GS_NKPickedBy = staff.GS_Code;

			AssertEquals(staff.GS_Code, lineWithEmptyPickTime1.GS_NKPickedBy);
			AssertEquals(staff.GS_Code, lineWithEmptyPickTime2.GS_NKPickedBy);

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;

				AssertEquals("Pre-condition", 2, grid.List.Count);

				var unAssignSelectedLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Un-assign selected lines");
				var pickOnlyMenuItem = unAssignSelectedLinesMenuItem.MenuItems.FindByText("Pick Only");

				grid.SelectElements(new BusinessObject[] { lineWithEmptyPickTime1 });
				pickOnlyMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("", lineWithEmptyPickTime1.GS_NKPickedBy);
				AssertEquals("line 2 is not cleared as it wasn't selected.", staff.GS_Code, lineWithEmptyPickTime2.GS_NKPickedBy);

				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should be enabled, since selected line is with empty pick and putaway times.", true, pickOnlyMenuItem.Enabled);
			}
		}

		public void TestUnAssignPickOnly_NoSelectedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("01", "T1", true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			var line1 = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			line1.GS_NKPickedBy = staff.GS_Code;
			var line2 = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			line2.GS_NKPickedBy = staff.GS_Code;

			AssertEquals(staff.GS_Code, line1.GS_NKPickedBy);
			AssertEquals(staff.GS_Code, line2.GS_NKPickedBy);

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;

				AssertEquals("Pre-condition", 2, grid.List.Count);

				var unAssignSelectedLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Un-assign selected lines");
				grid.SelectElements(Array.Empty<BusinessObject>());
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since no element is selected, Selected Lines menu item should be disabled.", false, unAssignSelectedLinesMenuItem.Enabled);
			}
		}

		public void TestUnAssignPickOnly_LinesWithPickedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("01", "T1", true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			var lineWithNonEmptyPickTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			lineWithNonEmptyPickTime.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			lineWithNonEmptyPickTime.GS_NKPickedBy = staff.GS_Code;

			AssertEquals(staff.GS_Code, lineWithNonEmptyPickTime.GS_NKPickedBy);

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var unAssignSelectedLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Un-assign selected lines");
				var pickOnlyMenuItem = unAssignSelectedLinesMenuItem.MenuItems.FindByText("Pick Only");

				grid.SelectElements(new BusinessObject[] { lineWithNonEmptyPickTime });
				pickOnlyMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(staff.GS_Code, lineWithNonEmptyPickTime.GS_NKPickedBy);

				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should be disabled, since selected line is with non-emtpy pick time.", false, pickOnlyMenuItem.Enabled);
			}
		}

		public void TestUnAssignPickOnly_LineWithPutawayTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("01", "T1", true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			var lineWithEmptyPickTimeAndNonEmptyPutAwayTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			lineWithEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy = staff.GS_Code;
			using (new SemaphoreManager(lineWithEmptyPickTimeAndNonEmptyPutAwayTime.FinaliseDocketLineSemaphore))
			{
				lineWithEmptyPickTimeAndNonEmptyPutAwayTime.WE_PutawayTime = ZDateTimeOffset.Now;
			}

			AssertEquals(staff.GS_Code, lineWithEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy);

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var unAssignSelectedLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Un-assign selected lines");
				var pickOnlyMenuItem = unAssignSelectedLinesMenuItem.MenuItems.FindByText("Pick Only");

				grid.SelectElements(new BusinessObject[] { lineWithEmptyPickTimeAndNonEmptyPutAwayTime });
				pickOnlyMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("", lineWithEmptyPickTimeAndNonEmptyPutAwayTime.GS_NKPickedBy);

				grid.SelectElements(new[] { lineWithEmptyPickTimeAndNonEmptyPutAwayTime });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Pick MenuItem should be enabled, since selected line is with empty pick time.", true, pickOnlyMenuItem.Enabled);
			}
		}

		#endregion

		#region TestUnAssignPickOnly_CannotChangeWhenIsPicking

		public void TestUnAssignPickOnly_CannotChangeWhenIsPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var ben = Helper.CreateGlbStaff("Ben", "Ben");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, loc1, loc2);
			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(ben);
			transfer.RunPreSaveValidation(); // to commit inventory.
			Factory.Save();

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;

				AssertEquals("Pre-condition", 1, grid.List.Count);
				AssertEquals("Pre-condition", false, transferLine.PickLines[0].WZ_IsPicking);

				var unAssignSelectedLinesMenuItem = grid.ContextMenu.MenuItems.FindByText("Un-assign selected lines");
				var pickOnlyMenuItem = unAssignSelectedLinesMenuItem.MenuItems.FindByText("Pick Only");

				grid.SelectElements(new BusinessObject[] { transferLine });
				transferLine.PickLines[0].WZ_IsPicking = true;
				AssertEquals("Precondition: GS_NKPickedBy is assigned.", ben.GS_Code, transferLine.GS_NKPickedBy);

				pickOnlyMenuItem.PerformClick();
				AssertNoErrors("Should not have error.", transferLine.GS_NKPickedByInfo);
				AssertEquals("Cannot assign the line", false, ((ILineStaffAssigner)transferLine).CanAssignOrUnAssignLine());
				AssertEquals("Should not change picker", ben.GS_Code, transferLine.GS_NKPickedBy);
			}
		}

		#endregion

		#endregion

		#region Drag-n-Drop

		public void TestOnDragDrop()
		{
			var transfer = (WhsTransfer)GetNewDocket();
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();
				AssertEquals("Precondition no lines in transfer", 0, transfer.Lines.Count);

				transfer.WD_WW_Whs = data.Whs1.PK;
				transfer.WD_OH_Client = data.Org1.PK;
				AssertEquals("Precondition collection allows new", true, ((IBindingList)transfer.Lines).AllowNew);

				form.UserControl.LinesGridDragAndDropManagerForTesting.OnDragDropCore(new BusinessObject[] { data.Line111 });
				AssertEquals("New line should be generated for transfer", 1, transfer.Lines.Count);
			}
		}

		public void TestOnDragDrop_TransferFromLocationNoError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var recieve1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, locationA1, "");
			var transfer = Factory.New<WhsTransfer>();

			using (var form = GetNewDocketLinesTestForm(transfer))
			{
				form.Show();
				AssertEquals("Precondition no lines in transfer", 0, transfer.Lines.Count);

				transfer.WD_WW_Whs = data.Whs1.PK;
				transfer.WD_OH_Client = data.Org1.PK;
				AssertEquals("Precondition collection allows new", true, ((IBindingList)transfer.Lines).AllowNew);

				var inventory = recieve1.Inventory[0];

				form.UserControl.LinesGridDragAndDropManagerForTesting.OnDragDropCore(new BusinessObject[] { inventory });
				AssertEquals("New line should be generated for transfer", 1, transfer.Lines.Count);
				AssertNoErrors(transfer.Lines[0].TransferFromLocationStringInfo);
			}
		}

		#endregion

		#region TestLocationAutoComplete

		public void TestLocationAutoCompleteDisabled()
		{
			using (var control = GetNewDocketLinesGridUserControl())
			{
				var transferFromLocationStyle = (ZCodeFindBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle("TransferFromLocationString");
				Assert("Transfer from location style should have auto complete disabled", transferFromLocationStyle.AutoCompleteDisabled);
				var destinationLocationStyle = (ZCodeFindBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle("LocationString");
				Assert("Destination location style should have auto complete disabled", destinationLocationStyle.AutoCompleteDisabled);
			}
		}

		public void TestNonLocationAutoCompleteEnabled()
		{
			using (var control = GetNewDocketLinesGridUserControl())
			{
				var putawayBystyle = (ZCodeFindBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle("WE_GS_NKPutawayBy");
				Assert("Put away by style should have auto complete enabled", !putawayBystyle.AutoCompleteDisabled);
			}
		}

		#endregion

		#region TestUnitAndPackQuantityNames

		protected override string UnitsColumnName => WhsTransferLine.Schema.QtyToMoveIncludingMatchingLines;

		protected override string PackQuantityColumnName => WhsTransferLine.Schema.PackQtyIncludingMatchingLines;

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsTransfer>();
		}

		protected override TransferDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
		{
			return new TransferDocketLinesGridUserControl();
		}

		protected override DocketLinesTestForm GetNewDocketLinesTestForm(WhsDocket docket)
		{
			return new TransferLinesTestForm(docket);
		}

		public class TransferLinesTestForm : DocketLinesTestForm
		{
			public TransferLinesTestForm(WhsDocket docket)
				: base(docket)
			{
			}

			protected override TransferDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
			{
				return new TransferDocketLinesGridUserControl();
			}
		}

		#endregion
	}
}
