using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class RateUOMGenerator
	{
		const string regexGetUOMValueBetweenSquareBrackets = @"(?<=\[).+?(?=\])";

		public static RefCusRateUOM[] GenerateRateUomRecords(string generatedFormula)
		{
			Argument.NotNullOrEmpty(generatedFormula, nameof(generatedFormula));
			var result = new List<RefCusRateUOM>();

			foreach (var match in Regex.Matches(generatedFormula, regexGetUOMValueBetweenSquareBrackets))
			{
				if (!result.Any(x => x.ZXG_UOM == match.ToString()))
				{
					result.Add(new RefCusRateUOM()
					{
						ZXG_UOM = match.ToString()
					});
				}
			}
			return result.ToArray();
		}
	}
}
