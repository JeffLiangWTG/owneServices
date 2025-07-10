using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	class DailyRecordMerger
	{
		public DailyRecordMerger(IEnumerable<IRawRateRecord> records)
		{
			this.records = Argument.NotNull(records, nameof(records));
		}

		public IEnumerable<IRawRateRecord> MergeWithDailyRecords(IEnumerable<IRawRateDailyRecord> dailyRecords)
		{
			var monthlyRateRecordDictionary = records.ToDictionary(x => GetRateKey(x));

			var orderedDailyRecords = dailyRecords.OrderBy(x => x.FileName).ThenBy(x => x.SequenceNumber);
			foreach (var dailyRecord in orderedDailyRecords)
			{
				var dailyRateKey = GetRateKey(dailyRecord);

				switch (dailyRecord.OperationType)
				{
					case "UPDATE":
					case "INSERT":
						monthlyRateRecordDictionary[dailyRateKey] = dailyRecord;
						break;

					case "DELETE":
						monthlyRateRecordDictionary.Remove(dailyRateKey);
						break;

					default:
						break;
				}
			}

			return monthlyRateRecordDictionary.Values;
		}

		static string GetRateKey(IRawRateRecord rawRateRecord)
			=> $"{rawRateRecord.TariffHeader}_{rawRateRecord.AdditionalCode}_{rawRateRecord.OrderNumber}_{rawRateRecord.StartDate:yyyyMMdd}_{rawRateRecord.TradeGroup}_{rawRateRecord.MeasureTypeId}";

		readonly IEnumerable<IRawRateRecord> records;
	}
}
