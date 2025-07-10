using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class ApprovalMessageProcessorTest : BaseResponseSectionProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("ApprovalMessage.xml");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationQuery, Core.SGConstants.DeclarationStatus.CancellationAccepted, applicationReference, messageText);
			var declarationReference = ((ApprovalMessageProcessorForTest)MessageProcessor).EntryHeader.Declaration.JE_DeclarationReference;
			var expectedSubject = "Cancellation for " + declarationReference;
			var expectedHtml = LoadHtmlFile("ApprovalMessageEmail.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.CancellationAccepted);
			var email = ((ApprovalMessageProcessorForTest)MessageProcessor).ResponseEmail;
			AssertEquals(expectedSubject, email.Subject);
			AssertMultilineASCIIEquals(expectedHtml, email.Body);
			var expectedMessageInterpretation = LoadHtmlFile("ApprovalMessageInterpretation.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.CancellationAccepted);
			AssertMultilineASCIIEquals(expectedMessageInterpretation, ((ApprovalMessageProcessorForTest)MessageProcessor).MessageInterpretation);
		}

		public void TestProcessWithInvalidMessage()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "<Invalid Message>";
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new ApprovalMessageProcessorForTest(new LoggingInformation(), new ApprovalMessage());
			processor.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			var email = processor.ResponseEmail;
			AssertEquals("ERROR PROCESSING", email.Subject);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, email.Body);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, processor.MessageInterpretation);
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new ApprovalMessageProcessorForTest(new LoggingInformation(), tradenetResponse.OutboundMessage.ApprovalMessage);
		sealed class ApprovalMessageProcessorForTest : ApprovalMessageProcessor
		{
			internal ApprovalMessageProcessorForTest(LoggingInformation logger, ApprovalMessage approvalMessage) : base(logger, approvalMessage)
			{
			}

			internal CusEntryHeader EntryHeader => entry;

			internal EmailDef ResponseEmail => responseEmail;

			internal ZString MessageInterpretation => IncomingMessage?.EM_MessageInterpretation ?? string.Empty;
		}
	}
}
