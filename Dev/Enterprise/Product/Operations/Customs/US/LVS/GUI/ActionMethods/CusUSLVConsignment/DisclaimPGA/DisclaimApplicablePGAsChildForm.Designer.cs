
namespace Enterprise.Customs.US.LVS.GUI
{
	partial class DisclaimApplicablePGAsChildForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.disclaimApplicablePGAsUserControl1 = new Enterprise.Customs.US.LVS.GUI.DisclaimApplicablePGAsUserControl();
			this.applyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.disclaimApplicablePGAsUserControl1.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 442, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.GUI.DisclaimApplicablePGAsApplicator);
			// 
			// disclaimApplicablePGAsUserControl1
			// 
			this.disclaimApplicablePGAsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.disclaimApplicablePGAsUserControl1, ".");
			this.disclaimApplicablePGAsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.disclaimApplicablePGAsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.disclaimApplicablePGAsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 395, true);
			this.disclaimApplicablePGAsUserControl1.Name = "disclaimApplicablePGAsUserControl1";
			this.disclaimApplicablePGAsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 415, true);
			this.disclaimApplicablePGAsUserControl1.TabIndex = 1;
			// 
			// applyButton
			// 
			this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.applyButton.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("def4e665-34db-4d5f-a46f-b3a9a69caa7d", "OK");
			this.applyButton.IsCaptionOverridden = false;
			this.applyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 2, true);
			this.applyButton.Name = "applyButton";
			this.applyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.applyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.applyButton.TabIndex = 2;
			this.applyButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.applyButton.ToolTipCaption = null;
			this.applyButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("9b8012eb-3a83-433c-9585-8d0bdcfeffd8", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 2, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.applyButton);
			this.bottomPanel.Controls.Add(this.cancelButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 415, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 27, true);
			this.bottomPanel.TabIndex = 4;
			// 
			// DisclaimApplicablePGAsChildForm
			// 
			this.AcceptButton = this.applyButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("a5fcb10b-c5f0-4be0-bc63-6109a5c4b0f8", "Disclaim Applicable PGAs");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 466, true);
			this.Controls.Add(this.disclaimApplicablePGAsUserControl1);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.LVS.GUI.DisclaimApplicablePGAsApplicator);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 504, true);
			this.Name = "DisclaimApplicablePGAsChildForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.disclaimApplicablePGAsUserControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.disclaimApplicablePGAsUserControl1.ResumeLayout(true);
			this.disclaimApplicablePGAsUserControl1.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DisclaimApplicablePGAsUserControl disclaimApplicablePGAsUserControl1;
		private Enterprise.ZArchitecture.GUI.ZButton applyButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
	}
}
