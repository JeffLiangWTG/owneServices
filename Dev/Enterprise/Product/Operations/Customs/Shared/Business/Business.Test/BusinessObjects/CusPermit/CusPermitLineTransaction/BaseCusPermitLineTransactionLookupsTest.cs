using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusPermitLineTransactionLookupsTest : SharedCusPermitLineTransactionLookupsTest<BaseCusPermitLineTransactionLookups, BaseCusPermitLineTransaction>
	{
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
