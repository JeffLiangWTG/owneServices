using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class RateEntryStripControl
	{
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilterStripsPanel
			// 
			this.FilterStripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 95, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RatingHeader);
			// 
			// RateEntryStripControl
			// 
			this.AutoScroll = false;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Name = "RateEntryStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 136, true);
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
