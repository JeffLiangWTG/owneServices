using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowJobModuleFilter))]
	sealed class WorkflowJobModuleFilterTest : ModuleFilterTestCase<WorkflowJobModuleFilter>
	{
		public void TestWorkflowJobFilter()
		{
			SetUpFilterData();

			#region Exact Filter Option

			workflowFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			workflowFilter.IsActive = true;
			workflowFilter.Property = ZGuid.Empty;

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionContains(task1_workflow_jobHeader, tasks);
			AssertCollectionContains(task2_workflow_jobHeader, tasks);
			AssertCollectionContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionContains(task_workflow2_jobHeader2, tasks);

			workflowFilter.Property = workflow.PK;

			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionContains(task1_workflow_jobHeader, tasks);
			AssertCollectionContains(task2_workflow_jobHeader, tasks);

			AssertCollectionNotContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionNotContains(task_workflow2_jobHeader2, tasks);

			((ModuleGuidFilter)filter["Workflow"]).Property = workflow1_jobHeader2.PK;
			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionContains(task_workflow1_jobHeader2, tasks);

			AssertCollectionNotContains(task_workflow2_jobHeader2, tasks);
			AssertCollectionNotContains(task1_workflow_jobHeader, tasks);
			AssertCollectionNotContains(task2_workflow_jobHeader, tasks);

			((ModuleGuidFilter)filter["Workflow"]).Property = workflow2_jobHeader2.PK;
			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionContains(task_workflow2_jobHeader2, tasks);

			AssertCollectionNotContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionNotContains(task1_workflow_jobHeader, tasks);
			AssertCollectionNotContains(task2_workflow_jobHeader, tasks);

			#endregion

			#region NotEqual Filter Option

			workflowFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			workflowFilter.Property = workflow.PK;
			workflowFilter.IsActive = true;

			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionNotContains(task1_workflow_jobHeader, tasks);
			AssertCollectionNotContains(task2_workflow_jobHeader, tasks);

			AssertCollectionContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionContains(task_workflow2_jobHeader2, tasks);

			((ModuleGuidFilter)filter["Workflow"]).Property = workflow1_jobHeader2.PK;
			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionNotContains(task_workflow1_jobHeader2, tasks);

			AssertCollectionContains(task_workflow2_jobHeader2, tasks);
			AssertCollectionContains(task1_workflow_jobHeader, tasks);
			AssertCollectionContains(task2_workflow_jobHeader, tasks);

			#endregion

			#region IsBlank Filter Option

			workflowFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;
			workflowFilter.IsActive = true;

			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionNotContains(task1_workflow_jobHeader, tasks);
			AssertCollectionNotContains(task2_workflow_jobHeader, tasks);
			AssertCollectionNotContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionNotContains(task_workflow2_jobHeader2, tasks);

			AssertCollectionContains(standaloneTask1, tasks);
			AssertCollectionContains(standaloneTask2, tasks);

			#endregion

			#region IsNotBlank FilterOption

			workflowFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			workflowFilter.IsActive = true;

			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionContains(task1_workflow_jobHeader, tasks);
			AssertCollectionContains(task2_workflow_jobHeader, tasks);
			AssertCollectionContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionContains(task_workflow2_jobHeader2, tasks);

			AssertCollectionNotContains(standaloneTask1, tasks);
			AssertCollectionNotContains(standaloneTask2, tasks);

			#endregion
		}

		public void TestWorkflowJobFilter_FilterMatch()
		{
			SetUpFilterData();

			var completionStatementFilter = workflowFilter.SelectedFilters.AddTextFilterStrip("Completion Statement", "Test");

			workflowFilter.IsActive = true;
			workflowFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionContains(task1_workflow_jobHeader, tasks);
			AssertCollectionContains(task2_workflow_jobHeader, tasks);
			AssertCollectionContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionContains(task_workflow2_jobHeader2, tasks);

			AssertCollectionNotContains(standaloneTask1, tasks);
			AssertCollectionNotContains(standaloneTask2, tasks);

			completionStatementFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			completionStatementFilter.Property = "Test 2";

			tasks = new ProcessTaskCollection(Factory);
			tasks.Load(filter.Filter);
			AssertCollectionContains(task_workflow1_jobHeader2, tasks);
			AssertCollectionContains(task_workflow2_jobHeader2, tasks);

			AssertCollectionNotContains(task1_workflow_jobHeader, tasks);
			AssertCollectionNotContains(task2_workflow_jobHeader, tasks);
			AssertCollectionNotContains(standaloneTask1, tasks);
			AssertCollectionNotContains(standaloneTask2, tasks);
		}

		void SetUpFilterData()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.EnableBMSInRegistry();

			filter = new ProcessTaskFilterBusinessObject();
			workflowFilter = (ModuleGuidFilter)filter["Workflow"];
			AssertNotNull(workflowFilter);

			// job 1 - one WF and 2 tasks
			var jobHeader = bmsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_CompletionStatement = "Test 1";
			workflow = bmsTestHelper.CreateWorkflow(jobHeader, "WF 1");

			task1_workflow_jobHeader = (ProcessTask)bmsTestHelper.CreateTask(workflow);
			task2_workflow_jobHeader = (ProcessTask)bmsTestHelper.CreateTask(workflow);

			//job 2 - two wfs and one task for each wf
			var jobHeader2 = bmsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_CompletionStatement = "Test 2";
			workflow1_jobHeader2 = bmsTestHelper.CreateWorkflow(jobHeader2, "WF jobHeader2");

			task_workflow1_jobHeader2 = (ProcessTask)bmsTestHelper.CreateTask(workflow1_jobHeader2);

			workflow2_jobHeader2 = bmsTestHelper.CreateWorkflow(jobHeader2, "WF 2 jobHeader2");
			task_workflow2_jobHeader2 = (ProcessTask)bmsTestHelper.CreateTask(workflow2_jobHeader2);

			standaloneTask1 = Factory.NewWithValidTestData<ProcessTask>();
			standaloneTask2 = Factory.NewWithValidTestData<ProcessTask>();

			Factory.Save();
		}
		ProcessTaskFilterBusinessObject filter;
		ModuleGuidFilter workflowFilter;
		ProcessTask task1_workflow_jobHeader;
		ProcessTask task2_workflow_jobHeader;
		ProcessTask task_workflow1_jobHeader2;
		ProcessTask task_workflow2_jobHeader2;
		ProcessTask standaloneTask1;
		ProcessTask standaloneTask2;
		IProcessHeader workflow;
		IProcessHeader workflow1_jobHeader2;
		IProcessHeader workflow2_jobHeader2;

		protected override WorkflowJobModuleFilter GetNewModuleFilter()
		{
			return new WorkflowJobModuleFilter("moo", ModuleIDs.ProcessHeader, ObjectFactory.Get<IProcessHeaderCollectionProvider>().GetCollection(Factory));
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
