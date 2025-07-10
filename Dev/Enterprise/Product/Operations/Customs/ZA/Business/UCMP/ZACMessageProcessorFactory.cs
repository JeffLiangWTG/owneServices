using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.UCMP
{
	public static class ZACMessageProcessorFactory
	{
		public static IMessageProcessor GetMessageProcessor(ZString messageType, LoggingInformation logger) =>
			messageType.ToString().ToUpperInvariant() switch
			{
				SARSEDIMessage.MessageTypes.CONTRL => new CONTRLMessageProcessor(logger),
				SARSEDIMessage.MessageTypes.CUSCAR => new CUSCARMessageProcessor(logger),
				SARSEDIMessage.MessageTypes.CUSRES => new CUSRESMessageProcessor(logger),
				SARSEDIMessage.MessageTypes.CUSRES_REQDOC => new CUSRES_REQDOCMessageProcessor(logger),
				SARSEDIMessage.MessageTypes.GENRAL => new GENRALMessageProcessor(logger),
				SARSEDIMessage.MessageTypes.STATAC => new STATACMessageProcessor(logger),
				_ => new UnsupportedTypeMessageProcessor(logger)
			};

		internal class UnsupportedTypeMessageProcessor : IMessageProcessor
		{
			public UnsupportedTypeMessageProcessor(LoggingInformation logger)
			{
				this.logger = logger;
			}

			public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
				=> ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, GetDiscardReason(message));

			public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
				=> ProcessingResult.New(ZGuid.Empty, GetDiscardReason(message));

			public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
				=> ProcessingResult.New(SerializationKeysResult.SerialProcessingInReceivedOrder, GetDiscardReason(message));

			public void ProcessMessage(EDIMessage message)
			{
				logger.Log(GetDiscardReason(message));
				message.EM_Status = ZAMessage.Status.Discarded;
			}

			MultilingualString GetDiscardReason(EDIMessage message) => ResString.GetMultilingualString("65258F81-6964-4B64-A164-83E7230E2CEC", "Message type {0} is not supported by {1}", message.EM_MessageType, message.EM_ApplicationCode);
			readonly LoggingInformation logger;
		}
	}
}
