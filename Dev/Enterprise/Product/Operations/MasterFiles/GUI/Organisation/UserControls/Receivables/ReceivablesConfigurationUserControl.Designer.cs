namespace Enterprise.MasterFiles.GUI
{
	public partial class ReceivablesConfigurationUserControl
	{

		#region Component Designer generated code

		internal Enterprise.ZArchitecture.ZLabel OB_ARCreateVATComplianceDocumentOnPostingLabel;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit OB_ARCreateVATComplianceDocumentOnPostingDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox OM_ARDontShowTaxOnDocsBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OM_ARWHTApplicableBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ARCurrencyUpliftGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ARCreditTermsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ARQualityAssuranceGroupBox;
		public Enterprise.ZArchitecture.GUI.ZCheckBox OM_QualityAssuredBoundARCheckBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OM_QualityAssuredCheckedDateBoundARDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_ARCategoryBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_ARConsolidatedAccountingCategoryBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OB_OJ_ARDebtorGroupBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TaxDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox PayToAccountGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ExternalDebtorCodeGroupBox;
		private Enterprise.ZArchitecture.ZTextBox ExternalDebtorCodeTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit OB_ARVATConfigDropEdit;
		internal Enterprise.ZArchitecture.ZLabel OB_ARVATConfigLabel;
		private Organisation.UserControls.Receivables.ARAccountDetailsGrid ARAccountDetailsGrid;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMAuthorityToLeaveDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OM_CMAuthorityToLeaveGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox OM_ARVATSplitPaymentApplicableBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ClientNumberGroupBox;
		private Enterprise.ZArchitecture.ZTextBox ClientNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit_TransCreationRestriction;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit OB_ARGoodsOwnershipDropEdit;
		internal Enterprise.ZArchitecture.ZLabel OB_ARGoodsOwnershipLabel;
		private AccCFXUpliftCfg cfxUpliftConfig;

		private void InitializeComponent()
		{
			this.OB_ARCreateVATComplianceDocumentOnPostingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_ARDontShowTaxOnDocsBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_ARWHTApplicableBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARCurrencyUpliftGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.cfxUpliftConfig = new Enterprise.MasterFiles.GUI.AccCFXUpliftCfg();
			this.ARCreditTermsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ARAccountDetailsGrid = new Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables.ARAccountDetailsGrid();
			this.OB_RX_NKAPDefltCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OverridePayToAccountCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PayToAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_ARCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ARQualityAssuranceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_QualityAssuredCheckedDateBoundARDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OM_QualityAssuredBoundARCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TaxDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OB_ARGoodsOwnershipLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OB_ARGoodsOwnershipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_ARVATSplitPaymentApplicableBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OB_ARVATConfigLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OB_ARVATConfigDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClientNumberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClientNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OM_CMAuthorityToLeaveGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_CMAuthorityToLeaveDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExternalDebtorCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExternalDebtorCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyDataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEdit_TransCreationRestriction = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AllowMultiCurrencyPaymentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ApplicableSurchargesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApplicableSurchargesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicableSurchargesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.SuspendLayout();
			this.ARCurrencyUpliftGroupBox.SuspendLayout();
			this.cfxUpliftConfig.SuspendLayout();
			this.ARCreditTermsGroupBox.SuspendLayout();
			this.ARAccountDetailsGrid.SuspendLayout();
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.SuspendLayout();
			this.PayToAccountGuidFindBox.SuspendLayout();
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.SuspendLayout();
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.SuspendLayout();
			this.OM_ARCategoryBoundDropEdit.SuspendLayout();
			this.ARQualityAssuranceGroupBox.SuspendLayout();
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.SuspendLayout();
			this.TaxDetailsGroupBox.SuspendLayout();
			this.OB_ARGoodsOwnershipDropEdit.SuspendLayout();
			this.OB_ARVATConfigDropEdit.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.ClientNumberGroupBox.SuspendLayout();
			this.OM_CMAuthorityToLeaveGroupBox.SuspendLayout();
			this.OM_CMAuthorityToLeaveDropEdit.SuspendLayout();
			this.ExternalDebtorCodeGroupBox.SuspendLayout();
			this.CompanyDataGroupBox.SuspendLayout();
			this.zDropEdit_TransCreationRestriction.SuspendLayout();
			this.ApplicableSurchargesPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// OB_ARCreateVATComplianceDocumentOnPostingLabel
			// 
			this.OB_ARCreateVATComplianceDocumentOnPostingLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|67933D98-9386-429A-B63A-BA4564269AA4", "Create Compliance Document Record on Posting");
			this.OB_ARCreateVATComplianceDocumentOnPostingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OB_ARCreateVATComplianceDocumentOnPostingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 154, true);
			this.OB_ARCreateVATComplianceDocumentOnPostingLabel.Name = "OB_ARCreateVATComplianceDocumentOnPostingLabel";
			this.OB_ARCreateVATComplianceDocumentOnPostingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 15, true);
			this.OB_ARCreateVATComplianceDocumentOnPostingLabel.TabIndex = 5;
			// 
			// OB_ARCreateVATComplianceDocumentOnPostingDropEdit
			// 
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit, "CompanyData+OB_ARCreateVATComplianceDocumentOnPosting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARCreateVATComplianceDocumentOnPosting)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit, false);
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 172, true);
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.Name = "OB_ARCreateVATComplianceDocumentOnPostingDropEdit";
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.TabIndex = 4;
			// 
			// OM_ARDontShowTaxOnDocsBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_ARDontShowTaxOnDocsBoundCheckEdit, "MiscServ.OM_ARDontShowTaxOnDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARDontShowTaxOnDocs)));
			this.OM_ARDontShowTaxOnDocsBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|f4a7617b-1e8f-4d4c-96b8-8ebc15c931e5", "Don\'t Show Tax on Documents");
			this.OM_ARDontShowTaxOnDocsBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_ARDontShowTaxOnDocsBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 87, true);
			this.OM_ARDontShowTaxOnDocsBoundCheckEdit.Name = "OM_ARDontShowTaxOnDocsBoundCheckEdit";
			this.OM_ARDontShowTaxOnDocsBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.OM_ARDontShowTaxOnDocsBoundCheckEdit.TabIndex = 2;
			// 
			// OM_ARWHTApplicableBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_ARWHTApplicableBoundCheckEdit, "MiscServ.OM_ARWHTApplicable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARWHTApplicable)));
			this.OM_ARWHTApplicableBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|8eac6e68-6a5d-4ceb-a8d2-ffb633ff148a", "Withholding Tax Is Applicable");
			this.OM_ARWHTApplicableBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_ARWHTApplicableBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 63, true);
			this.OM_ARWHTApplicableBoundCheckEdit.Name = "OM_ARWHTApplicableBoundCheckEdit";
			this.OM_ARWHTApplicableBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.OM_ARWHTApplicableBoundCheckEdit.TabIndex = 1;
			// 
			// ARCurrencyUpliftGroupBox
			// 
			this.ARCurrencyUpliftGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|99186415-8386-46c2-98c9-253ede0cb0e8", "Currency Uplift");
			this.ARCurrencyUpliftGroupBox.Controls.Add(this.cfxUpliftConfig);
			this.ARCurrencyUpliftGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 8, true);
			this.ARCurrencyUpliftGroupBox.Name = "ARCurrencyUpliftGroupBox";
			this.ARCurrencyUpliftGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 216, true);
			this.ARCurrencyUpliftGroupBox.TabIndex = 3;
			this.ARCurrencyUpliftGroupBox.TabStop = false;
			// 
			// cfxUpliftConfig
			// 
			this.cfxUpliftConfig.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cfxUpliftConfig, "CompanyData.AccCFXConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccCFXUpliftConfigurationCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AccCFXConfigurations)));
			this.cfxUpliftConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cfxUpliftConfig.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.cfxUpliftConfig.Name = "cfxUpliftConfig";
			this.cfxUpliftConfig.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 197, true);
			this.cfxUpliftConfig.TabIndex = 3;
			// 
			// ARCreditTermsGroupBox
			// 
			this.ARCreditTermsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|d1f39542-fd8a-44df-8726-dc1acba8dac3", "Account Details");
			this.ARCreditTermsGroupBox.Controls.Add(this.ARAccountDetailsGrid);
			this.ARCreditTermsGroupBox.Controls.Add(this.OB_RX_NKAPDefltCurrencyCodeFindBox);
			this.ARCreditTermsGroupBox.Controls.Add(this.OverridePayToAccountCheckBox);
			this.ARCreditTermsGroupBox.Controls.Add(this.PayToAccountGuidFindBox);
			this.ARCreditTermsGroupBox.Controls.Add(this.OB_OJ_ARDebtorGroupBoundGuidFindBox);
			this.ARCreditTermsGroupBox.Controls.Add(this.OM_ARConsolidatedAccountingCategoryBoundDropEdit);
			this.ARCreditTermsGroupBox.Controls.Add(this.OM_ARCategoryBoundDropEdit);
			this.ARCreditTermsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ARCreditTermsGroupBox.Name = "ARCreditTermsGroupBox";
			this.ARCreditTermsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 261, true);
			this.ARCreditTermsGroupBox.TabIndex = 0;
			this.ARCreditTermsGroupBox.TabStop = false;
			// 
			// ARAccountDetailsGrid
			// 
			this.ARAccountDetailsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ARAccountDetailsGrid, "CurrentCompanyDataAsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgCompanyData)(((Enterprise.MasterFiles.Business.OrgCompanyData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CurrentCompanyDataAsCollection)).SyncRoot)))));
			this.ARAccountDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 168, true);
			this.ARAccountDetailsGrid.Name = "ARAccountDetailsGrid";
			this.ARAccountDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 84, true);
			this.ARAccountDetailsGrid.TabIndex = 6;
			// 
			// OB_RX_NKAPDefltCurrencyCodeFindBox
			// 
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_RX_NKAPDefltCurrencyCodeFindBox, "CompanyData+OB_RX_NKARDDefltCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_RX_NKARDDefltCurrency)));
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 90, true);
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.Name = "OB_RX_NKAPDefltCurrencyCodeFindBox";
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.ParentType = null;
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.PopupCaption = "Select Currency";
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.TabIndex = 3;
			// 
			// OverridePayToAccountCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverridePayToAccountCheckBox, "CompanyData+OverrideBankAccountFromDebtorGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OverrideBankAccountFromDebtorGroup)));
			this.OverridePayToAccountCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|F17EFB17-3013-4E0A-B227-818607504743", "Do Not Use Account Group or Registry Bank Account Defaults", "A single, specific bank account for this client's AR documents and receipting can be set by ticking this checkbox.  When not ticked, the AR bank accounts used for this client will come from the AR Account Group falling back to Registry and Default Receipting bank account setups.");
			this.OverridePayToAccountCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverridePayToAccountCheckBox, false);
			this.OverridePayToAccountCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 114, true);
			this.OverridePayToAccountCheckBox.Name = "OverridePayToAccountCheckBox";
			this.OverridePayToAccountCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 24, true);
			this.OverridePayToAccountCheckBox.TabIndex = 4;
			this.OverridePayToAccountCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PayToAccountGuidFindBox
			// 
			this.PayToAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PayToAccountGuidFindBox, "CompanyData+ARBankAccountToDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.ARBankAccountToDisplay)));
			this.PayToAccountGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|F19B3C6D-0882-4964-A2F6-5449424FA70F", "Bank to This Account", "A single, specific bank account for this client's AR documents and receipting can be nominated here. The bank account nominated here will be used on all AR documents and when creating new AR receipts.  When blank, the bank accounts used for this client will come from the AR Account Group falling back to Registry and Default Receipting setups.");
			this.PayToAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 142, true);
			this.PayToAccountGuidFindBox.Name = "PayToAccountGuidFindBox";
			this.PayToAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PayToAccountGuidFindBox.ParentType = null;
			this.PayToAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.PayToAccountGuidFindBox.TabIndex = 5;
			// 
			// OB_OJ_ARDebtorGroupBoundGuidFindBox
			// 
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_OJ_ARDebtorGroupBoundGuidFindBox, "CompanyData+OB_OJ_ARDebtorGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_OJ_ARDebtorGroup)));
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|cbbb7592-5c5a-4f35-addb-67cf6d90ca15", "Account Group");
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 15, true);
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.Name = "OB_OJ_ARDebtorGroupBoundGuidFindBox";
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.ParentType = null;
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.PopupCaption = "Select Debtor Group";
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.TabIndex = 0;
			// 
			// OM_ARConsolidatedAccountingCategoryBoundDropEdit
			// 
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_ARConsolidatedAccountingCategoryBoundDropEdit, "MiscServ.OM_ARConsolidatedAccountingCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARConsolidatedAccountingCategory)));
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|7d7d6c10-3afb-45d4-ad21-53cc1bbaa606", "Consolidation Category");
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 65, true);
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.Name = "OM_ARConsolidatedAccountingCategoryBoundDropEdit";
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.TabIndex = 2;
			// 
			// OM_ARCategoryBoundDropEdit
			// 
			this.OM_ARCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_ARCategoryBoundDropEdit, "MiscServ.OM_ARCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARCategory)));
			this.OM_ARCategoryBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|1678feca-d3dc-4eaf-9b96-035800711249", "Accounts Relationship");
			this.OM_ARCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 40, true);
			this.OM_ARCategoryBoundDropEdit.Name = "OM_ARCategoryBoundDropEdit";
			this.OM_ARCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.OM_ARCategoryBoundDropEdit.TabIndex = 1;
			// 
			// ARQualityAssuranceGroupBox
			// 
			this.ARQualityAssuranceGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|ebfd27f0-7817-40b1-9ce5-0ecae880d663", "Quality Assurance");
			this.ARQualityAssuranceGroupBox.Controls.Add(this.OM_QualityAssuredCheckedDateBoundARDateEdit);
			this.ARQualityAssuranceGroupBox.Controls.Add(this.OM_QualityAssuredBoundARCheckBox);
			this.ARQualityAssuranceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 225, true);
			this.ARQualityAssuranceGroupBox.Name = "ARQualityAssuranceGroupBox";
			this.ARQualityAssuranceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 50, true);
			this.ARQualityAssuranceGroupBox.TabIndex = 4;
			this.ARQualityAssuranceGroupBox.TabStop = false;
			// 
			// OM_QualityAssuredCheckedDateBoundARDateEdit
			// 
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.AllowDrop = true;
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.OM_QualityAssuredCheckedDateBoundARDateEdit, "CompanyData+OB_ARQualityAssuredCheckedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARQualityAssuredCheckedDate)));
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 21, true);
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.Name = "OM_QualityAssuredCheckedDateBoundARDateEdit";
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.TabIndex = 1;
			// 
			// OM_QualityAssuredBoundARCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_QualityAssuredBoundARCheckBox, "CompanyData+OB_ARQualityAssured");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARQualityAssured)));
			this.OM_QualityAssuredBoundARCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_QualityAssuredBoundARCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 19, true);
			this.OM_QualityAssuredBoundARCheckBox.Name = "OM_QualityAssuredBoundARCheckBox";
			this.OM_QualityAssuredBoundARCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.OM_QualityAssuredBoundARCheckBox.TabIndex = 0;
			// 
			// TaxDetailsGroupBox
			// 
			this.TaxDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|c6074d28-d324-42c4-b7ed-b8876fc3043e", "Tax Details");
			this.TaxDetailsGroupBox.Controls.Add(this.OB_ARGoodsOwnershipLabel);
			this.TaxDetailsGroupBox.Controls.Add(this.OB_ARGoodsOwnershipDropEdit);
			this.TaxDetailsGroupBox.Controls.Add(this.OM_ARVATSplitPaymentApplicableBoundCheckEdit);
			this.TaxDetailsGroupBox.Controls.Add(this.OB_ARVATConfigLabel);
			this.TaxDetailsGroupBox.Controls.Add(this.OB_ARVATConfigDropEdit);
			this.TaxDetailsGroupBox.Controls.Add(this.OM_ARDontShowTaxOnDocsBoundCheckEdit);
			this.TaxDetailsGroupBox.Controls.Add(this.OM_ARWHTApplicableBoundCheckEdit);
			this.TaxDetailsGroupBox.Controls.Add(this.OB_ARCreateVATComplianceDocumentOnPostingLabel);
			this.TaxDetailsGroupBox.Controls.Add(this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit);
			this.TaxDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 275, true);
			this.TaxDetailsGroupBox.Name = "TaxDetailsGroupBox";
			this.TaxDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 199, true);
			this.TaxDetailsGroupBox.TabIndex = 1;
			this.TaxDetailsGroupBox.TabStop = false;
			// 
			// OB_ARGoodsOwnershipLabel
			// 
			this.OB_ARGoodsOwnershipLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|8D1E9595-E6A6-4237-A644-0E145B3512D3", "Goods Ownership");
			this.OB_ARGoodsOwnershipLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OB_ARGoodsOwnershipLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 114, true);
			this.OB_ARGoodsOwnershipLabel.Name = "OB_ARGoodsOwnershipLabel";
			this.OB_ARGoodsOwnershipLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 15, true);
			this.OB_ARGoodsOwnershipLabel.TabIndex = 7;
			// 
			// OB_ARGoodsOwnershipDropEdit
			// 
			this.OB_ARGoodsOwnershipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_ARGoodsOwnershipDropEdit, "CompanyData+OB_ARGoodsOwnership");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARGoodsOwnership)));
			this.OB_ARGoodsOwnershipDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|572F85A8-4BEE-405D-AD9F-E7FA1C6ADAA0", "Goods Ownership", "Tax ID defaulting rules can be configured for goods ownership using the Debtor Role column of a Tax Override Group.");
			this.OB_ARGoodsOwnershipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 132, true);
			this.OB_ARGoodsOwnershipDropEdit.Name = "OB_ARGoodsOwnershipDropEdit";
			this.OB_ARGoodsOwnershipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.OB_ARGoodsOwnershipDropEdit.TabIndex = 6;
			// 
			// OM_ARVATSplitPaymentApplicableBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_ARVATSplitPaymentApplicableBoundCheckEdit, "MiscServ.OM_ARVATSplitPaymentApplicable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARVATSplitPaymentApplicable)));
			this.OM_ARVATSplitPaymentApplicableBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1e86a95a-6c73-4a20-afd1-5d65e0ad932e", "Split Payment VAT is Applicable");
			this.OM_ARVATSplitPaymentApplicableBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_ARVATSplitPaymentApplicableBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 155, true);
			this.OM_ARVATSplitPaymentApplicableBoundCheckEdit.Name = "OM_ARVATSplitPaymentApplicableBoundCheckEdit";
			this.OM_ARVATSplitPaymentApplicableBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.OM_ARVATSplitPaymentApplicableBoundCheckEdit.TabIndex = 4;
			// 
			// OB_ARVATConfigLabel
			// 
			this.BindingSource.SetBindingMember(this.OB_ARVATConfigLabel, "CompanyData+OB_VATConfigLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_VATConfigLabel)));
			this.OB_ARVATConfigLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OB_ARVATConfigLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.OB_ARVATConfigLabel.Name = "OB_ARVATConfigLabel";
			this.OB_ARVATConfigLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 15, true);
			this.OB_ARVATConfigLabel.TabIndex = 3;
			// 
			// OB_ARVATConfigDropEdit
			// 
			this.OB_ARVATConfigDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_ARVATConfigDropEdit, "CompanyData+OB_ARVATConfig");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARVATConfig)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OB_ARVATConfigDropEdit, false);
			this.OB_ARVATConfigDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 35, true);
			this.OB_ARVATConfigDropEdit.Name = "OB_ARVATConfigDropEdit";
			this.OB_ARVATConfigDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.OB_ARVATConfigDropEdit.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.ClientNumberGroupBox);
			this.MainPanel.Controls.Add(this.OM_CMAuthorityToLeaveGroupBox);
			this.MainPanel.Controls.Add(this.ExternalDebtorCodeGroupBox);
			this.MainPanel.Controls.Add(this.CompanyDataGroupBox);
			this.MainPanel.Controls.Add(this.ARCreditTermsGroupBox);
			this.MainPanel.Controls.Add(this.TaxDetailsGroupBox);
			this.MainPanel.Controls.Add(this.ARCurrencyUpliftGroupBox);
			this.MainPanel.Controls.Add(this.ARQualityAssuranceGroupBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 610, true);
			this.MainPanel.TabIndex = 0;
			// 
			// ClientNumberGroupBox
			// 
			this.ClientNumberGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|d0146c47-df87-41d9-b2e4-fd7d0931f0e1", "Client Number");
			this.ClientNumberGroupBox.Controls.Add(this.ClientNumberTextBox);
			this.ClientNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 419, true);
			this.ClientNumberGroupBox.Name = "ClientNumberGroupBox";
			this.ClientNumberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 52, true);
			this.ClientNumberGroupBox.TabIndex = 6;
			this.ClientNumberGroupBox.TabStop = false;
			// 
			// ClientNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientNumberTextBox, "CompanyData+OB_ARClientNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARClientNumber)));
			this.ClientNumberTextBox.CaptionResourceString = null;
			this.ClientNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 19, true);
			this.ClientNumberTextBox.Name = "ClientNumberTextBox";
			this.ClientNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.ClientNumberTextBox.TabIndex = 0;
			// 
			// OM_CMAuthorityToLeaveGroupBox
			// 
			this.OM_CMAuthorityToLeaveGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cc4a8946-4420-4d8c-807b-f9322fb41ec1", "Authority To Leave");
			this.OM_CMAuthorityToLeaveGroupBox.Controls.Add(this.OM_CMAuthorityToLeaveDropEdit);
			this.OM_CMAuthorityToLeaveGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 348, true);
			this.OM_CMAuthorityToLeaveGroupBox.Name = "OM_CMAuthorityToLeaveGroupBox";
			this.OM_CMAuthorityToLeaveGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 62, true);
			this.OM_CMAuthorityToLeaveGroupBox.TabIndex = 4;
			this.OM_CMAuthorityToLeaveGroupBox.TabStop = false;
			// 
			// OM_CMAuthorityToLeaveDropEdit
			// 
			this.OM_CMAuthorityToLeaveDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMAuthorityToLeaveDropEdit, "MiscServ+OM_CMAuthorityToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMAuthorityToLeave)));
			this.OM_CMAuthorityToLeaveDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cc4a8946-4420-4d8c-807b-f9322fb41ec1", "Authority To Leave");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OM_CMAuthorityToLeaveDropEdit, false);
			this.OM_CMAuthorityToLeaveDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 29, true);
			this.OM_CMAuthorityToLeaveDropEdit.Name = "OM_CMAuthorityToLeaveDropEdit";
			this.OM_CMAuthorityToLeaveDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.OM_CMAuthorityToLeaveDropEdit.TabIndex = 0;
			// 
			// ExternalDebtorCodeGroupBox
			// 
			this.ExternalDebtorCodeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|DF5F1ABF-D7BC-4C9B-BF84-62011C5B267C", "External System Debtor Code");
			this.ExternalDebtorCodeGroupBox.Controls.Add(this.ExternalDebtorCodeTextBox);
			this.ExternalDebtorCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 285, true);
			this.ExternalDebtorCodeGroupBox.Name = "ExternalDebtorCodeGroupBox";
			this.ExternalDebtorCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 52, true);
			this.ExternalDebtorCodeGroupBox.TabIndex = 5;
			this.ExternalDebtorCodeGroupBox.TabStop = false;
			// 
			// ExternalDebtorCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExternalDebtorCodeTextBox, "CompanyData+OB_ARExternalDebtorCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARExternalDebtorCode)));
			this.ExternalDebtorCodeTextBox.CaptionResourceString = null;
			this.ExternalDebtorCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 19, true);
			this.ExternalDebtorCodeTextBox.Name = "ExternalDebtorCodeTextBox";
			this.ExternalDebtorCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.ExternalDebtorCodeTextBox.TabIndex = 0;
			// 
			// CompanyDataGroupBox
			// 
			this.CompanyDataGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|47436469-f916-4bfc-9bbd-c70244cd3c09", "Company Data");
			this.CompanyDataGroupBox.Controls.Add(this.zDropEdit_TransCreationRestriction);
			this.CompanyDataGroupBox.Controls.Add(this.AllowMultiCurrencyPaymentCheckBox);
			this.CompanyDataGroupBox.Controls.Add(this.ApplicableSurchargesPanel);
			this.CompanyDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 478, true);
			this.CompanyDataGroupBox.Name = "CompanyDataGroupBox";
			this.CompanyDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 95, true);
			this.CompanyDataGroupBox.TabIndex = 2;
			this.CompanyDataGroupBox.TabStop = false;
			// 
			// zDropEdit_TransCreationRestriction
			// 
			this.zDropEdit_TransCreationRestriction.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit_TransCreationRestriction, "CompanyData+OB_ARTransactionCreationRestriction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARTransactionCreationRestriction)));
			this.zDropEdit_TransCreationRestriction.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|eb1e46db-2cfd-4e74-a929-a2104167ed74", "Transaction Creation Restriction");
			this.zDropEdit_TransCreationRestriction.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 39, true);
			this.zDropEdit_TransCreationRestriction.Name = "zDropEdit_TransCreationRestriction";
			this.zDropEdit_TransCreationRestriction.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.zDropEdit_TransCreationRestriction.TabIndex = 1;
			// 
			// AllowMultiCurrencyPaymentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AllowMultiCurrencyPaymentCheckBox, "CompanyData+OB_ARAllowMultiCurrencyPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARAllowMultiCurrencyPayment)));
			this.AllowMultiCurrencyPaymentCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|1bc64719-67f7-4af2-8417-57c616f35729", "Allow Multiple Currency Payment");
			this.AllowMultiCurrencyPaymentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AllowMultiCurrencyPaymentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 12, true);
			this.AllowMultiCurrencyPaymentCheckBox.Name = "AllowMultiCurrencyPaymentCheckBox";
			this.AllowMultiCurrencyPaymentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.AllowMultiCurrencyPaymentCheckBox.TabIndex = 0;
			this.AllowMultiCurrencyPaymentCheckBox.UseVisualStyleBackColor = true;
			// 
			// ApplicableSurchargesPanel
			// 
			this.ApplicableSurchargesPanel.Controls.Add(this.ApplicableSurchargesTextBox);
			this.ApplicableSurchargesPanel.Controls.Add(this.ApplicableSurchargesEditButton);
			this.ApplicableSurchargesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 66, true);
			this.ApplicableSurchargesPanel.Name = "ApplicableSurchargesPanel";
			this.ApplicableSurchargesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 20, true);
			this.ApplicableSurchargesPanel.TabIndex = 2;
			// 
			// ApplicableSurchargesTextBox
			// 
			this.BindingSource.SetBindingMember(this.ApplicableSurchargesTextBox, "CompanyData+OB_ARApplicableSurcharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARApplicableSurcharges)));
			this.ApplicableSurchargesTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|80ed4ee4-2c13-4596-bd0d-704b81feb842", "Applicable Surcharges");
			this.ApplicableSurchargesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 0, true);
			this.ApplicableSurchargesTextBox.Name = "ApplicableSurchargesTextBox";
			this.ApplicableSurchargesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.ApplicableSurchargesTextBox.TabIndex = 0;
			// 
			// ApplicableSurchargesEditButton
			// 
			this.ApplicableSurchargesEditButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesConfigurationUserControl|6448c1f0-7027-4046-b258-487a03d4f16f", "More...");
			this.ApplicableSurchargesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 0, true);
			this.ApplicableSurchargesEditButton.Name = "ApplicableSurchargesEditButton";
			this.ApplicableSurchargesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.ApplicableSurchargesEditButton.TabIndex = 1;
			this.ApplicableSurchargesEditButton.ToolTipCaption = null;
			this.ApplicableSurchargesEditButton.Click += new System.EventHandler(this.ApplicableSurchargesEditButtonButton_Click);
			// 
			// ReceivablesConfigurationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "ReceivablesConfigurationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 610, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.ResumeLayout(true);
			this.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.PerformLayout();
			this.ARCurrencyUpliftGroupBox.ResumeLayout(false);
			this.ARCurrencyUpliftGroupBox.PerformLayout();
			this.cfxUpliftConfig.ResumeLayout(true);
			this.cfxUpliftConfig.PerformLayout();
			this.ARCreditTermsGroupBox.ResumeLayout(false);
			this.ARCreditTermsGroupBox.PerformLayout();
			this.ARAccountDetailsGrid.ResumeLayout(true);
			this.ARAccountDetailsGrid.PerformLayout();
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.ResumeLayout(true);
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.PerformLayout();
			this.PayToAccountGuidFindBox.ResumeLayout(true);
			this.PayToAccountGuidFindBox.PerformLayout();
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.ResumeLayout(true);
			this.OB_OJ_ARDebtorGroupBoundGuidFindBox.PerformLayout();
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.PerformLayout();
			this.OM_ARCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_ARCategoryBoundDropEdit.PerformLayout();
			this.ARQualityAssuranceGroupBox.ResumeLayout(false);
			this.ARQualityAssuranceGroupBox.PerformLayout();
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.ResumeLayout(true);
			this.OM_QualityAssuredCheckedDateBoundARDateEdit.PerformLayout();
			this.TaxDetailsGroupBox.ResumeLayout(false);
			this.TaxDetailsGroupBox.PerformLayout();
			this.OB_ARGoodsOwnershipDropEdit.ResumeLayout(true);
			this.OB_ARGoodsOwnershipDropEdit.PerformLayout();
			this.OB_ARVATConfigDropEdit.ResumeLayout(true);
			this.OB_ARVATConfigDropEdit.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ClientNumberGroupBox.ResumeLayout(false);
			this.ClientNumberGroupBox.PerformLayout();
			this.OM_CMAuthorityToLeaveGroupBox.ResumeLayout(false);
			this.OM_CMAuthorityToLeaveGroupBox.PerformLayout();
			this.OM_CMAuthorityToLeaveDropEdit.ResumeLayout(true);
			this.OM_CMAuthorityToLeaveDropEdit.PerformLayout();
			this.ExternalDebtorCodeGroupBox.ResumeLayout(false);
			this.ExternalDebtorCodeGroupBox.PerformLayout();
			this.CompanyDataGroupBox.ResumeLayout(false);
			this.CompanyDataGroupBox.PerformLayout();
			this.zDropEdit_TransCreationRestriction.ResumeLayout(true);
			this.zDropEdit_TransCreationRestriction.PerformLayout();
			this.ApplicableSurchargesPanel.ResumeLayout(false);
			this.ApplicableSurchargesPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
