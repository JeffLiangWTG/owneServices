using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsModuleInventory))]
	class WhsModuleInventoryTest : WhsBusinessObjectTestCase
	{
		#region Fetch Strategy

		public void TestGetFetchStrategyIsCorrectType()
		{
			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			var inventory = Factory.Load<WhsModuleInventory>(receive.Inventory[0].PK);
			AssertType<WhsModuleInventoryFetchStrategy>(inventory.FetchStrategy);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var data = new TestDataSimpleEnvironment(otherFactory, 2, 1);
			var receive = new WhsTestHelperFunctions(otherFactory).CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			otherFactory.Save();
			return Factory.Load<WhsModuleInventory>(receive.Inventory[0].PK);
		}

		#endregion
	}
}
