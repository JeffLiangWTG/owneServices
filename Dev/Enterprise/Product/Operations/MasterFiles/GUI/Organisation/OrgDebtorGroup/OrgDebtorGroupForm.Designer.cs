namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgDebtorGroupForm
	{

		#region Windows Form Designer generated code

		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private System.ComponentModel.IContainer components;
		private Enterprise.ZArchitecture.ZTextBox OJ_ClassBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox OJ_DescBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OverrideRegistryCurrencyToBankCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CurrencyToBankSettingsGroupBox;
		private Enterprise.ZArchitecture.ZGrid CurrencyToBankSettingsZGrid;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox PayToAccountGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl DebtorGroupTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage BankSettingsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage AccountFeeTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage zTabPage1;
		private AccExRateConfigs accExRateConfigs;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.MasterFiles.GUI.AccountFeeControl ctlAccountFee;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.OJ_ClassBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OJ_DescBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.OverrideRegistryCurrencyToBankCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CurrencyToBankSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrencyToBankSettingsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PayToAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DebtorGroupTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.BankSettingsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AccountFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ctlAccountFee = new Enterprise.MasterFiles.GUI.AccountFeeControl();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.accExRateConfigs = new Enterprise.MasterFiles.GUI.AccExRateConfigs();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.CurrencyToBankSettingsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CurrencyToBankSettingsZGrid)).BeginInit();
			this.CurrencyToBankSettingsZGrid.SuspendLayout();
			this.PayToAccountGuidFindBox.SuspendLayout();
			this.DebtorGroupTabControl.SuspendLayout();
			this.BankSettingsTabPage.SuspendLayout();
			this.AccountFeeTabPage.SuspendLayout();
			this.ctlAccountFee.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.accExRateConfigs.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 419, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(193);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(193);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgDebtorGroup);
			// 
			// OJ_ClassBoundTextBox
			// 
			this.OJ_ClassBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OJ_ClassBoundTextBox, "OJ_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OJ_Code)));
			this.OJ_ClassBoundTextBox.CaptionResourceString = null;
			this.OJ_ClassBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.OJ_ClassBoundTextBox.Name = "OJ_ClassBoundTextBox";
			this.OJ_ClassBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.OJ_ClassBoundTextBox.TabIndex = 0;
			// 
			// OJ_DescBoundTextBox
			// 
			this.OJ_DescBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OJ_DescBoundTextBox, "OJ_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OJ_Desc)));
			this.OJ_DescBoundTextBox.CaptionResourceString = null;
			this.OJ_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 36, true);
			this.OJ_DescBoundTextBox.Name = "OJ_DescBoundTextBox";
			this.OJ_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OJ_DescBoundTextBox.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 393, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 3;
			// 
			// OverrideRegistryCurrencyToBankCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideRegistryCurrencyToBankCheckBox, "OverrideRegistryCurrencyToBankSetting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OverrideRegistryCurrencyToBankSetting)));
			this.OverrideRegistryCurrencyToBankCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgDebtorGroupForm|3e8ecc88-441c-4aea-b19e-dd7c6950b7e8", "Override Registry Currency To Bank Setting");
			this.OverrideRegistryCurrencyToBankCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideRegistryCurrencyToBankCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.OverrideRegistryCurrencyToBankCheckBox.Name = "OverrideRegistryCurrencyToBankCheckBox";
			this.OverrideRegistryCurrencyToBankCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 22, true);
			this.OverrideRegistryCurrencyToBankCheckBox.TabIndex = 16;
			// 
			// CurrencyToBankSettingsGroupBox
			// 
			this.CurrencyToBankSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.CurrencyToBankSettingsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgDebtorGroupForm|8a5f71ee-f4b7-4055-8c65-2625f7273ba3", "Currency To Bank Settings");
			this.CurrencyToBankSettingsGroupBox.Controls.Add(this.CurrencyToBankSettingsZGrid);
			this.CurrencyToBankSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 63, true);
			this.CurrencyToBankSettingsGroupBox.Name = "CurrencyToBankSettingsGroupBox";
			this.CurrencyToBankSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 230, true);
			this.CurrencyToBankSettingsGroupBox.TabIndex = 17;
			this.CurrencyToBankSettingsGroupBox.TabStop = false;
			// 
			// CurrencyToBankSettingsZGrid
			// 
			this.CurrencyToBankSettingsZGrid.AllowNavigation = false;
			this.CurrencyToBankSettingsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CurrencyToBankSettingsZGrid, "OrgDebtorGroupBankCurrentOverrideCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OrgDebtorGroupBankCurrentOverrideCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDebtorGroupBankCurrentOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OrgDebtorGroupBankCurrentOverrideCollection)).SyncRoot)).PB_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDebtorGroupBankCurrentOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OrgDebtorGroupBankCurrentOverrideCollection)).SyncRoot)).CurrencyDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDebtorGroupBankCurrentOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OrgDebtorGroupBankCurrentOverrideCollection)).SyncRoot)).PB_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgDebtorGroupBankCurrentOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).OrgDebtorGroupBankCurrentOverrideCollection)).SyncRoot)).Lookups.BankAccountList)));
			this.CurrencyToBankSettingsZGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PB_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgDebtorGroupForm|1f58cb81-a36e-4c9e-aeca-565b8db7fe5e", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CurrencyDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+BankAccountList";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PB_AB";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CurrencyToBankSettingsZGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CurrencyToBankSettingsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CurrencyToBankSettingsZGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CurrencyToBankSettingsZGrid.GridId = "a9301bfc-d271-4add-b9f2-b79d34632bcf";
			this.CurrencyToBankSettingsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CurrencyToBankSettingsZGrid.LayoutKey = "zGrid1";
			this.CurrencyToBankSettingsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CurrencyToBankSettingsZGrid.Name = "CurrencyToBankSettingsZGrid";
			this.CurrencyToBankSettingsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 205, true);
			this.CurrencyToBankSettingsZGrid.TabIndex = 0;
			// 
			// PayToAccountGuidFindBox
			// 
			this.PayToAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PayToAccountGuidFindBox, "DefaultBankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).DefaultBankAccountPK)));
			this.PayToAccountGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgDebtorGroupForm|2b8be77b-25a9-4d32-a682-082eca9587ec", "Bank To Account");
			this.PayToAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 12, true);
			this.PayToAccountGuidFindBox.Name = "PayToAccountGuidFindBox";
			this.PayToAccountGuidFindBox.PopupCaption = null;
			this.PayToAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 20, true);
			this.PayToAccountGuidFindBox.TabIndex = 18;
			// 
			// DebtorGroupTabControl
			// 
			this.DebtorGroupTabControl.Controls.Add(this.BankSettingsTabPage);
			this.DebtorGroupTabControl.Controls.Add(this.AccountFeeTabPage);
			this.DebtorGroupTabControl.Controls.Add(this.zTabPage1);
			this.DebtorGroupTabControl.Controls.Add(this.zLogsTabPage1);
			this.DebtorGroupTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 64, true);
			this.DebtorGroupTabControl.Name = "DebtorGroupTabControl";
			this.DebtorGroupTabControl.SelectedIndex = 0;
			this.DebtorGroupTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 323, true);
			this.DebtorGroupTabControl.TabIndex = 0;
			// 
			// BankSettingsTabPage
			// 
			this.BankSettingsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|46c0b043-302c-41fb-91c2-c9076d5739c9", "Bank Settings");
			this.BankSettingsTabPage.Controls.Add(this.CurrencyToBankSettingsGroupBox);
			this.BankSettingsTabPage.Controls.Add(this.PayToAccountGuidFindBox);
			this.BankSettingsTabPage.Controls.Add(this.OverrideRegistryCurrencyToBankCheckBox);
			this.BankSettingsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BankSettingsTabPage.Name = "BankSettingsTabPage";
			this.BankSettingsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 296, true);
			this.BankSettingsTabPage.TabIndex = 0;
			// 
			// AccountFeeTabPage
			// 
			this.AccountFeeTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbCompanyForm|010e4384-fb10-4cef-b19a-451d225b788b", "Account Fee");
			this.AccountFeeTabPage.Controls.Add(this.ctlAccountFee);
			this.AccountFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccountFeeTabPage.Name = "AccountFeeTabPage";
			this.AccountFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 296, true);
			this.AccountFeeTabPage.TabIndex = 1;
			// 
			// ctlAccountFee
			// 
			this.ctlAccountFee.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ctlAccountFee, "AccountFeeSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccountFeeSettings)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).AccountFeeSettings)));
			this.ctlAccountFee.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("611bae99-8fda-4d74-a700-244c4c8c0aaf", "Account Fee Settings");
			this.ctlAccountFee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
			this.ctlAccountFee.Name = "ctlAccountFee";
			this.ctlAccountFee.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 3, true);
			this.ctlAccountFee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 115, true);
			this.ctlAccountFee.TabIndex = 0;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6df04646-a865-4d09-ab6b-2434321bca65", "Job Billing Exchange Rates");
			this.zTabPage1.Controls.Add(this.accExRateConfigs);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 296, true);
			this.zTabPage1.TabIndex = 2;
			this.zTabPage1.UseVisualStyleBackColor = true;
			// 
			// accExRateConfigs
			// 
			this.accExRateConfigs.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accExRateConfigs, "AccExchangeRateConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection)(((Enterprise.MasterFiles.Business.OrgDebtorGroup)(null)).AccExchangeRateConfigurations)));
			this.accExRateConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accExRateConfigs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.accExRateConfigs.Name = "accExRateConfigs";
			this.accExRateConfigs.ReadOnly = false;
			this.accExRateConfigs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 290, true);
			this.accExRateConfigs.TabIndex = 2;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 296, true);
			this.zLogsTabPage1.TabIndex = 3;
			// 
			// OrgDebtorGroupForm
			// 
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgDebtorGroupForm|a06bca8a-ef16-4768-a87e-92fc2da7a855", "Debtor Group");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 442, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OJ_DescBoundTextBox);
			this.Controls.Add(this.OJ_ClassBoundTextBox);
			this.Controls.Add(this.DebtorGroupTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgDebtorGroup);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 208, true);
			this.Name = "OrgDebtorGroupForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.DebtorGroupTabControl, 0);
			this.Controls.SetChildIndex(this.OJ_ClassBoundTextBox, 0);
			this.Controls.SetChildIndex(this.OJ_DescBoundTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.CurrencyToBankSettingsGroupBox.ResumeLayout(false);
			this.CurrencyToBankSettingsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CurrencyToBankSettingsZGrid)).EndInit();
			this.CurrencyToBankSettingsZGrid.ResumeLayout(false);
			this.CurrencyToBankSettingsZGrid.PerformLayout();
			this.PayToAccountGuidFindBox.ResumeLayout(true);
			this.PayToAccountGuidFindBox.PerformLayout();
			this.DebtorGroupTabControl.ResumeLayout(false);
			this.DebtorGroupTabControl.PerformLayout();
			this.BankSettingsTabPage.ResumeLayout(false);
			this.BankSettingsTabPage.PerformLayout();
			this.AccountFeeTabPage.ResumeLayout(false);
			this.AccountFeeTabPage.PerformLayout();
			this.ctlAccountFee.ResumeLayout(true);
			this.ctlAccountFee.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.accExRateConfigs.ResumeLayout(true);
			this.accExRateConfigs.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
