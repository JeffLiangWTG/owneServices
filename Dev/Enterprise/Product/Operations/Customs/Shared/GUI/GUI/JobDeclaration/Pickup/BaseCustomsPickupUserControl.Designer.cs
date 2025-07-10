using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsPickupUserControl
	{
		void InitializeComponent()
		{
			this.PickTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PickupUserControl = new ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.PickupUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PickTabControl
			// 
			this.PickTabControl.Anchor =((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PickTabControl.Controls.Add(this.DetailsTabPage);
			this.PickTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickTabControl.Name = "PickTabControl";
			this.PickTabControl.SelectedIndex = 0;
			this.PickTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 281, true);
			this.PickTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString =
				Enterprise.Customs.GUI.Res.GetData("1b2ad1fe-c79b-42f1-8378-09ad9f396fd0", "Details");
			this.DetailsTabPage.Controls.Add(this.PickupUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 254, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// PickupUserControl
			// 
			this.PickupUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupUserControl, ".");
			this.PickupUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickupUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickupUserControl.Name = "PickupUserControl";
			this.PickupUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 254, true);
			this.PickupUserControl.TabIndex = 0;
			// 
			// BaseCustomsPickupUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickTabControl);
			this.Name = "BaseCustomsPickupUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 281, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickTabControl.ResumeLayout(false);
			this.PickTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.PickupUserControl.ResumeLayout(true);
			this.PickupUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZTabControl PickTabControl;
		ZTabPage DetailsTabPage;
		ZArchitecture.GUI.ZDynamicControlCreationUserControl PickupUserControl;
	}
}
