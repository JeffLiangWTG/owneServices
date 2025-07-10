using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.Warehouse.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(ProcessTasksSyncUserProcessorFactory))]
	public class ProcessTasksSyncUserProcessorFactoryTest : WhsTestCaseWithFactory
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<ProcessTasksSyncUserProcessorFactory>(ObjectFactory.Get<IProcessTasksSyncUserProcessorFactory>());
		}

		public void TestGetProcessTaskSyncUserProcessor()
		{
			var syncUserProcessorFactory = new ProcessTasksSyncUserProcessorFactory();
			var hashTable = new KeyObjectHandleDictionaryObject();
			using (ObjectFactory.Substitute("ProcessTaskSyncUserProcessors", hashTable))
			{
				AssertNull(syncUserProcessorFactory.GetUserSyncProcessor(DummyFormFlowTypeForTesting));

				var mockSyncProcessor = Mock.Of<IProcessTaskSyncUserProcessor>();
				using (ObjectFactory.Substitute("TestProcessor", mockSyncProcessor))
				{
					hashTable.SourceDictionary = new Dictionary<string, string> { { DummyFormFlowTypeForTesting, "TestProcessor" } };

					AssertEquals(mockSyncProcessor, syncUserProcessorFactory.GetUserSyncProcessor(DummyFormFlowTypeForTesting));
					AssertNull(syncUserProcessorFactory.GetUserSyncProcessor("OFT"));
				}
			}
		}

		const string DummyFormFlowTypeForTesting = "DFT";

		public void TestGetRuleProcessor_EndToEnd_Putaway()
		{
			TestGetRuleProcessor_EndToEnd_Core(WarehouseTaskFormFlowTypes.PutawayJob, "WhsPutawayTaskSyncUserProcessor");
		}

		public void TestGetRuleProcessor_EndToEnd_Pick()
		{
			TestGetRuleProcessor_EndToEnd_Core(WarehouseTaskFormFlowTypes.PickJob, "WhsPickTaskSyncUserProcessor");
		}

		public void TestGetRuleProcessor_EndToEnd_DirectedPacking()
		{
			TestGetRuleProcessor_EndToEnd_Core(WarehouseTaskFormFlowTypes.DirectedPackingJob, "WhsDirectedPackingTaskSyncUserProcessor");
		}

		public void TestGetRuleProcessor_EndToEnd_Transfer()
		{
			TestGetRuleProcessor_EndToEnd_Core(WarehouseTaskFormFlowTypes.TransferJob, "WhsTransferTaskSyncUserProcessor");
		}

		public void TestGetRuleProcessor_EndToEnd_Replenishment()
		{
			TestGetRuleProcessor_EndToEnd_Core(WarehouseTaskFormFlowTypes.ReplenishmentJob, "WhsReplenishmentTaskSyncUserProcessor");
		}

		public void TestGetRuleProcessor_EndToEnd_CycleCount()
		{
			TestGetRuleProcessor_EndToEnd_Core(WarehouseTaskFormFlowTypes.CycleCountJob, "WhsCycleCountTaskSyncUserProcessor");
		}

		void TestGetRuleProcessor_EndToEnd_Core(string taskType, string processorName)
		{
			var userSyncProcessorFactory = new ProcessTasksSyncUserProcessorFactory();
			var userSyncProcessor = userSyncProcessorFactory.GetUserSyncProcessor(taskType);
			AssertNotNull(userSyncProcessor);

			var expectedUserSyncProcessor = ObjectFactory.Get<IProcessTaskSyncUserProcessor>(processorName);
			AssertEquals(userSyncProcessor, expectedUserSyncProcessor);
		}
	}
}
