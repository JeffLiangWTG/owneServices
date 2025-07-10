using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InventorySelectionLine))]
	sealed class InventorySelectionLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateSelectionLinesDetails()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsHelper = Helper.WhsHelper;
			var whsRowA = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "A");
			var whsRowB = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "B");
			Helper.Importer.Factory.Save();
			var locationAPK = whsHelper.FindLocation(whsWarehouse.PK, "A").PK;
			var locationBPK = whsHelper.FindLocation(whsWarehouse.PK, "B").PK;
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var date = ZDateTime.Today.AddMonths(-1);
			var dateOffset = ZDateTimeOffset.Today.AddMonths(-1);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-1");
			whsReceiveLine1.WE_WL = locationAPK;
			var whsInventory1 = whsReceiveLine1.Inventory;
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 200m, 1800m, 1800m, bondedEntryKey: "EN00123-2");
			whsReceiveLine2.WE_WL = locationAPK;
			var whsInventory2 = whsReceiveLine2.Inventory;
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-1");
			whsReceiveLine3.WE_WL = locationBPK;
			var whsInventory3 = whsReceiveLine3.Inventory;
			var whsReceiveLine4 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 200m, 1800m, 1800m, bondedEntryKey: "EN00123-2");
			whsReceiveLine4.WE_WL = locationBPK;
			var whsInventory4 = whsReceiveLine4.Inventory;
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsReceiveLine1.WE_AdjustmentArrivalDate = dateOffset;
			whsReceiveLine2.WE_AdjustmentArrivalDate = dateOffset.AddDays(1);
			whsReceiveLine3.WE_AdjustmentArrivalDate = dateOffset;
			whsReceiveLine4.WE_AdjustmentArrivalDate = dateOffset.AddDays(1);
			Helper.Importer.Factory.Save();
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
			var wrappers = Header.SelectionLines.Cast<InventorySelectionLine>().SelectMany(x => x.InventoryWrappers).ToArray();
			var inventoryWrapper1 = wrappers.First(x => x.Inventory.PK == whsInventory1.PK);
			var inventoryWrapper2 = wrappers.First(x => x.Inventory.PK == whsInventory2.PK);
			var inventoryWrapper3 = wrappers.First(x => x.Inventory.PK == whsInventory3.PK);
			var inventoryWrapper4 = wrappers.First(x => x.Inventory.PK == whsInventory4.PK);
			var line = Header.SelectionLines.AddNew();

			CombineAssertions(() =>
			{
				line.UpdateSelectionLinesDetails(inventoryWrapper1, inventoryWrapper2, inventoryWrapper3, inventoryWrapper4);
				AssertEquals("2US_Product", Helper.Part.OP_PartNum, line.US_Product);
				AssertEquals("2US_Description", Helper.Part.OP_Desc, line.US_Description);
				AssertEquals("2US_ArrivalDate", date, line.US_ArrivalDate);
				AssertEquals("2US_CustomsEntryKey", "EN00123-1", line.US_CustomsEntryKey);
				AssertEquals("2US_CartonQtyOnHand", ZInt.Zero, line.US_CartonQtyOnHand);
				AssertEquals("2US_OriginalBondedQty", 5400m, line.US_OriginalBondedQty);
				AssertEquals("2US_ProductQtyOnHand", 5400m, line.US_ProductQtyOnHand);
				AssertEquals("2US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("2US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("2US_ProductQtyPerCarton", 100m, line.US_ProductQtyPerCarton);
				AssertEquals("2US_OriginalPackType", "UNT", line.US_OriginalPackType);
				AssertEquals("2ErrorReporter.LastMessageReported", "Multiple Inventories when group by Inventory", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				line.UpdateSelectionLinesDetails(inventoryWrapper2, inventoryWrapper1, inventoryWrapper3, inventoryWrapper4);
				AssertEquals("3US_Product", Helper.Part.OP_PartNum, line.US_Product);
				AssertEquals("3US_Description", Helper.Part.OP_Desc, line.US_Description);
				AssertEquals("3US_ArrivalDate", date, line.US_ArrivalDate);
				AssertEquals("3US_CustomsEntryKey", "EN00123-1", line.US_CustomsEntryKey);
				AssertEquals("3US_CartonQtyOnHand", ZInt.Zero, line.US_CartonQtyOnHand);
				AssertEquals("3US_OriginalBondedQty", 5400m, line.US_OriginalBondedQty);
				AssertEquals("3US_ProductQtyOnHand", 5400m, line.US_ProductQtyOnHand);
				AssertEquals("3US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("3US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("3US_ProductQtyPerCarton", 200m, line.US_ProductQtyPerCarton);
				AssertEquals("3US_OriginalPackType", "UNT", line.US_OriginalPackType);
				AssertEquals("3ErrorReporter.LastMessageReported", "Multiple Inventories when group by Inventory", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				Header.IsGroupByProduct = true;
				line.UpdateSelectionLinesDetails(inventoryWrapper1, inventoryWrapper2, inventoryWrapper3, inventoryWrapper4);
				AssertEquals("4US_Product", Helper.Part.OP_PartNum, line.US_Product);
				AssertEquals("4US_Description", Helper.Part.OP_Desc, line.US_Description);
				AssertEquals("4US_ArrivalDate", ZDateTime.Empty, line.US_ArrivalDate);
				AssertEquals("4US_CustomsEntryKey", "", line.US_CustomsEntryKey);
				AssertEquals("4US_CartonQtyOnHand", ZInt.Zero, line.US_CartonQtyOnHand);
				AssertEquals("4US_OriginalBondedQty", 5400m, line.US_OriginalBondedQty);
				AssertEquals("4US_ProductQtyOnHand", 5400m, line.US_ProductQtyOnHand);
				AssertEquals("4US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("4US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("4US_ProductQtyPerCarton", ZDecimal.Zero, line.US_ProductQtyPerCarton);
				AssertEquals("4US_OriginalPackType", "UNT", line.US_OriginalPackType);

				line.UpdateSelectionLinesDetails(inventoryWrapper2, inventoryWrapper1, inventoryWrapper3, inventoryWrapper4);
				AssertEquals("5US_Product", Helper.Part.OP_PartNum, line.US_Product);
				AssertEquals("5US_Description", Helper.Part.OP_Desc, line.US_Description);
				AssertEquals("5US_ArrivalDate", ZDateTime.Empty, line.US_ArrivalDate);
				AssertEquals("5US_CustomsEntryKey", "", line.US_CustomsEntryKey);
				AssertEquals("5US_CartonQtyOnHand", ZInt.Zero, line.US_CartonQtyOnHand);
				AssertEquals("5US_OriginalBondedQty", 5400m, line.US_OriginalBondedQty);
				AssertEquals("5US_ProductQtyOnHand", 5400m, line.US_ProductQtyOnHand);
				AssertEquals("5US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("5US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("5US_ProductQtyPerCarton", ZDecimal.Zero, line.US_ProductQtyPerCarton);
				AssertEquals("5US_OriginalPackType", "UNT", line.US_OriginalPackType);

				Header.IsGroupByCarton = true;
				line.UpdateSelectionLinesDetails(inventoryWrapper1, inventoryWrapper2, inventoryWrapper3, inventoryWrapper4);
				AssertEquals("6US_Product", string.Format("{0} (100 Per), {0} (100 Per), {1} (200 Per), {1} (200 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum), line.US_Product);
				AssertEquals("6US_Description", string.Format("{0} (100 Per), {0} (100 Per), {1} (200 Per), {1} (200 Per)", Helper.Part.OP_Desc, Helper.Part2.OP_Desc), line.US_Description);
				AssertEquals("6US_ArrivalDate", date, line.US_ArrivalDate);
				AssertEquals("6US_CustomsEntryKey", "EN00123", line.US_CustomsEntryKey);
				AssertEquals("6US_CartonQtyOnHand", 18, line.US_CartonQtyOnHand);
				AssertEquals("6US_OriginalBondedQty", 5400m, line.US_OriginalBondedQty);
				AssertEquals("6US_ProductQtyOnHand", 5400m, line.US_ProductQtyOnHand);
				AssertEquals("6US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("6US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("6US_ProductQtyPerCarton", 300m, line.US_ProductQtyPerCarton);
				AssertEquals("6US_OriginalPackType", "UNT", line.US_OriginalPackType);

				line.UpdateSelectionLinesDetails(inventoryWrapper2, inventoryWrapper1, inventoryWrapper3, inventoryWrapper4);
				AssertEquals("7US_Product", string.Format("{0} (100 Per), {0} (100 Per), {1} (200 Per), {1} (200 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum), line.US_Product);
				AssertEquals("7US_Description", string.Format("{0} (100 Per), {0} (100 Per), {1} (200 Per), {1} (200 Per)", Helper.Part.OP_Desc, Helper.Part2.OP_Desc), line.US_Description);
				AssertEquals("7US_ArrivalDate", date, line.US_ArrivalDate);
				AssertEquals("7US_CustomsEntryKey", "EN00123", line.US_CustomsEntryKey);
				AssertEquals("7US_CartonQtyOnHand", 18, line.US_CartonQtyOnHand);
				AssertEquals("7US_OriginalBondedQty", 5400m, line.US_OriginalBondedQty);
				AssertEquals("7US_ProductQtyOnHand", 5400m, line.US_ProductQtyOnHand);
				AssertEquals("7US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("7US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("7US_ProductQtyPerCarton", 300m, line.US_ProductQtyPerCarton);
				AssertEquals("7US_OriginalPackType", "UNT", line.US_OriginalPackType);

				whsReceiveLine1.WE_PackageGroupId = ZString.Empty;
				whsReceiveLine2.WE_PackageGroupId = ZString.Empty;
				whsReceiveLine3.WE_PackageGroupId = ZString.Empty;
				whsReceiveLine4.WE_PackageGroupId = ZString.Empty;
				Helper.Importer.Factory.Save();
				Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
				wrappers = Header.SelectionLines.Cast<InventorySelectionLine>().SelectMany(x => x.InventoryWrappers).ToArray();
				inventoryWrapper1 = wrappers.First(x => x.Inventory.PK == whsInventory1.PK);
				inventoryWrapper2 = wrappers.First(x => x.Inventory.PK == whsInventory2.PK);
				inventoryWrapper3 = wrappers.First(x => x.Inventory.PK == whsInventory3.PK);
				inventoryWrapper4 = wrappers.First(x => x.Inventory.PK == whsInventory4.PK);
				line.UpdateSelectionLinesDetails(inventoryWrapper1, inventoryWrapper2);
				AssertEquals("8US_Product", string.Format("{0} (1 Per), {1} (1 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum), line.US_Product);
				AssertEquals("8US_Description", string.Format("{0} (1 Per), {1} (1 Per)", Helper.Part.OP_Desc, Helper.Part2.OP_Desc), line.US_Description);
				AssertEquals("8US_ArrivalDate", date, line.US_ArrivalDate);
				AssertEquals("8US_CustomsEntryKey", "EN00123", line.US_CustomsEntryKey);
				AssertEquals("8US_CartonQtyOnHand", 900, line.US_CartonQtyOnHand);
				AssertEquals("8US_OriginalBondedQty", 2700m, line.US_OriginalBondedQty);
				AssertEquals("8US_ProductQtyOnHand", 2700m, line.US_ProductQtyOnHand);
				AssertEquals("8US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("8US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("8US_ProductQtyPerCarton", 3m, line.US_ProductQtyPerCarton);
				AssertEquals("8US_OriginalPackType", "UNT", line.US_OriginalPackType);

				line.UpdateSelectionLinesDetails(inventoryWrapper2, inventoryWrapper1);
				AssertEquals("9US_Product", string.Format("{0} (1 Per), {1} (1 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum), line.US_Product);
				AssertEquals("9US_Description", string.Format("{0} (1 Per), {1} (1 Per)", Helper.Part.OP_Desc, Helper.Part2.OP_Desc), line.US_Description);
				AssertEquals("9US_ArrivalDate", date, line.US_ArrivalDate);
				AssertEquals("9US_CustomsEntryKey", "EN00123", line.US_CustomsEntryKey);
				AssertEquals("9US_CartonQtyOnHand", 900, line.US_CartonQtyOnHand);
				AssertEquals("9US_OriginalBondedQty", 2700m, line.US_OriginalBondedQty);
				AssertEquals("9US_ProductQtyOnHand", 2700m, line.US_ProductQtyOnHand);
				AssertEquals("9US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("9US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("9US_ProductQtyPerCarton", 3m, line.US_ProductQtyPerCarton);
				AssertEquals("9US_OriginalPackType", "UNT", line.US_OriginalPackType);
			});
		}

		public void TestClearInventoryDetail()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 900m, 900m, "UNT", "A", "1", "D", "", "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();
			whsInventory.WI_SerialNumber = "SN1";
			Helper.Importer.Factory.Save();

			CombineAssertions(() =>
			{
				Header.IsGroupByInventory = true;
				Header.UpdateSelectionLinesDetails(new[] { whsInventory });
				var line = Header.SelectionLines[0];
				line.US_CartonQtytoDraw = 8;
				line.US_ProductQtyToDraw = 800m;

				AssertEquals("Not Empty InventoryWrappers", true, line.InventoryWrappers.Any());
				AssertEquals("Not Empty US_CustomsEntryKey", false, line.US_CustomsEntryKey.IsEmpty);
				AssertEquals("Not Empty US_Product", false, line.US_Product.IsEmpty);
				AssertEquals("Not Empty US_Description", false, line.US_Description.IsEmpty);
				AssertEquals("Not Empty US_ArrivalDate", false, line.US_ArrivalDate.IsEmpty);
				AssertEquals("Not Empty US_CartonQtytoDraw", false, line.US_CartonQtytoDraw.IsEmpty);
				AssertEquals("Not Empty US_OriginalBondedQty", false, line.US_OriginalBondedQty.IsEmpty);
				AssertEquals("Not Empty US_ProductQtyToDraw", false, line.US_ProductQtyToDraw.IsEmpty);
				AssertEquals("Not Empty US_ProductQtyOnHand", false, line.US_ProductQtyOnHand.IsEmpty);
				AssertEquals("Not Empty US_ProductQtyPerCarton", false, line.US_ProductQtyPerCarton.IsEmpty);
				AssertEquals("Not Empty US_Warehouse", false, line.US_Warehouse.IsEmpty);
				AssertEquals("Not Empty US_Attribute1", false, line.US_Attribute1.IsEmpty);
				AssertEquals("Not Empty US_Attribute2", false, line.US_Attribute2.IsEmpty);
				AssertEquals("Not Empty US_Attribute3", false, line.US_Attribute3.IsEmpty);
				AssertEquals("Not Empty US_SerialNumber", false, line.US_SerialNumber.IsEmpty);
				AssertEquals("Not Empty US_GroupingID", false, line.US_GroupingID.IsEmpty);

				line.ClearInventoryDetail();
				AssertEquals("Cleared InventoryWrappers", false, line.InventoryWrappers.Any());
				AssertEquals("Cleared US_CustomsEntryKey", true, line.US_CustomsEntryKey.IsEmpty);
				AssertEquals("Cleared US_Product", true, line.US_Product.IsEmpty);
				AssertEquals("Cleared US_Description", true, line.US_Description.IsEmpty);
				AssertEquals("Cleared US_ArrivalDate", true, line.US_ArrivalDate.IsEmpty);
				AssertEquals("Cleared US_CartonQtytoDraw", true, line.US_CartonQtytoDraw.IsEmpty);
				AssertEquals("Cleared US_OriginalBondedQty", true, line.US_OriginalBondedQty.IsEmpty);
				AssertEquals("Cleared US_ProductQtyToDraw", true, line.US_ProductQtyToDraw.IsEmpty);
				AssertEquals("Cleared US_ProductQtyOnHand", true, line.US_ProductQtyOnHand.IsEmpty);
				AssertEquals("Cleared US_ProductQtyPerCarton", true, line.US_ProductQtyPerCarton.IsEmpty);
				AssertEquals("Cleared US_Warehouse", true, line.US_Warehouse.IsEmpty);
				AssertEquals("Cleared US_Attribute1", true, line.US_Attribute1.IsEmpty);
				AssertEquals("Cleared US_Attribute2", true, line.US_Attribute2.IsEmpty);
				AssertEquals("Cleared US_Attribute3", true, line.US_Attribute3.IsEmpty);
				AssertEquals("Cleared US_SerialNumber", true, line.US_SerialNumber.IsEmpty);
				AssertEquals("Cleared US_GroupingID", true, line.US_GroupingID.IsEmpty);
			});
		}

		public void TestUpdateSelectionLinesDetails_GroupByProduct()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 1m, 1m, 1m, "UNT", "A", "1", "D", "SN1", "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();
			CombineAssertions(() =>
			{
				Header.IsGroupByProduct = true;
				Header.UpdateSelectionLinesDetails(new[] { whsInventory });
				var line = Header.SelectionLines[0];
				AssertEquals("US_Attribute1", "A", line.US_Attribute1);
				AssertEquals("US_Attribute2", "1", line.US_Attribute2);
				AssertEquals("US_Attribute3", "D", line.US_Attribute3);
				AssertEquals("US_SerialNumber", "SN1", line.US_SerialNumber);
				AssertEquals("US_Product", Helper.Part.OP_PartNum, line.US_Product);
				AssertEquals("US_Description", Helper.Part.OP_Desc, line.US_Description);
				AssertEquals("US_CustomsEntryKey", "", line.US_CustomsEntryKey);
				AssertEquals("US_CartonQtyOnHand", 0, line.US_CartonQtyOnHand);
				AssertEquals("US_OriginalBondedQty", 1m, line.US_OriginalBondedQty);
				AssertEquals("US_ProductQtyOnHand", 1m, line.US_ProductQtyOnHand);
				AssertEquals("US_CartonQtytoDraw", 0, line.US_CartonQtytoDraw);
				AssertEquals("US_ProductQtyToDraw", 0m, line.US_ProductQtyToDraw);
				AssertEquals("US_ProductQtyPerCarton", 0m, line.US_ProductQtyPerCarton);
			});
		}

		public void TestUpdateSelectionLinesDetails_GroupByInventory()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 1m, 1m, 1m, "UNT", "A", "1", "D", "SN1", "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();
			CombineAssertions(() =>
			{
				Header.IsGroupByInventory = true;
				Header.UpdateSelectionLinesDetails(new[] { whsInventory });
				var line = Header.SelectionLines[0];
				AssertEquals("US_Attribute1", "A", line.US_Attribute1);
				AssertEquals("US_Attribute2", "1", line.US_Attribute2);
				AssertEquals("US_Attribute3", "D", line.US_Attribute3);
				AssertEquals("US_SerialNumber", "SN1", line.US_SerialNumber);
				AssertEquals("US_Product", Helper.Part.OP_PartNum, line.US_Product);
				AssertEquals("US_Description", Helper.Part.OP_Desc, line.US_Description);
				AssertEquals("US_CustomsEntryKey", "EN00123-1", line.US_CustomsEntryKey);
				AssertEquals("US_CartonQtyOnHand", 0, line.US_CartonQtyOnHand);
				AssertEquals("US_OriginalBondedQty", 1m, line.US_OriginalBondedQty);
				AssertEquals("US_ProductQtyOnHand", 1m, line.US_ProductQtyOnHand);
				AssertEquals("US_CartonQtytoDraw", 0, line.US_CartonQtytoDraw);
				AssertEquals("US_ProductQtyToDraw", 0m, line.US_ProductQtyToDraw);
				AssertEquals("US_ProductQtyPerCarton", 1m, line.US_ProductQtyPerCarton);
			});
		}

		public void TestUpdateSelectionLinesDetails_GroupByCarton_UpdateLine()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 1m, 1m, 1m, "UNT", "A", "1", "D", "SN1", "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();
			CombineAssertions(() =>
			{
				Header.IsGroupByCarton = true;
				Header.UpdateSelectionLinesDetails(new[] { whsInventory });
				var line = Header.SelectionLines[0];
				AssertEquals("US_Attribute1", ZString.Empty, line.US_Attribute1);
				AssertEquals("US_Attribute2", ZString.Empty, line.US_Attribute2);
				AssertEquals("US_Attribute3", ZString.Empty, line.US_Attribute3);
				AssertEquals("US_SerialNumber", ZString.Empty, line.US_SerialNumber);
				AssertEquals("US_Product", string.Format("{0} (1 Per)", Helper.Part.OP_PartNum), line.US_Product);
				AssertEquals("US_Description", string.Format("{0} (1 Per)", Helper.Part.OP_Desc), line.US_Description);
				AssertEquals("US_CustomsEntryKey", "EN00123", line.US_CustomsEntryKey);
				AssertEquals("US_CartonQtyOnHand", 1, line.US_CartonQtyOnHand);
				AssertEquals("US_OriginalBondedQty", 1m, line.US_OriginalBondedQty);
				AssertEquals("US_ProductQtyOnHand", 1m, line.US_ProductQtyOnHand);
				AssertEquals("US_CartonQtytoDraw", ZInt.Zero, line.US_CartonQtytoDraw);
				AssertEquals("US_ProductQtyToDraw", ZDecimal.Zero, line.US_ProductQtyToDraw);
				AssertEquals("US_ProductQtyPerCarton", 1m, line.US_ProductQtyPerCarton);
				AssertEquals("US_Attribute1", ZString.Empty, line.US_Attribute1);
				AssertEquals("US_Attribute2", ZString.Empty, line.US_Attribute2);
				AssertEquals("US_Attribute3", ZString.Empty, line.US_Attribute3);

				line.US_CartonQtytoDraw = 1;
				AssertEquals("US_CartonQtytoDraw Updated", 1, line.US_CartonQtytoDraw);
				AssertEquals("US_ProductQtyToDraw Updated", 1m, line.US_ProductQtyToDraw);
			});
		}

		public void TestValidateUS_CartonQtytoDraw()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();
			Header.IsGroupByCarton = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			var line = Header.SelectionLines[0];
			line.US_CartonQtytoDraw = 10;
			AssertHasError(line.US_CartonQtytoDrawInfo, InventorySelectionLine.CartonQtyToDrawIsGreaterThanOnHand);
			line.US_CartonQtytoDraw = -1;
			AssertNoError(line.US_CartonQtytoDrawInfo, InventorySelectionLine.CartonQtyToDrawIsGreaterThanOnHand);
			AssertHasError(line.US_CartonQtytoDrawInfo, "Please enter a 'Carton Qty to Draw' greater than or equal to 0.");
			line.US_CartonQtytoDraw = 1;
			AssertNoError(line.US_CartonQtytoDrawInfo, InventorySelectionLine.CartonQtyToDrawIsGreaterThanOnHand);
			AssertNoError(line.US_CartonQtytoDrawInfo, "Please enter a 'Carton Qty to Draw' greater than or equal to 0.");
		}

		public void TestValidateUS_ProductQtyToDraw()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV2", ZDateTimeOffset.Today.AddDays(-2));
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-2");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part, "PACKAGE2", 100m, 900m, 900m, bondedEntryKey: "EN00124-1");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Helper.Importer.Factory.Save();
			Header.IsGroupByCarton = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1 });
			var line = Header.SelectionLines[0];
			line.US_ProductQtyToDraw = -1;
			AssertHasError(line.US_ProductQtyToDrawInfo, "Please enter a 'Product Qty to Draw' greater than or equal to 0.");
			line.US_ProductQtyToDraw = 1000m;
			AssertNoError(line.US_ProductQtyToDrawInfo, "Please enter a 'Product Qty to Draw' greater than or equal to 0.");
			AssertHasError(line.US_ProductQtyToDrawInfo, InventorySelectionLine.ProductQtyToDrawIsGreaterThanOnHand);
			line.US_ProductQtyToDraw = 50m;
			var error = InventorySelectionLine.ProductQtyToDrawIncorrectMultiple(100m);
			AssertHasError(line.US_ProductQtyToDrawInfo, error);
			line.US_ProductQtyToDraw = 100m;
			AssertNoError(line.US_ProductQtyToDrawInfo, error);

			Header.IsGroupByProduct = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1 });
			line = Header.SelectionLines[0];
			line.US_ProductQtyToDraw = 50m;
			AssertNoError(line.US_ProductQtyToDrawInfo, error);

			Header.IsGroupByInventory = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1 });
			line = Header.SelectionLines[0];
			line.US_ProductQtyToDraw = 50m;
			AssertHasError(line.US_ProductQtyToDrawInfo, error);
			line.US_ProductQtyToDraw = 100m;
			AssertNoError(line.US_ProductQtyToDrawInfo, error);

			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory3, whsInventory2 });
			MockHeader.Protected().Setup<bool>("AllowWithdrawalOfMultipleEntryDetailsCore").Returns(false);
			AssertEquals(3, Header.SelectionLines.Count);
			var list = new List<InventorySelectionLine>(new[] { line });
			var line1 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_CustomsEntryKey == "EN00123-1");
			list.Remove(line1);
			var line2 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_CustomsEntryKey == "EN00124-1");
			list.Remove(line2);
			var line3 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_CustomsEntryKey == "EN00123-2");
			list.Remove(line3);
			AssertEquals(0, list.Count);
			line1.US_ProductQtyToDraw = 100m;
			line3.US_ProductQtyToDraw = 100m;
			AssertNoError(line3.US_ProductQtyToDrawInfo, InventorySelectionLine.CannotWithdrawProductHavingDifferentEntryNumber);
			line2.US_ProductQtyToDraw = 100m;
			AssertHasError(line2.US_ProductQtyToDrawInfo, InventorySelectionLine.CannotWithdrawProductHavingDifferentEntryNumber);
			line2.US_ProductQtyToDraw = 0m;
			AssertNoError(line2.US_ProductQtyToDrawInfo, InventorySelectionLine.CannotWithdrawProductHavingDifferentEntryNumber);

			Header.IsGroupByProduct = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory3, whsInventory2 });
			AssertEquals(1, Header.SelectionLines.Count);
			line1 = Header.SelectionLines[0];
			line1.US_ProductQtyToDraw = 100m;
			AssertNoError(line1.US_ProductQtyToDrawInfo, InventorySelectionLine.CannotWithdrawProductHavingDifferentEntryNumber);
		}

		public void TestChangingUS_ProductQtyToDrawUpdateSelectedLines()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV2", ZDateTimeOffset.Today.AddDays(-2));
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part, "PACKAGE2", 20m, 180m, 180m, bondedEntryKey: "EN00124-1");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part2, "PACKAGE2", 20m, 180m, 180m, bondedEntryKey: "EN00124-2");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			CombineAssertions(() =>
			{
				Header.IsGroupByProduct = true;
				Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
				MockHeader.Protected().Setup<bool>("AllowWithdrawalOfMultipleEntryDetailsCore").Returns(false);
				AssertEquals(2, Header.SelectionLines.Count);
				var line = Header.SelectionLines.Cast<InventorySelectionLine>().First(x => x.US_Product == Helper.Part.OP_PartNum);
				line.US_ProductQtyToDraw = 90m;
				AssertEquals(1, Header.SelectedLines.Count);
				var selectedLine = Header.SelectedLines[0];
				AssertEquals(whsInventory1.PK, selectedLine.Inventory.PK);
				AssertEquals(90m, selectedLine.QuantityToDraw);
				line.US_ProductQtyToDraw = 110m;
				AssertEquals("90 is from first and rest is the 2 lines in the packaging group", 3, Header.SelectedLines.Count);
				var selectedLine2 = Header.SelectedLines[1];
				var selectedLine3 = Header.SelectedLines[2];
				if (selectedLine3.Inventory.PK == whsInventory3.PK)
				{
					selectedLine2 = Header.SelectedLines[2];
					selectedLine3 = Header.SelectedLines[1];
				}
				AssertEquals(whsInventory1.PK, selectedLine.Inventory.PK);
				AssertEquals(90m, selectedLine.QuantityToDraw);
				AssertEquals(whsInventory3.PK, selectedLine2.Inventory.PK);
				AssertEquals(20m, selectedLine2.QuantityToDraw);
				AssertEquals(whsInventory2.PK, selectedLine3.Inventory.PK);
				AssertEquals(20m, selectedLine3.QuantityToDraw);
				AssertHasError(line.US_ProductQtyToDrawInfo, InventorySelectionLine.CannotWithdrawProductHavingDifferentEntryNumber);
				line.US_ProductQtyToDraw = 10m;
				AssertEquals(1, Header.SelectedLines.Count);
				AssertEquals(selectedLine, Header.SelectedLines[0]);
				AssertEquals(whsInventory1.PK, selectedLine.Inventory.PK);
				AssertEquals(10m, selectedLine.QuantityToDraw);
				AssertNoError(line.US_ProductQtyToDrawInfo, InventorySelectionLine.CannotWithdrawProductHavingDifferentEntryNumber);
			});
		}

		public void TestOnlyOneDrawQtyTypeIsSetAtATime()
		{
			var line = Header.SelectionLines.AddNew();
			Header.IsGroupByCarton = true;
			line.US_ProductQtyToDraw = 10m;
			AssertEquals("line.US_ProductQtyToDraw", 10m, line.US_ProductQtyToDraw);
			AssertEquals("line.US_CartonQtytoDraw", 0, line.US_CartonQtytoDraw);
			line.US_CartonQtytoDraw = 10;
			AssertEquals("line.US_ProductQtyToDraw", 0m, line.US_ProductQtyToDraw);
			AssertEquals("line.US_CartonQtytoDraw", 10, line.US_CartonQtytoDraw);
			line.US_ProductQtyToDraw = 10m;
			AssertEquals("line.US_ProductQtyToDraw", 10m, line.US_ProductQtyToDraw);
			AssertEquals("line.US_CartonQtytoDraw", 0, line.US_CartonQtytoDraw);
		}

		public void TestHasDrawQty()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();
			Header.IsGroupByCarton = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			var line = Header.SelectionLines[0];
			AssertEquals("HasDrawQty", false, line.HasDrawQty);
			line.US_ProductQtyToDraw = 1m;
			AssertEquals("HasDrawQty", true, line.HasDrawQty);
			line.US_ProductQtyToDraw = 0m;
			AssertEquals("HasDrawQty", false, line.HasDrawQty);
			line.US_CartonQtytoDraw = 1;
			AssertEquals("HasDrawQty", true, line.HasDrawQty);
		}

		protected override BusinessObject GetNewBusinessObject() => Header.SelectionLines.AddNew();

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		DeclarationInventorySelectionHeader Header => MockHeader.Object;

		Mock<DeclarationInventorySelectionHeader> MockHeader => mockHeader ?? (mockHeader = new Mock<DeclarationInventorySelectionHeader>(Factory.New<BaseJobDeclaration>()) { CallBase = true });
		Mock<DeclarationInventorySelectionHeader> mockHeader;
	}
}
