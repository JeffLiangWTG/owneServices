using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPermitLineTransactionValidationTest : SharedCusPermitLineTransactionValidationTest<BaseCusPermitLineTransactionValidation, BaseCusPermitLineTransaction>
	{
		public void TestCheckCPL_TranQty()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			LineTransaction.CPL_TranQty = -300;
			AssertHasError(LineTransaction.CPL_TranQtyInfo, SharedCusPermitLineTransactionValidation.BalanceCannotBeNegative);
			LineTransaction.CPL_TranQty = 300;
			AssertNoError(LineTransaction.CPL_TranQtyInfo, SharedCusPermitLineTransactionValidation.BalanceCannotBeNegative);
		}

		#region Implementation

		protected override BaseCusPermitLineTransaction GetNewLineTransaction(BusinessObjectFactory factory)
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var lineTransaction = permitHeader.CusPermitLineTransactions.AddNew();
			lineTransaction.FillWithValidTestData();
			return lineTransaction;
		}

		#endregion
	}
}
