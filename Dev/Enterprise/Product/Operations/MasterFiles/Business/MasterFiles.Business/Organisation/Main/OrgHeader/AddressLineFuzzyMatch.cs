using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class AddressLineFuzzyMatch
	{
		public static ZString GetStringForFuzzyComparing(ZString text)
		{
			if (text.KeepAlphabeticCharacters().IsEmpty)
			{
				return ZString.Empty;
			}

			ZString result = GetFromToughRegex(text);

			if (result.IsEmpty || result.KeepAlphabeticCharacters().IsEmpty)
			{
				result = GetFromRegexList(text);
			}
			if (!result.IsEmpty && !result.Trim().Contains(" ") && text.Trim().Contains(" "))
			{
				result = GetFromWordsWithoutRegex(text);
			}
			return Regex.Replace(result, regularExpressionToRemove, "").ToUpper();
		}

		static ZString GetFromToughRegex(ZString text)
		{
			ZString result = ZString.Empty;
			MatchCollection matches = ToughRegex.Matches(text);
			foreach (Match match in matches)
			{
				result += " " + match.Value;
			}
			return result;
		}

		static ZString GetFromRegexList(ZString text)
		{
			ZString result = ZString.Empty;
			ZString tmp = ZString.Empty;
			foreach (var r in Regexes)
			{
				tmp = r.Match(text).Value;
				if (tmp.Length > result.Length)
				{
					result = tmp;
				}
			}
			return result;
		}

		static ZString GetFromWordsWithoutRegex(ZString text)
		{
			ZString result = ZString.Empty;
			ZString[] words = text.Split(' ');
			for (int i = 0; i < 2; i++)
			{
				result += words[i];
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular expression")]
		static IEnumerable<string> RegexStrings
		{
			get
			{
				//Regex to match first numeric word followed by numbers and symbols and ending at the first word
				yield return (NoResString)@"[\d\W]+[^\d\W]+";
												//Regex as above but allowing letters in street number like unit 3a 
				yield return (NoResString)@"[\d\W]+[^\d\W]{0,2}[\d\W]+[^\d\W]*";
																   //Regex to match first non-numeric word and all previous numbers + symbols
				yield return @"[\d\W]*[^\d\W]+";
												//Regex to match European address format like Main street 5, apt 1
				yield return @"[a-zA-Z\s]+[\d]+[^\d\W]{0,2}[\W]*[\w\s]*[\d\W]";
			}
		}

		//Regex to match something really tough like Hong Kong addresses. This works good but very precise, so need other regexes as may return empty results if address is very incorrect
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular expression")]
		const string ToughRegexString = @"[\d]+[\W]*[\w]*[\W\d\W]*[\w]*";

		static Regex ToughRegex
		{
			get
			{
				if (toughRegex == null)
				{
					toughRegex = new Regex(ToughRegexString, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				}
				return toughRegex;
			}
		}

		[ThreadStatic]
		static Regex toughRegex;

		const string regularExpressionToRemove = @"\s";

		static List<Regex> Regexes
		{
			get
			{
				if (regexes == null)
				{
					regexes = new List<Regex>();
					foreach (string r in RegexStrings)
					{
						regexes.Add(new Regex(r, RegexOptions.IgnoreCase | RegexOptions.Compiled));
					}
				}
				return regexes;
			}
		}

		[ThreadStatic]
		static List<Regex> regexes;
	}
}
