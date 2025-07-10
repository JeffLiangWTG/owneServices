namespace Enterprise.Customs.GUI
{
	public partial class DeliveryUserControl
	{
		void InitializeComponent()
		{
			this.DeliveryPartiesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.DeliveryDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeliveryDetailsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// DeliveryPartiesUserControl
			// 
			this.DeliveryPartiesUserControl.AllowDrop = true;
			this.DeliveryPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.DeliveryPartiesUserControl.Name = "DeliveryPartiesUserControl";
			this.DeliveryPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 251, true);
			this.DeliveryPartiesUserControl.TabIndex = 0;
			// 
			// DeliveryDetailsGroupBox
			// 
			this.DeliveryDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("e182115a-e11e-4807-a0ab-3e5fe0f9806d", "Delivery Details");
			this.DeliveryDetailsGroupBox.Controls.Add(this.DeliveryDetailsUserControl);
			this.DeliveryDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 3, true);
			this.DeliveryDetailsGroupBox.Name = "DeliveryDetailsGroupBox";
			this.DeliveryDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 239, true);
			this.DeliveryDetailsGroupBox.TabIndex = 1;
			this.DeliveryDetailsGroupBox.TabStop = false;
			// 
			// DeliveryDetailsUserControl
			// 
			this.DeliveryDetailsUserControl.AllowDrop = true;
			this.DeliveryDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DeliveryDetailsUserControl.Name = "DeliveryDetailsUserControl";
			this.DeliveryDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 220, true);
			this.DeliveryDetailsUserControl.TabIndex = 0;
			// 
			// DeliveryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryDetailsGroupBox);
			this.Controls.Add(this.DeliveryPartiesUserControl);
			this.Name = "DeliveryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 297, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryDetailsGroupBox.ResumeLayout(false);
			this.DeliveryDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DeliveryPartiesUserControl;
		protected ZArchitecture.GUI.ZGroupBox DeliveryDetailsGroupBox;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DeliveryDetailsUserControl;
	}
}
