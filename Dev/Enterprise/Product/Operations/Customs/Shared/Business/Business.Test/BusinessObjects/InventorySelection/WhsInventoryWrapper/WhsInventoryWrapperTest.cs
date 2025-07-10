using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(WhsInventoryWrapper))]
	sealed class WhsInventoryWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAllocationKey()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;

			var wrapper = new WhsInventoryWrapper(whsInventory, Header);
			wrapper.Inventory.WI_AllocationKey = "TestKey";
			AssertEquals("wrapper key", "TestKey", wrapper.Inventory.WI_AllocationKey);
			AssertEquals("whsReceiveLine key", "TestKey", whsReceiveLine.Inventory.WI_AllocationKey);
		}

		public void TestProductGroupKey()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;

			var wrapper = new WhsInventoryWrapper(whsInventory, Header);
			var partKey = Helper.Part.PK.ToStringKey();
			var productGroupKeyWithNoAttribs = "_____EN00123-1_100";
			AssertEquals("ProductGroupKey", partKey + productGroupKeyWithNoAttribs, wrapper.ProductGroupKey);

			whsReceiveLine.WE_PartAttrib1 = "A";
			whsReceiveLine.WE_PartAttrib2 = "2";
			whsReceiveLine.WE_PartAttrib3 = "C";
			whsReceiveLine.WE_SerialNumber = "SN";
			var productGroupKeyWithAttribs = "_A_2_C_SN_EN00123-1_100";
			AssertEquals("ProductGroupKey", partKey + productGroupKeyWithNoAttribs, wrapper.ProductGroupKey);
			wrapper = new WhsInventoryWrapper(whsInventory, Header);
			AssertEquals("ProductGroupKey", partKey + productGroupKeyWithAttribs, wrapper.ProductGroupKey);
		}

		public void TestCalculatedProperties()
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
			whsReceiveLine1.WE_CustomAttrib1 = "1";
			whsReceiveLine1.WE_WL = locationAPK;
			var whsInventory1 = whsReceiveLine1.Inventory;
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 200m, 2000m, 2000m, bondedEntryKey: "EN00123-1", partAttribute1: "B", partAttribute2: "3", partAttribute3: "D");
			whsReceiveLine2.WE_CustomAttrib1 = "2";
			whsReceiveLine2.WE_WL = locationAPK;
			var whsInventory2 = whsReceiveLine2.Inventory;
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine3.WE_CustomAttrib1 = "3";
			whsReceiveLine3.WE_WL = locationBPK;
			var whsInventory3 = whsReceiveLine3.Inventory;
			var whsReceiveLine4 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 200m, 2000m, 2000m, bondedEntryKey: "EN00123-1", partAttribute1: "B", partAttribute2: "3", partAttribute3: "D");
			whsReceiveLine4.WE_CustomAttrib1 = "4";
			whsReceiveLine4.WE_WL = locationBPK;
			var whsInventory4 = whsReceiveLine4.Inventory;
			var whsReceiveLine5 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 300m, 3000m, 3000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine5.WE_CustomAttrib1 = "5";
			whsReceiveLine5.WE_WL = locationAPK;
			var whsInventory5 = whsReceiveLine5.Inventory;
			var whsReceiveLine6 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 300m, 3000m, 3000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine6.WE_CustomAttrib1 = "6";
			whsReceiveLine6.WE_WL = locationBPK;
			var whsInventory6 = whsReceiveLine6.Inventory;
			var whsReceiveLine7 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE2", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine7.WE_CustomAttrib1 = "7";
			var whsInventory7 = whsReceiveLine7.Inventory;
			var whsReceiveLine8 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine8.WE_CustomAttrib1 = "8";
			var whsInventory8 = whsReceiveLine8.Inventory;
			var whsReceiveLine9 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 1000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine9.WE_CustomAttrib1 = "9";
			whsReceiveLine9.WE_WL = locationCPK;
			var whsInventory9 = whsReceiveLine9.Inventory;
			var whsReceiveLine10 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 200m, 2000m, 2000m, bondedEntryKey: "EN00123-1", partAttribute1: "B", partAttribute2: "3", partAttribute3: "D");
			whsReceiveLine10.WE_CustomAttrib1 = "10";
			whsReceiveLine10.WE_WL = locationCPK;
			var whsInventory10 = whsReceiveLine10.Inventory;
			var whsReceiveLine11 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part2.PK, "PACKAGE1", 300m, 3000m, 3000m, bondedEntryKey: "EN00123-1", partAttribute1: "A", partAttribute2: "2", partAttribute3: "C");
			whsReceiveLine11.WE_CustomAttrib1 = "11";
			whsReceiveLine11.WE_WL = locationCPK;
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

			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4, whsInventory5, whsInventory6, whsInventory7, whsInventory8, whsInventory9, whsInventory10, whsInventory11 });
			var selectionLines = Header.SelectionLines.Cast<InventorySelectionLine>().ToArray();
			AssertEquals("selectionLines.Length", 5, selectionLines.Length);
			var wrappers = selectionLines.SelectMany(x => x.InventoryWrappers).OrderBy(x => x.ReceiveLine.WE_CustomAttrib1).ToArray();
			AssertEquals("wrappers.Length", 11, wrappers.Length);
			CombineAssertions(() =>
			{
				AssertWrapper("1", wrappers.First(x => x.Inventory.PK == whsInventory1.PK), 3000m, 1000m, 2500m, 900m);
				AssertWrapper("2", wrappers.First(x => x.Inventory.PK == whsInventory2.PK), 6000m, 2000m, 5000m, 1800m);
				AssertWrapper("3", wrappers.First(x => x.Inventory.PK == whsInventory3.PK), 3000m, 1000m, 2500m, 800m);
				AssertWrapper("4", wrappers.First(x => x.Inventory.PK == whsInventory4.PK), 6000m, 2000m, 5000m, 1600m);
				AssertWrapper("5", wrappers.First(x => x.Inventory.PK == whsInventory5.PK), 9000m, 3000m, 7500m, 2700m);
				AssertWrapper("6", wrappers.First(x => x.Inventory.PK == whsInventory6.PK), 9000m, 3000m, 7500m, 2400m);
				AssertWrapper("7", wrappers.First(x => x.Inventory.PK == whsInventory7.PK), 1000m, 1000m, 1000m, 1000m);
				AssertWrapper("8", wrappers.First(x => x.Inventory.PK == whsInventory8.PK), 1000m, 1000m, 1000m, 1000m);
				AssertWrapper("9", wrappers.First(x => x.Inventory.PK == whsInventory9.PK), 3000m, 1000m, 2500m, 800m);
				AssertWrapper("10", wrappers.First(x => x.Inventory.PK == whsInventory10.PK), 6000m, 2000m, 5000m, 1600m);
				AssertWrapper("11", wrappers.First(x => x.Inventory.PK == whsInventory11.PK), 9000m, 3000m, 7500m, 2400m);
			});
		}

		public void TestCalculatedProperties_SerialNumber()
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
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE31", 1m, 1m, 1m, "UNT", "A", "2", "C", "SER", "EN00123-1");
			whsReceiveLine1.WE_CustomAttrib1 = "1";
			whsReceiveLine1.WE_WL = locationCPK;
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE31", 1m, 1m, 1m, "UNT", "A", "2", "C", "SER2", "EN00123-1");
			whsReceiveLine2.WE_CustomAttrib1 = "2";
			whsReceiveLine2.WE_WL = locationCPK;
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE31", 1m, 1m, 1m, "UNT", "A", "2", "C", "SER3", "EN00123-1");
			whsReceiveLine3.WE_CustomAttrib1 = "3";
			whsReceiveLine3.WE_WL = locationCPK;

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();

			var whsInventory1 = whsReceiveLine1.Inventory;
			var whsInventory2 = whsReceiveLine2.Inventory;
			var whsInventory3 = whsReceiveLine3.Inventory;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			CombineAssertions(() =>
			{
				AssertEquals("SelectionLines", 3, Header.SelectionLines.Count);
				var wrappers = Header.SelectionLines.Cast<InventorySelectionLine>().SelectMany(x => x.InventoryWrappers).ToArray();
				AssertEquals("Wrappers", 3, wrappers.Length);
				AssertWrapper("1", wrappers.Single(x => x.Inventory.PK == whsInventory1.PK), 1m, 1m, 1m, 1m);
				AssertWrapper("2", wrappers.Single(x => x.Inventory.PK == whsInventory2.PK), 1m, 1m, 1m, 1m);
				AssertWrapper("3", wrappers.Single(x => x.Inventory.PK == whsInventory3.PK), 1m, 1m, 1m, 1m);
			});
		}

		public void TestQuantityOnHandExcludesReservedStock()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsHelper = Helper.WhsHelper;

			var whsRowC = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "C");
			Helper.Importer.Factory.Save();

			var locationCPK = whsHelper.FindLocation(whsWarehouse.PK, "C").PK;
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsReceive.WD_ArrivalDate = ZDateTimeOffset.Today;
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE31", 1m, 1m, 1m, "UNT", "A", "2", "C", "SER", "EN00123-1");
			whsReceiveLine1.WE_CustomAttrib1 = "1";
			whsReceiveLine1.WE_WL = locationCPK;

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Importer.Factory.Save();

			var whsInventory1 = whsReceiveLine1.Inventory;
			Header.UpdateSelectionLinesDetails(new[] { whsInventory1 });
			CombineAssertions(() =>
			{
				AssertEquals("SelectionLines", 1, Header.SelectionLines.Count);
				var wrappers = Header.SelectionLines.Cast<InventorySelectionLine>().SelectMany(x => x.InventoryWrappers).ToArray();
				AssertEquals("Wrappers", 1, wrappers.Length);
				AssertWrapper("1", wrappers.Single(x => x.Inventory.PK == whsInventory1.PK), 1m, 1m, 1m, 1m);

				var orderPk = Helper.WhsHelper.CreateWhsOrder(Helper.Importer.PK, whsWarehouse.PK, Helper.Importer.PK, "REF1").PK;
				var orderLinePk = helper.WhsHelper.CreateWhsOrderLine(orderPk, Helper.Part.PK, 1.0m);
				var orderLine = Helper.Importer.Factory.Load<IWhsDocketLine>(orderLinePk);
				orderLine.WE_PackageGroupId = "PACKAGE31";

				Header.UpdateSelectionLinesDetails(new[] { whsInventory1 });
				helper.WhsHelper.ReserveStockForOrderLineIfAbleTo(orderLine, whsInventory1);

				AssertWrapper("1", wrappers.Single(x => x.Inventory.PK == whsInventory1.PK), 1m, 1m, 0m, 0m);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			return new WhsInventoryWrapper(whsInventory, Header);
		}

		void AssertWrapper(ZString id, WhsInventoryWrapper wrapper, ZDecimal calculatedOriginalBondedQty, ZDecimal originalBondedQty, ZDecimal calculatedQuantityOnHand, ZDecimal quantityOnHand)
		{
			AssertEquals(id + " CalculatedOriginalBondedQty", calculatedOriginalBondedQty, wrapper.CalculatedOriginalBondedQty);
			AssertEquals(id + " OriginalBondedQty", originalBondedQty, wrapper.OriginalBondedQty);
			AssertEquals(id + " CalculatedQuantityOnHand", calculatedQuantityOnHand, wrapper.CalculatedQuantityOnHand);
			AssertEquals(id + " QuantityOnHand", quantityOnHand, wrapper.QuantityOnHand);
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Header.Factory));
		WhsDataTestHelper helper;

		DeclarationInventorySelectionHeader Header
		{
			get
			{
				if (header == null)
				{
					header = new DeclarationInventorySelectionHeader(Factory.New<BaseJobDeclaration>());
				}
				return header;
			}
		}
		DeclarationInventorySelectionHeader header;
	}
}
