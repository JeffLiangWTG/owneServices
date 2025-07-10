
namespace Enterprise.MasterFiles.GUI
{
	partial class EmailSenderForm
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
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmailSenderForm));
			this.eDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SaveToEDocsDocType = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CommunicationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommunicationToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ToolStripEMailButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ToolStripEMailViaMailClientButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelSendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EMailViaMailClientLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HtmlEmailUserControl = new Enterprise.MasterFiles.GUI.HtmlEmailUserControl();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.eDocsGroupBox.SuspendLayout();
			this.CommunicationGroupBox.SuspendLayout();
			this.CommunicationToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 638, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EmailSenderConfiguration);
			// 
			// eDocsGroupBox
			// 
			this.eDocsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.eDocsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SenderForm|004a3cc2-b03d-488e-b880-05c6c59edbb2", "Save to eDocs");
			this.eDocsGroupBox.Controls.Add(this.SaveToEDocsDocType);
			this.eDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 3, true);
			this.eDocsGroupBox.Name = "eDocsGroupBox";
			this.eDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 49, true);
			this.eDocsGroupBox.TabIndex = 1;
			this.eDocsGroupBox.TabStop = false;
			// 
			// SaveToEDocsDocType
			// 
			this.SaveToEDocsDocType.AllowDrop = true;
			this.SaveToEDocsDocType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SaveToEDocsDocType, "SaveToEDocsDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EmailSenderConfiguration)(null)).SaveToEDocsDocumentType)));
			this.SaveToEDocsDocType.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SenderForm|47a7edb0-854b-43a8-96d3-df4b166b40fe", "Document Type");
			this.SaveToEDocsDocType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 19, true);
			this.SaveToEDocsDocType.Name = "SaveToEDocsDocType";
			this.SaveToEDocsDocType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.SaveToEDocsDocType.TabIndex = 1;
			// 
			// CommunicationGroupBox
			// 
			this.CommunicationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommunicationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SenderForm|fa56f665-aef0-4067-9abc-f3ec4a0c6916", "How do you want to send this message?");
			this.CommunicationGroupBox.Controls.Add(this.CommunicationToolStrip);
			this.CommunicationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 3, true);
			this.CommunicationGroupBox.Name = "CommunicationGroupBox";
			this.CommunicationGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(11, 5, 11, 10, true);
			this.CommunicationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 49, true);
			this.CommunicationGroupBox.TabIndex = 0;
			this.CommunicationGroupBox.TabStop = false;
			// 
			// CommunicationToolStrip
			// 
			this.CommunicationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.CommunicationToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.CommunicationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripEMailButton,
            this.ToolStripEMailViaMailClientButton});
			this.CommunicationToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 18, true);
			this.CommunicationToolStrip.Name = "CommunicationToolStrip";
			this.CommunicationToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.CommunicationToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 25, true);
			this.CommunicationToolStrip.TabIndex = 0;
			this.CommunicationToolStrip.Text = "toolStrip1";
			// 
			// ToolStripEMailButton
			// 
			this.ToolStripEMailButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DB9D1AFA-B7F9-4B58-B41A-B9DB8616A3F1", "Create E-mail");
			this.ToolStripEMailButton.CheckOnClick = true;
			this.ToolStripEMailButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolStripEMailButton.Image")));
			this.ToolStripEMailButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ToolStripEMailButton.Name = "ToolStripEMailButton";
			this.ToolStripEMailButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.ToolStripEMailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 22, true);
			this.ToolStripEMailButton.Tag = Enterprise.MasterFiles.Business.MessageDeliveryMethod.EMail;
			this.ToolStripEMailButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ToolStripEMailButton.CheckStateChanged += new System.EventHandler(this.ToolStripButton_CheckStateChanged);
			// 
			// ToolStripEMailViaMailClientButton
			// 
			this.ToolStripEMailViaMailClientButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("FFBA315F-6E3A-47C3-849D-47B4BF5E60CA", "Create E-mail (via Outlook etc.)");
			this.ToolStripEMailViaMailClientButton.CheckOnClick = true;
			this.ToolStripEMailViaMailClientButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolStripEMailViaMailClientButton.Image")));
			this.ToolStripEMailViaMailClientButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ToolStripEMailViaMailClientButton.Name = "ToolStripEMailViaMailClientButton";
			this.ToolStripEMailViaMailClientButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.ToolStripEMailViaMailClientButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 22, true);
			this.ToolStripEMailViaMailClientButton.Tag = Enterprise.MasterFiles.Business.MessageDeliveryMethod.EMailViaMailClient;
			this.ToolStripEMailViaMailClientButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ToolStripEMailViaMailClientButton.CheckStateChanged += new System.EventHandler(this.ToolStripButton_CheckStateChanged);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SenderForm|SendButtonTextSendEmail", "Send E-mail");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 609, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.SendButton.TabIndex = 6;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelSendButton
			// 
			this.CancelSendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelSendButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SenderForm|49e66605-1c73-438f-add9-f3e8eb83ae4b", "Cancel");
			this.CancelSendButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelSendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 609, true);
			this.CancelSendButton.Name = "CancelSendButton";
			this.CancelSendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelSendButton.TabIndex = 7;
			this.CancelSendButton.UseVisualStyleBackColor = true;
			this.CancelSendButton.Click += new System.EventHandler(this.CancelSendButton_Click);
			// 
			// EMailViaMailClientLabel
			// 
			this.EMailViaMailClientLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EMailViaMailClientLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 217, true);
			this.EMailViaMailClientLabel.Name = "EMailViaMailClientLabel";
			this.EMailViaMailClientLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(30, true);
			this.EMailViaMailClientLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 358, true);
			this.EMailViaMailClientLabel.TabIndex = 0;
			this.EMailViaMailClientLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.EMailViaMailClientLabel.Visible = false;
			// 
			// HtmlEmailUserControl
			// 
			this.HtmlEmailUserControl.AllowDrop = true;
			this.HtmlEmailUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HtmlEmailUserControl, "HtmlEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(((Enterprise.MasterFiles.Business.EmailSenderConfiguration)(null)).HtmlEmail)));
			this.HtmlEmailUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 58, true);
			this.HtmlEmailUserControl.Name = "HtmlEmailUserControl";
			this.HtmlEmailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 517, true);
			this.HtmlEmailUserControl.TabIndex = 3;
			// 
			// PreviewButton
			// 
			this.PreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SenderForm|205aefe0-d2d4-4ba7-bbcc-b6cdcd09cda0", "Preview E-mail");
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 609, true);
			this.PreviewButton.Name = "PreviewButton";
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.PreviewButton.TabIndex = 5;
			this.PreviewButton.UseVisualStyleBackColor = true;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// EmailSenderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelSendButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SenderForm|880ce195-a4a7-431f-a7cb-012923224101", "Send E-mail");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 662, true);
			this.Controls.Add(this.EMailViaMailClientLabel);
			this.Controls.Add(this.HtmlEmailUserControl);
			this.Controls.Add(this.eDocsGroupBox);
			this.Controls.Add(this.CommunicationGroupBox);
			this.Controls.Add(this.PreviewButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.CancelSendButton);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.EmailSenderConfiguration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 693, true);
			this.Name = "SenderForm";
			this.Controls.SetChildIndex(this.CancelSendButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.PreviewButton, 0);
			this.Controls.SetChildIndex(this.CommunicationGroupBox, 0);
			this.Controls.SetChildIndex(this.eDocsGroupBox, 0);
			this.Controls.SetChildIndex(this.HtmlEmailUserControl, 0);
			this.Controls.SetChildIndex(this.EMailViaMailClientLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.eDocsGroupBox.ResumeLayout(false);
			this.CommunicationGroupBox.ResumeLayout(false);
			this.CommunicationGroupBox.PerformLayout();
			this.CommunicationToolStrip.ResumeLayout(false);
			this.CommunicationToolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripEMailButton;
		private Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripEMailViaMailClientButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelSendButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CommunicationGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox eDocsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox SaveToEDocsDocType;
		protected ZArchitecture.GUI.ZToolStrip CommunicationToolStrip;
		private ZArchitecture.ZLabel EMailViaMailClientLabel;
		private HtmlEmailUserControl HtmlEmailUserControl;
		private ZArchitecture.GUI.ZButton PreviewButton;
	}
}
