using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.XHCredentialConfig, typeof(Enterprise.Customs.Business.MessageProcessors.UCMP.CFGInterchangeUnpacker))]

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public class CFGInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
		{
			var factory = interchange.Factory;
			var message = factory.New<EDIMessage>();
			EDIInterchangeUnPackerUtils.PopulateEDIMessage(message,
				interchange.EI_ApplicationCode,
				outgoingMessage != null ? outgoingMessage.EM_MessageType : outgoingInterchange != null ? outgoingInterchange.EI_InterchangeType : interchange.EI_InterchangeType,
				interchange.EI_InterchangeType,
				interchange.EI_InterchangeNum,
				interchange.EI_BodyText);

			interchange.ContainedMessages.Add(message);

			return new EDIInterchangeUnpackerResult(new[] { message });
		}
	}
}
