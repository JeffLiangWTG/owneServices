using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Module.Testing
{
	sealed class WorkflowFilterStripsHelperWithRoutingSupportTest : TestCaseWithFactory
	{
		public void TestWorkflowFilterTypes()
		{
			var type = typeof(CommonConsolWithWorkflow);
			var helper = new WorkflowFilterStripsHelperWithRoutingSupport(type, "", Factory);
			helper.Initialise(type, Factory);

			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			AssertEquals(typeof(WorkflowModuleFilterWithRoutingSupport), filters["Milestone Date"].GetType());
			AssertEquals(typeof(WorkflowModuleTextFilterWithRoutingSupport), filters["Milestone Completed"].GetType());
			AssertEquals(typeof(WorkflowModuleFilterWithRoutingSupport), filters["Next Milestone"].GetType());
			AssertEquals(typeof(WorkflowModuleFilterWithRoutingSupport), filters["Last Completed Milestone"].GetType());
		}

		public void TestWorkflowRelatedCollectionFilterTypes()
		{
			var type = typeof(CommonConsolWithWorkflow);
			var helper = new WorkflowFilterStripsHelperWithRoutingSupportForTest(type, "", Factory);
			helper.Initialise(type, Factory);

			var filters = new ModuleFilterCollection();
			helper.AddRelatedMilestoneFilters_Exposed(filters);

			AssertEquals(typeof(WorkflowModuleFilterWithRoutingSupport), filters["Milestone Date (Related)"].GetType());
			AssertEquals(typeof(WorkflowModuleTextFilterWithRoutingSupport), filters["Milestone Completed (Related)"].GetType());
			AssertEquals(typeof(WorkflowModuleFilterWithRoutingSupport), filters["Next Milestone (Related)"].GetType());
			AssertEquals(typeof(WorkflowModuleFilterWithRoutingSupport), filters["Last Completed Milestone (Related)"].GetType());
		}

		class CommonConsolWithWorkflow : CommonConsol, IWorkflowProvider
		{
			public CommonConsolWithWorkflow(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				AutomaticallyUpdatePackLineContainers = true;
			}

			#region IWorkflowProvider Members

			public IWorkflowInformationProvider GetWorkflowInformationProvider()
			{
				throw new NotImplementedException();
			}

			public IProcessHeaderCollection Workflows => throw new NotImplementedException();

			public ProcessTaskCollection WorkflowItems
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IWorkflowProviderCore Members

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				throw new NotImplementedException();
			}

			public ZString WorkflowType
			{
				get { return "DUM"; }
			}

			#endregion
		}

		class WorkflowFilterStripsHelperWithRoutingSupportForTest : WorkflowFilterStripsHelperWithRoutingSupport
		{
			public WorkflowFilterStripsHelperWithRoutingSupportForTest(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
				: base(businessObjectType, templateCode, factory)
			{
			}

			public void AddRelatedMilestoneFilters_Exposed(ModuleFilterCollection filters, params ZDBOnlySubQuery[] relatedParentJoiningQueries)
			{
				AddRelatedMilestoneFilters(filters, relatedParentJoiningQueries);
			}
		}
	}
}
