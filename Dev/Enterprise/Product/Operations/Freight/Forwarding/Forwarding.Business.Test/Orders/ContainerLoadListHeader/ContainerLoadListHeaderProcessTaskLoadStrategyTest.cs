using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.Orders.ContainerLoadListHeader;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class ContainerLoadListHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestContanerLoadListHeaderProcessTaskLoadStrategyGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				RunTypeTest<ContainerLoadListProcessTask>(Factory.New<CYContainerLoadList>());
				RunTypeTest<ContainerLoadPlanProcessTask>(Factory.New<CFSContainerLoadList>());
			}
		}

		void RunTypeTest<TProcess>(CommonContainerLoadList clh)
		{
			var strategy = new ContainerLoadListHeaderProcessTaskLoadStrategy();
			AssertNull(strategy.GetTypeForLoad(ContainerLoadListHeaderSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertNull(strategy.GetTypeForLoad(ContainerLoadListHeaderSchema.Constants.Prefix, ZGuid.Invalid, Factory));
			ErrorReporter.Clear();

			AssertEquals(ObjectFactory.GetType<TProcess>(), strategy.GetTypeForLoad(clh.TablePrefix, clh.PK, Factory));
			AssertNull(strategy.GetTypeForLoad("Z@", clh.PK, Factory));
		}

		public void TestAddAdditionalParentFilters()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var cyContainerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var cyContainerLoadListTask = cyContainerLoadList.WorkflowItems.AddNew();

			var cfsContainerLoadList = Factory.NewWithValidTestData<CFSContainerLoadList>();
			var cfsContainerLoadListTask = cfsContainerLoadList.WorkflowItems.AddNew();

			Factory.Save();

			var processTasks1 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.ContainerLoadListWorkflowDescriptorCode);
			AssertEquals(1, processTasks1.Length);
			AssertCollectionContains(cyContainerLoadListTask, processTasks1);

			var processTask2 = GetProcessTasksForTestAddAdditionalParentFilters(WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode);
			AssertEquals(1, processTask2.Length);
			AssertCollectionContains(cfsContainerLoadListTask, processTask2);
		}

		ProcessTask[] GetProcessTasksForTestAddAdditionalParentFilters(string code)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(CommonContainerLoadList), ProcessTasksSchema.P9_ParentID);
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(code);
			new ContainerLoadListHeaderProcessTaskLoadStrategy().AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return Factory.Load<ProcessTask>(query);
		}
	}
}
