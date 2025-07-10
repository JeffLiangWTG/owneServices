using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class ErrorMessageProcessorTest : BaseResponseSectionProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("ErrorMessage.xml");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors, applicationReference, messageText);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors, applicationReference, messageText);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors, applicationReference, messageText);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.CancellationSent, Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors, applicationReference, messageText);
			var declarationReference = ((ErrorMessageProcessorForTest)MessageProcessor).EntryHeader.Declaration.JE_DeclarationReference;
			var expectedSubject = "Error for " + declarationReference;
			var expectedHtml = LoadHtmlFile("ErrorMessageEmail.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors);
			var email = ((ErrorMessageProcessorForTest)MessageProcessor).ResponseEmail;
			AssertEquals(expectedSubject, email.Subject);
			AssertMultilineASCIIEquals(expectedHtml, email.Body);
			var expectedMessageInterpretation = LoadHtmlFile("ErrorMessageInterpretation.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors);
			AssertMultilineASCIIEquals(expectedMessageInterpretation, ((ErrorMessageProcessorForTest)MessageProcessor).MessageInterpretation);
		}

		public void TestProcessWithInvalidMessage()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "<Invalid Message>";
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new ErrorMessageProcessorForTest(new LoggingInformation(), new ErrorMessage());
			processor.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			var email = processor.ResponseEmail;
			AssertEquals("ERROR PROCESSING", email.Subject);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, email.Body);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, processor.MessageInterpretation);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDuplicateError()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("ErrorMessage.xml");
			var messageTextWithDuplicateDeclarationError = messageText.Replace("<cbc:ErrorCode>E15053</cbc:ErrorCode>", "<cbc:ErrorCode>E17001</cbc:ErrorCode>");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, Core.SGConstants.DeclarationStatus.DeclarationSent, applicationReference, messageTextWithDuplicateDeclarationError, EDIMessage.Status.Pending, EDIMessage.Status.Discarded);
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, ((ErrorMessageProcessorForTest)MessageProcessor).ResponseEmail.Subject);
				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationReference, applicationReference));
				var outgoingMessage = messages.Single(_ => _.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
				var incomingMessage = messages.Single(_ => _.EM_ReceiveTransmit == EDIMessage.Direction.Receive);
				AssertEquals("Entry status should stay the same despite duplicate error", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				var note = incomingMessage.Notes.GetAllNotes().Single() as StmNote;
				AssertEquals("Incoming message discarded", note.ST_Description);
				AssertEquals("A duplicate error was returned. The message was discarded.", note.ST_NoteText);
			});
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new ErrorMessageProcessorForTest(new LoggingInformation(), tradenetResponse.OutboundMessage.ErrorMessage);

		sealed class ErrorMessageProcessorForTest : ErrorMessageProcessor
		{
			internal ErrorMessageProcessorForTest(LoggingInformation logger, ErrorMessage errorMessage) : base(logger, errorMessage)
			{
			}

			internal CusEntryHeader EntryHeader => entry;

			internal EmailDef ResponseEmail => responseEmail;

			internal ZString MessageInterpretation => IncomingMessage?.EM_MessageInterpretation ?? string.Empty;
		}
	}
}
