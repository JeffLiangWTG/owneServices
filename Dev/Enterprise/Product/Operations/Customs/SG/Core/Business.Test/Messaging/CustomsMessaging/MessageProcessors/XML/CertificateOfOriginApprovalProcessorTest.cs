using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class CertificateOfOriginApprovalProcessorTest : BaseResponseSectionProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("CertificateOfOriginApproval.xml");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationQuery, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, applicationReference, messageText);
			var entryHeader = GetEntryHeader(MessageProcessor);
			var declarationReference = entryHeader.Declaration.JE_DeclarationReference;
			var email = GetResponseEmail(MessageProcessor);
			var incomingMessage = GetIncomingMessage(MessageProcessor);
			var expectedSubject = "Certificate of Origin for " + declarationReference;
			var expectedHtml = LoadHtmlFile("CertificateOfOriginApproval.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.DeclarationPermitReceived);
			AssertEquals(expectedSubject, email.Subject);
			AssertMultilineASCIIEquals(expectedHtml, email.Body);
			var expectedMessageInterpretation = LoadHtmlFile("CertificateOfOriginApprovalInterpretation.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.DeclarationPermitReceived);
			AssertMultilineASCIIEquals(expectedMessageInterpretation, incomingMessage.EM_MessageInterpretation);
		}

		public void TestProcessWithInvalidMessage()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "<Invalid Message>";
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new CertificateOfOriginApprovalProcessor(new LoggingInformation(), new CertificateOfOriginApproval());
			processor.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			var email = GetResponseEmail(processor);
			var incomingMessage = GetIncomingMessage(processor);
			AssertEquals("ERROR PROCESSING", email.Subject);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, email.Body);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, incomingMessage.EM_MessageInterpretation);
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new CertificateOfOriginApprovalProcessor(new LoggingInformation(), tradenetResponse.OutboundMessage.CertificateOfOriginApproval);
	}
}
