namespace Enterprise.MasterFiles.GUI
{
	partial class EInvoicingCertificateUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateLoaderUserControl = new Enterprise.MasterFiles.GUI.DigitalCertificateControl_p12_EInvoicing();
			this.CertificateManagementLabel = new Enterprise.ZArchitecture.ZLabel();
			this.X509CertificateGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.X509CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CredentialPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificatePanel.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.X509CertificateGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.X509CertificatesGrid)).BeginInit();
			this.X509CertificatesGrid.SuspendLayout();
			this.CredentialPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CombinedEInvoicingCertificateCollection);
			// 
			// CertificatePanel
			// 
			this.CertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.CertificatePanel.Controls.Add(this.CertificateManagementLabel);
			this.CertificatePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 417, true);
			this.CertificatePanel.Name = "CertificatePanel";
			this.CertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(58, 7, 58, 7, true);
			this.CertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 40, true);
			this.CertificatePanel.TabIndex = 14;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowClear = false;
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, ".");
			this.CertificateLoaderUserControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d95777ef-4bfd-46d6-9239-96f3cf6d5fc4", "Select Certificate");
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.LoadButtonText = Enterprise.Registry.GUI.DigitalCertificateLoadButtonText.Add;
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 7, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.ShowUserFeedbackLabel = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			this.CertificateLoaderUserControl.OnDataRegister += new System.EventHandler(this.CertificateLoaderUserControl_OnDataRegister);
			this.CertificateLoaderUserControl.DataLoaded += new System.EventHandler(this.CertificateLoaderUserControl_DataChanged);
			this.CertificateLoaderUserControl.DataRegistered += this.CertificateLoaderUserControl_DataRegistered;
			// 
			// CertificateManagementLabel
			// 
			this.CertificateManagementLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CertificateManagementLabel.IsFontBold = true;
			this.CertificateManagementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 9, true);
			this.CertificateManagementLabel.Name = "CertificateManagementLabel";
			this.CertificateManagementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CertificateManagementLabel.TabIndex = 0;
			this.CertificateManagementLabel.UseMnemonic = false;
			// 
			// X509CertificateGroup
			// 
			this.X509CertificateGroup.Controls.Add(this.X509CertificatesGrid);
			this.X509CertificateGroup.Controls.Add(this.CertificatePanel);
			this.X509CertificateGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.X509CertificateGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.X509CertificateGroup.Name = "X509CertificateGroup";
			this.X509CertificateGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.X509CertificateGroup.TabIndex = 1;
			this.X509CertificateGroup.TabStop = false;
			// 
			// X509CertificatesGrid
			// 
			this.X509CertificatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.X509CertificatesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).CurrentDecryptedCertificatePassphrase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).SubjectNameCommonName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).SerialNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).PreferredSequenceNumberForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).PasswordStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).GP_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).GP_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).IssuerNameCommonName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredential)(null)).GP_UserID)));
			this.X509CertificatesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5b07e132-87eb-4eb2-9154-fff89ac9c454", "Pass Phrase");
			zTextBoxColumnStyleInfo1.ColumnName = "CurrentDecryptedCertificatePassphrase";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsSortable = false;
			zTextBoxColumnStyleInfo1.PasswordChar = '*';
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("81fc55a7-b14e-44dd-9c36-e41779fb9b81", "Certificate Owner");
			zTextBoxColumnStyleInfo2.ColumnName = "SubjectNameCommonName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3d27fc63-b71c-42af-abaf-6bec51efc242", "Serial Number");
			zTextBoxColumnStyleInfo3.ColumnName = "SerialNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("47e1d0c4-89e9-4215-9064-4d006260823c", "Seq.", "Preferred Sequence Number", "");
			zTextBoxColumnStyleInfo4.ColumnName = "PreferredSequenceNumberForDisplay";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("78db0b7b-b6af-43ae-9bc0-81f0bb98fdb3", "Status");
			zDropEditColumnStyleInfo1.ColumnName = "PasswordStatus";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eb78953f-e319-4f4d-a3d7-6a8bf52d871a", "Valid From");
			zDateEditColumnStyleInfo1.ColumnName = "GP_IssueDate";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d2e308d3-9caa-4254-8aac-f36a582b8f37", "Valid To");
			zDateEditColumnStyleInfo2.ColumnName = "GP_ExpiryDate";
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6404ef33-b5c8-4e61-b53f-bd6ccdb8efc0", "Certificate Issuer");
			zTextBoxColumnStyleInfo5.ColumnName = "IssuerNameCommonName";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("664f9550-05f5-4e13-a3a3-a66c10000a0d", "Certificate Thumb-print");
			zTextBoxColumnStyleInfo6.ColumnName = "GP_UserID";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.X509CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.X509CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.X509CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.X509CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.X509CertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.X509CertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.X509CertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.X509CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.X509CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.X509CertificatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.X509CertificatesGrid.GridId = "67c5b449-92aa-4c7a-9fec-abfbc95a3307";
			this.X509CertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.X509CertificatesGrid.LayoutKey = "X509CertificatesGrid";
			this.X509CertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.X509CertificatesGrid.Name = "X509CertificatesGrid";
			this.X509CertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 403, true);
			this.X509CertificatesGrid.TabIndex = 1;
			this.X509CertificatesGrid.AfterBind += new System.EventHandler(this.BranchCredentialsGrid_AfterBind);
			this.X509CertificatesGrid.Leave += new System.EventHandler(this.BranchCredentialsGrid_Leave);
			// 
			// CredentialPanel
			// 
			this.CredentialPanel.Controls.Add(this.X509CertificateGroup);
			this.CredentialPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialPanel.Name = "CredentialPanel";
			this.CredentialPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.CredentialPanel.TabIndex = 5;
			// 
			// EInvoicingCertificateUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialPanel);
			this.Name = "EInvoicingCertificateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificatePanel.ResumeLayout(false);
			this.CertificatePanel.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.X509CertificateGroup.ResumeLayout(false);
			this.X509CertificateGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.X509CertificatesGrid)).EndInit();
			this.X509CertificatesGrid.ResumeLayout(false);
			this.X509CertificatesGrid.PerformLayout();
			this.CredentialPanel.ResumeLayout(false);
			this.CredentialPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel CertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel CredentialPanel;
		DigitalCertificateControl_p12_EInvoicing CertificateLoaderUserControl;
		Enterprise.ZArchitecture.ZGrid X509CertificatesGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox X509CertificateGroup;
		Enterprise.ZArchitecture.ZLabel CertificateManagementLabel;
	}
}
