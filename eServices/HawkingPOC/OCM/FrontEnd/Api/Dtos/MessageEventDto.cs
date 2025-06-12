using System;

namespace OcmPoc.FrontEnd.Core.Dtos
{
	public class MessageEventDto
	{
		public string MessageId { get; set; }
		public DateTime Timestamp { get; set; }
		public string Type { get; set; }
	}
}
