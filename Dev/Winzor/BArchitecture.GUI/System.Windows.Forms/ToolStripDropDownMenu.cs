using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms;

public class ToolStripDropDownMenu : ToolStripDropDown
{
	public override LayoutEngine LayoutEngine => ToolStripDropDownLayoutEngine.LayoutInstance;

	internal sealed class ToolStripDropDownLayoutEngine : FlowLayout
	{
		public static ToolStripDropDownLayoutEngine LayoutInstance = new ToolStripDropDownLayoutEngine();
	}

	protected internal override ToolStripItem CreateDefaultItem(string? text, Image? image, EventHandler? onClick)
		=> text == "-" ? new ToolStripSeparator() : new ToolStripMenuItem(text, image, onClick);
}
