using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccChargeCodeForm
	{
		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.ZTextBox AC_CodeBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private ZGroupBox AutoRatingGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AC_RateCalculatorDropEdit;
		private ZGroupBox GLAccountSetupGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit MarginPercentageBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChargeTypeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl ChargeCodeTabControl;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox AC_RateCalculatorDescTextBox;
		private Enterprise.ZArchitecture.ZTextBox AC_DepartmentFilterListTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AC_AT_GSTRateBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AC_AW_WithholdingTaxRateBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox SalesGroupGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ExpenseGroupGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox RevenueAccountBoundFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox WIPAccountBoundFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CostAccountGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AccrualAccountGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox DetailsRevenueAccountBoundFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox DetailsRevenueClearingAccountFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox DetailsWIPAccountBoundFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox DetailsCostAccountGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox DetailsCostClearingAccountFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox DetailsAccrualAccountGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShowOnQuoteCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox SuppressOnQuoteIfZeroCheckbox;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChargeGroupDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChargeSubGroupDropEdit;
		private Enterprise.ZArchitecture.ZTextBox LocalLanguageDescriptionTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage TaxOverridesTabPage;
		private Enterprise.ZArchitecture.ZGrid TaxOverridesGrid;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AC_AT_GSTRateGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage ChargeTypeOverridesTabPage;
		private Enterprise.ZArchitecture.ZGrid ChargeTypeOverridesGrid;
		private Enterprise.ZArchitecture.ZCalcEdit DefaultMarginCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DefaultChargeTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AC_IATA_ChargeCodeMapDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ConsolLevelChargeCheckBox;
		private ZCheckBox SubjectToCommissionCheckBox;
		private ZCheckBox isAdhocServiceChargeCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit PrintSequenceEdit;
		private ZDropEdit zDropEdit1;
		private ZCheckBox AllowDescriptionOverrideCheckBox;
		private ZPanel ChargeTypeOverridesTabPageTopPanel;
		private ZPanel TaxOverridesTabPageTopPanel;
		private ZDropEdit GoodServiceTypeDropEdit;
		private ZTabPage RevenueRecOverridesTabPage;
		private ZGrid RevenueRecOverridesGrid;
		private ZTabPage SupplyTypeOverrideTabPage;
		private ZGrid SupplyTypeOverrideGrid;
		private ZTabPage SellComplianceDescriptionTabPage;
		private ZGrid SellComplianceDescriptionGrid;
		private ZTabPage GLPostingOverridesTabPage;
		private ZGrid GLPostingOverridesGrid;
		private ZPanel GLPostingOverridesPanel;
		private ZTabPage BranchOverridesTab;
		private ZGrid BranchOverridesGrid;
		private ZTabPage CreditorOverridesTab;
		private ZGrid CreditorOverridesGrid;
		private ZTabPage AirlineIATACodeTab;
		private ZGrid AirlineIATACodeGrid;
		protected IContainer components;
		private ZTabPage ApportionmentMethodOverrideTabPage;
		private ZGuidFindBox AC_AX_TaxOverrideGroupGiudFindBox;
		private ZCalcEdit InputGSTVATRecoverableCalcEdit;
		private ZTextBox GovtChargeCodeTextBox;
		private ZArchitecture.GUI.ZTabPage PlaceOfSupplyConfigurationTabPage;
		private MasterFiles.GUI.AccPlaceOfSupplyConfigurationControl PlaceOfSupplyConfiguration;
		private MasterFiles.GUI.AccChargeGovtChargeCodeOverrideConfigurationControl GovtChargeCodeOverrides;
		private MasterFiles.GUI.AccChargeApportionmentMethodOverrideControl ApportionmentMethodOverrides;
		private ZGuidFindBox costClearingAccountFindBox;
		private ZGuidFindBox revenueClearingAccountFindBox;
		private ZGroupBox AutoRatingUniversalCodeMappingGroupBox;
		private ZGrid UniversalChargeCodeMappingsGrid;
		private ZGuidFindBox DetailsDisbursementSurplusAccountGuidFindBox;
		private ZGuidFindBox DetailsDisbursementShortfallAccountGuidFindBox;
		private ZTabPage GovtChargeCodeOverrideTabPage;
		private ZTranslatableTextControl descriptionZTranslatableTextControl;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.AC_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsActiveBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChargeCodeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GovtChargeCodeOverrideTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PlaceOfSupplyConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TaxOverridesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChargeTypeOverridesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BranchOverridesTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CreditorOverridesTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RevenueRecOverridesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupplyTypeOverrideTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SellComplianceDescriptionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ApportionmentMethodOverrideTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GLPostingOverridesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirlineIATACodeTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.AC_DepartmentFilterListTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalLanguageDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AC_IATA_ChargeCodeMapDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PrintSequenceEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GoodServiceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GovtChargeCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descriptionZTranslatableTextControl = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ChargeCodeTabControl.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.AC_IATA_ChargeCodeMapDropEdit.SuspendLayout();
			this.GoodServiceTypeDropEdit.SuspendLayout();
			this.descriptionZTranslatableTextControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 659, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 9;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(570);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccChargeCode);
			// 
			// AC_CodeBoundTextBox
			// 
			this.AC_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AC_CodeBoundTextBox, "AC_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_Code)));
			this.AC_CodeBoundTextBox.CaptionResourceString = null;
			this.AC_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 10, true);
			this.AC_CodeBoundTextBox.Name = "AC_CodeBoundTextBox";
			this.AC_CodeBoundTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AC_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.AC_CodeBoundTextBox.TabIndex = 0;
			// 
			// IsActiveBoundCheckEdit
			// 
			this.IsActiveBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveBoundCheckEdit, "AC_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_IsActive)));
			this.IsActiveBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 10, true);
			this.IsActiveBoundCheckEdit.Name = "IsActiveBoundCheckEdit";
			this.IsActiveBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.IsActiveBoundCheckEdit.TabIndex = 2;
			// 
			// ChargeCodeTabControl
			// 
			this.ChargeCodeTabControl.Controls.Add(this.DetailsTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.GovtChargeCodeOverrideTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.PlaceOfSupplyConfigurationTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.SellComplianceDescriptionTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.SupplyTypeOverrideTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.TaxOverridesTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.ChargeTypeOverridesTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.BranchOverridesTab);
			this.ChargeCodeTabControl.Controls.Add(this.CreditorOverridesTab);
			this.ChargeCodeTabControl.Controls.Add(this.RevenueRecOverridesTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.ApportionmentMethodOverrideTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.GLPostingOverridesTabPage);
			this.ChargeCodeTabControl.Controls.Add(this.AirlineIATACodeTab);
			this.ChargeCodeTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.ChargeCodeTabControl.Controls.Add(this.zLogsTabPage1);
			this.ChargeCodeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.ChargeCodeTabControl.Name = "ChargeCodeTabControl";
			this.ChargeCodeTabControl.SelectedIndex = 0;
			this.ChargeCodeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 501, true);
			this.ChargeCodeTabControl.TabIndex = 7;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.AutoScroll = true;
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|aa177ed5-17e5-436b-82dd-2f3313f60e4a", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AllowDescriptionOvertype)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AW_WithholdingTaxRate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AT_GSTRate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AR_SalesGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AR_ExpenseGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_RevenueAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_RevenueClearingAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_CostClearingAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_WIPAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_CostAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_AccrualAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_MarginPercentage)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_ChargeType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_IsAdhocServiceCharge)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_IsCommissionable)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_IsGroupageCharge)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_ChargeOtherGroups)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_ChargeSubGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_ChargeGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_SuppressOnQuoteIfZero)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_RateCalculatorDesc)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_RateCalculator)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_ShowOnQuotation)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_Calc_InputGSTVATRecoverablePercentage)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).AUP_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).AUP_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).Lookups.ChargeCodeBizoCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).AUP_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).AUP_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).Lookups.Carriers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).AUP_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.AccChargeCodeUniversalCodeMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).UniversalChargeCodeMappingsCollection)).SyncRoot)).AUP_TransportMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_DisbursementSurplusAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_DisbursementShortfallAccount)));
			// 
			// GovtChargeCodeOverrideTabPage
			// 
			this.GovtChargeCodeOverrideTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("301F0AEA-35FE-4900-8700-0E1C45592294", "Government Charge Code Configuration");
			this.GovtChargeCodeOverrideTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GovtChargeCodeOverrideTabPage.Name = "GovtChargeCodeOverrideTabPage";
			this.GovtChargeCodeOverrideTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.GovtChargeCodeOverrideTabPage.TabIndex = 1;
			this.GovtChargeCodeOverrideTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.GovtChargeCodeOverrideTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GovtChargeCodeOverrides)).SyncRoot)))));
			// 
			// SupplyTypeOverrideTabPage
			// 
			this.SupplyTypeOverrideTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5090c23c-3c5d-4627-b096-1ae2cb771d32", "Supply Type Overrides");
			this.SupplyTypeOverrideTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupplyTypeOverrideTabPage.Name = "SupplyTypeOverrideTabPage";
			this.SupplyTypeOverrideTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.SupplyTypeOverrideTabPage.TabIndex = 2;
			this.SupplyTypeOverrideTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SupplyTypeOverrideTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).SupplyTypeOverrides)).SyncRoot)))));
			// 
			// PlaceOfSupplyConfigurationTabPage
			// 
			this.PlaceOfSupplyConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7e2afcf9-0268-43c6-aa0a-0c02b211ec54", "Place Of Supply Configuration");
			this.PlaceOfSupplyConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PlaceOfSupplyConfigurationTabPage.Name = "PlaceOfSupplyConfigurationTabPage";
			this.PlaceOfSupplyConfigurationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PlaceOfSupplyConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.PlaceOfSupplyConfigurationTabPage.TabIndex = 7;
			this.PlaceOfSupplyConfigurationTabPage.UseVisualStyleBackColor = true;
			this.PlaceOfSupplyConfigurationTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.PlaceOfSupplyConfigurationTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccPOSConfigurationCollection)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).PlaceOfSupplyConfigurations)));
			// 
			// TaxOverridesTabPage
			// 
			this.TaxOverridesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|805dae88-30be-4264-b6ef-c7bd2287f60a", "Tax Overrides");
			this.TaxOverridesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxOverridesTabPage.Name = "TaxOverridesTabPage";
			this.TaxOverridesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.TaxOverridesTabPage.TabIndex = 3;
			this.TaxOverridesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.TaxOverridesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_CostSellAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_TransactionContext)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_TaxRegCntryOrGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_DefaultingRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_VATExemptOnExportCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_SplitPaymentVATOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_HomeCountryOrZone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_A9_DefaultVATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_OrganisationCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).TaxOverrides)).SyncRoot)).AO_DebtorRole)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AX_TaxOverrideGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AT_GSTRate)));
			// 
			// ChargeTypeOverridesTabPage
			// 
			this.ChargeTypeOverridesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|8cf399f7-3985-4f39-abd7-0a2b97cda6f3", "Type Overrides");
			this.ChargeTypeOverridesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChargeTypeOverridesTabPage.Name = "ChargeTypeOverridesTabPage";
			this.ChargeTypeOverridesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.ChargeTypeOverridesTabPage.TabIndex = 4;
			this.ChargeTypeOverridesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ChargeTypeOverridesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeTypeOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTypeOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeTypeOverrides)).SyncRoot)).AN_JobDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTypeOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeTypeOverrides)).SyncRoot)).AN_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTypeOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeTypeOverrides)).SyncRoot)).AN_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccChargeTypeOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeTypeOverrides)).SyncRoot)).AN_MarginPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTypeOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeTypeOverrides)).SyncRoot)).AN_InvoiceType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_ChargeType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_MarginPercentage)));
			// 
			// SellComplianceDescriptionTabPage
			// 
			this.SellComplianceDescriptionTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|5bab9d9c-78af-46cc-8959-c1cc42fe4ac4", "Sell Compliance Description");
			this.SellComplianceDescriptionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SellComplianceDescriptionTabPage.Name = "SellComplianceDescriptionTabPage";
			this.SellComplianceDescriptionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.SellComplianceDescriptionTabPage.TabIndex = 9;
			this.SellComplianceDescriptionTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SellComplianceDescriptionTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeComplianceDescriptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeComplianceDescription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeComplianceDescriptions)).SyncRoot)).ADE_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeComplianceDescription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeComplianceDescriptions)).SyncRoot)).ADE_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeComplianceDescription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeComplianceDescriptions)).SyncRoot)).ADE_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeComplianceDescription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChargeComplianceDescriptions)).SyncRoot)).ADE_Description)));
			// 
			// BranchOverridesTab
			// 
			this.BranchOverridesTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|8d0f73d8-5478-4cf7-b3e9-6c5305f35cec", "Branch Overrides");
			this.BranchOverridesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BranchOverridesTab.Name = "BranchOverridesTab";
			this.BranchOverridesTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BranchOverridesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.BranchOverridesTab.TabIndex = 8;
			this.BranchOverridesTab.UseVisualStyleBackColor = true;
			this.BranchOverridesTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.BranchOverridesTab_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).BranchOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeBranchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).BranchOverrides)).SyncRoot)).YA_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeBranchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).BranchOverrides)).SyncRoot)).YA_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeBranchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).BranchOverrides)).SyncRoot)).YA_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeBranchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).BranchOverrides)).SyncRoot)).YA_DefaultingRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeBranchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).BranchOverrides)).SyncRoot)).YA_GB_SpecificBranch)));
			// 
			// CreditorOverridesTab
			// 
			this.CreditorOverridesTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|43fc7983-fb6e-4390-939b-fffef1698feb", "Creditor Overrides");
			this.CreditorOverridesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CreditorOverridesTab.Name = "CreditorOverridesTab";
			this.CreditorOverridesTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CreditorOverridesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.CreditorOverridesTab.TabIndex = 8;
			this.CreditorOverridesTab.UseVisualStyleBackColor = true;
			this.CreditorOverridesTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.CreditorOverridesTab_InitializeTab));
			this.CreditorOverridesTab.TabInitialized += new System.EventHandler(this.CreditorOverridesTab_TabInitialized);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_PaymentTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_DefaultingRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_GE_Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_CreditorRole)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).ACC_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCreditorOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).CreditorOverrides)).SyncRoot)).CreditorName)));
			// 
			// RevenueRecOverridesTabPage
			// 
			this.RevenueRecOverridesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|fea34414-671c-47ea-9335-49797b2cc5b9", "Revenue Recognition Overrides");
			this.RevenueRecOverridesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RevenueRecOverridesTabPage.Name = "RevenueRecOverridesTabPage";
			this.RevenueRecOverridesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.RevenueRecOverridesTabPage.TabIndex = 5;
			this.RevenueRecOverridesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RevenueRecOverridesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).RevenueRecOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeRevRecOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).RevenueRecOverrides)).SyncRoot)).AE_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeRevRecOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).RevenueRecOverrides)).SyncRoot)).AE_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeRevRecOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).RevenueRecOverrides)).SyncRoot)).AE_Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeRevRecOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).RevenueRecOverrides)).SyncRoot)).AE_BrokerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeRevRecOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).RevenueRecOverrides)).SyncRoot)).AE_RecognitionType)));
			// 
			// ApportionmentMethodOverrideTabPage
			// 
			this.ApportionmentMethodOverrideTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("84a83f03-7f80-4091-9c61-ad0a41f825e7", "Apportionment Method Overrides");
			this.ApportionmentMethodOverrideTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ApportionmentMethodOverrideTabPage.Name = "ApportionmentMethodOverrideTabPage";
			this.ApportionmentMethodOverrideTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.ApportionmentMethodOverrideTabPage.TabIndex = 6;
			this.ApportionmentMethodOverrideTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ApportionmentMethodOverrideTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ApportionmentMethodOverrides)).SyncRoot)))));
			// 
			// GLPostingOverridesTabPage
			// 
			this.GLPostingOverridesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GLPostingOverridesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|771b2566-4b9f-4a06-bf46-f36ba40c6dab", "GL Posting Overrides");
			this.GLPostingOverridesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GLPostingOverridesTabPage.Name = "GLPostingOverridesTabPage";
			this.GLPostingOverridesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GLPostingOverridesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.GLPostingOverridesTabPage.TabIndex = 7;
			this.GLPostingOverridesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.GLPostingOverridesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_ConsolidationAccountingCategoryClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_AG_REV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_AG_WIP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_AG_CST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_AG_ACR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_AG_REV_Clearing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_AG_CST_Clearing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_ConsolContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_MasterPaymentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGLPostingOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).GLPostingOverrides)).SyncRoot)).Y1_HousePaymentType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_RevenueAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_RevenueClearingAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_CostAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_AccrualAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_WIPAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_AG_CostClearingAccount)));
			// 
			// AirlineIATACodeTab
			// 
			this.AirlineIATACodeTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|65e7f2e6-64be-4ae6-8d6b-8db61b7e3dac", "Airline IATA Code");
			this.AirlineIATACodeTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AirlineIATACodeTab.Name = "AirlineIATACodeTab";
			this.AirlineIATACodeTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AirlineIATACodeTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.AirlineIATACodeTab.TabIndex = 8;
			this.AirlineIATACodeTab.UseVisualStyleBackColor = true;
			this.AirlineIATACodeTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.AirlineIATACodeTab_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AccChargeCodeCarrierIataMappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCodeCarrierIataMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AccChargeCodeCarrierIataMappings)).SyncRoot)).ACI_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCodeCarrierIataMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AccChargeCodeCarrierIataMappings)).SyncRoot)).AirLine2CharCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCodeCarrierIataMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AccChargeCodeCarrierIataMappings)).SyncRoot)).AirLineName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCodeCarrierIataMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AccChargeCodeCarrierIataMappings)).SyncRoot)).ACI_IATAChargeCodeMap)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.zStmNoteTabPage1.TabIndex = 5;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.zLogsTabPage1.TabIndex = 6;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 634, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 25, true);
			this.ButtonsUserControl.TabIndex = 8;
			// 
			// AC_DepartmentFilterListTextBox
			// 
			this.AC_DepartmentFilterListTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AC_DepartmentFilterListTextBox, "AC_DepartmentFilterList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_DepartmentFilterList)));
			this.AC_DepartmentFilterListTextBox.CaptionResourceString = null;
			this.AC_DepartmentFilterListTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 82, true);
			this.AC_DepartmentFilterListTextBox.Name = "AC_DepartmentFilterListTextBox";
			this.AC_DepartmentFilterListTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AC_DepartmentFilterListTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.AC_DepartmentFilterListTextBox.TabIndex = 5;
			// 
			// LocalLanguageDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalLanguageDescriptionTextBox, "AC_LocalLanguageDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_LocalLanguageDescription)));
			this.LocalLanguageDescriptionTextBox.CaptionResourceString = null;
			this.LocalLanguageDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LocalLanguageDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 58, true);
			this.LocalLanguageDescriptionTextBox.Name = "LocalLanguageDescriptionTextBox";
			this.LocalLanguageDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.LocalLanguageDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.LocalLanguageDescriptionTextBox.TabIndex = 4;
			// 
			// AC_IATA_ChargeCodeMapDropEdit
			// 
			this.AC_IATA_ChargeCodeMapDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_IATA_ChargeCodeMapDropEdit, "AC_IATA_ChargeCodeMap");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_IATA_ChargeCodeMap)));
			this.AC_IATA_ChargeCodeMapDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(524, 10, true);
			this.AC_IATA_ChargeCodeMapDropEdit.Name = "AC_IATA_ChargeCodeMapDropEdit";
			this.AC_IATA_ChargeCodeMapDropEdit.PreBoundMaxLength = 3;
			this.AC_IATA_ChargeCodeMapDropEdit.ShouldResizeByMaxLength = true;
			this.AC_IATA_ChargeCodeMapDropEdit.ShowDescriptionBox = false;
			this.AC_IATA_ChargeCodeMapDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AC_IATA_ChargeCodeMapDropEdit.TabIndex = 1;
			// 
			// PrintSequenceEdit
			// 
			this.PrintSequenceEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PrintSequenceEdit, "AC_PrintSequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_PrintSequence)));
			this.PrintSequenceEdit.CaptionResourceString = null;
			this.PrintSequenceEdit.DecimalPlaces = 0;
			this.PrintSequenceEdit.Decimals = 0;
			this.PrintSequenceEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 106, true);
			this.PrintSequenceEdit.Name = "PrintSequenceEdit";
			this.PrintSequenceEdit.ShouldEscapeAllSpecialCharacters = false;
			this.PrintSequenceEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PrintSequenceEdit.TabIndex = 6;
			this.PrintSequenceEdit.Text = "0";
			this.PrintSequenceEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GoodServiceTypeDropEdit
			// 
			this.GoodServiceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodServiceTypeDropEdit, "AC_GoodsServiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_GoodsServiceType)));
			this.GoodServiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 106, true);
			this.GoodServiceTypeDropEdit.Name = "GoodServiceTypeDropEdit";
			this.GoodServiceTypeDropEdit.PreBoundMaxLength = 3;
			this.GoodServiceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.GoodServiceTypeDropEdit.ShowDescriptionBox = false;
			this.GoodServiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.GoodServiceTypeDropEdit.TabIndex = 10;
			// 
			// GovtChargeCodeTextBox
			// 
			this.GovtChargeCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GovtChargeCodeTextBox, "AC_GovtChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_GovtChargeCode)));
			this.GovtChargeCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d6715aac-7fb0-4984-93d1-2802907f5da4", "Government Charge Code");
			this.GovtChargeCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 106, true);
			this.GovtChargeCodeTextBox.Name = "GovtChargeCodeTextBox";
			this.GovtChargeCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.GovtChargeCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.GovtChargeCodeTextBox.TabIndex = 7;
			// 
			// descriptionZTranslatableTextControl
			// 
			this.descriptionZTranslatableTextControl.AcceptsReturn = false;
			this.descriptionZTranslatableTextControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.descriptionZTranslatableTextControl, ".");
			this.descriptionZTranslatableTextControl.GridCurrent = null;
			this.descriptionZTranslatableTextControl.GridMember = null;
			this.descriptionZTranslatableTextControl.IsLanguageEditingEnabled = true;
			this.descriptionZTranslatableTextControl.IsMultiLine = false;
			this.descriptionZTranslatableTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.descriptionZTranslatableTextControl.Name = "descriptionZTranslatableTextControl";
			this.descriptionZTranslatableTextControl.ReadOnly = false;
			this.descriptionZTranslatableTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.descriptionZTranslatableTextControl.TabIndex = 10;
			// 
			// AccChargeCodeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|d9c3e0f0-79d2-450b-b75e-7fa0b381c1a6", "Charge Code");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 683, true);
			this.Controls.Add(this.descriptionZTranslatableTextControl);
			this.Controls.Add(this.GovtChargeCodeTextBox);
			this.Controls.Add(this.PrintSequenceEdit);
			this.Controls.Add(this.LocalLanguageDescriptionTextBox);
			this.Controls.Add(this.AC_DepartmentFilterListTextBox);
			this.Controls.Add(this.AC_CodeBoundTextBox);
			this.Controls.Add(this.GoodServiceTypeDropEdit);
			this.Controls.Add(this.ChargeCodeTabControl);
			this.Controls.Add(this.AC_IATA_ChargeCodeMapDropEdit);
			this.Controls.Add(this.IsActiveBoundCheckEdit);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccChargeCode);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 722, true);
			this.Name = "AccChargeCodeForm";
			this.ShouldSerializeTabPageMethods = true;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.IsActiveBoundCheckEdit, 0);
			this.Controls.SetChildIndex(this.AC_IATA_ChargeCodeMapDropEdit, 0);
			this.Controls.SetChildIndex(this.ChargeCodeTabControl, 0);
			this.Controls.SetChildIndex(this.GoodServiceTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.AC_CodeBoundTextBox, 0);
			this.Controls.SetChildIndex(this.AC_DepartmentFilterListTextBox, 0);
			this.Controls.SetChildIndex(this.LocalLanguageDescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.PrintSequenceEdit, 0);
			this.Controls.SetChildIndex(this.GovtChargeCodeTextBox, 0);
			this.Controls.SetChildIndex(this.descriptionZTranslatableTextControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChargeCodeTabControl.ResumeLayout(false);
			this.ChargeCodeTabControl.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.AC_IATA_ChargeCodeMapDropEdit.ResumeLayout(true);
			this.AC_IATA_ChargeCodeMapDropEdit.PerformLayout();
			this.GoodServiceTypeDropEdit.ResumeLayout(true);
			this.GoodServiceTypeDropEdit.PerformLayout();
			this.descriptionZTranslatableTextControl.ResumeLayout(true);
			this.descriptionZTranslatableTextControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void DetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.AccChargeCodeColumnStyleInfo chargeCodeColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.AccChargeCodeColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AllowDescriptionOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AC_AT_GSTRateBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GLAccountSetupGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SalesGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExpenseGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsRevenueAccountBoundFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsRevenueClearingAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsCostClearingAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsWIPAccountBoundFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsCostAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsAccrualAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MarginPercentageBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutoRatingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.isAdhocServiceChargeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SubjectToCommissionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsolLevelChargeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChargeSubGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChargeGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SuppressOnQuoteIfZeroCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AC_RateCalculatorDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AC_RateCalculatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShowOnQuoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InputGSTVATRecoverableCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AutoRatingUniversalCodeMappingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UniversalChargeCodeMappingsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DetailsDisbursementSurplusAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsDisbursementShortfallAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DetailsTabPage.SuspendLayout();
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.SuspendLayout();
			this.AC_AT_GSTRateBoundGuidFindBox.SuspendLayout();
			this.GLAccountSetupGroupBox.SuspendLayout();
			this.SalesGroupGuidFindBox.SuspendLayout();
			this.ExpenseGroupGuidFindBox.SuspendLayout();
			this.DetailsRevenueAccountBoundFindBox.SuspendLayout();
			this.DetailsRevenueClearingAccountFindBox.SuspendLayout();
			this.DetailsCostClearingAccountFindBox.SuspendLayout();
			this.DetailsWIPAccountBoundFindBox.SuspendLayout();
			this.DetailsCostAccountGuidFindBox.SuspendLayout();
			this.DetailsAccrualAccountGuidFindBox.SuspendLayout();
			this.ChargeTypeDropEdit.SuspendLayout();
			this.AutoRatingGroupBox.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.ChargeSubGroupDropEdit.SuspendLayout();
			this.ChargeGroupDropEdit.SuspendLayout();
			this.AC_RateCalculatorDropEdit.SuspendLayout();
			this.AutoRatingUniversalCodeMappingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UniversalChargeCodeMappingsGrid)).BeginInit();
			this.UniversalChargeCodeMappingsGrid.SuspendLayout();
			this.DetailsDisbursementSurplusAccountGuidFindBox.SuspendLayout();
			this.DetailsDisbursementShortfallAccountGuidFindBox.SuspendLayout();
			this.DetailsTabPage.Controls.Add(this.InputGSTVATRecoverableCalcEdit);
			this.DetailsTabPage.Controls.Add(this.AllowDescriptionOverrideCheckBox);
			this.DetailsTabPage.Controls.Add(this.AC_AW_WithholdingTaxRateBoundGuidFindBox);
			this.DetailsTabPage.Controls.Add(this.AC_AT_GSTRateBoundGuidFindBox);
			this.DetailsTabPage.Controls.Add(this.GLAccountSetupGroupBox);
			this.DetailsTabPage.Controls.Add(this.MarginPercentageBoundCalcEdit);
			this.DetailsTabPage.Controls.Add(this.ChargeTypeDropEdit);
			this.DetailsTabPage.Controls.Add(this.AutoRatingGroupBox);
			// 
			// AllowDescriptionOverrideCheckBox
			// 
			this.AllowDescriptionOverrideCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AllowDescriptionOverrideCheckBox, "AC_AllowDescriptionOvertype");
			this.AllowDescriptionOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllowDescriptionOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 6, true);
			this.AllowDescriptionOverrideCheckBox.Name = "AllowDescriptionOverrideCheckBox";
			this.AllowDescriptionOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.AllowDescriptionOverrideCheckBox.TabIndex = 0;
			this.AllowDescriptionOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// AC_AW_WithholdingTaxRateBoundGuidFindBox
			// 
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_AW_WithholdingTaxRateBoundGuidFindBox, "AC_AW_WithholdingTaxRate");
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 77, true);
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.Name = "AC_AW_WithholdingTaxRateBoundGuidFindBox";
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.ParentType = null;
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.PopupCaption = null;
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.TabIndex = 4;
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.Load += new System.EventHandler(this.AC_AW_WithholdingTaxRateBoundGuidFindBox_Load);
			// 
			// AC_AT_GSTRateBoundGuidFindBox
			// 
			this.AC_AT_GSTRateBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_AT_GSTRateBoundGuidFindBox, "AC_AT_GSTRate");
			this.AC_AT_GSTRateBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 53, true);
			this.AC_AT_GSTRateBoundGuidFindBox.Name = "AC_AT_GSTRateBoundGuidFindBox";
			this.AC_AT_GSTRateBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AC_AT_GSTRateBoundGuidFindBox.ParentType = null;
			this.AC_AT_GSTRateBoundGuidFindBox.PopupCaption = null;
			this.AC_AT_GSTRateBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.AC_AT_GSTRateBoundGuidFindBox.TabIndex = 3;
			this.AC_AT_GSTRateBoundGuidFindBox.Load += new System.EventHandler(this.AC_AT_GSTRateBoundGuidFindBox_Load);
			// 
			// GLAccountSetupGroupBox
			// 
			this.GLAccountSetupGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|ba547218-c3d4-4769-b7b5-eb5b81b03191", "GL Account Setup");
			this.GLAccountSetupGroupBox.Controls.Add(this.SalesGroupGuidFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.ExpenseGroupGuidFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsRevenueAccountBoundFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsRevenueClearingAccountFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsWIPAccountBoundFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsCostAccountGuidFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsCostClearingAccountFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsDisbursementShortfallAccountGuidFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsDisbursementSurplusAccountGuidFindBox);
			this.GLAccountSetupGroupBox.Controls.Add(this.DetailsAccrualAccountGuidFindBox);
			this.GLAccountSetupGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 101, true);
			this.GLAccountSetupGroupBox.Name = "GLAccountSetupGroupBox";
			this.GLAccountSetupGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 260, true);
			this.GLAccountSetupGroupBox.TabIndex = 5;
			this.GLAccountSetupGroupBox.TabStop = false;
			// 
			// SalesGroupGuidFindBox
			// 
			this.SalesGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalesGroupGuidFindBox, "AC_AR_SalesGroup");
			this.SalesGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.SalesGroupGuidFindBox.Name = "SalesGroupGuidFindBox";
			this.SalesGroupGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SalesGroupGuidFindBox.ParentType = null;
			this.SalesGroupGuidFindBox.PopupCaption = null;
			this.SalesGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.SalesGroupGuidFindBox.TabIndex = 0;
			// 
			// ExpenseGroupGuidFindBox
			// 
			this.ExpenseGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExpenseGroupGuidFindBox, "AC_AR_ExpenseGroup");
			this.ExpenseGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.ExpenseGroupGuidFindBox.Name = "ExpenseGroupGuidFindBox";
			this.ExpenseGroupGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExpenseGroupGuidFindBox.ParentType = null;
			this.ExpenseGroupGuidFindBox.PopupCaption = null;
			this.ExpenseGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.ExpenseGroupGuidFindBox.TabIndex = 1;
			// 
			// DetailsRevenueAccountBoundFindBox
			// 
			this.DetailsRevenueAccountBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsRevenueAccountBoundFindBox, "AC_AG_RevenueAccount");
			this.DetailsRevenueAccountBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.DetailsRevenueAccountBoundFindBox.Name = "DetailsRevenueAccountBoundFindBox";
			this.DetailsRevenueAccountBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsRevenueAccountBoundFindBox.ParentType = null;
			this.DetailsRevenueAccountBoundFindBox.PopupCaption = null;
			this.DetailsRevenueAccountBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsRevenueAccountBoundFindBox.TabIndex = 2;
			// 
			// DetailsRevenueClearingAccountFindBox
			// 
			this.DetailsRevenueClearingAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsRevenueClearingAccountFindBox, "AC_AG_RevenueClearingAccount");
			this.DetailsRevenueClearingAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.DetailsRevenueClearingAccountFindBox.Name = "DetailsRevenueClearingAccountFindBox";
			this.DetailsRevenueClearingAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsRevenueClearingAccountFindBox.ParentType = null;
			this.DetailsRevenueClearingAccountFindBox.PopupCaption = null;
			this.DetailsRevenueClearingAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsRevenueClearingAccountFindBox.TabIndex = 3;
			// 
			// DetailsCostClearingAccountFindBox
			// 
			this.DetailsCostClearingAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsCostClearingAccountFindBox, "AC_AG_CostClearingAccount");
			this.DetailsCostClearingAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 160, true);
			this.DetailsCostClearingAccountFindBox.Name = "DetailsCostClearingAccountFindBox";
			this.DetailsCostClearingAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsCostClearingAccountFindBox.ParentType = null;
			this.DetailsCostClearingAccountFindBox.PopupCaption = null;
			this.DetailsCostClearingAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsCostClearingAccountFindBox.TabIndex = 6;
			// 
			// DetailsWIPAccountBoundFindBox
			// 
			this.DetailsWIPAccountBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsWIPAccountBoundFindBox, "AC_AG_WIPAccount");
			this.DetailsWIPAccountBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 112, true);
			this.DetailsWIPAccountBoundFindBox.Name = "DetailsWIPAccountBoundFindBox";
			this.DetailsWIPAccountBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsWIPAccountBoundFindBox.ParentType = null;
			this.DetailsWIPAccountBoundFindBox.PopupCaption = null;
			this.DetailsWIPAccountBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsWIPAccountBoundFindBox.TabIndex = 4;
			// 
			// DetailsCostAccountGuidFindBox
			// 
			this.DetailsCostAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsCostAccountGuidFindBox, "AC_AG_CostAccount");
			this.DetailsCostAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
			this.DetailsCostAccountGuidFindBox.Name = "DetailsCostAccountGuidFindBox";
			this.DetailsCostAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsCostAccountGuidFindBox.ParentType = null;
			this.DetailsCostAccountGuidFindBox.PopupCaption = null;
			this.DetailsCostAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsCostAccountGuidFindBox.TabIndex = 5;
			// 
			// DetailsAccrualAccountGuidFindBox
			// 
			this.DetailsAccrualAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsAccrualAccountGuidFindBox, "AC_AG_AccrualAccount");
			this.DetailsAccrualAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 184, true);
			this.DetailsAccrualAccountGuidFindBox.Name = "DetailsAccrualAccountGuidFindBox";
			this.DetailsAccrualAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsAccrualAccountGuidFindBox.ParentType = null;
			this.DetailsAccrualAccountGuidFindBox.PopupCaption = null;
			this.DetailsAccrualAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsAccrualAccountGuidFindBox.TabIndex = 7;
			// 
			// MarginPercentageBoundCalcEdit
			// 
			this.MarginPercentageBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MarginPercentageBoundCalcEdit, "AC_MarginPercentage");
			this.MarginPercentageBoundCalcEdit.CaptionResourceString = null;
			this.MarginPercentageBoundCalcEdit.DecimalPlaces = 2;
			this.MarginPercentageBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 29, true);
			this.MarginPercentageBoundCalcEdit.Name = "MarginPercentageBoundCalcEdit";
			this.MarginPercentageBoundCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.MarginPercentageBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.MarginPercentageBoundCalcEdit.TabIndex = 2;
			this.MarginPercentageBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeTypeDropEdit
			// 
			this.ChargeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeTypeDropEdit, "AC_ChargeType");
			this.ChargeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 29, true);
			this.ChargeTypeDropEdit.Name = "ChargeTypeDropEdit";
			this.ChargeTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ChargeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.ChargeTypeDropEdit.TabIndex = 1;
			// 
			// AutoRatingGroupBox
			// 
			this.AutoRatingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|245bea2f-fadd-4a11-a0c7-9e0adfa16d97", "Rating and Quotations");
			this.AutoRatingGroupBox.Controls.Add(this.AutoRatingUniversalCodeMappingGroupBox);
			this.AutoRatingGroupBox.Controls.Add(this.isAdhocServiceChargeCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.SubjectToCommissionCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.ConsolLevelChargeCheckBox);
			this.AutoRatingGroupBox.Controls.Add(this.zDropEdit1);
			this.AutoRatingGroupBox.Controls.Add(this.ChargeSubGroupDropEdit);
			this.AutoRatingGroupBox.Controls.Add(this.ChargeGroupDropEdit);
			this.AutoRatingGroupBox.Controls.Add(this.SuppressOnQuoteIfZeroCheckbox);
			this.AutoRatingGroupBox.Controls.Add(this.AC_RateCalculatorDescTextBox);
			this.AutoRatingGroupBox.Controls.Add(this.AC_RateCalculatorDropEdit);
			this.AutoRatingGroupBox.Controls.Add(this.ShowOnQuoteCheckBox);
			this.AutoRatingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 363, true);
			this.AutoRatingGroupBox.Name = "AutoRatingGroupBox";
			this.AutoRatingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 275, true);
			this.AutoRatingGroupBox.TabIndex = 6;
			this.AutoRatingGroupBox.TabStop = false;
			// 
			// isAdhocServiceChargeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isAdhocServiceChargeCheckBox, "AC_IsAdhocServiceCharge");
			this.isAdhocServiceChargeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isAdhocServiceChargeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 40, true);
			this.isAdhocServiceChargeCheckBox.Name = "isAdhocServiceChargeCheckBox";
			this.isAdhocServiceChargeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.isAdhocServiceChargeCheckBox.TabIndex = 3;
			// 
			// SubjectToCommissionCheckBox
			// 
			this.SubjectToCommissionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SubjectToCommissionCheckBox, "AC_IsCommissionable");
			this.SubjectToCommissionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SubjectToCommissionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 252, true);
			this.SubjectToCommissionCheckBox.Name = "SubjectToCommissionCheckBox";
			this.SubjectToCommissionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.SubjectToCommissionCheckBox.TabIndex = 9;
			// 
			// ConsolLevelChargeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolLevelChargeCheckBox, "AC_IsGroupageCharge");
			this.ConsolLevelChargeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsolLevelChargeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 16, true);
			this.ConsolLevelChargeCheckBox.Name = "ConsolLevelChargeCheckBox";
			this.ConsolLevelChargeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.ConsolLevelChargeCheckBox.TabIndex = 1;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "AC_ChargeOtherGroups");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.ShouldResizeByMaxLength = true;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 20, true);
			this.zDropEdit1.TabIndex = 4;
			// 
			// ChargeSubGroupDropEdit
			// 
			this.ChargeSubGroupDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeSubGroupDropEdit, "AC_ChargeSubGroup");
			this.ChargeSubGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.ChargeSubGroupDropEdit.Name = "ChargeSubGroupDropEdit";
			this.ChargeSubGroupDropEdit.ShouldResizeByMaxLength = true;
			this.ChargeSubGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.ChargeSubGroupDropEdit.TabIndex = 2;
			// 
			// ChargeGroupDropEdit
			// 
			this.ChargeGroupDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeGroupDropEdit, "AC_ChargeGroup");
			this.ChargeGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.ChargeGroupDropEdit.Name = "ChargeGroupDropEdit";
			this.ChargeGroupDropEdit.ShouldResizeByMaxLength = true;
			this.ChargeGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.ChargeGroupDropEdit.TabIndex = 0;
			// 
			// SuppressOnQuoteIfZeroCheckbox
			// 
			this.SuppressOnQuoteIfZeroCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SuppressOnQuoteIfZeroCheckbox, "AC_SuppressOnQuoteIfZero");
			this.SuppressOnQuoteIfZeroCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SuppressOnQuoteIfZeroCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 252, true);
			this.SuppressOnQuoteIfZeroCheckbox.Name = "SuppressOnQuoteIfZeroCheckbox";
			this.SuppressOnQuoteIfZeroCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 17, true);
			this.SuppressOnQuoteIfZeroCheckbox.TabIndex = 8;
			// 
			// AC_RateCalculatorDescTextBox
			// 
			this.AC_RateCalculatorDescTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.AC_RateCalculatorDescTextBox, "AC_RateCalculatorDesc");
			this.AC_RateCalculatorDescTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.AC_RateCalculatorDescTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|8d42213e-b0e4-4e20-b08b-47a7e51fc2c2", "Description");
			this.AC_RateCalculatorDescTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AC_RateCalculatorDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 112, true);
			this.AC_RateCalculatorDescTextBox.Multiline = true;
			this.AC_RateCalculatorDescTextBox.Name = "AC_RateCalculatorDescTextBox";
			this.AC_RateCalculatorDescTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AC_RateCalculatorDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 32, true);
			this.AC_RateCalculatorDescTextBox.TabIndex = 6;
			// 
			// AC_RateCalculatorDropEdit
			// 
			this.AC_RateCalculatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_RateCalculatorDropEdit, "AC_RateCalculator");
			this.AC_RateCalculatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.AC_RateCalculatorDropEdit.Name = "AC_RateCalculatorDropEdit";
			this.AC_RateCalculatorDropEdit.ShouldResizeByMaxLength = true;
			this.AC_RateCalculatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 20, true);
			this.AC_RateCalculatorDropEdit.TabIndex = 5;
			// 
			// ShowOnQuoteCheckBox
			// 
			this.ShowOnQuoteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowOnQuoteCheckBox, "AC_ShowOnQuotation");
			this.ShowOnQuoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowOnQuoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 252, true);
			this.ShowOnQuoteCheckBox.Name = "ShowOnQuoteCheckBox";
			this.ShowOnQuoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.ShowOnQuoteCheckBox.TabIndex = 7;
			// 
			// InputGSTVATRecoverableCalcEdit
			// 
			this.InputGSTVATRecoverableCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.InputGSTVATRecoverableCalcEdit, "AC_Calc_InputGSTVATRecoverablePercentage");
			this.InputGSTVATRecoverableCalcEdit.CaptionResourceString = null;
			this.InputGSTVATRecoverableCalcEdit.DecimalPlaces = 2;
			this.InputGSTVATRecoverableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(601, 51, true);
			this.InputGSTVATRecoverableCalcEdit.Name = "InputGSTVATRecoverableCalcEdit";
			this.InputGSTVATRecoverableCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.InputGSTVATRecoverableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.InputGSTVATRecoverableCalcEdit.TabIndex = 7;
			this.InputGSTVATRecoverableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AutoRatingUniversalCodeMappingGroupBox
			// 
			this.AutoRatingUniversalCodeMappingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|e846c103-1f3d-4484-ae02-278767337965", "Universal Charge Code Mappings");
			this.AutoRatingUniversalCodeMappingGroupBox.Controls.Add(this.UniversalChargeCodeMappingsGrid);
			this.AutoRatingUniversalCodeMappingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 148, true);
			this.AutoRatingUniversalCodeMappingGroupBox.Name = "AutoRatingUniversalCodeMappingGroupBox";
			this.AutoRatingUniversalCodeMappingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 98, true);
			this.AutoRatingUniversalCodeMappingGroupBox.TabIndex = 10;
			this.AutoRatingUniversalCodeMappingGroupBox.TabStop = false;
			// 
			// UniversalChargeCodeMappingsGrid
			// 
			this.UniversalChargeCodeMappingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UniversalChargeCodeMappingsGrid, "UniversalChargeCodeMappingsCollection");
			this.UniversalChargeCodeMappingsGrid.CaptionVisible = false;
			zDropEditBoxColumnStyleInfo1.ColumnName = "AUP_Type";
			zDropEditBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditBoxColumnStyleInfo2.ColumnName = "AUP_TransportMode";
			zDropEditBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.Carriers";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AUP_OH_Carrier";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			chargeCodeColumnStyleInfo1.BindToList = "Lookups.ChargeCodeBizoCollection";
			chargeCodeColumnStyleInfo1.ColumnName = "AUP_Code";
			chargeCodeColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "AUP_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UniversalChargeCodeMappingsGrid.ColumnStyles.Add(zDropEditBoxColumnStyleInfo1);
			this.UniversalChargeCodeMappingsGrid.ColumnStyles.Add(zDropEditBoxColumnStyleInfo2);
			this.UniversalChargeCodeMappingsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UniversalChargeCodeMappingsGrid.ColumnStyles.Add(chargeCodeColumnStyleInfo1);
			this.UniversalChargeCodeMappingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UniversalChargeCodeMappingsGrid.GridId = "970663f7-5e62-4e24-994a-0429743919db";
			this.UniversalChargeCodeMappingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UniversalChargeCodeMappingsGrid.LayoutKey = "UniversalChargeCodeMappingsGrid";
			this.UniversalChargeCodeMappingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.UniversalChargeCodeMappingsGrid.Name = "UniversalChargeCodeMappingsGrid";
			this.UniversalChargeCodeMappingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 72, true);
			this.UniversalChargeCodeMappingsGrid.TabIndex = 0;
			// 
			// DetailsDisbursementSurplusAccountGuidFindBox
			// 
			this.DetailsDisbursementSurplusAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsDisbursementSurplusAccountGuidFindBox, "AC_AG_DisbursementSurplusAccount");
			this.DetailsDisbursementSurplusAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 208, true);
			this.DetailsDisbursementSurplusAccountGuidFindBox.Name = "DetailsDisbursementSurplusAccountGuidFindBox";
			this.DetailsDisbursementSurplusAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsDisbursementSurplusAccountGuidFindBox.ParentType = null;
			this.DetailsDisbursementSurplusAccountGuidFindBox.PopupCaption = null;
			this.DetailsDisbursementSurplusAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsDisbursementSurplusAccountGuidFindBox.TabIndex = 12;
			// 
			// DetailsDisbursementShortfallAccountGuidFindBox
			// 
			this.DetailsDisbursementShortfallAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsDisbursementShortfallAccountGuidFindBox, "AC_AG_DisbursementShortfallAccount");
			this.DetailsDisbursementShortfallAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 232, true);
			this.DetailsDisbursementShortfallAccountGuidFindBox.Name = "DetailsDisbursementShortfallAccountGuidFindBox";
			this.DetailsDisbursementShortfallAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DetailsDisbursementShortfallAccountGuidFindBox.ParentType = null;
			this.DetailsDisbursementShortfallAccountGuidFindBox.PopupCaption = null;
			this.DetailsDisbursementShortfallAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			this.DetailsDisbursementShortfallAccountGuidFindBox.TabIndex = 13;
			this.DetailsTabPage.PerformLayout();
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.ResumeLayout(true);
			this.AC_AW_WithholdingTaxRateBoundGuidFindBox.PerformLayout();
			this.AC_AT_GSTRateBoundGuidFindBox.ResumeLayout(true);
			this.AC_AT_GSTRateBoundGuidFindBox.PerformLayout();
			this.GLAccountSetupGroupBox.ResumeLayout(false);
			this.GLAccountSetupGroupBox.PerformLayout();
			this.SalesGroupGuidFindBox.ResumeLayout(true);
			this.SalesGroupGuidFindBox.PerformLayout();
			this.ExpenseGroupGuidFindBox.ResumeLayout(true);
			this.ExpenseGroupGuidFindBox.PerformLayout();
			this.DetailsRevenueAccountBoundFindBox.ResumeLayout(true);
			this.DetailsRevenueAccountBoundFindBox.PerformLayout();
			this.DetailsRevenueClearingAccountFindBox.ResumeLayout(true);
			this.DetailsRevenueClearingAccountFindBox.PerformLayout();
			this.DetailsCostClearingAccountFindBox.ResumeLayout(true);
			this.DetailsCostClearingAccountFindBox.PerformLayout();
			this.DetailsWIPAccountBoundFindBox.ResumeLayout(true);
			this.DetailsWIPAccountBoundFindBox.PerformLayout();
			this.DetailsCostAccountGuidFindBox.ResumeLayout(true);
			this.DetailsCostAccountGuidFindBox.PerformLayout();
			this.DetailsAccrualAccountGuidFindBox.ResumeLayout(true);
			this.DetailsAccrualAccountGuidFindBox.PerformLayout();
			this.ChargeTypeDropEdit.ResumeLayout(true);
			this.ChargeTypeDropEdit.PerformLayout();
			this.AutoRatingGroupBox.ResumeLayout(false);
			this.AutoRatingGroupBox.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.ChargeSubGroupDropEdit.ResumeLayout(true);
			this.ChargeSubGroupDropEdit.PerformLayout();
			this.ChargeGroupDropEdit.ResumeLayout(true);
			this.ChargeGroupDropEdit.PerformLayout();
			this.AC_RateCalculatorDropEdit.ResumeLayout(true);
			this.AC_RateCalculatorDropEdit.PerformLayout();
			this.AutoRatingUniversalCodeMappingGroupBox.ResumeLayout(false);
			this.AutoRatingUniversalCodeMappingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UniversalChargeCodeMappingsGrid)).EndInit();
			this.UniversalChargeCodeMappingsGrid.ResumeLayout(false);
			this.UniversalChargeCodeMappingsGrid.PerformLayout();
			this.DetailsDisbursementSurplusAccountGuidFindBox.ResumeLayout(true);
			this.DetailsDisbursementSurplusAccountGuidFindBox.PerformLayout();
			this.DetailsDisbursementShortfallAccountGuidFindBox.ResumeLayout(true);
			this.DetailsDisbursementShortfallAccountGuidFindBox.PerformLayout();
			this.DetailsTabPage.ResumeLayout(true);
		}

		private void SupplyTypeOverrideTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo46 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo47 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo48 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo49 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo50 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();

			this.SupplyTypeOverrideGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SupplyTypeOverrideTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupplyTypeOverrideGrid)).BeginInit();
			this.SupplyTypeOverrideGrid.SuspendLayout();
			this.SupplyTypeOverrideTabPage.Controls.Add(this.SupplyTypeOverrideGrid);
			// 
			// SupplyTypeOverrideGrid
			// 
			this.SupplyTypeOverrideGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupplyTypeOverrideGrid, "SupplyTypeOverrides");
			this.SupplyTypeOverrideGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo46.ColumnName = "ACS_JobType";
			zDropEditColumnStyleInfo46.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo47.ColumnName = "ACS_TransportMode";
			zDropEditColumnStyleInfo47.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo48.ColumnName = "ACS_Direction";
			zDropEditColumnStyleInfo48.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo49.ColumnName = "ACS_IncoTerm";
			zDropEditColumnStyleInfo49.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo50.ColumnName = "ACS_SupplyType";
			zDropEditColumnStyleInfo50.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnStyleInfo15.ColumnName = "ACS_GE";
			zGuidFindBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.SupplyTypeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo46);
			this.SupplyTypeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo47);
			this.SupplyTypeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo48);
			this.SupplyTypeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo49);
			this.SupplyTypeOverrideGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo15);
			this.SupplyTypeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo50);
			this.SupplyTypeOverrideGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyTypeOverrideGrid.GridId = "2b5e0ab6-9421-4434-97d8-171b67776d43";
			this.SupplyTypeOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupplyTypeOverrideGrid.LayoutKey = "SupplyTypeOverrideGrid";
			this.SupplyTypeOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyTypeOverrideGrid.Name = "SupplyTypeOverrideGrid";
			this.SupplyTypeOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.SupplyTypeOverrideGrid.TabIndex = 0;
			this.SupplyTypeOverrideTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupplyTypeOverrideGrid)).EndInit();
			this.SupplyTypeOverrideGrid.ResumeLayout(false);
			this.SupplyTypeOverrideGrid.PerformLayout();
			this.SupplyTypeOverrideTabPage.ResumeLayout(true);
		}

		private void PlaceOfSupplyConfigurationTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.PlaceOfSupplyConfiguration = new Enterprise.MasterFiles.GUI.AccPlaceOfSupplyConfigurationControl();
			this.PlaceOfSupplyConfigurationTabPage.SuspendLayout();
			this.PlaceOfSupplyConfiguration.SuspendLayout();
			this.PlaceOfSupplyConfigurationTabPage.Controls.Add(this.PlaceOfSupplyConfiguration);
			// 
			// PlaceOfSupplyConfiguration
			// 
			this.PlaceOfSupplyConfiguration.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfSupplyConfiguration, "PlaceOfSupplyConfigurations");
			this.PlaceOfSupplyConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlaceOfSupplyConfiguration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PlaceOfSupplyConfiguration.Name = "PlaceOfSupplyConfiguration";
			this.PlaceOfSupplyConfiguration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 468, true);
			this.PlaceOfSupplyConfiguration.TabIndex = 1;
			this.PlaceOfSupplyConfigurationTabPage.PerformLayout();
			this.PlaceOfSupplyConfiguration.ResumeLayout(true);
			this.PlaceOfSupplyConfiguration.PerformLayout();
			this.PlaceOfSupplyConfigurationTabPage.ResumeLayout(true);
		}

		private void TaxOverridesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TaxOverridesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TaxOverridesTabPageTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AC_AX_TaxOverrideGroupGiudFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AC_AT_GSTRateGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TaxOverridesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxOverridesGrid)).BeginInit();
			this.TaxOverridesGrid.SuspendLayout();
			this.TaxOverridesTabPageTopPanel.SuspendLayout();
			this.AC_AX_TaxOverrideGroupGiudFindBox.SuspendLayout();
			this.AC_AT_GSTRateGuidFindBox.SuspendLayout();
			this.TaxOverridesTabPage.Controls.Add(this.TaxOverridesGrid);
			this.TaxOverridesTabPage.Controls.Add(this.TaxOverridesTabPageTopPanel);
			// 
			// TaxOverridesGrid
			// 
			this.TaxOverridesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxOverridesGrid, "TaxOverrides");
			this.TaxOverridesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AO_CostSellAll";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "AO_JobType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "AO_TransactionContext";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo4.ColumnName = "AO_Direction";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "AO_SupplyType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("45e4316e-dd88-42ed-b42a-01d01e30144c", "Transport Mode");
			zDropEditColumnStyleInfo6.ColumnName = "AO_TransportMode";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "AO_IncoTerm";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo8.ColumnName = "AO_Origin";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo9.ColumnName = "AO_Destination";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo10.ColumnName = "AO_TaxRegCntryOrGroup";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zDropEditColumnStyleInfo11.ColumnName = "AO_DefaultingRule";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AO_AT";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo12.ColumnName = "AO_CustomsStatus";
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "AO_VATExemptOnExportCharges";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zCheckBoxColumnStyleInfo2.ColumnName = "AO_SplitPaymentVATOrganisation";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zDropEditColumnStyleInfo13.ColumnName = "AO_HomeCountryOrZone";
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AO_A9_DefaultVATClass";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo14.ColumnName = "AO_OrganisationCategory";
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AO_GB";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo15.ColumnName = "AO_DebtorRole";
			zDropEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.TaxOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.TaxOverridesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TaxOverridesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.TaxOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.TaxOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.TaxOverridesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxOverridesGrid.GridId = "17DDB7D7-1402-4F1A-A27F-42FE0B6FEBBD";
			this.TaxOverridesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxOverridesGrid.LayoutKey = "zGrid1";
			this.TaxOverridesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 66, true);
			this.TaxOverridesGrid.Name = "TaxOverridesGrid";
			this.TaxOverridesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 408, true);
			this.TaxOverridesGrid.TabIndex = 1;
			// 
			// TaxOverridesTabPageTopPanel
			// 
			this.TaxOverridesTabPageTopPanel.Controls.Add(this.AC_AX_TaxOverrideGroupGiudFindBox);
			this.TaxOverridesTabPageTopPanel.Controls.Add(this.AC_AT_GSTRateGuidFindBox);
			this.TaxOverridesTabPageTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TaxOverridesTabPageTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxOverridesTabPageTopPanel.Name = "TaxOverridesTabPageTopPanel";
			this.TaxOverridesTabPageTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 66, true);
			this.TaxOverridesTabPageTopPanel.TabIndex = 0;
			// 
			// AC_AX_TaxOverrideGroupGiudFindBox
			// 
			this.AC_AX_TaxOverrideGroupGiudFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_AX_TaxOverrideGroupGiudFindBox, "AC_AX_TaxOverrideGroup");
			this.AC_AX_TaxOverrideGroupGiudFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 35, true);
			this.AC_AX_TaxOverrideGroupGiudFindBox.Name = "AC_AX_TaxOverrideGroupGiudFindBox";
			this.AC_AX_TaxOverrideGroupGiudFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AC_AX_TaxOverrideGroupGiudFindBox.ParentType = null;
			this.AC_AX_TaxOverrideGroupGiudFindBox.PopupCaption = null;
			this.AC_AX_TaxOverrideGroupGiudFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 20, true);
			this.AC_AX_TaxOverrideGroupGiudFindBox.TabIndex = 1;
			// 
			// AC_AT_GSTRateGuidFindBox
			// 
			this.AC_AT_GSTRateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_AT_GSTRateGuidFindBox, "AC_AT_GSTRate");
			this.AC_AT_GSTRateGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|c01f0cbf-283b-45a1-9105-8c0282623c71", "Default Tax ID", "The default Tax ID unless specifically overridden by a Tax ID below.");
			this.AC_AT_GSTRateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 9, true);
			this.AC_AT_GSTRateGuidFindBox.Name = "AC_AT_GSTRateGuidFindBox";
			this.AC_AT_GSTRateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AC_AT_GSTRateGuidFindBox.ParentType = null;
			this.AC_AT_GSTRateGuidFindBox.PopupCaption = null;
			this.AC_AT_GSTRateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 20, true);
			this.AC_AT_GSTRateGuidFindBox.TabIndex = 0;
			this.TaxOverridesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxOverridesGrid)).EndInit();
			this.TaxOverridesGrid.ResumeLayout(false);
			this.TaxOverridesGrid.PerformLayout();
			this.TaxOverridesTabPageTopPanel.ResumeLayout(false);
			this.TaxOverridesTabPageTopPanel.PerformLayout();
			this.AC_AX_TaxOverrideGroupGiudFindBox.ResumeLayout(true);
			this.AC_AX_TaxOverrideGroupGiudFindBox.PerformLayout();
			this.AC_AT_GSTRateGuidFindBox.ResumeLayout(true);
			this.AC_AT_GSTRateGuidFindBox.PerformLayout();
			this.TaxOverridesTabPage.ResumeLayout(true);
		}

		private void ChargeTypeOverridesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo16 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo17 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo18 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo19 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ChargeTypeOverridesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ChargeTypeOverridesTabPageTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DefaultChargeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultMarginCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeTypeOverridesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeTypeOverridesGrid)).BeginInit();
			this.ChargeTypeOverridesGrid.SuspendLayout();
			this.ChargeTypeOverridesTabPageTopPanel.SuspendLayout();
			this.DefaultChargeTypeDropEdit.SuspendLayout();
			this.ChargeTypeOverridesTabPage.Controls.Add(this.ChargeTypeOverridesGrid);
			this.ChargeTypeOverridesTabPage.Controls.Add(this.ChargeTypeOverridesTabPageTopPanel);
			// 
			// ChargeTypeOverridesGrid
			// 
			this.ChargeTypeOverridesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeTypeOverridesGrid, "ChargeTypeOverrides");
			this.ChargeTypeOverridesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo16.ColumnName = "AN_JobDirection";
			zDropEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo17.ColumnName = "AN_JobType";
			zDropEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo18.ColumnName = "AN_ChargeType";
			zDropEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AN_MarginPercentage";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDropEditColumnStyleInfo19.ColumnName = "AN_InvoiceType";
			zDropEditColumnStyleInfo19.ToolTip = null;
			zDropEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ChargeTypeOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo16);
			this.ChargeTypeOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo17);
			this.ChargeTypeOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo18);
			this.ChargeTypeOverridesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChargeTypeOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo19);
			this.ChargeTypeOverridesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeTypeOverridesGrid.GridId = "88cdd1e3-cf86-4b64-a5bc-afd2c753c971";
			this.ChargeTypeOverridesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeTypeOverridesGrid.LayoutKey = "zGrid1";
			this.ChargeTypeOverridesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.ChargeTypeOverridesGrid.Name = "ChargeTypeOverridesGrid";
			this.ChargeTypeOverridesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 438, true);
			this.ChargeTypeOverridesGrid.TabIndex = 1;
			// 
			// ChargeTypeOverridesTabPageTopPanel
			// 
			this.ChargeTypeOverridesTabPageTopPanel.Controls.Add(this.DefaultChargeTypeDropEdit);
			this.ChargeTypeOverridesTabPageTopPanel.Controls.Add(this.DefaultMarginCalcEdit);
			this.ChargeTypeOverridesTabPageTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ChargeTypeOverridesTabPageTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeTypeOverridesTabPageTopPanel.Name = "ChargeTypeOverridesTabPageTopPanel";
			this.ChargeTypeOverridesTabPageTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 36, true);
			this.ChargeTypeOverridesTabPageTopPanel.TabIndex = 0;
			// 
			// DefaultChargeTypeDropEdit
			// 
			this.DefaultChargeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultChargeTypeDropEdit, "AC_ChargeType");
			this.DefaultChargeTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|8450ad23-1863-4a77-ac2b-67599d815285", "Default Charge Type", "DSB - Disbursement. where 100% of the Revenue is to be Accrued or no Profit is expected. Can only be used for Job related charges.\r\nMRG - Margin. Where there are Cost or Revenue or Both. Can only be used for Job related charges.  \r\nREV - Revenue. where there are No Cost involved, pure Revenue only. Can be used for both Job related charges and Non-Job related charges. \r\nOVR - Overhead. where there are No Revenue involved, pure Overhead only. Can be used only for Non-Job related charges. \r\nNON ? Non-Accruing. where there are Cost or Revenue or Both. Can only be used for Non-Job related charges.");
			this.DefaultChargeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 8, true);
			this.DefaultChargeTypeDropEdit.Name = "DefaultChargeTypeDropEdit";
			this.DefaultChargeTypeDropEdit.ShouldResizeByMaxLength = true;
			this.DefaultChargeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.DefaultChargeTypeDropEdit.TabIndex = 0;
			// 
			// DefaultMarginCalcEdit
			// 
			this.DefaultMarginCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DefaultMarginCalcEdit, "AC_MarginPercentage");
			this.DefaultMarginCalcEdit.CaptionResourceString = null;
			this.DefaultMarginCalcEdit.DecimalPlaces = 2;
			this.DefaultMarginCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 8, true);
			this.DefaultMarginCalcEdit.Name = "DefaultMarginCalcEdit";
			this.DefaultMarginCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.DefaultMarginCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.DefaultMarginCalcEdit.TabIndex = 1;
			this.DefaultMarginCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ChargeTypeOverridesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeTypeOverridesGrid)).EndInit();
			this.ChargeTypeOverridesGrid.ResumeLayout(false);
			this.ChargeTypeOverridesGrid.PerformLayout();
			this.ChargeTypeOverridesTabPageTopPanel.ResumeLayout(false);
			this.ChargeTypeOverridesTabPageTopPanel.PerformLayout();
			this.DefaultChargeTypeDropEdit.ResumeLayout(true);
			this.DefaultChargeTypeDropEdit.PerformLayout();
			this.ChargeTypeOverridesTabPage.ResumeLayout(true);
		}

		private void RevenueRecOverridesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo30 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo31 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo32 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo33 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo34 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RevenueRecOverridesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RevenueRecOverridesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RevenueRecOverridesGrid)).BeginInit();
			this.RevenueRecOverridesGrid.SuspendLayout();
			this.RevenueRecOverridesTabPage.Controls.Add(this.RevenueRecOverridesGrid);
			// 
			// RevenueRecOverridesGrid
			// 
			this.RevenueRecOverridesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RevenueRecOverridesGrid, "RevenueRecOverrides");
			this.RevenueRecOverridesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo30.ColumnName = "AE_JobType";
			zDropEditColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo31.ColumnName = "AE_Direction";
			zDropEditColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo32.ColumnName = "AE_Mode";
			zDropEditColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo33.ColumnName = "AE_BrokerType";
			zDropEditColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo34.ColumnName = "AE_RecognitionType";
			zDropEditColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.RevenueRecOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo30);
			this.RevenueRecOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo31);
			this.RevenueRecOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo32);
			this.RevenueRecOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo33);
			this.RevenueRecOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo34);
			this.RevenueRecOverridesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RevenueRecOverridesGrid.GridId = "f4aaa531-ee15-4e7a-b98c-fa400395d01b";
			this.RevenueRecOverridesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RevenueRecOverridesGrid.LayoutKey = "zGrid1";
			this.RevenueRecOverridesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RevenueRecOverridesGrid.Name = "RevenueRecOverridesGrid";
			this.RevenueRecOverridesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.RevenueRecOverridesGrid.TabIndex = 0;
			this.RevenueRecOverridesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RevenueRecOverridesGrid)).EndInit();
			this.RevenueRecOverridesGrid.ResumeLayout(false);
			this.RevenueRecOverridesGrid.PerformLayout();
			this.RevenueRecOverridesTabPage.ResumeLayout(true);
		}

		private void GLPostingOverridesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo35 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo36 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo37 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo38 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo39 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo40 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo41 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.GLPostingOverridesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GLPostingOverridesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RevenueAccountBoundFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.WIPAccountBoundFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CostAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AccrualAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.revenueClearingAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.costClearingAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GLPostingOverridesTabPage.SuspendLayout();
			this.GLPostingOverridesPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLPostingOverridesGrid)).BeginInit();
			this.GLPostingOverridesGrid.SuspendLayout();
			this.RevenueAccountBoundFindBox.SuspendLayout();
			this.WIPAccountBoundFindBox.SuspendLayout();
			this.CostAccountGuidFindBox.SuspendLayout();
			this.AccrualAccountGuidFindBox.SuspendLayout();
			this.revenueClearingAccountFindBox.SuspendLayout();
			this.costClearingAccountFindBox.SuspendLayout();
			this.GLPostingOverridesTabPage.Controls.Add(this.GLPostingOverridesPanel);
			// 
			// GLPostingOverridesPanel
			// 
			this.GLPostingOverridesPanel.Controls.Add(this.costClearingAccountFindBox);
			this.GLPostingOverridesPanel.Controls.Add(this.revenueClearingAccountFindBox);
			this.GLPostingOverridesPanel.Controls.Add(this.GLPostingOverridesGrid);
			this.GLPostingOverridesPanel.Controls.Add(this.RevenueAccountBoundFindBox);
			this.GLPostingOverridesPanel.Controls.Add(this.WIPAccountBoundFindBox);
			this.GLPostingOverridesPanel.Controls.Add(this.CostAccountGuidFindBox);
			this.GLPostingOverridesPanel.Controls.Add(this.AccrualAccountGuidFindBox);
			this.GLPostingOverridesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GLPostingOverridesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GLPostingOverridesPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GLPostingOverridesPanel.Name = "GLPostingOverridesPanel";
			this.GLPostingOverridesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 468, true);
			this.GLPostingOverridesPanel.TabIndex = 1;
			// 
			// GLPostingOverridesGrid
			// 
			this.GLPostingOverridesGrid.AllowNavigation = false;
			this.GLPostingOverridesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GLPostingOverridesGrid, "GLPostingOverrides");
			this.GLPostingOverridesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo35.ColumnName = "Y1_ConsolidationAccountingCategoryClass";
			zDropEditColumnStyleInfo35.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.ColumnName = "Y1_GE";
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo8.ColumnName = "Y1_AG_REV";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo9.ColumnName = "Y1_AG_WIP";
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo10.ColumnName = "Y1_AG_CST";
			zGuidFindBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo11.ColumnName = "Y1_AG_ACR";
			zGuidFindBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo12.ColumnName = "Y1_AG_REV_Clearing";
			zGuidFindBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo13.ColumnName = "Y1_AG_CST_Clearing";
			zGuidFindBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo36.ColumnName = "Y1_JobType";
			zDropEditColumnStyleInfo36.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo37.ColumnName = "Y1_TransportMode";
			zDropEditColumnStyleInfo37.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo38.ColumnName = "Y1_Direction";
			zDropEditColumnStyleInfo38.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo39.ColumnName = "Y1_ConsolContainerMode";
			zDropEditColumnStyleInfo39.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zDropEditColumnStyleInfo40.ColumnName = "Y1_MasterPaymentType";
			zDropEditColumnStyleInfo40.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo41.ColumnName = "Y1_HousePaymentType";
			zDropEditColumnStyleInfo41.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo35);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo10);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo11);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo12);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo13);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo36);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo37);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo38);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo39);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo40);
			this.GLPostingOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo41);
			this.GLPostingOverridesGrid.GridId = "d6780049-f7ce-45ec-a211-6481994704d4";
			this.GLPostingOverridesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GLPostingOverridesGrid.LayoutKey = "GLPostingOverridesGrid";
			this.GLPostingOverridesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.GLPostingOverridesGrid.Name = "GLPostingOverridesGrid";
			this.GLPostingOverridesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 294, true);
			this.GLPostingOverridesGrid.TabIndex = 15;
			// 
			// RevenueAccountBoundFindBox
			// 
			this.RevenueAccountBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RevenueAccountBoundFindBox, "AC_AG_RevenueAccount");
			this.RevenueAccountBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 9, true);
			this.RevenueAccountBoundFindBox.Name = "RevenueAccountBoundFindBox";
			this.RevenueAccountBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.RevenueAccountBoundFindBox.ParentType = null;
			this.RevenueAccountBoundFindBox.PopupCaption = null;
			this.RevenueAccountBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 20, true);
			this.RevenueAccountBoundFindBox.TabIndex = 6;
			// 
			// WIPAccountBoundFindBox
			// 
			this.WIPAccountBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WIPAccountBoundFindBox, "AC_AG_RevenueClearingAccount");
			this.WIPAccountBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 61, true);
			this.WIPAccountBoundFindBox.Name = "WIPAccountBoundFindBox";
			this.WIPAccountBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WIPAccountBoundFindBox.ParentType = null;
			this.WIPAccountBoundFindBox.PopupCaption = null;
			this.WIPAccountBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 20, true);
			this.WIPAccountBoundFindBox.TabIndex = 8;
			// 
			// CostAccountGuidFindBox
			// 
			this.CostAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CostAccountGuidFindBox, "AC_AG_CostAccount");
			this.CostAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 87, true);
			this.CostAccountGuidFindBox.Name = "CostAccountGuidFindBox";
			this.CostAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CostAccountGuidFindBox.ParentType = null;
			this.CostAccountGuidFindBox.PopupCaption = null;
			this.CostAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 20, true);
			this.CostAccountGuidFindBox.TabIndex = 9;
			// 
			// AccrualAccountGuidFindBox
			// 
			this.AccrualAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccrualAccountGuidFindBox, "AC_AG_AccrualAccount");
			this.AccrualAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 139, true);
			this.AccrualAccountGuidFindBox.Name = "AccrualAccountGuidFindBox";
			this.AccrualAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AccrualAccountGuidFindBox.ParentType = null;
			this.AccrualAccountGuidFindBox.PopupCaption = null;
			this.AccrualAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 20, true);
			this.AccrualAccountGuidFindBox.TabIndex = 11;
			// 
			// revenueClearingAccountFindBox
			// 
			this.revenueClearingAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.revenueClearingAccountFindBox, "AC_AG_WIPAccount");
			this.revenueClearingAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 35, true);
			this.revenueClearingAccountFindBox.Name = "revenueClearingAccountFindBox";
			this.revenueClearingAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.revenueClearingAccountFindBox.ParentType = null;
			this.revenueClearingAccountFindBox.PopupCaption = null;
			this.revenueClearingAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 20, true);
			this.revenueClearingAccountFindBox.TabIndex = 7;
			// 
			// costClearingAccountFindBox
			// 
			this.costClearingAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.costClearingAccountFindBox, "AC_AG_CostClearingAccount");
			this.costClearingAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 113, true);
			this.costClearingAccountFindBox.Name = "costClearingAccountFindBox";
			this.costClearingAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.costClearingAccountFindBox.ParentType = null;
			this.costClearingAccountFindBox.PopupCaption = null;
			this.costClearingAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 20, true);
			this.costClearingAccountFindBox.TabIndex = 10;
			this.GLPostingOverridesTabPage.PerformLayout();
			this.GLPostingOverridesPanel.ResumeLayout(false);
			this.GLPostingOverridesPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLPostingOverridesGrid)).EndInit();
			this.GLPostingOverridesGrid.ResumeLayout(false);
			this.GLPostingOverridesGrid.PerformLayout();
			this.RevenueAccountBoundFindBox.ResumeLayout(true);
			this.RevenueAccountBoundFindBox.PerformLayout();
			this.WIPAccountBoundFindBox.ResumeLayout(true);
			this.WIPAccountBoundFindBox.PerformLayout();
			this.CostAccountGuidFindBox.ResumeLayout(true);
			this.CostAccountGuidFindBox.PerformLayout();
			this.AccrualAccountGuidFindBox.ResumeLayout(true);
			this.AccrualAccountGuidFindBox.PerformLayout();
			this.revenueClearingAccountFindBox.ResumeLayout(true);
			this.revenueClearingAccountFindBox.PerformLayout();
			this.costClearingAccountFindBox.ResumeLayout(true);
			this.costClearingAccountFindBox.PerformLayout();
			this.GLPostingOverridesTabPage.ResumeLayout(true);
		}

		private void GLPostingOverridesPanel_Paint(object sender, PaintEventArgs e)
		{
		}

		private void BranchOverridesTab_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo20 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo21 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo22 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo23 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.BranchOverridesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BranchOverridesTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchOverridesGrid)).BeginInit();
			this.BranchOverridesGrid.SuspendLayout();
			this.BranchOverridesTab.Controls.Add(this.BranchOverridesGrid);
			// 
			// BranchOverridesGrid
			// 
			this.BranchOverridesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BranchOverridesGrid, "BranchOverrides");
			this.BranchOverridesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo20.ColumnName = "YA_JobType";
			zDropEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo21.ColumnName = "YA_Direction";
			zDropEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo22.ColumnName = "YA_TransportMode";
			zDropEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zDropEditColumnStyleInfo23.ColumnName = "YA_DefaultingRule";
			zDropEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "YA_GB_SpecificBranch";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.BranchOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo20);
			this.BranchOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo21);
			this.BranchOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo22);
			this.BranchOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo23);
			this.BranchOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.BranchOverridesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BranchOverridesGrid.GridId = "64480855-82b7-499d-88fc-5fe30f4b58e9";
			this.BranchOverridesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BranchOverridesGrid.LayoutKey = "zGrid1";
			this.BranchOverridesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BranchOverridesGrid.Name = "BranchOverridesGrid";
			this.BranchOverridesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 468, true);
			this.BranchOverridesGrid.TabIndex = 1;
			this.BranchOverridesTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchOverridesGrid)).EndInit();
			this.BranchOverridesGrid.ResumeLayout(false);
			this.BranchOverridesGrid.PerformLayout();
			this.BranchOverridesTab.ResumeLayout(true);
		}

		private void SellComplianceDescriptionTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo43 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo44 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo45 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SellComplianceDescriptionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SellComplianceDescriptionTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SellComplianceDescriptionGrid)).BeginInit();
			this.SellComplianceDescriptionGrid.SuspendLayout();
			this.SellComplianceDescriptionTabPage.Controls.Add(this.SellComplianceDescriptionGrid);
			// 
			// SellComplianceDescriptionGrid
			// 
			this.SellComplianceDescriptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SellComplianceDescriptionGrid, "ChargeComplianceDescriptions");
			this.SellComplianceDescriptionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo43.ColumnName = "ADE_JobType";
			zDropEditColumnStyleInfo43.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo44.ColumnName = "ADE_TransportMode";
			zDropEditColumnStyleInfo44.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo45.ColumnName = "ADE_SupplyType";
			zDropEditColumnStyleInfo45.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zTextBoxColumnStyleInfo5.ColumnName = "ADE_Description";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.SellComplianceDescriptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo43);
			this.SellComplianceDescriptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo44);
			this.SellComplianceDescriptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo45);
			this.SellComplianceDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SellComplianceDescriptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SellComplianceDescriptionGrid.GridId = "2ccf9320-df7a-477b-b12e-8568a4b87a3f";
			this.SellComplianceDescriptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SellComplianceDescriptionGrid.LayoutKey = "SellComplianceDescriptionGrid";
			this.SellComplianceDescriptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SellComplianceDescriptionGrid.Name = "SellComplianceDescriptionGrid";
			this.SellComplianceDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 468, true);
			this.SellComplianceDescriptionGrid.TabIndex = 0;
			this.SellComplianceDescriptionTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SellComplianceDescriptionGrid)).EndInit();
			this.SellComplianceDescriptionGrid.ResumeLayout(false);
			this.SellComplianceDescriptionGrid.PerformLayout();
			this.SellComplianceDescriptionTabPage.ResumeLayout(true);
		}

		private void CreditorOverridesTab_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo24 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo25 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo26 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo27 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo28 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo29 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CreditorOverridesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CreditorOverridesTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CreditorOverridesGrid)).BeginInit();
			this.CreditorOverridesGrid.SuspendLayout();
			this.CreditorOverridesTab.Controls.Add(this.CreditorOverridesGrid);
			// 
			// CreditorOverridesGrid
			// 
			this.CreditorOverridesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CreditorOverridesGrid, "CreditorOverrides");
			this.CreditorOverridesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo24.ColumnName = "ACC_JobType";
			zDropEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo25.ColumnName = "ACC_Direction";
			zDropEditColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo26.ColumnName = "ACC_TransportMode";
			zDropEditColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zDropEditColumnStyleInfo27.ColumnName = "ACC_PaymentTerm";
			zDropEditColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo28.ColumnName = "ACC_DefaultingRule";
			zDropEditColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "ACC_GE_Department";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo29.ColumnName = "ACC_CreditorRole";
			zDropEditColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "ACC_OH_Creditor";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.ColumnName = "CreditorName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.CreditorOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo24);
			this.CreditorOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo25);
			this.CreditorOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo26);
			this.CreditorOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo27);
			this.CreditorOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo28);
			this.CreditorOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.CreditorOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo29);
			this.CreditorOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.CreditorOverridesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CreditorOverridesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreditorOverridesGrid.GridId = "64480855-82b7-499d-88fc-5fe30f4b58e0";
			this.CreditorOverridesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CreditorOverridesGrid.LayoutKey = "zGrid1";
			this.CreditorOverridesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CreditorOverridesGrid.Name = "CreditorOverridesGrid";
			this.CreditorOverridesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 468, true);
			this.CreditorOverridesGrid.TabIndex = 1;
			this.CreditorOverridesTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CreditorOverridesGrid)).EndInit();
			this.CreditorOverridesGrid.ResumeLayout(false);
			this.CreditorOverridesGrid.PerformLayout();
			this.CreditorOverridesTab.ResumeLayout(true);
		}

		private void AirlineIATACodeTab_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo42 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AirlineIATACodeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AirlineIATACodeTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AirlineIATACodeGrid)).BeginInit();
			this.AirlineIATACodeGrid.SuspendLayout();
			this.AirlineIATACodeTab.Controls.Add(this.AirlineIATACodeGrid);
			// 
			// AirlineIATACodeGrid
			// 
			this.AirlineIATACodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AirlineIATACodeGrid, "AccChargeCodeCarrierIataMappings");
			this.AirlineIATACodeGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo14.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d9a8ad36-4897-4f26-b083-5aaa338cfd06", "Airline Org.");
			zGuidFindBoxColumnStyleInfo14.ColumnName = "ACI_OH_Carrier";
			zGuidFindBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3491c473-17a2-4ae5-ac39-358b6d377848", "Two Characters Code");
			zTextBoxColumnStyleInfo3.ColumnName = "AirLine2CharCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ac7376d6-586a-4af7-ad6f-5f51678bc9b2", "Airline Name");
			zTextBoxColumnStyleInfo4.ColumnName = "AirLineName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo42.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bfaaca8d-e5fd-4cd9-ab4c-aa97e1dbf07f", "IATA Code");
			zDropEditColumnStyleInfo42.ColumnName = "ACI_IATAChargeCodeMap";
			zDropEditColumnStyleInfo42.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AirlineIATACodeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo14);
			this.AirlineIATACodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AirlineIATACodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AirlineIATACodeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo42);
			this.AirlineIATACodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirlineIATACodeGrid.GridId = "7ce1c3e8-2ae7-4c3a-a1fe-c181486c4945";
			this.AirlineIATACodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AirlineIATACodeGrid.LayoutKey = "zGrid1";
			this.AirlineIATACodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AirlineIATACodeGrid.Name = "AirlineIATACodeGrid";
			this.AirlineIATACodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 468, true);
			this.AirlineIATACodeGrid.TabIndex = 1;
			this.AirlineIATACodeTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AirlineIATACodeGrid)).EndInit();
			this.AirlineIATACodeGrid.ResumeLayout(false);
			this.AirlineIATACodeGrid.PerformLayout();
			this.AirlineIATACodeTab.ResumeLayout(true);
		}

		private void zStmNoteTabPage1_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.zStmNoteTabPage1.SuspendLayout();
			this.zStmNoteTabPage1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(true);
		}

		private void GovtChargeCodeOverrideTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.GovtChargeCodeOverrides = new Enterprise.MasterFiles.GUI.AccChargeGovtChargeCodeOverrideConfigurationControl();
			this.GovtChargeCodeOverrideTabPage.SuspendLayout();
			this.GovtChargeCodeOverrides.SuspendLayout();
			this.GovtChargeCodeOverrideTabPage.Controls.Add(this.GovtChargeCodeOverrides);
			// 
			// GovtChargeCodeOverrides
			// 
			this.GovtChargeCodeOverrides.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GovtChargeCodeOverrides, "GovtChargeCodeOverrides");
			this.GovtChargeCodeOverrides.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GovtChargeCodeOverrides.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GovtChargeCodeOverrides.Name = "GovtChargeCodeOverrides";
			this.GovtChargeCodeOverrides.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.GovtChargeCodeOverrides.TabIndex = 1;
			this.GovtChargeCodeOverrideTabPage.PerformLayout();
			this.GovtChargeCodeOverrides.ResumeLayout(true);
			this.GovtChargeCodeOverrides.PerformLayout();
			this.GovtChargeCodeOverrideTabPage.ResumeLayout(true);
		}

		private void ApportionmentMethodOverrideTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ApportionmentMethodOverrides = new Enterprise.MasterFiles.GUI.AccChargeApportionmentMethodOverrideControl();
			this.ApportionmentMethodOverrideTabPage.SuspendLayout();
			this.ApportionmentMethodOverrides.SuspendLayout();
			this.ApportionmentMethodOverrideTabPage.Controls.Add(this.ApportionmentMethodOverrides);
			// 
			// ApportionmentMethodOverrides
			// 
			this.ApportionmentMethodOverrides.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApportionmentMethodOverrides, "ApportionmentMethodOverrides");
			this.ApportionmentMethodOverrides.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApportionmentMethodOverrides.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApportionmentMethodOverrides.Name = "ApportionmentMethodOverrides";
			this.ApportionmentMethodOverrides.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 474, true);
			this.ApportionmentMethodOverrides.TabIndex = 1;
			this.ApportionmentMethodOverrideTabPage.PerformLayout();
			this.ApportionmentMethodOverrides.ResumeLayout(true);
			this.ApportionmentMethodOverrides.PerformLayout();
			this.ApportionmentMethodOverrideTabPage.ResumeLayout(true);
		}

		void DelayedControlInitializationToDisplayWithFullWidth()
		{
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_Desc)));
			BindingSource.SetBindingMember(this.descriptionZTranslatableTextControl, "AC_Desc");

			descriptionZTranslatableTextControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4b3bda5f-ac48-4e26-97dd-6902d80a9bdb", "Description");
			descriptionZTranslatableTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			descriptionZTranslatableTextControl.GridCurrent = null;
			descriptionZTranslatableTextControl.GridMember = null;
			descriptionZTranslatableTextControl.IsLanguageEditingEnabled = true;
			descriptionZTranslatableTextControl.AllowDrop = true;
			descriptionZTranslatableTextControl.IsMultiLine = false;
			descriptionZTranslatableTextControl.AcceptsReturn = false;
			descriptionZTranslatableTextControl.ReadOnly = false;
			descriptionZTranslatableTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 32, true);
			descriptionZTranslatableTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			descriptionZTranslatableTextControl.Name = "descriptionZTranslatableTextControl";
			descriptionZTranslatableTextControl.TabIndex = 3;
		}

		#endregion

	}
}
