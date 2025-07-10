using System.Windows.Forms;
using Enterprise.MasterFiles.GUI.Workflow;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(WorkflowDiagnosticForm))]
	sealed class WorkflowDiagnosticFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new WorkflowDiagnosticForm(null);
		}
	}
}
