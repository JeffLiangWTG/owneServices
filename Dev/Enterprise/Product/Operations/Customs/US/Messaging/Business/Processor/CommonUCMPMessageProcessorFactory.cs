using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Messaging.Business.Processor
{
	public abstract class CommonUCMPMessageProcessorFactory
	{
		public static MultilingualString GetUnableToFindTheLinkedJobMessage(EDIMessage message) =>
			ResString.GetMultilingualString("47F2436D-C4EF-4085-9649-4303E948A633", "Unable to find the linked job for {0} Message: #{1}/{2}", message.EM_MessageType, message.EM_MessageNum, message.Interchange?.EI_InterchangeNum);
		public static MultilingualString GetMessageProcessorCannotProcessMessage(EDIMessage message, string messageType = null, string userLog = "") =>
			ResString.GetMultilingualString("9D940781-329E-4098-8C13-9759DC8717FE", "{0} Message Processor cannot process message {1} {2}", string.IsNullOrEmpty(messageType) ? message.EM_MessageType : messageType, message.GetType().Name, userLog);

		public class UnsupportedTypeMessageProcessor : IKeysForBlockingParallelProcessingProvider
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
				message.EM_Status = CBPEDIMessage.Status.Discarded;
			}

			readonly LoggingInformation logger;

			MultilingualString GetDiscardReason(EDIMessage message) => ResString.GetMultilingualString("2E32F362-D703-433E-8F82-3D1C71A073E3", "Message type {0} is not supported by {1}", message.EM_MessageType, message.EM_ApplicationCode);
		}
	}
}
