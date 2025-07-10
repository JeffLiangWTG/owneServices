using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI.Test
{
	public class DummyJiraProjectSelectorUserControl : JiraProjectSelectorUserControl
	{
		public void SetParentViewModel(ProjectsToImportViewModel viewModel)
		{
			SetDataBinding(viewModel, "");
		}

		ProgressForm progressForm;

		public bool ProgressFormWasDisposed => progressForm?.IsDisposed ?? false;

		protected override ProgressForm CreateProgressForm(IJiraImporterProgressTracker progressTracker)
		{
			progressForm = base.CreateProgressForm(progressTracker);

			return progressForm;
		}
	}
}
