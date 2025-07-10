using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public static class TWCMessageProcessorFactory
	{
		public static IMessageProcessor GetMessageProcessor(EDIMessage ediMessage, LoggingInformation logger)
		{
			if (ediMessage is TWMessage message)
			{
				if (message.IsCustomsDeliveryNotification)
				{
					return new CustomsDeliveryNotificationProcessor(logger);
				}
				else if (message.IsTranshipment)
				{
					return new TranshipmentProcessor(logger);
				}
				else if (message.IsManifestMessage)
				{
					return new ManifestMessageProcessor(logger);
				}
				else if (message.IsManifestDeliveryNotification)
				{
					return new ManifestDeliveryNotificationProcessor(logger);
				}
				else if (message.IsTransferApplication)
				{
					return new TransferApplicationFromEHubNotificationProcessor(logger);
				}
				else if (message.IsLicensingMessageDeliveryNotification && message.EntryType == MessageTypeList.Codes.NXM)
				{
					return new ControllingAgencyNotificationProcessor(logger);
				}
				else if (message.IsLicensingMessageResponse)
				{
					return new LicensingMessageProcessor(logger);
				}
				else
				{
					return new CustomsDeclarationMessageProcessor(logger);
				}
			}
			else
			{
				return new UnsupportedTypeMessageProcessor(logger);
			}
		}

		internal class UnsupportedTypeMessageProcessor(LoggingInformation logger) : IMessageProcessor
		{
			public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
				=> ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, GetDiscardReason(message));

			public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
				=> ProcessingResult.New(ZGuid.Empty, GetDiscardReason(message));

			public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
				=> ProcessingResult.New(SerializationKeysResult.SerialProcessingInReceivedOrder, GetDiscardReason(message));

			public void ProcessMessage(EDIMessage message)
			{
				logger.Log(GetDiscardReason(message));
				message.EM_Status = TWMessage.Status.Discarded;
			}

			public MultilingualString GetDiscardReason(EDIMessage message) => ResString.GetMultilingualString("F430C3F2-75A4-41C6-A0D7-CE9DFF47A7CD", "Message type {0} is not supported by {1}", message.EM_MessageType, message.EM_ApplicationCode);
			readonly LoggingInformation logger = logger;
		}
	}
}
