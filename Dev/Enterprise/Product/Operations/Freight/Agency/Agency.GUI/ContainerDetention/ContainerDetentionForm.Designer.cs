namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerDetentionForm
	{
		new void InitializeComponent()
		{
			mainControl = new Enterprise.Freight.Agency.GUI.ContainerDetentionControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 638, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(mainControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 611, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ContainerDetention);
			// 
			// mainControl
			// 
			this.BindingSource.SetBindingMember(mainControl, ".");
			mainControl.Dock = System.Windows.Forms.DockStyle.Fill;
			mainControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			mainControl.Name = "mainControl";
			mainControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 611, true);
			mainControl.TabIndex = 2;
			mainControl.Find += new System.EventHandler(this.mainControl_Find);
			// 
			// ContainerDetentionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 694, true);
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerDetentionForm|2f221c9f-c73c-44c4-a6f6-6f86ce865c2e", "Detention Invoice");
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ContainerDetention);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 721, true);
			this.Name = "ContainerDetentionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "DetentionInvoiceForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.Freight.Agency.GUI.ContainerDetentionControl mainControl;
	}
}
