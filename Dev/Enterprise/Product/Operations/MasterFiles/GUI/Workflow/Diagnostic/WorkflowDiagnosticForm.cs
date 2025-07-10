using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public partial class WorkflowDiagnosticForm : ZChildForm
	{
		public WorkflowDiagnosticForm(WorkflowDiagnosticViewModel viewModel) : base(viewModel)
		{
			InitializeComponent();
		}

		public override string FormVerb
		{
			get
			{
				return string.Empty;
			}
		}

		public override string FormCaption
		{
			get { return Res.GetString("3F9DA834-5C3A-41C4-8FF0-7B59616D62F4", "Workflow Diagnostic"); }
		}
	}
}
