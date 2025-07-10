using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccAlternateGLAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAGA_AccountNum()
		{	
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, MGTChart.PK, "LFO", false);
			Factory.Save();

			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AAC_AlternateChart = MGTChart.PK;

			AssertEquals(alternateGLAccount.AGA_AccountNum, string.Empty);
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError("Please enter an Alternate Account."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

			alternateGLAccount.Delete();
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

			MGTChart.AAC_IsFixedLength = true;
			alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, accountNum: "12");
			alternateGLAccount.ReadOnly = false;
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError("The length of Account Number is invalid. Please enter the number with 7 characters."));

			alternateGLAccount.AlternateChart.AAC_IsFixedLength = false;
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError("The length of Account Number is invalid. Please enter the number with 1, or 4, or 7 characters."));

			alternateGLAccount.AGA_AccountNum = "1212";
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError("The Account Number does not matched up with the Account Format '9X99' which is set in Chart MGT."));

			alternateGLAccount.AGA_AccountNum = "1A12A13";
			var newFactory = new BusinessObjectFactory();
			var newCreator = new AccountingTestObjectCreator(newFactory);
			var newAlternateGLAccount = newCreator.CreateAccAlternateGlAccount(MGTChart.PK, accountNum: "1A12A13");
			newFactory.Save();

			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError("Please select another Account Number as this one is already used."));

			alternateGLAccount.AGA_AccountType = "GRP";
			alternateGLAccount.AGA_AccountNum = "1A12A14";
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError("The Account type RUP/GRP must be the first GL Account of a GL Account Tier and have child GL Accounts."));

			alternateGLAccount.AGA_AccountNum = "1A12";
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasError("The Account type RUP/GRP must be the first GL Account of a GL Account Tier and have child GL Accounts."));

			alternateGLAccount.AGA_AccountNum = "1";
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasError("The Account type RUP/GRP must be the first GL Account of a GL Account Tier and have child GL Accounts."));

			newAlternateGLAccount.Delete();
			alternateGLAccount = newCreator.CreateAccAlternateGlAccount(MGTChart.PK, accountNum: "1A22A23");
			var attribute = newCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, attributeValue: AccountingMasterFilesConstants.LFOCodes.FOR);
			newFactory.Save();

			var newFactory2 = new BusinessObjectFactory();
			newCreator = new AccountingTestObjectCreator(newFactory2);

			attribute = newCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFECodes.WEU);
			newFactory2.Save();

			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '1A22A23' already existed in the Alternate Chart 'MGT' and mapped to Parent Account '10.00.1010'.
It cannot be mapped to a different Parent Account."));

			attribute.Delete();
			newFactory2.Save();
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '1A22A23' already existed in the Alternate Chart 'MGT' and cannot be linked to a different Parent Account as there is a mis-match in the Cash Flow Cat. and/or Units.
Please enter a different value."));

			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());
		}

		public void TestValidateCashFlowTypeAndUnitMessage()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, accountNum: "1A22A23");
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			GLHeader2.AG_CashFlowType = CashFlowCodeLists.Codes.F06;
			Factory.Save();
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, attribute: "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '1A22A23' already existed in the Alternate Chart 'MGT' and cannot be linked to a different Parent Account as there is a mis-match in the Cash Flow Cat.
Please enter a different value."));

			attribute.Delete();
			GLHeader2.AG_CashFlowType = "";
			Factory.Save();
			attribute = Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, attribute: "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '1A22A23' already existed in the Alternate Chart 'MGT' and cannot be linked to a different Parent Account as there is a mis-match in the Cash Flow Cat.
