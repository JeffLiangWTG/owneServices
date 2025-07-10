using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLAccountDescriptor))]
	public class AccGLAccountDescriptorTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAJ_Language_List()
		{
			CodeDescriptionPairList languages = new CodeDescriptionPairList(OLookUpEditType.Language);
			AssertEquals("Count", languages.Count + 1, TestAccGLAccountDescriptor.AJ_Language_List.Count);
			AssertEquals("GetDescriptionFromCode(\"ZZZ\")", "External Link to General Ledger", TestAccGLAccountDescriptor.AJ_Language_List.GetDescriptionFromCode("ZZZ"));
		}

		public void TestCircularFKReferenceSavesCorrectly()
		{
			//basic
			{
				var descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				descriptor1.AJ_AccountDescription = "1";
				descriptor2.AJ_AccountDescription = "2";
				descriptor1.AJ_AJ_AlternativeNum = descriptor2.PK;
				descriptor2.AJ_AJ_AlternativeNum = descriptor1.PK;
				Factory.Save();
				AssertEquals(descriptor1.AJ_AJ_AlternativeNum, descriptor2.PK);
				AssertEquals(descriptor2.AJ_AJ_AlternativeNum, descriptor1.PK);
				var factory2 = new BusinessObjectFactory();
				var descriptor1Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor1.PK);
				var descriptor2Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor2.PK);
				AssertEquals(descriptor1Reloaded.AJ_AJ_AlternativeNum, descriptor2.PK);
				AssertEquals(descriptor2Reloaded.AJ_AJ_AlternativeNum, descriptor1.PK);
			}

			//intermediate
			{
				var descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor3 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor4 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor5 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				descriptor1.AJ_AccountDescription = "1";
				descriptor2.AJ_AccountDescription = "2";
				descriptor3.AJ_AccountDescription = "3";
				descriptor4.AJ_AccountDescription = "4";
				descriptor5.AJ_AccountDescription = "5";
				descriptor1.AJ_AJ_AlternativeNum = descriptor2.PK;
				descriptor2.AJ_AJ_CarriedForwardAccount = descriptor3.PK;
				descriptor3.AJ_AJ_ConsolidationNum = descriptor4.PK;
				descriptor4.AJ_AJ_HeaderDependsOnTotal = descriptor5.PK;
				descriptor5.AJ_AJ_PercentNum = descriptor1.PK;
				Factory.Save();
				AssertEquals(descriptor1.AJ_AJ_AlternativeNum, descriptor2.PK);
				AssertEquals(descriptor2.AJ_AJ_CarriedForwardAccount, descriptor3.PK);
				AssertEquals(descriptor3.AJ_AJ_ConsolidationNum, descriptor4.PK);
				AssertEquals(descriptor4.AJ_AJ_HeaderDependsOnTotal, descriptor5.PK);
				AssertEquals(descriptor5.AJ_AJ_PercentNum, descriptor1.PK);
				var factory2 = new BusinessObjectFactory();
				var descriptor1Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor1.PK);
				var descriptor2Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor2.PK);
				var descriptor3Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor3.PK);
				var descriptor4Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor4.PK);
				var descriptor5Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor5.PK);
				AssertEquals(descriptor1Reloaded.AJ_AJ_AlternativeNum, descriptor2.PK);
				AssertEquals(descriptor2Reloaded.AJ_AJ_CarriedForwardAccount, descriptor3.PK);
				AssertEquals(descriptor3Reloaded.AJ_AJ_ConsolidationNum, descriptor4.PK);
				AssertEquals(descriptor4Reloaded.AJ_AJ_HeaderDependsOnTotal, descriptor5.PK);
				AssertEquals(descriptor5Reloaded.AJ_AJ_PercentNum, descriptor1.PK);
			}

			//advanced
			{
				var descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor3 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor4 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				var descriptor5 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
				descriptor1.AJ_AccountDescription = "1";
				descriptor2.AJ_AccountDescription = "2";
				descriptor3.AJ_AccountDescription = "3";
				descriptor4.AJ_AccountDescription = "4";
				descriptor5.AJ_AccountDescription = "5";
				descriptor1.AJ_AJ_AlternativeNum = descriptor2.PK;
				descriptor2.AJ_AJ_AlternativeNum = descriptor3.PK;
				descriptor3.AJ_AJ_AlternativeNum = descriptor4.PK;
				descriptor4.AJ_AJ_AlternativeNum = descriptor5.PK;
				descriptor5.AJ_AJ_AlternativeNum = descriptor1.PK;
				descriptor1.AJ_AJ_CarriedForwardAccount = descriptor5.PK;
				descriptor2.AJ_AJ_CarriedForwardAccount = descriptor1.PK;
				descriptor3.AJ_AJ_CarriedForwardAccount = descriptor2.PK;
				descriptor4.AJ_AJ_CarriedForwardAccount = descriptor3.PK;
				descriptor5.AJ_AJ_CarriedForwardAccount = descriptor4.PK;
				descriptor1.AJ_AJ_ConsolidationNum = descriptor3.PK;
				descriptor2.AJ_AJ_ConsolidationNum = descriptor4.PK;
				descriptor3.AJ_AJ_ConsolidationNum = descriptor5.PK;
				descriptor4.AJ_AJ_ConsolidationNum = descriptor1.PK;
				descriptor5.AJ_AJ_ConsolidationNum = descriptor2.PK;
				Factory.Save();
				AssertEquals(descriptor1.AJ_AJ_AlternativeNum, descriptor2.PK);
				AssertEquals(descriptor2.AJ_AJ_AlternativeNum, descriptor3.PK);
				AssertEquals(descriptor3.AJ_AJ_AlternativeNum, descriptor4.PK);
				AssertEquals(descriptor4.AJ_AJ_AlternativeNum, descriptor5.PK);
				AssertEquals(descriptor5.AJ_AJ_AlternativeNum, descriptor1.PK);
				AssertEquals(descriptor1.AJ_AJ_CarriedForwardAccount, descriptor5.PK);
				AssertEquals(descriptor2.AJ_AJ_CarriedForwardAccount, descriptor1.PK);
				AssertEquals(descriptor3.AJ_AJ_CarriedForwardAccount, descriptor2.PK);
				AssertEquals(descriptor4.AJ_AJ_CarriedForwardAccount, descriptor3.PK);
				AssertEquals(descriptor5.AJ_AJ_CarriedForwardAccount, descriptor4.PK);
				AssertEquals(descriptor1.AJ_AJ_ConsolidationNum, descriptor3.PK);
				AssertEquals(descriptor2.AJ_AJ_ConsolidationNum, descriptor4.PK);
				AssertEquals(descriptor3.AJ_AJ_ConsolidationNum, descriptor5.PK);
				AssertEquals(descriptor4.AJ_AJ_ConsolidationNum, descriptor1.PK);
				AssertEquals(descriptor5.AJ_AJ_ConsolidationNum, descriptor2.PK);
				var factory2 = new BusinessObjectFactory();
				var descriptor1Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor1.PK);
				var descriptor2Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor2.PK);
				var descriptor3Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor3.PK);
				var descriptor4Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor4.PK);
				var descriptor5Reloaded = factory2.Load<AccGLAccountDescriptor>(descriptor5.PK);
				AssertEquals(descriptor1Reloaded.AJ_AJ_AlternativeNum, descriptor2.PK);
				AssertEquals(descriptor2Reloaded.AJ_AJ_AlternativeNum, descriptor3.PK);
				AssertEquals(descriptor3Reloaded.AJ_AJ_AlternativeNum, descriptor4.PK);
				AssertEquals(descriptor4Reloaded.AJ_AJ_AlternativeNum, descriptor5.PK);
				AssertEquals(descriptor5Reloaded.AJ_AJ_AlternativeNum, descriptor1.PK);
				AssertEquals(descriptor1Reloaded.AJ_AJ_CarriedForwardAccount, descriptor5.PK);
				AssertEquals(descriptor2Reloaded.AJ_AJ_CarriedForwardAccount, descriptor1.PK);
				AssertEquals(descriptor3Reloaded.AJ_AJ_CarriedForwardAccount, descriptor2.PK);
				AssertEquals(descriptor4Reloaded.AJ_AJ_CarriedForwardAccount, descriptor3.PK);
				AssertEquals(descriptor5Reloaded.AJ_AJ_CarriedForwardAccount, descriptor4.PK);
				AssertEquals(descriptor1Reloaded.AJ_AJ_ConsolidationNum, descriptor3.PK);
				AssertEquals(descriptor2Reloaded.AJ_AJ_ConsolidationNum, descriptor4.PK);
				AssertEquals(descriptor3Reloaded.AJ_AJ_ConsolidationNum, descriptor5.PK);
				AssertEquals(descriptor4Reloaded.AJ_AJ_ConsolidationNum, descriptor1.PK);
				AssertEquals(descriptor5Reloaded.AJ_AJ_ConsolidationNum, descriptor2.PK);
			}
		}

		public void TestAGListForSetReportCategory()
		{
			var testGLAccountForBSH = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccountForBSH.AG_AccountNum = "1111.11.11";
			testGLAccountForBSH.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;

			var testGLAccountForPL = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccountForPL.AG_AccountNum = "2222.22.22";
			testGLAccountForPL.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			var testGLAccountForNTE = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccountForNTE.AG_AccountNum = "3333.33.33";
			testGLAccountForNTE.AG_AccountType = AccountTypeComboBoxConstants.Note;

			AssertAGList(AccountTypeComboBoxConstants.BalanceSheetAccount, testGLAccountForBSH.PK, testGLAccountForPL.PK, testGLAccountForNTE.PK);
			AssertAGList(AccountTypeComboBoxConstants.ProfitAndLossAccount, testGLAccountForPL.PK, testGLAccountForBSH.PK, testGLAccountForNTE.PK);
			AssertAGList(AccountTypeComboBoxConstants.Note, testGLAccountForNTE.PK, testGLAccountForPL.PK, testGLAccountForBSH.PK);
		}

		void AssertAGList(ZString reportCategory, ZGuid glHeaderPKForContains, params ZGuid[] glHeaderPKsForNotContain)
		{
			var descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = reportCategory;
			descriptor.ParentGLHeaderPK_List.Load();

			foreach (var glHeaderPK in glHeaderPKsForNotContain)
			{
				var accountFoundForNotContain = descriptor.ParentGLHeaderPK_List.FindByPK(glHeaderPK);
				AssertNull(accountFoundForNotContain);
			}

			var accountFoundForContains = descriptor.ParentGLHeaderPK_List.FindByPK(glHeaderPKForContains);
			AssertEquals(glHeaderPKForContains, accountFoundForContains.PK);
		}

		public void TestReadOnlyWhenReportTypeHDR()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;

			Assert(descriptor.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);

			Assert(descriptor.AJ_TotalLevelInfo.ReadOnly);

			Assert(descriptor.ParentGLHeaderPKInfo.ReadOnly);
		}

		public void TestReadOnlyWhenReportTypeCLN()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Consolidation;

			Assert(!descriptor.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);

			Assert(descriptor.AJ_TotalLevelInfo.ReadOnly);

			Assert(descriptor.ParentGLHeaderPKInfo.ReadOnly);
		}

		public void TestReadOnlyWhenReportTypeALT()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Alternate;

			Assert(!descriptor.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);

			Assert(descriptor.AJ_TotalLevelInfo.ReadOnly);

			Assert(descriptor.ParentGLHeaderPKInfo.ReadOnly);
		}

		public void TestReadOnlyWhenReportTypeCFW()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_ReportCategory = "CFW";

			Assert(!descriptor.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);

			Assert(descriptor.AJ_TotalLevelInfo.ReadOnly);

			Assert(descriptor.ParentGLHeaderPKInfo.ReadOnly);
		}

		public void TestReadOnlyWhenReportTypeTTL()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Total;

			Assert(!descriptor.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);

			Assert(!descriptor.AJ_TotalLevelInfo.ReadOnly);

			Assert(descriptor.ParentGLHeaderPKInfo.ReadOnly);
		}

		public void TestReadOnlyWhenReportTypeBSH()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;

			Assert(!descriptor.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);

			Assert(descriptor.AJ_TotalLevelInfo.ReadOnly);

			Assert(!descriptor.ParentGLHeaderPKInfo.ReadOnly);
		}

		public void TestReadOnlyWhenReportTypePL()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			Assert(!descriptor.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert(!descriptor.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert(descriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);

			Assert(descriptor.AJ_TotalLevelInfo.ReadOnly);

			Assert(!descriptor.ParentGLHeaderPKInfo.ReadOnly);
		}

		public void TestCLNAccountTypeValidation()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_AJ_ConsolidationNum = AlternativeLocalAccount.PK;
			IEnumerable<INotification> error = descriptor.AJ_AJ_ConsolidationNumInfo.GetErrors();
			Assert(error.Count() == 1);
			AssertEquals("Consolidated Account must be type of Consolidated.", error.GetFirstMessage());
		}

		public void TestCLNAccountTypeValidationForRightType()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_AJ_ConsolidationNum = ConsolidatedLocalAccount.PK;
			IEnumerable<INotification> error = descriptor.AJ_AJ_ConsolidationNumInfo.GetErrors();
			Assert(error.Count() == 0);
		}

		public void TestCLNAccountNumber()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			ConsolidatedLocalAccount.AJ_LocalAccountNumber = "1233.000";
			descriptor.AJ_AJ_ConsolidationNum = ConsolidatedLocalAccount.PK;

			descriptor.AJ_LocalAccountNumber = "1234.000";

			IEnumerable<INotification> error = descriptor.AJ_LocalAccountNumberInfo.GetErrors();
			AssertEquals(1, error.Count());
			AssertEquals("Account number must be less than Consolidation number", error.GetFirstMessage());
		}

		public void TestALTAccountTypeValidationForRightType()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_AJ_AlternativeNum = AlternativeLocalAccount.PK;
			IEnumerable<INotification> error = descriptor.AJ_AJ_ConsolidationNumInfo.GetErrors();
			Assert(error.Count() == 0);
		}

		public void TestALTAccountTypeValidation()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_AJ_AlternativeNum = ConsolidatedLocalAccount.PK;
			IEnumerable<INotification> error = descriptor.AJ_AJ_AlternativeNumInfo.GetErrors();
			Assert(error.Count() > 0);
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.HasError("Alternate Account must be type of Alternate."));
		}

		public void TestParentAccountValidationIfTypeConsolidated()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Consolidation;

			descriptor.ParentGLHeaderPK = Guid.Empty;
			IEnumerable<INotification> error = descriptor.ParentGLHeaderPKInfo.GetErrors();
			Assert(error.Count() == 0);
		}

		public void TestParentAccountValidationIfTypeAlternate()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Alternate;

			descriptor.ParentGLHeaderPK = Guid.Empty;
			IEnumerable<INotification> error = descriptor.ParentGLHeaderPKInfo.GetErrors();
			Assert(error.Count() == 0);
		}

		public void TestAlternateCorrectAccountDebitCredit()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			AlternativeLocalAccount.AJ_DebitCredit = "DR";
			descriptor.AJ_AJ_AlternativeNum = AlternativeLocalAccount.PK;

			descriptor.AJ_DebitCredit = "CR";

			Assert(!descriptor.AJ_DebitCreditInfo.HasError("Alternate account debit & credit must be opposite to the current setting."));
			Assert(!descriptor.AJ_AJ_AlternativeNumInfo.HasError("Alternate account debit & credit must be opposite to the current setting."));
		}

		public void TestAlternateAccountIncorrectDebitCredit()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			AlternativeLocalAccount.AJ_DebitCredit = "DR";
			descriptor.AJ_AJ_AlternativeNum = AlternativeLocalAccount.PK;

			descriptor.AJ_DebitCredit = "DR";

			Assert(descriptor.AJ_DebitCreditInfo.HasError("Alternate account debit & credit must be opposite to the current setting."));
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.HasError("Alternate account debit & credit must be opposite to the current setting."));
		}

		public void TestAlternateAccountIncorrect()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			descriptor.AJ_DebitCredit = "DR";

			AlternativeLocalAccount.AJ_DebitCredit = "DR";

			descriptor.AJ_AJ_AlternativeNum = AlternativeLocalAccount.PK;

			Assert(descriptor.AJ_DebitCreditInfo.HasError("Alternate account debit & credit must be opposite to the current setting."));
			Assert(descriptor.AJ_AJ_AlternativeNumInfo.HasError("Alternate account debit & credit must be opposite to the current setting."));
		}

		public void TestValidateParentAccountWhenBSH()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			ParentAccount.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ParentGLHeaderPK = ParentAccount.PK;
			AssertNoErrors(descriptor.ParentGLHeaderPKInfo);
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			AssertHasErrors(descriptor.ParentGLHeaderPKInfo);
		}

		public void TestValidateParentAccountWhenPL()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			ParentAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			descriptor.ParentGLHeaderPK = ParentAccount.PK;
			AssertNoErrors(descriptor.ParentGLHeaderPKInfo);
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			Assert(descriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestDoNotValidateParentAccountWhenNTE()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Note;
			ParentAccount.AG_AccountType = AccountTypeComboBoxConstants.Note;
			descriptor.ParentGLHeaderPK = ParentAccount.PK;
			AssertNoErrors(descriptor.ParentGLHeaderPKInfo);
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			Assert(!descriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestDoNotValidateParentAccountWhenHDR()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			Assert(!descriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestDoNotValidateParentAccountWhenTTL()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Total;
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			Assert(!descriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestDoNotValidateParentAccountWhenCLN()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Consolidation;
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			Assert(!descriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestDoNotValidateParentAccountWhenALT()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Alternate;
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			Assert(!descriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestDoNotValidateParentAccountWhenCFW()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.CarriedForwardAccount;
			descriptor.ParentGLHeaderPKInfo.ClearValue();
			Assert(!descriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestTTLAccountTypeValidation()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_AJ_HeaderDependsOnTotal = ConsolidatedLocalAccount.PK;
			IEnumerable<INotification> error = descriptor.AJ_AJ_HeaderDependsOnTotalInfo.GetErrors();
			Assert(error.Count() == 1);
			AssertEquals("Total Reference Account must be type of Total Reference.", error.GetFirstMessage());
		}

		public void TestTTLAccountTypeValidationForCorrectType()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_AJ_HeaderDependsOnTotal = GetAccountDesriptorByType(AccountTypeComboBoxConstants.Total).PK;
			IEnumerable<INotification> error = descriptor.AJ_AJ_HeaderDependsOnTotalInfo.GetErrors();
			Assert(error.Count() == 0);
		}

		public void TestTTLAccountNumberGreaterThanAccountNum()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			AccGLAccountDescriptor totalReference = GetAccountDesriptorByType(AccountTypeComboBoxConstants.Total);
			totalReference.AJ_LocalAccountNumber = "1233.000";
			descriptor.AJ_AJ_HeaderDependsOnTotal = totalReference.PK;

			descriptor.AJ_LocalAccountNumber = "1234.000";

			IEnumerable<INotification> error = descriptor.AJ_LocalAccountNumberInfo.GetErrors();
			AssertEquals(1, error.Count());
			AssertEquals("Account number must be less than Total Reference number", error.GetFirstMessage());
		}

		public void TestCFWAccountTypeValidation()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_AJ_CarriedForwardAccount = ConsolidatedLocalAccount.PK;
			IEnumerable<INotification> error = descriptor.AJ_AJ_CarriedForwardAccountInfo.GetErrors();
			Assert(error.Count() == 1);
			AssertEquals("Carried Forward Account must be type of Carried Forward.", error.GetFirstMessage());
		}

		public void TestOnlyOneCFWAccount()
		{
			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.CarriedForwardAccount;

			Assert(!descriptor1.AJ_ReportCategoryInfo.HasError("There can only be only one Carried Forward Reference per each language Code."));

			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor2.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.CarriedForwardAccount;

			Assert(descriptor2.AJ_ReportCategoryInfo.HasError("There can only be only one Carried Forward Reference per each language Code."));
		}

		public void TestOnlyOneCFWAccountForDIfferentLanguage()
		{
			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.CarriedForwardAccount;

			Assert(!descriptor1.AJ_ReportCategoryInfo.HasError("There can only be only one Carried Forward Reference per each language Code."));

			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor2.AJ_Language = Constants.Languages.Bulgarian;
			descriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.CarriedForwardAccount;

			Assert(!descriptor2.AJ_ReportCategoryInfo.HasError("There can only be only one Carried Forward Reference per each language Code."));
		}

		public void TestOnlyOneCFWAccountForNonCFWAccount()
		{
			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.Alternate;

			Assert(!descriptor1.AJ_ReportCategoryInfo.HasError("There can only be only one Carried Forward Reference per each language Code."));

			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor2.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.Alternate;

			Assert(!descriptor2.AJ_ReportCategoryInfo.HasError("There can only be only one Carried Forward Reference per each language Code."));
		}

		public void TestOnlyOneTotalReferenceIsReferdByDifferentAccount()
		{
			AccGLAccountDescriptor totalReference = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			totalReference.AJ_Language = Constants.Languages.ChineseSimplified;
			totalReference.AJ_ReportCategory = AccountTypeComboBoxConstants.Total;

			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			descriptor.AJ_AJ_HeaderDependsOnTotal = totalReference.PK;

			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor2.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			descriptor2.AJ_AJ_HeaderDependsOnTotal = totalReference.PK;
			descriptor2.Validation.ValidateAJ_AJ_HeaderDependsOnTotal();

			Assert(descriptor2.AJ_AJ_HeaderDependsOnTotalInfo.HasError("This Total Reference Account is already in use. Please select different Account."));
		}

		public void TestChangingLanguageClearsLinkingAccount()
		{
			AccGLAccountDescriptor testDescriptor = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			testDescriptor.AJ_Language = Constants.Languages.Arabic;

			testDescriptor.AJ_AJ_AlternativeNum = Guid.NewGuid();
			testDescriptor.AJ_AJ_ConsolidationNum = Guid.NewGuid();
			testDescriptor.AJ_AJ_HeaderDependsOnTotal = Guid.NewGuid();
			testDescriptor.AJ_AJ_CarriedForwardAccount = Guid.NewGuid();
			testDescriptor.AJ_AJ_PercentNum = Guid.NewGuid();

			testDescriptor.AJ_Language = Constants.Languages.Bulgarian;

			Assert(testDescriptor.AJ_AJ_AlternativeNum.IsEmpty);
			Assert(testDescriptor.AJ_AJ_ConsolidationNum.IsEmpty);
			Assert(testDescriptor.AJ_AJ_HeaderDependsOnTotal.IsEmpty);
			Assert(testDescriptor.AJ_AJ_CarriedForwardAccount.IsEmpty);
			Assert(testDescriptor.AJ_AJ_PercentNum.IsEmpty);

			testDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			testDescriptor.AJ_AJ_AlternativeNum = Guid.NewGuid();
			testDescriptor.AJ_AJ_ConsolidationNum = Guid.NewGuid();
			testDescriptor.AJ_AJ_HeaderDependsOnTotal = Guid.NewGuid();
			testDescriptor.AJ_AJ_CarriedForwardAccount = Guid.NewGuid();
			testDescriptor.AJ_AJ_PercentNum = Guid.NewGuid();

			testDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.HongKong;

			Assert(testDescriptor.AJ_AJ_AlternativeNum.IsEmpty);
			Assert(testDescriptor.AJ_AJ_ConsolidationNum.IsEmpty);
			Assert(testDescriptor.AJ_AJ_HeaderDependsOnTotal.IsEmpty);
			Assert(testDescriptor.AJ_AJ_CarriedForwardAccount.IsEmpty);
			Assert(testDescriptor.AJ_AJ_PercentNum.IsEmpty);
		}

		public void TestIsValidCharForChineseGLAccount()
		{
			AccGLAccountDescriptor descriptor = Factory.New<AccGLAccountDescriptor>();
			descriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			Assert(!descriptor.IsValidCharForChineseGLAccount('a'));
			Assert(descriptor.IsValidCharForChineseGLAccount('1'));
			Assert(descriptor.IsValidCharForChineseGLAccount('.'));
			Assert(!descriptor.IsValidCharForChineseGLAccount('='));
			Assert(descriptor.IsValidCharForChineseGLAccount('\t'));
		}

		public void TestClearLocalAccountNumberIfLanguageIsSetToChineseAndThereAreInvalidCharacters()
		{
			AccGLAccountDescriptor descriptor = Factory.New<AccGLAccountDescriptor>();
			descriptor.AJ_Language = Constants.Languages.English;
			descriptor.AJ_LocalAccountNumber = "123";
			descriptor.AJ_Language = Constants.Languages.ChineseTraditional;
			AssertEquals("123", descriptor.AJ_LocalAccountNumber);

			descriptor.AJ_Language = Constants.Languages.English;
			descriptor.AJ_LocalAccountNumber = "MEH";
			descriptor.AJ_Language = Constants.Languages.ChineseTraditional;
			AssertEquals("", descriptor.AJ_LocalAccountNumber);

			descriptor.AJ_Language = Constants.Languages.Danish;
			descriptor.AJ_LocalAccountNumber = "1000.00.00";
			descriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			AssertEquals("1000.00.00", descriptor.AJ_LocalAccountNumber);

			descriptor.AJ_Language = Constants.Languages.Danish;
			descriptor.AJ_LocalAccountNumber = "XXX";
			descriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			AssertEquals("", descriptor.AJ_LocalAccountNumber);
		}

		#region Clone

		public void TestClone()
		{
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			TestAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;

			AccGLAccountDescriptor clonedDescriptor = (AccGLAccountDescriptor)TestAccGLAccountDescriptor.Clone();
			foreach (ZPropertyInfo property in clonedDescriptor.ZPropertyInfoHash)
			{
				if (property.Name != "ParentGLHeaderPK")
				{
					AssertEquals("Property " + property.Name, TestAccGLAccountDescriptor[property.Name], clonedDescriptor[property.Name]);
				}
			}
		}

		#endregion

		#region Report Setup

		public void TestReportConfigurationPivotCollection()
		{
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Factory.Save();

			AssertEquals(1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);
			AssertEquals(0, TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.Count);

			GLDescriptorPivot testGLDescriptorPivot = TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.AddNew();
			AssertEquals(1, TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.Count);
			AssertEquals("PivotCollection still shoud be 1.", 1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);

			testGLDescriptorPivot.YJ_AG = testGLAccount.PK;
			testGLDescriptorPivot.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			testGLDescriptorPivot.ReportCategory = BalanceSheet_China.Codes.D01;
			Factory.Save();

			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, TestAccGLAccountDescriptor.ReportConfigurationPivotCollection[0].GLAccountDescriptor.AJ_ReportType);
			AssertEquals(BalanceSheet_China.Codes.D01, TestAccGLAccountDescriptor.ReportConfigurationPivotCollection[0].GLAccountDescriptor.AJ_ReportCategory);
		}

		public void TestReportSetupVisible()
		{
			var testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			var testGLAccountForNTE = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccountForNTE.AG_AccountNum = "1111.22.44";
			testGLAccountForNTE.AG_AccountType = AccountTypeComboBoxConstants.Note;

			TestAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;

			Assert("Not in the DB, ReportSetupVisible Should be is false", !TestAccGLAccountDescriptor.ReportSetupVisible);
			Factory.Save();
			Assert("ReportSetupVisible Should be is true", TestAccGLAccountDescriptor.ReportSetupVisible);
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;
			Assert("YJ_AG is Empty, the ReportSetupVisible Should be is false", !TestAccGLAccountDescriptor.ReportSetupVisible);

			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.English;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Assert("Language is English, ReportSetupVisible Should be is false", !TestAccGLAccountDescriptor.ReportSetupVisible);
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Assert("ReportSetupVisible Should be is true", TestAccGLAccountDescriptor.ReportSetupVisible);

			TestAccGLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.Australia;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Assert("Country isn't China, ReportSetupVisible Should be is false", !TestAccGLAccountDescriptor.ReportSetupVisible);

			AccountingMasterFilesRegistry.Instance.EnableReportSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert("Country isn't China, but EnableReportSetup set to true , ReportSetupVisible Should be is true", TestAccGLAccountDescriptor.ReportSetupVisible);

			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Note;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccountForNTE.PK;
			Assert("Account Type is Note, ReportSetupVisible Should be is false", !TestAccGLAccountDescriptor.ReportSetupVisible);
		}

		public void TestUpdateCommonPropertiesOnRelatedDescriptors()
		{
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Factory.Save();

			GLDescriptorPivot testGLDescPivot = TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.AddNew();

			testGLDescPivot.ReportType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLDescPivot.ReportCategory = ProfitAndLoss_China.Codes.D02;
			Factory.Save();

			AccGLAccountDescriptor tesAccDesc = Factory.Load<AccGLAccountDescriptor>(TestAccGLAccountDescriptor.PK);
			AccGLAccountDescriptor pAccGLAccountDescriptor = testGLDescPivot.GLAccountDescriptor;

			tesAccDesc.AJ_LocalAccountNumber = "8888.999";
			AssertEquals(1, tesAccDesc.ReportConfigurationPivotCollection.Count);
			AssertEquals(1, tesAccDesc.AccGLDescriptorPivotCOACollection.Count);
			AssertEquals("8888.999", pAccGLAccountDescriptor.AJ_LocalAccountNumber);

			TestAccGLAccountDescriptor.AJ_AccountDescription = "Test Synchronization";
			AssertEquals("Test Synchronization", pAccGLAccountDescriptor.AJ_AccountDescription);

			tesAccDesc.AJ_DebitCredit = "CR";
			AssertEquals(pAccGLAccountDescriptor.AJ_DebitCredit, "CR");

			tesAccDesc.AJ_PrintSequence = 11;
			AssertEquals((ZShort)11, pAccGLAccountDescriptor.AJ_PrintSequence);

			tesAccDesc.AJ_TotalLevel = 12;
			AssertEquals(pAccGLAccountDescriptor.AJ_TotalLevel, (ZShort)12);

			tesAccDesc.AJ_AJ_AlternativeNum = AlternativeLocalAccount.PK;
			AssertEquals(AlternativeLocalAccount.PK, pAccGLAccountDescriptor.AJ_AJ_AlternativeNum);

			AccGLAccountDescriptor carriedForwardAccount = GetAccountDesriptorByType(AccountTypeComboBoxConstants.CarriedForwardAccount);
			tesAccDesc.AJ_AJ_CarriedForwardAccount = carriedForwardAccount.PK;
			AssertEquals(carriedForwardAccount.PK, pAccGLAccountDescriptor.AJ_AJ_CarriedForwardAccount);

			tesAccDesc.AJ_AJ_ConsolidationNum = ConsolidatedLocalAccount.PK;
			AssertEquals(ConsolidatedLocalAccount.PK, pAccGLAccountDescriptor.AJ_AJ_ConsolidationNum);

			AccGLAccountDescriptor headerDependsOnTotal = GetAccountDesriptorByType(AccountTypeComboBoxConstants.Header);
			tesAccDesc.AJ_AJ_HeaderDependsOnTotal = headerDependsOnTotal.PK;
			AssertEquals(headerDependsOnTotal.PK, pAccGLAccountDescriptor.AJ_AJ_HeaderDependsOnTotal);
		}

		#endregion

		#region AccGLDescriptorPivotYJ_AG

		public void TestPivotCollectionAndAccGLDescriptorPivotCOA()
		{
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			TestAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;

			AssertNotNull(TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA);
			AssertEquals(1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);
			TestAccGLAccountDescriptor.AJ_ReportType = "XXA";
			AssertNull(TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA);
		}

		public void TestAccGLDescriptorPivotYJ_AG()
		{
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";

			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;

			Assert("P&L GL header must have a global GL account", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());

			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;

			Factory.Save();
			Assert("Should not have errors since global GL is not empty", !TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());
			AssertNotNull(TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA);

			GLDescriptorPivot testGLDescriptorPivot = TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.AddNew();
			AssertEquals(1, TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.Count);
			AssertEquals(1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);

			testGLDescriptorPivot.YJ_AG = testGLAccount.PK;
			testGLDescriptorPivot.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			testGLDescriptorPivot.ReportCategory = BalanceSheet_China.Codes.D01;
			Factory.Save();

			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;
			AssertNull("YJ_AG is Empty, the AccGLDescriptorPivotCOA has been deleted", TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA);
			AssertEquals("YJ_AG is Empty, the item of Report Configuration has been removed", 0, TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.Count);

			Assert("BSH header must have a global GL account", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());

			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Assert("Should not have errors since global GL is not empty", !TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());

			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;

			Assert("HDR GL header can have NULL for YJ_AG", !TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());
			Assert("HDR GL Header should have YJ_AG disabled", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.ReadOnly);
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLAccount.AG_DebitCredit = "DR";
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			AssertEquals("DR", TestAccGLAccountDescriptor.AJ_DebitCredit);
			TestAccGLAccountDescriptor.AJ_DebitCredit = "";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;
			AssertEquals("", TestAccGLAccountDescriptor.AJ_DebitCredit);

			testGLAccount.AG_DebitCredit = "CR";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			AssertEquals("CR", TestAccGLAccountDescriptor.AJ_DebitCredit);

			TestAccGLAccountDescriptor.AJ_ReportType = "XXA";
			AssertNull(TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA);
			Assert("YJ_AG Should be is empty", TestAccGLAccountDescriptor.ParentGLHeaderPK.IsEmpty);
		}

		#endregion

		public void TestDelete()
		{
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";

			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;

			Factory.Save();

			GLDescriptorPivot testReportConfigurationPivot = TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.AddNew();
			AssertEquals(1, TestAccGLAccountDescriptor.ReportConfigurationPivotCollection.Count);
			AssertEquals(1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);

			testReportConfigurationPivot.YJ_AG = testGLAccount.PK;
			testReportConfigurationPivot.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			testReportConfigurationPivot.ReportCategory = BalanceSheet_China.Codes.D01;
			Factory.Save();

			AccGLDescriptorPivot[] testAccGLDescriptorPivots = Factory.Load<AccGLDescriptorPivot>(new ZQuery());

			AssertEquals(2, testAccGLDescriptorPivots.Length);
			TestAccGLAccountDescriptor.Delete();
			Factory.Save();
			testAccGLDescriptorPivots = Factory.Load<AccGLDescriptorPivot>(new ZQuery());
			AssertEquals(0, testAccGLDescriptorPivots.Length);
		}

		public void TestGetLocalAccountDescriptor()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = Constants.Languages.ChineseSimplified;

			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountNum = "1111.22.33";
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";
			TestAccGLAccountDescriptor.AJ_AccountDescription = "Local Account Description";

			Factory.Save();

			var descriptor = AccGLAccountDescriptor.GetLocalAccountDescriptor(Factory, testGLAccount.PK);

			AssertEquals("Should get the right Local Account Descriptor", TestAccGLAccountDescriptor.PK, descriptor.PK);
			AssertEquals("Local Account Code", TestAccGLAccountDescriptor.AJ_LocalAccountNumber, descriptor.AJ_LocalAccountNumber);
			AssertEquals("Local Account Description", TestAccGLAccountDescriptor.AJ_AccountDescription, descriptor.AJ_AccountDescription);
		}

		public void TestHumanReadableNameCore()
		{
			var descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_LocalAccountNumber = "123456";
			descriptor.AJ_AccountDescription = "Testing";
			AssertEquals("General Ledger Multi-Language Mapping - 123456 - Testing", descriptor.HumanReadableName);
		}

		public void TestStatisticalUnitsInfo()
		{
			AssertNull(TestAccGLAccountDescriptor.StatisticalUnitsInfo);
		}

		public void TestNoStmALogs()
		{
			var accountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, accountDescriptor.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				accountDescriptor.AJ_AccountDescription = "UU Test";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				accountDescriptor.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Implementation

		AccGLAccountDescriptor TestAccGLAccountDescriptor;

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			TestAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
		}

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		AccGLAccountDescriptor fConsolidatedLocalAccount;
		AccGLAccountDescriptor ConsolidatedLocalAccount
		{
			get
			{
				return fConsolidatedLocalAccount ??
					 (fConsolidatedLocalAccount = GetAccountDesriptorByType(AccountTypeComboBoxConstants.Consolidation));
			}
		}

		AccGLAccountDescriptor fAlternativeLocalAccount;
		AccGLAccountDescriptor AlternativeLocalAccount
		{
			get
			{
				return fAlternativeLocalAccount ??
					 (fAlternativeLocalAccount = GetAccountDesriptorByType(AccountTypeComboBoxConstants.Alternate));
			}
		}

		AccGLHeader fParentAccount;
		AccGLHeader ParentAccount
		{
			get { return fParentAccount ?? (fParentAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader); }
		}

		AccGLAccountDescriptor GetAccountDesriptorByType(string type)
		{
			AccGLAccountDescriptor accGLAccountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			accGLAccountDescriptor.AJ_ReportCategory = type;
			return accGLAccountDescriptor;
		}

		protected ZGuid GetAccountDesriptorPKByType(string type)
		{
			AccGLAccountDescriptor accGLAccountDescriptor = GetAccountDesriptorByType(type);
			return accGLAccountDescriptor != null ? accGLAccountDescriptor.PK : ZGuid.Empty;
		}

		protected void ClearPropertyValue(AccGLAccountDescriptor descriptor)
		{
			descriptor.AJ_AJ_AlternativeNumInfo.ClearValue();
			descriptor.AJ_AJ_PercentNumInfo.ClearValue();
			descriptor.AJ_AJ_ConsolidationNumInfo.ClearValue();
			descriptor.AJ_AJ_CarriedForwardAccountInfo.ClearValue();
			descriptor.AJ_AJ_HeaderDependsOnTotalInfo.ClearValue();

			descriptor.AJ_TotalLevelInfo.ClearValue();

			descriptor.ParentGLHeaderPKInfo.ClearValue();
		}

		#endregion
	}
}
