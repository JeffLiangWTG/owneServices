using System;

namespace OcmPoc.FrontEnd.Core.Dtos
{
	public class ReceivedMessageDto
    {
		public int MessageFlowId { get; set; }
		public Guid MessageId { get; set; }
		public string Sender { get; set; }
	}
}
