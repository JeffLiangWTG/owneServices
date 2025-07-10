namespace Enterprise.Customs.US.GUI
{
	partial class CompanyCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZGrid USCredentialGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CredentialGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CredentialGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(USCredentialGrid)).BeginInit();
			USCredentialGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.GlbCompanyWrapper);
			// 
			// CredentialGroupBox
			// 
			this.CredentialGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("GlbCompanyForm|4A1D96EF-2156-4957-8C2A-81D4D0E1C285", "eBond Insurance Agent Credentials");
			this.CredentialGroupBox.Controls.Add(USCredentialGrid);
			//this.CredentialGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialGroupBox.Name = "CredentialGroupBox";
			this.CredentialGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.CredentialGroupBox.TabIndex = 1;
			this.CredentialGroupBox.TabStop = false;
			// 
			// USCredentialGrid
			// 
			USCredentialGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(USCredentialGrid, "PasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbCompanyWrapper)(null)).PasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).GP_MailBoxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).MailBoxIDDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).GP_UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).CurrentDecryptedCertificatePassphrase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).PasswordStatus)));
			USCredentialGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Agent Number";
			zDropEditColumnStyleInfo1.ColumnName = "GP_MailBoxID";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Agent Number Description";
			zTextBoxColumnStyleInfo2.ColumnName = "MailBoxIDDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo2.Caption = "User Name";
			zDropEditColumnStyleInfo2.ColumnName = "GP_UserID";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "Password";
			zTextBoxColumnStyleInfo1.ColumnName = "CurrentDecryptedCertificatePassphrase";
			zTextBoxColumnStyleInfo1.PasswordChar = '*';
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo4.Caption = "Status";
			zDropEditColumnStyleInfo4.ColumnName = "PasswordStatus";
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			USCredentialGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			USCredentialGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			USCredentialGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			USCredentialGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			USCredentialGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			USCredentialGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			USCredentialGrid.GridId = "9141621C-A189-4FC8-9509-A306F8D108E8";
			USCredentialGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			USCredentialGrid.LayoutKey = "USCredentialGrid";
			USCredentialGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			USCredentialGrid.Name = "USCredentialGrid";
			USCredentialGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 379, true);
			USCredentialGrid.TabIndex = 1;
			// 
			// CompanyCredentialsUserControl
			//
			this.Controls.Add(this.CredentialGroupBox);
			this.Name = "CompanyCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CredentialGroupBox.ResumeLayout(false);
			this.CredentialGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(USCredentialGrid)).EndInit();
			USCredentialGrid.ResumeLayout(false);
			USCredentialGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZGroupBox CredentialGroupBox;
	}
}
