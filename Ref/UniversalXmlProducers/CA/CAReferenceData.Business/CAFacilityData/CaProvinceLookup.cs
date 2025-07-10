using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData
{
	static class CaProvinceLookup
	{
		static Dictionary<string, string> ProvinceList = new Dictionary<string, string>()
		{
			{ "Alberta", "AB" },
			{ "British Columbia", "BC" },
			{ "Manitoba", "MB" },
			{ "New Brunswick", "NB" },
			{ "Newfoundland and Labrador", "NL" },
			{ "Northwest Territories", "NT" },
			{ "Nova Scotia", "NS" },
			{ "Nunavut", "NU" },
			{ "Ontario", "ON" },
			{ "Prince Edward Island", "PE" },
			{ "Quebec", "QC" },
			{ "Saskatchewan", "SK" },
			{ "Yukon", "YT" },
		};
		public static string GetProvince(string provinceAbbreviation)
		{
			if (ProvinceList.ContainsKey(provinceAbbreviation))
			{
				return ProvinceList[provinceAbbreviation];
			}
			return string.Empty;
		}
	}
}
