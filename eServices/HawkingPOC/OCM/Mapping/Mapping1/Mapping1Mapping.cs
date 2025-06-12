using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OcmPoc.Mapping.Interface;

namespace OcmPoc.Mapping1
{
	public class Mapping1Mapping : IMapping
	{
		public void Initialise()
		{
		}

		public byte[] Map(byte[] message, IDictionary<string, object> metadata)
		{
			return Encoding.UTF8.GetBytes($"Mapped from msg {metadata["OriginalMessageId"]} at {DateTime.Now}");
		}

		public Task<MapForSendResult> MapAsync(MapForSendCommand command)
		{
			throw new NotImplementedException();
		}

		public Task<MapReceivedResult> MapAsync(MapReceivedCommand command)
		{
			throw new NotImplementedException();
		}
	}
}
