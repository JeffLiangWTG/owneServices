using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class MeasureExclusionParser : ExcelDataParser, IMeasureExclusionParser
	{
		const string idxTariffHeader = "Goods code";
		const string idxAdditionalCode = "Add code";
		const string idxOrderNumber = "Order No.";
		const string idxStartDate = "Start date";
		const string idxEndDate = "End date";
		const string idxOrigin = "Origin";
		const string idxMeasureType = "Measure type";
		const string idxTradeGroup = "Origin code";
		const string idxMeasureTypeId = "Meas. type code";
		const string idxExcludedCountry = "Excluded country";

		public MeasureExclusionParser()
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxAdditionalCode, 1 },
				{ idxOrderNumber, 2 },
				{ idxStartDate, 3 },
				{ idxEndDate, 4 },
				{ idxOrigin, 5 },
				{ idxMeasureType, 6 },
				{ idxTradeGroup, 8 },
				{ idxMeasureTypeId, 9 },
				{ idxExcludedCountry, 10 }
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

			var additionalCode = rawDataRow.GetStringValue(HeaderMap[idxAdditionalCode]);
			var orderNumber = rawDataRow.GetStringValue(HeaderMap[idxOrderNumber]);
			var startDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxStartDate]));
			var endDate = EUNUtils.GetFromOADate(rawDataRow.GetCell(HeaderMap[idxEndDate]), isStartDate: false);
			var origin = rawDataRow.GetStringValue(HeaderMap[idxOrigin]);
			var measureType = rawDataRow.GetStringValue(HeaderMap[idxMeasureType]);
			var tradeGroup = rawDataRow.GetStringValue(HeaderMap[idxTradeGroup]);
			var measureTypeId = rawDataRow.GetStringValue(HeaderMap[idxMeasureTypeId]);
			var excludedCountry = rawDataRow.GetStringValue(HeaderMap[idxExcludedCountry]);

			return new RawMeasureExclusionRecord(tariffHeader, additionalCode, orderNumber, startDate, endDate, origin, measureType, tradeGroup, measureTypeId, excludedCountry);
		}

		public IEnumerable<IRawMeasureExclusionRecord> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath);
			return parsedRecords.Cast<IRawMeasureExclusionRecord>();
		}
	}
}
