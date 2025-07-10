namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class DpsResultWinform
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.DpsResultControl = new Enterprise.DeniedPartyScreening.GUI.DpsResultUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DpsResultControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 676, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1283, 24, true);
			// 
			// DpsResultControl
			// 
			this.DpsResultControl.AllowDrop = true;
			this.DpsResultControl.BackColor = System.Drawing.SystemColors.Control;
			this.DpsResultControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DpsResultControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DpsResultControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DpsResultControl.Name = "DpsResultControl";
			this.DpsResultControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1283, 676, true);
			this.DpsResultControl.TabIndex = 1;
			// 
			// DpsResultWinform
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("BCBF6FCE-AACA-499E-847A-8DFA3232F078", "Denied Party Screening Result");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1283, 700, true);
			this.Controls.Add(this.DpsResultControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1279, 679, true);
			this.Name = "DpsResultWinform";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DpsResultControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DpsResultControl.ResumeLayout(true);
			this.DpsResultControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DpsResultUserControl DpsResultControl;
	}
}
