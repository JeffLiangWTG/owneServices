namespace Enterprise.Customs.PL.GUI
{
	partial class StaffCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CompaniesGroupPL = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PLCertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SeapIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PUESCPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PUESCLoginTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.CertForLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommunicationChannelEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PLCredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompaniesGroupPL.SuspendLayout();
			this.PLCertificatePanel.SuspendLayout();
			this.PasswordStatusDropEdit.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.PLCredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.GlbStaffWrapper);
			// 
			// CompaniesGroupPL
			//
			this.CompaniesGroupPL.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLStaffCredentialsUserControl|D5A2E59C-CEC1-4E57-AA12-08FBFFE90E8B", "Subscriptions Management");
			this.CompaniesGroupPL.Controls.Add(this.PLCertificatePanel);
			this.CompaniesGroupPL.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompaniesGroupPL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompaniesGroupPL.Name = "CompaniesGroupPL";
			this.CompaniesGroupPL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 434, true);
			this.CompaniesGroupPL.TabIndex = 1;
			this.CompaniesGroupPL.TabStop = false;
			// 
			// PLCertificatePanel
			// 
			this.PLCertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PLCertificatePanel.Controls.Add(this.SeapIdTextBox);
			this.PLCertificatePanel.Controls.Add(this.PUESCPasswordTextBox);
			this.PLCertificatePanel.Controls.Add(this.PUESCLoginTextBox);
			this.PLCertificatePanel.Controls.Add(this.CertificatePasswordTextBox);
			this.PLCertificatePanel.Controls.Add(this.PasswordStatusDropEdit);
			this.PLCertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.PLCertificatePanel.Controls.Add(this.CertForLabel);
			this.PLCertificatePanel.Controls.Add(this.CommunicationChannelEmailTextBox);
			this.PLCertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 17, true);
			this.PLCertificatePanel.Name = "PLCertificatePanel";
			this.PLCertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(58, 7, 58, 7, true);
			this.PLCertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 179, true);
			this.PLCertificatePanel.TabIndex = 14;
			// 
			// SeapIdTextBox
			// 
			this.SeapIdTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SeapIdTextBox, "SeapId.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.GlbStaffWrapper)(null)).SeapId.GP_UserID)));
			this.SeapIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SeapIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 111, true);
			this.SeapIdTextBox.Name = "SeapIdTextBox";
			this.SeapIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.SeapIdTextBox.TabIndex = 5;
			// 
			// PUESCPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PUESCPasswordTextBox, "GlbExternalPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.GlbStaffWrapper)(null)).GlbExternalPassword.CurrentDecryptedPassword)));
			this.PUESCPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PUESCPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 10, true);
			this.PUESCPasswordTextBox.Name = "PUESCPasswordTextBox";
			this.PUESCPasswordTextBox.PasswordChar = '*';
			this.PUESCPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.PUESCPasswordTextBox.TabIndex = 1;
			// 
			// PUESCLoginTextBox
			// 
			this.BindingSource.SetBindingMember(this.PUESCLoginTextBox, "GlbExternalPassword.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.GlbStaffWrapper)(null)).GlbExternalPassword.GP_MailBoxID)));
			this.PUESCLoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 10, true);
			this.PUESCLoginTextBox.Name = "PUESCLoginTextBox";
			this.PUESCLoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.PUESCLoginTextBox.TabIndex = 0;
			// 
			// CertificatePasswordTextBox
			// 
			this.CertificatePasswordTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CertificatePasswordTextBox, "GlbExternalPassword.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.GlbStaffWrapper)(null)).GlbExternalPassword.CurrentDecryptedCertificatePassphrase)));
			this.CertificatePasswordTextBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("15069FC6-2E13-40AB-B737-544CD14C473A", "Certificate Password");
			this.CertificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 62, true);
			this.CertificatePasswordTextBox.Name = "CertificatePasswordTextBox";
			this.CertificatePasswordTextBox.PasswordChar = '*';
			this.CertificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.CertificatePasswordTextBox.TabIndex = 3;
			// 
			// PasswordStatusDropEdit
			// 
			this.PasswordStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PasswordStatusDropEdit, "GlbExternalPassword.GP_PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.GlbStaffWrapper)(null)).GlbExternalPassword.GP_PasswordStatus)));
			this.PasswordStatusDropEdit.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("44D62333-4D74-4B3C-A3D7-CA16F703D699", "Password Status");
			this.PasswordStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 86, true);
			this.PasswordStatusDropEdit.Name = "PasswordStatusDropEdit";
			this.PasswordStatusDropEdit.PreBoundMaxLength = 3;
			this.PasswordStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.PasswordStatusDropEdit.TabIndex = 4;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "GlbExternalPassword.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.GlbStaffWrapper)(null)).GlbExternalPassword.GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 32, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 2;
			// 
			// CertForLabel
			// 
			this.CertForLabel.AutoSize = true;
			this.CertForLabel.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("9260A948-B363-48C1-841B-E8A8392A5905", "", "PLB Certificate");
			this.CertForLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CertForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 38, true);
			this.CertForLabel.Name = "CertForLabel";
			this.CertForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.CertForLabel.TabIndex = 0;
			this.CertForLabel.UseMnemonic = false;
			// 
			// CommunicationChannelEmailTextBox
			// 
			this.CommunicationChannelEmailTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CommunicationChannelEmailTextBox, "CommunicationChannel.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.GlbStaffWrapper)(null)).CommunicationChannel.GP_MailBoxID)));
			this.CommunicationChannelEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommunicationChannelEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 135, true);
			this.CommunicationChannelEmailTextBox.Name = "CommunicationChannelEmailTextBox";
			this.CommunicationChannelEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.CommunicationChannelEmailTextBox.TabIndex = 6;
			// 
			// PLCredentialsPanel
			// 
			this.PLCredentialsPanel.Controls.Add(this.CompaniesGroupPL);
			this.PLCredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PLCredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.PLCredentialsPanel.Name = "PLCredentialsPanel";
			this.PLCredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 434, true);
			this.PLCredentialsPanel.TabIndex = 5;
			// 
			// StaffCredentialsUserControl
			// 
			this.Controls.Add(this.PLCredentialsPanel);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.PLCredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompaniesGroupPL.ResumeLayout(false);
			this.CompaniesGroupPL.PerformLayout();
			this.PLCertificatePanel.ResumeLayout(false);
			this.PLCertificatePanel.PerformLayout();
			this.PasswordStatusDropEdit.ResumeLayout(true);
			this.PasswordStatusDropEdit.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.PLCredentialsPanel.ResumeLayout(false);
			this.PLCredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel PLCertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel PLCredentialsPanel;
		Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		Enterprise.ZArchitecture.GUI.ZGroupBox CompaniesGroupPL;
		Enterprise.ZArchitecture.ZLabel CertForLabel;
		internal ZArchitecture.GUI.ZDropEdit PasswordStatusDropEdit;
		internal ZArchitecture.ZTextBox CertificatePasswordTextBox;
		internal ZArchitecture.ZTextBox PUESCLoginTextBox;
		internal ZArchitecture.ZTextBox PUESCPasswordTextBox;
		internal ZArchitecture.ZTextBox SeapIdTextBox;
		internal ZArchitecture.ZTextBox CommunicationChannelEmailTextBox;
	}
}
