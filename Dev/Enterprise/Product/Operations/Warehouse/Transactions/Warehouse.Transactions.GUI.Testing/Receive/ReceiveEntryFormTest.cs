using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.GUI.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class ReceiveEntryFormTest : WhsDocketFormTestCase
	{
		#region GetNewDocketForm

		protected override ZForm GetNewDocketForm()
		{
			return new ReceiveEntryForm(Factory.New<WhsReceive>(), new NotificationSubscriberGuiHelper());
		}

		#endregion

		#region ZForm Overloads

		#region TestOnLoad

		public void TestOnLoad()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.EndInvoke(form.LoadDeferredAsyncResultForTest);
				AssertEquals("Focus is not on OrgWhsFindControl", form.OrgWhsFindControlForTest, form.ActiveControl);
			}
		}

		#endregion

		#region TestValidateAndSave

		public void TestValidateAndSave()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = true;

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				data.Line111.WI_OP = ZGuid.Invalid;
				data.Line111.WI_OP_PartNum = "NewProduct";
				data.Line111.WI_InDocketLineUnits = 0m;
				form.ValidateAndSaveForTest();
				AssertEquals(0, UnitTestUserNotification.Instance.PreviousMessages.Count(
						 m => m.Text == form.CreateTemporaryProductsConfirmationMessageForTest)); // No Confirmation Notification expected

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				data.Line111.WI_InDocketLineUnits = 1m;
				form.ValidateAndSaveForTest();
				var queryUserMessage = UnitTestUserNotification.Instance.PreviousMessages.SingleOrDefault(
						m => m.Text == form.CreateTemporaryProductsConfirmationMessageForTest);
				AssertEquals("New Products Found", queryUserMessage.Caption); // Should have Confirmation Notification
			}
		}

		public void TestValidateAndSave_NewProductAndNoSecurityRight()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = false;

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				data.Line111.WI_OP = ZGuid.Invalid;
				data.Line111.WI_OP_PartNum = "NewProduct";
				data.Line111.WI_InDocketLineUnits = 1m;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSaveForTest();

				AssertEquals("No permission to Create Product Files", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(ZGuid.Invalid, data.Line111.WI_OP);
			}
		}

		public void TestValidateAndSave_NewProductAndUserChooseNotSave()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = false;

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				data.Line111.WI_OP = ZGuid.Invalid;
				data.Line111.WI_OP_PartNum = "NewProduct";
				data.Line111.WI_InDocketLineUnits = 1m;

				form.ValidateAndSaveForTest();
				var queryUserMessage = UnitTestUserNotification.Instance.PreviousMessages.SingleOrDefault(
						m => m.Text == form.CreateTemporaryProductsConfirmationMessageForTest);
				AssertEquals("New Products Found", queryUserMessage.Caption);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(ZGuid.Invalid, data.Line111.WI_OP);
			}
		}

		#endregion

		#endregion

		#region Properties

		public void TestDocket()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals(receive, form.DocketForTest);
			}
		}

		public void TestSelectedInventoryLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.InventoryGridControlForTest.SelectRow(receive.Lines[0].PK);
				form.InventoryGridControlForTest.SelectRow(receive.Lines[1].PK);
				form.InventoryGridUserControl2ForTest.SelectRow(receive.Lines[0].PK);

				AssertEquals(1, form.SelectedInventoryLinesForTest.Count());
				form.DetailsTabControlForTest.SelectedTab = form.DetailsTabControlForTest.GetTabPage("LinesTabPage");
				AssertEquals(2, form.SelectedInventoryLinesForTest.Count());
				form.DetailsTabControlForTest.SelectedTab = form.DetailsTabControlForTest.GetTabPage("ReferencesTabPage");
				AssertEquals(1, form.SelectedInventoryLinesForTest.Count());
				form.DetailsTabControlForTest.SelectedTab = form.DetailsTabControlForTest.GetTabPage("DetailsTabPage");
				AssertEquals(1, form.SelectedInventoryLinesForTest.Count());
			}
		}

		public void TestSelectedReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.InventoryGridControlForTest.SelectRow(receive.Lines[0].PK);
				form.InventoryGridControlForTest.SelectRow(receive.Lines[1].PK);
				form.InventoryGridUserControl2ForTest.SelectRow(receive.Lines[0].PK);

				AssertEquals("Should be 1 line on the Main Tab", 1, form.SelectedReceiveLinesForTest.Count());
				form.DetailsTabControlForTest.SelectedTab = form.DetailsTabControlForTest.GetTabPage("LinesTabPage");
				AssertEquals("Should be 2 lines on the Lines Tab", 2, form.SelectedReceiveLinesForTest.Count());
				form.DetailsTabControlForTest.SelectedTab = form.DetailsTabControlForTest.GetTabPage("ReferencesTabPage");
				AssertEquals("Should be 1 line on the References Tab", 1, form.SelectedReceiveLinesForTest.Count());
				form.DetailsTabControlForTest.SelectedTab = form.DetailsTabControlForTest.GetTabPage("DetailsTabPage");
				AssertEquals("Should be 1 line on the Details Tab", 1, form.SelectedReceiveLinesForTest.Count());
			}
		}

		#endregion

		#region ActionMenu

		public void TestConstructor_MenuItems()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Palletize Lines", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Palletize Lines").Enabled);
				AssertEquals("Generate New Pallet IDs", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Generate New Pallet IDs").Enabled);
				AssertEquals("Generate Same Pallet ID", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Generate Same Pallet ID").Enabled);
				AssertEquals("Generate Serial Numbers", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Generate Serial Numbers").Enabled);
				AssertEquals("Clear Pallet IDs", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Pallet IDs").Enabled);

				AssertEquals("Change Hold Code", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Hold Code").Enabled);
				AssertEquals("Set Same Hold Code for Product", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Set Same Hold Code for Product", findSubitems: true).Enabled);
				AssertEquals("Set Same Hold Code for Receipt", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Set Same Hold Code for Receipt", findSubitems: true).Enabled);
				AssertEquals("Clear Hold Codes", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Hold Codes", findSubitems: true).Enabled);

				AssertEquals("Allocate Locations", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Locations").Enabled);
				AssertEquals("Clear Locations", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Locations").Enabled);
				AssertEquals("Split Receipt by Quantity", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Split Receipt by Quantity").Enabled);
				AssertEquals("Split Receipt by Area Type", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Split Receipt by Area Type").Enabled);
				AssertEquals("Create Product Files", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Create Product Files").Enabled);
				AssertEquals("Start Receiving (Create ASN Lines)", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Start Receiving (Create ASN Lines)").Enabled);
				AssertEquals("Change Task Planning Status To Ready", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Enabled);

				AssertEquals("Update Actual Quantity with Expected", true, form.ActionsMenuItemForTest.MenuItems.FindByText("Update Actual Quantity with Expected").Enabled);
			}
		}

		public void TestActionsDisabled_WhenCreatedFromPickByBOM()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = helper.CreateWhsOrderLine(order, bike, 2m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			Factory.Save();

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			using (var form = new ReceiveEntryForm(createdReceive, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Palletize Lines").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Generate New Pallet IDs").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Generate Same Pallet ID").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Generate Serial Numbers").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Pallet IDs").Enabled);

				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Hold Code").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Set Same Hold Code for Product", findSubitems: true).Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Set Same Hold Code for Receipt", findSubitems: true).Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Hold Codes", findSubitems: true).Enabled);

				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Locations").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Locations").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Split Receipt by Quantity").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Split Receipt by Area Type").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Create Product Files").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Start Receiving (Create ASN Lines)").Enabled);
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Enabled);

				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Update Actual Quantity with Expected").Enabled);
			}
		}

		#region TestChangeTaskPlanningStatusMenuVisibility

		public void TestChangeTaskPlanningStatusMenuVisibility_InitialLoad()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);
			}
		}

		public void TestChangeTaskPlanningStatusMenuVisibility_TaskManagementEnabled()
		{
			TestChangeTaskPlanningStatusMenuVisibility_OnLoadCore(true, true);
		}

		public void TestChangeTaskPlanningStatusMenuVisibility_TaskManagementNotEnabled()
		{
			TestChangeTaskPlanningStatusMenuVisibility_OnLoadCore(false, false);
		}

		void TestChangeTaskPlanningStatusMenuVisibility_OnLoadCore(bool isTaskManagementEnabled, bool expectedVisibility)
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			receive.WD_WW_Whs = whs.PK;
			if (isTaskManagementEnabled)
			{
				whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			}
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(expectedVisibility, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);
			}
		}

		public void TestChangeTaskPlanningStatusMenuVisibility_OnSave_ChangeToWhsWithTaskManagementEnabled()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs1, "REC");
			receive.WD_BookingDate = ZDateTimeOffset.Now.AddDays(1);
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);

				whs2.WW_GG_ReleaseGroup = releaseGroup.PK;
				receive.WD_WW_Whs = whs2.PK;
				form.FireSaveButton();

				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);
			}
		}

		public void TestChangeTaskPlanningStatusMenuVisibility_OnSave_ChangeToWhsWithTaskManagementNotEnabled()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs1, "REC");
			receive.WD_BookingDate = ZDateTimeOffset.Now.AddDays(1);
			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);

				receive.WD_WW_Whs = whs2.PK;
				form.FireSaveButton();

				AssertEquals(false, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);
			}
		}

		#endregion

		#region TestChangeTaskPlanningStatusMenuText

		public void TestChangeTaskPlanningStatusMenuText_StatusIsEmpty()
		{
			TestChangeTaskPlanningStatusMenuTextCore(string.Empty, "Change Task Planning Status To Ready");
		}

		public void TestChangeTaskPlanningStatusMenuText_StatusIsNotReady()
		{
			TestChangeTaskPlanningStatusMenuTextCore(TaskPlanningStatus.Codes.NotReady, "Change Task Planning Status To Ready");
		}

		public void TestChangeTaskPlanningStatusMenuText_StatusIsReady()
		{
			TestChangeTaskPlanningStatusMenuTextCore(TaskPlanningStatus.Codes.Ready, "Change Task Planning Status To Not Ready");
		}

		public void TestChangeTaskPlanningStatusMenuText_StatusIsPlanned()
		{
			TestChangeTaskPlanningStatusMenuTextCore(TaskPlanningStatus.Codes.Planned, "Change Task Planning Status To Not Ready");
		}

		void TestChangeTaskPlanningStatusMenuTextCore(string status, string expectedText)
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			receive.WD_TaskPlanningStatus = status;
			receive.WD_WW_Whs = whs.PK;
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(expectedText).Visible);
			}
		}

		#endregion

		#region TestSetUnloadCompletedTimeMenu

		public void TestSetUnloadCompletedTimeMenu_Enable()
		{
			TestSetUnloadCompletedTimeMenuCore(false, true);
		}

		public void TestSetUnloadCompletedTimeMenu_Disable()
		{
			TestSetUnloadCompletedTimeMenuCore(true, false);
		}

		public void TestSetUnloadCompletedTime_PromptsToLinesWithInvalidLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation);

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Set Unload Completed Time");
				AssertEquals(true, menuItem.Enabled);

				menuItem.PerformClick();
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Cannot set unload complete time since at least one of the receive line(s) has not set a location."));
			}
		}

		void TestSetUnloadCompletedTimeMenuCore(bool isUnloadCompletedTimeSet, bool expectedEnableStatus)
		{
			var receive = Factory.New<WhsReceive>();
			if (isUnloadCompletedTimeSet)
			{
				receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			}

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Set Unload Completed Time");
				AssertEquals(expectedEnableStatus, menuItem.Enabled);

				if (!isUnloadCompletedTimeSet)
				{
					AssertEquals("Precondition:", false, receive.WD_UnloadCompletedTime.IsValid);
					menuItem.PerformClick();
					AssertEquals(true, receive.WD_UnloadCompletedTime.IsValid);
				}
			}
		}

		public void TestClearUnloadCompletedTimeMenu_Enable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC");
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompletedTime have already set", true, receive.WD_UnloadCompletedTime.IsValid);
			TestClearUnloadCompletedTimeMenu_DisableCore(receive, true);
		}

		public void TestClearUnloadCompletedTimeMenu_Disable_NoUnloadCompletedTimeSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC");
			Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompletedTime have already set", false, receive.WD_UnloadCompletedTime.IsValid);
			TestClearUnloadCompletedTimeMenu_DisableCore(receive, false);
		}

		public void TestClearUnloadCompletedTimeMenu_Disable_ReceiveFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			receive.FinaliseDocket();
			Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompletedTime have already set", true, receive.WD_UnloadCompletedTime.IsValid);
			TestClearUnloadCompletedTimeMenu_DisableCore(receive, false);
		}

		public void TestClearUnloadCompletedTimeMenu_Disable_HavePutAwayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT1");
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT1", 10m);
			putawayTransfer.RunPreSaveValidation();
			putawayTransferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompletedTime have already set", true, receive.WD_UnloadCompletedTime.IsValid);
			TestClearUnloadCompletedTimeMenu_DisableCore(receive, false);
		}

		void TestClearUnloadCompletedTimeMenu_DisableCore(WhsReceive receive, bool expectedEnableStatus)
		{
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Unload Completed Time");
				AssertEquals(expectedEnableStatus, menuItem.Enabled);

				if (expectedEnableStatus)
				{
					AssertEquals("Precondition:", true, receive.WD_UnloadCompletedTime.IsValid);
					menuItem.PerformClick();
					AssertEquals(false, receive.WD_UnloadCompletedTime.IsValid);
				}
			}
		}

		public void TestClearUnloadCompletedTimeMenu_DisablesMenuItemInstantly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompletedTime have already set", true, receive.WD_UnloadCompletedTime.IsValid);
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Unload Completed Time");
				AssertEquals("Clear Unload Completed Time menu item is enabled.", true, menuItem.Enabled);

				menuItem.PerformClick();
				AssertEquals(false, receive.WD_UnloadCompletedTime.IsValid);
				AssertEquals("Clear Unload Completed Time menu item is disabled.", false, menuItem.Enabled);
			}
		}

		public void TestSetUnloadCompletedTimeMenu_DisablesMenuItemInstantly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompletedTime is not set", false, receive.WD_UnloadCompletedTime.IsValid);
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Set Unload Completed Time");
				AssertEquals("Set Unload Completed Time menu item is enabled.", true, menuItem.Enabled);

				menuItem.PerformClick();
				AssertEquals(true, receive.WD_UnloadCompletedTime.IsValid);
				AssertEquals("Set Unload Completed Time menu item is disabled.", false, menuItem.Enabled);
			}
		}

		public void TestSetUnloadCompletedTimeMenu_PlannedReceiveWithActiveWorkingTasks()
			=> TestSetUnloadCompletedTimeMenu_PlannedReceiveWithActiveWorkingTasksCore(userResponse: true);

		public void TestSetUnloadCompletedTimeMenu_PlannedReceiveWithActiveWorkingTasks_UserCancels()
			=> TestSetUnloadCompletedTimeMenu_PlannedReceiveWithActiveWorkingTasksCore(userResponse: false);

		void TestSetUnloadCompletedTimeMenu_PlannedReceiveWithActiveWorkingTasksCore(bool userResponse)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals("Precondition", TaskPlanningStatus.Codes.Planned, receive.WD_TaskPlanningStatus);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Set Unload Completed Time");
				AssertEquals("Set Unload Completed Time menu item is enabled.", true, menuItem.Enabled);

				var testNotificationBuffer = (TestNotificationBuffer)form.GetNotificationBufferForTest();
				var expectedUserQueryMessage = "This Receipt has active unload job tasks set to working. The tasks need to be completed to complete the unload for this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to complete the unload for this Receipt?";
				testNotificationBuffer.PreQueryUser += (sender, e) =>
				{
					if (e is DefaultableQueryUserEventArgs args && args.Message.Equals(expectedUserQueryMessage))
					{
						args.Response = userResponse;
					}
				};

				menuItem.PerformClick();
				AssertEquals(testNotificationBuffer, receive.NotificationManager.LastPopped);

				var lastUserQuery = testNotificationBuffer.LastQueryUserEventArgs as DefaultableQueryUserEventArgs;
				AssertNotNull(lastUserQuery);
				AssertEquals("Completing unload with active unload tasks set to working confirmation", lastUserQuery.Caption);
				AssertEquals(expectedUserQueryMessage, lastUserQuery.Message);
				AssertEquals(ZMessageBoxButtons.YesNoCancel, lastUserQuery.Context.Buttons);

				AssertEquals(userResponse, receive.WD_UnloadCompletedTime.IsValid);
				AssertEquals(!userResponse, menuItem.Enabled);
			}
		}

		#endregion

		#endregion

		#region ActionMenu Handlers

		#region Allocate Locations Handler

		public void TestOnAllocateLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			var receiveLine = receive.Lines.Single();
			WhsDocumentPrinter.LastPrintedDocumentName = ZString.Empty;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", false, receive.IsPuttingAway);
				AssertEquals("Precondition", true, receive.IsInDatabase);
				var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

				using (ObjectFactory.Substitute(putawayEngineMock.Object))
				{
					form.OnAllocateLocationsForTest(null, EventArgs.Empty);
				}
				AssertEquals(true, receive.IsPuttingAway);
				AssertEquals("Location should be allocated", true, receiveLine.WE_WL.IsValid);
				putawayEngineMock.VerifyAll();
				AssertEquals(form.GetNotificationBufferForTest(), receive.NotificationManager.LastPopped);

				AssertEquals(ZString.Empty, WhsDocumentPrinter.LastPrintedDocumentName);
			}
		}

		public void TestOnAllocateLocations_PromptsToSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			var receiveLine = receive.Lines.Single();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", false, receive.IsPuttingAway);

				form.OnAllocateLocationsForTest(null, EventArgs.Empty);
				AssertEquals("No Locations should be Allocated.", false, receive.IsPuttingAway);
				AssertEquals("No Locations should be Allocated.", ZGuid.Empty, receiveLine.WE_WL);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Receipt before Allocating Locations."));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestOnAllocateLocations_PromptsToUnloadCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			var receiveLine = receive.Lines.Single();
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Locations");

				menuItem.PerformClick();
				AssertEquals("No Locations should be Allocated.", false, receive.IsPuttingAway);
				AssertEquals("No Locations should be Allocated.", ZGuid.Empty, receiveLine.WE_WL);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Can not allocate location(s) when the unload is completed."));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region Clear Locations Handler

		public void TestOnClearLocations()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertEquals("Precondition", true, data.Receive11.IsPuttingAway);
				AssertEquals("Precondition", true, data.Line111.InDocketLine.WE_WL.IsValid);

				form.OnClearLocationsForTest(null, EventArgs.Empty);
				AssertEquals("Putting away status should be cleared", false, data.Receive11.IsPuttingAway);
				AssertEquals("Location should now be cleared", true, data.Line111.InDocketLine.WE_WL.IsEmpty);
				AssertEquals("Should have no error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOnClearLocations_PromptsToUnloadCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: true, finalise: false);
			var receiveLine = receive.Lines.Single();
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			var allocationLocation = receiveLine.WE_WL;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Clear Locations");

				menuItem.PerformClick();
				AssertEquals("Locations should not be Cleared.", allocationLocation, receiveLine.WE_WL);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Can not clear allocated location(s) when the unload is completed."));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region Palletize Lines Handler

		public void TestOnPalletizeLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", 1, data.Receive11.Lines.Count);
				form.OnPalletizeLinesForTest(null, EventArgs.Empty);
				AssertEquals("Should have 5 lines, this proves palletize was executed", 5, data.Receive11.Lines.Count);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region Generate Pallet IDs Handlers

		public void TestOnGenerateSequentialPalletIDs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", "", data.Line111.WI_PalletID);

				data.Receive11.WD_DocketID = "W00000001";
				form.OnGenerateSequentialPalletIDsForTest(null, EventArgs.Empty);
				AssertNotEquals("Should have a Pallet ID, this proves pallet id generation was executed", "", data.Line111.WI_PalletID);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOnGenerateSequentialPalletIDsPromptsToSave()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.OnGenerateSequentialPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Generate should not of executed", "", data.Line111.WI_PalletID);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Receipt before Generating Pallet IDs"));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestOnGenerateIdenticalPalletIDs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", "", data.Line111.WI_PalletID);

				data.Receive11.WD_DocketID = "W00000001";
				form.OnGenerateIdenticalPalletIDsForTest(null, EventArgs.Empty);
				AssertNotEquals("Should have a Pallet ID, this proves pallet id generation was executed", "", data.Line111.WI_PalletID);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOnGenerateIdenticalPalletIDsPromptsToSave()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.OnGenerateIdenticalPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Generate should not of executed", "", data.Line111.WI_PalletID);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Receipt before Generating Pallet IDs"));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region Palletize Line And Generate Pallet IDs Handlers

		public void TestOnPalletizeLinesAndGeneratePalletIDs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 20m);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", 1, data.Receive11.Lines.Count);
				AssertEquals("Precondition", "", data.Receive11.Lines[0].WE_PalletID);

				data.Receive11.WD_DocketID = "W00000001";
				form.OnPalletizeLinesAndGeneratePalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Should have 5 lines, this proves palletize was executed", 5, data.Receive11.Lines.Count);
				AssertNotEquals("Should have a Pallet ID, this proves pallet id generation was executed", "", data.Line111.WI_PalletID);
				var recieveLine1 = data.Receive11.Lines.Single(l => l.WE_PalletID.Equals("W00000001-0001"));
				var recieveLine2 = data.Receive11.Lines.Single(l => l.WE_PalletID.Equals("W00000001-0002"));
				var recieveLine3 = data.Receive11.Lines.Single(l => l.WE_PalletID.Equals("W00000001-0003"));
				var recieveLine4 = data.Receive11.Lines.Single(l => l.WE_PalletID.Equals("W00000001-0004"));
				var recieveLine5 = data.Receive11.Lines.Single(l => l.WE_PalletID.Equals("W00000001-0005"));
				AssertEquals("receiveLine1 should have product of 20m", recieveLine1.WE_TransactionQuantity, 20m);
				AssertEquals("receiveLine2 should have product of 20m", recieveLine2.WE_TransactionQuantity, 20m);
				AssertEquals("receiveLine3 should have product of 20m", recieveLine3.WE_TransactionQuantity, 20m);
				AssertEquals("receiveLine4 should have product of 20m", recieveLine4.WE_TransactionQuantity, 20m);
				AssertEquals("receiveLine5 should have product of 20m", recieveLine5.WE_TransactionQuantity, 20m);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOnPalletizeLinesAndGeneratePalletIDsPromptsToSave()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", 1, data.Receive11.Lines.Count);
				AssertEquals("Precondition", "", data.Receive11.Lines[0].WE_PalletID);

				form.OnPalletizeLinesAndGeneratePalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Palletize should not executed", 1, data.Receive11.Lines.Count);
				AssertEquals("Generate should not executed", "", data.Line111.WI_PalletID);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Receipt before Generating Pallet IDs"));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region Clear Pallet IDs Handler

		public void TestOnClearPalletIDs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				data.Line111.WI_PalletID = "XXX123";

				form.OnClearPalletIDsForTest(null, EventArgs.Empty);
				AssertEquals("Should have cleared Pallet ID, this proves clear pallet was executed", "", data.Line111.WI_PalletID);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region Change Hold Code Handlers

		public void TestOnChangeHoldCodeForProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");

			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
				AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

				form.OnChangeHoldCodeForProductForTest(null, EventArgs.Empty);

				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("ReceiveLine1's Hold code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
				AssertEquals("ReceiveLine2's Hold code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			}
		}

		public void TestOnChangeHoldCodeForProductPromptsToSave()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
				AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

				form.OnChangeHoldCodeForProductForTest(null, EventArgs.Empty);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Receipt before changing Hold Code"));
			}
		}

		public void TestOnChangeHoldCodeForReceipt()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");

			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
				AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

				form.OnChangeHoldCodeForReceiptForTest(null, EventArgs.Empty);

				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("ReceiveLine1's Hold code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
				AssertEquals("ReceiveLine2's Hold code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			}
		}

		public void TestOnChangeHoldCodeForReceiptPromptsToSave()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
				AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

				form.OnChangeHoldCodeForReceiptForTest(null, EventArgs.Empty);
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Receipt before changing Hold Code"));
			}
		}

		#endregion

		#region Clear Hold Codes Handlers

		public void TestOnClearHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
				AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

				form.OnClearHoldCodesForTest(null, EventArgs.Empty);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have cleared Hold Code", receive.Lines.Count, receive.Lines.Count(t => t.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty));
			}
		}

		public void TestClearHoldCodes_DoesNotClearIfFinalisedOrCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");
			receive.FinaliseDocket();

			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
				AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

				form.OnClearHoldCodesForTest(null, EventArgs.Empty);
				AssertEquals("Should not have cleared Hold Code.", "Error: Cannot perform this operation because the job is finalized or canceled.", ((TestNotificationBuffer)receive.NotificationManager.LastPopped).LastEvent.Message);
			}
		}

		#endregion

		#region Generate Serial Numbers Handler

		public void TestOnGenerateSerialNumbers()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[1] { 3m }, false);

			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Receive11.Inventory[0].WI_SerialNumber = "S001";

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.OnGenerateSerialNumbersForTest(null, EventArgs.Empty);
				AssertEquals("Should have 3 lines created, this proves serial number generation was executed", 3, data.Receive11.Inventory.Count);
				AssertInventorySerialNumberAndTotalUnits(data.Receive11.Inventory[0], "S001", 1m);
				AssertInventorySerialNumberAndTotalUnits(data.Receive11.Inventory[1], "S002", 1m);
				AssertInventorySerialNumberAndTotalUnits(data.Receive11.Inventory[2], "S003", 1m);

				AssertEquals("Should have quantity 1, this proves serial number generation was executed", 1m, data.Receive11.Inventory[0].WI_TotalUnits);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		void AssertInventorySerialNumberAndTotalUnits(WhsInventoryView inventory, string expectedSerialNumber, decimal expectedTotalUnits)
		{
			AssertEquals(expectedSerialNumber, inventory.WI_SerialNumber);
			AssertEquals(expectedTotalUnits, inventory.WI_TotalUnits);
		}

		#endregion

		#region Create Product Files

		public void TestOnCreateProductFiles_SecurityCheckPoints_RightNotGranted()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = false;

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.OnCreateProductFilesForTest(null, EventArgs.Empty);
				AssertEquals("No permission to Create Product Files", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestOnCreateProductFiles_SecurityCheckPoints_RightGranted()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = true;

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				data.Line111.WI_OP = ZGuid.Invalid;
				data.Line111.WI_OP_PartNum = "NewProduct";
				data.Line111.WI_OP_Desc = "DESC";
				form.OnCreateProductFilesForTest(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOnCreateProductFiles()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = true;

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.OnCreateProductFilesForTest(null, EventArgs.Empty);
				AssertEquals("No New Products were entered, no New Product Files will be created.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				data.Line111.WI_OP = ZGuid.Invalid;
				data.Line111.WI_OP_PartNum = "NewProduct";
				form.OnCreateProductFilesForTest(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("One or more New Products have errors and could not be created. Please correct the errors and try again.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				data.Line111.WI_OP_Desc = "DESC";
				form.OnCreateProductFilesForTest(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region AutoFill Split Quantities Handler

		public void TestOnAutoFillSplitQuantities()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[2] { 3m, 5m }, false);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.OnAutoFillSplitQuantitiesForTest(null, EventArgs.Empty);
				data.Receive11.Inventory.Sort(WhsInventoryView.Schema.WI_SplitQuantity);

				AssertEquals(3m, data.Receive11.Inventory[0].WI_SplitQuantity);
				AssertEquals(5m, data.Receive11.Inventory[1].WI_SplitQuantity);
			}
		}

		#endregion

		#region Split Receipt by Quantity Handler

		public void TestOnSplitReceiptByQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[2] { 3m, 5m }, false);
			data.Receive11.Inventory[1].WI_SplitQuantity = 5m;
			Factory.Save();
			AssertEquals("Should saved before splitting", false, data.Receive11.HasChanges);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var receipts = new WhsReceiveCollection(Factory);
				AssertEquals("Precondition", 1, receipts.Count);

				form.OnSplitReceiptByQuantityForTest(null, EventArgs.Empty);
				AssertEquals("Receipt should be split", 2, receipts.Count);
				AssertEquals("Split Receipts should have 1 line each", true, receipts[0].Inventory.Count == 1 && receipts[1].Inventory.Count == 1);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOnSplitReceiptByQuantity_SplitNumberExceedsMaximumValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ExternalReferenceSplit = 255;

			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			inventoryLine.WI_SplitQuantity = 3m;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var receipts = new WhsReceiveCollection(Factory);
				AssertEquals("Precondition", 1, receipts.Count);

				AssertNoExceptionThrown("No exception is thrown when splitting.", () => form.OnSplitReceiptByQuantityForTest(null, EventArgs.Empty));
				AssertEquals("Receipt should not be split", 1, receipts.Count);
				AssertEquals("Should have unable to split error.", "Error: The receipt has reached the maximum number of allowable splits.", ((TestNotificationBuffer)receive.NotificationManager.LastPopped).LastEvent.Message);
			}
		}

		#endregion

		#region Split Receipt by Area Type Handler

		public void TestOnSplitReceiptByAreaType()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[2] { 3m, 5m }, false);
			var area = data.Whs1.Areas.AddNew();
			area.WA_Name = "AT2";
			area.WA_AreaType = "AT2";
			data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations[1].WLV_WA_PutawayArea = area.PK;
			Factory.Save();
			AssertEquals("Should saved before splitting", false, data.Receive11.HasChanges);

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var receipts = new WhsReceiveCollection(Factory);
				AssertEquals("Precondition", 1, receipts.Count);

				form.OnSplitReceiptByAreaTypeForTest(null, EventArgs.Empty);

				AssertEquals("Receipt should be split", 2, receipts.Count);
				AssertEquals("Split Receipts should have 1 line each", true, receipts[0].Inventory.Count == 1 && receipts[1].Inventory.Count == 1);
				AssertEquals("Should not have error", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOnSplitReceiptByAreaType_SplitNumberExceedsMaximumValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var area = data.Whs1.Areas.AddNew();
			area.WA_Name = "AT2";
			area.WA_AreaType = "AT2";
			data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations[1].WLV_WA_PutawayArea = area.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ExternalReferenceSplit = 255;

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-3"));
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var receipts = new WhsReceiveCollection(Factory);
				AssertEquals("Precondition", 1, receipts.Count);

				AssertNoExceptionThrown("No exception is thrown when splitting.", () => form.OnSplitReceiptByAreaTypeForTest(null, EventArgs.Empty));
				AssertEquals("Receipt should not be split", 1, receipts.Count);
				AssertEquals("Should have unable to split error.", "Error: The receipt has reached the maximum number of allowable splits.", ((TestNotificationBuffer)receive.NotificationManager.LastPopped).LastEvent.Message);
			}
		}

		public void TestOnSplitReceiptByAreaType_SplitNumberExceedsMaximumValue_SomeGotSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var area2 = Helper.CreateArea(data.Whs1, "B2", AreaTypes.Codes.Bonded);
			var area3 = Helper.CreateArea(data.Whs1, "B3", AreaTypes.Codes.Excise);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WA_PutawayArea = area2.PK;
			locations[2].WLV_WA_PutawayArea = area3.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			using (receive.GetValidationSuspender()) // It's currently not possible to have 3 separate area types on a receive, bypass the Customs/Goods receipt validation to allow this
			{
				receive.WD_ExternalReferenceSplit = 254;

				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"));
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-2"));
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-3"));
				Factory.Save();

				using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
				{
					form.Show();

					var receipts = new WhsReceiveCollection(Factory);
					AssertEquals("Precondition", 1, receipts.Count);

					AssertNoExceptionThrown("No exception is thrown when splitting.", () => form.OnSplitReceiptByAreaTypeForTest(null, EventArgs.Empty));
					AssertEquals("Only 1 new split created.", 2, receipts.Count);

					var expectedErrorMessage = @"Error: The action encountered an error while performing the split: Not all area types have been split into new receipts. The receipt has reached the maximum number of allowable splits.
The following new Receipt has been created as a result of this Split Action



Reference: R1-255




You can open the new Receipt(s) from the Related Splits tab";

					AssertEquals("Should have unable to split error.", expectedErrorMessage, ((TestNotificationBuffer)receive.NotificationManager.LastPopped).LastEvent.Message);
				}
			}
		}

		#endregion

		#region Update Actual Quantity with Expected Quantity Handler

		public void TestOnUpdateActualQuantityWithExpectedQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Factory.Save(); // required to ensure WI_InDocketLineUnits update will not affect WI_ExpectedReceiptQuantity.

			using (var form = new ReceiveEntryForm(data.Receive11, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				data.Line111.WI_InDocketLineUnits = 0m;
				AssertEquals("Precondition - Actual qty should be 0.", 100m, data.Line111.WI_ExpectedReceiptQuantity);
				AssertEquals("Precondition - Expected to Receive qty should be 100.", 100m, data.Line111.WI_ExpectedReceiptQuantity);

				form.OnUpdateActualQuantityWithExpectedQuantityForTest(null, EventArgs.Empty);
				AssertEquals("Should update Actual Quantity with Expected Quantity.", 100m, data.Line111.WI_InDocketLineUnits);
				AssertEquals("Should not have error.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region Cancel Docket Handler

		public void TestOnCancelDocket()
		{
			var receive = Factory.New<WhsReceive>();
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				Env.Security.WhsReceiveCancel.IsAllowed = false;
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);

				AssertEquals("No permission to Cancelation", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Shouldn't have proceeded with Cancelation", false, receive.IsCancelled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.WhsReceiveCancel.IsAllowed = true;
				receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
				var cancellable = (ICancellable)receive;
				string canCancelError = cancellable.CanCancel();
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);

				AssertEquals("No problems with permission", false, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

				AssertEquals("'" + canCancelError + "' error was expected for order with status Finalised", true, UnitTestUserNotification.Instance.LastMessage.Contains(canCancelError));
				AssertEquals("Error was expected for order with status Finalised", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Order with status Finalised cannot be Canceled", false, receive.IsCancelled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				receive.WD_DocketStatus = DocketStatus.Codes.Entered;
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);

				AssertEquals("No error was expected for order with status Entered (Saved)", false, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Orders should be Cancelled", true, receive.IsCancelled);

				receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);
				AssertEquals("After reactivation Receive status should be Entered", DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			}
		}

		public void TestOnReactivateDocket_ReceiveCannotBeCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "AAA", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "R1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Cancel receive; have to do this through row, as cancellation validation rules will prevent receive from being cancelled due to picked putaway transfer
			((IBusinessObjectInternals)receive).Row[WhsDocketSchema.Constants.WD_DocketStatus] = DocketStatus.Codes.Cancelled;
			receive.WD_GS_NKCanceledBy = "~BP";
			receive.WD_CanceledTimeUtc = ZDateTime.UtcNow;
			receive.HasChanges = true;
			receiveLine.InDocketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			Factory.Save();
			AssertEquals("Should be cancelled.", true, receive.IsCancelled);

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				Env.Security.WhsReceiveCancel.IsAllowed = true;
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);
				AssertEquals("After reactivation Receive status should be Entered", DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			}
		}

		#endregion

		#region Create ASN Lines

		public void TestOnCreateAsnLines_WarningYesResponse()
		{
			TestOnCreateAsnLinesCore(DialogResult.Yes);
		}

		public void TestOnCreateAsnLines_WarningNoResponse()
		{
			TestOnCreateAsnLinesCore(DialogResult.No);
		}

		void TestOnCreateAsnLinesCore(DialogResult dialogResult)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: Receive has no ASN lines.", false, receive.AsnLines.Count > 0);
				AssertEquals("Precondition: PopulateASNLines is not called.", false, receive.PopulateASNHasBeenCalled_TestsOnly);

				UnitTestUserNotification.Instance.AddAnswer(dialogResult);
				var startReceivingActionMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Start Receiving (Create ASN Lines)");
				startReceivingActionMenuItem.PerformClick();

				var expectedWarningMessage = @"Once the ASN lines are created, it will not be possible to add/update/delete the ASN lines after this.

Do you wish to continue?";
				AssertEquals("Should ask user whether to proceed or not with the Create ASN Lines action.",
							expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Message type is None.", UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert("Response is defaultable.", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
				AssertEquals("PopulateASNLinesHasBeenCalled", dialogResult == DialogResult.Yes, receive.PopulateASNHasBeenCalled_TestsOnly);
			}
		}

		public void TestOnCreateAsnLines_ReceiveHasAsnLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			Factory.Save();

			receive.PopulateASNLines();
			Factory.Save();

			Assert("Precondition: PopulateASNLines has been called.", receive.PopulateASNHasBeenCalled_TestsOnly);
			receive.PopulateASNHasBeenCalled_TestsOnly = false; // reset for test purposes

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				Assert("Precondition: Receive has ASN lines.", receive.AsnLines.Count > 0);

				var startReceivingActionMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Start Receiving (Create ASN Lines)");
				startReceivingActionMenuItem.PerformClick();

				AssertEquals("Should have error", "Cannot Overwrite existing ASN Lines. Receive has already started.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should have error", UnitTestUserNotification.Instance.LastMessage.WasError);
				Assert("PopulateASNLines is not called.", !receive.PopulateASNHasBeenCalled_TestsOnly);
			}
		}

		public void TestOnCreateAsnLines_ReceiveHasAsnLines_SerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, allocateLocations: false, finalise: false);
				Helper.CreateWhsReceiveLine(receive, data.Part2, 1m);

				var sn1PK = AddSerialNumber(receive.Lines[0].SerialNumbers, "SN1");
				var sn2PK = AddSerialNumber(receive.Lines[0].SerialNumbers, "SN2");
				var sn3PK = AddSerialNumber(receive.Lines[1].SerialNumbers, "SN3");

				Factory.Save();
				Assert("Precondition", receive.AsnLines.Cast<WhsAsnLine>().All(l => l.SerialNumbers.Count == 0));

				using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
				{
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var startReceivingActionMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Start Receiving (Create ASN Lines)");
					startReceivingActionMenuItem.PerformClick();

					var asnLines = receive.AsnLines.Cast<WhsAsnLine>();
					AssertContainsExactElementsInAnyOrder(["SN1", "SN2"], asnLines.Single(l => l.WN_OP.Equals(data.Part1.PK)).SerialNumbers.Select(l => l.SerialNumberValue));
					AssertContainsExactElementsInAnyOrder(["SN3"], asnLines.Single(l => l.WN_OP.Equals(data.Part2.PK)).SerialNumbers.Select(l => l.SerialNumberValue));
				}
			}

			ZGuid AddSerialNumber(WhsSerialNumberPivotCollection serialNumbers, string serialNumber)
			{
				var pivot = serialNumbers.AddNew();
				pivot.SerialNumberValue = serialNumber;
				return pivot.WSV_WSN_SerialNumber;
			}
		}

		#endregion

		#region TestOnChangeTaskPlanningStatus

		public void TestOnChangeTaskPlanningStatus_StatusIsEmpty()
		{
			TestOnChangeTaskPlanningStatusCore(string.Empty, TaskPlanningStatus.Codes.Ready, "Change Task Planning Status To Ready", "Change Task Planning Status To Not Ready");
		}

		public void TestOnChangeTaskPlanningStatus_StatusIsNotReady()
		{
			TestOnChangeTaskPlanningStatusCore(TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.Ready, "Change Task Planning Status To Ready", "Change Task Planning Status To Not Ready");
		}

		public void TestOnChangeTaskPlanningStatus_StatusIsReady()
		{
			TestOnChangeTaskPlanningStatusCore(TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.NotReady, "Change Task Planning Status To Not Ready", "Change Task Planning Status To Ready");
		}

		public void TestOnChangeTaskPlanningStatus_StatusIsPlanned()
		{
			TestOnChangeTaskPlanningStatusCore(TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.NotReady, "Change Task Planning Status To Not Ready", "Change Task Planning Status To Ready");
		}

		void TestOnChangeTaskPlanningStatusCore(string status, string expectedStatus, string expectedMenuTextBefore, string expectedMenuTextAfter)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			receive.WD_TaskPlanningStatus = status;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(expectedMenuTextBefore).Visible);

				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(expectedStatus, receive.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(expectedMenuTextAfter).Visible);
				});
			}
		}

		public void TestOnChangeTaskPlanningStatus_ChangeToReadyAndThenChangeAgain()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_TaskPlanningStatus = string.Empty;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);

				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.Ready, receive.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Not Ready").Visible);
				});
				form.FireSaveButton();

				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);
				});
			}
		}

		public void TestOnChangeTaskPlanningStatus_ChangeToNotReadyAndThenChangeAgain()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Not Ready").Visible);

				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Ready").Visible);
				});
				form.FireSaveButton();

				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.Ready, receive.WD_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change Task Planning Status To Not Ready").Visible);
				});
			}
		}

		public void TestOnChangeTaskPlanningStatus_NotCreated()
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			receive.WD_WW_Whs = whs.PK;

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Cannot change Task Planning Status as the Warehouse Receipt is not saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnChangeTaskPlanningStatus_FormHasChanges()
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			receive.WD_WW_Whs = whs.PK;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Cannot change Task Planning Status as the Warehouse Receipt is not saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnChangeTaskPlanningStatus_Finalised()
		{ 
			var whs = Helper.CreateWarehouse("WH1", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			receive.WD_WW_Whs = whs.PK;
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Cannot change Task Planning Status as the Warehouse Receipt is finalized or canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnChangeTaskPlanningStatus_Cancelled()
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			var org = Helper.CreateClient("CLI");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			Factory.Save();

			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			receive.WD_WW_Whs = whs.PK;
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.OnChangeTaskPlanningStatusForTest(null, EventArgs.Empty);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Cannot change Task Planning Status as the Warehouse Receipt is finalized or canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#endregion

		#region INotifications Members

		public void TestNotify()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				var type = new TestINotificationType("Message", "NotZErrorMessageBox");
				var e = new TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				((INotifications)form).Add(e);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("TestMessageToDisplay"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				type = new TestINotificationType("Putaway", "ZErrorMessageBox");
				e = new TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				((INotifications)form).Add(e);
				AssertEquals("There are errors that need to be corrected before this Receipt can be Putaway.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);

				type = new TestINotificationType("Finalise", "ZErrorMessageBox");
				e = new TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				((INotifications)form).Add(e);
				AssertEquals("There are errors that need to be corrected before this Warehouse Receipt can be Finalized.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			}
		}

		public void TestQueryUser()
		{
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
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
			var whs = Helper.CreateWarehouse(docketID, "A");
			var org = Helper.CreateClient(docketID);
			var part = Helper.CreateProduct(org, docketID);
			var docket = Helper.CreateWhsReceive(org, whs, docketID);
			var inventory = Helper.CreateWhsReceiveInventoryLine(docket, part, 10);

			inventory.WI_WL = whs.Rows.Single(r => r.WR_Name == "A").Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1).PK;

			docket.WD_DocketID = "";

			using (var form = new ReceiveEntryForm(docket, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", false, docket.IsInDatabase);
				form.FireSaveButton();
			}

			AssertEquals("Precondition", true, docket.IsInDatabase);
			AssertEquals(docketID, docket.WD_DocketID);
		}

		#endregion

		#region TestDeniedPartyScreeningLogs

		public void TestDeniedPartyScreeningLogs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			Factory.Save();

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var logsTabPage = form.FindSingle<ZTabPage>("LogsTabPage");
				var tabControl = (ZTabControl)logsTabPage.Controls[0].Controls[0];
				var screeningLogsTab = tabControl.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Text == "Denied Party Screening Logs");
				AssertNotNull(screeningLogsTab);

				var screeningLogControl = screeningLogsTab.Controls[0] as StmEntityScreeningLogControl;
				AssertNotNull(screeningLogControl);
				AssertNotNull(screeningLogControl);
				AssertEquals(nameof(WhsReceive.RelatedOrgPartyScreeningStatusCollection), screeningLogControl.GetBindingMember());
			}
		}

		#endregion

		#region Test Mark as Job Clear

		public void TestMarkAsJobClearMenuItemExist()
		{
			AssertMarkAsJobClearMenuItemExist(true);
			AssertMarkAsJobClearMenuItemExist(false);
		}

		public void TestMarkAsJobClearWhenSecurityRightsIsDenied_ShouldShowErrorMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ReceiveEntryForm(Factory.NewWithValidTestData<WhsReceive>(), new NotificationSubscriberGuiHelper()))
			{
				Factory.Save();
				form.Show();

				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;
				new DpsMarkJobScreeningStatusClearTest().AssertSecurityRightsAccessibilityCheckpoint(form, tmpSecurityCore);
			}
		}

		public void TestMarkAsJobClearWhenScreeningStatusIsJCLorCLR_ShouldShowWarningMessage()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			(receive as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = false;

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				AssertEquals("Precondition receive screening status", "JCL", receive.WD_ScreeningStatus);
				Factory.Save();

				var markJobClearTestHelper = new DpsMarkJobScreeningStatusClearTest();
				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);

				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Precondition receive screening status", "CLR", receive.WD_ScreeningStatus);
				Factory.Save();

				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);
			}
		}

		public void TestMarkAsJobClearWhenJobIsNotSaved_ShouldShowWarningMessage()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				AssertEquals("Precondition receive screening status", "NOT", receive.WD_ScreeningStatus);

				new DpsMarkJobScreeningStatusClearTest().AssertSaveBeforeMarkingClear(form);
			}
		}

		public void TestMarkAsJobClearSetScreeningStatusToJCL()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Precondition receive screening status", "MAT", receive.WD_ScreeningStatus);
				Factory.Save();

				new DpsMarkJobScreeningStatusClearTest().AssertScreeenigStatusToJCL(form, receive);
			}
		}

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		void AssertMarkAsJobClearMenuItemExist(bool registryValue)
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				new DpsMarkJobScreeningStatusClearTest().AssertMenuItemAccessibilityCheckpoint(form, registryValue);
			}
		}

		#endregion Test Mark as Job Clear

		#region PlugIns

		#region eConversations

		public void TestEConversationsPlugIn_Visible()
		{
			AssertEConversationPlugInVisibility(true);
		}

		public void TestEConversationsPlugIn_Hidden()
		{
			AssertEConversationPlugInVisibility(false);
		}

		void AssertEConversationPlugInVisibility(bool registryValue)
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new ReceiveEntryForm(Factory.New<WhsReceive>(), new NotificationSubscriberGuiHelper()))
			{
				var eConverationPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);

				if (registryValue)
				{
					AssertNotNull(eConverationPlugIn);
				}
				else
				{
					AssertNull(eConverationPlugIn);
				}
			}
		}

		#endregion

		#region JobInvoicing

		public void TestJobInvoicingPlugIn_Visibility()
		{
			using (var form = new ReceiveEntryForm(Factory.New<WhsReceive>(), new NotificationSubscriberGuiHelper()))
			{
				var jobInvoicingPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertNotNull(jobInvoicingPlugIn);
			}
		}

		public void TestJobInvoicingPlugIn_Visibility_HiddenWhenItIsPickByBOMReceive()
		{
			var receive = Factory.New<WhsReceive>();
			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				var jobInvoicingPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertNull(jobInvoicingPlugIn);
			}
		}

		#endregion

		#endregion
	}

	[CodeAlive("This test class is being used in TestDataButton")]
	sealed class ReceiveEntryFormForTest : ReceiveEntryForm
	{
		public ReceiveEntryFormForTest(WhsReceive docket, NotificationSubscriberGuiHelper whsNotificationSubscriberGuiHelper) : base(docket, whsNotificationSubscriberGuiHelper)
		{
		}

		internal override Action<WhsReceive> OnTestDataButtonClickInternal => PopulateTestData;

		static void PopulateTestData(WhsReceive docket)
		{
			if (docket.Inventory.Count > 0 && !docket.IsFinalisedOrCancelled)
			{
				var helper = new WhsTestHelperFunctions(docket.Factory);
				var part = docket.Inventory[docket.Inventory.Count - 1].SupplierPart;
				for (int i = 0; i < 50; i++)
				{
					var rand = new Random();
					var line = helper.CreateWhsReceiveInventoryLine(docket, part, rand.Next(1, 100));
					line.WI_PartAttrib1 = "A1";
					line.WI_PartAttrib2 = "A2";
					line.WI_PartAttrib3 = "A3";
					line.WI_ExpiryDate = ZDate.Today.AddMonths(1);
					line.WI_PackingDate = ZDate.Today.AddMonths(1);
					line.WI_OP = ZGuid.Empty;
					line.WI_OP = part.PK; // reset line details
				}
			}
		}
	}
}
