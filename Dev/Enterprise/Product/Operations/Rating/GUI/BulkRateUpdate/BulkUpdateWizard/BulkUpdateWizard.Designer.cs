namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdateWizard
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 535, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 24, true);
			// 
			// BulkUpdateWizard
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 600, true);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 700, true);
			this.MinimizeBox = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 700, true);
			this.Name = "BulkUpdateWizard";
			this.RememberFormSize = false;
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}
	}
}
