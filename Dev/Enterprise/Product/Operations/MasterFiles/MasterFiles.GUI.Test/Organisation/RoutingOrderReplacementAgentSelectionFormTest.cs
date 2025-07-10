using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RoutingOrderReplacementAgentSelectionForm))]
	sealed class RoutingOrderReplacementAgentSelectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RoutingOrderReplacementAgentSelectionForm(new AgentSelectionBusinessObject(Factory));
		}
	}
}
