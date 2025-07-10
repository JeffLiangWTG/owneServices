namespace Enterprise.Customs.US.GUI
{
	partial class ACEDrawbackMessageSendingForm
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
		private new  void InitializeComponent()
		{
			this.MessageOptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACEDrawbackSignPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PGACorrectionSignCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACEDrawbackTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DrawbackUS_SendCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MessageContextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageContentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageOptionGroupBox.SuspendLayout();
			this.ACEDrawbackSignPanel.SuspendLayout();
			this.ACEDrawbackTopPanel.SuspendLayout();
			this.MessageContextGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 590, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ACEDrawbackAcknowledgeAndSign);
			// 
			// MessageOptionGroupBox
			// 
			this.MessageOptionGroupBox.Controls.Add(this.ACEDrawbackSignPanel);
			this.MessageOptionGroupBox.Controls.Add(this.ACEDrawbackTopPanel);
			this.MessageOptionGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageOptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageOptionGroupBox.Name = "MessageOptionGroupBox";
			this.MessageOptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 67, true);
			this.MessageOptionGroupBox.TabIndex = 0;
			this.MessageOptionGroupBox.TabStop = false;
			this.MessageOptionGroupBox.Text = "Message Option";
			// 
			// ACEDrawbackSignPanel
			// 
			this.ACEDrawbackSignPanel.Controls.Add(this.PGACorrectionSignCheckBox);
			this.ACEDrawbackSignPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ACEDrawbackSignPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.ACEDrawbackSignPanel.Name = "ACEDrawbackSignPanel";
			this.ACEDrawbackSignPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 23, true);
			this.ACEDrawbackSignPanel.TabIndex = 1;
			this.ACEDrawbackSignPanel.TabStop = true;
			// 
			// PGACorrectionSignCheckBox
			// 
			this.PGACorrectionSignCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PGACorrectionSignCheckBox, "US_AcknowledgeAndSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ACEDrawbackAcknowledgeAndSign)(null)).US_AcknowledgeAndSign)));
			this.PGACorrectionSignCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PGACorrectionSignCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PGACorrectionSignCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 2, true);
			this.PGACorrectionSignCheckBox.Name = "PGACorrectionSignCheckBox";
			this.PGACorrectionSignCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.PGACorrectionSignCheckBox.TabIndex = 0;
			this.PGACorrectionSignCheckBox.Text = "Acknowledge and Sign";
			this.PGACorrectionSignCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PGACorrectionSignCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACEDrawbackTopPanel
			// 
			this.ACEDrawbackTopPanel.Controls.Add(this.DrawbackUS_SendCheckBox);
			this.ACEDrawbackTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ACEDrawbackTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ACEDrawbackTopPanel.Name = "ACEDrawbackTopPanel";
			this.ACEDrawbackTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 23, true);
			this.ACEDrawbackTopPanel.TabIndex = 0;
			// 
			// DrawbackUS_SendCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DrawbackUS_SendCheckBox, "US_SendMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ACEDrawbackAcknowledgeAndSign)(null)).US_SendMessage)));
			this.DrawbackUS_SendCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DrawbackUS_SendCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DrawbackUS_SendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.DrawbackUS_SendCheckBox.Name = "DrawbackUS_SendCheckBox";
			this.DrawbackUS_SendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.DrawbackUS_SendCheckBox.TabIndex = 0;
			this.DrawbackUS_SendCheckBox.Text = "Send Message?";
			this.DrawbackUS_SendCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DrawbackUS_SendCheckBox.UseVisualStyleBackColor = true;
			// 
			// MessageContextGroupBox
			// 
			this.MessageContextGroupBox.Controls.Add(this.MessageContentsTextBox);
			this.MessageContextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 67, true);
			this.MessageContextGroupBox.Name = "MessageContextGroupBox";
			this.MessageContextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 498, true);
			this.MessageContextGroupBox.TabIndex = 1;
			this.MessageContextGroupBox.TabStop = false;
			this.MessageContextGroupBox.Text = "Message Context";
			// 
			// MessageContentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageContentsTextBox, "US_MessageContents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEDrawbackAcknowledgeAndSign)(null)).US_MessageContents)));
			this.MessageContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContentsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageContentsTextBox, false);
			this.MessageContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageContentsTextBox.Multiline = true;
			this.MessageContentsTextBox.Name = "MessageContentsTextBox";
			this.MessageContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 479, true);
			this.MessageContentsTextBox.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Controls.Add(this.CancelButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 565, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 25, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 2, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 0;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 2, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 1;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ACEDrawbackMessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 614, true);
			this.Controls.Add(this.MessageContextGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.MessageOptionGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.ACEDrawbackAcknowledgeAndSign);
			this.Name = "ACEDrawbackMessageSendingForm";
			this.Text = "Send ACE Drawback Message";
			this.Controls.SetChildIndex(this.MessageOptionGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MessageContextGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageOptionGroupBox.ResumeLayout(false);
			this.MessageOptionGroupBox.PerformLayout();
			this.ACEDrawbackSignPanel.ResumeLayout(false);
			this.ACEDrawbackSignPanel.PerformLayout();
			this.ACEDrawbackTopPanel.ResumeLayout(false);
			this.ACEDrawbackTopPanel.PerformLayout();
			this.MessageContextGroupBox.ResumeLayout(false);
			this.MessageContextGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MessageOptionGroupBox;
		private ZArchitecture.GUI.ZPanel ACEDrawbackSignPanel;
		private ZArchitecture.GUI.ZCheckBox PGACorrectionSignCheckBox;
		private ZArchitecture.GUI.ZPanel ACEDrawbackTopPanel;
		private ZArchitecture.GUI.ZCheckBox DrawbackUS_SendCheckBox;
		private ZArchitecture.GUI.ZGroupBox MessageContextGroupBox;
		private ZArchitecture.ZTextBox MessageContentsTextBox;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton OKButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
	}
}