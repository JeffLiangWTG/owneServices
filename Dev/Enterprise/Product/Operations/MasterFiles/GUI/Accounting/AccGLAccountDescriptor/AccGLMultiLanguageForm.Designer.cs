using System;
using System.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGLAccountDescriptorForm
	{
		#region Windows Form Designer generated code

		internal Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		Enterprise.ZArchitecture.ZTextBox AccountNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox LocalAccountTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit AJ_LanguageDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit AJ_RN_NKCountryOfComplianceDropEdit;
		Enterprise.ZArchitecture.ZCalcEdit TotalLevelCalcEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit AccountTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit AccountTypeDropEdit1;
		Enterprise.ZArchitecture.ZCalcEdit PrintSequenceCalcEdit;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZDropEdit DebitCreditDropEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox GLAccountGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox CarriedFwdAccountGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ConsolidationAccoountFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox AlternateAccountFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox PercentOfAccountFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox TotalReferenceAccountFindBox;
		internal ZTabPage PivotCollectionTabPage;
		internal ZArchitecture.ZGrid PivotCollectionGrid;
		ZGuidFindBox GLAccountPKGuidFindBox;
		ZDropEdit DRCRDropEdit;
		ZArchitecture.ZTextBox ReportNameTextBox;
		ZArchitecture.ZTextBox LocalGLAccountNoTextBox;
		ZDropEdit LanguageDropEdit;
		IContainer components;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PivotCollectionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 363, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 618;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLAccountDescriptor);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.DetailsTabPage);
			this.MainTabControl.Controls.Add(this.PivotCollectionTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 320, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|f0e335ae-3b28-48b7-b335-49d0cd97c355", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 299, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_AJ_HeaderDependsOnTotal)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_AJ_PercentNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_AJ_AlternativeNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_AJ_ConsolidationNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_AJ_CarriedForwardAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).ParentGLHeaderPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_DebitCredit)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_PrintSequence)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_TotalLevel)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_ReportCategory)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_AccountDescription)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_LocalAccountNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_Language)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_RN_NKCountryOfCompliance)));
			// 
			// PivotCollectionTabPage
			// 
			this.PivotCollectionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.PivotCollectionTabPage.Name = "PivotCollectionTabPage";
			this.PivotCollectionTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PivotCollectionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 299, true);
			this.PivotCollectionTabPage.TabIndex = 4;
			this.PivotCollectionTabPage.CaptionResourceString = Res.GetData("A8BD659C-DCE2-43C5-94D5-30B9379F90FE", "Report Setup");
			this.PivotCollectionTabPage.UseVisualStyleBackColor = true;
			this.PivotCollectionTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.PivotCollectionTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_ReportCategory)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).ParentGLHeaderPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_DebitCredit)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_AccountDescription)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_LocalAccountNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).AJ_Language)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).ReportConfigurationPivotCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLDescriptorPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).ReportConfigurationPivotCollection)).SyncRoot)).ReportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLDescriptorPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).ReportConfigurationPivotCollection)).SyncRoot)).ReportTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLDescriptorPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).ReportConfigurationPivotCollection)).SyncRoot)).ReportCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLDescriptorPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccGLAccountDescriptor)(null)).ReportConfigurationPivotCollection)).SyncRoot)).ReportCategoryDescription)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 299, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 299, true);
			this.zLogsTabPage1.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 336, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// AccGLAccountDescriptorForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|816770b4-ea3e-4541-b94e-cfe33dade375", "General Ledger Multi-Language Mapping");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 387, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLAccountDescriptor);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 344, true);
			this.Name = "AccGLAccountDescriptorForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);
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
		private void DetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.TotalReferenceAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PercentOfAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AlternateAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ConsolidationAccoountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CarriedFwdAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GLAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DebitCreditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PrintSequenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLevelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AccountTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AccountNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AJ_LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AJ_RN_NKCountryOfComplianceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsTabPage.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.TotalReferenceAccountFindBox.SuspendLayout();
			this.PercentOfAccountFindBox.SuspendLayout();
			this.AlternateAccountFindBox.SuspendLayout();
			this.ConsolidationAccoountFindBox.SuspendLayout();
			this.CarriedFwdAccountGuidFindBox.SuspendLayout();
			this.GLAccountGuidFindBox.SuspendLayout();
			this.DebitCreditDropEdit.SuspendLayout();
			this.AccountTypeDropEdit.SuspendLayout();
			this.AJ_LanguageDropEdit.SuspendLayout();
			this.AJ_RN_NKCountryOfComplianceDropEdit.SuspendLayout();
			this.DetailsTabPage.Controls.Add(this.TotalReferenceAccountFindBox);
			this.DetailsTabPage.Controls.Add(this.PercentOfAccountFindBox);
			this.DetailsTabPage.Controls.Add(this.AlternateAccountFindBox);
			this.DetailsTabPage.Controls.Add(this.ConsolidationAccoountFindBox);
			this.DetailsTabPage.Controls.Add(this.CarriedFwdAccountGuidFindBox);
			this.DetailsTabPage.Controls.Add(this.GLAccountGuidFindBox);
			this.DetailsTabPage.Controls.Add(this.DebitCreditDropEdit);
			this.DetailsTabPage.Controls.Add(this.PrintSequenceCalcEdit);
			this.DetailsTabPage.Controls.Add(this.TotalLevelCalcEdit);
			this.DetailsTabPage.Controls.Add(this.AccountTypeDropEdit);
			this.DetailsTabPage.Controls.Add(this.AccountNameTextBox);
			this.DetailsTabPage.Controls.Add(this.LocalAccountTextBox);
			this.DetailsTabPage.Controls.Add(this.AJ_LanguageDropEdit);
			this.DetailsTabPage.Controls.Add(this.AJ_RN_NKCountryOfComplianceDropEdit);
			// 
			// TotalReferenceAccountFindBox
			// 
			this.TotalReferenceAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalReferenceAccountFindBox, "AJ_AJ_HeaderDependsOnTotal");
			this.TotalReferenceAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 184, true);
			this.TotalReferenceAccountFindBox.Name = "TotalReferenceAccountFindBox";
			this.TotalReferenceAccountFindBox.PopupCaption = null;
			this.TotalReferenceAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 21, true);
			this.TotalReferenceAccountFindBox.TabIndex = 17;
			// 
			// PercentOfAccountFindBox
			// 
			this.PercentOfAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PercentOfAccountFindBox, "AJ_AJ_PercentNum");
			this.PercentOfAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 112, true);
			this.PercentOfAccountFindBox.Name = "PercentOfAccountFindBox";
			this.PercentOfAccountFindBox.PopupCaption = null;
			this.PercentOfAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 21, true);
			this.PercentOfAccountFindBox.TabIndex = 11;
			// 
			// AlternateAccountFindBox
			// 
			this.AlternateAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlternateAccountFindBox, "AJ_AJ_AlternativeNum");
			this.AlternateAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 160, true);
			this.AlternateAccountFindBox.Name = "AlternateAccountFindBox";
			this.AlternateAccountFindBox.PopupCaption = null;
			this.AlternateAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 21, true);
			this.AlternateAccountFindBox.TabIndex = 15;
			// 
			// ConsolidationAccoountFindBox
			// 
			this.ConsolidationAccoountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolidationAccoountFindBox, "AJ_AJ_ConsolidationNum");
			this.ConsolidationAccoountFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|ff815328-6efa-4b4d-9f88-04f38cd042ba", "Consolidation Account");
			this.ConsolidationAccoountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 136, true);
			this.ConsolidationAccoountFindBox.Name = "ConsolidationAccoountFindBox";
			this.ConsolidationAccoountFindBox.PopupCaption = null;
			this.ConsolidationAccoountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 21, true);
			this.ConsolidationAccoountFindBox.TabIndex = 13;
			// 
			// CarriedFwdAccountGuidFindBox
			// 
			this.CarriedFwdAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarriedFwdAccountGuidFindBox, "AJ_AJ_CarriedForwardAccount");
			this.CarriedFwdAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 208, true);
			this.CarriedFwdAccountGuidFindBox.Name = "CarriedFwdAccountGuidFindBox";
			this.CarriedFwdAccountGuidFindBox.PopupCaption = null;
			this.CarriedFwdAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 21, true);
			this.CarriedFwdAccountGuidFindBox.TabIndex = 19;
			// 
			// GLAccountGuidFindBox
			// 
			this.GLAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GLAccountGuidFindBox, "ParentGLHeaderPK");
			this.GLAccountGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|54b5252b-b509-4276-b2cd-6a0de1a9507d", "Parent GL Account");
			this.GLAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 256, true);
			this.GLAccountGuidFindBox.Name = "GLAccountGuidFindBox";
			this.GLAccountGuidFindBox.PopupCaption = null;
			this.GLAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 21, true);
			this.GLAccountGuidFindBox.TabIndex = 25;
			// 
			// DebitCreditDropEdit
			// 
			this.DebitCreditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebitCreditDropEdit, "AJ_DebitCredit");
			this.DebitCreditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 40, true);
			this.DebitCreditDropEdit.Name = "DebitCreditDropEdit";
			this.DebitCreditDropEdit.PreBoundMaxLength = 3;
			this.DebitCreditDropEdit.ShowDescriptionBox = false;
			this.DebitCreditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.DebitCreditDropEdit.TabIndex = 5;
			// 
			// PrintSequenceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PrintSequenceCalcEdit, "AJ_PrintSequence");
			this.PrintSequenceCalcEdit.DecimalPlaces = 0;
			this.PrintSequenceCalcEdit.Decimals = 0;
			this.PrintSequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 232, true);
			this.PrintSequenceCalcEdit.Name = "PrintSequenceCalcEdit";
			this.PrintSequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.PrintSequenceCalcEdit.TabIndex = 23;
			this.PrintSequenceCalcEdit.Text = "0";
			this.PrintSequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalLevelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalLevelCalcEdit, "AJ_TotalLevel");
			this.TotalLevelCalcEdit.DecimalPlaces = 0;
			this.TotalLevelCalcEdit.Decimals = 0;
			this.TotalLevelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 232, true);
			this.TotalLevelCalcEdit.Name = "TotalLevelCalcEdit";
			this.TotalLevelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.TotalLevelCalcEdit.TabIndex = 21;
			this.TotalLevelCalcEdit.Text = "0";
			this.TotalLevelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AccountTypeDropEdit
			// 
			this.AccountTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccountTypeDropEdit, "AJ_ReportCategory");
			this.AccountTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|b6a6a7a4-df10-42fa-a948-e5da9e5cedb5", "Account Type", "Account Type", "");
			this.AccountTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 88, true);
			this.AccountTypeDropEdit.Name = "AccountTypeDropEdit";
			this.AccountTypeDropEdit.PreBoundMaxLength = 3;
			this.AccountTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.AccountTypeDropEdit.TabIndex = 9;
			// 
			// AccountNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccountNameTextBox, "AJ_AccountDescription");
			this.AccountNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|8424cbf1-5c7e-4556-8eac-c7a0db8c7b87", "Account Name");
			this.AccountNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 64, true);
			this.AccountNameTextBox.Name = "AccountNameTextBox";
			this.AccountNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 17, true);
			this.AccountNameTextBox.TabIndex = 7;
			// 
			// LocalAccountTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalAccountTextBox, "AJ_LocalAccountNumber");
			this.LocalAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 40, true);
			this.LocalAccountTextBox.Name = "LocalAccountTextBox";
			this.LocalAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.LocalAccountTextBox.TabIndex = 3;
			this.LocalAccountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LocalAccountTextBox_KeyPress);
			// 
			// AJ_LanguageDropEdit
			// 
			this.AJ_LanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AJ_LanguageDropEdit, "AJ_Language");
			this.AJ_LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.AJ_LanguageDropEdit.Name = "AJ_LanguageDropEdit";
			this.AJ_LanguageDropEdit.PreBoundMaxLength = 3;
			this.AJ_LanguageDropEdit.ShowDescriptionBox = false;
			this.AJ_LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AJ_LanguageDropEdit.TabIndex = 1;
			// 
			// AJ_RN_NKCountryOfComplianceDropEdit
			// 
			this.AJ_RN_NKCountryOfComplianceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AJ_RN_NKCountryOfComplianceDropEdit, "AJ_RN_NKCountryOfCompliance");
			this.AJ_RN_NKCountryOfComplianceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 16, true);
			this.AJ_RN_NKCountryOfComplianceDropEdit.Name = "AJ_RN_NKCountryOfComplianceDropEdit";
			this.AJ_RN_NKCountryOfComplianceDropEdit.PreBoundMaxLength = 2;
			this.AJ_RN_NKCountryOfComplianceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.AJ_RN_NKCountryOfComplianceDropEdit.TabIndex = 1;
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.DetailsTabPage.PerformLayout();
			this.TotalReferenceAccountFindBox.ResumeLayout(true);
			this.TotalReferenceAccountFindBox.PerformLayout();
			this.PercentOfAccountFindBox.ResumeLayout(true);
			this.PercentOfAccountFindBox.PerformLayout();
			this.AlternateAccountFindBox.ResumeLayout(true);
			this.AlternateAccountFindBox.PerformLayout();
			this.ConsolidationAccoountFindBox.ResumeLayout(true);
			this.ConsolidationAccoountFindBox.PerformLayout();
			this.CarriedFwdAccountGuidFindBox.ResumeLayout(true);
			this.CarriedFwdAccountGuidFindBox.PerformLayout();
			this.GLAccountGuidFindBox.ResumeLayout(true);
			this.GLAccountGuidFindBox.PerformLayout();
			this.DebitCreditDropEdit.ResumeLayout(true);
			this.DebitCreditDropEdit.PerformLayout();
			this.AccountTypeDropEdit.ResumeLayout(true);
			this.AccountTypeDropEdit.PerformLayout();
			this.AJ_LanguageDropEdit.ResumeLayout(true);
			this.AJ_LanguageDropEdit.PerformLayout();
			this.AJ_RN_NKCountryOfComplianceDropEdit.ResumeLayout(true);
			this.AJ_RN_NKCountryOfComplianceDropEdit.PerformLayout();
			this.DetailsTabPage.ResumeLayout(true);
		}
		private void PivotCollectionTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AccountTypeDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GLAccountPKGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DRCRDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReportNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalGLAccountNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PivotCollectionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PivotCollectionTabPage.SuspendLayout();
			this.PivotCollectionTabPage.SuspendLayout();
			this.AccountTypeDropEdit1.SuspendLayout();
			this.GLAccountPKGuidFindBox.SuspendLayout();
			this.DRCRDropEdit.SuspendLayout();
			this.LanguageDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotCollectionGrid)).BeginInit();
			this.PivotCollectionGrid.SuspendLayout();
			this.PivotCollectionTabPage.Controls.Add(this.PivotCollectionGrid);
			this.PivotCollectionTabPage.Controls.Add(this.GLAccountPKGuidFindBox);
			this.PivotCollectionTabPage.Controls.Add(this.DRCRDropEdit);
			this.PivotCollectionTabPage.Controls.Add(this.ReportNameTextBox);
			this.PivotCollectionTabPage.Controls.Add(this.LocalGLAccountNoTextBox);
			this.PivotCollectionTabPage.Controls.Add(this.LanguageDropEdit);
			this.PivotCollectionTabPage.Controls.Add(this.AccountTypeDropEdit1);
			// 
			// AccountTypeDropEdit1
			// 
			this.AccountTypeDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccountTypeDropEdit1, "AJ_ReportCategory");
			this.AccountTypeDropEdit1.Enabled = false;
			this.AccountTypeDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 88, true);
			this.AccountTypeDropEdit1.Name = "AccountTypeDropEdit1";
			this.AccountTypeDropEdit1.PreBoundMaxLength = 3;
			this.AccountTypeDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.AccountTypeDropEdit1.TabIndex = 9;
			// 
			// GLAccountPKGuidFindBox
			// 
			this.GLAccountPKGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GLAccountPKGuidFindBox, "ParentGLHeaderPK");
			this.GLAccountPKGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|78b515fa-06fb-45a4-8e7a-66eac595c710", "Parent GL Account");
			this.GLAccountPKGuidFindBox.Enabled = false;
			this.GLAccountPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 64, true);
			this.GLAccountPKGuidFindBox.Name = "GLAccountPKGuidFindBox";
			this.GLAccountPKGuidFindBox.PopupCaption = null;
			this.GLAccountPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 21, true);
			this.GLAccountPKGuidFindBox.TabIndex = 31;
			// 
			// DRCRDropEdit
			// 
			this.DRCRDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DRCRDropEdit, "AJ_DebitCredit");
			this.DRCRDropEdit.Enabled = false;
			this.DRCRDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 16, true);
			this.DRCRDropEdit.Name = "DRCRDropEdit";
			this.DRCRDropEdit.PreBoundMaxLength = 3;
			this.DRCRDropEdit.ShowDescriptionBox = false;
			this.DRCRDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.DRCRDropEdit.TabIndex = 28;
			// 
			// ReportNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReportNameTextBox, "AJ_AccountDescription");
			this.ReportNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|ff20a109-8d78-43d1-981e-c2b552906e39", "Account Name");
			this.ReportNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 40, true);
			this.ReportNameTextBox.Name = "ReportNameTextBox";
			this.ReportNameTextBox.ReadOnly = true;
			this.ReportNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 17, true);
			this.ReportNameTextBox.TabIndex = 29;
			// 
			// LocalGLAccountNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalGLAccountNoTextBox, "AJ_LocalAccountNumber");
			this.LocalGLAccountNoTextBox.Enabled = false;
			this.LocalGLAccountNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 40, true);
			this.LocalGLAccountNoTextBox.Name = "LocalGLAccountNoTextBox";
			this.LocalGLAccountNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.LocalGLAccountNoTextBox.TabIndex = 27;
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "AJ_Language");
			this.LanguageDropEdit.Enabled = false;
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.PreBoundMaxLength = 3;
			this.LanguageDropEdit.ShowDescriptionBox = false;
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.LanguageDropEdit.TabIndex = 26;
			// 
			// PivotCollectionGrid
			// 
			this.PivotCollectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PivotCollectionGrid, "ReportConfigurationPivotCollection");
			this.PivotCollectionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|b0a12806-2d40-45c1-aad5-b35cc068fea9", "Report", "Report type");
			zDropEditColumnStyleInfo1.ColumnName = "ReportType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|37b3e0c4-e8c2-4688-9429-6b35a467c43b", "Report Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ReportTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|46b9a9ac-ebdb-4792-ad16-05c4705ca5f5", "Category", "Report Category.");
			zDropEditColumnStyleInfo2.ColumnName = "ReportCategory";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGLAccountDescriptorForm|1de16dd2-2b97-4d1a-b5da-7701dcb20478", "Category Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ReportCategoryDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.PivotCollectionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PivotCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PivotCollectionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PivotCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PivotCollectionGrid.CopySelectedRowsAllowed = true;
			this.PivotCollectionGrid.GridId = "07232e79-2cdd-42cd-8601-dede910c3f1c";
			this.PivotCollectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PivotCollectionGrid.LayoutKey = "zGrid1";
			this.PivotCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 116, true);
			this.PivotCollectionGrid.Name = "PivotCollectionGrid";
			this.PivotCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 163, true);
			this.PivotCollectionGrid.TabIndex = 32;
			this.PivotCollectionTabPage.ResumeLayout(false);
			this.PivotCollectionTabPage.PerformLayout();
			this.PivotCollectionTabPage.PerformLayout();
			this.AccountTypeDropEdit1.ResumeLayout(true);
			this.AccountTypeDropEdit1.PerformLayout();
			this.GLAccountPKGuidFindBox.ResumeLayout(true);
			this.GLAccountPKGuidFindBox.PerformLayout();
			this.DRCRDropEdit.ResumeLayout(true);
			this.DRCRDropEdit.PerformLayout();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotCollectionGrid)).EndInit();
			this.PivotCollectionGrid.ResumeLayout(false);
			this.PivotCollectionGrid.PerformLayout();
			this.PivotCollectionTabPage.ResumeLayout(true);
		}

		#endregion
	}
}
