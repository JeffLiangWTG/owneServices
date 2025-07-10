namespace Enterprise.Customs.NO.GUI
{
	partial class CompanyCredentialsDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.CompanyCredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.UserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CurrentPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ExpiryDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CompanyCredentialsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.GlbCompanyWrapper);
			// 
			// CompanyCredentialsGroupBox
			//
			this.CompanyCredentialsGroupBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("EC378318-15D4-44C0-A770-0B1A98183FFA", "Digital Customs Exchange Details");
			this.CompanyCredentialsGroupBox.Controls.Add(this.CertificateLoaderUserControl);
			this.CompanyCredentialsGroupBox.Controls.Add(this.UserIDTextBox);
            this.CompanyCredentialsGroupBox.Controls.Add(this.CurrentPasswordTextBox);
            this.CompanyCredentialsGroupBox.Controls.Add(this.StatusTextBox);
            this.CompanyCredentialsGroupBox.Controls.Add(this.ExpiryDateTextBox);
            this.CompanyCredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.CompanyCredentialsGroupBox.Name = "CompanyCredentialsGroupBox";
            this.CompanyCredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 162, true);
            this.CompanyCredentialsGroupBox.TabIndex = 0;
            this.CompanyCredentialsGroupBox.TabStop = false;
			//
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "Credential.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.GlbCompanyWrapper)(null)).Credential.GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "p12 certificates (*.p12)|*.p12";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 20, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// UserIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserIDTextBox, "Credential.GP_UserID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.GlbCompanyWrapper)(null)).Credential.GP_UserID)));
            this.UserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 50, true);
            this.UserIDTextBox.Name = "UserIDTextBox";
            this.UserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 15, true);
            this.UserIDTextBox.TabIndex = 1;
            // 
            // CurrentPasswordTextBox
            // 
            this.BindingSource.SetBindingMember(this.CurrentPasswordTextBox, "Credential.CurrentDecryptedCertificatePassphrase");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.GlbCompanyWrapper)(null)).Credential.CurrentDecryptedCertificatePassphrase)));
            this.CurrentPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CurrentPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 70, true);
            this.CurrentPasswordTextBox.Name = "CurrentPasswordTextBox";
            this.CurrentPasswordTextBox.PasswordChar = '*';
            this.CurrentPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 15, true);
            this.CurrentPasswordTextBox.TabIndex = 2;
            // 
            // StatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.StatusTextBox, "Credential.GP_PasswordStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.GlbCompanyWrapper)(null)).Credential.GP_PasswordStatus)));
            this.StatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 90, true);
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 15, true);
            this.StatusTextBox.TabIndex = 3;
            // 
            // ExpiryDateTextBox
            // 
            this.BindingSource.SetBindingMember(this.ExpiryDateTextBox, "Credential.GP_ExpiryDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.NO.Business.GlbCompanyWrapper)(null)).Credential.GP_ExpiryDate)));
            this.ExpiryDateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ExpiryDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 110, true);
            this.ExpiryDateTextBox.Name = "ExpiryDateTextBox";
            this.ExpiryDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 15, true);
            this.ExpiryDateTextBox.TabIndex = 4;
			// 
			// CompanyCredentialsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.CompanyCredentialsGroupBox);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 100, true);
            this.Name = "CompanyCredentialsDetailsUserControl";
            this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 3, 3, true);
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 182, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CompanyCredentialsGroupBox.ResumeLayout(false);
            this.CompanyCredentialsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox CompanyCredentialsGroupBox;
		internal Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		internal ZArchitecture.ZTextBox StatusTextBox;
		internal ZArchitecture.ZTextBox CurrentPasswordTextBox;
		internal ZArchitecture.ZTextBox UserIDTextBox;
		internal ZArchitecture.ZTextBox ExpiryDateTextBox;
	}
}
