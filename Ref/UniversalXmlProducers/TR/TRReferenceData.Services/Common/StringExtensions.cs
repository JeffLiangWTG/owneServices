using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Fastenshtein;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class StringExtensions
	{
		public static string[] EmptyIfNoInvalid(this IEnumerable<string> input) => Enumerable.ToArray(input.Where(t => !string.IsNullOrWhiteSpace(t)));

		public static string ClearWhiteSpaces(this string input) => Regex.Replace(input, @"\s", string.Empty);

		static readonly Regex LineBreakRegex = new Regex(@"[\r\n]+");
		const string Whitespace = " ";
		public static string ReplaceLineBreaks(this string input) => LineBreakRegex.Replace(input, Whitespace);

		static readonly Regex SymbolRegex = new Regex(@"[\W_]*");
		public static string ToSearchKey(this string input) => SymbolRegex.Replace(input, string.Empty).ToUpper(CultureInfo.InvariantCulture);

		public static bool SameSearchKeyWith(this string input, string toCompare) => input.ToSearchKey() == toCompare.ToSearchKey();

		public static string ToPureTariffString(this string input) => Regex.Replace(input, @"[\D]", string.Empty);

		public static string[] SplitBy(this string input, string separator = null)
		{
			if (string.IsNullOrEmpty(input))
				return Array.Empty<string>();

			string[] separators = separator == null
				? new[] { ",", "." }
				: new[] { separator };

			return input.ClearWhiteSpaces().Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		}



		public static decimal GetSimilarity(this string searchKey1, string searchKey2)
		{
			var minLen = Math.Min(searchKey1.Length, searchKey2.Length);
			var maxLen = Math.Max(searchKey1.Length, searchKey2.Length);

			if (maxLen < 10 && maxLen + minLen < 10 || (maxLen - (double)minLen) / maxLen > 0.5)
			{
				return 0;
			}
			else
			{
				var editDistance = (decimal)Levenshtein.Distance(searchKey1, searchKey2);
				var similarity = (maxLen - editDistance) / maxLen;
				return similarity;
			}
		}
	}
}
