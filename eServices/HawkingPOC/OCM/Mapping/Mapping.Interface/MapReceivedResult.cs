using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Mapping.Interface.Helpers;

namespace OcmPoc.Mapping.Interface
{
	public class MapReceivedResult
    {
		public MapReceivedResult(string correlationId, string recipient, byte[] content)
		{
			CorrelationId = correlationId;
			Recipient = recipient;
			Content = content;
		}

		public MapReceivedResult(CommonMessage message)
		{
			CorrelationId = message.TrackingId;
			Recipient = message.Recipient;
			Content = message.SerializeToJson();
		}

		public string CorrelationId { get; }
		public string Recipient { get; }
		public byte[] Content { get; }
	}
}
