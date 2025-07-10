
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
	[TestedType(typeof(MoveHeaderInventorySelectionHeader))]
	sealed class MoveHeaderInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalculateWeightByCustomsSecondQuantity()
		{
			var selectionHeader = new MoveHeaderInventorySelectionHeader(MoveHeader);
			Helper.Part.OP_Weight = ZDecimal.Zero;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", "ATT1", "ATT2", "ATT3", "", "XJJ-EN00123-1");
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 10000m, "DPR", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			whsCustomsAttribute.WB_CustomsSecondQuantity = 20000m;
			whsCustomsAttribute.WB_CustomsSecondUnitQty = "T";
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
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
			AssertEquals(1, Header.Bills.Count);
			var bill = Header.Bills[0];
			AssertEquals("bill.B0_IssuerCode", "", bill.B0_IssuerCode);
			AssertEquals(1, MoveHeader.MovementDetails.Count);
			var moveDetail = MoveHeader.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill.PK);
			AssertEquals(1, moveDetail.Containers.Count);
			var container = moveDetail.Containers[0];
			AssertEquals(CusInBondContainer.NonContainerizedNumber, container.BC_ContainerNum);
			AssertEquals(1, container.Commodities.Count);
			var commodity = container.Commodities[0];
			var part = Helper.Part;
			AssertEquals(part.OP_WeightUQ, "KG");
			AssertEquals("WarehouseEntryNumber", "XJJ-EN00123", commodity.BY_WarehouseEntryNumber);
			AssertEquals("Calculate by CustomsSecondQuantity", commodity.BY_GrossWeight, 19754m);
			AssertEquals("Calculate by CustomsSecondQuantity", commodity.BY_GrossWeightUnit, "KG");
			part.OP_WeightUQ = "";
			whsCustomsAttribute.WB_CustomsSecondQuantity = 10000m;
			whsCustomsAttribute.WB_CustomsSecondUnitQty = "T";
			Factory.Save();
			container.Commodities.DeleteAll();
			selectionHeader.IsGroupByInventory = true;
			selectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory });
			line = selectionHeader.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			selectionHeader.ImportInventories();
			bill = Header.Bills[0];
			AssertEquals(1, MoveHeader.MovementDetails.Count);
			moveDetail = MoveHeader.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill.PK);
			AssertEquals(1, moveDetail.Containers.Count);
			container = moveDetail.Containers[0];
			AssertEquals(CusInBondContainer.NonContainerizedNumber, container.BC_ContainerNum);
			AssertEquals(1, container.Commodities.Count);
			commodity = container.Commodities[0];
			part = Helper.Part;
			AssertEquals(part.OP_WeightUQ, "");
			AssertEquals("Calculate by CustomsSecondQuantity", commodity.BY_GrossWeight, 10m);
			AssertEquals("Calculate by CustomsSecondQuantity", commodity.BY_GrossWeightUnit, "T");
		}

		public void TestCalculateWeightByCustomsQty()
		{
			var selectionHeader = new MoveHeaderInventorySelectionHeader(MoveHeader);
			Helper.Part.OP_Weight = ZDecimal.Zero;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", "ATT1", "ATT2", "ATT3", "", "XJJ-EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 10000m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
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
			AssertEquals(1, Header.Bills.Count);
			var bill = Header.Bills[0];
			AssertEquals("bill.B0_IssuerCode", "", bill.B0_IssuerCode);
			AssertEquals(1, MoveHeader.MovementDetails.Count);
			var moveDetail = MoveHeader.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill.PK);
			AssertEquals(1, moveDetail.Containers.Count);
			var container = moveDetail.Containers[0];
			AssertEquals(CusInBondContainer.NonContainerizedNumber, container.BC_ContainerNum);
			AssertEquals(1, container.Commodities.Count);
			var commodity = container.Commodities[0];
			AssertEquals("WarehouseEntryNumber", "XJJ-EN00123", commodity.BY_WarehouseEntryNumber);
			AssertEquals("Calculate by CustomsQty", commodity.BY_GrossWeight, 10m);
		}

		public void TestGetFilterDefaults()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "HO2");
			Factory.Save();
			var selectionHeader = new MoveHeaderInventorySelectionHeader(MoveHeader);
			var filters = selectionHeader.GetFilterDefaults();
			var filter = filters["Client:Property"];
			AssertEquals(Helper.Importer.PK, filter.Value);
			MoveHeader.BM_OA_WarehouseAddress = Helper.Warehouse.MainAddress.PK;
			filters = selectionHeader.GetFilterDefaults();
			filter = filters["Warehouse:Property"];
			AssertEquals(whsWarehouse.PK, filter.Value);
		}

		public void TestCreationOfCartonGroupingProducts()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1");
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKING1", 50m, 900m, 900m, "PK", "ATT1", "ATT2", "ATT3", "", "XJJ-EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory1.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			var supplierDocAddress1 = Factory.New<JobDocAddress>();
			supplierDocAddress1.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress1.E2_ParentID = whsInventory1.WI_WE_InDocketLine;
			supplierDocAddress1.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress1.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
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
			var selectionHeader = new MoveHeaderInventorySelectionHeader(MoveHeader);
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
			AssertEquals(1, Header.Bills.Count);
			var bill = Header.Bills[0];
			AssertEquals("bill.B0_MasterBillNumber", "", bill.B0_MasterBillNumber);
			AssertEquals("bill.B0_IssuerCode", "", bill.B0_IssuerCode);
			AssertEquals("bill.B0_ManifestQty", 318, bill.B0_ManifestQty);
			AssertEquals(1, MoveHeader.MovementDetails.Count);
			var moveDetail = MoveHeader.MovementDetails[0];
			AssertEquals("moveDetail.B9_InBoundQty", 318, moveDetail.B9_InBoundQty);
			AssertNoMessageErrors(moveDetail.B9_InBoundQtyInfo);
			AssertEquals(1, moveDetail.Containers.Count);
			var container = moveDetail.Containers[0];
			AssertEquals(3, container.Commodities.Count);
			var commodity1 = container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_Description == container.Bill.B0_MasterBillNumber + (container.Bill.B0_MasterBillNumber == ZString.Empty ? "" : " ") + Helper.Part.OP_Desc + " " + Helper.Part2.OP_Desc && x.BY_PartNumberForBinding == CusInBondCargoDesc.CheckSubLevelMessage && x.BY_PieceCount == 6);
			AssertEquals("commodity1.ChildCommodities.Count", 2, commodity1.ChildCommodities.Count);
			AssertProductLine(commodity1.ChildCommodities[0], Helper.Warehouse2.PK, Helper.Part, 300m, "XJJ-EN00123", 1, "ATT1", "ATT2", "ATT3", "");
			AssertProductLine(commodity1.ChildCommodities[1], ZGuid.Empty, Helper.Part2, 600m, "XJJ-EN00123", 2, "", "", "", "");
			var commodity2 = container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_Description == Helper.Part.OP_Desc && x.BY_PartNumberForBinding == Helper.Part.OP_PartNum && x.BY_PieceCount == 300);
			AssertEquals("commodity2.ChildCommodities.Count", 0, commodity2.ChildCommodities.Count);
			AssertProductLine(commodity2, ZGuid.Empty, Helper.Part, 300m, "XJJ-EN00123", 3, "", "", "", "");
			var commodity3 = container.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_Description == Helper.Part2.OP_Desc && x.BY_PartNumberForBinding == Helper.Part2.OP_PartNum && x.BY_PieceCount == 12);
			AssertEquals("commodity3.ChildCommodities.Count", 0, commodity3.ChildCommodities.Count);
			AssertProductLine(commodity3, Helper.Warehouse.PK, Helper.Part2, 600m, "XJJ-EN00124", 1, "", "", "", "");
			container.Commodities.DeleteAll();
			container.BC_ContainerNum = "CON3232";
			var container2 = moveDetail.Containers.AddNew();
			container2.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			var commodity4 = container2.Commodities.AddNew();
			commodity4.BY_PieceCount = 10;
			var moveHeader2 = Header.MovementHeaders.AddNew();
			var moveDetail3 = moveHeader2.MovementDetails.AddNew(bill.PK);
			bill.B0_ManifestQty = 1;
			selectionHeader.ImportInventories();
			AssertEquals("bill.B0_ManifestQty should not be updated as there are multiple movedetails linked to it", 1, bill.B0_ManifestQty);
			AssertEquals("moveDetail1.B9_InBoundQty", 328, moveDetail.B9_InBoundQty);
			AssertEquals(0, container.Commodities.Count);
			var commodities = container2.Commodities.OrderBy(x => x.BY_PieceCount).ToArray();
			AssertEquals(4, commodities.Length);
			commodity1 = commodities[0];
			AssertEquals("commodity1.ChildCommodities.Count", 2, commodity1.ChildCommodities.Count);
			AssertProductLine(commodity1.ChildCommodities[0], Helper.Warehouse2.PK, Helper.Part, 300m, "XJJ-EN00123", 1, "ATT1", "ATT2", "ATT3", "");
			AssertProductLine(commodity1.ChildCommodities[1], ZGuid.Empty, Helper.Part2, 600m, "XJJ-EN00123", 2, "", "", "", "");
			commodity2 = commodities[3];
			AssertEquals("commodity2.ChildCommodities.Count", 0, commodity2.ChildCommodities.Count);
			AssertProductLine(commodity2, ZGuid.Empty, Helper.Part, 300m, "XJJ-EN00123", 3, "", "", "", "");
			commodity3 = commodities[2];
			AssertEquals("commodity3.ChildCommodities.Count", 0, commodity3.ChildCommodities.Count);
			AssertProductLine(commodity3, Helper.Warehouse.PK, Helper.Part2, 600m, "XJJ-EN00124", 1, "", "", "", "");
			commodity1 = commodities[1];
			AssertEquals("commodity1.ChildCommodities.Count", 0, commodity1.ChildCommodities.Count);
			AssertProductLine(commodity1, ZGuid.Empty, null, ZDecimal.Zero, "", 0, "", "", "", "");
		}

		public void TestCreationOfCartonGroupingProducts_WithSerialNumber()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1");
			Helper.Importer.MiscServ.OM_IMUseSerialNumber = true;
			Helper.Part.RelatedOrganisations[0].OU_UseSerialNumber = true;
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKING1", 1m, 1m, 1m, "PK", serialNumber: "SN1", bondedEntryKey: "XJJ-EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 1m, 1m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			var supplierDocAddress1 = Factory.New<JobDocAddress>();
			supplierDocAddress1.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress1.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress1.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress1.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			var selectionHeader = new MoveHeaderInventorySelectionHeader(MoveHeader);
			selectionHeader.IsGroupByCarton = true;
			AssertEquals(0, selectionHeader.SelectionLines.Count);
			selectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, selectionHeader.SelectionLines.Count);
			var line = selectionHeader.SelectionLines.OfType<Customs.Business.InventorySelectionLine>().FirstOrDefault();
			AssertEquals(1m, line.US_ProductQtyPerCarton);
			line.US_ProductQtyToDraw = 1m;
			selectionHeader.ImportInventories();
			AssertEquals(1, Header.Bills.Count);
			var bill = Header.Bills[0];
			AssertEquals("bill.B0_MasterBillNumber", "", bill.B0_MasterBillNumber);
			AssertEquals("bill.B0_IssuerCode", "", bill.B0_IssuerCode);
			AssertEquals("bill.B0_ManifestQty", 1, bill.B0_ManifestQty);
			AssertEquals(1, MoveHeader.MovementDetails.Count);
			var moveDetail = MoveHeader.MovementDetails[0];
			AssertEquals("moveDetail.B9_InBoundQty", 1, moveDetail.B9_InBoundQty);
			AssertNoMessageErrors(moveDetail.B9_InBoundQtyInfo);
			AssertEquals(1, moveDetail.Containers.Count);
			var container = moveDetail.Containers[0];
			AssertEquals(1, container.Commodities.Count);
			var commodity = container.Commodities.OfType<CusInBondCargoDesc>().First();
			AssertEquals("commodity.ChildCommodities.Count", 0, commodity.ChildCommodities.Count);
			AssertProductLine(commodity, Helper.Warehouse2.PK, Helper.Part, 1m, "XJJ-EN00123", 1, "", "", "", "SN1");
		}

		public void TestUpdateSelectionLinesDetails()
		{
			var selectionHeader = new MoveHeaderInventorySelectionHeader(MoveHeader);
			Helper.Part.OP_Desc = Helper.Part.OP_Desc + "\r\n";
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", "ATT1", "ATT2", "ATT3", "", "XJJ-EN00123-1");
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			if (!whsCustomsAttribute.WB_AddInfo.Contains(US.Business.JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3)))
			{
				whsCustomsAttribute.WB_AddInfo = whsCustomsAttribute.WB_AddInfo + ZString.Format("*{0}={1}", US.Business.JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), ZoneStatusCodeList.Codes.D);
			}

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
			AssertEquals(1, Header.Bills.Count);
			var bill = Header.Bills[0];
			AssertEquals("bill.B0_IssuerCode", "", bill.B0_IssuerCode);
			AssertEquals(1, MoveHeader.MovementDetails.Count);
			var moveDetail = MoveHeader.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill.PK);
			AssertEquals(1, moveDetail.Containers.Count);
			var container = moveDetail.Containers[0];
			AssertEquals(CusInBondContainer.NonContainerizedNumber, container.BC_ContainerNum);
			AssertEquals(1, container.Commodities.Count);
			var commodity = container.Commodities[0];
			AssertProductLine(commodity, Helper.Warehouse2.PK, Helper.Part, 800m, "XJJ-EN00123", 1, "ATT1", "ATT2", "ATT3", "");
			var part = Helper.Part;
			AssertEquals(part.OP_Weight, 2.0m);
			AssertEquals(commodity.BY_GrossWeight, 1600m);
			Assert(commodity.BY_Description.Contains("\r\nFDP"));
			Assert(!commodity.BY_Description.Replace("\r\nFDP", "").Contains("\r\n"));
		}

		public void TestUpdateSelectionLinesDetails_WithSerialNumber()
		{
			var selectionHeader = new MoveHeaderInventorySelectionHeader(MoveHeader);
			Helper.Part.OP_Desc = Helper.Part.OP_Desc + "\r\n";
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "H02");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 1m, 1m, 1m, "PK", serialNumber: "SN1", bondedEntryKey: "XJJ-EN00123-1");
			var supplierDocAddress = Factory.New<JobDocAddress>();
			supplierDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierDocAddress.E2_ParentID = whsInventory.WI_WE_InDocketLine;
			supplierDocAddress.E2_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			supplierDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", "", "XJJ-EN00123", (ZShort)1);
			if (!whsCustomsAttribute.WB_AddInfo.Contains(US.Business.JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3)))
			{
				whsCustomsAttribute.WB_AddInfo = whsCustomsAttribute.WB_AddInfo + ZString.Format("*{0}={1}", US.Business.JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), ZoneStatusCodeList.Codes.D);
			}

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
			AssertEquals(1, Header.Bills.Count);
			var bill = Header.Bills[0];
			AssertEquals("bill.B0_IssuerCode", "", bill.B0_IssuerCode);
			AssertEquals(1, MoveHeader.MovementDetails.Count);
			var moveDetail = MoveHeader.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill.PK);
			AssertEquals(1, moveDetail.Containers.Count);
			var container = moveDetail.Containers[0];
			AssertEquals(CusInBondContainer.NonContainerizedNumber, container.BC_ContainerNum);
			AssertEquals(1, container.Commodities.Count);
			var commodity = container.Commodities[0];
			AssertProductLine(commodity, Helper.Warehouse2.PK, Helper.Part, 1m, "XJJ-EN00123", 1, "", "", "", "SN1");
			var part = Helper.Part;
			AssertEquals(part.OP_Weight, 2m);
			AssertEquals(commodity.BY_GrossWeight, 2m);
			Assert(commodity.BY_Description.Contains("\r\nFDP"));
			Assert(!commodity.BY_Description.Replace("\r\nFDP", "").Contains("\r\n"));
		}

		void AssertProductLine(CusInBondCargoDesc commodity, ZGuid supplierPK, OrgSupplierPart part, ZDecimal quantity, ZString warehouseEntryNumber, ZShort warehouseEntryLineNo, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber)
		{
			CombineAssertions(() =>
			{
				AssertEquals("BY_OH_Supplier", supplierPK, commodity.BY_OH_Supplier);
				if (part == null)
				{
					AssertEquals("BY_PartNumber", ZString.Empty, commodity.BY_PartNumber);
					AssertEquals("BY_OP_Part", ZGuid.Empty, commodity.BY_OP_Part);
				}
				else
				{
					AssertEquals("BY_PartNumber", part.OP_PartNum, commodity.BY_PartNumber);
					AssertEquals("BY_OP_Part", part.PK, commodity.BY_OP_Part);
				}

				AssertEquals("BY_InvoiceQuantity", quantity, commodity.BY_InvoiceQuantity);
				AssertEquals("BY_WarehouseEntryNumber", warehouseEntryNumber, commodity.BY_WarehouseEntryNumber);
				AssertEquals("BY_WarehouseEntryLineNo", warehouseEntryLineNo, commodity.BY_WarehouseEntryLineNo);
				AssertEquals("BY_PartAttrib1", partAttrib1, commodity.BY_PartAttrib1);
				AssertEquals("BY_PartAttrib2", partAttrib2, commodity.BY_PartAttrib2);
				AssertEquals("BY_PartAttrib3", partAttrib3, commodity.BY_PartAttrib3);
				AssertEquals("BY_SerialNumber", serialNumber, commodity.BY_SerialNumber);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new MoveHeaderInventorySelectionHeader(MoveHeader);

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

		CusInBondMoveHeader moveHeader;
		CusInBondMoveHeader MoveHeader => moveHeader ?? (moveHeader = Header.MovementHeaders.AddNew());

		WhsDataTestHelper helper;
		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
	}
}
