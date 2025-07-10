namespace Enterprise.MarketingManager.GUI
{
	partial class TradeProfileForOrgUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.salesBreakdownControl = new Enterprise.MarketingManager.GUI.SalesBreakdownControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.salesBreakdownControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// salesBreakdownControl
			// 
			this.salesBreakdownControl.AllowDrop = true;
			this.salesBreakdownControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesBreakdownControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.salesBreakdownControl.Name = "salesBreakdownControl";
			this.salesBreakdownControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			this.salesBreakdownControl.TabIndex = 3;
			// 
			// TradeProfileForOrgUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.salesBreakdownControl);
			this.Name = "TradeProfileForOrgUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.salesBreakdownControl.ResumeLayout(true);
			this.salesBreakdownControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.MarketingManager.GUI.SalesBreakdownControl salesBreakdownControl;
	}
}
