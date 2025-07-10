using System.Drawing;

namespace System.Windows.Forms;

public partial class ToolStripSeparator : ToolStripItem
{
	const int SeparatorThickness = 6;

	public override bool UseParentDivForLayout => false;

	protected override Padding DefaultMargin => Padding.Empty;

	bool IsVertical
	{
		get
		{
			var parent = ParentInternal ?? Owner;

			if (parent is null)
			{
				return true;
			}

			switch (parent.LayoutStyle)
			{
				case ToolStripLayoutStyle.VerticalStackWithOverflow:
					return false;
				case ToolStripLayoutStyle.HorizontalStackWithOverflow:
				case ToolStripLayoutStyle.Flow:
				case ToolStripLayoutStyle.Table:
				default:
					return true;
			}
		}
	}

	public override Size GetPreferredSize(Size constrainingSize) => IsVertical ? new Size(SeparatorThickness, constrainingSize.Height) : new Size(constrainingSize.Width, SeparatorThickness);
}
