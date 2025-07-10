using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusGuaranteeLineTransactionLookupsTest : SharedCusPermitLineTransactionLookupsTest<CusGuaranteeLineTransactionLookups, BaseCusGuaranteeLineTransaction>
	{
		#region Implementation

		protected override BaseCusGuaranteeLineTransaction GetNewLineTransaction(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var lineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			lineTransaction.FillWithValidTestData();
			return lineTransaction;
		}

		#endregion
	}
}
