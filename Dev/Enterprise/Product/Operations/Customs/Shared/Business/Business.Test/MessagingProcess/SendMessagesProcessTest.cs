using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	class SendMessagesProcessTest : TestCase
	{
		public void TestEmptyProcess()
		{
			var result = SendMessagesProcess.SendMessages();

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", true, result.Success);
				AssertEquals("No Notifications", 0, result.Notifications.Count);
			});
		}

		public void TestSendProcess()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", true, result.Success);
				AssertEquals("Actions taken", 15, actionsBus.StepsTaken + actionsGui.StepsTaken);

				var expected = new ZString[]
				{
					nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendDialog),
					nameof(ISendMessagesBusinessActionProvider.PreSendValidation),
					nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride),
					nameof(ISendMessagesBusinessActionProvider.CreditAndDPSCheck),
					nameof(ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride),
					nameof(ISendMessagesBusinessActionProvider.CreateMessages),
					nameof(ISendMessagesGuiActionProvider.ShowPreviewDialog),
					nameof(ISendMessagesBusinessActionProvider.SignMessages),
					nameof(ISendMessagesBusinessActionProvider.ProcessUpdates),
					nameof(ISendMessagesGuiActionProvider.ShowResultNotifications),
					nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}

		public void TestSendPartial()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest(false); // SignMessages will be excluded
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", true, result.Success);
				AssertEquals("Actions taken", 14, actionsBus.StepsTaken + actionsGui.StepsTaken);

				var expected = new ZString[]
				{
					nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendDialog),
					nameof(ISendMessagesBusinessActionProvider.PreSendValidation),
					nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride),
					nameof(ISendMessagesBusinessActionProvider.CreditAndDPSCheck),
					nameof(ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride),
					nameof(ISendMessagesBusinessActionProvider.CreateMessages),
					nameof(ISendMessagesGuiActionProvider.ShowPreviewDialog),
					nameof(ISendMessagesBusinessActionProvider.ProcessUpdates),
					nameof(ISendMessagesGuiActionProvider.ShowResultNotifications),
					nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}

		public void TestPartialFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsBus.FailureSteps = nameof(ISendMessagesBusinessActionProvider.CreateMessages);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			CombineAssertions(() =>
			{
				var expected = 12;
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", expected, actionsBus.StepsTaken + actionsGui.StepsTaken);
				AssertEquals("Message Count", expected, result.Notifications.Count);
				AssertEquals("Failure notification", $"{nameof(ISendMessagesBusinessActionProvider.CreateMessages)} Failure", result.Notifications[expected - 3].Message);
			});
		}

		public void TestNoGuiProvider()
		{
			var actions = new SendMessagesBusinessActionProviderImplForTest();
			var result = SendMessagesProcess.SendMessages(bActionProvider: actions);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", true, result.Success);
				AssertEquals("Actions taken", 8, actions.StepsTaken);
				AssertEquals("Message Count", 8, result.Notifications.Count);
				var expected = new ZString[]
				{
					nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint),
					nameof(ISendMessagesBusinessActionProvider.PreSendValidation),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint),
					nameof(ISendMessagesBusinessActionProvider.CreditAndDPSCheck),
					nameof(ISendMessagesBusinessActionProvider.CreateMessages),
					nameof(ISendMessagesBusinessActionProvider.SignMessages),
					nameof(ISendMessagesBusinessActionProvider.ProcessUpdates),
					nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}

		public void TestNoBusinessProvider()
		{
			var actions = new SendMessagesGuiActionProviderImplForTest();
			var result = SendMessagesProcess.SendMessages(gActionProvider: actions);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", true, result.Success);
				AssertEquals("Actions taken", 7, actions.StepsTaken);
				var expected = new ZString[]
				{
					nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog),
					nameof(ISendMessagesGuiActionProvider.ShowSendDialog),
					nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications),
					nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride),
					nameof(ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride),
					nameof(ISendMessagesGuiActionProvider.ShowPreviewDialog),
					nameof(ISendMessagesGuiActionProvider.ShowResultNotifications)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}

		public void TestSignMessageFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsBus.FailureSteps = nameof(ISendMessagesBusinessActionProvider.SignMessages);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			CombineAssertions(() =>
			{
				var expected = 15;
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", expected, actionsBus.StepsTaken + actionsGui.StepsTaken);
				AssertEquals("Message Count", expected, result.Notifications.Count);
				AssertEquals("Failure notification", $"{nameof(ISendMessagesBusinessActionProvider.SignMessages)} Failure", result.Notifications[expected - 4].Message);
				AssertEquals("Failure action", $"{nameof(ISendMessagesBusinessActionProvider.SignMessagesFailure)}", result.Notifications[expected - 3].Message);
			});
		}

		public void TestShowPreviewDialogFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsGui.FailureSteps = nameof(ISendMessagesGuiActionProvider.ShowPreviewDialog);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			CombineAssertions(() =>
			{
				var expected = 14;
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", expected, actionsBus.StepsTaken + actionsGui.StepsTaken);
				AssertEquals("Message Count", expected, result.Notifications.Count);
				AssertEquals("Failure notification", $"{nameof(ISendMessagesGuiActionProvider.ShowPreviewDialog)} Failure", result.Notifications[expected - 4].Message);
				AssertEquals("Failure action", $"{nameof(ISendMessagesBusinessActionProvider.PreviewDialogFailure)}", result.Notifications[expected - 3].Message);
			});
		}

		public void TestShowPreValidationNotificationsFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsGui.FailureSteps = nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			CombineAssertions(() =>
			{
				var expected = 7;
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", expected, actionsBus.StepsTaken + actionsGui.StepsTaken);
				AssertEquals("Message Count", expected, result.Notifications.Count);
				AssertEquals("Failure notification", $"{nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications)} Failure", result.Notifications[expected - 3].Message);
			});
		}

		public void TestSendProcessChain()
		{
			var process = new SendMessagesProcess(null, null);
			var chain = process.SendProcessChain;

			AssertNotNull("SendProcessChain", chain);
			AssertSame("Only created once", chain, process.SendProcessChain);
		}

		public void TestResultProcessChain()
		{
			var process = new SendMessagesProcess(null, null);
			var chain = process.ResultProcessChain;

			AssertNotNull("ResultProcessChain_Exposed", chain);
			AssertSame("Only created once", chain, process.ResultProcessChain);
		}

		public void TestConfigureProcess()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();
			var process = new SendMessagesProcess(actionsBus, actionsGui);
			var sendChain = process.SendProcessChain;

			CombineAssertions(() =>
			{
				AssertContains("Pre-req", "success: { ProcessUpdates: }", sendChain.GetChainAsString());

				actionsBus.ConfigProcessForTesting = (chain) =>
				{
					chain.FindAction("ProcessUpdates").AppendAction("BOFail", null, ActionLink.Failure);
				};
				actionsGui.ConfigProcessForTesting = (chain) =>
				{
					chain.FindAction("BOFail").AppendAction("GUICommon", null, ActionLink.Common);
					chain.FindAction("ProcessUpdates").InsertActionBefore("GUIPreUpdate", null);
				};

				process = new SendMessagesProcess(actionsBus, actionsGui);
				sendChain = process.SendProcessChain;

				var chainAsString = sendChain.GetChainAsString();

				AssertContains("BO Action", "success: { ProcessUpdates: failure: { BOFail:", chainAsString);
				AssertContains("GUI dependent on BO Step being created first", "success: { ProcessUpdates: failure: { BOFail: common: { GUICommon: } }", chainAsString);
				AssertContains("GUI Pre-Update", "success: { GUIPreUpdate: success: { ProcessUpdates: failure:", chainAsString);
			});
		}

		public void TestSendMessagesSecurityCheckpointFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsBus.FailureSteps = nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", 4, actionsBus.StepsTaken + actionsGui.StepsTaken);

				var expected = new ZString[]
				{
					nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog),
					$"{nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint)} Failure",
					nameof(ISendMessagesGuiActionProvider.ShowResultNotifications),
					nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}

		public void TestSendMessagesWithErrorsSecurityCheckpointFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsBus.FailureSteps = nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", 8, actionsBus.StepsTaken + actionsGui.StepsTaken);

				var expected = new ZString[]
				{
					nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendDialog),
					nameof(ISendMessagesBusinessActionProvider.PreSendValidation),
					nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications),
					$"{nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint)} Failure",
					nameof(ISendMessagesGuiActionProvider.ShowResultNotifications),
					nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}

		public void TestShowSendMessagesWithErrorsSecurityCheckpointOverrideFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsGui.FailureSteps = nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", 9, actionsBus.StepsTaken + actionsGui.StepsTaken);

				var expected = new ZString[]
				{
					nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendDialog),
					nameof(ISendMessagesBusinessActionProvider.PreSendValidation),
					nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint),
					$"{nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride)} Failure",
					nameof(ISendMessagesGuiActionProvider.ShowResultNotifications),
					nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}

		public void TestShowCreditAndDPSCheckOverrideFailure()
		{
			var actionsBus = new SendMessagesBusinessActionProviderImplForTest();
			var actionsGui = new SendMessagesGuiActionProviderImplForTest();

			actionsGui.FailureSteps = nameof(ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride);

			var result = SendMessagesProcess.SendMessages(actionsBus, actionsGui);

			AssertNotNull("result", result);
			CombineAssertions(() =>
			{
				AssertEquals("Success", false, result.Success);
				AssertEquals("Actions taken", 11, actionsBus.StepsTaken + actionsGui.StepsTaken);

				var expected = new ZString[]
				{
					nameof(ISendMessagesGuiActionProvider.ShowFormPreSaveDialog),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendDialog),
					nameof(ISendMessagesBusinessActionProvider.PreSendValidation),
					nameof(ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications),
					nameof(ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint),
					nameof(ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride),
					nameof(ISendMessagesBusinessActionProvider.CreditAndDPSCheck),
					$"{nameof(ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride)} Failure",
					nameof(ISendMessagesGuiActionProvider.ShowResultNotifications),
					nameof(ISendMessagesBusinessActionProvider.RefreshBusinessObjects)
				};
				AssertArrayEqualsByElements("Notifications", expected, result.Notifications.Select(x => x.Message).ToArray());
			});
		}
	}
}
