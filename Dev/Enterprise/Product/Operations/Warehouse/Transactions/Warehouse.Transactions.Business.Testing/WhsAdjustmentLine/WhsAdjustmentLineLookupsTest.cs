using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentLineLookupsTest : WhsDocketLineLookupsTest<WhsAdjustment, WhsAdjustmentLine>
	{
		#region TestAdjustmentReasonCodes

		public void TestAdjustmentReasonCodes()
		{
			var adjustment = GetNewWhsDocket();
			var line = GetNewDocketLine();
			line.WE_WD = adjustment.PK;
			var reasonCodesForNewAdjustment = line.Lookups.AdjustmentReasonCodes;
			AssertEquals("Precondition", 5, reasonCodesForNewAdjustment.Count);
			AssertEquals(true, reasonCodesForNewAdjustment.ContainsCode("STA"));
			AssertEquals(true, reasonCodesForNewAdjustment.ContainsCode("DAM"));
			AssertEquals(true, reasonCodesForNewAdjustment.ContainsCode("CLI"));
			AssertEquals(true, reasonCodesForNewAdjustment.ContainsCode("SHR"));
			AssertEquals(true, reasonCodesForNewAdjustment.ContainsCode("AMD"));

			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			var reasonCodesForNewOwnershipAdjustment = line.Lookups.AdjustmentReasonCodes;
			AssertEquals("Precondition", 1, reasonCodesForNewOwnershipAdjustment.Count);
			AssertEquals(true, reasonCodesForNewOwnershipAdjustment.ContainsCode("OCH"));
		}

		#endregion

		#region TestInventoryStatuses

		protected override void TestInventoryStatusesCore()
		{
			var available = new CodeDescriptionPair(InventoryStatus.Codes.Available, InventoryStatus.Descriptions.Available);
			var held = new CodeDescriptionPair(InventoryStatus.Codes.Held, InventoryStatus.Descriptions.Held);
			var staged = new CodeDescriptionPair(InventoryStatus.Codes.Staged, InventoryStatus.Descriptions.Staged);
			var readyToPack = new CodeDescriptionPair(InventoryStatus.Codes.ReadyToPack, InventoryStatus.Descriptions.ReadyToPack);

			// Adjust Out Qty Met Adjustment
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);
			recorder.LinkToDocket(Helper.CreateWhsOrder(data.Org1, data.Whs1));

			var inventory = receive.Inventory[0];
			recorder.AdjustOutInventory(inventory, inventory.WI_WL, 1m);
			var lostAdjustment = (WhsAdjustment)recorder;
			AssertEquals(1, lostAdjustment.Lines.Count);
			var lostAdjustmentLine = lostAdjustment.Lines[0];

			AssertEquals(4, lostAdjustmentLine.Lookups.InventoryStatuses.Count);
			AssertCollectionContains(available, lostAdjustmentLine.Lookups.InventoryStatuses);
			AssertCollectionContains(held, lostAdjustmentLine.Lookups.InventoryStatuses);
			AssertCollectionContains(staged, lostAdjustmentLine.Lookups.InventoryStatuses);
			AssertCollectionContains(readyToPack, lostAdjustmentLine.Lookups.InventoryStatuses);

			// Standard Adjustment
			var line = GetNewDocketLine();
			AssertEquals(2, line.Lookups.InventoryStatuses.Count);
			AssertCollectionContains(available, line.Lookups.InventoryStatuses);
			AssertCollectionContains(held, line.Lookups.InventoryStatuses);

			// Child Onwership Adjustment
			var childAdjustment = GetNewWhsDocket();
			var childAdjustmentLine = GetNewDocketLine();
			childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_WD_ParentDocket = lostAdjustment.PK;

			AssertEquals(2, childAdjustmentLine.Lookups.InventoryStatuses.Count);
			AssertCollectionContains(available, childAdjustmentLine.Lookups.InventoryStatuses);
			AssertCollectionContains(held, childAdjustmentLine.Lookups.InventoryStatuses);
		}

		#endregion

		#region TestBondedLocations

		public void TestBondedPickLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = adjustment.Lines.AddNew();
			adjustmentLine.WE_OP = data.Part1.PK;
			adjustmentLine.WE_TransactionQuantity = 10m;

			var propertyName = WhsLocationCollection.FilterSchema.BondedLocation + ":Property0";
			AssertEquals("Bonded Pick Locations filter is added.", true, adjustmentLine.Lookups.Locations.FilterBusinessObjectDefaults.ContainsDefaultFor(propertyName));

			adjustmentLine.WE_TransactionQuantity = -10m;
			AssertEquals("Bonded Pick Locations filter is not added.", false, adjustmentLine.Lookups.Locations.FilterBusinessObjectDefaults.ContainsDefaultFor(propertyName));

			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			adjustmentLine.WE_TransactionQuantity = 10m;
			AssertEquals("Bonded Pick Locations filter is not added.", false, adjustmentLine.Lookups.Locations.FilterBusinessObjectDefaults.ContainsDefaultFor(propertyName));
		}

		#endregion

		#region IncludeInactive

		public void TestGetSupplierParts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, data.Whs1.DefaultLocation);
			data.Part1.OP_IsActive = false;
			adjustment.RunPreSaveValidation();
			Factory.Save();

			var partsWithInActive = adjustOut.Lookups.SupplierParts;
			partsWithInActive.Load();
			AssertContainsExactElementsInAnyOrder("Inactive parts should be found in the collection.", new[] { data.Part1, data.Part2 }, partsWithInActive);
			AssertEquals("Filter business objects should have the show inactive parts filter", true, partsWithInActive.FilterBusinessObjectDefaults.ContainsDefaultFor("Active Status" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			var adjustIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, data.Whs1.DefaultLocation);
			Factory.Save();

			var activeParts = adjustIn.Lookups.SupplierParts;
			activeParts.Load();
			AssertContainsExactElementsInAnyOrder("Inactive parts should not be found in the collection.", new[] { data.Part2 }, activeParts);
			AssertEquals("Filter business objects should have the show inactive parts filter", false, activeParts.FilterBusinessObjectDefaults.ContainsDefaultFor("Active Status" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		#endregion

		#region Implementation

		protected override bool NeedWE_WL => true;

		#endregion
	}
}
