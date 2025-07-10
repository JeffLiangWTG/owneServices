using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

[assembly: UniversalCustomsMessageProcessor(EDIMessage.ApplicationCodes.TaiwanCustoms, typeof(Enterprise.Customs.TW.Business.TWCUniversalCustomsMessageProcessor))]
namespace Enterprise.Customs.TW.Business
{
	public class TWCUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
	{
		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
			=> TWCMessageProcessorFactory.GetMessageProcessor(message, logger).GetLinkedBusinessObjectMetaData(message, logger);

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
			=> TWCMessageProcessorFactory.GetMessageProcessor(message, logger).GetBranch(message, logger, linkedBusinessObjectBranchPk);

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
			=> TWCMessageProcessorFactory.GetMessageProcessor(message, logger).GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData);

		public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => false;

		public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper) =>
			TWCMessageProcessorFactory.GetMessageProcessor(message, logger).ProcessMessage(message);
	}
}
