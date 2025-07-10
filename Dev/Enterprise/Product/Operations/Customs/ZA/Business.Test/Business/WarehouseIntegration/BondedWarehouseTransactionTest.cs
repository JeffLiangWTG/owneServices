using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class BondedWarehouseTransactionTestCase : TestCaseWithFactory
	{
		public void TestWarehouseAddress()
		{
			var creator = new MergedDeclarationCreator(Factory);
			var transaction = new BondedWarehouseTransaction(creator.Declaration);
			AssertNull(transaction.Warehouse);
		}
	}
}
