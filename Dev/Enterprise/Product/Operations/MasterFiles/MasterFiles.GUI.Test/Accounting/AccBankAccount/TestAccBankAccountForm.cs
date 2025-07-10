using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccBankAccountForm))]
	sealed class TestAccBankAccountForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bank = Factory.New<AccBankAccount>();
			return new AccBankAccountForm(bank);
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AccBankAccountForm)GetFormToBashCore())
			{
				AssertNotNull("Bank Account form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		public void TestEPaymentProviderLogo()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				var providerLogoPictureBox = (KPictureBox)form.Controls.Find("ProviderLogoPictureBox", true)[0];
				AssertNotNull(providerLogoPictureBox);
				AssertEquals(PictureBoxSizeMode.Zoom, providerLogoPictureBox.SizeMode);

				AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, bankAccount.AB_AccountType);
				AssertNull("No logos should be displayed when bank account is not EPA type", providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click(null, null);
				AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;
				AssertImageEquals("OFX logo should be displayed", AccountingMasterFilesRegistry.Instance.OFXLogo.Value, providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click(null, null);
				AssertEquals(AccountingMasterFilesRegistry.Instance.OFXWebURL.Value, WebUrlLauncher.LastUrlLaunched);

				bankAccount.AB_PaymentProvider = ZString.Empty;
				AssertNull("No logos should be displayed when provider is not selected", providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click(null, null);
				AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);
			}
		}
		[RequiresSTA]
		public void TestEPaymentBankAccountDisclaimerIsNotShownDuringPreSaveWhenBankAccountIsNotEPATypeAndNotInDatabase()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AG = glHeader.PK;
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_Desc = "Description 1";
			bankAccount.AB_BankAddress = "Sydney";
			bankAccount.AB_BankName = "ABC BANK";
			bankAccount.AB_BankAbbreviation = "ABC";
			bankAccount.AB_AccountNum = "12387439";

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(!bankAccount.IsInDatabase);
				AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, bankAccount.AB_AccountType);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(bankAccount.IsInDatabase);
			}
		}

		public void TestEPaymentBankAccountDisclaimerIsNotShownDuringPreSaveWhenBankAccountIsNotEPATypeAndInDatabase()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AG = glHeader.PK;
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_Desc = "Description 1";
			bankAccount.AB_BankAddress = "Sydney";
			bankAccount.AB_BankName = "ABC BANK";
			bankAccount.AB_BankAbbreviation = "ABC";
			bankAccount.AB_AccountNum = "12387439";
			Factory.Save();

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(bankAccount.IsInDatabase);
				AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, bankAccount.AB_AccountType);

				bankAccount.AB_Desc = "Description 2";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var bankAccountReloaded = new BusinessObjectFactory().Load<AccBankAccount>(bankAccount.PK);
				AssertEquals("Description 2", bankAccountReloaded.AB_Desc);
			}
		}

		public void TestEPaymentBankAccountDisclaimerIsShownDuringPreSaveWhenBankAccountTypeIsChangedToEPATypeAndInDatabase()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AG = glHeader.PK;
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_Desc = "Description 1";
			bankAccount.AB_BankAddress = "Sydney";
			bankAccount.AB_BankName = "ABC BANK";
			bankAccount.AB_BankAbbreviation = "ABC";
			bankAccount.AB_AccountNum = "12387439";
			Factory.Save();

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(bankAccount.IsInDatabase);
				AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, bankAccount.AB_AccountType);
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("EPaymentBankAccountDisclaimerControl", UnitTestUserNotification.Instance.LastMessage.Text);
				var bankAccountReloaded = new BusinessObjectFactory().Load<AccBankAccount>(bankAccount.PK);
				AssertEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, bankAccountReloaded.AB_AccountType);
			}
		}

		[RequiresSTA]
		public void TestEPaymentBankAccountDisclaimerControlIsShownDuringPreSaveWhenBankAccountIsEPATypeAndNotInDatabase()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AG = glHeader.PK;
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_Desc = "Description 1";
			bankAccount.AB_BankAddress = "Sydney";
			bankAccount.AB_BankName = "ABC BANK";
			bankAccount.AB_BankAbbreviation = "ABC";
			bankAccount.AB_AccountNum = "12387439";
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(!bankAccount.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("EPaymentBankAccountDisclaimerControl", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(bankAccount.IsInDatabase);
			}
		}

		public void TestEPaymentBankAccountDisclaimerControlIsNotShownDuringPreSaveWhenBankAccountIsEPATypeAndInDatabase()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AG = glHeader.PK;
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_Desc = "Description 1";
			bankAccount.AB_BankAddress = "Sydney";
			bankAccount.AB_BankName = "ABC BANK";
			bankAccount.AB_BankAbbreviation = "ABC";
			bankAccount.AB_AccountNum = "12387439";
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;
			Factory.Save();

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(bankAccount.IsInDatabase);
				bankAccount.AB_Desc = "Description 2";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var bankAccountReloaded = new BusinessObjectFactory().Load<AccBankAccount>(bankAccount.PK);
				AssertEquals("Description 2", bankAccountReloaded.AB_Desc);
			}
		}

		public void TestDDRFileFormatReadonlyCorrect()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			using (var form = new AccBankAccountForm(bankAccount))
			{
				bankAccount.AB_AllowAutoDDR = false;
				form.Show();
				Assert("DDR format should be readonly beccuase DDR is not allowed", form.AB_AutoDDRFormatBoundDropDownEdit.ReadOnly);
				bankAccount.AB_AllowAutoDDR = true;
				AssertEquals("DDR fromat should be writeable because DDR is allowed", false, form.AB_AutoDDRFormatBoundDropDownEdit.ReadOnly);
			}
		}

		public void TestNewButtonClickBehaviourWithDifferentAccountTypes()
		{
			var cashAccount = Factory.NewWithValidTestData<AccBankAccount>();
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			cashAccount.AB_Desc = "Test Cash Account";

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			bankAccount.AB_Desc = "Test Bank Account";

			Factory.Save();

			var bankAccountController = ZControllerFactory.Create(ControllerIDs.AccBankAccount);
			using (var editForm = (AccBankAccountForm)bankAccountController.ShowEditForm(cashAccount))
			{
				editForm.GetControl<ZPostingButtonsUserControl>("ButtonsUserControl").SaveButton.PerformClick();

				using (var newForm = Application.OpenForms.OfType<AccBankAccountForm>().SingleOrDefault())
				{
					var newEntity = (AccBankAccount)newForm.BusinessEntity;
					AssertNotEquals("Primay keys should be different for New Cash Account and Existing Cash Account", cashAccount.PK, newEntity.PK);
					AssertEquals("Account Type of New Form", AccountTypeCodeDescriptionPairList.Codes.CSH, newEntity.AB_AccountType);
				}
			}

			using (var editForm = (AccBankAccountForm)bankAccountController.ShowEditForm(bankAccount))
			{
				editForm.GetControl<ZPostingButtonsUserControl>("ButtonsUserControl").SaveButton.PerformClick();

				using (var newForm = Application.OpenForms.OfType<AccBankAccountForm>().SingleOrDefault())
				{
					var newEntity = (AccBankAccount)newForm.BusinessEntity;
					AssertNotEquals("Primay keys should be different for New Bank Account and Existing Bank Account", bankAccount.PK, newEntity.PK);
					AssertEquals("Account Type of New Form", AccountTypeCodeDescriptionPairList.Codes.BNK, newEntity.AB_AccountType);
				}
			}
		}

		[RequiresSTA]
		public void TestPaymentProviderVisibility()
		{
			var bankAccount = Factory.New<AccBankAccount>();

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(form.AB_PaymentProviderDropEdit.Visible);
				Assert(form.staffTokenButton.Visible);
				Assert(form.ProviderLogoPictureBox.Visible);
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), false))
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(!form.AB_PaymentProviderDropEdit.Visible);
				Assert(!form.staffTokenButton.Visible);
				Assert(!form.ProviderLogoPictureBox.Visible);
			}
		}

		public void TestUpdateEPaymentControlsEnableness()
		{
			var bankAccount = Factory.New<AccBankAccount>();

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
				Assert(!form.AB_PaymentProviderDropEdit.Enabled);
				Assert(!form.staffTokenButton.Enabled);
				Assert(!form.ProviderLogoPictureBox.Enabled);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
				Assert(!form.AB_PaymentProviderDropEdit.Enabled);
				Assert(!form.staffTokenButton.Enabled);
				Assert(!form.ProviderLogoPictureBox.Enabled);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
				Assert(!form.AB_PaymentProviderDropEdit.Enabled);
				Assert(!form.staffTokenButton.Enabled);
				Assert(!form.ProviderLogoPictureBox.Enabled);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
				Assert(!form.AB_PaymentProviderDropEdit.Enabled);
				Assert(!form.staffTokenButton.Enabled);
				Assert(!form.ProviderLogoPictureBox.Enabled);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				Assert(form.AB_PaymentProviderDropEdit.Enabled);
				Assert(form.staffTokenButton.Enabled);
				Assert(form.ProviderLogoPictureBox.Enabled);
			}
		}

		public void TestUpdateCaptionsDependentOnAccountType()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				AssertEquals("Provider Name", form.AB_BankNameBoundTextEdit.CaptionResourceString.Caption);
				AssertEquals("Provider Address", form.AB_BankAddressBoundTextEdit.CaptionResourceString.Caption);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
				AssertEquals("Bank Name", form.AB_BankNameBoundTextEdit.CaptionResourceString.Caption);
				AssertEquals("Bank Address", form.AB_BankAddressBoundTextEdit.CaptionResourceString.Caption);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
				AssertEquals("Cash Account", form.MainTabPage.CaptionResourceString.Caption);
				AssertEquals("Cash Account Details", form.BankingDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Cash Account Name", form.BankAccountNameTextBox.CaptionResourceString.Caption);
			}
		}

		public void TestDisableBankAccountRelatedFieldsForEPaymentWhenItIsNotSaved() => AssertDisableBankAccountRelatedFieldsForEPayment(isInDatabase: false);

		[RequiresSTA]
		public void TestDisableBankAccountRelatedFieldsForEPaymentWhenItIsSaved() => AssertDisableBankAccountRelatedFieldsForEPayment(isInDatabase: true);

		void AssertDisableBankAccountRelatedFieldsForEPayment(bool isInDatabase)
		{
			var bankAccount = GetFilledAccBankAccount(AccountTypeCodeDescriptionPairList.Codes.BNK, isInDatabase: isInDatabase);
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(!form.AB_BSBBoundTextEdit.ReadOnly);
				Assert(!form.AB_AccountNumBoundTextBox.ReadOnly);
				Assert(!form.AB_FullAccountNumberBoundTextBox.ReadOnly);
				Assert(!form.AB_SWIFTBoundTextEdit.ReadOnly);
				Assert(!form.AB_AccountNumberBoundTextBox.ReadOnly);
				Assert(!form.AB_AccountEFTUserIDBoundTextEdit.ReadOnly);
				Assert(!form.AB_BankAbbreviationBoundTextEdit.ReadOnly);
				Assert(!form.AB_RX_NKAccountCurrencyCodeFindBox.ReadOnly);
				Assert(!form.ChequeTemplateGuidFindBox.ReadOnly);
				Assert(!form.AB_ChequeNumDigitsTextEdit.ReadOnly);
				Assert(!form.AB_RN_NKBankAccountCountryCodeFindBox.ReadOnly);
				Assert(!form.AB_AllowAutoDDRBoundCheckEdit.ReadOnly);
				Assert(!form.AB_DetailedDepositSlipBoundCheckEdit.ReadOnly);
				Assert(!form.ShowDetailsOnDirectDebitsCheckBox.ReadOnly);
				Assert(!form.AB_AutoDDRFormatBoundDropDownEdit.ReadOnly);
				Assert(!form.AB_IsDefaultReceiptBankAccountBoundCheckEdit.ReadOnly);
				Assert(!form.staffTokenButton.Enabled);
				Assert(!bankAccount.AB_AutoDDRFormat.IsEmpty);
				Assert(!bankAccount.AB_BSB.IsEmpty);
				Assert(!bankAccount.AB_AccountNum.IsEmpty);
				Assert(!bankAccount.AB_FullAccountNumber.IsEmpty);
				Assert(!bankAccount.AB_SWIFT.IsEmpty);
				Assert(!bankAccount.AB_AccountNumber.IsEmpty);
				Assert(!bankAccount.AB_AccountEFTUserID.IsEmpty);
				Assert(!bankAccount.AB_BankAbbreviation.IsEmpty);
				AssertEquals("USD", bankAccount.AB_RX_NKAccountCurrency);
				AssertEquals("US", bankAccount.AB_RN_NKBankAccountCountry);
				Assert(bankAccount.AB_DetailedDepositSlip);
				Assert(bankAccount.AB_ShowDetailsOnDirectDebits);
				Assert(bankAccount.AB_IsDefaultReceiptBankAccount);
				AssertEquals((ZByte)9, bankAccount.AB_ChequeNumDigits);
				Assert(!bankAccount.AB_SO_ChequeTemplate.IsEmpty);
				Assert(!bankAccount.AB_DebitCreditCardName.IsEmpty);
				Assert(!bankAccount.AB_DebitCreditCardExpiry.IsEmpty);
				Assert(!bankAccount.CreditCardExpiryMonth.IsEmpty);
				Assert(!bankAccount.CreditCardExpiryYear.IsEmpty);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				Assert(form.AB_BSBBoundTextEdit.ReadOnly);
				Assert(form.AB_AccountNumBoundTextBox.ReadOnly);
				Assert(form.AB_FullAccountNumberBoundTextBox.ReadOnly);
				Assert(form.AB_SWIFTBoundTextEdit.ReadOnly);
				Assert(form.AB_AccountNumberBoundTextBox.ReadOnly);
				Assert(form.AB_AccountEFTUserIDBoundTextEdit.ReadOnly);
				Assert(form.AB_BankAbbreviationBoundTextEdit.ReadOnly);
				Assert(form.AB_RX_NKAccountCurrencyCodeFindBox.ReadOnly);
				Assert(form.ChequeTemplateGuidFindBox.ReadOnly);
				Assert(form.AB_ChequeNumDigitsTextEdit.ReadOnly);
				Assert(form.AB_RN_NKBankAccountCountryCodeFindBox.ReadOnly);
				Assert(form.AB_AllowAutoDDRBoundCheckEdit.ReadOnly);
				Assert(form.AB_DetailedDepositSlipBoundCheckEdit.ReadOnly);
				Assert(form.ShowDetailsOnDirectDebitsCheckBox.ReadOnly);
				Assert(form.AB_AutoDDRFormatBoundDropDownEdit.ReadOnly);
				Assert(form.AB_IsDefaultReceiptBankAccountBoundCheckEdit.ReadOnly);
				Assert(form.staffTokenButton.Enabled);
				Assert(!bankAccount.AB_AllowAutoDDR);
				Assert(bankAccount.AB_AutoDDRFormat.IsEmpty);
				Assert(bankAccount.AB_BSB.IsEmpty);
				Assert(bankAccount.AB_AccountNum.IsEmpty);
				Assert(bankAccount.AB_FullAccountNumber.IsEmpty);
				Assert(bankAccount.AB_SWIFT.IsEmpty);
				Assert(bankAccount.AB_AccountNumber.IsEmpty);
				Assert(bankAccount.AB_AccountEFTUserID.IsEmpty);
				Assert(bankAccount.AB_BankAbbreviation.IsEmpty);
				AssertEquals("AUD", bankAccount.AB_RX_NKAccountCurrency);
				AssertEquals("AU", bankAccount.AB_RN_NKBankAccountCountry);
				Assert(!bankAccount.AB_DetailedDepositSlip);
				Assert(!bankAccount.AB_ShowDetailsOnDirectDebits);
				Assert(!bankAccount.AB_IsDefaultReceiptBankAccount);
				AssertEquals((ZByte)1, bankAccount.AB_ChequeNumDigits);
				AssertEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
				Assert(bankAccount.AB_DebitCreditCardName.IsEmpty);
				Assert(bankAccount.AB_DebitCreditCardExpiry.IsEmpty);
				Assert(bankAccount.CreditCardExpiryMonth.IsEmpty);
				Assert(bankAccount.CreditCardExpiryYear.IsEmpty);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
				bankAccount.AB_AllowAutoDDR = true;
				Assert(!form.AB_BSBBoundTextEdit.ReadOnly);
				Assert(form.AB_AccountNumBoundTextBox.ReadOnly);
				Assert(!form.AB_FullAccountNumberBoundTextBox.ReadOnly);
				Assert(!form.AB_SWIFTBoundTextEdit.ReadOnly);
				Assert(!form.AB_AccountNumberBoundTextBox.ReadOnly);
				Assert(!form.AB_AccountEFTUserIDBoundTextEdit.ReadOnly);
				Assert(!form.AB_BankAbbreviationBoundTextEdit.ReadOnly);
				Assert(!form.AB_RX_NKAccountCurrencyCodeFindBox.ReadOnly);
				Assert(!form.ChequeTemplateGuidFindBox.ReadOnly);
				Assert(!form.AB_ChequeNumDigitsTextEdit.ReadOnly);
				Assert(!form.AB_RN_NKBankAccountCountryCodeFindBox.ReadOnly);
				Assert(!form.AB_AllowAutoDDRBoundCheckEdit.ReadOnly);
				Assert(!form.AB_DetailedDepositSlipBoundCheckEdit.ReadOnly);
				Assert(!form.ShowDetailsOnDirectDebitsCheckBox.ReadOnly);
				Assert(!form.AB_AutoDDRFormatBoundDropDownEdit.ReadOnly);
				Assert(!form.AB_IsDefaultReceiptBankAccountBoundCheckEdit.ReadOnly);
			}
		}

		public void TestDisableBankAccountRelatedFieldsForCashAccountWhenItIsNotSaved() => AssertDisableBankAccountRelatedFieldsForCashAccount(isInDatabase: false);

		public void TestDisableBankAccountRelatedFieldsForCashAccountWhenItIsSaved() => AssertDisableBankAccountRelatedFieldsForCashAccount(isInDatabase: true);

		public void AssertDisableBankAccountRelatedFieldsForCashAccount(bool isInDatabase)
		{
			var bankAccount = GetFilledAccBankAccount(AccountTypeCodeDescriptionPairList.Codes.BNK, isInDatabase);
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();

				#region Banking Details Frame

				Assert(!form.AB_BankNameBoundTextEdit.ReadOnly);
				Assert(!form.AB_BankAbbreviationBoundTextEdit.ReadOnly);
				Assert(!form.AB_BankAddressBoundTextEdit.ReadOnly);
				Assert(!form.AB_BSBBoundTextEdit.ReadOnly);
				Assert(!form.AB_AccountNumBoundTextBox.ReadOnly);
				Assert(!form.AB_SWIFTBoundTextEdit.ReadOnly);
				Assert(!form.AB_AccountNumberBoundTextBox.ReadOnly);
				Assert(!form.AB_FullAccountNumberBoundTextBox.ReadOnly);
				Assert(!form.AB_RN_NKBankAccountCountryCodeFindBox.ReadOnly);
				Assert(!form.AB_RX_NKAccountCurrencyCodeFindBox.ReadOnly);

				#endregion

				#region Cheque Details Frame

				Assert(!form.ChequeTemplateGuidFindBox.ReadOnly);
				Assert(!form.AB_ChequeNumDigitsTextEdit.ReadOnly);

				#endregion

				#region Credit Card Details Frame

				Assert(form.CreditCardExpiryTextBox.ReadOnly);
				Assert(form.CreditCardExpiryDropEdit.ReadOnly);
				Assert(form.CreditCardNameTextBox.ReadOnly);

				#endregion

				#region Receipts Frame

				Assert(!form.AB_DetailedDepositSlipBoundCheckEdit.ReadOnly);
				Assert(!form.AB_IsDefaultReceiptBankAccountBoundCheckEdit.ReadOnly);

				#endregion

				#region Direct Debit Batch Frame

				Assert(!form.AB_AutoDDRFormatBoundDropDownEdit.ReadOnly);
				Assert(!form.AB_AllowAutoDDRBoundCheckEdit.ReadOnly);
				Assert(!form.ShowDetailsOnDirectDebitsCheckBox.ReadOnly);
				Assert(!form.AB_AccountEFTUserIDBoundTextEdit.ReadOnly);

				#endregion

				Assert(bankAccount.AB_AllowAutoDDR);
				Assert(!bankAccount.AB_BankName.IsEmpty);
				Assert(!bankAccount.AB_BankAbbreviation.IsEmpty);
				Assert(!bankAccount.AB_BankAddress.IsEmpty);
				Assert(!bankAccount.AB_BSB.IsEmpty);
				Assert(!bankAccount.AB_AccountNum.IsEmpty);
				Assert(!bankAccount.AB_AccountNumber.IsEmpty);
				Assert(!bankAccount.AB_FullAccountNumber.IsEmpty);
				Assert(!bankAccount.AB_SWIFT.IsEmpty);
				Assert(!bankAccount.AB_AccountEFTUserID.IsEmpty);
				Assert(bankAccount.AB_DetailedDepositSlip);
				Assert(bankAccount.AB_ShowDetailsOnDirectDebits);
				AssertNotEquals((ZByte)1, bankAccount.AB_ChequeNumDigits);
				AssertNotEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
				Assert(!bankAccount.AB_DebitCreditCardName.IsEmpty);
				Assert(!bankAccount.AB_DebitCreditCardExpiry.IsEmpty);
				Assert(!bankAccount.CreditCardExpiryMonth.IsEmpty);
				Assert(!bankAccount.CreditCardExpiryYear.IsEmpty);
				Assert(!bankAccount.AB_AutoDDRFormat.IsEmpty);
				Assert(!bankAccount.AB_PaymentProvider.IsEmpty);

				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

				#region Banking Details Frame

				Assert(form.AB_BankNameBoundTextEdit.ReadOnly);
				Assert(form.AB_BankAbbreviationBoundTextEdit.ReadOnly);
				Assert(form.AB_BankAddressBoundTextEdit.ReadOnly);
				Assert(form.AB_BSBBoundTextEdit.ReadOnly);
				Assert(form.AB_AccountNumBoundTextBox.ReadOnly);
				Assert(form.AB_SWIFTBoundTextEdit.ReadOnly);
				Assert(form.AB_AccountNumberBoundTextBox.ReadOnly);
				Assert(form.AB_FullAccountNumberBoundTextBox.ReadOnly);
				Assert(!form.AB_RN_NKBankAccountCountryCodeFindBox.ReadOnly);
				Assert(!form.AB_RX_NKAccountCurrencyCodeFindBox.ReadOnly);

				#endregion

				#region Cheque Details Frame

				Assert(form.ChequeTemplateGuidFindBox.ReadOnly);
				Assert(form.AB_ChequeNumDigitsTextEdit.ReadOnly);

				#endregion

				#region Credit Card Details Frame

				Assert(form.CreditCardExpiryTextBox.ReadOnly);
				Assert(form.CreditCardExpiryDropEdit.ReadOnly);
				Assert(form.CreditCardNameTextBox.ReadOnly);

				#endregion

				#region Receipts Frame

				Assert(!form.AB_DetailedDepositSlipBoundCheckEdit.ReadOnly);
				Assert(!form.AB_IsDefaultReceiptBankAccountBoundCheckEdit.ReadOnly);

				#endregion

				#region Direct Debit Batch Frame

				Assert(form.AB_AutoDDRFormatBoundDropDownEdit.ReadOnly);
				Assert(form.AB_AllowAutoDDRBoundCheckEdit.ReadOnly);
				Assert(form.ShowDetailsOnDirectDebitsCheckBox.ReadOnly);
				Assert(form.AB_AccountEFTUserIDBoundTextEdit.ReadOnly);

				#endregion

				Assert(!bankAccount.AB_AllowAutoDDR);
				Assert(bankAccount.AB_BankName.IsEmpty);
				Assert(bankAccount.AB_BankAbbreviation.IsEmpty);
				Assert(bankAccount.AB_BankAddress.IsEmpty);
				Assert(bankAccount.AB_BSB.IsEmpty);
				Assert(bankAccount.AB_AccountNum.IsEmpty);
				Assert(bankAccount.AB_AccountNumber.IsEmpty);
				Assert(bankAccount.AB_FullAccountNumber.IsEmpty);
				Assert(bankAccount.AB_SWIFT.IsEmpty);
				Assert(bankAccount.AB_AccountEFTUserID.IsEmpty);
				Assert(!bankAccount.AB_DetailedDepositSlip);
				Assert(!bankAccount.AB_ShowDetailsOnDirectDebits);
				AssertEquals((ZByte)1, bankAccount.AB_ChequeNumDigits);
				AssertEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
				Assert(bankAccount.AB_DebitCreditCardName.IsEmpty);
				Assert(bankAccount.AB_DebitCreditCardExpiry.IsEmpty);
				Assert(bankAccount.CreditCardExpiryMonth.IsEmpty);
				Assert(bankAccount.CreditCardExpiryYear.IsEmpty);
				Assert(bankAccount.AB_AutoDDRFormat.IsEmpty);
				Assert(bankAccount.AB_PaymentProvider.IsEmpty);
			}
		}

		public void TestEnterCreditCardNumberButtonBehaviour()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			bankAccount.AB_AllowAutoDDR = true;
			bankAccount.AB_AutoDDRFormat = "ASB";
			bankAccount.AB_BankName = "ROBBERY BANK";
			bankAccount.AB_BankAddress = "ROBBERY TOWN";
			bankAccount.AB_BSB = "111-222";
			bankAccount.AB_AccountNum = "1234567";
			bankAccount.AB_FullAccountNumber = "11112222";
			bankAccount.AB_SWIFT = "223344";
			bankAccount.AB_AccountNumber = "12345";
			bankAccount.AB_AccountEFTUserID = "12345";
			bankAccount.AB_BankAbbreviation = "AAA";
			bankAccount.AB_RX_NKAccountCurrency = "USD";
			bankAccount.AB_RN_NKBankAccountCountry = "US";
			bankAccount.AB_DetailedDepositSlip = true;
			bankAccount.AB_ShowDetailsOnDirectDebits = true;
			bankAccount.AB_IsDefaultReceiptBankAccount = true;
			bankAccount.AB_ChequeNumDigits = 9;
			bankAccount.AB_SO_ChequeTemplate = Factory.LoadTop1<StmTemplate>(new ZQuery()).PK;

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(form.EnterCreditCardNumberButton.Enabled);
				form.EnterCreditCardNumberButton.PerformClick();

				var creditCardForm = Application.OpenForms.OfType<CreditCardEntryForm>().SingleOrDefault();
				creditCardForm.CreditCardNumberTextBox.Text = "1234567812341234";
				creditCardForm.OKButton.PerformClick();
				AssertEquals(bankAccount.AB_AccountNum, "**** **** ***4 1234");
			}

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				form.EnterCreditCardNumberButton.PerformClick();
				Assert(!form.EnterCreditCardNumberButton.Enabled);
			}

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				form.EnterCreditCardNumberButton.PerformClick();
				Assert(!form.EnterCreditCardNumberButton.Enabled);
			}

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;

			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				form.EnterCreditCardNumberButton.PerformClick();
				Assert(!form.EnterCreditCardNumberButton.Enabled);
			}

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();
				Assert(form.EnterCreditCardNumberButton.Enabled);
				form.EnterCreditCardNumberButton.PerformClick();
				var creditCardForm = Application.OpenForms.OfType<CreditCardEntryForm>().SingleOrDefault();
				creditCardForm.CreditCardNumberTextBox.Text = "1234567812341234";
				creditCardForm.OKButton.PerformClick();
				AssertEquals(bankAccount.AB_AccountNum, "**** **** ***4 1234");
			}
		}

		public void TestBankAccountyValuesAreNotChangedOnEditing()
		{
			var bankAccount = GetFilledAccBankAccount(AccountTypeCodeDescriptionPairList.Codes.CSH, true);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TR"))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency("EUR"))
			using (var form = new AccBankAccountForm(bankAccount))
			{
				form.Show();

				AssertEquals("ABCBANK", bankAccount.AB_Code);
				AssertEquals(AccountTypeCodeDescriptionPairList.Codes.CSH, bankAccount.AB_AccountType);
				Assert(bankAccount.AB_AllowAutoDDR);
				AssertEquals("ASB", bankAccount.AB_AutoDDRFormat);
				AssertEquals("ROBBERY BANK", bankAccount.AB_BankName);
				AssertEquals("ROBBERY TOWN", bankAccount.AB_BankAddress);
				AssertEquals("111-222", bankAccount.AB_BSB);
				AssertEquals("1234567", bankAccount.AB_AccountNum);
				AssertEquals("11112222", bankAccount.AB_FullAccountNumber);
				AssertEquals("223344", bankAccount.AB_SWIFT);
				AssertEquals("12345", bankAccount.AB_AccountNumber);
				AssertEquals("12345", bankAccount.AB_AccountEFTUserID);
				AssertEquals("AAA", bankAccount.AB_BankAbbreviation);
				AssertEquals("US", bankAccount.AB_RN_NKBankAccountCountry);
				AssertEquals("USD", bankAccount.AB_RX_NKAccountCurrency);
				Assert(bankAccount.AB_DetailedDepositSlip);
				Assert(bankAccount.AB_ShowDetailsOnDirectDebits);
				Assert(bankAccount.AB_IsDefaultReceiptBankAccount);
				AssertEquals((ZByte)9, bankAccount.AB_ChequeNumDigits);
				AssertEquals(Factory.LoadTop1<StmTemplate>(new ZQuery()).PK, bankAccount.AB_SO_ChequeTemplate);
				AssertEquals("03", bankAccount.CreditCardExpiryMonth);
				AssertEquals("23", bankAccount.CreditCardExpiryYear);
				AssertEquals("john smith", bankAccount.AB_DebitCreditCardName);
				AssertEquals("0323", bankAccount.AB_DebitCreditCardExpiry);
				AssertEquals("ABC", bankAccount.AB_PaymentProvider);
			}
		}

		AccBankAccount GetFilledAccBankAccount(string accountType = AccountTypeCodeDescriptionPairList.Codes.CCD, bool isInDatabase = false)
		{
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_AccountType = accountType;
			bankAccount.AB_AllowAutoDDR = true;
			bankAccount.AB_AutoDDRFormat = "ASB";
			bankAccount.AB_BankName = "ROBBERY BANK";
			bankAccount.AB_BankAddress = "ROBBERY TOWN";
			bankAccount.AB_BSB = "111-222";
			bankAccount.AB_AccountNum = "1234567";
			bankAccount.AB_FullAccountNumber = "11112222";
			bankAccount.AB_SWIFT = "223344";
			bankAccount.AB_AccountNumber = "12345";
			bankAccount.AB_AccountEFTUserID = "12345";
			bankAccount.AB_BankAbbreviation = "AAA";
			bankAccount.AB_RX_NKAccountCurrency = "USD";
			bankAccount.AB_RN_NKBankAccountCountry = "US";
			bankAccount.AB_DetailedDepositSlip = true;
			bankAccount.AB_ShowDetailsOnDirectDebits = true;
			bankAccount.AB_IsDefaultReceiptBankAccount = true;
			bankAccount.AB_ChequeNumDigits = 9;
			bankAccount.AB_SO_ChequeTemplate = Factory.LoadTop1<StmTemplate>(new ZQuery()).PK;
			bankAccount.CreditCardExpiryMonth = "03";
			bankAccount.CreditCardExpiryYear = "23";
			bankAccount.AB_DebitCreditCardName = "john smith";
			bankAccount.AB_DebitCreditCardExpiry = "0323";
			bankAccount.AB_PaymentProvider = "ABC";
			if (isInDatabase)
			{
				((INeedRow)bankAccount).Row.AcceptChanges();
			}
			return bankAccount;
		}
	}
}
