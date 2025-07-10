namespace Enterprise.Customs.US.AMS.GUI
{
	partial class StowPlanForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.stowPlanUserControl = new Enterprise.Customs.US.AMS.GUI.StowPlanUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 446, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.StowPlanSailingData);
			// 
			// stowPlanUserControl
			// 
			this.stowPlanUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.stowPlanUserControl, ".");
			this.stowPlanUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stowPlanUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.stowPlanUserControl.Name = "stowPlanUserControl";
			this.stowPlanUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 470, true);
			this.stowPlanUserControl.TabIndex = 1;
			// 
			// StowPlanForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 470, true);
			this.Controls.Add(this.stowPlanUserControl);
			this.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.StowPlanSailingData);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 509, true);
			this.Name = "StowPlanForm";
			this.Text = "StowPlanForm";
			this.Controls.SetChildIndex(this.stowPlanUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal StowPlanUserControl stowPlanUserControl;

	}
}