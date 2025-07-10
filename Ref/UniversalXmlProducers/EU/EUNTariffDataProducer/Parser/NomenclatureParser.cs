using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class NomenclatureParser : ExcelDataParser, INomenclatureParser
	{
		const string idxTariffHeader = "Goods code";
		const string idxStartDate = "Start date";
		const string idxEndDate = "End date";
		const string idxLanguage = "Language";
		const string idxHierarchyPosition = "Hier. Pos.";
		const string idxLevel = "Indent";
		const string idxDescription = "Description";

		public NomenclatureParser()
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxStartDate, 1 },
				{ idxEndDate, 2 },
				{ idxLanguage, 3 },
				{ idxHierarchyPosition, 4 },
				{ idxLevel, 5 },
				{ idxDescription, 6 }
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override IExcelDataRecord ParseRecord(IRow rawDataRow)
		{
			var tariffHeader = rawDataRow.GetStringValue(HeaderMap[idxTariffHeader]);
			if (string.IsNullOrEmpty(tariffHeader))
			{
				return null;
			}

			var description = rawDataRow.GetStringValue(HeaderMap[idxDescription]);
			if (string.IsNullOrEmpty(description))
			{
				return null;
			}
			else
			{
				description = description.Replace("|", " ");
			}

			var startDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxStartDate]));
			var endDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxEndDate]), isStartDate: false);
			var language = rawDataRow.GetStringValue(HeaderMap[idxLanguage]);
			var hierarchyPosition = rawDataRow.GetStringValue(HeaderMap[idxHierarchyPosition]);
			var level = rawDataRow.GetStringValue(HeaderMap[idxLevel]);

			if (!EUNUtils.IsValidHierarchyPosition(hierarchyPosition))
			{
				var message = $"WARNING: Skipping nomenclature record with invalid hierarchy position. Tariff:'{tariffHeader}', HierarchyPosition:'{hierarchyPosition}', Level:'{level}', Language:'{language}', Description:'{description}', StartDate:'{startDate}', EndDate:'{endDate}'";
				Console.Error.WriteLine(message);
				return null;
			}

			if (!string.IsNullOrEmpty(level))
			{
				level = level.Replace(" ", string.Empty);
			}

			return new RawNomenclatureRecord(tariffHeader, startDate, endDate, language, hierarchyPosition, level, description);
		}

		public IEnumerable<IRawNomenclatureRecord> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath);
			return parsedRecords.Cast<IRawNomenclatureRecord>();
		}
	}
}
