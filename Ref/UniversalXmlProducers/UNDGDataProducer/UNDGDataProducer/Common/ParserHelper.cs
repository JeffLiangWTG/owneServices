using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class ParserHelper
	{
		public static UNDGAttribute CreateUNDGAttribute(string descriptor, string index, string type)
		{
			return new UNDGAttribute
			{
				DA_Descriptor = descriptor,
				DA_Index = index,
				DA_Type = type,
				DA_Language = string.IsNullOrEmpty(descriptor) ? string.Empty : "EN"
			};
		}

		static UNDGAttributeZZ CreateUNDGAttributeZZ(string descriptor, string index, string type, string language = "EN")
		{
			return new UNDGAttributeZZ
			{
				DAZ_Descriptor = descriptor,
				DAZ_Index = index,
				DAZ_Type = type,
				DAZ_Language = string.IsNullOrEmpty(descriptor) ? string.Empty : language
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		public static (decimal amount, string uq, string type) SeparateAmtAndUqAndType(string amountColumnValue)
		{
			if (amountColumnValue == "0")
			{
				return (0, "", Constants.AmtTypes.NetWeightLimitAmtType);
			}
			if (string.IsNullOrEmpty(amountColumnValue))
			{
				return (0, "", Constants.AmtTypes.NonApplicableAmtType);
			}
			if (amountColumnValue == "Forbidden")
			{
				return (0, "", Constants.AmtTypes.ForbiddenAmtType);
			}
			else if (amountColumnValue == "Not Restricted")
			{
				return (0, "", Constants.AmtTypes.NotRestrictedAmtType);
			}
			else if (String.Equals(amountColumnValue, "No Limit", StringComparison.OrdinalIgnoreCase))
			{
				return (0, "", Constants.AmtTypes.NoLimitAmtType);
			}
			else if (amountColumnValue.StartsWith("See", StringComparison.OrdinalIgnoreCase))
			{
				return (0, "", Constants.AmtTypes.NetWeightLimitAmtType);
			}
			var extractionRegex = @"([0-9]{1,4}\.?[0-9]{0,}).(\w*).?([A-Z]{0,})";
			var match = Regex.Match(amountColumnValue, extractionRegex);
			if (match.Success && match.Groups.Count > 0)
			{
				var amt = match.Groups[1].Value;
				var uq = match.Groups[2]?.Value;
				var codeType = match.Groups[3]?.Value;
				return (Convert.ToDecimal(amt, CultureInfo.InvariantCulture), uq ?? "", GetTypeByCodeType(codeType));
			}
			throw new Exception($"Amount configuration not found for the amount {amountColumnValue}");
		}

		static string GetTypeByCodeType(string uq)
		{
			switch (uq.ToUpperInvariant())
			{
				case "G":
					return Constants.AmtTypes.GrossWeightLimitAmtType;
				default:
					return Constants.AmtTypes.NetWeightLimitAmtType;
			}
		}

		public static UNDGAttributeZZ[] LoadAttributesZZ(string qDT)
		{
			var undgAttributes = new List<UNDGAttributeZZ>();
			if (!string.IsNullOrEmpty(qDT))
			{
				undgAttributes.Add(CreateUNDGAttributeZZ(qDT, "0", Constants.AttributeTypes.QualifyingDescriptive));
			}
			return undgAttributes.ToArray();
		}

		public static UNDGAttributeZZ[] AppendUNDGAttributeZZ(UNDGAttributeZZ[] originAttrZZ, string qDT, string attributeType = Constants.AttributeTypes.QualifyingDescriptive, string language = "EN")
		{
			var undgAttributes = new List<UNDGAttributeZZ>();
			foreach (var attr in originAttrZZ)
			{
				undgAttributes.Add(attr);
			}

			if (!string.IsNullOrEmpty(qDT))
			{
				undgAttributes.Add(CreateUNDGAttributeZZ(qDT, "0", attributeType, language));
			}
			return undgAttributes.ToArray();
		}

		public static string CheckFieldLength(string value, string unid, int maxLength)
		{
			if (value.Length > maxLength)
			{
				var errorMessage = $"Field truncated because source data exceeded maximum field size. Max Length: {maxLength.ToString(CultureInfo.InvariantCulture)} Value: {value} UNID: {unid}";
				Console.Error.WriteLine(errorMessage);
				return value.Substring(0, maxLength);
			}
			return value;
		}

		public static decimal TransformLQMaxAmt(string value)
		{
			var limitedQuantityValue = Regex.Match(value, @"\d+").Value;
			if (value.Contains("see", StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}
			return !string.IsNullOrEmpty(limitedQuantityValue) ? Convert.ToDecimal(limitedQuantityValue, CultureInfo.InvariantCulture) : 0;
		}

		public static string TransformLQMaxAmtUQ(string value)
		{
			if (value.Contains("see", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}
			var limitedQuantityMeasure = Regex.Match(value, @"[A-Za-z]+").Value.Trim();
			if (limitedQuantityMeasure.Length > 2)
			{
				return string.Empty;
			}
			return limitedQuantityMeasure;
		}

		public static Tuple<decimal, string>[] TransformLQMaxAmts(string value)
		{
			var lqArray = value.Split(new string[] { "or", "OR" }, StringSplitOptions.None);
			var lqTuples = new Tuple<decimal, string>[lqArray.Length];
			for (var i = 0; i < lqArray.Length; i++)
			{
				lqTuples[i] = new Tuple<decimal, string>(TransformLQMaxAmt(lqArray[i]), TransformLQMaxAmtUQ(lqArray[i]));
			}
			return lqTuples;
		}

		public static string TransformPackIns(string value)
		{
			var iBCIns = Regex.Matches(value, @"IBC(\S)*");
			if (iBCIns.Count > 0)
			{
				foreach (Match match in iBCIns)
				{
					value = value.Replace(match.Value, string.Empty);
				}
				value = Regex.Replace(value, @"\s+", " ");
			}
			return value.Trim();
		}

		public static string SeeContentCleanUp(string value)
		{
			var seeIndex = value.IndexOf("see", StringComparison.OrdinalIgnoreCase);
			if (seeIndex < 0)
			{
				return value;
			}
			else if (seeIndex < 2)
			{
				return string.Empty;
			}
			else
			{
				return value.Substring(0, seeIndex - 1).Trim();
			}
		}

		public static string TransformIBCIns(string value)
		{
			var iBCIns = Regex.Matches(value, @"IBC(\S)*");
			return iBCIns.Count > 0 ? string.Join(" ", iBCIns.Cast<Match>()) : string.Empty;
		}

		public static string TransformTransportCategory(string value, string unid)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			var manipulatedValue = CorrectOpeningAndClosingBrackets(value.ToUpper(CultureInfo.InvariantCulture));
			var transportMode = Regex.Match(manipulatedValue, @"((?<=\()?[a-zA-Z/-]+[0-9]*[a-zA-Z/]*)").Value;
			if (TransportCategories.Contains(transportMode))
			{
				return manipulatedValue;
			}

			Console.Error.WriteLine($"Transport mode not in list. Value: {value} UNID: {unid}");
			return string.Empty;
		}

		static string CorrectOpeningAndClosingBrackets(string value)
		{
			var result = value;
			var openBracketCount = 0;
			var removeTrailingBracket = false;
			for (int i = 0; i < value.Length; i++)
			{
				if (i == value.Length - 1 && value[i] == '(')
				{
					removeTrailingBracket = true;
					openBracketCount -= 1;
				}
				else if (value[i] == '(')
				{
					openBracketCount += 1;
				}
				else if (value[i] == ')')
				{
					if (openBracketCount > 0)
					{
						openBracketCount -= 1;
					}
				}
			}
			if (removeTrailingBracket)
			{
				result = result.Substring(0, result.Length - 1);
			}
			if (openBracketCount > 0)
			{
				result += new string(')', openBracketCount);
			}
			return result;
		}

		public static string TransformClassificationCode(string value)
		{
			if (value == "BLANK" || value == "EMPTY")
			{
				return string.Empty;
			}
			return value;
		}

		public static string TransformLabels(string value)
		{
			if (string.IsNullOrEmpty(value) || value.Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}

			return SeeContentCleanUp(value);
		}
		static readonly string[] TransportCategories = new string[]
		{
			"E",
			"B",
			"B/E",
			"C",
			"C/D",
			"D",
			"D/E",
			"A",
			"B/D",
			"B1000C",
			"C/E",
			"C5000D",
			"-"
		};

		public static string ParseUNNO(string rawString)
		{
			return rawString.PadLeft(4, '0');
		}

		public static string ParseUNNOAndVariant(string rawString)
		{
			var match = Regex.Match(rawString, @"^(\d+)([a-z]*)$");
			if (match.Success)
			{
				var UNNO = match.Groups[1].Value;
				var variant = match.Groups[2].Value;

				var paddedUNNO = ParseUNNO(UNNO);
				return paddedUNNO + variant;
			}

			throw new ArgumentException($"{rawString} is not a valid UNNO variant format");
		}
	}
}
