using System.Collections.Generic;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class PreferenceCreator : IPreferenceCreator
	{
		static string[] emptyStringArray = new string[] { string.Empty };

		public IEnumerable<string> Get(measure measure)
		{
			switch (measure.measureType)
			{
				case "103":
					return new[] { "100", "150" };
				case "112":
					return new[] { "110" };
				case "115":
					return new[] { "115" };
				case "119":
					return new[] { "119" };
				case "122":
					return new[] { "120", "125", "128" };
				case "123":
					return new[] { "123" };
				case "105":
				case "117":
					return new[] { "140" };
				case "142":
					return new[] { "200", "300" };
				case "143":
					return new[] { "220", "225", "320", "325" };
				case "146":
					return new[] { "223", "323" };
				case "145":
					return new[] { "240", "340" };
				case "141":
					return new[] { "310" };
				case "106":
					return new[] { "400" };
				case "147":
					return new[] { "420" };
				default:
					return emptyStringArray;
			}
		}
	}
}
