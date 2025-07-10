using System;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class ReapplyWorkflowTemplatesConfigurationForm : ZChildForm
	{
		public ReapplyWorkflowTemplatesConfigurationForm(ReapplyWorkflowTemplateUserOptions viewModel)
			: base(viewModel)
		{
			this.viewModel = viewModel;
			InitializeComponent();
			SetTemplateApplicationInforomationLabel(viewModel.Configuration);
		}

		readonly ReapplyWorkflowTemplateUserOptions viewModel;

		#region ZForm Overrides

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		void cancelButton_Click(object sender, EventArgs e)
		{
			viewModel.ShouldPerformReapplication = false;
			Close();
		}

		void ReapplyButton_Click(object sender, EventArgs e)
		{
			if (!viewModel.HasAtLeastOnOptionSelected())
			{
				Globals.Message.Show(Res.GetString("48f842b3-f2ec-4fd7-ab02-5b4e95ffed4a", "Please select at least one option."));
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
			else if (viewModel.HasErrors)
			{
				Globals.Message.Show(Res.GetString("BBA03DA5-954C-4CC0-A5ED-BD45488A9BB3", "Please fix validation errors."));
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
			else
			{
				viewModel.ShouldPerformReapplication = true;
				Close();
			}
		}

		void SetTemplateApplicationInforomationLabel(IReapplyWorkflowTemplateConfiguration configuration)
		{
			if (configuration.DelayReapplyTemplatesToServiceTask)
			{
				this.zLabel1.CaptionResourceString = Res.GetData("0d6b1159-1e93-4b96-b7f6-2cf7b115574a", "It may take some time for the templates to be applied in the background.");
			}
			else
			{
				if (configuration.ProcessAndSaveInNewFactory)
				{
					this.zLabel1.CaptionResourceString = Res.GetData("15B5CB86-4C35-49EF-8C80-AF8E97D26F87", "Templates will be applied and saved immediately.");
				}
				else
				{
					this.zLabel1.CaptionResourceString = Res.GetData("2E72383F-B52D-4C4C-80E6-D9B14D64D16D", "Templates will be applied but not saved until the main form is saved.");
				}
			}
		}
	}
}
