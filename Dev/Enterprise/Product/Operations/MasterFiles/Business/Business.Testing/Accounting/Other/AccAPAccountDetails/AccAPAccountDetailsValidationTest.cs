using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAPAccountDetailsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckA1_EPaymentReferenceType()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;

				AccountDetails.A1_EPaymentReferenceType = "123";
				AssertHasError(AccountDetails.A1_EPaymentReferenceTypeInfo, "Enter a valid Payment Reference Type.");

				AccountDetails.A1_EPaymentReferenceType = EPaymentReferenceTypes.FreeText;
				AssertNoErrors(AccountDetails.A1_EPaymentReferenceTypeInfo);

				AccountDetails.A1_EPaymentReferenceType = ZString.Empty;
				AssertHasError(AccountDetails.A1_EPaymentReferenceTypeInfo, "Payment Reference Type is Mandatory for E-Payments. Please select a Payment Reference Type.");
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				AccountDetails.A1_EPaymentReferenceType = "123";
				AssertHasRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
			}
		}

		public void TestCheckA1_EPaymentReference()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AccountDetails.A1_EPaymentReferenceType = EPaymentReferenceTypes.FreeText;

				AccountDetails.A1_EPaymentReference = ZString.Empty;
				AssertHasError(AccountDetails.A1_EPaymentReferenceInfo, "Please enter a Payment Reference.");

				AccountDetails.A1_EPaymentReference = "123";
				AssertNoErrors(AccountDetails.A1_EPaymentReferenceInfo);
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				AccountDetails.A1_EPaymentReferenceType = "123";
				AssertHasRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
			}
		}

		public void TestCheckA1_EPaymentReasonCode()
		{
			AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			AccountDetails.A1_EPaymentReasonCode = "ABC";
			AssertHasError(AccountDetails.A1_EPaymentReasonCodeInfo, "Enter a valid selection.");
			AccountDetails.A1_EPaymentReasonCode = ZString.Empty;
			AssertNoError(AccountDetails.A1_EPaymentReasonCodeInfo, "Enter a valid selection.");
			AccountDetails.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			AssertNoError(AccountDetails.A1_EPaymentReasonCodeInfo, "Enter a valid selection.");

			AccountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
			Assert(AccountDetails.A1_EPaymentReasonCode.IsEmpty);
			AssertNoError(AccountDetails.A1_EPaymentReasonCodeInfo, "Enter a valid selection.");

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AccountDetails.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
				AssertHasRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
				AccountDetails.A1_EPaymentReasonCode = ZString.Empty;
				AssertNoRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");

				AccountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
				Assert(AccountDetails.A1_EPaymentReasonCode.IsEmpty);
				AssertNoRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
			}
		}

		public void TestCheckA1_EPaymentBeneficiaryId()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				AssertHasRowWarningContaining(AccountDetails, "To link this account with the OFX Beneficiary, right-click and select Config for E-Payment");
				AssertNoRowErrorContaining(AccountDetails, "Provider Reference can be recorded only against bank accounts with EPO payment method");

				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				AssertNoRowWarningContaining(AccountDetails, "To link this account with the OFX Beneficiary, right-click and select Config for E-Payment");
				AssertNoRowErrorContaining(AccountDetails, "Provider Reference can be recorded only against bank accounts with EPO payment method");

				var anotherAccountDetails = Org.CompanyData.AccountDetailsCollection.AddNew();
				anotherAccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				anotherAccountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				AssertHasRowErrorContaining(anotherAccountDetails, "The beneficiary has already been linked to another account");
				anotherAccountDetails.A1_EPaymentBeneficiaryId = ZGuid.NewZGuid();
				AssertNoRowErrorContaining(anotherAccountDetails, "The beneficiary has already been linked to another account");

				AccountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				AssertNoRowWarningContaining(AccountDetails, "To link this account with the OFX Beneficiary, right-click and select Config for E-Payment");
				AssertNoRowErrorContaining(AccountDetails, "Provider Reference can be recorded only against bank accounts with EPO payment method");

				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				AssertNoRowWarningContaining(AccountDetails, "To link this account with the OFX Beneficiary, right-click and select Config for E-Payment");
				AssertHasRowError(AccountDetails, "Provider Reference can be recorded only against bank accounts with EPO payment method");
			}

			AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AccountDetails.A1_EPaymentReasonCode = ZString.Empty;
				AssertNoRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");

				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				AssertHasRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");

				AccountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
				AssertHasRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");

				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				AssertNoRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
			}
		}

		public void TestCheckA1_BankSwift()
		{
			AccountDetails.A1_BankSwift = "123456ABSSS";
			AssertHasErrors("Should be errors", AccountDetails.A1_BankSwiftInfo);

			AccountDetails.A1_BankSwift = "ABCDE1234LLL";
			AssertHasErrors("Should be errors", AccountDetails.A1_BankSwiftInfo);

			AccountDetails.A1_BankSwift = "ABCDEF12X8888";
			AssertHasErrors("Should be errors", AccountDetails.A1_BankSwiftInfo);

			AccountDetails.A1_BankSwift = "ABCDEF1O888";
			AssertHasErrors("Should be errors", AccountDetails.A1_BankSwiftInfo);

			AccountDetails.A1_BankSwift = "ABCDEF1X888";
			AssertHasErrors("Should be errors", AccountDetails.A1_BankSwiftInfo);

			AccountDetails.A1_BankSwift = "ABCDEF2X888";
			AssertNoErrors("Should be no errors", AccountDetails.A1_BankSwiftInfo);
		}

		public void TestIBANCheck_RaisesOnlyWarning_WhenCountryDifferentThanBankAccountCountry()
		{
			const string warning = "[Bank Account] IBAN Number: The first two characters of the IBAN Number do not match the Bank's Country/Region Code.";
			AccountDetails.A1_RN_NKCountryCode = "GB";

			AccountDetails.A1_IBANNumber = "GB82WEST12345698765432";
			AssertNoNotifications(AccountDetails.A1_IBANNumberInfo);

			AccountDetails.A1_IBANNumber = "DE75512108001245126199";
			AssertNoErrors(AccountDetails.A1_IBANNumberInfo);
			AssertHasWarning(AccountDetails.A1_IBANNumberInfo, warning);
		}

		public void TestIBANCheck_RaisesError_WhenNumericOrAlphanumericPartIsIncorrect()
		{
			const string error = "The third and fourth characters of the IBAN Number must be numeric. From fifth character onward, it should only contain alphanumeric values only.";
			AccountDetails.A1_RN_NKCountryCode = "GB";

			AccountDetails.A1_IBANNumber = "GBabXYZ1234";
			AssertHasError(AccountDetails.A1_IBANNumberInfo, error);

			AccountDetails.A1_IBANNumber = "GB12";
			AssertHasError(AccountDetails.A1_IBANNumberInfo, error);

			AccountDetails.A1_IBANNumber = "GB12af,dd";
			AssertHasError(AccountDetails.A1_IBANNumberInfo, error);
		}

		public void TestIBANCheck_RaisesError_WhenLengthIsDifferentThanIBANCountryRelatedLength()
		{
			const string error = "The length of IBAN Number is incorrect, it should be '22' in Country/Region 'GB'.";
			AccountDetails.A1_RN_NKCountryCode = "TR"; // IBAN length is checked against the Country/Region denoted by the first two characters of the IBAN, not by the bank account country.

			AccountDetails.A1_IBANNumber = "GB82WEST12345698765432";
			AssertNoErrors(AccountDetails.A1_IBANNumberInfo);

			AccountDetails.A1_IBANNumber = "GB82WEST123456";
			AssertHasError(AccountDetails.A1_IBANNumberInfo, error);
		}

		public void TestIBANCheck_RaisesError_WhenMod97CheckFailed()
		{
			const string error = "The remainder calculated using the MOD 97 algorithm should be equal to 1. Please check the IBAN to confirm it is entered correctly.";
			AccountDetails.A1_RN_NKCountryCode = "GB";

			AccountDetails.A1_IBANNumber = "GB82WEST12345698765432";
			AssertNoNotifications(AccountDetails.A1_IBANNumberInfo);

			AccountDetails.A1_IBANNumber = "GB82WEST12345698765431";
			AssertHasError(AccountDetails.A1_IBANNumberInfo, error);
		}

		public void TestCheckA1_PaymentMethod()
		{
			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertNoErrors("Should be no errors", AccountDetails.A1_PaymentMethodInfo);

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertHasErrors("Should be errors", AccountDetails.A1_PaymentMethodInfo);

			AccountDetails.A1_PaymentMethod = AccAPAccountDetailsLookups.DefaultPayment;
			AssertNoErrors("Should be no errors", AccountDetails.A1_PaymentMethodInfo);

			AccountDetails.A1_PaymentMethod = "";
			AssertHasErrors("Should errors", AccountDetails.A1_PaymentMethodInfo);

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertNoErrors("Should be NO errors", AccountDetails.A1_PaymentMethodInfo);

			Org.CompanyData.OB_IsCreditor = false;
			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertNoErrors("Should have no errors", AccountDetails.A1_PaymentMethodInfo);

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			{
				AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AssertHasRowWarning(AccountDetails, "To link this account with the OFX Beneficiary, right-click and select Config for E-Payment");
				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				AssertNoRowWarningContaining(AccountDetails, "To link this account with the OFX Beneficiary, right-click and select Config for E-Payment");
				AccountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
				AssertHasRowError(AccountDetails, "Provider Reference can be recorded only against bank accounts with EPO payment method");
				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				AssertNoRowError(AccountDetails, "Provider Reference can be recorded only against bank accounts with EPO payment method");
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), false))
			{
				AccountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AccountDetails.A1_EPaymentReasonCode = ZString.Empty;
				AssertNoRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				AssertHasRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
				AccountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
				AssertHasRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
				AccountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				AssertNoRowError(AccountDetails, "E-Payments Functionality has been disabled. Please remove this account.");
			}
		}

		public void TestCheckA1_RN_NKCountryCode()
		{
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery());

			Org.CompanyData.OB_IsCreditor = false;
			AccountDetails.A1_RN_NKCountryCode = country.RN_Code;
			AssertNoErrors("Should be no errors", AccountDetails.A1_RN_NKCountryCodeInfo);

			AccountDetails.A1_RN_NKCountryCode = "ZZ";
			AssertHasErrors("Should be errors", AccountDetails.A1_RN_NKCountryCodeInfo);

			AccountDetails.A1_RN_NKCountryCode = "";
			AssertNoErrors("Should be no errors", AccountDetails.A1_RN_NKCountryCodeInfo);

			Org.CompanyData.OB_IsCreditor = true;
			AccountDetails.A1_RN_NKCountryCode = country.RN_Code;
			AssertNoErrors("Should be no errors", AccountDetails.A1_RN_NKCountryCodeInfo);

			AccountDetails.A1_RN_NKCountryCode = "ZZ";
			AssertHasErrors("Should be errors", AccountDetails.A1_RN_NKCountryCodeInfo);

			AccountDetails.A1_RN_NKCountryCode = "";
			AssertNoErrors("Should be no errors", AccountDetails.A1_RN_NKCountryCodeInfo);
		}

		public void TestCheckA1_RX_NKAccountCurrency()
		{
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());

			Org.CompanyData.OB_IsCreditor = true;
			AccountDetails.A1_RX_NKAccountCurrency = currency.RX_Code;
			AssertNoErrors("Should be no errors", AccountDetails.A1_RX_NKAccountCurrencyInfo);

			AccountDetails.A1_RX_NKAccountCurrency = "AAA";
			AssertHasErrors("Should be errors", AccountDetails.A1_RX_NKAccountCurrencyInfo);

			AccountDetails.A1_RX_NKAccountCurrency = "";
			AssertHasErrors("Should be errors", AccountDetails.A1_RX_NKAccountCurrencyInfo);

			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.MarshallIslands;
			AssertNoErrors("Should be no errors", AccountDetails.A1_RX_NKAccountCurrencyInfo);

			Org.CompanyData.OB_IsCreditor = false;
			AccountDetails.A1_RX_NKAccountCurrency = "AAA";
			AssertHasErrors("Should be errors", AccountDetails.A1_RX_NKAccountCurrencyInfo);

			AccountDetails.A1_RX_NKAccountCurrency = "";
			AssertNoErrors("Should be no errors", AccountDetails.A1_RX_NKAccountCurrencyInfo);

			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.MarshallIslands;
			AssertNoErrors("Should be no errors", AccountDetails.A1_RX_NKAccountCurrencyInfo);
		}

		public void TestIsDefaultIsValidatedAfterCurrency()
		{
			AccountDetails.A1_IsDefaultAccount = true;
			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors("IsDefault should be no errors", AccountDetails.A1_IsDefaultAccountInfo);

			AccountDetails = Org.CompanyData.AccountDetailsCollection.AddNew();
			AccountDetails.A1_IsDefaultAccount = true;
			AssertNoErrors("IsDefault should have errors no errors", AccountDetails.A1_IsDefaultAccountInfo);

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertHasErrors("IsDefault should have errors, Should be validated after currency is validated", AccountDetails.A1_IsDefaultAccountInfo);
		}

		public void TestIsDefaultIsValidatedAfterPaymentMethod()
		{
			AccountDetails.A1_IsDefaultAccount = true;
			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors("IsDefault should be no errors", AccountDetails.A1_IsDefaultAccountInfo);

			AccountDetails = Org.CompanyData.AccountDetailsCollection.AddNew();
			AccountDetails.A1_IsDefaultAccount = true;
			AssertNoErrors("IsDefault should have errors no errors", AccountDetails.A1_IsDefaultAccountInfo);

			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AssertHasErrors("IsDefault should have errors, Should be validated after PaymentMethod is validated", AccountDetails.A1_IsDefaultAccountInfo);
		}

		public void TestCheckA1_IsDefaultAccount()
		{
			AccountDetails.A1_PaymentMethod = ZString.Empty;
			AssertNoErrors("Payment Method is empty, should not try to validate", AccountDetails.A1_IsDefaultAccountInfo);

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AccountDetails.A1_RX_NKAccountCurrency = ZString.Empty;
			AssertNoErrors("Currency is empty, should not try to validate", AccountDetails.A1_IsDefaultAccountInfo);

			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			AccountDetails.A1_IsDefaultAccount = false;
			AssertHasErrors("Should have errors, must have 1 account details marked as default with the same currency and payment method", AccountDetails.A1_IsDefaultAccountInfo);
			AssertEquals("Expecting One error", 1, AccountDetails.A1_IsDefaultAccountInfo.GetErrors().Count());
			ZString message = "This account must be the default because there is no other account listed with currency \"AUD\" and payment type \"Check\" and marked as \"Is Default Account\".";
			AssertEquals("Incorrect Message", message, AccountDetails.A1_IsDefaultAccountInfo.GetErrors().GetFirstMessage());

			AccAPAccountDetails accountDetails2 = Org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails2.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			accountDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails2.A1_IsDefaultAccount = true;
			AssertNoErrors("Should have no errors", accountDetails2.A1_IsDefaultAccountInfo);

			AccAPAccountDetails accountDetails3 = Org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails3.A1_IsDefaultAccount = true;
			accountDetails3.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			accountDetails3.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails3.Validation.ValidateA1_IsDefaultAccount();
			AssertHasErrors("Should have errors because more than 1 account details are marked as default with the same currency and payment method", accountDetails3.A1_IsDefaultAccountInfo);
			AssertEquals("Expecting One Error", 1, accountDetails3.A1_IsDefaultAccountInfo.GetErrors().Count());
			message = "You have more than one account listed as the default account for the currency \"AUD\" and the payment type \"Check\".\r\n\r\nPlease check which account really should be the default, and untick the \"Is Default Account\" flag of any others.";
			AssertEquals("Expected Message is incorrect", message, accountDetails3.A1_IsDefaultAccountInfo.GetErrors().GetFirstMessage());

			Org.CompanyData.OB_IsCreditor = false;
			foreach (AccAPAccountDetails accDetails in Org.CompanyData.AccountDetailsCollection)
			{
				accDetails.Validation.ValidateA1_IsDefaultAccount();
				AssertNoErrors("should be no errors because Company is NOT creditor, should not validate", accDetails.A1_IsDefaultAccountInfo);
			}
		}

		public void TestCheckA1_BankAccount()
		{
			Org.CompanyData.OB_IsCreditor = false;
			AccountDetails.A1_BankAccount = "";
			Assert("Company is not creditor, no notifications", !AccountDetails.A1_BankAccountInfo.HasNotifications());

			Org.CompanyData.OB_IsCreditor = true;
			AccountDetails.Validation.ValidateA1_BankAccount();
			Assert("Company is creditor, A1_BankAccount = '', has errors", AccountDetails.A1_BankAccountInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AccountDetails.Validation.ValidateA1_BankAccount();
			Assert("Company is creditor, A1_BankAccount = '', has errors", !AccountDetails.A1_BankAccountInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AccountDetails.Validation.ValidateA1_BankAccount();
			Assert("Company is creditor, A1_BankAccount = '', has errors", AccountDetails.A1_BankAccountInfo.HasErrors());

			AccountDetails.A1_BankAccount = "123456789";
			Assert("Company is creditor, A1_BankAccount = '123456789', no errors", !AccountDetails.A1_BankAccountInfo.HasErrors());
		}

		public void TestCheckA1_BankBsb()
		{
			Org.CompanyData.OB_IsCreditor = false;
			AccountDetails.A1_BankBsb = "";
			Assert("Company is not creditor, no notifications", !AccountDetails.A1_BankBsbInfo.HasNotifications());

			Org.CompanyData.OB_IsCreditor = true;
			AccountDetails.Validation.ValidateA1_BankBsb();
			Assert("Company is creditor, A1_BankBsb = '', has errors", AccountDetails.A1_BankBsbInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AccountDetails.Validation.ValidateA1_BankBsb();
			Assert("Company is creditor, A1_BankBsb = '', has errors", !AccountDetails.A1_BankBsbInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AccountDetails.Validation.ValidateA1_BankBsb();
			Assert("Company is creditor, A1_BankBsb = '', has errors", AccountDetails.A1_BankBsbInfo.HasErrors());

			AccountDetails.A1_BankBsb = "Test Bsb";
			Assert("Company is creditor, A1_BankBsb = 'Test Bsb', no errors", !AccountDetails.A1_BankBsbInfo.HasErrors());
		}

		public void TestCheckA1_BankName()
		{
			Org.CompanyData.OB_IsCreditor = false;
			AccountDetails.A1_BankName = "";
			Assert("Company is not creditor, no notifications", !AccountDetails.A1_BankNameInfo.HasNotifications());

			Org.CompanyData.OB_IsCreditor = true;
			AccountDetails.Validation.ValidateA1_BankName();
			Assert("Company is creditor, A1_BankName = '', has errors", AccountDetails.A1_BankNameInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AccountDetails.Validation.ValidateA1_BankName();
			Assert("Company is creditor, A1_BankName = '', has errors", !AccountDetails.A1_BankNameInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AccountDetails.Validation.ValidateA1_BankName();
			Assert("Company is creditor, A1_BankName = '', has errors", AccountDetails.A1_BankNameInfo.HasErrors());

			AccountDetails.A1_BankName = "Test Bank";
			Assert("Company is creditor, A1_BankName = 'Test Bank', no notifications", !AccountDetails.A1_BankNameInfo.HasNotifications());
		}

		public void TestCheckA1_AccountName()
		{
			Org.CompanyData.OB_IsCreditor = false;
			AccountDetails.A1_AccountName = "";
			Assert("Company is not creditor, no notifications", !AccountDetails.A1_AccountNameInfo.HasNotifications());

			Org.CompanyData.OB_IsCreditor = true;
			AccountDetails.Validation.ValidateA1_AccountName();
			Assert("Company is creditor, A1_AccountName = '', has errors", AccountDetails.A1_AccountNameInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AccountDetails.Validation.ValidateA1_AccountName();
			Assert("Company is creditor, A1_AccountName = '', has errors", !AccountDetails.A1_AccountNameInfo.HasErrors());

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AccountDetails.Validation.ValidateA1_AccountName();
			Assert("Company is creditor, A1_AccountName = '', has errors", AccountDetails.A1_AccountNameInfo.HasErrors());

			AccountDetails.A1_AccountName = "Test Bank Account Name";
			Assert("Company is creditor, A1_AccountName = 'Test Bank', no errors", !AccountDetails.A1_AccountNameInfo.HasErrors());
		}

		public void TestCheckA1_IBANNumberWhenIBANIsEmptyAndCountryIsEuropean()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "E1";
			country.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			AccountDetails.A1_RN_NKCountryCode = "E1";

			AccountDetails.A1_IBANNumber = "";
			AssertHasWarnings(
				"When saving a bank account in an EU country, it is recommended to specify the IBAN account number for more efficient transfer of payments.",
				AccountDetails.A1_IBANNumberInfo);
		}

		public void TestCheckA1_IBANNumberWhenIBANIsEmptyAndCountryIsNotEuropean()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "A1";
			AccountDetails.A1_RN_NKCountryCode = "A1";

			AccountDetails.A1_IBANNumber = "";
			AssertNoWarnings("No Warnings for IBAN", AccountDetails.A1_IBANNumberInfo);
		}

		OrgHeader Org;
		AccAPAccountDetails AccountDetails;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			Org.CompanyData.OB_OH = Factory.New(typeof(OrgHeader)).PK;
			Org.CompanyData.OB_IsCreditor = true;

			AccountDetails = Org.CompanyData.AccountDetailsCollection.AddNew();
			AccountDetails.A1_IsDefaultAccount = true;
			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Philippines;
		}
	}
}
