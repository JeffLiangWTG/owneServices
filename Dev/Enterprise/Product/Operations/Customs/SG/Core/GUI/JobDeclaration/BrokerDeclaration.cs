using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class BrokerDeclaration
	{
		public BrokerDeclaration()
		{
		}

		public bool GetBrokerDeclaration()
		{
			DialogResult result = Globals.Message.ShowConfirmation(FrontendDeclaration, "Submitting Broker Mandatory Declaration", "TYPE: ", "Yes", MessageBoxIcon.Exclamation);
			return result == DialogResult.OK;
		}

		const string FrontendDeclaration = "I/we declare that the information declared is true and correct. \n\n\nTo continue, and submit this Declaration;";
	}
}
