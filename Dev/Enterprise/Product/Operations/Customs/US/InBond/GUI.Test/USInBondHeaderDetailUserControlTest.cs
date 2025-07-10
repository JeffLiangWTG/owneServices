using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class USInBondHeaderDetailUserControlTest : TestCaseWithFactory
	{
		[TestDate(2015, 1, 1)]
		public void TestCannotAllocateWhenSomeoneElseIsAllocating()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var header = factory1.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var moveHeaderInFactory2 = factory2.Load<CusInBondMoveHeader>(moveHeader.PK);
			AssertEquals(true, moveHeaderInFactory2.LockInBondNumberAllocationMutex());
			var lockInfo = moveHeaderInFactory2.GetInBondNumberAllocationMutexLockInfo();
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				ZPanel mainPanel = (ZPanel)form.Controls["MainPanel"];
				ZTemplateTabControl mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				ZTabPage mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				USInBondHeaderDetailUserControl inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals(true, inBondHeaderDetailsUserControl.Visible);
				ZGroupBox movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				ZGroupBox headerDetailsGroupBox = (ZGroupBox)movementHeadersGroupBox.Controls["HeaderDetailsGroupBox"];
				ZTabControl headerTabControl = (ZTabControl)headerDetailsGroupBox.Controls["HeaderTabControl"];
				ZTabPage departureTabPage = (ZTabPage)headerTabControl.Controls["DepartureTabPage"];
				ZButton inBondNumberAllocationButton = (ZButton)departureTabPage.Controls["InBondNumberAllocationButton"];
				ZGrid movementHeadersGrid = (ZGrid)movementHeadersGroupBox.Controls["MovementHeadersGrid"];
				AssertEquals(false, inBondNumberAllocationButton.ReadOnly);
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberAllocationButton.PerformClick();
				AssertEquals("Allocate In-Bond Number", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(CusInBondMoveHeader.InBondNumberAllocationMutexLockText(lockInfo), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("", moveHeader.InBondNumber);
			}

			moveHeaderInFactory2.UnLockInBondNumberAllocationMutex();
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			var foreignDest = Factory.New<RefUNLOCO>();
			foreignDest.RL_Code = "!ZZ22";
			foreignDest.RL_PortName = "Crystal Lawns";
			var locoMapping = Factory.New<RefLocoMap>();
			locoMapping.RY_LocalPortCode = "!ZZ11";
			locoMapping.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping.RY_IsSystem = false;
			var locoMapping2 = Factory.New<RefLocoMap>();
			locoMapping2.RY_LocalPortCode = "!ZZFF";
			locoMapping2.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping2.RY_IsSystem = false;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZ11", "!ZZ11 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZFF", "!ZZFF Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var movementHeadersGrid = form.FindSingle<ZGrid>("MovementHeadersGrid");
				var foreignDestPortKCodeColumn = movementHeadersGrid.GetColumnStyle("BM_ForeignDestPortKCode");
				AssertType<ZMultiControlColumnStyleInfo>(foreignDestPortKCodeColumn);
				AssertEquals("BM_ForeignDestPortKCodeType", ((ZMultiControlColumnStyleInfo)foreignDestPortKCodeColumn).FieldTypeColumnName);

				AssertEquals(false, form.FindSingle<ZDropEdit>("BM_ForeignDestPortKCodeCodeDropEdit").Visible);
				AssertEquals(true, form.FindSingle<ZCodeFindBox>("BM_ForeignDestPortKCodeCodeFindBox").Visible);

				moveHeader.BM_RL_NKForeignDestPort = "!ZZ22";
				AssertEquals(true, form.FindSingle<ZDropEdit>("BM_ForeignDestPortKCodeCodeDropEdit").Visible);
				AssertEquals(false, form.FindSingle<ZCodeFindBox>("BM_ForeignDestPortKCodeCodeFindBox").Visible);
			}
		}

		public void TestBondedWarehouseMenuItemSecurity()
		{
			var helper = new WhsDataTestHelper(Factory);
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

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZ11", "!ZZ11 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZFF", "!ZZFF Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			using (var form = new USInBondForm(header))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				var mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				var inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals(true, inBondHeaderDetailsUserControl.Visible);
				var movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				var movementHeadersGrid = (ZGrid)movementHeadersGroupBox.Controls["MovementHeadersGrid"];
				movementHeadersGrid.SelectSingleElement(moveHeader2);
				var disableBondedWarehouseIntegrationMenuItem = movementHeadersGrid.ContextMenu.MenuItems.FindByText("&Disable Inventory Management Integration");
				AssertEquals("disableBondedWarehouseIntegrationMenuItem.Visible", true, disableBondedWarehouseIntegrationMenuItem.Visible);
				Env.Security.CustomsBondedWhsDisable.IsAllowed = false;
				disableBondedWarehouseIntegrationMenuItem.PerformClick();
				AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", ZString.Empty, moveHeader1.BM_WarehouseTransactionStatus);
				AssertEquals("moveHeader2.BM_WarehouseTransactionStatus", ZString.Empty, moveHeader2.BM_WarehouseTransactionStatus);
				AssertContains("No permission to Disable", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.CustomsBondedWhsDisable.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				disableBondedWarehouseIntegrationMenuItem.PerformClick();
				AssertEquals("moveHeader1.BM_WarehouseTransactionStatus", ZString.Empty, moveHeader1.BM_WarehouseTransactionStatus);
				AssertEquals("moveHeader2.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, moveHeader2.BM_WarehouseTransactionStatus);
				AssertEquals("Inventory Management Integration has been disabled for movement.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestInBondClosedDate_Click()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var header = factory1.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.BM_InBondCarrierID = "61-059874300";
			factory1.Save();
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				ZPanel mainPanel = (ZPanel)form.Controls["MainPanel"];
				ZTemplateTabControl mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				ZTabPage mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				USInBondHeaderDetailUserControl inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals(true, inBondHeaderDetailsUserControl.Visible);
				ZGroupBox movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				ZGroupBox headerDetailsGroupBox = (ZGroupBox)movementHeadersGroupBox.Controls["HeaderDetailsGroupBox"];
				ZTabControl headerTabControl = (ZTabControl)headerDetailsGroupBox.Controls["HeaderTabControl"];
				ZTabPage departureTabPage = (ZTabPage)headerTabControl.Controls["DepartureTabPage"];
				ZGrid movementHeadersGrid = (ZGrid)movementHeadersGroupBox.Controls["MovementHeadersGrid"];
				movementHeadersGrid.SelectSingleElement(moveHeader1);
				movementHeadersGrid.ContextMenu.MenuItems.FindByText("Mark as Closed").PerformClick();
				AssertNotNullOrEmpty(moveHeader1.BM_InBondClosedDate.ToString());
				Assert(moveHeader1.HasCloseInBondLog());
				movementHeadersGrid.SelectSingleElement(moveHeader2);
				movementHeadersGrid.ContextMenu.MenuItems.FindByText("Mark as Closed").PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("In-Bond records closed manually"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestInBondNumberResetButton_Click()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.AllocateInBondNumber("1234");
			using (var form = new USInBondForm(header))
			{
				form.Show();
				ZPanel mainPanel = (ZPanel)form.Controls["MainPanel"];
				ZTemplateTabControl mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				ZTabPage mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				USInBondHeaderDetailUserControl inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals(true, inBondHeaderDetailsUserControl.Visible);
				ZGroupBox movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				ZGroupBox headerDetailsGroupBox = (ZGroupBox)movementHeadersGroupBox.Controls["HeaderDetailsGroupBox"];
				ZTabControl headerTabControl = (ZTabControl)headerDetailsGroupBox.Controls["HeaderTabControl"];
				ZTabPage departureTabPage = (ZTabPage)headerTabControl.Controls["DepartureTabPage"];
				var inBondNumberResetButton = (ZButton)departureTabPage.Controls["InBondNumberResetButton"];
				Env.Security.USInBondResetToOriginal.IsAllowed = false;
				inBondNumberResetButton.PerformClick();
				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.USInBondResetToOriginal), UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.USInBondResetToOriginal.IsAllowed = true;
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				inBondNumberResetButton.PerformClick();
				AssertEquals(USInBondHeaderDetailUserControl.CustomsStatusIsAwaitingOrLodged, UnitTestUserNotification.Instance.LastMessage.Text);
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				inBondNumberResetButton.PerformClick();
				Assert(!moveHeader.InBondNumber.IsEmpty);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddUserResponse("TEST RESET");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddOKAnswer();
				inBondNumberResetButton.PerformClick();
				AssertEquals(ZString.Empty, moveHeader.InBondNumber);
			}
		}

		public void TestInBondNumberAllocationButton_Click()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				ZPanel mainPanel = (ZPanel)form.Controls["MainPanel"];
				ZTemplateTabControl mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				ZTabPage mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				USInBondHeaderDetailUserControl inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals(true, inBondHeaderDetailsUserControl.Visible);
				ZGroupBox movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				ZGroupBox headerDetailsGroupBox = (ZGroupBox)movementHeadersGroupBox.Controls["HeaderDetailsGroupBox"];
				ZTabControl headerTabControl = (ZTabControl)headerDetailsGroupBox.Controls["HeaderTabControl"];
				ZTabPage departureTabPage = (ZTabPage)headerTabControl.Controls["DepartureTabPage"];
				ZButton inBondNumberAllocationButton = (ZButton)departureTabPage.Controls["InBondNumberAllocationButton"];
				ZGrid movementHeadersGrid = (ZGrid)movementHeadersGroupBox.Controls["MovementHeadersGrid"];
				AssertEquals(true, inBondNumberAllocationButton.ReadOnly);
				CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
				CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
				AssertEquals(false, inBondNumberAllocationButton.ReadOnly);
				movementHeadersGrid.SelectSingleElement(moveHeader1);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberAllocationButton.PerformClick();
				AssertEquals("The In-Bond Job must be saved before an In-Bond Number is allocated.\r\nDo you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("", moveHeader1.InBondNumber);
				AssertEquals("", moveHeader2.InBondNumber);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberAllocationButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("", moveHeader1.InBondNumber);
				AssertEquals("", moveHeader2.InBondNumber);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondNumberAllocationButton.PerformClick();
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("", moveHeader1.InBondNumber);
				AssertEquals("", moveHeader2.InBondNumber);
				moveHeader1.InBondNumber = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondNumberAllocationButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				var message = Factory.NewWithValidTestData<US.Business.MQEDIMessage>();
				moveHeader1.Messages.Add(message);
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondNumberAllocationButton.PerformClick();
				AssertEquals(CusInBondMoveHeader.Constants.InBondNumberAlreadyAllocated(moveHeader1.InBondNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestInValidCusEntryNum_CS00621924()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var header = Factory.New<CusInBondHeader>();
			Factory.RefreshEnabled = false;
			var moveHeader = header.MovementHeaders.AddNew();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var header2 = newFactory.Load<CusInBondHeader>(header.PK);
			var moveHeader2 = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
			using (USInBondForm form = new USInBondForm(header2))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				var mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				var inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals(true, inBondHeaderDetailsUserControl.Visible);
				var movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				var headerDetailsGroupBox = (ZGroupBox)movementHeadersGroupBox.Controls["HeaderDetailsGroupBox"];
				var headerTabControl = (ZTabControl)headerDetailsGroupBox.Controls["HeaderTabControl"];
				var departureTabPage = (ZTabPage)headerTabControl.Controls["DepartureTabPage"];
				var inBondNumberAllocationButton = (ZButton)departureTabPage.Controls["InBondNumberAllocationButton"];
				var movementHeadersGrid = (ZGrid)movementHeadersGroupBox.Controls["MovementHeadersGrid"];
				movementHeadersGrid.SelectSingleElement(moveHeader2);
				moveHeader.Delete();
				Factory.Save();
				Assert(moveHeader2.HasBeenDeleted);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberAllocationButton.PerformClick();
				AssertEquals("This In-Bond Movement Header has been deleted by other user while you have the job open. Please close the job, re-open and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestControlsVisibility_CS00195497()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				Control inBondHeaderDetailUserControl = form.Controls.Find("InBondHeaderDetailsUserControl", true)[0];
				// Air
				header.BH_ImportTransportMode = "40";
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("BH_ImportConveyanceNameCodeFindBox", true)[0].Visible);
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberTextBox", true)[0].Visible);
				AssertEquals(true, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberAirTextBox", true)[0].Visible);
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("TruckRailImportConveyanceNameTextBox", true)[0].Visible);
				// Non Air
				header.BH_ImportTransportMode = "10";
				AssertEquals(true, inBondHeaderDetailUserControl.Controls.Find("BH_ImportConveyanceNameCodeFindBox", true)[0].Visible);
				AssertEquals(true, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberTextBox", true)[0].Visible);
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberAirTextBox", true)[0].Visible);
				// Truck
				header.BH_ImportTransportMode = "30";
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("BH_ImportConveyanceNameCodeFindBox", true)[0].Visible);
				AssertEquals(true, inBondHeaderDetailUserControl.Controls.Find("TruckRailImportConveyanceNameTextBox", true)[0].Visible);
				AssertEquals("Truck Reg. No.", ((ZTextBox)inBondHeaderDetailUserControl.Controls.Find("TruckRailImportConveyanceNameTextBox", true)[0]).CaptionResourceString.Caption);
				AssertEquals(true, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberTextBox", true)[0].Visible);
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberAirTextBox", true)[0].Visible);
				// Rail
				header.BH_ImportTransportMode = "20";
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("BH_ImportConveyanceNameCodeFindBox", true)[0].Visible);
				AssertEquals(true, inBondHeaderDetailUserControl.Controls.Find("TruckRailImportConveyanceNameTextBox", true)[0].Visible);
				AssertEquals("Conveyance", ((ZTextBox)inBondHeaderDetailUserControl.Controls.Find("TruckRailImportConveyanceNameTextBox", true)[0]).CaptionResourceString.Caption);
				AssertEquals(true, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberTextBox", true)[0].Visible);
				AssertEquals(false, inBondHeaderDetailUserControl.Controls.Find("BH_VoyageNumberAirTextBox", true)[0].Visible);
			}
		}

		public void TestAirCarrier3LetterCodeFindBox()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = "40";
			header.BH_CarrierSCAC = "A1";
			header.ThreeLetterAirCarrierCode = "AAA";
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var inBondHeaderDetailUserControl = form.Controls.Find("InBondHeaderDetailsUserControl", true)[0];
				var airCarrier3LetterCodeFindBox = inBondHeaderDetailUserControl.FindSingle<ZCodeFindBox>("AirCarrier3LetterCodeFindBox");
				AssertEquals("airCarrier3LetterCodeFindBox.Visible", true, airCarrier3LetterCodeFindBox.Visible);
				AssertEquals("airCarrier3LetterCodeFindBox.Text", "AAA", airCarrier3LetterCodeFindBox.Text);
			}

			header.ThreeLetterAirCarrierCode = ZString.Empty;
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var inBondHeaderDetailUserControl = form.Controls.Find("InBondHeaderDetailsUserControl", true)[0];
				var airCarrier3LetterCodeFindBox = inBondHeaderDetailUserControl.FindSingle<ZCodeFindBox>("AirCarrier3LetterCodeFindBox");
				KeySender.SendKeyDownToProcessCmdKey(airCarrier3LetterCodeFindBox.CodeBox, Keys.F3);
				Application.DoEvents();
				using (var newAirlineForm = ZApplication.GetOpenForms().FirstOrDefault(x => x.GetType() == typeof(RefAirlineForm)))
				{
					AssertNotNull(newAirlineForm);
					AssertEquals("Module New Object title", "New Airline", newAirlineForm.Text);
					newAirlineForm.Close();
				}
			}
		}

		public void TestAirCarrier3LetterCodeFindBoxVisibility()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var inBondHeaderDetailUserControl = form.Controls.Find("InBondHeaderDetailsUserControl", true)[0];
				AssertNotNull(inBondHeaderDetailUserControl);
				var airCarrier3LetterCodeFindBox = inBondHeaderDetailUserControl.Controls.Find("AirCarrier3LetterCodeFindBox", true)[0];
				AssertNotNull(airCarrier3LetterCodeFindBox);
				header.BH_ImportTransportMode = "30";
				AssertEquals("AirCarrier3LetterCodeFindBox should be hidden when TransportMode is not Air", false, airCarrier3LetterCodeFindBox.Visible);
				header.BH_ImportTransportMode = "40";
				AssertEquals("AirCarrier3LetterCodeFindBox should be visible when TransportMode is Air", true, airCarrier3LetterCodeFindBox.Visible);
				header.BH_ImportTransportMode = "70";
				AssertEquals("AirCarrier3LetterCodeFindBox should be hidden when TransportMode is not Air", false, airCarrier3LetterCodeFindBox.Visible);
			}
		}

		public void TestMovementHeaderAirCarrier3LetterCodeFindBoxes()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.ThreeLetterInBondAirCarrierCode = "AAA";
			moveHeader1.ThreeLetterSplitAirCarrierCode = "BBB";
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				var mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				var inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals("inBondHeaderDetailsUserControl.Visible", true, inBondHeaderDetailsUserControl.Visible);
				var movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				var headerDetailsGroupBox = (ZGroupBox)movementHeadersGroupBox.Controls["HeaderDetailsGroupBox"];
				var headerTabControl = (ZTabControl)headerDetailsGroupBox.Controls["HeaderTabControl"];
				var departureTabPage = (ZTabPage)headerTabControl.Controls["DepartureTabPage"];
				var movementHeadersGrid = (ZGrid)movementHeadersGroupBox.Controls["MovementHeadersGrid"];
				movementHeadersGrid.SelectSingleElement(moveHeader1);
				var movementHeaderAirCarrier3LetterCodeFindBox = departureTabPage.Controls.Find("MovementHeaderAirCarrier3LetterCodeFindBox", true)[0];
				AssertNotNull(movementHeaderAirCarrier3LetterCodeFindBox);
				AssertEquals("Movement3-LetterCodeFindBox should be visible when Transport mode is Air", true, movementHeaderAirCarrier3LetterCodeFindBox.Visible);
				AssertEquals("movementHeaderAirCarrier3LetterCodeFindBox.Text", "AAA", movementHeaderAirCarrier3LetterCodeFindBox.Text);
				AssertEquals("Grid cell 'In-Bond Carrier 3-Letter' should be editable when Transport mode is Air", false, moveHeader1.ThreeLetterInBondAirCarrierCode_ReadOnly);
				var splitAirCarrier3LetterCodeFindBox = departureTabPage.Controls.Find("SplitAirCarrier3LetterCodeFindBox", true)[0];
				AssertNotNull(splitAirCarrier3LetterCodeFindBox);
				AssertEquals("Split3-LetterCodeFindBox should be visible when Transport mode is Air", true, splitAirCarrier3LetterCodeFindBox.Visible);
				AssertEquals("splitAirCarrier3LetterCodeFindBox.Text", "BBB", splitAirCarrier3LetterCodeFindBox.Text);
				AssertEquals("Grid cell 'Split Carrier 3-Letter' should be editable when Transport mode is Air", false, moveHeader1.ThreeLetterSplitAirCarrierCode_ReadOnly);
				header.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.TruckNonContainer;
				AssertEquals("Movement3-LetterCodeFindBox should only be visible when Transport mode is Air", false, movementHeaderAirCarrier3LetterCodeFindBox.Visible);
				AssertEquals("Grid cell 'In-Bond Carrier 3-Letter' should be readonly when Transport mode is not Air", true, moveHeader1.ThreeLetterInBondAirCarrierCode_ReadOnly);
				AssertEquals("Split3-LetterCodeFindBox should only be visible when Transport mode is Air", false, splitAirCarrier3LetterCodeFindBox.Visible);
				AssertEquals("Grid cell 'Split Carrier 3-Letter' should be readonly when Transport mode is not Air", true, moveHeader1.ThreeLetterSplitAirCarrierCode_ReadOnly);
			}
		}

		public void TestPostDepatureOnlyEditability()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_CustomsStatus = "CDO";
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				var mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				var inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				var postDepartureOnly = (ZCheckBox)inBondHeaderDetailsUserControl.Controls.Find("PostDepartureOnlyCheckBox", true)[0];
				AssertEquals(true, postDepartureOnly.ReadOnly);
			}
		}

		public void TestMutexUnLockedWhenInBondNumberAllocated()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			Factory.Save();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				ZPanel mainPanel = (ZPanel)form.Controls["MainPanel"];
				ZTemplateTabControl mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				ZTabPage mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				USInBondHeaderDetailUserControl inBondHeaderDetailsUserControl = (USInBondHeaderDetailUserControl)mainTabPage.Controls["InBondHeaderDetailsUserControl"];
				AssertEquals(true, inBondHeaderDetailsUserControl.Visible);
				ZGroupBox movementHeadersGroupBox = (ZGroupBox)inBondHeaderDetailsUserControl.Controls["MovementHeadersGroupBox"];
				ZGroupBox headerDetailsGroupBox = (ZGroupBox)movementHeadersGroupBox.Controls["HeaderDetailsGroupBox"];
				ZTabControl headerTabControl = (ZTabControl)headerDetailsGroupBox.Controls["HeaderTabControl"];
				ZTabPage departureTabPage = (ZTabPage)headerTabControl.Controls["DepartureTabPage"];
				ZButton inBondNumberAllocationButton = (ZButton)departureTabPage.Controls["InBondNumberAllocationButton"];
				ZGrid movementHeadersGrid = (ZGrid)movementHeadersGroupBox.Controls["MovementHeadersGrid"];
				AssertEquals(false, inBondNumberAllocationButton.ReadOnly);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				inBondNumberAllocationButton.PerformClick();
				AssertNotEquals(ZString.Empty, moveHeader.InBondNumber);
				Assert(!moveHeader.InBondNumberAllocationMutexIsLocked());
			}
		}
	}
}
