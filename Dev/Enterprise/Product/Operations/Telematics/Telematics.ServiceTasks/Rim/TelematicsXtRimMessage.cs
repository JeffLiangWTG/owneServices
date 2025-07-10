using System.Xml.Serialization;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	public class TelematicsXtRimMessage
	{
		[XmlAttribute]
		public string TcaBatchId { get; set; }

		[XmlAttribute]
		public string TcaAddress { get; set; }

		[XmlAttribute]
		public string TcaUsername { get; set; }

		[XmlAttribute]
		public string TcaPassword { get; set; }

		[XmlAttribute]
		public string JsonData { get; set; }
	}
}
