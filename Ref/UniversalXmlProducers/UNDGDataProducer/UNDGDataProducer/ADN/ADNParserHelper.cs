using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
	public static class ADNParserHelper
	{
		public static byte ConvertStringToByte(string str)
		{
			var res = 0;
			foreach (var c in str)
			{
				if (c < '0' || c > '9')
				{
					throw new ApplicationException($"Expect valid byte string; value: {str}");
				}
				res = res * 10 + (c - '0');
			}

			if (res > 255)
			{
				throw new ApplicationException($"Expect valid byte string; value: {str}");
			}

			return (byte)res;
		}

		public static int ConvertCSVIndexToInt(string id)
		{
			if (id.Length == 0)
			{
				throw new ApplicationException($"Expect non empty CSV index string");
			}

			var res = 0;
			var base_val = 1;
			for (int i = 0; i < id.Length; i++)
			{
				var cur = id[id.Length - 1 - i];
				if (cur < 'A' || cur > 'Z')
				{
					throw new ApplicationException($"Expect valid CSV index string; value {id}");
				}

				var order = (i == 0 ? cur - 'A' : cur - 'A' + 1);
				res += order * base_val;
				base_val *= 26;
			}
			return res;
		}

		public static void CreateADNVariant(IEnumerable<UNDGSubstanceADN> substances)
		{
			var variants = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

			foreach (var subs in substances.GroupBy(x => x.ADN_UNNO))
			{
				if (subs.Count() == 1)
				{
					subs.ElementAt(0).ADN_Variant = "";
				}
				else
				{
					for (var i = 0; i < subs.Count(); i++)
					{
						subs.ElementAt(i).ADN_Variant = variants[i];
					}
				}
			}
		}

		public static Tuple<string, string>[] ParseNameAndDescription(string str)
		{
			var variants = Regex.Split(str, @"\s*or\s(?![a-z])");
			var result = variants.Select(variant => ParseSingleVariant(variant)).ToArray();

			for (var i = 0; i <  result.Length; i++)
			{
				if (string.IsNullOrEmpty(result[i].Item2))
				{
					result[i] = Tuple.Create(result[i].Item1, result[result.Length - 1].Item2);
				}
			}
			return result;
		}

		static Tuple<string, string> ParseSingleVariant(string str)
		{
			var words = str.Split(' ');
			for (var i = 0; i < words.Length; i++)
			{
				if (i > 0 && HasLowerCaseCharacter(words[i]) && !IsSpecialWord(words[i]))
				{
					var index = str.IndexOf(words[i], StringComparison.Ordinal);
					return Tuple.Create(str.Substring(0, index).TrimEnd(' ', ','), str.Substring(index));
				}
			}

			return Tuple.Create(str, string.Empty);
		}

		static bool HasLowerCaseCharacter(string str)
		{
			foreach (var ch in str)
			{
				if (ch >= 'a' && ch <= 'z')
				{
					return true;
				}
			}

			return false;
		}

		static bool IsSpecialWord(string str)
		{
			return str.Contains(")") || str.Contains("-o-");
		}
	}
}
