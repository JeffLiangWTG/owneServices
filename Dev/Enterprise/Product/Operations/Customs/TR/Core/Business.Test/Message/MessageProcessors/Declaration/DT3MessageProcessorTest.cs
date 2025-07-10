using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class DT3MessageProcessorTest : DeclarationMessageProcessorBaseTest<DT3MessageProcessor>
	{
		public void TestProcessMessage()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DT3001", TRMessageTypes.Codes.DT3);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var jobDeclaration = cusEntryHeader.Declaration;
			CombineAssertions("Registered Message", () =>
			{
				AssertEquals("Declaration | Registration Number", "23066666EX00000026", jobDeclaration.DeclarationNumber);
				AssertEquals("Declaration | Registration Date", new ZDateTime(2023, 3, 24), jobDeclaration.JE_DeclarationDate);
				AssertEquals("Declaration | Issue Date", new ZDateTime(2023, 3, 24), jobDeclaration.EarliestCustomsEntryIssueDate);
				AssertNotNull("Entry Header", cusEntryHeader);
			});

			var actualMessage = incomingMessage.EM_MessageInterpretation.GetBodyText().RemoveLineBreakingsAndIndents();
			CombineAssertions("Message Process Result", () =>
			{
				AssertNotNullOrEmpty("Actual Message", actualMessage);
				Assert("EM_MessageInterpretation should contains 'Column Labels'", actualMessage.Contains(@"<tr class=""tableheadings""><th>Label</th><th>Value</th></tr>"));
				Assert("EM_MessageInterpretation should contains 'Registration Number'", actualMessage.Contains("<tr><td>Registration Number:</td><td>23066666EX00000026</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'Registration Date'", actualMessage.Contains("<tr><td>Registration Date:</td><td>24/03/2023</td></tr>"));
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.AGQ, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByNotRegistered()
		{
			var successNotRegistered = TRMessageTestHelper.GetFileText("DT3SuccessNotRegistered.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming.");
			var messageObjects = CreateMessageWithOrigins(successNotRegistered, "DT3002", TRMessageTypes.Codes.DT3);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var actualMessage = incomingMessage.EM_MessageInterpretation;
			Assert("EM_MessageInterpretation should contains 'Not Registered'", actualMessage.Contains(@"No Registration Information in the message content sent back from customs."));
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DT3001", TRMessageTypes.Codes.DT3);
			var outgoingMessage = messageObjects.originalMessage;
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			var incomingMessage = messageObjects.incomingMessage;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var declarationReference = cusEntryHeader.Declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(declarationReference));

			var bodyText = email.Body;

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", user.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains cleared Information", bodyText.Contains("A response message has been received from Customs."));
				Assert("Contains cleared Information", bodyText.Contains("Shown below is a summary of relevant information received in the message."));
				Assert("Contains RegistrationNumber and RegistrationDate", bodyText.Contains("<tr><td>Registration Number:</td><td>23066666EX00000026</td></tr><tr><td>Registration Date:</td><td>24/03/2023</td></tr>"));
				Assert("Contains Accepted", bodyText.Contains("has been Accepted"));
			});
		}

		protected override DT3MessageProcessor Processor => new DT3MessageProcessor(logger);

		protected override ZString DefaultMessageType => TRMessageTypes.Codes.DT3;
		ZString GetDefaultMessageText() => TRMessageTestHelper.GetFileText("DT3Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming.");
	}
}
