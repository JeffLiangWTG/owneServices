using System.Linq;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Workflow.Business.Test
{
	class ProcessTaskToProcessHeaderLinkerTest : WorkflowTestCase
	{
		ProcessTask AssertTaskLinkedToWorkflow(IWorkflowProvider dummy, string workflowName, string taskName, int totalExpectedTasks)
		{
			var matchingTask = dummy.WorkflowItems.Tasks.FirstOrDefault(t =>
			{
				var processTask = t as ProcessTask;
				return processTask.P9_Description == taskName && processTask.ProcessHeader != null && processTask.ProcessHeader.FH_CompletionStatement == workflowName;
			}) as ProcessTask;
			AssertNotNull($"Could not find task '{taskName}' with workflow '{workflowName}'", matchingTask);
			AssertEquals($"Expected number of tasks on '{workflowName}' workflow", totalExpectedTasks, matchingTask.ProcessHeader.Tasks.Count());
			return matchingTask;
		}

		public void TestProcessTaskToProcessHeaderLinker_ReapplyTemaplate()
		{
			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, "DUM");
			Factory.Save();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var codingWorkflow = BMTestHelper.CreateWorkflow(template, "CODING");
			var templateCodingTask = BMTestHelper.CreateTask(template, codingWorkflow, description: "Coding Task");

			var reviewWorkflow = BMTestHelper.CreateWorkflow(template, "REVIEW");
			var templateReviewTask = BMTestHelper.CreateTask(template, reviewWorkflow, description: "Review Task");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();

			var dummyJobHeader = BMTestHelper.GetJobHeaderForParent(dummy, Factory, false);
			AssertEquals("Header is created from template", 2, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Task is created from template", 2, dummy.WorkflowItems.Tasks.Count);

			var parameters = TemplateApplicationParameters.ReapplyTemaplate();
			dummy.ApplyWorkflowTemplates(parameters);

			AssertTaskLinkedToWorkflow(dummy, "CODING", "Coding Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "CODING (1)", "Coding Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "REVIEW", "Review Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "REVIEW (1)", "Review Task", 1);

			Factory.Save();

			dummy.ApplyWorkflowTemplates(parameters);

			AssertTaskLinkedToWorkflow(dummy, "CODING", "Coding Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "CODING (1)", "Coding Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "CODING (2)", "Coding Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "REVIEW", "Review Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "REVIEW (1)", "Review Task", 1);
			AssertTaskLinkedToWorkflow(dummy, "REVIEW (2)", "Review Task", 1);
		}

		[TestDate(2000, 1, 1)]
		public void TestProcessTaskToProcessHeaderLinker_PartialTemplate()
		{
			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, "DUM");
			Factory.Save();

			var partialTemplate = BMTestHelper.CreateWorkflowTemplate(Factory, "DUM") as ProcessTaskTemplate;
			partialTemplate.P0_IsPartialTemplate = true;
			var partialTemplateCodingWorkflow = BMTestHelper.CreateWorkflow(partialTemplate, "CODING");
			var partialTemplateReviewWorkflow = BMTestHelper.CreateWorkflow(partialTemplate, "REVIEW");
			BMTestHelper.CreateTask(partialTemplate, partialTemplateCodingWorkflow, description: "Partial Template Coding Task");
			BMTestHelper.CreateTask(partialTemplate, partialTemplateReviewWorkflow, description: "Partial Template Review Task");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var codingWorkflow = BMTestHelper.CreateWorkflow(template, "CODING");
			var templateCodingTask = BMTestHelper.CreateTask(template, codingWorkflow, description: "Coding Task");

			var reviewWorkflow = BMTestHelper.CreateWorkflow(template, "REVIEW");
			var templateReviewTask = BMTestHelper.CreateTask(template, reviewWorkflow, description: "Review Task");

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Apply partial template";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			action.PQ_P0_WorkflowTemplate = partialTemplate.PK;
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			TestDateAttribute.AddMilliseconds(1);
			var parameters = TemplateApplicationParameters.ReapplyTemaplate();
			dummy.ApplyWorkflowTemplates(parameters);
			Factory.Save();

			AssertEquals("Header is created from template", 4, dummy.Workflows.Count);
			AssertEquals("Task is created from template", 4, dummy.WorkflowItems.Tasks.Count);

			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			TestDateAttribute.AddMilliseconds(1);
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				MasterFilesTestHelper.RunLogWalker();
			}

			dummy.WorkflowItems.Reload(true);
			AssertTaskLinkedToWorkflow(dummy, "CODING", "Coding Task", 1);

			AssertTaskLinkedToWorkflow(dummy, "CODING (1)", "Coding Task", 2);
			AssertTaskLinkedToWorkflow(dummy, "CODING (1)", "Partial Template Coding Task", 2);

			AssertTaskLinkedToWorkflow(dummy, "REVIEW", "Review Task", 1);

			AssertTaskLinkedToWorkflow(dummy, "REVIEW (1)", "Review Task", 2);
			AssertTaskLinkedToWorkflow(dummy, "REVIEW (1)", "Partial Template Review Task", 2);
		}
	}
}
