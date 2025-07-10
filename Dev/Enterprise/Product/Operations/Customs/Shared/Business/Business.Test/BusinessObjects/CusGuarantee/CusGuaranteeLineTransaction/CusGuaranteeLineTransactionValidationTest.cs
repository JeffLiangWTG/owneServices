using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusGuaranteeLineTransactionValidationTest : SharedCusPermitLineTransactionValidationTest<CusGuaranteeLineTransactionValidation, BaseCusGuaranteeLineTransaction>
	{
		public void TestCheckCPL_Comment()
		{
			AssertNoErrorContaining(transactionLine1.CPL_CommentInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transactionLine1.CPL_Comment = ZString.Empty;
			AssertNoErrorContaining(transactionLine1.CPL_CommentInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transactionLine1.CPL_Comment = ZString.Empty;
			AssertNoErrorContaining(transactionLine1.CPL_CommentInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			transactionLine1.CPL_Comment = ZString.Empty;
			AssertNoErrorContaining(transactionLine1.CPL_CommentInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			transactionLine1.CPL_Comment = ZString.Empty;
			AssertHasErrorContaining(transactionLine1.CPL_CommentInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_Comment = "COMMENT";
			AssertNoErrorContaining(transactionLine1.CPL_CommentInfo, MandatoryValidation.MustBeEntered);
		}

		public override void TestCheckCPL_Reference()
		{
			AssertNoErrorContaining(transactionLine1.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transactionLine1.CPL_Reference = ZString.Empty;
			AssertNoErrorContaining(transactionLine1.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transactionLine1.CPL_Reference = ZString.Empty;
			AssertNoErrorContaining(transactionLine1.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			transactionLine1.CPL_Reference = ZString.Empty;
			AssertNoErrorContaining(transactionLine1.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			transactionLine1.CPL_Reference = ZString.Empty;
			AssertHasErrorContaining(transactionLine1.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
			transactionLine1.CPL_Reference = "REFERENCE";
			AssertNoErrorContaining(transactionLine1.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
		}

		public override void TestCheckCPL_TranValue()
		{
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transactionLine1.CPL_TranValue = 0;
			AssertHasError(transactionLine1.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);
			transactionLine1.CPL_TranValue = -300;
			AssertHasError(transactionLine1.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);
			transactionLine1.CPL_TranValue = 300;
			AssertNoError(transactionLine1.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);

			transactionLine2.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			transactionLine2.CPL_TranValue = 0;
			AssertHasError(transactionLine2.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);
			transactionLine2.CPL_TranValue = -300;
			AssertNoError(transactionLine2.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);
			transactionLine2.CPL_TranValue = 300;
			AssertNoError(transactionLine2.CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);

			transactionLine2.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			transactionLine1.CPL_TranValue = -500;
			AssertNoErrors(transactionLine2.CPL_TranValueInfo);
		}

		public override void TestCheckCPL_TransactionType()
		{
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transactionLine2.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transactionLine3.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertHasError(transactionLine2.CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);
			AssertNoErrors(transactionLine3.CPL_TransactionTypeInfo);
			transactionLine1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transactionLine2.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			transactionLine3.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertNoErrors(transactionLine2.CPL_TransactionTypeInfo);
		}

		#region Implementation

		protected override BaseCusGuaranteeLineTransaction GetNewLineTransaction(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var lineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			lineTransaction.FillWithValidTestData();
			return lineTransaction;
		}

		protected override void SetUp()
		{
			base.SetUp();
			guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			transactionLine1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionLine1.CPL_Reference = "TRANSLINE1";
			transactionLine2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionLine2.CPL_Reference = "TRANSLINE2";
			transactionLine3 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionLine3.CPL_Reference = "TRANSLINE3";
		}

		BaseCusGuaranteeHeader guaranteeHeader;
		BaseCusGuaranteeLineTransaction transactionLine1;
		BaseCusGuaranteeLineTransaction transactionLine2;
		BaseCusGuaranteeLineTransaction transactionLine3;

		#endregion
	}
}
