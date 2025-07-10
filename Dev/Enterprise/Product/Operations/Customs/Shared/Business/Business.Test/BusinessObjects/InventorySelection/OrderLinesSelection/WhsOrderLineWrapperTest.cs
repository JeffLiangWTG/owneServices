using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(WhsOrderLineWrapper))]
	sealed class WhsOrderLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrderLine()
		{
			var wrapper = CreateWhsOrderLineWrapper(orderLinePicked);
			Assert(wrapper.OrderLine is IWhsDocketLine);
			AssertEquals((ZShort)1, wrapper.OrderLine.WE_LineNo);
		}

		public void TestPickLines()
		{
			var wrapper = CreateWhsOrderLineWrapper(orderLinePicked);
			AssertContainsExactElementsInAnyOrder(new[] { pickLines.First().PK }, wrapper.PickLines.Select(line => line.PK));

			wrapper = CreateWhsOrderLineWrapper(orderLinePickedMultiInventory);
			AssertContainsExactElementsInAnyOrder(new[] { pickLines.ElementAt(1).PK, pickLines.ElementAt(2).PK }, wrapper.PickLines.Select(line => line.PK));

			wrapper = CreateWhsOrderLineWrapper(orderLineUnpicked);
			AssertEquals(0, wrapper.PickLines.Length);
		}

		public void TestInventoryPickLineDict()
		{
			var wrapper = CreateWhsOrderLineWrapper(orderLinePicked);
			AssertContainsExactElementsInAnyOrder(new[] { (whsInventory1.PK, pickLines.First().PK) }, wrapper.InventoryPickLineDict.Select(pair => (pair.Key.PK, pair.Value.PK)));

			wrapper = CreateWhsOrderLineWrapper(orderLinePickedMultiInventory);
			AssertContainsExactElementsInAnyOrder(new[] { (whsInventory1.PK, pickLines.ElementAt(1).PK), (whsInventory2.PK, pickLines.ElementAt(2).PK) }, wrapper.InventoryPickLineDict.Select(pair => (pair.Key.PK, pair.Value.PK)));

			wrapper = CreateWhsOrderLineWrapper(orderLineUnpicked);
			AssertEquals(0, wrapper.InventoryPickLineDict.Count);
		}

		public void TestAllocatedInventories()
		{
			var wrapper = CreateWhsOrderLineWrapper(orderLinePicked);
			AssertContainsExactElementsInAnyOrder(new[] { whsInventory1.PK }, wrapper.AllocatedInventories.Select(inventory => inventory.PK));

			wrapper = CreateWhsOrderLineWrapper(orderLinePickedMultiInventory);
			AssertContainsExactElementsInAnyOrder(new[] { whsInventory1.PK, whsInventory2.PK }, wrapper.AllocatedInventories.Select(inventory => inventory.PK));

			wrapper = CreateWhsOrderLineWrapper(orderLineUnpicked);
			AssertEquals(0, wrapper.AllocatedInventories.Count());
		}

		public void TestOrder()
		{
			var wrapper = CreateWhsOrderLineWrapper(orderLinePicked);
			AssertEquals("W0000111", wrapper.Order.WD_DocketID);

			wrapper = CreateWhsOrderLineWrapper(orderLinePickedMultiInventory);
			AssertEquals("W0000222", wrapper.Order.WD_DocketID);

			wrapper = CreateWhsOrderLineWrapper(orderLineUnpicked);
			AssertEquals("W0000333", wrapper.Order.WD_DocketID);
		}

		public void TestIsAllocatedToInventories()
		{
			var wrapper = CreateWhsOrderLineWrapper(orderLinePicked);
			Assert(wrapper.IsAllocatedToInventories);

			wrapper = CreateWhsOrderLineWrapper(orderLinePickedMultiInventory);
			Assert(wrapper.IsAllocatedToInventories);

			wrapper = CreateWhsOrderLineWrapper(orderLineUnpicked);
			Assert(!wrapper.IsAllocatedToInventories);
		}

		protected override void SetUp()
		{
			base.SetUp();
			pickLines = new List<IWhsPickLine>();

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-2");

			orderLinePicked = CreateNewPickedOrderAndOrderLines("ORD1", "W0000111", 1, 10m, (whsInventory1, qtyToPick: 10m));
			orderLinePickedMultiInventory = CreateNewPickedOrderAndOrderLines("ORD2", "W0000222", 2, 15m, (whsInventory1, qtyToPick: 6m), (whsInventory2, qtyToPick: 9m));
			orderLineUnpicked = CreateNewPickedOrderAndOrderLines("ORD3", "W0000333", 3, 8m);

			Factory.Save();

			IWhsDocketLine CreateNewPickedOrderAndOrderLines(ZString orderRefText, ZString orderID, ZShort orderLineNo, ZDecimal totalOrderQty, params (IWhsInventoryView inventory, ZDecimal qtyToPick)[] inventoryQtyPairs)
			{
				var order = Helper.GetNewWhsOrder(Helper.Importer.PK, whsWarehouse, orderRefText);
				order.WD_DocketID = orderID;
				var orderLine = Helper.GetNewWhsOrderLine(order, Helper.Part, totalOrderQty);
				orderLine.WE_LineNo = orderLineNo;
				Helper.GetNewWhsPick(new[] { order });
				foreach (var pair in inventoryQtyPairs)
				{
					pickLines.Add(Helper.GetNewWhsPickLine(orderLine, pair.inventory, pair.qtyToPick));
				}

				return orderLine;
			}
		}
		IWhsInventoryView whsInventory1;
		IWhsInventoryView whsInventory2;
		IWhsDocketLine orderLinePicked;
		IWhsDocketLine orderLinePickedMultiInventory;
		IWhsDocketLine orderLineUnpicked;
		IList<IWhsPickLine> pickLines;

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateWhsOrderLineWrapper(orderLinePicked);
		}

		WhsOrderLineWrapper CreateWhsOrderLineWrapper(IWhsDocketLine orderLine)
		{
			return new WhsOrderLineWrapper(orderLine, Factory);
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;
	}
}
