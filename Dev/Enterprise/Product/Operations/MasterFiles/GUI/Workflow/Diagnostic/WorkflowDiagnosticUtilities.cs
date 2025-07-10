using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	internal static class WorkflowDiagnosticUtilities
	{
		internal static void ShowWorkflowDiagnosisForm(ProcessTask task, Form parentForm)
		{
			if (task != null)
			{
				var viewModel = new WorkflowDiagnosticViewModel(task);
				var form = new WorkflowDiagnosticForm(viewModel);
				parentForm.FormClosing += (object sender, FormClosingEventArgs e) => { form.Close(); };
				form.Show();
			}
			else
			{
				Globals.Message.Show(
					Res.GetString("E43457C9-D8AB-410B-94E2-01893BC8A0DC", "No milestone or trigger was selected in the grid."),
					Res.GetString("F5221D32-6470-4AEC-854D-449F07270FDD", "Workflow Diagnosis Warning"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning);
			}
		}

		internal static void ShowMatchingTemplateForm(IWorkflowItemCollection workflowItems)
		{
			ZFormModaliser.ShowDialogAndDispose(new MatchingTemplateForm(workflowItems));
		}
	}
}
