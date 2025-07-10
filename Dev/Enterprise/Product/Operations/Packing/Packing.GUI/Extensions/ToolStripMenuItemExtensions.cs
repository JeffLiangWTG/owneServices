using System.Windows.Forms;

namespace Enterprise.Packing.GUI
{
	public static class ToolStripMenuItemExtensions
	{
		public static void PerformClickEnableFirst(this ToolStripMenuItem item)
		{
			item.Enabled = true;
			item.PerformClick();
		}
	}
}
