using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Environment;
using MessageManager = Enterprise.Customs.NO.Business.MessageManager;

namespace Enterprise.Customs.NO.GUI.Testing;

sealed class MessageSendingTest : TestCaseWithFactory
{
	public void TestSendMessageShouldNotUpdateBGMReferenceOnFailedSend()
	{
		const string bgmReferenceWithoutVersion = "11122233320240601000001";
		const string bgmRefVersion1 = $"{bgmReferenceWithoutVersion}01";
		const string bgmRefVersion2 = $"{bgmReferenceWithoutVersion}02";

		var notification = new MessageNotificationCollector();
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		header.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = header.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var messageSendingObjectParent = new MessageSendingObjectParent(declaration);
		var manager = new MessageManager(messageSendingObjectParent, notification);

		CombineAssertions("Successful sending", () =>
		{
			header.CH_BGMReference = bgmRefVersion1;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			manager.SendMessages();

			AssertEquals("Last msg should be an Info message", expected: true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("Last Message", "Create CUSDEC message queued for sending.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			AssertEquals("Header should update the BGMref version", bgmRefVersion2, header.CH_BGMReference);
			AssertEquals("One message should be created", 1, header.Messages.Count);
		});

		CombineAssertions("Unsuccessful or aborted sending", () =>
		{
			header.CH_BGMReference = bgmRefVersion1;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			manager.SendMessages();

			Assert("Last msg should be an info message", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals("Last Message", "This job is waiting for a Customs response.\r\nAre you sure that you want to resend to Customs?", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			AssertEquals("Header should NOT update the BGMref version", bgmRefVersion1, header.CH_BGMReference);
			AssertEquals("No new messages should be created", 1, header.Messages.Count);
		});
	}
}
