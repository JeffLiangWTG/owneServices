using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusApplicability
	{
		[XmlElement(ElementName = "ZZT_StartDate")]
		public string StartDate { get; set; }

		[XmlElement(ElementName = "ZZT_AdditionalCode")]
		public string AdditionalCode { get; set; }

		[XmlElement(ElementName = "ZZT_ZZA_NKTradeGroup")]
		public string TradeGroup { get; set; }

		[XmlElement(ElementName = "RefCusExcludedTradeGroup")]
		public List<RefCusExcludedTradeGroup> ExcludedTradeGroups { get; set; }

		internal RefCusApplicability()
		{ }

		public RefCusApplicability(string tradeGroup, string additionalCode, DateTime startDate, List<RefCusExcludedTradeGroup> excludedTradeGroups)
		{
			StartDate = startDate.ToString("s");
			AdditionalCode = additionalCode ?? string.Empty;
			TradeGroup = tradeGroup;
			if (excludedTradeGroups != null)
			{
				ExcludedTradeGroups = excludedTradeGroups;
			}
			else
			{
				ExcludedTradeGroups = new List<RefCusExcludedTradeGroup>();
			}
		}
	}
}
