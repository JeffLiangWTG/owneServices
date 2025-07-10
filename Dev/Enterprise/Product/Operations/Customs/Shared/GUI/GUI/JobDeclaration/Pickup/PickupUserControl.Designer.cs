namespace Enterprise.Customs.GUI
{
	public partial class PickupUserControl
	{
		void InitializeComponent()
		{
			this.PickupPartiesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.PickupDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PickupDetailsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickupDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PickupPartiesUserControl
			// 
			this.PickupPartiesUserControl.AllowDrop = true;
			this.PickupPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 3, true);
			this.PickupPartiesUserControl.Name = "PickupPartiesUserControl";
			this.PickupPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 252, true);
			this.PickupPartiesUserControl.TabIndex = 0;
			// 
			// PickupDetailsGroupBox
			// 
			this.PickupDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("b664b816-f38f-420f-b9fc-386f7dbbccc6", "Pickup Details");
			this.PickupDetailsGroupBox.Controls.Add(this.PickupDetailsUserControl);
			this.PickupDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 3, true);
			this.PickupDetailsGroupBox.Name = "PickupDetailsGroupBox";
			this.PickupDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 242, true);
			this.PickupDetailsGroupBox.TabIndex = 1;
			this.PickupDetailsGroupBox.TabStop = false;
			// 
			// PickupDetailsUserControl
			// 
			this.PickupDetailsUserControl.AllowDrop = true;
			this.PickupDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickupDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PickupDetailsUserControl.Name = "PickupDetailsUserControl";
			this.PickupDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 223, true);
			this.PickupDetailsUserControl.TabIndex = 0;
			// 
			// PickupUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickupDetailsGroupBox);
			this.Controls.Add(this.PickupPartiesUserControl);
			this.Name = "PickupUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 266, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickupDetailsGroupBox.ResumeLayout(false);
			this.PickupDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZGroupBox PickupDetailsGroupBox;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl PickupDetailsUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl PickupPartiesUserControl;
	}
}
