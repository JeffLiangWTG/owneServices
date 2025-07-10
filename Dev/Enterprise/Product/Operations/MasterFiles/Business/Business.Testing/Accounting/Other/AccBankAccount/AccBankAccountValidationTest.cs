using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.Testing.AccBankAccountTest;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccBankAccountValidationTest : BusinessObjectValidationTestCase
	{
		#region Implementation

		protected AccGLHeader GLHeader1;
		protected AccGLHeader GLHeader2;
		protected AccGLHeader GLHeader6;

		internal MockAccBankAccount BranchABankAccountUSD;
		protected AccBankAccount BranchABankAccountAUD;

		protected override void SetUp()
		{
			base.SetUp();

			GlbBranch branchA = Factory.New<GlbBranch>();
			branchA.GB_Code = "ABC";
			branchA.GB_GC = Env.CurrentCompany.PK;

			BranchABankAccountUSD = Factory.New<MockAccBankAccount>();
			BranchABankAccountAUD = Factory.New<AccBankAccount>();

			RefCurrency localCurrency = Factory.Load<RefCurrency>(Env.CurrentCompany.LocalCurrency.PK);
			RefCurrency currencyUSD = (RefCurrency)Factory.Load(typeof(RefCurrency), new ZQuery(RefCurrencySchema.RX_Code, "USD"))[0];

			ClearAllExistingDefaultBankAccounts();

			do
			{
				SetupBankAccount(BranchABankAccountUSD, branchA, currencyUSD);
				SetupBankAccount(BranchABankAccountAUD, branchA, localCurrency);
			}
			while (CheckIfEqualPairExists());

			SetupGLHeaders();

			Factory.Save();
		}

		// checks if there are two Bank accounts with the same AB_Code in the test data
		protected bool CheckIfEqualPairExists()
		{
			AccBankAccount[] testAccounts = { BranchABankAccountUSD, BranchABankAccountAUD };
			for (int i = 0; i < 2; i++)
			{
				for (int j = i + 1; j < 2; j++)
				{
					if (testAccounts[i].AB_Code == testAccounts[j].AB_Code)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected void SetupBankAccount(AccBankAccount bankAccount, GlbBranch branch, RefCurrency currency)
		{
			bankAccount.AB_AccountNum = AccBankAccountTest.GetRandomString(10);
			bankAccount.AB_BSB = AccBankAccountTest.GetRandomString(6);
			bankAccount.AB_Code = AccBankAccountTest.GetRandomString(3);
			bankAccount.AB_IsDefaultReceiptBankAccount = new ZBool(Core.Constants.BooleanTrueString);
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;

			if (branch == null)
			{
				bankAccount.AB_GB = ZGuid.Empty;
			}
			else
			{
				bankAccount.AB_GB = branch.PK;
			}
			bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;
		}

		protected void SetupGLHeaders()
		{
			GLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader6 = Factory.NewWithValidTestData<AccGLHeader>();

			GLHeader1.AG_AccountNum = "1234567890";
			GLHeader2.AG_AccountNum = "2345678901";
			GLHeader6.AG_AccountNum = "6789012345";

			BranchABankAccountUSD.AB_AG = GLHeader1.PK;
			BranchABankAccountAUD.AB_AG = GLHeader2.PK;
		}

		protected void ClearAllExistingDefaultBankAccounts()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccBankAccount[] existingBankAccounts = (AccBankAccount[])newFactory.Load(typeof(AccBankAccount), new ZQuery());
			foreach (AccBankAccount bankAccount in existingBankAccounts)
			{
				bankAccount.AB_IsDefaultReceiptBankAccount = new ZBool(Core.Constants.BooleanFalseString);
			}
			newFactory.Save();
		}

		GlbBranch SetupNewNewZelandCompanyAndBranch()
		{
			RefCountry newZeland = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand));
			GlbCompany newZelandCompany = Factory.NewWithValidTestData<GlbCompany>();
			newZelandCompany.GC_RN_NKCountryCode = newZeland.Code;
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newZelandCompany.PK;
			Factory.Save();
			return newBranch;
		}

		#endregion

		public void TestCheckAB_PaymentProvider()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = ProviderCodes.OFX;
			bankAccount.AB_PaymentProvider = ZString.Empty;
			AssertHasError(bankAccount.AB_PaymentProviderInfo, "Please enter a value.");
			bankAccount.AB_PaymentProvider = "AAA";
			AssertHasError(bankAccount.AB_PaymentProviderInfo, "Enter a valid selection.");
			bankAccount.AB_PaymentProvider = ProviderCodes.OFX;
			AssertNoErrors(bankAccount.AB_PaymentProviderInfo);
		}

		public void TestUniqueAB_PaymentProvider()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = ProviderCodes.OFX;
			AssertNoErrors(bankAccount.AB_PaymentProviderInfo);
			AssertNoExceptionThrown(() => Factory.Save());

			var secondBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			AssertNoErrors(secondBankAccount.AB_PaymentProviderInfo);

			var thirdBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			thirdBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			thirdBankAccount.AB_PaymentProvider = ProviderCodes.OFX;
			AssertHasError("Cannot have multiple E-Payment accounts with the same provider for a given company.", thirdBankAccount.AB_PaymentProviderInfo, "An E-Payment Account with Provider \"OFX\" already exists in the current login company.");

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var fourthBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
				fourthBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				fourthBankAccount.AB_PaymentProvider = ProviderCodes.OFX;
				AssertNoErrors(fourthBankAccount.AB_PaymentProviderInfo);
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		public void TestCheckAB_SWIFT()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_SWIFT = "123456ABSSS";
			AssertHasError("Should be error", bankAccount.AB_SWIFTInfo, "123456ABSSS is not a valid SWIFT Code.");

			bankAccount.AB_SWIFT = "ABCDE1234LLL";
			AssertHasError("Should be error", bankAccount.AB_SWIFTInfo, "ABCDE1234LLL is not a valid SWIFT Code.");

			bankAccount.AB_SWIFT = "ABCDEF12X8888";
			AssertHasError("Should be error", bankAccount.AB_SWIFTInfo, "ABCDEF12X8888 is not a valid SWIFT Code.");

			bankAccount.AB_SWIFT = "ABCDEF1O888";
			AssertHasError("Should be error", bankAccount.AB_SWIFTInfo, "ABCDEF1O888 is not a valid SWIFT Code.");

			bankAccount.AB_SWIFT = "ABCDEF1X888";
			AssertHasError("Should be error", bankAccount.AB_SWIFTInfo, "ABCDEF1X888 is not a valid SWIFT Code.");

			bankAccount.AB_SWIFT = "ABCDEF2X888";
			AssertNoErrors("Should be no errors", bankAccount.AB_SWIFTInfo);
		}

		public void TestIBANCheck_RaisesOnlyWarning_WhenCountryDifferentThanBankAccountCountry()
		{
			const string warning = "[Bank Account] IBAN Number: The first two characters of the IBAN Number do not match the Bank's Country/Region Code.";
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_RN_NKBankAccountCountry = "GB";

			bankAccount.AB_AccountNumber = "GB82WEST12345698765432";
			AssertNoNotifications(bankAccount.AB_AccountNumberInfo);
			AssertNoNotifications(bankAccount.IBANInfo);

			bankAccount.AB_AccountNumber = "DE75512108001245126199";
			AssertNoErrors(bankAccount.AB_AccountNumberInfo);
			AssertNoErrors(bankAccount.IBANInfo);
			AssertHasWarning(bankAccount.AB_AccountNumberInfo, warning);
			AssertHasWarning(bankAccount.IBANInfo, warning);
		}

		public void TestIBANCheck_RaisesError_WhenNumericOrAlphanumericPartIsIncorrect()
		{
			const string error = "The third and fourth characters of the IBAN Number must be numeric. From fifth character onward, it should only contain alphanumeric values only.";
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_RN_NKBankAccountCountry = "GB";

			bankAccount.AB_AccountNumber = "GBabXYZ1234";
			AssertHasError(bankAccount.AB_AccountNumberInfo, error);
			AssertHasError(bankAccount.IBANInfo, error);

			bankAccount.AB_AccountNumber = "GB12";
			AssertHasError(bankAccount.AB_AccountNumberInfo, error);
			AssertHasError(bankAccount.IBANInfo, error);

			bankAccount.AB_AccountNumber = "GB12af,dd";
			AssertHasError(bankAccount.AB_AccountNumberInfo, error);
			AssertHasError(bankAccount.IBANInfo, error);
		}

		public void TestIBANCheck_RaisesError_WhenLengthIsDifferentThanIBANCountryRelatedLength()
		{
			const string error = "The length of IBAN Number is incorrect, it should be '22' in Country/Region 'GB'.";
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_RN_NKBankAccountCountry = "TR"; // IBAN length is checked against the Country/Region denoted by the first two characters of the IBAN, not by the bank account country.

			bankAccount.AB_AccountNumber = "GB82WEST12345698765432";
			AssertNoErrors(bankAccount.AB_AccountNumberInfo);
			AssertNoErrors(bankAccount.IBANInfo);

			bankAccount.AB_AccountNumber = "GB82WEST123456";
			AssertHasError(bankAccount.AB_AccountNumberInfo, error);
			AssertHasError(bankAccount.IBANInfo, error);
		}

		public void TestIBANCheck_RaisesError_WhenMod97CheckFailed()
		{
			const string error = "The remainder calculated using the MOD 97 algorithm should be equal to 1. Please check the IBAN to confirm it is entered correctly.";
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_RN_NKBankAccountCountry = "GB";

			bankAccount.AB_AccountNumber = "GB82WEST12345698765432";
			AssertNoNotifications(bankAccount.AB_AccountNumberInfo);
			AssertNoNotifications(bankAccount.IBANInfo);

			bankAccount.AB_AccountNumber = "GB82WEST12345698765431";
			AssertHasError(bankAccount.AB_AccountNumberInfo, error);
			AssertHasError(bankAccount.IBANInfo, error);
		}

		public void TestValidateAB_AccountEFTUserID()
		{
			string errorMessage = "The maximum length is 6 characters when DDR File Format is '" + Core.Constants.DDRFileFormat.WBC + "'.";

			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AccountEFTUserID = "1234567";
			AssertNoError(bankAccount.AB_AccountEFTUserIDInfo, errorMessage);

			bankAccount.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.WBC;
			bankAccount.Validation.ValidateAB_AccountEFTUserID();
			AssertHasError(bankAccount.AB_AccountEFTUserIDInfo, errorMessage);

			bankAccount.AB_AccountEFTUserID = "123456";
			AssertNoError(bankAccount.AB_AccountEFTUserIDInfo, errorMessage);
		}

		public void TestValidateAB_Code()
		{
			BranchABankAccountUSD.AB_Code = "";
			BranchABankAccountUSD.Validation.ValidateAB_Code();
			AssertHasError(BranchABankAccountUSD.AB_CodeInfo, "Please enter a Bank Code.");

			BranchABankAccountUSD.AB_Code = "aaa.";
			BranchABankAccountUSD.Validation.ValidateAB_Code();
			AssertHasError(BranchABankAccountUSD.AB_CodeInfo, "aaa. is not a valid Code.");

			BranchABankAccountUSD.AB_Code = "aaa";
			BranchABankAccountUSD.Validation.ValidateAB_Code();
			AssertNoErrors(BranchABankAccountUSD.AB_CodeInfo);
		}

		public void TestCheckAB_CodeIsUnique()
		{
			BranchABankAccountUSD.AB_Code = BranchABankAccountAUD.AB_Code;
			BranchABankAccountUSD.Validation.ValidateAB_Code();
			AssertHasError(BranchABankAccountUSD.AB_CodeInfo, "This Bank Code is already used by another Bank Account");

			BranchABankAccountUSD.AB_Code = "aaa";
			BranchABankAccountUSD.Validation.ValidateAB_Code();
			AssertNoErrors(BranchABankAccountUSD.AB_CodeInfo);
		}

		public void TestValidateAB_GB()
		{
			// Bank account USD has branch A 
			// Note: at this stage AB_GB is NOT empty
			BranchABankAccountUSD.Validation.ValidateAB_GB();
			AssertNoErrors(BranchABankAccountUSD.AB_GBInfo);

			// Bank account USD is referenced by a cheque book
			AccChequeBook cheque = Factory.New<AccChequeBook>();
			cheque.AK_AB = BranchABankAccountUSD.PK;
			cheque.AK_GB = BranchABankAccountUSD.AB_GB;

			BranchABankAccountUSD.ChequeBooks = new AccChequeBook[] { cheque };
			BranchABankAccountUSD.Validation.ValidateAB_GB();
			AssertNoErrors(BranchABankAccountUSD.AB_GBInfo);

			AccChequeBook cheque2 = Factory.New<AccChequeBook>();
			cheque2.AK_AB = BranchABankAccountUSD.PK;
			cheque2.AK_GB = ZGuid.NewZGuid();
			BranchABankAccountUSD.ChequeBooks = new AccChequeBook[] { cheque, cheque2 };
			BranchABankAccountUSD.Validation.ValidateAB_GB();
			AssertHasError(BranchABankAccountUSD.AB_GBInfo, "Branch code cannot be change as this account is referenced by one or more check books with different branches");

			BranchABankAccountUSD.AB_GB = ZGuid.Empty;
			BranchABankAccountUSD.Validation.ValidateAB_GB();
			AssertNoErrors(BranchABankAccountUSD.AB_GBInfo);
		}

		public void TestValidateAB_Desc()
		{
			BranchABankAccountUSD.AB_Desc = "";
			BranchABankAccountUSD.Validation.ValidateAB_Desc();
			AssertHasError(BranchABankAccountUSD.AB_DescInfo, "Please enter a Description.");

			BranchABankAccountUSD.AB_Desc = "something";
			BranchABankAccountUSD.Validation.ValidateAB_Desc();
			AssertNoErrors(BranchABankAccountUSD.AB_DescInfo);
		}

		public void TestValidateAB_AG()
		{
			BranchABankAccountUSD.AB_AG = GLHeader1.PK;
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_AB = BranchABankAccountUSD.PK;
			header.AH_PostToGL = Core.Constants.BooleanTrueString;
			Factory.Save();

			BranchABankAccountUSD.AB_AG = GLHeader2.PK;
			AssertHasErrors(BranchABankAccountUSD.AB_AGInfo);
			AssertHasError("The General Ledger account cannot be changed", BranchABankAccountUSD.AB_AGInfo, "The General Ledger account cannot be changed because there are transactions posted for this bank account. Reset the General Ledger account to 1234567890.");
			header.Delete();
			BranchABankAccountUSD.AB_AG = GLHeader1.PK;
			Factory.Save();

			BranchABankAccountAUD.AB_AG = ZGuid.Empty;
			AssertHasErrors(BranchABankAccountAUD.AB_AGInfo);
			AssertHasError("Empty GL Account", BranchABankAccountAUD.AB_AGInfo, "Please enter a GL Account.");

			BranchABankAccountAUD.AB_AG = GLHeader6.PK;
			AssertNoErrors(BranchABankAccountAUD.AB_AGInfo);

			BranchABankAccountUSD.AB_AG = GLHeader6.PK;
			AssertHasError("Same GL account used for different bank accounts in one company", BranchABankAccountUSD.AB_AGInfo, "This GL Account is already used by another Bank Account in the current login company.");
			header.Delete();
			BranchABankAccountUSD.AB_AG = GLHeader1.PK;
			Factory.Save();

			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China)).Code;
			GlbBranch branchB = Factory.New<GlbBranch>();
			branchB.GB_GC = newCompany.PK;
			branchB.GB_Code = "DEF";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchB.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccBankAccount branchBBankAccountCNY = Factory.New<AccBankAccount>();
				SetupBankAccount(branchBBankAccountCNY, branchB, Factory.Load<RefCurrency>(Env.CurrentCompany.LocalCurrency.PK));

				branchBBankAccountCNY.AB_AG = GLHeader1.PK;
				AssertNoErrors(branchBBankAccountCNY.AB_AGInfo);
				Factory.Save();
			}
		}

		public void TestValidateAB_BankName()
		{
			BranchABankAccountUSD.AB_BankName = "";
			BranchABankAccountUSD.Validation.ValidateAB_BankName();
			AssertHasError(BranchABankAccountUSD.AB_BankNameInfo, "Please enter a Bank Name.");

			BranchABankAccountUSD.AB_BankName = "blah";
			BranchABankAccountUSD.Validation.ValidateAB_BankName();
			AssertNoErrors(BranchABankAccountUSD.AB_BankNameInfo);

			BranchABankAccountUSD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			BranchABankAccountUSD.AB_BankName = "";
			BranchABankAccountUSD.Validation.ValidateAB_BankName();
			AssertNoErrors(BranchABankAccountUSD.AB_BankNameInfo);
		}

		public void TestValidateAB_BankAddress()
		{
			BranchABankAccountUSD.AB_BankAddress = "";
			BranchABankAccountUSD.Validation.ValidateAB_BankAddress();
			AssertHasError(BranchABankAccountUSD.AB_BankAddressInfo, "Please enter a Bank Address.");

			BranchABankAccountUSD.AB_BankAddress = "blah";
			BranchABankAccountUSD.Validation.ValidateAB_BankAddress();
			AssertNoErrors(BranchABankAccountUSD.AB_BankAddressInfo);

			BranchABankAccountUSD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			BranchABankAccountUSD.AB_BankAddress = "";
			BranchABankAccountUSD.Validation.ValidateAB_BankName();
			AssertNoErrors(BranchABankAccountUSD.AB_BankNameInfo);
		}

		public void TestValidateAB_BSB()
		{
			BranchABankAccountUSD.AB_AllowAutoDDR = true;

			BranchABankAccountUSD.AB_BSB = "";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "Please enter a B S B.");

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			BranchABankAccountUSD.AB_BSB = "123-345";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertNoErrors(BranchABankAccountUSD.AB_BSBInfo);

			BranchABankAccountUSD.AB_BSB = "123456";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'ANZ' Direct Debit System must have the pattern 'XXX-XXX'");

			GlbBranch newBranch = SetupNewNewZelandCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				BranchABankAccountUSD.Validation.ValidateAB_BSB();
				AssertNoErrors(BranchABankAccountUSD.AB_BSBInfo);

				BranchABankAccountUSD.AB_BSB = "123-456";
				BranchABankAccountUSD.Validation.ValidateAB_BSB();
				AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'ANZ' Direct Debit System must have the pattern 'XXXXXX'");

				BranchABankAccountUSD.AB_BSB = "123456";
				BranchABankAccountUSD.Validation.ValidateAB_BSB();
				AssertNoErrors(BranchABankAccountUSD.AB_BSBInfo);
			}

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.NAB;
			BranchABankAccountUSD.AB_BSB = "123-3456";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'NAB' Direct Debit System must have the pattern 'XXX-XXX'");

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BNZ;
			BranchABankAccountUSD.AB_BSB = "123-345";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'BNZ' Direct Debit System must have the pattern 'XXXXXX'");

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BNZ;
			BranchABankAccountUSD.AB_BSB = "1233456";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'BNZ' Direct Debit System must have the pattern 'XXXXXX'");

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BNZ;
			BranchABankAccountUSD.AB_BSB = "123345";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertNoErrors(BranchABankAccountUSD.AB_BSBInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			AssertNoErrors("Precondition: BSB number should not have errors", BranchABankAccountUSD.AB_BSBInfo);
			BranchABankAccountUSD.AB_BSB = "123-456";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertHasError("BSB number should have errors", BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'ASB' Direct Debit System must have the pattern 'XXXXXX'");
			BranchABankAccountUSD.AB_BSB = "889083";
			BranchABankAccountUSD.Validation.ValidateAB_BSB();
			AssertNoErrors("BSB number should not have errors", BranchABankAccountUSD.AB_BSBInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BBL;
			BranchABankAccountUSD.AB_BSB = "583-309";
			AssertNoErrors("BSB number should not have errors", BranchABankAccountUSD.AB_BSBInfo);
			BranchABankAccountUSD.AB_BSB = "579909";
			AssertHasError("BSB number should have errors", BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'BBL' Direct Debit System must have the pattern 'XXX-XXX'");

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.WNZ;
			BranchABankAccountUSD.AB_BSB = "389-590";
			AssertHasError("BSB number should have errors", BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'WNZ' Direct Debit System must have the pattern 'XXXXXX'");
			BranchABankAccountUSD.AB_BSB = "389590";
			AssertNoErrors("BSB number should not have errors", BranchABankAccountUSD.AB_BSBInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BCS;
			BranchABankAccountUSD.AB_BSB = "123-345";
			AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'BCS' Direct Debit System must have the pattern 'XXXXXX'");

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BCS;
			BranchABankAccountUSD.AB_BSB = "1233456";
			AssertHasError(BranchABankAccountUSD.AB_BSBInfo, "The BSB when using the 'BCS' Direct Debit System must have the pattern 'XXXXXX'");

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BCS;
			BranchABankAccountUSD.AB_BSB = "123345";
			AssertNoErrors(BranchABankAccountUSD.AB_BSBInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			BranchABankAccountUSD.AB_BSB = "123345333333";
			AssertNoErrors(BranchABankAccountUSD.AB_BSBInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BTM;
			AssertNoErrors(BranchABankAccountUSD.AB_BSBInfo);
		}

		public void TestValidateAB_AccountNum()
		{
			BranchABankAccountUSD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			BranchABankAccountUSD.AB_AccountNum = "";
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors(BranchABankAccountUSD.AB_AccountNumInfo);

			BranchABankAccountUSD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			BranchABankAccountUSD.AB_AccountNum = "";
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors(BranchABankAccountUSD.AB_AccountNumInfo);

			BranchABankAccountUSD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			BranchABankAccountUSD.AB_AccountNum = "";
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertHasError(BranchABankAccountUSD.AB_AccountNumInfo, "Please enter an Account Number.");

			BranchABankAccountUSD.AB_AccountNum = "somethinglongerthan9characters";
			BranchABankAccountUSD.AB_AllowAutoDDR = false;
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors(BranchABankAccountUSD.AB_AccountNumInfo);

			BranchABankAccountUSD.AB_AccountNum = "somethinglongerthan9characters";
			BranchABankAccountUSD.AB_AllowAutoDDR = true;
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertHasError(BranchABankAccountUSD.AB_AccountNumInfo, "The account number must be 9 or less characters in length");

			BranchABankAccountUSD.AB_AccountNum = "1234567890";
			BranchABankAccountUSD.AB_AllowAutoDDR = true;
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertHasError(BranchABankAccountUSD.AB_AccountNumInfo, "The account number must be 9 or less characters in length");

			BranchABankAccountUSD.AB_AccountNum = "123456789";
			BranchABankAccountUSD.AB_AllowAutoDDR = true;
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors(BranchABankAccountUSD.AB_AccountNumInfo);

			BranchABankAccountUSD.AB_AccountNum = "123456";
			BranchABankAccountUSD.AB_AllowAutoDDR = true;
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors("Bank account number should be of maxlength 9, so no errors", BranchABankAccountUSD.AB_AccountNumInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertHasError("Bank account number must be 9 chars long for ASB Bank, so there should be errors", BranchABankAccountUSD.AB_AccountNumInfo, "The account number must be 9 characters in length for ASB Bank DDR format");

			BranchABankAccountUSD.AB_AccountNum = "987654321";
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors("Bank account number should be 9 chars long - no errors", BranchABankAccountUSD.AB_AccountNumInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			BranchABankAccountUSD.AB_AccountNum = "9876543213233232322232";
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors(BranchABankAccountUSD.AB_AccountNumInfo);

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BTM;
			BranchABankAccountUSD.Validation.ValidateAB_AccountNum();
			AssertNoErrors(BranchABankAccountUSD.AB_AccountNumInfo);
		}

		AccBankAccount CreateBankAccount(ZString accountType, ZString currency)
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AccountType = accountType;
			bankAccount.AB_RX_NKAccountCurrency = currency;

			return bankAccount;
		}

		public void TestValidateAB_RXAccountCurrency()
		{
			var testBankAccountTH = Factory.NewWithValidTestData<AccBankAccount>();
			var testBankAccountJC = Factory.NewWithValidTestData<AccBankAccount>();
			var testBankAccountPA = Factory.NewWithValidTestData<AccBankAccount>();
			var testBankAccountHC = Factory.NewWithValidTestData<AccBankAccount>();
			var testBankAccountCC = Factory.NewWithValidTestData<AccBankAccount>();

			testBankAccountTH.AB_RX_NKAccountCurrency = "XXX";
			AssertHasError("Invalid currency", testBankAccountTH.AB_RX_NKAccountCurrencyInfo, "Enter a valid Currency.");

			testBankAccountTH.AB_RX_NKAccountCurrency = testBankAccountJC.AB_RX_NKAccountCurrency = testBankAccountPA.AB_RX_NKAccountCurrency = testBankAccountHC.AB_RX_NKAccountCurrency = testBankAccountCC.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors("Valid currency", testBankAccountTH.AB_RX_NKAccountCurrencyInfo);

			Factory.Save();

			testBankAccountTH.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			AssertNoErrors("Must Has No Errors Because Change in Currency Allowed Before Bank is Used By Any Transaction", testBankAccountTH.AB_RX_NKAccountCurrencyInfo);

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var payment = Factory.NewWithValidTestData<AccPaymentApproval>();
			var chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			var consolCost = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IJobConsolCost)));

			header.AH_AB = testBankAccountTH.PK;
			charge.JR_AB = testBankAccountJC.PK;
			payment.AV_AB = testBankAccountPA.PK;
			chequeBook.AK_AB = testBankAccountHC.PK;
			consolCost[JobConsolCostSchema.E6_AB_BankAccount] = testBankAccountCC.PK;
			Factory.Save();

			var sql = $"INSERT INTO dbo.AccHotCheque (AQ_PK, AQ_AK, AQ_ActualOrMaxIndicator, AQ_SystemCreateTimeUtc, AQ_SystemCreateUser, AQ_SystemLastEditTimeUtc, AQ_SystemLastEditUser) values (NEWID(), '{chequeBook.PK}', '{ZArchitecture.Core.ActualOrMaxIndicator.Actual}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);

			AssertEquals(Core.Constants.CurrencyCodes.UnitedKingdom, testBankAccountTH.AB_RX_NKAccountCurrency);
			AssertEquals("Bank Account currency has no changes", false, testBankAccountTH.AB_RX_NKAccountCurrencyInfo.HasChanges);
			testBankAccountTH.Validation.ValidateAB_RX_NKAccountCurrency();
			AssertNoErrors("Must Has No Errors Because No Change in Currency", testBankAccountTH.AB_RX_NKAccountCurrencyInfo);

			AssertEquals(Core.Constants.CurrencyCodes.UnitedKingdom, testBankAccountTH.AB_RX_NKAccountCurrency);
			testBankAccountTH.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertHasError("Must Has Errors Because Change in Currency Not Allowed After Bank is Used By TransactionHeader", testBankAccountTH.AB_RX_NKAccountCurrencyInfo, "Bank Account Currency cannot be changed. At least one posted transaction references this Bank Account and its existing currency");

			AssertEquals(Core.Constants.CurrencyCodes.Australia, testBankAccountJC.AB_RX_NKAccountCurrency);
			testBankAccountJC.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertHasError("Must Has Errors Because Change in Currency Not Allowed After Bank is Used By JobCharge", testBankAccountJC.AB_RX_NKAccountCurrencyInfo, "Bank Account Currency cannot be changed. At least one job charge references this Bank Account and its existing currency");

			AssertEquals(Core.Constants.CurrencyCodes.Australia, testBankAccountPA.AB_RX_NKAccountCurrency);
			testBankAccountPA.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertHasError("Must Has Errors Because Change in Currency Not Allowed After Bank is Used By AccPaymentApproval", testBankAccountPA.AB_RX_NKAccountCurrencyInfo, "Bank Account Currency cannot be changed. At least one payment approval references this Bank Account and its existing currency");

			AssertEquals(Core.Constants.CurrencyCodes.Australia, testBankAccountHC.AB_RX_NKAccountCurrency);
			testBankAccountHC.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertHasError("Must Has Errors Because Change in Currency Not Allowed After Bank is Used By AccHotCheque", testBankAccountHC.AB_RX_NKAccountCurrencyInfo, "Bank Account Currency cannot be changed. At least one hot cheque references this Bank Account and its existing currency");

			AssertEquals(Core.Constants.CurrencyCodes.Australia, testBankAccountCC.AB_RX_NKAccountCurrency);
			testBankAccountCC.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertHasError("Must Has Errors Because Change in Currency Not Allowed After Bank is Used By JobConsolCost", testBankAccountCC.AB_RX_NKAccountCurrencyInfo, "Bank Account Currency cannot be changed. At least one consol cost references this Bank Account and its existing currency");

			// Currency is unset for the cases below
			var bankAccountBNK = CreateBankAccount(AccountTypeCodeDescriptionPairList.Codes.BNK, "");
			AssertHasError("BNK account type, empty currency should be an error.", bankAccountBNK.AB_RX_NKAccountCurrencyInfo, "Please enter a Currency.");

			var bankAccountCCD = CreateBankAccount(AccountTypeCodeDescriptionPairList.Codes.CCD, "");
			AssertHasError("CCD account type, empty currency should be an error.", bankAccountCCD.AB_RX_NKAccountCurrencyInfo, "Please enter a Currency.");

			var bankAccountLNK = CreateBankAccount(AccountTypeCodeDescriptionPairList.Codes.LNK, "");
			AssertHasError("LNK account type, empty currency should be an error.", bankAccountLNK.AB_RX_NKAccountCurrencyInfo, "Please enter a Currency.");

			// Currency is set for the cases below
			var bankAccountBNK2 = CreateBankAccount(AccountTypeCodeDescriptionPairList.Codes.BNK, "USD");
			AssertNoError("BNK account type, filled currency should not be an error.", bankAccountBNK2.AB_RX_NKAccountCurrencyInfo, "Please enter a Currency.");

			var bankAccountCCD2 = CreateBankAccount(AccountTypeCodeDescriptionPairList.Codes.CCD, "USD");
			AssertNoError("CCD account type, filled currency should not be an error.", bankAccountCCD2.AB_RX_NKAccountCurrencyInfo, "Please enter a Currency.");

			var bankAccountLNK2 = CreateBankAccount(AccountTypeCodeDescriptionPairList.Codes.LNK, "USD");
			AssertNoError("LNK account type, filled currency should not be an error.", bankAccountLNK2.AB_RX_NKAccountCurrencyInfo, "Please enter a Currency.");
		}

		public void TestValidateAB_RX_NKAccountCurrencyIsMandatoryForCashAccount()
		{
			var testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			testBankAccount.AB_RX_NKAccountCurrency = "";

			AssertHasError("Currency can't be empty", testBankAccount.AB_RX_NKAccountCurrencyInfo, "Please enter a Currency.");
		}

		public void TestValidateAB_BankAbbreviation()
		{
			BranchABankAccountAUD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			BranchABankAccountAUD.AB_BankAbbreviation = "";
			BranchABankAccountAUD.Validation.ValidateAB_BankAbbreviation();
			AssertNoErrors(BranchABankAccountAUD.AB_BankAbbreviationInfo);

			BranchABankAccountAUD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			BranchABankAccountAUD.AB_BankAbbreviation = "";
			BranchABankAccountAUD.Validation.ValidateAB_BankAbbreviation();
			AssertNoErrors(BranchABankAccountAUD.AB_BankAbbreviationInfo);

			BranchABankAccountAUD.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			BranchABankAccountAUD.AB_BankAbbreviation = "";
			BranchABankAccountAUD.Validation.ValidateAB_BankAbbreviation();
			AssertHasError(BranchABankAccountAUD.AB_BankAbbreviationInfo, "Please enter a Bank Abbreviation.");

			BranchABankAccountAUD.AB_BankAbbreviation = "BOB";
			BranchABankAccountAUD.Validation.ValidateAB_BankAbbreviation();
			AssertNoErrors(BranchABankAccountAUD.AB_BankAbbreviationInfo);
		}

		public void TestValidateAB_AutoDDRFormat()
		{
			BranchABankAccountAUD.AB_AllowAutoDDR = true;
			BranchABankAccountAUD.AB_AutoDDRFormat = "";
			BranchABankAccountAUD.Validation.ValidateAB_AutoDDRFormat();
			AssertHasError(BranchABankAccountAUD.AB_AutoDDRFormatInfo, "Please enter a DDR File Format.");

			BranchABankAccountAUD.AB_AutoDDRFormat = "POP";
			BranchABankAccountAUD.Validation.ValidateAB_AutoDDRFormat();
			AssertHasError(BranchABankAccountAUD.AB_AutoDDRFormatInfo, "Enter a valid DDR File Format.");

			BranchABankAccountAUD.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			BranchABankAccountAUD.Validation.ValidateAB_AutoDDRFormat();
			AssertNoErrors(BranchABankAccountAUD.AB_AutoDDRFormatInfo);
		}

		public void TestValidateAB_IsDefaultReceiptBankAccount()
		{
			AccBankAccount testBankAccountA = Factory.New<AccBankAccount>();
			testBankAccountA.AB_GC = ZGuid.NewZGuid();
			testBankAccountA.AB_RX_NKAccountCurrency = "AAA";
			testBankAccountA.AB_IsDefaultReceiptBankAccount = true;

			// TestBankAccountB has the same company and currency as TestBankAccountA and both are the default account
			AccBankAccount testBankAccountB = Factory.New<AccBankAccount>();
			testBankAccountB.AB_GC = testBankAccountA.AB_GC;
			testBankAccountB.AB_RX_NKAccountCurrency = testBankAccountA.AB_RX_NKAccountCurrency;
			testBankAccountB.AB_IsDefaultReceiptBankAccount = true;

			testBankAccountB.Validation.ValidateAB_IsDefaultReceiptBankAccount();
			AssertHasError(testBankAccountB.AB_IsDefaultReceiptBankAccountInfo, "You cannot set more than one bank as default bank account for a particular currency and specific branch or all branch");

			// TestBankAccountB has a branch, TestBankAccountA has null branch
			testBankAccountB.AB_GB = ZGuid.NewZGuid();
			testBankAccountB.Validation.ValidateAB_IsDefaultReceiptBankAccount();
			AssertNoErrors(testBankAccountB.AB_IsDefaultReceiptBankAccountInfo);

			// Both test accounts have the same branch
			testBankAccountA.AB_GB = testBankAccountB.AB_GB;
			testBankAccountB.Validation.ValidateAB_IsDefaultReceiptBankAccount();
			AssertHasError(testBankAccountB.AB_IsDefaultReceiptBankAccountInfo, "You cannot set more than one bank as default bank account for a particular currency and specific branch or all branch");

			// The test accounts have different companies
			testBankAccountB.AB_GC = ZGuid.NewZGuid();
			testBankAccountB.Validation.ValidateAB_IsDefaultReceiptBankAccount();
			AssertNoErrors(testBankAccountB.AB_IsDefaultReceiptBankAccountInfo);
		}

		public void TestValidateAB_ChequeNumDigits()
		{
			BranchABankAccountUSD.AB_ChequeNumDigits = 0;
			AssertHasErrors("0 length for no of cheque digits is an error", BranchABankAccountUSD.AB_ChequeNumDigitsInfo);
			AssertHasError("Has this error message", BranchABankAccountUSD.AB_ChequeNumDigitsInfo, "The number of check digits must be between 1 and " + AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength.ToString());

			BranchABankAccountUSD.AB_ChequeNumDigits = Convert.ToByte(AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength - 1);
			AssertNoErrors("Has no error", BranchABankAccountUSD.AB_ChequeNumDigitsInfo);

			BranchABankAccountUSD.AB_ChequeNumDigits = Convert.ToByte(AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength + 1);
			AssertHasErrors("Has this error", BranchABankAccountUSD.AB_ChequeNumDigitsInfo);
			AssertHasError("Has this error", BranchABankAccountUSD.AB_ChequeNumDigitsInfo, "The number of check digits must be between 1 and " + AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength.ToString());
		}

		public void TestValidateBSBForWNZ()
		{
			AccBankAccount testBankAccount = Factory.New<AccBankAccount>();
			testBankAccount.AB_AutoDDRFormat = Constants.DDRFileFormat.WNZ;

			testBankAccount.AB_BSB = "122345";
			testBankAccount.Validation.ValidateAB_BSB();

			AssertNoErrors(testBankAccount.AB_BSBInfo);

			testBankAccount.AB_BSB = "122-345";
			testBankAccount.Validation.ValidateAB_BSB();
			Assert(testBankAccount.AB_BSBInfo.HasError("The BSB when using the 'WNZ' Direct Debit System must have the pattern 'XXXXXX'"));

			testBankAccount.AB_BSB = "12345";
			testBankAccount.Validation.ValidateAB_BSB();
			Assert(testBankAccount.AB_BSBInfo.HasError("The BSB when using the 'WNZ' Direct Debit System must have the pattern 'XXXXXX'"));
		}

		public void TestValidateAccountNumForWNZ()
		{
			AccBankAccount testBankAccount = Factory.New<AccBankAccount>();
			testBankAccount.AB_AutoDDRFormat = Constants.DDRFileFormat.WNZ;
			testBankAccount.AB_AllowAutoDDR = true;

			testBankAccount.AB_AccountNum = "12345";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertNoErrors(testBankAccount.AB_AccountNumInfo);

			testBankAccount.AB_AccountNum = "123456789012";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertNoErrors(testBankAccount.AB_AccountNumInfo);

			testBankAccount.AB_AccountNum = "12345678901234";
			testBankAccount.Validation.ValidateAB_AccountNum();
			Assert(testBankAccount.AB_AccountNumInfo.HasError("The account number must be 12 or less characters in length"));
		}

		public void TestValidateAccountNumForASB()
		{
			AccBankAccount testBankAccount = Factory.New<AccBankAccount>();
			testBankAccount.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			testBankAccount.AB_AllowAutoDDR = true;

			testBankAccount.AB_AccountNum = "123456789";
			Assert(testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "1234567890";
			Assert(testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "12345678901234";
			Assert(!testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));
			AssertEquals("The account number must be 9 or 10 characters in length for ASB Bank DDR format", testBankAccount.GetInvalidAccountNumberErrorMessage());
		}

		public void TestValidateAccountNumForBNZ()
		{
			AccBankAccount testBankAccount = Factory.New<AccBankAccount>();
			testBankAccount.AB_AutoDDRFormat = Constants.DDRFileFormat.BNZ;
			testBankAccount.AB_AllowAutoDDR = true;

			testBankAccount.AB_AccountNum = "123456789";
			Assert(testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "1234567890";
			Assert(testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "12345678901234";
			Assert(!testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));
			AssertEquals("The account number must be 9 or 10 characters in length for BNZ Bank DDR format", testBankAccount.GetInvalidAccountNumberErrorMessage());
		}

		public void TestValidateAccountNumForBCS()
		{
			AccBankAccount testBankAccount = Factory.New<AccBankAccount>();
			testBankAccount.AB_AutoDDRFormat = Constants.DDRFileFormat.BCS;
			testBankAccount.AB_AllowAutoDDR = true;

			testBankAccount.AB_AccountNum = "";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertHasError("Empty", testBankAccount.AB_AccountNumInfo, "Please enter an Account Number.");
			Assert("Empty", !testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "1";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertHasError("Too short", testBankAccount.AB_AccountNumInfo, "The account number must comprise exactly 8 numeric characters for BCS Bank DDR format");
			Assert("Too short", !testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "1234567";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertHasError("Short", testBankAccount.AB_AccountNumInfo, "The account number must comprise exactly 8 numeric characters for BCS Bank DDR format");
			Assert("Short", !testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "123456789";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertHasError("Long", testBankAccount.AB_AccountNumInfo, "The account number must comprise exactly 8 numeric characters for BCS Bank DDR format");
			Assert("Long", !testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "123X5678";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertHasError("Not all chars are numerics", testBankAccount.AB_AccountNumInfo, "The account number must comprise exactly 8 numeric characters for BCS Bank DDR format");
			Assert("Not all chars are numeric", !testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));

			testBankAccount.AB_AccountNum = "12345678";
			testBankAccount.Validation.ValidateAB_AccountNum();
			AssertNoErrors(testBankAccount.AB_AccountNumInfo);
			Assert(testBankAccount.IsValidAccountNumber(testBankAccount.AB_AccountNum));
		}

		public void TestValidateAB_IsActive()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_IsActive = true;
			bankAccount.AB_IsDefaultReceiptBankAccount = true;
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			bankAccount.Validation.ValidateAB_IsActive();

			AssertNoErrors("Bank account should not have errors: " + bankAccount.GetErrors().ToUniqueMessageListString(), bankAccount);

			bankAccount.AB_IsActive = false;
			bankAccount.Validation.ValidateAB_IsActive();

			AssertHasError(bankAccount.AB_IsActiveInfo, "It is not possible to set a Bank Account to inactive while it is set as a default receipt bank account.");

			bankAccount.AB_IsDefaultReceiptBankAccount = false;

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "XYZAAA";
			orgHeader.CompanyData.OverrideBankAccountFromDebtorGroup = true;
			orgHeader.CompanyData.OB_AB_ARPayToAccount = bankAccount.PK;
			Factory.Save();

			bankAccount.Validation.ValidateAB_IsActive();

			AssertEquals("Bank account should have a warning.", true, bankAccount.HasWarnings());
			AssertHasWarning(bankAccount.AB_IsActiveInfo, "This Bank Account is nominated as the 'Bank to This Account' for the following Organizations: XYZAAA.");

			OrgDebtorGroup orgDebtorGroup = Factory.New<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "XYZ";
			orgDebtorGroup.OJ_Desc = "XYZ Company";
			orgDebtorGroup.DefaultBankAccountPK = bankAccount.PK;
			Factory.Save();

			bankAccount.Validation.ValidateAB_IsActive();

			AssertHasWarning(bankAccount.AB_IsActiveInfo, "This Bank Account is nominated as a Default Bank Account for the following Debtor Groups: XYZ.");

			BankAccountBasedOnCurrencyCollection originalValue = OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.Value;
			try
			{
				BankAccountBasedOnCurrencyCollection collection = OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.Value;
				BankAccountBasedOnCurrency bankAccountBasedOnCurrency = collection.AddNew();
				bankAccountBasedOnCurrency.BankAccount = bankAccount.PK;
				bankAccountBasedOnCurrency.Currency = "AUD";
				OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				bankAccount.Validation.ValidateAB_IsActive();

				AssertHasWarning(bankAccount.AB_IsActiveInfo, "This Bank Account is nominated as a Default Bank Account in the following registry setting: Organizations > Default Values > Bank Accounts for AR Documents and Receipting.");
			}
			finally
			{
				OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateAB_RN_NKBankAccountCountry()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();

			bankAccount.AB_RN_NKBankAccountCountry = "";
			AssertHasError("Country code can't be empty", bankAccount.AB_RN_NKBankAccountCountryInfo, "Please enter a Bank Account Country/Region.");

			bankAccount.AB_RN_NKBankAccountCountry = "XX";
			AssertHasError("Invalid country code", bankAccount.AB_RN_NKBankAccountCountryInfo, "Enter a valid Bank Account Country/Region.");

			bankAccount.AB_RN_NKBankAccountCountry = "NZ";
			AssertNoErrors("Valid country code", bankAccount.AB_RN_NKBankAccountCountryInfo);
		}

		public void TestCheckAB_AccountNumberWhenIBANIsEmptyAndCountryIsEuropean()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "E1";
			country.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;

			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_RN_NKBankAccountCountry = "E1";
			bankAccount.AB_AccountNumber = "";
			AssertHasWarnings(
				"When saving a bank account in an EU country, it is recommended to specify the IBAN account number for more efficient transfer of payments.",
				bankAccount.AB_AccountNumberInfo);
		}

		public void TestCheckAB_AccountNumberWhenIBANIsEmptyAndCountryIsNotEuropean()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "A1";

			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_RN_NKBankAccountCountry = "A1";
			bankAccount.AB_AccountNumber = "";
			AssertNoWarnings("No Warnings for IBAN", bankAccount.AB_AccountNumberInfo);
		}
	}
}
