using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickAvailableInventorySplitByUOMValidationTest : WhsPickAvailableInventorySplitBaseValidationTest<WhsPickAvailableInventorySplitByUOM, WhsPickAvailableInventorySplitByUOMValidation>
	{
		#region TestValidatePickedDate

		public override void TestValidatePickedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Two Inventories are picked.", 2, availableInventory1.PickLines.Count()); // because of 2 inventories

			var availableInventory2 = CreateAvailableInventory();
			availableInventory2.SetData("O1", 3m, "CAS", UOMPackTypesList.Codes.Pallet, availableInventory1.PickLines.ToPickLinePairs(), availableInventory1);
			AssertNoErrors(availableInventory2.PickedDateInfo);

			availableInventory2.PickedDate = ZDateTimeOffset.Now;
			AssertHasError(availableInventory2.PickedDateInfo, "The number of Attributes that were Release Captured does not match the numbers of Units that were Picked.");

			foreach (WhsPickLine pickLine in availableInventory1.PickLines)
			{
				// fully release all pick lines
				pickLine.WZ_ReleaseCapturedPartAttrib1 = "RED";
			}

			availableInventory2.Validation.ValidatePickedDate();
			AssertNoErrors(availableInventory2.PickedDateInfo);
		}

		#endregion

		#region Implementation

		protected override void SetDataOnInventorySplit(WhsPickAvailableInventorySplitByUOM inventorySplit, WhsPickAvailableInventory availableInventory, IPickLinePair[] pickLines)
		{
			inventorySplit.SetData("O1", 3m, "CAS", UOMPackTypesList.Codes.Pallet, pickLines, availableInventory);
		}

		protected override WhsPickAvailableInventorySplitByUOM CreateAvailableInventory() => new WhsPickAvailableInventorySplitByUOM(Factory);

		protected override WhsPickAvailableInventorySplitByUOM CreateAvailableInventoryWithPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertNotNull("Precondition", pick.OrderedInventories[0]);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull("Precondition", availableInventory);
			AssertEquals("Precondition", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Precondition", 1, availableInventory.PickLines.Count());
			AssertEquals("Precondition", 1, availableInventory.AvailableInventoriesSplitByUOM.Count);
			return availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>().Single();
		}

		#endregion
	}
}
