using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class InvoiceLineDetailsUserControl
	{
		void InitializeComponent()
		{
			this.DynamicLineDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceLine);
			// 
			// DynamicLineDetailsPanel
			// 
			this.DynamicLineDetailsPanel.AllowDrop = true;
			this.DynamicLineDetailsPanel.AutoScroll = true;
			this.DynamicLineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicLineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicLineDetailsPanel.Name = "DynamicLineDetailsPanel";
			this.DynamicLineDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicLineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 281, true);
			this.DynamicLineDetailsPanel.TabIndex = 1;
			this.DynamicLineDetailsPanel.CaptionRenderingEnabled = true;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(DynamicLineDetailsPanel);
		}

		DynamicLayoutPanel DynamicLineDetailsPanel;
	}
}
