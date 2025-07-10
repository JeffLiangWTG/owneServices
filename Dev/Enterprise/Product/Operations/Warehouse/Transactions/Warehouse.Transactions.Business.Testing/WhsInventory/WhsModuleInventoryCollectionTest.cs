using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsModuleInventoryCollection))]
	class WhsModuleInventoryCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		#region TestImplementation

		public void TestImplementation()
		{
			var collection = new WhsModuleInventoryCollection(Factory);
			var moduleInventory = collection.AddNew();
			WhsModuleInventory compilerEnforcedTypeCheck = collection[0];
			AssertEquals(typeof(WhsModuleInventory), moduleInventory.GetType());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsModuleInventoryCollection(Factory);
		}

		#endregion
	}
}
