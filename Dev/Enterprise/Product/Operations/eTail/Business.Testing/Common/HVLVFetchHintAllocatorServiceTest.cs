using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVFetchHintAllocatorServiceTest : TestCaseWithFactory
	{
		public void TestIncrementCount()
		{
			var service = new HVLVFetchHintAllocatorService<BusinessObjectCollection>(Factory, null);

			var testClusterKey1 = 10;
			service.IncrementCount(testClusterKey1);
			AssertEquals("clusterKey 1: 1", 1, service.GetCollectionCountForTesting(testClusterKey1));
			service.IncrementCount(testClusterKey1);
			AssertEquals("clusterKey 1: 2", 2, service.GetCollectionCountForTesting(testClusterKey1));

			var testClusterKey2 = 20;
			service.IncrementCount(testClusterKey2);
			AssertEquals("clusterKey 2: 1", 1, service.GetCollectionCountForTesting(testClusterKey2));
		}

		public void TestIncrementCount_AddsFetchHint()
		{
			var clusterKeyColumn = HVLVItemSchema.HVI_ClusterKey;
			var service = new HVLVFetchHintAllocatorService<BusinessObjectCollection>(Factory, clusterKeyColumn);

			var testClusterKey1 = 10;

			for (var i = 1; i < service.ThresholdToApplyFetchHintExposed; i++)
			{
				service.IncrementCount(testClusterKey1);
			}

			AssertEquals("No fetch hints should yet be added before threshold reached", 0, Factory.ActiveTableFetchHints);

			service.IncrementCount(testClusterKey1);
			AssertEquals(service.ThresholdToApplyFetchHintExposed, service.GetCollectionCountForTesting(testClusterKey1));

			AssertEquals("A Fetch hint should now be added", 1, Factory.ActiveFetchHintsForTable(clusterKeyColumn.TableSchema.TableName));

			Factory.AddFetchHint(clusterKeyColumn.TableSchema, new ZQuery(clusterKeyColumn, testClusterKey1));
			AssertEquals("Fetch hint number should be 1 because this should be the exact fetch hint added by the service", 1, Factory.ActiveFetchHintsForTable(clusterKeyColumn.TableSchema.TableName));
		}

		public void TestThresholdToApplyFetchHint()
		{
			var service = new HVLVFetchHintAllocatorService<BusinessObjectCollection>(null, null);
			AssertGreaterThanOrEqualTo("Threshold should be greater than or equal to 2", service.ThresholdToApplyFetchHintExposed, 2);
		}

		public void TestSetCounterForTesting()
		{
			var service = new HVLVFetchHintAllocatorService<BusinessObjectCollection>(null, null);
			var count = 4;
			service.SetCounterForTesting(10, count);
			AssertEquals(count, service.GetCollectionCountForTesting(10));
		}
	}
}
