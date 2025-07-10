using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		#region TestGetTypeForLoad

		public void TestGetTypeForLoad()
		{
			var order = Factory.New<WhsOrder>();
			var receive = Factory.New<WhsReceive>();
			var adjustment = Factory.New<WhsAdjustment>();
			var transfer = Factory.New<WhsTransfer>();
			var workOrder = Factory.New<WhsWorkOrder>();
			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();

			var loadStrategy = new WhsDocketProcessTaskLoadStrategy();
			AssertEquals(typeof(WhsOrderProcessTasks), loadStrategy.GetTypeForLoad(WhsDocketSchema.Constants.Prefix, order.PK, Factory));
			AssertEquals(typeof(WhsReceiveProcessTasks), loadStrategy.GetTypeForLoad(WhsDocketSchema.Constants.Prefix, receive.PK, Factory));
			AssertEquals(typeof(WhsAdjustmentProcessTasks), loadStrategy.GetTypeForLoad(WhsDocketSchema.Constants.Prefix, adjustment.PK, Factory));
			AssertEquals(typeof(WhsTransferProcessTasks), loadStrategy.GetTypeForLoad(WhsDocketSchema.Constants.Prefix, transfer.PK, Factory));
			AssertEquals(typeof(WhsWorkOrderProcessTasks), loadStrategy.GetTypeForLoad(WhsDocketSchema.Constants.Prefix, workOrder.PK, Factory));
			AssertEquals(typeof(WhsDynamicWorkOrderProcessTasks), loadStrategy.GetTypeForLoad(WhsDocketSchema.Constants.Prefix, dynamicWorkOrder.PK, Factory));
		}

		#endregion

		#region TestAddAdditionalParentFilters

		public void TestAddAdditionalParentFilters()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var order = Factory.NewWithValidTestData<WhsOrder>();
			var orderTask = order.WorkflowItems.AddNew();

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var receiveTask = receive.WorkflowItems.AddNew();

			var adjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			var adjustmentTask = adjustment.WorkflowItems.AddNew();

			var transfer = Factory.NewWithValidTestData<WhsTransfer>();
			var transferTask = transfer.WorkflowItems.AddNew();

			var workOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			var workOrderTask = workOrder.WorkflowItems.AddNew();

			Factory.Save();

			var processTasks1 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode);
			AssertEquals(1, processTasks1.Length);
			AssertCollectionContains(adjustmentTask, processTasks1);

			var processTask2 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.WhsOrderWorkflowDescriptorCode);
			AssertEquals(1, processTask2.Length);
			AssertCollectionContains(orderTask, processTask2);

			var processTask3 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode);
			AssertEquals(1, processTask3.Length);
			AssertCollectionContains(receiveTask, processTask3);

			var processTasks4 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.WhsTransferWorkflowDescriptorCode);
			AssertEquals(1, processTasks4.Length);
			AssertCollectionContains(transferTask, processTasks4);

			var processTasks5 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.WhsWorkOrderWorkflowDescriptorCode);
			AssertEquals(1, processTasks5.Length);
			AssertCollectionContains(workOrderTask, processTasks5);
		}

		ProcessTask[] GetProcessTasksForTestAddAdditionalParentFilters(string code)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(WhsDocket), ProcessTasksSchema.P9_ParentID);
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(code);
			new WhsDocketProcessTaskLoadStrategy().AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return Factory.Load<ProcessTask>(query);
		}

		#endregion
	}
}
