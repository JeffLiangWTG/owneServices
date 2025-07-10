using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.INCustoms, typeof(Enterprise.Customs.Business.MessageProcessors.UCMP.BaseInterchangeUnpacker))]
[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.AUCOLS, typeof(Enterprise.Customs.Business.MessageProcessors.UCMP.BaseInterchangeUnpacker))]

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public class BaseInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
		{
			EDIMessage message;
			if (interchange.EI_InterchangeType == Constant.MessageTypes.XER)
			{
				return XTFailureInterchangeHandler.Unpack(interchange, outgoingMessage);
			}
			else
			{
				message = EDIInterchangeUnPackerUtils.CreateReceivedEDIMessage(interchange, interchange.EI_BodyText);
			}

			return new EDIInterchangeUnpackerResult(new[] { message });
		}
	}
}
