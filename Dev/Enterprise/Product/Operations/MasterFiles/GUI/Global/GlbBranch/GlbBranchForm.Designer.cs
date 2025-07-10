using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbBranchForm : ZForm, ISupportWebAddressValidationControl
	{
		private ZTabPage DepartmentsTabPage;
		private ZLabel DepartmentsLabel;
		private AccAllowedBranchDepartmentComboModuleButtonGrid DepartmentsModuleButtonGrid;
		private ZPanel zPanel1;
		private ZPanel zPanel2;
		private ZPanel zPanel3;
		private ZTabPage cfxUpliftConfigTabPage;
		private AccCFXUpliftCfg cfxUpliftConfig;
		private ZTabPage EInvoicingCredentialTaxCoreTabPage;
		private GlbBranchForm_CredentialUserControl EInvoicingCertificateTaxCoreUserControl;
		private ZTabPage taxConfigurationTabPage;
		private ZGrid taxConfigurationsGrid;
		private ZTabPage BranchCredentialIndiaTabPage;
		private GlbBranchForm_IndiaCredentialUserControl BranchCredentialIndiaUserControl;
		private ZTabPage EInvoicingCredentialTabPage;
		private EInvoicingCertificateUserControl EInvoicingCertificateCredentialUserControl;
		protected GlbBranch Branch;

		private ZGrid ExtraPortsGrid;
		private ZGrid DefaultPortsGrid;
		private ZTextBox GB_CodeBoundTextEdit;
		private ZTextBox GB_BranchNameBoundTextEdit;
		private ZDropEdit GB_AccountingGroupCodeZDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GB_IsActiveBoundCheckBox;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl BranchTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage BranchTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage HolidayTabPage;
		private Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength GB_CountryFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth GB_StateBoundDropEdit;
		private Enterprise.ZArchitecture.ZTextBox GB_PostCodeBoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox GB_Address1BoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox GB_CityBoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox GB_Address2BoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox GB_WebAddressBoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox GB_EmailBoundTextEdit;
		internal Enterprise.MasterFiles.GUI.PhoneNumberUserControl PhoneNumberControl;
		internal Enterprise.MasterFiles.GUI.PhoneNumberUserControl FaxNumberControl;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox GB_GCBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox GB_OHBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox GB_RL_NKHomePortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.ZGrid GlbHolidayBoundGrid;
		private ZButton ValidateAddressButton;
		private ZButton ClearFieldsButton;
		internal Enterprise.MasterFiles.GUI.PhoneNumberUserControl InternalExtensionNumberControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl PortsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DefaultPortsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage AdditionalPortsTabPage;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.GB_AccountingGroupCodeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GB_BranchNameBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.GB_CodeBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.GB_IsActiveBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.BranchTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.BranchTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PortsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.AdditionalPortsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExtraPortsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DefaultPortsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DefaultPortsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InternalExtensionNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.GB_OHBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GB_GCBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GB_CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.GB_StateBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.GB_PostCodeBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.GB_Address1BoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.GB_CityBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.GB_Address2BoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.GB_WebAddressBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.GB_EmailBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.FaxNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.GB_RL_NKHomePortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DepartmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DepartmentsModuleButtonGrid = new Enterprise.MasterFiles.GUI.AccAllowedBranchDepartmentComboModuleButtonGrid();
			this.DepartmentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HolidayTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GlbHolidayBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.cfxUpliftConfigTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cfxUpliftConfig = new Enterprise.MasterFiles.GUI.AccCFXUpliftCfg();
			this.taxConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.taxConfigurationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EInvoicingCredentialTaxCoreTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EInvoicingCertificateTaxCoreUserControl = new Enterprise.MasterFiles.GUI.GlbBranchForm_CredentialUserControl();
			this.BranchCredentialIndiaTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BranchCredentialIndiaUserControl = new Enterprise.MasterFiles.GUI.GlbBranchForm_IndiaCredentialUserControl();
			this.EInvoicingCredentialTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EInvoicingCertificateCredentialUserControl = new Enterprise.MasterFiles.GUI.EInvoicingCertificateUserControl();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GB_AccountingGroupCodeZDropEdit.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.BranchTabControl.SuspendLayout();
			this.BranchTabPage.SuspendLayout();
			this.PortsTabControl.SuspendLayout();
			this.AdditionalPortsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExtraPortsGrid)).BeginInit();
			this.ExtraPortsGrid.SuspendLayout();
			this.DefaultPortsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DefaultPortsGrid)).BeginInit();
			this.DefaultPortsGrid.SuspendLayout();
			this.InternalExtensionNumberControl.SuspendLayout();
			this.GB_OHBoundGuidFindBox.SuspendLayout();
			this.GB_GCBoundGuidFindBox.SuspendLayout();
			this.GB_CountryFindBox.SuspendLayout();
			this.GB_StateBoundDropEdit.SuspendLayout();
			this.PhoneNumberControl.SuspendLayout();
			this.FaxNumberControl.SuspendLayout();
			this.GB_RL_NKHomePortBoundCodeFindBox.SuspendLayout();
			this.DepartmentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DepartmentsModuleButtonGrid.InnerGrid)).BeginInit();
			this.DepartmentsModuleButtonGrid.SuspendLayout();
			this.HolidayTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GlbHolidayBoundGrid)).BeginInit();
			this.GlbHolidayBoundGrid.SuspendLayout();
			this.cfxUpliftConfigTabPage.SuspendLayout();
			this.cfxUpliftConfig.SuspendLayout();
			this.taxConfigurationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationsGrid)).BeginInit();
			this.taxConfigurationsGrid.SuspendLayout();
			this.EInvoicingCredentialTaxCoreTabPage.SuspendLayout();
			this.EInvoicingCertificateTaxCoreUserControl.SuspendLayout();
			this.BranchCredentialIndiaTabPage.SuspendLayout();
			this.BranchCredentialIndiaUserControl.SuspendLayout();
			this.EInvoicingCredentialTabPage.SuspendLayout();
			this.EInvoicingCertificateCredentialUserControl.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 497, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(847);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbBranch);
			// 
			// GB_AccountingGroupCodeZDropEdit
			// 
			this.GB_AccountingGroupCodeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GB_AccountingGroupCodeZDropEdit, "GB_AccountingGroupCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_AccountingGroupCode)));
			this.GB_AccountingGroupCodeZDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ade8bfa9-c560-4636-837c-dc1ec9ad9eab", "Branch Management Code");
			this.GB_AccountingGroupCodeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 330, true);
			this.GB_AccountingGroupCodeZDropEdit.Name = "GB_AccountingGroupCodeZDropEdit";
			this.GB_AccountingGroupCodeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 17, true);
			this.GB_AccountingGroupCodeZDropEdit.TabIndex = 25;
			this.GB_AccountingGroupCodeZDropEdit.Visible = false;
			// 
			// GB_BranchNameBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.GB_BranchNameBoundTextEdit, "GB_BranchName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_BranchName)));
			this.GB_BranchNameBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_BranchNameBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 29, true);
			this.GB_BranchNameBoundTextEdit.Name = "GB_BranchNameBoundTextEdit";
			this.GB_BranchNameBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.GB_BranchNameBoundTextEdit.TabIndex = 4;
			// 
			// GB_CodeBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.GB_CodeBoundTextEdit, "GB_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_Code)));
			this.GB_CodeBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 6, true);
			this.GB_CodeBoundTextEdit.Name = "GB_CodeBoundTextEdit";
			this.GB_CodeBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.GB_CodeBoundTextEdit.TabIndex = 1;
			// 
			// GB_IsActiveBoundCheckBox
			// 
			this.GB_IsActiveBoundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GB_IsActiveBoundCheckBox, "GB_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_IsActive)));
			this.GB_IsActiveBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 8, true);
			this.GB_IsActiveBoundCheckBox.Name = "GB_IsActiveBoundCheckBox";
			this.GB_IsActiveBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 16, true);
			this.GB_IsActiveBoundCheckBox.TabIndex = 2;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 6;
			// 
			// BranchTabControl
			// 
			this.BranchTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BranchTabControl.Controls.Add(this.BranchTabPage);
			this.BranchTabControl.Controls.Add(this.DepartmentsTabPage);
			this.BranchTabControl.Controls.Add(this.HolidayTabPage);
			this.BranchTabControl.Controls.Add(this.cfxUpliftConfigTabPage);
			this.BranchTabControl.Controls.Add(this.taxConfigurationTabPage);
			this.BranchTabControl.Controls.Add(this.EInvoicingCredentialTaxCoreTabPage);
			this.BranchTabControl.Controls.Add(this.BranchCredentialIndiaTabPage);
			this.BranchTabControl.Controls.Add(this.EInvoicingCredentialTabPage);
			this.BranchTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.BranchTabControl.Controls.Add(this.zLogsTabPage1);
			this.BranchTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BranchTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.BranchTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 379, true);
			this.BranchTabControl.Name = "BranchTabControl";
			this.BranchTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 414, true);
			this.BranchTabControl.TabIndex = 5;
			// 
			// BranchTabPage
			// 
			this.BranchTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|d9041ab0-e5a0-498c-9f4d-6fe50dd30d75", "Branch Details");
			this.BranchTabPage.Controls.Add(this.PortsTabControl);
			this.BranchTabPage.Controls.Add(this.InternalExtensionNumberControl);
			this.BranchTabPage.Controls.Add(this.GB_OHBoundGuidFindBox);
			this.BranchTabPage.Controls.Add(this.GB_GCBoundGuidFindBox);
			this.BranchTabPage.Controls.Add(this.GB_CountryFindBox);
			this.BranchTabPage.Controls.Add(this.GB_StateBoundDropEdit);
			this.BranchTabPage.Controls.Add(this.GB_PostCodeBoundTextEdit);
			this.BranchTabPage.Controls.Add(this.GB_Address1BoundTextEdit);
			this.BranchTabPage.Controls.Add(this.GB_CityBoundTextEdit);
			this.BranchTabPage.Controls.Add(this.GB_Address2BoundTextEdit);
			this.BranchTabPage.Controls.Add(this.GB_WebAddressBoundTextEdit);
			this.BranchTabPage.Controls.Add(this.GB_EmailBoundTextEdit);
			this.BranchTabPage.Controls.Add(this.PhoneNumberControl);
			this.BranchTabPage.Controls.Add(this.FaxNumberControl);
			this.BranchTabPage.Controls.Add(this.GB_RL_NKHomePortBoundCodeFindBox);
			this.BranchTabPage.Controls.Add(this.GB_AccountingGroupCodeZDropEdit);
			this.BranchTabPage.Controls.Add(this.ValidateAddressButton);
			this.BranchTabPage.Controls.Add(this.ClearFieldsButton);
			this.BranchTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.BranchTabPage.Name = "BranchTabPage";
			this.BranchTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.BranchTabPage.TabIndex = 0;
			// 
			// PortsTabControl
			// 
			this.PortsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PortsTabControl.Controls.Add(this.AdditionalPortsTabPage);
			this.PortsTabControl.Controls.Add(this.DefaultPortsTabPage);
			this.PortsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 13, true);
			this.PortsTabControl.Name = "PortsTabControl";
			this.PortsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 312, true);
			this.PortsTabControl.TabIndex = 27;
			// 
			// AdditionalPortsTabPage
			// 
			this.AdditionalPortsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ForwarderUserControl|7acde8cd-b0f3-40db-8b88-0a42de28858a", "Additional Related Ports");
			this.AdditionalPortsTabPage.Controls.Add(this.ExtraPortsGrid);
			this.AdditionalPortsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AdditionalPortsTabPage.Name = "AdditionalPortsTabPage";
			this.AdditionalPortsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.AdditionalPortsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 290, true);
			this.AdditionalPortsTabPage.TabIndex = 0;
			// 
			// ExtraPortsGrid
			// 
			this.ExtraPortsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExtraPortsGrid, "ExtraPorts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).ExtraPorts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchExtraPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).ExtraPorts)).SyncRoot)).GY_RL_NKAdditionalBranchRelatedPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchExtraPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).ExtraPorts)).SyncRoot)).PortName)));
			this.ExtraPortsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GY_RL_NKAdditionalBranchRelatedPort";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|2cb98449-c1e4-42e4-a9b0-7062ebc13cc5", "Port Name");
			zTextBoxColumnStyleInfo1.ColumnName = "PortName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ExtraPortsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ExtraPortsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExtraPortsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtraPortsGrid.GridId = "2a08707b-8ae4-42b4-b8ce-a9f3a5239816";
			this.ExtraPortsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExtraPortsGrid.LayoutKey = "GlbBranchExtraPortsBoundGrid";
			this.ExtraPortsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ExtraPortsGrid.Name = "ExtraPortsGrid";
			this.ExtraPortsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 281, true);
			this.ExtraPortsGrid.TabIndex = 26;
			// 
			// DefaultPortsTabPage
			// 
			this.DefaultPortsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ForwarderUserControl|809e745e-8703-416a-b77b-df17d769205f", "Default Ports");
			this.DefaultPortsTabPage.Controls.Add(this.DefaultPortsGrid);
			this.DefaultPortsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultPortsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DefaultPortsTabPage.Name = "DefaultPortsTabPage";
			this.DefaultPortsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.DefaultPortsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 290, true);
			this.DefaultPortsTabPage.TabIndex = 1;
			// 
			// DefaultPortsGrid
			// 
			this.DefaultPortsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DefaultPortsGrid, "DefaultPorts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).DefaultPorts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchDefaultPort)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).DefaultPorts)).SyncRoot)).GBP_DefaultTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchDefaultPort)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).DefaultPorts)).SyncRoot)).GBP_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchDefaultPort)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).DefaultPorts)).SyncRoot)).GBP_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranchDefaultPort)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).DefaultPorts)).SyncRoot)).GBP_RL_NKPort)));
			this.DefaultPortsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|a4b0aa12-944c-43eb-b5b1-65dd9cd61e21", "Default To");
			zDropEditColumnStyleInfo1.ColumnName = "GBP_DefaultTo";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|dc4a2729-b23a-4fa0-a8d7-ebf62391ea70", "Transport Mode");
			zDropEditColumnStyleInfo2.ColumnName = "GBP_TransportMode";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|dbb890c3-4cb3-4230-b697-a0292acfad38", "Container Mode");
			zDropEditColumnStyleInfo3.ColumnName = "GBP_ContainerMode";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|d3341d8a-170c-4195-b02c-109cb4ca9b0a", "Port Code");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "GBP_RL_NKPort";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.DefaultPortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DefaultPortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DefaultPortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.DefaultPortsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.DefaultPortsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultPortsGrid.GridId = "EB1F1CD3-B3DD-4B6A-B17F-F273A3D9D051";
			this.DefaultPortsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultPortsGrid.LayoutKey = "GlbBranchDefaultPortsBoundGrid";
			this.DefaultPortsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.DefaultPortsGrid.Name = "DefaultPortsGrid";
			this.DefaultPortsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 281, true);
			this.DefaultPortsGrid.TabIndex = 26;
			// 
			// InternalExtensionNumberControl
			// 
			this.InternalExtensionNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InternalExtensionNumberControl, "GB_InternalExtension_Wrapper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_InternalExtension_Wrapper)));
			this.InternalExtensionNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|GB_InternalExtension", "Extension", "Branch Internal Extension");
			this.InternalExtensionNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 154, true);
			this.InternalExtensionNumberControl.Name = "InternalExtensionNumberControl";
			this.InternalExtensionNumberControl.ShowDiallerControl = false;
			this.InternalExtensionNumberControl.ShowLocalNumberLabel = false;
			this.InternalExtensionNumberControl.ShowPublishedCheckBox = false;
			this.InternalExtensionNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.InternalExtensionNumberControl.TabIndex = 12;
			// 
			// GB_OHBoundGuidFindBox
			// 
			this.GB_OHBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GB_OHBoundGuidFindBox, "GB_OH_OrgProxy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_OH_OrgProxy)));
			this.GB_OHBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 305, true);
			this.GB_OHBoundGuidFindBox.Name = "GB_OHBoundGuidFindBox";
			this.GB_OHBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GB_OHBoundGuidFindBox.ParentType = null;
			this.GB_OHBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 17, true);
			this.GB_OHBoundGuidFindBox.TabIndex = 24;
			// 
			// GB_GCBoundGuidFindBox
			// 
			this.GB_GCBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GB_GCBoundGuidFindBox, "GB_GC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_GC)));
			this.GB_GCBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 279, true);
			this.GB_GCBoundGuidFindBox.Name = "GB_GCBoundGuidFindBox";
			this.GB_GCBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GB_GCBoundGuidFindBox.ParentType = null;
			this.GB_GCBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 17, true);
			this.GB_GCBoundGuidFindBox.TabIndex = 22;
			// 
			// GB_CountryFindBox
			// 
			this.GB_CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GB_CountryFindBox, "GB_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_RN_NKCountryCode)));
			this.GB_CountryFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("08CCDE48-CA27-4103-B5D6-A8AB1322BD52", "Ctry/Rgn.", "Country/Region");
			this.GB_CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 68, true);
			this.GB_CountryFindBox.Name = "GB_CountryFindBox";
			this.GB_CountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GB_CountryFindBox.ParentType = null;
			this.GB_CountryFindBox.PreBoundMaxLength = 3;
			this.GB_CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 17, true);
			this.GB_CountryFindBox.TabIndex = 3;
			// 
			// GB_StateBoundDropEdit
			// 
			this.GB_StateBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GB_StateBoundDropEdit, "GB_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_State)));
			this.GB_StateBoundDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_StateBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 92, true);
			this.GB_StateBoundDropEdit.Name = "GB_StateBoundDropEdit";
			this.GB_StateBoundDropEdit.PreBoundMaxLength = 4;
			this.GB_StateBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 17, true);
			this.GB_StateBoundDropEdit.TabIndex = 6;
			// 
			// GB_PostCodeBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.GB_PostCodeBoundTextEdit, "GB_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_PostCode)));
			this.GB_PostCodeBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_PostCodeBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 92, true);
			this.GB_PostCodeBoundTextEdit.Name = "GB_PostCodeBoundTextEdit";
			this.GB_PostCodeBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 17, true);
			this.GB_PostCodeBoundTextEdit.TabIndex = 4;
			// 
			// GB_Address1BoundTextEdit
			// 
			this.GB_Address1BoundTextEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GB_Address1BoundTextEdit, "GB_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_Address1)));
			this.GB_Address1BoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_Address1BoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 20, true);
			this.GB_Address1BoundTextEdit.Name = "GB_Address1BoundTextEdit";
			this.GB_Address1BoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 17, true);
			this.GB_Address1BoundTextEdit.TabIndex = 1;
			// 
			// GB_CityBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.GB_CityBoundTextEdit, "GB_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_City)));
			this.GB_CityBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_CityBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 68, true);
			this.GB_CityBoundTextEdit.Name = "GB_CityBoundTextEdit";
			this.GB_CityBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 17, true);
			this.GB_CityBoundTextEdit.TabIndex = 5;
			// 
			// GB_Address2BoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.GB_Address2BoundTextEdit, "GB_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_Address2)));
			this.GB_Address2BoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_Address2BoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 44, true);
			this.GB_Address2BoundTextEdit.Name = "GB_Address2BoundTextEdit";
			this.GB_Address2BoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 17, true);
			this.GB_Address2BoundTextEdit.TabIndex = 2;
			// 
			// GB_WebAddressBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.GB_WebAddressBoundTextEdit, "GB_WebAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_WebAddress)));
			this.GB_WebAddressBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_WebAddressBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 228, true);
			this.GB_WebAddressBoundTextEdit.Name = "GB_WebAddressBoundTextEdit";
			this.GB_WebAddressBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 17, true);
			this.GB_WebAddressBoundTextEdit.TabIndex = 18;
			// 
			// GB_EmailBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.GB_EmailBoundTextEdit, "GB_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_Email)));
			this.GB_EmailBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GB_EmailBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 204, true);
			this.GB_EmailBoundTextEdit.Name = "GB_EmailBoundTextEdit";
			this.GB_EmailBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 17, true);
			this.GB_EmailBoundTextEdit.TabIndex = 16;
			// 
			// PhoneNumberControl
			// 
			this.PhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhoneNumberControl, "GB_Phone_Wrapper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_Phone_Wrapper)));
			this.PhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|GB_Phone", "Phone", "Phone Number", "Branch Phone number");
			this.PhoneNumberControl.EnableValidStateColor = true;
			this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 130, true);
			this.PhoneNumberControl.Name = "PhoneNumberControl";
			this.PhoneNumberControl.ShowPublishedCheckBox = false;
			this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.PhoneNumberControl.TabIndex = 10;
			// 
			// FaxNumberControl
			// 
			this.FaxNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FaxNumberControl, "GB_Fax_Wrapper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_Fax_Wrapper)));
			this.FaxNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|GB_Fax", "Fax", "Fax Number", "Branch Fax number");
			this.FaxNumberControl.EnableValidStateColor = true;
			this.FaxNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 180, true);
			this.FaxNumberControl.Name = "FaxNumberControl";
			this.FaxNumberControl.ShowDiallerControl = false;
			this.FaxNumberControl.ShowLocalNumberLabel = false;
			this.FaxNumberControl.ShowPublishedCheckBox = false;
			this.FaxNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.FaxNumberControl.TabIndex = 14;
			// 
			// GB_RL_NKHomePortBoundCodeFindBox
			// 
			this.GB_RL_NKHomePortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GB_RL_NKHomePortBoundCodeFindBox, "GB_RL_NKHomePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GB_RL_NKHomePort)));
			this.GB_RL_NKHomePortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 252, true);
			this.GB_RL_NKHomePortBoundCodeFindBox.Name = "GB_RL_NKHomePortBoundCodeFindBox";
			this.GB_RL_NKHomePortBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GB_RL_NKHomePortBoundCodeFindBox.ParentType = null;
			this.GB_RL_NKHomePortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 17, true);
			this.GB_RL_NKHomePortBoundCodeFindBox.TabIndex = 20;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.IsCaptionOverridden = true;
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 19, true);
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
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 19, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 19;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.ToolTipCaption = null;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += new System.EventHandler(this.ClearFieldsButton_Click);
			// 
			// DepartmentsTabPage
			// 
			this.DepartmentsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|25a72cd9-01d0-47aa-98ae-4604f5cd6f52", "Departments");
			this.DepartmentsTabPage.Controls.Add(this.DepartmentsModuleButtonGrid);
			this.DepartmentsTabPage.Controls.Add(this.DepartmentsLabel);
			this.DepartmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DepartmentsTabPage.Name = "DepartmentsTabPage";
			this.DepartmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.DepartmentsTabPage.TabIndex = 4;
			// 
			// DepartmentsModuleButtonGrid
			// 
			this.DepartmentsModuleButtonGrid.AllowDrop = true;
			this.DepartmentsModuleButtonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DepartmentsModuleButtonGrid, "AllowedDepartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AllowedDepartments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).Lookups.AllowedDepartmentsExcludeSelected)));
			this.DepartmentsModuleButtonGrid.BindToFindBoxList = "Lookups+AllowedDepartmentsExcludeSelected";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f73efbaa-7834-434f-9338-17a1441ed62a", "Department Code");
			zTextBoxColumnStyleInfo2.ColumnName = "DepartmentCode";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2141b5d7-49d7-41bd-82f4-618ec22fddec", "Department Description");
			zTextBoxColumnStyleInfo3.ColumnName = "DepartmentDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.DepartmentsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DepartmentsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DepartmentsModuleButtonGrid.DetachMessage = null;
			this.DepartmentsModuleButtonGrid.GridId = null;
			// 
			// 
			// 
			this.DepartmentsModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DepartmentsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DepartmentsModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DepartmentsModuleButtonGrid.InnerGrid.GridId = null;
			this.DepartmentsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DepartmentsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DepartmentsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DepartmentsModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DepartmentsModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.DepartmentsModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.DepartmentsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 294, true);
			this.DepartmentsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DepartmentsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 29, true);
			this.DepartmentsModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbDepartment;
			this.DepartmentsModuleButtonGrid.Name = "DepartmentsModuleButtonGrid";
			this.DepartmentsModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("87cb6eea-fc0c-4397-b1d1-468b1a7fe51f", "Departments");
			this.DepartmentsModuleButtonGrid.ReadOnly = true;
			this.DepartmentsModuleButtonGrid.ShowEditButton = false;
			this.DepartmentsModuleButtonGrid.ShowNewButton = false;
			this.DepartmentsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 330, true);
			this.DepartmentsModuleButtonGrid.TabIndex = 8;
			// 
			// DepartmentsLabel
			// 
			this.DepartmentsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DepartmentsLabel.AutoSize = true;
			this.DepartmentsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("29396b11-22e8-449c-83d0-e98c339244d4", "Departments that can be used with this branch:");
			this.DepartmentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DepartmentsLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.DepartmentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 13, true);
			this.DepartmentsLabel.Name = "DepartmentsLabel";
			this.DepartmentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 13, true);
			this.DepartmentsLabel.TabIndex = 3;
			this.DepartmentsLabel.UseMnemonic = false;
			// 
			// HolidayTabPage
			// 
			this.HolidayTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|b76446ca-f8ee-4ca7-a028-2c6c34f069a0", "Holidays");
			this.HolidayTabPage.Controls.Add(this.GlbHolidayBoundGrid);
			this.HolidayTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.HolidayTabPage.Name = "HolidayTabPage";
			this.HolidayTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.HolidayTabPage.TabIndex = 1;
			// 
			// GlbHolidayBoundGrid
			// 
			this.GlbHolidayBoundGrid.AllowNavigation = false;
			this.GlbHolidayBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GlbHolidayBoundGrid, "GlbHolidays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GlbHolidays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GlbHolidays)).SyncRoot)).GH_HolidayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GlbHolidays)).SyncRoot)).GH_Recurring)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GlbHolidays)).SyncRoot)).GH_RecurrType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GlbHolidays)).SyncRoot)).GH_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GlbHolidays)).SyncRoot)).GH_RecurrMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).GlbHolidays)).SyncRoot)).GH_RecurrDay)));
			this.GlbHolidayBoundGrid.CaptionVisible = false;
			zTranslatableTextBoxColumnStyleInfo1.ColumnName = "GH_HolidayName";
			zTranslatableTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTranslatableTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "GH_Recurring";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.ColumnName = "GH_RecurrType";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDateEditColumnStyleInfo1.ColumnName = "GH_Date";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo5.ColumnName = "GH_RecurrMonth";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.ColumnName = "GH_RecurrDay";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.GlbHolidayBoundGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo1);
			this.GlbHolidayBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GlbHolidayBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.GlbHolidayBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.GlbHolidayBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.GlbHolidayBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.GlbHolidayBoundGrid.GridId = "c7619814-960c-48cb-9c3f-0aa2b92e7724";
			this.GlbHolidayBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GlbHolidayBoundGrid.LayoutKey = "GlbHolidayBoundGrid";
			this.GlbHolidayBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.GlbHolidayBoundGrid.Name = "GlbHolidayBoundGrid";
			this.GlbHolidayBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 357, true);
			this.GlbHolidayBoundGrid.TabIndex = 0;
			// 
			// cfxUpliftConfigTabPage
			// 
			this.cfxUpliftConfigTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|72c51c96-47e4-4ed8-a8f5-5261882e1566", "Currency Exchange (CFX) Uplift");
			this.cfxUpliftConfigTabPage.Controls.Add(this.cfxUpliftConfig);
			this.cfxUpliftConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.cfxUpliftConfigTabPage.Name = "cfxUpliftConfigTabPage";
			this.cfxUpliftConfigTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.cfxUpliftConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.cfxUpliftConfigTabPage.TabIndex = 5;
			this.cfxUpliftConfigTabPage.UseVisualStyleBackColor = true;
			// 
			// cfxUpliftConfig
			// 
			this.cfxUpliftConfig.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cfxUpliftConfig, "AccCFXConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccCFXUpliftConfigurationCollection)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccCFXConfigurations)));
			this.cfxUpliftConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cfxUpliftConfig.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cfxUpliftConfig.Name = "cfxUpliftConfig";
			this.cfxUpliftConfig.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 389, true);
			this.cfxUpliftConfig.TabIndex = 0;
			// 
			// taxConfigurationTabPage
			// 
			this.taxConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d3be13ab-b2b8-4094-a77e-72693871493f", "Tax Configuration");
			this.taxConfigurationTabPage.Controls.Add(this.taxConfigurationsGrid);
			this.taxConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.taxConfigurationTabPage.Name = "taxConfigurationTabPage";
			this.taxConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 392, true);
			this.taxConfigurationTabPage.TabIndex = 29;
			// 
			// taxConfigurationsGrid
			// 
			this.taxConfigurationsGrid.AllowDrop = true;
			this.taxConfigurationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.taxConfigurationsGrid, "AccTaxConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxAuthorityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxSystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxRealisationMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_RecoveryMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_ThresholdMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_ThresholdAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_TaxAmountRounding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_LedgerControlAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_TaxControlAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_TaxExpenseAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).AccTaxConfigurations)).SyncRoot)).ETC_AG_TaxPendingControlAccount)));
			this.taxConfigurationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "ETC_Code";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.ColumnName = "ETC_Description";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "ETC_RN_NKCountry";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "ETC_TaxAuthorityCode";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "ETC_TaxSystemCode";
			zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo9.ColumnName = "ETC_Ledger";
			zDropEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo10.ColumnName = "ETC_TaxRealisationMethod";
			zDropEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo11.ColumnName = "ETC_RecoveryMethod";
			zDropEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.ColumnName = "ETC_ThresholdMethod";
			zDropEditColumnStyleInfo12.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ETC_ThresholdAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "ETC_IsActive";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo13.ColumnName = "ETC_TaxAmountRounding";
			zDropEditColumnStyleInfo13.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ETC_AG_LedgerControlAccount";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ETC_AG_TaxControlAccount";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "ETC_AG_TaxExpenseAccount";
			zGuidFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "ETC_AG_TaxPendingControlAccount";
			zGuidFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.taxConfigurationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.taxConfigurationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.taxConfigurationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.taxConfigurationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.taxConfigurationsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.taxConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.taxConfigurationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.taxConfigurationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taxConfigurationsGrid.GridId = "aabfd43d-3b91-4056-8750-b0ee4fac402b";
			this.taxConfigurationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.taxConfigurationsGrid.LayoutKey = "taxConfigurationsGrid";
			this.taxConfigurationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.taxConfigurationsGrid.Name = "taxConfigurationsGrid";
			this.taxConfigurationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 392, true);
			this.taxConfigurationsGrid.TabIndex = 0;
			// 
			// EInvoicingCredentialTaxCoreTabPage
			// 
			this.EInvoicingCredentialTaxCoreTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|3AE52981-5BA9-45C1-95FC-CAA3AB213E5C", "Authentication Certificates");
			this.EInvoicingCredentialTaxCoreTabPage.Controls.Add(this.EInvoicingCertificateTaxCoreUserControl);
			this.EInvoicingCredentialTaxCoreTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EInvoicingCredentialTaxCoreTabPage.Name = "EInvoicingCredentialTaxCoreTabPage";
			this.EInvoicingCredentialTaxCoreTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.EInvoicingCredentialTaxCoreTabPage.TabIndex = 28;
			// 
			// EInvoicingCertificateTaxCoreUserControl
			// 
			this.EInvoicingCertificateTaxCoreUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EInvoicingCertificateTaxCoreUserControl, "CertificateCredentialsTaxCore");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.EInvoicingCertificateCredentialCollectionForTaxCore)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).CertificateCredentialsTaxCore)));
			this.EInvoicingCertificateTaxCoreUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EInvoicingCertificateTaxCoreUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EInvoicingCertificateTaxCoreUserControl.Name = "EInvoicingCertificateTaxCoreUserControl";
			this.EInvoicingCertificateTaxCoreUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.EInvoicingCertificateTaxCoreUserControl.TabIndex = 0;
			// 
			// BranchCredentialIndiaTabPage
			// 
			this.BranchCredentialIndiaTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|3AE52981-5BA9-45C1-95FC-CAA3AB213E5C", "Authentication Certificates");
			this.BranchCredentialIndiaTabPage.Controls.Add(this.BranchCredentialIndiaUserControl);
			this.BranchCredentialIndiaTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.BranchCredentialIndiaTabPage.Name = "BranchCredentialIndiaTabPage";
			this.BranchCredentialIndiaTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.BranchCredentialIndiaTabPage.TabIndex = 30;
			// 
			// BranchCredentialIndiaUserControl
			// 
			this.BranchCredentialIndiaUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchCredentialIndiaUserControl, "BranchCredentialsIndia");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).BranchCredentialsIndia)));
			this.BranchCredentialIndiaUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BranchCredentialIndiaUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BranchCredentialIndiaUserControl.Name = "BranchCredentialIndiaUserControl";
			this.BranchCredentialIndiaUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.BranchCredentialIndiaUserControl.TabIndex = 0;
			// 
			// EInvoicingCredentialTabPage
			// 
			this.EInvoicingCredentialTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|14cc7043-0aba-4dba-8ca9-5b001a7ad735", "E-Invoicing Credentials");
			this.EInvoicingCredentialTabPage.Controls.Add(this.EInvoicingCertificateCredentialUserControl);
			this.EInvoicingCredentialTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EInvoicingCredentialTabPage.Name = "EInvoicingCredentialTabPage";
			this.EInvoicingCredentialTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EInvoicingCredentialTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.EInvoicingCredentialTabPage.TabIndex = 31;
			// 
			// EInvoicingCertificateCredentialUserControl
			// 
			this.EInvoicingCertificateCredentialUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EInvoicingCertificateCredentialUserControl, "EInvoicingCertificateCredentials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.CombinedEInvoicingCertificateCollection)(((Enterprise.MasterFiles.Business.GlbBranch)(null)).EInvoicingCertificateCredentials)));
			this.EInvoicingCertificateCredentialUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EInvoicingCertificateCredentialUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EInvoicingCertificateCredentialUserControl.Name = "EInvoicingCertificateCredentialUserControl";
			this.EInvoicingCertificateCredentialUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 389, true);
			this.EInvoicingCertificateCredentialUserControl.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 392, true);
			this.zLogsTabPage1.TabIndex = 3;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zPanel2);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 466, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 31, true);
			this.zPanel1.TabIndex = 8;
			// 
			// zPanel2
			// 
			this.zPanel2.AutoSize = true;
			this.zPanel2.Controls.Add(this.ButtonsUserControl);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 31, true);
			this.zPanel2.TabIndex = 7;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.GB_CodeBoundTextEdit);
			this.zPanel3.Controls.Add(this.GB_BranchNameBoundTextEdit);
			this.zPanel3.Controls.Add(this.GB_IsActiveBoundCheckBox);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 52, true);
			this.zPanel3.TabIndex = 9;
			// 
			// GlbBranchForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbBranchForm|01cebe18-3bca-48f4-941e-9a90e56af996", "Branch");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 519, true);
			this.Controls.Add(this.BranchTabControl);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.zPanel3);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbBranch);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 556, true);
			this.Name = "GlbBranchForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.zPanel3, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.BranchTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GB_AccountingGroupCodeZDropEdit.ResumeLayout(true);
			this.GB_AccountingGroupCodeZDropEdit.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.BranchTabControl.ResumeLayout(false);
			this.BranchTabControl.PerformLayout();
			this.BranchTabPage.ResumeLayout(false);
			this.BranchTabPage.PerformLayout();
			this.PortsTabControl.ResumeLayout(false);
			this.PortsTabControl.PerformLayout();
			this.AdditionalPortsTabPage.ResumeLayout(false);
			this.AdditionalPortsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExtraPortsGrid)).EndInit();
			this.ExtraPortsGrid.ResumeLayout(false);
			this.ExtraPortsGrid.PerformLayout();
			this.DefaultPortsTabPage.ResumeLayout(false);
			this.DefaultPortsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DefaultPortsGrid)).EndInit();
			this.DefaultPortsGrid.ResumeLayout(false);
			this.DefaultPortsGrid.PerformLayout();
			this.InternalExtensionNumberControl.ResumeLayout(true);
			this.InternalExtensionNumberControl.PerformLayout();
			this.GB_OHBoundGuidFindBox.ResumeLayout(true);
			this.GB_OHBoundGuidFindBox.PerformLayout();
			this.GB_GCBoundGuidFindBox.ResumeLayout(true);
			this.GB_GCBoundGuidFindBox.PerformLayout();
			this.GB_CountryFindBox.ResumeLayout(true);
			this.GB_CountryFindBox.PerformLayout();
			this.GB_StateBoundDropEdit.ResumeLayout(true);
			this.GB_StateBoundDropEdit.PerformLayout();
			this.PhoneNumberControl.ResumeLayout(true);
			this.PhoneNumberControl.PerformLayout();
			this.FaxNumberControl.ResumeLayout(true);
			this.FaxNumberControl.PerformLayout();
			this.GB_RL_NKHomePortBoundCodeFindBox.ResumeLayout(true);
			this.GB_RL_NKHomePortBoundCodeFindBox.PerformLayout();
			this.DepartmentsTabPage.ResumeLayout(false);
			this.DepartmentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DepartmentsModuleButtonGrid.InnerGrid)).EndInit();
			this.DepartmentsModuleButtonGrid.ResumeLayout(true);
			this.DepartmentsModuleButtonGrid.PerformLayout();
			this.HolidayTabPage.ResumeLayout(false);
			this.HolidayTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GlbHolidayBoundGrid)).EndInit();
			this.GlbHolidayBoundGrid.ResumeLayout(false);
			this.GlbHolidayBoundGrid.PerformLayout();
			this.cfxUpliftConfigTabPage.ResumeLayout(false);
			this.cfxUpliftConfigTabPage.PerformLayout();
			this.cfxUpliftConfig.ResumeLayout(true);
			this.cfxUpliftConfig.PerformLayout();
			this.taxConfigurationTabPage.ResumeLayout(false);
			this.taxConfigurationTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationsGrid)).EndInit();
			this.taxConfigurationsGrid.ResumeLayout(false);
			this.taxConfigurationsGrid.PerformLayout();
			this.EInvoicingCredentialTaxCoreTabPage.ResumeLayout(false);
			this.EInvoicingCredentialTaxCoreTabPage.PerformLayout();
			this.EInvoicingCertificateTaxCoreUserControl.ResumeLayout(true);
			this.EInvoicingCertificateTaxCoreUserControl.PerformLayout();
			this.BranchCredentialIndiaTabPage.ResumeLayout(false);
			this.BranchCredentialIndiaTabPage.PerformLayout();
			this.BranchCredentialIndiaUserControl.ResumeLayout(true);
			this.BranchCredentialIndiaUserControl.PerformLayout();
			this.EInvoicingCredentialTabPage.ResumeLayout(false);
			this.EInvoicingCredentialTabPage.PerformLayout();
			this.EInvoicingCertificateCredentialUserControl.ResumeLayout(true);
			this.EInvoicingCertificateCredentialUserControl.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
