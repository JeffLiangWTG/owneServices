using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class OpportunitySalesValueAnalysisControl : ZUserControl
	{
		public OpportunitySalesValueAnalysisControl()
		{
			InitializeComponent();
		}

		public ZGrid Grid
		{
			get { return grid; }
		}

		void Grid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (grid.HitTest(e.X, e.Y).Row > -1)
			{
				if (GridRowDoubleClicked != null)
				{
					GridRowDoubleClicked(this, e);
				}
			}
		}

		public event EventHandler GridRowDoubleClicked;
	}
}
