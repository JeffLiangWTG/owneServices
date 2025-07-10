namespace Enterprise.Customs.US.GUI
{
	partial class ReconMessageSendingForm
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
			this.CertificationSignaturePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificationSignatureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PaymentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PaidDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageContextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageContentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageOptionGroupBox.SuspendLayout();
			this.CertificationSignaturePanel.SuspendLayout();
			this.PaymentPanel.SuspendLayout();
			this.PaidDropEdit.SuspendLayout();
			this.MessageContextGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 429, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.MessageBuilders.ACEReconMessageSendingAction);
			// 
			// MessageOptionGroupBox
			// 
			this.MessageOptionGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("772b1bc8-da77-410c-979d-6f9d9e43d102", "Message Option");
			this.MessageOptionGroupBox.Controls.Add(this.PaymentPanel);
			this.MessageOptionGroupBox.Controls.Add(this.CertificationSignaturePanel);
			this.MessageOptionGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageOptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageOptionGroupBox.Name = "MessageOptionGroupBox";
			this.MessageOptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 69, true);
			this.MessageOptionGroupBox.TabIndex = 1;
			this.MessageOptionGroupBox.TabStop = false;
			this.MessageOptionGroupBox.Text = "Message Option";
			// 
			// CertificationSignaturePanel
			// 
			this.CertificationSignaturePanel.Controls.Add(this.CertificationSignatureCheckBox);
			this.CertificationSignaturePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CertificationSignaturePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CertificationSignaturePanel.Name = "CertificationSignaturePanel";
			this.CertificationSignaturePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 23, true);
			this.CertificationSignaturePanel.TabIndex = 16;
			// 
			// CertificationSignatureCheckBox
			// 
			this.CertificationSignatureCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CertificationSignatureCheckBox, "CertificationSignature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.MessageBuilders.ACEReconMessageSendingAction)(null)).CertificationSignature)));
			this.CertificationSignatureCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ae078934-8f7c-4e22-adc0-dd2d95b0bbf3", "Acknowledge and Sign");
			this.CertificationSignatureCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CertificationSignatureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CertificationSignatureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.CertificationSignatureCheckBox.Name = "CertificationSignatureCheckBox";
			this.CertificationSignatureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.CertificationSignatureCheckBox.TabIndex = 17;
			this.CertificationSignatureCheckBox.Text = "Acknowledge and Sign";
			this.CertificationSignatureCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CertificationSignatureCheckBox.UseVisualStyleBackColor = true;
			// 
			// PaymentPanel
			// 
			this.PaymentPanel.Controls.Add(this.PaidDropEdit);
			this.PaymentPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PaymentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 38, true);
			this.PaymentPanel.Name = "PaymentPanel";
			this.PaymentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 23, true);
			this.PaymentPanel.TabIndex = 18;
			this.PaymentPanel.TabStop = true;
			// 
			// PaidDropEdit
			// 
			this.PaidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaidDropEdit, "PaymentFinalized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MessageBuilders.ACEReconMessageSendingAction)(null)).PaymentFinalized)));
			this.PaidDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("819ABFB3-E0D4-4D37-85FF-90821AED5AC5", "Suppress Payment Info");
			this.PaidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 3, true);
			this.PaidDropEdit.Name = "PaidDropEdit";
			this.PaidDropEdit.PreBoundMaxLength = 1;
			this.PaidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.PaidDropEdit.TabIndex = 18;
			// 
			// MessageContextGroupBox
			// 
			this.MessageContextGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b09caf18-23e3-4251-ba0d-49de57bc5176", "Message Details");
			this.MessageContextGroupBox.Controls.Add(this.MessageContentsTextBox);
			this.MessageContextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.MessageContextGroupBox.Name = "MessageContextGroupBox";
			this.MessageContextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 335, true);
			this.MessageContextGroupBox.TabIndex = 2;
			this.MessageContextGroupBox.TabStop = false;
			this.MessageContextGroupBox.Text = "Message Details";
			// 
			// MessageContentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageContentsTextBox, "US_MessageContents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.ACEReconMessageSendingAction)(null)).US_MessageContents)));
			this.MessageContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContentsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageContentsTextBox, false);
			this.MessageContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.MessageContentsTextBox.Multiline = true;
			this.MessageContentsTextBox.Name = "MessageContentsTextBox";
			this.MessageContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 318, true);
			this.MessageContentsTextBox.TabIndex = 1;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 0, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 5;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 0, true);
			this.OKButton.Name = "OKButton";
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
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 404, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 25, true);
			this.BottomPanel.TabIndex = 6;
			// 
			// ReconMessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 453, true);
			this.Controls.Add(this.MessageContextGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.MessageOptionGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.MessageBuilders.ACEReconMessageSendingAction);
			this.Name = "ReconMessageSendingForm";
			this.Text = "Send Messages";
			this.Controls.SetChildIndex(this.MessageOptionGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MessageContextGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageOptionGroupBox.ResumeLayout(false);
			this.MessageOptionGroupBox.PerformLayout();
			this.CertificationSignaturePanel.ResumeLayout(false);
			this.CertificationSignaturePanel.PerformLayout();
			this.PaymentPanel.ResumeLayout(false);
			this.PaymentPanel.PerformLayout();
			this.PaidDropEdit.ResumeLayout(false);
			this.PaidDropEdit.PerformLayout();
			this.MessageContextGroupBox.ResumeLayout(false);
			this.MessageContextGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MessageOptionGroupBox;
		private ZArchitecture.GUI.ZPanel CertificationSignaturePanel;
		private ZArchitecture.GUI.ZCheckBox CertificationSignatureCheckBox;
		internal ZArchitecture.GUI.ZPanel PaymentPanel;
		private ZArchitecture.GUI.ZDropEdit PaidDropEdit;
		private ZArchitecture.GUI.ZGroupBox MessageContextGroupBox;
		private ZArchitecture.ZTextBox MessageContentsTextBox;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZPanel BottomPanel;
	}
}
