using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbCompany_ICS2CredentialUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            this.ICS2CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ReporterEORI = new Enterprise.ZArchitecture.ZTextBox();
            this.PrivateKey = new Enterprise.ZArchitecture.ZTextBox();
            this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
            this.PrivateKeyStatus = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ICS2CertificateGroupBox.SuspendLayout();
            this.CertificateLoaderUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbCompanyWrapper);
            // 
            // ICS2CertificateGroupBox
            // 
			this.ICS2CertificateGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("23D1B814-EC48-4412-B4E0-7F884C809112", "ICS2 Company Reporting Details");
            this.ICS2CertificateGroupBox.Controls.Add(this.ReporterEORI);
            this.ICS2CertificateGroupBox.Controls.Add(this.PrivateKey);
            this.ICS2CertificateGroupBox.Controls.Add(this.CertificateLoaderUserControl);
            this.ICS2CertificateGroupBox.Controls.Add(this.PrivateKeyStatus);
            this.ICS2CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
            this.ICS2CertificateGroupBox.Name = "ICS2CertificateGroupBox";
            this.ICS2CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 158, true);
            this.ICS2CertificateGroupBox.TabIndex = 0;
            this.ICS2CertificateGroupBox.TabStop = false;
            // 
            // ReporterEORI
            // 
            this.BindingSource.SetBindingMember(this.ReporterEORI, "GlbCompanyCredentialICS2+GP_MailBoxID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanyWrapper)(null)).GlbCompanyCredentialICS2.GP_MailBoxID)));
			this.ReporterEORI.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("091F5CCF-5947-4775-8651-D8FD224DF092", "Reporter EORI");
            this.ReporterEORI.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 126, true);
            this.ReporterEORI.Name = "ReporterEORI";
            this.ReporterEORI.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
            this.ReporterEORI.TabIndex = 3;
            // 
            // PrivateKey
            // 
            this.BindingSource.SetBindingMember(this.PrivateKey, "GlbCompanyCredentialICS2+CurrentDecryptedCertificatePassphrase");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanyWrapper)(null)).GlbCompanyCredentialICS2.CurrentDecryptedCertificatePassphrase)));
			this.PrivateKey.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7C41B7CC-47DB-43A8-A6BB-A4FB29E441E3", "Private Key Password");
            this.PrivateKey.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PrivateKey.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 70, true);
            this.PrivateKey.Name = "PrivateKey";
            this.PrivateKey.PasswordChar = '*';
            this.PrivateKey.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
            this.PrivateKey.TabIndex = 1;
            // 
            // CertificateLoaderUserControl
            // 
            this.CertificateLoaderUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "GlbCompanyCredentialICS2+GP_Certificate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbCompanyWrapper)(null)).GlbCompanyCredentialICS2.GP_Certificate)));
            this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
            this.CertificateLoaderUserControl.FileDataAsString = "";
            this.CertificateLoaderUserControl.FileDialogTitle = "";
            this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
            this.CertificateLoaderUserControl.InitialDirectory = "";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CertificateLoaderUserControl, false);
            this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 38, true);
            this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
            this.CertificateLoaderUserControl.ReadOnly = false;
            this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
            this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
            this.CertificateLoaderUserControl.TabIndex = 0;
            // 
            // PrivateKeyStatus
            // 
            this.BindingSource.SetBindingMember(this.PrivateKeyStatus, "GlbCompanyCredentialICS2+PasswordStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.Business.GlbCompanyWrapper)(null)).GlbCompanyCredentialICS2.PasswordStatus)));
			this.PrivateKeyStatus.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C0713795-FE5C-4830-B1F6-5F45E9F418AF", "Private Key Status");
            this.PrivateKeyStatus.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PrivateKeyStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 98, true);
            this.PrivateKeyStatus.Name = "PrivateKeyStatus";
            this.PrivateKeyStatus.ReadOnly = true;
            this.PrivateKeyStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
            this.PrivateKeyStatus.TabIndex = 2;
            // 
            // GlbCompany_ICS2CredentialUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ICS2CertificateGroupBox);
            this.Name = "GlbCompany_ICS2CredentialUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 163, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ICS2CertificateGroupBox.ResumeLayout(false);
            this.ICS2CertificateGroupBox.PerformLayout();
            this.CertificateLoaderUserControl.ResumeLayout(true);
            this.CertificateLoaderUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ICS2CertificateGroupBox;
		ZArchitecture.ZTextBox PrivateKeyStatus;
		internal Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		ZArchitecture.ZTextBox PrivateKey;
		internal ZArchitecture.ZTextBox ReporterEORI;
	}
}
