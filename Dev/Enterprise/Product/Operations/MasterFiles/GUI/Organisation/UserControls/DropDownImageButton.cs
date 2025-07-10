using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DropDownImageButton : ZImageButton
	{
		public DropDownImageButton()
		{
			ContextMenuStrip = new ContextMenuStrip();

			Click += DropImageButton_Click;
			ContextMenuStrip.VisibleChanged += ContextMenuStrip_VisibleChanged;
		}

		public ToolStripItemCollection Items
		{
			get { return ContextMenuStrip.Items; }
		}

		protected override bool IsDown
		{
			get { return ContextMenuStrip.Visible || base.IsDown; }
		}

		void DropImageButton_Click(object sender, System.EventArgs e)
		{
			var scaleY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height);

			ContextMenuStrip.Show(this, ControlDpiScalingHelper.NewScaledPoint(0, scaleY));
		}

		void ContextMenuStrip_VisibleChanged(object sender, System.EventArgs e)
		{
			RefreshBackgroundImage();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (ContextMenuStrip != null && !ContextMenuStrip.IsDisposed)
				{
					ContextMenuStrip.Dispose();
					ContextMenuStrip = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}
