using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class MeasureConditionParser : ExcelDataParser, IMeasureConditionParser
	{
		const string idxTariffHeader = "Goods code";
		const string idxAdditionalCode = "Add code";
		const string idxOrderNumber = "Order No.";
		const string idxStartDate = "Start date";
		const string idxEndDate = "End date";
		const string idxTradeGroup = "Origin code";
		const string idxMeasureTypeId = "Meas. type code";
		const string idxMeasureConditionCode = "Meas. cond";
		const string idxCertificateTypeCode = "Certificate";
		const string idxConditionAmount = "Cond. amount";
		const string idxMonetaryUnit = "Mon. unit";
		const string idxMeasureUnit = "Meas. unit";
		const string idxMeasureAction = "Meas. action";

		public MeasureConditionParser()
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxAdditionalCode, 1 },
				{ idxOrderNumber, 2 },
				{ idxStartDate, 3 },
				{ idxEndDate, 4 },
				{ idxTradeGroup, 5 },
				{ idxMeasureTypeId, 6 },
				{ idxMeasureConditionCode, 7 },
				{ idxCertificateTypeCode, 9 },
				{ idxConditionAmount, 10 },
				{ idxMonetaryUnit, 11 },
				{ idxMeasureUnit, 12 },
				{ idxMeasureAction, 14 }
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
			var tradeGroup = rawDataRow.GetStringValue(HeaderMap[idxTradeGroup]);
			var measureTypeId = rawDataRow.GetStringValue(HeaderMap[idxMeasureTypeId]);
			var measureConditionCode = rawDataRow.GetStringValue(HeaderMap[idxMeasureConditionCode]);
			var certificateTypeCode = rawDataRow.GetStringValue(HeaderMap[idxCertificateTypeCode]);
			var conditionAmount = rawDataRow.GetStringValue(HeaderMap[idxConditionAmount]);
			var monetaryUnit = rawDataRow.GetStringValue(HeaderMap[idxMonetaryUnit]);
			var measureUnit = rawDataRow.GetStringValue(HeaderMap[idxMeasureUnit]);
			var measureAction = rawDataRow.GetStringValue(HeaderMap[idxMeasureAction]);

			if (string.IsNullOrEmpty(measureTypeId)
				|| !ApplicationConfig.MeasureConditionValidMeasureTypeIds.Contains(measureTypeId)
				|| !ApplicationConfig.MeasureConditionValidMeasureActionCode.Contains(measureAction))
			{
				return null;
			}

			return new RawMeasureConditionRecord(tariffHeader, additionalCode, orderNumber, startDate, endDate, tradeGroup,
				measureTypeId, measureConditionCode, certificateTypeCode, conditionAmount, monetaryUnit, measureUnit, measureAction, null);
		}

		public IEnumerable<IRawMeasureConditionRecord> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath);
			return parsedRecords.Cast<IRawMeasureConditionRecord>();
		}
	}
}
