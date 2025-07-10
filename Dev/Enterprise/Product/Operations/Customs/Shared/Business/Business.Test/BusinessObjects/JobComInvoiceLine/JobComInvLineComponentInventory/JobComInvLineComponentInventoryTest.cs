using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvLineComponentInventory))]
	sealed class JobComInvLineComponentInventoryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestInventoryAndCustomsEntryKey()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsInventory1 = helper.GetNewReceiveInventory(whsReceive, helper.Part, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = helper.GetNewReceiveInventory(whsReceive, helper.Part2, "PACKAGE2", 20m, 180m, 180m, bondedEntryKey: "EN00124-2");
			var whsInventory3 = helper.GetNewReceiveInventory(whsReceive, helper.Part2, "PACKAGE3", 20m, 180m, 180m);
			whsInventory3.WI_AllocationKey = "WI-12345";
			var whsInventoryWithEmptyEntryKey = helper.GetNewReceiveInventory(whsReceive, helper.Part2, "PACKAGE3", 20m, 180m, 180m, bondedEntryKey: "");
			Factory.Save();

			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_AllocationKey = "EN00123-1";
			AssertEquals("EN00123-1", inventory.CustomsEntryKey);
			AssertEquals(whsInventory1.PK, inventory.Inventory.PK);

			inventory.CustomsEntryKey = "EN00124-2";
			AssertEquals("EN00124-2", inventory.JIV_AllocationKey);
			AssertEquals(whsInventory2.PK, inventory.Inventory.PK);

			inventory.CustomsEntryKey = ZString.Empty;
			AssertEquals(ZString.Empty, inventory.JIV_AllocationKey);
			AssertNull($"{nameof(whsInventoryWithEmptyEntryKey)} should not be load.", inventory.Inventory);
			AssertEquals(ZString.Empty, inventory.CustomsEntryKey);

			inventory.CustomsEntryKey = "WI-12345";
			AssertEquals(ZString.Empty, whsInventory3.WI_BondedEntryKey);
			AssertEquals("WI-12345", whsInventory3.WI_AllocationKey);
			AssertEquals("Inventory should be retrieved based on WI_AllocationKey, if not found then based on WI_BondedEntryKey.", whsInventory3.PK, inventory.Inventory.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (JobComInvLineComponentInventory)base.GetNewBusinessObjectForDeleteTest(factory);
			result.JIV_ClusterKey = 1;
			return result;
		}
	}
}
