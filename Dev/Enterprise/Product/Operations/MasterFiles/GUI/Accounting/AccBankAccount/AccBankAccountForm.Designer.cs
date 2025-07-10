using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class AccBankAccountForm
	{
		#region Windows Form Designer generated code
		private Enterprise.ZArchitecture.GUI.ZGroupBox ReceiptsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OpeningBalanceGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AB_IsActiveBoundCheckEdit;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AB_AGBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AB_GBBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox OpeningBalanceCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox ClosingBalanceCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox OpeningLocalBalanceCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox ClosingLocalBalanceCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		protected internal ZDropEdit AB_AccountTypeDropEdit;
		public ZButton EnterCreditCardNumberButton;
		private System.ComponentModel.IContainer components;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 497, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1169, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(297);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(297);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccBankAccount);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 590, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1159, 461, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.TabOrderExtendedToTabPages = true;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|0086a40d-8948-4bee-8021-15765850c971", "Bank Account");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 439, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_AccountType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_Desc)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_AG)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_GB)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).IBAN)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).CreditCardExpiryYear)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).CreditCardExpiryMonth)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_DebitCreditCardName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_RX_NKAccountCurrency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_RN_NKBankAccountCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_ShowDetailsOnDirectDebits)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_ChequeNumDigits)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_SO_ChequeTemplate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_AutoDDRFormat)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_DetailedDepositSlip)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_AllowAutoDDR)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_BankName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_AccountEFTUserID)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_SWIFT)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_BankAbbreviation)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_BankAccountName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_BankAddress)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_AccountNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_BSB)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_IsDefaultReceiptBankAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_FullAccountNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccBankAccount)(null)).AB_PaymentProvider)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 439, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 439, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// AccBankAccountForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|d29edb08-a76a-4cb8-91a4-2b9ad41a10b2", "Bank Account");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1169, 521, true);
			this.Controls.Add(this.ButtonsUserControl);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccBankAccount);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 497, true);
			this.Name = "AccBankAccountForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AB_AccountTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OpeningLocalBalanceCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AB_DescBoundTexEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_IsActiveBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AB_AGBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AB_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_GBBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BankingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EPaymentAccountGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChequeDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CreditCardDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReceiptsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DirectDebitBatchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OpeningBalanceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AB_AccountNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreditCardExpiryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreditCardExpiryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CreditCardNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EnterCreditCardNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AB_RX_NKAccountCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AB_RN_NKBankAccountCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShowDetailsOnDirectDebitsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AB_ChequeNumDigitsTextEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChequeTemplateGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AB_AutoDDRFormatBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AB_DetailedDepositSlipBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AB_AllowAutoDDRBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AB_BankNameBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_AccountEFTUserIDBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_SWIFTBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_BankAbbreviationBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.BankAccountNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_BankAddressBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_AccountNumBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_BSBBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_IsDefaultReceiptBankAccountBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OpeningBalanceCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ClosingLocalBalanceCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ClosingBalanceCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AB_FullAccountNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AB_PaymentProviderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.staffTokenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProviderLogoPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.MainTabPage.SuspendLayout();
			this.AB_AccountTypeDropEdit.SuspendLayout();
			this.OpeningLocalBalanceCalcFindBox.SuspendLayout();
			this.AB_AGBoundGuidFindBox.SuspendLayout();
			this.AB_GBBoundGuidFindBox.SuspendLayout();
			this.BankingDetailsGroupBox.SuspendLayout();
			this.EPaymentAccountGroupBox.SuspendLayout();
			this.ChequeDetailsGroupBox.SuspendLayout();
			this.CreditCardDetailsGroupBox.SuspendLayout();
			this.ReceiptsGroupBox.SuspendLayout();
			this.DirectDebitBatchGroupBox.SuspendLayout();
			this.OpeningBalanceGroupBox.SuspendLayout();
			this.CreditCardExpiryDropEdit.SuspendLayout();
			this.AB_RX_NKAccountCurrencyCodeFindBox.SuspendLayout();
			this.AB_RN_NKBankAccountCountryCodeFindBox.SuspendLayout();
			this.ChequeTemplateGuidFindBox.SuspendLayout();
			this.AB_AutoDDRFormatBoundDropDownEdit.SuspendLayout();
			this.OpeningBalanceCalcFindBox.SuspendLayout();
			this.ClosingLocalBalanceCalcFindBox.SuspendLayout();
			this.ClosingBalanceCalcFindBox.SuspendLayout();
			this.AB_PaymentProviderDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProviderLogoPictureBox)).BeginInit();
			this.MainTabPage.Controls.Add(this.AB_AccountTypeDropEdit);
			this.MainTabPage.Controls.Add(this.AB_DescBoundTexEdit);
			this.MainTabPage.Controls.Add(this.AB_IsActiveBoundCheckEdit);
			this.MainTabPage.Controls.Add(this.AB_AGBoundGuidFindBox);
			this.MainTabPage.Controls.Add(this.AB_CodeBoundTextBox);
			this.MainTabPage.Controls.Add(this.AB_GBBoundGuidFindBox);
			this.MainTabPage.Controls.Add(this.BankingDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.EPaymentAccountGroupBox);
			this.MainTabPage.Controls.Add(this.ChequeDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.CreditCardDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.ReceiptsGroupBox);
			this.MainTabPage.Controls.Add(this.DirectDebitBatchGroupBox);
			this.MainTabPage.Controls.Add(this.OpeningBalanceGroupBox);
			this.MainTabPage.Controls.Add(this.ClosingLocalBalanceCalcFindBox);
			this.MainTabPage.Controls.Add(this.ClosingBalanceCalcFindBox);
			// 
			// AB_AccountTypeDropEdit
			// 
			this.AB_AccountTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AB_AccountTypeDropEdit, "AB_AccountType");
			this.AB_AccountTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|e5f48934-ce31-4881-b3e3-5911858ed32d", "Account Type");
			this.AB_AccountTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 12, true);
			this.AB_AccountTypeDropEdit.Name = "AB_AccountTypeDropEdit";
			this.AB_AccountTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 17, true);
			this.AB_AccountTypeDropEdit.TabIndex = 0;
			// 
			// OpeningLocalBalanceCalcFindBox
			// 
			this.OpeningLocalBalanceCalcFindBox.AllowDrop = true;
			this.OpeningLocalBalanceCalcFindBox.BindToAmount = "AB_OpenBalance";
			this.OpeningLocalBalanceCalcFindBox.BindToUnit = "CompanyCurrency";
			this.OpeningLocalBalanceCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OpeningLocalBalanceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 18, true);
			this.OpeningLocalBalanceCalcFindBox.Name = "OpeningLocalBalanceCalcFindBox";
			this.OpeningLocalBalanceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.OpeningLocalBalanceCalcFindBox.TabIndex = 12;
			// 
			// AB_DescBoundTexEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_DescBoundTexEdit, "AB_Desc");
			this.AB_DescBoundTexEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 79, true);
			this.AB_DescBoundTexEdit.Name = "AB_DescBoundTexEdit";
			this.AB_DescBoundTexEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 17, true);
			this.AB_DescBoundTexEdit.TabIndex = 4;
			// 
			// AB_IsActiveBoundCheckEdit
			// 
			this.AB_IsActiveBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AB_IsActiveBoundCheckEdit, "AB_IsActive");
			this.AB_IsActiveBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 14, true);
			this.AB_IsActiveBoundCheckEdit.Name = "AB_IsActiveBoundCheckEdit";
			this.AB_IsActiveBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 16, true);
			this.AB_IsActiveBoundCheckEdit.TabIndex = 2;
			// 
			// AB_AGBoundGuidFindBox
			// 
			this.AB_AGBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AB_AGBoundGuidFindBox, "AB_AG");
			this.AB_AGBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 101, true);
			this.AB_AGBoundGuidFindBox.Name = "AB_AGBoundGuidFindBox";
			this.AB_AGBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AB_AGBoundGuidFindBox.ParentType = null;
			this.AB_AGBoundGuidFindBox.PopupCaption = null;
			this.AB_AGBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 17, true);
			this.AB_AGBoundGuidFindBox.TabIndex = 5;
			// 
			// AB_CodeBoundTextBox
			// 
			this.AB_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AB_CodeBoundTextBox, "AB_Code");
			this.AB_CodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("238154f8-57e3-4544-94e5-b8f31e05ad59", "Code");
			this.AB_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 34, true);
			this.AB_CodeBoundTextBox.Name = "AB_CodeBoundTextBox";
			this.AB_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
			this.AB_CodeBoundTextBox.TabIndex = 1;
			// 
			// AB_GBBoundGuidFindBox
			// 
			this.AB_GBBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AB_GBBoundGuidFindBox, "AB_GB");
			this.AB_GBBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 56, true);
			this.AB_GBBoundGuidFindBox.Name = "AB_GBBoundGuidFindBox";
			this.AB_GBBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AB_GBBoundGuidFindBox.ParentType = null;
			this.AB_GBBoundGuidFindBox.PopupCaption = null;
			this.AB_GBBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 17, true);
			this.AB_GBBoundGuidFindBox.TabIndex = 3;
			// 
			// BankingDetailsGroupBox
			// 
			this.BankingDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|1f2b9556-cf2c-4d62-b8fd-90e6a15bb8ca", "Banking Details");
			this.BankingDetailsGroupBox.Controls.Add(this.AB_FullAccountNumberBoundTextBox);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_AccountNumberBoundTextBox);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_RX_NKAccountCurrencyCodeFindBox);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_RN_NKBankAccountCountryCodeFindBox);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_BankNameBoundTextEdit);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_SWIFTBoundTextEdit);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_BankAbbreviationBoundTextEdit);
			this.BankingDetailsGroupBox.Controls.Add(this.BankAccountNameTextBox);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_BankAddressBoundTextEdit);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_AccountNumBoundTextBox);
			this.BankingDetailsGroupBox.Controls.Add(this.AB_BSBBoundTextEdit);
			this.BankingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 131, true);
			this.BankingDetailsGroupBox.Name = "BankingDetailsGroupBox";
			this.BankingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 198, true);
			this.BankingDetailsGroupBox.TabIndex = 6;
			this.BankingDetailsGroupBox.TabStop = false;
			// 
			// EPaymentAccountGroupBox
			// 
			this.EPaymentAccountGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|835150D6-EF46-4C93-81C9-9A029D34E4B0", "E-Payment Account");
			this.EPaymentAccountGroupBox.Controls.Add(this.staffTokenButton);
			this.EPaymentAccountGroupBox.Controls.Add(this.ProviderLogoPictureBox);
			this.EPaymentAccountGroupBox.Controls.Add(this.AB_PaymentProviderDropEdit);
			this.EPaymentAccountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(654, 5, true);
			this.EPaymentAccountGroupBox.Name = "EPaymentAccountGroupBox";
			this.EPaymentAccountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 121, true);
			this.EPaymentAccountGroupBox.TabIndex = 8;
			this.EPaymentAccountGroupBox.TabStop = false;
			// 
			// ChequeDetailsGroupBox
			// 
			this.ChequeDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|E133CE43-BB18-4C06-A8B2-F8FF48394BAA", "Cheque Details");
			this.ChequeDetailsGroupBox.Controls.Add(this.AB_ChequeNumDigitsTextEdit);
			this.ChequeDetailsGroupBox.Controls.Add(this.ChequeTemplateGuidFindBox);
			this.ChequeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(654, 128, true);
			this.ChequeDetailsGroupBox.Name = "ChequeDetailsGroupBox";
			this.ChequeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 67, true);
			this.ChequeDetailsGroupBox.TabIndex = 9;
			this.ChequeDetailsGroupBox.TabStop = false;
			// 
			// CreditCardDetailsGroupBox
			// 
			this.CreditCardDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|19CA4901-F528-4F38-91F3-5F79B2F37161", "Credit Card Details");
			this.CreditCardDetailsGroupBox.Controls.Add(this.CreditCardExpiryTextBox);
			this.CreditCardDetailsGroupBox.Controls.Add(this.CreditCardExpiryDropEdit);
			this.CreditCardDetailsGroupBox.Controls.Add(this.CreditCardNameTextBox);
			this.CreditCardDetailsGroupBox.Controls.Add(this.EnterCreditCardNumberButton);
			this.CreditCardDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(654, 199, true);
			this.CreditCardDetailsGroupBox.Name = "CreditCardDetailsGroupBox";
			this.CreditCardDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 118, true);
			this.CreditCardDetailsGroupBox.TabIndex = 10;
			this.CreditCardDetailsGroupBox.TabStop = false;
			// 
			// ReceiptsGroupBox
			// 
			this.ReceiptsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|AB2C40BB-E075-49AA-B5A8-E2BA0F1176CB", "Receipts");
			this.ReceiptsGroupBox.Controls.Add(this.AB_DetailedDepositSlipBoundCheckEdit);
			this.ReceiptsGroupBox.Controls.Add(this.AB_IsDefaultReceiptBankAccountBoundCheckEdit);
			this.ReceiptsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(654, 321, true);
			this.ReceiptsGroupBox.Name = "ReceiptsGroupBox";
			this.ReceiptsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 40, true);
			this.ReceiptsGroupBox.TabIndex = 11;
			this.ReceiptsGroupBox.TabStop = false;
			// 
			// DirectDebitBatchGroupBox
			// 
			this.DirectDebitBatchGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|A6442A57-B67C-4F51-95CF-D074F1A0F048", "Direct Debit Batch");
			this.DirectDebitBatchGroupBox.Controls.Add(this.AB_AutoDDRFormatBoundDropDownEdit);
			this.DirectDebitBatchGroupBox.Controls.Add(this.AB_AllowAutoDDRBoundCheckEdit);
			this.DirectDebitBatchGroupBox.Controls.Add(this.ShowDetailsOnDirectDebitsCheckBox);
			this.DirectDebitBatchGroupBox.Controls.Add(this.AB_AccountEFTUserIDBoundTextEdit);
			this.DirectDebitBatchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(654, 365, true);
			this.DirectDebitBatchGroupBox.Name = "DirectDebitBatchGroupBox";
			this.DirectDebitBatchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 65, true);
			this.DirectDebitBatchGroupBox.TabIndex = 12;
			this.DirectDebitBatchGroupBox.TabStop = false;
			// 
			// OpeningBalanceGroupBox
			// 
			this.OpeningBalanceGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|2CF9B9D4-48E9-49C1-ADA5-2C8B10A6FD7E", "Opening Balance");
			this.OpeningBalanceGroupBox.Controls.Add(this.OpeningBalanceCalcFindBox);
			this.OpeningBalanceGroupBox.Controls.Add(this.OpeningLocalBalanceCalcFindBox);
			this.OpeningBalanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 378, true);
			this.OpeningBalanceGroupBox.Name = "OpeningBalanceGroupBox";
			this.OpeningBalanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 42, true);
			this.OpeningBalanceGroupBox.TabIndex = 7;
			this.OpeningBalanceGroupBox.TabStop = false;
			// 
			// AB_AccountNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.AB_AccountNumberBoundTextBox, "IBAN");
			this.AB_AccountNumberBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("794b1473-5217-4ebc-80fe-8c4bcc7b55b4", "IBAN Number");
			this.AB_AccountNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 106, true);
			this.AB_AccountNumberBoundTextBox.Name = "AB_AccountNumberBoundTextBox";
			this.AB_AccountNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 17, true);
			this.AB_AccountNumberBoundTextBox.TabIndex = 7;
			// 
			// CreditCardExpiryTextBox
			// 
			this.BindingSource.SetBindingMember(this.CreditCardExpiryTextBox, "CreditCardExpiryYear");
			this.CreditCardExpiryTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|1c4c4e44-9e9f-4c5f-95a0-3734d0a0d0c9", "Expiry Year");
			this.CreditCardExpiryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 89, true);
			this.CreditCardExpiryTextBox.Name = "CreditCardExpiryTextBox";
			this.CreditCardExpiryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.CreditCardExpiryTextBox.TabIndex = 21;
			// 
			// CreditCardExpiryDropEdit
			// 
			this.CreditCardExpiryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditCardExpiryDropEdit, "CreditCardExpiryMonth");
			this.CreditCardExpiryDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|fe4bec41-634e-4a09-b1ba-d0d612589dde", "Expiry Month");
			this.CreditCardExpiryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 67, true);
			this.CreditCardExpiryDropEdit.MaxItemsToShowInDropDown = 12;
			this.CreditCardExpiryDropEdit.Name = "CreditCardExpiryDropEdit";
			this.CreditCardExpiryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 17, true);
			this.CreditCardExpiryDropEdit.TabIndex = 20;
			// 
			// CreditCardNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CreditCardNameTextBox, "AB_DebitCreditCardName");
			this.CreditCardNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|5DBE215C-CFDB-4CDB-ADF1-8885F87F9514", "Card Name");
			this.CreditCardNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 45, true);
			this.CreditCardNameTextBox.Name = "CreditCardNameTextBox";
			this.CreditCardNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 17, true);
			this.CreditCardNameTextBox.TabIndex = 19;
			// 
			// EnterCreditCardNumberButton
			// 
			this.EnterCreditCardNumberButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|40c6834f-582c-4c94-83ad-bdef5e0af4d7", "Enter Credit Card Number");
			this.EnterCreditCardNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 17, true);
			this.EnterCreditCardNumberButton.Name = "EnterCreditCardNumberButton";
			this.EnterCreditCardNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 23, true);
			this.EnterCreditCardNumberButton.TabIndex = 18;
			this.EnterCreditCardNumberButton.ToolTipCaption = null;
			this.EnterCreditCardNumberButton.Click += new System.EventHandler(this.EnterCreditCardNumberButton_Click);
			// 
			// AB_RX_NKAccountCurrencyCodeFindBox
			// 
			this.AB_RX_NKAccountCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AB_RX_NKAccountCurrencyCodeFindBox, "AB_RX_NKAccountCurrency");
			this.AB_RX_NKAccountCurrencyCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|2f6d6b7a-677b-4e80-bcd0-e0f28908cdab", "Currency");
			this.AB_RX_NKAccountCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 173, true);
			this.AB_RX_NKAccountCurrencyCodeFindBox.Name = "AB_RX_NKAccountCurrencyCodeFindBox";
			this.AB_RX_NKAccountCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AB_RX_NKAccountCurrencyCodeFindBox.ParentType = null;
			this.AB_RX_NKAccountCurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.AB_RX_NKAccountCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
			this.AB_RX_NKAccountCurrencyCodeFindBox.TabIndex = 10;
			// 
			// AB_RN_NKBankAccountCountryCodeFindBox
			// 
			this.AB_RN_NKBankAccountCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AB_RN_NKBankAccountCountryCodeFindBox, "AB_RN_NKBankAccountCountry");
			this.AB_RN_NKBankAccountCountryCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|74f368b7-9ac6-4259-b4d9-86744e60464b", "Ctry/Rgn. of Domicile", "Country/Region of Domicile");
			this.AB_RN_NKBankAccountCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 151, true);
			this.AB_RN_NKBankAccountCountryCodeFindBox.Name = "AB_RN_NKBankAccountCountryCodeFindBox";
			this.AB_RN_NKBankAccountCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AB_RN_NKBankAccountCountryCodeFindBox.ParentType = null;
			this.AB_RN_NKBankAccountCountryCodeFindBox.PreBoundMaxLength = 2;
			this.AB_RN_NKBankAccountCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 17, true);
			this.AB_RN_NKBankAccountCountryCodeFindBox.TabIndex = 9;
			// 
			// ShowDetailsOnDirectDebitsCheckBox
			// 
			this.ShowDetailsOnDirectDebitsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowDetailsOnDirectDebitsCheckBox, "AB_ShowDetailsOnDirectDebits");
			this.ShowDetailsOnDirectDebitsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|abee9782-5b40-45ce-a783-25700afdd308", "Show Individual DDR Payment");
			this.ShowDetailsOnDirectDebitsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 40, true);
			this.ShowDetailsOnDirectDebitsCheckBox.Name = "ShowDetailsOnDirectDebitsCheckBox";
			this.ShowDetailsOnDirectDebitsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 16, true);
			this.ShowDetailsOnDirectDebitsCheckBox.TabIndex = 26;
			// 
			// AB_ChequeNumDigitsTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_ChequeNumDigitsTextEdit, "AB_ChequeNumDigits");
			this.AB_ChequeNumDigitsTextEdit.DecimalPlaces = 0;
			this.AB_ChequeNumDigitsTextEdit.Decimals = 0;
			this.AB_ChequeNumDigitsTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 40, true);
			this.AB_ChequeNumDigitsTextEdit.Name = "AB_ChequeNumDigitsTextEdit";
			this.AB_ChequeNumDigitsTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 17, true);
			this.AB_ChequeNumDigitsTextEdit.TabIndex = 17;
			this.AB_ChequeNumDigitsTextEdit.Text = "0";
			this.AB_ChequeNumDigitsTextEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AB_ChequeNumDigitsTextEdit.TrackDisposedAccess = true;
			// 
			// ChequeTemplateGuidFindBox
			// 
			this.ChequeTemplateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeTemplateGuidFindBox, "AB_SO_ChequeTemplate");
			this.ChequeTemplateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 18, true);
			this.ChequeTemplateGuidFindBox.Name = "ChequeTemplateGuidFindBox";
			this.ChequeTemplateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChequeTemplateGuidFindBox.ParentType = null;
			this.ChequeTemplateGuidFindBox.PopupCaption = null;
			this.ChequeTemplateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 17, true);
			this.ChequeTemplateGuidFindBox.TabIndex = 16;
			// 
			// AB_AutoDDRFormatBoundDropDownEdit
			// 
			this.AB_AutoDDRFormatBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AB_AutoDDRFormatBoundDropDownEdit, "AB_AutoDDRFormat");
			this.AB_AutoDDRFormatBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 18, true);
			this.AB_AutoDDRFormatBoundDropDownEdit.Name = "AB_AutoDDRFormatBoundDropDownEdit";
			this.AB_AutoDDRFormatBoundDropDownEdit.PreBoundMaxLength = 3;
			this.AB_AutoDDRFormatBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 17, true);
			this.AB_AutoDDRFormatBoundDropDownEdit.TabIndex = 25;
			// 
			// AB_DetailedDepositSlipBoundCheckEdit
			// 
			this.AB_DetailedDepositSlipBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AB_DetailedDepositSlipBoundCheckEdit, "AB_DetailedDepositSlip");
			this.AB_DetailedDepositSlipBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 18, true);
			this.AB_DetailedDepositSlipBoundCheckEdit.Name = "AB_DetailedDepositSlipBoundCheckEdit";
			this.AB_DetailedDepositSlipBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 16, true);
			this.AB_DetailedDepositSlipBoundCheckEdit.TabIndex = 22;
			// 
			// AB_AllowAutoDDRBoundCheckEdit
			// 
			this.AB_AllowAutoDDRBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AB_AllowAutoDDRBoundCheckEdit, "AB_AllowAutoDDR");
			this.AB_AllowAutoDDRBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 18, true);
			this.AB_AllowAutoDDRBoundCheckEdit.Name = "AB_AllowAutoDDRBoundCheckEdit";
			this.AB_AllowAutoDDRBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.AB_AllowAutoDDRBoundCheckEdit.TabIndex = 24;
			// 
			// AB_BankNameBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_BankNameBoundTextEdit, "AB_BankName");
			this.AB_BankNameBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 18, true);
			this.AB_BankNameBoundTextEdit.Name = "AB_BankNameBoundTextEdit";
			this.AB_BankNameBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
			this.AB_BankNameBoundTextEdit.TabIndex = 0;
			// 
			// AB_AccountEFTUserIDBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_AccountEFTUserIDBoundTextEdit, "AB_AccountEFTUserID");
			this.AB_AccountEFTUserIDBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|e9dde0eb-88d0-4f8e-b619-3f1b6690f29f", "User ID No.");
			this.AB_AccountEFTUserIDBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 40, true);
			this.AB_AccountEFTUserIDBoundTextEdit.Name = "AB_AccountEFTUserIDBoundTextEdit";
			this.AB_AccountEFTUserIDBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 17, true);
			this.AB_AccountEFTUserIDBoundTextEdit.TabIndex = 27;
			// 
			// AB_SWIFTBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_SWIFTBoundTextEdit, "AB_SWIFT");
			this.AB_SWIFTBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|aa1d8f1a-37a1-4767-bd2f-7c5c3d5160f9", "SWIFT Code", "The Swift Code if known.");
			this.AB_SWIFTBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 106, true);
			this.AB_SWIFTBoundTextEdit.Name = "AB_SWIFTBoundTextEdit";
			this.AB_SWIFTBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.AB_SWIFTBoundTextEdit.TabIndex = 6;
			// 
			// AB_BankAbbreviationBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_BankAbbreviationBoundTextEdit, "AB_BankAbbreviation");
			this.AB_BankAbbreviationBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b0a51f23-e2b6-478c-83ff-e3eda8dd23cc", "Abbreviation");
			this.AB_BankAbbreviationBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 18, true);
			this.AB_BankAbbreviationBoundTextEdit.Name = "AB_BankAbbreviationBoundTextEdit";
			this.AB_BankAbbreviationBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.AB_BankAbbreviationBoundTextEdit.TabIndex = 1;
			// 
			// BankAccountNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BankAccountNameTextBox, "AB_BankAccountName");
			this.BankAccountNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 62, true);
			this.BankAccountNameTextBox.Name = "BankAccountNameTextBox";
			this.BankAccountNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 17, true);
			this.BankAccountNameTextBox.TabIndex = 3;
			// 
			// AB_BankAddressBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_BankAddressBoundTextEdit, "AB_BankAddress");
			this.AB_BankAddressBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 40, true);
			this.AB_BankAddressBoundTextEdit.Name = "AB_BankAddressBoundTextEdit";
			this.AB_BankAddressBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 17, true);
			this.AB_BankAddressBoundTextEdit.TabIndex = 2;
			// 
			// AB_AccountNumBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.AB_AccountNumBoundTextBox, "AB_AccountNum");
			this.AB_AccountNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 84, true);
			this.AB_AccountNumBoundTextBox.Name = "AB_AccountNumBoundTextBox";
			this.AB_AccountNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 17, true);
			this.AB_AccountNumBoundTextBox.TabIndex = 5;
			// 
			// AB_BSBBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AB_BSBBoundTextEdit, "AB_BSB");
			this.AB_BSBBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|feda4480-1a03-435e-8eea-8231ffe13bd5", "BSB Number", "The Branch Number of the Bank.");
			this.AB_BSBBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 84, true);
			this.AB_BSBBoundTextEdit.Name = "AB_BSBBoundTextEdit";
			this.AB_BSBBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.AB_BSBBoundTextEdit.TabIndex = 4;
			// 
			// AB_IsDefaultReceiptBankAccountBoundCheckEdit
			// 
			this.AB_IsDefaultReceiptBankAccountBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AB_IsDefaultReceiptBankAccountBoundCheckEdit, "AB_IsDefaultReceiptBankAccount");
			this.AB_IsDefaultReceiptBankAccountBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 18, true);
			this.AB_IsDefaultReceiptBankAccountBoundCheckEdit.Name = "AB_IsDefaultReceiptBankAccountBoundCheckEdit";
			this.AB_IsDefaultReceiptBankAccountBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 16, true);
			this.AB_IsDefaultReceiptBankAccountBoundCheckEdit.TabIndex = 23;
			// 
			// OpeningBalanceCalcFindBox
			// 
			this.OpeningBalanceCalcFindBox.AllowDrop = true;
			this.OpeningBalanceCalcFindBox.BindToAmount = "AB_OpenOSBalance";
			this.OpeningBalanceCalcFindBox.BindToUnit = "OSCurrency";
			this.OpeningBalanceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 18, true);
			this.OpeningBalanceCalcFindBox.Name = "OpeningBalanceCalcFindBox";
			this.OpeningBalanceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.OpeningBalanceCalcFindBox.TabIndex = 7;
			// 
			// ClosingLocalBalanceCalcFindBox
			// 
			this.ClosingLocalBalanceCalcFindBox.AllowDrop = true;
			this.ClosingLocalBalanceCalcFindBox.BindToAmount = "AB_ClosingBalance";
			this.ClosingLocalBalanceCalcFindBox.BindToUnit = "CompanyCurrency";
			this.ClosingLocalBalanceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 546, true);
			this.ClosingLocalBalanceCalcFindBox.Name = "ClosingLocalBalanceCalcFindBox";
			this.ClosingLocalBalanceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ClosingLocalBalanceCalcFindBox.TabIndex = 10;
			this.ClosingLocalBalanceCalcFindBox.Visible = false;
			// 
			// ClosingBalanceCalcFindBox
			// 
			this.ClosingBalanceCalcFindBox.AllowDrop = true;
			this.ClosingBalanceCalcFindBox.BindToAmount = "AB_ClosingOSBalance";
			this.ClosingBalanceCalcFindBox.BindToUnit = "OSCurrency";
			this.ClosingBalanceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 546, true);
			this.ClosingBalanceCalcFindBox.Name = "ClosingBalanceCalcFindBox";
			this.ClosingBalanceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ClosingBalanceCalcFindBox.TabIndex = 9;
			this.ClosingBalanceCalcFindBox.Visible = false;
			// 
			// AB_FullAccountNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.AB_FullAccountNumberBoundTextBox, "AB_FullAccountNumber");
			this.AB_FullAccountNumberBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|db879854-27a3-40ac-bdf3-6a776ef724e4", "Unique Acc. No.", "Unique Account Number", "Enter the Unique Account Number that identifies the bank account in your country");
			this.AB_FullAccountNumberBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AB_FullAccountNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 129, true);
			this.AB_FullAccountNumberBoundTextBox.Name = "AB_FullAccountNumberBoundTextBox";
			this.AB_FullAccountNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 17, true);
			this.AB_FullAccountNumberBoundTextBox.TabIndex = 8;
			// 
			// AB_PaymentProviderDropEdit
			// 
			this.AB_PaymentProviderDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AB_PaymentProviderDropEdit, "AB_PaymentProvider");
			this.AB_PaymentProviderDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1368edf5-8b7f-470c-9171-ba4877ae6f36", "Provider");
			this.AB_PaymentProviderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 18, true);
			this.AB_PaymentProviderDropEdit.Name = "AB_PaymentProviderDropEdit";
			this.AB_PaymentProviderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 17, true);
			this.AB_PaymentProviderDropEdit.TabIndex = 13;
			// 
			// staffTokenButton
			// 
			this.staffTokenButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("40222AED-BF52-460B-9AFC-FE0BC4B8D9B6", "Manage Account Users");
			this.staffTokenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 40, true);
			this.staffTokenButton.Name = "staffTokenButton";
			this.staffTokenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 23, true);
			this.staffTokenButton.TabIndex = 15;
			this.staffTokenButton.ToolTipCaption = null;
			this.staffTokenButton.UseVisualStyleBackColor = true;
			this.staffTokenButton.Click += new System.EventHandler(this.StaffTokenButton_Click);
			// 
			// ProviderLogoPictureBox
			// 
			this.ProviderLogoPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 18, true);
			this.ProviderLogoPictureBox.Name = "ProviderLogoPictureBox";
			this.ProviderLogoPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 29, true);
			this.ProviderLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.ProviderLogoPictureBox.TabIndex = 14;
			this.ProviderLogoPictureBox.TabStop = false;
			this.ProviderLogoPictureBox.Click += new System.EventHandler(this.ProviderLogoPictureBox_Click);
			this.MainTabPage.PerformLayout();
			this.AB_AccountTypeDropEdit.ResumeLayout(true);
			this.AB_AccountTypeDropEdit.PerformLayout();
			this.OpeningLocalBalanceCalcFindBox.ResumeLayout(true);
			this.OpeningLocalBalanceCalcFindBox.PerformLayout();
			this.AB_AGBoundGuidFindBox.ResumeLayout(true);
			this.AB_AGBoundGuidFindBox.PerformLayout();
			this.AB_GBBoundGuidFindBox.ResumeLayout(true);
			this.AB_GBBoundGuidFindBox.PerformLayout();
			this.BankingDetailsGroupBox.ResumeLayout(false);
			this.BankingDetailsGroupBox.PerformLayout();
			this.EPaymentAccountGroupBox.ResumeLayout(false);
			this.EPaymentAccountGroupBox.PerformLayout();
			this.ChequeDetailsGroupBox.ResumeLayout(false);
			this.ChequeDetailsGroupBox.PerformLayout();
			this.CreditCardDetailsGroupBox.ResumeLayout(false);
			this.CreditCardDetailsGroupBox.PerformLayout();
			this.ReceiptsGroupBox.ResumeLayout(false);
			this.ReceiptsGroupBox.PerformLayout();
			this.DirectDebitBatchGroupBox.ResumeLayout(false);
			this.DirectDebitBatchGroupBox.PerformLayout();
			this.OpeningBalanceGroupBox.ResumeLayout(false);
			this.OpeningBalanceGroupBox.PerformLayout();
			this.CreditCardExpiryDropEdit.ResumeLayout(true);
			this.CreditCardExpiryDropEdit.PerformLayout();
			this.AB_RX_NKAccountCurrencyCodeFindBox.ResumeLayout(true);
			this.AB_RX_NKAccountCurrencyCodeFindBox.PerformLayout();
			this.AB_RN_NKBankAccountCountryCodeFindBox.ResumeLayout(true);
			this.AB_RN_NKBankAccountCountryCodeFindBox.PerformLayout();
			this.ChequeTemplateGuidFindBox.ResumeLayout(true);
			this.ChequeTemplateGuidFindBox.PerformLayout();
			this.AB_AutoDDRFormatBoundDropDownEdit.ResumeLayout(true);
			this.AB_AutoDDRFormatBoundDropDownEdit.PerformLayout();
			this.OpeningBalanceCalcFindBox.ResumeLayout(true);
			this.OpeningBalanceCalcFindBox.PerformLayout();
			this.ClosingLocalBalanceCalcFindBox.ResumeLayout(true);
			this.ClosingLocalBalanceCalcFindBox.PerformLayout();
			this.ClosingBalanceCalcFindBox.ResumeLayout(true);
			this.ClosingBalanceCalcFindBox.PerformLayout();
			this.AB_PaymentProviderDropEdit.ResumeLayout(true);
			this.AB_PaymentProviderDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProviderLogoPictureBox)).EndInit();
			this.MainTabPage.ResumeLayout(true);

        }

        protected internal ZGroupBox BankingDetailsGroupBox;
		protected internal ZGroupBox EPaymentAccountGroupBox;
		protected internal ZGroupBox ChequeDetailsGroupBox;
		protected internal ZGroupBox CreditCardDetailsGroupBox;
		protected internal ZGroupBox DirectDebitBatchGroupBox;
		protected internal ZTextBox BankAccountNameTextBox;
		internal ZPictureBox ProviderLogoPictureBox;
		internal ZCheckBox AB_DetailedDepositSlipBoundCheckEdit;
		internal ZCheckBox AB_AllowAutoDDRBoundCheckEdit;
		internal ZCheckBox AB_IsDefaultReceiptBankAccountBoundCheckEdit;
		internal ZGuidFindBox ChequeTemplateGuidFindBox;
		internal ZCheckBox ShowDetailsOnDirectDebitsCheckBox;
		internal ZCalcEdit AB_ChequeNumDigitsTextEdit;
		internal ZTextBox CreditCardNameTextBox;
		internal ZDropEdit CreditCardExpiryDropEdit;
		internal ZTextBox CreditCardExpiryTextBox;
		internal ZCodeFindBox AB_RX_NKAccountCurrencyCodeFindBox;
		internal ZCodeFindBox AB_RN_NKBankAccountCountryCodeFindBox;
		internal ZButton staffTokenButton;
		internal ZTextBox AB_AccountNumberBoundTextBox;
		internal ZDropEdit AB_PaymentProviderDropEdit;
		internal ZDropEdit AB_AutoDDRFormatBoundDropDownEdit;
		protected internal ZTextBox AB_CodeBoundTextBox;
		protected internal ZTextBox AB_BSBBoundTextEdit;
		protected internal ZTextBox AB_AccountNumBoundTextBox;
		protected internal ZTextBox AB_BankAddressBoundTextEdit;
		protected internal ZTextBox AB_BankAbbreviationBoundTextEdit;
		protected internal ZTextBox AB_SWIFTBoundTextEdit;
		protected internal ZTextBox AB_AccountEFTUserIDBoundTextEdit;
		protected internal ZTextBox AB_BankNameBoundTextEdit;
		protected internal ZTextBox AB_DescBoundTexEdit;
		protected internal ZTextBox AB_FullAccountNumberBoundTextBox;
	}
}
