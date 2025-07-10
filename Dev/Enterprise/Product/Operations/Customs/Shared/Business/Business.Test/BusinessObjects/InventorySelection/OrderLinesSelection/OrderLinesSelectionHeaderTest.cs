using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(OrderLinesSelectionHeader))]
	sealed class OrderLinesSelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatus()
		{
			var orderLines_AllPicked = new[] { orderLinePicked.PK, orderLinePickedMultiInventory.PK };
			var header = new OrderLinesSelectionHeader(declaration, orderLines_AllPicked);
			AssertEquals(OrderLinesSelectionHeaderStatus.AllQualified, header.Status);

			var orderLines_PartiallyPicked = new[] { orderLinePicked.PK, orderLinePickedMultiInventory.PK, orderLineUnpicked.PK };
			header = new OrderLinesSelectionHeader(declaration, orderLines_PartiallyPicked);
			AssertEquals(OrderLinesSelectionHeaderStatus.PartiallyQualified, header.Status);

			var orderLines_NonePicked = new[] { orderLineUnpicked.PK };
			header = new OrderLinesSelectionHeader(declaration, orderLines_NonePicked);
			AssertEquals(OrderLinesSelectionHeaderStatus.NoneQualified, header.Status);
		}

		public void TestImportInventoriesByOrderLines()
		{
			header.ImportInventoriesByOrderLines();

			var invoiceLines = declaration.InvoiceLines;

			CombineAssertions("InvoiceLines should be imported to the declaration.", () =>
			{
				AssertContainsExactElementsInAnyOrder("JI_BondedWHSOrderNumber", new ZString[] { "W0000111", "W0000222", "W0000222" }, invoiceLines.Select(line => line.JI_BondedWHSOrderNumber));
				AssertContainsExactElementsInAnyOrder("JI_BondedWHSOrderLineNumber", new ZShort[] { 1, 2, 2 }, invoiceLines.Select(line => line.JI_BondedWHSOrderLineNumber));
				AssertContainsExactElementsInAnyOrder("JI_BondedWhsQuantity", new ZDecimal[] { 10m, 6m, 9m }, invoiceLines.Select(line => line.JI_BondedWhsQuantity));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-2");

			orderLinePicked = CreateNewPickedOrderAndOrderLines("ORD1", "W0000111", 1, 10m, (whsInventory1, qtyToPick: 10m));
			orderLinePickedMultiInventory = CreateNewPickedOrderAndOrderLines("ORD2", "W0000222", 2, 15m, (whsInventory1, qtyToPick: 6m), (whsInventory2, qtyToPick: 9m));
			orderLineUnpicked = CreateNewPickedOrderAndOrderLines("ORD3", "W0000333", 3, 8m);
			Factory.Save();

			header = GetNewBusinessObject() as OrderLinesSelectionHeader;

			IWhsDocketLine CreateNewPickedOrderAndOrderLines(ZString orderRefText, ZString orderID, ZShort orderLineNo, ZDecimal totalOrderQty, params (IWhsInventoryView inventory, ZDecimal qtyToPick)[] inventoryQtyPairs)
			{
				var order = Helper.GetNewWhsOrder(Helper.Importer.PK, whsWarehouse, orderRefText);
				order.WD_DocketID = orderID;
				var orderLine = Helper.GetNewWhsOrderLine(order, Helper.Part, totalOrderQty);
				orderLine.WE_LineNo = orderLineNo;
				Helper.GetNewWhsPick(new[] { order });
				foreach (var pair in inventoryQtyPairs)
				{
					Helper.GetNewWhsPickLine(orderLine, pair.inventory, pair.qtyToPick);
				}

				return orderLine;
			}
		}
		OrderLinesSelectionHeader header;
		IWhsDocketLine orderLinePicked;
		IWhsDocketLine orderLinePickedMultiInventory;
		IWhsDocketLine orderLineUnpicked;
		BaseJobDeclaration declaration;

		protected override BusinessObject GetNewBusinessObject()
		{
			declaration = Factory.New<BaseJobDeclaration>();
			var orderLines = new[] { orderLinePicked.PK, orderLinePickedMultiInventory.PK, orderLineUnpicked.PK };
			var header = new OrderLinesSelectionHeader(declaration, orderLines);

			return header;
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;
	}
}
