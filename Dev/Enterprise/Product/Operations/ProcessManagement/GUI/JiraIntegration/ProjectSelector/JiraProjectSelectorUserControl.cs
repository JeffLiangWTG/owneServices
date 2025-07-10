using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class JiraProjectSelectorUserControl : ZUserControl
	{
		public JiraProjectSelectorUserControl()
		{
			InitializeComponent();
		}

		void BeginImportButton_Click(object sender, EventArgs e)
		{
			BeginImport((JiraProjectSelectorForm)FindForm());
		}

		void BeginImport(JiraProjectSelectorForm parentForm)
		{
			var viewModel = (ProjectsToImportViewModel)parentForm?.BusinessEntity;

			if (viewModel != null)
			{
				viewModel.RunPreSaveValidation();

				if (viewModel.HasErrors)
				{
					parentForm.ShowErrorsDialog();
				}
				else
				{
					ProjectImportReport importResult;
					var view = new JiraImporterProgressTracker();

					using (CreateAndShowProgressForm(parentForm, view))
					{
						importResult = viewModel.ImportJiraContentAndReportResult(view);
					}

					if (importResult.ProjectReportMessage.Equals(ProjectImportReport.Success()))
					{
						Globals.Message.Show(importResult.ProjectReportMessage);
					}
					else
					{
						Globals.Message.ShowError(importResult.ProjectReportMessage);
					}

					if (importResult.ShouldCloseImportForm)
					{
						parentForm.Close();
					}
				}
			}
		}

		ProgressForm CreateAndShowProgressForm(ZForm parentForm, IJiraImporterProgressTracker progressTracker)
		{
			var progressForm = CreateProgressForm(progressTracker);
			ZFormModaliser.Show(progressForm, parentForm);

			return progressForm;
		}

		protected virtual ProgressForm CreateProgressForm(IJiraImporterProgressTracker progressTracker)
		{
			return new JiraImporterProgressForm(Res.GetString("23A04ACC-6DC6-4EA1-975B-340679D758CF", "Starting your Jira Data Import now"), progressTracker);
		}

		#region For Test

		public void BeginImport_ForTest(JiraProjectSelectorForm parentForm = null) => BeginImport(parentForm ?? (JiraProjectSelectorForm)FindForm());

		#endregion
	}
}
