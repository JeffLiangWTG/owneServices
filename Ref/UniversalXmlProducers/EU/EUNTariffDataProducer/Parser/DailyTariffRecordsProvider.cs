using CargoWise.RefDbRepo.Common.Argument;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IDailyTariffRecordsProvider
	{
		IEnumerable<IRawRateDailyRecord> GetDailyRecords();
	}

	public class DailyTariffRecordsProvider : IDailyTariffRecordsProvider
	{
		public DailyTariffRecordsProvider(
			IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider,
			IEnumerable<string> validMeasureTypeIdsForMeasureConditionInRate)
		{
			this.dailyTariffUpdatesFileProvider = Argument.NotNull(dailyTariffUpdatesFileProvider, nameof(dailyTariffUpdatesFileProvider));
			this.validMeasureTypeIdsForMeasureConditionInRate = Argument.NotNull(validMeasureTypeIdsForMeasureConditionInRate, nameof(validMeasureTypeIdsForMeasureConditionInRate))
				.ToHashSet();
		}

		public IEnumerable<IRawRateDailyRecord> GetDailyRecords()
		{
			var unsupportedMeasureTypes = dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection
				.Select(x => x.MeasureTypeId)
				.Where(x => !validMeasureTypeIdsForMeasureConditionInRate.Contains(x))
				.Distinct()
				.Order()
				.ToList();
			if (unsupportedMeasureTypes.Any())
			{
				Console.WriteLine($"WARNING: (daily updates) Measure types are not supported: {string.Join(", ", unsupportedMeasureTypes)}");
			}

			return dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection
				.Where(x => validMeasureTypeIdsForMeasureConditionInRate.Contains(x.MeasureTypeId));
		}

		readonly IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider;
		readonly HashSet<string> validMeasureTypeIdsForMeasureConditionInRate;
	}
}
