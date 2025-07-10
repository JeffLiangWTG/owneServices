using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccBankAccount))]
	class AccBankAccountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccBankAccount>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestSaveMultipleEpaAccounts()
		{
			var account1 = Factory.NewWithValidTestData<AccBankAccount>();
			account1.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			account1.AB_PaymentProvider = ProviderCodes.OFX;
			account1.AB_AccountNum = "";
			account1.AB_BSB = "";
			AssertNoExceptionThrown(() => Factory.Save());

			var account2 = Factory.NewWithValidTestData<AccBankAccount>();
			account2.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			account2.AB_PaymentProvider = ProviderCodes.OFX;
			account2.AB_AccountNum = "";
			account2.AB_BSB = "";
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestUniqueAccountNumberBSB()
		{
			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount1.AB_AccountNum = "12345678";
			bankAccount1.AB_BSB = "12-34-56";
			AssertNoExceptionThrown(() => Factory.Save());

			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount2.AB_AccountNum = "12345678";
			bankAccount2.AB_BSB = "12-34-56";
			AssertExceptionThrown<ZSaveException>("Account Number + BSB should be unique", () => Factory.Save());
		}

		public void TestIsEPaymentAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			Assert(!bankAccount.IsEPaymentAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			Assert(!bankAccount.IsEPaymentAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
			Assert(!bankAccount.IsEPaymentAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			Assert(bankAccount.IsEPaymentAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			Assert(!bankAccount.IsEPaymentAccount);
		}

		public void TestIsCashAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			Assert(!bankAccount.IsCashAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			Assert(!bankAccount.IsCashAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
			Assert(!bankAccount.IsCashAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			Assert(!bankAccount.IsCashAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			Assert(bankAccount.IsCashAccount);
		}

		public void TestIsCreditCardAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			Assert(!bankAccount.IsCreditCardAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			Assert(bankAccount.IsCreditCardAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
			Assert(!bankAccount.IsCreditCardAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			Assert(!bankAccount.IsCreditCardAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			Assert(!bankAccount.IsCreditCardAccount);
		}

		public void TestIsLinkedAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			Assert(!bankAccount.IsLinkedAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			Assert(!bankAccount.IsLinkedAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
			Assert(bankAccount.IsLinkedAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			Assert(!bankAccount.IsLinkedAccount);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			Assert(!bankAccount.IsLinkedAccount);
		}

		public void TestEPaymentStaffTokenCollection()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var staffToken1 = Factory.New<AccEPaymentStaffToken>();
			staffToken1.TK_AB = bankAccount.PK;
			var staffToken2 = Factory.New<AccEPaymentStaffToken>();
			staffToken2.TK_AB = bankAccount.PK;
			var staffToken3 = Factory.New<AccEPaymentStaffToken>();
			staffToken3.TK_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			AssertEquals("Should be 2 staff tokens in the collection", 2, bankAccount.EPaymentStaffTokenCollection.Count);
			AssertCollectionContains(staffToken1, bankAccount.EPaymentStaffTokenCollection);
			AssertCollectionContains(staffToken2, bankAccount.EPaymentStaffTokenCollection);
		}

		public void TestSetAccountTypeResetPaymentProvider()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = ProviderCodes.OFX;
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			AssertEquals(string.Empty, bankAccount.AB_PaymentProvider);
		}

		public void TestAB_PaymentProvider_ReadOnly()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var codesAndResultsToTest = new Dictionary<string, bool>()
			{
				{ AccountTypeCodeDescriptionPairList.Codes.BNK, true },
				{ AccountTypeCodeDescriptionPairList.Codes.CCD, true },
				{ AccountTypeCodeDescriptionPairList.Codes.EPA, false },
				{ AccountTypeCodeDescriptionPairList.Codes.LNK, true },
				{ AccountTypeCodeDescriptionPairList.Codes.CSH, true }
			};

			foreach (var codeWithResult in codesAndResultsToTest)
			{
				bankAccount.AB_AccountType = codeWithResult.Key;
				AssertEquals("Payment Provider has " + (codeWithResult.Value ? "not" : "") + " to be read-only for '" + codeWithResult.Key + "' Account Type", codeWithResult.Value, bankAccount.AB_PaymentProviderInfo.ReadOnly);
			}
		}

		public void TestReadOnlyFieldsWhenAccountTypeIsCSH()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			Assert(bankAccount.AB_AccountTypeInfo.ReadOnly);
			Assert(bankAccount.AB_BankNameInfo.ReadOnly);
			Assert(bankAccount.AB_BankAbbreviationInfo.ReadOnly);
			Assert(bankAccount.AB_BankAddressInfo.ReadOnly);
			Assert(bankAccount.AB_BSBInfo.ReadOnly);
			Assert(bankAccount.AB_AccountNumInfo.ReadOnly);
			Assert(bankAccount.AB_SWIFTInfo.ReadOnly);
			Assert(bankAccount.IBANInfo.ReadOnly);
			Assert(bankAccount.AB_FullAccountNumberInfo.ReadOnly);
			Assert(bankAccount.AB_PaymentProviderInfo.ReadOnly);
			Assert(bankAccount.AB_SO_ChequeTemplateInfo.ReadOnly);
			Assert(bankAccount.AB_ChequeNumDigitsInfo.ReadOnly);
			Assert(bankAccount.AB_DebitCreditCardNameInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryMonthInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryYearInfo.ReadOnly);
			Assert(bankAccount.AB_AllowAutoDDRInfo.ReadOnly);
			Assert(bankAccount.AB_ShowDetailsOnDirectDebitsInfo.ReadOnly);
			Assert(bankAccount.AB_AutoDDRFormatInfo.ReadOnly);
			Assert(bankAccount.AB_AccountEFTUserIDInfo.ReadOnly);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccBankAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			var balanceList = new List<string>
			{
				nameof(bankAccount.AB_ClosingBalance),
				nameof(bankAccount.AB_OpenBalance)
			};

			var osBalanceList = new List<string>
			{
				nameof(bankAccount.AB_ClosingOSBalance),
				nameof(bankAccount.AB_OpenOSBalance)
			};

			var tester = new DecimalPlacesAttributeTester(bankAccount, bankAccount.Company);
			tester.CheckLocalCurrency(balanceList, nameof(bankAccount.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osBalanceList, nameof(bankAccount.OSCurrencyDecimals), nameof(bankAccount.AB_RX_NKAccountCurrency), bankAccount);
		}

		public void TestGlobalHeaders()
		{
			var globalGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			globalGLHeader.AG_AccountType = "BSH";
			globalGLHeader.AG_ControlAccount = true;
			globalGLHeader.AG_IsGlobal = true;
			var nonGlobalGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			nonGlobalGLHeader1.AG_AccountType = "BSH";
			nonGlobalGLHeader1.AG_ControlAccount = true;
			nonGlobalGLHeader1.AG_IsGlobal = false;
			var nonGlobalGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			nonGlobalGLHeader2.AG_AccountType = "BSH";
			nonGlobalGLHeader2.AG_ControlAccount = true;
			nonGlobalGLHeader2.AG_IsGlobal = false;
			nonGlobalGLHeader2.CompanyFilters.AddNew().ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var result = Factory.Load<AccGLHeader>(bankAccount.GlobalHeaders.CompleteFilter);
			AssertEquals(true, result.Any(x => x.PK == globalGLHeader.PK));
			AssertEquals(false, result.Any(x => x.PK == nonGlobalGLHeader1.PK));
			AssertEquals(true, result.Any(x => x.PK == nonGlobalGLHeader2.PK));
		}

		public void TestNoStmALogs()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, bankAccount.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				bankAccount.AB_Desc = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				bankAccount.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestSettingAB_SO_ChequeTemplateCreatesMissingClientTemplateMenuItems()
		{
			int stmMenuItemCountBeforeAddingCheque = Factory.GetDatabaseCount(typeof(StmMenuItem));
			int stmMenuTemplatePivotCountBeforeAddingCheque = Factory.GetDatabaseCount(typeof(StmMenuTemplatePivot));
			int stmTemplateCountBeforeAddingCheque = Factory.GetDatabaseCount(typeof(StmTemplate));

			StmTemplate clientChequeTemplate = Factory.NewWithValidTestData<StmTemplate>();
			clientChequeTemplate.SO_Name = "Client Cheque Template";
			clientChequeTemplate.SO_DataContext = nameof(Core.Constants.DataContext.Cheques);
			clientChequeTemplate.SO_IsSystemDefined = false;
			clientChequeTemplate.SO_IsClientSpecific = false;

			StmMenuItem clientChequeMenu = Factory.NewWithValidTestData<StmMenuItem>();
			clientChequeMenu.SU_IsSystemDefined = false;
			clientChequeMenu.SU_IsClientSpecific = false;
			clientChequeMenu.SU_MenuName = "Client Cheque Template";
			clientChequeMenu.SU_BusinessContext = nameof(BusinessContext.APTransaction);
			clientChequeMenu.SU_MenuPath = "Check";

			Factory.Save();

			AssertEquals("There should be one new Template", stmTemplateCountBeforeAddingCheque + 1, Factory.GetDatabaseCount(typeof(StmTemplate)));
			AssertEquals("There should be no new Menus", stmMenuItemCountBeforeAddingCheque + 1, Factory.GetDatabaseCount(typeof(StmMenuItem)));
			AssertEquals("There should be no new Pivots", stmMenuTemplatePivotCountBeforeAddingCheque, Factory.GetDatabaseCount(typeof(StmMenuTemplatePivot)));

			BranchABankAccountAUD.AB_SO_ChequeTemplate = clientChequeTemplate.PK;

			AssertEquals("There should be one new Template", stmTemplateCountBeforeAddingCheque + 1, Factory.GetDatabaseCount(typeof(StmTemplate)));
			AssertEquals("There should be 4 new Menus", stmMenuItemCountBeforeAddingCheque + 4, Factory.GetDatabaseCount(typeof(StmMenuItem)));
			AssertEquals("There should be 3 new Pivots", stmMenuTemplatePivotCountBeforeAddingCheque + 3, Factory.GetDatabaseCount(typeof(StmMenuTemplatePivot)));

			StmMenuItem menuItemForCBDirectPayment = StmMenuItem.FindDocumentMenu(Factory, clientChequeTemplate, BusinessContext.CBDirectPayment);
			AssertNotNull("CBDirectPayment Document Menu", menuItemForCBDirectPayment);
			AssertNotNull("CBDirectPayment Document Pivot", StmMenuTemplatePivot.FindDocumentPivot(Factory, clientChequeTemplate, menuItemForCBDirectPayment));

			StmMenuItem menuItemForHotCheque = StmMenuItem.FindDocumentMenu(Factory, clientChequeTemplate, BusinessContext.HotCheque);
			AssertNotNull("HotCheque Document Menu", menuItemForHotCheque);
			AssertNotNull("HotCheque Document Pivot", StmMenuTemplatePivot.FindDocumentPivot(Factory, clientChequeTemplate, menuItemForHotCheque));

			StmMenuItem menuItemForAPTransaction = StmMenuItem.FindDocumentMenu(Factory, clientChequeTemplate, BusinessContext.APTransaction, ZString.Empty);
			AssertNotNull("APTransaction Document Menu", menuItemForAPTransaction);
			AssertNotNull("APTransaction Document Pivot", StmMenuTemplatePivot.FindDocumentPivot(Factory, clientChequeTemplate, menuItemForAPTransaction));

			BranchABankAccountAUD.AB_SO_ChequeTemplate = ZGuid.NewZGuid();

			BranchABankAccountAUD.AB_SO_ChequeTemplate = clientChequeTemplate.PK;

			AssertEquals("There should still be only be one new Template", stmTemplateCountBeforeAddingCheque + 1, Factory.GetDatabaseCount(typeof(StmTemplate)));
			AssertEquals("There should still be only be 3 new Menus", stmMenuItemCountBeforeAddingCheque + 4, Factory.GetDatabaseCount(typeof(StmMenuItem)));
			AssertEquals("There should still be only 3 new Pivots", stmMenuTemplatePivotCountBeforeAddingCheque + 3, Factory.GetDatabaseCount(typeof(StmMenuTemplatePivot)));
		}

		#region Test Objects

		public class MockAccBankAccount : AccBankAccount
		{
			public MockAccBankAccount(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new MockAccBankAccountValidation Validation
			{
				get { return base.Validation as MockAccBankAccountValidation; }
			}

			protected override AccBankAccountValidation GetNewValidation()
			{
				return new MockAccBankAccountValidation(this);
			}

			public AccChequeBook[] ChequeBooks;
		}

		public class MockAccBankAccountValidation : AccBankAccountValidation
		{
			public MockAccBankAccountValidation(MockAccBankAccount parent)
				: base(parent)
			{
			}

			protected override AccChequeBook[] GetAttachedChequeBooks(ZGuid pK)
			{
				return ((MockAccBankAccount)Parent).ChequeBooks;
			}
		}

		#endregion

		#region BankAccountName

		public void TestBankAccountName()
		{
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, account.AB_BankAccountName);

			account.AB_BankAccountName = "XYZ";
			AssertEquals("XYZ", account.AB_BankAccountName);
			AssertNoErrors(account.AB_BankAccountNameInfo);

			account.AB_BankAccountName = "";
			AssertHasErrors(account.AB_BankAccountNameInfo);
		}

		#endregion

		public void TestAB_FullAccountNumber()
		{
			var bank_AU = Factory.NewWithValidTestData<AccBankAccount>();
			bank_AU.AB_RN_NKBankAccountCountry = Core.Constants.CountryCodes.Australia;
			var anyCountryAllowedCodes = new[] { "AUABC12abcXYX1234", "ABC12abc,XyZ", "", null };
			for (var i = 0; i < anyCountryAllowedCodes.Length; i++)
			{
				bank_AU.AB_FullAccountNumber = anyCountryAllowedCodes[i];
				bank_AU.Validation.ValidateAB_FullAccountNumber();
				AssertExpectedValuesWithoutErrors(anyCountryAllowedCodes[i], bank_AU);
				bank_AU.AB_FullAccountNumber = "";
			}

			var bank_AR = Factory.NewWithValidTestData<AccBankAccount>();
			bank_AR.AB_RN_NKBankAccountCountry = Core.Constants.CountryCodes.Argentina;
			var allowedCodesForArgentina = new[] { "1500054100030036346096", "1500054100030036646332", "1910024755002400452700", "0170999920000004920517", "3300041910410184678101", "0720208920000000604572", "" };
			for (var i = 0; i < allowedCodesForArgentina.Length; i++)
			{
				bank_AR.AB_FullAccountNumber = allowedCodesForArgentina[i];
				bank_AR.Validation.ValidateAB_FullAccountNumber();
				AssertExpectedValuesWithoutErrors(allowedCodesForArgentina[i], bank_AR);
				bank_AR.AB_FullAccountNumber = "";
			}

			var incorrectCodesForArgentina = new[] { "1234560", "  Hi  ", ".", "123ABC.DEF456", "abc.123@456", "1500054100a30036346096" };
			var expectedErrorMessage = "Unique account number must be numeric and 22 characters long";
			AssertEquals("Precondition: no notifications for AB_FullAccountNumberInfo", 0, bank_AR.AB_FullAccountNumberInfo.Notifications.Count());
			for (var i = 0; i < incorrectCodesForArgentina.Length; i++)
			{
				bank_AR.AB_FullAccountNumber = incorrectCodesForArgentina[i];
				bank_AR.Validation.ValidateAB_FullAccountNumber();
				AssertEquals("AB_FullAccountNumberInfo property should have 1 Mesage Error", true, bank_AR.AB_FullAccountNumberInfo.Notifications.Any(x => x.Message.Contains(expectedErrorMessage)));
				bank_AR.AB_FullAccountNumber = "";
			}

			void AssertExpectedValuesWithoutErrors(ZString expectedValue, AccBankAccount bankAccountObject)
			{
				AssertEquals(expectedValue, bankAccountObject.AB_FullAccountNumber);
				AssertNoErrors(bankAccountObject.AB_FullAccountNumberInfo);
			}
		}

		public void TestDefaultAB_RN_NKBankAccountCountry()
		{
			var account = Factory.New<AccBankAccount>();
			AssertEquals("Should have default value", GlbCompany.CurrentCompany.Country.Code, account.AB_RN_NKBankAccountCountry);
		}

		#region AB_AccountNumber & IBAN Properties

		public void TestIBANPropertyWrapsAB_AccountNumber()
		{
			var spainBranch = CreateCompanyWithBranch(Core.Constants.CountryCodes.Spain);
			var account = Factory.New<AccBankAccount>();
			account.AB_GC = spainBranch.GB_GC;

			ZString validIBAN = "ES2637011181545485279943";
			account.AB_AccountNumber = validIBAN;
			AssertEquals("Setting AB_AccountNumber should also set IBAN", validIBAN, account.IBAN);
			account.AB_AccountNumber = "";

			account.IBAN = validIBAN;
			AssertEquals("Setting IBAN should also set AB_AccountNumber", validIBAN, account.AB_AccountNumber);
			account.IBAN = "";

			// Note: validation of exact validation message text is done in TestAccBankAccountValidation.TestCheckAB_AccountNumber()
			ZString invalidIBAN = "ES2637011181545485279940";
			AssertEquals("Precondition: no notifications for AB_AccountNumber", 0, account.AB_AccountNumberInfo.Notifications.Count());
			AssertEquals("Precondition: no notifications for IBAN", 0, account.IBANInfo.Notifications.Count());
			account.AB_AccountNumber = invalidIBAN;
			account.Validation.ValidateAB_AccountNumber();
			Assert("Validation rules for AB_AccountNumber should be triggered.", account.AB_AccountNumberInfo.Notifications.Any());
			Assert("Validation rules for AB_AccountNumber should apply to IBAN when set via AB_AccountNumber.", account.IBANInfo.Notifications.Any());
			foreach (var notificationPair in account.AB_AccountNumberInfo.Notifications.Zip(account.IBANInfo.Notifications, (acctNum, iban) => new { acctNum, iban }))
			{
				AssertEquals("Validation type for AB_AccountNumber and IBAN should be identical.", notificationPair.acctNum.Type, notificationPair.iban.Type);
				AssertEquals("Validation message for AB_AccountNumber and IBAN should be identical.", notificationPair.acctNum.Message, notificationPair.iban.Message);
			}
			account.AB_AccountNumber = "";

			// Note: validation of exact validation message text is done in TestAccBankAccountValidation.TestCheckAB_AccountNumber()
			AssertEquals("Precondition: no validation errors for AB_AccountNumber", 0, account.AB_AccountNumberInfo.Notifications.Count());
			AssertEquals("Precondition: no validation errors for IBAN", 0, account.IBANInfo.Notifications.Count());
			account.IBAN = invalidIBAN;
			account.Validation.ValidateAB_AccountNumber();
			Assert("Validation rules for AB_AccountNumber should apply to IBAN when set via IBAN.", account.IBANInfo.Notifications.Any());
			Assert("Validation rules for AB_AccountNumber should apply to AB_AccountNumber when set via IBAN.", account.AB_AccountNumberInfo.Notifications.Any());
			foreach (var notificationPair in account.AB_AccountNumberInfo.Notifications.Zip(account.IBANInfo.Notifications, (acctNum, iban) => new { acctNum, iban }))
			{
				AssertEquals("Validation type for AB_AccountNumber and IBAN should be identical.", notificationPair.acctNum.Type, notificationPair.iban.Type);
				AssertEquals("Validation message for AB_AccountNumber and IBAN should be identical.", notificationPair.acctNum.Message, notificationPair.iban.Message);
			}
			account.IBAN = "";
		}

		#endregion

		#region GetDefaultReceiptBankAccount

		public void TestNoResultFound()
		{
			ClearAllExistingDefaultBankAccounts();
			AssertNull("Should not return any bank account as there are no defaults", AccBankAccount.GetDefaultReceiptBankAccount(CurrencyGBP.RX_Code, BranchC, Factory));
		}

		public void TestFindAccountForSpecificOrganisation()
		{
			ClearAllExistingDefaultBankAccounts();
			NoBranchBankAccountAUD.AB_IsDefaultReceiptBankAccount = true;

			Factory.Save();

			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();
			bank2.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank2.AB_Code = "Bank2";

			AccBankAccount bank3 = Factory.NewWithValidTestData<AccBankAccount>();
			bank3.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank3.AB_Code = "Bank3";

			AccBankAccount bank4 = Factory.NewWithValidTestData<AccBankAccount>();
			bank4.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank4.AB_Code = "Bank4";

			AccBankAccount bank5 = Factory.NewWithValidTestData<AccBankAccount>();
			bank5.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank5.AB_Code = "Bank5";

			OrgDebtorGroup group = Factory.New<OrgDebtorGroup>();
			group.OJ_Code = "XXX";
			group.OJ_Desc = "XXX";

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CompanyData.OB_OJ_ARDebtorGroup = group.PK;

			Factory.Save();

			AccBankAccount result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("'Bank To' Bank Accounts not specified: Invoice receipt bank should be the default bank ", NoBranchBankAccountAUD.AB_Code, result.AB_Code);

			group.DefaultBankAccountPK = bank2.PK;

			bank2.AB_IsActive = false;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Orgs Debtor Group 'Bank To' Bank Account is specified but is not active: Invoice receipt bank should be this", NoBranchBankAccountAUD.AB_Code, result.AB_Code);

			bank2.AB_IsActive = true;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Orgs Debtor Group 'Bank To' Bank Account is specified: Invoice receipt bank should be this", bank2.AB_Code, result.AB_Code);

			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
			collection.AddNew();
			collection[0].Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			collection[0].BankAccount = bank3.PK;
			OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			bank3.AB_IsActive = false;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Currency to Bank Account settings is specified in registry but is not active: Invoice receipt bank should be this", bank2.AB_Code, result.AB_Code);

			bank3.AB_IsActive = true;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Currency to Bank Account settings is specified in registry: Invoice receipt bank should be this", bank3.AB_Code, result.AB_Code);

			group.OverrideRegistryCurrencyToBankSetting = true;
			group.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_AB = bank4.PK;

			bank4.AB_IsActive = false;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Currency to Bank Account settings is overriden in Debtor Group: Invoice receipt bank should be this", bank3.AB_Code, result.AB_Code);

			bank4.AB_IsActive = true;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Currency to Bank Account settings is overriden in Debtor Group: Invoice receipt bank should be this", bank4.AB_Code, result.AB_Code);

			organisation.CompanyData.OB_AB_ARPayToAccount = bank5.PK;

			bank5.AB_IsActive = false;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Organisations 'Bank To' Bank Account is specified: Invoice receipt bank should be this", bank4.AB_Code, result.AB_Code);

			bank5.AB_IsActive = true;
			Factory.Save();

			result = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
			AssertEquals("Organisations 'Bank To' Bank Account is specified: Invoice receipt bank should be this", bank5.AB_Code, result.AB_Code);
		}

		public void TestFindAccountForSpecificCurrencyAndSpecificBranch()
		{
			AccBankAccount actualBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(CurrencyUSD.RX_Code, BranchA, Factory);
			AssertEquals("Should find Bank Account for foreign currency and specific branch", BranchABankAccountUSD.PK, actualBankAccount.PK);
		}

		public void TestFindAccountForSpecificCurrencyAndCompany()
		{
			AccBankAccount actualBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(CurrencyUSD.RX_Code, BranchB, Factory);
			AssertEquals("Should find Bank Account for foreign currency and company", NoBranchBankAccountUSD.PK, actualBankAccount.PK);
		}

		public void TestFindAccountForLocalCurrencyWithNonExistentForeignCurrencyAndSpecificBranch()
		{
			AccBankAccount actualBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(CurrencyGBP.RX_Code, BranchA, Factory);
			AssertEquals("Should find Bank Account for local currency when foreign currency with no default receipt bank account and specific branch is passed",
				BranchABankAccountAUD.PK, actualBankAccount.PK);
		}

		public void TestFindAccountForLocalCurrencyAndCompany()
		{
			AccBankAccount actualBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(CurrencyEUR.RX_Code, BranchB, Factory);
			AssertEquals("Should find Account for local Currency and company", NoBranchBankAccountAUD.PK, actualBankAccount.PK);
		}

		public void TestFindAccountForLocalCurrencyWithNonExistentForeignCurrencyAndNullBranch()
		{
			AccBankAccount actualBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(CurrencyGBP.RX_Code, null, Factory);
			AssertEquals("Should find Bank Account for local currency when foreign currency with no default receipt bank account and null branch is passed",
				NoBranchBankAccountAUD.PK, actualBankAccount.PK);
		}

		public void TestUseCurrentCompanyBankAccountsOnly()
		{
			ClearAllExistingDefaultBankAccounts();
			GlbCompany demoCompany = (GlbCompany)Factory.Load(typeof(GlbCompany), new ZQuery(GlbCompanySchema.GC_Code, "DEM"))[0];
			AccBankAccount nonCurrentCompanyBankAccount = Factory.New<AccBankAccount>();
			SetupBankAccount(nonCurrentCompanyBankAccount, null, LocalCurrency);
			nonCurrentCompanyBankAccount.AB_GC = demoCompany.PK;

			AssertNull("Should not find bank account for company other than current", AccBankAccount.GetDefaultReceiptBankAccount(LocalCurrency.RX_Code, BranchA, Factory));
		}

		#endregion

		#region Lookups

		public void TestTemplateList()
		{
			AccBankAccount testBankAccount = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;
			testBankAccount.ChequeTemplates.Load();

			int originalCount = testBankAccount.ChequeTemplates.Count;

			StmTemplate template1 = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;
			StmTemplate template2 = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;

			template1.SO_DataContext = "Cheques";
			template2.SO_DataContext = "SomethingElse";

			Assert("Precondition: Does not contain Template 1", !testBankAccount.ChequeTemplates.Contains(template1));

			testBankAccount.ChequeTemplates.Load();

			AssertEquals(1, testBankAccount.ChequeTemplates.Count - originalCount);
			Assert("Contains Template 1", testBankAccount.ChequeTemplates.Contains(template1));
			Assert("Does not contain Template 2", !testBankAccount.ChequeTemplates.Contains(template2));
		}

		public void TestAutoDDRFormatList()
		{
			AccBankAccount bank = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;
			Assert("DDR Format list should contain ASB", bank.AB_AutoDDRFormat_List.ContainsCode(Constants.DDRFileFormat.ASB));
			Assert("DDR Format list should contain BBL", bank.AB_AutoDDRFormat_List.ContainsCode(Constants.DDRFileFormat.BBL));
			Assert("DDR Format list should contain BCS", bank.AB_AutoDDRFormat_List.ContainsCode(Constants.DDRFileFormat.BCS));
			Assert("DDR Format list should contain WNZ", bank.AB_AutoDDRFormat_List.ContainsCode(Constants.DDRFileFormat.WNZ));
		}

		#endregion

		#region CompanyCurrency

		//[ToDo("Deb", "10-03-04", "Fix with Imraan later")]
		public void TestCompanyCurrency()
		{
			Assert("Testing Current company's currency", BranchABankAccountUSD.CompanyCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
		}

		#endregion

		#region IDocManagerSupport

		public void TestDocManagerCode()
		{
			AssertEquals("Code should be BAC. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "BAC", ((IDocManagerSupport)BranchABankAccountUSD).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Utility Methods Test

		public void TestIsValidBSBNumber()
		{
			BranchABankAccountUSD.AB_AllowAutoDDR = true;

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			Assert(BranchABankAccountUSD.IsValidBSBNumber("123-345"));
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("123345"));

			GlbBranch newBranch = SetupNewNewZelandCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert(BranchABankAccountUSD.IsValidBSBNumber("123345"));
				Assert(!BranchABankAccountUSD.IsValidBSBNumber("123-345"));
				Assert(BranchABankAccountUSD.IsValidBSBNumber("123456"));
			}

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.NAB;
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("123-3456"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BNZ;
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("123-345"));
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("1233456"));
			Assert(BranchABankAccountUSD.IsValidBSBNumber("123345"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("1233456"));
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("123-456"));
			Assert(BranchABankAccountUSD.IsValidBSBNumber("889083"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BBL;
			Assert(BranchABankAccountUSD.IsValidBSBNumber("583-309"));
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("579909"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.WNZ;
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("389-590"));
			Assert(BranchABankAccountUSD.IsValidBSBNumber("389590"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BCS;
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("123-345"));
			Assert(!BranchABankAccountUSD.IsValidBSBNumber("1233456"));
			Assert(BranchABankAccountUSD.IsValidBSBNumber("123345"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			Assert(BranchABankAccountUSD.IsValidBSBNumber("123345333333"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BTM;
			Assert(BranchABankAccountUSD.IsValidBSBNumber("123345333333"));
		}

		public void TestIsValidAccountNumber()
		{
			BranchABankAccountUSD.AB_AllowAutoDDR = true;
			Assert(!BranchABankAccountUSD.IsValidAccountNumber("somethinglongerthan9characters"));
			Assert(!BranchABankAccountUSD.IsValidAccountNumber("1234567890"));
			Assert(BranchABankAccountUSD.IsValidAccountNumber("123456789"));
			Assert(BranchABankAccountUSD.IsValidAccountNumber("123456"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			Assert(!BranchABankAccountUSD.IsValidAccountNumber("123456"));
			Assert(BranchABankAccountUSD.IsValidAccountNumber("987654321"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			Assert(BranchABankAccountUSD.IsValidAccountNumber("9876543213233232322232"));

			BranchABankAccountUSD.AB_AutoDDRFormat = Constants.DDRFileFormat.BTM;
			Assert(BranchABankAccountUSD.IsValidAccountNumber("9876543213233232322232"));
		}

		public void TestHasChequeNumberBeenUsedOnAnotherTransactionAndNotYetSaved()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			header.AH_AB = BranchABankAccountAUD.PK;
			header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			header.AH_ChequeOrReference = "123456";

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAnotherTransactionAndNotYetSaved("123455", ZGuid.Empty));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAnotherTransactionAndNotYetSaved("123456", ZGuid.Empty));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAnotherTransactionAndNotYetSaved("123456", header.PK));

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123455", new List<ZGuid>() { ZGuid.Empty }));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { ZGuid.Empty }));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { header.PK }));
		}

		public void TestHasChequeNumberBeenUsedWhenUsedByAPaymentApproval()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			AccPaymentApproval paymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval.AV_AB = BranchABankAccountAUD.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			paymentApproval.AV_ChequeOrReference = "123456";
			Factory.Save();

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123455", ZGuid.NewZGuid(), ZGuid.NewZGuid()));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123455", paymentApproval.PK, ZGuid.NewZGuid()));

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123456", paymentApproval.PK, ZGuid.NewZGuid()));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123456", ZGuid.Empty, ZGuid.Empty));

			paymentApproval.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123456", ZGuid.Empty, ZGuid.Empty));

			paymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsed("123455", ZGuid.Empty));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsed("123456", ZGuid.Empty));

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123455", new List<ZGuid>() { ZGuid.Empty }));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { ZGuid.Empty }));
		}

		public void TestHasChequeNumberBeenUsedWhenUsedByPaymentApprovals()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			AccPaymentApproval paymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval.AV_AB = BranchABankAccountAUD.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			paymentApproval.AV_ChequeOrReference = "123450";

			AccPaymentApproval paymentApproval2 = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval2.AV_AB = BranchABankAccountAUD.PK;
			paymentApproval2.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval2.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			paymentApproval2.AV_ChequeOrReference = "123451";

			AccPaymentApproval paymentApproval3 = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval3.AV_AB = BranchABankAccountAUD.PK;
			paymentApproval3.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval3.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			paymentApproval3.AV_ChequeOrReference = "123452";
			Factory.Save();

			List<ZGuid> paymentApprovalsToExclude = new List<ZGuid>();

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123449", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123453", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123450", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123451", paymentApprovalsToExclude));

			paymentApprovalsToExclude.Add(paymentApproval.PK);
			paymentApprovalsToExclude.Add(paymentApproval2.PK);
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123450", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123451", paymentApprovalsToExclude));

			paymentApproval.AV_Status = PaymentApprovalStatus.Posted;
			paymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();

			paymentApprovalsToExclude = new List<ZGuid>();
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123450", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123451", paymentApprovalsToExclude));

			paymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			paymentApproval2.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123449", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123453", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123450", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123451", paymentApprovalsToExclude));

			paymentApprovalsToExclude.Add(paymentApproval.PK);
			paymentApprovalsToExclude.Add(paymentApproval2.PK);
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123450", paymentApprovalsToExclude));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAPaymentApproval("123451", paymentApprovalsToExclude));
		}

		public void TestHasChequeNumberBeenUsedWhenUsedByAJobCharge()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AB = BranchABankAccountAUD.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "123456";
			Factory.Save();

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsed("123455", ZGuid.Empty));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsed("123456", ZGuid.Empty));

			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsed("123456", ZGuid.Empty, ZGuid.Empty));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsed("123456", ZGuid.Empty, charge.PK));

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123455", new List<ZGuid>() { ZGuid.Empty }));
			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { ZGuid.Empty }));
		}

		public void TestHasChequeNumberBeenUsedOnAJobCharge()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AB = BranchABankAccountAUD.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "123456";
			charge.JR_E6 = ZGuid.NewZGuid();

			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_AB = BranchABankAccountAUD.PK;
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_ChequeNo = "123457";
			charge2.JR_E6 = ZGuid.NewZGuid();

			List<ZGuid> jobConsolCostsPKToExclude = new List<ZGuid>();

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAJobCharge("123455", jobConsolCostsPKToExclude));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAJobCharge("123456", jobConsolCostsPKToExclude));

			jobConsolCostsPKToExclude = new List<ZGuid> { charge.JR_E6, charge2.JR_E6 };

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnAJobCharge("123456", jobConsolCostsPKToExclude));
		}

		public void TestHasChequeNumberBeenUsedOnPostedTransactions()
		{
			var header1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header1.AH_Ledger = LedgerTypes.AccountsPayable;
			header1.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			header1.AH_AB = BranchABankAccountAUD.PK;
			header1.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			header1.AH_ChequeOrReference = "123456";

			var header2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header2.AH_Ledger = LedgerTypes.CashBook;
			header2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			header2.AH_AB = BranchABankAccountAUD.PK;
			header2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			header2.AH_ChequeOrReference = "123456";
			Factory.Save();

			AssertEquals(false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { header1.PK, header2.PK }));
			AssertEquals(true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { header1.PK }));
			AssertEquals(true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { header2.PK }));
		}

		public void TestHasChequeNumberBeenUsed()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.CashBook;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			header.AH_AB = BranchABankAccountAUD.PK;
			header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			header.AH_ChequeOrReference = "123456";
			Factory.Save();

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsed("123455", ZGuid.Empty));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsed("123456", ZGuid.Empty));

			AssertEquals("Cheque number in use", false, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123455", new List<ZGuid>() { ZGuid.Empty }));
			AssertEquals("Cheque number in use", true, BranchABankAccountAUD.HasChequeNumberBeenUsedOnPostedTransactions("123456", new List<ZGuid>() { ZGuid.Empty }));
		}

		public void TestGetTheFirstUsedChequeNumber()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.CashBook;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			header.AH_AB = BranchABankAccountAUD.PK;
			header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			header.AH_ChequeOrReference = "123456";

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AB = BranchABankAccountAUD.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "123460";

			AccPaymentApproval paymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval.AV_AB = BranchABankAccountAUD.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			paymentApproval.AV_ChequeOrReference = "123450";
			paymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			Factory.Save();

			AssertEquals("Cheque number in use", "123456", BranchABankAccountAUD.GetTheFirstUsedChequeNumber("123455", "123456", "123457", "123457"));
			AssertEquals("Cheque number in use", ZString.Empty, BranchABankAccountAUD.GetTheFirstUsedChequeNumber("123455", "123457", "123457"));
			AssertEquals("Cheque number in use", "123460", BranchABankAccountAUD.GetTheFirstUsedChequeNumber("123455", "123457", "123460"));
			AssertEquals("Cheque number in use", "123450", BranchABankAccountAUD.GetTheFirstUsedChequeNumber("123450", "123457", "123459"));
		}

		public void TestIsChequeNumberUsedByCancelledPayment()
		{
			BranchABankAccountAUD.AB_ChequeNumDigits = 6;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.CashBook;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			header.AH_AB = BranchABankAccountAUD.PK;
			header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			header.AH_IsCancelled = true;
			header.AH_ChequeOrReference = "123456";
			Factory.Save();

			AssertEquals("Cancelled cheque number", true, BranchABankAccountAUD.IsChequeNumberUsedByCancelledPayment("123456"));
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(AccBankAccount)));
		}

		#endregion

		#region TestDecimals

		public void TestLocalCurrencyDecimals()
		{
			ZInt defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				AssertEquals("Decimals should be 0", 0, BranchABankAccountAUD.LocalCurrencyDecimals);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				AssertEquals("Decimals should be 2", 2, BranchABankAccountAUD.LocalCurrencyDecimals);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		public void TestOSCurrencyDecimals()
		{
			AccBankAccount account = (AccBankAccount)GetNewBusinessObject();
			account.AB_RX_NKAccountCurrency = string.Empty;

			AssertNull("Pre-condition: AccountCurrency should be null", account.AccountCurrency);
			AssertEquals("Decimals should be local currency decimals", GlbCompany.CurrentCompany.LocalCurrency.Decimals, account.OSCurrencyDecimals);

			account.AB_RX_NKAccountCurrency = "IDR";
			AssertNotNull("Account currency as assigned", account.AccountCurrency);
			AssertEquals("Decimals should be assigned currency decimals", RefCurrency.LoadFromCurrencyCode(Factory, "IDR").Decimals, account.OSCurrencyDecimals);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BranchA = Factory.New<GlbBranch>();
			BranchB = Factory.New<GlbBranch>();
			BranchC = Factory.New<GlbBranch>();

			BranchA.GB_Code = "ABC";
			BranchB.GB_Code = "DEF";
			BranchC.GB_Code = "GHI";

			SetupBranch(BranchA);
			SetupBranch(BranchB);
			SetupBranch(BranchC);

			BranchABankAccountUSD = Factory.New<MockAccBankAccount>();
			BranchABankAccountAUD = Factory.New<AccBankAccount>();
			BranchBBankAccountGBP = Factory.New<AccBankAccount>();
			NoBranchBankAccountAUD = Factory.New<AccBankAccount>();
			NoBranchBankAccountUSD = Factory.New<AccBankAccount>();

			LocalCurrency = Factory.Load<RefCurrency>(Env.CurrentCompany.LocalCurrency.PK);
			CurrencyUSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			CurrencyGBP = RefCurrency.LoadFromCurrencyCode(Factory, "GBP");
			CurrencyEUR = RefCurrency.LoadFromCurrencyCode(Factory, "EUR");

			ClearAllExistingDefaultBankAccounts();

			do
			{
				SetupBankAccount(BranchABankAccountUSD, BranchA, CurrencyUSD);
				SetupBankAccount(BranchABankAccountAUD, BranchA, LocalCurrency);
				SetupBankAccount(BranchBBankAccountGBP, BranchB, CurrencyGBP);
				SetupBankAccount(NoBranchBankAccountAUD, null, LocalCurrency);
				SetupBankAccount(NoBranchBankAccountUSD, null, CurrencyUSD);
			}
			while (CheckIfEqualPairExists());

			SetupGLHeaders();

			Factory.Save();
		}

		// checks if there are two Bank accounts with the same AB_Code in the test data
		protected bool CheckIfEqualPairExists()
		{
			AccBankAccount[] testAccounts = { BranchABankAccountUSD, BranchABankAccountAUD, BranchBBankAccountGBP, NoBranchBankAccountAUD, NoBranchBankAccountUSD };
			for (int i = 0; i < 5; i++)
			{
				for (int j = i + 1; j < 5; j++)
				{
					if (testAccounts[i].AB_Code == testAccounts[j].AB_Code)
					{
						return true;
					}
				}
			}
			return false;
		}

		//protected BusinessObjectFactory Factory;

		protected GlbBranch BranchA;
		protected GlbBranch BranchB;
		protected GlbBranch BranchC;

		protected AccGLHeader GLHeader1;
		protected AccGLHeader GLHeader2;
		protected AccGLHeader GLHeader3;
		protected AccGLHeader GLHeader4;
		protected AccGLHeader GLHeader5;
		protected AccGLHeader GLHeader6;

		protected MockAccBankAccount BranchABankAccountUSD;
		protected AccBankAccount BranchABankAccountAUD;
		protected AccBankAccount BranchBBankAccountGBP;
		protected AccBankAccount NoBranchBankAccountAUD;
		protected AccBankAccount NoBranchBankAccountUSD;

		protected RefCurrency LocalCurrency;
		protected RefCurrency CurrencyUSD;
		protected RefCurrency CurrencyGBP;
		protected RefCurrency CurrencyEUR;

		protected void SetupBranch(GlbBranch branch)
		{
			branch.GB_GC = Env.CurrentCompany.PK;
		}

		protected void SetupBankAccount(AccBankAccount bankAccount, GlbBranch branch, RefCurrency currency)
		{
			bankAccount.AB_AccountNum = GetRandomString(10);
			bankAccount.AB_BSB = GetRandomString(6);
			bankAccount.AB_Code = GetRandomString(3);
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
			GLHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader4 = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader5 = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader6 = Factory.NewWithValidTestData<AccGLHeader>();

			GLHeader1.AG_AccountNum = "1234567890";
			GLHeader2.AG_AccountNum = "2345678901";
			GLHeader3.AG_AccountNum = "3456789012";
			GLHeader4.AG_AccountNum = "4567890123";
			GLHeader5.AG_AccountNum = "5678901234";
			GLHeader6.AG_AccountNum = "6789012345";

			BranchABankAccountUSD.AB_AG = GLHeader1.PK;
			BranchABankAccountAUD.AB_AG = GLHeader2.PK;
			BranchBBankAccountGBP.AB_AG = GLHeader3.PK;
			NoBranchBankAccountAUD.AB_AG = GLHeader4.PK;
			NoBranchBankAccountUSD.AB_AG = GLHeader5.PK;
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

		protected static Random Generator
		{
			get { return generator ?? (generator = new Random()); }
		}
		[ThreadStatic]
		static Random generator;

		protected internal static string GetRandomString(int stringLength)
		{
			string result = "";
			for (int i = 0; i < stringLength; i++)
			{
				int index = Generator.Next(65, 90);
				result += (char)index;
			}
			return result;
		}

		GlbBranch SetupNewNewZelandCompanyAndBranch() => CreateCompanyWithBranch(Core.Constants.CountryCodes.NewZealand);

		GlbBranch CreateCompanyWithBranch(ZString companyCountryCode)
		{
			var ptCompany = Factory.NewWithValidTestData<GlbCompany>();
			ptCompany.GC_RN_NKCountryCode = companyCountryCode;
			ptCompany.GC_IsGSTRegistered = false;

			var ptBranch = Factory.NewWithValidTestData<GlbBranch>();
			ptBranch.GB_GC = ptCompany.PK;

			Factory.Save();

			return ptBranch;
		}

		#endregion

		#region Concurrency Policy

		public void TestSettingConcurrencyPolicy()
		{
			var account = Factory.New<AccBankAccount>();
			AssertEquals("Strict for LastReconciledDate", ConcurrencyPolicy.Strict, account.AB_LastReconcileDateInfo.ConcurrencyPolicy);
			AssertEquals("Strict for LastStatementDate", ConcurrencyPolicy.Strict, account.AB_LastStatementDateInfo.ConcurrencyPolicy);
			AssertEquals("Strict for StatementBalance", ConcurrencyPolicy.Strict, account.AB_StatementBalanceInfo.ConcurrencyPolicy);
		}

		#endregion

		public void TestReadOnlyProperties_CSH()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			Assert(bankAccount.AB_BankNameInfo.ReadOnly);
			Assert(bankAccount.AB_BankAddressInfo.ReadOnly);
			Assert(bankAccount.AB_AccountTypeInfo.ReadOnly);
			Assert(bankAccount.AB_PaymentProviderInfo.ReadOnly);
			Assert(bankAccount.AB_BankAbbreviationInfo.ReadOnly);
			Assert(bankAccount.AB_BSBInfo.ReadOnly);
			Assert(bankAccount.AB_SWIFTInfo.ReadOnly);
			Assert(bankAccount.AB_AccountNumberInfo.ReadOnly);
			Assert(bankAccount.AB_FullAccountNumberInfo.ReadOnly);
			Assert(bankAccount.AB_SO_ChequeTemplateInfo.ReadOnly);
			Assert(bankAccount.AB_ChequeNumDigitsInfo.ReadOnly);
			Assert(bankAccount.AB_AllowAutoDDRInfo.ReadOnly);
			Assert(bankAccount.AB_ShowDetailsOnDirectDebitsInfo.ReadOnly);
			Assert(bankAccount.AB_AccountEFTUserIDInfo.ReadOnly);
			Assert(bankAccount.IBANInfo.ReadOnly);
			Assert(bankAccount.AB_DebitCreditCardNameInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryMonthInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryYearInfo.ReadOnly);
			Assert(bankAccount.AB_AccountNumInfo.ReadOnly);

			Assert(!bankAccount.AB_RN_NKBankAccountCountryInfo.ReadOnly);
			Assert(!bankAccount.AB_RX_NKAccountCurrencyInfo.ReadOnly);
			Assert(!bankAccount.AB_DetailedDepositSlipInfo.ReadOnly);
			Assert(!bankAccount.AB_IsDefaultReceiptBankAccountInfo.ReadOnly);
		}

		public void TestReadOnlyProperties_EPA()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;

			Assert(bankAccount.AB_RN_NKBankAccountCountryInfo.ReadOnly);
			Assert(bankAccount.AB_RX_NKAccountCurrencyInfo.ReadOnly);
			Assert(bankAccount.AB_DetailedDepositSlipInfo.ReadOnly);
			Assert(bankAccount.AB_IsDefaultReceiptBankAccountInfo.ReadOnly);
			Assert(bankAccount.AB_BankAbbreviationInfo.ReadOnly);
			Assert(bankAccount.AB_BSBInfo.ReadOnly);
			Assert(bankAccount.AB_SWIFTInfo.ReadOnly);
			Assert(bankAccount.AB_AccountNumberInfo.ReadOnly);
			Assert(bankAccount.AB_FullAccountNumberInfo.ReadOnly);
			Assert(bankAccount.AB_SO_ChequeTemplateInfo.ReadOnly);
			Assert(bankAccount.AB_ChequeNumDigitsInfo.ReadOnly);
			Assert(bankAccount.AB_AllowAutoDDRInfo.ReadOnly);
			Assert(bankAccount.AB_ShowDetailsOnDirectDebitsInfo.ReadOnly);
			Assert(bankAccount.AB_AccountEFTUserIDInfo.ReadOnly);
			Assert(bankAccount.IBANInfo.ReadOnly);
			Assert(bankAccount.AB_DebitCreditCardNameInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryMonthInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryYearInfo.ReadOnly);
			Assert(bankAccount.AB_AccountNumInfo.ReadOnly);

			Assert(!bankAccount.AB_BankNameInfo.ReadOnly);
			Assert(!bankAccount.AB_BankAddressInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountTypeInfo.ReadOnly);
			Assert(!bankAccount.AB_PaymentProviderInfo.ReadOnly);
		}

		public void TestReadOnlyProperties_BNK()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;

			Assert(bankAccount.AB_PaymentProviderInfo.ReadOnly);
			Assert(bankAccount.AB_DebitCreditCardNameInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryMonthInfo.ReadOnly);
			Assert(bankAccount.CreditCardExpiryYearInfo.ReadOnly);

			Assert(!bankAccount.AB_BankNameInfo.ReadOnly);
			Assert(!bankAccount.AB_BankAddressInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountTypeInfo.ReadOnly);
			Assert(!bankAccount.AB_RN_NKBankAccountCountryInfo.ReadOnly);
			Assert(!bankAccount.AB_RX_NKAccountCurrencyInfo.ReadOnly);
			Assert(!bankAccount.AB_DetailedDepositSlipInfo.ReadOnly);
			Assert(!bankAccount.AB_IsDefaultReceiptBankAccountInfo.ReadOnly);
			Assert(!bankAccount.AB_BankAbbreviationInfo.ReadOnly);
			Assert(!bankAccount.AB_BSBInfo.ReadOnly);
			Assert(!bankAccount.AB_SWIFTInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountNumberInfo.ReadOnly);
			Assert(!bankAccount.AB_FullAccountNumberInfo.ReadOnly);
			Assert(!bankAccount.AB_SO_ChequeTemplateInfo.ReadOnly);
			Assert(!bankAccount.AB_ChequeNumDigitsInfo.ReadOnly);
			Assert(!bankAccount.AB_AllowAutoDDRInfo.ReadOnly);
			Assert(!bankAccount.AB_ShowDetailsOnDirectDebitsInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountEFTUserIDInfo.ReadOnly);
			Assert(!bankAccount.IBANInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountNumInfo.ReadOnly);
		}

		public void TestReadOnlyProperties_CCD()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;

			Assert(bankAccount.AB_PaymentProviderInfo.ReadOnly);
			Assert(bankAccount.AB_AccountNumInfo.ReadOnly);

			Assert(!bankAccount.AB_BankNameInfo.ReadOnly);
			Assert(!bankAccount.AB_BankAddressInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountTypeInfo.ReadOnly);
			Assert(!bankAccount.AB_RN_NKBankAccountCountryInfo.ReadOnly);
			Assert(!bankAccount.AB_RX_NKAccountCurrencyInfo.ReadOnly);
			Assert(!bankAccount.AB_DetailedDepositSlipInfo.ReadOnly);
			Assert(!bankAccount.AB_IsDefaultReceiptBankAccountInfo.ReadOnly);
			Assert(!bankAccount.AB_BankAbbreviationInfo.ReadOnly);
			Assert(!bankAccount.AB_BSBInfo.ReadOnly);
			Assert(!bankAccount.AB_SWIFTInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountNumberInfo.ReadOnly);
			Assert(!bankAccount.AB_FullAccountNumberInfo.ReadOnly);
			Assert(!bankAccount.AB_SO_ChequeTemplateInfo.ReadOnly);
			Assert(!bankAccount.AB_ChequeNumDigitsInfo.ReadOnly);
			Assert(!bankAccount.AB_AllowAutoDDRInfo.ReadOnly);
			Assert(!bankAccount.AB_ShowDetailsOnDirectDebitsInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountEFTUserIDInfo.ReadOnly);
			Assert(!bankAccount.IBANInfo.ReadOnly);
			Assert(!bankAccount.AB_DebitCreditCardNameInfo.ReadOnly);
			Assert(!bankAccount.CreditCardExpiryMonthInfo.ReadOnly);
			Assert(!bankAccount.CreditCardExpiryYearInfo.ReadOnly);
		}

		public void TestReadOnlyProperties_LNK()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;

			Assert(bankAccount.AB_PaymentProviderInfo.ReadOnly);

			Assert(!bankAccount.AB_BankNameInfo.ReadOnly);
			Assert(!bankAccount.AB_BankAddressInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountTypeInfo.ReadOnly);
			Assert(!bankAccount.AB_RN_NKBankAccountCountryInfo.ReadOnly);
			Assert(!bankAccount.AB_RX_NKAccountCurrencyInfo.ReadOnly);
			Assert(!bankAccount.AB_DetailedDepositSlipInfo.ReadOnly);
			Assert(!bankAccount.AB_IsDefaultReceiptBankAccountInfo.ReadOnly);
			Assert(!bankAccount.AB_BankAbbreviationInfo.ReadOnly);
			Assert(!bankAccount.AB_BSBInfo.ReadOnly);
			Assert(!bankAccount.AB_SWIFTInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountNumberInfo.ReadOnly);
			Assert(!bankAccount.AB_FullAccountNumberInfo.ReadOnly);
			Assert(!bankAccount.AB_SO_ChequeTemplateInfo.ReadOnly);
			Assert(!bankAccount.AB_ChequeNumDigitsInfo.ReadOnly);
			Assert(!bankAccount.AB_AllowAutoDDRInfo.ReadOnly);
			Assert(!bankAccount.AB_ShowDetailsOnDirectDebitsInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountEFTUserIDInfo.ReadOnly);
			Assert(!bankAccount.IBANInfo.ReadOnly);
			Assert(!bankAccount.AB_DebitCreditCardNameInfo.ReadOnly);
			Assert(!bankAccount.CreditCardExpiryMonthInfo.ReadOnly);
			Assert(!bankAccount.CreditCardExpiryYearInfo.ReadOnly);
			Assert(!bankAccount.AB_AccountNumInfo.ReadOnly);
		}

		public void TestReadOnly_AB_AutoDDRFormat()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;

			Assert(!bankAccount.AB_AllowAutoDDR);
			Assert(bankAccount.AB_AutoDDRFormatInfo.ReadOnly);
			bankAccount.AB_AllowAutoDDR = true;
			Assert(!bankAccount.AB_AutoDDRFormatInfo.ReadOnly);
		}

		public void TestSetAccountTypeDefaults_CSH()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			FillAllBankAccountFields(bankAccount, AccountTypeCodeDescriptionPairList.Codes.BNK);

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			AssertEquals(AccountTypeCodeDescriptionPairList.Codes.CSH, bankAccount.AB_AccountType);

			//these fields were reset
			AssertEquals(string.Empty, bankAccount.AB_BankName);
			AssertEquals(string.Empty, bankAccount.AB_BankAddress);
			AssertEquals(string.Empty, bankAccount.AB_PaymentProvider);
			AssertEquals(string.Empty, bankAccount.AB_BankAbbreviation);
			AssertEquals(string.Empty, bankAccount.AB_BSB);
			AssertEquals(string.Empty, bankAccount.AB_AccountNum);
			AssertEquals(string.Empty, bankAccount.AB_SWIFT);
			AssertEquals(string.Empty, bankAccount.AB_AccountNumber);
			AssertEquals(string.Empty, bankAccount.IBAN);
			AssertEquals(string.Empty, bankAccount.AB_FullAccountNumber);
			AssertEquals(string.Empty, bankAccount.AB_AutoDDRFormat);
			AssertEquals(string.Empty, bankAccount.AB_AccountEFTUserID);
			AssertEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
			AssertEquals((ZByte)1, bankAccount.AB_ChequeNumDigits);
			AssertEquals(false, bankAccount.AB_AllowAutoDDR);
			AssertEquals(false, bankAccount.AB_ShowDetailsOnDirectDebits);
			AssertEquals(string.Empty, bankAccount.AB_DebitCreditCardName);
			AssertEquals(string.Empty, bankAccount.AB_DebitCreditCardExpiry);
			AssertEquals(string.Empty, bankAccount.CreditCardExpiryMonth);
			AssertEquals(string.Empty, bankAccount.CreditCardExpiryYear);
			AssertEquals("AUD", bankAccount.AB_RX_NKAccountCurrency);
			AssertEquals("AU", bankAccount.AB_RN_NKBankAccountCountry);
			AssertEquals(false, bankAccount.AB_DetailedDepositSlip);
			AssertEquals(false, bankAccount.AB_IsDefaultReceiptBankAccount);
		}

		public void TestSetAccountTypeDefaults_BNK()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			FillAllBankAccountFields(bankAccount, AccountTypeCodeDescriptionPairList.Codes.CSH);

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			AssertEquals(AccountTypeCodeDescriptionPairList.Codes.BNK, bankAccount.AB_AccountType);

			//these fields were reset
			AssertEquals(string.Empty, bankAccount.AB_AccountNum);
			AssertEquals(string.Empty, bankAccount.AB_PaymentProvider);
			AssertEquals(string.Empty, bankAccount.AB_DebitCreditCardName);
			AssertEquals(string.Empty, bankAccount.AB_DebitCreditCardExpiry);
			AssertEquals(string.Empty, bankAccount.CreditCardExpiryMonth);
			AssertEquals(string.Empty, bankAccount.CreditCardExpiryYear);

			//these fields are not changed
			AssertEquals("ROBBERY BANK", bankAccount.AB_BankName);
			AssertEquals("ROBBERY TOWN", bankAccount.AB_BankAddress);
			AssertEquals("AAA", bankAccount.AB_BankAbbreviation);
			AssertEquals("111-222", bankAccount.AB_BSB);
			AssertEquals("223344", bankAccount.AB_SWIFT);
			AssertEquals("1234567", bankAccount.AB_AccountNumber);
			AssertEquals("1234567", bankAccount.IBAN);
			AssertEquals("11112222", bankAccount.AB_FullAccountNumber);
			AssertEquals("ASB", bankAccount.AB_AutoDDRFormat);
			AssertEquals("12345", bankAccount.AB_AccountEFTUserID);
			AssertNotEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
			AssertEquals((ZByte)9, bankAccount.AB_ChequeNumDigits);
			AssertEquals(true, bankAccount.AB_AllowAutoDDR);
			AssertEquals(true, bankAccount.AB_ShowDetailsOnDirectDebits);
			AssertEquals("USD", bankAccount.AB_RX_NKAccountCurrency);
			AssertEquals("US", bankAccount.AB_RN_NKBankAccountCountry);
			AssertEquals(true, bankAccount.AB_DetailedDepositSlip);
			AssertEquals(true, bankAccount.AB_IsDefaultReceiptBankAccount);
		}

		public void TestSetAccountTypeDefaults_LNK()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			FillAllBankAccountFields(bankAccount, AccountTypeCodeDescriptionPairList.Codes.CSH);

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
			AssertEquals(AccountTypeCodeDescriptionPairList.Codes.LNK, bankAccount.AB_AccountType);

			//these fields were reset
			AssertEquals(string.Empty, bankAccount.AB_PaymentProvider);
			AssertEquals(string.Empty, bankAccount.AB_AccountNum);

			//these fields are not changed
			AssertEquals("ROBBERY BANK", bankAccount.AB_BankName);
			AssertEquals("ROBBERY TOWN", bankAccount.AB_BankAddress);
			AssertEquals("AAA", bankAccount.AB_BankAbbreviation);
			AssertEquals("111-222", bankAccount.AB_BSB);
			AssertEquals("223344", bankAccount.AB_SWIFT);
			AssertEquals("1234567", bankAccount.AB_AccountNumber);
			AssertEquals("1234567", bankAccount.IBAN);
			AssertEquals("11112222", bankAccount.AB_FullAccountNumber);
			AssertEquals("ASB", bankAccount.AB_AutoDDRFormat);
			AssertEquals("12345", bankAccount.AB_AccountEFTUserID);
			AssertNotEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
			AssertEquals((ZByte)9, bankAccount.AB_ChequeNumDigits);
			AssertEquals(true, bankAccount.AB_AllowAutoDDR);
			AssertEquals(true, bankAccount.AB_ShowDetailsOnDirectDebits);
			AssertEquals("USD", bankAccount.AB_RX_NKAccountCurrency);
			AssertEquals("US", bankAccount.AB_RN_NKBankAccountCountry);
			AssertEquals(true, bankAccount.AB_DetailedDepositSlip);
			AssertEquals(true, bankAccount.AB_IsDefaultReceiptBankAccount);
			AssertEquals("John Doe", bankAccount.AB_DebitCreditCardName);
			AssertEquals("0623", bankAccount.AB_DebitCreditCardExpiry);
			AssertEquals("06", bankAccount.CreditCardExpiryMonth);
			AssertEquals("23", bankAccount.CreditCardExpiryYear);
		}

		public void TestSetAccountTypeDefaults_CCD()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			FillAllBankAccountFields(bankAccount, AccountTypeCodeDescriptionPairList.Codes.CSH);

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			AssertEquals(AccountTypeCodeDescriptionPairList.Codes.CCD, bankAccount.AB_AccountType);

			//these fields were reset
			AssertEquals(string.Empty, bankAccount.AB_PaymentProvider);
			AssertEquals(string.Empty, bankAccount.AB_AccountNum);

			//these fields are not changed
			AssertEquals("ROBBERY BANK", bankAccount.AB_BankName);
			AssertEquals("ROBBERY TOWN", bankAccount.AB_BankAddress);
			AssertEquals("AAA", bankAccount.AB_BankAbbreviation);
			AssertEquals("111-222", bankAccount.AB_BSB);
			AssertEquals("223344", bankAccount.AB_SWIFT);
			AssertEquals("1234567", bankAccount.AB_AccountNumber);
			AssertEquals("1234567", bankAccount.IBAN);
			AssertEquals("11112222", bankAccount.AB_FullAccountNumber);
			AssertEquals("ASB", bankAccount.AB_AutoDDRFormat);
			AssertEquals("12345", bankAccount.AB_AccountEFTUserID);
			AssertNotEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
			AssertEquals((ZByte)9, bankAccount.AB_ChequeNumDigits);
			AssertEquals(true, bankAccount.AB_AllowAutoDDR);
			AssertEquals(true, bankAccount.AB_ShowDetailsOnDirectDebits);
			AssertEquals("USD", bankAccount.AB_RX_NKAccountCurrency);
			AssertEquals("US", bankAccount.AB_RN_NKBankAccountCountry);
			AssertEquals(true, bankAccount.AB_DetailedDepositSlip);
			AssertEquals(true, bankAccount.AB_IsDefaultReceiptBankAccount);
			AssertEquals("John Doe", bankAccount.AB_DebitCreditCardName);
			AssertEquals("0623", bankAccount.AB_DebitCreditCardExpiry);
			AssertEquals("06", bankAccount.CreditCardExpiryMonth);
			AssertEquals("23", bankAccount.CreditCardExpiryYear);
		}

		public void TestSetAccountTypeDefaults_EPA()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			FillAllBankAccountFields(bankAccount, AccountTypeCodeDescriptionPairList.Codes.CSH);

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			AssertEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, bankAccount.AB_AccountType);

			//these fields were reset
			AssertEquals(string.Empty, bankAccount.AB_PaymentProvider);
			AssertEquals("AUD", bankAccount.AB_RX_NKAccountCurrency);
			AssertEquals("AU", bankAccount.AB_RN_NKBankAccountCountry);
			AssertEquals(false, bankAccount.AB_DetailedDepositSlip);
			AssertEquals(false, bankAccount.AB_IsDefaultReceiptBankAccount);
			AssertEquals(string.Empty, bankAccount.AB_BankAbbreviation);
			AssertEquals(string.Empty, bankAccount.AB_BSB);
			AssertEquals(string.Empty, bankAccount.AB_AccountNum);
			AssertEquals(string.Empty, bankAccount.AB_SWIFT);
			AssertEquals(string.Empty, bankAccount.AB_AccountNumber);
			AssertEquals(string.Empty, bankAccount.IBAN);
			AssertEquals(string.Empty, bankAccount.AB_FullAccountNumber);
			AssertEquals(string.Empty, bankAccount.AB_AutoDDRFormat);
			AssertEquals(string.Empty, bankAccount.AB_AccountEFTUserID);
			AssertEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
			AssertEquals((ZByte)1, bankAccount.AB_ChequeNumDigits);
			AssertEquals(false, bankAccount.AB_AllowAutoDDR);
			AssertEquals(false, bankAccount.AB_ShowDetailsOnDirectDebits);
			AssertEquals(string.Empty, bankAccount.AB_DebitCreditCardName);
			AssertEquals(string.Empty, bankAccount.AB_DebitCreditCardExpiry);
			AssertEquals(string.Empty, bankAccount.CreditCardExpiryMonth);
			AssertEquals(string.Empty, bankAccount.CreditCardExpiryYear);

			//these fields are not changed
			AssertEquals("ROBBERY BANK", bankAccount.AB_BankName);
			AssertEquals("ROBBERY TOWN", bankAccount.AB_BankAddress);
		}

		void FillAllBankAccountFields(AccBankAccount bankAccount, string accountType)
		{
			bankAccount.AB_Code = "ABCBANK";
			bankAccount.AB_AccountType = accountType;
			bankAccount.AB_AllowAutoDDR = true;
			bankAccount.AB_AutoDDRFormat = "ASB";
			bankAccount.AB_BankName = "ROBBERY BANK";
			bankAccount.AB_BankAddress = "ROBBERY TOWN";
			bankAccount.AB_BSB = "111-222";
			bankAccount.AB_AccountNum = "1234567";
			bankAccount.AB_AccountNumber = "1234567";
			bankAccount.IBAN = "1234567";
			bankAccount.AB_FullAccountNumber = "11112222";
			bankAccount.AB_SWIFT = "223344";
			bankAccount.AB_AccountEFTUserID = "12345";
			bankAccount.AB_PaymentProvider = "ABC";
			bankAccount.AB_BankAbbreviation = "AAA";
			bankAccount.AB_RX_NKAccountCurrency = "USD";
			bankAccount.AB_RN_NKBankAccountCountry = "US";
			bankAccount.AB_DetailedDepositSlip = true;
			bankAccount.AB_ShowDetailsOnDirectDebits = true;
			bankAccount.AB_DebitCreditCardName = "John Doe";
			bankAccount.CreditCardExpiryMonth = "06";
			bankAccount.CreditCardExpiryYear = "23";
			bankAccount.AB_DebitCreditCardExpiry = "0623";
			bankAccount.AB_IsDefaultReceiptBankAccount = true;
			bankAccount.AB_ChequeNumDigits = 9;
			bankAccount.AB_SO_ChequeTemplate = Factory.LoadTop1<StmTemplate>(new ZQuery()).PK;

			AssertNotEquals(string.Empty, bankAccount.AB_BankName);
			AssertNotEquals(string.Empty, bankAccount.AB_BankAddress);
			AssertNotEquals(string.Empty, bankAccount.AB_PaymentProvider);
			AssertNotEquals(string.Empty, bankAccount.AB_BankAbbreviation);
			AssertNotEquals(string.Empty, bankAccount.AB_BSB);
			AssertNotEquals(string.Empty, bankAccount.AB_AccountNum);
			AssertNotEquals(string.Empty, bankAccount.AB_SWIFT);
			AssertNotEquals(string.Empty, bankAccount.AB_AccountNumber);
			AssertNotEquals(string.Empty, bankAccount.IBAN);
			AssertNotEquals(string.Empty, bankAccount.AB_FullAccountNumber);
			AssertNotEquals(string.Empty, bankAccount.AB_DebitCreditCardName);
			AssertNotEquals(string.Empty, bankAccount.AB_DebitCreditCardExpiry);
			AssertNotEquals(string.Empty, bankAccount.CreditCardExpiryMonth);
			AssertNotEquals(string.Empty, bankAccount.CreditCardExpiryYear);
			AssertNotEquals(string.Empty, bankAccount.AB_AutoDDRFormat);
			AssertNotEquals(string.Empty, bankAccount.AB_AccountEFTUserID);
			AssertNotEquals("AUD", bankAccount.AB_RX_NKAccountCurrency);
			AssertNotEquals("AU", bankAccount.AB_RN_NKBankAccountCountry);
			AssertNotEquals(ZGuid.Empty, bankAccount.AB_SO_ChequeTemplate);
			Assert(bankAccount.AB_DetailedDepositSlip);
			Assert(bankAccount.AB_IsDefaultReceiptBankAccount);
			Assert(bankAccount.AB_AllowAutoDDR);
			Assert(bankAccount.AB_ShowDetailsOnDirectDebits);
		}
	}
}
