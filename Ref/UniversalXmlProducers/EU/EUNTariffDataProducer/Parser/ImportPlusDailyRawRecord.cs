using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ImportPlusDailyRawRecord : ImportRawRecord
	{
		public ImportPlusDailyRawRecord(IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider)
		{
			Argument.NotNull(dailyTariffUpdatesFileProvider, nameof(dailyTariffUpdatesFileProvider));
			dailyTariffRecordsProvider = new DailyTariffRecordsProvider(dailyTariffUpdatesFileProvider, ValidMeasureTypeIdsForMeasureConditionInRate);
		}

		protected override IEnumerable<IRawRateRecord> FinalizeDutyFileParsing(IEnumerable<IRawRateRecord> records) =>
			new DailyRecordMerger(records).MergeWithDailyRecords(dailyTariffRecordsProvider.GetDailyRecords());

		readonly IDailyTariffRecordsProvider dailyTariffRecordsProvider;
	}
}
