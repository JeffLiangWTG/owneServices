using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestMenuItemsVisibilityInArrivalOrExportationMessagingMenuItem()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var arrivalItem = testMenu.MenuItems.FindByText("Send Arri&val");
				var arrivalInbondMessagingMenuItem = arrivalItem.MenuItems.FindByText("Send Arri&val (Entire In-Bond)");
				var arrivalBillMessagingMenuItem = arrivalItem.MenuItems.FindByText("Send Arri&val (Bill of Lading)");
				var arrivalContainerMessagingMenuItem = arrivalItem.MenuItems.FindByText("Send Arri&val (Container/Equipment)");
				var exportationItem = testMenu.MenuItems.FindByText("Send E&xportation");
				var exportationInbondMessagingMenuItem = exportationItem.MenuItems.FindByText("Send E&xportation (Entire In-Bond)");
				var exportationBillMessagingMenuItem = exportationItem.MenuItems.FindByText("Send E&xportation (Bill of Lading)");
				var exportationContainerMessagingMenuItem = exportationItem.MenuItems.FindByText("Send E&xportation (Container/Equipment)");
				AssertEquals(true, arrivalInbondMessagingMenuItem.Visible);
				AssertEquals(true, arrivalBillMessagingMenuItem.Visible);
				AssertEquals(true, arrivalContainerMessagingMenuItem.Visible);
				AssertEquals(true, exportationInbondMessagingMenuItem.Visible);
				AssertEquals(true, exportationBillMessagingMenuItem.Visible);
				AssertEquals(true, exportationContainerMessagingMenuItem.Visible);
				header.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
				testMenu.OnPopup(EventArgs.Empty);
				AssertEquals(true, arrivalInbondMessagingMenuItem.Visible);
				AssertEquals(true, arrivalBillMessagingMenuItem.Visible);
				AssertEquals(false, arrivalContainerMessagingMenuItem.Visible);
				AssertEquals(true, exportationInbondMessagingMenuItem.Visible);
				AssertEquals(true, exportationBillMessagingMenuItem.Visible);
				AssertEquals(false, exportationContainerMessagingMenuItem.Visible);
			}
		}

		public void TestNoExceptionInBondClickOnDeclaration()
		{
			var declaration = Factory.New<US.Business.JobDeclaration>();
			using (var plugin = new InBondPlugIn(declaration))
			{
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				Env.Security.USInBondNew.IsAllowed = true;
				plugin.Enabled = true;
				EDIMenu menu = (EDIMenu)plugin.TopLevelMenu;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("No DeveloperNotificationException", "", ErrorReporter.LastMessageReported);
			}
		}

		public void TestMenuItemsVisibilityInPostDepartureMessagesOnlyMode()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var addItem = testMenu.MenuItems.FindByText("Send Departure &Add");
				var deleteItem = testMenu.MenuItems.FindByText("Send Departure &Delete");
				var amendItem = testMenu.MenuItems.FindByText("Send Departure &Amend (Delete/Re-Add)");
				var deleteInBondItem = deleteItem.MenuItems.FindByText("Send Departure Delete (Entire In-Bond)");
				var deleteBillItem = deleteItem.MenuItems.FindByText("Send Departure Delete (Bill of Lading)");
				AssertEquals(true, addItem.Enabled);
				AssertEquals(true, deleteItem.Enabled);
				AssertEquals(true, amendItem.Enabled);
				AssertEquals(true, deleteInBondItem.Enabled);
				AssertEquals(true, deleteBillItem.Enabled);
				header.BH_PostDepartureOnly = true;
				testMenu.OnPopup(EventArgs.Empty);
				AssertEquals(false, addItem.Enabled);
				AssertEquals(false, deleteItem.Enabled);
				AssertEquals(false, amendItem.Enabled);
				AssertEquals(true, deleteInBondItem.Enabled);
				AssertEquals(true, deleteBillItem.Enabled);
			}
		}

		public void TestSendAddClick()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				MenuItem item = testMenu.MenuItems.FindByText("Send Departure &Add");
				AssertNotNull(item);
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
				moveHeader.BM_MonetaryValue = 100m;
				AssertEquals("PreCondition: Has Changes", true, header.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(MessagingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
				item.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestBondedWarehouseMenuItemSecurity()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var declaration = helper.GetNewDeclaration(US.Business.EntryTypeList.Codes.Warehouse, "B0323565", "XJ5", "ENS32423", 100m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
				var importer = helper.Importer;
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENS32423";
				var moveHeader1 = header.MovementHeaders.AddNew();
				moveHeader1.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill.PK);
				var container1 = moveDetail1.Containers.AddNew();
				container1.BC_ContainerNum = "NC";
				var commodity1 = container1.Commodities.AddNew();
				commodity1.BY_PartNumber = helper.Part.OP_PartNum;
				commodity1.BY_WarehouseEntryNumber = "XJ5-ENS32423";
				commodity1.BY_WarehouseEntryLineNo = 1;
				commodity1.BY_InvoiceQuantity = 60m;
				var moveHeader2 = header.MovementHeaders.AddNew();
				var moveDetail2 = moveHeader2.MovementDetails.AddNew(bill.PK);
				var container2 = moveDetail2.Containers.AddNew();
				container2.BC_ContainerNum = "NC";
				var commodity2 = container2.Commodities.AddNew();
				commodity2.BY_PartNumber = helper.Part.OP_PartNum;
				commodity2.BY_WarehouseEntryNumber = "XJ5-ENS32423";
				commodity2.BY_WarehouseEntryLineNo = 1;
				commodity2.BY_InvoiceQuantity = 60m;
				Factory.Save();
				using (var menu = new EDIMenu())
				using (var form = new ZForm(header))
				{
					menu.Header = header;
					form.Menu.MenuItems.Add(menu);
					form.Show();
					Env.Security.CustomsBondedWhsUpdate.IsAllowed = false;
					Env.Security.CustomsBondedWhsCancel.IsAllowed = false;
					Env.Security.CustomsBondedWhsDisable.IsAllowed = false;
					menu.OnPopup(EventArgs.Empty);
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("&Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("&Cancel Inventory Stock Release");
					AssertEquals(false, cancelBondedWarehouseMenuItem.Visible);
					var disableBondedWarehouseIntegrationMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Disable &Integration");
					AssertEquals(true, disableBondedWarehouseIntegrationMenuItem.Visible);
					disableBondedWarehouseIntegrationMenuItem.PerformClick();
					AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", ZString.Empty, moveHeader1.BM_WarehouseTransactionStatus);
					AssertEquals("moveHeader2.BM_WarehouseTransactionStatus", ZString.Empty, moveHeader2.BM_WarehouseTransactionStatus);
					AssertContains("No permission to Disable", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
					Env.Security.CustomsBondedWhsDisable.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					disableBondedWarehouseIntegrationMenuItem.PerformClick();
					AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, moveHeader1.BM_WarehouseTransactionStatus);
					AssertEquals("moveHeader2.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, moveHeader2.BM_WarehouseTransactionStatus);
					AssertEquals("Inventory Management Integration has been disabled.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
					moveHeader2.Delete();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, moveHeader1.BM_WarehouseTransactionStatus);
					AssertContains("No permission to Update", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
					Env.Security.CustomsBondedWhsUpdate.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, moveHeader1.BM_WarehouseTransactionStatus);
					AssertNotContains("Has permission to Update", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 40m);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, moveHeader1.BM_WarehouseTransactionStatus);
					AssertContains("No permission to Cancel", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 40m);
					Env.Security.CustomsBondedWhsCancel.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader1.BM_WarehouseTransactionStatus);
					AssertNotContains("Has permission to Cancel", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
				}
			}
		}

		public void TestSendDeleteClick()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				MenuItem item = testMenu.MenuItems.FindByText("Send Departure Delete");
				AssertNotNull(item);
				var deleteItem = item.MenuItems.FindByText("Send Departure Delete (Entire In-Bond)");
				AssertNotNull(deleteItem);
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
				moveHeader.BM_MonetaryValue = 100m;
				AssertEquals("PreCondition: Has Changes", true, header.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				deleteItem.PerformClick();
				AssertEquals("There is no In-Bond Movement available for sending a 'Departure Delete' message to Customs.\r\nPossibly all In-Bond Movements are awaiting Customs response, or they have been already accepted by Customs, or they have been created in a legacy system, but they have not been allocated the original In-Bond Number.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				deleteItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(MessagingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
				deleteItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendDeleteBillClick()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				MenuItem item = testMenu.MenuItems.FindByText("Send Departure Delete");
				AssertNotNull(item);
				var deleteItem = item.MenuItems.FindByText("Send Departure Delete (Bill of Lading)");
				AssertNotNull(deleteItem);
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
				moveHeader.BM_MonetaryValue = 100m;
				AssertEquals("PreCondition: Has Changes", true, header.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				deleteItem.PerformClick();
				AssertEquals("There is no Bill of Lading available for sending a 'Departure Delete' message to Customs.\r\nPossibly all Bill are awaiting Customs response, or they have been already accepted by Customs, or they have been created in a legacy system, but they have not been allocated the original In-Bond Number.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				bill.B0_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				bill.LogManager.AddAClearLogIfNecessary(ZString.Empty, ImportMessageStatusList.Codes.ClearDepartureOriginal);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				deleteItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(MessagingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
				deleteItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestDocumentOnlyMode()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			using (var menu = new EDIMenu())
			using (var form = new ZForm(header))
			{
				menu.Header = header;
				form.Menu.MenuItems.Add(menu);
				menu.OnPopup(null);
				var item = menu.MenuItems.FindByText("Send Departure &Add");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("Messages can not be sent in Document Only mode.", UnitTestUserNotification.Instance.LastMessage.Text);
				header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
				item.PerformClick();
				AssertNotEquals("Messages can not be sent in Document Only mode.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2021, 08, 16)]
		public void TestNoDataToSendWhenHeaderIsLocked()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB12345678";
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB16082101";
			bill.B0_HouseBillNumber = "HB16082101";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			Factory.Save();
			using (var menu = new EDIMenu())
			using (var form = new ZForm(header))
			{
				header.LockSendCustomsMessageMutex();
				Assert("Mutex is locked", header.IsSendCustomsMessageMutexLocked);
				menu.Header = header;
				form.Menu.MenuItems.Add(menu);
				menu.OnPopup(null);
				var item = menu.MenuItems.FindByText("Send Departure &Add");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertContains("is in the process of sending messages for INB12345678, please try again later.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				header.UnlockSendCustomsMessageMutex();
				Assert("Mutex is unlocked", !header.IsSendCustomsMessageMutexLocked);
				item.PerformClick();
				AssertEquals(typeof(MessagingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestMutexIsReleaseOnDisposal()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader2.LockInBondNumberAllocationMutex());
			var moveHeader3 = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader3.LockInBondNumberAllocationMutex());
			using (var testMenu = new EDIMenu())
			using (var form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
			}

			AssertEquals(false, moveHeader1.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader2.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader3.InBondNumberAllocationMutexHasLock());
		}

		public void TestShowNoDataToSend()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = "40";
			Factory.Save();
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				testMenu.OnPopup(null);
				MenuItem item = testMenu.MenuItems.FindByText("Send Departure &Add");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no In-Bond Movement available for sending a 'Departure Add' message to Customs.\r\nPossibly all In-Bond Movements are either pending Customs response or have already been accepted by Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				item = testMenu.MenuItems.FindByText("Send Departure Delete");
				var deleteItem = item.MenuItems.FindByText("Send Departure Delete (Entire In-Bond)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteItem.PerformClick();
				AssertEquals("There is no In-Bond Movement available for sending a 'Departure Delete' message to Customs.\r\nPossibly all In-Bond Movements are awaiting Customs response, or they have been already accepted by Customs, or they have been created in a legacy system, but they have not been allocated the original In-Bond Number.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				deleteItem = item.MenuItems.FindByText("Send Departure Delete (Bill of Lading)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteItem.PerformClick();
				AssertEquals("There is no Bill of Lading available for sending a 'Departure Delete' message to Customs.\r\nPossibly all Bill are awaiting Customs response, or they have been already accepted by Customs, or they have been created in a legacy system, but they have not been allocated the original In-Bond Number.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				item = testMenu.MenuItems.FindByText("Send Departure &Amend (Delete/Re-Add)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("Departure Amend (Delete/Re-add) message(s) cannot be sent to Customs.\r\nAt least one In-Bond should have a cleared status.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				var arrivalItem = testMenu.MenuItems.FindByText("Send Arri&val");
				item = arrivalItem.MenuItems.FindByText("Send Arri&val (Entire In-Bond)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no In-Bond Movement available for sending a 'Arrival' message to Customs.\r\nPossibly all In-Bond Movements are pending Customs response.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				item = arrivalItem.MenuItems.FindByText("Send Arri&val (Bill of Lading)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no Bill of Lading available for sending a 'Arrival' message to Customs.\r\nPossibly all Bill of Ladings are pending Customs response or multiple In-bonds exist against the Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				item = arrivalItem.MenuItems.FindByText("Send Arri&val (Container/Equipment)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no Container available for sending a 'Arrival' message to Customs.\r\nPossibly all Containers are either pending Customs response or Number being 'NC' or multiple In-bonds exist against the Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				var exportationItem = testMenu.MenuItems.FindByText("Send E&xportation");
				item = exportationItem.MenuItems.FindByText("Send E&xportation (Entire In-Bond)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no In-Bond Movement available for sending a 'Exportation' message to Customs.\r\nPossibly all In-Bond Movements are either pending Customs response or do not have Entry Type '62' or '63'.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				item = exportationItem.MenuItems.FindByText("Send E&xportation (Bill of Lading)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no Bill of Lading available for sending a 'Exportation' message to Customs.\r\nPossibly all Bill of Ladings are pending Customs response or multiple In-bonds exist against the Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				item = exportationItem.MenuItems.FindByText("Send E&xportation (Container/Equipment)");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no Container available for sending a 'Exportation' message to Customs.\r\nPossibly all Containers are either pending Customs response or Number being 'NC' or multiple In-bonds exist against the Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
				item = testMenu.MenuItems.FindByText("Send &Diversion Request");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("There is no In-Bond Movement available for sending a 'Diversion request' message to Customs.\r\nAt least one In-Bond Movement have already been accepted by Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestMovementHeaderResetToOriginal()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.Mail;
			Factory.Save();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.Auto;
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				testMenu.OnPopup(null);
				MenuItem item = testMenu.MenuItems.FindByText("Reset to Original");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				item.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				AssertEquals("There are no Movement Headers that need to be reset to original. No movements have been lodged at Customs or are waiting for responses.", UnitTestUserNotification.Instance.LastMessage.Text);
				var moveHeader = header.MovementHeaders.AddNew();
				header.MovementHeader.AllocateInBondNumber("dsfsd");
				Factory.Save();
				moveHeader.GeneratePendingOriginalForAmendment(InBondMessageType.DepartureAmend);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendACEInBondQueryClick()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			using (var testMenu = new EDIMenu())
			using (var form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var item = testMenu.MenuItems.FindByText("Query Cargo/Manifest Status");
				AssertNotNull(item);
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
				moveHeader.BM_MonetaryValue = 100m;
				AssertEquals("PreCondition: Has Changes", true, header.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				item.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownForTest);
			}

			header = Factory.New<CusInBondHeader>();
			header.Factory.Save();
			using (var testMenu = new EDIMenu())
			using (var form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var item = testMenu.MenuItems.FindByText("Query Cargo/Manifest Status");
				AssertNotNull(item);
				item.PerformClick();
				AssertEquals("There is no In-Bond Movement/Bills available for sending ACE Cargo/Manifest query message to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoExceptionWhenInbonNotSavedAndSendMessage()
		{
			Env.Security.USInBondMessaging.IsAllowed = true;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			using (EDIMenu testMenu = new EDIMenu())
			using (ZForm form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				testMenu.OnPopup(null);
				MenuItem item = testMenu.MenuItems.FindByText("Send Departure &Add");
				AssertNoExceptionThrown(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					item.PerformClick();
					AssertEquals(false, header.IsInDatabase);
					AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				});
				AssertNoExceptionThrown(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					item.PerformClick();
					AssertEquals(true, header.IsInDatabase);
					AssertEquals("There is no In-Bond Movement available for sending a 'Departure Add' message to Customs.\r\nPossibly all In-Bond Movements are either pending Customs response or have already been accepted by Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}
}
