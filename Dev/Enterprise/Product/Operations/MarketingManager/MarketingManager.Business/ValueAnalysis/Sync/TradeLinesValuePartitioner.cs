using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	internal class TradeLinesValuePartition
	{
		public TradeLinesValuePartition(ZDate period)
		{
			Period = period;
			TradeLinesPartitions = new List<IDictionary<TradeLaneKey, TradeLaneValue>>(1);
		}
		public ZDate Period { get; private set; }
		public IList<IDictionary<TradeLaneKey, TradeLaneValue>> TradeLinesPartitions { get; private set; }

		public void ClearTradeLinesPartitions()
		{
			foreach (var tradeLinesPartition in TradeLinesPartitions)
			{
				tradeLinesPartition.Clear();
			}
			TradeLinesPartitions.Clear();
		}
	}

	internal static class TradeLinesValuePartitioner
	{
		class TradePeriodData
		{
			public TradePeriodData(TradeLaneKey tradeLaneKey, TradeDetailKey tradeDetailKey, ZString tradeDetailStatus, TradePeriodKey tradePeriodKey, TradePeriodValue tradePeriodValue)
			{
				TradeLaneKey = tradeLaneKey;
				TradeDetailKey = tradeDetailKey;
				TradeDetailStatus = tradeDetailStatus;
				TradePeriodKey = tradePeriodKey;
				TradePeriodValue = tradePeriodValue;
			}

			public TradeLaneKey TradeLaneKey;
			public TradeDetailKey TradeDetailKey;
			public ZString TradeDetailStatus;
			public TradePeriodKey TradePeriodKey;
			public TradePeriodValue TradePeriodValue;
		}

		public static List<TradeLinesValuePartition> PartitionActualValues(IDictionary<TradeLaneKey, TradeLaneValue> actualTradeLanes, int partitionSize)
		{
			List<TradeLinesValuePartition> result;
			var actualTradeLanesSize = actualTradeLanes.Sum(x => x.Value.TradeDetails.Sum(t => t.Value.TradePeriods.Count));
			if (actualTradeLanesSize <= partitionSize)
			{
				result = CreateSinglePartition(actualTradeLanes);
			}
			else
			{
				result = CreatePartitions(actualTradeLanes, partitionSize);
			}
			return result;
		}

		static List<TradeLinesValuePartition> CreateSinglePartition(IDictionary<TradeLaneKey, TradeLaneValue> actualTradeLanes)
		{
			List<TradeLinesValuePartition> result = new List<TradeLinesValuePartition>(1);
			var partition = new TradeLinesValuePartition(ZDate.Empty);
			partition.TradeLinesPartitions.Add(actualTradeLanes);
			result.Add(partition);
			return result;
		}

		static List<TradeLinesValuePartition> CreatePartitions(IDictionary<TradeLaneKey, TradeLaneValue> actualTradeLanes, int partitionSize)
		{
			List<TradeLinesValuePartition> result = new List<TradeLinesValuePartition>(actualTradeLanes.Count);
			var tradePeriods = new List<TradePeriodData>(actualTradeLanes.Count);
			foreach (var tradeLane in actualTradeLanes)
			{
				foreach (var tradeDetail in tradeLane.Value.TradeDetails)
				{
					foreach (var tradePeriod in tradeDetail.Value.TradePeriods)
					{
						tradePeriods.Add(new TradePeriodData(tradeLane.Key, tradeDetail.Key, tradeDetail.Value.StatusCode, tradePeriod.Key, tradePeriod.Value));
					}
				}
			}

			foreach (var periodGroup in tradePeriods.GroupBy(x => x.TradePeriodKey.Period))
			{
				var partition = new TradeLinesValuePartition(periodGroup.Key);
				partition.TradeLinesPartitions.Add(new Dictionary<TradeLaneKey, TradeLaneValue>());

				foreach (var periodTuple in periodGroup)
				{
					if (!partition.TradeLinesPartitions[0].TryGetValue(periodTuple.TradeLaneKey, out TradeLaneValue tradeLaneValue))
					{
						tradeLaneValue = new TradeLaneValue();
						partition.TradeLinesPartitions[0].Add(periodTuple.TradeLaneKey, tradeLaneValue);
					}
					if (!tradeLaneValue.TradeDetails.TryGetValue(periodTuple.TradeDetailKey, out TradeDetailValue tradeDetailValue))
					{
						tradeDetailValue = new TradeDetailValue(periodTuple.TradeDetailStatus);
						tradeLaneValue.TradeDetails.Add(periodTuple.TradeDetailKey, tradeDetailValue);
					}
					if (!tradeDetailValue.TradePeriods.ContainsKey(periodTuple.TradePeriodKey))
					{
						tradeDetailValue.TradePeriods.Add(periodTuple.TradePeriodKey, periodTuple.TradePeriodValue);
					}
				}

				var partitionPeriodsCount = partition.TradeLinesPartitions[0].Sum(x => x.Value.TradeDetails.Sum(t => t.Value.TradePeriods.Count));
				if (partitionPeriodsCount > partitionSize)
				{
					partition = SplitPeriodPartition(partition, partitionSize);
				}

				result.Add(partition);
			}

			return result;
		}

		static TradeLinesValuePartition SplitPeriodPartition(TradeLinesValuePartition partitionToSplit, int partitionSize)
		{
			var result = new TradeLinesValuePartition(partitionToSplit.Period);

			var actualTradeLanes = partitionToSplit.TradeLinesPartitions[0];

			var tradeLanesOrderedByPeriodCount = actualTradeLanes
				.Where(x => x.Value.TradeDetails.Sum(t => t.Value.TradePeriods.Count) <= partitionSize)
				.OrderByDescending(x => x.Value.TradeDetails.Sum(t => t.Value.TradePeriods.Count));

			if (tradeLanesOrderedByPeriodCount.Any())
			{
				var currentPartition = new Dictionary<TradeLaneKey, TradeLaneValue>();
				result.TradeLinesPartitions.Add(currentPartition);
				int partitionAvailableSpace = partitionSize;

				foreach (var tradeLane in tradeLanesOrderedByPeriodCount)
				{
					var tradeLaneSize = tradeLane.Value.TradeDetails.Sum(t => t.Value.TradePeriods.Count);
					if (tradeLaneSize > partitionAvailableSpace)
					{
						currentPartition = new Dictionary<TradeLaneKey, TradeLaneValue>();
						result.TradeLinesPartitions.Add(currentPartition);
						partitionAvailableSpace = partitionSize;
					}
					currentPartition.Add(tradeLane.Key, tradeLane.Value);
					partitionAvailableSpace -= tradeLaneSize;
				}
			}

			foreach (var tradeLane in actualTradeLanes.Where(x => x.Value.TradeDetails.Sum(t => t.Value.TradePeriods.Count) > partitionSize))
			{
				var tradeLanePartitions = SplitTradeLane(tradeLane, partitionSize);
				foreach (var tradeLanePartition in tradeLanePartitions)
				{
					var partition = new Dictionary<TradeLaneKey, TradeLaneValue>();
					partition.Add(tradeLanePartition.Key, tradeLanePartition.Value);
					result.TradeLinesPartitions.Add(partition);
				}
			}

			return result;
		}

		static List<KeyValuePair<TradeLaneKey, TradeLaneValue>> SplitTradeLane(KeyValuePair<TradeLaneKey, TradeLaneValue> tradeLane, int partitionSize)
		{
			var result = new List<KeyValuePair<TradeLaneKey, TradeLaneValue>>((tradeLane.Value.TradeDetails.Sum(x => x.Value.TradePeriods.Count) / partitionSize) + 1);

			var detailsOrderedByPeriodCount = tradeLane.Value.TradeDetails
				.Where(x => x.Value.TradePeriods.Count <= partitionSize)
				.OrderByDescending(x => x.Value.TradePeriods.Count);

			if (detailsOrderedByPeriodCount.Any())
			{
				var subTradeLane = new KeyValuePair<TradeLaneKey, TradeLaneValue>(tradeLane.Key, new TradeLaneValue());
				int subTradeLaneAvailableSpace = partitionSize;
				result.Add(subTradeLane);

				foreach (var detail in detailsOrderedByPeriodCount)
				{
					var detailSize = detail.Value.TradePeriods.Count;
					if (detailSize > subTradeLaneAvailableSpace)
					{
						subTradeLane = new KeyValuePair<TradeLaneKey, TradeLaneValue>(tradeLane.Key, new TradeLaneValue());
						result.Add(subTradeLane);
						subTradeLaneAvailableSpace = partitionSize;
					}
					subTradeLane.Value.TradeDetails.Add(detail);
					subTradeLaneAvailableSpace -= detailSize;
				}
			}

			foreach (var detail in tradeLane.Value.TradeDetails.Where(x => x.Value.TradePeriods.Count > partitionSize))
			{
				var tradeDetailPartitions = SplitTradeDetail(detail, partitionSize);
				foreach (var tradeDetailPartition in tradeDetailPartitions)
				{
					var subTradeLane = new KeyValuePair<TradeLaneKey, TradeLaneValue>(tradeLane.Key, new TradeLaneValue());
					subTradeLane.Value.TradeDetails.Add(tradeDetailPartition.Key, tradeDetailPartition.Value);
					result.Add(subTradeLane);
				}
			}

			return result;
		}

		static List<KeyValuePair<TradeDetailKey, TradeDetailValue>> SplitTradeDetail(KeyValuePair<TradeDetailKey, TradeDetailValue> tradeDetail, int partitionSize)
		{
			var result = new List<KeyValuePair<TradeDetailKey, TradeDetailValue>>((tradeDetail.Value.TradePeriods.Count / partitionSize) + 1);
			var orderedPeriods = tradeDetail.Value.TradePeriods.OrderBy(x => x.Key.Period);
			var periodPartitions = orderedPeriods.Batch(partitionSize);

			foreach (var periodPartition in periodPartitions)
			{
				var subTradeDetail = new KeyValuePair<TradeDetailKey, TradeDetailValue>(tradeDetail.Key, new TradeDetailValue());
				foreach (var period in periodPartition)
				{
					subTradeDetail.Value.TradePeriods.Add(period.Key, period.Value);
				}
				result.Add(subTradeDetail);
			}

			return result;
		}

		#region Constants

		public const int DefaultPartitionSize = 3000;

		#endregion
	}

	static class PartitionLinqExtensions
	{
		public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
		{
			using (var enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					yield return YieldBatchElements(enumerator, batchSize - 1);
				}
			}
		}

		static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
		{
			yield return source.Current;
			for (int i = 0; i < batchSize && source.MoveNext(); i++)
			{
				yield return source.Current;
			}
		}
	}
}
