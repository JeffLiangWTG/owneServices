using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ZA.Business.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

[assembly: UniversalCustomsMessageProcessor(EDIMessage.ApplicationCodes.SouthAfricanCustoms, typeof(Enterprise.Customs.ZA.Business.ZACUniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.ZA.Business
{
	public class ZACUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
	{
		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
			=> ZACMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).GetLinkedBusinessObjectMetaData(message, logger);

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
			=> ZACMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).GetBranch(message, logger, linkedBusinessObjectBranchPk);

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
			=> ZACMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData);

		public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => false;

		public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper) =>
			ZACMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).ProcessMessage(message);
	}
}
