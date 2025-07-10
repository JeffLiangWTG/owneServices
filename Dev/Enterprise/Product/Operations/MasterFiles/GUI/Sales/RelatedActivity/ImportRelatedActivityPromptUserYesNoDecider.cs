using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class ImportRelatedActivityPromptUserYesNoDecider : ImportRelatedActivityPromptUserDecider, IImportRelatedActivityYesNoDecider
	{
		public ImportRelatedActivityPromptUserYesNoDecider(KForm parentForm)
			: base(parentForm)
		{
		}

		public bool GetDecision(string question)
		{
			var dialogResult = Globals.Message.Show(question, DecideReason, MessageBoxButtons.YesNo, DialogResult.No);
			return dialogResult == DialogResult.Yes;
		}
	}
}
