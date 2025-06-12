using System.Collections.Generic;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Mapping.Interface;
using OcmPoc.Mapping.Interface.Helpers;
using OcmPoc.Mapping.Provider.CW1;

namespace OcmPoc.Mapping.Provider
{
	public class CW1Mapping : IMapping
	{
		public void Initialise()
		{
		}

		public Task<MapForSendResult> MapAsync(MapForSendCommand command)
		{
			var commonMessage = command.Message;

			var message = new CW1Message
			{
				To = commonMessage.Recipient,
				From = commonMessage.Sender,
				TrackingId = commonMessage.TrackingId,
				Content = new List<string>(commonMessage.Content)
			};

			return Task.FromResult(new MapForSendResult("", message.SerializeToXml()));
		}

		public Task<MapReceivedResult> MapAsync(MapReceivedCommand command)
		{
			var message = command.Content.DeserializeFromXml<CW1Message>();

			var commonMessage = new CommonMessage
			{
				Sender = message.From ?? "CW1",
				Recipient = message.To,
				TrackingId = message.TrackingId,
				Content = new List<string>(message.Content)
			};

			return Task.FromResult(
				new MapReceivedResult(
					message.TrackingId,
					message.To,
					commonMessage.SerializeToJson()));
		}
	}
}
