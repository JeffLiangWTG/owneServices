using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLHeader))]
	sealed class AccGLHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccGLHeader>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestCompanyFiltersAsString()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "BBB";
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CCC";

			var header1 = Factory.NewWithValidTestData<AccGLHeader>();
			var filter1ForHeader1 = header1.CompanyFilters.AddNew();
			filter1ForHeader1.ACF_GC_Company = company1.PK;
			var filter2ForHeader2 = header1.CompanyFilters.AddNew();
			filter2ForHeader2.ACF_GC_Company = company2.PK;

			var header2 = Factory.NewWithValidTestData<AccGLHeader>();
			var filter1ForHeader3 = header2.CompanyFilters.AddNew();
			filter1ForHeader3.ACF_GC_Company = company3.PK;

			var header3 = Factory.NewWithValidTestData<AccGLHeader>();

			Factory.Save();

			AssertEquals("AAA,BBB", header1.CompanyFiltersAsString);
			AssertEquals("CCC", header2.CompanyFiltersAsString);
			AssertEquals("ALL", header3.CompanyFiltersAsString);
		}

		public void TestSettingAG_IsGlobalRemovesCompanyFilterCollection()
		{
			var header = Factory.NewWithValidTestData<AccGLHeader>();
			AssertEquals(0, header.CompanyFilters.Count);

			header.AG_IsGlobal = false;
			var companyFilter = header.CompanyFilters.AddNew();
			companyFilter.ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertEquals(1, header.CompanyFilters.Count);

			header.AG_IsGlobal = true;
			AssertEquals(0, header.CompanyFilters.Count);
		}

		public void TestCompaniesCollection()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccGLHeader>();
			AssertEquals(0, header.CompanyFilters.Count);
			var companyFilter1 = header.CompanyFilters.AddNew();
			companyFilter1.ACF_GC_Company = company1.PK;
			var companyFilter2 = header.CompanyFilters.AddNew();
			companyFilter2.ACF_GC_Company = company2.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerInNewFactory = newFactory.Load<AccGLHeader>(header.PK);
			AssertEquals(2, headerInNewFactory.CompanyFilters.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			// fix the mismatch between DB and Accounting.xml first
			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_Description, "BANK GURANTEE");
			AccGLHeader header = Factory.LoadTop1<AccGLHeader>(query);
			header.Delete();
			Factory.Save();

			AccGLHeader header1 = Factory.NewWithValidTestData<AccGLHeader>();
			header1.AG_Description = "COST SUSPENSE CONTROL ACCOUNT";
			header1.AG_AccountNum = "8215.00.00";

			AccGLHeader header2 = Factory.NewWithValidTestData<AccGLHeader>();
			header2.AG_Description = "OUTPUT TAX PAYABLE - PENDING";
			header2.AG_AccountNum = "8310.10.00";

			AccGLHeader header3 = Factory.NewWithValidTestData<AccGLHeader>();
			header3.AG_Description = "JOB REVENUE JOURNAL CONTROL ACCOUNT";
			header3.AG_AccountNum = "6245.00.00";

			AccGLHeader header4 = Factory.NewWithValidTestData<AccGLHeader>();
			header4.AG_Description = "REVENUE SUSPENSE CONTROL ACCOUNT";
			header4.AG_AccountNum = "6215.00.00";

			AccGLHeader header5 = Factory.NewWithValidTestData<AccGLHeader>();
			header5.AG_Description = "CLEARING JOURNAL CLEARING ACCOUNT";
			header5.AG_AccountNum = "6211.00.00";

			AccGLHeader header6 = Factory.NewWithValidTestData<AccGLHeader>();
			header6.AG_Description = "INPUT TAX RECEIVABLE - PENDING";
			header6.AG_AccountNum = "6310.10.00";

			AccGLHeader header7 = Factory.NewWithValidTestData<AccGLHeader>();
			header7.AG_Description = "BANK GUARANTEE";
			header7.AG_AccountNum = "6610.00.00";

			Factory.Save();

			base.TestBizObjectFields();
		}

		public void TestConstantsForReportSections()
		{
			ZString message = "If you change these codes, you need to change the database functions that the GL Reports use.";
			AssertEquals(message, "TS", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement);
			AssertEquals(message, "OV", AccGLHeader.Constants.SectionTypes.Codes.Overheads);
			AssertEquals(message, "AP", AccGLHeader.Constants.SectionTypes.Codes.ProfitAndLossAppropriation);
			AssertEquals(message, "OE", AccGLHeader.Constants.SectionTypes.Codes.OwnersEquity);
			AssertEquals(message, "AS", AccGLHeader.Constants.SectionTypes.Codes.Assets);
			AssertEquals(message, "LI", AccGLHeader.Constants.SectionTypes.Codes.Liabilities);

			message = "These descriptions are used for the 'Report Section' CodeDescriptionPairList.";
			AssertEquals(message, "(1) Trading Statement", AccGLHeader.Constants.SectionTypes.Descriptions.TradingStatement);
			AssertEquals(message, "(2) Overheads", AccGLHeader.Constants.SectionTypes.Descriptions.Overheads);
			AssertEquals(message, "(3) Profit & Loss Appropriation", AccGLHeader.Constants.SectionTypes.Descriptions.ProfitAndLossAppropriation);
			AssertEquals(message, "(4) Owners Equity", AccGLHeader.Constants.SectionTypes.Descriptions.OwnersEquity);
			AssertEquals(message, "(5) Assets", AccGLHeader.Constants.SectionTypes.Descriptions.Assets);
			AssertEquals(message, "(6) Liabilities", AccGLHeader.Constants.SectionTypes.Descriptions.Liabilities);
		}

		public void TestSectionPrefixes()
		{
			Header.AG_AccountNum = "1234.56.78";

			AssertPrefixValues(AccGLHeader.Constants.SectionTypes.Codes.TradingStatement,
				AccGLHeader.Constants.SectionTypes.Descriptions.TradingStatement, 1, "1.1234.56.78");

			AssertPrefixValues(AccGLHeader.Constants.SectionTypes.Codes.Overheads,
				AccGLHeader.Constants.SectionTypes.Descriptions.Overheads, 2, "2.1234.56.78");

			AssertPrefixValues(AccGLHeader.Constants.SectionTypes.Codes.ProfitAndLossAppropriation,
				AccGLHeader.Constants.SectionTypes.Descriptions.ProfitAndLossAppropriation, 3, "3.1234.56.78");

			AssertPrefixValues(AccGLHeader.Constants.SectionTypes.Codes.OwnersEquity,
				AccGLHeader.Constants.SectionTypes.Descriptions.OwnersEquity, 4, "4.1234.56.78");

			AssertPrefixValues(AccGLHeader.Constants.SectionTypes.Codes.Assets, AccGLHeader.Constants.SectionTypes.Descriptions.Assets, 5, "5.1234.56.78");

			AssertPrefixValues(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, AccGLHeader.Constants.SectionTypes.Descriptions.Liabilities, 6, "6.1234.56.78");
		}

		void AssertPrefixValues(ZString code, ZString description, ZInt prefix, ZString accountNumWithPrefix)
		{
			Header.AG_Column = code;
			AssertEquals("Prefix", prefix, Header.SectionPrefix);
			AssertEquals("Description", description, Header.SectionDescription);
			AssertEquals("Description", accountNumWithPrefix, Header.AG_Calc_AccountNumberWithPrefix);
		}

		public void TestSectionTypeList()
		{
			AssertNotNull("SectionTypeList should not be null", Header.SectionTypeList);
			AssertEquals("SectionTypeList.Count", 6, Header.SectionTypeList.Count);

			AssertEquals(AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, Header.SectionTypeList[0].Code);
			AssertEquals(AccGLHeader.Constants.SectionTypes.Descriptions.TradingStatement, Header.SectionTypeList[0].Description);

			AssertEquals(AccGLHeader.Constants.SectionTypes.Codes.Overheads, Header.SectionTypeList[1].Code);
			AssertEquals(AccGLHeader.Constants.SectionTypes.Descriptions.Overheads, Header.SectionTypeList[1].Description);

			AssertEquals(AccGLHeader.Constants.SectionTypes.Codes.ProfitAndLossAppropriation, Header.SectionTypeList[2].Code);
			AssertEquals(AccGLHeader.Constants.SectionTypes.Descriptions.ProfitAndLossAppropriation, Header.SectionTypeList[2].Description);

			AssertEquals(AccGLHeader.Constants.SectionTypes.Codes.OwnersEquity, Header.SectionTypeList[3].Code);
			AssertEquals(AccGLHeader.Constants.SectionTypes.Descriptions.OwnersEquity, Header.SectionTypeList[3].Description);

			AssertEquals(AccGLHeader.Constants.SectionTypes.Codes.Assets, Header.SectionTypeList[4].Code);
			AssertEquals(AccGLHeader.Constants.SectionTypes.Descriptions.Assets, Header.SectionTypeList[4].Description);

			AssertEquals(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, Header.SectionTypeList[5].Code);
			AssertEquals(AccGLHeader.Constants.SectionTypes.Descriptions.Liabilities, Header.SectionTypeList[5].Description);
		}

		void SetRegistryGuid(Guid newGuid, string registryName)
		{
			var cmd = $@"DECLARE @NewGuid UNIQUEIDENTIFIER
				SET @newGuid = IIF('{newGuid}' = CAST(0x0 AS UNIQUEIDENTIFIER), null, '{newGuid}');
				UPDATE dbo.StmData
				SET SD_GuidValue = @newGuid
				WHERE SD_Name = '{registryName}'

				IF (@@ROWCOUNT = 0)
				BEGIN
				    INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled)
				    VALUES (NEWID(), '{registryName}', null, null, 'GID', 0, null, @newGuid, 0)
				END";
			Db.Connection.ExecuteNonQuery(cmd);
		}

		public void TestDelete()
		{
			var header = Factory.NewWithValidTestData<AccGLHeader>();
			SetRegistryGuid(header.PK.ToGuid(), "GL_AR_CONTROL_ACCOUNT");

			var subAccountType = header.SubAccountTypes.AddNew();
			subAccountType.FillWithValidTestData();

			var companyFilter = header.CompanyFilters.AddNew();
			companyFilter.FillWithValidTestData();
			
			header.Delete();
			Assert(!header.CanDelete);

			header.AG_AccountNum = "Test";
			AssertEquals("This GL account Test is used at least once in the Registry. Please choose different registry values before attempting to delete again.", header.ReasonForNotAbleToDelete);

			SetRegistryGuid(Guid.Empty, "GL_AR_CONTROL_ACCOUNT");
			header.Delete();
			Assert(header.CanDelete);
			AssertEquals(0, header.SubAccountTypes.Count);
			AssertEquals(0, header.CompanyFilters.Count);
			Assert(header.IsDeleted);
		}

		public void TestCanNotDeleteGLAccountsRelatedToEFEE()
		{
			var disbursementClearingAccount = Factory.NewWithValidTestData<AccGLHeader>();
			var payableClearingAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			Assert("This disbursement clearing account should be deletable.", disbursementClearingAccount.CanDelete);
			Assert("This payable clearing account should be deletable.", payableClearingAccount.CanDelete);

			SetRegistryGuid(disbursementClearingAccount.PK.ToGuid(), "ElectronicProcessingChargeDisbursementClearingAccount");
			SetRegistryGuid(payableClearingAccount.PK.ToGuid(), "ElectronicProcessingChargePayableClearingAccount");

			Assert("This disbursement clearing account should not be deletable.", !disbursementClearingAccount.CanDelete);
			Assert("This payable clearing account should not be deletable.", !payableClearingAccount.CanDelete);

			disbursementClearingAccount.AG_AccountNum = "Test1";
			payableClearingAccount.AG_AccountNum = "Test2";

			AssertEquals("The GL Account Test1 is a system defined GL Account used in Electronic Processing Fee management and cannot be deleted.",
				disbursementClearingAccount.ReasonForNotAbleToDelete.GetUnresolvedString());
			AssertEquals("The GL Account Test2 is a system defined GL Account used in Electronic Processing Fee management and cannot be deleted.",
				payableClearingAccount.ReasonForNotAbleToDelete.GetUnresolvedString());
		}

		public void TestCurrentGLAccountFormat()
		{
			AssertMatch(new Regex("^[X.]+$"), AccGLHeader.CurrentGLAccountFormat);
		}

		public void TestCurrentGLAccountFormatComesFromRegistry()
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.GLAccountFormat).Returns("X.XX.XXX");
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("CurrentGLAccountFormat", "X.XX.XXX", AccGLHeader.CurrentGLAccountFormat);
			}

			mock = new Mock<IAccounting>();
			mock.Setup(m => m.GLAccountFormat).Returns("XXX.XX.X");
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("CurrentGLAccountFormat", "XXX.XX.X", AccGLHeader.CurrentGLAccountFormat);
			}
		}

		public void TestAG_CashFlowTypeList()
		{
			AssertNotNull(AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value);
			Assert("CashFlowActivityConfiguration default list must have more than 0 elements.", AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value.Count > 0);
			AssertEquals(AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value.Count, Header.AG_CashFlowTypeList.Count);
			foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
			{
				Assert(Header.AG_CashFlowTypeList.ContainsCode(cashFlowActivity.Code));
			}
		}

		public void TestAG_DebitCreditList()
		{
			AssertEquals("AG_DebitCreditList.Count should be 2", 2, Header.AG_DebitCreditList.Count);
		}

		public void TestAG_AccountTypeList()
		{
			var expectedCodes = new string[]
			{
					Core.Constants.AccountType.BalanceSheetAccount,
					Core.Constants.AccountType.ProfitAndLossAccount,
					Core.Constants.AccountType.Total,
					Core.Constants.AccountType.Header,
					Core.Constants.AccountType.Consolidation,
					Core.Constants.AccountType.Alternate,
					Core.Constants.AccountType.Note
			};

			var glAccountTypeActualCodesArray = Header.AG_AccountTypeList.GetAllCodes().ToArray();
			AssertContainsExactElementsInAnyOrder(expectedCodes, glAccountTypeActualCodesArray);
			AssertEquals("AG_AccountTypeList.Count should be 7", 7, Header.AG_AccountTypeList.Count);
		}

		public void TestAG_StatisticalUnitsList()
		{
			AssertEquals(ObjectFactory.Get<IAccounting>().Registry.NoteGLAccountsStatisticalUnitsofMeasurement(GlbCompany.CurrentCompany.PK.ToGuid()) as ReadOnlyCodeDescriptionPairList, Header.AG_StatisticalUnitsList);
		}

		public void TestAG_SubAccountTypeList()
		{
			AssertEquals("AG_SubAccountTypeList.Count should be 4", 4, Header.AG_SubAccountTypeList.Count);
			Assert("ORG is a AG_SubAccountTypeList", Header.AG_SubAccountTypeList.ContainsCode("ORG"));
			AssertEquals("Organization", Header.AG_SubAccountTypeList["ORG"].Description);
			Assert("SEG is a AG_SubAccountTypeList", Header.AG_SubAccountTypeList.ContainsCode("SEG"));
			AssertEquals("Sales/Expense Groups", Header.AG_SubAccountTypeList["SEG"].Description);
			Assert("STR is a AG_SubAccountTypeList", Header.AG_SubAccountTypeList.ContainsCode("STR"));
			AssertEquals("Staff and Resources", Header.AG_SubAccountTypeList["STR"].Description);
			Assert("SGP is a AG_SubAccountTypeList", Header.AG_SubAccountTypeList.ContainsCode("SGP"));
			AssertEquals("Staff Group", Header.AG_SubAccountTypeList["SGP"].Description);
		}

		public void TestCashFlowTypeLogs()
		{
			AssertEquals("", Header.AG_CashFlowType);
			Factory.Save();

			Header.AG_CashFlowType = "O01";
			Factory.Save();
			AssertLogExists("Cash Flow Type updated. Previous value: '[]', New value: '(O01)'");

			Header.AG_CashFlowType = "F01";
			Factory.Save();
			AssertLogExists("Cash Flow Type updated. Previous value: '[]', New value: '(O01)'");
			AssertLogExists("Cash Flow Type updated. Previous value: '[O01]', New value: '(F01)'");
		}

		public void TestSubAccountTypesLogs()
		{
			AssertEquals(0, Header.SubAccountTypes.Count);
			Factory.Save();

			var subAccountTypes1 = Header.SubAccountTypes.AddNew();
			subAccountTypes1.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
			subAccountTypes1.ASA_IsSubClassValidationRuleMandatory = true;
			var subAccountTypes2 = Header.SubAccountTypes.AddNew();
			subAccountTypes2.ASA_SubClassDisplayName = Core.Constants.SubAccountType.SalesGroup;
			Factory.Save();
			AssertLogExists("Sub Account Types updated to [Type=ORG, Mandatory=Y], [Type=SEG, Mandatory=N]");

			subAccountTypes1.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffAndResources;
			subAccountTypes1.ASA_IsSubClassValidationRuleMandatory = false;
			subAccountTypes2.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
			Factory.Save();
			AssertLogExists("Sub Account Types updated to [Type=STR, Mandatory=N], [Type=ORG, Mandatory=N]");

			Header.SubAccountTypes.RemoveAndDeleteAll();
			Factory.Save();
			AssertLogExists("Sub Account Types updated to empty");
		}

		public void TestAccountNumLogs()
		{
			Header.AG_AccountNum = "12345";
			Factory.Save();
			Header.AG_AccountNum = "54321";
			Factory.Save();
			AssertLogExists("Account Number Updated. Previous Value: '12345', New Value: '54321'");
		}

		void AssertLogExists(string logEntry)
		{
			AssertNotNull(Header.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(s => s.SL_SE_NKEvent == Events.EditedARecord.Code && s.SL_Reference == logEntry));
		}

		[ExpectException(typeof(ZSaveException))]
		public void TestDeleteGLHeaderWithLocalMapping()
		{
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor1.ParentGLHeaderPK = gLHeader.PK;

			Factory.Save();

			gLHeader.Delete();
			Factory.Save();
		}

		public void TestSortOnFactorySaving()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var subAccountType1 = glHeader.SubAccountTypes.AddNew();
			var subAccountType2 = glHeader.SubAccountTypes.AddNew();
			var subAccountType3 = glHeader.SubAccountTypes.AddNew();
			var subAccountType4 = glHeader.SubAccountTypes.AddNew();

			subAccountType1.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffGroup;
			subAccountType2.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffAndResources;
			subAccountType3.ASA_SubClassDisplayName = Core.Constants.SubAccountType.SalesGroup;
			subAccountType4.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;

			var glHeaderSubAccountCollection = glHeader.SubAccountTypes.Cast<AccGLHeaderSubAccount>();

			AssertEquals("1st sub account", subAccountType1, glHeaderSubAccountCollection.FirstOrDefault());
			AssertEquals("2nd sub account", subAccountType2, glHeaderSubAccountCollection.Skip(1).FirstOrDefault());
			AssertEquals("3rd sub account", subAccountType3, glHeaderSubAccountCollection.Skip(2).FirstOrDefault());
			AssertEquals("4th sub account", subAccountType4, glHeaderSubAccountCollection.Skip(3).FirstOrDefault());

			Factory.Save();

			AssertEquals("1st sub account", subAccountType4, glHeaderSubAccountCollection.FirstOrDefault());
			AssertEquals("2nd sub account", subAccountType3, glHeaderSubAccountCollection.Skip(1).FirstOrDefault());
			AssertEquals("3rd sub account", subAccountType2, glHeaderSubAccountCollection.Skip(2).FirstOrDefault());
			AssertEquals("4th sub account", subAccountType1, glHeaderSubAccountCollection.Skip(3).FirstOrDefault());
		}

		public void TestSubAccountTypes()
		{
			var gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var testGLHeaderSubAccount1 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount2 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount3 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			testGLHeaderSubAccount1.ASA_AG = gLHeader.PK;
			testGLHeaderSubAccount2.ASA_AG = gLHeader.PK;
			testGLHeaderSubAccount1.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.Organization);
			testGLHeaderSubAccount2.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.SalesGroup);
			testGLHeaderSubAccount3.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.StaffAndResources);

			Factory.Save();

			AssertEquals(2, gLHeader.SubAccountTypes.Count);
			AssertCollectionContains(testGLHeaderSubAccount1, gLHeader.SubAccountTypes);
			AssertCollectionContains(testGLHeaderSubAccount2, gLHeader.SubAccountTypes);
		}

		public void TestAG_Calc_SubAccountTypes()
		{
			var gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var gLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			var gLHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			var testGLHeaderSubAccount1 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount2 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount3 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			testGLHeaderSubAccount1.ASA_AG = gLHeader.PK;
			testGLHeaderSubAccount2.ASA_AG = gLHeader.PK;
			testGLHeaderSubAccount3.ASA_AG = gLHeader2.PK;
			testGLHeaderSubAccount1.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.SalesGroup);
			testGLHeaderSubAccount2.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.Organization);
			testGLHeaderSubAccount3.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.StaffAndResources);

			Factory.Save();

			AssertEquals("ORG, SEG", gLHeader.AG_Calc_SubAccountTypes);
			AssertEquals("STR", gLHeader2.AG_Calc_SubAccountTypes);
			AssertEquals("", gLHeader3.AG_Calc_SubAccountTypes);
		}

		public void TestGetIsSubClassValidationRuleMandatory()
		{
			var gLHeader = Factory.NewWithValidTestData<AccGLHeader>();

			var subAccountType1 = gLHeader.SubAccountTypes.AddNew();
			subAccountType1.ASA_SubClass = OrgHeaderSchema.Constants.Prefix;
			subAccountType1.ASA_IsSubClassValidationRuleMandatory = false;

			var subAccountType2 = gLHeader.SubAccountTypes.AddNew();
			subAccountType2.ASA_SubClass = GlbStaffSchema.Constants.Prefix;
			subAccountType2.ASA_IsSubClassValidationRuleMandatory = true;

			AssertEquals("result should be false when ASA_IsSubClassValidationRuleMandatory is false", false, gLHeader.GetIsSubClassValidationRuleMandatory(OrgHeaderSchema.Constants.Prefix));
			AssertEquals("result should be true when ASA_IsSubClassValidationRuleMandatory is true", true, gLHeader.GetIsSubClassValidationRuleMandatory(GlbStaffSchema.Constants.Prefix));
			AssertEquals("result should be false when ASA_IsSubClassValidationRuleMandatory is not exist", false, gLHeader.GetIsSubClassValidationRuleMandatory("AA"));
		}

		public void TestIsAllowedToHaveAttributes_BankAccount()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			Assert("Allowed if it is not bank account", glHeader.IsAllowedToHaveAttributes());

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AG = glHeader.PK;
			Factory.Save();
			Assert("Disallowed if it is bank account", !glHeader.IsAllowedToHaveAttributes());
		}

		public void TestIsAllowedToHaveAttributes_MasterRegistry()
		{
			var creator = new AccountingTestObjectCreator(Factory);
			var accountDictionary = new Dictionary<IRegistryItem, AccGLHeader>();

			IRegistryItem[] registryItemsToCheck =
			{
				AccountingMasterFilesRegistry.Instance.PendingTaxTransactionPrepaidAssetControlAccount,
				AccountingMasterFilesRegistry.Instance.PendingTaxTransactionRemittanceLiabilityControlAccount,
				AccountingMasterFilesRegistry.Instance.TaxTransactionExpenseAccount,
				AccountingMasterFilesRegistry.Instance.TaxTransactionNegativeRevenueAccount,
				AccountingMasterFilesRegistry.Instance.TaxTransactionRemittanceLiabilityControlAccount,
				AccountingMasterFilesRegistry.Instance.TaxTransactionPrepaidAssetControlAccount,
				AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateDifferenceAccount,
			};

			foreach (var registry in registryItemsToCheck)
			{
				var account = Factory.NewWithValidTestData<AccGLHeader>();
				accountDictionary.Add(registry, account);
				registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account.PK.ToGuid());
			}

			CombineAssertions("Disallowed", () =>
			{
				foreach (var account in accountDictionary)
				{
					Assert(account.Key.Caption, !account.Value.IsAllowedToHaveAttributes());
				}
			});
		}

		public void TestIsAllowedToHaveAttributes_AccountingRegistry()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForDissectionAttributes(glHeader.PK)).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				Assert("Disallowed", !glHeader.IsAllowedToHaveAttributes());
			}

			mock.Setup(m => m.IsNotAllowedForDissectionAttributes(glHeader.PK)).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				Assert("Allowed", glHeader.IsAllowedToHaveAttributes());
			}
		}

		public void TestStatisticalUnitsInfo()
		{
			AssertEquals(Header.AG_StatisticalUnitsInfo, Header.StatisticalUnitsInfo);
		}

		public void TestHumanReadableName()
		{
			Header.AG_AccountNum = string.Empty;
			AssertEquals("HumanReadableName", (NoResString)"General Ledger Account", Header.HumanReadableName);
			Header.AG_AccountNum = "code";
			AssertEquals("HumanReadableName", (NoResString)"General Ledger Account" + " (code)", Header.HumanReadableName);
		}

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(AccGLHeader)));
		}

		#endregion

		#region Implementation

		AccGLHeader Header;

		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.NewWithValidTestData<AccGLHeader>();
		}

		#endregion
	}
}
