using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EventContextForm))]
	sealed class EventContextFormTest : ZFormBasherTest
	{
		#region Implemntation

		protected override Form GetFormToBashCore()
		{
			return new EventContextForm(new WorkflowEventContextBizo(new DummyEnterpriseBusinessObjectWorkflowDescriptor()));
		}

		#endregion
	}
}
