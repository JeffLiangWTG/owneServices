using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitLineTransactionValidationTest<TSharedCusPermitLineTransactionValidation, TSharedCusPermitLineTransaction> : BusinessObjectValidationTestCase
			where TSharedCusPermitLineTransactionValidation : SharedCusPermitLineTransactionValidation
			where TSharedCusPermitLineTransaction : SharedCusPermitLineTransaction
	{
		public void TestValidationType()
		{
			AssertType<TSharedCusPermitLineTransactionValidation>(LineTransaction.Validation);
		}

		public virtual void TestCheckCPL_TranValue()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			LineTransaction.CPL_TranValue = -300;
			AssertHasError(LineTransaction.CPL_TranValueInfo, SharedCusPermitLineTransactionValidation.BalanceCannotBeNegative);
			LineTransaction.CPL_TranValue = 300;
			AssertNoError(LineTransaction.CPL_TranValueInfo, SharedCusPermitLineTransactionValidation.BalanceCannotBeNegative);
		}

		public virtual void TestCheckCPL_TransactionType()
		{
			LineTransaction.CPL_TransactionType = ZString.Empty;
			AssertHasErrorContaining(LineTransaction.CPL_TransactionTypeInfo, MandatoryValidation.MustBeEntered);
			LineTransaction.CPL_TransactionType = "ZZZ";
			AssertHasErrorContaining(LineTransaction.CPL_TransactionTypeInfo, ListValidation.InvalidCodeError);
			LineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertNoErrorContaining(LineTransaction.CPL_TransactionTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCPL_TransactionType_OBA()
		{
			var message = "Transaction with OBA type can only be added for Guarantee.";
			LineTransaction.PermitHeader.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Permit;
			LineTransaction.CPL_TransactionType = GuaranteeTransactionTypeList.Codes.OBA;
			AssertHasErrorContaining(LineTransaction.CPL_TransactionTypeInfo, message);

			LineTransaction.PermitHeader.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			LineTransaction.Validation.ValidateCPL_TransactionType();
			AssertNoErrorContaining(LineTransaction.CPL_TransactionTypeInfo, message);
		}

		public void TestCPL_TransactionCategory()
		{
			LineTransaction.CPL_TransactionCategory = ZString.Empty;
			AssertHasErrorContaining(LineTransaction.CPL_TransactionCategoryInfo, MandatoryValidation.MustBeEntered);
			LineTransaction.CPL_TransactionCategory = "ZZZ";
			AssertHasErrorContaining(LineTransaction.CPL_TransactionCategoryInfo, ListValidation.InvalidCodeError);
			LineTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			AssertNoErrorContaining(LineTransaction.CPL_TransactionCategoryInfo, ListValidation.InvalidCodeError);
		}

		public virtual void TestCheckCPL_Reference()
		{
			LineTransaction.CPL_Reference = ZString.Empty;
			AssertHasErrorContaining(LineTransaction.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
			LineTransaction.CPL_Reference = "REF";
			AssertNoErrorContaining(LineTransaction.CPL_ReferenceInfo, MandatoryValidation.MustBeEntered);
		}

		#region Implementation

		protected abstract TSharedCusPermitLineTransaction GetNewLineTransaction(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			LineTransaction = GetNewLineTransaction(Factory);
		}

		protected TSharedCusPermitLineTransaction LineTransaction { get; set; }

		protected SharedCusPermitHeader PermitHeader => LineTransaction.PermitHeader;

		#endregion
	}
}
