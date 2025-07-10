using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class NPBOCusGuaranteeLineTransactionValidationTest : TestCaseWithFactory
	{
		public void TestCheckTransactionType()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();

			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
			newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			AssertNoErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, NPBOCusGuaranteeLineTransactionValidation.MustAddOBLBeforeAnyOtherTransactionType);
			newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertHasErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, NPBOCusGuaranteeLineTransactionValidation.MustAddOBLBeforeAnyOtherTransactionType);

			guaranteeHeader.CusGuaranteeLineTransactions.DeleteAll();
			var existingTransactionLine = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			existingTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;

			Env.Security.GuaranteesManualTransactions.IsAllowed = true;
			newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			AssertHasErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);
			newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertNoErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);
			AssertNoErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, NPBOCusGuaranteeLineTransactionValidation.DoNotHaveManualTransactionPermission);
			newTransactionLine.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
			AssertNoErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, NPBOCusGuaranteeLineTransactionValidation.DoNotHaveManualTransactionPermission);

			Env.Security.GuaranteesManualTransactions.IsAllowed = false;
			newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			AssertNoErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, NPBOCusGuaranteeLineTransactionValidation.DoNotHaveManualTransactionPermission);
			newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertHasErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, NPBOCusGuaranteeLineTransactionValidation.DoNotHaveManualTransactionPermission);
			newTransactionLine.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
			AssertHasErrorContaining(newTransactionLine.CPL_TransactionTypeInfo, NPBOCusGuaranteeLineTransactionValidation.DoNotHaveManualTransactionPermission);
		}

		public void TestCheckTransactionTypeNotAffectedByGuaranteeLineTransactionsFilters()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.CusGuaranteeLineTransactions.AdditionalFilter = new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionDate, SQLComparisonOperator.GreaterThan, ZDate.Today.AddMonths(-1));

			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
			newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			AssertNoErrorContaining("When there are no line transactions", newTransactionLine.CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);

			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transaction.CPL_TransactionDate = ZDate.Today.AddMonths(-2);
			newTransactionLine.Validation.ValidateCPL_TransactionType();
			AssertHasErrorContaining("When there is one line transaction and not matching filter on TransactionDate (proves that filter on CusGuaranteeLineTransactions does not affect validation)", newTransactionLine.CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);

			transaction.CPL_TransactionDate = ZDate.Today;
			newTransactionLine.Validation.ValidateCPL_TransactionType();
			AssertHasErrorContaining("When there is one line transaction matching filter on TransactionDate", newTransactionLine.CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);
		}

		public void TestCheckTransactionValue()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);

			CombineAssertions(() =>
			{
				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				newTransactionLine.CPL_TranValue = 0m;
				AssertHasErrorContaining("OBL-Empty", newTransactionLine.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);
				newTransactionLine.CPL_TranValue = -50m;
				AssertHasErrorContaining("OBL-Negative", newTransactionLine.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);
				newTransactionLine.CPL_TranValue = 50m;
				AssertNoErrorContaining("OBL-Positive", newTransactionLine.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);

				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
				newTransactionLine.CPL_TranValue = -50m;
				AssertNoErrorContaining("ADJ-Negative", newTransactionLine.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);
				ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);

				newTransactionLine.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
				ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);
			});
		}

		public void TestCheckTransactionValue_OBAType_RemainingBalance()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			guaranteeHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 100m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.OBL);
			guaranteeHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, -50m, ZDecimal.Zero);

			CombineAssertions(() =>
			{
				AssertEquals("Default CPH_Calc_OpeningBalance", new ZDecimal(100), guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertEquals("Default CPH_Calc_TotalBalanceIncludingPendingDecimal", new ZDecimal(50), guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);

				var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
				newTransactionLine.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
				newTransactionLine.CPL_TranValue = -60;
				AssertHasErrorContaining("Has error", newTransactionLine.CPL_TranValueInfo, NPBOCusGuaranteeLineTransactionValidation.RemainingBalanceMustNotBeBelowZeroAfterTransactionIsAdded);

				newTransactionLine.CPL_TranValue = -50;
				AssertNoErrorContaining("No error", newTransactionLine.CPL_TranValueInfo, NPBOCusGuaranteeLineTransactionValidation.RemainingBalanceMustNotBeBelowZeroAfterTransactionIsAdded);
			});
		}

		public void TestCheckTransactionValue_ADJType_RemainingBalance_BeyondGuaranteeAmount()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			guaranteeHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 100m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.OBL);
			CombineAssertions(() =>
			{
				AssertEquals("Default CPH_Calc_OpeningBalance", new ZDecimal(100), guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertEquals("Default CPH_Calc_TotalBalanceIncludingPendingDecimal", new ZDecimal(100), guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);

				var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
				newTransactionLine.CPL_TranValue = 1;
				AssertHasErrorContaining("Has error", newTransactionLine.CPL_TranValueInfo, NPBOCusGuaranteeLineTransactionValidation.RemainingBalanceMustNotBeBeyondGuaranteeAmountAfterADJTransactionIsAdded);

				newTransactionLine.CPL_TranValue = -1;
				AssertNoErrorContaining("No error", newTransactionLine.CPL_TranValueInfo, NPBOCusGuaranteeLineTransactionValidation.RemainingBalanceMustNotBeBeyondGuaranteeAmountAfterADJTransactionIsAdded);
			});
		}

		public void TestCheckTransactionValue_ADJType_RemainingBalance_BelowZero()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			guaranteeHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 100m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.OBL);
			CombineAssertions(() =>
			{
				AssertEquals("Default CPH_Calc_OpeningBalance", new ZDecimal(100), guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertEquals("Default CPH_Calc_TotalBalanceIncludingPendingDecimal", new ZDecimal(100), guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);

				var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
				newTransactionLine.CPL_TranValue = -101;
				AssertHasErrorContaining("Has error", newTransactionLine.CPL_TranValueInfo, NPBOCusGuaranteeLineTransactionValidation.RemainingBalanceMustNotBeBelowZeroAfterTransactionIsAdded);

				newTransactionLine.CPL_TranValue = -1;
				AssertNoErrorContaining("No error", newTransactionLine.CPL_TranValueInfo, NPBOCusGuaranteeLineTransactionValidation.RemainingBalanceMustNotBeBelowZeroAfterTransactionIsAdded);
			});
		}

		public void TestCheckReference()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);

			CombineAssertions(() =>
			{
				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_ReferenceInfo);

				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
				ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_ReferenceInfo);

				newTransactionLine.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
				ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_ReferenceInfo);
			});
		}

		public void TestCheckComment()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);

			CombineAssertions(() =>
			{
				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				newTransactionLine.CPL_Comment = ZString.Empty;
				AssertNoErrorContaining(newTransactionLine.CPL_CommentInfo, MandatoryValidation.MustBeEntered);

				newTransactionLine.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
				ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_CommentInfo);

				newTransactionLine.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
				ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_CommentInfo);
			});
		}

		public void TestCheckCPL_TransactionDateIsNotEmpty()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			var newTransactionLine = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);

			newTransactionLine.CPL_TransactionType = ZString.Empty;
			newTransactionLine.Validation.ValidateCPL_TranValue();
			AssertNoNotifications(newTransactionLine.CPL_TransactionDateInfo);

			newTransactionLine.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
			ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_TransactionDateInfo);

			newTransactionLine.CPL_TransactionType = "abc";
			ValidationTestHelper.AssertErrorIfNotEntered(newTransactionLine.CPL_TransactionDateInfo);
		}
	}
}
