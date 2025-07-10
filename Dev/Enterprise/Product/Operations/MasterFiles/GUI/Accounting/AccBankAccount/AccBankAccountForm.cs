using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccBankAccountForm : ZForm
	{
		public AccBankAccountForm(AccBankAccount bO) : base(bO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.Audit);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			if (bO.IsCashAccount)
			{
				this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("33C1DB4A-55E4-400A-8DA7-3B8CAD83DE54", "Cash Account");
			}
		}

		protected override void ShowNewForm()
		{
			var bankAccount = BankAccount.Factory.New<AccBankAccount>();
			if (BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CSH)
			{
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			}

			ZControllerFactory.Create(ControllerID).ShowFormForNewEntity(bankAccount);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref ButtonsUserControl, MainStatusBar.Top - ButtonsUserControl.Height - ControlDpiScalingHelper.OnePixel, false);
			AccBankAccount bankAccount = BusinessEntity as AccBankAccount;
			if (bankAccount != null)
			{
				bankAccount.AB_AccountTypeInfo.ValueChanged += AB_AccountTypeInfo_ValueChanged;
				bankAccount.AB_PaymentProviderInfo.ValueChanged += AB_PaymentProviderInfo_ValueChanged;
			}
			AB_AccountTypeInfo_ValueChanged(this, null);
			AB_PaymentProviderInfo_ValueChanged(this, null);
			var isEPaymentEnabled = AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider;
			AB_PaymentProviderDropEdit.Visible = isEPaymentEnabled;
			staffTokenButton.Visible = isEPaymentEnabled;
			ProviderLogoPictureBox.Visible = isEPaymentEnabled;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && ShouldDisplayEPaymentAccountDisclaimerMessage())
			{
				Globals.Message.ShowOrDefault(EPaymentBankAccountDisclaimerControl.DialogDefaultContext, () => new EPaymentBankAccountDisclaimerControl(BankAccount.AB_PaymentProvider));
			}
			if (result == ContinueWithSave.Yes && ShouldDisplayDDRAbbreviationWarningMessage())
			{
				Globals.Message.ShowOrDefault(DDRBankAccountDisclaimerControl.DialogDefaultContext, () => new DDRBankAccountDisclaimerControl());
			}
			return result;
		}

		bool ShouldDisplayEPaymentAccountDisclaimerMessage()
		{
			var isNewEPABankAccount = !BankAccount.IsInDatabase && BankAccount.IsEPaymentAccount;
			var isExistingBankAccountAndOriginalAccountTypeIsNotEPA = BankAccount.IsInDatabase && BankAccount.IsEPaymentAccount && (ZString)BankAccount.AB_AccountTypeInfo.OriginalValue != AccountTypeCodeDescriptionPairList.Codes.EPA;
			return isNewEPABankAccount || isExistingBankAccountAndOriginalAccountTypeIsNotEPA;
		}

		bool ShouldDisplayDDRAbbreviationWarningMessage() => BankAccount.AB_AllowAutoDDR && (BankAccount.AB_AutoDDRFormat == Core.Constants.DDRFileFormat.AB1 || BankAccount.AB_AutoDDRFormat == Core.Constants.DDRFileFormat.AB2);

		void AB_PaymentProviderInfo_ValueChanged(object sender, EventArgs e)
		{
			ProviderLogoPictureBox.Image = EPaymentProviderLogoFinder.FindEPaymentProviderLogo(BankAccount.AB_PaymentProvider);
		}

		void AB_AccountTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (BankAccount != null)
			{
				UpdateEnterCreditCardNumberButtonEnableness(BankAccount);
				UpdateBankAccountNameAndAddressCaption(BankAccount);
				UpdateEPaymentControlsEnableness(BankAccount);
			}
		}

		AccBankAccount BankAccount => BusinessEntity as AccBankAccount;

		void UpdateEPaymentControlsEnableness(AccBankAccount bankAccount)
		{
			var isEPayment = bankAccount.IsEPaymentAccount;
			AB_PaymentProviderDropEdit.Enabled = isEPayment;
			staffTokenButton.Enabled = isEPayment;
			ProviderLogoPictureBox.Enabled = isEPayment;
		}

		void UpdateEnterCreditCardNumberButtonEnableness(AccBankAccount bankAccount)
		{
			if (bankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CCD
					|| bankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.LNK)
			{
				EnterCreditCardNumberButton.Enabled = true;
			}
			else
			{
				EnterCreditCardNumberButton.Enabled = false;
			}
		}

		void UpdateBankAccountNameAndAddressCaption(AccBankAccount bankAccount)
		{
			MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|0086a40d-8948-4bee-8021-15765850c971", "Bank Account");
			BankingDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|1f2b9556-cf2c-4d62-b8fd-90e6a15bb8ca", "Banking Details");
			AB_BankNameBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|7de20e73-a239-421e-bd3d-e301a01cd134", "Bank Name");
			AB_BankAddressBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|0b149e45-fcf7-42d8-b6b3-6c0927aa4ccc", "Bank Address");
			BankAccountNameTextBox.CaptionResourceString = null;

			if (bankAccount.IsEPaymentAccount)
			{
				AB_BankNameBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|a6fb0c0b-08dd-47f1-962a-82002fef967d", "Provider Name");
				AB_BankAddressBoundTextEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|f6e38143-3ad0-4c6e-9f69-b9a8396721d2", "Provider Address");
			}
			else if (bankAccount.IsCashAccount)
			{
				MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|518E5B14-65A0-4EAE-AA17-503C2F2C440A", "Cash Account");
				BankingDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|1556CC1B-DB27-48BB-8794-6E18A9F55B8E", "Cash Account Details");
				BankAccountNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccBankAccountForm|D7CDBB41-AF9A-4CC0-89CC-4B17D7D26F7B", "Cash Account Name");
			}

			MainTabPage.UpdateCaption();
			AB_BankNameBoundTextEdit.UpdateCaption();
			AB_BankAddressBoundTextEdit.UpdateCaption();
			BankingDetailsGroupBox.UpdateCaption();
			BankAccountNameTextBox.UpdateCaption();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnsubscribeHandlers();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void UnsubscribeHandlers()
		{
			AccBankAccount bankAccount = BusinessEntity as AccBankAccount;
			if (bankAccount != null)
			{
				bankAccount.AB_AccountTypeInfo.ValueChanged -= AB_AccountTypeInfo_ValueChanged;
			}
		}

		#endregion

		void EnterCreditCardNumberButton_Click(object sender, EventArgs e)
		{
			CreditCardNumberBusinessObject bo = new CreditCardNumberBusinessObject((AccBankAccount)BusinessEntity);
			CreditCardEntryForm form = new CreditCardEntryForm(bo);
			ZFormModaliser.Show(form, this);
		}

		void zStmNoteTabPage1_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.zStmNoteTabPage1.SuspendLayout();
			this.zStmNoteTabPage1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(true);
		}

		void StaffTokenButton_Click(object sender, EventArgs e)
		{
			if (BankAccount.HasChanges)
			{
				Globals.Message.ShowInformation(Res.GetString("4636120C-5325-4BCB-AFC5-EB69BD1ADDEF", "Please save your bank account first."));
			}
			else
			{
				var newFactory = new BusinessObjectFactory();
				var bankAccountReloaded = newFactory.Load<AccBankAccount>(BankAccount.PK);
				var form = new StaffTokenManagementForm(bankAccountReloaded);
				ZFormModaliser.Show(form, this);
			}
		}

		internal void ProviderLogoPictureBox_Click(object sender, EventArgs e)
		{
			EPaymentUrlLauncher.LaunchEPaymentProviderURL(BankAccount.AB_PaymentProvider);
		}
	}
}
