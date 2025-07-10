using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class InNonPaymentPermitProcessorTest : BasePermitSectionProcessorTest<InNonPaymentPermit>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("INPPMT.xml");
			var status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			var tradenetResponse = AssertProcessValidMessage(string.Empty, status, applicationReference, messageText);
			var entryHeader = GetEntryHeader(MessageProcessor);
			var email = GetResponseEmail(MessageProcessor);
			CombineAssertions(() =>
			{
				var tradeNetOutSection = tradenetResponse.OutboundMessage.InNonPaymentPermit;
				var permit = tradeNetOutSection.Permit;
				var header = tradeNetOutSection.Declaration.Header;
				var authorisationDate = new ZDateTime(2011, 02, 14, 00, 28, 00);
				AssertPermitInfos(entryHeader, permit, authorisationDate);
				AssertEntryPayInfos(entryHeader, header, permit, authorisationDate, 12.00m, "DEF");
				AssertEmailContent(email, permit, entryHeader.Declaration, applicationReference, status, entryHeader.Messages.LastIncomingMessage.EM_MessageInterpretation);
			});
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new InNonPaymentPermitProcessor(new LoggingInformation(), tradenetResponse.OutboundMessage.InNonPaymentPermit);
	}
}
