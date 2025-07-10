using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(NonMatchingAgentsDialog))]
	internal class NonMatchingAgentsDialogBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			NonMatchingAgentsSecurity agentsSecurity = new NonMatchingAgentsSecurity();
			var dialog = new NonMatchingAgentsDialog(agentsSecurity);
			MissingResourceStringChecker.ExcludeFromTest(dialog.MessageLabel);
			return dialog;
		}

		#endregion
	}
}
