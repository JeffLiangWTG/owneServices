using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class Extensions
	{
		public static RowData GetSafe(this Dictionary<string, RowData> dictionary, string key)
		{
			return dictionary.TryGetValue(key, out var value) ? value : null;
		}

		public static IEnumerable<RowData> GetSafe(this Dictionary<string, IEnumerable<RowData>> dictionary, string key)
		{
			return dictionary.TryGetValue(key, out var value) ? value : Enumerable.Empty<RowData>();
		}

		public static string PreOperateCondition(this string input)
		{
			return Regex.Replace(input, @"[\/\.]", string.Empty);
		}

		public static bool FitsConfig(this string path, ExcelReadConfiguration readConfig)
		{
			var fileName = Path.GetFileName(path);
			var configFileMark = readConfig.FileIndex.ToString("0#", CultureInfo.InvariantCulture);
			return fileName.StartsWith(configFileMark, StringComparison.Ordinal)
				|| fileName.Replace("（慧咨）", string.Empty).StartsWith(configFileMark, StringComparison.Ordinal)
				|| Regex.Replace(fileName, @"^（[^）]+）", string.Empty).StartsWith(configFileMark, StringComparison.Ordinal);
		}

		public static DateTime OrMaxSmallDateTime(this DateTime input)
		{
			return input < GlobalOption.Instance.MaxSmallDateTime ? input : GlobalOption.Instance.MaxSmallDateTime;
		}

		public static DateTime MinOrMaxSmallDateTime(this DateTime input1, DateTime input2)
		{
			var result = input1 < input2 ? input1 : input2;
			return result.OrMaxSmallDateTime();
		}

		public static string FallbackTo(this string input, string fallbackTo)
		{
			return string.IsNullOrEmpty(input) ? fallbackTo : input;
		}
	}
}
