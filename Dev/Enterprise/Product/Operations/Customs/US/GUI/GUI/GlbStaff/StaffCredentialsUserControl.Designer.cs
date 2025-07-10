namespace Enterprise.Customs.US.GUI
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CredentialGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.USCredentialGrid = new Enterprise.ZArchitecture.ZGrid();
			this.USCredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CredentialGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.USCredentialGrid)).BeginInit();
			this.USCredentialGrid.SuspendLayout();
			this.USCredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.GlbStaffWrapper);
			// 
			// CredentialGroupBox
			// 
			this.CredentialGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("GlbStaffForm|6655EFD9-F4DA-4259-87FE-7F7D195C24B3", "eBond Insurance Agent Credentials");
			this.CredentialGroupBox.Controls.Add(this.USCredentialGrid);
			this.CredentialGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialGroupBox.Name = "CredentialGroupBox";
			this.CredentialGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.CredentialGroupBox.TabIndex = 1;
			this.CredentialGroupBox.TabStop = false;
			// 
			// USCredentialGrid
			// 
			this.USCredentialGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.USCredentialGrid, "PasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbStaffWrapper)(null)).PasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbStaffCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_MailBoxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbStaffCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).MailBoxIDDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbStaffCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbStaffCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).CurrentDecryptedCertificatePassphrase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.GlbStaffCredential)(((System.Collections.IList)(((Enterprise.Customs.US.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).PasswordStatus)));
			this.USCredentialGrid.CaptionVisible = false;
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
			this.USCredentialGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.USCredentialGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.USCredentialGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.USCredentialGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.USCredentialGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.USCredentialGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.USCredentialGrid.GridId = "3DA510D7-7C16-4497-9ED2-9C21A59C8DEC";
			this.USCredentialGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.USCredentialGrid.LayoutKey = "USCredentialGrid";
			this.USCredentialGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.USCredentialGrid.Name = "USCredentialGrid";
			this.USCredentialGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 379, true);
			this.USCredentialGrid.TabIndex = 1;
			// 
			// USCredentialsPanel
			// 
			this.USCredentialsPanel.Controls.Add(this.CredentialGroupBox);
			this.USCredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.USCredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.USCredentialsPanel.Name = "USCredentialsPanel";
			this.USCredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.USCredentialsPanel.TabIndex = 5;
			// 
			// StaffCredentialsUserControl
			// 
			this.Controls.Add(this.USCredentialsPanel);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.USCredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CredentialGroupBox.ResumeLayout(false);
			this.CredentialGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.USCredentialGrid)).EndInit();
			this.USCredentialGrid.ResumeLayout(false);
			this.USCredentialGrid.PerformLayout();
			this.USCredentialsPanel.ResumeLayout(false);
			this.USCredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel USCredentialsPanel;
		Enterprise.ZArchitecture.ZGrid USCredentialGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox CredentialGroupBox;
	}
}
