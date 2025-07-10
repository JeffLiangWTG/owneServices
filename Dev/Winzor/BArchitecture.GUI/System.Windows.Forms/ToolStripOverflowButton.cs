using System.Drawing;

namespace System.Windows.Forms;

public class ToolStripOverflowButton : ToolStripDropDownButton
{
	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Accessed via reflection in class KToolStrip")]
	readonly ToolStrip parentToolStrip;
	const int maxWidth = 16;
	const int maxHeight = 16;

	internal ToolStripOverflowButton(ToolStrip parentToolStrip)
	{
		this.parentToolStrip = parentToolStrip;
	}

	protected override Padding DefaultMargin => Padding.Empty;

	public override Size GetPreferredSize(Size constrainingSize)
	{
		Size preferredSize = constrainingSize;
		if (ParentInternal != null)
		{
			if (ParentInternal.Orientation == Orientation.Horizontal)
			{
				preferredSize.Width = Math.Min(constrainingSize.Width, maxWidth);
			}
			else
			{
				preferredSize.Height = Math.Min(constrainingSize.Height, maxHeight);
			}
		}
		return preferredSize + Padding.Size;
	}
}
