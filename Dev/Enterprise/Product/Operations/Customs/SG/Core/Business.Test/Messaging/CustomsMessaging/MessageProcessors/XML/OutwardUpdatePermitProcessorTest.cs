using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class OutwardUpdatePermitProcessorTest : BasePermitSectionProcessorTest<OutwardUpdatePermit>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "199702247W202005208000";
			var messageText = LoadXmlFile("OUTUPT.xml");
			var status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			var tradenetResponse = AssertProcessValidMessage(string.Empty, status, applicationReference, messageText);
			var entryHeader = GetEntryHeader(MessageProcessor);
			var email = GetResponseEmail(MessageProcessor);
			CombineAssertions(() =>
			{
				var tradeNetOutSection = tradenetResponse.OutboundMessage.OutwardUpdatePermit;
				var permit = tradeNetOutSection.Permit;
				var header = tradeNetOutSection.Declaration.Header;
				var authorisationDate = new ZDateTime(2011, 02, 14, 08, 52, 00);
				AssertPermitInfos(entryHeader, permit, authorisationDate);
				AssertEmailContent(email, permit, entryHeader.Declaration, applicationReference, status, entryHeader.Messages.LastIncomingMessage.EM_MessageInterpretation);
			});
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new OutwardUpdatePermitProcessor(new LoggingInformation(), tradenetResponse.OutboundMessage.OutwardUpdatePermit);
	}
}
