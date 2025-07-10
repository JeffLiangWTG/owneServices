using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public partial class WorkflowDiagnosticUserControl : ZUserControl
	{
		public WorkflowDiagnosticUserControl()
		{
			InitializeComponent();
		}

		void SourceLogsHintLabel_TextChanged(object sender, EventArgs e)
		{
			SourceLogsSplitContainer.Panel1Collapsed = string.IsNullOrEmpty(SourceLogsHintLabel.Text);
		}
	}
}
