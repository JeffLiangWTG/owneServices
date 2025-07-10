using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMMessageProcessor : Enterprise.Messaging.MessageProcessors.ApplicationTypeMessageProcessor
	{
		public UEMMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => string.Empty;

		protected override string ApplicationCodeCore => ApplicationCodeList.Codes.USExportManifest;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var messageStatus = EDIMessage.Status.Failed;

			if (message is UEMEDIMessage uemMessage)
			{
				var processor = GetMessageTypeProcessor(uemMessage.EM_MessageType);
				if (processor?.Process(uemMessage) ?? false)
				{
					messageStatus = EDIMessage.Status.Received;
				}
			}

			message.EM_Status = messageStatus;
		}

		IUEMMessageTypeProcessor GetMessageTypeProcessor(ZString messageType)
		{
			switch (messageType)
			{
				case MessageTypeList.Codes.ExportManifestResponse:
					return new EMRMessageTypeProcessor();
				default:
					return null;
			}
		}
	}
}