Please enter a different value."));
		}

		public void TestMoreThanOneNTEGLAccountsNotAllowMapToOneAlternateGLAccount()
		{
			TRRChart = Creator.CreateAlternateChart("TRR", "TRR Desc");
			Creator.CreateAccAlternateChartFormat(TRRChart, 1, "99", "2", ".");
			var glHeader = Creator.CreateAccGLHeader("1333.33.33", Core.Constants.AccountType.Note);
			var glHeader2 = Creator.CreateAccGLHeader("1333.33.44", Core.Constants.AccountType.Note);
			Factory.Save();
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(TRRChart.PK, "11", Core.Constants.AccountType.Note);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, 1, "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader2.PK, 1, "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '11' already existed and cannot be linked to a different Parent Account as NTE Alternate GL Account cannot be mapped from multiple NTE Parent Accounts.
Please enter a different value."));
		}

		public void TestMultipleGLAccountsAllowMapToOneAlternateGLAccountWithoutSpecifiedDissectionConfiguration()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, accountNum: "1A22A23");
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 1, "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, 1, "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

			var newFactory = new BusinessObjectFactory();
			var newCreator = new AccountingTestObjectCreator(newFactory);
			TRRChart = newCreator.CreateAlternateChart("TRR", "TRR Desc");
			newCreator.CreateAccAlternateChartFormat(TRRChart, 1, "99", "2", ".");
			var glHeader = newCreator.CreateAccGLHeader("1333.33.33");
			var glHeader2 = newCreator.CreateAccGLHeader("1333.33.44");
			newCreator.CreateAccAlternateGLAccountDissection(glHeader, MGTChart.PK, "LFO", false);
			newFactory.Save();

			var newAlternateGLAccount = newCreator.CreateAccAlternateGlAccount(TRRChart.PK, accountNum: "11");
			newCreator.CreateAccAlternateGlAccountAttribute(newAlternateGLAccount, glHeader.PK, 1, "");
			newAlternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!newAlternateGLAccount.AGA_AccountNumInfo.HasErrors());

			newCreator.CreateAccAlternateGlAccountAttribute(newAlternateGLAccount, glHeader2.PK, 1, "");
			newAlternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!newAlternateGLAccount.AGA_AccountNumInfo.HasErrors());
		}

		public void TestMultipleGLAccountsNotAllowMapToOneAlternateGLAccountWithSpecifiedDissectionConfiguration_NoDissectionConfigurationParentMappedFirst()
		{
			TRRChart = Creator.CreateAlternateChart("TRR", "TRR Desc");
			Creator.CreateAccAlternateChartFormat(TRRChart, 1, "99", "2", ".");
			var glHeader = Creator.CreateAccGLHeader("1333.33.33");
			var glHeader2 = Creator.CreateAccGLHeader("1333.33.44");
			Creator.CreateAccAlternateGLAccountDissection(glHeader, TRRChart.PK, "LFO", false);
			Factory.Save();

			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(TRRChart.PK, accountNum: "11");
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader2.PK, 1, "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());
			Factory.Save();

			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, 1, "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '11' already existed in the Alternate Chart 'TRR' and mapped to Parent Account '1333.33.44'.
It cannot be mapped to a different Parent Account."));
		}

		public void TestValidateAGA_Description()
		{
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();

			AssertEquals(alternateGLAccount.AGA_Description, string.Empty);
			alternateGLAccount.Validation.ValidateAGA_Description();
			Assert(alternateGLAccount.AGA_DescriptionInfo.HasError("Please enter an Alternate Account Name."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_Description();
			Assert(!alternateGLAccount.AGA_DescriptionInfo.HasErrors());

			alternateGLAccount.ReadOnly = false;
			alternateGLAccount.AGA_Description = "TEST";
			alternateGLAccount.Validation.ValidateAGA_Description();
			Assert(!alternateGLAccount.AGA_DescriptionInfo.HasErrors());
		}

		public void TestValidateAGA_AGA_PercentNum()
		{
			SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.BalanceSheetAccount, out var clnAlternateGLAccount, out var otherAlternateGLAccount, out var cln2AlternateGLAccount);

			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, "4D33", Core.Constants.AccountType.BalanceSheetAccount, description: "DES3", percentNum: otherAlternateGLAccount.PK);
			alternateGLAccount.Validation.ValidateAGA_AGA_PercentNum();
			Assert(alternateGLAccount.AGA_AGA_PercentNumInfo.HasError("Invalid Percent Account. A valid Percent Account must be 'CLN' or 'TTL' account type."));

			alternateGLAccount.AGA_AGA_PercentNum = clnAlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_PercentNum();
			Assert(!alternateGLAccount.AGA_AGA_AlternateNumInfo.HasErrors());

			alternateGLAccount.AGA_AGA_PercentNum = cln2AlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_PercentNum();
			Assert(alternateGLAccount.AGA_AGA_PercentNumInfo.HasError("The alternate account '3F99 - DES2' cannot be chosen here as it belongs to a different chart 'TRR - TRR Desc', please choose another 'CLN or TTL' alternate account from the same chart 'MGT - Management Reporting'."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_AGA_PercentNum();
			Assert(!alternateGLAccount.AGA_AGA_PercentNumInfo.HasErrors());
		}

		public void TestValidateAGA_AGA_AlternateNum()
		{
			SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Alternate, Core.Constants.AccountType.BalanceSheetAccount, out var alternateAlternateGLAccount, out var otherAlternateGLAccount, out var alt2AlternateGLAccount);
			otherAlternateGLAccount.AGA_AGA_AlternateNum = alternateAlternateGLAccount.PK;
			otherAlternateGLAccount.Factory.Save();

			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(TRRChart.PK, "1", Core.Constants.AccountType.BalanceSheetAccount, alternateNum: alternateAlternateGLAccount.PK);
			alternateGLAccount.Validation.ValidateAGA_AGA_AlternateNum();
			Assert(alternateGLAccount.AGA_AGA_AlternateNumInfo.HasError("The Alternate Number is already used by another Alternate GL account 3F99."));

			alternateGLAccount.AGA_AGA_AlternateNum = otherAlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_AlternateNum();
			Assert(alternateGLAccount.AGA_AGA_AlternateNumInfo.HasError("Invalid Alternate Account. A valid Alternate Account must be 'ALT' account type."));

			alternateGLAccount.AGA_AGA_AlternateNum = alt2AlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_AlternateNum();
			Assert(!alternateGLAccount.AGA_AGA_AlternateNumInfo.HasErrors());

			otherAlternateGLAccount.AGA_AGA_AlternateNum = ZGuid.Empty;
			otherAlternateGLAccount.Factory.Save();
			alternateGLAccount.AGA_AGA_AlternateNum = alternateAlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_AlternateNum();
			Assert(alternateGLAccount.AGA_AGA_AlternateNumInfo.HasError("The alternate account '2F99 - DES1' cannot be chosen here as it belongs to a different chart 'MGT - Management Reporting', please choose another 'ALT' alternate account from the same chart 'TRR - TRR Desc'."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_AGA_AlternateNum();
			Assert(!alternateGLAccount.AGA_AGA_AlternateNumInfo.HasErrors());
		}

		public void TestValidateAGA_AGA_ConsolidationNum()
		{
			SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.BalanceSheetAccount, out var consolidateAccount, out var otherAlternateGLAccount, out var cln2AlternateGLAccount);

			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AAC_AlternateChart = TRRChart.PK;
			alternateGLAccount.AGA_AGA_ConsolidationNum = otherAlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_ConsolidationNum();
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.HasError("Invalid Consolidate Account. A valid Consolidate Account must be 'CLN' account type."));

			alternateGLAccount.AGA_AGA_ConsolidationNum = cln2AlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_ConsolidationNum();
			Assert(!alternateGLAccount.AGA_AGA_ConsolidationNumInfo.HasErrors());

			alternateGLAccount.AGA_AGA_ConsolidationNum = consolidateAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_ConsolidationNum();
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.HasError("The alternate account '2F99 - DES1' cannot be chosen here as it belongs to a different chart 'MGT - Management Reporting', please choose another 'CLN' alternate account from the same chart 'TRR - TRR Desc'."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_AGA_ConsolidationNum();
			Assert(!alternateGLAccount.AGA_AGA_ConsolidationNumInfo.HasErrors());
		}

		public void TestValidateAGA_AGA_HeaderDependsOnTotal()
		{
			SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Total, Core.Constants.AccountType.BalanceSheetAccount, out var ttlAlternateGLAccount, out var otherAlternateGLAccount, out var ttl2AlternateGLAccount);

			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(TRRChart.PK, "1", Core.Constants.AccountType.Header, totalReference: otherAlternateGLAccount.PK);
			alternateGLAccount.Validation.ValidateAGA_AGA_HeaderDependsOnTotal();
			Assert(alternateGLAccount.AGA_AGA_HeaderDependsOnTotalInfo.HasError("Invalid Total Reference. A valid Total Reference must be 'TTL' account type."));

			alternateGLAccount.AGA_AGA_HeaderDependsOnTotal = ttl2AlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_HeaderDependsOnTotal();
			Assert(!alternateGLAccount.AGA_AGA_HeaderDependsOnTotalInfo.HasErrors());

			alternateGLAccount.AGA_AGA_HeaderDependsOnTotal = ttlAlternateGLAccount.PK;
			alternateGLAccount.Validation.ValidateAGA_AGA_HeaderDependsOnTotal();
			Assert(alternateGLAccount.AGA_AGA_HeaderDependsOnTotalInfo.HasError("The alternate account '2F99 - DES1' cannot be chosen here as it belongs to a different chart 'MGT - Management Reporting', please choose another 'TTL' alternate account from the same chart 'TRR - TRR Desc'."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_AGA_HeaderDependsOnTotal();
			Assert(!alternateGLAccount.AGA_AGA_HeaderDependsOnTotalInfo.HasErrors());
		}

		void SetUpForPercentAndConsolidateAndAlternateAndTotalReference(string validAccountType, string otherAccountType, out AccAlternateGLAccount notCurrentChartAlternateGLAccount, out AccAlternateGLAccount otherAlternateGLAccount, out AccAlternateGLAccount currentChartAlternateGLAccount)
		{
			TRRChart = Creator.CreateAlternateChart("TRR", "TRR Desc");
			Creator.CreateAccAlternateChartFormat(TRRChart, 1, "9", "2", ".");
			Factory.Save();
			notCurrentChartAlternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, "2F99", validAccountType, description: "DES1", drCR: "DR", printSequence: 1);
			otherAlternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, "3F99", otherAccountType, description: "DES2", drCR: "DR", printSequence: 2);
			currentChartAlternateGLAccount = Creator.CreateAccAlternateGlAccount(TRRChart.PK, "3F99", validAccountType, description: "DES2", drCR: "DR", printSequence: 2);

			Factory.Save();
		}

		public void TestControlAccountCannotBeMappedToAlternateGLAccountWhichIsMappedByMultipleGLAccounts()
		{
			var list = new List<IRegistryItem>();
			list.AddRange(AccountingMasterFilesUtils.NotAllowedForDissectionControlAccount);

			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, "3X11X01", Core.Constants.AccountType.BalanceSheetAccount, description: "test", drCR: Core.Constants.DebitCredit.Debit, reportSection: AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, totalLevel: 0, printSequence: 5);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			Factory.Save();
			AccAlternateGLAccountAttribute attribute;

			foreach (var controlAccount in list)
			{
				using (controlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader.PK.ToGuid()))
				{
					Validate();
					attribute.Delete();
				}
			}

			void Validate()
			{
				alternateGLAccount.Validation.ValidateAGA_AccountNum();
				Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

				attribute = Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, attribute: "");
				alternateGLAccount.Validation.ValidateAGA_AccountNum();
				Assert(alternateGLAccount.AGA_AccountNumInfo.HasErrors());
				Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '3X11X01' already existed in the Alternate Chart 'MGT' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.
