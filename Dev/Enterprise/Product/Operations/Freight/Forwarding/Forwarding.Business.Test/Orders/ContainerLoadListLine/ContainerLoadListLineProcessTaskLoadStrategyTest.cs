using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class ContainerLoadListLineProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var strategy = new ContainerLoadListLineProcessTaskLoadStrategy();
			AssertEquals(typeof(ContainerLoadListLineProcessTask), strategy.GetTypeForLoad(ContainerLoadListLineSchema.Constants.Prefix, ZGuid.Empty, Factory));
		}

		public void TestAddAdditionalParentFilters()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var containerloadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			containerloadListLine.CLL_LoadMode = CommonContainerLoadListLoadModeList.Codes.CY;
			containerloadListLine.CLL_JSL_BookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>().PK;
			var cliTask = containerloadListLine.WorkflowItems.AddNew();

			var cargoLoadPlanLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			cargoLoadPlanLine.CLL_LoadMode = CommonContainerLoadListLoadModeList.Codes.CFS;
			cargoLoadPlanLine.CLL_JSL_BookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>().PK;
			var cplTask = cargoLoadPlanLine.WorkflowItems.AddNew();

			Factory.Save();

			var processTask1 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.ContainerLoadListLineWorkflowDescriptorCode);
			AssertEquals(1, processTask1.Length);
			AssertCollectionContains(cliTask, processTask1);

			var processTask2 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.CargoLoadPlanLineWorkflowDescriptorCode);
			AssertEquals(1, processTask2.Length);
			AssertCollectionContains(cplTask, processTask2);
		}

		ProcessTask[] GetProcessTasksForTestAddAdditionalParentFilters(string code)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ProcessTasksSchema.P9_ParentID);
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(code);
			new ContainerLoadListLineProcessTaskLoadStrategy().AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return Factory.Load<ProcessTask>(query);
		}
	}
}
