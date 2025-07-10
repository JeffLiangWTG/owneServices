using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGuaranteeHeader))]
	public class BaseCusGuaranteeHeaderTest : SharedCusPermitHeaderTest<BaseCusGuaranteeHeader>
	{
		public void TestMainAccessCode_CheckMaximumLength()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<MaxLengthExceededException>("MaxLengthExceededException", () =>
				{
					guaranteeHeader.MainAccessCode = new ZString('A', guaranteeHeader.MainAccessCodeInfo.MaxLength + 1);
				});
				AssertContains("LastMessageReported", "The maximum length of 'MainAccessCode' has been exceeded", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		public void TestMainAccessPersonName_CheckMaximumLength()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<MaxLengthExceededException>("MaxLengthExceededException", () =>
				{
					guaranteeHeader.MainAccessPersonName = new ZString('A', guaranteeHeader.MainAccessPersonNameInfo.MaxLength + 1);
				});
				AssertContains("LastMessageReported", "The maximum length of 'MainAccessPersonName' has been exceeded", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		public void TestAddTransaction_PreventGuaranteeBusting()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

			CombineAssertions(() =>
			{
				var transactionLine = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				transactionLine.CPL_TranValue = 100;
				var transactionLine2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				transactionLine2.CPL_TranValue = -20;

				var message = string.Empty;
				var availableAmount = ZDecimal.Zero;
				AssertNull("Busting", guaranteeHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -101m, 0m, PermitTransactionStatusList.Codes.Pending, 1, notifier: (m, a) =>
				{
					message = m;
					availableAmount = a;
				}));
				AssertEquals("Busting Message", "The guarantee 12345 available amount will be exceeded by 21. The total is 100 and the available is 80.", message);
				AssertEquals("The available guarantee amount", 80m, availableAmount);
			});
		}

		public void TestAddTransaction_PreventGuaranteeBusting_OpeningBalance()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

			CombineAssertions(() =>
			{
				var transactionLine = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				transactionLine.CPL_TranValue = 100;
				var transactionLine2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				transactionLine2.CPL_TranValue = -20;
				AssertEquals("Default CPH_Calc_OpeningBalance", new ZDecimal(100), guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertEquals("Default CPH_Calc_TotalBalanceIncludingPendingDecimal", new ZDecimal(80), guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);
				AssertEquals("Transaction Added", 2, guaranteeHeader.GetTransactions().Count());

				var message = string.Empty;
				var availableAmount = ZDecimal.Zero;
				AssertNull("Busting", guaranteeHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", 30m, 0m, PermitTransactionStatusList.Codes.Pending, 1, notifier: (m, a) =>
				{
					message = m;
					availableAmount = a;
				}));
				AssertEquals("Busting Message", "The guarantee 12345 total amount will be exceeded by 10. The total is 100 and the available is 80.", message);
				AssertEquals("The available guarantee amount", 80m, availableAmount);
				AssertEquals("Still 2 transactions", 2, guaranteeHeader.GetTransactions().Count());
			});
		}

		public void TestAddTransaction_IfGuaranteeIsAlreadyBusted()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "Entry Number";
			transaction.CPL_TranValue = -100m;
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction.CPL_AppId = "Entry Reference";
			transaction.CPL_Comment = "Instruction Desc.";
			transaction.CPL_Procedure = "AAA";

			CombineAssertions(() =>
			{
				AssertEquals("single transaction with CPL_TranValue", -100m, guaranteeHeader.GetTransactions().Single().CPL_TranValue);

				guaranteeHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", 1m, 0m, PermitTransactionStatusList.Codes.Pending, 1);
				var transactions = guaranteeHeader.GetTransactions().ToArray();
				AssertEquals("We can reduce busted value - transactions", 2, transactions.Length);
				AssertEquals("We can reduce busted value - 1st CPL_TranValue", -100m, transactions[0].CPL_TranValue);
				AssertEquals("We can reduce busted value - 2nd CPL_TranValue", 1m, transactions[1].CPL_TranValue);

				guaranteeHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -1m, 0m, PermitTransactionStatusList.Codes.Pending, 1);
				transactions = guaranteeHeader.GetTransactions().ToArray();
				AssertEquals("We can't increase busted value - transactions", 2, transactions.Length);
			});
		}

		public void TestAddTransaction_OpeningBalanceAdjustment()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			CombineAssertions(() =>
			{
				var transactionLine = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				transactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				transactionLine.CPL_TranValue = 100;

				var message = string.Empty;
				var transaction = guaranteeHeader.AddTransaction("REF", "COMMENT", "APPID", "PROCEDURE", 200m, 0m, transactionType: GuaranteeTransactionTypeList.Codes.OBA, notifier: (m, a) =>
				{
					message = m;
				});
				AssertNotNull("No busting", transaction);
				AssertEquals("No busting message", string.Empty, message);
				AssertEquals("The available guarantee amount", 300m, guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertEquals("Number of transactions", 2, guaranteeHeader.GetTransactions().Count());

				message = string.Empty;
				transaction = guaranteeHeader.AddTransaction("REF", "COMMENT", "APPID", "PROCEDURE", -399m, 0m, transactionType: GuaranteeTransactionTypeList.Codes.OBA, notifier: (m, a) =>
				{
					message = m;
				});
				AssertNull("Busting", transaction);
				AssertEquals("Busting message", "The guarantee 12345 available amount will be exceeded by 99. The total is 300 and the available is 300.", message);
				AssertEquals("The available guarantee amount", 300m, guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertEquals("Number of transactions", 2, guaranteeHeader.GetTransactions().Count());

				message = string.Empty;
				transaction = guaranteeHeader.AddTransaction("REF", "COMMENT", "APPID", "PROCEDURE", -299m, 0m, transactionType: GuaranteeTransactionTypeList.Codes.OBA, notifier: (m, a) =>
				{
					message = m;
				});
				AssertNotNull("No busting", transaction);
				AssertEquals("No busting message", string.Empty, message);
				AssertEquals("The available guarantee amount", 1m, guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertEquals("Number of transactions", 3, guaranteeHeader.GetTransactions().Count());
			});
		}

		public void TestCPH_Type()
		{
			var header = Factory.New<BaseCusGuaranteeHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("default CPH_Type", ZString.Empty, header.CPH_Type);
				AssertEquals("default CPH_UnitOfMeasure", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, header.CPH_UnitOfMeasure);
				header.CPH_Type = GuaranteeTransactionTypeList.Codes.OBA;
				AssertEquals("CPH_Type changed, new CPH_Type value", GuaranteeTransactionTypeList.Codes.OBA, header.CPH_Type);
				AssertEquals("CPH_Type changed, new CPH_UnitOfMeasure value", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, header.CPH_UnitOfMeasure);
			});
		}

		public void TestCPH_Type_Caption()
		{
			AssertEquals("Guarantee Type", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_TypeInfo).Caption);
		}

		public void TestCPH_SubType_Caption()
		{
			AssertEquals("Sub Type", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_SubTypeInfo).Caption);
		}

		public void TestCPH_OH_PermitHolder_Caption()
		{
			AssertEquals("Guarantee Holder", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_OH_PermitHolderInfo).Caption);
		}

		public void TestCPH_Number_Caption()
		{
			AssertEquals("Guarantee Number", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_NumberInfo).Caption);
		}

		public void TestCPH_UnitOfMeasure_Caption()
		{
			AssertEquals("Currency", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_UnitOfMeasureInfo).Caption);
		}

		public void TestCPH_QtyValIndicator_Caption()
		{
			AssertEquals("Quantity", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_QtyValIndicatorInfo).Caption);
		}

		public void TestCPH_StartDate_Caption()
		{
			AssertEquals("Start Date", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_StartDateInfo).Caption);
		}

		public void TestCPH_EndDate_Caption()
		{
			AssertEquals("End Date", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_EndDateInfo).Caption);
		}

		public void TestCPH_IsSingleUse_Caption()
		{
			AssertEquals("Is Single Transaction", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_IsSingleUseInfo).Caption);
		}

		public void TestCPH_RN_NKCountryCode_Caption()
		{
			AssertEquals("Creation Country", DataBoundResourceStrings.GetDataForProperty(GuaranteeHeader.CPH_RN_NKCountryCodeInfo).Caption);
		}

		public void TestMessages()
		{
			AssertType<EDIMessageCollection>(GuaranteeHeader.Messages);
		}

		public void TestValidationType()
		{
			AssertType<CusGuaranteeHeaderValidation>(GuaranteeHeader.Validation);
		}

		public void TestLookupsType()
		{
			AssertType<CusGuaranteeHeaderLookups>(GuaranteeHeader.Lookups);
		}

		public void TestHumanReadableName()
		{
			var header = GuaranteeHeader;
			header.CPH_Number = "12345678";
			AssertEquals($"Guarantee {header.PermitHolder.OH_Code} - 12345678", header.HumanReadableName);
		}

		public void TestDelete()
		{
			var header = GuaranteeHeader;
			var rule = header.CusGuaranteeRules.AddNew();
			var accessCode = header.AdditionalAccessCodes.AddNew();
			header.MainAccessCode = "1234";
			var mainAccessCodeRule = header.MainAccessCodeRule;
			var transaction = header.CusGuaranteeLineTransactions.AddNew();
			AssertEquals(false, rule.IsDeleted);
			AssertEquals(false, accessCode.IsDeleted);
			AssertEquals(false, mainAccessCodeRule.IsDeleted);
			AssertEquals(false, transaction.IsDeleted);
			header.Delete();
			AssertEquals(true, rule.IsDeleted);
			AssertEquals(true, accessCode.IsDeleted);
			AssertEquals(true, mainAccessCodeRule.IsDeleted);
			AssertEquals(true, transaction.IsDeleted);
		}

		public void TestDefaultValues()
		{
			AssertEquals("Default CPH_QtyValIndicator", PermitQtyValIndicatorList.Codes.VAL, GuaranteeHeader.CPH_QtyValIndicator);
			AssertEquals("Default CPH_ApplicationCode", CusPermitHeaderApplicationCodeList.Codes.Guarantee, GuaranteeHeader.CPH_ApplicationCode);
			AssertEquals("Default CPH_UnitOfMeasure", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GuaranteeHeader.CPH_UnitOfMeasure);
		}

		public void TestMainAccessCode()
		{
			GuaranteeHeader.MainAccessCode = "10";
			AssertEquals(PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin, GuaranteeHeader.MainAccessCodeRule.CPR_RuleCode);
			AssertEquals("10", GuaranteeHeader.MainAccessCode);
			AssertEquals("10", GuaranteeHeader.MainAccessCodeRule.CPR_ValueFrom);
			AssertEquals("", GuaranteeHeader.MainAccessCodeRule.CPR_Description);
		}

		public void TestMainAccessPersonName()
		{
			GuaranteeHeader.MainAccessPersonName = "JAMES";
			AssertEquals(PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin, GuaranteeHeader.MainAccessCodeRule.CPR_RuleCode);
			AssertEquals("JAMES", GuaranteeHeader.MainAccessPersonName);
			AssertEquals("JAMES", GuaranteeHeader.MainAccessCodeRule.CPR_Description);
		}

		public void TestAdditionalAccessCodes()
		{
			Assert(!GuaranteeHeader.AdditionalAccessCodes.Any());
			GuaranteeHeader.AdditionalAccessCodes.AddNew();
			GuaranteeHeader.AdditionalAccessCodes.AddNew();
			GuaranteeHeader.CusGuaranteeRules.AddNew();
			AssertEquals(2, GuaranteeHeader.AdditionalAccessCodes.Count);
			AssertEquals(1, GuaranteeHeader.CusGuaranteeRules.Count);
		}

		public void TestAdditionalAccessCodes_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalAccessCodes is ReadOnly default", ZBool.True, GuaranteeHeader.AdditionalAccessCodes.ReadOnly);

				GuaranteeHeader.MainAccessCode = "10";
				GuaranteeHeader.AdditionalAccessCodes.AddNew();
				GuaranteeHeader.AdditionalAccessCodes.AddNew();
				AssertEquals("AdditionalAccessCodes is not ReadOnly when MainAccessCode isn't empty", ZBool.False, GuaranteeHeader.AdditionalAccessCodes.ReadOnly);

				GuaranteeHeader.MainAccessCode = ZString.Empty;
				AssertEquals("AdditionalAccessCodes count", 0, GuaranteeHeader.AdditionalAccessCodes.Count);
				AssertEquals("AdditionalAccessCodes is ReadOnly when MainAccessCode is set to empty", ZBool.True, GuaranteeHeader.AdditionalAccessCodes.ReadOnly);
			});
		}

		public void TestCPH_RN_NKCountryCode_ReadOnly()
		{
			AssertEquals(true, GuaranteeHeader.CPH_RN_NKCountryCodeInfo.ReadOnly);
		}

		public void TestReadOnlyCurrencyAfterOpeningBalanceTransaction()
		{
			AssertEquals(false, GuaranteeHeader.CPH_UnitOfMeasureInfo.ReadOnly);
			GuaranteeHeader.CPH_UnitOfMeasure = "EUR";
			AssertEquals(false, GuaranteeHeader.CPH_UnitOfMeasureInfo.ReadOnly);
			Factory.Save();
			GuaranteeHeader.CPH_UnitOfMeasure = "AUD";
			AssertEquals(false, GuaranteeHeader.CPH_UnitOfMeasureInfo.ReadOnly);
			var openingBalanceTransaction = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			openingBalanceTransaction.CPL_Comment = "COMMENT";
			openingBalanceTransaction.CPL_Reference = "REFERENCE";
			Factory.Save();
			AssertEquals(true, GuaranteeHeader.CPH_UnitOfMeasureInfo.ReadOnly);
		}

		public void TestCPH_UnitOfMeasure_ReadOnly()
		{
			var guarantee1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guarantee1.CPH_Number = "1133890";
			CombineAssertions(() =>
			{
				AssertEquals("No transactions", false, guarantee1.CPH_UnitOfMeasure_ReadOnly);
				guarantee1.CusGuaranteeLineTransactions.AddNew().CPL_Reference = "3345112";
				Factory.Save();
				AssertEquals("Currency is readonly if there is any transaction.", true, guarantee1.CPH_UnitOfMeasure_ReadOnly);

				var guarantee2 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
				guarantee2.CPH_Number = "1133891";
				guarantee2.CPH_UnitOfMeasure = Core.Constants.CurrencyCodes.EuropeanUnion;
				Factory.Save();
				AssertEquals("Currency is editable even if it has been entered.", false, guarantee2.CPH_UnitOfMeasure_ReadOnly);

				var guarantee3 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
				guarantee3.CPH_Type = "GEN";
				guarantee1.CPH_Number = "1133892";
				AssertEquals("CPH_Type is GEN", true, guarantee1.CPH_UnitOfMeasure_ReadOnly);
			});
		}

		public void TestCPH_Calc_RemainingBalance()
		{
			var openingBalanceTransaction = PermitHeader.CusGuaranteeLineTransactions.AddNew();
			openingBalanceTransaction.CPL_Reference = "Ref";
			openingBalanceTransaction.CPL_TransactionStatus = "CON";
			openingBalanceTransaction.CPL_IsAggregated = true;
			openingBalanceTransaction.CPL_TranValue = 1234.56;
			var pendingBalanceTransaction = PermitHeader.CusGuaranteeLineTransactions.AddNew();
			pendingBalanceTransaction.CPL_Reference = "Ref";
			pendingBalanceTransaction.CPL_TransactionStatus = "PND";
			pendingBalanceTransaction.CPL_TranValue = 234.56;
			Factory.Save();
			AssertEquals(new ZDecimal(1234.56), PermitHeader.CPH_Calc_OpeningBalance);
			AssertEquals(new ZDecimal(234.56), PermitHeader.CPH_Calc_TotalBalanceIncludingPending.Amount.Truncate(2));
			AssertEquals(new ZDecimal(1000), PermitHeader.CPH_Calc_UsedBalance.Truncate(2));
		}

		public void TestGetTotalDataBaseTransactionAmounts()
		{
			GenerateGuaranteeHeader();
			var totals = GuaranteeHeader.GetTotalDataBaseTransactionAmounts();
			AssertEquals((decimal)5045, totals.ConfirmedBalance.Amount);
			AssertEquals((decimal)50, totals.PendingBalance.Amount);
			AssertEquals((decimal)5095, totals.TotalBalance.Amount);
			AssertEquals(true, (ZDateTime.UtcToday - totals.CalculationTimeUtc.Date).Days == 0);
		}

		public void TestLastTransactionCalculation()
		{
			GenerateGuaranteeHeader();
			var result = GuaranteeHeader.LastDataBaseTransactionCalculation;
			AssertEquals((decimal)5045, result.ConfirmedBalance.Amount);
			AssertEquals((decimal)50, result.PendingBalance.Amount);
			AssertEquals((decimal)5095, result.TotalBalance.Amount);
			AssertEquals(true, (ZDateTime.UtcToday - result.CalculationTimeUtc.Date).Days == 0);
		}

		public void TestCPH_Calc_PendingBalance()
		{
			GenerateGuaranteeHeader();
			var result = GuaranteeHeader.LastDataBaseTransactionCalculation;
			AssertEquals(GuaranteeHeader.CPH_Calc_PendingBalance, result.PendingBalance);
		}

		public void TestCPH_Calc_TotalBalanceIncludingPending()
		{
			GenerateGuaranteeHeader();
			var result = GuaranteeHeader.LastDataBaseTransactionCalculation;
			AssertEquals(GuaranteeHeader.CPH_Calc_TotalBalanceIncludingPending, result.TotalBalance);
		}

		public void TestCPH_Calc_UsedBalance()
		{
			GenerateGuaranteeHeader();
			var result = GuaranteeHeader.LastDataBaseTransactionCalculation;
			AssertEquals(GuaranteeHeader.CPH_Calc_UsedBalance, GuaranteeHeader.CPH_Calc_OpeningBalance - result.TotalBalance.Amount);
		}

		public new void TestCPH_Calc_OpeningBalance()
		{
			GenerateGuaranteeHeader();
			CombineAssertions(() =>
			{
				GuaranteeHeader.CusGuaranteeLineTransactions.AdditionalFilter = new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionDate, SQLComparisonOperator.GreaterThan, ZDate.Today.AddMonths(-1));

				AssertEquals("When OBA transactions are not entered, CPH_Calc_OpeningBalance", new ZDecimal(498), GuaranteeHeader.CPH_Calc_OpeningBalance);

				var adjustmentTransaction1 = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				adjustmentTransaction1.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
				adjustmentTransaction1.CPL_TranValue = -100;
				var adjustmentTransaction2 = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				adjustmentTransaction2.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
				adjustmentTransaction2.CPL_TranValue = 50.56;
				AssertEquals("When OBA transactions are entered (matching filter on TransactionDate), CPH_Calc_OpeningBalance", new ZDecimal(448.56), GuaranteeHeader.CPH_Calc_OpeningBalance);

				adjustmentTransaction2.CPL_TransactionDate = ZDate.Today.AddMonths(-2);
				AssertEquals("When OBA transactions are entered (not matching filter on TransactionDate, proves that filter on CusGuaranteeLineTransactions does not affect this property), CPH_Calc_OpeningBalance", new ZDecimal(448.56), GuaranteeHeader.CPH_Calc_OpeningBalance);
			});
			GuaranteeHeader.UnlockMutex();
		}

		public void TestAdditionalGuaranteeReferences()
		{
			AssertEquals(0, GuaranteeHeader.AdditionalGuaranteeReferences.Count);
			GuaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			GuaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			AssertEquals(2, GuaranteeHeader.AdditionalGuaranteeReferences.Count);
		}

		public void TestSupportsAdditionalCustomsReferences()
		{
			Assert(!GuaranteeHeader.SupportsAdditionalCustomsReferences);
		}

		public void TestSupportsMessages()
		{
			Assert(!GuaranteeHeader.SupportsMessages);
		}

		public void TestGetApplicationSpecificReferenceWhenSupportsAdditionalCustomsReferencesIsTrue()
		{
			var guaranteeWithAdditionalReferences = Factory.New<GuaranteeWithAdditionalReferencesForTest>();
			Assert(guaranteeWithAdditionalReferences.SupportsAdditionalCustomsReferences);
			guaranteeWithAdditionalReferences.AdditionalGuaranteeReferences.AddNew("ABC", "Value for ABC");
			guaranteeWithAdditionalReferences.CPH_Number = "12345";
			AssertEquals("12345", guaranteeWithAdditionalReferences.GetApplicationSpecificReference(""));
			AssertEquals("12345", guaranteeWithAdditionalReferences.GetApplicationSpecificReference("Blah"));
			AssertEquals("Value for ABC", guaranteeWithAdditionalReferences.GetApplicationSpecificReference("ABC"));
		}

		public void TestGetApplicationSpecificReferenceWhenSupportsAdditionalCustomsReferencesIsFalse()
		{
			GuaranteeHeader.CPH_Number = "12345";
			AssertEquals("12345", GuaranteeHeader.GetApplicationSpecificReference(""));
		}

		public void TestGetApplicationSpecificReferenceWithoutFallbackToPermitNumber()
		{
			var guaranteeWithAdditionalReferences = Factory.New<GuaranteeWithAdditionalReferencesForTest>();
			guaranteeWithAdditionalReferences.CPH_Number = "12345";
			AssertEquals("12345", guaranteeWithAdditionalReferences.GetApplicationSpecificReference("ABC"));
			AssertEquals("", guaranteeWithAdditionalReferences.GetApplicationSpecificReferenceWithoutFallbackToPermitNumber("ABC"));

			guaranteeWithAdditionalReferences.AdditionalGuaranteeReferences.AddNew("ABC", "Value for ABC");
			AssertEquals("Value for ABC", guaranteeWithAdditionalReferences.GetApplicationSpecificReference("ABC"));
			AssertEquals("Value for ABC", guaranteeWithAdditionalReferences.GetApplicationSpecificReferenceWithoutFallbackToPermitNumber("ABC"));
		}

		public void TestAdditionalCustomsReferenceAreDeletedWhenNotSupportedWhenSaving()
		{
			AssertEquals(0, GuaranteeHeader.AdditionalGuaranteeReferences.Count);
			GuaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			GuaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			AssertEquals(2, GuaranteeHeader.AdditionalGuaranteeReferences.Count);
			Factory.Save();
			AssertEquals(0, GuaranteeHeader.AdditionalGuaranteeReferences.Count);
		}

		public void TestOpeningCusGuaranteeLineTransactions()
		{
			AssertEquals("OpeningCusGuaranteeLineTransactions.Count", 0, GuaranteeHeader.OpeningCusGuaranteeLineTransactions.Count);

			var transaction1 = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
			var transaction2 = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction2.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			var transaction3 = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction3.CPL_TransactionType = "XYZ";
			AssertEquals("OpeningCusGuaranteeLineTransactions.Count", 2, GuaranteeHeader.OpeningCusGuaranteeLineTransactions.Count);
			AssertEquals($"Contains {nameof(transaction1)}", true, GuaranteeHeader.OpeningCusGuaranteeLineTransactions.Contains(transaction1));
			AssertEquals($"Contains {nameof(transaction2)}", true, GuaranteeHeader.OpeningCusGuaranteeLineTransactions.Contains(transaction2));
			AssertEquals($"Contains {nameof(transaction3)}", false, GuaranteeHeader.OpeningCusGuaranteeLineTransactions.Contains(transaction3));
		}

		public void TestHasOpeningBalanceTransaction()
		{
			GuaranteeHeader.CusGuaranteeLineTransactions.AdditionalFilter = new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionDate, SQLComparisonOperator.GreaterThan, ZDate.Today.AddMonths(-1));
			AssertEquals("When there are no line transactions, HasOpeningBalanceTransaction", false, GuaranteeHeader.HasOpeningBalanceTransaction);

			var transaction = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transaction.CPL_TransactionDate = ZDate.Today;
			CombineAssertions("When there is at least one transaction matching filter on TransactionDate", () =>
			{
				AssertEquals("CusGuaranteeLineTransactions.Count", 1, GuaranteeHeader.CusGuaranteeLineTransactions.Count);
				AssertEquals("OpeningCusGuaranteeLineTransactions.Count", 1, GuaranteeHeader.OpeningCusGuaranteeLineTransactions.Count);
				AssertEquals("HasOpeningBalanceTransaction", true, GuaranteeHeader.HasOpeningBalanceTransaction);
			});

			transaction.CPL_TransactionDate = ZDate.Today.AddMonths(-2);
			CombineAssertions("When there is at least one transaction not matching filter on TransactionDate (proves that filter on CusGuaranteeLineTransactions does not affect HasOpeningBalanceTransaction)", () =>
			{
				AssertEquals("CusGuaranteeLineTransactions.Count", 0, GuaranteeHeader.CusGuaranteeLineTransactions.Count);
				AssertEquals("OpeningCusGuaranteeLineTransactions.Count", 1, GuaranteeHeader.OpeningCusGuaranteeLineTransactions.Count);
				AssertEquals("HasOpeningBalanceTransaction", true, GuaranteeHeader.HasOpeningBalanceTransaction);
			});
		}

		public void TestBalanceCannotBeChangedToNonZeroAfterInitialisation()
		{
			Factory.Save();
			AssertEquals(0m, GuaranteeHeader.CPH_Balance);

			GuaranteeHeader.CPH_Balance = 1000m;
			Factory.Save();
			AssertEquals(1000m, GuaranteeHeader.CPH_Balance);

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				GuaranteeHeader.CPH_Balance = 5000m;
			});
		}

		void GenerateGuaranteeHeader()
		{
			var permitHeaderTest = GuaranteeHeader;
			permitHeaderTest.CPH_Balance = 5000;
			permitHeaderTest.CusGuaranteeLineTransactions.DeleteAll();

			var tran1 = (AutoCusPermitLineTransaction)permitHeaderTest.CusGuaranteeLineTransactions.AddNew();
			tran1.CPL_Reference = "Ref";
			tran1.CPL_TranValue = 498;
			tran1.CPL_TransactionStatus = "CON";
			tran1.CPL_IsAggregated = true;
			var tran2 = (AutoCusPermitLineTransaction)permitHeaderTest.CusGuaranteeLineTransactions.AddNew();
			tran2.CPL_Reference = "Ref";
			tran2.CPL_TranValue = 2;
			tran2.CPL_TransactionStatus = "CON";
			tran2.CPL_IsAggregated = true;

			var tran3 = (AutoCusPermitLineTransaction)permitHeaderTest.CusGuaranteeLineTransactions.AddNew();
			tran3.CPL_Reference = "Ref";
			tran3.CPL_TranValue = 47;
			tran3.CPL_TransactionStatus = "PND";
			tran3.CPL_IsAggregated = false;
			var tran4 = (AutoCusPermitLineTransaction)permitHeaderTest.CusGuaranteeLineTransactions.AddNew();
			tran4.CPL_Reference = "Ref";
			tran4.CPL_TranValue = 3;
			tran4.CPL_TransactionStatus = "PND";
			tran4.CPL_IsAggregated = false;

			var tran5 = (AutoCusPermitLineTransaction)permitHeaderTest.CusGuaranteeLineTransactions.AddNew();
			tran5.CPL_Reference = "Ref";
			tran5.CPL_TranValue = 25;
			tran5.CPL_TransactionStatus = "CON";
			tran5.CPL_IsAggregated = false;
			var tran6 = (AutoCusPermitLineTransaction)permitHeaderTest.CusGuaranteeLineTransactions.AddNew();
			tran6.CPL_Reference = "Ref";
			tran6.CPL_TranValue = 20;
			tran6.CPL_TransactionStatus = "CON";
			tran6.CPL_IsAggregated = false;

			Factory.Save();
		}

		#region Implementation

		protected override ControllerID ExpectedControllerID => ControllerIDs.Customs.Guarantees;

		protected override ZString ShortName => "Guarantee";

		protected override BaseCusGuaranteeHeader GetNewPermitHeader(BusinessObjectFactory factory) => factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

		protected override bool CanPersistedObjectBeDeleted => false;

		BaseCusGuaranteeHeader GuaranteeHeader => PermitHeader;

		#endregion

		class GuaranteeWithAdditionalReferencesForTest : BaseCusGuaranteeHeader
		{
			public GuaranteeWithAdditionalReferencesForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override bool SupportsAdditionalCustomsReferencesCore => true;
		}
	}
}
