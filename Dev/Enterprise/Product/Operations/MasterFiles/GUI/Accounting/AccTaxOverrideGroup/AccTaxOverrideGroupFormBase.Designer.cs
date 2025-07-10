
namespace Enterprise.MasterFiles.GUI
{
	partial class AccTaxOverrideGroupFormBase
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TaxOverrideGroupsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TaxOverridesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TaxOverridesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zStmNoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.TaxOverrideGroupsTabPage.SuspendLayout();
			this.TaxOverridesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxOverridesGrid)).BeginInit();
			this.TaxOverridesGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.zStmNoteTabPage.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 439, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccTaxOverrideGroup);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.TaxOverrideGroupsTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage);
			this.MainTabControl.Controls.Add(this.zLogsTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 405, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// TaxOverrideGroupsTabPage
			// 
			this.TaxOverrideGroupsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccTaxOverrideGroupForm|75b8e34e-3d21-4cfc-b2c0-5a20001d2ec5", "Tax Override Group");
			this.TaxOverrideGroupsTabPage.Controls.Add(this.TaxOverridesGroupBox);
			this.TaxOverrideGroupsTabPage.Controls.Add(this.TopPanel);
			this.TaxOverrideGroupsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxOverrideGroupsTabPage.Name = "TaxOverrideGroupsTabPage";
			this.TaxOverrideGroupsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 378, true);
			this.TaxOverrideGroupsTabPage.TabIndex = 0;
			this.TaxOverrideGroupsTabPage.UseVisualStyleBackColor = true;
			// 
			// TaxOverridesGroupBox
			// 
			this.TaxOverridesGroupBox.Controls.Add(this.TaxOverridesGrid);
			this.TaxOverridesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxOverridesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.TaxOverridesGroupBox.Name = "TaxOverridesGroupBox";
			this.TaxOverridesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 313, true);
			this.TaxOverridesGroupBox.TabIndex = 1;
			this.TaxOverridesGroupBox.TabStop = false;
			// 
			// TaxOverridesGrid
			// 
			this.TaxOverridesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxOverridesGrid, "TaxOverrides");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_CostSellAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_TransactionContext)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_TaxRegCntryOrGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_CreateTaxRecord)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_DefaultingRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_A9_DefaultVATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_VATExemptOnExportCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_HomeCountryOrZone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_OrganisationCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_SplitPaymentVATOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeTaxOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).TaxOverrides)).SyncRoot)).AO_DebtorRole)));
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
			zCheckBoxColumnStyleInfo1.ColumnName = "AO_CreateTaxRecord";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.ColumnName = "AO_DefaultingRule";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AO_AT";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AO_A9_DefaultVATClass";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.ColumnName = "AO_CustomsStatus";
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "AO_VATExemptOnExportCharges";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zDropEditColumnStyleInfo13.ColumnName = "AO_HomeCountryOrZone";
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo14.ColumnName = "AO_OrganisationCategory";
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "AO_SplitPaymentVATOrganisation";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
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
			this.TaxOverridesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.TaxOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TaxOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.TaxOverridesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.TaxOverridesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.TaxOverridesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.TaxOverridesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.TaxOverridesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxOverridesGrid.GridId = "b1e8a95a-8935-4af3-8f01-19518dc8be5f";
			this.TaxOverridesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxOverridesGrid.LayoutKey = "TaxOverridesGrid";
			this.TaxOverridesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TaxOverridesGrid.Name = "TaxOverridesGrid";
			this.TaxOverridesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 294, true);
			this.TaxOverridesGrid.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.DescriptionTextBox);
			this.TopPanel.Controls.Add(this.CodeTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 65, true);
			this.TopPanel.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "AX_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).AX_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 36, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "AX_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxOverrideGroup)(null)).AX_Code)));
			this.CodeTextBox.CaptionResourceString = null;
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 10, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			this.CodeTextBox.TabIndex = 0;
			// 
			// zStmNoteTabPage
			// 
			this.zStmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage.Name = "zStmNoteTabPage";
			this.zStmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 378, true);
			this.zStmNoteTabPage.TabIndex = 1;
			// 
			// zLogsTabPage
			// 
			this.zLogsTabPage.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage.Name = "zLogsTabPage";
			this.zLogsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 378, true);
			this.zLogsTabPage.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 405, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 34, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 5, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// AccTaxOverrideGroupFormBase
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccTaxOverrideGroupFormBase|165A51A6-5065-45C8-A365-7D7392D076DD", "Base Tax Override Group Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 463, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccTaxOverrideGroup);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			this.Name = "AccTaxOverrideGroupFormBase";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AccTaxOverrideGroupFormBase";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.TaxOverrideGroupsTabPage.ResumeLayout(false);
			this.TaxOverrideGroupsTabPage.PerformLayout();
			this.TaxOverridesGroupBox.ResumeLayout(false);
			this.TaxOverridesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxOverridesGrid)).EndInit();
			this.TaxOverridesGrid.ResumeLayout(false);
			this.TaxOverridesGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.zStmNoteTabPage.ResumeLayout(false);
			this.zStmNoteTabPage.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage TaxOverrideGroupsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox TaxOverridesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage;
		protected Enterprise.ZArchitecture.ZGrid TaxOverridesGrid;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.ZArchitecture.ZTextBox CodeTextBox;

	}
}
