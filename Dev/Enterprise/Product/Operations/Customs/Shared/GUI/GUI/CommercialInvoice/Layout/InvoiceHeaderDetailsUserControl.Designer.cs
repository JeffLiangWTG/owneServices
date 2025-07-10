using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.CommercialInvoice
{
	partial class InvoiceHeaderDetailsUserControl
	{
		void InitializeComponent()
		{
			this.DynamicHeaderDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceHeader);
			// 
			// DynamicHeaderDetailsPanel
			// 
			this.DynamicHeaderDetailsPanel.AllowDrop = true;
			this.DynamicHeaderDetailsPanel.AutoScroll = true;
			this.DynamicHeaderDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicHeaderDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicHeaderDetailsPanel.Name = "DynamicHeaderDetailsPanel";
			this.DynamicHeaderDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicHeaderDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 206, true);
			this.DynamicHeaderDetailsPanel.TabIndex = 1;
			// 
			// InvoiceHeaderDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicHeaderDetailsPanel);
			this.Name = "InvoiceHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 206, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		DynamicLayoutPanel DynamicHeaderDetailsPanel;
	}
}
