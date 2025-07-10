using System.Drawing;

namespace System.Windows.Forms;

public class MenuStrip : ToolStrip
{
	protected override bool DefaultShowItemToolTips => false;

	protected internal override ToolStripItem CreateDefaultItem(string? text, Image? image, EventHandler? onClick)
		=> text == "-" ? new ToolStripSeparator() : new ToolStripMenuItem(text, image, onClick);
}
