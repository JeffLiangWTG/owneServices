using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestAccGLHeaderValidation : BusinessObjectValidationTestCase
	{
		public void TestValidateAG_IsGlobal()
		{
			Header.CompanyFilters.AddNew();
			Assert(Header.AG_IsGlobal);

			Header.AG_IsGlobal = false;
			Header.AG_ControlAccount = true;
			AssertHasError(Header.AG_IsGlobalInfo, "A Control Account must be valid for all companies. Please tick the Is Global checkbox.");
			AssertNoError(Header.AG_IsGlobalInfo, "Company filters must be set up.");
			Header.AG_IsGlobal = true;
			AssertNoErrors(Header.AG_IsGlobalInfo);
			Header.AG_ControlAccount = false;
			AssertNoErrors(Header.AG_IsGlobalInfo);
			Header.AG_IsGlobal = false;
			AssertHasError(Header.AG_IsGlobalInfo, "Company filters must be set up.");
			Header.AG_ControlAccount = true;
			AssertHasError(Header.AG_IsGlobalInfo, "A Control Account must be valid for all companies. Please tick the Is Global checkbox.");
			AssertHasError(Header.AG_IsGlobalInfo, "Company filters must be set up.");

			Header.AG_ControlAccount = false;
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_AG_AccrualAccount = Header.PK;
			Header.Validation.ValidateAG_IsGlobal();
			AssertHasError(Header.AG_IsGlobalInfo, "GL Accounts used on Global Charge Codes must be a Global GL Account.");
			AssertHasError(Header.AG_IsGlobalInfo, "Company filters must be set up.");

			AssertNoError(Header.AG_IsGlobalInfo, "TTL, HDR, CLN and ALT account types must be Global.");
			Header.AG_AccountType = Core.Constants.AccountType.Consolidation;
			Header.Validation.ValidateAG_IsGlobal();
			AssertHasError(Header.AG_IsGlobalInfo, "TTL, HDR, CLN and ALT account types must be Global.");

			Header.AG_IsGlobal = false;
			Header.Validation.ValidateAG_IsGlobal();
			AssertNoError(Header.AG_IsGlobalInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Company Level. Please set it to Global.");
			var mock1 = new Mock<IAccountingRegistryProvider>();
			mock1.Setup(m => m.ElectronicProcessingChargeDisbursementClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock1.Object))
			{
				Header.Validation.ValidateAG_IsGlobal();
				AssertHasError(Header.AG_IsGlobalInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Company Level. Please set it to Global.");
			}

			Header.AG_IsGlobal = false;
			var mock2 = new Mock<IAccountingRegistryProvider>();
			mock2.Setup(m => m.ElectronicProcessingChargePayableClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock2.Object))
			{
				Header.Validation.ValidateAG_IsGlobal();
				AssertHasError(Header.AG_IsGlobalInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Company Level. Please set it to Global.");
			}
		}

		public void TestValidateAG_IsActive()
		{
			Header.AG_IsActive = true;
			AssertNoErrors(Header.AG_IsActiveInfo);
			Header.AG_IsActive = false;
			AssertNoErrors(Header.AG_IsActiveInfo);
			Header.AG_IsActive = true;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AG_CostAccount = Header.PK;
			chargeCode.AC_IsActive = true;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertHasError(Header.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			Header.AG_IsActive = true;
			chargeCode.AC_IsActive = false;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertNoErrors(Header.AG_IsActiveInfo);

			Header.AG_IsActive = true;
			chargeCode.AC_AG_CostAccount = ZGuid.Empty;
			chargeCode.AC_IsActive = true;
			Factory.Save();

			chargeCode.AC_AG_AccrualAccount = Header.PK;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertHasError(Header.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			chargeCode.AC_AG_AccrualAccount = ZGuid.Empty;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertNoErrors(Header.AG_IsActiveInfo);

			Header.AG_IsActive = true;
			chargeCode.AC_AG_WIPAccount = Header.PK;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertHasError(Header.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			chargeCode.AC_AG_WIPAccount = ZGuid.Empty;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertNoErrors(Header.AG_IsActiveInfo);

			Header.AG_IsActive = true;
			chargeCode.AC_AG_RevenueAccount = Header.PK;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertHasError(Header.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			chargeCode.AC_AG_RevenueAccount = ZGuid.Empty;
			Factory.Save();
			Header.AG_IsActive = false;
			AssertNoErrors(Header.AG_IsActiveInfo);

			Header.AG_IsActive = false;
			Factory.Save();
			AssertNoError(Header.AG_IsActiveInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to inactive. Please set it to active.");
			var mock1 = new Mock<IAccountingRegistryProvider>();
			mock1.Setup(m => m.ElectronicProcessingChargeDisbursementClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock1.Object))
			{
				Header.Validation.ValidateAG_IsActive();
				AssertHasError(Header.AG_IsActiveInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to inactive. Please set it to active.");
			}

			Header.AG_IsActive = false;
			var mock2 = new Mock<IAccountingRegistryProvider>();
			mock2.Setup(m => m.ElectronicProcessingChargePayableClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock2.Object))
			{
				Header.Validation.ValidateAG_IsActive();
				AssertHasError(Header.AG_IsActiveInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to inactive. Please set it to active.");
			}
		}

		public void TestValidateAG_IsActive_GLPostingOverride()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_IsActive = true;
			glHeader.AG_IsGlobal = true;
			glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			glHeader.AG_ControlAccount = false;
			Factory.Save();

			AssertNoErrors(glHeader.AG_IsActiveInfo);
			glHeader.AG_IsActive = false;
			AssertNoErrors(glHeader.AG_IsActiveInfo);
			glHeader.AG_IsActive = true;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_IsActive = true;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode.AC_Desc = "Test Charge Code";
			var glPostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			glPostingOverride.Y1_AG_ACR = glHeader.PK;
			Factory.Save();

			glHeader.AG_IsActive = false;
			AssertHasError(glHeader.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			glHeader.AG_IsActive = true;
			glPostingOverride.Y1_AG_ACR = ZGuid.Empty;
			Factory.Save();

			glPostingOverride.Y1_AG_CST = glHeader.PK;
			Factory.Save();
			glHeader.AG_IsActive = false;
			AssertHasError(glHeader.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			glPostingOverride.Y1_AG_CST = ZGuid.Empty;
			Factory.Save();
			glHeader.AG_IsActive = false;
			AssertNoErrors(glHeader.AG_IsActiveInfo);

			glHeader.AG_IsActive = true;
			glPostingOverride.Y1_AG_WIP = glHeader.PK;
			Factory.Save();
			glHeader.AG_IsActive = false;
			AssertHasError(glHeader.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			glPostingOverride.Y1_AG_WIP = ZGuid.Empty;
			Factory.Save();
			glHeader.AG_IsActive = false;
			AssertNoErrors(Header.AG_IsActiveInfo);

			glHeader.AG_IsActive = true;
			glPostingOverride.Y1_AG_REV = glHeader.PK;
			Factory.Save();
			glHeader.AG_IsActive = false;
			AssertHasError(glHeader.AG_IsActiveInfo, "This GL Account is used on active Charge Codes.");

			glPostingOverride.Y1_AG_REV = ZGuid.Empty;
			Factory.Save();
			glHeader.AG_IsActive = false;
			AssertNoErrors(glHeader.AG_IsActiveInfo);
		}

		public void TestValidateAG_AccountNum()
		{
			Header.AG_AccountNum = "";
			Assert("Account num cannot be null", Header.AG_AccountNumInfo.HasErrors());

			Header.AG_AccountNum = "fail";
			Assert("Account num only allows '9' and '.'", Header.AG_AccountNumInfo.HasErrors());

			AccGLHeader consolidateAccount = Factory.New<AccGLHeader>();
			consolidateAccount.AG_AccountType = "CLN";
			consolidateAccount.AG_AccountNum = "3000.00.00";
			consolidateAccount.AG_Column = "TS"; //1

			Header.AG_Column = "OE"; //4
			Header.AG_AG_ConsolidationNum = consolidateAccount.PK;
			Header.AG_AccountNum = "4000.00.00";
			AssertHasErrors(Header.AG_AccountNumInfo);
			AssertNoErrors(Header.AG_AG_ConsolidationNumInfo);

			Header.AG_AccountNum = "2000.00.00";
			AssertHasErrors(Header.AG_AccountNumInfo);
			AssertNoErrors(Header.AG_AG_ConsolidationNumInfo);

			Header.AG_Column = "TS"; //1
			consolidateAccount.AG_Column = "OE"; //4
			Header.AG_AccountNum = "4000.00.00";
			AssertNoErrors(Header.AG_AccountNumInfo);
			AssertNoErrors(Header.AG_AG_ConsolidationNumInfo);
		}

		public void TestValidateAG_DebitCredit()
		{
			Header.AG_DebitCredit = "";
			Assert("Debit/credit cannot be null", Header.AG_DebitCreditInfo.HasErrors());
		}

		public void TestValidateAG_Description()
		{
			Header.AG_Description = "";
			Assert(Header.AG_DescriptionInfo.HasErrors());

			Header.AG_Description = "som";
			Assert(!Header.AG_DescriptionInfo.HasErrors());
		}

		public void TestValidateAG_CashFlowType()
		{
			Header.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Header.AG_CashFlowType = "";
			Assert(Header.AG_CashFlowTypeInfo.HasErrors());
			Assert(Header.AG_CashFlowTypeInfo.HasError("Please enter a Cash Flow Cat.."));

			Header.AG_CashFlowType = "ABC";
			Assert(Header.AG_CashFlowTypeInfo.HasErrors());
			Assert(Header.AG_CashFlowTypeInfo.HasError("Enter a valid Cash Flow Cat.."));

			Header.AG_CashFlowType = "O01";
			Assert(!Header.AG_CashFlowTypeInfo.HasErrors());

			Header.AG_AccountType = Core.Constants.AccountType.Alternate;
			Header.AG_CashFlowType = "F01";
			Assert(Header.AG_CashFlowTypeInfo.HasError("cash flow type is only applicable for 'P&L' and 'BSH' type GL Account."));

			Header.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Header.AG_CashFlowType = "CSH";
			Assert(Header.AG_CashFlowTypeInfo.HasWarning("You have selected CSH. Please note that 'CSH' should only be used for GL Accounts relating to Maintain > Accounts > Bank Accounts."));
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AG = Header.PK;
			Factory.Save();
			Header.AG_CashFlowType = "";
			Header.AG_CashFlowType = "CSH";
			Assert(!Header.AG_CashFlowTypeInfo.HasWarnings());

			Header.AG_CashFlowType = "EXX";
			Assert(Header.AG_CashFlowTypeInfo.HasWarning("EXX can not be chosen for accounts other than the one defined in registry Bank Currency Adjustment Account"));
			string cmd = string.Format(@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled)
			VALUES (NEWID(), 'GL_BANKCURRENCY_ADJUSTMENT_ACCOUNT', null, null, 'GID', 0, null, '{0}', 0) ", Header.PK);
			Db.Connection.ExecuteNonQuery(cmd);
			Header.AG_CashFlowType = "";
			Header.AG_CashFlowType = "EXX";
			Assert(!Header.AG_CashFlowTypeInfo.HasWarnings());
		}

		public void TestValidateAG_AccountType()
		{
			Header.AG_AccountType = "";
			Assert(Header.AG_AccountTypeInfo.HasErrors());

			Header.AG_AccountType = "BSB";
			Assert(Header.AG_AccountTypeInfo.HasErrors());

			Header.AG_AccountType = "BSH";
			Assert(!Header.AG_AccountTypeInfo.HasErrors());

			Header.AG_IsGlobal = false;
			AssertNoError(Header.AG_IsGlobalInfo, "TTL, HDR, CLN and ALT account types must be Global.");
			Header.AG_AccountType = Core.Constants.AccountType.Consolidation;
			AssertHasError(Header.AG_IsGlobalInfo, "TTL, HDR, CLN and ALT account types must be Global.");
		}

		public void TestValidateAG_AccountType_ChangeUsedALTAccount()
		{
			AccGLHeader anotherHeader = Factory.New<AccGLHeader>();

			Header.AG_AccountType = "ALT";
			AssertNoErrors(Header.AG_AccountTypeInfo);

			foreach (CodeDescriptionPair pair in Header.AG_AccountTypeList)
			{
				if (pair.Code != "ALT")
				{
					Header.AG_AccountType = pair.Code;
					AssertNoErrors(Header.AG_AccountTypeInfo);
				}
			}

			anotherHeader.AG_AG_AlternateNum = Header.PK;
			foreach (CodeDescriptionPair pair in Header.AG_AccountTypeList)
			{
				if (pair.Code != "ALT")
				{
					Header.AG_AccountType = pair.Code;
					AssertHasErrors(Header.AG_AccountTypeInfo);
				}
			}

			Header.AG_AccountType = "ALT";
			AssertNoErrors(Header.AG_AccountTypeInfo);

			anotherHeader.AG_AG_AlternateNum = ZGuid.Empty;
			foreach (CodeDescriptionPair pair in Header.AG_AccountTypeList)
			{
				if (pair.Code != "ALT")
				{
					Header.AG_AccountType = pair.Code;
					AssertNoErrors(Header.AG_AccountTypeInfo);
				}
			}
		}

		public void TestValidateAG_AccountType_ChangeUsedProfitAndLossAccount()
		{
			AssertValidateAG_AccountType_ChangeUsed(Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.AccountType.Header);
		}

		public void TestValidateAG_AccountType_ChangeUsedBalanceSheetAccount()
		{
			AssertValidateAG_AccountType_ChangeUsed(Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.AccountType.Header);
		}

		void AssertValidateAG_AccountType_ChangeUsed(string originalType, string safeType, string invalidType)
		{
			Header.AG_AccountType = originalType;
			AssertNoErrors(Header.AG_AccountTypeInfo);
			Factory.Save();

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(false);
			mock.Setup(m => m.WIPMustHaveDebtorCode(It.IsAny<Guid>())).Returns(false);
			mock.Setup(m => m.GetCaptionsOfRegistryItemsUsingGLHeader(Header.PK.ToGuid())).Returns(new string[] { "Some Registry Item", "Another Registry Item" });
			using (ObjectFactory.Substitute(mock.Object))
			{
				Header.AG_AccountType = invalidType;
				string expectedError = string.Format("This account cannot be converted to account type {0} because it is currently in use as a {1}. It is used by accounting configuration registry item(s) 'Some Registry Item', 'Another Registry Item'.", invalidType, originalType);
				AssertHasError(Header.AG_AccountTypeInfo, expectedError);

				Header.AG_AccountType = originalType;
				AssertNoErrors(Header.AG_AccountTypeInfo);

				AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
				bankAccount.AB_AG = Header.PK;
				Factory.Save();

				Header.AG_AccountType = invalidType;
				expectedError = string.Format("This account cannot be converted to account type {0} because it is currently in use as a {1}. It is used by bank account '{2}'; accounting configuration registry item(s) 'Some Registry Item', 'Another Registry Item'.", invalidType, originalType, bankAccount.AB_Code);
				AssertHasError(Header.AG_AccountTypeInfo, expectedError);

				Header.AG_AccountType = originalType;
				AssertNoErrors(Header.AG_AccountTypeInfo);

				AccChargeCode charge = Factory.NewWithValidTestData<AccChargeCode>();
				AccChargeGLPostingOverride chargePostingOverride = Factory.New<AccChargeGLPostingOverride>();
				chargePostingOverride.Y1_AC = charge.PK;
				chargePostingOverride.Y1_AG_ACR = Header.PK;
				chargePostingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
				Factory.Save();

				Header.AG_AccountType = invalidType;
				expectedError = string.Format("This account cannot be converted to account type {0} because it is currently in use as a {1}. It is used by bank account '{2}'; charge posting override for charge code {3} in company {4}; accounting configuration registry item(s) 'Some Registry Item', 'Another Registry Item'.", invalidType, originalType, bankAccount.AB_Code, charge.AC_Code, charge.Company.GC_Code);
				AssertHasError(Header.AG_AccountTypeInfo, expectedError);

				Header.AG_AccountType = originalType;
				AssertNoErrors(Header.AG_AccountTypeInfo);

				charge.AC_AG_AccrualAccount = Header.PK;
				Factory.Save();

				Header.AG_AccountType = invalidType;
				expectedError = string.Format("This account cannot be converted to account type {0} because it is currently in use as a {1}. It is used by bank account '{2}'; charge code {3} in company {4}; charge posting override for charge code {3} in company {4}; accounting configuration registry item(s) 'Some Registry Item', 'Another Registry Item'.", invalidType, originalType, bankAccount.AB_Code, charge.AC_Code, charge.Company.GC_Code);
				AssertHasError(Header.AG_AccountTypeInfo, expectedError);

				Header.AG_AccountType = originalType;
				AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
				AccTransactionLines line1 = Factory.NewWithValidTestData<AccTransactionLines>();
				line1.AL_AH = header.PK;
				line1.AL_AG = Header.PK;
				line1.AL_PostDate = ZDateTime.Today.AddDays(-2);
				AccTransactionLines line2 = Factory.NewWithValidTestData<AccTransactionLines>();
				line2.AL_AH = header.PK;
				line2.AL_AG = Header.PK;
				line2.AL_PostDate = ZDateTime.Today;
				Factory.Save();

				Header.AG_AccountType = invalidType;
				expectedError = string.Format("This account cannot be converted to account type {0} because it is currently in use as a {1}. It is used by transaction lines in company {2} posted between {3} and {4}.", invalidType, originalType, header.Company.GC_Code, line1.AL_PostDate, line2.AL_PostDate);
				AssertHasError(Header.AG_AccountTypeInfo, expectedError);

				Header.AG_AccountType = safeType;
				AssertNoErrors(Header.AG_AccountTypeInfo);
			}
		}

		public void TestValidateAG_AG_AlternateNum()
		{
			AccGLHeader alternateAccount = Factory.New<AccGLHeader>();
			alternateAccount.AG_AccountType = "ALT";
			alternateAccount.AG_AccountNum = "1000.00.00";
			alternateAccount.AG_Column = "AS";

			AccGLHeader accountWithAlternateNum = Factory.NewWithValidTestData<AccGLHeader>();
			accountWithAlternateNum.AG_AccountType = "BSH";
			accountWithAlternateNum.AG_AccountNum = "2000.00.00";
			accountWithAlternateNum.AG_AG_AlternateNum = alternateAccount.PK;

			Header.AG_AG_AlternateNum = alternateAccount.PK;
			AssertHasError(Header.AG_AG_AlternateNumInfo, "This alternate number is already used for another GL Account");
		}

		public void TestValidateAG_TotalLevel()
		{
			Header.AG_AccountType = "TTL";
			Header.AG_TotalLevel = 0;
			Assert("Expect error: total level should be between 1 and 999", Header.AG_TotalLevelInfo.HasErrors());

			Header.AG_TotalLevel = 999;
			Assert("Not expecting errors", !Header.AG_TotalLevelInfo.HasErrors());

			Header.AG_TotalLevel = 1000;
			Assert("Expect error: total level should be between 1 and 999", Header.AG_TotalLevelInfo.HasErrors());
		}

		public void TestValidateAG_PrintSequence()
		{
			Header.AG_PrintSequence = 0;
			Assert("Not expecting errors", !Header.AG_PrintSequenceInfo.HasErrors());

			Header.AG_PrintSequence = -1;
			Assert("Error: print sequence should be between 0 and 9", Header.AG_PrintSequenceInfo.HasErrors());
		}

		public void TestValidateAG_AG_HeaderDependsOnTotal()
		{
			var testBSHGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testBSHGLHeader.AG_AccountType = "BSH";
			testBSHGLHeader.AG_AccountNum = "%%%";

			var testTTLGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testTTLGLHeader.AG_AccountType = "TTL";
			testTTLGLHeader.AG_AccountNum = "4444.44.44";

			var testHDRGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testHDRGLHeader.AG_AccountType = "HDR";
			testHDRGLHeader.AG_AccountNum = "^^^";

			Factory.Save();

			Header.AG_AccountType = "BSH";
			Assert("Total Reference should be readonly for all account types except HDR", Header.AG_AG_HeaderDependsOnTotalInfo.ReadOnly);

			Header.AG_AccountType = "HDR";
			Assert("Total Reference should not be read only since this is a HDR account", !Header.AG_AG_HeaderDependsOnTotalInfo.ReadOnly);

			Header.AG_AccountType = "ALT";
			Assert("Total Reference should be readonly for all account types except HDR", Header.AG_AG_HeaderDependsOnTotalInfo.ReadOnly);

			Header.AG_AccountType = "HDR";
			Assert("Total Reference should not be read only since this is a HDR account", !Header.AG_AG_HeaderDependsOnTotalInfo.ReadOnly);

			Assert(!Header.AG_AG_HeaderDependsOnTotalInfo.HasErrors());

			Header.AG_AG_HeaderDependsOnTotal = ZGuid.Empty;
			Assert("Total Reference should not have errors since the Total Reference field is not mandatory", !Header.AG_AG_HeaderDependsOnTotalInfo.HasErrors());

			Header.AG_AccountNum = "5555.55.55";
			Header.AG_AG_HeaderDependsOnTotal = testTTLGLHeader.PK;
			Assert("Total Reference should have error since it references an account that is less than current account", Header.AG_AG_HeaderDependsOnTotalInfo.HasErrors());

			Header.AG_AccountNum = "3333.33.33";
			testHDRGLHeader.AG_AG_HeaderDependsOnTotal = testTTLGLHeader.PK;
			Header.AG_AG_HeaderDependsOnTotal = testTTLGLHeader.PK;
			Assert("Total Reference should have error since it reference account that is already referenced by another HDR account", Header.AG_AG_HeaderDependsOnTotalInfo.HasErrors());

			testHDRGLHeader.AG_AG_HeaderDependsOnTotal = ZGuid.NewZGuid();
			Header.AG_AG_HeaderDependsOnTotal = testTTLGLHeader.PK;
			Assert("Total Reference should not have error since it references account that is greater than current account and is not referenced by other HDR accounts", !Header.AG_AG_HeaderDependsOnTotalInfo.HasErrors());
		}

		public void TestValidateAG_AG_PercentNum()
		{
			AccGLHeader gLHeader = Factory.New<AccGLHeader>();
			gLHeader.AG_AG_PercentNum = gLHeader.PK;
			AssertHasError("Validation Error", gLHeader.AG_AG_PercentNumInfo, "Percentage number (0.) must be greater than Account number (0.).");
			gLHeader.AG_AG_PercentNum = ZGuid.NewZGuid();
			AssertHasError("List Validation Error", gLHeader.AG_AG_PercentNumInfo, "Enter a valid " + gLHeader.AG_AG_PercentNumInfo.Description + ".");
			gLHeader.AG_AG_PercentNum = ZGuid.Empty;
			AssertNoError(gLHeader.AG_AG_PercentNumInfo, "Enter a valid " + gLHeader.AG_AG_PercentNumInfo.Description + ".");
		}

		public void TestValidateAG_Column()
		{
			Header.AG_Column = ZString.Empty;
			AssertHasError(Header.AG_ColumnInfo, "Please enter a GL Section.");

			Header.AG_Column = "XX";
			AssertNoError(Header.AG_ColumnInfo, "Please enter a GL Section.");
			AssertHasError(Header.AG_ColumnInfo, "Enter a valid " + Header.AG_ColumnInfo.Description + ".");

			Header.AG_Column = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;
			AssertNoError(Header.AG_ColumnInfo, "Enter a valid " + Header.AG_ColumnInfo.Description + ".");
			AssertNoError(Header.AG_ColumnInfo, "Please enter a GL Section.");
			AssertNoErrors(Header.AG_ColumnInfo);
		}

		public void TestValidateAG_ControlAccount()
		{
			AssertNull("Precondition : No bank accounts linked to GL Account", Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_AG, Header.PK)));
			Header.AG_ControlAccount = false;
			AssertNoError(Header.AG_ControlAccountInfo, "GL Accounts used on Bank Accounts must be a Control Account.");
			Header.AG_ControlAccount = true;
			AssertNoError(Header.AG_ControlAccountInfo, "GL Accounts used on Bank Accounts must be a Control Account.");

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AG = Header.PK;
			Factory.Save();
			AssertNotNull("Precondition : One bank account linked to GL Account", Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_AG, Header.PK)));
			Header.AG_ControlAccount = false;
			AssertHasError(Header.AG_ControlAccountInfo, "GL Accounts used on Bank Accounts must be a Control Account.");
			Header.AG_ControlAccount = true;
			AssertNoError(Header.AG_ControlAccountInfo, "GL Accounts used on Bank Accounts must be a Control Account.");

			Header.AG_ControlAccount = true;
			Factory.Save();
			AssertNoError(Header.AG_ControlAccountInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Control Account.");
			var mock1 = new Mock<IAccountingRegistryProvider>();
			mock1.Setup(m => m.ElectronicProcessingChargeDisbursementClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock1.Object))
			{
				Header.Validation.ValidateAG_ControlAccount();
				AssertHasError(Header.AG_ControlAccountInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Control Account.");
			}

			Header.AG_ControlAccount = true;
			var mock2 = new Mock<IAccountingRegistryProvider>();
			mock2.Setup(m => m.ElectronicProcessingChargePayableClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock2.Object))
			{
				Header.Validation.ValidateAG_ControlAccount();
				AssertHasError(Header.AG_ControlAccountInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Control Account.");
			}
		}

		[TestDate(2020, 04, 07)]
		public void TestValidateAG_StatisticalUnits()
		{
			AssertNoErrors(Header.AG_StatisticalUnitsInfo);
			Header.AG_AccountType = Core.Constants.AccountType.Note;
			Header.AG_StatisticalUnits = ZString.Empty;
			AssertHasError(Header.AG_StatisticalUnitsInfo, "Please enter a GL Units.");

			foreach (CodeDescriptionPair statisticalUnits in Header.AG_StatisticalUnitsList)
			{
				Header.AG_StatisticalUnits = statisticalUnits.Code;
				AssertNoErrors(Header.AG_StatisticalUnitsInfo);
			}

			Assert("Precondition", !Header.AG_StatisticalUnitsList.ToList<CodeDescriptionPair>().Any(x => x.Code == "DFG"));
			Header.AG_StatisticalUnits = "DFG";
			AssertHasError(Header.AG_StatisticalUnitsInfo, "Enter a valid Units.");

			Assert("Precondition", Header.AG_StatisticalUnitsList.GetAllCodes().Length > 0);
			Header.AG_StatisticalUnits = Header.AG_StatisticalUnitsList.GetAllCodes()[0];

			Factory.Save();

			AssertNoErrors("Precondition", Header.AG_StatisticalUnitsInfo);
			Assert("Precondition", Header.IsInDatabase);
			Assert("Precondition", Header.AG_StatisticalUnitsList.GetAllCodes().Length > 1);
			Header.AG_StatisticalUnits = Header.AG_StatisticalUnitsList.GetAllCodes()[1];
			Assert("Precondition", Header.AG_StatisticalUnitsInfo.HasChanges);
			AssertNoErrors(Header.AG_StatisticalUnitsInfo);

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.General;
			header.AH_TransactionType = TransactionTypes.GLNoteJournal;
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			line.AL_AG = Header.PK;
			line.AL_PostDate = ZDateTime.Today;
			Factory.Save();

			AssertNoErrors("Precondition", Header.AG_StatisticalUnitsInfo);
			Header.AG_StatisticalUnits = Header.AG_StatisticalUnitsList.GetAllCodes()[0];
			Assert("Precondition", Header.AG_StatisticalUnitsInfo.HasChanges);
			AssertHasError(Header.AG_StatisticalUnitsInfo, "This account cannot be converted to units KWH because it is currently in use as a KG. It is used by transaction lines in company EDI posted between 07-Apr-20 00:00:00 and 07-Apr-20 00:00:00.");
		}

		[TestDate(2020, 04, 07)]
		public void TestValidateAG_AccountType_ChangeUsedNote()
		{
			AssertNoErrors(Header.AG_AccountTypeInfo);
			Header.AG_AccountType = Core.Constants.AccountType.Note;
			Assert("Precondition", Header.AG_StatisticalUnitsList.GetAllCodes().Length > 0);
			Header.AG_StatisticalUnits = Header.AG_StatisticalUnitsList.GetAllCodes()[0];
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.General;
			header.AH_TransactionType = TransactionTypes.GLNoteJournal;
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			line.AL_AG = Header.PK;
			line.AL_PostDate = ZDateTime.Today;
			Factory.Save();

			Header.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			AssertEquals("Precondition", Core.Constants.AccountType.Note, Header.AG_AccountTypeInfo.OriginalValue);
			Assert("Precondition", Header.AG_AccountTypeInfo.HasChanges);
			Assert("Precondition", Header.IsInDatabase);
			AssertHasError(Header.AG_AccountTypeInfo, "This account cannot be converted to account type BSH because it is currently in use as a NTE. It is used by transaction lines in company EDI posted between 07-Apr-20 00:00:00 and 07-Apr-20 00:00:00.");
		}

		public void TestValidateAG_AccountType_UsedInRegistry()
		{
			AssertNoErrors(Header.AG_AccountTypeInfo);
			Header.AG_AccountType = Core.Constants.AccountType.Note;
			Header.Validation.ValidateAG_AccountType();
			AssertNoError(Header.AG_AccountTypeInfo, "This is a system defined GL Account used in Electronic Processing Fee management and Account Type must be set to \"BSH - Balance Sheet\" or \"P&L - Profit and Loss\".");
			var mock1 = new Mock<IAccountingRegistryProvider>();
			mock1.Setup(m => m.ElectronicProcessingChargeDisbursementClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock1.Object))
			{
				Header.Validation.ValidateAG_AccountType();
				AssertHasError(Header.AG_AccountTypeInfo, "This is a system defined GL Account used in Electronic Processing Fee management and Account Type must be set to \"BSH - Balance Sheet\" or \"P&L - Profit and Loss\".");
			}

			Header.AG_AccountType = Core.Constants.AccountType.Note;
			var mock2 = new Mock<IAccountingRegistryProvider>();
			mock2.Setup(m => m.ElectronicProcessingChargePayableClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock2.Object))
			{
				Header.Validation.ValidateAG_AccountType();
				AssertHasError(Header.AG_AccountTypeInfo, "This is a system defined GL Account used in Electronic Processing Fee management and Account Type must be set to \"BSH - Balance Sheet\" or \"P&L - Profit and Loss\".");
			}

			Header.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			var mock3 = new Mock<IAccountingRegistryProvider>();
			mock3.Setup(m => m.ElectronicProcessingChargeDisbursementClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock3.Object))
			{
				Header.Validation.ValidateAG_AccountType();
				AssertNoError(Header.AG_AccountTypeInfo, "This is a system defined GL Account used in Electronic Processing Fee management and Account Type must be set to \"BSH - Balance Sheet\" or \"P&L - Profit and Loss\".");
			}

			var mock4 = new Mock<IAccountingRegistryProvider>();
			mock4.Setup(m => m.ElectronicProcessingChargePayableClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock4.Object))
			{
				Header.Validation.ValidateAG_AccountType();
				AssertNoError(Header.AG_AccountTypeInfo, "This is a system defined GL Account used in Electronic Processing Fee management and Account Type must be set to \"BSH - Balance Sheet\" or \"P&L - Profit and Loss\".");
			}
		}

		public void TestCheckAG_DisallowDirectPosting()
		{
			Header.AG_DisallowDirectPosting = false;
			Header.Validation.ValidateAG_DisallowDirectPosting();
			AssertNoError(Header.AG_DisallowDirectPostingInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be allowed for Direct Posting.");
			var mock1 = new Mock<IAccountingRegistryProvider>();
			mock1.Setup(m => m.ElectronicProcessingChargeDisbursementClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock1.Object))
			{
				Header.Validation.ValidateAG_DisallowDirectPosting();
				AssertHasError(Header.AG_DisallowDirectPostingInfo, "This is a system defined GL Account used in Electronic Processing Fee management and cannot be allowed for Direct Posting.");
			}

			Header.AG_DisallowDirectPosting = true;
			AssertNoError(Header.AG_DisallowDirectPostingInfo, "This is a system defined GL Account used in Electronic Processing Fee management and must be allowed for Direct Posting.");
			var mock2 = new Mock<IAccountingRegistryProvider>();
			mock2.Setup(m => m.ElectronicProcessingChargePayableClearingAccount).Returns(Header.PK.ToGuid());
			using (ObjectFactory.Substitute(mock2.Object))
			{
				Header.Validation.ValidateAG_DisallowDirectPosting();
				AssertHasError(Header.AG_DisallowDirectPostingInfo, "This is a system defined GL Account used in Electronic Processing Fee management and must be allowed for Direct Posting.");
			}
		}

		#region implementation

		AccGLHeader Header;

		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.NewWithValidTestData<AccGLHeader>();
		}

		#endregion
	}
}
