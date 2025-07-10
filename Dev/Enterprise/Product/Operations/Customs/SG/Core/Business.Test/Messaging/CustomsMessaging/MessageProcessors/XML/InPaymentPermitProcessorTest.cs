using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class InPaymentPermitProcessorTest : BasePermitSectionProcessorTest<InPaymentPermit>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("IPTPMT.xml");
			var status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			var tradenetResponse = AssertProcessValidMessage(string.Empty, status, applicationReference, messageText);
			var entryHeader = GetEntryHeader(MessageProcessor);
			var email = GetResponseEmail(MessageProcessor);
			CombineAssertions(() =>
			{
				var tradeNetOutSection = tradenetResponse.OutboundMessage.InPaymentPermit;
				var permit = tradeNetOutSection.Permit;
				var authorisationDate = new ZDateTime(2011, 02, 14, 10, 50, 00);
				AssertPermitInfos(entryHeader, permit, authorisationDate);
				AssertEmailContent(email, permit, entryHeader.Declaration, applicationReference, status, entryHeader.Messages.LastIncomingMessage.EM_MessageInterpretation);
			});
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new InPaymentPermitProcessor(new LoggingInformation(), tradenetResponse.OutboundMessage.InPaymentPermit);
	}
}
