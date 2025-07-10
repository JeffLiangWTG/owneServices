using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;

namespace Enterprise.Workflow.Business.Test
{
	//JR TODO: Does this work with UI config?
	class ReleaseGroupRecalculationEndToEndTest : WorkflowTestCase
	{
		public void TestReapplyWorkflowTemplates_AndRunLogWalker_WhenTasksAndReleaseGroupsUpdatedTogether_ShouldRecalculateReleaseGroupAndTaskDescription()
		{
			var group1 = MasterFilesTestHelper.CreateGroup(Factory);
			var group2 = MasterFilesTestHelper.CreateGroup(Factory);

			group1.GG_Code = "Group1";
			group2.GG_Code = "Group2";

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var workflow = bmTestHelper.CreateWorkflow(template);
			var templateTask = bmTestHelper.CreateTask(template, workflow, description: "Task One");

			var mapping = MapJobCodeToGroup(template, "AAA", group1);

			Factory.Save();

			var job = MasterFilesTestHelper.CreateWorkflowProvider<DummyWithWorkflow>(Factory);
			job.Z0_Code = "AAA";
			job.ApplyWorkflowTemplates();

			Factory.Save();
			AssertReleaseGroupCodeAndTaskDescription(job, "Group1", "Task One");

			ReapplyWorkflowTemplates(job, true);
			AssertReleaseGroupCodeAndTaskDescription(job, "Group1", "Task One");

			mapping.PTM_GG_Group = group2.PK;
			templateTask.P9_Description = "Task Two";
			Factory.Save();

			ReapplyWorkflowTemplates(job, true);
			AssertReleaseGroupCodeAndTaskDescription(job, "Group2", "Task Two");
		}

		public void TestReapplyWorkflowTemplates_AndRunLogWalker_WhenReleaseGroupsFlaggedAlone_ShouldRecalculateReleaseGroupOnly()
		{
			var group1 = MasterFilesTestHelper.CreateGroup(Factory);
			var group2 = MasterFilesTestHelper.CreateGroup(Factory);

			group1.GG_Code = "Group1";
			group2.GG_Code = "Group2";

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var workflow = bmTestHelper.CreateWorkflow(template);
			var templateTask = bmTestHelper.CreateTask(template, workflow, description: "Task One");

			var mapping = MapJobCodeToGroup(template, "AAA", group1);

			Factory.Save();

			var job = MasterFilesTestHelper.CreateWorkflowProvider<DummyWithWorkflow>(Factory);
			job.Z0_Code = "AAA";
			job.ApplyWorkflowTemplates();

			Factory.Save();
			AssertReleaseGroupCodeAndTaskDescription(job, "Group1", "Task One");

			ReapplyWorkflowTemplates(job, false);
			AssertReleaseGroupCodeAndTaskDescription(job, "Group1", "Task One");

			mapping.PTM_GG_Group = group2.PK;
			templateTask.P9_Description = "Task Two";
			Factory.Save();

			ReapplyWorkflowTemplates(job, false);
			AssertReleaseGroupCodeAndTaskDescription(job, "Group2", "Task One");
		}

		static IProcessTemplateReleaseGroupRuleMapping MapJobCodeToGroup(ProcessTaskTemplate template, string jobCode, GlbGroup group)
		{
			var rule = (IProcessTemplateReleaseGroupRule)template.ReleaseGroupRules.AddNew();
			var mapping = (IProcessTemplateReleaseGroupRuleMapping)rule.GroupMappings.AddNew();

			rule.PTR_ValueSelectionMacro = "<Z0_Code>";
			mapping.PTM_Value = jobCode;
			mapping.PTM_GG_Group = group.PK;

			return mapping;
		}

		static void ReapplyWorkflowTemplates(IWorkflowProvider job, bool reapplyTasks)
		{
			var reapplyTasksOption = reapplyTasks ? ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply : ReapplyWorkflowAndTasksOptionsList.Codes.Exclude;
			var config = new ReapplyWorkflowTemplateInServiceTaskConfiguration();
			var options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: reapplyTasksOption, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.Exclude, reapplyTriggers: ReapplyTriggersOptionsList.Codes.Exclude, releaseGroups: true);

			WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, new[] { job }, new Mock<IProgressReporter>().Object);
			MasterFilesTestHelper.RunLogWalker();
		}

		void AssertReleaseGroupCodeAndTaskDescription(IWorkflowProvider job, string expectedReleaseGroupCode, string expectedTaskDescription)
		{
			var newFactory = ((IBusiness)job).Factory.CreateNewFactory();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(job, newFactory);

			AssertNotNull(jobHeader);
			AssertNotNull(jobHeader.ReleaseGroup);
			AssertEquals(expectedReleaseGroupCode, jobHeader.ReleaseGroup.GG_Code);

			var task = jobHeader.Tasks.Single();

			AssertEquals(expectedTaskDescription, task.P9_Description);
		}

		static string WorkflowType => DummyWorkflowDescriptor.Instance.Code;

		protected override void SetUp()
		{
			base.SetUp();

			bmTestHelper = ObjectFactory.Get<IBMTestHelper>();

			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, WorkflowType);
		}

		IBMTestHelper bmTestHelper;
	}
}
