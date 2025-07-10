using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAOfficeCode
{
	public static class CAProvinceLookup
	{
		static Dictionary<string, string> ProvinceList = new Dictionary<string, string>()
		{
			{ "0", "NS" },
			{ "1", "PE" },
			{ "2", "NB" },
			{ "3", "QC" },
			{ "4", "ON" },
			{ "5", "MB" },
			{ "6", "SK" },
			{ "7", "AB" },
			{ "8", "BC" },
			{ "9", "NL" },
		};

		public static string GetProvince(string officeCode)
		{
			var secondValue = officeCode.Substring(1, 1);
			if (secondValue == "5" ||  secondValue == "8")
			{
				var lastThreeValue = officeCode.Substring(1);
				if (Regex.IsMatch(lastThreeValue, @"^51[2-5]$"))
				{
					return "NT";
				}

				if (new string[] { "890", "892", "894" }.Contains(lastThreeValue))
				{
					return "YT";
				}
			}

			if (ProvinceList.ContainsKey(secondValue))
			{
				return ProvinceList[secondValue];
			}
			return string.Empty;
		}
	}
}
