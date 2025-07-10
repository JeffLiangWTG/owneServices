using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusExcludedTradeGroup
	{
		[XmlElement(ElementName = "ZZC_ZZA_NKTradeGroup")]
		public string TradeGroup { get; set; }

		internal RefCusExcludedTradeGroup()
		{
		}

		public RefCusExcludedTradeGroup(string tradeGroup)
		{
			TradeGroup = tradeGroup;
		}
	}
}
