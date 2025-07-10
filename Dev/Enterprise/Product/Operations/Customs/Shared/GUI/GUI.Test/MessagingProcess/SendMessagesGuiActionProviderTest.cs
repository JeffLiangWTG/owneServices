using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.MessagingProcess.Testing
{
	sealed class SendMessagesGuiActionProviderTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullConstructor()
		{
			_ = new SendMessagesGuiActionProvider(null);
		}

		public void TestShowPreValidationNotifications()
		{
			var prev = new ActionResult(true);
			prev.AppendInformationNotification("Hello World");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

			var actionInterface = CreateGuiProvider(typeof(CustomsMessagingGuiImplForTest));
			var result = actionInterface.ShowPreSendValidationNotifications(prev);
			var lastNotification = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("User Continued", true, result.Success);
			AssertEquals("Test Message", "Please review the following notifications:\n\nHello World\n\nDo you wish to continue?", lastNotification.Text);
			AssertEquals("Notifications cleared", 0, result.Notifications.Count);
			AssertEquals("Previous notifications", 1, result.PreviousNotifications.Count);

			prev.AppendInformationNotification("Hello World");
			prev.AppendWarningNotification("Goodbye Universe");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);

			result = actionInterface.ShowPreSendValidationNotifications(prev);
			lastNotification = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("User stopped", false, result.Success);
			AssertEquals("Test Message", "Please review the following notifications:\n\nWarnings:\r\nGoodbye Universe\r\nInformation:\r\nHello World\n\nDo you wish to continue?", lastNotification.Text);
		}

		public void TestShowFormPreSaveDialog()
		{
			var actionInterface = CreateGuiProvider(typeof(CustomsMessagingGuiImplForTest));
			var prev = new ActionResult(true);
			var result = actionInterface.ShowFormPreSaveDialog(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("No Notifications", 0, result.Notifications?.Count ?? 0);

			topLevelBO.Z0_Description = "Save should be required";
			prev = new ActionResult(true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);

			prev = new ActionResult(true);
			result = actionInterface.ShowFormPreSaveDialog(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notification", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Notifcation content", CustomsMessagingGuiExtensions.PreSaveDialogCancelledMessage, result.Notifications[0].Message);

			prev = new ActionResult(true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

			result = actionInterface.ShowFormPreSaveDialog(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("0 Notifications", 0, result.Notifications?.Count ?? 0);
		}

		public void TestShowSendDialog()
		{
			var customsMessagingGui = new CustomsMessagingGuiWithSendDialogSupportImplForTest(customsMessagingSupporter, mainForm);
			var actionInterface = new SendMessagesGuiActionProvider(customsMessagingGui) as ISendMessagesGuiActionProvider;

			AssertDialog("Send cancelled", actionInterface.ShowSendDialog, (f) => customsMessagingGui.SendDialog = f, () => customsMessagingGui.SendDialogCancelledMessage = null, CustomsMessagingGuiExtensions.SendDialogCancelledMessage);
		}

		public void TestShowPreviewDialog()
		{
			var customsMessagingGui = new CustomsMessagingGuiWithPreviewDialogSupportImplForTest(customsMessagingSupporter, mainForm);
			var actionInterface = new SendMessagesGuiActionProvider(customsMessagingGui) as ISendMessagesGuiActionProvider;

			AssertDialog("Preview cancelled", actionInterface.ShowPreviewDialog, (f) => customsMessagingGui.PreviewDialog = f, () => customsMessagingGui.PreviewDialogCancelledMessage = null, CustomsMessagingGuiExtensions.PreviewDialogCancelledMessage);
		}

		public void TestShowResultNotifications()
		{
			var actionInterface = CreateGuiProvider(typeof(CustomsMessagingGuiImplForTest));
			var prev = new ActionResult(true);
			prev.AppendInformationNotification("Hello World");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK);

			var result = actionInterface.ShowResultNotifications(prev);
			var lastNotification = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Result remains", true, result.Success);
			AssertEquals("Test Message", "Hello World", lastNotification.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK);
			prev.AppendErrorNotification("Only show this error");

			result = actionInterface.ShowResultNotifications(prev);
			lastNotification = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Result remains", true, result.Success);
			AssertEquals("Test Message", "Only show this error", lastNotification.Text);
		}

		void AssertDialog(string cancelNotification, ActionStep actionToPerform, Action<IDialog> actionToSetDialog, Action clearCancelNotificaitonAction, string defaultCancelNotification)
		{
			mainForm.Show();

			var prev = new ActionResult(true);
			var result = actionToPerform(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("No Notifications", 0, result.Notifications?.Count ?? 0);

			var dlg = new Dialog(topLevelBO, typeof(ValidDialogTestForm));
			actionToSetDialog(dlg);

			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.Cancel;

			prev = new ActionResult(true);
			result = actionToPerform(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Cancel Notification content", cancelNotification, result.Notifications[0].Message);

			clearCancelNotificaitonAction();
			prev = new ActionResult(true);
			result = actionToPerform(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Custom Cancel Notification content", defaultCancelNotification, result.Notifications[0].Message);

			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

			prev = new ActionResult(true);
			result = actionToPerform(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("0 Notifications", 0, result.Notifications?.Count ?? 0);

			mainForm.Close();
		}

		public void TestInvalidDialogTypeUsed()
		{
			var customsMessagingGui = new CustomsMessagingGuiWithPreviewDialogSupportImplForTest(customsMessagingSupporter, mainForm);
			var invalidDialog = new Dialog(topLevelBO, typeof(InvalidDialogTestForm));
			customsMessagingGui.PreviewDialog = invalidDialog;
			var actionInterface = new SendMessagesGuiActionProvider(customsMessagingGui) as ISendMessagesGuiActionProvider;

			AssertExceptionThrown<DeveloperNotificationException>("Expect Developer exception", () => actionInterface.ShowPreviewDialog(new ActionResult(true)));
		}

		public void TestConfigureProcess()
		{
			var sendProc = new SendMessagesProcess();
			var sendChain = sendProc.SendProcessChain;

			var messagingGui = new CustomsMessagingGuiWithConfigureProcessSupportImplForTest(customsMessagingSupporter, mainForm);
			var provider = new SendMessagesGuiActionProvider(messagingGui) as ISendMessagesGuiActionProvider;

			messagingGui.ConfigProcessForTesting = (chain) => chain.FindAction("CreateMessages").InsertActionAfter("FromGui", null, ActionLink.Success);

			provider.ConfigureProcess(sendChain);

			AssertContains("New step in chain", "success: { CreateMessages: success: { FromGui:", sendChain.GetChainAsString());
		}

		public void TestShowSendMessagesWithErrorsSecurityCheckpointOverride()
		{
			var overrideCheckpoint = Env.Security.AllowMessageErrors;

			var user = GlbStaff.CurrentUser;
			user.GS_IsController = false;

			var super = Factory.NewWithValidTestData<GlbStaff>();
			super.GS_Code = "SUP";
			super.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;

			var secUser = Factory.New<GlbSecurity>();
			secUser.GU_SecurityRight = overrideCheckpoint.Code;
			secUser.GU_SecurityItemIsAllowed = false;
			secUser.GU_GS = user.PK;
			secUser.GU_GB = GlbBranch.CurrentBranch.PK;

			var secSuper = Factory.New<GlbSecurity>();
			secSuper.GU_SecurityRight = overrideCheckpoint.Code;
			secSuper.GU_SecurityItemIsAllowed = true;
			secSuper.GU_GS = super.PK;
			secSuper.GU_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var bo = Factory.New<BaseJobDeclaration>();
			var boProvider = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger });
			var boSupporter = new CustomsMessagingSupporter(bo, new CustomsMessagingProviderFactoryImplForTest(boProvider));
			var boGui = new CustomsMessagingGuiImplForTest(boSupporter, mainForm);
			ISendMessagesGuiActionProvider guiActionProvider = new SendMessagesGuiActionProvider(boGui);

			mainForm.Show();

			overrideCheckpoint.IsAllowed = false;
			boProvider.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = overrideCheckpoint;

			var result = guiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride(new ActionResult(true));

			AssertEquals("No message errors, no problem", true, result.Success);
			AssertEquals("No errors no Notifications", 0, result.Notifications.Count);

			boProvider.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = null;
			result = guiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride(new ActionResult(true, new[] { new MessageSendingWarning(MessageSendingValidation.MessageErrorsExistHeaderText) }));

			AssertEquals("Override not defined", true, result.Success);
			AssertEquals("Override not defined Notification count", 1, result.Notifications.Count);

			boProvider.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = overrideCheckpoint;
			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.Cancel;

			result = guiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride(new ActionResult(true, new[] { new MessageSendingWarning(MessageSendingValidation.MessageErrorsExistHeaderText) }));

			AssertEquals("Override cancelled", false, result.Success);
			AssertEquals("Override cancelled Notification count", 2, result.Notifications.Count);
			AssertEquals("Override cancelled notification message", "Supervisor override not completed for sending messages with errors", result.Notifications[1].Message);

			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

			result = guiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride(new ActionResult(true, new[] { new MessageSendingWarning(MessageSendingValidation.MessageErrorsExistHeaderText) }));

			AssertEquals("Override completed", true, result.Success);
			AssertEquals("Override completed Notification count", 1, result.Notifications.Count);

			customsMessagingProvider.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = overrideCheckpoint;
			var actionInterface = CreateGuiProvider(typeof(CustomsMessagingGuiImplForTest));
			var ex = AssertExceptionThrown<DeveloperNotificationException>(() => actionInterface.ShowSendMessagesWithErrorsSecurityCheckpointOverride(new ActionResult(true, new[] { new MessageSendingWarning(MessageSendingValidation.MessageErrorsExistHeaderText) })));
			AssertEquals("Unsupported BO TypeException Msg", "Top Level business object should be 'EnterpriseBusinessObject' to allow support of Logs for Supervisor Overrides", ex.Message);

			mainForm.Close();
		}

		public void TestShowCreditAndDPSCheckOverride()
		{
			var docObj = Factory.New<CreditControlledDocumentDeliveryImplForTest>();

			customsMessagingProvider.DocumentDeliveryObjectForTesting = docObj;

			var customsMessagingGui = new CustomsMessagingGuiImplForTest(customsMessagingSupporter, mainForm);
			ISendMessagesGuiActionProvider guiActionProvider = new SendMessagesGuiActionProvider(customsMessagingGui);

			var result = guiActionProvider.ShowCreditAndDPSCheckOverride(new ActionResult(true));
			AssertEquals("No CreditCheckResult, no problem", true, result.Success);

			var creditCheckResult = CreditCheckAndDPSHelper.RunCheck(docObj, false, "Because I want to");
			CombineAssertions("Pre-reqs - Clean credit check", () =>
			{
				AssertEquals("Allowed", true, creditCheckResult.IsAllowedToProceed);
				AssertContains("Message", string.Empty, creditCheckResult.Message);
			});

			var actionResult = new ActionResult(true);
			actionResult.PassThroughData = creditCheckResult;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			result = guiActionProvider.ShowCreditAndDPSCheckOverride(actionResult);
			AssertEquals("Clean CreditCheckResult, no problem", true, result.Success);
			Assert("No notification displayed", UnitTestUserNotification.Instance.LastMessage.WasNone);

			docObj.IsDPSFreightMovementRestrictedForTesting = true;
			creditCheckResult = CreditCheckAndDPSHelper.RunCheck(docObj, false, "Because I want to");

			CombineAssertions("Pre-reqs - Bad credit/DPS", () =>
			{
				AssertEquals("Not allowed", false, creditCheckResult.IsAllowedToProceed);
				AssertContains("Message Line1", "Delivery of this message is restricted because:", creditCheckResult.Message);
				AssertContains("Message Line2", "Screening Status is not Clear", creditCheckResult.Message);
			});

			actionResult = new ActionResult(true);
			actionResult.PassThroughData = creditCheckResult;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Cancel);

			result = guiActionProvider.ShowCreditAndDPSCheckOverride(actionResult);
			AssertEquals("Bad Credit Check cancelled", false, result.Success);
			AssertEquals("Cancel Message", "Error: Unable to submit message due to Denied Party Screening cancellation.", result.Notifications[0].MessageIncludingPrefix);

			creditCheckResult = CreditCheckAndDPSHelper.RunCheck(docObj, false, "Because I still want to");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK); // "Warning - Denied Party Screening" - "Warning – Submitting a Customs Declaration to Customs..."
			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.Yes; // Login Dialog

			actionResult = new ActionResult(true);
			actionResult.PassThroughData = creditCheckResult;
			result = guiActionProvider.ShowCreditAndDPSCheckOverride(actionResult);
			AssertEquals("Bad Credit Check overridden", true, result.Success);
			AssertEquals("No Messages", 0, result.Notifications.Count);
		}

		ISendMessagesGuiActionProvider CreateGuiProvider(Type typeOfGuiImplForTest)
		{
			var customsMessagingGui = Activator.CreateInstance(typeOfGuiImplForTest, customsMessagingSupporter, mainForm) as ICustomsMessagingGui;
			return new SendMessagesGuiActionProvider(customsMessagingGui);
		}

		protected override void SetUp()
		{
			base.SetUp();

			topLevelBO = Factory.New<DummyBizObjWithMessages>();
			childBO1 = Factory.New<DummyBizObjWithMessages>();
			customsMessenger = new CustomsMessengerImplForTest(childBO1);
			customsMessagingProvider = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger });
			customsMessagingSupporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(customsMessagingProvider));

			mainForm = new ZForm(topLevelBO);
		}

		protected override void TearDown()
		{
			base.TearDown();
			mainForm?.Dispose();
		}

		DummyBizObjWithMessages topLevelBO;
		DummyBizObjWithMessages childBO1;
		CustomsMessagingProviderAllImplForTest customsMessagingProvider;
		CustomsMessagingSupporter customsMessagingSupporter;
		CustomsMessengerImplForTest customsMessenger;

		ZForm mainForm;
	}
}
