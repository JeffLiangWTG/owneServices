
namespace Enterprise.Rating.DataTransfer.TACT
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Threading;

	public class TACTImportResult
	{
		public enum SkipReason
		{
			UnknownCarrier,
			UnknownCategory,
			UnknownCarrierServiceLevel,
			Invalid,
			Expired,
			ActionIsDelete
		}

		public IDictionary<string, int> UnknownCarrierServiceLevel => unknownCarrierServiceLevel;
		public IDictionary<string, int> UnknownCategories => unknownCategories;
		public IDictionary<string, int> UnknownCarriers => unknownCarriers;

		public long TotalRates { get; private set; }
		public long ProcessedRates => processedRates;
		public long ImportedRates => importedRates;
		public long ExpiredRates => expiredRates;
		public long InvalidRates => invalidRates;
		public long DeleteActionRates => deleteActionRates;

		public void AddSkippedRates(int count, SkipReason reason, string state = null)
		{
			switch (reason)
			{
				case SkipReason.Expired:
					Interlocked.Add(ref expiredRates, count);
					break;

				case SkipReason.Invalid:
					Interlocked.Add(ref invalidRates, count);
					break;

				case SkipReason.ActionIsDelete:
					Interlocked.Add(ref deleteActionRates, count);
					break;

				case SkipReason.UnknownCarrier:
					unknownCarriers.AddOrUpdate(state, count, (key, value) => value + count);
					break;

				case SkipReason.UnknownCategory:
					unknownCategories.AddOrUpdate(state, count, (key, value) => value + count);
					break;

				case SkipReason.UnknownCarrierServiceLevel:
					unknownCarrierServiceLevel.AddOrUpdate(state, count, (key, value) => value + count);
					break;
			}

			Interlocked.Add(ref processedRates, count);
		}

		public void AddConvertedRates(int count)
		{
			Interlocked.Add(ref importedRates, count);
			Interlocked.Add(ref processedRates, count);
		}

		public void SetTotalRatesCount(long count)
		{
			TotalRates = count;
		}

		readonly ConcurrentDictionary<string, int> unknownCarrierServiceLevel = new ConcurrentDictionary<string, int>();
		readonly ConcurrentDictionary<string, int> unknownCategories = new ConcurrentDictionary<string, int>();
		readonly ConcurrentDictionary<string, int> unknownCarriers = new ConcurrentDictionary<string, int>();
		long expiredRates;
		long invalidRates;
		long importedRates;
		long processedRates;
		long deleteActionRates;
	}
}
