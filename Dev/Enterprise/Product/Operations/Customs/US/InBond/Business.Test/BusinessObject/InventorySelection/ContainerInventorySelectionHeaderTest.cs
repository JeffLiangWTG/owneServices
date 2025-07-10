using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(ContainerInventorySelectionHeader))]
	sealed class ContainerInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetFilterDefaults()
		{
			var selectionHeader = new ContainerInventorySelectionHeader(Container);
			var filters = selectionHeader.GetFilterDefaults();
			AssertEquals(false, filters.ContainsDefaultFor("Customs Entry Key:Property"));
			var commodity1 = Container.Commodities.AddNew();
			commodity1.BY_PartNumber = "PART1";
			filters = selectionHeader.GetFilterDefaults();
			AssertEquals(false, filters.ContainsDefaultFor("Customs Entry Key:Property"));
			commodity1.BY_WarehouseEntryNumber = "XJ5-OB2343";
			filters = selectionHeader.GetFilterDefaults();
			var filter = filters["Customs Entry Key:Property"];
			AssertEquals("XJ5-OB2343", filter.Value);
			var commodity2 = Container.Commodities.AddNew();
			commodity2.BY_PartNumber = "PART1";
			filters = selectionHeader.GetFilterDefaults();
			filter = filters["Customs Entry Key:Property"];
			AssertEquals("XJ5-OB2343", filter.Value);
			commodity2.BY_WarehouseEntryNumber = "XJ5-OB2343";
			filters = selectionHeader.GetFilterDefaults();
			filter = filters["Customs Entry Key:Property"];
			AssertEquals("XJ5-OB2343", filter.Value);
			commodity2.BY_WarehouseEntryNumber = "XJ5-OB2344";
			filters = selectionHeader.GetFilterDefaults();
			AssertEquals(false, filters.ContainsDefaultFor("Customs Entry Key:Property"));
			commodity2.BY_WarehouseEntryNumber = ZString.Empty;
			filters = selectionHeader.GetFilterDefaults();
			filter = filters["Customs Entry Key:Property"];
			AssertEquals("XJ5-OB2343", filter.Value);
		}

		public void TestCanWithdrawFromDifferentEntryNumbers()
		{
			var header = new ContainerInventorySelectionHeader(Container);
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1");
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", bondedEntryKey: "XJJ-EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory1.WI_WE_InDocketLine, 10000m, 100m, "LP", "AU", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", bondedEntryKey: "XJJ-EN00123-3");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory2.WI_WE_InDocketLine, 10000m, 100m, "LP", "AU", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)3);
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", bondedEntryKey: "XJJ-EN00123-2");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory3.WI_WE_InDocketLine, 10000m, 100m, "LP", "AU", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)2);
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			header.IsGroupByInventory = true;
			AssertEquals(0, header.SelectionLines.Count);
			header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			AssertEquals(3, header.SelectionLines.Count);
			var line1 = header.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00123-1");
			line1.US_ProductQtyToDraw = 100m;
			AssertNoErrors(line1.US_ProductQtyToDrawInfo);
			var line2 = header.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00123-2");
			line2.US_ProductQtyToDraw = 100m;
			AssertNoErrors(line2.US_ProductQtyToDrawInfo);
			var line3 = header.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00123-3");
			line3.US_ProductQtyToDraw = 100m;
			AssertNoErrors(line3.US_ProductQtyToDrawInfo);
			header.ImportInventories();
			AssertEquals("Bill.B0_ManifestQty", 300, Bill.B0_ManifestQty);
			AssertEquals("MoveDetail.B9_InBoundQty", 300, MoveDetail.B9_InBoundQty);
			AssertEquals(3, Container.Commodities.Count);
			var commodity1 = Container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_WarehouseEntryNumber == "XJJ-EN00123" && x.BY_WarehouseEntryLineNo == 1);
			AssertProductLine(commodity1, ZGuid.Empty, Helper.Part, 100m, "XJJ-EN00123", 1, "", "", "", "");
			var commodity2 = Container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_WarehouseEntryNumber == "XJJ-EN00123" && x.BY_WarehouseEntryLineNo == 2);
			AssertProductLine(commodity2, ZGuid.Empty, Helper.Part, 100m, "XJJ-EN00123", 2, "", "", "", "");
			var commodity3 = Container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_WarehouseEntryNumber == "XJJ-EN00123" && x.BY_WarehouseEntryLineNo == 3);
			AssertProductLine(commodity3, ZGuid.Empty, Helper.Part, 100m, "XJJ-EN00123", 3, "", "", "", "");
		}

		public void TestCreationOfCartonGroupingProducts()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1");
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKING1", 50m, 900m, 900m, "PK", "ATT1", "ATT2", "ATT3", "", "XJJ-EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory1.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory1.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part2, "PACKING1", 100m, 1800m, 1800m, "PK", bondedEntryKey: "XJJ-EN00123-2");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory2.WI_WE_InDocketLine, 30000m, 200m, "LP", "AU", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)2);
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "", 1m, 400m, 400m, "PK", bondedEntryKey: "XJJ-EN00123-3");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory3.WI_WE_InDocketLine, 5000m, 250m, "LP", "AU", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)3);
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV2");
			var whsInventory4 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part2, "PACKING2", 50m, 900m, 900m, "PK", bondedEntryKey: "XJJ-EN00124-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory4.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00124", (ZShort)1);
			var supplierDocAddress2 = Factory.New<JobDocAddress>();
			supplierDocAddress2.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress2.E2_ParentID = whsReceive2.PK;
			supplierDocAddress2.E2_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			supplierDocAddress2.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			Bill.B0_ManifestQty = 10;
			MoveDetail.B9_InBoundQty = 10;
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory4.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			var selectionHeader = new ContainerInventorySelectionHeader(Container);
			selectionHeader.IsGroupByCarton = true;
			AssertEquals(0, selectionHeader.SelectionLines.Count);
			selectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3, whsInventory4 });
			AssertEquals(3, selectionHeader.SelectionLines.Count);
			var line1 = selectionHeader.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().FirstOrDefault(x => x.US_ProductQtyOnHand == 2700m);
			AssertEquals(150m, line1.US_ProductQtyPerCarton);
			line1.US_ProductQtyToDraw = 900m;
			var line2 = selectionHeader.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().FirstOrDefault(x => x.US_ProductQtyOnHand == 400m);
			AssertEquals(1m, line2.US_ProductQtyPerCarton);
			line2.US_ProductQtyToDraw = 300m;
			var line3 = selectionHeader.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().FirstOrDefault(x => x.US_ProductQtyOnHand == 900m);
			AssertEquals(50m, line3.US_ProductQtyPerCarton);
			line3.US_ProductQtyToDraw = 600m;
			selectionHeader.ImportInventories();
			AssertEquals("Bill.B0_ManifestQty", 318, Bill.B0_ManifestQty);
			AssertEquals("MoveDetail.B9_InBoundQty", 318, MoveDetail.B9_InBoundQty);
			AssertEquals(3, Container.Commodities.Count);
			var commodity1 = Container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_PartNumberForBinding == CusInBondCargoDesc.CheckSubLevelMessage && x.BY_PieceCount == 6);
			AssertEquals("commodity1.ChildCommodities.Count", 2, commodity1.ChildCommodities.Count);
			AssertProductLine(commodity1.ChildCommodities[0], Helper.Warehouse2.PK, Helper.Part, 300m, "XJJ-EN00123", 1, "ATT1", "ATT2", "ATT3", "");
			AssertProductLine(commodity1.ChildCommodities[1], ZGuid.Empty, Helper.Part2, 600m, "XJJ-EN00123", 2, "", "", "", "");
			var commodity2 = Container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_Description == Helper.Part2.OP_Desc && x.BY_PartNumberForBinding == Helper.Part2.OP_PartNum && x.BY_PieceCount == 12);
			AssertEquals("commodity2.ChildCommodities.Count", 0, commodity2.ChildCommodities.Count);
			AssertProductLine(commodity2, Helper.Warehouse.PK, Helper.Part2, 600m, "XJJ-EN00124", 1, "", "", "", "");
			var commodity3 = Container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_Description == Helper.Part.OP_Desc && x.BY_PartNumberForBinding == Helper.Part.OP_PartNum && x.BY_PieceCount == 300);
			AssertEquals("commodity3.ChildCommodities.Count", 0, commodity3.ChildCommodities.Count);
			AssertProductLine(commodity3, ZGuid.Empty, Helper.Part, 300m, "XJJ-EN00123", 3, "", "", "", "");
		}

		public void TestCreationOfCartonGroupingProducts_WithSerialNumber()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1");
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKING1", 1m, 1m, 1m, "PK", serialNumber: "SN", bondedEntryKey: "XJJ-EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			Bill.B0_ManifestQty = 1;
			MoveDetail.B9_InBoundQty = 1;
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			var selectionHeader = new ContainerInventorySelectionHeader(Container);
			selectionHeader.IsGroupByCarton = true;
			AssertEquals(0, selectionHeader.SelectionLines.Count);
			selectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, selectionHeader.SelectionLines.Count);
			var line = selectionHeader.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().FirstOrDefault();
			AssertEquals(1m, line.US_ProductQtyPerCarton);
			line.US_ProductQtyToDraw = 1m;
			selectionHeader.ImportInventories();
			AssertEquals("Bill.B0_ManifestQty", 1, Bill.B0_ManifestQty);
			AssertEquals("MoveDetail.B9_InBoundQty", 1, MoveDetail.B9_InBoundQty);
			AssertEquals(1, Container.Commodities.Count);
			var commodity = Container.Commodities.OfType<CusInBondCargoDesc>().First();
			AssertEquals("commodity1.ChildCommodities.Count", 0, commodity.ChildCommodities.Count);
			AssertProductLine(commodity, Helper.Warehouse2.PK, Helper.Part, 1m, "XJJ-EN00123", 1, "", "", "", "SN");
		}

		public void TestUpdateSelectionLinesDetails()
		{
			var selectionHeader = new ContainerInventorySelectionHeader(Container);
			Helper.Warehouse2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", "ATT1", "ATT2", "ATT3", "", "XJJ-EN00123-1");
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			selectionHeader.IsGroupByInventory = true;
			AssertEquals(0, selectionHeader.SelectionLines.Count);
			selectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, selectionHeader.SelectionLines.Count);
			var line = selectionHeader.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			selectionHeader.ImportInventories();
			AssertEquals("moveHeader.WarehouseAddress", Helper.Warehouse.MainAddress, MoveHeader.WarehouseAddress);
			AssertEquals(1, Container.Commodities.Count);
			var commodity = Container.Commodities[0];
			AssertProductLine(commodity, Helper.Warehouse2.PK, Helper.Part, 800m, "XJJ-EN00123", 1, "ATT1", "ATT2", "ATT3", "");
		}

		public void TestUpdateSelectionLinesDetails_WithSerialNumber()
		{
			var selectionHeader = new ContainerInventorySelectionHeader(Container);
			Helper.Warehouse2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 1m, 1m, 1m, "PK", serialNumber: "SN", bondedEntryKey: "XJJ-EN00123-1");
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			selectionHeader.IsGroupByInventory = true;
			AssertEquals(0, selectionHeader.SelectionLines.Count);
			selectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, selectionHeader.SelectionLines.Count);
			var line = selectionHeader.SelectionLines[0];
			line.US_ProductQtyToDraw = 1m;
			selectionHeader.ImportInventories();
			AssertEquals("moveHeader.WarehouseAddress", Helper.Warehouse.MainAddress, MoveHeader.WarehouseAddress);
			AssertEquals(1, Container.Commodities.Count);
			var commodity = Container.Commodities[0];
			AssertProductLine(commodity, Helper.Warehouse2.PK, Helper.Part, 1m, "XJJ-EN00123", 1, "", "", "", "SN");
		}

		protected override BusinessObject GetNewBusinessObject() => new ContainerInventorySelectionHeader(MoveDetail.Containers.AddNew());

		CusInBondHeader header;
		CusInBondHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<CusInBondHeader>();
					header.BH_OA_Importer = Helper.Importer.MainAddress.PK;
					header.BH_FTZMove = true;
				}

				return header;
			}
		}

		CusInBondMoveDetail moveDetail;
		CusInBondMoveDetail MoveDetail => moveDetail ?? (moveDetail = MoveHeader.MovementDetails.AddNew(Bill.PK));

		CusInBondMoveHeader moveHeader;
		CusInBondMoveHeader MoveHeader => moveHeader ?? (moveHeader = Header.MovementHeaders.AddNew());

		CusInBondContainer container;
		CusInBondContainer Container => container ?? (container = MoveDetail.Containers.AddNew());

		CusInBondBill bill;
		CusInBondBill Bill => bill ?? (bill = Header.Bills.AddNew());

		WhsDataTestHelper helper;
		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));

		void AssertProductLine(CusInBondCargoDesc commodity, ZGuid supplierPK, OrgSupplierPart part, ZDecimal quantity, ZString warehouseEntryNumber, ZShort warehouseEntryLineNo, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber)
		{
			CombineAssertions(() =>
			{
				AssertEquals("BY_OH_Supplier", supplierPK, commodity.BY_OH_Supplier);
				AssertEquals("BY_PartNumber", part.OP_PartNum, commodity.BY_PartNumber);
				AssertEquals("BY_OP_Part", part.PK, commodity.BY_OP_Part);
				AssertEquals("BY_InvoiceQuantity", quantity, commodity.BY_InvoiceQuantity);
				AssertEquals("BY_WarehouseEntryNumber", warehouseEntryNumber, commodity.BY_WarehouseEntryNumber);
				AssertEquals("BY_WarehouseEntryLineNo", warehouseEntryLineNo, commodity.BY_WarehouseEntryLineNo);
				AssertEquals("BY_PartAttrib1", partAttrib1, commodity.BY_PartAttrib1);
				AssertEquals("BY_PartAttrib2", partAttrib2, commodity.BY_PartAttrib2);
				AssertEquals("BY_PartAttrib3", partAttrib3, commodity.BY_PartAttrib3);
				AssertEquals("BY_SerialNumber", serialNumber, commodity.BY_SerialNumber);
			});
		}
	}
}