Please enter a different value."));
			}
		}

		public void TestControlAccountCannotBeMappedToAlternateGLAccountWhichIsMappedByMultipleGLAccounts2()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, "3X11X01", Core.Constants.AccountType.BalanceSheetAccount, description: "test", drCR: Core.Constants.DebitCredit.Debit, reportSection: AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, totalLevel: 0, printSequence: 5);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			Factory.Save();

			var attribute = Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, attribute: "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForSeparateNumbering(It.IsAny<ZGuid>())).Returns(false);
			mock.Setup(m => m.IsNotAllowedForDissectionAttributes(It.IsAny<ZGuid>())).Returns(false);
			Validate(false);

			mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForDissectionAttributes(It.IsAny<ZGuid>())).Returns(true);
			Validate();

			mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForSeparateNumbering(It.IsAny<ZGuid>())).Returns(true);
			Validate();

			void Validate(bool expectError = true)
			{
				using (ObjectFactory.Substitute(mock.Object))
				{
					alternateGLAccount.Validation.ValidateAGA_AccountNum();

					if (expectError)
					{
						Assert(alternateGLAccount.AGA_AccountNumInfo.HasErrors());
						Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '3X11X01' already existed in the Alternate Chart 'MGT' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.
Please enter a different value."));
					}
					else
					{
						Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());
					}
				}
			}
		}

		public void TestGLAccountCannotMaptoAlternateGLAccountWhichIsMappeedByDissection()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, "3X11X01", Core.Constants.AccountType.BalanceSheetAccount, description: "test", drCR: Core.Constants.DebitCredit.Debit, reportSection: AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, totalLevel: 0, printSequence: 5);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, MGTChart.PK, "LFE", false);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			Factory.Save();

			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());
			
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, attribute: "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '3X11X01' already existed in the Alternate Chart 'MGT' and mapped to Parent Account '10.00.1010'.
It cannot be mapped to a different Parent Account."));
		}

		public void TestGLAccountCannotMapToAlternateGLAccountWhichLinkedToBankAccount()
		{
			Creator.CreateBankAccount("AAA", "AAA", RefCurrency.LoadFromCurrencyCode(Factory, "CNY"), GLHeader);
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(MGTChart.PK, "3X11X01", Core.Constants.AccountType.BalanceSheetAccount, description: "test", drCR: Core.Constants.DebitCredit.Debit, reportSection: AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, totalLevel: 0, printSequence: 5);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			Factory.Save();

			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(!alternateGLAccount.AGA_AccountNumInfo.HasErrors());

			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, attribute: "");
			alternateGLAccount.Validation.ValidateAGA_AccountNum();
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasErrors());
			Assert(alternateGLAccount.AGA_AccountNumInfo.HasError(@"The specified Alternate Account's Number '3X11X01' already existed in the Alternate Chart 'MGT' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.
Please enter a different value."));
		}

		public void TestAGA_TotalLevel()
		{
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AccountType = "TTL";
			alternateGLAccount.AGA_TotalLevel = -10;
			alternateGLAccount.Validation.ValidateAGA_TotalLevel();
			Assert(alternateGLAccount.AGA_TotalLevelInfo.HasError("Total Level should be between 1 and 999."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_TotalLevel();
			Assert(!alternateGLAccount.AGA_TotalLevelInfo.HasErrors());

			alternateGLAccount.ReadOnly = false;
			alternateGLAccount.AGA_TotalLevel = 10;
			alternateGLAccount.Validation.ValidateAGA_TotalLevel();
			Assert(!alternateGLAccount.AGA_TotalLevelInfo.HasErrors());
		}

		public void TestAGA_PrintSequence()
		{
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccount.AGA_PrintSequence = -10;
			alternateGLAccount.Validation.ValidateAGA_PrintSequence();
			Assert(alternateGLAccount.AGA_PrintSequenceInfo.HasError("Print Sequence should be between 0 and 999."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_PrintSequence();
			Assert(!alternateGLAccount.AGA_PrintSequenceInfo.HasErrors());

			alternateGLAccount.ReadOnly = false;
			alternateGLAccount.AGA_PrintSequence = 10;
			alternateGLAccount.Validation.ValidateAGA_PrintSequence();
			Assert(!alternateGLAccount.AGA_PrintSequenceInfo.HasErrors());
		}

		public void TestAGA_AccountType()
		{
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();

			AssertEquals(alternateGLAccount.AGA_AccountType, string.Empty);
			alternateGLAccount.Validation.ValidateAGA_AccountType();
			Assert(alternateGLAccount.AGA_AccountTypeInfo.HasError("Please enter an Account Type."));

			alternateGLAccount.AGA_AccountType = "XXX";
			alternateGLAccount.Validation.ValidateAGA_AccountType();
			Assert(alternateGLAccount.AGA_AccountTypeInfo.HasError("Enter a valid Account Type."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_AccountType();
			Assert(!alternateGLAccount.AGA_AccountTypeInfo.HasErrors());

			alternateGLAccount.ReadOnly = false;
			alternateGLAccount.AGA_AccountType = "BSH";
			alternateGLAccount.AGA_AccountNum = "2F99";
			alternateGLAccount.AGA_AAC_AlternateChart = MGTChart.PK;
			alternateGLAccount.AGA_Description = "DES1";
			alternateGLAccount.AGA_DebitCredit = "DR";
			alternateGLAccount.AGA_PrintSequence = 1;
			Factory.Save();

			alternateGLAccount.Validation.ValidateAGA_AccountType();
			Assert(!alternateGLAccount.AGA_AccountTypeInfo.HasErrors());
		}

		public void TestValidateAGA_DebitCredit()
		{
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();

			AssertEquals(alternateGLAccount.AGA_DebitCredit, string.Empty);
			alternateGLAccount.Validation.ValidateAGA_DebitCredit();
			Assert(alternateGLAccount.AGA_DebitCreditInfo.HasError("Please enter a DR/CR."));

			alternateGLAccount.AGA_DebitCredit = "XX";
			alternateGLAccount.Validation.ValidateAGA_DebitCredit();
			Assert(alternateGLAccount.AGA_DebitCreditInfo.HasError("Enter a valid DR/CR."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_DebitCredit();
			Assert(!alternateGLAccount.AGA_DebitCreditInfo.HasErrors());

			alternateGLAccount.ReadOnly = false;
			alternateGLAccount.AGA_DebitCredit = "DR";
			alternateGLAccount.Validation.ValidateAGA_DebitCredit();
			Assert(!alternateGLAccount.AGA_DebitCreditInfo.HasErrors());
		}

		public void TestValidateAGA_ReportSection()
		{
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();

			alternateGLAccount.AGA_ReportSection = string.Empty;
			alternateGLAccount.Validation.ValidateAGA_ReportSection();
			Assert(alternateGLAccount.AGA_ReportSectionInfo.HasError("Please enter a Report Section."));

			alternateGLAccount.AGA_ReportSection = "XX";
			alternateGLAccount.Validation.ValidateAGA_ReportSection();
			Assert(alternateGLAccount.AGA_ReportSectionInfo.HasError("Enter a valid Report Section."));

			alternateGLAccount.ReadOnly = true;
			alternateGLAccount.Validation.ValidateAGA_ReportSection();
			Assert(!alternateGLAccount.AGA_ReportSectionInfo.HasErrors());

			alternateGLAccount.ReadOnly = false;
			alternateGLAccount.AGA_ReportSection = "OV";
			alternateGLAccount.Validation.ValidateAGA_ReportSection();
			Assert(!alternateGLAccount.AGA_ReportSectionInfo.HasErrors());
		}

		#region Implement

		protected override void SetUp()
		{
			base.SetUp();

			MGTChart = Creator.CreateAlternateChart("MGT", "Management Reporting");
			Creator.CreateAccAlternateChartFormat(MGTChart, 1, "9", "2");
			Creator.CreateAccAlternateChartFormat(MGTChart, 2, "X99", "2");
			Creator.CreateAccAlternateChartFormat(MGTChart, 3, "X99", "2");

			GLHeader = Creator.CreateAccGLHeader("10.00.1010", Core.Constants.AccountType.BalanceSheetAccount);
			GLHeader2 = Creator.CreateAccGLHeader("20.00.1010", Core.Constants.AccountType.BalanceSheetAccount);
			Factory.Save();
		}

		#endregion

		AccAlternateChart MGTChart;
		AccAlternateChart TRRChart;
		AccGLHeader GLHeader;
		AccGLHeader GLHeader2;
		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}
