using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.PL.Business;

public abstract class UniversalCustomsMessageProcessorBase<TMessageProcessorFactory> : IUniversalCustomsMessageProcessor
	where TMessageProcessorFactory : MessageProcessorFactoryBase, new()
{
	TMessageProcessorFactory MessageProcessorFactory { get; } = new();

	public bool ShouldMessageBeProcessedInASeparateFactory(EnterpriseEDIMessage message) => false;

	public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(
		EnterpriseEDIMessage message,
		LoggingInformation logger)
		=> new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, jobNumber: ZString.Empty);

	public ProcessingResult<ZGuid> GetBranch(
		EnterpriseEDIMessage message,
		LoggingInformation logger,
		ZGuid linkedBusinessObjectBranchPk)
		=> message.EM_GB;

	public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(
		EnterpriseEDIMessage message,
		LoggingInformation logger,
		LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		=> new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new SortedSet<string>([message.PK.ToStringKey()]));

	public void ProcessMessage(
		EnterpriseEDIMessage message,
		LoggingInformation logger,
		IUniversalCustomsMessageProcessorHelper helper)
	{
		var messageProcessor = MessageProcessorFactory.CreateProcessor(message, logger);
		messageProcessor?.ProcessMessage(message);
	}
}
