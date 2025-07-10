using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq.Protected;

namespace Enterprise.Customs.GUI.MessagingProcess.Testing
{
	sealed class CustomsMessagingGuiExtensionsTest : TestCaseWithFactory
	{
		public void TestSendMessages()
		{
			var msg1 = Factory.NewMoq<DummyEDIMessage>();
			msg1.Protected().Setup<string>("GetMessageReferenceNumber").Returns("1");
			msg1.Object.EM_MessageText = "Hello World";

			customsMessenger.CreateMessageForTest = msg1.Object;
			customsMessenger.ProcessUpdatesForTest = true;

			var result = customsMessagingGuiForTest.SendMessages();
			CombineAssertions(() =>
			{
				AssertEquals(true, result.Success);
				AssertEquals("Notifications", 1, result.Notifications?.Count ?? 0);
				AssertEquals("Caption", "Sending Messages", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Msg Content", "1 Message(s) queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			customsMessenger.CreateMessageForTest = null;
		}

		internal class DummyEDIMessage : EDIMessage
		{
			public DummyEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			protected override string GetMessageReferenceNumber()
			{
				return "111";
			}
		}

		public void TestShowSummaryNotification()
		{
			var col = new MessageSendingNotificationCollection();
			col.AddInformation("Info 1");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK);

			CombineAssertions(() =>
			{
				var res = CustomsMessagingGuiExtensions.ShowSummaryNotification(col, System.Windows.Forms.MessageBoxButtons.OK, "Captain", "PRE-", "-POST");
				AssertEquals("OK?", System.Windows.Forms.DialogResult.OK, res);
				AssertEquals("Caption", "Captain", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Msg Content 1", "PRE-Info 1-POST", UnitTestUserNotification.Instance.LastMessage.Text);

				col.AddWarning("Warn 1");
				res = CustomsMessagingGuiExtensions.ShowSummaryNotification(col, System.Windows.Forms.MessageBoxButtons.OK, "Captain");
				AssertEquals("Msg Content 2", "Warnings:\r\nWarn 1\r\nInformation:\r\nInfo 1", UnitTestUserNotification.Instance.LastMessage.Text);

				col.AddError("Err 1");
				res = CustomsMessagingGuiExtensions.ShowSummaryNotification(col, System.Windows.Forms.MessageBoxButtons.OK, "Captain");
				AssertEquals("Msg Content 3", "Errors:\r\nErr 1\r\nWarnings:\r\nWarn 1\r\nInformation:\r\nInfo 1", UnitTestUserNotification.Instance.LastMessage.Text);

				res = CustomsMessagingGuiExtensions.ShowSummaryNotification(col, System.Windows.Forms.MessageBoxButtons.OK, "Captain", showErrorsAlone: true);
				AssertEquals("Msg Content Err Only", "Err 1", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestDialogCancellationMessages()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PreSave", "User canceled, send aborted", CustomsMessagingGuiExtensions.PreSaveDialogCancelledMessage);
				AssertEquals("Preview", "User canceled, preview aborted", CustomsMessagingGuiExtensions.PreviewDialogCancelledMessage);
				AssertEquals("Send", "User canceled, send aborted", CustomsMessagingGuiExtensions.SendDialogCancelledMessage);
			});
		}

		public void TestGetSendMessagesGuiActionProvider()
		{
			var msgGui = new CustomsMessagingGuiImplForTest(customsMessagingSupporter, mainForm);

			AssertType<SendMessagesGuiActionProvider>(msgGui.GetSendMessagesGuiActionProvider());
		}

		public void TestConfigureProcess()
		{
			var sendProc = new SendMessagesProcess();
			var sendChain = sendProc.SendProcessChain;

			var preConfigureChain = sendChain.GetChainAsString();

			var messagingGuiNoSupport = new CustomsMessagingGuiImplForTest(customsMessagingSupporter, mainForm);
			var providerNoSupport = new SendMessagesGuiActionProvider(messagingGuiNoSupport) as ISendMessagesGuiActionProvider;

			providerNoSupport.ConfigureProcess(sendChain);
			AssertEquals("No support, no change", preConfigureChain, sendChain.GetChainAsString());

			var messagingGuiWithSupport = new CustomsMessagingGuiWithConfigureProcessSupportImplForTest(customsMessagingSupporter, mainForm);
			var providerWithSupport = new SendMessagesGuiActionProvider(messagingGuiWithSupport) as ISendMessagesGuiActionProvider;

			messagingGuiWithSupport.ConfigProcessForTesting = (chain) => chain.FindAction("CreateMessages").InsertActionAfter("FromGui", null, ActionLink.Success);
			providerWithSupport.ConfigureProcess(sendChain);

			AssertContains("New step in chain", "success: { CreateMessages: success: { FromGui:", sendChain.GetChainAsString());
		}

		public void TestGetPreviewDialogSupporter()
		{
			var messagingGuiNoSupport = new CustomsMessagingGuiImplForTest(customsMessagingSupporter, mainForm) as ICustomsMessagingGui;
			var messagingGuiWithSupport = new CustomsMessagingGuiWithPreviewDialogSupportImplForTest(customsMessagingSupporter, mainForm) as ICustomsMessagingGui;

			CombineAssertions(() =>
			{
				AssertNull("PreviewDialog Not Supported", messagingGuiNoSupport.GetPreviewDialogSupporter());
				AssertNotNull("PreviewDialog supported", messagingGuiWithSupport.GetPreviewDialogSupporter());
			});
		}

		public void TestGetSendDialogSupporter()
		{
			var messagingGuiNoSupport = new CustomsMessagingGuiImplForTest(customsMessagingSupporter, mainForm) as ICustomsMessagingGui;
			var messagingGuiWithSupport = new CustomsMessagingGuiWithSendDialogSupportImplForTest(customsMessagingSupporter, mainForm) as ICustomsMessagingGui;

			CombineAssertions(() =>
			{
				AssertNull("SendDialog Not Supported", messagingGuiNoSupport.GetSendDialogSupporter());
				AssertNotNull("SendDialog supported", messagingGuiWithSupport.GetSendDialogSupporter());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			topLevelBO = Factory.New<DummyBizObjWithMessages>();
			childBO1 = Factory.New<DummyBizObjWithMessages>();
			customsMessenger = new CustomsMessengerImplForTest(childBO1);
			customsMessagingProvider = new CustomsMessagingProviderImplForTest(new[] { customsMessenger });
			customsMessagingSupporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(customsMessagingProvider));
			mainForm = new ZForm(topLevelBO);

			customsMessagingGuiForTest = new CustomsMessagingGuiImplForTest(customsMessagingSupporter, mainForm);
		}

		protected override void TearDown()
		{
			mainForm?.Dispose();
			base.TearDown();
		}

		CustomsMessagingGuiImplForTest customsMessagingGuiForTest;
		DummyBizObjWithMessages topLevelBO;
		DummyBizObjWithMessages childBO1;
		CustomsMessagingProviderImplForTest customsMessagingProvider;
		CustomsMessagingSupporter customsMessagingSupporter;
		CustomsMessengerImplForTest customsMessenger;
		ZForm mainForm;
	}
}
