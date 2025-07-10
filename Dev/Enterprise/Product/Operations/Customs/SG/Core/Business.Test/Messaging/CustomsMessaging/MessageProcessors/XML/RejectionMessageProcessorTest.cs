using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class RejectionMessageProcessorTest : BaseResponseSectionProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessWithQueryResponse()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("RejectionMessage.xml");
			messageText = messageText.Replace("<cbc:StatusType>CRD</cbc:StatusType>", $"<cbc:StatusType>{RejectionMessageProcessor.ControllingAgencyQuery}</cbc:StatusType>");
			AssertEntryStatusAndCreateResponseEmail(messageText);
			messageText = messageText.Replace($"<cbc:StatusType>{RejectionMessageProcessor.ControllingAgencyQuery}</cbc:StatusType>", $"<cbc:StatusType>{RejectionMessageProcessor.SingaporeCustomsQuery}</cbc:StatusType>");
			AssertEntryStatusAndCreateResponseEmail(messageText);
			void AssertEntryStatusAndCreateResponseEmail(string text)
			{
				AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, Core.SGConstants.DeclarationStatus.DeclarationQuery, applicationReference, text);
				AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, Core.SGConstants.DeclarationStatus.AmendmentQuery, applicationReference, text);
				AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.CancellationSent, Core.SGConstants.DeclarationStatus.CancellationQuery, applicationReference, text);
				AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, Core.SGConstants.DeclarationStatus.RefundQuery, applicationReference, text);
				var declarationReference = ((RejectionMessageProcessorForTest)MessageProcessor).EntryHeader.Declaration.JE_DeclarationReference;
				var expectedSubject = "Impediment Query from Customs for " + declarationReference;
				var expectedHtml = LoadHtmlFile("RejectionMessageEmailWithQuery.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.RefundQuery);
				var email = ((RejectionMessageProcessorForTest)MessageProcessor).ResponseEmail;
				AssertEquals(expectedSubject, email.Subject);
				AssertMultilineASCIIEquals(expectedHtml, email.Body);
				var expectedMessageInterpretation = LoadHtmlFile("RejectionMessageInterpretationWithQuery.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.RefundQuery);
				AssertMultilineASCIIEquals(expectedMessageInterpretation, ((RejectionMessageProcessorForTest)MessageProcessor).MessageInterpretation);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessWithoutQueryResponse()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("RejectionMessage.xml");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationQuery, Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms, applicationReference, messageText);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms, applicationReference, messageText);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.CancellationSent, Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms, applicationReference, messageText);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms, applicationReference, messageText);
			var declarationReference = ((RejectionMessageProcessorForTest)MessageProcessor).EntryHeader.Declaration.JE_DeclarationReference;
			var expectedSubject = "Error for " + declarationReference;
			var expectedHtml = LoadHtmlFile("RejectionMessageEmail.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms);
			var email = ((RejectionMessageProcessorForTest)MessageProcessor).ResponseEmail;
			AssertEquals(expectedSubject, email.Subject);
			AssertMultilineASCIIEquals(expectedHtml, email.Body);
			var expectedMessageInterpretation = LoadHtmlFile("RejectionMessageInterpretation.htm").Replace("##JobNumber##", declarationReference).Replace("##EntryStatus##", Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms);
			AssertMultilineASCIIEquals(expectedMessageInterpretation, ((RejectionMessageProcessorForTest)MessageProcessor).MessageInterpretation);
		}

		public void TestProcessWithInvalidMessage()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "<Invalid Message>";
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new RejectionMessageProcessorForTest(new LoggingInformation(), new RejectionMessage());
			processor.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			var email = processor.ResponseEmail;
			AssertEquals("REJECTION PROCESSING", email.Subject);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, email.Body);
			AssertMultilineASCIIEquals("FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText, processor.MessageInterpretation);
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new RejectionMessageProcessorForTest(new LoggingInformation(), tradenetResponse.OutboundMessage.RejectionMessage);

		sealed class RejectionMessageProcessorForTest : RejectionMessageProcessor
		{
			internal RejectionMessageProcessorForTest(LoggingInformation logger, RejectionMessage rejectionMessage) : base(logger, rejectionMessage)
			{
			}

			internal CusEntryHeader EntryHeader => entry;

			internal EmailDef ResponseEmail => responseEmail;

			internal ZString MessageInterpretation => IncomingMessage?.EM_MessageInterpretation ?? string.Empty;
		}
	}
}
