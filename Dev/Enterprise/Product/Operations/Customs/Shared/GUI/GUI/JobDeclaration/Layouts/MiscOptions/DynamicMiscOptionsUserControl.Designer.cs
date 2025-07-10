namespace Enterprise.Customs.GUI
{
	partial class DynamicMiscOptionsUserControl
	{
		void InitializeComponent()
		{
			this.DynamicMiscOptionsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceLine);
			// 
			// DynamicMiscOptionsPanel
			// 
			this.DynamicMiscOptionsPanel.AllowDrop = true;
			this.DynamicMiscOptionsPanel.AutoScroll = true;
			this.DynamicMiscOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicMiscOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.DynamicMiscOptionsPanel.Name = "DynamicMiscOptionsPanel";
			this.DynamicMiscOptionsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicMiscOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 520, true);
			this.DynamicMiscOptionsPanel.TabIndex = 1;
			this.DynamicMiscOptionsPanel.CaptionRenderingEnabled = true;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 520, true);
			this.Controls.Add(DynamicMiscOptionsPanel);
		}

		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicMiscOptionsPanel;
	}
}
