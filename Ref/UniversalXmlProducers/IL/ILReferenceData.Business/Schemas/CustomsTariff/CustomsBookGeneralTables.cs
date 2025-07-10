using System.Collections.Generic;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff
{
	public class CustomsBookGeneralTables
	{
		[XmlElement(ElementName = "CustomsItemComputedData")]
		public List<CustomsItemComputedData> CustomsItemComputedData { get; set; }

		[XmlElement(ElementName = "PropertiesDetailsHistory")]
		public List<PropertiesDetailsHistory> PropertiesDetailsHistory { get; set; }

		[XmlElement(ElementName = "CustomsItemDetailsHistory")]
		public List<CustomsItemDetailsHistory> CustomsItemDetailsHistory { get; set; }
	}
}
