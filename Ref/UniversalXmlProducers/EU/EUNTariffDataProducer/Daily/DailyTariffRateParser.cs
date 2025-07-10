using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class DailyTariffRateParser : ExcelDataParser, IDailyTariffRateParser
	{
		const string idxTariffHeader = "GOODS CODE";
		const string idxAdditionalCode = "ADD_CODE";
		const string idxOrderNumber = "ORD_NUMB";
		const string idxStartDate = "START_DATE";
		const string idxEndDate = "END_DATE";
		const string idxReductionIndicator = "RED_IND";
		const string idxLegalBase = "REGULATION";
		const string idxRate = "DUTY";
		const string idxTradeGroup = "GEOGR_AREA";
		const string idxMeasureTypeId = "MEAS_TYP_ID";
		const string idxRecordType = "PUBLISH";

		const string DateTimeFormat = "dd/MM/yyyy hh:mm";
		readonly List<RawRateRecordWithType> _parsedRecords;

		public override IDictionary<string, int> HeaderMap { get; }

		public DailyTariffRateParser()
		{
			_parsedRecords = new List<RawRateRecordWithType>();
			HeaderMap = new Dictionary<string, int>
			{
				{ idxTariffHeader, 0 },
				{ idxAdditionalCode, 1 },
				{ idxOrderNumber, 2 },
				{ idxStartDate, 3 },
				{ idxEndDate, 5 },
				{ idxReductionIndicator, 6 },
				{ idxLegalBase, 7 },
				{ idxRate, 8 },
				{ idxTradeGroup, 9 },
				{ idxMeasureTypeId, 10 },
				{ idxRecordType, 11 }
			};
		}

		public void Parse(string filePath)
		{
			ParseRecords(filePath);
		}

		public IEnumerable<IRawRateRecord> GetParsedRecords()
		{
			return _parsedRecords.Select(x => x.RawRateRecord);
		}

		void CheckDuplicateOnRecordType(RawRateRecord record, string recordType)
		{
			switch (recordType)
			{
				case "INSERT":
				case "UPDATE":
					CheckDuplicateUpdateRecordAndAddNew(record, recordType);
					break;
				case "DELETE":
					CheckRecordIsDeleted(record);
					break;
				default:
					throw new ArgumentException("RecordType is invalid.");
			}
		}

		void CheckDuplicateUpdateRecordAndAddNew(RawRateRecord record, string recordType)
		{
			var existingRecord = _parsedRecords.FirstOrDefault(x => RawRateRecordComparer.AreEqual(x.RawRateRecord, record));
			if (existingRecord != null)
			{
				if (recordType == "UPDATE" || (recordType == "INSERT" && existingRecord.RecordType == "UPDATE"))
				{
					_parsedRecords.Remove(existingRecord);
				}
				else if (recordType == "INSERT")
				{
					throw new ArgumentException("Cannot insert a duplicate record with RecordType INSERT.");
				}
			}
			_parsedRecords.Add(new RawRateRecordWithType(recordType, record));
		}

		void CheckRecordIsDeleted(RawRateRecord record)
		{
			var rawRateRecord = _parsedRecords.FirstOrDefault(x => RawRateRecordComparer.AreEqual(x.RawRateRecord, record));
			if (rawRateRecord != null)
			{
				_parsedRecords.Remove(rawRateRecord);
			}
		}

		public override IExcelDataRecord ParseRecord(IRow rawDataRow)
		{
			var tariffHeader = rawDataRow.GetStringValue(HeaderMap[idxTariffHeader]);

			if (tariffHeader == null)
			{
				return null;
			}

			var additionalCode = CheckStringValue(rawDataRow.GetStringValue(HeaderMap[idxAdditionalCode]));
			var orderNumber = CheckStringValue(rawDataRow.GetStringValue(HeaderMap[idxOrderNumber]));

			var startDate =  EUNUtils.MinDateTime;
			var endDate = EUNUtils.MaxDateTime;
			var startDateStringValue = rawDataRow.GetStringValue(HeaderMap[idxStartDate]);
			if (!string.IsNullOrEmpty(startDateStringValue) && DateTime.TryParseExact(startDateStringValue, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateValue))
			{
				startDate = startDateValue;
			}

				var endDateStringValue = rawDataRow.GetStringValue(HeaderMap[idxEndDate]);
			if (!string.IsNullOrEmpty(endDateStringValue) && DateTime.TryParseExact(endDateStringValue, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateValue))
			{
				endDate = endDateValue.MidnightToEndOfDay();
			}

			var reductionIndicator = CheckStringValue(rawDataRow.GetStringValue(HeaderMap[idxReductionIndicator]));
			var legalBase = rawDataRow.GetStringValue(HeaderMap[idxLegalBase]);
			var rate = CheckStringValue(rawDataRow.GetStringValue(HeaderMap[idxRate]));
			var tradeGroup = rawDataRow.GetStringValue(HeaderMap[idxTradeGroup]);
			var measureTypeId = rawDataRow.GetStringValue(HeaderMap[idxMeasureTypeId]);
			var recordType = rawDataRow.GetStringValue(HeaderMap[idxRecordType]);

			var rawRateRecord = new RawRateRecord(tariffHeader, additionalCode, orderNumber, startDate, endDate, string.Empty, string.Empty, legalBase, tradeGroup, measureTypeId, rate, reductionIndicator, IsImport);
			CheckDuplicateOnRecordType(rawRateRecord, recordType);
			return rawRateRecord;
		}

		static string CheckStringValue(string originValue)
		{
			if (originValue == null || originValue.Equals("N/A", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}
			return originValue;
		}

		public bool IsImport { get; set; }

		class RawRateRecordWithType
		{
			public RawRateRecordWithType(string recordType, IRawRateRecord rawRateRecord)
			{
				RecordType = recordType;
				RawRateRecord = rawRateRecord;
			}

			public string RecordType { get; }
			public IRawRateRecord RawRateRecord { get; }
		}
	}
}
