using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public static class StopPhraseMatcher
	{
		public static IEnumerable<ZString> FindMatchingStopPhrasesPluralized(ZString text, string[] stopPhrases)
		{
			return FindMatchingStopPhrases(text, stopPhrases, (textWithoutPunctuation, stopPhraseWithSpacePrefix) => StopWordFoundCondition(textWithoutPunctuation, stopPhraseWithSpacePrefix));

			ZBool StopWordFoundCondition(ZString textToCheck, ZString stopPhrase)
			{
				var pluarlisedPhrase = GetPluralisedPhrase(stopPhrase);
				var stringsToCheckFor = new ZString[]
				{
					stopPhrase.AddPrefixSuffix(ZString.Empty, " "),
					stopPhrase.AddPrefixSuffix(ZString.Empty, (NoResString)"s "),	// Plural version of the string
					stopPhrase.AddPrefixSuffix(ZString.Empty, (NoResString)"es ")	// Plural version of the string
				};

				return stringsToCheckFor.Any(s => textToCheck.Contains(s, StringComparison.OrdinalIgnoreCase))
					|| (!pluarlisedPhrase.IsEmpty && textToCheck.Contains(pluarlisedPhrase.AddPrefixSuffix(ZString.Empty, " "), StringComparison.OrdinalIgnoreCase));
			}
		}

		public static IEnumerable<ZString> FindMatchingStopPhrasesExact(ZString text, string[] stopPhrases)
		{
			return FindMatchingStopPhrases(text, stopPhrases, (textWithoutPunctuation, stopPhraseWithSpacePrefix) => textWithoutPunctuation.Contains(stopPhraseWithSpacePrefix.AddPrefixSuffix(ZString.Empty, " "), StringComparison.OrdinalIgnoreCase));
		}

		static IEnumerable<ZString> FindMatchingStopPhrases(ZString text, string[] stopPhrases, Func<ZString, ZString, ZBool> stopWordFoundCondition)
		{
			var result = new List<ZString>();
			if (stopPhrases.Length > 0 && !text.IsEmpty)
			{
				var textWithoutPunctuation = AddPrefixSuffix(WhitespaceAndPunctuationRegex.Replace(text, " "), " ", " ");
				var textWithoutPunctuationLength = textWithoutPunctuation.Length;
				foreach (ZString stopPhrase in stopPhrases)
				{
					var stopPhraseWithSpacePrefix = stopPhrase.AddPrefixSuffix(" ", ZString.Empty);
					if (stopPhraseWithSpacePrefix.Length <= textWithoutPunctuationLength)
					{
						if (stopWordFoundCondition.Invoke(textWithoutPunctuation, stopPhraseWithSpacePrefix))
						{
							result.Add(stopPhrase);
						}
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Strings that end in y and are not all pluralised, Check ends in 'y', Adding a standard pluralised version")]
		static ZString GetPluralisedPhrase(ZString phrase)
		{
			var result = ZString.Empty;
			var suffixesToCheckFor = new ZString[] { (NoResString)"ay", (NoResString)"ey", (NoResString)"iy", (NoResString)"oy", (NoResString)"uy" };

			if (phrase.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !suffixesToCheckFor.Any(s => phrase.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
			{
				result = phrase.TrimEnd('y') + (NoResString)"ies";
			}
			return result;
		}

		static ZString AddPrefixSuffix(this ZString text, ZString prefix, ZString suffix) => string.Concat(prefix, text, suffix);

		static readonly Regex WhitespaceAndPunctuationRegex = new Regex(@"[ \t\W]+", RegexOptions.Compiled);
	}
}
