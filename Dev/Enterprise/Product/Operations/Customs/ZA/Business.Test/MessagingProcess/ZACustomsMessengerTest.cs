using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.MessagingProcess.Testing
{
	sealed class ZACustomsMessengerTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var (messengerInterface, sendingObject) = GetMessengerAndSendingObject();

			CombineAssertions("Pre-Requisites", () =>
			{
				AssertType<ZACustomsMessenger>("Interface implementation should be CustomsMessenger", messengerInterface);
			});

			CombineAssertions("Messenger Properties", () =>
			{
				AssertSame("Owner and Entry should be the same Object", sendingObject.Header, messengerInterface.Owner);
				AssertType<CUSDECMessageBuilder>("Generator should be CUSDECMessageBuilder", messengerInterface.MessageGenerator);
			});
		}

		public void TestMessageGenerator()
		{
			var messenger = GetMessenger();

			AssertType<CUSDECMessageBuilder>(messenger.MessageGenerator);
		}

		public void TestShouldCreateMessage()
		{
			var (messengerInterface, sendingObject) = GetMessengerAndSendingObject();
			var ar = new ActionResult();

			CombineAssertions("ShouldCreateMessage", () =>
			{
				sendingObject.ShouldSend = false;
				var shouldCreate = messengerInterface.ShouldCreateMessage(ar);
				AssertEquals("No message created", false, shouldCreate);

				sendingObject.ShouldSend = true;
				shouldCreate = messengerInterface.ShouldCreateMessage(ar);
				AssertEquals("No message created", true, shouldCreate);
			});
		}

		public void TestProcessUpdates()
		{
			var messenger = GetMessenger();
			var result = messenger.ProcessUpdates(new ActionResult());

			AssertEquals("Always true", true, result);
		}

		public void TestProcessUpdates_PopulateEntrySubmittedDate()
		{
			var (messenger, sendingObj) = GetMessengerAndSendingObject();

			CombineAssertions(() =>
			{
				AssertEquals("Pre-req EntrySubmitted date should be empty", true, sendingObj.Header.CH_EntrySubmittedDate.IsEmpty);

				_ = messenger.ProcessUpdates(new ActionResult(false));
				AssertEquals("No change if prev-result not successful", true, sendingObj.Header.CH_EntrySubmittedDate.IsEmpty);

				_ = messenger.ProcessUpdates(new ActionResult(true));
				AssertEquals("EntrySubmitted should not be empty", false, sendingObj.Header.CH_EntrySubmittedDate.IsEmpty);
			});
		}

		public void TestProcessUpdates_LogCustomsCommenced()
		{
			var (messenger, sendingObj) = GetMessengerAndSendingObject();

			CombineAssertions(() =>
			{
				var logs = sendingObj.Header.Declaration.Logs;
				AssertEquals("Pre-req no log for Customs commenced", false, logs.Find(x => x.SL_SE_NKEvent == "CCC").Any());

				_ = messenger.ProcessUpdates(new ActionResult(false));
				AssertEquals("No change if prev-result not successful", false, logs.Find(x => x.SL_SE_NKEvent == "CCC").Any());

				_ = messenger.ProcessUpdates(new ActionResult(true));
				AssertEquals("Customs commenced event should be logged", true, logs.Find(x => x.SL_SE_NKEvent == "CCC").Any());
			});
		}

		public void TestProcessUpdates_PopulateTargetEntryLineNo()
		{
			var (messenger, sendingObj) = GetMessengerAndSendingObject();

			CombineAssertions(() =>
			{
				var invoiceLine = sendingObj.Header.AllMergedLines[0].InvoiceLines[0] as JobComInvoiceLine;
				AssertEquals("Pre-req UZ_TargetEntryLineNo should not be set", true, invoiceLine.JI_TargetEntryLineNumber.IsEmpty);

				_ = messenger.ProcessUpdates(new ActionResult(false));
				AssertEquals("No change if prev-result not successful", true, invoiceLine.JI_TargetEntryLineNumber.IsEmpty);

				_ = messenger.ProcessUpdates(new ActionResult(true));
				AssertEquals("UZ_TargetEntryLineNo should not be empty", false, invoiceLine.JI_TargetEntryLineNumber.IsEmpty);
			});
		}

		public void TestProcessUpdates_UpdateLRN()
		{
			var (messenger, sendingObj) = GetMessengerAndSendingObject();

			CombineAssertions(() =>
			{
				sendingObj.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				sendingObj.Header.CH_BGMReference = "BGM000001";
				sendingObj.LocalReferenceNumber = "100000MGB";
				AssertEquals("Pre-req CH_BGMReference should not be changed", "BGM000001", sendingObj.Header.CH_BGMReference);

				_ = messenger.ProcessUpdates(new ActionResult(false));
				AssertEquals("No change if prev-result not successful", "BGM000001", sendingObj.Header.CH_BGMReference);

				_ = messenger.ProcessUpdates(new ActionResult(true));
				AssertEquals("CH_BGMReference should be updated", "100000MGB", sendingObj.Header.CH_BGMReference);
			});
		}

		public void TestProcessUpdates_UpdateMessageStatus()
		{
			var (messenger, sendingObj) = GetMessengerAndSendingObject();

			CombineAssertions(() =>
			{
				AssertEquals("Pre-req CH_Status/MessageStatus should be empty", ZString.Empty, sendingObj.Header.MessageStatus);

				_ = messenger.ProcessUpdates(new ActionResult(false));
				AssertEquals("No change if prev-result not successful", ZString.Empty, sendingObj.Header.MessageStatus);

				_ = messenger.ProcessUpdates(new ActionResult(true));
				AssertEquals("CH_Status/MessageStatus should be updated", "AWA", sendingObj.Header.MessageStatus);
			});
		}

		public void TestOwner()
		{
			var (messengerInterface, sendingObject) = GetMessengerAndSendingObject();

			AssertSame("Owner is JobDeclaration", sendingObject.Header, messengerInterface.Owner);
		}

		public void TestISupportPermitProcessing()
		{
			var msg = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			msg.EM_MessageNum = "OUT123";

			var messenger = GetMessenger();
			var permitSupporter = messenger as ISupportPermitProcessing;

			AssertNotNull("Messenger is ISupportPermitProcessing", permitSupporter);
			AssertType<ZACusPermitCusDecProcessor>("ZA Permit Processor", permitSupporter.PermitProcessor);
			AssertEquals("AppId", "OUT123", permitSupporter.GetPermitAppIdForMessage(msg));
		}

		ICustomsMessenger GetMessenger() => GetMessengerAndSendingObject().messenger;

		(ICustomsMessenger messenger, MessageSendingObject sendingObject) GetMessengerAndSendingObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";

			_ = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			new LineMerger(declaration).DoMerge();

			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject = decWrapper.SendingObjectsCollection.OfType<MessageSendingObject>().First();
			var messengerInterface = ZACustomsMessenger.New(sendingObject);

			return (messengerInterface, sendingObject);
		}
	}
}
