using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	[TestedType(typeof(USAMSMessageSendingActionForm))]
	sealed class USAMSMessageSendingActionFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var inBondMovement = header.InBondMovementHeaders.AddNew();
			inBondMovement.MovementDetails.AddNew(bill1.PK);
			foreach (var data in new Tuple<ActionCode, string>[] {
				new Tuple<ActionCode, string>(ActionCode.InBondArrival, "In-Bond Arrival"),
				new Tuple<ActionCode, string>(ActionCode.InBondExportation, "In-Bond Exportation"),
				new Tuple<ActionCode, string>(ActionCode.InBondDiversion, "In-Bond Diversion"),
				new Tuple<ActionCode, string>(ActionCode.InBondTransferOfLiability, "In-Bond Transfer of Liability"),
				new Tuple<ActionCode, string>(ActionCode.SubsequentInBondOriginal, "Subsequent In-Bond Original"),
				new Tuple<ActionCode, string>(ActionCode.SubsequentInBondAmendment, "Subsequent In-Bond Amendment"),
				new Tuple<ActionCode, string>(ActionCode.CancelPermitToTransfer, "Cancel Permit To Transfer by Bill"),
				})
			{
				using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, data.Item1)))
				{
					AssertEquals("Form Caption for " + data.Item1.ToString(), data.Item2, form.FormCaption);
				}
			}
		}

		public void TestMutexReleaseOnDispose()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var inBondMovement1 = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail1 = inBondMovement1.MovementDetails.AddNew(bill1.PK);
			var inBondMovement2 = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail2 = inBondMovement2.MovementDetails.AddNew(bill1.PK);
			var inBondMovement3 = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail3 = inBondMovement3.MovementDetails.AddNew(bill1.PK);
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.SubsequentInBondOriginal)))
			{
				var sendingAction = form.BusinessEntity;
				form.Show();
				AssertEquals(3, sendingAction.Movements.Count);
				foreach (MessageSendingMovement movement in sendingAction.Movements)
				{
					movement.MM_Send = true;
				}

				AssertEquals(true, inBondMovement1.InBondNumberAllocationMutexHasLock());
				AssertEquals(true, inBondMovement2.InBondNumberAllocationMutexHasLock());
				AssertEquals(true, inBondMovement3.InBondNumberAllocationMutexHasLock());
			}

			AssertEquals(false, inBondMovement1.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, inBondMovement2.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, inBondMovement3.InBondNumberAllocationMutexHasLock());
		}

		public void TestColumnVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var inBondMovement = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail = inBondMovement.MovementDetails.AddNew(bill1.PK);
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.SubsequentInBondOriginal)))
			{
				var sendingAction = form.BusinessEntity;
				form.Show();
				AssertEquals(1, sendingAction.Movements.Count);
				var inBondSendingAction = sendingAction.Movements[0];
				inBondSendingAction.MM_Send = ZBool.True;
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_RelatedDetails));
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.SubsequentInBondAmendment)))
			{
				var sendingAction = form.BusinessEntity;
				form.Show();
				AssertEquals(1, sendingAction.Movements.Count);
				var inBondSendingAction = sendingAction.Movements[0];
				inBondSendingAction.MM_Send = ZBool.True;
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_RelatedDetails));
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd)))
			{
				var sendingAction = form.BusinessEntity;
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_RelatedDetails));
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingDelete)))
			{
				var sendingAction = form.BusinessEntity;
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_PortOfUnlading));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_PortOfUnladingOverride));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_RelatedDetails));
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.InBondArrival)))
			{
				var sendingAction = form.BusinessEntity;
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_RelatedDetails));
			}
		}

		public void TestSelectTickAndUntickMenu()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB2";
			var bill3 = header.Bills.AddNew();
			bill3.B0_MasterBillNumber = "MB3";
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.Creating)))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(3, messageAction.MessageSendingObjects.Count);
				form.Show();
				var movementsAndBillsSplitContainer = (CargoWise.Windows.UI.KSplitContainer)form.Controls["MovementsAndBillsSplitContainer"];
				var splitContainer = (CargoWise.Windows.UI.KSplitContainer)movementsAndBillsSplitContainer.Panel2.Controls["BillsAndMessageContentSplitContainer"];
				var billsGroupBox = (ZGroupBox)splitContainer.Panel1.Controls["BillsGroupBox"];
				var billGrid = (ZArchitecture.ZGrid)billsGroupBox.Controls["BillsGrid"];
				billGrid.Focus();
				billGrid.SelectAllElements();
				var tickSendAllMenuItem = billGrid.ContextMenu.MenuItems.FindByText("Tick 'Send' for Selected");
				AssertNotNull("Precondition: Tick 'Send' for Selected", tickSendAllMenuItem);
				tickSendAllMenuItem.PerformClick();
				foreach (MessageSendingObject bill in messageAction.MessageSendingObjects)
				{
					AssertEquals(true, bill.MB_Send);
				}

				billGrid.SelectAllElements();
				var untickSendAllMenuItem = billGrid.ContextMenu.MenuItems.FindByText("Untick 'Send' for Selected");
				AssertNotNull("Precondition: Untick 'Send' for Selected", untickSendAllMenuItem);
				untickSendAllMenuItem.PerformClick();
				foreach (MessageSendingObject bill in messageAction.MessageSendingObjects)
				{
					AssertEquals(false, bill.MB_Send);
				}

				billGrid.UnSelect(1);
				AssertEquals("Precondition", 2, billGrid.SelectedElements.Length);
				tickSendAllMenuItem.PerformClick();
				AssertEquals(true, messageAction.MessageSendingObjects[0].MB_Send);
				AssertEquals(false, messageAction.MessageSendingObjects[1].MB_Send);
				AssertEquals(true, messageAction.MessageSendingObjects[2].MB_Send);
			}
		}

		public void TestCorrectMessageErrorIsIncludedInNotification()
		{
			var carrier = Factory.New<US.Business.USCarrierCombined>();
			carrier.UI_Code = "Z!!1";
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_ImportTransportMode = "10";
			header.BH_ImportConveyanceName = "VESSEL 1";
			header.BH_ImportConveyanceCountry = "GB";
			header.BH_LloydsNumber = "1111111";
			header.BH_VoyageNumber = "2343";
			header.BH_ETA = ZDateTime.Today.AddDays(1);
			header.BH_PortUnladingDCode = "2704";
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "Z!!1";
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "Z!!1";
			bill2.B0_MasterBillNumber = "MB2";
			var inbondMoveHeader1 = header.InBondMovementHeaders.AddNew();
			inbondMoveHeader1.InBondNumber = "INB1";
			inbondMoveHeader1.BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
			inbondMoveHeader1.BM_DestinationPortCode = "0416";
			var inbondMoveHeader1Detail1 = inbondMoveHeader1.MovementDetails.AddNew(bill1.PK);
			var inbondMoveHeader1Detail2 = inbondMoveHeader1.MovementDetails.AddNew(bill2.PK);
			var inbondMoveHeader2 = header.InBondMovementHeaders.AddNew();
			inbondMoveHeader2.InBondNumber = "INB2";
			inbondMoveHeader2.BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
			var inbondMoveHeader2Detail1 = inbondMoveHeader2.MovementDetails.AddNew(bill1.PK);
			var inbondMoveHeader2Detail2 = inbondMoveHeader2.MovementDetails.AddNew(bill2.PK);
			header.IsTopLevel = true;
			AssertEquals(ValidationModes.InventoryRecord, header.ValidationModes);
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.InBondArrival)))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(2, messageAction.Movements.Count);
				form.Show();
				messageAction.Movements[0].MM_Send = true;
				var bottomPanel = (ZPanel)form.Controls["BottomPanel"];
				var sendButton = (ZButton)bottomPanel.Controls["SendButton"];
				AssertEquals(true, header.IsTopLevel);
				sendButton.PerformClick();
				AssertEquals(true, header.IsTopLevel);
				AssertContains("Should contain In-Bond movement 'INB1' message error", "Arrival Date: You have not entered an Arrival Date.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("Should not contain In-Bond movement 'INB2' message error", "US Port Of Destination: You have not entered an US Port Of Destination.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Should contain header message error", "Carrier Code: You have not entered a Carrier Code.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(ValidationModes.InventoryRecord, header.ValidationModes);
		}

		public void TestBillValidationModesIsResetOnDisposal()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.ValidationModes = ValidationModes.InBondTOL;
			var bill1 = header.Bills.AddNew();
			bill1.ValidationModes = ValidationModes.InventoryRecord;
			var bill2 = header.Bills.AddNew();
			bill2.ValidationModes = ValidationModes.None;
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.Creating)))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(2, messageAction.MessageSendingObjects.Count);
				AssertEquals(ValidationModes.InventoryRecord, bill1.ValidationModes);
				AssertEquals(ValidationModes.InventoryRecord, bill2.ValidationModes);
			}

			AssertEquals(ValidationModes.InBondTOL, bill1.ValidationModes);
			AssertEquals(ValidationModes.None, bill2.ValidationModes);
		}

		public void TestColumnVisibilityForVesselEvents()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var pttMovement = header.PTTMovements.AddNew();
			var pttMoveDetail = pttMovement.MovementDetails.AddNew(bill1.PK);
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.PermitToTransfer)))
			{
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				var movementsGrid = GetMovementsGridFromForm(form);
				movementsGrid.Focus();
				AssertEquals(false, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_Date));
				AssertEquals(true, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_PTTFiler));
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.VesselArrival)))
			{
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_PortOfUnlading));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Date));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				var movementsGrid = GetMovementsGridFromForm(form);
				movementsGrid.Focus();
				AssertEquals(false, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_Date));
				AssertEquals(false, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_PTTFiler));
				var style = billGrid.GetColumnStyle(MessageSendingObject.Schema.MB_Date);
				AssertEquals("Arrival Date", style.CaptionResourceString.Caption);
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.VesselDeparture)))
			{
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Date));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				var movementsGrid = GetMovementsGridFromForm(form);
				movementsGrid.Focus();
				AssertEquals(true, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_Date));
				AssertEquals(false, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_PTTFiler));
				var style = movementsGrid.GetColumnStyle(MessageSendingMovement.Schema.MM_Date);
				AssertEquals("Date", style.CaptionResourceString.Caption);
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.ChangeEstDateOfArrival)))
			{
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_PortOfUnlading));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Date));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				var movementsGrid = GetMovementsGridFromForm(form);
				movementsGrid.Focus();
				AssertEquals(false, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_Date));
				AssertEquals(false, movementsGrid.Columns.Contains(MessageSendingMovement.Schema.MM_PTTFiler));
				var style = billGrid.GetColumnStyle(MessageSendingObject.Schema.MB_Date);
				AssertEquals("Date", style.CaptionResourceString.Caption);
			}

			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.CancelPermitToTransfer)))
			{
				form.Show();
				var billGrid = GetBillsGridFromForm(form);
				billGrid.Focus();
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(false, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(true, billGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
			}
		}

		public void TestOverridePortCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_PortUnladingDCode = "2222";
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd)))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(1, messageAction.MessageSendingObjects.Count);
				form.Show();
				var bottomPanel = (ZPanel)form.Controls["BottomPanel"];
				var billsGrid = GetBillsGridFromForm(form);
				billsGrid.Focus();
				AssertEquals(true, billsGrid.Columns.Contains(MessageSendingObject.Schema.MB_Send));
				AssertEquals(true, billsGrid.Columns.Contains(MessageSendingObject.Schema.MB_AmendmentCode));
				AssertEquals(true, billsGrid.Columns.Contains(MessageSendingObject.Schema.MB_BillActionCode));
				AssertEquals(false, billsGrid.Columns.Contains(MessageSendingObject.Schema.MB_PortOfUnlading));
				AssertEquals(true, billsGrid.Columns.Contains(MessageSendingObject.Schema.MB_PortOfUnladingOverride));
				AssertEquals(false, billsGrid.Columns.Contains(MessageSendingObject.Schema.MB_RelatedDetails));
				billsGrid.Select(0);
				var message = (MessageSendingObject)billsGrid.SelectedElements[0];
				message.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.AddBill;
				AssertEquals(true, message.MB_PortOfUnladingOverrideInfo.ReadOnly);
				AssertContains("INPA01", message.MB_MessageContents);
				AssertContains("C B P Port (8-11)                      :2222", message.MB_MessageContents);
				AssertContains("INPP01", message.MB_MessageContents);
				AssertContains("Port Of Unlading Code (4-7) :2222", message.MB_MessageContents);
				message.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.DeleteBill;
				AssertEquals(false, message.MB_PortOfUnladingOverrideInfo.ReadOnly);
				message.MB_PortOfUnladingOverride = "1234";
				AssertContains("INPA01", message.MB_MessageContents);
				AssertContains("C B P Port (8-11)                      :1234", message.MB_MessageContents);
				AssertContains("INPP01", message.MB_MessageContents);
				AssertContains("Port Of Unlading Code (4-7) :1234", message.MB_MessageContents);
				message.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
				AssertEquals(true, message.MB_PortOfUnladingOverrideInfo.ReadOnly);
				AssertContains("INPA01", message.MB_MessageContents);
				AssertContains("C B P Port (8-11)                      :2222", message.MB_MessageContents);
				AssertContains("INPP01", message.MB_MessageContents);
				AssertContains("Port Of Unlading Code (4-7) :2222", message.MB_MessageContents);
				message.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd;
				AssertEquals(false, message.MB_PortOfUnladingOverrideInfo.ReadOnly);
				message.MB_PortOfUnladingOverride = "1234";
				AssertContains("INPA01", message.MB_MessageContents);
				AssertContains("C B P Port (8-11)                      :1234", message.MB_MessageContents);
				AssertContains("INPP01", message.MB_MessageContents);
				AssertContains("Port Of Unlading Code (4-7) :1234", message.MB_MessageContents);
			}
		}

		public void TestSendButton_WillShowProgressDialog_WhenClicked()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var inBondMovement = header.InBondMovementHeaders.AddNew();
			inBondMovement.MovementDetails.AddNew(bill1.PK);
			Factory.Save();
			using (var form = new USAMSMessageSendingActionForm(new MessageSendingAction(header, ActionCode.Creating)))
			{
				form.Show();
				var sendButton = form.Controls.Find("SendButton", true).Single() as ZButton;
				sendButton.PerformClick();
				var progressForm = ZFormModaliser.LastFormShownForTest as ProgressForm;
				AssertNotNull(progressForm);
				AssertProgressFormProperties(progressForm, "Validating data");
			}
		}

		ZArchitecture.ZGrid GetBillsGridFromForm(USAMSMessageSendingActionForm form)
		{
			var movementsAndBillsSplitContainer = (CargoWise.Windows.UI.KSplitContainer)form.Controls["MovementsAndBillsSplitContainer"];
			var splitContainer = (CargoWise.Windows.UI.KSplitContainer)movementsAndBillsSplitContainer.Panel2.Controls["BillsAndMessageContentSplitContainer"];
			var billsGroupBox = (ZGroupBox)splitContainer.Panel1.Controls["BillsGroupBox"];
			return (ZArchitecture.ZGrid)billsGroupBox.Controls["BillsGrid"];
		}

		ZArchitecture.ZGrid GetMovementsGridFromForm(USAMSMessageSendingActionForm form)
		{
			var movementsAndBillsSplitContainer = (CargoWise.Windows.UI.KSplitContainer)form.Controls["MovementsAndBillsSplitContainer"];
			var movementsGroupBox = (ZGroupBox)movementsAndBillsSplitContainer.Panel1.Controls["MovementsGroupBox"];
			return (ZArchitecture.ZGrid)movementsGroupBox.Controls["MovementsGrid"];
		}

		protected override Form GetFormToBashCore()
		{
			return new USAMSMessageSendingActionForm(new MessageSendingAction(Factory.New<CusInBondHeader>(), ActionCode.Creating));
		}

		void AssertProgressFormProperties(ProgressForm progressForm, string caption)
		{
			CombineAssertions(() =>
			{
				Assert("cancel button should not be shown", !progressForm.ShowCancelButton);
				Assert("progress bar should not be shown", !progressForm.ShowProgressBar);
				AssertEquals(caption, progressForm.CaptionResourceString.Caption);
			});
		}
	}
}
