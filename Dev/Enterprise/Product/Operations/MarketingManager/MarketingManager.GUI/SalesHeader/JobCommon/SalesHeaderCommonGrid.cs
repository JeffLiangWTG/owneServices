using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	public class SalesHeaderCommonGrid : ZGrid
	{
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				base.OnMouseDown(e);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				base.OnMouseUp(e);
			}
		}
	}
}
