using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class DBKTariffXmlProducer
	{
		public DBKTariffXmlProducer(string dataSource, string inputPath)
		{
			this.dataSource = dataSource;
			this.inputPath = inputPath;
			this.logger = new Logger(new DateTimeProvider());
		}
		readonly string dataSource;
		readonly string inputPath;
		readonly Logger logger;
		DateTime? startDate;

		public void ProduceXml()
		{
			var dbkTariffs = ReadAndParsePDF();
			var validator = new DBKTariffValidator(logger);
			dbkTariffs = validator.ValidateAndRemoveInvalidData(dbkTariffs);
			GenerateXML(dbkTariffs);
		}

		List<DBKTariff> ReadAndParsePDF()
		{
			var result = new List<DBKTariff>();
			var bytes = File.ReadAllBytes(inputPath);
			var text = PDFParserHelper.ExtractTextFromPdfUseSpire(bytes, 1, 10);
			startDate = GetStartDatetime(text);
			if (startDate != null)
			{
				using (var dataTable = PDFParserHelper.ExtractTableToDataTable(bytes))
				{
					if (dataTable == null || dataTable.Rows.Count == 0)
					{
						logger.Log(LogType.ReviewRequired, "No data found in PDF");
					}
					else if (dataTable.Columns.Count != 5)
					{
						logger.Log(LogType.ReviewRequired, "The number of table columns in the current PDF is not 5.");
					}
					else
					{
						result = CreateDBKTariffs(dataTable);
					}
				}
			}
			else
			{
				logger.Log(LogType.ReviewRequired, "The effective time cannot be parsed out within the PDF file.");
			}
			return result;
		}

		static List<DBKTariff> CreateDBKTariffs(DataTable dataTable)
		{
			var result = new List<DBKTariff>();
			var rateCodeIsB = true;
			var lastParentTariffCode = string.Empty;
			var lastParentTariffDesc = string.Empty;
			var lastChapter = 1;
			foreach (DataRow row in dataTable.Rows)
			{
				var intermediateData = ExtractInformationFromDataRow(row);
				if (intermediateData.Tariff.IsNullOrEmpty() && !intermediateData.TariffDesc.IsNullOrEmpty())
				{
					var dbkTariff = result.Last();
					dbkTariff.TariffDescription = $"{dbkTariff.TariffDescription} {intermediateData.TariffDesc}";
				}
				else if (Regex.IsMatch(intermediateData.Tariff, TariffCodeRegex))
				{
					if (rateCodeIsB)
					{
						var currentChapter = int.Parse(intermediateData.Tariff.Substring(0, 2), CultureInfo.InvariantCulture);
						if (currentChapter < lastChapter)
						{
							rateCodeIsB = false;
						}
						else
						{
							lastChapter = currentChapter;
						}
					}

					var lastTariffIsParentNode = false;
					if (!lastParentTariffCode.IsNullOrEmpty() && intermediateData.Tariff.StartsWith(lastParentTariffCode, StringComparison.OrdinalIgnoreCase))
					{
						lastTariffIsParentNode = true;
					}

					if (!lastTariffIsParentNode)
					{
						lastParentTariffCode = intermediateData.Tariff;
						lastParentTariffDesc = intermediateData.TariffDesc;
					}
					else
					{
						result.Remove(result.FirstOrDefault(x => x.TariffCode == lastParentTariffCode));
						intermediateData.TariffDesc = $"{lastParentTariffDesc} - {intermediateData.TariffDesc}";
					}

					var dbkTariff = new DBKTariff
					{
						TariffCode = intermediateData.Tariff,
						TariffDescription = intermediateData.TariffDesc,
						Unit = intermediateData.Unit,
						Rate = intermediateData.Rate,
						RateString = intermediateData.RateString,
						SpecificRate = intermediateData.SpecificRate,
						RateCode = rateCodeIsB ? "B" : "D"
					};

					result.Add(dbkTariff);
				}

			}
			return result;
		}
		public const string TariffCodeRegex = @"^\d{4}$|^\d{6}$";

		static DBKTariffIntermediateData ExtractInformationFromDataRow(DataRow row)
		{
			var intermediateData = new DBKTariffIntermediateData();
			for (int columnNum = 0; columnNum < row.Table.Columns.Count; columnNum++)
			{
				var text = Encoding.UTF8.GetString(Encoding.Default.GetBytes(row[columnNum]?.ToString() ?? string.Empty)).Trim();
				text = Regex.Replace(text, @"\s+", " ");
				switch (columnNum)
				{
					case 0:
						intermediateData.Tariff = text;
						break;
					case 1:
						intermediateData.TariffDesc = text;
						break;
					case 2:
						intermediateData.Unit = text;
						break;
					case 3:
						intermediateData.RateString = text;
						var d1 = ExtractDecimal(text);
						if (text.Contains("%"))
						{
							intermediateData.Rate = d1 / 100m;
						}
						else
						{
							intermediateData.SpecificRate = d1;
						}
						break;
					case 4:
						var d2 = ExtractDecimal(text);
						if (intermediateData.SpecificRate == 0m)
						{
							intermediateData.SpecificRate = d2;
						}
						break;
				}
			}

			return intermediateData;
		}

		static decimal ExtractDecimal(string input)
		{
			var match = Regex.Match(input, @"(\d+(\.\d+)?)");
			if (match.Success && decimal.TryParse(match.Value, out var result))
			{
				return result;
			}
			return 0m;
		}

		void GenerateXML(List<DBKTariff> dbkTariffs)
		{
			if (dbkTariffs.Count > 0)
			{
				var xmlWriterConfig = XMLWriterHelper.GetDBKTariffXMLWriterConfiguration(startDate.Value);
				var refData = CreateRefData(dbkTariffs);
				var xmlFilePath = GetOutputTariffXmlFilePath();
				XMLWriterHelper.ExportToXMLFile(
					xmlWriterConfig,
					refData,
					dataSource,
					startDate.Value,
					UpdateType.Full,
					xmlFilePath);
			}
		}

		static List<RefCusTariff> CreateRefData(List<DBKTariff> dbkTariffs)
		{
			var refData = new List<RefCusTariff>();
			foreach (var dbkTariff in dbkTariffs)
			{
				var refTariff = refData.FirstOrDefault(x => x.ZZ1_TariffCode == dbkTariff.TariffCode);
				var alreadyExist = refTariff != null;
				if (!alreadyExist)
				{
					refTariff = new RefCusTariff
					{
						ZZ1_TariffCode = dbkTariff.TariffCode,
						ZZ1_Description = dbkTariff.TariffDescription,
					};

					var relationship = new RefCusTariffRelationship
					{
						ZZH_TariffCode = dbkTariff.TariffCode,
					};
					refTariff.RefCusTariffRelationships = new[] { relationship };
				}

				var rate = new RefCusRate();
				rate.ZZ2_ZY1_NKRateCode = dbkTariff.RateCode;
				var (matchingUnit, factor) = GetMatchingUnitAndFactor(dbkTariff.Unit);

				if (dbkTariff.Rate != 0 && dbkTariff.SpecificRate != 0m)
				{
					rate.ZZ2_RateFormula = $"MIN(VFD * {dbkTariff.Rate.ToString(CultureInfo.InvariantCulture)}, {GetRateFormulaForSpecificRate(dbkTariff.SpecificRate, matchingUnit, factor)})";
					rate.ZZ2_RateFormulaDerivedFrom = $"{dbkTariff.RateString}, Drawback cap per unit in Rs.: {dbkTariff.SpecificRate.ToString(CultureInfo.InvariantCulture)}, Unit: {dbkTariff.Unit}";
				}
				else if (dbkTariff.SpecificRate != 0m)
				{
					rate.ZZ2_RateFormula = GetRateFormulaForSpecificRate(dbkTariff.SpecificRate, matchingUnit, factor);
					rate.ZZ2_RateFormulaDerivedFrom = $"₹{dbkTariff.SpecificRate.ToString(CultureInfo.InvariantCulture)}, Unit: {dbkTariff.Unit}";
				}
				else
				{
					var rateFormula = dbkTariff.Rate == 0 ? "0" : $"VFD * {dbkTariff.Rate.ToString(CultureInfo.InvariantCulture)}";
					rate.ZZ2_RateFormula = rateFormula;
					rate.ZZ2_RateFormulaDerivedFrom = dbkTariff.RateString;
				}

				if (!matchingUnit.IsNullOrEmpty())
				{
					var rateUOM = new RefCusRateUOM()
					{
						ZXG_UOM = matchingUnit,
					};
					rate.RefCusRateUOMs = new[] { rateUOM };
				}

				if (!alreadyExist)
				{
					refTariff.RefCusRates = new[] { rate };
				}
				else
				{
					refTariff.RefCusRates = refTariff.RefCusRates.Append(rate).ToArray();
				}

				if (!alreadyExist)
				{
					refData.Add(refTariff);
				}
			}
			return refData;
		}

		public static (string, int) GetMatchingUnitAndFactor(string unitAndFactorFromCustoms)
		{
			if (unitAndFactorFromCustoms.IsNullOrEmpty())
			{
				return (string.Empty, 0);
			}
			else
			{
				unitAndFactorFromCustoms = Regex.Replace(unitAndFactorFromCustoms, @"[\s\p{P}]", "");
				var unitFromCustoms = Regex.Replace(unitAndFactorFromCustoms, @"[^A-Za-z]", "").ToUpperInvariant();
				if (UnitMapping.TryGetValue(unitFromCustoms, out var matchingUnit))
				{
					var factorMatch = Regex.Match(unitAndFactorFromCustoms, @"\d+");
					var factor = factorMatch.Success ? int.Parse(factorMatch.Value, CultureInfo.InvariantCulture) : 1;
					return (matchingUnit, factor);
				}
				else
				{
					return (string.Empty, 0);
				}
			}
		}

		static Dictionary<string, string> UnitMapping => unitmapping ?? (unitmapping = AppConfig.DBKTariff.UnitMapping);
		static Dictionary<string, string> unitmapping;

		static string GetRateFormulaForSpecificRate(decimal specificRate, string unit, int factor)
		{
			var result = $"{specificRate.ToString(CultureInfo.InvariantCulture)} * [{unit}]";
			if (factor > 1)
			{
				result = $"{result} / {factor.ToString(CultureInfo.InvariantCulture)}";
			}
			return result;
		}

		string GetOutputTariffXmlFilePath()
		{
			var fileName = $"RefCusTariff_IN_DBK_{startDate.Value:yyyyMMdd}.xml";
			var outputPath = AppConfig.Shared.OutputDirectory;
			Directory.CreateDirectory(outputPath);
			var filePath = Path.Combine(outputPath, fileName);
			return filePath;
		}

		static DateTime? GetStartDatetime(string text)
		{
			DateTime? result = null;
			var regex = @"\b(\d{1,2})(st|nd|rd|th)\s+day\s+of\s+([A-Za-z]+),\s+(\d{4})\b";
			var match = Regex.Match(text, regex, RegexOptions.IgnoreCase);
			if (match.Success)
			{
				var dayPart = match.Groups[1].Value;
				var monthName = match.Groups[3].Value.Trim();
				var year = match.Groups[4].Value.Trim();
				if (DateTime.TryParse($"{dayPart} {monthName} {year}", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
				{
					result = date;
				}
			}
			return result;
		}
	}
}
