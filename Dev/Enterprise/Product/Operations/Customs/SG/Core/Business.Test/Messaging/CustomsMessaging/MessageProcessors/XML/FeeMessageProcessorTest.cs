using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class FeeMessageProcessorTest : BaseResponseSectionProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "XXXXXXXXE01T201102148889";
			var messageText = LoadXmlFile("FeeMessage.xml");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, applicationReference, messageText);
			var email = ((FeeMessageProcessor)MessageProcessor).responseEmail;
			AssertEquals("Licence fee payment advice for B00001000", email.Subject);
			var expectedHtml = LoadHtmlFile("Debadv.htm");
			AssertMultilineASCIIEquals(expectedHtml, email.Body);
			var expectedMessageInterpretation = LoadHtmlFile("DebadvInterpretation.htm");
			AssertMultilineASCIIEquals(expectedMessageInterpretation, ((FeeMessageProcessor)MessageProcessor).incomingMessage?.EM_MessageInterpretation ?? string.Empty);
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new FeeMessageProcessor(new LoggingInformation(), tradenetResponse.OutboundMessage.FeeMessage);
	}
}
