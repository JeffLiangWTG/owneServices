using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsBondedChangeOfInventory))]
	class WhsBondedChangeOfInventoryTest : WhsNonPersistentBusinessObjectTestCase<WhsBondedChangeOfInventory>
	{
		#region Related Entities

		public void TestOrder()
		{
			var order = Factory.New<WhsOrder>();
			var changeOfInventory = new WhsBondedChangeOfInventory(Factory);
			AssertNull(changeOfInventory.Order);

			changeOfInventory.Order = order;
			AssertEquals(order, changeOfInventory.Order);
		}

		public void TestReceive()
		{
			var receive = Factory.New<WhsReceive>();
			var changeOfInventory = new WhsBondedChangeOfInventory(Factory);
			AssertNull(changeOfInventory.Receive);

			changeOfInventory.Receive = receive;
			AssertEquals(receive, changeOfInventory.Receive);
		}

		#endregion

		#region TestIStmALogParent

		public void TestLogs()
		{
			var changeOfInventory = GetNewBusinessObject();
			var log = changeOfInventory.GetLogs().AddNew(Events.DataImport);
			Factory.Save();
			AssertEquals("Logs should be non persistent", false, log.IsInDatabase);
		}

		#endregion

		#region TestIJobNumber

		public void TestIJobNumber()
		{
			var changeOfInventory = GetNewBusinessObject();
			AssertEquals("NON PERSISTENT WAREHOUSE BONDED CHANGE OF INVENTORY", ((IJobNumber)changeOfInventory).JobNumber);
		}

		#endregion
	}
}
