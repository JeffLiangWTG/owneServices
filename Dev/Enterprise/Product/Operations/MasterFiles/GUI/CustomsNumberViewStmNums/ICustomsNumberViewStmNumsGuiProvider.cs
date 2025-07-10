using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public interface ICustomsNumberViewStmNumsGuiProvider
	{
		Control GetUserControl();
		Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper);
	}
}
