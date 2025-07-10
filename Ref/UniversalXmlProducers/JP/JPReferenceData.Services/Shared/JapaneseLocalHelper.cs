using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class JapaneseLocalHelper
	{
		public static string ConvertFullWidthToHalfWidth(string record)
		{
			char[] c = record.ToCharArray();
			for (int i = 0; i < c.Length; i++)
			{
				if (c[i] == 12288)
				{
					c[i] = (char)32;
					continue;
				}
				if (c[i] > 65280 && c[i] < 65375)
				{
					c[i] = (char)(c[i] - 65248);
				}
			}
			return new string(c);
		}

		public static (bool, DateTime) RetrieveTimeFromDescritpionWithJapaneseEras(string description)
		{
			var regex = new Regex(@"(?<Name>(平成|令和))(?<Year>[0-9]{1,2})年(?<Month>[0-9]{1,2})月(?<Day>[0-9]{1,2})日(?<Direction>.{2})");
			var match = regex.Match(description);
			if (match.Success)
			{
				var matchGroup = match.Groups;
				var startYear = matchGroup["Name"].Value == "平成" ? HeiseiStartYear : ReiwaStartYear;
				var year = int.Parse(matchGroup["Year"].Value, JapanCultureInfo) + startYear;
				var month = int.Parse(matchGroup["Month"].Value, JapanCultureInfo);
				var day = int.Parse(matchGroup["Day"].Value, JapanCultureInfo);
				var result = new DateTime(year, month, day);

				switch (matchGroup["Direction"].Value)
				{
					case "以前":
						return (false, result);
					case "以降":
						return (true, result);
					default:
						throw new ArgumentException("The input description does not contain '以前' or '以降'.");
				}
			}
			else
			{
				return (false, DateTime.MaxValue);
			}
		}

		const int HeiseiStartYear = 1989;
		const int ReiwaStartYear = 2019;

		public static CultureInfo JapanCultureInfo => new CultureInfo("ja-JP", true);
	}
}
