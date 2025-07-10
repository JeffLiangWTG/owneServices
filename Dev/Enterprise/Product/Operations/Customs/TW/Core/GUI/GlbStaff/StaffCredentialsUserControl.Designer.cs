namespace Enterprise.Customs.TW.GUI
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.SubscriptionsGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SubscriptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.CertAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertForLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SubscriptionsGroupbox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGrid)).BeginInit();
			this.SubscriptionsGrid.SuspendLayout();
			this.CertificatePanel.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.CredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.TWGlbStaffWrapper);
			// 
			// SubscriptionsGroupbox
			// 
			this.SubscriptionsGroupbox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|8273A270-22A9-4B41-9193-35B0E8A1144F", "Subscriptions Management");
			this.SubscriptionsGroupbox.Controls.Add(this.SubscriptionsGrid);
			this.SubscriptionsGroupbox.Controls.Add(this.CertificatePanel);
			this.SubscriptionsGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubscriptionsGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubscriptionsGroupbox.Name = "SubscriptionsGroupbox";
			this.SubscriptionsGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.SubscriptionsGroupbox.TabIndex = 1;
			this.SubscriptionsGroupbox.TabStop = false;
			this.SubscriptionsGroupbox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SubscriptionsGroupbox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			// 
			// SubscriptionsGrid
			// 
			this.SubscriptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SubscriptionsGrid, "TWPasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_PasswordType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_MailBoxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).CurrentDecryptedPassword)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).CertificateStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).CurrentDecryptedCertificatePassphrase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_ReceiveAutomatically)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_PasswordStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_StatusReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_ExpiryDate)));
			this.SubscriptionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|E24F21FD-D952-42A3-8634-096D812CFB37", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "GP_PasswordType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|A4FEDC1A-435E-49A8-8EF9-B77BEFCA7128", "Mail Box");
			zTextBoxColumnStyleInfo1.ColumnName = "GP_MailBoxID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|9A1F0ACA-C120-49A5-95E7-DE5E0CA76AC3", "Mail Box Password");
			zTextBoxColumnStyleInfo2.ColumnName = "CurrentDecryptedPassword";
			zTextBoxColumnStyleInfo2.PasswordChar = '*';
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);

			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|9E225AF9-E5C8-458A-AD7E-BC272CD5C620", "Sender ID");
			zTextBoxColumnStyleInfo3.ColumnName = "GP_UserID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|C8BF48CB-E1B5-49C0-9223-3C08C10694BA", "Certificate File");
			zTextBoxColumnStyleInfo4.ColumnName = "CertificateStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);

			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|06991C6D-D48F-4880-97AE-CB7DE12C4782", "Certificate Password");
			zTextBoxColumnStyleInfo5.ColumnName = "CurrentDecryptedCertificatePassphrase";
			zTextBoxColumnStyleInfo5.PasswordChar = '*';
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);

			zCheckBoxColumnStyleInfo1.ColumnName = "GP_ReceiveAutomatically";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo6.ColumnName = "GP_PasswordStatus";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|C1168546-6D88-48B3-87A3-35281164D4F2", "Status");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo7.ColumnName = "GP_StatusReason";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.CaptionResourceString= Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|79C71931-9EA8-4672-AC37-B141749C494B", "Status Reason");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);

			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("A383D80B-F4F7-49F3-B219-92EA437B495F", "Issue Date");
			zDateEditColumnStyleInfo1.ColumnComparer = null;
			zDateEditColumnStyleInfo1.ColumnName = "GP_IssueDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("894B31E3-3EB7-4697-942C-65E0F669774E", "Expiry Date");
			zDateEditColumnStyleInfo2.ColumnComparer = null;
			zDateEditColumnStyleInfo2.ColumnName = "GP_ExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			this.SubscriptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SubscriptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SubscriptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SubscriptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);

			this.SubscriptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubscriptionsGrid.GridId = "F546E900-2E39-4D59-8F3A-8F56A867125E";
			this.SubscriptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubscriptionsGrid.LayoutKey = "SubscriptionsGrid";
			this.SubscriptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.SubscriptionsGrid.Name = "SubscriptionsGrid";
			this.SubscriptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 378, true);
			this.SubscriptionsGrid.TabIndex = 1;
			this.SubscriptionsGrid.AfterBind += new System.EventHandler(this.SubscriptionsGrid_AfterBind);
			this.SubscriptionsGrid.CurrentCellChanged += new System.EventHandler(this.SubscriptionsGrid_CurrentCellChanged);
			this.SubscriptionsGrid.Leave += new System.EventHandler(this.SubscriptionsGrid_Leave);
			// 
			// CertificatePanel
			// 
			this.CertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.CertificatePanel.Controls.Add(this.CertAccountTextBox);
			this.CertificatePanel.Controls.Add(this.CertForLabel);
			this.CertificatePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 392, true);
			this.CertificatePanel.Name = "CertificatePanel";
			this.CertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(58, 7, 58, 7, true);
			this.CertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 40, true);
			this.CertificatePanel.TabIndex = 14;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "TWPasswordCollection.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).GP_Certificate)));
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 7, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// CertAccountTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertAccountTextBox, "TWPasswordCollection.SubscriptionTag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.TWGlbStaffWrapper)(null)).TWPasswordCollection)).SyncRoot)).SubscriptionTag)));
			this.CertAccountTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|4013261A-FEDD-4284-92C5-2BE3DDFBDEF4", "Next Password");
			this.CertAccountTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertAccountTextBox.Enabled = false;
			this.CertAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 10, true);
			this.CertAccountTextBox.Name = "CertAccountTextBox";
			this.CertAccountTextBox.ReadOnly = true;
			this.CertAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 18, true);
			this.CertAccountTextBox.TabIndex = 7;
			this.CertAccountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// CertForLabel
			// 
			this.CertForLabel.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("StaffCredentialsUserControl|A45A4EEC-FF4D-41E5-BF5D-0F02AAB8DEB7", "", "Certificate For:");
			this.CertForLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CertForLabel.IsFontBold = true;
			this.CertForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 9, true);
			this.CertForLabel.Name = "CertForLabel";
			this.CertForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CertForLabel.TabIndex = 0;
			// 
			// CredentialsPanel
			// 
			this.CredentialsPanel.Controls.Add(this.SubscriptionsGroupbox);
			this.CredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialsPanel.Name = "CredentialsPanel";
			this.CredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 400, true);
			this.CredentialsPanel.TabIndex = 5;
			// 
			// GlbStaffForm_TWCredentialsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialsPanel);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.CredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SubscriptionsGroupbox.ResumeLayout(false);
			this.SubscriptionsGroupbox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGrid)).EndInit();
			this.SubscriptionsGrid.ResumeLayout(false);
			this.SubscriptionsGrid.PerformLayout();
			this.CertificatePanel.ResumeLayout(false);
			this.CertificatePanel.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.CredentialsPanel.ResumeLayout(false);
			this.CredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		Enterprise.ZArchitecture.GUI.ZPanel CertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel CredentialsPanel;
		Enterprise.ZArchitecture.ZGrid SubscriptionsGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox SubscriptionsGroupbox;
		Enterprise.ZArchitecture.ZLabel CertForLabel;
		Enterprise.ZArchitecture.ZTextBox CertAccountTextBox;
	}
}
