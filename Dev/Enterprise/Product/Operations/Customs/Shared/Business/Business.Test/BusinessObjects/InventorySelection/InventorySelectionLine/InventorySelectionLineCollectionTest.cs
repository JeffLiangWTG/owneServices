using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InventorySelectionLineCollection<InventorySelectionLine>))]
	sealed class InventorySelectionLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InventorySelectionLineCollection<InventorySelectionLine>>
	{
		public void TestHasALeastOneLineWithDrawQty()
		{
			var lines = Header.SelectionLines;
			AssertEquals(false, lines.HasALeastOneLineWithDrawQty);
			var line = lines.AddNew();
			AssertEquals(false, lines.HasALeastOneLineWithDrawQty);
			line.US_ProductQtyToDraw = 10m;
			AssertEquals(true, lines.HasALeastOneLineWithDrawQty);
			line.US_ProductQtyToDraw = 0m;
			AssertEquals(false, lines.HasALeastOneLineWithDrawQty);
			line.US_CartonQtytoDraw = 10;
			AssertEquals(false, lines.HasALeastOneLineWithDrawQty);
		}

		public void TestWillDrawLineFromDifferentWarehouses()
		{
			HeaderMock.Protected().Setup<bool>("AllowWithdrawalOfMultipleEntryDetailsCore").Returns(false);
			Header.IsGroupByCarton = true;
			var lines = Header.SelectionLines;
			AssertEquals(false, lines.WillDrawLineFromDifferentWarehouses);

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1");
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, lines.Count);
			AssertEquals(false, lines.WillDrawLineFromDifferentWarehouses);

			var whsWarehouse2 = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N20");
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse2.PK, Helper.Importer.PK, "RCV2");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part2, "PACKAGE1", 100m, 2000m, 1800m, bondedEntryKey: "EN00123-2");
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { whsInventory, whsInventory2 });
			AssertEquals(2, lines.Count);
			AssertEquals(false, lines.WillDrawLineFromDifferentWarehouses);
			var line1 = lines[0];
			var line2 = lines[1];
			line1.US_ProductQtyToDraw = 100m;
			AssertEquals(false, lines.WillDrawLineFromDifferentWarehouses);
			line2.US_CartonQtytoDraw = 1;
			AssertEquals(true, lines.WillDrawLineFromDifferentWarehouses);
		}

		public void TestGetFirstWarehouseAddress()
		{
			var lines = Header.SelectionLines;
			AssertNull(lines.GetFirstWarehouseAddress());
			var line1 = lines.AddNew();
			AssertNull(lines.GetFirstWarehouseAddress());

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			var inventoryWrapper = new WhsInventoryWrapper(whsInventory, Header);
			line1.UpdateSelectionLinesDetails(inventoryWrapper);
			AssertEquals(Helper.Warehouse.MainAddress, lines.GetFirstWarehouseAddress());

			var whsWarehouse2 = Helper.GetNewWhsWarehouse(Helper.Warehouse2.MainAddress.PK, true, "N20");
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse2.PK, Helper.Importer.PK);
			var whsReceive2Line = Helper.GetNewWhsReceiveLine(whsReceive2.PK, Helper.Part2.PK, "PACKAGE1", 100m, 2000m, 1800m, bondedEntryKey: "EN00123-2");
			var whsInventory2 = whsReceive2Line.Inventory;
			var inventoryWrapper2 = new WhsInventoryWrapper(whsInventory2, Header);
			line1.UpdateSelectionLinesDetails(inventoryWrapper2);
			AssertEquals(Helper.Warehouse2.MainAddress, lines.GetFirstWarehouseAddress());

			line1.UpdateSelectionLinesDetails(inventoryWrapper2, inventoryWrapper);
			AssertEquals(Helper.Warehouse.MainAddress, lines.GetFirstWarehouseAddress());
			AssertEquals("Multiple Inventories when group by Inventory", CargoWise.Common.ErrorReporter.LastMessageReported);
			CargoWise.Common.ErrorReporter.Clear();
			line1.UpdateSelectionLinesDetails(inventoryWrapper, inventoryWrapper2);
			AssertEquals(Helper.Warehouse.MainAddress, lines.GetFirstWarehouseAddress());
			AssertEquals("Multiple Inventories when group by Inventory", CargoWise.Common.ErrorReporter.LastMessageReported);
			CargoWise.Common.ErrorReporter.Clear();

			var line2 = lines.AddNew();
			line1.UpdateSelectionLinesDetails(inventoryWrapper2);
			line2.UpdateSelectionLinesDetails(inventoryWrapper);
			AssertEquals(Helper.Warehouse2.MainAddress, lines.GetFirstWarehouseAddress());

			line1.UpdateSelectionLinesDetails(inventoryWrapper);
			line2.UpdateSelectionLinesDetails(inventoryWrapper2);
			AssertEquals(Helper.Warehouse.MainAddress, lines.GetFirstWarehouseAddress());
		}

		WhsDataTestHelper Helper
		{
			get { return helper ?? (helper = new WhsDataTestHelper(Header.Factory)); }
		}
		WhsDataTestHelper helper;

		protected override InventorySelectionLineCollection<InventorySelectionLine> GetCollectionToTest()
		{
			return new InventorySelectionLineCollection<InventorySelectionLine>(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InventorySelectionLine(Header);
		}

		Mock<DeclarationInventorySelectionHeader> HeaderMock
		{
			get { return headerMock ?? (headerMock = new Mock<DeclarationInventorySelectionHeader>(Declaration) { CallBase = true }); }
		}
		Mock<DeclarationInventorySelectionHeader> headerMock;

		DeclarationInventorySelectionHeader Header
		{
			get { return HeaderMock.Object; }
		}

		BaseJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<BaseJobDeclaration>()); }
		}
		BaseJobDeclaration declaration;
	}
}
