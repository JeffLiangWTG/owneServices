using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class UpdateInventoryStatusesApplicatorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInventoryHeldCodeCollection()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("C1");
			var client2 = helper.CreateClient("C2");
			var holdCode1 = helper.CreateInventoryHeldCode("ABC", "DESC");
			var holdCode2 = helper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1.PK);
			var holdCode3 = helper.CreateInventoryHeldCode("BBB", "BBB description", client1.PK);
			var holdCode4 = helper.CreateInventoryHeldCode("AAA2", "AAA for client 2", client2.PK);
			var holdCode5 = helper.CreateInventoryHeldCode("BBB2", "BBB description", client2.PK);
			var holdCode6 = helper.CreateInventoryHeldCode("CCC", "CCC for client 2", client2.PK);

			var lookups = new UpdateInventoryHeldCodesApplicatorLookups(new UpdateInventoryHeldCodeActionMethodApplicator(Factory));
			var heldCodeCollection = lookups.InventoryHeldCodeCollection.ToArray();
			var emptyHeldCode = heldCodeCollection[0];
			AssertEquals(11, heldCodeCollection.Length);
			AssertNotNull(emptyHeldCode);
			AssertEquals(string.Empty, emptyHeldCode.Code);
			AssertEquals("None", emptyHeldCode.Description);
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "SHORT" && c.Description == "Short Picked"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "LCC" && c.Description == "Lost in Cycle Count"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "HEL" && c.Description == "Held"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "DAM" && c.Description == "Damaged"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "ABC" && c.Description == "DESC"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "AAA" && c.Description == "AAA for client 1"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "AAA2" && c.Description == "AAA for client 2"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "BBB" && c.Description == "BBB description"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "BBB2" && c.Description == "BBB description"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "CCC" && c.Description == "CCC for client 2"));
		}
	}
}
