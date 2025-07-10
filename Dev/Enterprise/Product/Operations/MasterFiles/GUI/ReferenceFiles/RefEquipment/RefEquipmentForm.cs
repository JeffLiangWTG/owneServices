using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefEquipmentForm : ZForm
	{
		public RefEquipmentForm(RefEquipment bO)
			: base(bO)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
			PlugIns.Add(ControllerIDs.GPSSupporter);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref ButtonsUserControl, MainStatusBar.Top - ButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedTab = ((TabControl)sender).SelectedTab;
		}

		protected void SetTabPageControls(TabPage page, ZLabel label, bool showControls)
		{
			foreach (Control ctrl in page.Controls)
			{
				ctrl.Visible = showControls;
			}

			if (label != null)
			{
				label.Visible = !showControls;
			}
		}

		void UnsubscribeHandlers()
		{
			if (MainTabControl != null)
			{
				MainTabControl.SelectedIndexChanged -= MainTabControl_SelectedIndexChanged;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnsubscribeHandlers();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
