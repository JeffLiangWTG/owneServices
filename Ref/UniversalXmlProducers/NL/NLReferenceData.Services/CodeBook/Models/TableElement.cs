using System;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	[Serializable]
	[XmlType("elm")]
	public class TableElement
	{
		[XmlElement("ecd")]
		public string elementCode { get; set; }
		[XmlElement("oms")]
		public string elementDescription { get; set; }
		[XmlElement("wet")]
		public string elementLegalDescription { get; set; }
	}
}
