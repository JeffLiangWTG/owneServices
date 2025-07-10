using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public static class ParserHelper
	{
		#region CARM

		public static DateTime ParseDateSafe(DateTime parseDate, bool fixEndDate = false)
		{
			if (parseDate >= Constants.DefaultValues.MaxDateTime)
			{
				parseDate = Constants.DefaultValues.MaxDateTime;
			}
			else if (parseDate <= Constants.DefaultValues.MinDateTime)
			{
				parseDate = Constants.DefaultValues.MinDateTime;
			}
			else if (fixEndDate)
			{
				parseDate = parseDate.AddDays(1).AddMinutes(-1);
			}

			return parseDate;
		}

		public static string GenerateRateFormula(double value, string code, string unitOfMeasureCode)
		{
			var formula = "0";
			if (value > 0)
			{
				var unit = $"[{unitOfMeasureCode}]";
				if ("3" == code || "2" == code)
				{
					value = (double)(decimal.Round((decimal)value, 5) / 100);

					if ("3" == code)
					{
						unit = "VFD";
					}

					formula = $"{value.ToString(CultureInfo.InvariantCulture)}*{unit}";
				}
				else if ("1" == code || "S" == code)
				{
					formula = $"{value.ToString("0.#####", CultureInfo.InvariantCulture)}*{unit}";
				}
			}
			return formula;
		}

		#endregion

		#region Harmonized

		public static string GetRateFormula(string rate, string ttCode, string uom)
		{
			var rateFormula = string.Empty;
			if (rate.Contains(Constants.DefaultValues.FreeTariff) || rate.Contains(Constants.DefaultValues.NA))
			{
				if (ttCode.Equals("02", StringComparison.Ordinal) || ttCode.Equals("03", StringComparison.Ordinal))
				{
					rateFormula = "0";
				}
			}
			else if (rate.Contains("but not less than"))
			{
				string[] rates = Regex.Split(rate, "but not less than", RegexOptions.IgnoreCase);
				var rate1 = GetRateFormula(rates[0], ttCode, uom);
				if (rate.Contains("or more than"))
				{
					string[] newRates = Regex.Split(rates[1], "or more than", RegexOptions.IgnoreCase);
					var rate2 = GetRateFormula(newRates[0], ttCode, uom);
					var rate3 = GetRateFormula(newRates[1], ttCode, uom);
					rateFormula = $"MIN({rate3}, MAX({rate1},{rate2}))";
				}
				else if (rate.Contains("plus"))
				{
					rates = Regex.Split(rates[1], "plus", RegexOptions.IgnoreCase);
					var rate2 = GetRateFormula(rates[0], ttCode, uom);
					var rate3 = GetRateFormula(rates[1], ttCode, uom);
					rateFormula = $"MAX({rate1},{rate2}) + {rate3}";
				}
				else
				{
					var rate2 = GetRateFormula(rates[1], ttCode, uom);
					rateFormula = $"MAX({rate1},{rate2})";
				}
			}
			else if (rate.Contains("plus"))
			{
				string[] rates = Regex.Split(rate, "plus", RegexOptions.IgnoreCase);
				rateFormula = GetRateFormula(rates[0], ttCode, uom) + " + " + GetRateFormula(rates[1], ttCode, uom);
			}
			else if (rate.Contains("%"))
			{
				rate = (Convert.ToDouble(rate.Trim().TrimEnd('%'), CultureInfo.InvariantCulture) / 100).ToString("0.###", CultureInfo.InvariantCulture);
				rateFormula = $"{rate}*VFD";
			}
			else
			{
				rate = Regex.Replace(rate, @"[^\d.\d]", "");
				if (string.IsNullOrEmpty(rate))
				{
					rateFormula = GetRateFormula(Constants.DefaultValues.FreeTariff, ttCode, uom);
				}
				else
				{
					rate = rate.ToString();
					rateFormula = $"{rate}*[{uom}]";
				}
			}
			return rateFormula;
		}

		public static int BinarySearchMatchingHSCode(List<string> hsCodeList, string targetTariff)
		{
			int left = 0, right = hsCodeList.Count - 1, mid;
			while (left <= right)
			{
				mid = left + (right - left) / 2;
				var compare = string.Compare(hsCodeList[mid], targetTariff, StringComparison.OrdinalIgnoreCase);
				if (compare > 0)
				{
					right = mid - 1;
				}
				else if (compare < 0)
				{
					left = mid + 1;
				}
				else
				{
					return mid + 1;
				}
			}
			return left;
		}

		public static void GetConditionValues(List<RefCusConditionValue> conditionValues, CAHarmonizedCondition condition)
		{
			foreach (var pga in condition.PGAs)
			{
				var pgaCode = pga.PGACode;
				var value = RefCusConditionValueMapper.GetConditionValueWithPGACodeAndProgram(pgaCode, pga.Program);
				if (!conditionValues.Exists(x => x.ZX3_ZX4_NKValueType == pgaCode && x.ZX3_Value == value))
				{
					conditionValues.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = pgaCode,
						ZX3_Value = value
					});
				}
			}
		}

		public static string GetAndExtractAccdbFile(string tempPath, string zipPath)
		{
			string accdbName = null;
			using (var zip = ZipFile.OpenRead(zipPath))
			{
				foreach (var entry in zip.Entries)
				{
					if (entry.Name.Contains(".accdb"))
					{
						accdbName = entry.Name;
						if (File.Exists(Path.Combine(tempPath, accdbName)))
						{
							File.Delete(Path.Combine(tempPath, accdbName));
						}
						ZipFile.ExtractToDirectory(zipPath, tempPath, Encoding.UTF8);
						break;
					}
				}
			}
			return accdbName;
		}

		/// <summary>
		/// Remove illegal XML characters from a string.
		/// </summary>
		public static string SanitizeXmlString(string xml)
		{
			if (xml == null)
			{
				throw new ArgumentNullException(nameof(xml));
			}

			var buffer = new StringBuilder(xml.Length);

			foreach (var c in xml)
			{
				if (IsLegalXmlChar(c))
				{
					buffer.Append(c);
				}
			}

			return buffer.ToString();
		}

		/// <summary>
		/// Whether a given character is allowed by XML 1.0.
		/// </summary>
		public static bool IsLegalXmlChar(int character)
		{
			return
			(
				 character == 0x9 /* == '\t' == 9   */          ||
				 character == 0xA /* == '\n' == 10  */          ||
				 character == 0xD /* == '\r' == 13  */          ||
				(character >= 0x20 && character <= 0xD7FF) ||
				(character >= 0xE000 && character <= 0xFFFD) ||
				(character >= 0x10000 && character <= 0x10FFFF)
			);
		}

		#endregion

		public static void AddUOM(string uom, RefCusTariff tariff)
		{
			if (!string.IsNullOrWhiteSpace(uom) && uom.Trim() != "-")
			{
				var cusUOM = new RefCusTariffUOM
				{
					ZZ8_UOM = uom
				};
				tariff.RefCusTariffUOMs = new[] { cusUOM };
			}
		}

		public static void MarkConveyanceTariffs(RefCusTariff tariff)
		{
			if (ConveyanceRequiredTariffList.Any(x => x.Replace(".", "") == tariff.ZZ1_TariffCode))
			{
				var cusTariffAttribute = new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.ConveyanceRequiredTariffAttrName,
					ZZ3_Value = "Y"
				};
				tariff.RefCusTariffAttributes = new[] { cusTariffAttribute };
			}
		}

		static IEnumerable<string> ConveyanceRequiredTariffList => conveyanceRequiredTariffList ?? (conveyanceRequiredTariffList = CsvLoader.GetConveyanceRequiredTariffList());
		static IEnumerable<string> conveyanceRequiredTariffList;

		public static List<CAHarmonizedTTCode> TTCodeList => ttCodeList ?? (ttCodeList = CsvLoader.Deserialize<CAHarmonizedTTCode>(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.TTCodeFileName)));
		static List<CAHarmonizedTTCode> ttCodeList;

		public static string RemoveNewLines(string str)
		{
			return string.IsNullOrEmpty(str) ? str : str.Replace("&#xD;", "").Replace("&#xA;", "").Replace("\r", "").Replace("\n", "").Trim();
		}
	}
}
