using System.Collections.Generic;

namespace OcmPoc.Mapping.Provider.CW1
{
	public class CW1Message
    {
		public string To { get; set; }
		public string From { get; set; }
		public string TrackingId { get; set; }
		public List<string> Content { get; set; }
	}
}
