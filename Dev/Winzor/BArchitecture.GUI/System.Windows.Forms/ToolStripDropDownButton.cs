using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms;

public partial class ToolStripDropDownButton : ToolStripDropDownItem
{
	protected override bool DefaultAutoToolTip => true;

	public override bool UseParentDivForLayout => false;

	//This constant is to increase the width of the ToolStripDropDownButton by 6 pixels to size it closer to the WinForms counterpart
	const int ToolStripDropDownButtonExtraWidth = 6;

	internal override Size GetPreferredSizeCore(Size proposedSize)
	{
		var preferredSize = base.GetPreferredSizeCore(proposedSize);
		preferredSize.Width += ToolStripDropDownButtonExtraWidth;
		return preferredSize;
	}

	public bool ShowDropDownArrow
	{
		get
		{
			return showDropDownArrow;
		}
		set
		{
			if (showDropDownArrow != value)
			{
				showDropDownArrow = value;
				InvalidateItemLayout(PropertyNames.ShowDropDownArrow);
				NotifyRenderRequired();
			}
		}
	}
	bool showDropDownArrow = true;

	/// <summary>
	///  Inheriting classes should override this method to handle this event.
	/// </summary>
	protected override void OnPaint(PaintEventArgs e)
	{
		if (Owner is not null)
		{
			ToolStripRenderer renderer = Renderer!;
			renderer.DrawDropDownButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
		}
	}

	/// <summary>
	///  Overriden to invoke displaying the popup.
	/// </summary>
	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (ModifierKeys != Keys.Alt && e.Button == MouseButtons.Left)
		{
			if (!DropDown.Visible)
			{
				ShowDropDown();
			}
		}

		base.OnMouseDown(e);
	}
}
