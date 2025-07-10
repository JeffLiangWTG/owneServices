namespace Enterprise.Customs.UY.Manifest.GUI
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
            this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.StatusReason = new Enterprise.ZArchitecture.ZTextBox();
            this.CertificatePassword = new Enterprise.ZArchitecture.ZTextBox();
            this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
            this.Status = new Enterprise.ZArchitecture.ZTextBox();
            this.UserID = new Enterprise.ZArchitecture.ZTextBox();
            this.UserPassword = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zGroupBox1.SuspendLayout();
            this.CertificateLoaderUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.UY.Manifest.Business.GlbCompanyWrapper);
            // 
            // zGroupBox1
            // 
            this.zGroupBox1.CaptionResourceString = Enterprise.Customs.UY.Manifest.GUI.Res.GetData("GlbCompanyForm|7AE87A8B-93AA-4F5D-A786-7913609B9C3F", "Company Credentials");
            this.zGroupBox1.Controls.Add(this.StatusReason);
            this.zGroupBox1.Controls.Add(this.CertificatePassword);
            this.zGroupBox1.Controls.Add(this.CertificateLoaderUserControl);
            this.zGroupBox1.Controls.Add(this.Status);
            this.zGroupBox1.Controls.Add(this.UserID);
            this.zGroupBox1.Controls.Add(this.UserPassword);
            this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
            this.zGroupBox1.Name = "zGroupBox1";
            this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 231, true);
            this.zGroupBox1.TabIndex = 0;
            this.zGroupBox1.TabStop = false;
			// 
			// UserID
			// 
			this.BindingSource.SetBindingMember(this.UserID, "GlbExternalPassword.GP_UserID");
			this.UserID.CaptionResourceString = Enterprise.Customs.UY.Manifest.GUI.Res.GetData("a934f7d2-4cb9-4872-9066-4d7dcfbec165", "Username");
			this.UserID.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 46, true);
			this.UserID.Name = "UserID";
			this.UserID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.UserID.TabIndex = 1;
			// 
			// UserPassword
			// 
			this.BindingSource.SetBindingMember(this.UserPassword, "GlbExternalPassword.CurrentDecryptedPassword");
			this.UserPassword.CaptionResourceString = Enterprise.Customs.UY.Manifest.GUI.Res.GetData("65f251b0-b982-4963-875b-f56ba4cf8a0b", "User Password");
			this.UserPassword.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserPassword.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 72, true);
			this.UserPassword.Name = "UserPassword";
			this.UserPassword.PasswordChar = '*';
			this.UserPassword.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.UserPassword.TabIndex = 2;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.CertificateLoaderUserControl.CaptionRenderingEnabled = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "GlbExternalPassword.GP_Certificate");
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 98, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 3;
			// 
			// CertificatePassword
			// 
			this.BindingSource.SetBindingMember(this.CertificatePassword, "GlbExternalPassword.CurrentDecryptedCertificatePassphrase");
            this.CertificatePassword.CaptionResourceString = Enterprise.Customs.UY.Manifest.GUI.Res.GetData("6cb0c8d0-06f3-432c-8ace-2f35d52b3138", "Certificate Password");
			this.CertificatePassword.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CertificatePassword.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 132, true);
            this.CertificatePassword.Name = "CertificatePassword";
            this.CertificatePassword.PasswordChar = '*';
            this.CertificatePassword.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
            this.CertificatePassword.TabIndex = 4;
            // 
            // Status
            // 
            this.BindingSource.SetBindingMember(this.Status, "GlbExternalPassword.PasswordStatus");
            this.Status.CaptionResourceString = Enterprise.Customs.UY.Manifest.GUI.Res.GetData("29d5b0ff-771f-428f-b589-f671ca659b13", "Status");
            this.Status.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.Status.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 158, true);
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
            this.Status.TabIndex = 5;
			// 
			// StatusReason
			// 
			this.BindingSource.SetBindingMember(this.StatusReason, "GlbExternalPassword.GP_StatusReason");
			this.StatusReason.CaptionResourceString = Enterprise.Customs.UY.Manifest.GUI.Res.GetData("dde15807-4797-41b5-bc98-c58db388fdb1", "Status Reason");
			this.StatusReason.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 184, true);
			this.StatusReason.Name = "StatusReason";
			this.StatusReason.ReadOnly = true;
			this.StatusReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.StatusReason.TabIndex = 6;
			// 
			// CompanyCredentialsDetailsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.zGroupBox1);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
            this.Name = "CompanyCredentialsDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zGroupBox1.ResumeLayout(false);
            this.zGroupBox1.PerformLayout();
            this.CertificateLoaderUserControl.ResumeLayout(true);
            this.CertificateLoaderUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZTextBox UserPassword;
		private ZArchitecture.ZTextBox UserID;
		private ZArchitecture.ZTextBox Status;
		private Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		private ZArchitecture.ZTextBox CertificatePassword;
		private ZArchitecture.ZTextBox StatusReason;
	}
}
