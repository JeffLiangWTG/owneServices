using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsMessageProcessor(ApplicationCodeList.Codes.NOCustoms, typeof(Enterprise.Customs.NO.Business.EDIFactMessageResponseProcessor))]

namespace Enterprise.Customs.NO.Business;

sealed class EDIFactMessageResponseProcessor : IUniversalCustomsMessageProcessor
{
	bool IUniversalCustomsMessageProcessor.ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => false;

	public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		=> new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, ZString.Empty);

	public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
		=> message.EM_GB;

	public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
	{
		var linkedObject = message.EM_LinkedObject;
		if (linkedObject is CusEntryHeader header)
		{
			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
			{
				header.CH_BGMReference,
				header.DeclarationReference
			});
		}

		return SerializationKeysResult.SerialProcessingInReceivedOrder;
	}

	void IUniversalCustomsMessageProcessor.ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
	{
		var messageProcessor = MessageProcessorFactory.GetMessageProcessor(message, logger);
		messageProcessor?.ProcessMessage(message, logger);
	}
}
