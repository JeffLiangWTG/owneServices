namespace Enterprise.Customs.US.GUI
{
	partial class PGACorrectionSendingForm
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.MessageOptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PGACorrectionTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PGACorrectionUS_SendCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MessageContextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageContentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageOptionGroupBox.SuspendLayout();
			this.PGACorrectionTopPanel.SuspendLayout();
			this.MessageContextGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 537, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.PGACorrectionMessageSendingAction);
			// 
			// MessageOptionGroupBox
			// 
			this.MessageOptionGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("772b1bc8-da77-410c-979d-6f9d9e43d102", "Message Option");
			this.MessageOptionGroupBox.Controls.Add(this.PGACorrectionTopPanel);
			this.MessageOptionGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageOptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageOptionGroupBox.Name = "MessageOptionGroupBox";
			this.MessageOptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 52, true);
			this.MessageOptionGroupBox.TabIndex = 1;
			this.MessageOptionGroupBox.TabStop = false;
			this.MessageOptionGroupBox.Text = "Message Option";
			// 
			// PGACorrectionTopPanel
			// 
			this.PGACorrectionTopPanel.Controls.Add(this.PGACorrectionUS_SendCheckBox);
			this.PGACorrectionTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PGACorrectionTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PGACorrectionTopPanel.Name = "PGACorrectionTopPanel";
			this.PGACorrectionTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 23, true);
			this.PGACorrectionTopPanel.TabIndex = 19;
			// 
			// PGACorrectionUS_SendCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PGACorrectionUS_SendCheckBox, "US_SendMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.PGACorrectionMessageSendingAction)(null)).US_SendMessage)));
			this.PGACorrectionUS_SendCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("13799168-478e-4398-9b9a-7bb3588d1d1f", "Send?");
			this.PGACorrectionUS_SendCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PGACorrectionUS_SendCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PGACorrectionUS_SendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.PGACorrectionUS_SendCheckBox.Name = "PGACorrectionUS_SendCheckBox";
			this.PGACorrectionUS_SendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.PGACorrectionUS_SendCheckBox.TabIndex = 0;
			this.PGACorrectionUS_SendCheckBox.Text = "Send Message?";
			this.PGACorrectionUS_SendCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PGACorrectionUS_SendCheckBox.UseVisualStyleBackColor = true;
			// 
			// MessageContextGroupBox
			// 
			this.MessageContextGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bf20a08e-51fd-4905-b35c-9db642d5cba5", "Message Context");
			this.MessageContextGroupBox.Controls.Add(this.MessageContentsTextBox);
			this.MessageContextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.MessageContextGroupBox.Name = "MessageContextGroupBox";
			this.MessageContextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 460, true);
			this.MessageContextGroupBox.TabIndex = 2;
			this.MessageContextGroupBox.TabStop = false;
			this.MessageContextGroupBox.Text = "Message Context";
			// 
			// MessageContentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageContentsTextBox, "US_MessageContents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.PGACorrectionMessageSendingAction)(null)).US_MessageContents)));
			this.MessageContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContentsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageContentsTextBox, false);
			this.MessageContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageContentsTextBox.Multiline = true;
			this.MessageContentsTextBox.Name = "MessageContentsTextBox";
			this.MessageContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 441, true);
			this.MessageContentsTextBox.TabIndex = 1;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(524, 0, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 5;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 0, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Controls.Add(this.CancelButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 512, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 25, true);
			this.BottomPanel.TabIndex = 6;
			// 
			// PGACorrectionSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 561, true);
			this.Controls.Add(this.MessageContextGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.MessageOptionGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.PGACorrectionMessageSendingAction);
			this.Name = "PGACorrectionSendingForm";
			this.Text = "Send \'PGA Correction\' Messages";
			this.Controls.SetChildIndex(this.MessageOptionGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MessageContextGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageOptionGroupBox.ResumeLayout(false);
			this.MessageOptionGroupBox.PerformLayout();
			this.PGACorrectionTopPanel.ResumeLayout(false);
			this.PGACorrectionTopPanel.PerformLayout();
			this.MessageContextGroupBox.ResumeLayout(false);
			this.MessageContextGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MessageOptionGroupBox;
		private ZArchitecture.GUI.ZPanel PGACorrectionTopPanel;
		private ZArchitecture.GUI.ZCheckBox PGACorrectionUS_SendCheckBox;
		private ZArchitecture.GUI.ZGroupBox MessageContextGroupBox;
		private ZArchitecture.ZTextBox MessageContentsTextBox;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZPanel BottomPanel;
	}
}