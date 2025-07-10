using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class ConditionDataParser
	{
		readonly string excelDirectory;

		public ConditionDataParser(string excelDirectory)
		{
			this.excelDirectory = excelDirectory;
		}

		public (Dictionary<string, CAHarmonizedCondition> EqualToDictionary, Dictionary<string, CAHarmonizedCondition> StartsWithDictionary, Dictionary<string, CAHarmonizedCondition> WithinRangeDictionary, List<string> WithinRangeHSCodeFromList) Parse()
		{
			Console.WriteLine("Start parsing Condition Data.");
			equalToDictionary = new Dictionary<string, CAHarmonizedCondition>();
			startsWithDictionary = new Dictionary<string, CAHarmonizedCondition>();
			withinRangeDictionary = new Dictionary<string, CAHarmonizedCondition>();
			withinRangeHSCodeFromList = new List<string>();
			var xls = new XlsFile(excelDirectory, false);
			for (int sheetIndex = 1; sheetIndex <= xls.SheetCount; sheetIndex++)
			{
				xls.ActiveSheet = sheetIndex;
				var rowCount = xls.GetRowCount(xls.ActiveSheet);
				var map = new Dictionary<int, string>();
				for (var row = 2; row <= rowCount; row++)
				{
					if (map.Count > 0)
					{
						var hsCodeOperator = string.Empty;
						var condition = new CAHarmonizedCondition();
						var pga = new PGA();
						foreach (var keyAndValue in map)
						{
							var colValue = xls.GetCellValue(row, keyAndValue.Key)?.ToString().Trim();
							switch (keyAndValue.Value)
							{
								case "PROGRAM":
									pga.Program = colValue?.ToUpper(CultureInfo.CurrentCulture);
									break;
								case "HS OPERATOR":
								case "HS CODE OPERATOR":
									hsCodeOperator = colValue;
									break;
								case "HS CODE":
								case "HS CODE FROM":
									condition.HSCodeFrom = colValue;
									break;
								case "HS CODE TO":
									condition.HSCodeTo = colValue;
									break;
							}
						}
						if (!string.IsNullOrEmpty(hsCodeOperator) && !string.IsNullOrEmpty(condition.HSCodeFrom))
						{
							pga.PGACode = xls.SheetName.Trim().ToUpper(CultureInfo.CurrentCulture);
							hsCodeOperator = hsCodeOperator.Replace(" ", string.Empty).ToUpper(CultureInfo.CurrentCulture);
							switch (hsCodeOperator)
							{
								case "EQUALTO":
								case "EQUALSTO":
									AddItemToDictionary(equalToDictionary, condition, pga);
									break;
								case "STARTSWITH":
								case "STARTWITH":
									AddItemToDictionary(startsWithDictionary, condition, pga);
									break;
								case "WITHINRANGE":
									if (!string.IsNullOrEmpty(condition.HSCodeTo))
									{
										withinRangeHSCodeFromList.Add(condition.HSCodeFrom);
										AddItemToDictionary(withinRangeDictionary, condition, pga);
									}
									break;
							}
						}
					}

					AddValueToMap(map, xls, row);
				}
			}
			withinRangeHSCodeFromList.Sort();
			Console.WriteLine("End parsing Condition Data.");
			return (equalToDictionary, startsWithDictionary, withinRangeDictionary, withinRangeHSCodeFromList);
		}

		void AddValueToMap(Dictionary<int, string> map, XlsFile xls, int row)
		{
			var cellValue = xls.GetCellValue(row - 1, 1);
			if (cellValue != null && cellValue.ToString().StartsWith("REGULATED COMMODITIES", System.StringComparison.Ordinal))
			{
				for (var col = 1; col <= 6; col++)
				{
					var colValue = xls.GetCellValue(row, col)?.ToString().Trim().ToUpper(CultureInfo.CurrentCulture);
					if (colValue != null && (colValue.Contains("PROGRAM") || colValue.Contains("OPERATOR") || colValue.Contains("HS CODE")))
					{
						map.Add(col, colValue);
					}
				}
			}
		}

		void AddItemToDictionary(Dictionary<string, CAHarmonizedCondition> dictionay, CAHarmonizedCondition condition, PGA pga)
		{
			var hsCodeFrom = condition.HSCodeFrom;
			if (condition.HSCodeFrom.Contains(","))
			{
				foreach (var hsCode in hsCodeFrom.Split(','))
				{
					if (!string.IsNullOrEmpty(hsCode.Trim()))
					{
						AddItem(hsCode.Trim());
					}
				}
			}
			else if (hsCodeFrom.Contains("\n"))
			{
				foreach (var hsCode in hsCodeFrom.Split('\n'))
				{
					if (!string.IsNullOrEmpty(hsCode.Trim()))
					{
						AddItem(hsCode.Trim());
					}
				}
			}
			else
			{
				AddItem(hsCodeFrom);
			}
			void AddItem(string value)
			{
				if (dictionay.ContainsKey(value))
				{
					if (!dictionay[value].PGAs.Exists(x => x.PGACode == pga.PGACode && x.Program == pga.Program))
					{
						dictionay[value].PGAs.Add(pga);
					}
				}
				else
				{
					condition.PGAs = new List<PGA>() { pga };
					dictionay.Add(value, condition);
				}
			}
		}

		public List<RefCusCondition> GetRefCusConditions(string tariffCode)
		{
			var result = new List<RefCusCondition>();
			if (equalToDictionary == null)
			{
				Parse();
			}
			var conditionValues = new List<RefCusConditionValue>();
			if (equalToDictionary.TryGetValue(tariffCode, out var condition1))
			{
				ParserHelper.GetConditionValues(conditionValues, condition1);
			}

			var startWithDictionary = startsWithDictionary;
			for (var i = 4; i <= 10; i++)
			{
				if (startWithDictionary.TryGetValue(tariffCode.Substring(0, i), out var condition2))
				{
					ParserHelper.GetConditionValues(conditionValues, condition2);
				}
			}

			foreach (var hsCode in withinRangeHSCodeFromList.GetRange(0, ParserHelper.BinarySearchMatchingHSCode(withinRangeHSCodeFromList, tariffCode)))
			{
				withinRangeDictionary.TryGetValue(hsCode, out var condition3);
				if (string.Compare(condition3.HSCodeTo, tariffCode, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					ParserHelper.GetConditionValues(conditionValues, condition3);
				}
			}

			RefCusCondition pgacCondition = null;
			if (CATariffPGAConditionPatch.TryGetValue(tariffCode, out var caTariffPGACondition))
			{
				if (!conditionValues.Exists(x => x.ZX3_ZX4_NKValueType == caTariffPGACondition.ConditionValueType))
				{
					conditionValues.Add(new RefCusConditionValue()
					{
						ZX3_ZX4_NKValueType = caTariffPGACondition.ConditionValueType,
						ZX3_Value = caTariffPGACondition.PGAValue
					});
				}
				pgacCondition = new RefCusCondition();
				pgacCondition.ZX1_ZX2_NKConditionType = Constants.ConditionType.PGAC;
				pgacCondition.RefCusConditionValues = new RefCusConditionValue[] { new RefCusConditionValue() { ZX3_ZX4_NKValueType = caTariffPGACondition.ConditionValueType, ZX3_Value = caTariffPGACondition.PGACValue } };
			}

			if (conditionValues.Any())
			{
				var condition = new RefCusCondition();
				condition.RefCusConditionValues = conditionValues.ToArray();
				result.Add(condition);
			}
			if (pgacCondition != null)
			{
				result.Add(pgacCondition);
			}
			return result;
		}

		Dictionary<string, CAHarmonizedCondition> equalToDictionary;
		Dictionary<string, CAHarmonizedCondition> startsWithDictionary;
		Dictionary<string, CAHarmonizedCondition> withinRangeDictionary;
		List<string> withinRangeHSCodeFromList;

		Dictionary<string, CATariffPGACondition> CATariffPGAConditionPatch
		{
			get
			{
				if (caTariffPGAConditionPatch == null)
				{
					caTariffPGAConditionPatch = CsvLoader.Deserialize<CATariffPGACondition>(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.CATariffPGAConditionPatchFileName)).ToDictionary(x => x.TariffCode, x => x);
				}
				return caTariffPGAConditionPatch;
			}
		}
		Dictionary<string, CATariffPGACondition> caTariffPGAConditionPatch;
	}
}
