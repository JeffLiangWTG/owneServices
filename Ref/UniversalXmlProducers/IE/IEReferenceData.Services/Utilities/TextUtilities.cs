using System;
using System.Linq;
using System.Text.RegularExpressions;
using Fastenshtein;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class TextUtilities
	{
		public static string GetSearchKey(this string[] strings)
		{
			var allWords = strings.Cast<string>()
				.Select(str => str.ToUpperInvariant().Trim())
				.Select(str => Regex.Replace(str, @"[^a-zA-Z0-9().%]+", " "))
				.SelectMany(str => str.Split(" "))
				.Where(str => !string.IsNullOrWhiteSpace(str))
				.ToList();

			var result = string.Join(" ", allWords);
			return result;
		}

		public static decimal GetSimilarity(this string searchKey1, string searchKey2)
		{
			var minLen = Math.Min(searchKey1.Length, searchKey2.Length);
			var maxLen = Math.Max(searchKey1.Length, searchKey2.Length);

			if (maxLen < 10 && maxLen + minLen < 10
				|| (maxLen - (double)minLen) / maxLen > 0.5
			)
			{
				return 0;
			}
			else
			{
				var key1Scope = searchKey1.GetDecimalScope();
				var key2Scope = searchKey2.GetDecimalScope();

				if (key1Scope.Valid != key2Scope.Valid)
				{
					return 0;
				}
				else if (key1Scope.Valid)
				{
					if (!Contains(
						(key1Scope.From, key1Scope.To),
						(key2Scope.From, key2Scope.To)
					))
					{
						return 0;
					}
				}

				var editDistance = (decimal)Levenshtein.Distance(searchKey1, searchKey2);
				var similarity = (maxLen - editDistance) / maxLen;
				return similarity;
			}
		}

		static bool Contains((decimal from, decimal to) scope1, (decimal from, decimal to) scope2) =>
			scope1.from <= scope2.from && scope1.to >= scope2.to
			|| scope2.from <= scope1.from && scope2.to >= scope1.to;

		public static string GetSearchKey(this string input) => GetSearchKey(new[] { input });

		public static (bool Valid, decimal From, decimal To) GetDecimalScope(this string clearedSearchKey)
		{
			bool valid = false;
			decimal from = decimal.MinValue;
			decimal to = decimal.MaxValue;

			var fromRegex = new Regex(@"(?<!NOT )(EXCEEDING) (\d+(\.\d+)?)%?").Match(clearedSearchKey);
			if (fromRegex.Success && decimal.TryParse(fromRegex.Groups[2].Value, out var fromValue))
			{
				valid = true;
				from = fromValue;
			}

			var toRegex = new Regex(@"(NOT EXCEEDING) (\d+(\.\d+)?)%?").Match(clearedSearchKey);
			if (toRegex.Success && decimal.TryParse(toRegex.Groups[2].Value, out var toValue))
			{
				valid = true;
				to = toValue;
			}

			return (valid, from, to);
		}

		public static string IfEmpty(this string input, Func<string> replacement) => string.IsNullOrWhiteSpace(input) ? replacement() : input;
	}
}
