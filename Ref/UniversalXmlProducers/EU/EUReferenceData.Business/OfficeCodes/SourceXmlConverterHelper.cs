using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business
{
	public static class SourceXmlConverterHelper
	{
		public static string GetLanguageOfCountry(string country)
		{
			if (countryLanguageMap.TryGetValue(country.ToUpper(CultureInfo.InvariantCulture), out var language))
			{
				return language;
			}
			return string.Empty;
		}

		public static string GetTransportMode(string traTyp71)
		{
			switch (traTyp71.ToUpper(CultureInfo.InvariantCulture))
			{
				case "V":
					return "RAI";
				case "R":
					return "ROA";
				case "C":
					return "INW";
				case "P":
					return "SEA";
				case "AIR":
					return "AIR";
				default:
					return string.Empty;
			}
		}

		static readonly Dictionary<string, string> countryLanguageMap = new Dictionary<string, string>
		{
			{ "AD", "ES" },
			{ "AT", "DE" },
			{ "BE", "FR" },
			{ "BG", "BG" },
			{ "HR", "HR" },
			{ "CY", "EL" },
			{ "CZ", "CS" },
			{ "DK", "DA" },
			{ "EE", "ET" },
			{ "FI", "ES" },
			{ "FR", "FR" },
			{ "DE", "DE" },
			{ "GR", "GR" },
			{ "HU", "HU" },
			{ "IS", "IS" },
			{ "IT", "IT" },
			{ "LV", "LV" },
			{ "LT", "LT" },
			{ "LU", "FR" },
			{ "MK", "MK" },
			{ "NL", "NL" },
			{ "NO", "NO" },
			{ "PL", "PL" },
			{ "PT", "PT" },
			{ "RO", "RO" },
			{ "SM", "IT" },
			{ "RS", "SR" },
			{ "SK", "SK" },
			{ "SI", "SL" },
			{ "ES", "ES" },
			{ "SE", "SV" },
			{ "CH", "DE" },
			{ "TR", "TR" },
		};
	}
}
