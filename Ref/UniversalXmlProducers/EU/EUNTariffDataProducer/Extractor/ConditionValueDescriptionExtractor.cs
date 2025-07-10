using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ConditionValueDescriptionExtractor : IConditionValueDescriptionExtractor
	{
		public string GetComment(string measureConditionCode)
		{
			var description = "";

			if (Map.ContainsKey(measureConditionCode))
			{
				description = Map[measureConditionCode];
			}

			if (string.IsNullOrEmpty(description))
			{
				description = measureConditionCode;
			}

			return description;
		}

		static readonly IDictionary<string, string> Map = new Dictionary<string, string>
		{
			{ "A", "Condition A:Presentation of an anti-dumping/countervailing document" },
			{ "B", "Condition B:Presentation of a certificate/licence/document" },
			{ "C", "Condition C:Presentation of a certificate/licence/document" },
			{ "D", "Condition D:Intended for processing" },
			{ "E", "Condition E:The quantity or the price per unit declared, as appropriate, is equal or less than the specified max" },
			{ "F", "Condition F:The net free at frontier price before duty must be equal to or greater than the minimum price (see c" },
			{ "G", "Condition G:The CIF price plus the duty to be paid/ton must be equal to or greater than the minimum price (see c" },
			{ "H", "Condition H:Presentation of a certificate/licence/document" },
			{ "I", "Condition I:The quantity or the price per unit declared, as appropriate, is equal or less than the specified max" },
			{ "K", "Condition K:Also applicable simultaneously with tariff quota shown in the field \"certificates\"" },
			{ "L", "Condition L:CIF price must be higher than the minimum price (see components)" },
			{ "M", "Condition M:Import price must be equal to or greater than the minimum price/reference price (see components)" },
			{ "N", "Condition N:The CIF price before duty must be equal to or greater than the minimum price (see components)" },
			{ "P", "Condition P:Only particular ingredients are eligible for export refund" },
			{ "Q", "Condition Q:Presentation of an endorsed certificate/licence" },
			{ "R", "Condition R:Ratio \"net weight/supplementary unit\" is equal to or higher than the condition amount" },
			{ "S", "Condition S:Lodgement of a security" },
			{ "U", "Condition U:Ratio \"declared value/supplementary unit\" should be higher than the condition amount" },
			{ "V", "Condition V:Import price must be equal to or greater than the entry price (see components)" },
			{ "W", "Condition W:Washington Convention" },
			{ "Y", "Condition Y:Other conditions" },
			{ "YA", "Condition YA:Other conditions" },
			{ "YB", "Condition YB:Other conditions" },
			{ "YC", "Condition YC:Other conditions" },
			{ "YD", "Condition YD:Other conditions" },
			{ "YE", "Condition YE:Other conditions" },
			{ "YF", "Condition YF:Other conditions" },
			{ "YG", "Condition YG:Other conditions" },
			{ "YH", "Condition YH:Other conditions" },
			{ "Z", "Condition Z:Presentation of more than one certificate" }
		};
	}
}
