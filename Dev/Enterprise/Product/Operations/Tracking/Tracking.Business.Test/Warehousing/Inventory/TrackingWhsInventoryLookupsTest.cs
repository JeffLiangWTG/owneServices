using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingWhsInventoryLookupsTest : WhsInventoryViewLookupsTestCase
	{
		public void TestInventoryStatuses()
		{
			var nonSupportUser = Helper.CreateGlbStaff("NS", "notsupport");
			using (Env.SetTemporaryUserContext(nonSupportUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var inventory = Factory.New<TrackingWhsInventory>();
				AssertEquals("Should return 10 status codes", 10, inventory.Lookups.InventoryStatuses.Count);

				Assert("Should contain AVL", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Available));
				Assert("Should contain HEL", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Held));
				Assert("Should contain PUT", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Putaway));
				Assert("Should contain ARV", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Arrived));
				Assert("Should contain PND", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Pending));
				Assert("Should contain PTA", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.PuttingAway));
				Assert("Should contain REC", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Received));
				Assert("Should contain INT", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.InTransit));
				Assert("Should contain STA", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Staged));
				Assert("Should contain RTP", inventory.Lookups.InventoryStatuses.ContainsCode(InventoryStatus.Codes.ReadyToPack));

				var anotherInventory = Factory.New<TrackingWhsInventory>();
				AssertEquals(inventory.Lookups.InventoryStatuses, anotherInventory.Lookups.InventoryStatuses);
			}
		}
	}
}
