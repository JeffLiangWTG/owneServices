namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class LicensingCertificateUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BrokeragePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LicensingCertificateGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceiveAutomaticallyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReceiveAutomaticallyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.StatusReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SenderIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MailBoxPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.MailBoxTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BrokeragePanel.SuspendLayout();
			this.LicensingCertificateGroupbox.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper);
			// 
			// BrokeragePanel
			// 
			this.BrokeragePanel.Controls.Add(this.LicensingCertificateGroupbox);
			this.BrokeragePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrokeragePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BrokeragePanel.Name = "BrokeragePanel";
			this.BrokeragePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 267, true);
			this.BrokeragePanel.TabIndex = 6;
			// 
			// LicensingCertificateGroupbox
			// 
			this.LicensingCertificateGroupbox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("f8c66f88-e201-4bfd-9e26-4cdd6043ed1f", "Licensing Certificate");
			this.LicensingCertificateGroupbox.Controls.Add(this.CertificateStatusTextBox);
			this.LicensingCertificateGroupbox.Controls.Add(this.ReceiveAutomaticallyLabel);
			this.LicensingCertificateGroupbox.Controls.Add(this.ReceiveAutomaticallyCheckBox);
			this.LicensingCertificateGroupbox.Controls.Add(this.StatusReasonTextBox);
			this.LicensingCertificateGroupbox.Controls.Add(this.StatusTextBox);
			this.LicensingCertificateGroupbox.Controls.Add(this.CertificatePasswordTextBox);
			this.LicensingCertificateGroupbox.Controls.Add(this.SenderIDTextBox);
			this.LicensingCertificateGroupbox.Controls.Add(this.MailBoxPasswordTextBox);
			this.LicensingCertificateGroupbox.Controls.Add(this.CertificateLoaderUserControl);
			this.LicensingCertificateGroupbox.Controls.Add(this.MailBoxTextBox);
			this.LicensingCertificateGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicensingCertificateGroupbox.Name = "LicensingCertificateGroupbox";
			this.LicensingCertificateGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 267, true);
			this.LicensingCertificateGroupbox.TabIndex = 1;
			this.LicensingCertificateGroupbox.TabStop = false;
			// 
			// CertificateStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateStatusTextBox, "LicensingCertificate.CertificateStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.CertificateStatus)));
			this.CertificateStatusTextBox.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("4cfdf7bd-d17a-4363-a439-cd41db784ea6", "Certificate File");
			this.CertificateStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 209, true);
			this.CertificateStatusTextBox.Name = "CertificateStatusTextBox";
			this.CertificateStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.CertificateStatusTextBox.TabIndex = 23;
			// 
			// ReceiveAutomaticallyLabel
			// 
			this.ReceiveAutomaticallyLabel.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("314f7a05-a994-4a74-85ce-ac31716fd87d", "Receive Automatically");
			this.ReceiveAutomaticallyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ReceiveAutomaticallyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 179, true);
			this.ReceiveAutomaticallyLabel.Name = "ReceiveAutomaticallyLabel";
			this.ReceiveAutomaticallyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 23, true);
			this.ReceiveAutomaticallyLabel.TabIndex = 22;
			this.ReceiveAutomaticallyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ReceiveAutomaticallyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReceiveAutomaticallyCheckBox, "LicensingCertificate.GP_ReceiveAutomatically");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.GP_ReceiveAutomatically)));
			this.ReceiveAutomaticallyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 179, true);
			this.ReceiveAutomaticallyCheckBox.Name = "ReceiveAutomaticallyCheckBox";
			this.ReceiveAutomaticallyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 24, true);
			this.ReceiveAutomaticallyCheckBox.TabIndex = 21;
			this.ReceiveAutomaticallyCheckBox.UseVisualStyleBackColor = true;
			// 
			// StatusReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusReasonTextBox, "LicensingCertificate.GP_StatusReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.GP_StatusReason)));
			this.StatusReasonTextBox.CaptionResourceString = null;
			this.StatusReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 153, true);
			this.StatusReasonTextBox.Name = "StatusReasonTextBox";
			this.StatusReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.StatusReasonTextBox.TabIndex = 20;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "LicensingCertificate.GP_PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.GP_PasswordStatus)));
			this.StatusTextBox.CaptionResourceString = null;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 126, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.StatusTextBox.TabIndex = 19;
			// 
			// CertificatePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificatePasswordTextBox, "LicensingCertificate.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.CurrentDecryptedCertificatePassphrase)));
			this.CertificatePasswordTextBox.CaptionResourceString = null;
			this.CertificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 99, true);
			this.CertificatePasswordTextBox.Name = "CertificatePasswordTextBox";
			this.CertificatePasswordTextBox.PasswordChar = '*';
			this.CertificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.CertificatePasswordTextBox.TabIndex = 18;
			// 
			// SenderIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.SenderIDTextBox, "LicensingCertificate.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.GP_UserID)));
			this.SenderIDTextBox.CaptionResourceString = null;
			this.SenderIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 72, true);
			this.SenderIDTextBox.Name = "SenderIDTextBox";
			this.SenderIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.SenderIDTextBox.TabIndex = 17;
			// 
			// MailBoxPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.MailBoxPasswordTextBox, "LicensingCertificate.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.CurrentDecryptedPassword)));
			this.MailBoxPasswordTextBox.CaptionResourceString = null;
			this.MailBoxPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MailBoxPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 46, true);
			this.MailBoxPasswordTextBox.Name = "MailBoxPasswordTextBox";
			this.MailBoxPasswordTextBox.PasswordChar = '*';
			this.MailBoxPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.MailBoxPasswordTextBox.TabIndex = 16;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "LicensingCertificate.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 235, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// MailBoxTextBox
			// 
			this.BindingSource.SetBindingMember(this.MailBoxTextBox, "LicensingCertificate.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper)(null)).LicensingCertificate.GP_MailBoxID)));
			this.MailBoxTextBox.CaptionResourceString = null;
			this.MailBoxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 19, true);
			this.MailBoxTextBox.Name = "MailBoxTextBox";
			this.MailBoxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.MailBoxTextBox.TabIndex = 15;
			// 
			// LicensingCertificateUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BrokeragePanel);
			this.Name = "LicensingCertificateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BrokeragePanel.ResumeLayout(false);
			this.BrokeragePanel.PerformLayout();
			this.LicensingCertificateGroupbox.ResumeLayout(false);
			this.LicensingCertificateGroupbox.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BrokeragePanel;
		private ZArchitecture.GUI.ZGroupBox LicensingCertificateGroupbox;
		public Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		private ZArchitecture.GUI.ZCheckBox ReceiveAutomaticallyCheckBox;
		private ZArchitecture.ZTextBox StatusReasonTextBox;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.ZTextBox CertificatePasswordTextBox;
		private ZArchitecture.ZTextBox SenderIDTextBox;
		private ZArchitecture.ZTextBox MailBoxPasswordTextBox;
		private ZArchitecture.ZTextBox MailBoxTextBox;
		private ZArchitecture.ZLabel ReceiveAutomaticallyLabel;
		private ZArchitecture.ZTextBox CertificateStatusTextBox;
	}
}
