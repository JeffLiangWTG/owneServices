namespace Enterprise.MasterFiles.GUI
{
	public partial class PayablesUserControl
	{

		#region Component Designer generated code

		private CargoWise.Windows.UI.KSplitContainer PayablesDetailsContainer;
		private CreditReportUserControl creditReportControl;
		private Enterprise.ZArchitecture.GUI.ZPanel PayablesDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl PayablesDetailsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage PayablesDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CreditorDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_ARConsolidatedAccountingCategoryBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_APCategoryBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OB_OG_APCreditorGroupGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APQualityAssuranceGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OM_QualityAssuredCheckedDateBoundAPDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OM_QualityAssuredBoundAPCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APOtherGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APPaymentTermsGroupBox;
		private Enterprise.ZArchitecture.ZLabel APPaymentsLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OB_APPaymentTermsBoundDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OB_APPaymentTermDaysBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APTaxDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OM_APWHTApplicableBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AccountDetailsGroupBox;
		internal Enterprise.MasterFiles.GUI.Organisation.UserControls.Payables.AccountDetailsGrid AccountDetailsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APAccountDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OM_AB_APDefaultBankAccountBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OM_AC_APDefaultChargeCodeBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox APSettlementGroupBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APCreditDetailsGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit OM_APCreditLimitBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OB_APCostsSelfBilledBoundCheckEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox OB_APPrintContractorFormCheckBox;
		internal ZArchitecture.ZLabel OB_APCreateVATComplianceDocumentOnPostingLabel;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit OB_APCreateVATComplianceDocumentOnPostingDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox OB_RX_NKAPDefltCurrencyCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ExternalCreditorCodeGroupBox;
		private ZArchitecture.ZTextBox ExternalCreditorCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PaymentMethodDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit OB_APVATConfigDropEdit;
		internal ZArchitecture.ZLabel OB_APVATConfigLabel;
		internal ZArchitecture.ZLabel lblNotAllowedToSeeAccDetails;
		private Enterprise.ZArchitecture.GUI.ZTabPage JobBillingExchangeRateConfigTabPage;
		private AccExRateConfigs accExRateConfigs;
		private Enterprise.ZArchitecture.GUI.ZTabPage taxConfigurationTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CompanyDataGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit_TransCreationRestriction;
		private CargoWise.Windows.UI.KSplitContainer taxConfigurationSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox accOrgTaxConfigurationGroup;
		private Enterprise.ZArchitecture.ZGrid accOrgTaxConfigurationGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox accOrgTaxRateGroup;
		private Enterprise.ZArchitecture.ZGrid accOrgTaxRateGrid;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox OB_APExcludeFromPaymentReportsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox taxConfigurationMainGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox taxConfigurationTemplateGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton redefaultFromTemplateButton;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.creditReportControl = new CreditReportUserControl();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PayablesDetailsContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PayablesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PayablesDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PayablesDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompanyDataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEdit_TransCreationRestriction = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExternalCreditorCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExternalCreditorCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreditorDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_APCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OB_OG_APCreditorGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.APQualityAssuranceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OM_QualityAssuredBoundAPCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APOtherGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OB_APPrintContractorFormCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OB_APExcludeFromPaymentReportsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OB_APCostsSelfBilledBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OB_APCreateVATComplianceDocumentOnPostingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APPaymentTermsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.APPaymentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OB_APPaymentTermsBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OB_APPaymentTermDaysBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.APTaxDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OB_APVATConfigLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OB_APVATConfigDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_APWHTApplicableBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AccountDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.lblNotAllowedToSeeAccDetails = new Enterprise.ZArchitecture.ZLabel();
			this.AccountDetailsGrid = new Enterprise.MasterFiles.GUI.Organisation.UserControls.Payables.AccountDetailsGrid();
			this.APAccountDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OB_RX_NKAPDefltCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.APSettlementGroupBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.APCreditDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_APCreditLimitBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JobBillingExchangeRateConfigTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.accExRateConfigs = new Enterprise.MasterFiles.GUI.AccExRateConfigs();
			this.taxConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.taxConfigurationMainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.redefaultFromTemplateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.taxConfigurationTemplateGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.taxConfigurationSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.accOrgTaxConfigurationGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.accOrgTaxConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.accOrgTaxRateGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.accOrgTaxRateGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PayablesDetailsTabControl.SuspendLayout();
			this.PayablesDetailsTabPage.SuspendLayout();
			this.CompanyDataGroupBox.SuspendLayout();
			this.zDropEdit_TransCreationRestriction.SuspendLayout();
			this.ExternalCreditorCodeGroupBox.SuspendLayout();
			this.CreditorDetailsGroupBox.SuspendLayout();
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.SuspendLayout();
			this.OM_APCategoryBoundDropEdit.SuspendLayout();
			this.OB_OG_APCreditorGroupGuidFindBox.SuspendLayout();
			this.APQualityAssuranceGroupBox.SuspendLayout();
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.SuspendLayout();
			this.APOtherGroupBox.SuspendLayout();
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.SuspendLayout();
			this.APPaymentTermsGroupBox.SuspendLayout();
			this.OB_APPaymentTermsBoundDropEdit.SuspendLayout();
			this.APTaxDetailsGroupBox.SuspendLayout();
			this.OB_APVATConfigDropEdit.SuspendLayout();
			this.AccountDetailsGroupBox.SuspendLayout();
			this.AccountDetailsGrid.SuspendLayout();
			this.APAccountDetailsGroupBox.SuspendLayout();
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.SuspendLayout();
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.SuspendLayout();
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.SuspendLayout();
			this.APSettlementGroupBoundGuidFindBox.SuspendLayout();
			this.APCreditDetailsGroupBox.SuspendLayout();
			this.PaymentMethodDropEdit.SuspendLayout();
			this.JobBillingExchangeRateConfigTabPage.SuspendLayout();
			this.accExRateConfigs.SuspendLayout();
			this.taxConfigurationTabPage.SuspendLayout();
			this.taxConfigurationMainGroupBox.SuspendLayout();
			this.taxConfigurationTemplateGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationSplitContainer)).BeginInit();
			this.taxConfigurationSplitContainer.Panel1.SuspendLayout();
			this.taxConfigurationSplitContainer.Panel2.SuspendLayout();
			this.taxConfigurationSplitContainer.SuspendLayout();
			this.accOrgTaxConfigurationGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxConfigurationGrid)).BeginInit();
			this.accOrgTaxConfigurationGrid.SuspendLayout();
			this.accOrgTaxRateGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxRateGrid)).BeginInit();
			this.accOrgTaxRateGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// PayablesDetailsTabControl
			// 
			this.PayablesDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PayablesDetailsTabControl.Controls.Add(this.PayablesDetailsTabPage);
			this.PayablesDetailsTabControl.Controls.Add(this.JobBillingExchangeRateConfigTabPage);
			this.PayablesDetailsTabControl.Controls.Add(this.taxConfigurationTabPage);
			this.PayablesDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PayablesDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.PayablesDetailsTabControl.Name = "PayablesDetailsTabControl";
			this.PayablesDetailsTabControl.SelectedIndex = 0;
			this.PayablesDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 563, true);
			this.PayablesDetailsTabControl.TabIndex = 14;
			// 
			// PayablesDetailsTabPage
			// 
			this.PayablesDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|c6931b37-1415-4026-9b9a-319934138a78", "Configuration");
			this.PayablesDetailsTabPage.Controls.Add(this.PayablesDetailsContainer);
			this.PayablesDetailsTabPage.Controls.Add(this.PayablesDetailsPanel);
			this.PayablesDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PayablesDetailsTabPage.Name = "PayablesDetailsTabPage";
			this.PayablesDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 536, true);
			this.PayablesDetailsTabPage.TabIndex = 0;
			// 
			// PayablesDetailsContainer
			// 
			this.PayablesDetailsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PayablesDetailsContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.PayablesDetailsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PayablesDetailsContainer.Name = "PayablesDetailsContainer";
			this.PayablesDetailsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 492, true);
			this.PayablesDetailsContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.PayablesDetailsContainer.TabIndex = 11;
			this.PayablesDetailsContainer.IsSplitterFixed = true;
			// 
			// PayablesDetailsContainer.Panel1
			// 
			this.PayablesDetailsContainer.Panel1.Controls.Add(creditReportControl);
			// 
			// PayablesDetailsContainer.Panel2
			// 
			this.PayablesDetailsContainer.Panel2.AutoScroll = true;
			this.PayablesDetailsContainer.Panel2.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 396, true);
			this.PayablesDetailsContainer.Panel2.Controls.Add(this.PayablesDetailsPanel);
			//
			// PayablesDetailsPanel
			//
			this.PayablesDetailsPanel.BackColor = System.Drawing.Color.Transparent;
			this.PayablesDetailsPanel.Controls.Add(this.CompanyDataGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.ExternalCreditorCodeGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.CreditorDetailsGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.APQualityAssuranceGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.APOtherGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.APPaymentTermsGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.APTaxDetailsGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.AccountDetailsGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.APAccountDetailsGroupBox);
			this.PayablesDetailsPanel.Controls.Add(this.APCreditDetailsGroupBox);
			this.PayablesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PayablesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PayablesDetailsPanel.Name = "PayablesDetailsPanel";
			this.PayablesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 396, true);
			this.PayablesDetailsPanel.TabIndex = 0;
			// 
			// CompanyDataGroupBox
			// 
			this.CompanyDataGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|943ca3d3-daf7-4c54-a647-002331e684eb", "Company Data");
			this.CompanyDataGroupBox.Controls.Add(this.zDropEdit_TransCreationRestriction);
			this.CompanyDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 447, true);
			this.CompanyDataGroupBox.Name = "CompanyDataGroupBox";
			this.CompanyDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 57, true);
			this.CompanyDataGroupBox.TabIndex = 9;
			this.CompanyDataGroupBox.TabStop = false;
			// 
			// zDropEdit_TransCreationRestriction
			// 
			this.zDropEdit_TransCreationRestriction.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit_TransCreationRestriction, "CompanyData+OB_APTransactionCreationRestriction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APTransactionCreationRestriction)));
			this.zDropEdit_TransCreationRestriction.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|6f985298-2ffb-4e32-92cb-110221c6a952", "Transaction Creation Restriction");
			this.zDropEdit_TransCreationRestriction.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 29, true);
			this.zDropEdit_TransCreationRestriction.Name = "zDropEdit_TransCreationRestriction";
			this.zDropEdit_TransCreationRestriction.ShouldResizeByMaxLength = true;
			this.zDropEdit_TransCreationRestriction.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.zDropEdit_TransCreationRestriction.TabIndex = 1;
			// 
			// ExternalCreditorCodeGroupBox
			// 
			this.ExternalCreditorCodeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|14B353D0-10E4-465F-987F-CE5A2E0CB01A", "External System Creditor Code");
			this.ExternalCreditorCodeGroupBox.Controls.Add(this.ExternalCreditorCodeTextBox);
			this.ExternalCreditorCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 441, true);
			this.ExternalCreditorCodeGroupBox.Name = "ExternalCreditorCodeGroupBox";
			this.ExternalCreditorCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 51, true);
			this.ExternalCreditorCodeGroupBox.TabIndex = 8;
			this.ExternalCreditorCodeGroupBox.TabStop = false;
			// 
			// ExternalCreditorCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExternalCreditorCodeTextBox, "CompanyData+OB_APExternalCreditorCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APExternalCreditorCode)));
			this.ExternalCreditorCodeTextBox.CaptionResourceString = null;
			this.ExternalCreditorCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 19, true);
			this.ExternalCreditorCodeTextBox.Name = "ExternalCreditorCodeTextBox";
			this.ExternalCreditorCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.ExternalCreditorCodeTextBox.TabIndex = 0;
			// 
			// CreditorDetailsGroupBox
			// 
			this.CreditorDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|7af17587-3d35-4912-a008-42673bba7ce1", "Creditor Details");
			this.CreditorDetailsGroupBox.Controls.Add(this.OM_ARConsolidatedAccountingCategoryBoundDropEdit);
			this.CreditorDetailsGroupBox.Controls.Add(this.OM_APCategoryBoundDropEdit);
			this.CreditorDetailsGroupBox.Controls.Add(this.OB_OG_APCreditorGroupGuidFindBox);
			this.CreditorDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CreditorDetailsGroupBox.Name = "CreditorDetailsGroupBox";
			this.CreditorDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 120, true);
			this.CreditorDetailsGroupBox.TabIndex = 0;
			this.CreditorDetailsGroupBox.TabStop = false;
			// 
			// OM_ARConsolidatedAccountingCategoryBoundDropEdit
			// 
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_ARConsolidatedAccountingCategoryBoundDropEdit, "MiscServ.OM_APConsolidatedAccountingCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_APConsolidatedAccountingCategory)));
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|1bd9a7cb-aea0-496e-af66-604369098ca8", "Consolidation Category", "AR  Consolidated Accounting Category.");
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 88, true);
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.Name = "OM_ARConsolidatedAccountingCategoryBoundDropEdit";
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.ShouldResizeByMaxLength = true;
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.TabIndex = 2;
			// 
			// OM_APCategoryBoundDropEdit
			// 
			this.OM_APCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_APCategoryBoundDropEdit, "MiscServ.OM_APCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_APCategory)));
			this.OM_APCategoryBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|43ef2357-78ee-488d-b7a9-a5528bf27c9e", "Accounts Relationship");
			this.OM_APCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 56, true);
			this.OM_APCategoryBoundDropEdit.Name = "OM_APCategoryBoundDropEdit";
			this.OM_APCategoryBoundDropEdit.ShouldResizeByMaxLength = true;
			this.OM_APCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.OM_APCategoryBoundDropEdit.TabIndex = 1;
			// 
			// OB_OG_APCreditorGroupGuidFindBox
			// 
			this.OB_OG_APCreditorGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_OG_APCreditorGroupGuidFindBox, "CompanyData+OB_OG_APCreditorGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_OG_APCreditorGroup)));
			this.OB_OG_APCreditorGroupGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|1b0d1bda-8e17-4889-8b81-f3afc4168655", "Account Group");
			this.OB_OG_APCreditorGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 24, true);
			this.OB_OG_APCreditorGroupGuidFindBox.Name = "OB_OG_APCreditorGroupGuidFindBox";
			this.OB_OG_APCreditorGroupGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OB_OG_APCreditorGroupGuidFindBox.ParentType = null;
			this.OB_OG_APCreditorGroupGuidFindBox.PopupCaption = "Select Creditor Group";
			this.OB_OG_APCreditorGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.OB_OG_APCreditorGroupGuidFindBox.TabIndex = 0;
			// 
			// APQualityAssuranceGroupBox
			// 
			this.APQualityAssuranceGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|6801a5eb-25f7-4283-860d-4889e78e53bc", "Quality Assurance");
			this.APQualityAssuranceGroupBox.Controls.Add(this.OM_QualityAssuredCheckedDateBoundAPDateEdit);
			this.APQualityAssuranceGroupBox.Controls.Add(this.OM_QualityAssuredBoundAPCheckBox);
			this.APQualityAssuranceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 385, true);
			this.APQualityAssuranceGroupBox.Name = "APQualityAssuranceGroupBox";
			this.APQualityAssuranceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 50, true);
			this.APQualityAssuranceGroupBox.TabIndex = 7;
			this.APQualityAssuranceGroupBox.TabStop = false;
			// 
			// OM_QualityAssuredCheckedDateBoundAPDateEdit
			// 
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.AllowDrop = true;
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.AutoCompleteMonthThreshold = 1;
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OM_QualityAssuredCheckedDateBoundAPDateEdit, "CompanyData+OB_APQualityAssuredCheckedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APQualityAssuredCheckedDate)));
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 18, true);
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.Name = "OM_QualityAssuredCheckedDateBoundAPDateEdit";
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.TabIndex = 1;
			// 
			// OM_QualityAssuredBoundAPCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_QualityAssuredBoundAPCheckBox, "CompanyData+OB_APQualityAssured");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APQualityAssured)));
			this.OM_QualityAssuredBoundAPCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_QualityAssuredBoundAPCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_QualityAssuredBoundAPCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.OM_QualityAssuredBoundAPCheckBox.Name = "OM_QualityAssuredBoundAPCheckBox";
			this.OM_QualityAssuredBoundAPCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.OM_QualityAssuredBoundAPCheckBox.TabIndex = 0;
			// 
			// APOtherGroupBox
			// 
			this.APOtherGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|521fa9aa-e71a-4aa8-b642-eda516fba565", "Other Details");
			this.APOtherGroupBox.Controls.Add(this.OB_APPrintContractorFormCheckBox);
			this.APOtherGroupBox.Controls.Add(this.OB_APExcludeFromPaymentReportsCheckBox);
			this.APOtherGroupBox.Controls.Add(this.OB_APCostsSelfBilledBoundCheckEdit);
			this.APOtherGroupBox.Controls.Add(this.OB_APCreateVATComplianceDocumentOnPostingLabel);
			this.APOtherGroupBox.Controls.Add(this.OB_APCreateVATComplianceDocumentOnPostingDropEdit);
			this.APOtherGroupBox.Controls.Add(this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit);
			this.APOtherGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 238, true);
			this.APOtherGroupBox.Name = "APOtherGroupBox";
			this.APOtherGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 144, true);
			this.APOtherGroupBox.TabIndex = 6;
			this.APOtherGroupBox.TabStop = false;
			// 
			// OB_APPrintContractorFormCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OB_APPrintContractorFormCheckBox, "CompanyData+OB_APPrintContractorForm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APPrintContractorForm)));
			this.OB_APPrintContractorFormCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|3b93a2d6-10be-4bcc-8aaf-5208db15165a", "Eligible IRS 1099-MISC Form Org.", "This flag identifies AP organizations that may require your login company to report what was paid to them in a calendar year by issuing IRS form 1099-MISC.");
			this.OB_APPrintContractorFormCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OB_APPrintContractorFormCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OB_APPrintContractorFormCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 92, true);
			this.OB_APPrintContractorFormCheckBox.Name = "OB_APPrintContractorFormCheckBox";
			this.OB_APPrintContractorFormCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.OB_APPrintContractorFormCheckBox.TabIndex = 3;
			this.OB_APPrintContractorFormCheckBox.UseVisualStyleBackColor = true;
			// 
			// OB_APExcludeFromPaymentReportsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OB_APExcludeFromPaymentReportsCheckBox, "CompanyData+OB_APExcludeFromPaymentReports");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APExcludeFromPaymentReports)));
			this.OB_APExcludeFromPaymentReportsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|1882cf87-ed78-4ecc-92ee-203150112b1b", "Exclude from PTRS Reporting", "This flag identifies AP organizations that should be excluded from reporting what was paid in a particular reporting period by your login company (AU: by submitting PTRS - Payment Times Reporting Scheme report).");
			this.OB_APExcludeFromPaymentReportsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OB_APExcludeFromPaymentReportsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OB_APExcludeFromPaymentReportsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 116, true);
			this.OB_APExcludeFromPaymentReportsCheckBox.Name = "OB_APExcludeFromPaymentReportsCheckBox";
			this.OB_APExcludeFromPaymentReportsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.OB_APExcludeFromPaymentReportsCheckBox.TabIndex = 8;
			this.OB_APExcludeFromPaymentReportsCheckBox.UseVisualStyleBackColor = true;
			// 
			// OB_APCostsSelfBilledBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OB_APCostsSelfBilledBoundCheckEdit, "CompanyData+OB_APCostsSelfBilled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APCostsSelfBilled)));
			this.OB_APCostsSelfBilledBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|b39901df-d8f2-4f21-9eb3-17b45c92366a", "Issue Self Billing Invoice");
			this.OB_APCostsSelfBilledBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OB_APCostsSelfBilledBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OB_APCostsSelfBilledBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 39, true);
			this.OB_APCostsSelfBilledBoundCheckEdit.Name = "OB_APCostsSelfBilledBoundCheckEdit";
			this.OB_APCostsSelfBilledBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.OB_APCostsSelfBilledBoundCheckEdit.TabIndex = 1;
			// 
			// OB_APCreateVATComplianceDocumentOnPostingLabel
			// 
			this.OB_APCreateVATComplianceDocumentOnPostingLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|8036aead-5615-4156-8720-78127d560627", "Create Compliance Document Record on Posting");
			this.OB_APCreateVATComplianceDocumentOnPostingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OB_APCreateVATComplianceDocumentOnPostingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 63, true);
			this.OB_APCreateVATComplianceDocumentOnPostingLabel.Name = "OB_APCreateVATComplianceDocumentOnPostingLabel";
			this.OB_APCreateVATComplianceDocumentOnPostingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 15, true);
			this.OB_APCreateVATComplianceDocumentOnPostingLabel.TabIndex = 7;
			// 
			// OB_APCreateVATComplianceDocumentOnPostingDropEdit
			// 
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_APCreateVATComplianceDocumentOnPostingDropEdit, "CompanyData+OB_APCreateVATComplianceDocumentOnPosting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APCreateVATComplianceDocumentOnPosting)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OB_APCreateVATComplianceDocumentOnPostingDropEdit, false);
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 82, true);
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.Name = "OB_APCreateVATComplianceDocumentOnPostingDropEdit";
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.ShouldResizeByMaxLength = true;
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.TabIndex = 6;
			// 
			// OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit, "MiscServ.OM_APPayInvoiceAfterPostingDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_APPayInvoiceAfterPostingDefault)));
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|5f590c33-acfc-4a85-8f83-6529b10eac86", "Pay Invoice After Posting", "AP Pay Invoice After Posting Default.");
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.Checked = true;
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.CheckState = System.Windows.Forms.CheckState.Checked;
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 15, true);
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.Name = "OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit";
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.OM_APPayInvoiceAfterPostingDefaultBoundCheckEdit.TabIndex = 0;
			// 
			// APPaymentTermsGroupBox
			// 
			this.APPaymentTermsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|6eb413e7-55fd-41a0-958f-f5e5eea8f1c5", "Payment Terms");
			this.APPaymentTermsGroupBox.Controls.Add(this.APPaymentsLabel);
			this.APPaymentTermsGroupBox.Controls.Add(this.OB_APPaymentTermsBoundDropEdit);
			this.APPaymentTermsGroupBox.Controls.Add(this.OB_APPaymentTermDaysBoundCalcEdit);
			this.APPaymentTermsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 85, true);
			this.APPaymentTermsGroupBox.Name = "APPaymentTermsGroupBox";
			this.APPaymentTermsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 52, true);
			this.APPaymentTermsGroupBox.TabIndex = 4;
			this.APPaymentTermsGroupBox.TabStop = false;
			// 
			// APPaymentsLabel
			// 
			this.APPaymentsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|7822ea31-9fcc-49b4-b997-b26ed8241e67", "Payment -");
			this.APPaymentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.APPaymentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 17, true);
			this.APPaymentsLabel.Name = "APPaymentsLabel";
			this.APPaymentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 23, true);
			this.APPaymentsLabel.TabIndex = 12;
			// 
			// OB_APPaymentTermsBoundDropEdit
			// 
			this.OB_APPaymentTermsBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_APPaymentTermsBoundDropEdit, "CompanyData+OB_APPaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APPaymentTerms)));
			this.OB_APPaymentTermsBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|06c241c5-40a4-4fb0-8df8-426a26e887b8", "Terms", "Payment Terms.");
			this.OB_APPaymentTermsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 19, true);
			this.OB_APPaymentTermsBoundDropEdit.Name = "OB_APPaymentTermsBoundDropEdit";
			this.OB_APPaymentTermsBoundDropEdit.PreBoundMaxLength = 3;
			this.OB_APPaymentTermsBoundDropEdit.ShouldResizeByMaxLength = true;
			this.OB_APPaymentTermsBoundDropEdit.ShowDescriptionBox = false;
			this.OB_APPaymentTermsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OB_APPaymentTermsBoundDropEdit.TabIndex = 0;
			// 
			// OB_APPaymentTermDaysBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OB_APPaymentTermDaysBoundCalcEdit, "CompanyData+OB_APPaymentTermDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APPaymentTermDays)));
			this.OB_APPaymentTermDaysBoundCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|adee0e93-2434-439f-9b2d-006974bce390", "Days", "Payment Term Days.");
			this.OB_APPaymentTermDaysBoundCalcEdit.DecimalPlaces = 0;
			this.OB_APPaymentTermDaysBoundCalcEdit.Decimals = 0;
			this.OB_APPaymentTermDaysBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 19, true);
			this.OB_APPaymentTermDaysBoundCalcEdit.Name = "OB_APPaymentTermDaysBoundCalcEdit";
			this.OB_APPaymentTermDaysBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OB_APPaymentTermDaysBoundCalcEdit.TabIndex = 1;
			this.OB_APPaymentTermDaysBoundCalcEdit.Text = "0";
			this.OB_APPaymentTermDaysBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// APTaxDetailsGroupBox
			// 
			this.APTaxDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|500fe226-8fbc-490d-839f-a3830973a9f3", "Tax Details");
			this.APTaxDetailsGroupBox.Controls.Add(this.OB_APVATConfigLabel);
			this.APTaxDetailsGroupBox.Controls.Add(this.OB_APVATConfigDropEdit);
			this.APTaxDetailsGroupBox.Controls.Add(this.OM_APWHTApplicableBoundCheckEdit);
			this.APTaxDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 143, true);
			this.APTaxDetailsGroupBox.Name = "APTaxDetailsGroupBox";
			this.APTaxDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 89, true);
			this.APTaxDetailsGroupBox.TabIndex = 5;
			this.APTaxDetailsGroupBox.TabStop = false;
			// 
			// OB_APVATConfigLabel
			// 
			this.BindingSource.SetBindingMember(this.OB_APVATConfigLabel, "CompanyData+OB_VATConfigLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_VATConfigLabel)));
			this.OB_APVATConfigLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OB_APVATConfigLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.OB_APVATConfigLabel.Name = "OB_APVATConfigLabel";
			this.OB_APVATConfigLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 15, true);
			this.OB_APVATConfigLabel.TabIndex = 4;
			// 
			// OB_APVATConfigDropEdit
			// 
			this.OB_APVATConfigDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_APVATConfigDropEdit, "CompanyData+OB_APVATConfig");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APVATConfig)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OB_APVATConfigDropEdit, false);
			this.OB_APVATConfigDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 34, true);
			this.OB_APVATConfigDropEdit.Name = "OB_APVATConfigDropEdit";
			this.OB_APVATConfigDropEdit.ShouldResizeByMaxLength = true;
			this.OB_APVATConfigDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.OB_APVATConfigDropEdit.TabIndex = 0;
			// 
			// OM_APWHTApplicableBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_APWHTApplicableBoundCheckEdit, "MiscServ.OM_APWHTApplicable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_APWHTApplicable)));
			this.OM_APWHTApplicableBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|c926b78d-9724-4f40-a284-361fa678b5d8", "Withholding Tax Is Applicable");
			this.OM_APWHTApplicableBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_APWHTApplicableBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_APWHTApplicableBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 60, true);
			this.OM_APWHTApplicableBoundCheckEdit.Name = "OM_APWHTApplicableBoundCheckEdit";
			this.OM_APWHTApplicableBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.OM_APWHTApplicableBoundCheckEdit.TabIndex = 1;
			// 
			// AccountDetailsGroupBox
			// 
			this.AccountDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|ccf69801-0eea-4b15-969b-1db9b195c8a6", "Account Details");
			this.AccountDetailsGroupBox.Controls.Add(this.lblNotAllowedToSeeAccDetails);
			this.AccountDetailsGroupBox.Controls.Add(this.AccountDetailsGrid);
			this.AccountDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 263, true);
			this.AccountDetailsGroupBox.Name = "AccountDetailsGroupBox";
			this.AccountDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 180, true);
			this.AccountDetailsGroupBox.TabIndex = 2;
			this.AccountDetailsGroupBox.TabStop = false;
			// 
			// lblNotAllowedToSeeAccDetails
			// 
			this.lblNotAllowedToSeeAccDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lblNotAllowedToSeeAccDetails.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblNotAllowedToSeeAccDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.lblNotAllowedToSeeAccDetails.Name = "lblNotAllowedToSeeAccDetails";
			this.lblNotAllowedToSeeAccDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 156, true);
			this.lblNotAllowedToSeeAccDetails.TabIndex = 1;
			this.lblNotAllowedToSeeAccDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblNotAllowedToSeeAccDetails.Visible = false;
			// 
			// AccountDetailsGrid
			// 
			this.AccountDetailsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccountDetailsGrid, "CurrentCompanyDataAsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgCompanyData)(((Enterprise.MasterFiles.Business.OrgCompanyData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CurrentCompanyDataAsCollection)).SyncRoot)))));
			this.AccountDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.AccountDetailsGrid.Name = "AccountDetailsGrid";
			this.AccountDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 155, true);
			this.AccountDetailsGrid.TabIndex = 0;
			// 
			// APAccountDetailsGroupBox
			// 
			this.APAccountDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|092747c8-fbc7-4313-90f0-52b73cf98855", "Defaults");
			this.APAccountDetailsGroupBox.Controls.Add(this.OB_RX_NKAPDefltCurrencyCodeFindBox);
			this.APAccountDetailsGroupBox.Controls.Add(this.OM_AB_APDefaultBankAccountBoundGuidFindBox);
			this.APAccountDetailsGroupBox.Controls.Add(this.OM_AC_APDefaultChargeCodeBoundGuidFindBox);
			this.APAccountDetailsGroupBox.Controls.Add(this.APSettlementGroupBoundGuidFindBox);
			this.APAccountDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 129, true);
			this.APAccountDetailsGroupBox.Name = "APAccountDetailsGroupBox";
			this.APAccountDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 128, true);
			this.APAccountDetailsGroupBox.TabIndex = 1;
			this.APAccountDetailsGroupBox.TabStop = false;
			// 
			// OB_RX_NKAPDefltCurrencyCodeFindBox
			// 
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_RX_NKAPDefltCurrencyCodeFindBox, "CompanyData+OB_RX_NKAPDefltCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_RX_NKAPDefltCurrency)));
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 19, true);
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.Name = "OB_RX_NKAPDefltCurrencyCodeFindBox";
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.ParentType = null;
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.PopupCaption = "Select Currency";
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.TabIndex = 0;
			// 
			// OM_AB_APDefaultBankAccountBoundGuidFindBox
			// 
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_AB_APDefaultBankAccountBoundGuidFindBox, "MiscServ.OM_AB_APDefaultBankAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_AB_APDefaultBankAccount)));
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|164962d7-548f-400d-9f5a-b602aa13b1bb", "Bank Account", "AP Default Bank Account.");
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 45, true);
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.Name = "OM_AB_APDefaultBankAccountBoundGuidFindBox";
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.ParentType = null;
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.PopupCaption = "Select Bank Account";
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.TabIndex = 1;
			// 
			// OM_AC_APDefaultChargeCodeBoundGuidFindBox
			// 
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_AC_APDefaultChargeCodeBoundGuidFindBox, "MiscServ.OM_AC_APDefaultChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_AC_APDefaultChargeCode)));
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|be12f103-9da7-4e2b-90a2-3bcd03a411cb", "Charge Code", "AP Default Charge Code.");
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 71, true);
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.Name = "OM_AC_APDefaultChargeCodeBoundGuidFindBox";
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.ParentType = null;
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.PopupCaption = "Select Charge Code";
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.TabIndex = 2;
			// 
			// APSettlementGroupBoundGuidFindBox
			// 
			this.APSettlementGroupBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.APSettlementGroupBoundGuidFindBox, "APSettlementGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).APSettlementGroupPK)));
			this.APSettlementGroupBoundGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|a4b7e83f-dde9-4fd2-b500-f7c588511722", "Settlement Group");
			this.APSettlementGroupBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 97, true);
			this.APSettlementGroupBoundGuidFindBox.Name = "APSettlementGroupBoundGuidFindBox";
			this.APSettlementGroupBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.APSettlementGroupBoundGuidFindBox.ParentType = null;
			this.APSettlementGroupBoundGuidFindBox.PopupCaption = "Select Settlement Group";
			this.APSettlementGroupBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.APSettlementGroupBoundGuidFindBox.TabIndex = 3;
			// 
			// APCreditDetailsGroupBox
			// 
			this.APCreditDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|a0776869-8fd9-4caf-8866-7fba12f6acdd", "Credit Details");
			this.APCreditDetailsGroupBox.Controls.Add(this.PaymentMethodDropEdit);
			this.APCreditDetailsGroupBox.Controls.Add(this.OM_APCreditLimitBoundCalcEdit);
			this.APCreditDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 3, true);
			this.APCreditDetailsGroupBox.Name = "APCreditDetailsGroupBox";
			this.APCreditDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 76, true);
			this.APCreditDetailsGroupBox.TabIndex = 3;
			this.APCreditDetailsGroupBox.TabStop = false;
			// 
			// PaymentMethodDropEdit
			// 
			this.PaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentMethodDropEdit, "CompanyData+OB_APCreditAgreedPaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_APCreditAgreedPaymentMethod)));
			this.PaymentMethodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4170b93d-cdc9-4b0d-ad87-e7aed93da9bb", "Agreed Payment Method", "AP Credit Agreed Payment Method", "");
			this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 50, true);
			this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
			this.PaymentMethodDropEdit.PreBoundMaxLength = 3;
			this.PaymentMethodDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.PaymentMethodDropEdit.TabIndex = 10;
			// 
			// OM_APCreditLimitBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_APCreditLimitBoundCalcEdit, "MiscServ.OM_APCreditLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_APCreditLimit)));
			this.OM_APCreditLimitBoundCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PayablesUserControl|5257e42a-9c4a-461a-bcf6-257036e17fa9", "Credit Limit");
			this.OM_APCreditLimitBoundCalcEdit.DecimalPlaces = 2;
			this.OM_APCreditLimitBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 24, true);
			this.OM_APCreditLimitBoundCalcEdit.Name = "OM_APCreditLimitBoundCalcEdit";
			this.OM_APCreditLimitBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.OM_APCreditLimitBoundCalcEdit.TabIndex = 0;
			this.OM_APCreditLimitBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobBillingExchangeRateConfigTabPage
			// 
			this.JobBillingExchangeRateConfigTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dfd669c9-6c92-42a5-bac1-bb4aeeb7ea2b", "Job Billing Exchange Rate Configuration");
			this.JobBillingExchangeRateConfigTabPage.Controls.Add(this.accExRateConfigs);
			this.JobBillingExchangeRateConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JobBillingExchangeRateConfigTabPage.Name = "JobBillingExchangeRateConfigTabPage";
			this.JobBillingExchangeRateConfigTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JobBillingExchangeRateConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 536, true);
			this.JobBillingExchangeRateConfigTabPage.TabIndex = 1;
			this.JobBillingExchangeRateConfigTabPage.UseVisualStyleBackColor = true;
			// 
			// accExRateConfigs
			// 
			this.accExRateConfigs.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accExRateConfigs, "CompanyData.AccAPExchangeRateConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AccAPExchangeRateConfigurations)));
			this.accExRateConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accExRateConfigs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.accExRateConfigs.Name = "accExRateConfigs";
			this.accExRateConfigs.ReadOnly = false;
			this.accExRateConfigs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 530, true);
			this.accExRateConfigs.TabIndex = 4;
			// 
			// taxConfigurationTabPage
			// 
			this.taxConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a0572e13-16ce-43c3-b251-fdaef7319a21", "Tax Configuration");
			this.taxConfigurationTabPage.Controls.Add(this.taxConfigurationMainGroupBox);
			this.taxConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.taxConfigurationTabPage.Name = "taxConfigurationTabPage";
			this.taxConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 536, true);
			this.taxConfigurationTabPage.TabIndex = 2;
			this.taxConfigurationTabPage.RunWhenBindingOrFirstShown(TaxConfigurationTabPage_InitializeTab);
			// 
			// taxConfigurationMainGroupBox
			// 
			this.taxConfigurationMainGroupBox.Controls.Add(this.redefaultFromTemplateButton);
			this.taxConfigurationMainGroupBox.Controls.Add(this.taxConfigurationTemplateGuidFindBox);
			this.taxConfigurationMainGroupBox.Controls.Add(this.taxConfigurationSplitContainer);
			this.taxConfigurationMainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taxConfigurationMainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.taxConfigurationMainGroupBox.Name = "taxConfigurationMainGroupBox";
			this.taxConfigurationMainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 536, true);
			this.taxConfigurationMainGroupBox.TabIndex = 3;
			this.taxConfigurationMainGroupBox.TabStop = false;
			// 
			// redefaultFromTemplateButton
			// 
			this.redefaultFromTemplateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0fe0bba9-228c-44b6-ad15-51036be6bbe8", "Re-default from Template");
			this.redefaultFromTemplateButton.IsCaptionOverridden = false;
			this.redefaultFromTemplateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 19, true);
			this.redefaultFromTemplateButton.Name = "redefaultFromTemplateButton";
			this.redefaultFromTemplateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.redefaultFromTemplateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 24, true);
			this.redefaultFromTemplateButton.TabIndex = 2;
			this.redefaultFromTemplateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.redefaultFromTemplateButton.ToolTipCaption = null;
			this.redefaultFromTemplateButton.UseVisualStyleBackColor = true;
			this.redefaultFromTemplateButton.Click += new System.EventHandler(this.RedefaultFromTemplateButton_Click);
			// 
			// taxConfigurationTemplateGuidFindBox
			// 
			this.taxConfigurationTemplateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.taxConfigurationTemplateGuidFindBox, "CompanyData.OB_OCT_APTaxTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_OCT_APTaxTemplate)));
			this.taxConfigurationTemplateGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f2f5bb5a-b492-465d-a2a9-4fbc27f169cc", "Tax Configuration Template");
			this.taxConfigurationTemplateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 23, true);
			this.taxConfigurationTemplateGuidFindBox.Name = "taxConfigurationTemplateGuidFindBox";
			this.taxConfigurationTemplateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.taxConfigurationTemplateGuidFindBox.ParentType = null;
			this.taxConfigurationTemplateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 20, true);
			this.taxConfigurationTemplateGuidFindBox.TabIndex = 1;
			// 
			// taxConfigurationSplitContainer
			// 
			this.taxConfigurationSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.taxConfigurationSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.taxConfigurationSplitContainer.Name = "taxConfigurationSplitContainer";
			// 
			// taxConfigurationSplitContainer.Panel1
			// 
			this.taxConfigurationSplitContainer.Panel1.Controls.Add(this.accOrgTaxConfigurationGroup);
			// 
			// taxConfigurationSplitContainer.Panel2
			// 
			this.taxConfigurationSplitContainer.Panel2.Controls.Add(this.accOrgTaxRateGroup);
			this.taxConfigurationSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 478, true);
			this.taxConfigurationSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(272);
			this.taxConfigurationSplitContainer.TabIndex = 0;
			// 
			// accOrgTaxConfigurationGroup
			// 
			this.accOrgTaxConfigurationGroup.Controls.Add(this.accOrgTaxConfigurationGrid);
			this.accOrgTaxConfigurationGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxConfigurationGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.accOrgTaxConfigurationGroup.Name = "accOrgTaxConfigurationGroup";
			this.accOrgTaxConfigurationGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 478, true);
			this.accOrgTaxConfigurationGroup.TabIndex = 5;
			this.accOrgTaxConfigurationGroup.TabStop = false;
			// 
			// accOrgTaxConfigurationGrid
			// 
			this.accOrgTaxConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.accOrgTaxConfigurationGrid, "CompanyData.APOrgTaxConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).OTC_ETC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).OTC_ETC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).OTC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).OTC_IsThresholdUsed)));
			this.accOrgTaxConfigurationGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "OTC_ETC";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo1.ColumnName = "OTC_ETC_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo1.ColumnName = "OTC_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo2.ColumnName = "OTC_IsThresholdUsed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.accOrgTaxConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxConfigurationGrid.GridId = "3fb1e733-bd99-4c76-bb98-75f72508cfb6";
			this.accOrgTaxConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.accOrgTaxConfigurationGrid.LayoutKey = "accOrgTaxConfigurationGrid";
			this.accOrgTaxConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.accOrgTaxConfigurationGrid.Name = "accOrgTaxConfigurationGrid";
			this.accOrgTaxConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 459, true);
			this.accOrgTaxConfigurationGrid.TabIndex = 2;
			// 
			// accOrgTaxRateGroup
			// 
			this.accOrgTaxRateGroup.Controls.Add(this.accOrgTaxRateGrid);
			this.accOrgTaxRateGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxRateGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.accOrgTaxRateGroup.Name = "accOrgTaxRateGroup";
			this.accOrgTaxRateGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 478, true);
			this.accOrgTaxRateGroup.TabIndex = 5;
			this.accOrgTaxRateGroup.TabStop = false;
			// 
			// accOrgTaxRateGrid
			// 
			this.accOrgTaxRateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.accOrgTaxRateGrid, "CompanyData.APOrgTaxConfigurations.TaxRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).TaxRates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_RateNumerator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_RateDenominator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.APOrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).Rate)));
			this.accOrgTaxRateGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "OTR_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "OTR_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "OTR_Source";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OTR_RateNumerator";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OTR_RateDenominator";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Rate";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.accOrgTaxRateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxRateGrid.GridId = "3fb1e733-bd99-4c76-bb98-75f72508cfb6";
			this.accOrgTaxRateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.accOrgTaxRateGrid.LayoutKey = "accOrgTaxConfigurationGrid";
			this.accOrgTaxRateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.accOrgTaxRateGrid.Name = "accOrgTaxRateGrid";
			this.accOrgTaxRateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 459, true);
			this.accOrgTaxRateGrid.TabIndex = 2;
			// 
			// PayablesUserControl
			// 
			this.Controls.Add(this.PayablesDetailsTabControl);
			this.IsModifyPayables = true;
			this.Name = "PayablesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 587, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.PayablesDetailsTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PayablesDetailsTabControl.ResumeLayout(false);
			this.PayablesDetailsTabControl.PerformLayout();
			this.PayablesDetailsTabPage.ResumeLayout(false);
			this.PayablesDetailsTabPage.PerformLayout();
			this.CompanyDataGroupBox.ResumeLayout(false);
			this.CompanyDataGroupBox.PerformLayout();
			this.zDropEdit_TransCreationRestriction.ResumeLayout(true);
			this.zDropEdit_TransCreationRestriction.PerformLayout();
			this.ExternalCreditorCodeGroupBox.ResumeLayout(false);
			this.ExternalCreditorCodeGroupBox.PerformLayout();
			this.CreditorDetailsGroupBox.ResumeLayout(false);
			this.CreditorDetailsGroupBox.PerformLayout();
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_ARConsolidatedAccountingCategoryBoundDropEdit.PerformLayout();
			this.OM_APCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_APCategoryBoundDropEdit.PerformLayout();
			this.OB_OG_APCreditorGroupGuidFindBox.ResumeLayout(true);
			this.OB_OG_APCreditorGroupGuidFindBox.PerformLayout();
			this.APQualityAssuranceGroupBox.ResumeLayout(false);
			this.APQualityAssuranceGroupBox.PerformLayout();
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.ResumeLayout(true);
			this.OM_QualityAssuredCheckedDateBoundAPDateEdit.PerformLayout();
			this.APOtherGroupBox.ResumeLayout(false);
			this.APOtherGroupBox.PerformLayout();
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.ResumeLayout(true);
			this.OB_APCreateVATComplianceDocumentOnPostingDropEdit.PerformLayout();
			this.APPaymentTermsGroupBox.ResumeLayout(false);
			this.APPaymentTermsGroupBox.PerformLayout();
			this.OB_APPaymentTermsBoundDropEdit.ResumeLayout(true);
			this.OB_APPaymentTermsBoundDropEdit.PerformLayout();
			this.APTaxDetailsGroupBox.ResumeLayout(false);
			this.APTaxDetailsGroupBox.PerformLayout();
			this.OB_APVATConfigDropEdit.ResumeLayout(true);
			this.OB_APVATConfigDropEdit.PerformLayout();
			this.AccountDetailsGroupBox.ResumeLayout(false);
			this.AccountDetailsGroupBox.PerformLayout();
			this.AccountDetailsGrid.ResumeLayout(true);
			this.AccountDetailsGrid.PerformLayout();
			this.APAccountDetailsGroupBox.ResumeLayout(false);
			this.APAccountDetailsGroupBox.PerformLayout();
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.ResumeLayout(true);
			this.OB_RX_NKAPDefltCurrencyCodeFindBox.PerformLayout();
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.ResumeLayout(true);
			this.OM_AB_APDefaultBankAccountBoundGuidFindBox.PerformLayout();
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.ResumeLayout(true);
			this.OM_AC_APDefaultChargeCodeBoundGuidFindBox.PerformLayout();
			this.APSettlementGroupBoundGuidFindBox.ResumeLayout(true);
			this.APSettlementGroupBoundGuidFindBox.PerformLayout();
			this.APCreditDetailsGroupBox.ResumeLayout(false);
			this.APCreditDetailsGroupBox.PerformLayout();
			this.PaymentMethodDropEdit.ResumeLayout(true);
			this.PaymentMethodDropEdit.PerformLayout();
			this.JobBillingExchangeRateConfigTabPage.ResumeLayout(false);
			this.JobBillingExchangeRateConfigTabPage.PerformLayout();
			this.accExRateConfigs.ResumeLayout(true);
			this.accExRateConfigs.PerformLayout();
			this.taxConfigurationTabPage.ResumeLayout(false);
			this.taxConfigurationTabPage.PerformLayout();
			this.taxConfigurationMainGroupBox.ResumeLayout(false);
			this.taxConfigurationMainGroupBox.PerformLayout();
			this.taxConfigurationTemplateGuidFindBox.ResumeLayout(true);
			this.taxConfigurationTemplateGuidFindBox.PerformLayout();
			this.taxConfigurationSplitContainer.Panel1.ResumeLayout(false);
			this.taxConfigurationSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationSplitContainer)).EndInit();
			this.taxConfigurationSplitContainer.ResumeLayout(false);
			this.taxConfigurationSplitContainer.PerformLayout();
			this.accOrgTaxConfigurationGroup.ResumeLayout(false);
			this.accOrgTaxConfigurationGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxConfigurationGrid)).EndInit();
			this.accOrgTaxConfigurationGrid.ResumeLayout(false);
			this.accOrgTaxConfigurationGrid.PerformLayout();
			this.accOrgTaxRateGroup.ResumeLayout(false);
			this.accOrgTaxRateGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxRateGrid)).EndInit();
			this.accOrgTaxRateGrid.ResumeLayout(false);
			this.accOrgTaxRateGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
