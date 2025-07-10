namespace Enterprise.Warehouse.Environment.GUI
{
	partial class ProductStyleForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.ProductStyleControl = new Enterprise.Warehouse.Environment.GUI.ProductStyleUserControl();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ProductStyleControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 404, true);
            // 
            // MainTabPage
            // 
            this.MainTabPage.Controls.Add(this.ProductStyleControl);
            this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 381, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 381, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 404, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsProductStyle);
            // 
            // ProductStyleControl
            // 
            this.ProductStyleControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ProductStyleControl, ".");
            this.ProductStyleControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProductStyleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ProductStyleControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 320, true);
            this.ProductStyleControl.Name = "ProductStyleControl";
            this.ProductStyleControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
            this.ProductStyleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 381, true);
            this.ProductStyleControl.TabIndex = 2;
            // 
            // ProductStyleForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 460, true);
            this.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsProductStyle);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 460, true);
            this.Name = "ProductStyleForm";
            this.ShouldSerializeTabPageMethods = false;
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ProductStyleControl.ResumeLayout(true);
            this.ProductStyleControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ProductStyleUserControl ProductStyleControl;
	}
}