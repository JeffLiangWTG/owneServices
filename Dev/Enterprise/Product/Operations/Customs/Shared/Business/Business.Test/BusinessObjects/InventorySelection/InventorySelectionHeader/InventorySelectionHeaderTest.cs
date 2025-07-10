using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DeclarationInventorySelectionHeader))]
	class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSameQuantityReturnedByInventoryAndReceiveLine()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 1000m, 1000m);
			var whsInventory1 = whsReceiveLine1.Inventory;
			Factory.Save();

			AssertEquals("Should be same.", whsReceiveLine1.WE_TransactionQuantity, whsInventory1.WI_InDocketLineUnits);
		}

		public void TestSelectingFromAdjustment()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whsWarehouse = (IWhsWarehouse)whsHelper.CreateWarehouse("N10", "N10");
			Factory.Save();
			whsWarehouse.WW_OA_WarehouseAddress = Helper.Warehouse.MainAddress.PK;
			whsWarehouse.WW_IsVirtualWarehouse = true;
			var whsArea = Factory.LoadTop1<IWhsArea>(new ZQuery(WhsAreaSchema.WA_WW_Whs, whsWarehouse.PK));
			whsArea.WA_AreaType = "BON";
			Factory.Save();
			var whsLocation = Factory.LoadTop1<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WA_PickingArea, whsArea.PK));
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsReceive.WD_ExternalReference = "DKSN32432";
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			whsReceiveLine.WE_WL = whsLocation.PK;
			whsReceiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			var whsReceiveLineCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 15000m, 200m, "KG", "AU", ZDecimal.Zero, ZString.Empty, ZString.Empty, "EN00123", (ZShort)1);

			var whsAdjustment = (IWhsDocket)whsHelper.CreateWhsAdjustment(Helper.Importer.PK, whsWarehouse.PK, ZString.Empty, new ZArchitecture.NotificationBuffer());
			whsAdjustment.WD_DocketSubType = "CUS";
			whsAdjustment.WD_ExternalReference = "DKSN32432";
			var whsAdjustmentLine = Factory.Load<IWhsDocketLine>(whsHelper.CreateWhsAdjustmentLine(whsAdjustment.PK, Helper.Part.PK, 2000m, whsReceiveLine.WE_WL));
			whsAdjustmentLine.WE_BondedEntryKey = "EN00123-1";
			var whsAdjustmentLinCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsAdjustmentLine.PK, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, "EN00123", (ZShort)1);
			whsAdjustment.FinaliseDocketWithoutUserConfirmation();
			var whsInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, whsAdjustmentLine.PK));
			Factory.Save();
			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, Header.SelectionLines.Count);
			AssertEquals(2000m, Header.SelectionLines[0].US_ProductQtyOnHand);
		}

		public void TestUpdateSelectionLinesDetails()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);

			IWhsInventoryView whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1500m, 1500m, bondedEntryKey: "EN00122-3");
			IWhsInventoryView whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 200m, 2000m, 2000m, bondedEntryKey: "EN00122-1");
			IWhsInventoryView whsInventory3 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part2, "PACKAGE1", 250m, 2500m, 2500m, bondedEntryKey: "EN00122-2");
			IWhsInventoryView whsInventory4 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 1m, 1700m, 1700m, "BOX", bondedEntryKey: "EN00122-2");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory4.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.IsGroupByInventory = true;
			AssertEquals(0, Header.SelectionLines.Count);

			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
			AssertEquals(4, Header.SelectionLines.Count);

			var line1 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 1500m);
			AssertInventory(line1, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1500m, 0);

			var line2 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 2000m);
			AssertInventory(line2, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 2000m, 0);

			var line3 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part2.OP_PartNum && x.US_ProductQtyOnHand == 2500m);
			AssertInventory(line3, Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, 2500m, 0);

			var line4 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 1700m);
			AssertInventory(line4, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1700m, 0);

			Header.IsGroupByProduct = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
			AssertEquals("Two different products, one with two different pack types", 3, Header.SelectionLines.Count);

			var list = new List<InventorySelectionLine>(new[] { line1, line2, line3, line4 });
			var line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 3500m);
			AssertInventory(line, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 3500m, 0);
			list.Remove(line);

			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 1700m);
			AssertInventory(line, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1700m, 0);
			list.Remove(line);

			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part2.OP_PartNum && x.US_ProductQtyOnHand == 2500m);
			AssertInventory(line, Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, 2500m, 0);
			list.Remove(line);

			foreach (var lineNotUsed in list)
			{
				AssertInventory(lineNotUsed, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZInt.Zero);
			}
			Header.IsGroupByCarton = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
			AssertEquals(3, Header.SelectionLines.Count);
			list = new List<InventorySelectionLine>(new[] { line1, line2, line3, line4 });
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (1 Per)", Helper.Part.OP_PartNum) && x.US_ProductQtyOnHand == 1500m);
			AssertInventory(line, string.Format("{0} (1 Per)", Helper.Part.OP_PartNum), string.Format("{0} (1 Per)", Helper.Part.OP_Desc), 1500m, 1500);
			list.Remove(line);
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (200 Per), {1} (250 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum) && x.US_ProductQtyOnHand == 4500m);
			AssertInventory(line, string.Format("{0} (200 Per), {1} (250 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum), string.Format("{0} (200 Per), {1} (250 Per)", Helper.Part.OP_Desc, Helper.Part2.OP_Desc), 4500m, 10);
			list.Remove(line);
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (1 Per)", Helper.Part.OP_PartNum) && x.US_ProductQtyOnHand == 1700m);
			AssertInventory(line, string.Format("{0} (1 Per)", Helper.Part.OP_PartNum), string.Format("{0} (1 Per)", Helper.Part.OP_Desc), 1700m, 1700);
			list.Remove(line);
			foreach (var lineNotUsed in list)
			{
				AssertInventory(lineNotUsed, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZInt.Zero);
			}

			Header.IsGroupByInventory = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
			AssertEquals(4, Header.SelectionLines.Count);

			list = new List<InventorySelectionLine>(new[] { line1, line2, line3, line4 });
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 1500);
			AssertInventory(line, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1500, 0);

			list.Remove(line);
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 2000m);
			AssertInventory(line, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 2000, 0);

			list.Remove(line);
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part2.OP_PartNum && x.US_ProductQtyOnHand == 2500);
			AssertInventory(line, Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, 2500m, 0);

			list.Remove(line);
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 1700m);
			AssertInventory(line4, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1700m, 0);

			list.Remove(line);
			AssertEquals(0, list.Count);
		}

		public void TestUpdateSelectionLinesDetails_GroupByProduct_WithSerialNumber()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);

			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 1m, 1m, 1m, serialNumber: "SN1", bondedEntryKey: "EN00122-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 1m, 1m, 1m, serialNumber: "SN2", bondedEntryKey: "EN00122-1");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.IsGroupByProduct = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2 });

			AssertEquals("Only selected lines", 2, Header.SelectionLines.Count);
			var inventorySelectionLines = Header.SelectionLines.OfType<InventorySelectionLine>();

			var line1 = inventorySelectionLines.Single(x => x.US_SerialNumber == "SN1");
			AssertInventory(line1, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1m, 0, serialNumber: "SN1");

			var line2 = inventorySelectionLines.Single(x => x.US_SerialNumber == "SN2");
			AssertInventory(line2, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1m, 0, serialNumber: "SN2");
		}

		public void TestUpdateSelectionLinesDetails_IsInventoryValidIsTrue()
		{
			var whsHelper = Helper.WhsHelper;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsWarehouse2 = Helper.GetNewWhsWarehouse(Helper.Warehouse2.MainAddress.PK, true, "N20");

			var area = whsHelper.CreateWhsArea(whsWarehouse.PK, "REC", "REC");
			var row = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "R");
			var location = row.Locations[0] as IWhsLocation;
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsReceive.WD_DocketSubType = "REC";
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse2.PK, Helper.Importer2.PK);

			var part2 = Helper.CreateProduct(Helper.Importer2.PK, "~~2");
			part2.OP_Desc = "~~2 DESC";

			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 1m, 1m, 1m, serialNumber: "SN1", bondedEntryKey: "EN00122-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive2, part2, ZString.Empty, 1m, 1m, 1m, serialNumber: "SN2", bondedEntryKey: "EN00122-2");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			Header.IsGroupByInventory = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2 });

			var line1 = Header.SelectionLines.OfType<InventorySelectionLine>().Single(x => x.US_SerialNumber == "SN1");
			var line2 = Header.SelectionLines.OfType<InventorySelectionLine>().Single(x => x.US_SerialNumber == "SN2");
			string docketSubType1 = Factory.Load<IWhsDocket>(line1.InventoryWrappers[0].Receive.PK)?.WD_DocketSubType;
			string docketSubType2 = Factory.Load<IWhsDocket>(line2.InventoryWrappers[0].Receive.PK)?.WD_DocketSubType;

			AssertEquals("2 lines should be selected, as as GetOnlyCustomsInventories  is false", 2, Header.SelectionLines.Count);
			AssertEquals("Line 1: WD_DocketSubType is 'REC'", "REC", docketSubType1);
			AssertEquals("Line 2: WD_DocketSubType is 'CUS'", "CUS", docketSubType2);
		}

		public void TestChangingQtyOfProductsThatBelongToSamePackingGroup_SplitLines()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsHelper = Helper.WhsHelper;
			var whsRowA = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "A");
			var whsRowB = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "B");
			var whsRowC = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "C");
			Helper.Importer.Factory.Save();
			var locationAPK = whsHelper.FindLocation(whsWarehouse.PK, "A").PK;
			var locationBPK = whsHelper.FindLocation(whsWarehouse.PK, "B").PK;
			var locationCPK = whsHelper.FindLocation(whsWarehouse.PK, "C").PK;
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsReceive.WD_ArrivalDate = ZDateTimeOffset.Today;
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine1.WE_LineNo = 2;
			whsReceiveLine1.WE_SubLineNo = 1;
			whsReceiveLine1.WE_CustomAttrib1 = "1";
			whsReceiveLine1.WE_WL = locationAPK;
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 10000m, 500m, "KG", "AU", 1000m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory1 = whsReceiveLine1.Inventory;
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 200m, 2000m, 2000m, bondedEntryKey: "EN00123-1", partAttribute1: "B", partAttribute2: "3", partAttribute3: "D");
			whsReceiveLine2.WE_LineNo = 2;
			whsReceiveLine2.WE_SubLineNo = 1;
			whsReceiveLine2.WE_CustomAttrib1 = "2";
			whsReceiveLine2.WE_WL = locationAPK;
			var whsReceiveLine2CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine2.PK, 20000m, 1000m, "K0", "US", 2000m, "N0", "", "EN00123", (ZShort)1);
			var whsInventory2 = whsReceiveLine2.Inventory;
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine3.WE_LineNo = 1;
			whsReceiveLine3.WE_SubLineNo = 2;
			whsReceiveLine3.WE_CustomAttrib1 = "3";
			whsReceiveLine3.WE_WL = locationBPK;
			var whsReceiveLine3CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine3.PK, 21000m, 1100m, "K1", "U1", 2100m, "N1", "", "EN00123", (ZShort)1);
			var whsInventory3 = whsReceiveLine3.Inventory;
			var whsReceiveLine4 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 200m, 2000m, 2000m, bondedEntryKey: "EN00123-1", partAttribute1: "B", partAttribute2: "3", partAttribute3: "D");
			whsReceiveLine4.WE_LineNo = 1;
			whsReceiveLine4.WE_SubLineNo = 2;
			whsReceiveLine4.WE_CustomAttrib1 = "4";
			whsReceiveLine4.WE_WL = locationBPK;
			var whsReceiveLine4CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine4.PK, 22000m, 1200m, "K2", "U2", 2200m, "N2", "", "EN00123", (ZShort)1);
			var whsInventory4 = whsReceiveLine4.Inventory;
			var whsReceiveLine5 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 300m, 3000m, 3000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine5.WE_LineNo = 2;
			whsReceiveLine5.WE_SubLineNo = 1;
			whsReceiveLine5.WE_CustomAttrib1 = "5";
			whsReceiveLine5.WE_WL = locationAPK;
			var whsReceiveLine5CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine5.PK, 23000m, 1300m, "K3", "U3", 2300m, "N3", "", "EN00123", (ZShort)1);
			var whsInventory5 = whsReceiveLine5.Inventory;
			var whsReceiveLine6 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 300m, 3000m, 3000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine6.WE_LineNo = 1;
			whsReceiveLine6.WE_SubLineNo = 2;
			whsReceiveLine6.WE_CustomAttrib1 = "6";
			whsReceiveLine6.WE_WL = locationBPK;
			var whsReceiveLine6CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine6.PK, 24000m, 1400m, "K4", "U4", 2400m, "N4", "", "EN00123", (ZShort)1);
			var whsInventory6 = whsReceiveLine6.Inventory;
			var whsReceiveLine7 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE2", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine7.WE_LineNo = 1;
			whsReceiveLine7.WE_SubLineNo = 1;
			whsReceiveLine7.WE_CustomAttrib1 = "7";
			var whsReceiveLine7CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine7.PK, 25000m, 1500m, "K5", "U5", 2500m, "N5", "", "EN00123", (ZShort)1);
			var whsInventory7 = whsReceiveLine7.Inventory;
			var whsReceiveLine8 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine8.WE_LineNo = 1;
			whsReceiveLine8.WE_SubLineNo = 1;
			whsReceiveLine8.WE_CustomAttrib1 = "8";
			var whsReceiveLine8CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine8.PK, 26000m, 1600m, "K6", "U6", 2600m, "N6", "", "EN00123", (ZShort)1);
			var whsInventory8 = whsReceiveLine8.Inventory;
			var whsReceiveLine9 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine9.WE_LineNo = 1;
			whsReceiveLine9.WE_SubLineNo = 3;
			whsReceiveLine9.WE_CustomAttrib1 = "9";
			whsReceiveLine9.WE_WL = locationCPK;
			var whsReceiveLine9CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine9.PK, 21000m, 1100m, "K7", "U7", 2700m, "N7", "", "EN00123", (ZShort)1);
			var whsInventory9 = whsReceiveLine9.Inventory;
			var whsReceiveLine10 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 200m, 2000m, 2000m, bondedEntryKey: "EN00123-1", partAttribute1: "B", partAttribute2: "3", partAttribute3: "D");
			whsReceiveLine10.WE_LineNo = 1;
			whsReceiveLine10.WE_SubLineNo = 3;
			whsReceiveLine10.WE_CustomAttrib1 = "10";
			whsReceiveLine10.WE_WL = locationCPK;
			var whsReceiveLine10CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine10.PK, 22000m, 1200m, "K2", "U2", 2200m, "N2", "", "EN00123", (ZShort)1);
			var whsInventory10 = whsReceiveLine10.Inventory;
			var whsReceiveLine11 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 300m, 3000m, 3000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine11.WE_LineNo = 1;
			whsReceiveLine11.WE_SubLineNo = 3;
			whsReceiveLine11.WE_CustomAttrib1 = "11";
			whsReceiveLine11.WE_WL = locationCPK;
			var whsReceiveLine11CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine11.PK, 23000m, 1300m, "K3", "U3", 2300m, "N3", "", "EN00123", (ZShort)1);
			var whsInventory11 = whsReceiveLine11.Inventory;

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();

			var adjustment = Helper.WhsHelper.CreateWhsAdjustment(Helper.Importer.PK, whsWarehouse.PK, "AD1", null);
			adjustment[WhsDocketSchema.WD_DocketSubType] = "CUS";
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -100m, "A", "A", "2", "C", "EN00123-1", 100m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -200m, "A", "B", "3", "D", "EN00123-1", 200m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -200m, "B", "A", "2", "C", "EN00123-1", 100m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -400m, "B", "B", "3", "D", "EN00123-1", 200m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -300m, "A", "A", "2", "C", "EN00123-1", 300m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -600m, "B", "A", "2", "C", "EN00123-1", 300m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -200m, "C", "A", "2", "C", "EN00123-1", 100m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -400m, "C", "B", "3", "D", "EN00123-1", 200m, "PACKAGE1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -600m, "C", "A", "2", "C", "EN00123-1", 300m, "PACKAGE1");
			Helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);
			Helper.Importer.Factory.Save();

			AssertEquals(WhsDataTestHelper.InventoryAvailableStatusCode, whsInventory1.WI_InventoryStatus);
			AssertEquals("whsInventory1.WI_AvailableToPickQuantity", 900m, whsInventory1.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory2.WI_AvailableToPickQuantity", 1800m, whsInventory2.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory3.WI_AvailableToPickQuantity", 800m, whsInventory3.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory4.WI_AvailableToPickQuantity", 1600m, whsInventory4.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory5.WI_AvailableToPickQuantity", 2700m, whsInventory5.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory6.WI_AvailableToPickQuantity", 2400m, whsInventory6.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory7.WI_AvailableToPickQuantity", 1000m, whsInventory7.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory8.WI_AvailableToPickQuantity", 1000m, whsInventory8.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory9.WI_AvailableToPickQuantity", 800m, whsInventory9.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory10.WI_AvailableToPickQuantity", 1600m, whsInventory10.WI_AvailableToPickQuantity);
			AssertEquals("whsInventory11.WI_AvailableToPickQuantity", 2400m, whsInventory11.WI_AvailableToPickQuantity);

			Header.IsGroupByInventory = true;
			AssertEquals(0, Header.SelectionLines.Count);
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4, whsInventory5, whsInventory6, whsInventory7, whsInventory8, whsInventory9, whsInventory10, whsInventory11 });
			var selectionLines = Header.SelectionLines.Cast<InventorySelectionLine>().OrderBy(x => x.InventoryWrappers.Length + string.Join(",", x.InventoryWrappers.Select(y => y.ReceiveLine.WE_CustomAttrib1).OrderBy(y => y))).ToArray();
			AssertEquals(5, selectionLines.Length);
			var line1 = selectionLines[0];
			AssertInventory(line1, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1000m, 0, "PACKAGE2", "A", "2", "C");
			var line2 = selectionLines[1];
			AssertInventory(line2, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1000m, 0, "", "A", "2", "C");
			var line3 = selectionLines[2];
			AssertInventory(line3, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 2500m, 0, "PACKAGE1", "A", "2", "C");
			var line4 = selectionLines[3];
			AssertInventory(line4, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 5000m, 0, "PACKAGE1", "B", "3", "D");
			var line5 = selectionLines[4];
			AssertInventory(line5, Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, 7500m, 0, "PACKAGE1", "A", "2", "C");

			line2.US_ProductQtyToDraw = 400m;
			AssertEquals(0m, line1.US_ProductQtyToDraw);
			AssertEquals(400m, line2.US_ProductQtyToDraw);
			AssertEquals(0m, line3.US_ProductQtyToDraw);
			AssertEquals(0m, line4.US_ProductQtyToDraw);
			AssertEquals(0m, line5.US_ProductQtyToDraw);

			line1.US_ProductQtyToDraw = 100m;
			AssertEquals(100m, line1.US_ProductQtyToDraw);
			AssertEquals(400m, line2.US_ProductQtyToDraw);
			AssertEquals(0m, line3.US_ProductQtyToDraw);
			AssertEquals(0m, line4.US_ProductQtyToDraw);
			AssertEquals(0m, line5.US_ProductQtyToDraw);

			line3.US_ProductQtyToDraw = 300m;
			AssertEquals(100m, line1.US_ProductQtyToDraw);
			AssertEquals(400m, line2.US_ProductQtyToDraw);
			AssertEquals(300m, line3.US_ProductQtyToDraw);
			AssertEquals(600m, line4.US_ProductQtyToDraw);
			AssertEquals(900m, line5.US_ProductQtyToDraw);

			AssertEquals("Header.SelectedLines.Count", 5, Header.SelectedLines.Count);
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			Header.ImportInventories();
			var invoiceLines = Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_InvoiceQuantity).ToArray();
			AssertEquals("invoiceLines.Length", 5, invoiceLines.Length);
			AssertInvoiceLine(invoiceLines[0], Helper.Part, "A", "2", "C", "", 2500m, 100m, "UNT", 0m, "", "U5");
			AssertInvoiceLine(invoiceLines[1], Helper.Part, "A", "2", "C", "", 6300m, 300m, "UNT", 0m, "", "U1");
			AssertInvoiceLine(invoiceLines[2], Helper.Part, "A", "2", "C", "", 10400m, 400m, "UNT", 0m, "", "U6");
			AssertInvoiceLine(invoiceLines[3], Helper.Part, "B", "3", "D", "", 6600m, 600m, "UNT", 0m, "", "U2");
			AssertInvoiceLine(invoiceLines[4], Helper.Part2, "A", "2", "C", "", 7200m, 900m, "UNT", 0m, "", "U4");

			Header.IsGroupByProduct = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4, whsInventory5, whsInventory6, whsInventory7, whsInventory8, whsInventory9, whsInventory10, whsInventory11 });
			selectionLines = Header.SelectionLines.Cast<InventorySelectionLine>().OrderBy(x => x.InventoryWrappers.Length + string.Join(",", x.InventoryWrappers.Select(y => y.ReceiveLine.WE_CustomAttrib1).OrderBy(y => y))).ToArray();
			AssertEquals(3, selectionLines.Length);
			var list = new List<InventorySelectionLine>(new[] { line1, line2, line3, line4, line5 });
			line1 = selectionLines[0];
			list.Remove(line1);
			line2 = selectionLines[1];
			list.Remove(line2);
			line3 = selectionLines[2];
			list.Remove(line3);
			AssertInventory(line1, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 5000m, 0, "", "B", "3", "D");
			AssertInventory(line2, Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, 7500m, 0, "", "A", "2", "C");
			AssertInventory(line3, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 4500m, 0, "", "A", "2", "C");
			foreach (var lineNotUsed in list)
			{
				AssertInventory(lineNotUsed, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZInt.Zero, ZString.Empty);
			}
			line1.US_ProductQtyToDraw = 300;
			AssertEquals(300m, line1.US_ProductQtyToDraw);
			AssertEquals(0m, line2.US_ProductQtyToDraw);
			AssertEquals(0m, line3.US_ProductQtyToDraw);

			AssertEquals("Header.SelectedLines.Count", 3, Header.SelectedLines.Count);
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			Header.ImportInventories();
			invoiceLines = Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_InvoiceQuantity + x.JI_PartAttrib1).ToArray();
			AssertEquals("invoiceLines.Length", 3, invoiceLines.Length);
			AssertInvoiceLine(invoiceLines[0], Helper.Part, "A", "2", "C", "", 4200m, 200m, "UNT", 0m, "", "U1");
			AssertInvoiceLine(invoiceLines[1], Helper.Part, "B", "3", "D", "", 4400m, 400m, "UNT", 0m, "", "U2");
			AssertInvoiceLine(invoiceLines[2], Helper.Part2, "A", "2", "C", "", 4800m, 600m, "UNT", 0m, "", "U4");

			line2.US_ProductQtyToDraw = 200;
			AssertEquals(300m, line1.US_ProductQtyToDraw);
			AssertEquals(200m, line2.US_ProductQtyToDraw);
			AssertEquals(0m, line3.US_ProductQtyToDraw);

			AssertEquals("Header.SelectedLines.Count", 3, Header.SelectedLines.Count);
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			Header.ImportInventories();
			invoiceLines = Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_InvoiceQuantity + x.JI_PartAttrib1).ToArray();
			AssertEquals("invoiceLines.Length", 3, invoiceLines.Length);
			AssertInvoiceLine(invoiceLines[0], Helper.Part, "A", "2", "C", "", 2100m, 100m, "UNT", 0m, "", "U1");
			AssertInvoiceLine(invoiceLines[1], Helper.Part, "B", "3", "D", "", 2200m, 200m, "UNT", 0m, "", "U2");
			AssertInvoiceLine(invoiceLines[2], Helper.Part2, "A", "2", "C", "", 2400m, 300m, "UNT", 0m, "", "U4");
		}

		public void TestChangingQtyOfProductsThatBelongToSamePackingGroup()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 200m, 1800m, 1800m, bondedEntryKey: "EN00123-2");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part2, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-3");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			AssertEquals(WhsDataTestHelper.InventoryAvailableStatusCode, whsInventory1.WI_InventoryStatus);
			AssertEquals("PickableUnits", 900m, whsInventory1.WI_AvailableToPickQuantity);
			AssertEquals("PickableUnits", 1800m, whsInventory2.WI_AvailableToPickQuantity);
			AssertEquals("PickableUnits", 900m, whsInventory3.WI_AvailableToPickQuantity);

			Header.IsGroupByInventory = true;
			AssertEquals(0, Header.SelectionLines.Count);
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			AssertEquals(3, Header.SelectionLines.Count);
			var line1 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line1, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 900m, 0);
			var line2 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 1800m);
			AssertInventory(line2, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 1800m, 0);
			var line3 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part2.OP_PartNum && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line3, Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, 900m, 0);

			line2.US_ProductQtyToDraw = 400m;
			AssertEquals(0m, line1.US_ProductQtyToDraw);
			AssertEquals(400m, line2.US_ProductQtyToDraw);
			AssertEquals(200m, line3.US_ProductQtyToDraw);

			line1.US_ProductQtyToDraw = 100m;
			AssertEquals(100m, line1.US_ProductQtyToDraw);
			AssertEquals(400m, line2.US_ProductQtyToDraw);
			AssertEquals(200m, line3.US_ProductQtyToDraw);

			line3.US_ProductQtyToDraw = 300m;
			AssertEquals(100m, line1.US_ProductQtyToDraw);
			AssertEquals(600m, line2.US_ProductQtyToDraw);
			AssertEquals(300m, line3.US_ProductQtyToDraw);

			Header.IsGroupByProduct = true;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			AssertEquals(2, Header.SelectionLines.Count);
			var list = new List<InventorySelectionLine>(new[] { line1, line2, line3 });
			var line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part.OP_PartNum && x.US_ProductQtyOnHand == 2700m);
			list.Remove(line);
			AssertInventory(line, Helper.Part.OP_PartNum, Helper.Part.OP_Desc, 2700m, 0);
			line = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == Helper.Part2.OP_PartNum && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line, Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, 900m, 0);
			list.Remove(line);
			foreach (var lineNotUsed in list)
			{
				AssertInventory(lineNotUsed, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZInt.Zero);
			}
			line1.US_ProductQtyToDraw = 300;
			AssertEquals(300m, line1.US_ProductQtyToDraw);
			AssertEquals(0m, line2.US_ProductQtyToDraw);
			AssertEquals(0m, line3.US_ProductQtyToDraw);

			line2.US_ProductQtyToDraw = 200;
			AssertEquals(300m, line1.US_ProductQtyToDraw);
			AssertEquals(200m, line2.US_ProductQtyToDraw);
			AssertEquals(0m, line3.US_ProductQtyToDraw);
		}

		public void TestSelectedLines()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "S1";
			supplier1.MainAddress.OA_Address1 = "S1 ADDRESS 1";
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "S2";
			supplier2.MainAddress.OA_Address1 = "S2 ADDRESS 1";
			var supplier3 = Factory.New<OrgHeader>();
			supplier3.OH_Code = "S3";
			supplier3.MainAddress.OA_Address1 = "S3 ADDRESS 1";

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");

			var arrivalDate = ZDateTime.Today.AddDays(-4);
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", arrivalDate.ToOffset());

			var supplierDoc1 = Factory.NewWithValidTestData<JobDocAddress>();
			supplierDoc1.E2_ParentID = whsReceive.PK;
			supplierDoc1.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDoc1.E2_OA_Address = supplier3.MainAddress.PK;

			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 200m, 1800m, 1800m, bondedEntryKey: "EN00123-2");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part2, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-3");

			var supplierDoc2 = Factory.NewWithValidTestData<JobDocAddress>();
			supplierDoc2.E2_ParentID = whsInventory3.WI_WE_InDocketLine;
			supplierDoc2.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDoc2.E2_OA_Address = supplier2.MainAddress.PK;

			var part3 = Helper.CreateProduct(Helper.Importer.PK, "~~~3");
			part3.RelatedOrganisations.AddSupplier(supplier1);
			var whsInventory4 = Helper.GetNewReceiveInventory(whsReceive, part3, "PACKAGE3", 150m, 600m, 600m, bondedEntryKey: "EN00123-4");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory4.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.IsGroupByCarton = true;
			AssertEquals(0, Header.SelectionLines.Count);
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
			AssertEquals(3, Header.SelectionLines.Count);
			var line1 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (1 Per)", Helper.Part.OP_PartNum) && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line1, string.Format("{0} (1 Per)", Helper.Part.OP_PartNum), string.Format("{0} (1 Per)", Helper.Part.OP_Desc), 900m, 900);
			var line2 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (200 Per), {1} (100 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum) && x.US_ProductQtyOnHand == 2700m);
			AssertInventory(line2, string.Format("{0} (200 Per), {1} (100 Per)", Helper.Part.OP_PartNum, Helper.Part2.OP_PartNum), string.Format("{0} (200 Per), {1} (100 Per)", Helper.Part.OP_Desc, Helper.Part2.OP_Desc), 2700m, 9);
			var line3 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (150 Per)", part3.OP_PartNum) && x.US_ProductQtyOnHand == 600m);
			AssertInventory(line3, string.Format("{0} (150 Per)", part3.OP_PartNum), string.Format("{0} (150 Per)", part3.OP_Desc), 600m, 4);
			AssertEquals("Header.SelectedLines.Count", 0, Header.SelectedLines.Count);

			var warehouseName = whsWarehouse.WW_WarehouseName;
			line1.US_ProductQtyToDraw = 500m;
			AssertEquals("Header.SelectedLines.Count", 1, Header.SelectedLines.Count);
			AssertInventoryWrapper(Header.SelectedLines[0], Helper.Part.OP_PartNum, Helper.Part.OP_Desc, "EN00123-1", 500m, 900m, supplier1.OH_FullName, warehouseName, arrivalDate, 900m);

			line2.US_CartonQtytoDraw = 2;
			AssertEquals("Header.SelectedLines.Count", 3, Header.SelectedLines.Count);
			AssertInventoryWrapper(Header.SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault(x => x.CustomsEntryKey == "EN00123-1"), Helper.Part.OP_PartNum, Helper.Part.OP_Desc, "EN00123-1", 500m, 900m, supplier3.OH_FullName, warehouseName, arrivalDate, 900m);
			AssertInventoryWrapper(Header.SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault(x => x.CustomsEntryKey == "EN00123-2"), Helper.Part.OP_PartNum, Helper.Part.OP_Desc, "EN00123-2", 400m, 1800m, supplier3.OH_FullName, warehouseName, arrivalDate, 1800m);
			AssertInventoryWrapper(Header.SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault(x => x.CustomsEntryKey == "EN00123-3"), Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, "EN00123-3", 200m, 900m, supplier2.OH_FullName, warehouseName, arrivalDate, 900m);

			line3.US_ProductQtyToDraw = 300;
			AssertEquals("Header.SelectedLines.Count", 4, Header.SelectedLines.Count);
			AssertInventoryWrapper(Header.SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault(x => x.CustomsEntryKey == "EN00123-1"), Helper.Part.OP_PartNum, Helper.Part.OP_Desc, "EN00123-1", 500m, 900m, supplier3.OH_FullName, warehouseName, arrivalDate, 900m);
			AssertInventoryWrapper(Header.SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault(x => x.CustomsEntryKey == "EN00123-2"), Helper.Part.OP_PartNum, Helper.Part.OP_Desc, "EN00123-2", 400m, 1800m, supplier3.OH_FullName, warehouseName, arrivalDate, 1800m);
			AssertInventoryWrapper(Header.SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault(x => x.CustomsEntryKey == "EN00123-3"), Helper.Part2.OP_PartNum, Helper.Part2.OP_Desc, "EN00123-3", 200m, 900m, supplier2.OH_FullName, warehouseName, arrivalDate, 900m);
			AssertInventoryWrapper(Header.SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault(x => x.CustomsEntryKey == "EN00123-4"), part3.OP_PartNum, part3.OP_Desc, "EN00123-4", 300m, 600m, supplier1.OH_FullName, warehouseName, arrivalDate, 600m);
		}

		void AssertInventoryWrapper(WhsInventoryWrapper inventoryWrapper, ZString partNo, ZString partDesc, ZString customsEntryKey, ZDecimal quantityToDraw, ZDecimal quantityOnHand, ZString supplierName, ZString warehouseName, ZDateTime arrivalDate, ZDecimal originalBondedQty)
		{
			AssertEquals("inventoryWrapper.Product", partNo, inventoryWrapper.Product);
			AssertEquals("inventoryWrapper.ProductDescription", partDesc, inventoryWrapper.ProductDescription);
			AssertEquals("inventoryWrapper.CustomsEntryKey", customsEntryKey, inventoryWrapper.CustomsEntryKey);
			AssertEquals("inventoryWrapper.ArrivalDate", arrivalDate, inventoryWrapper.ArrivalDate);
			AssertEquals("inventoryWrapper.SupplierName", supplierName, inventoryWrapper.SupplierName);
			AssertEquals("inventoryWrapper.QuantityOnHand", quantityOnHand, inventoryWrapper.QuantityOnHand);
			AssertEquals("inventoryWrapper.QuantityToDraw", quantityToDraw, inventoryWrapper.QuantityToDraw);
			AssertEquals("inventoryWrapper.WarehouseName", warehouseName, inventoryWrapper.WarehouseName);
			AssertEquals("inventoryWrapper.OriginalBondedQty", originalBondedQty, inventoryWrapper.OriginalBondedQty);
		}

		public void TestImportInventories()
		{
			var branch = Declaration.Branch;
			branch.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			Declaration.Invoices.DeleteAll();
			Factory.Save();

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1000m, 1000m, "NO", "PATT1", "PATT2", "PATT3", "", "EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", "NZ", ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, Header.SelectionLines.Count);
			var line = Header.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			AssertNull("WarehouseAddress", Declaration.WarehouseAddress);
			AssertEquals(0, Declaration.Invoices.Count);
			AssertEquals(0, Declaration.InvoiceLines.Count);
			Header.ImportInventories();
			AssertEquals("WarehouseAddress", Helper.Warehouse.MainAddress, Declaration.WarehouseAddress);
			AssertEquals(1, Declaration.Invoices.Count);
			var invoice = Declaration.Invoices[0];
			AssertEquals("JZ_InvoiceAmount", 12000m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.NewZealand, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(1, Declaration.InvoiceLines.Count);
			var invoiceLine = Declaration.InvoiceLines[0];
			AssertInvoiceLine(Declaration.InvoiceLines[0], Helper.Part, "PATT1", "PATT2", "PATT3", "", 12000m, 800m, "NO", 0m, "", "NZ");
		}

		public void TestImportInventories_WithSerialNumber()
		{
			var branch = Declaration.Branch;
			branch.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			Declaration.Invoices.DeleteAll();
			Factory.Save();

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 1m, 1m, 1m, "NO", "PATT1", "PATT2", "PATT3", "SN1", "EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 1m, "KG", "NZ", ZDecimal.Zero, "", "", "EN00123", 1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			CombineAssertions(() =>
			{
				AssertEquals(1, Header.SelectionLines.Count);
				var line = Header.SelectionLines[0];
				line.US_ProductQtyToDraw = 1m;
				AssertNull("Before Import WarehouseAddress", Declaration.WarehouseAddress);
				AssertEquals("Before Import no invoices", 0, Declaration.Invoices.Count);
				AssertEquals("Before Import no invoice lines", 0, Declaration.InvoiceLines.Count);
				Header.ImportInventories();
				AssertEquals("WarehouseAddress", Helper.Warehouse.MainAddress, Declaration.WarehouseAddress);
				AssertEquals("Invoices", 1, Declaration.Invoices.Count);
				var invoice = Declaration.Invoices[0];
				AssertEquals("JZ_InvoiceAmount", 15000m, invoice.JZ_InvoiceAmount);
				AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.NewZealand, invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals("Invoice Line", 1, Declaration.InvoiceLines.Count);
				AssertInvoiceLine(Declaration.InvoiceLines[0], Helper.Part, "PATT1", "PATT2", "PATT3", "SN1", 15000m, 1m, "NO", 0m, "", "NZ");
			});
		}

		public void TestGroupBy()
		{
			AssertEquals("IsGroupByInventory", true, Header.IsGroupByInventory);
			AssertEquals("IsGroupByProduct", false, Header.IsGroupByProduct);
			AssertEquals("IsGroupByCarton", false, Header.IsGroupByCarton);

			Header.IsGroupByProduct = true;
			AssertEquals("IsGroupByInventory", false, Header.IsGroupByInventory);
			AssertEquals("IsGroupByProduct", true, Header.IsGroupByProduct);
			AssertEquals("IsGroupByCarton", false, Header.IsGroupByCarton);

			Header.IsGroupByProduct = false;
			AssertEquals("IsGroupByInventory", false, Header.IsGroupByInventory);
			AssertEquals("IsGroupByProduct", false, Header.IsGroupByProduct);
			AssertEquals("IsGroupByCarton", false, Header.IsGroupByCarton);

			Header.IsGroupByCarton = true;
			AssertEquals("IsGroupByInventory", false, Header.IsGroupByInventory);
			AssertEquals("IsGroupByProduct", false, Header.IsGroupByProduct);
			AssertEquals("IsGroupByCarton", true, Header.IsGroupByCarton);

			Header.IsGroupByCarton = false;
			AssertEquals("IsGroupByInventory", false, Header.IsGroupByInventory);
			AssertEquals("IsGroupByProduct", false, Header.IsGroupByProduct);
			AssertEquals("IsGroupByCarton", false, Header.IsGroupByCarton);

			Header.IsGroupByInventory = true;
			AssertEquals("IsGroupByInventory", true, Header.IsGroupByInventory);
			AssertEquals("IsGroupByProduct", false, Header.IsGroupByProduct);
			AssertEquals("IsGroupByCarton", false, Header.IsGroupByCarton);

			Header.IsGroupByInventory = false;
			AssertEquals("IsGroupByInventory", false, Header.IsGroupByInventory);
			AssertEquals("IsGroupByProduct", false, Header.IsGroupByProduct);
			AssertEquals("IsGroupByCarton", false, Header.IsGroupByCarton);
		}

		public void TestGetFilterDefaults()
		{
			var filters = Header.GetFilterDefaults();
			AssertEquals(1, filters.OfType<FilterBusinessObjectDefault>().Count());
			var filter = filters["Client:Property"];
			AssertEquals(Helper.Importer.PK, filter.Value);

			Declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			filters = Header.GetFilterDefaults();
			AssertEquals(1, filters.OfType<FilterBusinessObjectDefault>().Count());
			filter = filters["Client:Property"];
			AssertEquals(Helper.Importer.PK, filter.Value);

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, ZBool.True, "N10");
			filters = Header.GetFilterDefaults();
			AssertEquals(2, filters.OfType<FilterBusinessObjectDefault>().Count());
			filter = filters["Client:Property"];
			AssertEquals(Helper.Importer.PK, filter.Value);
			filter = filters["Warehouse:Property"];
			AssertEquals(whsWarehouse.PK, filter.Value);

			Declaration.JE_OH_Importer = ZGuid.Empty;
			filters = Header.GetFilterDefaults();
			AssertEquals(1, filters.OfType<FilterBusinessObjectDefault>().Count());
			filter = filters["Warehouse:Property"];
			AssertEquals(whsWarehouse.PK, filter.Value);

			Declaration.JE_OH_Importer = Helper.Importer.PK;
			Declaration.JE_OH_Supplier = Helper.Supplier.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			filters = Header.GetFilterDefaults();
			AssertEquals(2, filters.OfType<FilterBusinessObjectDefault>().Count());
			filter = filters["Client:Property"];
			AssertEquals(Helper.Supplier.PK, filter.Value);
			filter = filters["Warehouse:Property"];
			AssertEquals(whsWarehouse.PK, filter.Value);
		}

		public void TestClearDrawQuantitiesAndFillOutDrawQuantities()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 200m, 1800m, 1800m, bondedEntryKey: "EN00123-2");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part2, "PACKAGE2", 100m, 900m, 900m, bondedEntryKey: "EN00123-3");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.IsGroupByCarton = true;
			AssertEquals(0, Header.SelectionLines.Count);
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			AssertEquals(3, Header.SelectionLines.Count);

			var line1 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (1 Per)", Helper.Part.OP_PartNum) && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line1, string.Format("{0} (1 Per)", Helper.Part.OP_PartNum), string.Format("{0} (1 Per)", Helper.Part.OP_Desc), 900m, 900);
			var line2 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (200 Per)", Helper.Part.OP_PartNum) && x.US_ProductQtyOnHand == 1800m);
			AssertInventory(line2, string.Format("{0} (200 Per)", Helper.Part.OP_PartNum), string.Format("{0} (200 Per)", Helper.Part.OP_Desc), 1800m, 9);
			var line3 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (100 Per)", Helper.Part2.OP_PartNum) && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line3, string.Format("{0} (100 Per)", Helper.Part2.OP_PartNum), string.Format("{0} (100 Per)", Helper.Part2.OP_Desc), 900m, 9);

			line1.US_ProductQtyToDraw = 800m;
			line1.US_CartonQtytoDraw = 800;
			line2.US_ProductQtyToDraw = 801m;
			line2.US_CartonQtytoDraw = 4;
			line3.US_ProductQtyToDraw = 802m;
			line3.US_CartonQtytoDraw = 8;
			Header.ClearDrawQuantities();
			AssertEquals("line1.US_ProductQtyToDraw", 0m, line1.US_ProductQtyToDraw);
			AssertEquals("line1.US_CartonQtytoDraw", 0, line1.US_CartonQtytoDraw);
			AssertEquals("line2.US_ProductQtyToDraw", 0m, line2.US_ProductQtyToDraw);
			AssertEquals("line2.US_CartonQtytoDraw", 0, line2.US_CartonQtytoDraw);
			AssertEquals("line3.US_ProductQtyToDraw", 0m, line3.US_ProductQtyToDraw);
			AssertEquals("line3.US_CartonQtytoDraw", 0, line3.US_CartonQtytoDraw);

			Header.FillOutDrawQuantities();
			AssertEquals("line1.US_ProductQtyToDraw", 900m, line1.US_ProductQtyToDraw);
			AssertEquals("line1.US_CartonQtytoDraw", 900, line1.US_CartonQtytoDraw);
			AssertEquals("line2.US_ProductQtyToDraw", 1800m, line2.US_ProductQtyToDraw);
			AssertEquals("line2.US_CartonQtytoDraw", 9, line2.US_CartonQtytoDraw);
			AssertEquals("line3.US_ProductQtyToDraw", 900m, line3.US_ProductQtyToDraw);
			AssertEquals("line3.US_CartonQtytoDraw", 9, line3.US_CartonQtytoDraw);
		}

		public void TestClearDrawQuantitiesAndFillOutDrawQuantitiesExcludingHeld()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 200m, 1800m, 1800m, bondedEntryKey: "EN00123-2");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part2, "PACKAGE2", 100m, 900m, 900m, bondedEntryKey: "EN00123-3");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryHeldStatusCode;
			Factory.Load<IWhsReceiveLine>(whsInventory2.WI_WE_InDocketLine).WE_WHC_NKCurrentInventoryHeldCode = "HELLO";
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			AssertEquals(WhsDataTestHelper.InventoryAvailableStatusCode, whsInventory1.WI_InventoryStatus);
			AssertEquals("PickableUnits", 900m, whsInventory1.WI_AvailableToPickQuantity);
			AssertEquals("PickableUnits", 0m, whsInventory2.WI_AvailableToPickQuantity);
			AssertEquals("PickableUnits", 900m, whsInventory3.WI_AvailableToPickQuantity);

			Header.IsGroupByCarton = true;
			AssertEquals(0, Header.SelectionLines.Count);
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			AssertEquals(2, Header.SelectionLines.Count);

			var line1 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (1 Per)", Helper.Part.OP_PartNum) && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line1, string.Format("{0} (1 Per)", Helper.Part.OP_PartNum), string.Format("{0} (1 Per)", Helper.Part.OP_Desc), 900m, 900);
			var line2 = Header.SelectionLines.OfType<InventorySelectionLine>().FirstOrDefault(x => x.US_Product == string.Format("{0} (100 Per)", Helper.Part2.OP_PartNum) && x.US_ProductQtyOnHand == 900m);
			AssertInventory(line2, string.Format("{0} (100 Per)", Helper.Part2.OP_PartNum), string.Format("{0} (100 Per)", Helper.Part2.OP_Desc), 900m, 9);

			line1.US_ProductQtyToDraw = 800m;
			line1.US_CartonQtytoDraw = 800;
			line2.US_ProductQtyToDraw = 802m;
			line2.US_CartonQtytoDraw = 8;
			Header.ClearDrawQuantities();
			AssertEquals("line1.US_ProductQtyToDraw", 0m, line1.US_ProductQtyToDraw);
			AssertEquals("line1.US_CartonQtytoDraw", 0, line1.US_CartonQtytoDraw);
			AssertEquals("line2.US_ProductQtyToDraw", 0m, line2.US_ProductQtyToDraw);
			AssertEquals("line2.US_CartonQtytoDraw", 0, line2.US_CartonQtytoDraw);

			Header.FillOutDrawQuantities();
			AssertEquals("line1.US_ProductQtyToDraw", 900m, line1.US_ProductQtyToDraw);
			AssertEquals("line1.US_CartonQtytoDraw", 900, line1.US_CartonQtytoDraw);
			AssertEquals("line2.US_ProductQtyToDraw", 900m, line2.US_ProductQtyToDraw);
			AssertEquals("line2.US_CartonQtytoDraw", 9, line2.US_CartonQtytoDraw);
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeclarationInventorySelectionHeader(Factory.New<BaseJobDeclaration>());
		}

		DeclarationInventorySelectionHeader Header => header ?? (header = new DeclarationInventorySelectionHeader(Declaration));
		DeclarationInventorySelectionHeader header;

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_OH_Importer = Helper.Importer.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;

		void AssertInventory(InventorySelectionLine line, ZString product, ZString productDesc, ZDecimal productQty, ZInt cartonQty, ZString? groupingID = null, ZString? attribute1 = null, ZString? attribute2 = null, ZString? attribute3 = null, ZString? serialNumber = null)
		{
			AssertEquals("US_Product", product, line.US_Product);
			AssertEquals("US_Description", productDesc, line.US_Description);
			AssertEquals("US_ProductQtyOnHand", productQty, line.US_ProductQtyOnHand);
			AssertEquals("US_CartonQtyOnHand", cartonQty, line.US_CartonQtyOnHand);
			if (groupingID.HasValue)
			{
				AssertEquals("US_GroupingID", groupingID.Value, line.US_GroupingID);
			}
			if (attribute1.HasValue)
			{
				AssertEquals("US_Attribute1", attribute1.Value, line.US_Attribute1);
			}
			if (attribute2.HasValue)
			{
				AssertEquals("US_Attribute2", attribute2.Value, line.US_Attribute2);
			}
			if (attribute3.HasValue)
			{
				AssertEquals("US_Attribute3", attribute3.Value, line.US_Attribute3);
			}
			if (serialNumber.HasValue)
			{
				AssertEquals("US_SerialNumber", serialNumber.Value, line.US_SerialNumber);
			}
		}

		void AssertInvoiceLine(BaseJobComInvoiceLine invoiceLine, OrgSupplierPart part, ZString partAttribute1, ZString partAttribute2, ZString partAttribute3, ZString serialNumber, ZDecimal linePrice, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal customsQuantity, ZString customsUnitQty, ZString countryOfOrigin)
		{
			AssertEquals("JI_PartNo", part.OP_PartNum, invoiceLine.JI_PartNo);
			AssertEquals("JI_OP", part.PK, invoiceLine.JI_OP);
			AssertEquals("JI_PartAttrib1", partAttribute1, invoiceLine.JI_PartAttrib1);
			AssertEquals("JI_PartAttrib2", partAttribute2, invoiceLine.JI_PartAttrib2);
			AssertEquals("JI_PartAttrib3", partAttribute3, invoiceLine.JI_PartAttrib3);
			AssertEquals("JI_SerialNumber", serialNumber, invoiceLine.JI_SerialNumber);
			AssertEquals("JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
			AssertEquals("JI_CustomsQuantity", customsQuantity, invoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", customsUnitQty, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CountryOfOrigin", countryOfOrigin, invoiceLine.JI_CountryOfOrigin);
		}
	}
}
