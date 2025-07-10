using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradeLinesValuePartitionerTest : TestCaseWithFactory
	{
		public void TestPartitionActualValues()
		{
			var actualValues = new Dictionary<TradeLaneKey, TradeLaneValue>();

			var tradeLane1 = CreateTradeLane(new int[] { 5, 6, 25 });
			var tradeLane2 = CreateTradeLane(new int[] { 10, 4, 27, 14 });
			var tradeLane3 = CreateTradeLane(new int[] { 5, 3, 16, 2 });
			var tradeLane4 = CreateTradeLane(new int[] { 7, 15, 20, 6 });

			actualValues.Add(tradeLane1.Key, tradeLane1.Value);
			actualValues.Add(tradeLane2.Key, tradeLane2.Value);
			actualValues.Add(tradeLane3.Key, tradeLane3.Value);
			actualValues.Add(tradeLane4.Key, tradeLane4.Value);

			var partitions = TradeLinesValuePartitioner.PartitionActualValues(actualValues, 50);

			AssertEquals(4, partitions.Count);

			CombineAssertions(() =>
			{
				var partition1 = partitions.Single(x => x.Period == new ZDate(2017, 10, 1));
				AssertEquals(2, partition1.TradeLinesPartitions.Count);
				AssertEquals("10*2 + 5*2 + 7*2", 44, GetPeriodsCount(partition1.TradeLinesPartitions[0]));
				AssertEquals("5*2", 10, GetPeriodsCount(partition1.TradeLinesPartitions[1]));

				var partition2 = partitions.Single(x => x.Period == new ZDate(2017, 9, 1));
				AssertEquals(2, partition2.TradeLinesPartitions.Count);
				AssertEquals("15*2 + 6*2 + 4*2", 50, GetPeriodsCount(partition2.TradeLinesPartitions[0]));
				AssertEquals("3*2", 6, GetPeriodsCount(partition2.TradeLinesPartitions[1]));

				var partition3 = partitions.Single(x => x.Period == new ZDate(2017, 8, 1));
				AssertEquals(5, partition3.TradeLinesPartitions.Count);
				AssertEquals("25*2", 50, GetPeriodsCount(partition3.TradeLinesPartitions[0]));
				AssertEquals("20*2", 40, GetPeriodsCount(partition3.TradeLinesPartitions[1]));
				AssertEquals("16*2", 32, GetPeriodsCount(partition3.TradeLinesPartitions[2]));
				AssertEquals("50", 50, GetPeriodsCount(partition3.TradeLinesPartitions[3]));
				AssertEquals("27*2 - 50", 4, GetPeriodsCount(partition3.TradeLinesPartitions[4]));

				var partition4 = partitions.Single(x => x.Period == new ZDate(2017, 7, 1));
				AssertEquals(1, partition4.TradeLinesPartitions.Count);
				AssertEquals("14*2 + 6*2 + 2*2", 44, GetPeriodsCount(partition4.TradeLinesPartitions[0]));
			});
		}

		public void TestPartitionActualValues_SmallDataSet()
		{
			var actualValues = new Dictionary<TradeLaneKey, TradeLaneValue>();

			var tradeLane1 = CreateTradeLane(new int[] { 1, 1, 1 });
			var tradeLane2 = CreateTradeLane(new int[] { 1, 1, 1, 1 });

			actualValues.Add(tradeLane1.Key, tradeLane1.Value);
			actualValues.Add(tradeLane2.Key, tradeLane2.Value);

			var partitions = TradeLinesValuePartitioner.PartitionActualValues(actualValues, 50);

			AssertEquals(1, partitions.Count);

			CombineAssertions(() =>
			{
				var partition = partitions[0];
				AssertEquals(partition.Period, ZDate.Empty);
				AssertEquals(1, partition.TradeLinesPartitions.Count);
			});
		}

		int GetPeriodsCount(IDictionary<TradeLaneKey, TradeLaneValue> tradeLinesPartition)
		{
			return tradeLinesPartition.Sum(x => x.Value.TradeDetails.Sum(t => t.Value.TradePeriods.Count));
		}

		KeyValuePair<TradeLaneKey, TradeLaneValue> CreateTradeLane(int[] priodSizeList)
		{
			var org2Pk = ZGuid.NewZGuid();
			var org3Pk = ZGuid.NewZGuid();

			var sales = Factory.NewWithValidTestData<OrgSales>();
			sales.OW_MP_Product = ZGuid.NewZGuid();

			var key = new TradeLaneKey(sales);

			var result = new KeyValuePair<TradeLaneKey, TradeLaneValue>(key, new TradeLaneValue());

			for (int i = 0; i < priodSizeList.Length; i++)
			{
				var mode = ((char)i).ToString();
				var type = ((char)i).ToString();

				var period = new ZDate(2017, 10, 1).AddMonths(-i);

				var periodsSize = priodSizeList[i];
				for (int j = 0; j < periodsSize; j++)
				{
					var localClientPk = ZGuid.NewZGuid();

					var tradeLineOrg1 = new TradeLineForTest(
						mode, type, ZGuid.Empty, 3,
						period,
						localClientPk, org2Pk, org3Pk,
						period, 1, 0, 0, 0, 0, 0, 0, "KG", 0,
						"AUD", ZGuid.NewZGuid(),
						100, -50, 0, 0,
						ZGuid.Empty);

					result.Value.AddData(
						localClientPk,
						tradeLineOrg1
					);
				}
			}

			return result;
		}
	}
}
