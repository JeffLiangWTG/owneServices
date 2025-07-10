namespace Enterprise.MasterData.GUI
{
	partial class PersonMergeSummaryForm
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
		protected new void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MergeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MergeWarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RetainedControls = new Enterprise.MasterData.GUI.PersonMergeSummaryRetainedUserControls();
			this.CandidatesControls = new Enterprise.MasterData.GUI.PersonMergeSummaryCandidatesUserControls();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RetainedControls.SuspendLayout();
			this.CandidatesControls.SuspendLayout();
			this.SuspendLayout();
			// 
			// MergeWarningLabel
			// 
			this.BindingSource.SetBindingMember(this.MergeWarningLabel, "MergingWarningMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).MergingWarningMessage)));
			this.MergeWarningLabel.AutoSize = true;
			this.MergeWarningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MergeWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 335, true);
			this.MergeWarningLabel.Name = "mergeWarningLabel";
			this.MergeWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 200, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 368, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 24, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("E952A2A8-E523-415C-BFBB-7291C7E10DE7", "Cancel");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 366, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 28, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MergeButton
			// 
			this.MergeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MergeButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("94993CCF-5A86-436E-97D5-1938447B1431", "Confirm Merge");
			this.MergeButton.IsCaptionOverridden = false;
			this.MergeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 366, true);
			this.MergeButton.Name = "MergeButton";
			this.MergeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MergeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 28, true);
			this.MergeButton.TabIndex = 3;
			this.MergeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.MergeButton.ToolTipCaption = null;
			this.MergeButton.UseVisualStyleBackColor = true;
			this.MergeButton.Click += new System.EventHandler(this.MergeButton_Click);
			// 
			// RetainedControls
			// 
			this.RetainedControls.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RetainedControls, ".");
			this.RetainedControls.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.RetainedControls.Name = "RetainedControls";
			this.RetainedControls.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 96, true);
			this.RetainedControls.TabIndex = 1;
			// 
			// CandidatesControls
			// 
			this.CandidatesControls.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CandidatesControls, ".");
			this.CandidatesControls.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 110, true);
			this.CandidatesControls.Name = "CandidatesControls";
			this.CandidatesControls.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 224, true);
			this.CandidatesControls.TabIndex = 2;
			// 
			// PersonMergeSummaryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 422, true);
			this.Controls.Add(this.CandidatesControls);
			this.Controls.Add(this.RetainedControls);
			this.Controls.Add(this.MergeWarningLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.MergeButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "PersonMergeSummaryForm";
			this.Text = "Person Merge Summary";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MergeWarningLabel, 0);
			this.Controls.SetChildIndex(this.MergeButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.RetainedControls, 0);
			this.Controls.SetChildIndex(this.CandidatesControls, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RetainedControls.ResumeLayout(true);
			this.RetainedControls.PerformLayout();
			this.CandidatesControls.ResumeLayout(true);
			this.CandidatesControls.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		protected ZArchitecture.GUI.ZButton CloseButton;
		protected ZArchitecture.GUI.ZButton MergeButton;
		internal ZArchitecture.ZLabel MergeWarningLabel;
		protected PersonMergeSummaryRetainedUserControls RetainedControls;
		protected PersonMergeSummaryCandidatesUserControls CandidatesControls;
	}
}