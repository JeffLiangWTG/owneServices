using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CartageCalculatorPanel : ZUserControl
	{
		public CartageCalculatorPanel()
		{
			InitializeComponent();

			InitialDistance = SplitContainer.SplitterDistance;
		}

		void ZonesButton_CheckedChanged(object sender, EventArgs e)
		{
			if (ZonesButton.Checked)
			{
				if (SplitContainer.SplitterDistance <= ToolStripWidthWithPadding)
				{
					if (PrevSplitterDistance <= ToolStripWidthWithPadding)
					{
						PrevSplitterDistance = InitialDistance;
					}
					SplitContainer.SplitterDistance = PrevSplitterDistance;
				}
			}
			else
			{
				if (SplitContainer.SplitterDistance > ToolStripWidthWithPadding)
				{
					PrevSplitterDistance = SplitContainer.SplitterDistance;
					SplitContainer.SplitterDistance = ToolStripWidthWithPadding;
				}
			}
		}

		int ToolStripWidthWithPadding => ToolStrip.Width + ControlDpiScalingHelper.OnePixel;

		void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (SplitContainer.SplitterDistance > ToolStripWidthWithPadding)
			{
				if (!ZonesButton.Checked)
				{
					ZonesButton.Checked = true;
				}
			}
			else
			{
				if (ZonesButton.Checked)
				{
					ZonesButton.Checked = false;
				}
			}
		}

		readonly int InitialDistance;
		int PrevSplitterDistance;
	}
}
