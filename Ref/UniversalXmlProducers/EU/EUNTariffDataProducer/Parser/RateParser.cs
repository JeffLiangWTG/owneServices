using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RateParser : ExcelDataParser, IRateParser
	{
		const string idxTariffHeader = "Goods code";
		const string idxAdditionalCode = "Add code";
		const string idxOrderNumber = "Order No.";
		const string idxStartDate = "Start date";
		const string idxEndDate = "End date";
		const string idxOrigin = "Origin";
		const string idxMeasureType = "Measure type";
		const string idxLegalBase = "Legal base";
		const string idxRate = "Duty";
		const string idxTradeGroup = "Origin code";
		const string idxMeasureTypeId = "Meas. type code";
		const string idxReductionIndicator = "RED_IND";

		public RateParser(bool isImport)
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxAdditionalCode, 1 },
				{ idxOrderNumber, 2 },
				{ idxStartDate, 3 },
				{ idxEndDate, 4 },
				{ idxReductionIndicator, 5 },
				{ idxOrigin, 6 },
				{ idxMeasureType, 7 },
				{ idxLegalBase, 8 },
				{ idxRate, 9 },
				{ idxTradeGroup, 10 },
				{ idxMeasureTypeId, 11 }
			};

			IsImport = isImport;
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
			var legalBase = rawDataRow.GetStringValue(HeaderMap[idxLegalBase]);
			var rate = rawDataRow.GetStringValue(HeaderMap[idxRate]);
			var tradeGroup = rawDataRow.GetStringValue(HeaderMap[idxTradeGroup]);
			var measureTypeId = rawDataRow.GetStringValue(HeaderMap[idxMeasureTypeId]);
			var reductionIndicator = rawDataRow.GetStringValue(HeaderMap[idxReductionIndicator]) ?? string.Empty;

			return new RawRateRecord(tariffHeader, additionalCode, orderNumber, startDate, endDate, origin, measureType, legalBase, tradeGroup, measureTypeId, rate, reductionIndicator, IsImport);
		}

		public IEnumerable<IRawRateRecord> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath);
			return parsedRecords.Cast<IRawRateRecord>();
		}

		bool IsImport { get; set; }
	}
}
