using System;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickAvailableInventorySplitByPickedDetails))]
	class WhsPickAvailableInventorySplitByPickedDetailsTest : WhsPickAvailableInventorySplitBaseTest<WhsPickAvailableInventorySplitByPickedDetails>
	{
		#region TestValidation

		protected override Type ExpectedValidationType => typeof(WhsPickAvailableInventorySplitByPickedDetailsValidation);

		#endregion

		#region TestSetDataAndCheckProperties_SplitAfterPicking

		public void TestSetDataAndCheckProperties_SplitAfterPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Assert(receive.IsFinalised);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5);
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories.Count);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(1, availableInventory.PickLines.Count());

			var orderLine = order.Lines[0];
			var pickLine = orderLine.PickLines[0];

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			var splitPickLine = pickLine.Split(2m);

			var splitInventory = CreateAvailableInventory();
			splitInventory.SetData(availableInventory.PickLines.ToPickLinePairs(), availableInventory);
			AssertEquals(5m, splitInventory.StockUnitQuantity);
			AssertEquals("KEG", splitInventory.StockKeepingUnit);
		}

		#endregion

		#region Implementation

		protected override WhsPickAvailableInventorySplitByPickedDetails CreateAvailableInventoryWithPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertNotNull("Precondition", pick.OrderedInventories[0]);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull("Precondition", availableInventory);
			AssertEquals("Precondition", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Precondition", 1, availableInventory.PickLines.Count());
			AssertEquals("Precondition", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			return availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single();
		}

		protected override WhsPickAvailableInventorySplitByPickedDetails CreateAvailableInventory() => new WhsPickAvailableInventorySplitByPickedDetails(Factory);

		protected override WhsPickAvailableInventorySplitBaseCollection<WhsPickAvailableInventorySplitByPickedDetails> GetCollectionFromAvailableInventory(WhsPickAvailableInventory availableInventory)
		{
			return availableInventory.AvailableInventoriesSplitByPickedDetails;
		}

		#endregion
	}
}
