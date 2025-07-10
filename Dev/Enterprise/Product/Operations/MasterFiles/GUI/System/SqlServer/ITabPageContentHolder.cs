using System.Windows.Forms;

namespace Enterprise.MasterFiles.GUI
{
	interface ITabPageContentHolder
	{
		void OnParentFormClosing(Form parent, FormClosingEventArgs e);
		void OnParentTabControlSwitchingToOtherTab();
	}
}
