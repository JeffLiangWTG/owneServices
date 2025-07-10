using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class NPBOCusGuaranteeLineTransactionLookupsTest : TestCaseWithFactory
	{
		public void TestTransactionTypes()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			guaranteeHeader.FillWithValidTestData();
			NPBOCusGuaranteeLineTransaction newLineTransaction = new NPBOCusGuaranteeLineTransaction(guaranteeHeader);
			AssertEquals("ADJ, OBL, OBA", newLineTransaction.Lookups.TransactionTypes.CodesAsString);
		}
	}
}
