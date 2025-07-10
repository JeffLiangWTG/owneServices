using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	class WorkflowReapplicationStrategyTest : WorkflowTestCase
	{
		IWorkflowProvider workItem;
		IProcessJobHeader jobHeader;
		GlbStaff staff;

		public void TestWorkflowReapplication_NewFactory()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());
			var jobHeaderInNewFactory = (IProcessJobHeader)new BusinessObjectFactory().Load("FH", jobHeader.PK);
			var workItemInNewFactory = (IWorkflowProvider)new BusinessObjectFactory().Load("Z0", workItem.PK);

			AssertEquals(0, jobHeaderInNewFactory.Tasks.Count());
			AssertEquals(0, jobHeaderInNewFactory.ProcessHeaders.Count);
			AssertEquals(0, workItemInNewFactory.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItemInNewFactory.WorkflowItems.Triggers.Count);

			var log = GetReapplicationRequestedEvent(workItemInNewFactory);

			AssertNotNull(log);
			Assert(log.SL_FireWorkflow);
		}

		public void TestReapplyWorkflowTemplate_ShouldNotRemainSuppressed()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);
			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			Assert("Creating workflow from template should not be suppressed", !ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);

			var standardTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			standardTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			const string templateSubType = "COL";
			standardTemplate.P0_SubType1 = templateSubType;

			var templateTrigger = (ITemplateTrigger)standardTemplate.WorkflowItems.Triggers.AddNew();
			var templateMilestone = (ITemplateTrigger)standardTemplate.WorkflowItems.Milestones.AddNew();
			var templateTask = (ITemplateTrigger)standardTemplate.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.SubType1 = templateSubType;
			Factory.Save();
			AssertEquals("Workflow item should not be applied to the job from Workflow Reapplication.", 0, job.WorkflowItems.Triggers.Count);
			AssertEquals("Workflow item should not be applied to the job from Workflow Reapplication.", 0, job.WorkflowItems.Milestones.Count);
			AssertEquals("Workflow item should not be applied to the job from Workflow Reapplication.", 0, job.WorkflowItems.Tasks.Count);

			AssertEquals(false, job.HasChanges);
			job.ApplyWorkflowTemplates();
			AssertEquals(true, job.HasChanges);
			Factory.Save();

			AssertEquals("Workflow items should be applied to the job when workflow templates are applied.", 1, job.WorkflowItems.Milestones.Count);
			AssertEquals("Workflow items should be applied to the job when workflow templates are applied.", 1, job.WorkflowItems.Triggers.Count);
			AssertEquals("Workflow items should be applied to the job when workflow templates are applied.", 1, job.WorkflowItems.Tasks.Count);
		}

		public void TestWorkflowReapplication_HasLog()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItem.WorkflowItems.Triggers.Count);

			var log = GetReapplicationRequestedEvent(workItem);

			AssertNotNull(log);
			Assert("Should defer firing workflow templates to the Log Walker service task to avoid performance overhead.", log.SL_FireWorkflow);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestWorkflowReapplication_ShouldRecordReferenceParametersBasedOnSelections()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			ReapplyTemplates_WithOptions_AssertingEventReference(new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: false), "|MIL=N|RG=N|TRG=N|TSK=Y", "Tasks: Y, Milestones: N, Triggers: N, Release Groups: N");
			ReapplyTemplates_WithOptions_AssertingEventReference(new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: false), "|MIL=Y|RG=N|TRG=N|TSK=N", "Tasks: N, Milestones: Y, Triggers: N, Release Groups: N");
			ReapplyTemplates_WithOptions_AssertingEventReference(new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false), "|MIL=N|RG=N|TRG=Y|TSK=N", "Tasks: N, Milestones: N, Triggers: Y, Release Groups: N");
			ReapplyTemplates_WithOptions_AssertingEventReference(new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: true), "|MIL=N|RG=Y|TRG=N|TSK=N", "Tasks: N, Milestones: N, Triggers: N, Release Groups: Y");
			ReapplyTemplates_WithOptions_AssertingEventReference(new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: true), "|MIL=Y|RG=Y|TRG=Y|TSK=Y", "Tasks: Y, Milestones: Y, Triggers: Y, Release Groups: Y");

			void ReapplyTemplates_WithOptions_AssertingEventReference(ReapplyWorkflowTemplateUserOptions options, string expectedEventReference, string expectedHumanReadableEventReference)
			{
				WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, new[] { workItem }, new DummyProgressReporter());

				var log = GetReapplicationRequestedEvent(workItem);

				AssertNotNull(log);
				AssertEquals(expectedEventReference, log.SL_Reference);
				AssertEquals(expectedHumanReadableEventReference, log.DisplayEventReference);
			}
		}

		void AssertCounts(IProcessJobHeader header, IWorkflowProvider job, int taskCount, int workflowCount, int milestoneCount, int triggerCount)
		{
			AssertEquals(taskCount, header.Tasks.Count());
			AssertEquals(workflowCount, header.ProcessHeaders.Count);
			AssertEquals(milestoneCount, job.WorkflowItems.Milestones.Count);
			AssertEquals(triggerCount, job.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_MultipleWorkflowProvidersIncludingExceptionThrowingProvider_HandlesSaveException_SavesOtherJobs()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			var badJob = Factory.New<SaveExceptionThrowingWorkflowProvider>();
			var badJobHeader = BMTestHelper.GetJobHeaderForParent(badJob, Factory, addDefaultProcessHeaderIfNone: false);
			SetupJobRelatedElements(badJobHeader, badJob, "bad boy", "Play hooky", "Eat your trans fats", "Slleeeeppp");

			var goodJobHeader = BMTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var goodJob = (DummyWithWorkflow)goodJobHeader.Parent;
			SetupJobRelatedElements(goodJobHeader, goodJob, "good girl", "Play the clarinet", "Eat your vegetables", "Slleeeeppp");

			badJob.SuspendThrowException = true;

			Factory.Save();

			badJob.SuspendThrowException = false;

			AssertCounts(jobHeader, workItem, taskCount: 3, workflowCount: 1, milestoneCount: 2, triggerCount: 2);
			AssertCounts(badJobHeader, badJob, taskCount: 3, workflowCount: 1, milestoneCount: 2, triggerCount: 2);
			AssertCounts(goodJobHeader, goodJob, taskCount: 3, workflowCount: 1, milestoneCount: 2, triggerCount: 2);

			var workflowProviders = new[] { workItem, badJob, goodJob };

			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			CombineAssertions("Only the job that caused the save exception should have been last reported", () =>
			{
				AssertContains("Part of WI000HN000 could not be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("** Error Saving Record **", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Inner Message = Couldn't save his way out of a wet paper bag", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("This job cannot have templates reapplied. Other jobs will be processed.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertCounts(jobHeader, workItem, taskCount: 0, workflowCount: 0, milestoneCount: 0, triggerCount: 0);
				AssertCounts(badJobHeader, badJob, taskCount: 3, workflowCount: 1, milestoneCount: 2, triggerCount: 2);
				AssertCounts(goodJobHeader, goodJob, taskCount: 0, workflowCount: 0, milestoneCount: 0, triggerCount: 0);
			});
		}

		public void TestWorkflowReapplication_DeletesMilestones()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_DeletesTriggers()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_DeletesTasks()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_WorkflowTasks()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_DeletesMilestones_AndWorkflows()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_DeleteAll()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_DeleteTriggersAndMilestones()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_DeleteWorkflowsAndTriggers()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_SecurityCheckpointWorks()
		{
			Env.Security.WorkflowTaskTemplatesReapply.IsAllowed = false;

			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			Env.Security.WorkflowTaskTemplatesReapply.IsAllowed = true;
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_DoNotDeleteForSomtimesProviders()
		{
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			var dummy = Factory.New<NonWorkflowTemplateWorkflowProvider>();
			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Tasks.AddNew();
			Factory.Save();

			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, new[] { dummy }, new DummyProgressReporter());
			AssertEquals("Don't apply for this type.", 2, dummy.WorkflowItems.Tasks.Count);
		}

		void AssertWorkflowReapplication_IReapplyWorkflowTemplateConfiguration(bool delayReapplyTemplatesToServiceTask, bool processAndSaveInNewFactory)
		{
			var config = GetMockConfig(delayReapplyTemplatesToServiceTask, processAndSaveInNewFactory);

			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);

			AssertEquals(3, jobHeader.Tasks.Count());
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(2, workItem.WorkflowItems.Triggers.Count);

			var workflowProviders = new[] { workItem };

			var factorySaveCount = BusinessObjectFactory.GlobalSaveCount;
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			var expectedSaves = config.ProcessAndSaveInNewFactory ? factorySaveCount + 1 : factorySaveCount;
			AssertEquals("Incorrect save count", expectedSaves, BusinessObjectFactory.GlobalSaveCount);

			var log = GetReapplicationRequestedEvent(workItem);
			if (config.DelayReapplyTemplatesToServiceTask)
			{
				AssertNotNull(log);
			}
			else
			{
				AssertNull(log);
			}

			AssertEquals(0, jobHeader.Tasks.Count());
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, workItem.WorkflowItems.Milestones.Count);
			AssertEquals(0, workItem.WorkflowItems.Triggers.Count);
		}

		public void TestWorkflowReapplication_IReapplyWorkflowTemplateConfiguration1()
		{
			AssertWorkflowReapplication_IReapplyWorkflowTemplateConfiguration(delayReapplyTemplatesToServiceTask: false, processAndSaveInNewFactory: false);
		}

		public void TestWorkflowReapplication_IReapplyWorkflowTemplateConfiguration2()
		{
			AssertWorkflowReapplication_IReapplyWorkflowTemplateConfiguration(delayReapplyTemplatesToServiceTask: true, processAndSaveInNewFactory: true);
		}

		public void TestWorkflowReapplication_IReapplyWorkflowTemplateConfiguration3()
		{
			AssertWorkflowReapplication_IReapplyWorkflowTemplateConfiguration(delayReapplyTemplatesToServiceTask: true, processAndSaveInNewFactory: false);
		}

		public void TestWorkflowReapplication_IReapplyWorkflowTemplateConfiguration4()
		{
			AssertWorkflowReapplication_IReapplyWorkflowTemplateConfiguration(delayReapplyTemplatesToServiceTask: false, processAndSaveInNewFactory: true);
		}

		public void TestWorkflowReapplication_OptionsNotSupportedInServiceTask()
		{
			var config = GetMockConfig(delayReapplyTemplatesToServiceTask: true, processAndSaveInNewFactory: false);
			var workflowProviders = new[] { workItem };
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);
			AssertExceptionThrown<NotSupportedException>("Not supported yet when delayReapplyTemplatesToServiceTask", () => WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter()));

			options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);
			AssertExceptionThrown<NotSupportedException>("Not supported yet when delayReapplyTemplatesToServiceTask", () => WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter()));
		}

		public void TestWorkflowReapplication_KeepsExistingAndReapply()
		{
			var template = BMTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow1 = BMTestHelper.CreateWorkflow(template, "Alpha");
			var workflow2 = BMTestHelper.CreateWorkflow(template, "Beta");
			var task1 = BMTestHelper.CreateTask(template, workflow1, description: "See the light");
			var task2 = BMTestHelper.CreateTask(template, workflow2, description: "It burns eyes");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			var dummyJobHeader = BMTestHelper.GetJobHeaderForParent(dummy, Factory, false);
			AssertEquals("Should have both headers created", 2, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Tasks should be added from tempalte", 2, dummy.WorkflowItems.Tasks.Count);
			Factory.Save();

			var config = GetMockConfig(delayReapplyTemplatesToServiceTask: false, processAndSaveInNewFactory: false);
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);
			var workflowProviders = new[] { dummy };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals(4, dummy.WorkflowItems.Tasks.Count);
			AssertEquals(4, dummyJobHeader.ProcessHeaders.Count);
		}

		public void TestWorkflowReapplication_DeleteUnactionedAndReapply()
		{
			var template = BMTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow1 = BMTestHelper.CreateWorkflow(template, "Alpha");
			var workflow2 = BMTestHelper.CreateWorkflow(template, "Beta");
			var workflow3 = BMTestHelper.CreateWorkflow(template, "Charlie");
			BMTestHelper.CreateTask(template, workflow1, description: "Task1");
			BMTestHelper.CreateTask(template, workflow2, description: "Task2");
			BMTestHelper.CreateTask(template, workflow1, description: "Task3");
			BMTestHelper.CreateTask(template, workflow2, description: "Task4");
			BMTestHelper.CreateTask(template, workflow3, description: "Task5");
			BMTestHelper.CreateTask(template, workflow3, description: "Task6");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			var dummyJobHeader = BMTestHelper.GetJobHeaderForParent(dummy, Factory, false);
			AssertEquals("Should have both headers created", 3, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Tasks should be added from tempalte", 6, dummy.WorkflowItems.Tasks.Count);
			Factory.Save();

			var header1 = dummyJobHeader.ProcessHeaders[0];
			var task1 = header1.Tasks.ElementAt(0);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task2 = header1.Tasks.ElementAt(1);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var header2 = dummyJobHeader.ProcessHeaders[1];
			var task3 = header2.Tasks.ElementAt(0);
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task4 = header2.Tasks.ElementAt(1);
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var header3 = dummyJobHeader.ProcessHeaders[2];
			var task5 = header3.Tasks.ElementAt(0);
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task6 = header3.Tasks.ElementAt(1);
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var config = GetMockConfig(delayReapplyTemplatesToServiceTask: false, processAndSaveInNewFactory: false);
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);
			var workflowProviders = new[] { dummy };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals("6 new tasks + 1 CLS task + 1 SUS task + 1 WRK task", 9, dummy.WorkflowItems.Tasks.Count);
			AssertEquals("3 new headers + 2 that had CLS/SUS/WRK tasks",5, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Delete this header as all its tasks are deleted", true, header1.IsDeleted);
			AssertEquals("Not deleted as it has a CLS task", false, header2.IsDeleted);
			AssertEquals("Not deleted as it WRK and SUS task", false, header3.IsDeleted);
			AssertEquals("Delete OPN tasks", true, ((BusinessObject)task1).IsDeleted);
			AssertEquals("Delete ASN tasks", true, ((BusinessObject)task2).IsDeleted);
			AssertEquals("Do not delete CLS tasks", false, ((BusinessObject)task3).IsDeleted);
			AssertEquals("Delete ASN tasks", true, ((BusinessObject)task4).IsDeleted);
			AssertEquals("Do not delete WRK tasks", false, ((BusinessObject)task5).IsDeleted);
			AssertEquals("Do not delete SUS tasks", false, ((BusinessObject)task6).IsDeleted);
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestWorkflowReapplication_ExistingWorkflowMustBeSaved()
		{
			var template = BMTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow1 = BMTestHelper.CreateWorkflow(template, "Alpha");
			var task1 = BMTestHelper.CreateTask(template, workflow1, description: "See the light");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			var dummyJobHeader = BMTestHelper.GetJobHeaderForParent(dummy, Factory, false);
			AssertEquals("Should have both headers created", 1, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Tasks should be added from tempalte", 1, dummy.WorkflowItems.Tasks.Count);
			var task = dummy.WorkflowItems.Tasks[0];

			var config = GetMockConfig(delayReapplyTemplatesToServiceTask: false, processAndSaveInNewFactory: false);
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);
			var workflowProviders = new[] { dummy };
			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, new DummyProgressReporter());

			AssertEquals("Exisiting Tasks and Workflows must be saved before reapplying", 1, dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Exisiting Tasks and Workflows must be saved before reapplying", 1, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Task shouldn't be deleted", task, dummy.WorkflowItems.Tasks[0]);

			AssertContains("Existing Workflows must be saved", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		IReapplyWorkflowTemplateConfiguration GetMockConfig(bool delayReapplyTemplatesToServiceTask, bool processAndSaveInNewFactory)
		{
			var config = new Mock<IReapplyWorkflowTemplateConfiguration>();
			config.Setup(x => x.DelayReapplyTemplatesToServiceTask).Returns(delayReapplyTemplatesToServiceTask);
			config.Setup(x => x.ProcessAndSaveInNewFactory).Returns(processAndSaveInNewFactory);
			return config.Object;
		}

		#region Implementation

		static StmALog GetReapplicationRequestedEvent(IWorkflowProvider job)
		{
			return ((BusinessObject)job).GetLogs().MostRecentLogByEventTime(AutoEvents.ReapplyWorkflowTemplatesRequested);
		}

		void SetupJobRelatedElements(IProcessJobHeader processJobHeader, IWorkflowProvider job, string completionStatement, string taskDescription1, string taskDescription2, string taskDescription3)
		{
			var wf = BMTestHelper.CreateWorkflow(processJobHeader, completionStatement);

			BMTestHelper.CreateTask(wf, staff.GS_Code, 10, sequence: 1, description: taskDescription1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDU");
			BMTestHelper.CreateTask(wf, staff.GS_Code, 10, sequence: 2, description: taskDescription2, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, taskType: "CDF");
			BMTestHelper.CreateTask(wf, staff.GS_Code, 10, sequence: 3, description: taskDescription3, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			job.WorkflowItems.Milestones.AddNew();
			job.WorkflowItems.Milestones.AddNew();
			job.WorkflowItems.Triggers.AddNew();
			job.WorkflowItems.Triggers.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, "WKI");
			BMTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: true);
			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Jon";
			staff.GS_LoginName = "Jonathon";

			jobHeader = BMTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			workItem = (DummyWithWorkflow)jobHeader.Parent;

			SetupJobRelatedElements(jobHeader, workItem, "get gud", "Play doto", "Eat onion like an apple", "Slleeeeppp");

			Factory.Save();
		}

		#endregion

		#region Test Classes

		class NonWorkflowTemplateWorkflowProvider : DummyWithWorkflow, ISometimesWorkflowProvider
		{
			public NonWorkflowTemplateWorkflowProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ShouldSupportWorkflowTemplateApplication => false;
		}

		class SaveExceptionThrowingWorkflowProvider : ExceptionThrowingWorkflowProvider
		{
			public SaveExceptionThrowingWorkflowProvider(BusinessObjectFactory factory, DataRow row)
					: base(factory, row)
			{
				this.factory = factory;
				this.row = row;
			}

			readonly BusinessObjectFactory factory;
			readonly DataRow row;

			public bool SuspendThrowException { get; set; }

			protected override void OnFactorySaving()
			{
				if (!SuspendThrowException)
				{
					throw new ZSaveException(new ZDataException(new Exception("Couldn't save his way out of a wet paper bag"), row, CargoWise.Data.Db.Connection), factory);
				}
			}
		}

		abstract class ExceptionThrowingWorkflowProvider : DummyWithWorkflow, IWorkflowProvider
		{
			public ExceptionThrowingWorkflowProvider(BusinessObjectFactory factory, DataRow row)
					: base(factory, row)
			{
				Z0_Description = "WI000HN000";
			}

			public ZGuid Identifier => Guid.Empty;
			public ZString WorkflowType => "DUM";

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			public IWorkflowInformationProvider GetWorkflowInformationProvider()
			{
				throw new NotImplementedException();
			}
		}

		public class DummyProgressReporter : IProgressReporter
		{
			public bool IsCancelled { get; set; }
			public bool IsDisposed { get; set; }
			public int ItemsProcessed { get; set; }
			public int CancelAfterItemNumber { get; set; }

			public void Report(int value)
			{
				ItemsProcessed = value;

				if (value == CancelAfterItemNumber)
				{
					IsCancelled = true;
				}
			}

			public void Dispose()
			{
				IsDisposed = true;
			}

			public void ShowForm(IComponent parentFormToShowModallyTo, string initialMessage)
			{
			}
		}

		#endregion
	}
}
