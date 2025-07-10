using System.Windows.Forms;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	[TestedType(typeof(ReapplyWorkflowTemplatesConfigurationForm))]
	class ReapplyWorkflowTemplateConfigurationFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				AssertEquals("Reapply Workflow Templates", form.FormCaption);
			}
		}

		public void TestApplyButton_WhenNoOptionsSelected_ShouldShowError()
		{
			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				viewModel.ReapplyWorkflowAndTasksOptions = ReapplyWorkflowAndTasksOptionsList.Codes.Exclude;
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				form.ReapplyButton.PerformClick();

				AssertEquals("Please select at least one option.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, viewModel.ShouldPerformReapplication);
				AssertEquals(false, form.IsDisposed);

				form.milestonesOptionsDropEdit.SelectItem(ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply);
				form.milestonesOptionsDropEdit.CommitBoundValue();
				form.ReapplyButton.PerformClick();

				AssertEquals(true, viewModel.ShouldPerformReapplication);
				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestApplyButton_WhenValidationError_ShouldShowError()
		{
			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				viewModel.ReapplyWorkflowAndTasksOptions = "SOMETHING INVALID";
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				form.ReapplyButton.PerformClick();

				AssertEquals("Please fix validation errors.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, viewModel.ShouldPerformReapplication);
				AssertEquals(false, form.IsDisposed);

				viewModel.ReapplyWorkflowAndTasksOptions = ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply;
				form.ReapplyButton.PerformClick();

				AssertEquals(true, viewModel.ShouldPerformReapplication);
				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestApplyButton_ShouldCopyValuesToViewModel()
		{
			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.Exclude, viewModel.ReapplyWorkflowAndTasksOptions);
			AssertEquals(ReapplyMilestonesOptionsList.Codes.Exclude, viewModel.ReapplyMilestonesOptions);
			AssertEquals(ReapplyTriggersOptionsList.Codes.Exclude, viewModel.ReapplyTriggersOptions);
			AssertEquals(false, viewModel.ReCalculateReleaseGroups);

			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				form.Show();
				form.tasksAndWorkflowsOptionsDropEdit.SelectItem(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply);
				form.tasksAndWorkflowsOptionsDropEdit.CommitBoundValue();
				form.ReapplyButton.PerformClick();
			}

			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyWorkflowAndTasksOptions);
			AssertEquals(ReapplyMilestonesOptionsList.Codes.Exclude, viewModel.ReapplyMilestonesOptions);
			AssertEquals(ReapplyTriggersOptionsList.Codes.Exclude, viewModel.ReapplyTriggersOptions);
			AssertEquals(false, viewModel.ReCalculateReleaseGroups);

			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				form.Show();
				form.milestonesOptionsDropEdit.SelectItem(ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply);
				form.milestonesOptionsDropEdit.CommitBoundValue();
				form.ReapplyButton.PerformClick();
			}

			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyWorkflowAndTasksOptions);
			AssertEquals(ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyMilestonesOptions);
			AssertEquals(ReapplyTriggersOptionsList.Codes.Exclude, viewModel.ReapplyTriggersOptions);
			AssertEquals(false, viewModel.ReCalculateReleaseGroups);

			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				form.Show();
				form.triggersOptionsDropEdit.SelectItem(ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply);
				form.triggersOptionsDropEdit.CommitBoundValue();
				form.ReapplyButton.PerformClick();
			}

			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyWorkflowAndTasksOptions);
			AssertEquals(ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyMilestonesOptions);
			AssertEquals(ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyTriggersOptions);
			AssertEquals(false, viewModel.ReCalculateReleaseGroups);

			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				form.Show();
				form.recalculateReleaseGroupsCheckBox.Checked = true;
				form.ReapplyButton.PerformClick();
			}

			AssertEquals(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyWorkflowAndTasksOptions);
			AssertEquals(ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyMilestonesOptions);
			AssertEquals(ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, viewModel.ReapplyTriggersOptions);
			AssertEquals(true, viewModel.ReCalculateReleaseGroups);
		}

		public void TestTemplateApplicationInformationLabelBasedOnConfig()
		{
			AssertTemplateApplicationInformationLabelBasedOnConfig(delayReapplyTemplatesToServiceTask: true, processAndSaveInNewFactory: true, expectedLabelText:
				"It may take some time for the templates to be applied in the background.");
			AssertTemplateApplicationInformationLabelBasedOnConfig(delayReapplyTemplatesToServiceTask: true, processAndSaveInNewFactory: false, expectedLabelText:
				"It may take some time for the templates to be applied in the background.");//Ussure this combination would ever make sense in reality
			AssertTemplateApplicationInformationLabelBasedOnConfig(delayReapplyTemplatesToServiceTask: false, processAndSaveInNewFactory: true, "Templates will be applied and saved immediately.");
			AssertTemplateApplicationInformationLabelBasedOnConfig(delayReapplyTemplatesToServiceTask: false, processAndSaveInNewFactory: false, "Templates will be applied but not saved until the main form is saved.");
		}

		 void AssertTemplateApplicationInformationLabelBasedOnConfig(bool delayReapplyTemplatesToServiceTask, bool processAndSaveInNewFactory, string expectedLabelText)
		{
			var config = GetMockConfig(delayReapplyTemplatesToServiceTask, processAndSaveInNewFactory);
			var viewModel = new ReapplyWorkflowTemplateUserOptions(config, ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply, ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply, ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply, true);

			using (var form = new ReapplyWorkflowTemplatesConfigurationForm(viewModel))
			{
				form.Show();
				var label = form.Controls.Find("zLabel1", false)?[0] as ZLabel;
				AssertEquals(expectedLabelText, label.Text);
			}
		}

		IReapplyWorkflowTemplateConfiguration GetMockConfig(bool delayReapplyTemplatesToServiceTask, bool processAndSaveInNewFactory = false)
		{
			var config = new Mock<IReapplyWorkflowTemplateConfiguration>();
			config.Setup(x => x.DelayReapplyTemplatesToServiceTask).Returns(delayReapplyTemplatesToServiceTask);
			config.Setup(x => x.ProcessAndSaveInNewFactory).Returns(processAndSaveInNewFactory);
			return config.Object;
		}

		protected override void SetUp()
		{
			base.SetUp();

			viewModel = new ReapplyWorkflowTemplateUserOptions(new ReapplyWorkflowTemplateInServiceTaskConfiguration());
		}

		protected override Form GetFormToBashCore()
		{
			return new ReapplyWorkflowTemplatesConfigurationForm(viewModel);
		}

		ReapplyWorkflowTemplateUserOptions viewModel;
	}
}
