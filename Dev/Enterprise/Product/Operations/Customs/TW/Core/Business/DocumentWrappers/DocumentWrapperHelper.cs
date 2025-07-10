using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public static class DocumentWrapperHelper
	{
		public static class ResourcePath
		{
			public const string N5110 = "Enterprise.Customs.TW.Business.DocumentWrappers.N5110.Resources.";
			public const string N5111 = "Enterprise.Customs.TW.Business.DocumentWrappers.N5111.Resources.";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		public static class SayTotalConst
		{
			public const string Cent = "CENT";
			public const string Cents = "CENTS";
			public const string Comma = ",";
			public const string Dollar = "DOLLAR";
			public const string Dollars = "DOLLARS";
			public const string SayTotalFormatCurrency = "USD";
			public const string SayTotalOnly = " ONLY";
			public const string SayTotalString = "SAY TOTAL";
		}

		public static ZString ReplaceLineBreakWithSpace(this ZString input) => Regex.Replace(input, @"(?:\r\n|\n|\r)", " ");

		public static Image GetGovernmentAgencyImage(ZString governmentAgencyID, ZString resourcePath)
		{
			var path = ZString.Empty;
			Image image = null;
			if (resourcePath == ResourcePath.N5110)
			{
				if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9901AndR9920.png";
				}
				else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9921AndR9950(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9921AndR9950.png";
				}
				else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9951AndR9980(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9951AndR9980.png";
				}
				else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9981AndR9999(governmentAgencyID))
				{
					path = "GovernmentAgencyImageForIDIsBetweenR9981AndR9999.png";
				}
			}
			else if (resourcePath == ResourcePath.N5111)
			{
				if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID))
				{
					path = "GovernmentAgencyImage.png";
				}
			}

			if (!path.IsEmpty)
			{
				path = resourcePath + path;
				using (var stream = typeof(DocumentWrapperHelper).Assembly.GetManifestResourceStream(path))
				{
					if (stream != null)
					{
						image = Image.FromStream(stream);
					}
				}
			}
			return image;
		}

		public static ZString GetAddressLine(ZString chineseAddressLine, ZString englishAddressLine)
		{
			var addressLineBuilder = new ZStringBuilder();
			addressLineBuilder.AppendIfNotEmpty(chineseAddressLine);
			addressLineBuilder.AppendIfNotEmpty(englishAddressLine);
			return addressLineBuilder.ToStringWithNewLineBetweenAppends();
		}

		public static ZString GetCheckDigit(ZString barCode)
		{
			var checkDigit = ZString.Empty;
			if (barCode.Length != 42)
			{
				return checkDigit;
			}

			var reg = new Regex(@"[A-Z0-9]{42}");
			if (!reg.IsMatch(barCode))
			{
				return checkDigit;
			}

			var result = 0;
			foreach (var value in barCode)
			{
				result += char.IsLetter(value) ? ZString.AlphabeticCharacters.IndexOf(value) + 10 : ZString.NumericCharacters.IndexOf(value);
			}

			result %= 36;

			if (result >= 10 && result <= 35)
			{
				checkDigit = ZString.AlphabeticCharacters[result - 10].ToString();
			}
			else
			{
				checkDigit = result.ToString(CultureInfo.InvariantCulture);
			}
			return checkDigit;
		}

		public static ZDateTime StringAsDateTime(ZString dateStringValue) => !dateStringValue.IsEmpty && ZDateTime.TryParseISO8601Date(dateStringValue, out var e) ? e : ZDateTime.Invalid;

		public static ZString GetDeclarationIDFormat(ZString declarationID) => declarationID.Length > 13 ? ZString.Format("{0}/{1}/{2}/{3}/{4}", declarationID.Substring(0, 2), declarationID.Substring(2, 2), declarationID.Substring(4, 2), declarationID.Substring(6, 3), declarationID.Substring(9)) : ZString.Empty;

		public static List<ZString> SplitTextByWord(ZString text, int maxLength, bool processChineseChars = false)
		{
			if (processChineseChars)
			{
				Argument.GreaterThan(maxLength, 1, nameof(maxLength));
			}
			else
			{
				Argument.GreaterThan(maxLength, 0, nameof(maxLength));
			}

			var resultList = new List<ZString>();
			var line = ZString.Empty;
			var words = text.Trim().Split(' ');

			foreach (var word in words)
			{
				var chineseCharsCountInLine = 0;
				var chineseCharsCountInWord = 0;
				if (processChineseChars)
				{
					chineseCharsCountInLine = line.Length - line.StripNonWesternEuropeanCharacters().Length;
					chineseCharsCountInWord = word.Length - word.StripNonWesternEuropeanCharacters().Length;
				}

				if (line.Length + chineseCharsCountInLine + word.Length + chineseCharsCountInWord > maxLength)
				{
					line = line.Trim();
					if (!line.IsEmpty)
					{
						resultList.Add(line);
						line = ZString.Empty;
					}

					var lengthToSplit = maxLength;
					if (chineseCharsCountInWord > 0 && lengthToSplit < chineseCharsCountInWord * 2)
					{
						lengthToSplit /= 2;
					}
					var splitWords = word.Split(lengthToSplit);
					var splitWordsLength = splitWords.Length;
					if (splitWordsLength > 0)
					{
						for (var i = 0; i < splitWordsLength - 1; i++)
						{
							resultList.Add(splitWords[i]);
						}
						line = splitWords[splitWordsLength - 1] + " ";
					}
				}
				else
				{
					line += word + " ";
				}
			}

			if (!line.IsEmpty)
			{
				resultList.Add(line.Trim());
			}

			return resultList;
		}

		public static List<ZString> SplitTextByLineBreak(string text, int maxLength, bool processChineseChars = false)
		{
			if (processChineseChars)
			{
				Argument.GreaterThan(maxLength, 1, nameof(maxLength));
			}
			else
			{
				Argument.GreaterThan(maxLength, 0, nameof(maxLength));
			}

			var resultList = new List<ZString>();
			var lineSeparator = new string[] { "\r\n" };
			var lines = text.Split(lineSeparator, StringSplitOptions.RemoveEmptyEntries);
			for (var i = 0; i < lines.Length; ++i)
			{
				var line = lines[i].Trim();
				if (!string.IsNullOrEmpty(line))
				{
					var chineseCharsCount = 0;
					if (processChineseChars)
					{
						chineseCharsCount = line.Length - new ZString(line).StripNonWesternEuropeanCharacters().Length;
					}

					if (line.Length + chineseCharsCount > maxLength)
					{
						var newLines = SplitTextByWord(line, maxLength, processChineseChars);
						resultList.AddRange(newLines);
					}
					else
					{
						resultList.Add(line);
					}
				}
			}
			return resultList;
		}

		public static bool IsTextExceedsMaxChar(ZString text, int maxLength)
		{
			return 2 * text.Length - text.StripNonWesternEuropeanCharacters().Length > maxLength;
		}

		public static (ZString FirstRowText, ZString RemainingText) SplitTextByWidthOnce(ZString text, int width, Font font)
		{
			var resultText = text;

			var currentLineIndex = 0;
			var linebreakIndex = text.IndexOf(System.Environment.NewLine, currentLineIndex, StringComparison.InvariantCultureIgnoreCase);

			int leadingSpacesTrimmed;
			ZString lineToProcess;
			if (linebreakIndex >= 0)
			{
				var untrimmedString = text.SubstringSafe(currentLineIndex, linebreakIndex - currentLineIndex);
				lineToProcess = untrimmedString.TrimStart();
				leadingSpacesTrimmed = untrimmedString.Length - lineToProcess.Length;
			}
			else
			{
				var untrimmedString = text.SubstringSafe(currentLineIndex);
				lineToProcess = untrimmedString.TrimStart();
				leadingSpacesTrimmed = untrimmedString.Length - lineToProcess.Length;
			}

			using (var graphics = Graphics.FromImage(new Bitmap(250, 250)))
			{
				var size = graphics.MeasureString(lineToProcess, font);
				if (size.Width > width)
				{
					var idxL = 0;
					var idxR = lineToProcess.Length - 1;

					while (idxL != idxR)
					{
						var m = (idxL + idxR) / 2;
						if ((idxL + idxR) % 2 > 0)
						{
							m += 1;
						}
						var lineToMeasure = lineToProcess.SubstringSafe(0, m);

						var w2 = graphics.MeasureString(lineToMeasure, font).Width;

						if (w2 > width)
						{
							idxR = m - 1;
						}
						else
						{
							idxL = m;
						}
					}

					lineToProcess = lineToProcess.SubstringSafe(0, idxR);
					var spaceIdx = lineToProcess.LastIndexOf(' ');
					if (spaceIdx >= 0)
					{
						resultText = lineToProcess.SubstringSafe(0, spaceIdx);
						currentLineIndex += spaceIdx + leadingSpacesTrimmed;
					}
					else
					{
						resultText = lineToProcess;
						currentLineIndex += idxR + leadingSpacesTrimmed;
					}
				}
				else
				{
					resultText = lineToProcess;
					currentLineIndex = linebreakIndex >= 0 ? linebreakIndex + 2 : text.Length;
				}
			}

			resultText = resultText.Replace(System.Environment.NewLine, "").Trim();

			return (FirstRowText: resultText, RemainingText: text.SubstringSafe(currentLineIndex));
		}

		public static (List<ZString> SubRowTexts, ZString RemainingText) SplitTextByWidth(ZString text, int width, Font font, int resultsNeeded = -1)
		{
			var resultList = new List<ZString>();
			var textToProcess = Regex.Replace(text, "(?<!\r)\n", "\r\n");
			if (resultsNeeded != 0)
			{
				while (textToProcess.Length > 0)
				{
					var splitOnceResult = SplitTextByWidthOnce(textToProcess, width, font);
					resultList.Add(splitOnceResult.FirstRowText);
					textToProcess = splitOnceResult.RemainingText;

					if (resultsNeeded > 0 && resultsNeeded == resultList.Count)
					{
						break;
					}
				}
			}

			return (SubRowTexts: resultList, RemainingText: textToProcess);
		}

		public static ZString FormatCurrencyMacro(ZDecimal amount, ZString currencyCode, bool withTwoDecimalPlaces = false)
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"<Currency({0},{1})>", withTwoDecimalPlaces ? amount.ToString(2) : amount.ToString(), currencyCode);
		}

		public static ZString FormatFormatNumberMacro(ZDecimal amount, int decimalPlaces)
		{
			return string.Format(CultureInfo.InvariantCulture, "<FormatNumber({0},{1})>", amount.ToString(), decimalPlaces);
		}

		public static ZString FormatTariffNumber(ZString tariffNum)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}-{4}", tariffNum.SubstringSafe(0, 4), tariffNum.SubstringSafe(4, 2), tariffNum.SubstringSafe(6, 2), tariffNum.SubstringSafe(8, 2), tariffNum.SubstringSafe(10, 1));
		}

		public static ZString FormatQuantityNumber(INumericZType num)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:###,##0.#####}", num);
		}

		public static ZString FormatWeightNumber(INumericZType num)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:###,##0.######}", num);
		}

		public static ZString FormatNumber(INumericZType num, int decimalPlace)
		{
			string format = decimalPlace > 0 ? string.Format(CultureInfo.InvariantCulture, "{0}0:###,##0{1}{2}", "{", ".".PadRight(decimalPlace + 1, '0'), "}") : "{0:###,##0}";
			return string.Format(CultureInfo.InvariantCulture, format, num);
		}

		public static ZString FormatNumber(ZDecimal num)
		{
			return FormatNumber(num, num.DecimalPlaces);
		}

		public static ZString CombineTextLines(List<ZString> lines, int startIndex) => CombineTextLines(lines, startIndex, lines.Count - startIndex);

		public static ZString CombineTextLines(List<ZString> lines, int startIndex, int length)
		{
			int maxLength = lines.Count;
			int endIndex = startIndex + length;
			endIndex = endIndex > maxLength ? maxLength : endIndex;
			var result = new ZStringBuilder();
			for (int i = startIndex; i < endIndex; i++)
			{
				result.Append(lines[i]);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		#region GetSayTotalDescription
		public static ZString GetSayTotalDescription(RefCurrency currency, ZDecimal amount)
		{
			var currencyDescription = GetSayTotalCurrencyDescription(currency, amount);
			var amountDescription = GetSayTotalAmountDescription(amount);

			return SayTotalConst.SayTotalString + " " + currencyDescription + " " + amountDescription;
		}

		static ZString GetSayTotalCurrencyDescription(RefCurrency currency, ZDecimal amount)
		{
			var description = currency?.RX_DescMultilingual?.ToString(Enterprise.Core.SharedConstants.Languages.English)?.ToUpper(CultureInfo.CurrentCulture) ?? ZString.Empty;
			if (Convert.ToInt64(amount) > 1)
			{
				description = description.Replace(SayTotalConst.Dollar, SayTotalConst.Dollars);
			}
			return description;
		}

		static ZString GetSayTotalAmountDescription(ZDecimal amount)
		{
			var result = ZString.Empty;
			var sayTotalConvert = new CurrencyToWords_ENG_SAF();
			var sayTotalAmountDescription = sayTotalConvert.ConvertToWords(decimal.ToDouble(amount), SayTotalConst.SayTotalFormatCurrency)?.Replace(SayTotalConst.Comma, "")?.ToUpper(CultureInfo.CurrentCulture) ?? ZString.Empty;
			var cents = Convert.ToInt64((amount - Convert.ToInt64(Math.Floor(amount))) * 100);
			switch (cents)
			{
				case 0:
					result = sayTotalAmountDescription + ".";
					break;
				case 1:
					result = sayTotalAmountDescription.Replace(SayTotalConst.Cents, SayTotalConst.Cent) + SayTotalConst.SayTotalOnly + ".";
					break;
				default:
					result = sayTotalAmountDescription + SayTotalConst.SayTotalOnly + ".";
					break;
			}
			return result;
		}
		#endregion

		public static ZString GetFormatAdditionalDocument(IAdditionalDocument additionalDoc)
		{
			var result = ZString.Empty;
			if (additionalDoc != null)
			{
				result = FormatAdditionalDocument(additionalDoc.ID, additionalDoc.SequenceNumeric);
			}
			return result;
		}

		public static ZString FormatAdditionalDocument(ZString id, ZInt numeric)
		{
			var result = ZString.Empty;
			if (!id.IsEmpty)
			{
				result = numeric.IsEmpty ? id : ZString.Format("{0}-{1}", id, numeric);
			}
			return result;
		}

		public static ZString FormatBarCode(string id) => ZString.Format("*{0}*", id);

		internal static int GetDecimalPlaces(IEnumerable<IGovernmentAgencyGoodsItem> goodsItems, Func<IGovernmentAgencyGoodsItem, ZDecimal> currentDecimal)
		{
			var decimalSeparator = ObjectCache.CultureProvider.Culture.NumberFormat.NumberDecimalSeparator;
			var decimalPlaces = goodsItems.Select((x) =>
			{
				var decimals = currentDecimal(x).ToStringTrimZeros().Split(decimalSeparator[0]);
				return decimals.Length == 2 ? decimals[1].Length : 0;
			});
			return decimalPlaces.Any() ? decimalPlaces.Max() : 0;
		}

		#region GetUNLOCOProperNameWithCountry
		static ZString GetOriginCountryNameOrFallbackToUNLOCO(JobDeclaration declaration) => declaration?.PortOfOriginCountry?.RN_Desc ?? (declaration?.JE_RL_NKOrigin ?? ZString.Empty);

		static ZString GetPortOfOriginProperName(JobDeclaration declaration) => declaration?.PortOfOriginProperName ?? ZString.Empty;

		static ZString GetDestinationCountryNameOrFallbackToUNLOCO(JobDeclaration declaration) => declaration?.FinalDestinationCountry?.RN_Desc ?? (declaration?.JE_RL_NKFinalDestination ?? ZString.Empty);

		static ZString GetFinalDestinationProperName(JobDeclaration declaration) => declaration?.FinalDestinationProperName ?? ZString.Empty;

		static ZString GetProperNameWithCountry(ZString countryName, ZString portName, ZString separator) => string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", countryName, separator, portName);

		public static ZString GetOriginProperNameWithCountry(JobDeclaration declaration, ZString separator) => GetProperNameWithCountry(GetOriginCountryNameOrFallbackToUNLOCO(declaration), GetPortOfOriginProperName(declaration), separator);

		public static ZString GetFinalDestinationProperNameWithCountry(JobDeclaration declaration, ZString separator) => GetProperNameWithCountry(GetDestinationCountryNameOrFallbackToUNLOCO(declaration), GetFinalDestinationProperName(declaration), separator);
		#endregion

		[ThreadSafe]
		static readonly Dictionary<ZString, ZString> packageVolumeUQMapping = new Dictionary<ZString, ZString>
		{
			{ Core.Constants.Volume.CubicMetres, "CBM" },
			{ Core.Constants.Volume.CubicFeet, (NoResString)"Cuft" },
		};

		public static ZString GetDetailInfo(ZDecimal value, ZString unit, ZInt packageQty) => new PackedDetailInfo(value, unit, packageQty, 3).GetDetail(0);

		public static int GetDecimalPlaces(ZString value)
		{
			int result = 0;
			if (value.Contains(ObjectCache.CultureProvider.Culture.NumberFormat.NumberDecimalSeparator, System.StringComparison.OrdinalIgnoreCase))
			{
				result = value.Length - value.LastIndexOf(ObjectCache.CultureProvider.Culture.NumberFormat.NumberDecimalSeparator, System.StringComparison.OrdinalIgnoreCase) - 1;
			}
			return result;
		}

		public static ZString BuildKeyValueStringFromDictionary(IDictionary<ZString, ZDecimal> dictionary)
		{
			return BuildKeyValueStringFromDictionary(dictionary, 0);
		}

		public static ZString BuildKeyValueStringFromDictionary(IDictionary<ZString, ZDecimal> dictionary, int decimalPlace)
		{
			var result = ZString.Empty;
			foreach (var keyValuePair in dictionary)
			{
				if (keyValuePair.Value > 0)
				{
					var formatNumber = decimalPlace == 0 ? FormatNumber(new ZDecimal(ZArchitecture.Core.Utilities.Round(keyValuePair.Value, 3))) : FormatNumber(new ZDecimal(ZArchitecture.Core.Utilities.Round(keyValuePair.Value, 3)), decimalPlace);
					result += string.Format(CultureInfo.InvariantCulture, "{0} {1}\n", formatNumber, keyValuePair.Key);
				}
			}
			return result.TrimEnd().ToString();
		}

		public static ZString BuildKeyValueStringFromDictionaryForPrefix(IDictionary<ZString, ZDecimal> dictionary, string prefix)
		{
			var result = ZString.Empty;
			foreach (var keyValuePair in dictionary)
			{
				if (keyValuePair.Value > 0)
				{
					var formatNumber = FormatNumber(keyValuePair.Value);
					result += string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}\r\n", (result.IsEmpty ? string.Empty : prefix), formatNumber, keyValuePair.Key);
				}
			}
			return result.TrimEnd().ToString();
		}

		public static void AddOrUpdateDictionary(IDictionary<ZString, ZDecimal> dictionaryToUpdate, ZString key, ZDecimal value)
		{
			if (dictionaryToUpdate.ContainsKey(key))
			{
				dictionaryToUpdate[key] += value;
			}
			else
			{
				dictionaryToUpdate.Add(key, value);
			}
		}

		public static string GetPackageDimension(ZDecimal length, ZDecimal width, ZDecimal height, ZString dimensionUQ)
		{
			ZString result;
			if (length.IsEmpty || width.IsEmpty || height.IsEmpty || dimensionUQ.IsEmpty)
			{
				result = ZString.Empty;
			}
			else
			{
				var finalDimensionUQ = GetDimensionUQ(dimensionUQ);
				result = new ZString(FormattableString.Invariant($"{length.Normalize()}*{width.Normalize()}*{height.Normalize()} {finalDimensionUQ}"));
			}
			return result;
		}

		static ZString GetDimensionUQ(ZString dimensionUQ)
		{
			return dimensionUQ == Core.Constants.Length.Metres ? Constants.Volume.CubicMeter : FormattableString.Invariant($"{dimensionUQ}³");
		}

		public static string GetPackageVolumeInfo(ZString onlyGoodsDescriptionAndQuantity, ZDecimal volume, ZString volumeUQ, ZInt packageQty, ZString dimension)
		{
			var result = new ZStringBuilder();
			if (onlyGoodsDescriptionAndQuantity.IsEmpty)
			{
				result.AppendIfNotEmpty(GetDetailInfo(volume, volumeUQ, packageQty));
				result.AppendIfNotEmpty(dimension);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public static string GetPackageVolumeInfo(ZString onlyGoodsDescriptionAndQuantity, ZString volumeDetailInfo, ZString dimension)
		{
			var result = new ZStringBuilder();
			if (onlyGoodsDescriptionAndQuantity.IsEmpty)
			{
				result.AppendIfNotEmpty(volumeDetailInfo);
				result.AppendIfNotEmpty(dimension);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public static string GetPackageVolumeUQInfo(ZString volumeUQ)
		{
			if (!packageVolumeUQMapping.TryGetValue(volumeUQ, out var result))
			{
				result = volumeUQ;
			}
			return result;
		}

		public static ZString GetPackingListSummaryLineWithTailFlag(string summary, int count)
		{
			var result = new ZStringBuilder();
			if (!string.IsNullOrWhiteSpace(summary))
			{
				result.Append(summary);
				result.Append(ZString.Replicate('v', count));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public static string GetMarksNumbersByAlignedFields(this IEnumerator<ZString> marksNumbersTotalRows, ZString value, ref bool hasNext)
		{
			var marksNumbersInfo = new ZStringBuilder();
			if (!value.IsEmpty)
			{
				foreach (var line in value.Split("\r\n"))
				{
					marksNumbersInfo.Append(marksNumbersTotalRows.Current);
					if (!marksNumbersTotalRows.MoveNext())
					{
						hasNext = false;
						break;
					}
				}
			}
			return marksNumbersInfo.ToStringWithNewLineBetweenAppends();
		}
	}

	public class PackedDetailInfo
	{
		readonly ZDecimal packedQty;
		readonly ZString packedUQ;
		readonly ZInt packageQty;
		readonly ZInt quantityRatioDecimalPlaces;
		ZString packedQtyStr;
		ZString quantityRatioStr;

		public PackedDetailInfo(ZDecimal packedQty, ZString packedUQ, ZInt packageQty, ZInt quantityRatioDecimalPlaces)
		{
			this.packedQty = packedQty;
			this.packedUQ = packedUQ;
			this.packageQty = packageQty;
			this.quantityRatioDecimalPlaces = quantityRatioDecimalPlaces;
			SetPackedDetailInfo();
		}

		void SetPackedDetailInfo()
		{
			var packedQtyStr = DocumentWrapperHelper.FormatNumber(packedQty);
			var quantityRatio = ZString.Empty;
			if (packedQty.IsEmpty)
			{
				packedQtyStr = ZString.Empty;
			}
			else if (packageQty > 0)
			{
				quantityRatio = DocumentWrapperHelper.FormatNumber(new ZDecimal(ZArchitecture.Core.Utilities.Round(packedQty / packageQty, quantityRatioDecimalPlaces)));
				var decimalPlaces1 = DocumentWrapperHelper.GetDecimalPlaces(packedQtyStr);
				var decimalPlaces2 = DocumentWrapperHelper.GetDecimalPlaces(quantityRatio);
				if (decimalPlaces1 > decimalPlaces2)
				{
					quantityRatio = DocumentWrapperHelper.FormatNumber(new ZDecimal(ZArchitecture.Core.Utilities.Round(packedQty / packageQty, decimalPlaces1)));
				}
				else if (decimalPlaces1 < decimalPlaces2)
				{
					packedQtyStr = DocumentWrapperHelper.FormatNumber(new ZDecimal(ZArchitecture.Core.Utilities.Round(packedQty, decimalPlaces2)));
				}
			}

			if (packageQty > 1 && !quantityRatio.IsEmpty)
			{
				this.quantityRatioStr = quantityRatio;
			}
			if (!packedQtyStr.IsEmpty)
			{
				this.packedQtyStr = packedQtyStr;
			}
		}

		public int MaxDecimalPlace => Math.Max(DocumentWrapperHelper.GetDecimalPlaces(packedQtyStr), DocumentWrapperHelper.GetDecimalPlaces(quantityRatioStr));

		public ZString GetDetail(int decimalPlace)
		{
			var result = new ZStringBuilder();

			if (!quantityRatioStr.IsEmpty)
			{
				result.Append(string.Format(CultureInfo.InvariantCulture, "@{0} {1}", FormatNumber(quantityRatioStr, decimalPlace), packedUQ));
			}
			if (!packedQtyStr.IsEmpty)
			{
				result.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", FormatNumber(packedQtyStr, decimalPlace), packedUQ));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		ZString FormatNumber(ZString value, int decimalPlace)
		{
			return decimalPlace > 0 ? DocumentWrapperHelper.FormatNumber(ZDecimal.Parse(value), decimalPlace) : DocumentWrapperHelper.FormatNumber(ZDecimal.Parse(value));
		}
	}
}
