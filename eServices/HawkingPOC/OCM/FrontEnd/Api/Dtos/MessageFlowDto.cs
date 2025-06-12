using System.Collections.Generic;

namespace OcmPoc.FrontEnd.Core.Dtos
{
	public class MessageFlowDto
	{
		public int Id { get; set; }
		public string Sender { get; set; }
		public string Recipient { get; set; }
		public string Status { get; set; }

		public IReadOnlyCollection<MessageEventDto> Events { get; set; }
	}
}
