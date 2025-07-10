using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class TransferEntryFormTest : WhsDocketFormTestCase
	{
		#region Constructors

		#region TestConstructor

		public void TestConstructor()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Posting not setup", true, form.SetupPostingCalledForTest);
				AssertBinding(form.FinaliseButtonForTest, "ReadOnly", "IsUserAllowedToFinalise");
			}
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new TransferEntryForm(transfer, null));
		}

		#endregion

		#region TestConstructor_PlugIns

		public void TestConstructor_PlugIns()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				AssertNotNull("DocDataPlugIn is not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull("eDocsPlugIn is not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		#endregion

		#region TestEDocsModifySetToOffNotDisablePlugin

		public void TestEDocsModifySetToOffNotDisablePlugin()
		{
			Env.Security.eDocs.IsAllowed = true;
			Env.Security.eDocsModify.IsAllowed = false;

			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var eDocPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				Assert("Plugin should not be disabled.", eDocPlugin.SecurityCheckpoint.IsAllowed);
			}
		}

		#endregion

		#region TestConstructor_MenuItems

		public void TestConstructor_MenuItems()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				AssertNotNull("Finalize Line(s)", form.ActionsMenuItemForTest.MenuItems.FindByText("Finalize Line(s)"));

				AssertNotNull("Allocate Same Destination Locations", form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Same Destination Locations"));
				AssertNotNull("Clear Destination Locations", form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Destination Locations"));

				AssertNotNull("Clear Destination Pallet IDs", form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Destination Pallet IDs"));
				AssertNotNull("Generate Destination Pallet IDs", form.ActionsMenuItemForTest.MenuItems.FindByText("Generate Destination Pallet IDs"));
				AssertNotNull("Update Destination Pallet IDs", form.ActionsMenuItemForTest.MenuItems.FindByText("Update Destination Pallet IDs"));
				AssertNotNull("Change task planning status to ready", form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to ready"));
			}
		}

		#endregion

		public void TestSetupInventoryFilterStripUserControl()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region GetNewDocketForm

		protected override ZForm GetNewDocketForm()
		{
			return new TransferEntryForm(Factory.New<WhsTransfer>(), new NotificationSubscriberGuiHelper());
		}

		#endregion

		#region ZForm Overloads

		public void TestOnLoad()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.EndInvoke(form.LoadDeferredAsyncResultForTest);
				AssertEquals("Focus is not on OrgWhsFindControl", form.OrgWhsFindControlForTest, form.ActiveControl);
			}
		}

		#endregion

		#region Properties

		public void TestDocket()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals(transfer, form.DocketForTest);
			}
		}

		public void TestFormCaption()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Transfer", form.FormCaption.Trim());
				transfer.WD_DocketID = "W00000001";
				AssertEquals("Transfer W00000001", form.FormCaption);
			}
		}

		public void TestShowBottomPanel()
		{
			using (var form = new TransferEntryForm(Factory.New<WhsTransfer>(), new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.BottomPanelForTest.Visible);
				AssertEquals(true, form.Splitter1ForTest.Visible);

				form.ShowBottomPanel = false;
				AssertEquals(false, form.BottomPanelForTest.Visible);
				AssertEquals(false, form.Splitter1ForTest.Visible);
			}
		}

		#endregion

		#region Action Menu Handlers

		#region AllocateSameDestinationLocations

		public void TestOnAllocateSameDestinationLocations()
		{
			Helper.CreateWarehouse("WHS", "A", 4, 1);
			var transfer = Factory.New<WhsTransfer>();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();
			line1.TransferFromLocationString = "A-1-1";
			line2.TransferFromLocationString = "A-2-1";
			line3.TransferFromLocationString = "A-3-1";
			line1.LocationString = "A-4-1";

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.OnAllocateSameDestinationLocationsForTest(null, EventArgs.Empty);
				AssertEquals("A-4-1", line2.LocationString);
				AssertEquals("A-4-1", line3.LocationString);

				line1.LocationString = "";
				line2.LocationString = "";
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(0);
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(2);

				form.OnAllocateSameDestinationLocationsForTest(null, EventArgs.Empty);
				AssertEquals("A-4-1", line1.LocationString);
				AssertEquals("", line2.LocationString);
				AssertEquals("A-4-1", line3.LocationString);
			}
		}

		#endregion

		#region ClearDestinationLocations

		public void TestOnClearDestinationLocations()
		{
			Helper.CreateWarehouse("WHS", "A", 3, 1);
			var transfer = Factory.New<WhsTransfer>();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				Assert("Precondition", line1.LocationString.IsEmpty && line2.LocationString.IsEmpty && line3.LocationString.IsEmpty);

				line1.LocationString = "A-1-1";
				line2.LocationString = "A-2-1";
				line3.LocationString = "A-3-1";
				form.OnClearDestinationLocationsForTest(null, EventArgs.Empty);
				AssertEquals("", line1.LocationString);
				AssertEquals("", line2.LocationString);
				AssertEquals("", line3.LocationString);

				line1.LocationString = "A-1-1";
				line2.LocationString = "A-2-1";
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(0);
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(2);

				form.OnClearDestinationLocationsForTest(null, EventArgs.Empty);
				AssertEquals("", line1.LocationString);
				AssertEquals("A-2-1", line2.LocationString);
				AssertEquals("", line3.LocationString);
			}
		}

		#endregion

		#region GenerateDestinationPalletIDs

		public void TestOnGenerateDestinationPalletIDs()
		{
			var transfer = Factory.New<WhsTransfer>();
			transfer.WD_DocketID = "Transfer_000666";
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();
			line1.WE_TransferFromPalletId = "P_ID_001";
			line2.WE_TransferFromPalletId = "P_ID_002";
			line3.WE_TransferFromPalletId = "P_ID_003";

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				Assert("Precondition", line1.WE_PalletID.IsEmpty && line2.WE_PalletID.IsEmpty && line3.WE_PalletID.IsEmpty);

				form.OnGenerateDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Transfer_000666-0001", line1.WE_PalletID);
				AssertEquals("Transfer_000666-0002", line2.WE_PalletID);
				AssertEquals("Transfer_000666-0003", line3.WE_PalletID);

				line1.WE_PalletID = "";
				line2.WE_PalletID = "";
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(0);
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(2);

				form.OnGenerateDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Transfer_000666-0001", line1.WE_PalletID);
				AssertEquals("", line2.WE_PalletID);
				AssertEquals("Transfer_000666-0003", line3.WE_PalletID);
			}
		}

		public void TestOnGenerateDestinationPalletIDs_NoDocketID()
		{
			var transfer = Factory.New<WhsTransfer>();
			var line1 = transfer.Lines.AddNew();
			line1.WE_TransferFromPalletId = "P_ID_001";

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				Assert("Precondition", line1.WE_PalletID.IsEmpty);

				form.OnGenerateDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Generate should not of executed", "", line1.WE_PalletID);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Transfer before Generating Pallet IDs"));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);

				UnitTestUserNotification.Instance.ClearMessages();
				transfer.WD_DocketID = "Transfer_000666";
				form.OnGenerateDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Transfer_000666-0001", line1.WE_PalletID);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region UpdateDestinationPalletIDs

		public void TestOnUpdateDestinationPalletIDs()
		{
			var transfer = Factory.New<WhsTransfer>();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();
			line1.WE_TransferFromPalletId = "P_ID_001";
			line2.WE_TransferFromPalletId = "P_ID_002";
			line3.WE_TransferFromPalletId = "P_ID_003";

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				Assert("Precondition", line1.WE_PalletID.IsEmpty && line2.WE_PalletID.IsEmpty && line3.WE_PalletID.IsEmpty);

				form.OnUpdateDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("P_ID_001", line1.WE_PalletID);
				AssertEquals("P_ID_002", line2.WE_PalletID);
				AssertEquals("P_ID_003", line3.WE_PalletID);

				line1.WE_PalletID = "";
				line2.WE_PalletID = "";
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(0);
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(2);

				form.OnUpdateDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("P_ID_001", line1.WE_PalletID);
				AssertEquals("", line2.WE_PalletID);
				AssertEquals("P_ID_003", line3.WE_PalletID);
			}
		}

		#endregion

		#region ClearDestinationPalletIDs

		public void TestOnClearDestinationPalletIDs()
		{
			var transfer = Factory.New<WhsTransfer>();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				Assert("Precondition", line1.WE_PalletID.IsEmpty && line2.WE_PalletID.IsEmpty && line3.WE_PalletID.IsEmpty);

				line1.WE_PalletID = "P_ID_001";
				line2.WE_PalletID = "P_ID_002";
				line3.WE_PalletID = "P_ID_003";
				form.OnClearDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("", line1.WE_PalletID);
				AssertEquals("", line2.WE_PalletID);
				AssertEquals("", line3.WE_PalletID);

				line1.WE_PalletID = "P_ID_001";
				line2.WE_PalletID = "P_ID_002";
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(0);
				form.TransferDocketLinesGridUserControlForTest.LinesGrid.Select(2);

				form.OnClearDestinationPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("", line1.WE_PalletID);
				AssertEquals("P_ID_002", line2.WE_PalletID);
				AssertEquals("", line3.WE_PalletID);
			}
		}

		#endregion

		#region Assign All Lines to users menu items

		#region TestOnAssignAllLinesToUserMenuItems

		public void TestOnAssignAllLinesToUserMenuItems()
		{
			var transfer = Factory.New<WhsTransfer>();
			transfer.Lines.AddNew();

			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var assignAllLinesMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Assign All Lines To User");

				AssertEquals(true, assignAllLinesMenuItem.Enabled);

				transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
				transfer.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertEquals(false, assignAllLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region Test PickMenuItem

		[TestDate(2012, 05, 18)]
		public void TestPickMenuItem()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			var activeStaff2 = Helper.CreateGlbStaff("02", "T2", true);
			var activeStaff3 = Helper.CreateGlbStaff("03", "T3", true);
			var inactiveStaff = Helper.CreateGlbStaff("04", "T4", false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			var lineWithEmptyPickTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			using (new SemaphoreManager(lineWithEmptyPickTime.FinaliseDocketLineSemaphore))
			{
				lineWithEmptyPickTime.WE_PutawayTime = ZDateTimeOffset.Now.AddDays(2);
			}
			lineWithEmptyPickTime.WE_GS_NKPutawayBy = ""; // clear default

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var pickMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Assign All Lines To User").MenuItems.FindByText("Pick Only");
				pickMenuItem.PerformClick();

				var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
				var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(form.TransferLineAttacherForTesting);

				AssertCollectionContains("Collection should contain active staff.", activeStaff1, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff2, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff3, staffCollection);
				AssertCollectionNotContains("Collection should not contain inactive staff.", inactiveStaff, staffCollection);

				// All assigned lines

				Enterprise.Warehouse.Transactions.GUI.Testing.TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(form.LastShownTransferLineAttachPopupForTesting, activeStaff1);
				AssertEquals("", lineWithEmptyPickTime.WE_GS_NKPutawayBy);
				AssertEquals(activeStaff1.GS_Code, lineWithEmptyPickTime.GS_NKPickedBy);

				// Assigned and un assigned lines

				var lineWithNonEmptyPickTime = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
				lineWithNonEmptyPickTime.PickedTime = ZDateTimeOffset.Now.AddDays(-1);
				lineWithNonEmptyPickTime.GS_NKPickedBy = ""; // set automatically, clear default

				pickMenuItem.PerformClick();
				Enterprise.Warehouse.Transactions.GUI.Testing.TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(form.LastShownTransferLineAttachPopupForTesting, activeStaff2);

				AssertEquals("", lineWithEmptyPickTime.WE_GS_NKPutawayBy);
				AssertEquals(activeStaff2.GS_Code, lineWithEmptyPickTime.GS_NKPickedBy);

				AssertEquals("", lineWithNonEmptyPickTime.WE_GS_NKPutawayBy);
				AssertEquals("", lineWithNonEmptyPickTime.GS_NKPickedBy);

				// All assigned lines
				using (new SemaphoreManager(lineWithEmptyPickTime.FinaliseDocketLineSemaphore))
				{
					lineWithEmptyPickTime.WE_PutawayTime = ZDateTimeOffset.Empty;
				}
				lineWithEmptyPickTime.PickedTime = ZDateTimeOffset.Now.AddDays(-1);
				lineWithNonEmptyPickTime.PickedTime = ZDateTimeOffset.Now.AddDays(-1);
				lineWithNonEmptyPickTime.GS_NKPickedBy = ""; // set automatically, clear default

				pickMenuItem.PerformClick();
				AssertEquals("Since there are no un-assigned lines, it should show an error message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("No lines can be assigned. There are no lines with empty Picked Time, or Picking has already commenced.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("", lineWithEmptyPickTime.WE_GS_NKPutawayBy);
				AssertEquals(activeStaff2.GS_Code, lineWithEmptyPickTime.GS_NKPickedBy);

				AssertEquals("", lineWithNonEmptyPickTime.WE_GS_NKPutawayBy);
				AssertEquals("", lineWithNonEmptyPickTime.GS_NKPickedBy);
			}
		}

		#endregion

		#endregion

		#region Test Unassign Lines

		#region TestUnAssignAllLines_MenuEnable

		public void TestUnAssignAllLines_MenuEnable()
		{
			var transfer = Factory.New<WhsTransfer>();
			transfer.Lines.AddNew();

			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var unassignAllLinesMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Un-assign All Lines");

				AssertEquals(true, unassignAllLinesMenuItem.Enabled);

				transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
				transfer.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertEquals(false, unassignAllLinesMenuItem.Enabled);

				transfer.WD_FinalisedDate = ZDateTimeOffset.Empty;
				transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
				AssertEquals(false, unassignAllLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestUnAssignAllLines_Click

		public void TestUnAssignAllLines_Click()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var notify = new TestNotificationBuffer();
			var user = Helper.CreateGlbStaff("XYZ", "XYZ");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, loc1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, loc1);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, loc1, loc2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, loc1, loc2);

			Helper.CreateWhsPickLine(transferLine1, inventory1, 10m);
			Helper.CreateWhsPickLine(transferLine2, inventory2, 5m);

			Factory.Save();

			var pickLines = new List<WhsPickLine>();

			pickLines.AddRange(transferLine1.PickLines);
			pickLines.AddRange(transferLine2.PickLines);

			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			foreach (var line in pickLines)
			{
				line.WZ_GS_NKAssignedTo = user.GS_Code;
			}

			Factory.Save();

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var unassignAllLinesMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Un-assign All Lines").MenuItems.FindByText("Pick Only");
				AssertEquals("Pre-condition", true, unassignAllLinesMenuItem.Enabled);
				unassignAllLinesMenuItem.PerformClick();

				AssertEquals("Pick lines have been un-assigned", true, pickLines.All(pl => pl.AssignedTo == null));
			}
		}

		#endregion

		#endregion

		#region TestChangeTaskPlanningStatusMenu

		const string ChangeTaskPlanningStatusToReadyMenuText = "Change task planning status to ready";
		const string ChangeTaskPlanningStatusToNotReadyMenuText = "Change task planning status to not ready";

		#region TestChangeTaskPlanningStatusMenuVisibility

		public void TestChangeTaskPlanningStatusMenuVisibility_TaskManagementEnabled()
		{
			TestChangeTaskPlanningStatusMenuVisibilityCore(true, true);
		}

		public void TestChangeTaskPlanningStatusMenuVisibility_TaskManagementNotEnabled()
		{
			TestChangeTaskPlanningStatusMenuVisibilityCore(false, false);
		}

		void TestChangeTaskPlanningStatusMenuVisibilityCore(bool isTaskManagementEnabled, bool expectedVisibility)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			if (isTaskManagementEnabled)
			{
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			}
			Factory.Save();

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			Factory.Save();

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(expectedVisibility, form.ActionsMenuItemForTest.MenuItems.FindByText(ChangeTaskPlanningStatusToReadyMenuText).Visible);
			}
		}

		#endregion

		#region TestChangeTaskPlanningStatusMenu_Status

		public void TestChangeTaskPlanningStatusMenu_StatusIsEmpty()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(string.Empty, TaskPlanningStatus.Codes.Ready, ChangeTaskPlanningStatusToReadyMenuText, ChangeTaskPlanningStatusToNotReadyMenuText);
		}

		public void TestChangeTaskPlanningStatusMenu_StatusIsNotReady()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.Ready, ChangeTaskPlanningStatusToReadyMenuText, ChangeTaskPlanningStatusToNotReadyMenuText);
		}

		public void TestChangeTaskPlanningStatusMenu_StatusIsReady()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.NotReady, ChangeTaskPlanningStatusToNotReadyMenuText, ChangeTaskPlanningStatusToReadyMenuText);
		}

		public void TestChangeTaskPlanningStatusMenu_StatusIsPlanned()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.NotReady, ChangeTaskPlanningStatusToNotReadyMenuText, ChangeTaskPlanningStatusToReadyMenuText);
		}

		void TestChangeTaskPlanningStatusMenu_StatusCore(string status, string expectedStatus, string expectedMenuTextBefore, string expectedMenuTextAfter)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			transfer.WD_TaskPlanningStatus = status;
			Factory.Save();

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menu = form.ActionsMenuItemForTest.MenuItems.FindByText(expectedMenuTextBefore);
				AssertEquals(true, menu.Visible);
				TestChangeTaskPlanningStatusMenu_FieldsReadOnlyCore(transfer, status == TaskPlanningStatus.Codes.Ready || status == TaskPlanningStatus.Codes.Planned);

				menu.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(expectedStatus, transfer.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(expectedMenuTextAfter).Visible);
				});
				TestChangeTaskPlanningStatusMenu_FieldsReadOnlyCore(transfer, expectedStatus == TaskPlanningStatus.Codes.Ready || expectedStatus == TaskPlanningStatus.Codes.Planned);
			}
		}

		void TestChangeTaskPlanningStatusMenu_FieldsReadOnlyCore(WhsTransfer transfer, bool expectedReadOnly)
		{
			CombineAssertions(() =>
			{
				AssertEquals(expectedReadOnly, transfer.ReadOnly);
				AssertEquals(expectedReadOnly, transfer.WD_OH_ClientInfo.ReadOnly);
				AssertEquals(expectedReadOnly, transfer.WD_ExternalReferenceInfo.ReadOnly);
				AssertEquals(expectedReadOnly, transfer.WD_WW_WhsInfo.ReadOnly);
				AssertEquals(expectedReadOnly, transfer.WD_DocketSubTypeInfo.ReadOnly);
				AssertEquals(expectedReadOnly, transfer.WD_WP_PickBeingReplenishedInfo.ReadOnly);
			});
		}

		public void TestChangeTaskPlanningStatusMenu_ChangeToReadyAndThenChangeAgain()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			Factory.Save();

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menu = form.ActionsMenuItemForTest.MenuItems.FindByText(ChangeTaskPlanningStatusToReadyMenuText);
				AssertEquals(true, menu.Visible);

				menu.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(ChangeTaskPlanningStatusToNotReadyMenuText).Visible);
				});
				form.FireSaveButton();

				menu.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.NotReady, transfer.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(ChangeTaskPlanningStatusToReadyMenuText).Visible);
				});
			}
		}

		public void TestChangeTaskPlanningStatusMenu_ChangeToNotReadyAndThenChangeAgain()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menu = form.ActionsMenuItemForTest.MenuItems.FindByText(ChangeTaskPlanningStatusToNotReadyMenuText);
				AssertEquals(true, menu.Visible);

				menu.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.NotReady, transfer.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(ChangeTaskPlanningStatusToReadyMenuText).Visible);
				});
				form.FireSaveButton();

				menu.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(ChangeTaskPlanningStatusToNotReadyMenuText).Visible);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Finalise Button

		public void TestFinaliseButton()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryWithSaveFactoryForWarehouse();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", data.Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10, "A-1", "A-2");

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				// test user has no access
				Env.Security.WhsTransferFinalise.IsAllowed = false;
				form.FinaliseButtonForTest.PerformClick();

				// check for security error
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				// ensure transfer wasnt processed
				AssertEquals(false, transfer.IsFinalised);

				// test user has access
				Env.Security.WhsTransferFinalise.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(false, form.BusinessEntity.HasErrors());

				// test finalisation worked and notification subscriber was popped
				form.FinaliseButtonForTest.PerformClick();
				AssertEquals(true, transfer.IsFinalised);
				AssertEquals(form.GetNotificationBufferForTest(), transfer.NotificationManager.LastPopped);

				// ensure no security error occurred
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
			}
		}

		public void TestFinaliseButtonValidatesAndSaves()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryWithSaveFactoryForWarehouse();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", data.Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 0, "A-1", "A-2");

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.FinaliseButtonForTest.PerformClick();
				AssertEquals(false, transfer.IsInDatabase);
				AssertEquals(false, transfer.IsFinalised);
				AssertEquals(true, transfer.HasErrors);

				transferLine.WE_TransactionQuantity = 10;
				form.FinaliseButtonForTest.PerformClick();
				AssertEquals(true, transfer.IsInDatabase);
				AssertEquals(true, transfer.IsFinalised);
				AssertEquals(false, transfer.HasErrors);
			}
		}

		public void TestFinaliseButton_CallTransferPreSaveValidation()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryWithSaveFactoryForWarehouse();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", data.Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, "A-1", "A-2");

			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var countOfTransferRunPreSaveValidation = 0;
				var countOfTransferLineLineRunPreSaveValidation = 0;
				transfer.WD_DocketTypeInfo.AdditionalValidation += () => { countOfTransferRunPreSaveValidation++; };
				transferLine.WE_OPInfo.AdditionalValidation += () => { countOfTransferLineLineRunPreSaveValidation++; };

				form.FinaliseButtonForTest.PerformClick();
				AssertEquals(true, transfer.IsFinalised);
				AssertEquals("Should have run Transfer's RunPreSaveValidation before Finalise and during Finalise. This is because validation during finalise can be different.", 2, countOfTransferRunPreSaveValidation);
				AssertEquals("Should have run TransferLine's RunPreSaveValidation before Finalise and during Finalise. This is because validation during finalise can be different.", 2, countOfTransferLineLineRunPreSaveValidation);
			}
		}

		#endregion

		#region INotifications Members

		public void TestNotify()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()))
			{
				var type = new TestINotificationType("Message", "NotZErrorMessageBox");
				var e = new TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				form.Notify(e);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("TestMessageToDisplay"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				type = new TestINotificationType("Finalize", "ZErrorMessageBox");
				e = new TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				form.Notify(e);
				AssertEquals("There are errors that need to be corrected before this Warehouse Transfer can be Finalized.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			}
		}

		public void TestQueryUser()
		{
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			var transfer = Factory.New<WhsTransfer>();
			using (var form = new TransferEntryForm(transfer, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
			}
		}

		#endregion

		#region TestConcurrentTransferFinalisation

		public void TestConcurrentTransferFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, sourceLocation, "", false);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, sourceLocation, destinationLocation);
			line.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var factoryToUpdateDBFirst = new BusinessObjectFactory() { RefreshEnabled = false };
			var factoryToUpdateDBSecond = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferLoadedInFactoryToUpdateDBFirst = factoryToUpdateDBFirst.Load<WhsTransfer>(transfer.PK);
			var transferLoadedInFactoryToUpdateDBSecond = factoryToUpdateDBSecond.Load<WhsTransfer>(transfer.PK);
			var lineInFactory1 = transferLoadedInFactoryToUpdateDBFirst.Lines.Single();
			var lineInFactory2 = transferLoadedInFactoryToUpdateDBSecond.Lines.Single();

			using (var form = new TransferEntryForm(transferLoadedInFactoryToUpdateDBFirst, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				lineInFactory1.LocationString = "A-1-3";
				transferLoadedInFactoryToUpdateDBFirst.NotificationManager.Push(Notify);
				form.FinaliseButtonForTest.PerformClick();
				AssertIsFinalisedPrecondition(transferLoadedInFactoryToUpdateDBFirst);
				form.FireSaveButton();
			}

			using (var form = new TransferEntryForm(transferLoadedInFactoryToUpdateDBSecond, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				transferLoadedInFactoryToUpdateDBSecond.NotificationManager.Push(Notify);
				AssertEquals("Precondtion", false, transferLoadedInFactoryToUpdateDBSecond.IsFinalised);

				lineInFactory2.LocationString = "A-1-4";
				AssertNoExceptionThrown("No exception should be thrown during finalisation.", () => form.FinaliseButtonForTest.PerformClick());
				AssertContains("While you have been working with this form, another user has made changes", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Transfer should not finalised.", false, transferLoadedInFactoryToUpdateDBSecond.IsFinalised);
				AssertNoExceptionThrown("No exception should be thrown during the save.", () => form.FireSaveButton());
			}
		}

		#endregion

		#region DocketID Sequence

		public void TestDocketIDSequence()
		{
			AssertDocketIDSequence("W00000001");
			AssertDocketIDSequence("W00000002");
			AssertDocketIDSequence("W00000003");
		}

		void AssertDocketIDSequence(ZString docketID)
		{
			var docket = Factory.NewWithValidTestData<WhsTransfer>();
			docket.WD_ExternalReference = docketID;
			docket.WD_DocketID = "";
			docket.WD_BookingDate = ZDateTimeOffset.Now;

			using (var form = new TransferEntryForm(docket, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", false, docket.IsInDatabase);
				form.PostingButtonsUserControlForTest.SaveAndCloseButton.PerformClick();
			}

			AssertEquals("Precondition", true, docket.IsInDatabase);
			AssertEquals(docketID, docket.WD_DocketID);
		}

		#endregion
	}
}
