using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class EUNUtils
	{
		internal class TariffHeaderComparer : IEqualityComparer<RefCusTariff>
		{
			public bool Equals(RefCusTariff x, RefCusTariff y)
			{
				return x.ZZ1_TariffCode == y.ZZ1_TariffCode;
			}

			public int GetHashCode(RefCusTariff obj)
			{
				return obj.ZZ1_TariffCode.GetHashCode();
			}
		}

		public static IEnumerable<IWebTariffHeader> GenerateWebTariffHeadersFromRefCusTariff(IEnumerable<string> tariffHeaders, ICollection<RefCusTariff> tariffCollection)
		{
			if (tariffCollection != null || tariffCollection.Count > 0)
			{
				var tariffsToProduce = new List<RefCusTariff>();
				foreach (var header in tariffHeaders)
				{
					tariffsToProduce.AddRange(tariffCollection.Where(x => NormalizeTariffCode(x.ZZ1_TariffCode).StartsWith(Common.Utils.RemoveTrailingZeros(header), StringComparison.InvariantCultureIgnoreCase)));
				}

				tariffsToProduce = tariffsToProduce.Distinct(new TariffHeaderComparer()).ToList();

				foreach (var tariff in tariffsToProduce)
				{
					yield return new WebTariffHeader(NormalizeTariffCode(tariff.ZZ1_TariffCode), tariff.ZZ1_Description) { StartDate = tariff.ZZ1_StartDate, CompositeKey = tariff.ZZ1_CompositeKeyOnZZ5, TariffLanguage = tariff.RefCusTariffLanguages };
				}
			}
		}

		public static string NormalizeTariffCode(string rawTariffCode, int lenght = 10)
		{
			Argument.NotNullOrEmpty(rawTariffCode, nameof(rawTariffCode));

			while (true)
			{
				if (rawTariffCode.Length >= lenght || rawTariffCode.Length % 2 != 0)
				{
					return rawTariffCode;
				}

				rawTariffCode = rawTariffCode + "00";
			}
		}

		public static string NormalizeExportTariffCode(string rawTariffCode)
		{
			Argument.NotNullOrEmpty(rawTariffCode, nameof(rawTariffCode));
			return rawTariffCode.Length > 8 ? rawTariffCode.Substring(0, 8) : NormalizeTariffCode(rawTariffCode, lenght: 8);
		}

		public static DateTime GetFromOADate(ICell column, bool isStartDate = true, string format = "dd-MM-yyyy")
		{
			var defaultDate = isStartDate ? MinDateTime : MaxDateTime;
			var parsed = defaultDate;
			if (column != null)
			{
				if (column.CellType == CellType.Numeric)
				{
					var cellValue = column.NumericCellValue;
					if (cellValue != 0)
					{
						parsed = DateTime.FromOADate(cellValue);
					}
				}
				else
				{
					var cellValue = column.StringCellValue;
					if (!string.IsNullOrEmpty(cellValue))
					{
						parsed = GetParsedDateTime(cellValue, format);
					}
				}

				if (parsed < MinDateTime)
				{
					parsed = MinDateTime;
				}
				else if (parsed > MaxDateTime)
				{
					parsed = MaxDateTime;
				}

				if (!isStartDate)
				{
					parsed = parsed.MidnightToEndOfDay();
				}
			}

			return parsed;
		}

		public static DateTime GetParsedDateTime(string value, string format = "dd-MM-yyyy")
		{
			return DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
				? parsedDate
				: throw new ArgumentException($"Invalid date format was present expected:[{format}] got:[{value}]");
		}

		internal static bool BeginsWithRomanNumeral(string description)
		{
			var romanNumerals = new[]
			{
				"I.", "II.", "III.", "IV.", "V.", "VI.", "VII.", "VIII.", "IX.", "X.",
				"XI.", "XII.", "XIII.", "XIV.", "XV.", "XVI.", "XVII.", "XVIII.", "XIX.", "XX."
			};

			return romanNumerals.Any(description.StartsWith);
		}

		internal static int ConvertFromRomanNumeral(string romanNumeral)
		{
			Argument.NotNullOrEmpty(romanNumeral, nameof(romanNumeral));

			var numeralDictionary = new Dictionary<char, int>
			{
				{ 'I', 1 },
				{ 'V', 5 },
				{ 'X', 10 },
				{ 'L', 50 },
				{ 'C', 100 },
				{ 'D', 500 },
				{ 'M', 1000 }
			};

			if (string.IsNullOrEmpty(romanNumeral) || romanNumeral.Length == 0)
			{
				return 0;
			}

			romanNumeral = romanNumeral.ToUpper(CultureInfo.InvariantCulture);

			var total = 0;
			var lastValue = 0;
			for (var i = romanNumeral.Length - 1; i >= 0; i--)
			{
				var newValue = numeralDictionary[romanNumeral[i]];

				if (newValue < lastValue)
				{
					total -= newValue;
				}
				else
				{
					total += newValue;
					lastValue = newValue;
				}
			}

			return total;
		}

		internal static ICompositeKeyNode CreateAndAssignNomenclatureGroupNodeFromTariff(INomenclatureRecord record, ICompositeKeyNode parentNode)
		{
			Argument.NotNull(record, nameof(record));
			Argument.NotNull(parentNode, nameof(parentNode));

			var tariffHeader = record.TariffHeader[..record.HierarchyPosition];

			var description = record.Description;
			if (!parentNode.Children.Any(x => x.Value == tariffHeader && x.NodeType == CompositeKeyNodeType.NomenclatureGroup))
			{
				if (string.IsNullOrEmpty(tariffHeader) || string.IsNullOrEmpty(description))
				{
					Console.WriteLine($"WARNING: Tariff header or description is empty for record: Header: '{record.TariffHeader}', Description: '{record.Description}', Hierarchy Position: {record.HierarchyPosition}, Level: {record.Level}");
					return null;
				}

				return new CompositeKeyNode(tariffHeader, description, record.DeclarableStartDate, record.EndDate, CompositeKeyNodeType.NomenclatureGroup, parentNode.Level + 1, parentNode, record.Language.ToList());
			}
			return null;
		}

		internal static ICompositeKeyNode CreateAndAssignCompositeKeyTreeNode(INomenclatureRecord record, ICompositeKeyNode parentNode, string overriddenValue = null)
		{
			Argument.NotNull(record, nameof(record));
			Argument.NotNull(parentNode, nameof(parentNode));

			if (!string.IsNullOrEmpty(overriddenValue))
			{
				if (string.IsNullOrEmpty(record.Description))
				{
					Console.WriteLine($"WARNING: Tariff description is empty for record: Header: '{record.TariffHeader}', Description: '{record.Description}', Hierarchy Position: {record.HierarchyPosition}, Level: {record.Level}");
					return null;
				}

				return new CompositeKeyNode(overriddenValue, record.Description, record.DeclarableStartDate,
					record.EndDate, CompositeKeyNodeType.PlaceHolder, parentNode.Level + 1, parentNode, record.Language.ToList());
			}

			var splitValues = record.TariffHeader.Split(' ');
			var tariffCode = splitValues.First();
			var nodeValue = Common.Utils.RemoveTrailingZeros(tariffCode);
			var zz5Value = nodeValue;

			var nodeType = record.IsTariff ? CompositeKeyNodeType.Tariff : CompositeKeyNodeType.NomenclatureGroup;

			if (splitValues.Last() != "80")
			{
				zz5Value = string.Empty;
			}

			// Create node for heading, the child node will be subheading. These 2 nodes are represented
			// in the excel having the same values but at different level. That's why heading node should
			// have 5 digits only when subheading should have 6 digits
			if (record.HierarchyPosition == 6 && !record.IsTariff)
			{
				if (record.Level == 1 && !nodeValue.EndsWith("0", StringComparison.InvariantCultureIgnoreCase))
				{
					nodeValue = nodeValue.Substring(0, 5);
				}
			}

			var node = new CompositeKeyNode(nodeValue, record.Description, record.DeclarableStartDate,
				record.EndDate, nodeType, parentNode.Level + 1, parentNode, record.Language.ToList())
			{
				ZZ5Value = zz5Value
			};

			return node;
		}

		internal static bool IsValidHierarchyPosition(string hierarchyPosition)
		{
			if (!int.TryParse(hierarchyPosition, out var hierarchyPositionValue))
			{
				hierarchyPositionValue = 0;
			}

			return IsValidHierarchyPosition(hierarchyPositionValue);
		}

		internal static bool IsValidHierarchyPosition(int hierarchyPosition) => hierarchyPosition > 0;
		
		internal static readonly DateTime MinDateTime = new(1900, 01, 01, 00, 00, 00);
		internal static readonly DateTime MaxDateTime = new(2079, 06, 06, 23, 59, 00);
	}
}
