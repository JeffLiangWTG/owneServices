using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Documents
{
	public class CommonMessage
	{
		public string TrackingId { get; set; }
		public string Sender { get; set; }
		public string Recipient { get; set; }
		public string ResponseTo { get; set; }
		public List<string> Content { get; set; } = new List<string>();
	}
}
