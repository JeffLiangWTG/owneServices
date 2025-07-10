using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RoutingOrderReplacementAgentSelectionForm : ZChildForm
	{
		public RoutingOrderReplacementAgentSelectionForm(AgentSelectionBusinessObject agentSelectionBizo) : base(agentSelectionBizo)
		{
			this.InstructionsLabel.Text = Res.GetString("RoutingOrderReplacementAgentSelectionForm|InstructionsLabel.Text", "Specify the agent you wish to use instead of the current organization. Clicking OK will produce a Routing Order for each relationship that was previously using this agent.");
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			CloseForm();
		}

		void CloseForm()
		{
			if (!BusinessEntity.HasErrors())
			{
				Close();
			}
		}
	}
}
