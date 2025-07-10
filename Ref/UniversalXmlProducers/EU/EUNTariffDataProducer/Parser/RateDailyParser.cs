using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RateDailyParser : ExcelDataParser, IRateDailyParser
	{
		const string idxTariffHeader = "Goods code";
		const string idxAdditionalCode = "Add code";
		const string idxOrderNumber = "Order No.";
		const string idxStartDate = "Start date";
		const string idxDescrStartDate = "Descr Start date";
		const string idxEndDate = "End date";
		const string idxReductionIndicator = "RED_IND";
		const string idxRegulation = "Regulation";
		const string idxDuty = "Duty";
		const string idxGeogrArea = "Geogr Area";
		const string idxMeasureTypeId = "Meas. type Id";
		const string idxPublish = "Publish";
		const string idxSequenceNumber = "Record Sequence Number";

		public RateDailyParser(string fileName)
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxAdditionalCode, 1 },
				{ idxOrderNumber, 2 },
				{ idxStartDate, 3 },
				{ idxDescrStartDate, 4 },
				{ idxEndDate, 5 },
				{ idxReductionIndicator, 6 },
				{ idxRegulation, 7 },
				{ idxDuty, 8 },
				{ idxGeogrArea, 9 },
				{ idxMeasureTypeId, 10 },
				{ idxPublish, 11 },
				{ idxSequenceNumber, 12 },
			};
			this.fileName = Argument.NotNullOrEmpty(fileName, nameof(fileName));
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override IExcelDataRecord ParseRecord(IRow rawDataRow)
		{
			var tariffHeader = rawDataRow.GetStringValue(HeaderMap[idxTariffHeader]);
			if (string.IsNullOrEmpty(tariffHeader))
			{
				return null;
			}

			var additionalCode = rawDataRow.GetStringValue(HeaderMap[idxAdditionalCode]);
			var orderNumber = rawDataRow.GetStringValue(HeaderMap[idxOrderNumber]);
			var startDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxStartDate]), format: DateFormat);
			var endDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxEndDate]), isStartDate: false, format: DateFormat).MidnightToEndOfDay();
			var reductionIndicator = rawDataRow.GetStringValue(HeaderMap[idxReductionIndicator]) ?? string.Empty;
			var regulation = rawDataRow.GetStringValue(HeaderMap[idxRegulation]);
			var duty = rawDataRow.GetStringValue(HeaderMap[idxDuty]);
			var geogArea = rawDataRow.GetStringValue(HeaderMap[idxGeogrArea]);
			var measureTypeId = rawDataRow.GetStringValue(HeaderMap[idxMeasureTypeId]);
			var publish = rawDataRow.GetStringValue(HeaderMap[idxPublish]);
			var sequenceNumber = int.Parse(rawDataRow.GetStringValue(HeaderMap[idxSequenceNumber]), CultureInfo.InvariantCulture);
			var isImport = GetIsImport(measureTypeId);

			return new RawRateDailyRecord(tariffHeader,
				additionalCode,
				orderNumber,
				startDate,
				endDate,
				regulation,
				geogArea,
				measureTypeId,
				duty,
				reductionIndicator,
				isImport,
				publish,
				sequenceNumber,
				fileName);
		}

		public IEnumerable<IRawRateDailyRecord> Parse(string filePath)
			 => ParseRecords(filePath).Cast<IRawRateDailyRecord>();

		static bool GetIsImport(string measureTypeId)
			=> ApplicationConfig.ValidMeasureTypeIdsForMeasureConditionInImportRate.Contains(measureTypeId)
			|| !ApplicationConfig.ValidMeasureTypeIdsForMeasureConditionInExportRate.Contains(measureTypeId);

		const string DateFormat = "dd/MM/yyyy HH:mm";
		readonly string fileName;
	}
}
