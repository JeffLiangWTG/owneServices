using System;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.PortHubs.GUI
{
	public partial class PortHubStripControl : StripControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilterStripsPanel
			// 
			this.FilterStripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 45, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PortHubSelectionCollectionWrapper);
			// 
			// PortHubStripControl
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Name = "PortHubStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 51, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		readonly System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
