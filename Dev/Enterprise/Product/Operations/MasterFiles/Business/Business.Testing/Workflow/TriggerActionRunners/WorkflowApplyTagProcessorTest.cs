using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowApplyTagProcessorTest : TestCaseWithFactory
	{
		public void TestWorkflowApplyTag_Constructor()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var triggerAction = trigger.ProcessTaskNotifications.AddNew();

			AssertExceptionThrown<ArgumentNullException>(() => _ = new WorkflowApplyTagProcessor(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new WorkflowApplyTagProcessor(triggerAction, null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new WorkflowApplyTagProcessor(null, dummy));
			AssertNoExceptionThrown(() => _ = new WorkflowApplyTagProcessor(triggerAction, dummy));
		}

		public void TestWorkflowApplyTag_NoMatchingWorkflowsFound()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Awesome Group");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "BST");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag, Events.CustomisableEvent00Code, "<Workflows.Where(\"<FH_Category>\" == \"ZZZ\")");

			Factory.Save();

			Assert("TG1 is not present in job workflow", !workflow1.IsTagApplied(tag));
			Assert("TG2 is not present in job workflow", !workflow2.IsTagApplied(tag));

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("No matching workflows found.", logs);
		}

		public void TestWorkflowApplyTag_NonExclusiveTag_JobWorkflow()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Awesome Group", isExclusive: false);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG2");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag1, Events.CustomisableEvent00Code);
			_ = CreateTriggerWithApplyTagAction(dummy, tag2, Events.CustomisableEvent01Code);

			Factory.Save();

			Assert("TG1 is not present in job workflow", !jobHeader.IsTagApplied(tag1));
			Assert("TG2 is not present in job workflow", !jobHeader.IsTagApplied(tag2));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is applied to job workflow", jobHeader.IsTagApplied(tag1));

			FireEvent(dummy, Events.CustomisableEvent01);
			logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG2' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG2 is applied to job workflow", jobHeader.IsTagApplied(tag2));

			FireEvent(dummy, Events.CustomisableEvent00);
			logs = MasterFilesTestHelper.RunLogWalker();

			AssertNotContains("Tag: 'TG1' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			AssertTagAppliedOnlyOnce(dummy, tag1);

			FireEvent(dummy, Events.CustomisableEvent01);
			logs = MasterFilesTestHelper.RunLogWalker();

			AssertNotContains("Tag: 'TG2' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			AssertTagAppliedOnlyOnce(dummy, tag1);
		}

		public void TestWorkflowApplyTag_NonExclusiveTag_MatchingWorkflows()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Awesome Group", isExclusive: false);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG2");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Workflow1";

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag1, Events.CustomisableEvent00Code, "Workflows.Where(\"<FH_CompletionStatement>\"==\"Workflow1\")");
			_ = CreateTriggerWithApplyTagAction(dummy, tag2, Events.CustomisableEvent01Code, "Workflows.Where(\"<FH_CompletionStatement>\"==\"Workflow2\")");

			Factory.Save();

			Assert("TG1 is not present in workflow1", !workflow1.IsTagApplied(tag1));
			Assert("TG2 is not present in workflow2", !workflow2.IsTagApplied(tag2));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Workflow1' successfully.", logs);
			Assert("TG1 is applied to workflow1", workflow1.IsTagApplied(tag1));

			FireEvent(dummy, Events.CustomisableEvent01);
			logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG2' applied to Workflow: 'Workflow2' successfully.", logs);
			Assert("TG2 is applied to workflow2", workflow2.IsTagApplied(tag2));

			FireEvent(dummy, Events.CustomisableEvent00);
			logs = MasterFilesTestHelper.RunLogWalker();

			AssertNotContains("Tag: 'TG1' applied to Workflow: 'Workflow1' successfully.", logs);

			FireEvent(dummy, Events.CustomisableEvent01);
			logs = MasterFilesTestHelper.RunLogWalker();

			AssertNotContains("Tag: 'TG2' applied to Workflow: 'Workflow2' successfully.", logs);
		}

		public void TestWorkflowApplyTag_NonExclusiveTag_MultipleMatchingWorkflows()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Awesome Group", isExclusive: false);
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Workflow1";
			workflow1.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";
			workflow2.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag, Events.CustomisableEvent00Code, "Workflows");

			Factory.Save();

			Assert("TG1 is not present in workflow1", !workflow1.IsTagApplied(tag));
			Assert("TG1 is not present in workflow2", !workflow2.IsTagApplied(tag));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Workflow1' successfully.", logs);
			AssertContains("Tag: 'TG1' applied to Workflow: 'Workflow2' successfully.", logs);
			Assert("TG1 is applied to workflow1", workflow1.IsTagApplied(tag));
			Assert("TG1 is applied to workflow2", workflow2.IsTagApplied(tag));
		}

		public void TestWorkflowApplyTag_NonExclusiveTag_MultipleMatchingWorkflows_WithWhereClause()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Awesome Group", isExclusive: false);
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Workflow1";
			workflow1.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";
			workflow2.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag, Events.CustomisableEvent00Code, "Workflows.Where(\"<FH_Category>\"==\"LSR\")");

			Factory.Save();

			Assert("TG1 is not present in workflow1", !workflow1.IsTagApplied(tag));
			Assert("TG1 is not present in workflow2", !workflow2.IsTagApplied(tag));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Workflow1' successfully.", logs);
			AssertContains("Tag: 'TG1' applied to Workflow: 'Workflow2' successfully.", logs);
			Assert("TG1 is applied to workflow1", workflow1.IsTagApplied(tag));
			Assert("TG1 is applied to workflow2", workflow2.IsTagApplied(tag));
		}

		public void TestWorkflowApplyTag_NonExclusiveTag_RUL_UsageScope()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Awesome Group", isExclusive: false, usageScope: TagUsageScopeList.Codes.Rule);
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Workflow1";
			workflow1.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";
			workflow2.FH_Category = BMConstants.LastSuccessfulReleaseNoteType;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag, Events.CustomisableEvent00Code, "Workflows");

			Factory.Save();

			Assert("TG1 is not present in workflow1", !workflow1.IsTagApplied(tag));
			Assert("TG1 is not present in workflow2", !workflow2.IsTagApplied(tag));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Workflow1' successfully.", logs);
			AssertContains("Tag: 'TG1' applied to Workflow: 'Workflow2' successfully.", logs);
			Assert("TG1 is applied to workflow1", workflow1.IsTagApplied(tag));
			Assert("TG1 is applied to workflow2", workflow2.IsTagApplied(tag));
		}

		public void TestWorkflowApplyTag_ExclusiveTag_JobWorkflow_NoExistingTagFromSameTagGroup()
		{
			var tagGroup1 = BMSTestHelper.CreateTagDefinition(Factory, "AW1", "Awesome Group1", isExclusive: true);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup1, "TG1");
			var tagGroup2 = BMSTestHelper.CreateTagDefinition(Factory, "AW2", "Awesome Group2", isExclusive: true);
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup2, "TG2");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag1, Events.CustomisableEvent00Code);
			_ = CreateTriggerWithApplyTagAction(dummy, tag2, Events.CustomisableEvent01Code);

			Factory.Save();

			Assert("TG1 is not present in job workflow", !jobHeader.IsTagApplied(tag1));
			Assert("TG2 is not present in job workflow", !jobHeader.IsTagApplied(tag2));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is applied to job workflow", jobHeader.IsTagApplied(tag1));

			FireEvent(dummy, Events.CustomisableEvent01);
			logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG2' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG2 is applied to job workflow", jobHeader.IsTagApplied(tag2));
		}

		public void TestWorkflowApplyTag_ExclusiveTag_JobWorkflow_WithMoreExclusiveExistingTagFromSameTagGroup()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AW1", "Awesome Group1", isExclusive: true);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");
			tag1.TGM_RuleRunSequence = 10;
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG2");
			tag2.TGM_RuleRunSequence = 20;

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag1, Events.CustomisableEvent00Code);
			_ = CreateTriggerWithApplyTagAction(dummy, tag2, Events.CustomisableEvent01Code);

			Factory.Save();

			Assert("TG1 is not present in job workflow", !jobHeader.IsTagApplied(tag1));
			Assert("TG2 is not present in job workflow", !jobHeader.IsTagApplied(tag2));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is applied to job workflow", jobHeader.IsTagApplied(tag1));

			FireEvent(dummy, Events.CustomisableEvent01);
			logs = MasterFilesTestHelper.RunLogWalker();

			var factory = new BusinessObjectFactory();
			var jobHeaderInNewFactory = ProcessJobHeaderProvider.GetForParent(dummy, factory, false);

			AssertContains("Tag: 'TG2' applied, Tag: 'TG1' removed from Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is removed from job workflow", !jobHeaderInNewFactory.IsTagApplied(tag1));
			Assert("TG2 is applied to job workflow", jobHeaderInNewFactory.IsTagApplied(tag2));
		}

		public void TestWorkflowApplyTag_ExclusiveTag_JobWorkflow_WithLessExclusiveExistingTagFromSameTagGroup()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AW1", "Awesome Group1", isExclusive: true);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");
			tag1.TGM_RuleRunSequence = 20;
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG2");
			tag2.TGM_RuleRunSequence = 10;

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag1, Events.CustomisableEvent00Code);
			_ = CreateTriggerWithApplyTagAction(dummy, tag2, Events.CustomisableEvent01Code);

			Factory.Save();

			Assert("TG1 is not present in job workflow", !jobHeader.IsTagApplied(tag1));
			Assert("TG2 is not present in job workflow", !jobHeader.IsTagApplied(tag2));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is applied to job workflow", jobHeader.IsTagApplied(tag1));

			FireEvent(dummy, Events.CustomisableEvent01);
			logs = MasterFilesTestHelper.RunLogWalker();

			var factory = new BusinessObjectFactory();
			var jobHeaderInNewFactory = ProcessJobHeaderProvider.GetForParent(dummy, factory, false);

			AssertContains("Tag: 'TG2' applied, Tag: 'TG1' removed from Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is not applied to job workflow anymore", !jobHeaderInNewFactory.IsTagApplied(tag1));
			Assert("TG2 is applied to job workflow", jobHeaderInNewFactory.IsTagApplied(tag2));
		}

		public void TestWorkflowApplyTag_MultipleExclusiveTags_JobWorkflow_AllExistingTagsAreLessExclusive()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AW1", "Awesome Group1", isExclusive: true);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");
			tag1.TGM_RuleRunSequence = 10;
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG2");
			tag2.TGM_RuleRunSequence = 100;
			var tag3 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG3");
			tag3.TGM_RuleRunSequence = 1;

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;

			jobHeader.AddTag(tag1);
			jobHeader.AddTag(tag2);

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag3, Events.CustomisableEvent00Code);

			Factory.Save();

			Assert("TG1 is present in job workflow", jobHeader.IsTagApplied(tag1));
			Assert("TG2 is present in job workflow", jobHeader.IsTagApplied(tag2));
			Assert("TG3 is not present in job workflow", !jobHeader.IsTagApplied(tag3));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			var factory = new BusinessObjectFactory();
			var jobHeaderInNewFactory = ProcessJobHeaderProvider.GetForParent(dummy, factory, false);

			AssertContains("Tag: 'TG3' applied, Tag: 'TG1' removed from Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is not applied to job workflow anymore", !jobHeaderInNewFactory.IsTagApplied(tag1));
			Assert("TG2 is not applied to job workflow anymore", !jobHeaderInNewFactory.IsTagApplied(tag2));
			Assert("TG3 is applied to job workflow", jobHeaderInNewFactory.IsTagApplied(tag3));
		}

		public void TestWorkflowApplyTag_MultipleExclusiveTags_JobWorkflow_AllExistingTagsAreMoreExclusive()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AW1", "Awesome Group1", isExclusive: true);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");
			tag1.TGM_RuleRunSequence = 1;
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG2");
			tag2.TGM_RuleRunSequence = 10;
			var tag3 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG3");
			tag3.TGM_RuleRunSequence = 100;

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;

			jobHeader.AddTag(tag1);
			jobHeader.AddTag(tag2);

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag3, Events.CustomisableEvent00Code);

			Factory.Save();

			Assert("TG1 is present in job workflow", jobHeader.IsTagApplied(tag1));
			Assert("TG2 is present in job workflow", jobHeader.IsTagApplied(tag2));
			Assert("TG3 is not present in job workflow", !jobHeader.IsTagApplied(tag3));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			var factory = new BusinessObjectFactory();
			var jobHeaderInNewFactory = ProcessJobHeaderProvider.GetForParent(dummy, factory, false);

			AssertContains("Tag: 'TG3' applied, Tag: 'TG1' removed from Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is not applied to job workflow anymore", !jobHeaderInNewFactory.IsTagApplied(tag1));
			Assert("TG2 is not applied to job workflow anymore", !jobHeaderInNewFactory.IsTagApplied(tag2));
			Assert("TG3 is applied to job workflow", jobHeaderInNewFactory.IsTagApplied(tag3));
		}

		public void TestWorkflowApplyTag_MultipleExclusiveTags_JobWorkflow_AtleastOneExistingTagIsMoreExclusive()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AW1", "Awesome Group1", isExclusive: true);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");
			tag1.TGM_RuleRunSequence = 1;
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG2");
			tag2.TGM_RuleRunSequence = 100;
			var tag3 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG3");
			tag3.TGM_RuleRunSequence = 10;

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false);
			jobHeader.FH_ParentId = dummy.PK;

			jobHeader.AddTag(tag1);
			jobHeader.AddTag(tag2);

			Factory.Save();

			_ = CreateTriggerWithApplyTagAction(dummy, tag3, Events.CustomisableEvent00Code);

			Factory.Save();

			Assert("TG1 is present in job workflow", jobHeader.IsTagApplied(tag1));
			Assert("TG2 is present in job workflow", jobHeader.IsTagApplied(tag2));
			Assert("TG3 is not present in job workflow", !jobHeader.IsTagApplied(tag3));

			FireEvent(dummy, Events.CustomisableEvent00);
			var logs = MasterFilesTestHelper.RunLogWalker();

			var factory = new BusinessObjectFactory();
			var jobHeaderInNewFactory = ProcessJobHeaderProvider.GetForParent(dummy, factory, false);

			AssertContains("Tag: 'TG3' applied, Tag: 'TG1' removed from Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is not applied to job workflow anymore", !jobHeaderInNewFactory.IsTagApplied(tag1));
			Assert("TG2 is not applied to job workflow anymore", !jobHeaderInNewFactory.IsTagApplied(tag2));
			Assert("TG3 is applied to job workflow", jobHeaderInNewFactory.IsTagApplied(tag3));
		}

		#region Edge Cases

		public void TestWorkflowApplyTag_TaskLineTrigger()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Awesome Group", isExclusive: false);
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TG1");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.GetJobHeaderForParent(dummy, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			CreateTriggerWithApplyTagAction(dummy, tag1, Events.CustomisableEvent69Code, lineTriggerType: ProcessTasksLookups.TaskLineTriggerCode);

			Factory.Save();

			Assert("TG1 is not present in job workflow", !jobHeader.IsTagApplied(tag1));

			FireEvent(task, Events.CustomisableEvent69);
			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Tag: 'TG1' applied to Workflow: 'Job Dummy Business Object Default is complete' successfully.", logs);
			Assert("TG1 is applied to job workflow", jobHeader.IsTagApplied(tag1));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			Factory.Save();
		}

		IBMTestHelper BMSTestHelper
		{
			get { return ObjectFactory.Get<IBMTestHelper>(); }
		}

		static ProcessTask CreateTriggerWithApplyTagAction(IWorkflowProvider parent, ITagMagnitude tag1, string eventCode, string fieldName = "", string lineTriggerType = "")
		{
			var trigger1 = parent.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger";
			trigger1.TriggerConditions.TriggerEventCode = eventCode;
			trigger1.P9_LineTriggerType = lineTriggerType;

			var triggerAction1 = trigger1.ProcessTaskNotifications.AddNew();
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction1.PQ_FieldName = fieldName;
			triggerAction1.PQ_RelatedEntityId = tag1.PK;

			return trigger1;
		}

		StmALog FireEvent(BusinessObject parent, Event workflowEvent)
		{
			var triggeringEvent = parent.GetLogs().AddNew(workflowEvent);
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_GS_NKUser = "FRO";
			}

			Factory.Save();

			return triggeringEvent;
		}

		void AssertTagAppliedOnlyOnce(IWorkflowProvider parent, ITagMagnitude tag)
		{
			var factory = new BusinessObjectFactory();
			var jobHeaderInNewFactory = ProcessJobHeaderProvider.GetForParent(parent, factory, false);

			var appliedTagLinks = jobHeaderInNewFactory.TagLinks.Cast<TagLink>().Where((TagLink l) => l.TGL_TGM_Magnitude == tag.PK);
			if (!appliedTagLinks.Any())
			{
				Fail($"{tag.TGM_Code} is not applied");
			}

			Assert($"{tag.TGM_Code} is applied more than once", appliedTagLinks.Count() == 1);
		}

		#endregion
	}
}
