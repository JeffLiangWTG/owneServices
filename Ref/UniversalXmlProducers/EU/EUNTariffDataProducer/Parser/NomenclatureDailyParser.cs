using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class NomenclatureDailyParser : ExcelDataParser, INomenclatureDailyParser
	{
		const string idxTariffHeader = "Goods code";
		const string idxStartDate = "Start date";
		const string idxEndDate = "End date";
		const string idxLanguage = "Language";
		const string idxHierarchyPosition = "Hier. Pos.";
		const string idxLevel = "Indent";
		const string idxDescription = "Description";
		const string idxPublish = "Publish";
		const string idxDeclarableStartDate = "Decl Start Date";
		const string idxSequenceNumber = "Seq Number";

		public override IDictionary<string, int> HeaderMap { get; }

		public NomenclatureDailyParser(string fileName)
			: base()
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxStartDate, 1 },
				{ idxEndDate, 2 },
				{ idxLanguage, 3 },
				{ idxHierarchyPosition, 4 },
				{ idxLevel, 5 },
				{ idxDescription, 6 },
				{ idxPublish, 7 },
				{ idxDeclarableStartDate, 8 },
				{ idxSequenceNumber, 9 },
			};
			this.fileName = Argument.NotNullOrEmpty(fileName, nameof(fileName));
		}

		public override IExcelDataRecord ParseRecord(IRow rawDataRow)
		{
			var rawTariffHeader = rawDataRow.GetStringValue(HeaderMap[idxTariffHeader]);
			if (string.IsNullOrWhiteSpace(rawTariffHeader))
			{
				return null;
			}
			var tariffHeader = rawTariffHeader.Insert(10, " ");
			var startDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxStartDate]), format: DateFormat);
			var endDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxEndDate]), isStartDate: false, format: DateFormat).MidnightToEndOfDay();
			var language = rawDataRow.GetStringValue(HeaderMap[idxLanguage]);
			var hierarchyPosition = rawDataRow.GetStringValue(HeaderMap[idxHierarchyPosition]);
			var level = rawDataRow.GetStringValue(HeaderMap[idxLevel]);
			if (!string.IsNullOrEmpty(level))
			{
				level = level.Replace(" ", string.Empty);
			}

			var description = rawDataRow.GetStringValue(HeaderMap[idxDescription]);
			if (string.IsNullOrEmpty(description))
			{
				description = "-";
			}

			var publish = rawDataRow.GetStringValue(HeaderMap[idxPublish]);
			var declStartDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxDeclarableStartDate]), format: DateFormat);
			var sequenceNumber = int.Parse(rawDataRow.GetStringValue(HeaderMap[idxSequenceNumber]), CultureInfo.InvariantCulture);

			return new RawDailyNomenclatureRecord(tariffHeader,
				startDate,
				endDate,
				language,
				hierarchyPosition,
				level,
				description,
				publish,
				declStartDate,
				sequenceNumber,
				fileName);
		}

		public IEnumerable<IRawNomenclatureDailyRecord> Parse(string filePath)
			=> ParseRecords(filePath).Cast<IRawNomenclatureDailyRecord>();

		const string DateFormat = "dd/MM/yyyy HH:mm";
		readonly string fileName;
	}
}
