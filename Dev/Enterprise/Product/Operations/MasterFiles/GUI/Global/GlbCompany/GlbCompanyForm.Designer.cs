namespace Enterprise.MasterFiles.GUI
{
	partial class GlbCompanyForm
	{
		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.GUI.ZDateEdit IncorporationDateEdit;
		private System.ComponentModel.IContainer components;
		private Enterprise.ZArchitecture.GUI.ZTabPage CompanyInfoTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage AccountingConfigurationTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage AccountingFeeTabPage;
		private Enterprise.ZArchitecture.ZTextBox GC_CustomsRegistrationNoTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GC_IsReciprocalCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GC_IsActiveCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl CompanyTabControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl AccConfigTabControl;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox GC_OHGuidFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox GC_RX_NKLocalCurrencyCodeFindBox;
		internal Enterprise.MasterFiles.GUI.PhoneNumberUserControl FaxNumberControl;
		private Enterprise.ZArchitecture.ZTextBox GC_BusinessRegNoTextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_CodeTextBox;
		internal Enterprise.MasterFiles.GUI.PhoneNumberUserControl PhoneNumberControl;
		private Enterprise.ZArchitecture.ZTextBox GC_NameTextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_Address1TextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_Address2TextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_CityTextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_PostCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_EmailTextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_WebAddressTextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_BusinessRegNo2TextBox;
		private Enterprise.ZArchitecture.ZTextBox GC_StateTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth GC_StateDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GC_IsGSTRegisteredCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GC_IsWHTRegisteredCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GC_IsGSTCashBasisCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GC_IsWHTCashBasisCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GC_IsWHTAccrualBasisCheckBox;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid GlbBranchModuleButtonGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BranchesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox GC_RN_NKCountryCodeCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CountryGroupBox;
		private Enterprise.ZArchitecture.ZLabel CountrySettingLabel;
		private Enterprise.MasterFiles.GUI.AccountFeeControl ctlAccountFee;
		private Enterprise.ZArchitecture.GUI.ZButton ValidateAddressButton;
		private Enterprise.ZArchitecture.GUI.ZButton ClearFieldsButton;
		private Enterprise.ZArchitecture.GUI.ZButton NewOrgProxyButton;
		private ZArchitecture.GUI.ZTabPage cfxUpliftConfigTabPage;
		private AccCFXUpliftCfg cfxUpliftConfig;
		ZArchitecture.GUI.ZTabPage jobBillingExRatesTabPage;
		AccExRateConfigs accExRateConfigs;
		internal ZArchitecture.GUI.ZTabPage NumberRangesTabPage;
		internal CustomsNumberViewStmNumsTabPageUserControl customsNumberViewStmNumsTabPageUserControl;
		private ZArchitecture.GUI.ZTabPage taxConfigurationTabPage;
		private ZArchitecture.ZGrid taxConfigurationsGrid;
		private ZArchitecture.GUI.ZTabPage surchargeTabPage;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl surchargeTabControl;
		private ZArchitecture.GUI.ZTabPage surchargeApplicationTabPage;
		private AccSurchargeApplicationUserControl accSurchargeApplicationGrid;
		private ZArchitecture.GUI.ZTabPage surchargeConfigurationTabPage;
		private AccSurchargeConfigurationUserControl accSurchargeConfiguration;
		private AccSurchargeBasisUserControl accSurchargeBasis;
		private ZArchitecture.GUI.ZTabPage EInvoiceCredentialsForTurkeyTabPage;
		private MasterFiles.GUI.GlbCompanySignatureCredentialUserControl EInvoiceCredentialsForTurkeyUserControl;
		private ZArchitecture.GUI.ZTabPage EInvoiceCredentialsForHungaryTabPage;
		private MasterFiles.GUI.GlbCompany_HungaryCredentialUserControl EInvoiceCredentialsForHungaryUserControl;
		private ZArchitecture.GUI.ZTabPage EInvoiceCredentialsForPhilippinesTabPage;
		private MasterFiles.GUI.GlbCompany_PhilippinesCredentialUserControl EInvoiceCredentialsForPhilippinesUserControl;
		private ZArchitecture.GUI.ZTabPage EInvoiceOAuthAuthorizationTabPage;
		private MasterFiles.GUI.GlbCompany_EInvoicingOAuthAuthorizationUserControl EInvoicingOAuthAuthorizationUserControl;
		private ZArchitecture.GUI.ZTabPage PlaceOfSupplyConfigurationTabPage;
		private MasterFiles.GUI.AccPlaceOfSupplyConfigurationControl PlaceOfSupplyConfiguration;
		private ZArchitecture.GUI.ZTabPage ARInvTemplateTabPage;
		private ZArchitecture.GUI.ZTemplateTabControl ARInvoiceTemplateConfigTabControl;
		private ZArchitecture.GUI.ZTabPage XSLTTemplateUploadTabPage;
		private ZArchitecture.GUI.ZTabPage XSLTFileConfigurationTabPage;
		private MasterFiles.GUI.TemplateFileUserControl TemplateFileUploadControl;
		private MasterFiles.GUI.TemplateConfigurationUserControl TemplateConfigurationUserControl;
		private ZArchitecture.GUI.ZTabPage EInvoiceCertificatesTabPage;
		private Enterprise.MasterFiles.GUI.GlbCompanyForm_CredentialUserControl CompanyEInvoicingCredentialUserControl;

		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.CompanyInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CountryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GC_RX_NKLocalCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GC_IsReciprocalCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CountrySettingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BranchesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GlbBranchModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.GC_IsWHTAccrualBasisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncorporationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GC_IsGSTRegisteredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GC_OHGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GC_IsWHTCashBasisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FaxNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.GC_IsWHTRegisteredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GC_IsGSTCashBasisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GC_BusinessRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.GC_CustomsRegistrationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GC_NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_RN_NKCountryCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GC_EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_WebAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_BusinessRegNo2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_StateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GC_StateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewOrgProxyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AccountingConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AccConfigTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.AccountingFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ctlAccountFee = new Enterprise.MasterFiles.GUI.AccountFeeControl();
			this.jobBillingExRatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.accExRateConfigs = new Enterprise.MasterFiles.GUI.AccExRateConfigs();
			this.cfxUpliftConfigTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cfxUpliftConfig = new Enterprise.MasterFiles.GUI.AccCFXUpliftCfg();
			this.PlaceOfSupplyConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PlaceOfSupplyConfiguration = new Enterprise.MasterFiles.GUI.AccPlaceOfSupplyConfigurationControl();
			this.taxConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.taxConfigurationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.surchargeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.surchargeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.surchargeConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.accSurchargeConfiguration = new Enterprise.MasterFiles.GUI.AccSurchargeConfigurationUserControl();
			this.accSurchargeBasis = new Enterprise.MasterFiles.GUI.AccSurchargeBasisUserControl();
			this.surchargeApplicationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.accSurchargeApplicationGrid = new Enterprise.MasterFiles.GUI.AccSurchargeApplicationUserControl();
			this.EInvoiceCredentialsForTurkeyTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EInvoiceCredentialsForTurkeyUserControl = new Enterprise.MasterFiles.GUI.GlbCompanySignatureCredentialUserControl();
			this.EInvoiceCredentialsForHungaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EInvoiceCredentialsForHungaryUserControl = new Enterprise.MasterFiles.GUI.GlbCompany_HungaryCredentialUserControl();
			this.EInvoiceCredentialsForPhilippinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EInvoiceCredentialsForPhilippinesUserControl = new Enterprise.MasterFiles.GUI.GlbCompany_PhilippinesCredentialUserControl();
			this.EInvoiceOAuthAuthorizationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EInvoicingOAuthAuthorizationUserControl = new Enterprise.MasterFiles.GUI.GlbCompany_EInvoicingOAuthAuthorizationUserControl();
			this.ARInvTemplateTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ARInvoiceTemplateConfigTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.XSLTTemplateUploadTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TemplateFileUploadControl = new Enterprise.MasterFiles.GUI.TemplateFileUserControl();
			this.XSLTFileConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TemplateConfigurationUserControl = new Enterprise.MasterFiles.GUI.TemplateConfigurationUserControl();
			this.EInvoiceCertificatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompanyEInvoicingCredentialUserControl = new Enterprise.MasterFiles.GUI.GlbCompanyForm_CredentialUserControl();
			this.CashAdvanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cashAdvanceJobConfig1 = new Enterprise.MasterFiles.GUI.CashAdvanceJobConfig();
			this.CompanyTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.NumberRangesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.customsNumberViewStmNumsTabPageUserControl = new Enterprise.MasterFiles.GUI.CustomsNumberViewStmNumsTabPageUserControl();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompanyInfoTabPage.SuspendLayout();
			this.CountryGroupBox.SuspendLayout();
			this.GC_RX_NKLocalCurrencyCodeFindBox.SuspendLayout();
			this.BranchesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GlbBranchModuleButtonGrid.InnerGrid)).BeginInit();
			this.GlbBranchModuleButtonGrid.SuspendLayout();
			this.IncorporationDateEdit.SuspendLayout();
			this.GC_OHGuidFindBox.SuspendLayout();
			this.FaxNumberControl.SuspendLayout();
			this.PhoneNumberControl.SuspendLayout();
			this.GC_RN_NKCountryCodeCodeFindBox.SuspendLayout();
			this.GC_StateDropEdit.SuspendLayout();
			this.AccountingConfigurationTabPage.SuspendLayout();
			this.AccConfigTabControl.SuspendLayout();
			this.AccountingFeeTabPage.SuspendLayout();
			this.ctlAccountFee.SuspendLayout();
			this.jobBillingExRatesTabPage.SuspendLayout();
			this.accExRateConfigs.SuspendLayout();
			this.cfxUpliftConfigTabPage.SuspendLayout();
			this.cfxUpliftConfig.SuspendLayout();
			this.PlaceOfSupplyConfigurationTabPage.SuspendLayout();
			this.PlaceOfSupplyConfiguration.SuspendLayout();
			this.taxConfigurationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationsGrid)).BeginInit();
			this.taxConfigurationsGrid.SuspendLayout();
			this.surchargeTabPage.SuspendLayout();
			this.surchargeTabControl.SuspendLayout();
			this.surchargeConfigurationTabPage.SuspendLayout();
			this.accSurchargeConfiguration.SuspendLayout();
			this.accSurchargeBasis.SuspendLayout();
			this.surchargeApplicationTabPage.SuspendLayout();
			this.accSurchargeApplicationGrid.SuspendLayout();
			this.EInvoiceCredentialsForTurkeyTabPage.SuspendLayout();
			this.EInvoiceCredentialsForTurkeyUserControl.SuspendLayout();
			this.EInvoiceCredentialsForHungaryTabPage.SuspendLayout();
			this.EInvoiceCredentialsForHungaryUserControl.SuspendLayout();
			this.EInvoiceCredentialsForPhilippinesTabPage.SuspendLayout();
			this.EInvoiceCredentialsForPhilippinesUserControl.SuspendLayout();
			this.EInvoiceOAuthAuthorizationTabPage.SuspendLayout();
			this.EInvoicingOAuthAuthorizationUserControl.SuspendLayout();
			this.ARInvTemplateTabPage.SuspendLayout();
			this.ARInvoiceTemplateConfigTabControl.SuspendLayout();
			this.XSLTTemplateUploadTabPage.SuspendLayout();
			this.TemplateFileUploadControl.SuspendLayout();
			this.XSLTFileConfigurationTabPage.SuspendLayout();
			this.TemplateConfigurationUserControl.SuspendLayout();
			this.EInvoiceCertificatesTabPage.SuspendLayout();
			this.CompanyEInvoicingCredentialUserControl.SuspendLayout();
			this.CashAdvanceTabPage.SuspendLayout();
			this.cashAdvanceJobConfig1.SuspendLayout();
			this.CompanyTabControl.SuspendLayout();
			this.NumberRangesTabPage.SuspendLayout();
			this.customsNumberViewStmNumsTabPageUserControl.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.PostingButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 621, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(483);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(484);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbCompany);
			// 
			// CompanyInfoTabPage
			// 
			this.CompanyInfoTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|45f16fd5-98c4-420f-a4bc-656345cf3250", "Company Info.");
			this.CompanyInfoTabPage.Controls.Add(this.CountryGroupBox);
			this.CompanyInfoTabPage.Controls.Add(this.BranchesGroupBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_IsWHTAccrualBasisCheckBox);
			this.CompanyInfoTabPage.Controls.Add(this.IncorporationDateEdit);
			this.CompanyInfoTabPage.Controls.Add(this.GC_IsGSTRegisteredCheckBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_OHGuidFindBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_IsWHTCashBasisCheckBox);
			this.CompanyInfoTabPage.Controls.Add(this.FaxNumberControl);
			this.CompanyInfoTabPage.Controls.Add(this.GC_IsWHTRegisteredCheckBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_IsGSTCashBasisCheckBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_BusinessRegNoTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_CodeTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.PhoneNumberControl);
			this.CompanyInfoTabPage.Controls.Add(this.GC_CustomsRegistrationNoTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_IsActiveCheckBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_NameTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_Address1TextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_Address2TextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_CityTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_PostCodeTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_RN_NKCountryCodeCodeFindBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_EmailTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_WebAddressTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_BusinessRegNo2TextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_StateTextBox);
			this.CompanyInfoTabPage.Controls.Add(this.GC_StateDropEdit);
			this.CompanyInfoTabPage.Controls.Add(this.ValidateAddressButton);
			this.CompanyInfoTabPage.Controls.Add(this.ClearFieldsButton);
			this.CompanyInfoTabPage.Controls.Add(this.NewOrgProxyButton);
			this.CompanyInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.CompanyInfoTabPage.Name = "CompanyInfoTabPage";
			this.CompanyInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 565, true);
			this.CompanyInfoTabPage.TabIndex = 0;
			// 
			// CountryGroupBox
			// 
			this.CountryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CountryGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8a072efa-970f-41b4-aff2-b5d5eef22f77", "Country/Region Settings");
			this.CountryGroupBox.Controls.Add(this.GC_RX_NKLocalCurrencyCodeFindBox);
			this.CountryGroupBox.Controls.Add(this.GC_IsReciprocalCheckEdit);
			this.CountryGroupBox.Controls.Add(this.CountrySettingLabel);
			this.CountryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 192, true);
			this.CountryGroupBox.Name = "CountryGroupBox";
			this.CountryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 74, true);
			this.CountryGroupBox.TabIndex = 24;
			this.CountryGroupBox.TabStop = false;
			// 
			// GC_RX_NKLocalCurrencyCodeFindBox
			// 
			this.GC_RX_NKLocalCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GC_RX_NKLocalCurrencyCodeFindBox, "GC_RX_NKLocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_RX_NKLocalCurrency)));
			this.GC_RX_NKLocalCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 45, true);
			this.GC_RX_NKLocalCurrencyCodeFindBox.Name = "GC_RX_NKLocalCurrencyCodeFindBox";
			this.GC_RX_NKLocalCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GC_RX_NKLocalCurrencyCodeFindBox.ParentType = null;
			this.GC_RX_NKLocalCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.GC_RX_NKLocalCurrencyCodeFindBox.TabIndex = 1;
			// 
			// GC_IsReciprocalCheckEdit
			// 
			this.GC_IsReciprocalCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GC_IsReciprocalCheckEdit, "GC_IsReciprocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_IsReciprocal)));
			this.GC_IsReciprocalCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 47, true);
			this.GC_IsReciprocalCheckEdit.Name = "GC_IsReciprocalCheckEdit";
			this.GC_IsReciprocalCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.GC_IsReciprocalCheckEdit.TabIndex = 2;
			// 
			// CountrySettingLabel
			// 
			this.CountrySettingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CountrySettingLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a8cf8cdd-0ac1-40de-b18b-6be4a285f62f", "To change these settings, please contact support.");
			this.CountrySettingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CountrySettingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.CountrySettingLabel.Name = "CountrySettingLabel";
			this.CountrySettingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 23, true);
			this.CountrySettingLabel.TabIndex = 0;
			this.CountrySettingLabel.UseMnemonic = false;
			// 
			// BranchesGroupBox
			// 
			this.BranchesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BranchesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|1b57fda9-f8ea-43b1-9e47-b4b072fa3c53", "Branches");
			this.BranchesGroupBox.Controls.Add(this.GlbBranchModuleButtonGrid);
			this.BranchesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 278, true);
			this.BranchesGroupBox.Name = "BranchesGroupBox";
			this.BranchesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 277, true);
			this.BranchesGroupBox.TabIndex = 25;
			this.BranchesGroupBox.TabStop = false;
			// 
			// GlbBranchModuleButtonGrid
			// 
			this.GlbBranchModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GlbBranchModuleButtonGrid, "Branches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).Branches)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).Lookups.Branch_List)));
			this.GlbBranchModuleButtonGrid.BindToFindBoxList = "Lookups+Branch_List";
			zTextBoxColumnStyleInfo1.ColumnName = "GB_Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "GB_BranchName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "GB_IsActive";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GB_GC";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "GB_OH_OrgProxy";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "GB_Address1";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "GB_Address2";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "GB_City";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "GB_PostCode";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "GB_State";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "GB_RL_NKHomePort";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "GB_Phone_Formatted";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "GB_Fax_Formatted";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "GB_Email";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "GB_WebAddress";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0c16f408-f2de-49ad-bccb-fc05f702d782", "Branch Management Code");
			zDropEditColumnStyleInfo1.ColumnName = "GB_AccountingGroupCode";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.GlbBranchModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GlbBranchModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GlbBranchModuleButtonGrid.GridId = null;
			// 
			// 
			// 
			this.GlbBranchModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.GlbBranchModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.GlbBranchModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.GlbBranchModuleButtonGrid.InnerGrid.GridId = null;
			this.GlbBranchModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GlbBranchModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.GlbBranchModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GlbBranchModuleButtonGrid.InnerGrid.Name = "Grid";
			this.GlbBranchModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 220, true);
			this.GlbBranchModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.GlbBranchModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.GlbBranchModuleButtonGrid.Name = "GlbBranchModuleButtonGrid";
			this.GlbBranchModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("5B9D91C0-B47D-47A6-8A94-530F31A960ED", "Branch");
			this.GlbBranchModuleButtonGrid.ReadOnly = false;
			this.GlbBranchModuleButtonGrid.ShowAttachButton = false;
			this.GlbBranchModuleButtonGrid.ShowDetachButton = false;
			this.GlbBranchModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 258, true);
			this.GlbBranchModuleButtonGrid.TabIndex = 0;
			// 
			// GC_IsWHTAccrualBasisCheckBox
			// 
			this.GC_IsWHTAccrualBasisCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GC_IsWHTAccrualBasisCheckBox, "GC_IsWHTAccrualBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_IsWHTAccrualBasis)));
			this.GC_IsWHTAccrualBasisCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|b757637d-f998-4b17-91d8-fb1b9dc21303", "Is WHT Accrual Basis");
			this.GC_IsWHTAccrualBasisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(811, 165, true);
			this.GC_IsWHTAccrualBasisCheckBox.Name = "GC_IsWHTAccrualBasisCheckBox";
			this.GC_IsWHTAccrualBasisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
			this.GC_IsWHTAccrualBasisCheckBox.TabIndex = 23;
			// 
			// IncorporationDateEdit
			// 
			this.IncorporationDateEdit.AllowDrop = true;
			this.IncorporationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.IncorporationDateEdit, "GC_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_StartDate)));
			this.IncorporationDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6e7dcde1-46d3-41ac-bdc2-1eda2b36cd5c", "Date of Incorporation");
			this.IncorporationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 105, true);
			this.IncorporationDateEdit.Name = "IncorporationDateEdit";
			this.IncorporationDateEdit.TabIndex = 18;
			// 
			// GC_IsGSTRegisteredCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GC_IsGSTRegisteredCheckBox, "GC_IsGSTRegistered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_IsGSTRegistered)));
			this.GC_IsGSTRegisteredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 140, true);
			this.GC_IsGSTRegisteredCheckBox.Name = "GC_IsGSTRegisteredCheckBox";
			this.GC_IsGSTRegisteredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 21, true);
			this.GC_IsGSTRegisteredCheckBox.TabIndex = 19;
			// 
			// GC_OHGuidFindBox
			// 
			this.GC_OHGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GC_OHGuidFindBox, "GC_OH_OrgProxy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_OH_OrgProxy)));
			this.GC_OHGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 8, true);
			this.GC_OHGuidFindBox.Name = "GC_OHGuidFindBox";
			this.GC_OHGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GC_OHGuidFindBox.ParentType = null;
			this.GC_OHGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.GC_OHGuidFindBox.TabIndex = 14;
			// 
			// GC_IsWHTCashBasisCheckBox
			// 
			this.GC_IsWHTCashBasisCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GC_IsWHTCashBasisCheckBox, "GC_IsWHTCashBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_IsWHTCashBasis)));
			this.GC_IsWHTCashBasisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(811, 142, true);
			this.GC_IsWHTCashBasisCheckBox.Name = "GC_IsWHTCashBasisCheckBox";
			this.GC_IsWHTCashBasisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
			this.GC_IsWHTCashBasisCheckBox.TabIndex = 22;
			// 
			// FaxNumberControl
			// 
			this.FaxNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FaxNumberControl, "GC_Fax_Wrapper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_Fax_Wrapper)));
			this.FaxNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|GC_Fax", "Fax");
			this.FaxNumberControl.EnableValidStateColor = true;
			this.FaxNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 201, true);
			this.FaxNumberControl.Name = "FaxNumberControl";
			this.FaxNumberControl.ShowDiallerControl = false;
			this.FaxNumberControl.ShowLocalNumberLabel = false;
			this.FaxNumberControl.ShowPublishedCheckBox = false;
			this.FaxNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.FaxNumberControl.TabIndex = 11;
			// 
			// GC_IsWHTRegisteredCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GC_IsWHTRegisteredCheckBox, "GC_IsWHTRegistered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_IsWHTRegistered)));
			this.GC_IsWHTRegisteredCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|dc5573d7-5e20-42e5-b2e3-2fadecb3f06a", "WHT Registered");
			this.GC_IsWHTRegisteredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 140, true);
			this.GC_IsWHTRegisteredCheckBox.Name = "GC_IsWHTRegisteredCheckBox";
			this.GC_IsWHTRegisteredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 21, true);
			this.GC_IsWHTRegisteredCheckBox.TabIndex = 21;
			// 
			// GC_IsGSTCashBasisCheckBox
			// 
			this.GC_IsGSTCashBasisCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GC_IsGSTCashBasisCheckBox, "GC_IsGSTCashBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_IsGSTCashBasis)));
			this.GC_IsGSTCashBasisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 142, true);
			this.GC_IsGSTCashBasisCheckBox.Name = "GC_IsGSTCashBasisCheckBox";
			this.GC_IsGSTCashBasisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.GC_IsGSTCashBasisCheckBox.TabIndex = 20;
			// 
			// GC_BusinessRegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_BusinessRegNoTextBox, "GC_BusinessRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_BusinessRegNo)));
			this.GC_BusinessRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 33, true);
			this.GC_BusinessRegNoTextBox.Name = "GC_BusinessRegNoTextBox";
			this.GC_BusinessRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.GC_BusinessRegNoTextBox.TabIndex = 15;
			// 
			// GC_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_CodeTextBox, "GC_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_Code)));
			this.GC_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 9, true);
			this.GC_CodeTextBox.Name = "GC_CodeTextBox";
			this.GC_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.GC_CodeTextBox.TabIndex = 0;
			// 
			// PhoneNumberControl
			// 
			this.PhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhoneNumberControl, "GC_Phone_Wrapper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_Phone_Wrapper)));
			this.PhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|GC_Phone", "Phone");
			this.PhoneNumberControl.EnableValidStateColor = true;
			this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 177, true);
			this.PhoneNumberControl.Name = "PhoneNumberControl";
			this.PhoneNumberControl.ShowPublishedCheckBox = false;
			this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.PhoneNumberControl.TabIndex = 10;
			// 
			// GC_CustomsRegistrationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_CustomsRegistrationNoTextBox, "GC_CustomsRegistrationNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_CustomsRegistrationNo)));
			this.GC_CustomsRegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 81, true);
			this.GC_CustomsRegistrationNoTextBox.Name = "GC_CustomsRegistrationNoTextBox";
			this.GC_CustomsRegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.GC_CustomsRegistrationNoTextBox.TabIndex = 17;
			// 
			// GC_IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GC_IsActiveCheckBox, "GC_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_IsActive)));
			this.GC_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 10, true);
			this.GC_IsActiveCheckBox.Name = "GC_IsActiveCheckBox";
			this.GC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 18, true);
			this.GC_IsActiveCheckBox.TabIndex = 1;
			this.GC_IsActiveCheckBox.UseVisualStyleBackColor = false;
			// 
			// GC_NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_NameTextBox, "GC_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_Name)));
			this.GC_NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 33, true);
			this.GC_NameTextBox.Name = "GC_NameTextBox";
			this.GC_NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.GC_NameTextBox.TabIndex = 2;
			// 
			// GC_Address1TextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_Address1TextBox, "GC_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_Address1)));
			this.GC_Address1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 57, true);
			this.GC_Address1TextBox.Name = "GC_Address1TextBox";
			this.GC_Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.GC_Address1TextBox.TabIndex = 3;
			// 
			// GC_Address2TextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_Address2TextBox, "GC_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_Address2)));
			this.GC_Address2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 81, true);
			this.GC_Address2TextBox.Name = "GC_Address2TextBox";
			this.GC_Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.GC_Address2TextBox.TabIndex = 4;
			// 
			// GC_CityTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_CityTextBox, "GC_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_City)));
			this.GC_CityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 129, true);
			this.GC_CityTextBox.Name = "GC_CityTextBox";
			this.GC_CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.GC_CityTextBox.TabIndex = 7;
			// 
			// GC_PostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_PostCodeTextBox, "GC_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_PostCode)));
			this.GC_PostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 129, true);
			this.GC_PostCodeTextBox.Name = "GC_PostCodeTextBox";
			this.GC_PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.GC_PostCodeTextBox.TabIndex = 6;
			// 
			// GC_RN_NKCountryCodeCodeFindBox
			// 
			this.GC_RN_NKCountryCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GC_RN_NKCountryCodeCodeFindBox, "AccountingCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccountingCountry)));
			this.GC_RN_NKCountryCodeCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|3bd07b38-c084-4d8c-80e7-721887d9f7cf", "Ctry./Rgn.", "Country/Region");
			this.GC_RN_NKCountryCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 105, true);
			this.GC_RN_NKCountryCodeCodeFindBox.Name = "GC_RN_NKCountryCodeCodeFindBox";
			this.GC_RN_NKCountryCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GC_RN_NKCountryCodeCodeFindBox.ParentType = null;
			this.GC_RN_NKCountryCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.GC_RN_NKCountryCodeCodeFindBox.TabIndex = 5;
			// 
			// GC_EmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_EmailTextBox, "GC_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_Email)));
			this.GC_EmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 225, true);
			this.GC_EmailTextBox.Name = "GC_EmailTextBox";
			this.GC_EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.GC_EmailTextBox.TabIndex = 12;
			// 
			// GC_WebAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_WebAddressTextBox, "GC_WebAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_WebAddress)));
			this.GC_WebAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_WebAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 249, true);
			this.GC_WebAddressTextBox.Name = "GC_WebAddressTextBox";
			this.GC_WebAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.GC_WebAddressTextBox.TabIndex = 13;
			// 
			// GC_BusinessRegNo2TextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_BusinessRegNo2TextBox, "GC_BusinessRegNo2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_BusinessRegNo2)));
			this.GC_BusinessRegNo2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 57, true);
			this.GC_BusinessRegNo2TextBox.Name = "GC_BusinessRegNo2TextBox";
			this.GC_BusinessRegNo2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.GC_BusinessRegNo2TextBox.TabIndex = 16;
			// 
			// GC_StateTextBox
			// 
			this.BindingSource.SetBindingMember(this.GC_StateTextBox, "GC_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_State)));
			this.GC_StateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GC_StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 153, true);
			this.GC_StateTextBox.Name = "GC_StateTextBox";
			this.GC_StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.GC_StateTextBox.TabIndex = 8;
			// 
			// GC_StateDropEdit
			// 
			this.GC_StateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GC_StateDropEdit, "GC_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).GC_State)));
			this.GC_StateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 153, true);
			this.GC_StateDropEdit.Name = "GC_StateDropEdit";
			this.GC_StateDropEdit.PreBoundMaxLength = 4;
			this.GC_StateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.GC_StateDropEdit.TabIndex = 9;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.IsCaptionOverridden = true;
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 56, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 18;
			this.ValidateAddressButton.TabStop = false;
			this.ValidateAddressButton.Text = " ";
			this.ValidateAddressButton.ToolTipCaption = null;
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 56, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 19;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.ToolTipCaption = null;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			// 
			// NewOrgProxyButton
			// 
			this.NewOrgProxyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|485912A2-D1E9-42DA-9BA7-A56F9995C90F", "New");
			this.NewOrgProxyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(879, 7, true);
			this.NewOrgProxyButton.Name = "NewOrgProxyButton";
			this.NewOrgProxyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.NewOrgProxyButton.TabIndex = 15;
			this.NewOrgProxyButton.TabStop = false;
			this.NewOrgProxyButton.ToolTipCaption = null;
			this.NewOrgProxyButton.UseVisualStyleBackColor = true;
			this.NewOrgProxyButton.Click += new System.EventHandler(this.NewOrgProxyButton_Click);
			// 
			// AccountingConfigurationTabPage
			// 
			this.AccountingConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|333f6039-d0e5-4462-9b6c-c432b2077253", "Accounting Configuration");
			this.AccountingConfigurationTabPage.Controls.Add(this.AccConfigTabControl);
			this.AccountingConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.AccountingConfigurationTabPage.Name = "AccountingConfigurationTabPage";
			this.AccountingConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 565, true);
			this.AccountingConfigurationTabPage.TabIndex = 0;
			// 
			// AccConfigTabControl
			// 
			this.AccConfigTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AccConfigTabControl.Controls.Add(this.AccountingFeeTabPage);
			this.AccConfigTabControl.Controls.Add(this.jobBillingExRatesTabPage);
			this.AccConfigTabControl.Controls.Add(this.cfxUpliftConfigTabPage);
			this.AccConfigTabControl.Controls.Add(this.PlaceOfSupplyConfigurationTabPage);
			this.AccConfigTabControl.Controls.Add(this.taxConfigurationTabPage);
			this.AccConfigTabControl.Controls.Add(this.surchargeTabPage);
			this.AccConfigTabControl.Controls.Add(this.EInvoiceCredentialsForTurkeyTabPage);
			this.AccConfigTabControl.Controls.Add(this.EInvoiceCredentialsForHungaryTabPage);
			this.AccConfigTabControl.Controls.Add(this.EInvoiceCredentialsForPhilippinesTabPage);
			this.AccConfigTabControl.Controls.Add(this.EInvoiceOAuthAuthorizationTabPage);
			this.AccConfigTabControl.Controls.Add(this.ARInvTemplateTabPage);
			this.AccConfigTabControl.Controls.Add(this.EInvoiceCertificatesTabPage);
			this.AccConfigTabControl.Controls.Add(this.CashAdvanceTabPage);
			this.AccConfigTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccConfigTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccConfigTabControl.Name = "AccConfigTabControl";
			this.AccConfigTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 565, true);
			this.AccConfigTabControl.TabIndex = 0;
			// 
			// AccountingFeeTabPage
			// 
			this.AccountingFeeTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|7e102814-b5bf-40d0-9bde-c2d422155bf9", "Accounting Fee");
			this.AccountingFeeTabPage.Controls.Add(this.ctlAccountFee);
			this.AccountingFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccountingFeeTabPage.Name = "AccountingFeeTabPage";
			this.AccountingFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.AccountingFeeTabPage.TabIndex = 0;
			// 
			// ctlAccountFee
			// 
			this.ctlAccountFee.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ctlAccountFee, "AccountFeeSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccountFeeSettings)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccountFeeSettings)));
			this.ctlAccountFee.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("611bae99-8fda-4d74-a700-244c4c8c0aaf", "Account Fee Settings");
			this.ctlAccountFee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.ctlAccountFee.Name = "ctlAccountFee";
			this.ctlAccountFee.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 3, true);
			this.ctlAccountFee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 115, true);
			this.ctlAccountFee.TabIndex = 0;
			// 
			// jobBillingExRatesTabPage
			// 
			this.jobBillingExRatesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("37ccd8de-5303-46fa-bd74-2ffb4151e262", "Job Billing Exchange Rates");
			this.jobBillingExRatesTabPage.Controls.Add(this.accExRateConfigs);
			this.jobBillingExRatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.jobBillingExRatesTabPage.Name = "jobBillingExRatesTabPage";
			this.jobBillingExRatesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.jobBillingExRatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.jobBillingExRatesTabPage.TabIndex = 2;
			this.jobBillingExRatesTabPage.UseVisualStyleBackColor = true;
			// 
			// accExRateConfigs
			// 
			this.accExRateConfigs.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accExRateConfigs, "AccExchangeRateConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccExchangeRateConfigurations)));
			this.accExRateConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accExRateConfigs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.accExRateConfigs.Name = "accExRateConfigs";
			this.accExRateConfigs.ReadOnly = false;
			this.accExRateConfigs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 532, true);
			this.accExRateConfigs.TabIndex = 10;
			// 
			// cfxUpliftConfigTabPage
			// 
			this.cfxUpliftConfigTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("12a49fed-b7fa-481f-b0d5-0b62cfa0cf44", "Currency Exchange (CFX) Uplift");
			this.cfxUpliftConfigTabPage.Controls.Add(this.cfxUpliftConfig);
			this.cfxUpliftConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.cfxUpliftConfigTabPage.Name = "cfxUpliftConfigTabPage";
			this.cfxUpliftConfigTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.cfxUpliftConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.cfxUpliftConfigTabPage.TabIndex = 1;
			this.cfxUpliftConfigTabPage.UseVisualStyleBackColor = true;
			// 
			// cfxUpliftConfig
			// 
			this.cfxUpliftConfig.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cfxUpliftConfig, "AccCFXConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccCFXUpliftConfigurationCollection)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccCFXConfigurations)));
			this.cfxUpliftConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cfxUpliftConfig.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cfxUpliftConfig.Name = "cfxUpliftConfig";
			this.cfxUpliftConfig.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 532, true);
			this.cfxUpliftConfig.TabIndex = 1;
			// 
			// PlaceOfSupplyConfigurationTabPage
			// 
			this.PlaceOfSupplyConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7e2afcf9-0268-43c6-aa0a-0c02b211ec54", "Place Of Supply Configuration");
			this.PlaceOfSupplyConfigurationTabPage.Controls.Add(this.PlaceOfSupplyConfiguration);
			this.PlaceOfSupplyConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PlaceOfSupplyConfigurationTabPage.Name = "PlaceOfSupplyConfigurationTabPage";
			this.PlaceOfSupplyConfigurationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PlaceOfSupplyConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.PlaceOfSupplyConfigurationTabPage.TabIndex = 6;
			this.PlaceOfSupplyConfigurationTabPage.UseVisualStyleBackColor = true;
			// 
			// PlaceOfSupplyConfiguration
			// 
			this.PlaceOfSupplyConfiguration.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfSupplyConfiguration, "AccPlaceOfSupplyConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccPOSConfigurationCollection)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccPlaceOfSupplyConfigurations)));
			this.PlaceOfSupplyConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlaceOfSupplyConfiguration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PlaceOfSupplyConfiguration.Name = "PlaceOfSupplyConfiguration";
			this.PlaceOfSupplyConfiguration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 532, true);
			this.PlaceOfSupplyConfiguration.TabIndex = 1;
			// 
			// taxConfigurationTabPage
			// 
			this.taxConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("140b4373-90a7-493b-ad7f-eb3470c68d9b", "Tax Configuration");
			this.taxConfigurationTabPage.Controls.Add(this.taxConfigurationsGrid);
			this.taxConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.taxConfigurationTabPage.Name = "taxConfigurationTabPage";
			this.taxConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.taxConfigurationTabPage.TabIndex = 3;
			// 
			// taxConfigurationsGrid
			// 
			this.taxConfigurationsGrid.AllowDrop = true;
			this.taxConfigurationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.taxConfigurationsGrid, "AccTaxConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxAuthorityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxSystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxRealisationMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_RecoveryMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_ThresholdMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_ThresholdAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxAmountRounding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_LedgerControlAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_TaxControlAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_TaxExpenseAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_TaxPendingControlAccount)));
			this.taxConfigurationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "ETC_Code";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo14.ColumnName = "ETC_Description";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo15.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo15.ColumnName = "ETC_RN_NKCountry";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "ETC_TaxAuthorityCode";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "ETC_TaxSystemCode";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "ETC_Ledger";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "ETC_TaxRealisationMethod";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "ETC_RecoveryMethod";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "ETC_ThresholdMethod";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ETC_ThresholdAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "ETC_IsActive";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "ETC_TaxAmountRounding";
			zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "ETC_AG_LedgerControlAccount";
			zGuidFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "ETC_AG_TaxControlAccount";
			zGuidFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "ETC_AG_TaxExpenseAccount";
			zGuidFindBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "ETC_AG_TaxPendingControlAccount";
			zGuidFindBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.taxConfigurationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.taxConfigurationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.taxConfigurationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.taxConfigurationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.taxConfigurationsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.taxConfigurationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taxConfigurationsGrid.GridId = "aabfd43d-3b91-4056-8750-b0ee4fac402b";
			this.taxConfigurationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.taxConfigurationsGrid.LayoutKey = "taxConfigurationsGrid";
			this.taxConfigurationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.taxConfigurationsGrid.Name = "taxConfigurationsGrid";
			this.taxConfigurationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.taxConfigurationsGrid.TabIndex = 11;
			// 
			// surchargeTabPage
			// 
			this.surchargeTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|26620742-5AFB-4D51-A869-8B274FCEB489", "Surcharge");
			this.surchargeTabPage.Controls.Add(this.surchargeTabControl);
			this.surchargeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.surchargeTabPage.Name = "surchargeTabPage";
			this.surchargeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.surchargeTabPage.TabIndex = 0;
			// 
			// surchargeTabControl
			// 
			this.surchargeTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.surchargeTabControl.Controls.Add(this.surchargeConfigurationTabPage);
			this.surchargeTabControl.Controls.Add(this.surchargeApplicationTabPage);
			this.surchargeTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.surchargeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.surchargeTabControl.Name = "surchargeTabControl";
			this.surchargeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.surchargeTabControl.TabIndex = 0;
			// 
			// surchargeConfigurationTabPage
			// 
			this.surchargeConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|4010CBE2-C6DD-4C46-A2C4-0A26056D1F29", "Surcharge Configuration");
			this.surchargeConfigurationTabPage.Controls.Add(this.accSurchargeConfiguration);
			this.surchargeConfigurationTabPage.Controls.Add(this.accSurchargeBasis);
			this.surchargeConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.surchargeConfigurationTabPage.Name = "surchargeConfigurationTabPage";
			this.surchargeConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(958, 511, true);
			this.surchargeConfigurationTabPage.TabIndex = 0;
			// 
			// accSurchargeConfiguration
			// 
			this.accSurchargeConfiguration.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accSurchargeConfiguration, "AccSurchargeConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccSurchargeConfiguration)(((Enterprise.MasterFiles.Business.AccSurchargeConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccSurchargeConfigurations)).SyncRoot)))));
			this.accSurchargeConfiguration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.accSurchargeConfiguration.Name = "accSurchargeConfiguration";
			this.accSurchargeConfiguration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 253, true);
			this.accSurchargeConfiguration.TabIndex = 10;
			// 
			// accSurchargeBasis
			// 
			this.accSurchargeBasis.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accSurchargeBasis, "AccSurchargeConfigurations.AccSurchargeBasises");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccSurchargeBasis)(((Enterprise.MasterFiles.Business.AccSurchargeBasis)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccSurchargeConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccSurchargeConfigurations)).SyncRoot)).AccSurchargeBasises)).SyncRoot)))));
			this.accSurchargeBasis.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 256, true);
			this.accSurchargeBasis.Name = "accSurchargeBasis";
			this.accSurchargeBasis.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 241, true);
			this.accSurchargeBasis.TabIndex = 10;
			// 
			// surchargeApplicationTabPage
			// 
			this.surchargeApplicationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|1BDC60AF-5C7F-4E4D-A430-B31AE77FADD5", "Surcharge Application");
			this.surchargeApplicationTabPage.Controls.Add(this.accSurchargeApplicationGrid);
			this.surchargeApplicationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.surchargeApplicationTabPage.Name = "surchargeApplicationTabPage";
			this.surchargeApplicationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(958, 511, true);
			this.surchargeApplicationTabPage.TabIndex = 1;
			// 
			// accSurchargeApplicationGrid
			// 
			this.accSurchargeApplicationGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accSurchargeApplicationGrid, "AccSurchargeApplications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccSurchargeApplicationCollection)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).AccSurchargeApplications)));
			this.accSurchargeApplicationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accSurchargeApplicationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.accSurchargeApplicationGrid.Name = "accSurchargeApplicationGrid";
			this.accSurchargeApplicationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(958, 511, true);
			this.accSurchargeApplicationGrid.TabIndex = 11;
			// 
			// EInvoiceCredentialsForTurkeyTabPage
			// 
			this.EInvoiceCredentialsForTurkeyTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3cb87564-ad1b-4608-9a86-db62f52f0e88", "Signature Credentials");
			this.EInvoiceCredentialsForTurkeyTabPage.Controls.Add(this.EInvoiceCredentialsForTurkeyUserControl);
			this.EInvoiceCredentialsForTurkeyTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EInvoiceCredentialsForTurkeyTabPage.Name = "EInvoiceCredentialsForTurkeyTabPage";
			this.EInvoiceCredentialsForTurkeyTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceCredentialsForTurkeyTabPage.TabIndex = 4;
			// 
			// EInvoiceCredentialsForTurkeyUserControl
			// 
			this.EInvoiceCredentialsForTurkeyUserControl.AllowDrop = true;
			this.EInvoiceCredentialsForTurkeyUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EInvoiceCredentialsForTurkeyUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EInvoiceCredentialsForTurkeyUserControl.Name = "EInvoiceCredentialsForTurkeyUserControl";
			this.EInvoiceCredentialsForTurkeyUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceCredentialsForTurkeyUserControl.TabIndex = 0;
			// 
			// EInvoiceCredentialsForHungaryTabPage
			// 
			this.EInvoiceCredentialsForHungaryTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4495c633-a998-4a8a-988c-e24124ee1a91", "e-Invoice Credentials");
			this.EInvoiceCredentialsForHungaryTabPage.Controls.Add(this.EInvoiceCredentialsForHungaryUserControl);
			this.EInvoiceCredentialsForHungaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EInvoiceCredentialsForHungaryTabPage.Name = "EInvoiceCredentialsForHungaryTabPage";
			this.EInvoiceCredentialsForHungaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceCredentialsForHungaryTabPage.TabIndex = 5;
			// 
			// EInvoiceCredentialsForHungaryUserControl
			// 
			this.EInvoiceCredentialsForHungaryUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EInvoiceCredentialsForHungaryUserControl, "HungaryEInvoicingCredentials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbCompanyExternalPasswordHUI)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).HungaryEInvoicingCredentials)));
			this.EInvoiceCredentialsForHungaryUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EInvoiceCredentialsForHungaryUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EInvoiceCredentialsForHungaryUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.EInvoiceCredentialsForHungaryUserControl.Name = "EInvoiceCredentialsForHungaryUserControl";
			this.EInvoiceCredentialsForHungaryUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceCredentialsForHungaryUserControl.TabIndex = 0;
			// 
			// EInvoiceCredentialsForPhilippinesTabPage
			// 
			this.EInvoiceCredentialsForPhilippinesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4495c633-a998-4a8a-988c-e24124ee1a91", "e-Invoice Credentials");
			this.EInvoiceCredentialsForPhilippinesTabPage.Controls.Add(this.EInvoiceCredentialsForPhilippinesUserControl);
			this.EInvoiceCredentialsForPhilippinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EInvoiceCredentialsForPhilippinesTabPage.Name = "EInvoiceCredentialsForPhilippinesTabPage";
			this.EInvoiceCredentialsForPhilippinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceCredentialsForPhilippinesTabPage.TabIndex = 6;
			// 
			// EInvoiceCredentialsForPhilippinesUserControl
			// 
			this.EInvoiceCredentialsForPhilippinesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EInvoiceCredentialsForPhilippinesUserControl, "PhilippinesEInvoicingCredentials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).PhilippinesEInvoicingCredentials)));
			this.EInvoiceCredentialsForPhilippinesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EInvoiceCredentialsForPhilippinesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EInvoiceCredentialsForPhilippinesUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.EInvoiceCredentialsForPhilippinesUserControl.Name = "EInvoiceCredentialsForPhilippinesUserControl";
			this.EInvoiceCredentialsForPhilippinesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceCredentialsForPhilippinesUserControl.TabIndex = 0;
			// 
			// EInvoiceOAuthAuthorizationTabPage
			// 
			this.EInvoiceOAuthAuthorizationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4495c633-a998-4a8a-988c-e24124ee1a91", "e-Invoice Credentials");
			this.EInvoiceOAuthAuthorizationTabPage.Controls.Add(this.EInvoicingOAuthAuthorizationUserControl);
			this.EInvoiceOAuthAuthorizationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EInvoiceOAuthAuthorizationTabPage.Name = "EInvoiceOAuthAuthorizationTabPage";
			this.EInvoiceOAuthAuthorizationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceOAuthAuthorizationTabPage.TabIndex = 6;
			// 
			// EInvoicingOAuthAuthorizationUserControl
			// 
			this.EInvoicingOAuthAuthorizationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EInvoicingOAuthAuthorizationUserControl, ".");
			this.EInvoicingOAuthAuthorizationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EInvoicingOAuthAuthorizationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EInvoicingOAuthAuthorizationUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.EInvoicingOAuthAuthorizationUserControl.Name = "EInvoicingOAuthAuthorizationUserControl";
			this.EInvoicingOAuthAuthorizationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoicingOAuthAuthorizationUserControl.TabIndex = 0;
			// 
			// ARInvTemplateTabPage
			// 
			this.ARInvTemplateTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b1b22d33-b734-4204-bfa2-6a5ede309847", "AR Inv. Template Config");
			this.ARInvTemplateTabPage.Controls.Add(this.ARInvoiceTemplateConfigTabControl);
			this.ARInvTemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ARInvTemplateTabPage.Name = "ARInvTemplateTabPage";
			this.ARInvTemplateTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ARInvTemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.ARInvTemplateTabPage.TabIndex = 6;
			this.ARInvTemplateTabPage.Text = "AR Inv Template Config";
			this.ARInvTemplateTabPage.UseVisualStyleBackColor = true;
			// 
			// ARInvoiceTemplateConfigTabControl
			// 
			this.ARInvoiceTemplateConfigTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ARInvoiceTemplateConfigTabControl.Controls.Add(this.XSLTTemplateUploadTabPage);
			this.ARInvoiceTemplateConfigTabControl.Controls.Add(this.XSLTFileConfigurationTabPage);
			this.ARInvoiceTemplateConfigTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ARInvoiceTemplateConfigTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ARInvoiceTemplateConfigTabControl.Name = "ARInvoiceTemplateConfigTabControl";
			this.ARInvoiceTemplateConfigTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 532, true);
			this.ARInvoiceTemplateConfigTabControl.TabIndex = 1;
			// 
			// XSLTTemplateUploadTabPage
			// 
			this.XSLTTemplateUploadTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("933f7769-e492-47c3-97f5-380e03409da7", "XSLT Template Upload");
			this.XSLTTemplateUploadTabPage.Controls.Add(this.TemplateFileUploadControl);
			this.XSLTTemplateUploadTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.XSLTTemplateUploadTabPage.Name = "XSLTTemplateUploadTabPage";
			this.XSLTTemplateUploadTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.XSLTTemplateUploadTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 505, true);
			this.XSLTTemplateUploadTabPage.TabIndex = 0;
			this.XSLTTemplateUploadTabPage.Text = "XSLT Template Upload";
			this.XSLTTemplateUploadTabPage.UseVisualStyleBackColor = true;
			// 
			// TemplateFileUploadControl
			// 
			this.TemplateFileUploadControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateFileUploadControl, ".");
			this.TemplateFileUploadControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateFileUploadControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TemplateFileUploadControl.Name = "TemplateFileUploadControl";
			this.TemplateFileUploadControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 499, true);
			this.TemplateFileUploadControl.TabIndex = 0;
			// 
			// XSLTFileConfigurationTabPage
			// 
			this.XSLTFileConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("93bac56d-0c86-4066-85ca-b273fa17d169", "Tax Invoice Template Config");
			this.XSLTFileConfigurationTabPage.Controls.Add(this.TemplateConfigurationUserControl);
			this.XSLTFileConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.XSLTFileConfigurationTabPage.Name = "XSLTFileConfigurationTabPage";
			this.XSLTFileConfigurationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.XSLTFileConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 505, true);
			this.XSLTFileConfigurationTabPage.TabIndex = 1;
			this.XSLTFileConfigurationTabPage.Text = "Tax Invoice Template Config";
			this.XSLTFileConfigurationTabPage.UseVisualStyleBackColor = true;
			// 
			// TemplateConfigurationUserControl
			// 
			this.TemplateConfigurationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateConfigurationUserControl, "EInvoicingTemplateFileConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileViewCollection)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).EInvoicingTemplateFileConfigurations)));
			this.TemplateConfigurationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateConfigurationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TemplateConfigurationUserControl.Name = "TemplateConfigurationUserControl";
			this.TemplateConfigurationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 499, true);
			this.TemplateConfigurationUserControl.TabIndex = 0;
			// 
			// EInvoiceCertificatesTabPage
			// 
			this.EInvoiceCertificatesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F938AA34-772C-4916-959F-2D5E57AB978C", "e-Invoice Credentials");
			this.EInvoiceCertificatesTabPage.Controls.Add(this.CompanyEInvoicingCredentialUserControl);
			this.EInvoiceCertificatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EInvoiceCertificatesTabPage.Name = "EInvoiceCertificatesTabPage";
			this.EInvoiceCertificatesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EInvoiceCertificatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.EInvoiceCertificatesTabPage.TabIndex = 7;
			// 
			// CompanyEInvoicingCredentialUserControl
			// 
			this.CompanyEInvoicingCredentialUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyEInvoicingCredentialUserControl, "EInvoicingCertificateCredentials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.CombinedEInvoicingCertificateCollection)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).EInvoicingCertificateCredentials)));
			this.CompanyEInvoicingCredentialUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompanyEInvoicingCredentialUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CompanyEInvoicingCredentialUserControl.Name = "CompanyEInvoicingCredentialUserControl";
			this.CompanyEInvoicingCredentialUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 532, true);
			this.CompanyEInvoicingCredentialUserControl.TabIndex = 0;
			// 
			// CashAdvanceTabPage
			// 
			this.CashAdvanceTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c89a66e6-c32b-44c3-b7b9-cfe39ba294ab", "Advance Payment");
			this.CashAdvanceTabPage.Controls.Add(this.cashAdvanceJobConfig1);
			this.CashAdvanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CashAdvanceTabPage.Name = "CashAdvanceTabPage";
			this.CashAdvanceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CashAdvanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 538, true);
			this.CashAdvanceTabPage.TabIndex = 8;
			this.CashAdvanceTabPage.UseVisualStyleBackColor = true;
			// 
			// cashAdvanceJobConfig1
			// 
			this.cashAdvanceJobConfig1.AllowDrop = true;
			this.cashAdvanceJobConfig1.AutoScroll = true;
			this.cashAdvanceJobConfig1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cashAdvanceJobConfig1, "CashAdvanceConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).CashAdvanceConfigurations)).SyncRoot)))));
			this.cashAdvanceJobConfig1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cashAdvanceJobConfig1.Name = "cashAdvanceJobConfig1";
			this.cashAdvanceJobConfig1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 487, true);
			this.cashAdvanceJobConfig1.TabIndex = 0;
			// 
			// CompanyTabControl
			// 
			this.CompanyTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CompanyTabControl.Controls.Add(this.CompanyInfoTabPage);
			this.CompanyTabControl.Controls.Add(this.AccountingConfigurationTabPage);
			this.CompanyTabControl.Controls.Add(this.NumberRangesTabPage);
			this.CompanyTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.CompanyTabControl.Controls.Add(this.zLogsTabPage1);
			this.CompanyTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompanyTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.CompanyTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompanyTabControl.Name = "CompanyTabControl";
			this.CompanyTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 593, true);
			this.CompanyTabControl.TabIndex = 0;
			// 
			// NumberRangesTabPage
			// 
			this.NumberRangesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("acf9a4cb-883a-4eed-9c1c-e2435ca3429f", "Number Ranges");
			this.NumberRangesTabPage.Controls.Add(this.customsNumberViewStmNumsTabPageUserControl);
			this.NumberRangesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.NumberRangesTabPage.Name = "NumberRangesTabPage";
			this.NumberRangesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NumberRangesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 565, true);
			this.NumberRangesTabPage.TabIndex = 5;
			this.NumberRangesTabPage.UseVisualStyleBackColor = true;
			// 
			// customsNumberViewStmNumsTabPageUserControl
			// 
			this.customsNumberViewStmNumsTabPageUserControl.AllowDrop = true;
			this.customsNumberViewStmNumsTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customsNumberViewStmNumsTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.customsNumberViewStmNumsTabPageUserControl.Name = "customsNumberViewStmNumsTabPageUserControl";
			this.customsNumberViewStmNumsTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 559, true);
			this.customsNumberViewStmNumsTabPageUserControl.TabIndex = 27;
			this.customsNumberViewStmNumsTabPageUserControl.TabStop = false;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 565, true);
			this.zStmNoteTabPage1.TabIndex = 3;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 565, true);
			this.zLogsTabPage1.TabIndex = 4;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.PostingButtons);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 593, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 28, true);
			this.bottomPanel.TabIndex = 0;
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(670, 0, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 28, true);
			this.PostingButtons.TabIndex = 0;
			// 
			// GlbCompanyForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|21510bbc-315f-4be9-a3cd-21f18b464092", "Company");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 645, true);
			this.Controls.Add(this.CompanyTabControl);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbCompany);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 650, true);
			this.Name = "GlbCompanyForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Load += new System.EventHandler(this.GlbCompanyForm_Load);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.CompanyTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompanyInfoTabPage.ResumeLayout(false);
			this.CompanyInfoTabPage.PerformLayout();
			this.CountryGroupBox.ResumeLayout(false);
			this.CountryGroupBox.PerformLayout();
			this.GC_RX_NKLocalCurrencyCodeFindBox.ResumeLayout(true);
			this.GC_RX_NKLocalCurrencyCodeFindBox.PerformLayout();
			this.BranchesGroupBox.ResumeLayout(false);
			this.BranchesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GlbBranchModuleButtonGrid.InnerGrid)).EndInit();
			this.GlbBranchModuleButtonGrid.ResumeLayout(true);
			this.GlbBranchModuleButtonGrid.PerformLayout();
			this.IncorporationDateEdit.ResumeLayout(true);
			this.IncorporationDateEdit.PerformLayout();
			this.GC_OHGuidFindBox.ResumeLayout(true);
			this.GC_OHGuidFindBox.PerformLayout();
			this.FaxNumberControl.ResumeLayout(true);
			this.FaxNumberControl.PerformLayout();
			this.PhoneNumberControl.ResumeLayout(true);
			this.PhoneNumberControl.PerformLayout();
			this.GC_RN_NKCountryCodeCodeFindBox.ResumeLayout(true);
			this.GC_RN_NKCountryCodeCodeFindBox.PerformLayout();
			this.GC_StateDropEdit.ResumeLayout(true);
			this.GC_StateDropEdit.PerformLayout();
			this.AccountingConfigurationTabPage.ResumeLayout(false);
			this.AccountingConfigurationTabPage.PerformLayout();
			this.AccConfigTabControl.ResumeLayout(false);
			this.AccConfigTabControl.PerformLayout();
			this.AccountingFeeTabPage.ResumeLayout(false);
			this.AccountingFeeTabPage.PerformLayout();
			this.ctlAccountFee.ResumeLayout(true);
			this.ctlAccountFee.PerformLayout();
			this.jobBillingExRatesTabPage.ResumeLayout(false);
			this.jobBillingExRatesTabPage.PerformLayout();
			this.accExRateConfigs.ResumeLayout(true);
			this.accExRateConfigs.PerformLayout();
			this.cfxUpliftConfigTabPage.ResumeLayout(false);
			this.cfxUpliftConfigTabPage.PerformLayout();
			this.cfxUpliftConfig.ResumeLayout(true);
			this.cfxUpliftConfig.PerformLayout();
			this.PlaceOfSupplyConfigurationTabPage.ResumeLayout(false);
			this.PlaceOfSupplyConfigurationTabPage.PerformLayout();
			this.PlaceOfSupplyConfiguration.ResumeLayout(true);
			this.PlaceOfSupplyConfiguration.PerformLayout();
			this.taxConfigurationTabPage.ResumeLayout(false);
			this.taxConfigurationTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationsGrid)).EndInit();
			this.taxConfigurationsGrid.ResumeLayout(false);
			this.taxConfigurationsGrid.PerformLayout();
			this.surchargeTabPage.ResumeLayout(false);
			this.surchargeTabPage.PerformLayout();
			this.surchargeTabControl.ResumeLayout(false);
			this.surchargeTabControl.PerformLayout();
			this.surchargeConfigurationTabPage.ResumeLayout(false);
			this.surchargeConfigurationTabPage.PerformLayout();
			this.accSurchargeConfiguration.ResumeLayout(true);
			this.accSurchargeConfiguration.PerformLayout();
			this.accSurchargeBasis.ResumeLayout(true);
			this.accSurchargeBasis.PerformLayout();
			this.surchargeApplicationTabPage.ResumeLayout(false);
			this.surchargeApplicationTabPage.PerformLayout();
			this.accSurchargeApplicationGrid.ResumeLayout(true);
			this.accSurchargeApplicationGrid.PerformLayout();
			this.EInvoiceCredentialsForTurkeyTabPage.ResumeLayout(false);
			this.EInvoiceCredentialsForTurkeyTabPage.PerformLayout();
			this.EInvoiceCredentialsForTurkeyUserControl.ResumeLayout(true);
			this.EInvoiceCredentialsForTurkeyUserControl.PerformLayout();
			this.EInvoiceCredentialsForHungaryTabPage.ResumeLayout(false);
			this.EInvoiceCredentialsForHungaryTabPage.PerformLayout();
			this.EInvoiceCredentialsForHungaryUserControl.ResumeLayout(true);
			this.EInvoiceCredentialsForHungaryUserControl.PerformLayout();
			this.EInvoiceCredentialsForPhilippinesTabPage.ResumeLayout(false);
			this.EInvoiceCredentialsForPhilippinesTabPage.PerformLayout();
			this.EInvoiceCredentialsForPhilippinesUserControl.ResumeLayout(true);
			this.EInvoiceCredentialsForPhilippinesUserControl.PerformLayout();
			this.EInvoiceOAuthAuthorizationTabPage.ResumeLayout(false);
			this.EInvoiceOAuthAuthorizationTabPage.PerformLayout();
			this.EInvoicingOAuthAuthorizationUserControl.ResumeLayout(true);
			this.EInvoicingOAuthAuthorizationUserControl.PerformLayout();
			this.ARInvTemplateTabPage.ResumeLayout(false);
			this.ARInvTemplateTabPage.PerformLayout();
			this.ARInvoiceTemplateConfigTabControl.ResumeLayout(false);
			this.ARInvoiceTemplateConfigTabControl.PerformLayout();
			this.XSLTTemplateUploadTabPage.ResumeLayout(false);
			this.XSLTTemplateUploadTabPage.PerformLayout();
			this.TemplateFileUploadControl.ResumeLayout(true);
			this.TemplateFileUploadControl.PerformLayout();
			this.XSLTFileConfigurationTabPage.ResumeLayout(false);
			this.XSLTFileConfigurationTabPage.PerformLayout();
			this.TemplateConfigurationUserControl.ResumeLayout(true);
			this.TemplateConfigurationUserControl.PerformLayout();
			this.EInvoiceCertificatesTabPage.ResumeLayout(false);
			this.EInvoiceCertificatesTabPage.PerformLayout();
			this.CompanyEInvoicingCredentialUserControl.ResumeLayout(true);
			this.CompanyEInvoicingCredentialUserControl.PerformLayout();
			this.CashAdvanceTabPage.ResumeLayout(false);
			this.CashAdvanceTabPage.PerformLayout();
			this.cashAdvanceJobConfig1.ResumeLayout(true);
			this.cashAdvanceJobConfig1.PerformLayout();
			this.CompanyTabControl.ResumeLayout(false);
			this.CompanyTabControl.PerformLayout();
			this.NumberRangesTabPage.ResumeLayout(false);
			this.NumberRangesTabPage.PerformLayout();
			this.customsNumberViewStmNumsTabPageUserControl.ResumeLayout(true);
			this.customsNumberViewStmNumsTabPageUserControl.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel bottomPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtons;
		private ZArchitecture.GUI.ZTabPage CashAdvanceTabPage;
		private CashAdvanceJobConfig cashAdvanceJobConfig1;
	}
}
