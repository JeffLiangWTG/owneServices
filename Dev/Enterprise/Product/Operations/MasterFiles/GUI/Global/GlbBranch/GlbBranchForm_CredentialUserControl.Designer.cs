namespace Enterprise.MasterFiles.GUI
{
	partial class GlbBranchForm_CredentialUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.CertForLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BranchGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BranchCredentialsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CredentialPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificatePanel.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.BranchGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchCredentialsGrid)).BeginInit();
			this.BranchCredentialsGrid.SuspendLayout();
			this.CredentialPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbBranchEInvoicingCredentialCollection);
			// 
			// CertificatePanel
			// 
			this.CertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.CertificatePanel.Controls.Add(this.CertForLabel);
			this.CertificatePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 415, true);
			this.CertificatePanel.Name = "CertificatePanel";
			this.CertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(58, 7, 58, 7, true);
			this.CertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 40, true);
			this.CertificatePanel.TabIndex = 14;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, ".");
			this.CertificateLoaderUserControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|8F4B0619-4122-47E8-B5E5-D57DEF1C467E", "Select Certificate");
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 7, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.CaptionRenderingEnabled = true;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// CertForLabel
			// 
			this.CertForLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|37BB3730-812B-457C-AB63-1A03A31EEA95", "", "Certificate For:");
			this.CertForLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CertForLabel.IsFontBold = true;
			this.CertForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 9, true);
			this.CertForLabel.Name = "CertForLabel";
			this.CertForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CertForLabel.TabIndex = 0;
			// 
			// BranchGroup
			// 
			this.BranchGroup.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|8DE39959-91E4-4A18-95F8-252683901C3A", "Certificate Management");
			this.BranchGroup.Controls.Add(this.BranchCredentialsGrid);
			this.BranchGroup.Controls.Add(this.CertificatePanel);
			this.BranchGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BranchGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BranchGroup.Name = "BranchGroup";
			this.BranchGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.BranchGroup.TabIndex = 1;
			this.BranchGroup.TabStop = false;
			// 
			// BranchCredentialsGrid
			// 
			this.BranchCredentialsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BranchCredentialsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)).GP_MailBoxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)).CurrentDecryptedCertificatePassphrase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)).SerialNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)).IssuerNameCommonName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)).GP_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)).GP_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchEInvoicingCertificateCredential)(null)).PasswordStatus)));
			this.BranchCredentialsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|36A54D91-6F42-4A6F-B05E-190C7E850BC7", "PAC");
			zTextBoxColumnStyleInfo1.ColumnName = "GP_MailBoxID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|6B66571E-F243-496E-A993-3D6B41167C53", "Pass Phrase");
			zTextBoxColumnStyleInfo2.ColumnName = "CurrentDecryptedCertificatePassphrase";
			zTextBoxColumnStyleInfo2.PasswordChar = '*';
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|8A7EF793-BAF8-415D-AE0F-7986D0299CFF", "Serial Number");
			zTextBoxColumnStyleInfo3.ColumnName = "SerialNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|F7A4C9A9-0CD9-4E53-8FB3-045F557EBBE5", "Issuer");
			zTextBoxColumnStyleInfo4.ColumnName = "IssuerNameCommonName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|C24650DB-C943-4C89-A4E8-FDAF29B3DD09", "Issue Date");
			zDateEditColumnStyleInfo1.ColumnName = "GP_IssueDate";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|F43AD2A7-D150-45F3-A5CC-8858515F1F05", "Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "GP_ExpiryDate";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|A80F3EF4-7889-4B58-8BC3-1E6A2B174CBD", "Status");
			zDropEditColumnStyleInfo1.ColumnName = "PasswordStatus";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.BranchCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BranchCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BranchCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BranchCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BranchCredentialsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BranchCredentialsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.BranchCredentialsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BranchCredentialsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BranchCredentialsGrid.GridId = "d35b96df-79f1-483f-8770-3aa78d5599f0";
			this.BranchCredentialsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BranchCredentialsGrid.LayoutKey = "BranchCredentialsGrid";
			this.BranchCredentialsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.BranchCredentialsGrid.Name = "BranchCredentialsGrid";
			this.BranchCredentialsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 401, true);
			this.BranchCredentialsGrid.TabIndex = 1;
			this.BranchCredentialsGrid.AfterBind += new System.EventHandler(this.BranchCredentialsGrid_AfterBind);
			this.BranchCredentialsGrid.CurrentCellChanged += new System.EventHandler(this.BranchCredentialsGrid_CurrentCellChanged);
			this.BranchCredentialsGrid.Leave += new System.EventHandler(this.BranchCredentialsGrid_Leave);
			// 
			// CredentialPanel
			// 
			this.CredentialPanel.Controls.Add(this.BranchGroup);
			this.CredentialPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialPanel.Name = "CredentialPanel";
			this.CredentialPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.CredentialPanel.TabIndex = 5;
			// 
			// GlbBranchForm_CredentialUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialPanel);
			this.Name = "GlbBranchForm_CredentialUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificatePanel.ResumeLayout(false);
			this.CertificatePanel.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.BranchGroup.ResumeLayout(false);
			this.BranchGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchCredentialsGrid)).EndInit();
			this.BranchCredentialsGrid.ResumeLayout(false);
			this.BranchCredentialsGrid.PerformLayout();
			this.CredentialPanel.ResumeLayout(false);
			this.CredentialPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel CertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel CredentialPanel;
		Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		protected Enterprise.ZArchitecture.ZGrid BranchCredentialsGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox BranchGroup;
		Enterprise.ZArchitecture.ZLabel CertForLabel;
	}
}
