using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowProcessTaskBaseCollectionViewTest : TestCaseWithFactory
	{
		public void TestSortDoesNotTriggerRebuild()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var collection = dummy.WorkflowItems.Tasks;
			var task1 = collection.AddNew();
			var task2 = collection.AddNew();

			task1.P9_Description = "2";
			task2.P9_Description = "1";

			var rebuildCount = 0;
			collection.OnRebuild += (s, e) => rebuildCount++;
			collection.Sort(nameof(task1.P9_Description));
			AssertEquals(0, rebuildCount);

			dummy.WorkflowItems.Sort(nameof(task1.P9_Description));
			AssertEquals(0, rebuildCount);
		}

		public void TestAddNewDoesNotTriggerRebuild()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var collection = dummy.WorkflowItems.Tasks;
			var rebuildCount = 0;
			collection.OnRebuild += (s, e) => rebuildCount++;
			var task1 = collection.AddNew();
			AssertEquals(0, rebuildCount);
		}

		public void TestApplyTemplate_WithNonTasks_ShouldNotSetWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = helper.CreateSystem(Factory, "DUM");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			var templateTrigger = template.WorkflowItems.Triggers.AddNew();

			var job = Factory.New<DummyWithWorkflow>();
			job.WorkflowItems.Tasks.CreateItemsFromTemplate_ForTest(template);
			job.WorkflowItems.Milestones.CreateItemsFromTemplate_ForTest(template);
			job.WorkflowItems.Triggers.CreateItemsFromTemplate_ForTest(template);

			var task = (ProcessTask)job.WorkflowItems.Tasks.Single();
			var milestone = (ProcessTask)job.WorkflowItems.Milestones.Single();
			var trigger = (ProcessTask)job.WorkflowItems.Triggers.Single();

			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			AssertNotNull(jobHeader);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);

			var workflow = jobHeader.ProcessHeaders[0];
			AssertEquals(workflow.PK, task.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, milestone.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, trigger.P9_FH_ProcessHeader);
		}

		public void TestCreateItemsFromTemplateNullErrorReporting()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "";

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			var job = Factory.New<Freight.Integration.Agency.IAgencyShipment>();
			var jobWithWorkflow = (IWorkflowProvider)job;

			jobWithWorkflow.WorkflowItems.Tasks.CreateItemsFromTemplate_ForTest(new[] { new TemplateItemApplication(template, new[] { templateTask }) });

			AssertEquals(ErrorReporter.LastMessageReported, $"Workflow Descriptor cannot be null. [WorkflowDescriptorCore: {templateTask.WorkflowDescriptorCore}, WorkflowType: {templateTask.WorkflowType}, TemplateName: {template.P0_Name}, TemplateType: {template.P0_ProcessType}]");

			ErrorReporter.Clear();
		}
	}
}
