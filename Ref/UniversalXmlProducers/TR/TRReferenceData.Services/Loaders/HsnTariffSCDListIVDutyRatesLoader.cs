using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class HsnTariffSCDListIVDutyRatesLoader
	{
		public static IEnumerable<HsnTariffSCDListIVDutyRate> GetRates(string inputPath, StringBuilder logger = null)
		{
			var result = new List<HsnTariffSCDListIVDutyRate>();

			if (File.Exists(inputPath))
			{
				var xlsFile = new XlsFile(inputPath) { ActiveSheet = 1 };
				var xlsRows = GetRows(xlsFile).ToArray();

				var lastRateWithCodeIndex = 0;
				for (var rowIndex = 0; rowIndex < xlsRows.Length; rowIndex++)
				{
					var row = xlsRows[rowIndex];

					var rowCode = row.code;
					if (!string.IsNullOrWhiteSpace(rowCode))
					{
						lastRateWithCodeIndex = rowIndex;
						if (!string.IsNullOrWhiteSpace(row.rate))
						{
							result.Add(new HsnTariffSCDListIVDutyRate(
								tariffCodePattern: row.code,
								description: row.description,
								rate: decimal.Parse(row.rate, CultureInfo.InvariantCulture) / 100,
								exemptedTariffCodes: row.exempted.Split(',').EmptyIfNoInvalid(),
								additionalCode: row.additionalCode,
								startDate: row.start,
								endDate: row.end
							));
						}
						else
						{
							continue;
						}
					}
					else
					{
						var parentRow = xlsRows[lastRateWithCodeIndex];

						string rateToUse = string.Empty;
						if (string.IsNullOrWhiteSpace(row.rate))
						{
							if (!string.IsNullOrWhiteSpace(parentRow.rate))
							{
								rateToUse = parentRow.rate;
							}
						}
						else
						{
							rateToUse = row.rate;
						}

						if (!string.IsNullOrWhiteSpace(rateToUse))
						{
							result.Add(new HsnTariffSCDListIVDutyRate(
								tariffCodePattern: parentRow.code,
								description: parentRow.description + row.description,
								rate: decimal.Parse(rateToUse, CultureInfo.InvariantCulture) / 100,
								exemptedTariffCodes: row.exempted.Split(',').EmptyIfNoInvalid(),
								additionalCode: row.additionalCode,
								startDate: row.start,
								endDate: row.end
							));
						}
						else
						{
							logger.AppendLine(CultureInfo.InvariantCulture, $"Line: {rowIndex}, unable to find a rate, either in current row or its presumptive parent row({parentRow}).");
						}
					}
				}
			}

			return result;
		}

		static IEnumerable<(string code, string description, string rate, string exempted, string additionalCode, string start, string end)> GetRows(XlsFile xlsFile)
		{
			var rowCount = xlsFile.RowCount;
			for (var row = 2; row <= rowCount; row++)
			{
				yield return GetRow(xlsFile, row);
			}
		}

		static (string code, string description, string rate, string exempted, string additionalCode, string start, string end) GetRow(XlsFile xlsFile, int row)
		{
			const string TariffFormatSplitter = ".";
			return (
				code: xlsFile.GetTrimmedStringFromCell(row, 1).Replace(TariffFormatSplitter, string.Empty),
				description: xlsFile.GetTrimmedStringFromCell(row, 2),
				rate: xlsFile.GetTrimmedStringFromCell(row, 3).Replace(",", "."),
				exempted: xlsFile.GetTrimmedStringFromCell(row, 4).Replace(TariffFormatSplitter, string.Empty).ClearWhiteSpaces(),
				additionalCode: xlsFile.GetTrimmedStringFromCell(row, 5),
				start: xlsFile.GetTrimmedStringFromCell(row, 6),
				end: xlsFile.GetTrimmedStringFromCell(row, 7)
			);
		}
	}
}
