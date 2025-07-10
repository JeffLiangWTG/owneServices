namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickSlipUserControl
	{
		PickHeaderUserControl HeaderUserControl;
		public PickLinesUserControl LinesUserControl;

		void InitializeComponent()
		{
			this.LinesUserControl = new PickLinesUserControl();
			this.HeaderUserControl = new PickHeaderUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LinesUserControl.SuspendLayout();
			this.HeaderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsPick);
			// 
			// LinesUserControl
			// 
			this.LinesUserControl.AllowDrop = true;
			this.LinesUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LinesUserControl, ".");
			this.LinesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 118, true);
			this.LinesUserControl.Name = "LinesUserControl";
			this.LinesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 335, true);
			this.LinesUserControl.TabIndex = 1;
			// 
			// HeaderUserControl
			// 
			this.HeaderUserControl.AllowDrop = true;
			this.HeaderUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HeaderUserControl, ".");
			this.HeaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.HeaderUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.HeaderUserControl.Name = "HeaderUserControl";
			this.HeaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.HeaderUserControl.TabIndex = 0;
			// 
			// PickSlipUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LinesUserControl);
			this.Controls.Add(this.HeaderUserControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 454, true);
			this.Name = "PickSlipUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 454, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LinesUserControl.ResumeLayout(true);
			this.LinesUserControl.PerformLayout();
			this.HeaderUserControl.ResumeLayout(true);
			this.HeaderUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
