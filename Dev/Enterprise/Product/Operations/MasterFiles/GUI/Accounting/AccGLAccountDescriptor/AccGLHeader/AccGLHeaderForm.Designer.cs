using System;
using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGLHeaderForm
	{
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox AG_CodeBoundTextEdit;
		private Enterprise.ZArchitecture.ZTranslatableTextControl AG_DescriptionBoundTextEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AG_AG_PercentNumBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AG_AG_ConsolidationNumBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AG_AG_AlternateNumBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AG_DebitCreditBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AG_AccountTypeBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveBoundCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AG_ControlAccountBoundCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit AG_TotalLevelCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit AG_PrintSequenceBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox DisallowDirectPostingCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl AccGLHeaderTabControl;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox GLAccountDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox TotalReferenceFindBox;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private ZDropEdit SectionDropEdit;
		private ZTextBox AG_Calc_AccountNumberWithPrefixTextBox;
		private ZDropEdit AG_CashFlowTypeDropEdit;
		private ZCheckBox AG_IsGlobalBoundCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage CompaniesTabPage;
		private ZTranslatableTextControl AG_DescriptionBoundText;
		private ZTextBox AG_CodeBoundText;
		private ZGrid CompaniesGrid;
		private ZLabel CompanyFilterLabel;
		private ZGroupBox SubAccountTypesGroupBox;
		private ZGrid SubAccountTypesGrid;
		private IContainer components;
		private bool IsInitializing;
		private ZTabPage AlternateChartsDissectionConfigurationTabPage;
		private ZGrid AlternateChartsDissectionConfigurationGrid;
		private ZLabel NotBSHPLDissectionConfigurationLabel;
		private ZLabel NoDissectionConfigurationLabel;
		private ZLabel NoDissectionConfigurationSecurityLabel;
		private ZDropEdit AG_StatisticalUnitsBoundDropEdit;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.AccGLHeaderTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompaniesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AlternateChartsDissectionConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsUserControl.SuspendLayout();
			this.AccGLHeaderTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 390, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(287);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLHeader);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(630, 358, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// AccGLHeaderTabControl
			// 
			this.AccGLHeaderTabControl.Controls.Add(this.DetailsTabPage);
			this.AccGLHeaderTabControl.Controls.Add(this.CompaniesTabPage);
			this.AccGLHeaderTabControl.Controls.Add(this.AlternateChartsDissectionConfigurationTabPage);
			this.AccGLHeaderTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.AccGLHeaderTabControl.Controls.Add(this.zLogsTabPage1);
			this.AccGLHeaderTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 10, true);
			this.AccGLHeaderTabControl.Name = "AccGLHeaderTabControl";
			this.AccGLHeaderTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 342, true);
			this.AccGLHeaderTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|b2aadae0-f747-4b36-b48c-f5ed0f05238b", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 438, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_AccountNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_AG_PercentNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_AG_ConsolidationNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_AG_AlternateNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_DebitCredit)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_ControlAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_AccountType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_TotalLevel)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_PrintSequence)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_DisallowDirectPosting)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_AG_HeaderDependsOnTotal)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_Column)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_Calc_AccountNumberWithPrefix)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_CashFlowType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_IsGlobal)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).SubAccountTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLHeaderSubAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).SubAccountTypes)).SyncRoot)).ASA_SubClassDisplayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeaderSubAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).SubAccountTypes)).SyncRoot)).GLHeader.AG_SubAccountTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccGLHeaderSubAccount)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).SubAccountTypes)).SyncRoot)).ASA_IsSubClassValidationRuleMandatory)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_StatisticalUnits)));
			// 
			// CompaniesTabPage
			// 
			this.CompaniesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|90146d18-3af0-4ee0-b790-dae184de112e", "Companies");
			this.CompaniesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.CompaniesTabPage.Name = "CompaniesTabPage";
			this.CompaniesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CompaniesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 438, true);
			this.CompaniesTabPage.TabIndex = 4;
			this.CompaniesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CompaniesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AG_AccountNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).CompanyFilters)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderCompanyFilter)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).CompanyFilters)).SyncRoot)).ACF_GC_Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLHeaderCompanyFilter)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).CompanyFilters)).SyncRoot)).Company.GC_Name)));
			// 
			// AlternateChartsDissectionConfigurationTabPage
			// 
			this.AlternateChartsDissectionConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.AlternateChartsDissectionConfigurationTabPage.Name = "AlternateChartsDissectionConfigurationTabPage";
			this.AlternateChartsDissectionConfigurationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AlternateChartsDissectionConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 438, true);
			this.AlternateChartsDissectionConfigurationTabPage.TabIndex = 5;
			this.AlternateChartsDissectionConfigurationTabPage.Text = "Alternate Charts Dissection Configuration";
			this.AlternateChartsDissectionConfigurationTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AlternateChartsDissectionConfigurationTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AlternateGLAccountDissections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccAlternateGLAccountDissection)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AlternateGLAccountDissections)).SyncRoot)).ADC_AAC_AlternateChart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateGLAccountDissection)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AlternateGLAccountDissections)).SyncRoot)).ADC_Attribute)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateGLAccountDissection)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AlternateGLAccountDissections)).SyncRoot)).AttributeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccAlternateGLAccountDissection)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLHeader)(null)).AlternateGLAccountDissections)).SyncRoot)).ADC_SeparateNumbering)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 418, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 438, true);
			this.zLogsTabPage1.TabIndex = 3;
			// 
			// AccGLHeaderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|b55af8fe-46f6-41db-b99a-6bb5cd09cef6", "General Ledger Account");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 414, true);
			this.Controls.Add(this.AccGLHeaderTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 450, true);
			this.Name = "AccGLHeaderForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.AccGLHeaderTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.AccGLHeaderTabControl.ResumeLayout(false);
			this.AccGLHeaderTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void DetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.AG_CodeBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AG_DescriptionBoundTextEdit = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.AG_AG_PercentNumBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AG_AG_ConsolidationNumBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AG_AG_AlternateNumBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AG_DebitCreditBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AG_ControlAccountBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AG_AccountTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsActiveBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AG_TotalLevelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AG_PrintSequenceBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DisallowDirectPostingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GLAccountDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TotalReferenceFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AG_Calc_AccountNumberWithPrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AG_CashFlowTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AG_IsGlobalBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SubAccountTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SubAccountTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AG_StatisticalUnitsBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsTabPage.SuspendLayout();
			this.AG_DescriptionBoundTextEdit.SuspendLayout();
			this.AG_AG_PercentNumBoundGuidFindBox.SuspendLayout();
			this.AG_AG_ConsolidationNumBoundGuidFindBox.SuspendLayout();
			this.AG_AG_AlternateNumBoundGuidFindBox.SuspendLayout();
			this.AG_DebitCreditBoundDropEdit.SuspendLayout();
			this.AG_AccountTypeBoundDropEdit.SuspendLayout();
			this.GLAccountDetailsGroupBox.SuspendLayout();
			this.TotalReferenceFindBox.SuspendLayout();
			this.SectionDropEdit.SuspendLayout();
			this.AG_CashFlowTypeDropEdit.SuspendLayout();
			this.SubAccountTypesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubAccountTypesGrid)).BeginInit();
			this.SubAccountTypesGrid.SuspendLayout();
			this.AG_StatisticalUnitsBoundDropEdit.SuspendLayout();
			this.DetailsTabPage.Controls.Add(this.GLAccountDetailsGroupBox);
			// 
			// AG_CodeBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AG_CodeBoundTextEdit, "AG_AccountNum");
			this.AG_CodeBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 19, true);
			this.AG_CodeBoundTextEdit.Name = "AG_CodeBoundTextEdit";
			this.AG_CodeBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 15, true);
			this.AG_CodeBoundTextEdit.TabIndex = 1;
			this.AG_CodeBoundTextEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AG_CodeBoundTextEdit_KeyPress);
			// 
			// AG_DescriptionBoundTextEdit
			// 
			this.AG_DescriptionBoundTextEdit.AcceptsReturn = false;
			this.AG_DescriptionBoundTextEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_DescriptionBoundTextEdit, "AG_Description");
			this.AG_DescriptionBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AG_DescriptionBoundTextEdit.GridCurrent = null;
			this.AG_DescriptionBoundTextEdit.GridMember = null;
			this.AG_DescriptionBoundTextEdit.IsLanguageEditingEnabled = true;
			this.AG_DescriptionBoundTextEdit.IsMultiLine = false;
			this.AG_DescriptionBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 43, true);
			this.AG_DescriptionBoundTextEdit.Name = "AG_DescriptionBoundTextEdit";
			this.AG_DescriptionBoundTextEdit.ReadOnly = false;
			this.AG_DescriptionBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.AG_DescriptionBoundTextEdit.TabIndex = 5;
			// 
			// AG_AG_PercentNumBoundGuidFindBox
			// 
			this.AG_AG_PercentNumBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_AG_PercentNumBoundGuidFindBox, "AG_AG_PercentNum");
			this.AG_AG_PercentNumBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 146, true);
			this.AG_AG_PercentNumBoundGuidFindBox.Name = "AG_AG_PercentNumBoundGuidFindBox";
			this.AG_AG_PercentNumBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AG_AG_PercentNumBoundGuidFindBox.ParentType = null;
			this.AG_AG_PercentNumBoundGuidFindBox.PopupCaption = null;
			this.AG_AG_PercentNumBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 15, true);
			this.AG_AG_PercentNumBoundGuidFindBox.TabIndex = 15;
			// 
			// AG_AG_ConsolidationNumBoundGuidFindBox
			// 
			this.AG_AG_ConsolidationNumBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_AG_ConsolidationNumBoundGuidFindBox, "AG_AG_ConsolidationNum");
			this.AG_AG_ConsolidationNumBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 172, true);
			this.AG_AG_ConsolidationNumBoundGuidFindBox.Name = "AG_AG_ConsolidationNumBoundGuidFindBox";
			this.AG_AG_ConsolidationNumBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AG_AG_ConsolidationNumBoundGuidFindBox.ParentType = null;
			this.AG_AG_ConsolidationNumBoundGuidFindBox.PopupCaption = null;
			this.AG_AG_ConsolidationNumBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 15, true);
			this.AG_AG_ConsolidationNumBoundGuidFindBox.TabIndex = 16;
			// 
			// AG_AG_AlternateNumBoundGuidFindBox
			// 
			this.AG_AG_AlternateNumBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_AG_AlternateNumBoundGuidFindBox, "AG_AG_AlternateNum");
			this.AG_AG_AlternateNumBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 198, true);
			this.AG_AG_AlternateNumBoundGuidFindBox.Name = "AG_AG_AlternateNumBoundGuidFindBox";
			this.AG_AG_AlternateNumBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AG_AG_AlternateNumBoundGuidFindBox.ParentType = null;
			this.AG_AG_AlternateNumBoundGuidFindBox.PopupCaption = null;
			this.AG_AG_AlternateNumBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 15, true);
			this.AG_AG_AlternateNumBoundGuidFindBox.TabIndex = 17;
			// 
			// AG_DebitCreditBoundDropEdit
			// 
			this.AG_DebitCreditBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_DebitCreditBoundDropEdit, "AG_DebitCredit");
			this.AG_DebitCreditBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 19, true);
			this.AG_DebitCreditBoundDropEdit.Name = "AG_DebitCreditBoundDropEdit";
			this.AG_DebitCreditBoundDropEdit.ShowDescriptionBox = false;
			this.AG_DebitCreditBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
			this.AG_DebitCreditBoundDropEdit.TabIndex = 2;
			// 
			// AG_ControlAccountBoundCheckBox
			// 
			this.AG_ControlAccountBoundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AG_ControlAccountBoundCheckBox, "AG_ControlAccount");
			this.AG_ControlAccountBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 19, true);
			this.AG_ControlAccountBoundCheckBox.Name = "AG_ControlAccountBoundCheckBox";
			this.AG_ControlAccountBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 14, true);
			this.AG_ControlAccountBoundCheckBox.TabIndex = 4;
			// 
			// AG_AccountTypeBoundDropEdit
			// 
			this.AG_AccountTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_AccountTypeBoundDropEdit, "AG_AccountType");
			this.AG_AccountTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 68, true);
			this.AG_AccountTypeBoundDropEdit.Name = "AG_AccountTypeBoundDropEdit";
			this.AG_AccountTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.AG_AccountTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 15, true);
			this.AG_AccountTypeBoundDropEdit.TabIndex = 8;
			// 
			// IsActiveBoundCheckBox
			// 
			this.IsActiveBoundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveBoundCheckBox, "AG_IsActive");
			this.IsActiveBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 19, true);
			this.IsActiveBoundCheckBox.Name = "IsActiveBoundCheckBox";
			this.IsActiveBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 14, true);
			this.IsActiveBoundCheckBox.TabIndex = 3;
			// 
			// AG_TotalLevelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AG_TotalLevelCalcEdit, "AG_TotalLevel");
			this.AG_TotalLevelCalcEdit.DecimalPlaces = 2;
			this.AG_TotalLevelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 252, true);
			this.AG_TotalLevelCalcEdit.Name = "AG_TotalLevelCalcEdit";
			this.AG_TotalLevelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.AG_TotalLevelCalcEdit.TabIndex = 19;
			this.AG_TotalLevelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AG_TotalLevelCalcEdit.TrackDisposedAccess = true;
			// 
			// AG_PrintSequenceBoundCalcEdit
			// 
			this.AG_PrintSequenceBoundCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AG_PrintSequenceBoundCalcEdit, "AG_PrintSequence");
			this.AG_PrintSequenceBoundCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|3f03f045-0717-448f-8632-5543a36f5194", "Print Sequence", "Enter a number between 0 & 999 to display on the report in a sequence other than " +
					"account number.");
			this.AG_PrintSequenceBoundCalcEdit.DecimalPlaces = 2;
			this.AG_PrintSequenceBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(573, 252, true);
			this.AG_PrintSequenceBoundCalcEdit.Name = "AG_PrintSequenceBoundCalcEdit";
			this.AG_PrintSequenceBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.AG_PrintSequenceBoundCalcEdit.TabIndex = 20;
			this.AG_PrintSequenceBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AG_PrintSequenceBoundCalcEdit.TrackDisposedAccess = true;
			// 
			// DisallowDirectPostingCheckBox
			// 
			this.DisallowDirectPostingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DisallowDirectPostingCheckBox, "AG_DisallowDirectPosting");
			this.DisallowDirectPostingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 42, true);
			this.DisallowDirectPostingCheckBox.Name = "DisallowDirectPostingCheckBox";
			this.DisallowDirectPostingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 14, true);
			this.DisallowDirectPostingCheckBox.TabIndex = 7;
			// 
			// GLAccountDetailsGroupBox
			// 
			this.GLAccountDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|8f2ec05f-31f8-467b-8785-1be1ffc6413d", "GL Account Details");
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_StatisticalUnitsBoundDropEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.SubAccountTypesGroupBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_CashFlowTypeDropEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_IsGlobalBoundCheckBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_Calc_AccountNumberWithPrefixTextBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.SectionDropEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.TotalReferenceFindBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_TotalLevelCalcEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_PrintSequenceBoundCalcEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_AG_AlternateNumBoundGuidFindBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_AG_ConsolidationNumBoundGuidFindBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_AG_PercentNumBoundGuidFindBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_AccountTypeBoundDropEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_DescriptionBoundTextEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_CodeBoundTextEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_DebitCreditBoundDropEdit);
			this.GLAccountDetailsGroupBox.Controls.Add(this.DisallowDirectPostingCheckBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.AG_ControlAccountBoundCheckBox);
			this.GLAccountDetailsGroupBox.Controls.Add(this.IsActiveBoundCheckBox);
			this.GLAccountDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.GLAccountDetailsGroupBox.Name = "GLAccountDetailsGroupBox";
			this.GLAccountDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 282, true);
			this.GLAccountDetailsGroupBox.TabIndex = 0;
			this.GLAccountDetailsGroupBox.TabStop = false;
			// 
			// TotalReferenceFindBox
			// 
			this.TotalReferenceFindBox.AllowDrop = true;
			this.TotalReferenceFindBox.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.TotalReferenceFindBox, "AG_AG_HeaderDependsOnTotal");
			this.TotalReferenceFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 226, true);
			this.TotalReferenceFindBox.Name = "TotalReferenceFindBox";
			this.TotalReferenceFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TotalReferenceFindBox.ParentType = null;
			this.TotalReferenceFindBox.PopupCaption = null;
			this.TotalReferenceFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 15, true);
			this.TotalReferenceFindBox.TabIndex = 18;
			// 
			// SectionDropEdit
			// 
			this.SectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SectionDropEdit, "AG_Column");
			this.SectionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|a8db07f9-5bd0-421c-aa7b-c7000e9076e1", "Report Section");
			this.SectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 120, true);
			this.SectionDropEdit.Name = "SectionDropEdit";
			this.SectionDropEdit.PreBoundMaxLength = 2;
			this.SectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 15, true);
			this.SectionDropEdit.TabIndex = 11;
			// 
			// AG_Calc_AccountNumberWithPrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.AG_Calc_AccountNumberWithPrefixTextBox, "AG_Calc_AccountNumberWithPrefix");
			this.AG_Calc_AccountNumberWithPrefixTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|89ceff99-7fdb-4906-91d8-91117bda4e75", "Prefixed A/C Num.", "Prefixed A/C Number");
			this.AG_Calc_AccountNumberWithPrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 120, true);
			this.AG_Calc_AccountNumberWithPrefixTextBox.Name = "AG_Calc_AccountNumberWithPrefixTextBox";
			this.AG_Calc_AccountNumberWithPrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 15, true);
			this.AG_Calc_AccountNumberWithPrefixTextBox.TabIndex = 12;
			// 
			// AG_CashFlowTypeDropEdit
			// 
			this.AG_CashFlowTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_CashFlowTypeDropEdit, "AG_CashFlowType");
			this.AG_CashFlowTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 94, true);
			this.AG_CashFlowTypeDropEdit.Name = "AG_CashFlowTypeDropEdit";
			this.AG_CashFlowTypeDropEdit.PreBoundMaxLength = 3;
			this.AG_CashFlowTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 15, true);
			this.AG_CashFlowTypeDropEdit.TabIndex = 10;
			// 
			// AG_IsGlobalBoundCheckBox
			// 
			this.AG_IsGlobalBoundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AG_IsGlobalBoundCheckBox, "AG_IsGlobal");
			this.AG_IsGlobalBoundCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|91001415-2bb2-440d-b29d-bb9d4fe5665d", "Is Global");
			this.AG_IsGlobalBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 42, true);
			this.AG_IsGlobalBoundCheckBox.Name = "AG_IsGlobalBoundCheckBox";
			this.AG_IsGlobalBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.AG_IsGlobalBoundCheckBox.TabIndex = 6;
			this.AG_IsGlobalBoundCheckBox.CheckedChanged += new System.EventHandler(this.WarnIfIsGlobalChanged);
			// 
			// SubAccountTypesGroupBox
			// 
			this.SubAccountTypesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8aba526c-aae6-45ab-abb5-21d7e2a14ac1", "Sub Account Types");
			this.SubAccountTypesGroupBox.Controls.Add(this.SubAccountTypesGrid);
			this.SubAccountTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 19, true);
			this.SubAccountTypesGroupBox.Name = "SubAccountTypesGroupBox";
			this.SubAccountTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 170, true);
			this.SubAccountTypesGroupBox.TabIndex = 19;
			this.SubAccountTypesGroupBox.TabStop = false;
			// 
			// SubAccountTypesGrid
			// 
			this.SubAccountTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SubAccountTypesGrid, "SubAccountTypes");
			this.SubAccountTypesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "GLHeader.AG_SubAccountTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cdbc6769-f0a9-4ede-859a-17b7c747c980", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "ASA_SubClassDisplayName";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a28fc201-b175-4304-9ea5-6f7546acd665", "Mandatory");
			zCheckBoxColumnStyleInfo1.ColumnName = "ASA_IsSubClassValidationRuleMandatory";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SubAccountTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SubAccountTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SubAccountTypesGrid.GridId = "67117c22-d736-492e-bb51-febef5490511";
			this.SubAccountTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubAccountTypesGrid.LayoutKey = "SubAccountTypesGrid";
			this.SubAccountTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.SubAccountTypesGrid.Name = "SubAccountTypesGrid";
			this.SubAccountTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 150, true);
			this.SubAccountTypesGrid.TabIndex = 0;
			// 
			// AG_StatisticalUnitsBoundDropEdit
			// 
			this.AG_StatisticalUnitsBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_StatisticalUnitsBoundDropEdit, "AG_StatisticalUnits");
			this.AG_StatisticalUnitsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 68, true);
			this.AG_StatisticalUnitsBoundDropEdit.Name = "AG_StatisticalUnitsBoundDropEdit";
			this.AG_StatisticalUnitsBoundDropEdit.PreBoundMaxLength = 3;
			this.AG_StatisticalUnitsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 15, true);
			this.AG_StatisticalUnitsBoundDropEdit.TabIndex = 9;
			this.DetailsTabPage.PerformLayout();
			this.AG_DescriptionBoundTextEdit.ResumeLayout(true);
			this.AG_DescriptionBoundTextEdit.PerformLayout();
			this.AG_AG_PercentNumBoundGuidFindBox.ResumeLayout(true);
			this.AG_AG_PercentNumBoundGuidFindBox.PerformLayout();
			this.AG_AG_ConsolidationNumBoundGuidFindBox.ResumeLayout(true);
			this.AG_AG_ConsolidationNumBoundGuidFindBox.PerformLayout();
			this.AG_AG_AlternateNumBoundGuidFindBox.ResumeLayout(true);
			this.AG_AG_AlternateNumBoundGuidFindBox.PerformLayout();
			this.AG_DebitCreditBoundDropEdit.ResumeLayout(true);
			this.AG_DebitCreditBoundDropEdit.PerformLayout();
			this.AG_AccountTypeBoundDropEdit.ResumeLayout(true);
			this.AG_AccountTypeBoundDropEdit.PerformLayout();
			this.GLAccountDetailsGroupBox.ResumeLayout(false);
			this.GLAccountDetailsGroupBox.PerformLayout();
			this.TotalReferenceFindBox.ResumeLayout(true);
			this.TotalReferenceFindBox.PerformLayout();
			this.SectionDropEdit.ResumeLayout(true);
			this.SectionDropEdit.PerformLayout();
			this.AG_CashFlowTypeDropEdit.ResumeLayout(true);
			this.AG_CashFlowTypeDropEdit.PerformLayout();
			this.SubAccountTypesGroupBox.ResumeLayout(false);
			this.SubAccountTypesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubAccountTypesGrid)).EndInit();
			this.SubAccountTypesGrid.ResumeLayout(false);
			this.SubAccountTypesGrid.PerformLayout();
			this.AG_StatisticalUnitsBoundDropEdit.ResumeLayout(true);
			this.AG_StatisticalUnitsBoundDropEdit.PerformLayout();
			this.DetailsTabPage.ResumeLayout(true);
		}

		private void CompaniesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AG_DescriptionBoundText = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.AG_CodeBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.CompaniesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CompanyFilterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompaniesTabPage.SuspendLayout();
			this.AG_DescriptionBoundText.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CompaniesGrid)).BeginInit();
			this.CompaniesGrid.SuspendLayout();
			this.CompaniesTabPage.Controls.Add(this.CompanyFilterLabel);
			this.CompaniesTabPage.Controls.Add(this.CompaniesGrid);
			this.CompaniesTabPage.Controls.Add(this.AG_DescriptionBoundText);
			this.CompaniesTabPage.Controls.Add(this.AG_CodeBoundText);
			// 
			// AG_DescriptionBoundText
			// 
			this.AG_DescriptionBoundText.AcceptsReturn = false;
			this.AG_DescriptionBoundText.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AG_DescriptionBoundText, "AG_Description");
			this.AG_DescriptionBoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AG_DescriptionBoundText.GridCurrent = null;
			this.AG_DescriptionBoundText.GridMember = null;
			this.AG_DescriptionBoundText.IsLanguageEditingEnabled = true;
			this.AG_DescriptionBoundText.IsMultiLine = false;
			this.AG_DescriptionBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 47, true);
			this.AG_DescriptionBoundText.Name = "AG_DescriptionBoundText";
			this.AG_DescriptionBoundText.ReadOnly = true;
			this.AG_DescriptionBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.AG_DescriptionBoundText.TabIndex = 8;
			// 
			// AG_CodeBoundText
			// 
			this.BindingSource.SetBindingMember(this.AG_CodeBoundText, "AG_AccountNum");
			this.AG_CodeBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 23, true);
			this.AG_CodeBoundText.Name = "AG_CodeBoundText";
			this.AG_CodeBoundText.ReadOnly = true;
			this.AG_CodeBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 15, true);
			this.AG_CodeBoundText.TabIndex = 7;
			// 
			// CompaniesGrid
			// 
			this.CompaniesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CompaniesGrid, "CompanyFilters");
			this.CompaniesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|fb0ebdb0-b6f7-43ac-99e2-90c56ee3e958", "Company Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ACF_GC_Company";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|e008f878-6138-48eb-a31d-2e4daf2d5d31", "Company Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Company+GC_Name";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.CompaniesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CompaniesGrid.GridId = "99b9bab6-6da2-4c75-bfe8-4e7b0c7b6957";
			this.CompaniesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CompaniesGrid.LayoutKey = "CompaniesGrid";
			this.CompaniesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 87, true);
			this.CompaniesGrid.Name = "CompaniesGrid";
			this.CompaniesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 204, true);
			this.CompaniesGrid.TabIndex = 9;
			// 
			// CompanyFilterLabel
			// 
			this.CompanyFilterLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|7ddcff9f-cb5e-4c3a-96d9-9bacfa086204", "The Is Global checkbox on the Details tab MUST NOT be ticked in order to setup this page.");
			this.CompanyFilterLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CompanyFilterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-3, 0, true);
			this.CompanyFilterLabel.Name = "CompanyFilterLabel";
			this.CompanyFilterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 288, true);
			this.CompanyFilterLabel.TabIndex = 10;
			this.CompanyFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CompanyFilterLabel.UseMnemonic = false;
			this.CompaniesTabPage.PerformLayout();
			this.AG_DescriptionBoundText.ResumeLayout(true);
			this.AG_DescriptionBoundText.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CompaniesGrid)).EndInit();
			this.CompaniesGrid.ResumeLayout(false);
			this.CompaniesGrid.PerformLayout();
			this.CompaniesTabPage.ResumeLayout(true);
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

		private void AlternateChartsDissectionConfigurationTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.AlternateChartsDissectionConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NoDissectionConfigurationSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NotBSHPLDissectionConfigurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NoDissectionConfigurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AlternateChartsDissectionConfigurationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternateChartsDissectionConfigurationGrid)).BeginInit();
			this.AlternateChartsDissectionConfigurationGrid.SuspendLayout();
			this.AlternateChartsDissectionConfigurationTabPage.Controls.Add(this.NotBSHPLDissectionConfigurationLabel);
			this.AlternateChartsDissectionConfigurationTabPage.Controls.Add(this.NoDissectionConfigurationSecurityLabel);
			this.AlternateChartsDissectionConfigurationTabPage.Controls.Add(this.NoDissectionConfigurationLabel);
			this.AlternateChartsDissectionConfigurationTabPage.Controls.Add(this.AlternateChartsDissectionConfigurationGrid);
			// 
			// AlternateChartsDissectionConfigurationGrid
			// 
			this.AlternateChartsDissectionConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AlternateChartsDissectionConfigurationGrid, "AlternateGLAccountDissections");
			this.AlternateChartsDissectionConfigurationGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a7ac636d-f31c-44fc-9744-af5d50034a75", "Chart");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ADC_AAC_AlternateChart";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ModuleID = ModuleIDs.AlternateChartofAccounts;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1d28ec93-3afe-4a01-99a8-896ce494dd0d", "Attribute");
			zDropEditColumnStyleInfo2.ColumnName = "ADC_Attribute";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7012b051-c11e-43fc-b129-4aacb58f2b0c", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "AttributeDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("92962805-7aaf-4973-9966-f40e5ef1eac5", "Separate Numbering");
			zCheckBoxColumnStyleInfo2.ColumnName = "ADC_SeparateNumbering";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AlternateChartsDissectionConfigurationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.AlternateChartsDissectionConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AlternateChartsDissectionConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AlternateChartsDissectionConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.AlternateChartsDissectionConfigurationGrid.GridId = "67117c22-d736-492e-bb51-febef5490511";
			this.AlternateChartsDissectionConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AlternateChartsDissectionConfigurationGrid.LayoutKey = "SubAccountTypesGrid";
			this.AlternateChartsDissectionConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.AlternateChartsDissectionConfigurationGrid.Name = "AlternateChartsDissectionConfigurationGrid";
			this.AlternateChartsDissectionConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 198, true);
			this.AlternateChartsDissectionConfigurationGrid.TabIndex = 1;
			// 
			// NoDissectionConfigurationSecurityLabel
			// 
			this.NoDissectionConfigurationSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|574A0FF3-B476-4ED1-B152-9DDFD90345B1", "You do not have security access to modify the Dissection configuration settings");
			this.NoDissectionConfigurationSecurityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NoDissectionConfigurationSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.NoDissectionConfigurationSecurityLabel.Name = "NoDissectionConfigurationSecurityLabel";
			this.NoDissectionConfigurationSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 359, true);
			this.NoDissectionConfigurationSecurityLabel.TabIndex = 11;
			this.NoDissectionConfigurationSecurityLabel.Text = "You do not have security access to modify the Dissection configuration settings";
			this.NoDissectionConfigurationSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NoDissectionConfigurationSecurityLabel.UseMnemonic = false;
			// 
			// NotBSHPLDissectionConfigurationLabel
			// 
			this.NotBSHPLDissectionConfigurationLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|3DD7B613-C1B2-4978-BC7B-642C252D497E", "Dissection Configuration is only supported for the \'BSH\' and \'P&L\' account types");
			this.NotBSHPLDissectionConfigurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NotBSHPLDissectionConfigurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.NotBSHPLDissectionConfigurationLabel.Name = "NotBSHPLDissectionConfigurationLabel";
			this.NotBSHPLDissectionConfigurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 359, true);
			this.NotBSHPLDissectionConfigurationLabel.TabIndex = 12;
			this.NotBSHPLDissectionConfigurationLabel.Text = "Dissection Configuration is only supported for the \'BSH\' and \'P&L\' account types";
			this.NotBSHPLDissectionConfigurationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NotBSHPLDissectionConfigurationLabel.UseMnemonic = false;
			// 
			// NoDissectionConfigurationLabel
			// 
			this.NoDissectionConfigurationLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLHeaderForm|A9807FCC-A5B3-467D-8DB0-3692DC657714", "Dissection Configurations are not applicable to this GL Account and cannot be selected");
			this.NoDissectionConfigurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NoDissectionConfigurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.NoDissectionConfigurationLabel.Name = "NoDissectionConfigurationLabel";
			this.NoDissectionConfigurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 359, true);
			this.NoDissectionConfigurationLabel.TabIndex = 12;
			this.NoDissectionConfigurationLabel.Text = "Dissection Configurations are not applicable to this GL Account and cannot be selected";
			this.NoDissectionConfigurationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NoDissectionConfigurationLabel.UseMnemonic = false;
			this.AlternateChartsDissectionConfigurationTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternateChartsDissectionConfigurationGrid)).EndInit();
			this.AlternateChartsDissectionConfigurationGrid.ResumeLayout(false);
			this.AlternateChartsDissectionConfigurationGrid.PerformLayout();
			this.AlternateChartsDissectionConfigurationTabPage.ResumeLayout(true);
		}
	}
}
