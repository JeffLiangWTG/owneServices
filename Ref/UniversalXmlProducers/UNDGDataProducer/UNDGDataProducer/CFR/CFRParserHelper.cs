using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
	public static class CFRParserHelper
	{
		public static string ParsePrimaryClass(string rawString)
		{
			if (string.IsNullOrEmpty(rawString))
			{
				return string.Empty;
			}

			return rawString.Length > 4
				? rawString.Substring(0, 4)
				: rawString;
		}

		public static string ParseSecondaryClass(string value)
		{
			if (value.StartsWith("see", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}
			return value;
		}

		public static string ParseTertiaryClass(string rawString)
		{
			return string.IsNullOrEmpty(rawString)
				? string.Empty
				: rawString.Substring(0, 1);
		}

		public static bool ParseLimitedQuantityPermitted(string rawString)
		{
			if (string.IsNullOrEmpty(rawString)
				|| string.Compare(rawString, "0", StringComparison.Ordinal) == 0
				|| rawString.StartsWith("see", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return true;
		}

		public static string ParseExceptedQuantityValue(string value)
		{
			if (value.StartsWith("see", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}
			return value;
		}

		public static string ParseLimitedQuantityValue(string rawString)
		{
			if (StringIsForLimitedQuantitiesNotPermitted(rawString) || rawString.StartsWith("see", StringComparison.OrdinalIgnoreCase))
			{
				return "0";
			}

			return GetValueAndUnit(rawString).value;
		}

		public static string ParseLimitedQuantityUnit(string rawString)
		{
			if (StringIsForLimitedQuantitiesNotPermitted(rawString) || rawString.StartsWith("see", StringComparison.OrdinalIgnoreCase))
			{
				return "kg";
			}

			return GetValueAndUnit(rawString).unit;
		}

		public static string ParseReportableQuantityValue(string rawString)
		{
			if (string.IsNullOrEmpty(rawString))
			{
				return "0";
			}

			var valueInPoundsAndKg = rawString.Split(' ');
			if (valueInPoundsAndKg.Length != 2)
			{
				throw new ApplicationException($"Reportable Quantity should be in format 'n (n)'. Value is {rawString}");
			}

			return valueInPoundsAndKg[0];
		}

		public static bool ParseIsPSNFixed(string rawString)
		{
			return rawString.Contains("+");
		}

		public static bool ParseAppliesForAirTransport(string rawString)
		{
			return rawString.Contains("A");
		}

		public static bool ParseAppliesForDomesticTransport(string rawString)
		{
			return rawString.Contains("D");
		}

		public static bool ParseAppliesForInternationalTransport(string rawString)
		{
			return rawString.Contains("I");
		}

		public static bool ParseAppliesForVesselTransport(string rawString)
		{
			return rawString.Contains("W");
		}

		public static bool ParseRequiresTechnicalNameInParenthesis(string rawString)
		{
			return rawString.Contains("G");
		}

		public static string ParseEmergencyResponseGuideline(string rawString)
		{
			if (string.IsNullOrEmpty(rawString))
			{
				return rawString;
			}

			return rawString.Substring(0, 3);
		}

		public static string ParsePAXAirRailLimitType(string rawString)
		{
			return ParseAirRailLimitType(rawString);
		}

		public static string ParseCargoAirRailLimitType(string rawString)
		{
			return ParseAirRailLimitType(rawString);
		}

		static string ParseAirRailLimitType(string rawString)
		{
			if (Regex.Match(rawString, @"(.?\d+)\s?(g|kg|L|mL)").Success)
			{
				if (rawString.Contains("gross", StringComparison.OrdinalIgnoreCase))
				{
					return "GLM";
				}
				return "NLM";
			}
			switch (rawString.ToUpper(CultureInfo.InvariantCulture))
			{
				case "FORBIDDEN":
					return "FOB";

				case "NO LIMIT":
					return "NLT";

				default:
					return string.Empty;
			}
		}

		public static bool ParseIsPAXAirRailForbidden(string rawString)
		{
			return ParseIsForbidden(rawString);
		}

		public static bool ParseIsCargoAirRailForbidden(string rawString)
		{
			return ParseIsForbidden(rawString);
		}

		public static string ParsePAXAirRailLimitUnit(string rawString, int index)
		{
			string sanitizedString = SanitizeStringForOccurrenceOfOr(rawString, index);
			return ParseAirRailLimitUnit(sanitizedString);
		}

		public static string ParseCargoAirRailLimitUnit(string rawString, int index)
		{
			string sanitizedString = SanitizeStringForOccurrenceOfOr(rawString, index);
			return ParseAirRailLimitUnit(sanitizedString);
		}

		public static string ParsePAXAirRailLimitValue(string rawString, int index)
		{
			string sanitizedString = SanitizeStringForOccurrenceOfOr(rawString, index);
			return ParseAirRailLimitValue(sanitizedString);
		}

		public static string ParseCargoAirRailLimitValue(string rawString, int index)
		{
			string sanitizedString = SanitizeStringForOccurrenceOfOr(rawString, index);
			return ParseAirRailLimitValue(sanitizedString);
		}

		static bool StringIsForLimitedQuantitiesNotPermitted(string rawString)
		{
			return string.IsNullOrEmpty(rawString)
				|| string.Compare(rawString, "0", StringComparison.Ordinal) == 0;
		}

		static string SanitizeStringForOccurrenceOfOr(string rawString, int index)
		{
			return rawString.Contains(" or ")
				? rawString.Split(new string[] { " or " }, StringSplitOptions.None)[index]
				: index == 0 ? rawString : string.Empty;
		}

		static string ParseAirRailLimitUnit(string rawString)
		{
			if (StringContainsInvalidValuesForAirRailLimits(rawString))
			{
				return "kg";
			}

			return GetValueAndUnit(rawString).unit;
		}

		static string ParseAirRailLimitValue(string rawString)
		{
			if (StringContainsInvalidValuesForAirRailLimits(rawString))
			{
				return "0";
			}

			return GetValueAndUnit(rawString).value;
		}

		static bool StringContainsInvalidValuesForAirRailLimits(string rawString)
		{
			return string.IsNullOrEmpty(rawString)
				|| ParseIsForbidden(rawString)
				|| string.Compare(rawString, "No Limit", StringComparison.OrdinalIgnoreCase) == 0
				|| string.Compare(rawString, "See A105", StringComparison.OrdinalIgnoreCase) == 0
				|| string.Compare(rawString, "A", StringComparison.Ordinal) == 0;
		}

		static bool ParseIsForbidden(string rawString)
		{
			return (string.Compare(rawString, "Forbidden", StringComparison.Ordinal) == 0);
		}

		static (string value, string unit) GetValueAndUnit(string rawString)
		{
			var valueUnitMatch = Regex.Match(rawString, @"(.?\d+)\s?(g|kg|L|mL)");
			if (valueUnitMatch.Success)
			{
				return (valueUnitMatch.Groups[1].Value, valueUnitMatch.Groups[2].Value);
			}
			else
			{
				throw new ApplicationException($"Invalid format {rawString}");
			}
		}
	}
}
