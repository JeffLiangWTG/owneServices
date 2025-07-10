using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusRateUOM
	{
		[XmlElement(ElementName = "ZXG_UOM")]
		public string UOM { get; set; }

		internal RefCusRateUOM()
		{ }

		public RefCusRateUOM(string uomCode)
		{
			UOM = uomCode;
		}
	}
}
