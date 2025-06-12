using OcmPoc.Infrastructure.MessageInterfaces.Documents;

namespace OcmPoc.Mapping.Interface
{
	public class MapForSendCommand
    {
		public MapForSendCommand(CommonMessage message)
		{
			CorrelationId = message.TrackingId;
			Sender = message.Sender;
			Message = message;
		}

		public string CorrelationId { get; }
        public string Sender { get; }
        public CommonMessage Message { get; }
    }
}
