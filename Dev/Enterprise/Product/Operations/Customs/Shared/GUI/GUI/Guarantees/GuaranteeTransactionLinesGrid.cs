using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Guarantees
{
	class GuaranteeTransactionLinesGrid : ZDisplayGrid
	{
		protected override bool ShouldShowNotifications
		{
			get { return true; }
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Enter || keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift))
			{
				return false;
			}
			else
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}
		}
	}
}
