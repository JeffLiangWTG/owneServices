using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class CampaignTrackingStatusGroupBox : ZGroupBox
	{
		public CampaignTrackingStatusGroupBox()
			: base()
		{
		}

#if !WINZOR

		Font SetGroupBoxFont
		{
			get
			{
				return new Font(Font, FontStyle.Bold);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint(e);

			using (var path = new GraphicsPath())
			{
				path.AddRectangle(ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					ControlDpiScalingHelper.ScaleToCurrentDpiY(7),
					Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(101),
					ControlDpiScalingHelper.ScaleToCurrentDpiY(8),
					false));
				path.AddRectangle(ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.ScaleToCurrentDpiX(1),
					ControlDpiScalingHelper.ScaleToCurrentDpiY(15),
					Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(2),
					Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(16),
					false));
				e.Graphics.FillPath(Brushes.White, path);
				using (var bodyBrush = new SolidBrush(Parent.BackColor))
				{
					e.Graphics.FillRectangle(bodyBrush, ControlDpiScalingHelper.NewScaledRectangle(5, 0, 107, 15));
				}
				using (var fontBrush = new SolidBrush(ForeColor))
				{
					TextRendererHelper.DrawText(e.Graphics, Res.GetString("890b2b53-3a38-4099-b900-d8db8697871a", "Campaign Summary"), SetGroupBoxFont, ControlDpiScalingHelper.NewScaledPoint(5, 0), fontBrush);
				}
			}
		}

#endif
	}
}
