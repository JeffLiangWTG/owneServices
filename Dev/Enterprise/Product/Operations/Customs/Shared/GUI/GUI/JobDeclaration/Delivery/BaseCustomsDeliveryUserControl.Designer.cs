namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsDeliveryUserControl
	{
		void InitializeComponent()
		{
			this.DeliveryTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DeliveryDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DeliveryUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryTabControl.SuspendLayout();
			this.DeliveryDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// DeliveryTabControl
			// 
			this.DeliveryTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DeliveryTabControl.Controls.Add(this.DeliveryDetailsTabPage);
			this.DeliveryTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeliveryTabControl.Name = "DeliveryTabControl";
			this.DeliveryTabControl.SelectedIndex = 0;
			this.DeliveryTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 293, true);
			this.DeliveryTabControl.TabIndex = 0;
			// 
			// DeliveryDetailsTabPage
			// 
			this.DeliveryDetailsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8c88a7de-5d51-43aa-9b7f-26b476d2d508", "Details");
			this.DeliveryDetailsTabPage.Controls.Add(this.DeliveryUserControl);
			this.DeliveryDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeliveryDetailsTabPage.Name = "DeliveryDetailsTabPage";
			this.DeliveryDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeliveryDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 266, true);
			this.DeliveryDetailsTabPage.TabIndex = 0;
			this.DeliveryDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// DeliveryUserControl
			// 
			this.DeliveryUserControl.AllowDrop = true;
			this.DeliveryUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DeliveryUserControl.Name = "DeliveryUserControl";
			this.DeliveryUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 260, true);
			this.DeliveryUserControl.TabIndex = 0;
			// 
			// BaseCustomsDeliveryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryTabControl);
			this.Name = "BaseCustomsDeliveryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryTabControl.ResumeLayout(false);
			this.DeliveryTabControl.PerformLayout();
			this.DeliveryDetailsTabPage.ResumeLayout(false);
			this.DeliveryDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.GUI.ZTabControl DeliveryTabControl;
		ZArchitecture.GUI.ZTabPage DeliveryDetailsTabPage;
		ZArchitecture.GUI.ZDynamicControlCreationUserControl DeliveryUserControl;
	}
}
