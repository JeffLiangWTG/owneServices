namespace Enterprise.MasterFiles.GUI
{
	partial class SalesActivityUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ToolStrip
			// 
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 25, true);
			this.ToolStrip.TabIndex = 0;
			// 
			// FilterPanel
			// 
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 305, true);
			this.FilterPanel.TabIndex = 1;
			// 
			// SalesActivityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "SalesActivityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 469, true);
			this.Controls.Add(this.FilterPanel);
			this.Controls.Add(this.ToolStrip);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();

		}

		#endregion

		private ZArchitecture.GUI.ZToolStrip ToolStrip;
		private ZArchitecture.GUI.ZPanel FilterPanel;
	}
}
