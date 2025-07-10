using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using static CargoWise.RefDbRepo.KRReferenceData.Business.Constants;
using RefCusNomenclatureLanguage = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusNomenclatureLanguage;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class NomenclaturePDFParser
	{
		public NomenclaturePDFParser(List<RefCusNomenclatureGroup> nomenclatures)
		{
			this.nomenclatures = nomenclatures;
		}
		readonly List<RefCusNomenclatureGroup> nomenclatures;

		public void Update(string dataFilePath)
		{
			using (var pdf = new PdfReader(dataFilePath))
			using (var pdfDocument = new PdfDocument(pdf))
			{
				string currentPageText = null;
				string nextPageText = null;

				for (var page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
				{
					if (currentPageText == null)
					{
						currentPageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), new SimpleTextExtractionStrategy());
					}

					SetKRDescriptionsForSectionAndChapters(currentPageText, new NomenclatureProperties() { Pattern = SectionPattern, CompositeKeyLength = 2, StartingWordOfEnglishDescription = Constants.NomenclatureCategories.Section });

					SetKRDescriptionsForSectionAndChapters(currentPageText, new NomenclatureProperties() { Pattern = ChapterPattern, CompositeKeyLength = 5, StartingWordOfEnglishDescription = Constants.NomenclatureCategories.Chapter });

					nextPageText = !IsLastPage(page, pdfDocument) ? PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page + 1), new SimpleTextExtractionStrategy()) : null;
					SetKRDescriptionsForSubchapters(currentPageText, nextPageText);

					currentPageText = nextPageText;
				}
			}
		}

		static bool IsLastPage(int currentPage, PdfDocument document) => currentPage == document.GetNumberOfPages();

		class NomenclatureProperties
		{
			public string Pattern { get; set; }
			public int CompositeKeyLength { get; set; }
			public string StartingWordOfEnglishDescription { get; set; }
		}

		void SetKRDescriptionsForSectionAndChapters(string currentText, NomenclatureProperties properties)
		{
			var match = Regex.Match(currentText, properties.Pattern, RegexOptions.Singleline);
			if (!match.Success)
			{
				if (properties.Pattern.StartsWith("\\n", System.StringComparison.Ordinal))
				{
					var regExWithoutLineFeed = properties.Pattern.Substring(2);
					if (Regex.IsMatch(currentText, $"^{regExWithoutLineFeed}"))
					{
						match = Regex.Match(currentText, regExWithoutLineFeed, RegexOptions.Singleline);
					}
				}
			}
			while (match.Success)
			{
				var title = match.Groups[1].Value.Trim();
				var remainingTextAfterTitle = match.Groups[2].Value;

				var nomenclature = nomenclatures.SingleOrDefault(x => x.ZZ5_Value == title.PadLeft(2, '0') && x.ZZ5_CompositeKey.Length == properties.CompositeKeyLength);
				if (nomenclature != null)
				{
					var koreanDescription = GetKoreanDescription(remainingTextAfterTitle, properties.StartingWordOfEnglishDescription);
					if (!string.IsNullOrEmpty(koreanDescription))
					{
						SetKoreanDescriptionTo(nomenclature, koreanDescription);
					}
				}
				match = Regex.Match(remainingTextAfterTitle, properties.Pattern, RegexOptions.Singleline);
			}
		}

		void SetKRDescriptionsForSubchapters(string currentPageText, string nextPageText)
		{
			var match = Regex.Match(currentPageText, SubChapterPattern, RegexOptions.Singleline);
			while (match.Success)
			{
				var title = match.Groups[1].Value.Trim();
				var remainingTextAfterTitle = match.Groups[2].Value;

				var chapter = string.Empty;
				var romanNumeralTitle = IntToRomanNumeral(int.Parse(title, CultureInfo.CurrentCulture));
				var patternOfFirstNomenclatureOfThisSubChapter = new Regex(@"[0-9]{4}\s[0-9]*");
				var chapterInfo = patternOfFirstNomenclatureOfThisSubChapter.Match(remainingTextAfterTitle);
				if (!chapterInfo.Success && !string.IsNullOrEmpty(nextPageText))
				{
					chapterInfo = patternOfFirstNomenclatureOfThisSubChapter.Match(nextPageText);
				}
				chapter = chapterInfo.Value.Substring(0, 2);

				var nomenclature = nomenclatures.SingleOrDefault(x => x.ZZ5_Value == romanNumeralTitle && x.ZZ5_CompositeKey.Substring(3, 2) == chapter);
				if (nomenclature != null)
				{
					var startingWordOfEnglishDescription = GetCorrespondingEnglishDescriptionCharacter(romanNumeralTitle);
					var koreanDescription = GetKoreanDescription(remainingTextAfterTitle, startingWordOfEnglishDescription);
					if (!string.IsNullOrEmpty(koreanDescription))
					{
						SetKoreanDescriptionTo(nomenclature, koreanDescription);
					}
				}
				match = Regex.Match(remainingTextAfterTitle, SubChapterPattern, RegexOptions.Singleline);
			}
		}

		static void SetKoreanDescriptionTo(RefCusNomenclatureGroup nomenclature, string koreanDescription)
		{
			nomenclature.RefCusNomenclatureLanguages = new RefCusNomenclatureLanguage[]
			{
				new RefCusNomenclatureLanguage
				{
					ZX8_ZX6_NKLanguage = Constants.LanguageCodes.Korean,
					ZX8_Description = koreanDescription
				}
			};
		}

		static string GetKoreanDescription(string remainingTextAfterTitle, string startingWordOfEnglishDescription)
		{
			var result = string.Empty;
			var indexOfEnglishDescription = remainingTextAfterTitle.IndexOf(startingWordOfEnglishDescription, System.StringComparison.Ordinal);
			if (indexOfEnglishDescription > 0)
			{
				result = remainingTextAfterTitle.Substring(0, indexOfEnglishDescription - 1).Replace("\n", " ").Trim();
			}
			return result;
		}

		string GetCorrespondingEnglishDescriptionCharacter(string title)
		{
			var result = string.Empty;

			if (!RomanNumeralSymbols.TryGetValue(title, out result))
			{
				result = title;
			}
			return result;
		}

		public static string IntToRomanNumeral(int number)
		{
			var i = RomanNumericCharacters.I.ToCharArray()[0];
			return new string(i, number)
					.Replace(new string(i, 100), RomanNumericCharacters.C)
					.Replace(new string(i, 90), RomanNumericCharacters.XC)
					.Replace(new string(i, 50), RomanNumericCharacters.L)
					.Replace(new string(i, 40), RomanNumericCharacters.XL)
					.Replace(new string(i, 10), RomanNumericCharacters.X)
					.Replace(new string(i, 9), RomanNumericCharacters.IX)
					.Replace(new string(i, 5), RomanNumericCharacters.V)
					.Replace(new string(i, 4), RomanNumericCharacters.IV);
		}

		readonly Dictionary<string, string> RomanNumeralSymbols = new Dictionary<string, string>()
		{
			{ "I", "Ⅰ" }, { "II", "Ⅱ" }, { "III", "Ⅲ" }, { "IV", "Ⅳ" }, { "V", "Ⅴ" }, { "VI", "Ⅵ" }, { "VII", "Ⅶ" }, { "VIII", "Ⅷ" }, { "IX", "Ⅸ" }, { "X", "Ⅹ" }
		};

		const string SectionPattern = @"\n\uc81c([0-9]{1,2})\ubd80\s(.+)";
		const string ChapterPattern = @"\n\uc81c([0-9]{1,2})\ub958\s(.+)";
		const string SubChapterPattern = @"\n\uc81c([0-9]*)\uc808\s(.+)";
	}
}
