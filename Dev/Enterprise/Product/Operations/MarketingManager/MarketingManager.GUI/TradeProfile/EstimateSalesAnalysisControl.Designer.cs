namespace Enterprise.MarketingManager.GUI
{
	partial class EstimateSalesAnalysisControl
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
			this.dynamicTradeLaneWithDetailsControl = new Enterprise.MarketingManager.GUI.DynamicTradeLaneWithDetailsControl();
			this.filterControl = new Enterprise.MarketingManager.GUI.EstimateSalesAnalysisFilterControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.filterControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesHeader);
			// 
			// dynamicTradeLaneWithDetailsControl
			// 
			this.dynamicTradeLaneWithDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dynamicTradeLaneWithDetailsControl, ".");
			this.dynamicTradeLaneWithDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dynamicTradeLaneWithDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.dynamicTradeLaneWithDetailsControl.Name = "dynamicTradeLaneWithDetailsControl";
			this.dynamicTradeLaneWithDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 254, true);
			this.dynamicTradeLaneWithDetailsControl.TabIndex = 2;
			// 
			// filterControl
			// 
			this.filterControl.AllowDrop = true;
			this.filterControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.filterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.filterControl.Name = "filterControl";
			this.filterControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 24, true);
			this.filterControl.TabIndex = 1;
			// 
			// EstimateSalesAnalysisControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.dynamicTradeLaneWithDetailsControl);
			this.Controls.Add(this.filterControl);
			this.Name = "EstimateSalesAnalysisControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 278, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.filterControl.ResumeLayout(true);
			this.filterControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DynamicTradeLaneWithDetailsControl dynamicTradeLaneWithDetailsControl;
		private EstimateSalesAnalysisFilterControl filterControl;
	}
}
