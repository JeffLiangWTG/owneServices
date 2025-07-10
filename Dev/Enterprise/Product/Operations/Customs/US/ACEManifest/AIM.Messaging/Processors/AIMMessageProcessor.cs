using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMMessageProcessor : Enterprise.Messaging.MessageProcessors.ApplicationTypeMessageProcessor
	{
		public AIMMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string ApplicationCodeCore => ApplicationCodeList.Codes.USAMA;

		protected override string MessageFriendlyNameCore => string.Empty;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var messageStatus = EDIMessage.Status.Failed;

			if (message is AIMEDIMessage aimMessage)
			{
				var processor = GetMessageTypeProcessor(aimMessage.ComponentIdentifier);
				if (processor?.Process(aimMessage) ?? false)
				{
					messageStatus = EDIMessage.Status.Received;
				}
			}

			message.EM_Status = messageStatus;
		}

		IAIMMessageTypeProcessor GetMessageTypeProcessor(ZString messageType)
		{
			switch (messageType)
			{
				case Constants.AIMMessageSubTypes.FER:
					return new FERMessageTypeProcessor(Logger);
				case Constants.AIMMessageSubTypes.FSC:
					return new FSCMessageTypeProcessor(Logger);
				case Constants.AIMMessageSubTypes.FSI:
					return new FSIMessageTypeProcessor(Logger);
				case Constants.AIMMessageSubTypes.FSN:
					return new FSNMessageTypeProcessor(Logger);
			}
			return null;
		}
	}
}
