using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	public class WhsPickAvailableInventoryValidationUS : WhsPickAvailableInventoryValidationTest
	{
		#region TestCheckPickLineQuantity_FullPackageGroupID

		public void TestCheckPickLineQuantity_FullPackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA1 = data.Whs1.DefaultLocation;
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "Allocating this inventory would split products in Package Group ID '123'. You must allocate the same whole Package Count for all other products with this Package Group ID from the same location.";

			// receive 3 packages "123"
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, locationA1.PK, "KEY-1", "123", 4m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 12m, locationA1.PK, "KEY-1", "123", 4m);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// order 2 packages "123"
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m, "KEY-1", "DummyOutward-1", "123");
			Helper.CreateWhsOrderLine(order, data.Part2, 8m, "KEY-1", "DummyOutward-1", "123");

			var pick = Helper.CreatePickNew(order);
			var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPart == data.Part1).AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPart == data.Part2).AvailableInventories[0];
			availableInventory1.PickLineQuantity = 4m;
			AssertHasError(availableInventory1.PickLineQuantityInfo, expectedErrorMessage);

			availableInventory2.PickLineQuantity = 4m;
			AssertNoError(availableInventory2.PickLineQuantityInfo, expectedErrorMessage);

			availableInventory2.PickLineQuantity = 8m;
			AssertHasError(availableInventory2.PickLineQuantityInfo, expectedErrorMessage);

			availableInventory1.PickLineQuantity = 8m;
			AssertNoError(availableInventory1.PickLineQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckPickLineQuantity_DivisableByPerPackageQty

		public void TestCheckPickLineQuantity_DivisableByPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA1 = data.Whs1.DefaultLocation;
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "Units picked must be divisible by Per Group Quantity.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, locationA1.PK, "KEY-1", "", 4m);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);

			var pick = Helper.CreatePickNew(order);
			var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPart == data.Part1).AvailableInventories[0];
			AssertNoError(availableInventory1.PickLineQuantityInfo, expectedErrorMessage);

			availableInventory1.PickLineQuantity = 2m;
			AssertHasError(availableInventory1.PickLineQuantityInfo, expectedErrorMessage);

			availableInventory1.PickLineQuantity = 8m;
			AssertNoError(availableInventory1.PickLineQuantityInfo, expectedErrorMessage);

			availableInventory1.PickLineQuantity = 5m;
			AssertHasError(availableInventory1.PickLineQuantityInfo, expectedErrorMessage);
		}

		#endregion
	}
}
