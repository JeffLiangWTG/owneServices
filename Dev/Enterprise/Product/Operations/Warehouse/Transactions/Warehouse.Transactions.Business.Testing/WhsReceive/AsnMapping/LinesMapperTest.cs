using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.AsnMapping;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class LinesMapperTest : WhsTestCaseWithFactory
	{
		#region MapInventoriesToAsnLines

		#region TestMapValidationChecks

		public void TestMapValidationChecks_NullReceive()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument null exception thrown if WhsReceive is null.", () => new LinesMapper(null));
		}

		public void TestMapValidationChecks_ZeroInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = data.Org1.PK;

			var linesMapper = new LinesMapper(receive);

			AssertEquals("Precondition", false, receive.Lines.Count > 0);
			linesMapper.MapInventoriesToAsnLines(new List<WhsInventoryView>(), new List<WhsAsnLine>());
			AssertEquals(false, receive.Lines.Count > 0);

			var productInfo = AsnMapperDummies.CreateDummyProduct();
			var dummyInventory = AsnMapperDummies.CreateDummyInventory(receive, productInfo, 5, 22, 0);
			var dummyAsnLine = AsnMapperDummies.CreateDummyAsnLine(receive, productInfo, 12, 4, 11);

			AssertEquals("Precondition", 1, receive.Inventory.Count);
			linesMapper.MapInventoriesToAsnLines(new List<WhsInventoryView>() { dummyInventory }, new[] { dummyAsnLine });
			AssertEquals(1, receive.Inventory.Count);
			AssertEquals(11m, dummyInventory.WI_TotalUnits);
			AssertEquals(dummyAsnLine.WN_OP, dummyInventory.WI_OP);
		}

		public void TestMapValidationChecks_NoASNLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = data.Org1.PK;

			var linesMapper = new LinesMapper(receive);

			AssertEquals("Precondition", false, receive.Lines.Count > 0);
			linesMapper.MapInventoriesToAsnLines(new List<WhsInventoryView>(), new List<WhsAsnLine>());
			AssertEquals(false, receive.Lines.Count > 0);

			var dummyInventory = AsnMapperDummies.CreateDummyInventory(receive, AsnMapperDummies.CreateDummyProduct(), 5, 22, 13);
			AssertEquals("Precondition", 1, receive.Inventory.Count);
			AssertEquals("Precondition", 13m, dummyInventory.WI_InDocketLineUnits);
			AssertEquals("Precondition", (ZShort)5, dummyInventory.WI_LineNo);
			AssertEquals("Precondition", (ZShort)22, dummyInventory.WI_SubLineNo);
			// no ASN lines - inventory should stay unchanged
			linesMapper.MapInventoriesToAsnLines(new List<WhsInventoryView>() { dummyInventory }, new List<WhsAsnLine>());
			// inventory remains unchanged
			AssertEquals(1, receive.Inventory.Count);
			AssertEquals(13m, dummyInventory.WI_InDocketLineUnits);
			AssertEquals((ZShort)5, dummyInventory.WI_LineNo);
			AssertEquals((ZShort)22, dummyInventory.WI_SubLineNo);
		}

		public void TestMapValidationChecks_DifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = data.Org1.PK;

			var linesMapper = new LinesMapper(receive);
			var dummyInventory = AsnMapperDummies.CreateDummyInventory(receive, AsnMapperDummies.CreateDummyProduct(), 5, 22, 13);
			// asn line for another product
			var dummyAsnLine = AsnMapperDummies.CreateDummyAsnLine(receive, AsnMapperDummies.CreateDummyProduct(), 12, 4, 11);

			AssertExceptionThrown<ArgumentException>("Lines mapper works only with inventories and asnLines of the same product",
				() => linesMapper.MapInventoriesToAsnLines(
					new List<WhsInventoryView>() { dummyInventory },
					new[] { dummyAsnLine }));
		}

		#endregion

		#region TestExistingInventoryIsSplitIfNotFitInAsn

		public void TestExistingInventoryIsNotAffectedDuringMapping()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var linesMapper = new LinesMapper(receive);
			var product = AsnMapperDummies.CreateDummyProduct();

			var dummyInventory = AsnMapperDummies.CreateDummyInventory(receive, product, 5, 22, 13);
			var dummyAsnLine = AsnMapperDummies.CreateDummyAsnLine(receive, product, 5, 22, 10);
			// 13 units in Inventory, but only 10 in ASNLine with same LineNo+SubLineNo
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			linesMapper.MapInventoriesToAsnLines(
				new List<WhsInventoryView>() { dummyInventory },
				new[] { dummyAsnLine });

			AssertEquals(1, receive.Inventory.Count);
			AssertNotNull("Original inventory should stay 13.",
				receive.Inventory.Cast<WhsInventoryView>().Single(x => x.WI_InDocketLineUnits == 13 && x.WI_LineNo == 5 && x.WI_SubLineNo == 22));
		}

		#endregion

		#region TestUnassignedInventoryIsSplitToFitMultipleAsn

		public void TestUnassignedInventoryIsSplitToFitMultipleAsn()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var linesMapper = new LinesMapper(receive);
			var product = AsnMapperDummies.CreateDummyProduct();

			var dummyInventory = AsnMapperDummies.CreateDummyInventory(receive, product, 0, 0, 29);
			var dummyAsnLine = AsnMapperDummies.CreateDummyAsnLine(receive, product, 5, 22, 10);
			var dummyAsnLine2 = AsnMapperDummies.CreateDummyAsnLine(receive, product, 5, 23, 15);
			// 29 units in Inventory, but only 10 and 15 in ASNLines
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			linesMapper.MapInventoriesToAsnLines(
				new List<WhsInventoryView>() { dummyInventory },
				new[] { dummyAsnLine, dummyAsnLine2 });

			var inventories = receive.Inventory.Cast<WhsInventoryView>();
			AssertEquals(3, inventories.Count());
			Assert("One inventory to fill first ASN line",
				inventories.Any(x => x.WI_InDocketLineUnits == 10 && x.WI_LineNo == 5 && x.WI_SubLineNo == 22));
			Assert("One inventory to fill second ASN line",
				inventories.Any(x => x.WI_InDocketLineUnits == 15 && x.WI_LineNo == 5 && x.WI_SubLineNo == 23));
			Assert("One inventory for the excess inventory count",
				inventories.Any(x => x.WI_InDocketLineUnits == 4 && x.WI_LineNo == 0 && x.WI_SubLineNo == 0));

			AssertEquals("All inventories should have a docketline assigned", false, inventories.Any(line => line.WI_WE_InDocketLine == ZGuid.Empty));
		}

		#endregion

		#region TestManyInventoryMapToManyAsn

		public void TestManyInventoryMapToManyAsn()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var linesMapper = new LinesMapper(receive);
			var product = AsnMapperDummies.CreateDummyProduct();

			// Inventory
			// Line 0:0   13 UNT
			// Line 0:0   14 UNT
			// AsnLines
			// Line 5:22 10 UNT
			// Line 5:23 15 UNT
			var dummyInventory = AsnMapperDummies.CreateDummyInventory(receive, product, 0, 0, 13);
			var dummyInventory2 = AsnMapperDummies.CreateDummyInventory(receive, product, 0, 0, 14);
			var dummyAsnLine = AsnMapperDummies.CreateDummyAsnLine(receive, product, 5, 22, 10);
			var dummyAsnLine2 = AsnMapperDummies.CreateDummyAsnLine(receive, product, 5, 23, 15);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			linesMapper.MapInventoriesToAsnLines(
				new List<WhsInventoryView>() { dummyInventory, dummyInventory2 },
				new[] { dummyAsnLine, dummyAsnLine2 });

			var inventories = receive.Inventory.Cast<WhsInventoryView>();
			AssertEquals(4, inventories.Count());

			Assert("One inventory to fill part of first ASN line.",
				inventories.Any(x => x.WI_InDocketLineUnits == 13 && x.WI_LineNo == 5 && x.WI_SubLineNo == 23));
			Assert("Second inventory for the unallocated from first ASN line.",
				inventories.Any(x => x.WI_InDocketLineUnits == 2 && x.WI_LineNo == 5 && x.WI_SubLineNo == 23));
			Assert("Third inventory for the second ASN line.",
				inventories.Any(x => x.WI_InDocketLineUnits == 10 && x.WI_LineNo == 5 && x.WI_SubLineNo == 22));
			Assert("4th inventory from the excess unallocated from the inventories.",
				inventories.Any(x => x.WI_InDocketLineUnits == 2 && x.WI_LineNo == 0 && x.WI_SubLineNo == 0));
		}

		#endregion

		#region MapInventoriesToAsnLines_AttributesWithDifferentCases

		public void TestMapInventoriesToAsnLines_AttributesWithDifferentCases_PalletID()
		{
			MapInventoriesToAsnLines_AttributesWithDifferentCasesCore((inv, value) => inv.WI_PalletID = value, (asn, value) => asn.WN_PalletId = value);
		}

		public void TestMapInventoriesToAsnLines_AttributesWithDifferentCases_PartAttrib1()
		{
			MapInventoriesToAsnLines_AttributesWithDifferentCasesCore((inv, value) => inv.WI_PartAttrib1 = value, (asn, value) => asn.WN_PartAttrib1 = value);
		}

		public void TestMapInventoriesToAsnLines_AttributesWithDifferentCases_PartAttrib2()
		{
			MapInventoriesToAsnLines_AttributesWithDifferentCasesCore((inv, value) => inv.WI_PartAttrib2 = value, (asn, value) => asn.WN_PartAttrib2 = value);
		}

		public void TestMapInventoriesToAsnLines_AttributesWithDifferentCases_PartAttrib3()
		{
			MapInventoriesToAsnLines_AttributesWithDifferentCasesCore((inv, value) => inv.WI_PartAttrib3 = value, (asn, value) => asn.WN_PartAttrib3 = value);
		}

		public void TestMapInventoriesToAsnLines_AttributesWithDifferentCases_SerialNumber()
		{
			MapInventoriesToAsnLines_AttributesWithDifferentCasesCore((inv, value) => inv.WI_SerialNumber = value, (asn, value) => asn.WN_SerialNumber = value);
		}

		void MapInventoriesToAsnLines_AttributesWithDifferentCasesCore(Action<WhsInventoryView, string> setInventoryAttribute, Action<WhsAsnLine, string> setAsnLineAttribute)
		{
			var receive = Factory.New<WhsReceive>();
			var linesMapper = new LinesMapper(receive);
			var productPK = Guid.NewGuid();
			var product = AsnMapperDummies.CreateDummyProduct();

			var dummyInventory = AsnMapperDummies.CreateDummyInventory(receive, product, 0, 0, 13);
			var dummyAsnLine = AsnMapperDummies.CreateDummyAsnLine(receive, product, 0, 0, 13);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			var inventoryValue = "PALLET";
			setInventoryAttribute(dummyInventory, inventoryValue);

			var asnValue = "pALLet";
			setAsnLineAttribute(dummyAsnLine, asnValue);

			linesMapper.MapInventoriesToAsnLines(
				new List<WhsInventoryView>() { dummyInventory },
				new[] { dummyAsnLine });

			var inventories = receive.Inventory.Cast<WhsInventoryView>();
			AssertEquals("Only 1 inventory correctly mapped.", 1, inventories.Count());
			Assert("One inventory to fill first ASN line.", inventories.Any(x => x.WI_InDocketLineUnits == 13 && x.WI_LineNo == 0 && x.WI_SubLineNo == 0));
		}

		#endregion

		#endregion

		#region RemapReservedStock

		public void TestRemapReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithUnfulfilledReservedStock = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLineWithQuantity = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithUnfulfilledReservedStock.Inventory[0], 10m);
			Factory.Save();

			receiveLineWithUnfulfilledReservedStock.WE_TransactionQuantity = 0m;
			receiveLineWithUnfulfilledReservedStock.WE_ClientOrderedUnits = 0m;

			AssertEquals("Precondition", 10m, receiveLineWithUnfulfilledReservedStock.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithUnfulfilledReservedStock.WE_TransactionQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithUnfulfilledReservedStock.WE_ClientOrderedUnits);
			AssertEquals("Precondition", 0m, receiveLineWithQuantity.ReservedQuantity);
			AssertEquals("Precondition", 10m, receiveLineWithQuantity.WE_TransactionQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			LinesMapper.RemapUnfulfilledReservedStock(receive);

			AssertEquals("Reserved stocks are remapped to inventory with stock.", 10m, receiveLineWithQuantity.ReservedQuantity);
			AssertEquals("Reserved stocks are remapped to inventory with stock.", 0m, receiveLineWithUnfulfilledReservedStock.ReservedQuantity);
		}

		public void TestRemapReservedStock_ReservedStockFulfilledByExpectedQuantityOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithNoTxnQty.WE_TransactionQuantity);
			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.WE_ClientOrderedUnits);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, receiveLineWithTxnQty.WE_TransactionQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			LinesMapper.RemapUnfulfilledReservedStock(receive);

			AssertEquals("Reserved stocks are remapped to inventory with stock.", 10m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Reserved stocks are remapped to inventory with stock.", 0m, receiveLineWithNoTxnQty.ReservedQuantity);
		}

		public void TestRemapReservedStock_NoUnfulfilledReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLineWithTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithTxnQty1.Inventory[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", 10m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Precondition", 10m, receiveLineWithTxnQty1.WE_TransactionQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty2.ReservedQuantity);
			AssertEquals("Precondition", 10m, receiveLineWithTxnQty2.WE_TransactionQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			LinesMapper.RemapUnfulfilledReservedStock(receive);

			AssertEquals("No reserved stocks are remapped.", 10m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("No reserved stocks are remapped.", 0m, receiveLineWithTxnQty2.ReservedQuantity);
		}

		public void TestRemapReservedStock_TransferReceivedStockOnInventoryWithMultiplePickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			line1.WE_TransactionQuantity = 0m;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			line2.WE_TransactionQuantity = 15m;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine1, line1.Inventory[0], 10m);
			Helper.CreateReservePickLine(orderLine2, line1.Inventory[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", 20m, line1.ReservedQuantity);
			AssertEquals("Precondition", 0m, line1.WE_TransactionQuantity);
			AssertEquals("Precondition", 0m, line2.ReservedQuantity);
			AssertEquals("Precondition", 15m, line2.WE_TransactionQuantity);
			AssertEquals("Precondition", 10m, orderLine1.WE_CrossDockQuantity);
			AssertEquals("Precondition", 10m, orderLine2.WE_CrossDockQuantity);
			LinesMapper.RemapUnfulfilledReservedStock(receive);

			AssertEquals("Reserved stocks are remapped, line2 takes only reserved stocks equal to its transaction quantity.", 15m, line2.ReservedQuantity);
			AssertEquals("Reserved stocks are remapped, some reserved stocks are not remapped from line1.", 5m, line1.ReservedQuantity);
		}

		#endregion
	}
}
