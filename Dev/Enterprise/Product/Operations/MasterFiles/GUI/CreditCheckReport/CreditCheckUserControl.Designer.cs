namespace Enterprise.MasterFiles.GUI
{
	partial class CreditCheckUserControl
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

			if (creditCheckService != null)
			{
				creditCheckService.OnCertificateMismatched -= ClearCreditReportsPublicCertificate;
				creditCheckService = null;
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
			this.CreditCheckMainPageControlPanel = new ZArchitecture.GUI.ZPanel();
			this.SuspendLayout();
			// 
			// CreditCheckMainPageControlPanel
			// 
			this.CreditCheckMainPageControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreditCheckMainPageControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.CreditCheckMainPageControlPanel.Name = "CreditCheckMainPageControlPanel";
			this.CreditCheckMainPageControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 592);
			this.CreditCheckMainPageControlPanel.TabIndex = 1;
			this.CreditCheckMainPageControlPanel.Visible = false;
			// 
			// CreditCheckUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.CreditCheckMainPageControlPanel);
			this.Name = "CreditCheckUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 592);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZPanel CreditCheckMainPageControlPanel;
	}
}
