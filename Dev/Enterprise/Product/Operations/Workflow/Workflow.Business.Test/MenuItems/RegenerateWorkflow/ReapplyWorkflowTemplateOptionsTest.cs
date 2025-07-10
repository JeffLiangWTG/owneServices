using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ReapplyWorkflowTemplateUserOptions))]
	class ReapplyWorkflowTemplateUserOptionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var config = GetMockConfig(delayReapplyTemplatesToServiceTask: true);
			return new ReapplyWorkflowTemplateUserOptions(config);
		}

		IReapplyWorkflowTemplateConfiguration GetMockConfig(bool delayReapplyTemplatesToServiceTask, bool processAndSaveInNewFactory = false)
		{
			var config = new Mock<IReapplyWorkflowTemplateConfiguration>();
			config.Setup(x => x.DelayReapplyTemplatesToServiceTask).Returns(delayReapplyTemplatesToServiceTask);
			config.Setup(x => x.ProcessAndSaveInNewFactory).Returns(processAndSaveInNewFactory);
			return config.Object;
		}

		public void TestReapplyWorkflowAndTasksOptionsDefaultValues()
		{
			var config = GetMockConfig(delayReapplyTemplatesToServiceTask: true);
			var options = new ReapplyWorkflowTemplateUserOptions(config);
			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, options.ReapplyWorkflowAndTasksOptions);

			config = GetMockConfig(delayReapplyTemplatesToServiceTask: false);
			options = new ReapplyWorkflowTemplateUserOptions(config);
			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply, options.ReapplyWorkflowAndTasksOptions);

			config = GetMockConfig(delayReapplyTemplatesToServiceTask: false);
			options = new ReapplyWorkflowTemplateUserOptions(config, reapplyWorkflowAndTasksOptions: ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, reapplyMilestones: ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, reapplyTriggers: ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, releaseGroups: false);
			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, options.ReapplyWorkflowAndTasksOptions);
		}

		public void TestReapplyWorkflowAndTasksOptionsLookup_RemoveUnsuportedOption()
		{
			var config = GetMockConfig(delayReapplyTemplatesToServiceTask: true);
			var options = new ReapplyWorkflowTemplateUserOptions(config);
			AssertEquals(false, options.ReapplyWorkflowAndTasksOptionsLookup.ContainsCode(ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply));
			AssertEquals(false, options.ReapplyWorkflowAndTasksOptionsLookup.ContainsCode(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply));

			config = GetMockConfig(delayReapplyTemplatesToServiceTask: false);
			options = new ReapplyWorkflowTemplateUserOptions(config);
			AssertEquals(true, options.ReapplyWorkflowAndTasksOptionsLookup.ContainsCode(ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply));
			AssertEquals(true, options.ReapplyWorkflowAndTasksOptionsLookup.ContainsCode(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply));
		}
	}
}
