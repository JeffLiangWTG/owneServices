using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	[Serializable, XmlRoot("response")]
	public class MHAccessResponse
	{
		[XmlElement("status")]
		public string Status { get; set; }
		[XmlElement("state")]
		public string State { get; set; }
		[XmlElement("errorCode")]
		public string ErrorCode { get; set; }
		[XmlElement("errorMessage")]
		public string ErrorMessage { get; set; }
		[XmlElement("messages")] //For receiving MHAccess messages
		public string Messages { get; set; }
		public override string ToString()
		{
			return string.Format("Status: {0}; State: {1}; ErrorCode: {2}; ErrorMessage: {3}", Status, State, ErrorCode, ErrorMessage);
		}
	}
}