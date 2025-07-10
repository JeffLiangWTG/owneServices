using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class JiraProjectSelectorForm : ZChildForm
	{
		public static void ShowForm()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(JiraProjectSelectorForm) };
			var viewModel = new ProjectsToImportViewModel(factory);

			if (viewModel.JiraUrls.Count == 0)
			{
				var message = Res.GetString("e696c352-4f8d-474a-8983-18bec8ceaa83", "Please enter the Jira site URL in the registry item {0} within {1}.", ProcessManagementRegistry.Instance.JiraSiteUrls.Caption, ProcessManagementRegistry.Instance.JiraSiteUrls.Category);
				Globals.Message.Show(message);
			}
			else
			{
				var form = new JiraProjectSelectorForm(viewModel);
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		public JiraProjectSelectorForm(BusinessObjectFactory factory)
			: this(new ProjectsToImportViewModel(factory))
		{
		}

		JiraProjectSelectorForm(ProjectsToImportViewModel viewModel)
			: base(viewModel)
		{
			InitializeComponent();
			CancelButton = JiraProjectSelectorControl.CancelImportButton;
		}

		internal void ShowErrorsDialog() => base.ShowErrorsDialog();

		public override string FormVerb => string.Empty;
	}
}
