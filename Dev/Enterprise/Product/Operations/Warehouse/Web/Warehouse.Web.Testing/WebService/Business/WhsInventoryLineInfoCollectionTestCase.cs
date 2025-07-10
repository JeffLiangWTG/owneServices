using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsInventoryLineInfoCollectionTestCase : WhsInventoryLineBaseInfoCollectionTestCase<WhsInventoryLineInfo>
	{
		#region Test Cases

		#region TestAdditionalConstructors

		public void TestAdditionalConstructors()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);

			var collection1 = WhsInventoryLineInfoCollection.GetWhsInventoryLineInfoCollectionWithFetchHints(new WhsInventoryViewCollection(factory).Cast<WhsInventoryView>());
			AssertNotNull(collection1);
			AssertEquals(0, collection1.InventoryLineInfos.Count);
			var client = helper.CreateClient("C1");
			var part = helper.CreateProduct("P1", client);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 2);

			var lines = new WhsInventoryViewCollection(factory);
			var line1 = lines.AddNew();
			line1.WI_PalletID = "PalletID1";
			line1.WI_OH_Client = client.PK;
			line1.WI_OP = part.PK;
			line1.WI_WL = whs.DefaultLocation.PK;

			AssertEquals(1, lines.Count);
			var collection2 = WhsInventoryLineInfoCollection.GetWhsInventoryLineInfoCollectionWithFetchHints(lines.Cast<WhsInventoryView>());
			AssertNotNull(collection2);
			AssertEquals(1, collection2.InventoryLineInfos.Count);
			AssertEquals("PalletID1", collection2.InventoryLineInfos[0].PalletID);

			var line2 = lines.AddNew();
			line2.WI_PalletID = "PalletID2";
			line2.WI_OH_Client = client.PK;
			line2.WI_OP = part.PK;
			line2.WI_WL = whs.DefaultLocation.PK;

			AssertEquals(2, lines.Count);
			var collection3 = WhsInventoryLineInfoCollection.GetWhsInventoryLineInfoCollectionWithFetchHints(lines.Cast<WhsInventoryView>());
			AssertNotNull(collection3);
			AssertEquals(2, collection3.InventoryLineInfos.Count);
			AssertEquals("PalletID1", collection3.InventoryLineInfos[0].PalletID);
			AssertEquals("PalletID2", collection3.InventoryLineInfos[1].PalletID);
		}

		public void TestAdditionalConstructors_LocationFormattedCheckDigit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(Factory);
			var location = data.Whs1.DefaultLocation;
			location.FormattedCheckDigit = "11";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "Pallet1");
			line.WI_WL = location.PK;
			line.WI_InDocketLineUnits = 10m;

			var lines = new WhsInventoryViewCollection(Factory) { line };
			var collection = WhsInventoryLineInfoCollection.GetWhsInventoryLineInfoCollectionWithFetchHints(lines.Cast<WhsInventoryView>());
			AssertNotNull(collection);
			AssertEquals(1, collection.InventoryLineInfos.Count);
			AssertEquals("Pallet1", collection.InventoryLineInfos[0].PalletID);
			AssertEquals("11", collection.InventoryLineInfos[0].LocationFormattedCheckDigit);
			AssertEquals("", collection.InventoryLineInfos[0].DestinationLocationFormattedCheckDigit);
		}

		#endregion

		#endregion

		#region Implementation

		protected override WhsInventoryLineBaseInfoCollection<WhsInventoryLineInfo> GetCollection()
		{
			return new WhsInventoryLineInfoCollection();
		}

		protected override WhsInventoryLineInfo GetInventoryLineInfo(WhsInventoryLineBaseInfoCollection<WhsInventoryLineInfo> collection, WhsInventoryView inventory)
		{
			return new WhsInventoryLineInfo(collection, inventory);
		}

		protected override WhsInventoryLineInfo GetInventoryLineInfo()
		{
			return new WhsInventoryLineInfo();
		}

		#endregion
	}
}
