using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.TW.Business
{
	public interface IMessageProcessor
	{
		ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger);

		ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk);

		ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData);

		void ProcessMessage(EDIMessage message);
	}
}
