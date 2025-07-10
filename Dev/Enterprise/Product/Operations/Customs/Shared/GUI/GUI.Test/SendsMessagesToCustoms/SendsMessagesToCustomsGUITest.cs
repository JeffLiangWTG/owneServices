using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class SendsMessagesToCustomsGUITest : TestCaseWithFactory
	{
		public void TestContinueWithAction()
		{
			SendsMessagesToCustomsGUI messageSender = new SendsMessagesToCustomsGUI();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, messageSender.ContinueWithAction("MyMessage", "MyCaption"));
			AssertEquals("MyMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, messageSender.ContinueWithAction("MyMessage2", "MyCaption"));
			AssertEquals("MyMessage2", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
		}

		public void TestAskUserToContinueWithAction()
		{
			SendsMessagesToCustomsGUI messageSender = new SendsMessagesToCustomsGUI();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, messageSender.AskUserToContinueWithAction("MyMessage", "MyCaption"));
			AssertEquals("MyMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, messageSender.AskUserToContinueWithAction("MyMessage2", "MyCaption"));
			AssertEquals("MyMessage2", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			BaseJobDeclaration decl = Factory.New<BaseJobDeclaration>();

			bool oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
			bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			try
			{
				Env.Security.SupervisorOverrides.IsAllowed = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				GlbStaff.CurrentUser.GS_IsController = false;
				decl.JE_MessageType = "ZZZ";
				decl.Validation.ValidateAll();
				GlbStaff staff = Factory.New<GlbStaff>();
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_IsActive = true;

				GlbSecurity se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = Env.Security.AllowMessageErrors.Code;
				se.GU_SecurityItemIsAllowed = true;
				se.GU_GS = staff.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals(false, messageSender.AskUserToContinueWithAction("MyMessage", "MyCaption", decl));
				AssertEquals("MyMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
			finally
			{
				Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
				GlbStaff.CurrentUser.GS_IsController = oldIsController;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
			}
		}

		public void TestYesNoQuery()
		{
			SendsMessagesToCustomsGUI messageSender = new SendsMessagesToCustomsGUI();
			messageSender.YesNoQuery("Are Cuckoo Squeakers awesome?", "Cuckoo Squeakers are pretty cool you know");
			AssertEquals("Question Are Cuckoo Squeakers awesome?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestYesNoCancelQuery()
		{
			var messageSender = new SendsMessagesToCustomsGUI();
			var result = messageSender.YesNoCancelQuery("Have you ever stopped to think and forgotten to start again?", "Thinking");
			AssertEquals("Question Have you ever stopped to think and forgotten to start again?", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Yes", YesNoCancel.Yes, result);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			result = messageSender.YesNoCancelQuery("No", "No");
			AssertEquals("No", YesNoCancel.No, result);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			result = messageSender.YesNoCancelQuery("Cancel", "Cancel");
			AssertEquals("Cancel", YesNoCancel.Cancel, result);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Ignore);
			result = messageSender.YesNoCancelQuery("Ignore it and it will go away", "Cancel");
			AssertEquals("Default Cancel", YesNoCancel.Cancel, result);
		}

		public void TestWhichMessagesShouldWeSend()
		{
			TestWhichMessagesShouldWeSend(false, false);
			TestWhichMessagesShouldWeSend(false, true);
			TestWhichMessagesShouldWeSend(true, false);
			TestWhichMessagesShouldWeSend(true, true);
		}

		public void TestWhichMessagesShouldWeWithdraw()
		{
			TestWhichMessagesShouldWeWithdraw(false, false);
			TestWhichMessagesShouldWeWithdraw(false, true);
			TestWhichMessagesShouldWeWithdraw(true, false);
			TestWhichMessagesShouldWeWithdraw(true, true);
		}

		public void TestWhichMessagesShouldWeSendShowsMessageIfThereAreNoMessagesToSend()
		{
			SingleMessageManager[] result = Sender.WhichMessagesShouldWeSend(Array.Empty<SingleMessageManager>());
			AssertEquals("LastMessage.WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("LastMessage.Text", "There is nothing available for sending", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhichMessagesShouldWeWithdrawShowsMessageIfThereAreNoMessagesToSend()
		{
			SingleMessageManager[] result = Sender.WhichMessagesShouldWeWithdraw(Array.Empty<SingleMessageManager>());
			AssertEquals("LastMessage.WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("LastMessage.Text", "There is nothing available for withdrawing", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhichMessagesShouldWeResetShowsMessageIfThereAreNoMessagesToSend()
		{
			SingleMessageManager[] result = Sender.WhichMessagesShouldWeReset(Array.Empty<SingleMessageManager>());
			AssertEquals("LastMessage.WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("LastMessage.Text", "There is nothing available for resetting", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOverdueCargoReportException()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			exceptionProcessTask1 = (ForwardingShipmentProcessTask)shipment1.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask1.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
			exceptionProcessTask1.P9_Notes = notesTestBlob;
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			exceptionProcessTask2 = (ForwardingShipmentProcessTask)shipment2.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask2.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code;
			Assert("Pre condition - not actioned", !exceptionProcessTask1.IsExceptionActioned);
			Assert("Pre condition - not actioned", !exceptionProcessTask2.IsExceptionActioned);

			ZFormModaliser.ShowDialogsInTest = true;

			RunOneOverdueCargoReportExceptionTest(exceptionProcessTask1, exceptionProcessTask2, DialogResult.OK, 2, true, true, notesTestBlob, notesTestBlob);
			RunOneOverdueCargoReportExceptionTest(exceptionProcessTask1, exceptionProcessTask2, DialogResult.Cancel, 0, false, false, notesTestBlob, emptyBlob);
			RunOneOverdueCargoReportExceptionTest(null, exceptionProcessTask2, DialogResult.OK, 2, false, true, notesTestBlob, emptyBlob);
			RunOneOverdueCargoReportExceptionTest(null, exceptionProcessTask2, DialogResult.Cancel, 1, false, false, notesTestBlob, emptyBlob);
			RunOneOverdueCargoReportExceptionTest(exceptionProcessTask1, null, DialogResult.OK, 2, true, false, notesTestBlob, emptyBlob);
			RunOneOverdueCargoReportExceptionTest(exceptionProcessTask1, null, DialogResult.Cancel, 1, false, false, notesTestBlob, emptyBlob);
			RunOneOverdueCargoReportExceptionTest(null, null, DialogResult.OK, 2, false, false, notesTestBlob, emptyBlob);
			RunOneOverdueCargoReportExceptionTest(null, null, DialogResult.Cancel, 2, false, false, notesTestBlob, emptyBlob);
		}

		public void TestOverdueCargoReportExceptionErrorsAreReported()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			exceptionProcessTask1 = (ForwardingShipmentProcessTask)shipment1.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask1.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			exceptionProcessTask2 = (ForwardingShipmentProcessTask)shipment2.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask2.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code;
			Assert("Pre condition - not actioned", !exceptionProcessTask1.IsExceptionActioned);
			Assert("Pre condition - not actioned", !exceptionProcessTask2.IsExceptionActioned);

			ZString lateCargoReportReasonToSet = "";
			ZString lateCargoReportTextToSet = "";
			DialogResult result = DialogResult.None;
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				if (obj is ExceptionReasonDialog reasonDlg)
				{
					exceptionProcessTask1.LateCargoReportReason = lateCargoReportReasonToSet;
					exceptionProcessTask1.LateCargoReportText = lateCargoReportTextToSet;

					reasonDlg.YesButtonX.PerformClick();
					result = reasonDlg.DialogResult;
				}
			});

			ZFormModaliser.ShowDialogsInTest = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			RunOneOverdueCargoReportExceptionTest(exceptionProcessTask1, exceptionProcessTask2, DialogResult.Cancel, 0, false, false, emptyBlob, emptyBlob);
			AssertContains("There are errors", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(DialogResult.None, result);
			AssertEquals(1, exceptionProcessTask1.Notifications.Count());

			lateCargoReportReasonToSet = LateCargoReportingReasons.Codes.MISCReferNotes;
			lateCargoReportTextToSet = notesTestBlob.ToAscii();
			var expectedBlob = ZBlob.FromAscii(lateCargoReportReasonToSet + ":" + lateCargoReportTextToSet);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			RunOneOverdueCargoReportExceptionTest(exceptionProcessTask1, exceptionProcessTask2, DialogResult.OK, 2, true, true, expectedBlob, expectedBlob);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(DialogResult.OK, result);
			AssertEquals(0, exceptionProcessTask1.Notifications.Count());
		}

		public void TestContinueWithSaveWhenThereIsNoMessageToSend()
		{
			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			AssertEquals("No messages to send", false, info.HasMessagesToSend);

			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			AssertEquals("ContinueWithSave when there is no message to send", ContinueWithSave.Yes, controller.DetermineRequiredMessagesAndSendThem(manager));
		}

		public void TestContinueWithSaveWhenThereIsAnError()
		{
			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);

			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());

			singleManager.AdditionalCommonNotifications.AddWarning("TEST");
			singleManager.IsWaitingForResponseExposed = true;//error
			singleManager.RequiresAmendmentCoreExposed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("ContinueWithSave when there is an error", ContinueWithSave.No, controller.DetermineRequiredMessagesAndSendThem(manager));

			ZString notification = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("PendingMessages are notified", true, notification.Contains(SingleMessageManager.PendingMessagesErrorMessage));
		}

		public void TestContinueWithSaveWhenThereIsAnErrorCollectedFromManagers()
		{
			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());

			info.AllNotifications.AddError("TEST Error");
			mockManager.Setup(m => m.SendAnyMessagesRequired(info)).Returns(new MessageGenerationResultCollection());
			singleManager.IsWaitingForResponseExposed = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("ContinueWithSave when there is an error", ContinueWithSave.No, controller.DetermineRequiredMessagesAndSendThem(manager));

			ZString notification = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("PendingMessages are notified", true, notification.Contains("TEST Error"));
		}

		public void TestBackDoorSavingFormOpenedAndUserCancelled()
		{
			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;
			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.GetDeferredAmendmentSavingOptions()).Returns(savingOptions);
			AssertEquals("PreCondition:No errors", false, info.AllNotifications.ContainsError());

			savingOptions.IsCancelled = true;
			AssertEquals("ContinueWithSave when Users cancelled", ContinueWithSave.No, controller.DetermineRequiredMessagesAndSendThem(manager));

			savingOptions.IsCancelled = true;
			mockManager.Verify(m => m.SendAnyMessagesRequired(It.IsAny<RequiredMessagesInformation>()), Times.Never);
			AssertEquals("ContinueWithSave when Users cancelled", ContinueWithSave.No, controller.DetermineRequiredMessagesAndSendThem(manager));
			mockManager.VerifyAll();
		}

		public void TestBackDoorSavingFormOpenedAndUsersChooseToSaveWithoutSending()
		{
			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;
			IDeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.GetDeferredAmendmentSavingOptions()).Returns(savingOptions);

			savingOptions.SetSaveWithEntryChangesValueForTestingTo(true);

			mockManager.Verify(m => m.SendAnyMessagesRequired(It.IsAny<RequiredMessagesInformation>()), Times.Never);
			AssertEquals("ContinueWithSave when users choose to save without sending", ContinueWithSave.Yes, controller.DetermineRequiredMessagesAndSendThem(manager));
			mockManager.VerifyAll();//no calls for ProcessWithDetectionResultAndSendAnyMessagesRequired
		}

		[ExpectNoExceptions]
		public void TestSavingWithoutEntryChangesEventCourses()
		{
			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;
			IDeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();

			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.GetDeferredAmendmentSavingOptions()).Returns(savingOptions);

			savingOptions.SetSaveWithoutEntryChangesValueForTestingTo(true);

			var mockController = new Mock<SendsMessagesToCustomsGUI>();
			mockController.CallBase = true;
			SendsMessagesToCustomsGUI controller = mockController.Object;

			mockController.Verify(m => m.GetAmendmentWithdrawalReason(It.IsAny<AmendmentWithdrawalReason>()), Times.Never);

			controller.DetermineRequiredMessagesAndSendThem(manager);

			mockController.VerifyAll();
			mockManager.VerifyAll();//should not have called GetAmendmentWithdrawalReason, but called ProcessWhenSavedWithoutSendingAmendment
		}

		public void TestSavingWithEntryChangesTakesAmendmentReason()
		{
			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;
			IDeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.GetDeferredAmendmentSavingOptions()).Returns(savingOptions);

			savingOptions.SetSaveWithEntryChangesValueForTestingTo(true);

			var mockController = new Mock<SendsMessagesToCustomsGUI>();
			mockController.CallBase = true;
			mockController.Setup(m => m.GetAmendmentWithdrawalReason(info.AmendmentWithdrawalReason)).Returns(ContinueWithSave.Yes);

			SendsMessagesToCustomsGUI controller = mockController.Object;
			controller.DetermineRequiredMessagesAndSendThem(manager);

			mockController.VerifyAll();//should have called GetAmendmentWithdrawalReason

			mockController.Setup(m => m.GetAmendmentWithdrawalReason(info.AmendmentWithdrawalReason)).Returns(ContinueWithSave.No);
			AssertEquals("User cancelled at this form", ContinueWithSave.No, controller.DetermineRequiredMessagesAndSendThem(manager));
			mockController.VerifyAll();
			mockManager.VerifyAll();//should not call ProcessWhenSavedWithoutSendingAmendment as the process was cancelled

			mockController.Setup(m => m.GetAmendmentWithdrawalReason(info.AmendmentWithdrawalReason)).Returns(ContinueWithSave.Yes);
			AssertEquals("User choose to save with entry changes after entering amendment reason", ContinueWithSave.Yes, controller.DetermineRequiredMessagesAndSendThem(manager));
			mockController.VerifyAll();//should call ProcessWhenSavedWithoutSendingAmendment
			mockManager.VerifyAll();
		}

		public void TestWhichMessagesToSendDoesNotPopUpWhenBackDoorSavingIsSupportedToAvoidTooManyDialogs()
		{
			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;
			IDeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();

			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.DeferredAmendmentTillAfterSaveSuccessful).Returns(false);
			mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
			mockManager.Setup(m => m.SendAnyMessagesRequired(info)).Returns(new MessageGenerationResultCollection());
			mockManager.Setup(m => m.GetDeferredAmendmentSavingOptions()).Returns(savingOptions);

			AssertEquals("PreCondition:No errors", false, info.AllNotifications.ContainsError());
			AssertEquals("PreCondition:No warnings", false, info.AllNotifications.ContainsWarning());

			savingOptions.SetSendAmendmentValueForTestingTo(true);

			var mockController = new Mock<SendsMessagesToCustomsGUI>();
			SendsMessagesToCustomsGUI controller = mockController.Object;

			mockController.Protected().Verify("AdviseWhichMessagesToSend", Times.Never(), ItExpr.IsAny<RequiredMessagesInformation>());
			controller.DetermineRequiredMessagesAndSendThem(manager);
			mockController.VerifyAll();//AdviseWhichMessagesToSend should not be triggered as backDoorSaving will inform users
		}

		public void TestUsersCancelWhenAdvisedWhichMessageToSend()
		{
			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = false;

			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
			AssertEquals("PreCondition:No errors", false, info.AllNotifications.ContainsError());
			AssertEquals("PreCondition:No warnings", false, info.AllNotifications.ContainsWarning());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);//user cancels

			ContinueWithSave result = controller.DetermineRequiredMessagesAndSendThem(manager);
			AssertEquals("Should not allow to save as user cancels", ContinueWithSave.No, result);

			info.AllNotifications.AddWarning("teapot cuckoo sqeaker");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);//user agrees for which messages to send
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);//user cancels when notification is shown

			result = controller.DetermineRequiredMessagesAndSendThem(manager);
			AssertEquals("Should not allow to save as user cancels", ContinueWithSave.No, result);

			ZString warning = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("warning is shown", true, warning.Contains("teapot cuckoo sqeaker"));
		}

		public void TestAdditionalActionBeforeSendingMessagesReturnsNo()
		{
			controller.AdditionalActionResultExposed = ContinueWithSave.No;

			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = false;

			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);//yes to which message to send

			mockManager.Verify(m => m.SendAnyMessagesRequired(It.IsAny<RequiredMessagesInformation>()), Times.Never);

			ContinueWithSave result = controller.DetermineRequiredMessagesAndSendThem(manager);
			AssertEquals("Should not allow to save", ContinueWithSave.No, result);

			mockManager.VerifyAll();//should not send any messages
		}

		public void TestMessagesSentAndUsersAreAdvised()
		{
			controller.AdditionalActionResultExposed = ContinueWithSave.No;

			bizObj.SupportBackDoorForSavingWhenAmendmentDetected = false;

			RequiredMessagesInformation info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.DeferredAmendmentTillAfterSaveSuccessful).Returns(false);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
			mockManager.Setup(m => m.SendAnyMessagesRequired(info)).Returns(new MessageGenerationResultCollection());
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);//yes to which message to send
			controller.AdditionalActionResultExposed = ContinueWithSave.Yes;

			ContinueWithSave result = controller.DetermineRequiredMessagesAndSendThem(manager);
			AssertEquals("Should allow to save", ContinueWithSave.Yes, result);

			mockManager.VerifyAll();//should send messages
		}

		public void TestWarehouseAutomationIsDoneOnAmendmentForInward()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var whsDataHelper = new WhsDataTestHelper(Factory);

			using (whsDataHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (whsDataHelper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var department = Factory.New<GlbDepartment>();
				department.GE_Code = "234";
				department.GE_Warehouse = true;
				department.GE_CustomsBrokerage = true;

				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				var importer = (IOrgHeader)helper.CreateClient("IMP234", "WHTest");
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.MainAddress.OA_Address1 = "ADDRESS 1";

				var warehouseOrg = Factory.New<OrgHeader>();
				warehouseOrg.OH_Code = "W1";
				warehouseOrg.MainAddress.LocalControlledPremisesID = "23423";

				var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "BOND");
				warehouse.WW_WarehouseName = warehouseOrg.MainAddress.OA_Address1;
				warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;

				var part = Factory.New<Business.OrgSupplierPart>();
				part.OP_PartNum = "~~1";
				part.OP_StockKeepingUnit = "NO";
				part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				var classification = Factory.New<BaseCusClassification>();
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				classification.CC_LookupCode = "~~1L";
				classification.CC_TariffNum = "0000000000";

				var pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				inwardInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine.JI_InvoiceUQ = "NO";
				inwardInvoiceLine.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine.JI_CustomsQuantity = 10m;
				inwardInvoiceLine.JI_LinePrice = 1000m;
				inwardInvoiceLine.JI_CL = inwardEntryLine.PK;
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = department.PK;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);
				AssertEquals("inwardDeclaration.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);

				var outwardDeclaration = Factory.New<BaseJobDeclaration>();
				outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				outwardDeclaration.JE_OH_Importer = importer.PK;
				outwardDeclaration.JE_DeclarationReference = "B00000125";
				outwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
				outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders.AddNew();
				outwardEntry.CH_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardEntry.EntryNumber = "ENT2343";
				var outwardEntryLine = outwardEntry.MergedLines.AddNew();
				outwardEntryLine.CL_LineNumber = 1;
				outwardDeclaration.Invoices.DeleteAll();
				var outwardInvoice = outwardDeclaration.Invoices.AddNew();
				outwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				outwardInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				outwardInvoice.JZ_InvoiceAmount = 600m;

				var outwardInvoiceLine = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				outwardInvoiceLine.JI_InvoiceQuantity = 60m;
				outwardInvoiceLine.JI_CL = outwardEntryLine.PK;
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT2343*WRL=1";
				job = new JobHeader.Loader(outwardDeclaration).TryCreate();
				job.JH_GE = department.PK;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);

				inwardInvoiceLine.JI_InvoiceQuantity = 110m;
				mockManager.Setup(m => m.TopLevelBusinessObject).Returns(inwardDeclaration);
				var info = new RequiredMessagesInformation(bizObj);
				MakeRequiredMessagesInfoToHaveMessagesToSend(info);
				mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
				mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
				mockManager.Setup(m => m.DeferredAmendmentTillAfterSaveSuccessful).Returns(true);
				var messageGenerationCollection = new MessageGenerationResultCollection();
				messageGenerationCollection.Add("IMP", 1, Array.Empty<Messaging.Business.EDIMessage>());
				mockManager.Setup(m => m.SendAnyMessagesRequired(info)).Returns(messageGenerationCollection);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				inwardDeclaration.MessageInitiator = controller;
				var result = controller.DetermineRequiredMessagesAndSendThem(manager);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);
				AssertEquals("inwardDeclaration.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);
				inwardDeclaration.HasChangesChanged += declaration_HasChangesChanged;
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 0m);
				AssertEquals("inwardDeclaration.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, inwardDeclaration.WarehouseTransactionStatus);
			}
		}

		public void TestWarehouseAutomationIsDoneOnAmendment()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var whsDataHelper = new WhsDataTestHelper(Factory);

			using (whsDataHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (whsDataHelper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var department = Factory.New<GlbDepartment>();
				department.GE_Code = "234";
				department.GE_Warehouse = true;
				department.GE_CustomsBrokerage = true;

				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				var importer = (IOrgHeader)helper.CreateClient("IMP234", "WHTest");
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.MainAddress.OA_Address1 = "ADDRESS 1";

				var warehouseOrg = Factory.New<OrgHeader>();
				warehouseOrg.OH_Code = "W1";
				warehouseOrg.MainAddress.LocalControlledPremisesID = "23423";

				var warehouse = (IWhsWarehouse)helper.CreateWarehouse(warehouseOrg.MainAddress.OA_Address1, "WHS", "BOND");
				warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;

				var part = Factory.New<Business.OrgSupplierPart>();
				part.OP_PartNum = "~~1";
				part.OP_StockKeepingUnit = "NO";
				part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				var classification = Factory.New<BaseCusClassification>();
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				classification.CC_LookupCode = "~~1L";
				classification.CC_TariffNum = "0000000000";

				var pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				inwardInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine.JI_InvoiceUQ = "NO";
				inwardInvoiceLine.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine.JI_CustomsQuantity = 10m;
				inwardInvoiceLine.JI_LinePrice = 1000m;
				inwardInvoiceLine.JI_CL = inwardEntryLine.PK;
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = department.PK;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);

				var outwardDeclaration = Factory.New<BaseJobDeclaration>();
				outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				outwardDeclaration.JE_OH_Importer = importer.PK;
				outwardDeclaration.JE_DeclarationReference = "B00000125";
				outwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
				outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders.AddNew();
				outwardEntry.CH_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardEntry.EntryNumber = "ENT2343";
				var outwardEntryLine = outwardEntry.MergedLines.AddNew();
				outwardEntryLine.CL_LineNumber = 1;
				outwardDeclaration.Invoices.DeleteAll();
				var outwardInvoice = outwardDeclaration.Invoices.AddNew();
				outwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				outwardInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				outwardInvoice.JZ_InvoiceAmount = 600m;

				var outwardInvoiceLine = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				outwardInvoiceLine.JI_InvoiceQuantity = 60m;
				outwardInvoiceLine.JI_CL = outwardEntryLine.PK;
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT2343*WRL=1";
				job = new JobHeader.Loader(outwardDeclaration).TryCreate();
				job.JH_GE = department.PK;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);

				outwardInvoiceLine.JI_InvoiceQuantity = 70m;
				mockManager.Setup(m => m.TopLevelBusinessObject).Returns(outwardDeclaration);
				var info = new RequiredMessagesInformation(bizObj);
				MakeRequiredMessagesInfoToHaveMessagesToSend(info);
				mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
				mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
				mockManager.Setup(m => m.DeferredAmendmentTillAfterSaveSuccessful).Returns(true);
				var messageGenerationCollection = new MessageGenerationResultCollection();
				messageGenerationCollection.Add("IMP", 1, Array.Empty<Messaging.Business.EDIMessage>());
				mockManager.Setup(m => m.SendAnyMessagesRequired(info)).Returns(messageGenerationCollection);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				outwardDeclaration.MessageInitiator = controller;
				var result = controller.DetermineRequiredMessagesAndSendThem(manager);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);
				outwardDeclaration.HasChangesChanged += declaration_HasChangesChanged;
				AssertEquals("outwardDeclaration.WarehouseTransactionStatus is OutwardCreatedPending and will not process", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);
			}
		}

		public void TestWarehouseAutomationIsDoneOnAmendment_NotToReportHasChanges()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var outwardDeclaration = Factory.New<JobDeclarationForTest>();
			outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			mockManager.Setup(m => m.TopLevelBusinessObject).Returns(outwardDeclaration);
			var info = new RequiredMessagesInformation(bizObj);
			MakeRequiredMessagesInfoToHaveMessagesToSend(info);
			mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
			mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
			mockManager.Setup(m => m.DeferredAmendmentTillAfterSaveSuccessful).Returns(true);
			var messageGenerationCollection = new MessageGenerationResultCollection();
			messageGenerationCollection.Add("IMP", 1, Array.Empty<Messaging.Business.EDIMessage>());
			mockManager.Setup(m => m.SendAnyMessagesRequired(info)).Returns(messageGenerationCollection);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			outwardDeclaration.MessageInitiator = controller;
			var result = controller.DetermineRequiredMessagesAndSendThem(manager);

			AssertNoExceptionThrown(() =>
			{
				Factory.Save();
			});
		}

		public void TestMessageStatusAmendmentIsUpdatedIfWarehouseAutomationIsFails()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var whsDataHelper = new WhsDataTestHelper(Factory);

			using (whsDataHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (whsDataHelper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var department = Factory.New<GlbDepartment>();
				department.GE_Code = "234";
				department.GE_Warehouse = true;
				department.GE_CustomsBrokerage = true;

				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				var importer = (IOrgHeader)helper.CreateClient("IMP234", "WHTest");
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.MainAddress.OA_Address1 = "ADDRESS 1";

				var warehouseOrg = Factory.New<OrgHeader>();
				warehouseOrg.OH_Code = "W1";
				warehouseOrg.MainAddress.LocalControlledPremisesID = "23423";

				var warehouse = (IWhsWarehouse)helper.CreateWarehouse(warehouseOrg.MainAddress.OA_Address1, "WHS", "BOND");
				warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;

				var part = Factory.New<Business.OrgSupplierPart>();
				part.OP_PartNum = "~~1";
				part.OP_StockKeepingUnit = "NO";
				part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				var classification = Factory.New<BaseCusClassification>();
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				classification.CC_LookupCode = "~~1L";
				classification.CC_TariffNum = "0000000000";

				var pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;

				var part1 = Factory.New<Business.OrgSupplierPart>();
				part1.OP_PartNum = "~~2";
				part1.OP_StockKeepingUnit = "NO";
				part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part1.PK;

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				inwardInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine.JI_InvoiceUQ = "NO";
				inwardInvoiceLine.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine.JI_CustomsQuantity = 10m;
				inwardInvoiceLine.JI_LinePrice = 1000m;
				inwardInvoiceLine.JI_CL = inwardEntryLine.PK;
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = department.PK;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);

				var outwardDeclaration = Factory.New<BaseJobDeclaration>();
				outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				outwardDeclaration.JE_OH_Importer = importer.PK;
				outwardDeclaration.JE_DeclarationReference = "B00000125";
				outwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
				outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders.AddNew();
				outwardEntry.CH_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardEntry.EntryNumber = "ENT2343";
				var outwardEntryLine = outwardEntry.MergedLines.AddNew();
				outwardEntryLine.CL_LineNumber = 1;
				outwardDeclaration.Invoices.DeleteAll();
				var outwardInvoice = outwardDeclaration.Invoices.AddNew();
				outwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				outwardInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				outwardInvoice.JZ_InvoiceAmount = 600m;

				var outwardInvoiceLine = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				outwardInvoiceLine.JI_InvoiceQuantity = 60m;
				outwardInvoiceLine.JI_CL = outwardEntryLine.PK;
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT2343*WRL=1";
				job = new JobHeader.Loader(outwardDeclaration).TryCreate();
				job.JH_GE = department.PK;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward();
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);

				outwardInvoiceLine.JI_PartNo = part1.OP_PartNum;
				mockManager.Setup(m => m.TopLevelBusinessObject).Returns(outwardDeclaration);
				bizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;
				var info = new RequiredMessagesInformation(bizObj);
				info.AmendmentWithdrawalReason.ReasonText = "HELLO WORLD";
				MakeRequiredMessagesInfoToHaveMessagesToSend(info);
				mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
				mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
				mockManager.Setup(m => m.DeferredAmendmentTillAfterSaveSuccessful).Returns(true);
				var savingOptions = new DeferredAmendmentSavingOptions();
				mockManager.Setup(m => m.GetDeferredAmendmentSavingOptions()).Returns(savingOptions);

				var lastAmendmentReason = ZString.Empty;
				var messageGenerationCollection = new MessageGenerationResultCollection();
				messageGenerationCollection.Add("IMP", 1, Array.Empty<Messaging.Business.EDIMessage>());
				mockManager.Setup(m => m.SendAnyMessagesRequired(info)).Returns(messageGenerationCollection);
				mockManager.Setup(m => m.ProcessWhenChangesAreSavedWithoutSending(savingOptions, info))
					.Callback<IDeferredAmendmentSavingOptions, RequiredMessagesInformation>((s, i) => lastAmendmentReason = i.AmendmentWithdrawalReason.ReasonText);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				outwardDeclaration.MessageInitiator = controller;
				AssertEquals(false, savingOptions.SaveWithEntryChanges);
				var result = controller.DetermineRequiredMessagesAndSendThem(manager);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);
				AssertEquals(false, savingOptions.SaveWithEntryChanges);
				AssertEquals(ZString.Empty, lastAmendmentReason);
				outwardDeclaration.HasChangesChanged += declaration_HasChangesChanged;
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);
				AssertEquals(true, savingOptions.SaveWithEntryChanges);
				AssertEquals("HELLO WORLD", lastAmendmentReason);
				AssertEquals(false, bizObj.HasChanges);
			}
		}

		public void TestCheckSecurityRightWhenSendMessages()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "IMP234";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.MainAddress.OA_Address1 = "ADDRESS 1";

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_TransportMode = "AIR";
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_DeclarationReference = "B00000125";
				declaration.SetSupportsBondedWarehousingForTesting(true);

				mockManager.Setup(m => m.TopLevelBusinessObject).Returns(declaration);
				bizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;
				var info = new RequiredMessagesInformation(bizObj);
				info.AmendmentWithdrawalReason.ReasonText = "HELLO WORLD";
				MakeRequiredMessagesInfoToHaveMessagesToSend(info);
				mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(info);
				mockManager.Setup(m => m.CheckBusinessObjectLevelValidationIfRequired()).Returns(new MessageSendingNotificationCollection());
				mockManager.Setup(m => m.DeferredAmendmentTillAfterSaveSuccessful).Returns(true);
				var savingOptions = new DeferredAmendmentSavingOptions();
				mockManager.Setup(m => m.GetDeferredAmendmentSavingOptions()).Returns(savingOptions);

				var newStaff = Factory.New<IGlbStaff>();
				newStaff.GS_Code = "TUS";
				newStaff.GS_LoginName = "Test.User";
				newStaff.GS_FullName = "Test User";

				var securityA = Factory.New<IGlbSecurity>();
				securityA.GU_GS = newStaff.PK;
				securityA.GU_GC = EnvProxy.Instance.CurrentCompany.PK;
				securityA.GU_SecurityRight = Environment.Env.Security.CustomsDeclarationLodgement.Code;
				securityA.GU_SecurityItemIsAllowed = ZBool.False;

				var securityB = Factory.New<IGlbSecurity>();
				securityB.GU_GS = newStaff.PK;
				securityB.GU_GC = EnvProxy.Instance.CurrentCompany.PK;
				securityB.GU_SecurityRight = Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.Code;
				securityB.GU_SecurityItemIsAllowed = ZBool.False;

				Factory.Save();

				using (EnvProxy.Instance.SetTemporaryUserContext(newStaff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					declaration.MessageInitiator = controller;
					var result = controller.DetermineRequiredMessagesAndSendThem(manager);
					AssertEquals("Error " + Environment.Env.Security.CustomsDeclarationLodgement.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(ContinueWithSave.No, result);
				}

				securityA.GU_SecurityItemIsAllowed = ZBool.True;
				Factory.Save();

				using (EnvProxy.Instance.SetTemporaryUserContext(newStaff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var result = controller.DetermineRequiredMessagesAndSendThem(manager);
					AssertEquals("Error " + Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(ContinueWithSave.No, result);
				}

				securityB.GU_SecurityItemIsAllowed = ZBool.True;
				Factory.Save();

				using (EnvProxy.Instance.SetTemporaryUserContext(newStaff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var result = controller.DetermineRequiredMessagesAndSendThem(manager);
					AssertEquals(ContinueWithSave.Yes, result);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			bizObj = Factory.New<MessageManageableBusinessObject>();
			mockManager = new Mock<IMessageManager>();
			mockManager.Setup(m => m.GetCriticalErrorsForCanSaveExcludingMessagingLevelNotifications(It.IsAny<RequiredMessagesInformation>())).Returns("");
			manager = mockManager.Object;
			controller = new TestController();
			singleManager = new TestHelperSingleMessageManager(bizObj, "apple");
		}

		void MakeRequiredMessagesInfoToHaveMessagesToSend(RequiredMessagesInformation infoWithMessagesToSend)
		{
			infoWithMessagesToSend.AddAmendmentBridgeForTesting(new SingleMessageManager[] { singleManager });
		}

		void declaration_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (((IBusinessObjectFactoryInternals)((BusinessObject)sender).Factory).IsProcessingOnAllTransactionsCommitted && e.ObjectJustWasChanged)
			{
				throw new Exception("HasChanges should not be set after factory transaction is committed");
			}
		}

		readonly ZBlob notesTestBlob = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Some text"));
		readonly ZBlob emptyBlob = ZBlob.Empty;
		ForwardingShipmentProcessTask exceptionProcessTask1;
		ForwardingShipmentProcessTask exceptionProcessTask2;
		TestController controller;
		Mock<IMessageManager> mockManager;
		MessageManageableBusinessObject bizObj;
		TestHelperSingleMessageManager singleManager;
		IMessageManager manager;

		void RunOneOverdueCargoReportExceptionTest(ForwardingShipmentProcessTask exceptionTaskToReturn1, ForwardingShipmentProcessTask exceptionTaskToReturn2, DialogResult dialogResult,
			ZInt numMessagesToSend, bool exception1Actioned, bool exception2Actioned, ZBlob exception1NotesBlob, ZBlob exception2NotesBlob)
		{
			exceptionProcessTask1.IsExceptionActioned = false;
			exceptionProcessTask2.IsExceptionActioned = false;
			exceptionProcessTask2.P9_Notes = emptyBlob;
			Factory.Save();

			var mockManager1 = new Mock<SingleMessageManager>();
			SingleMessageManager manager1 = mockManager1.Object;
			mockManager1.Setup(m => m.OverdueCargoReportException).Returns(exceptionTaskToReturn1);

			var mockManager2 = new Mock<SingleMessageManager>();
			SingleMessageManager manager2 = mockManager2.Object;
			mockManager2.Setup(m => m.OverdueCargoReportException).Returns(exceptionTaskToReturn2);

			ZFormModaliser.ResultToReturnFromShowDialog = dialogResult;

			Sender.CheckAllBoxes = true;
			Sender.ClickSend = true;
			SingleMessageManager[] result = Sender.WhichMessagesShouldWeSend(new SingleMessageManager[] { manager1, manager2 });

			AssertEquals("Should correct number of messages to send", numMessagesToSend, result.Length);
			AssertEquals("exception1 actioned", exception1Actioned, exceptionProcessTask1.IsExceptionActioned);
			AssertEquals("exception2 actioned", exception2Actioned, exceptionProcessTask2.IsExceptionActioned);
			AssertEquals("exception1 notes", exception1NotesBlob, exceptionProcessTask1.P9_Notes);
			AssertEquals("exception2 notes", exception2NotesBlob, exceptionProcessTask2.P9_Notes);
		}

		void TestWhichMessagesShouldWeSend(bool checkAllBoxes, bool clickSend)
		{
			Sender.CheckAllBoxes = checkAllBoxes;
			Sender.ClickSend = clickSend;
			SingleMessageManager[] result = Sender.WhichMessagesShouldWeSend(new SingleMessageManager[] { new TestHelperSingleMessageManager() });
			AssertEquals("Result.Length", (checkAllBoxes && clickSend) ? 1 : 0, result.Length);
		}

		void TestWhichMessagesShouldWeWithdraw(bool checkAllBoxes, bool clickSend)
		{
			Sender.CheckAllBoxes = checkAllBoxes;
			Sender.ClickSend = clickSend;
			SingleMessageManager[] result = Sender.WhichMessagesShouldWeWithdraw(new SingleMessageManager[] { new TestHelperSingleMessageManager() });
			AssertEquals("Result.Length", (checkAllBoxes && clickSend) ? 1 : 0, result.Length);
		}

		TestHelperSendsMessagesToCustomsGUI sender;
		TestHelperSendsMessagesToCustomsGUI Sender => sender ?? (sender = new TestHelperSendsMessagesToCustomsGUI());

		sealed class JobDeclarationForTest : BaseJobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaved(bool saveSucceeded)
			{
				base.OnSaved(saveSucceeded);
				JE_DeclarationReference = "VWGTest";
			}
		}

		sealed class TestController : SendsMessagesToCustomsGUI
		{
			public ContinueWithSave AdditionalActionResultExposed = ContinueWithSave.Yes;
			protected override ContinueWithSave DoActionsBeforeSendingRequiredMessages(IMessageManager manager, RequiredMessagesInformation detectionResult)
			{
				return AdditionalActionResultExposed;
			}
		}
	}

	public class TestHelperSendsMessagesToCustomsGUI : SendsMessagesToCustomsGUI
	{
		protected override void ShowMessagesToSendDialogWithoutDispose(MessageChooserNonPersistent chooser, MessagesChooserDialog dialog)
		{
			dialog.Show();
			for (int i = 0; i < dialog.MessagesCheckedListBox.Items.Count; i++)
			{
				if (CheckAllBoxes)
				{
					dialog.MessagesCheckedListBox.SetItemCheckState(i, CheckState.Checked);
				}
			}
			if (ClickSend)
			{
				dialog.SendButton.PerformClick();
			}
		}

		public bool CheckAllBoxes;
		public bool ClickSend;
	}
}
