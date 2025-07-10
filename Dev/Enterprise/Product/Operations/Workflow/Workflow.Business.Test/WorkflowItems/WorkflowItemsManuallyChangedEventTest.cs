using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Workflow.Business.Test.WorkflowReapplicationStrategyTest;

namespace Enterprise.Workflow.Business.Test
{
	class WorkflowItemsManuallyChangedEventTest : WorkflowTestCase
	{
		public void TestWorkflowItemDeleteHasLog()
		{
			var standardTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			standardTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			standardTemplate.WorkflowItems.Triggers.AddNew();
			standardTemplate.WorkflowItems.Milestones.AddNew();
			standardTemplate.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.ApplyWorkflowTemplates();

			Factory.Save();

			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded));

			AssertEquals(1, workflowJob.WorkflowItems.Milestones.Count);
			AssertEquals(1, workflowJob.WorkflowItems.Triggers.Count);
			AssertEquals(1, workflowJob.WorkflowItems.Tasks.Count);

			workflowJob.WorkflowItems.Tasks.First().Delete();
			workflowJob.WorkflowItems.Triggers.First().Delete();
			workflowJob.WorkflowItems.Milestones.First().Delete();

			Factory.Save();

			var log = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsDeleted);
			AssertNotNull(log);
			AssertEquals("|TSK=1|MIL=1|TRG=1", log.SL_Reference);
		}

		public void TestWorkflowItemAddHasLog()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.WorkflowItems.Milestones.AddNew();
			workflowJob.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			var log = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded);
			AssertNotNull(log);
			AssertEquals("|TSK=1|MIL=1|TRG=1", log.SL_Reference);
		}

		public void TestWorkflowRegenerationDoesNotTriggerLog()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			var standardTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			standardTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			standardTemplate.WorkflowItems.Triggers.AddNew();
			standardTemplate.WorkflowItems.Milestones.AddNew();
			standardTemplate.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.ApplyWorkflowTemplates();

			Factory.Save();

			AssertEquals(1, workflowJob.WorkflowItems.Milestones.Count);
			AssertEquals(1, workflowJob.WorkflowItems.Triggers.Count);
			AssertEquals(1, workflowJob.WorkflowItems.Tasks.Count);

			var workflowProviders = new[] { workflowJob };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, workflowJob.WorkflowItems.Milestones.Count);
			AssertEquals(0, workflowJob.WorkflowItems.Triggers.Count);
			AssertEquals(0, workflowJob.WorkflowItems.Tasks.Count);

			Factory.Save();

			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded));
			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsDeleted));
		}

		public void TestRegistryItemToggle()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowManualChangeEvent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.WorkflowItems.Milestones.AddNew();
			workflowJob.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			workflowJob.WorkflowItems.Tasks.First().Delete();
			workflowJob.WorkflowItems.Triggers.First().Delete();
			workflowJob.WorkflowItems.Milestones.First().Delete();

			Factory.Save();

			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded));
			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsDeleted));
		}

		public void TestHasChangesSetToFalse()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();

			var task = workflowJob.WorkflowItems.Tasks.AddNew();
			task.Delete();

			AssertEquals(0, workflowJob.Logs.GetAllLogs().Count);
			Assert(!workflowJob.Logs.GetAllLogs().HasChanges);
			Assert(!workflowJob.HasChanges);
		}

		public void TestFactorySaveResetsLogCount()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.WorkflowItems.Milestones.AddNew();
			workflowJob.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			var log = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded);
			AssertNotNull(log);
			AssertEquals("|TSK=1|MIL=1|TRG=1", log.SL_Reference);

			Factory.Save();

			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.WorkflowItems.Milestones.AddNew();
			workflowJob.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			var log2 = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded);
			AssertNotNull(log2);
			AssertNotEquals(log.PK, log2.PK);
			AssertEquals("|TSK=1|MIL=1|TRG=1", log2.SL_Reference);
		}

		public void TestServiceMultipleBizosInSameFactory()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.WorkflowItems.Milestones.AddNew();
			workflowJob.WorkflowItems.Triggers.AddNew();

			var workflowJob2 = Factory.New<DummyWithWorkflow>();
			workflowJob2.WorkflowItems.Tasks.AddNew();
			workflowJob2.WorkflowItems.Milestones.AddNew();
			workflowJob2.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			var log = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded);
			AssertNotNull(log);
			AssertEquals("|TSK=1|MIL=1|TRG=1", log.SL_Reference);

			var log2 = workflowJob2.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded);
			AssertNotNull(log2);
			AssertEquals("|TSK=1|MIL=1|TRG=1", log2.SL_Reference);

			Factory.Save();

			workflowJob.WorkflowItems.Tasks.First().Delete();
			workflowJob2.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			log = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsDeleted);
			AssertNotNull(log);
			AssertEquals("|TSK=1", log.SL_Reference);
			log2 = workflowJob2.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded);
			AssertNotNull(log2);
			AssertEquals("|TSK=1", log2.SL_Reference);
		}

		public void TestServiceIgnoresAddingDeletingExceptions()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Exceptions.AddNew();
			Factory.Save();
			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded));

			workflowJob.WorkflowItems.Exceptions.First().Delete();
			Factory.Save();
			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsDeleted));
		}

		public void TestAddingAndDeletingProcessTaskShouldNotResultInLog()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.WorkflowItems.Tasks.First().Delete();
			Factory.Save();

			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsDeleted));
			AssertNull(workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded));
		}

		public void TestDeletingExistingAndUnsavedProcessTasks()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Tasks.AddNew();
			Factory.Save();
			var log = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsManuallyAdded);
			AssertEquals("|TSK=1", log.SL_Reference);

			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.WorkflowItems.Tasks.DeleteAll();
			Factory.Save();

			log = workflowJob.GetLogs().MostRecentLogByEventTime(AutoEvents.WorkflowItemsDeleted);
			AssertEquals("|TSK=1", log.SL_Reference);
		}

		public void TestUniversalTriggerDoesNotCreateWTMOnSave()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
				AssertNull(job.Logs.GetAllLogs().FirstOrDefault(l => (l as StmALog).SL_SE_NKEvent == AutoEvents.WorkflowItemsManuallyAddedCode));

				job.Z0_Description = "test";
				Factory.Save();

				AssertNull(job.Logs.GetAllLogs().FirstOrDefault(l => (l as StmALog).SL_SE_NKEvent == AutoEvents.WorkflowItemsManuallyAddedCode));
			});
		}

		public void TestDeleteParent()
		{
			var workflowJob = Factory.New<DummyWithWorkflow>();
			workflowJob.WorkflowItems.Tasks.AddNew();
			workflowJob.Delete();

			AssertNoExceptionThrown(Factory.Save);
		}
	}
}
