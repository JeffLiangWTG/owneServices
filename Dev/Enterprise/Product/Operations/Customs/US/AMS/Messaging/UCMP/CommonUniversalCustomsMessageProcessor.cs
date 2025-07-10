using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public abstract class CommonUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
	{
		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
			=> UCMPMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).GetLinkedBusinessObjectMetaData(message, logger);

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
			=> UCMPMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).GetBranch(message, logger, linkedBusinessObjectBranchPk);

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
			=> UCMPMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData);

		public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => false;

		public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper) => QueryMessageProcessorFactory(logger).ProcessMessage(message);

		protected abstract ApplicationTypeMessageProcessor QueryMessageProcessorFactory(LoggingInformation logger);
	}
}
