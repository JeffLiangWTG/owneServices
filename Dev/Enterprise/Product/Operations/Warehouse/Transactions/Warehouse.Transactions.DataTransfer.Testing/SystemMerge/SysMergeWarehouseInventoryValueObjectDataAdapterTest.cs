using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class SysMergeWarehouseInventoryValueObjectDataAdapterTest : WhsTestCaseWithFactory
	{
		#region TestExport

		#region TestExport_Receive

		public void TestExport_Receive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ExWhsJobGuid = new ZGuid();
			receive.WD_WL_CrossDock = dockLocationA.PK;

			receive.WD_DocketID = "W00000123";
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_TotalWeightUnit = "KG";
			receive.WD_TotalCubicUnit = "M3";
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			receive.WD_PickOption = WhsPickOption.Codes.Manual;
			receive.WD_CustomerReference = "CUSTOM REF";
			receive.WD_TransportReference = "TRANSPORT REF";
			receive.WD_DropMode = "ZZZ";
			receive.WD_CODPayMethod = "AAA";
			receive.WD_INCO = "BBB";
			receive.WD_F3_NKTotalPackType = "BOX";
			receive.WD_PL_NKCarrierServiceLevel = "D2D";
			receive.WD_RS_NKServiceLevel = "TSL";
			receive.WD_CustomAttrib1 = "CUSTOM 1";
			receive.WD_CustomAttrib2 = "CUSTOM 2";
			receive.WD_CustomAttrib3 = "CUSTOM 3";
			receive.WD_CustomAttrib4 = "CUSTOM 4";
			receive.WD_CustomAttrib5 = "CUSTOM 5";

			receive.WD_TotalUnits = 1m;
			receive.WD_TotalWeight = 2m;
			receive.WD_TotalCubic = 3m;
			receive.WD_UnitsSent = 4m;
			receive.WD_CubicSent = 6m;
			receive.WD_LocalCartInsuranceCost = 7m;
			receive.WD_ShipperCODAmount = 8m;
			receive.WD_WeightSent = 9m;
			receive.WD_WeightSentUserEntered = 10;
			receive.WD_CustomDecimal1 = 11m;
			receive.WD_CustomDecimal2 = 12m;
			receive.WD_CustomDecimal3 = 13m;
			receive.WD_CustomDecimal4 = 14m;
			receive.WD_CustomDecimal5 = 15m;

			receive.WD_PackagesSent = 16;

			receive.WD_TotalPallets = 17;
			receive.WD_PalletsSent = 19;

			receive.WD_ExternalReferenceSplit = 0;

			receive.WD_WeightVolSetFromImport = true;
			receive.WD_AddPalletWeightToOrder = true;
			receive.WD_AutoFinaliseBOMIntoInventory = true;
			receive.WD_CustomFlag1 = true;
			receive.WD_CustomFlag2 = true;
			receive.WD_CustomFlag3 = true;
			receive.WD_CustomFlag4 = true;
			receive.WD_CustomFlag5 = true;

			var today = ZDateTime.Today;
			var todayOffset = today.ToOffset();
			receive.WD_BookingDate = todayOffset.AddDays(1);
			receive.WD_ArrivalDate = todayOffset.AddDays(2);
			receive.WD_FinalisedDate = todayOffset.AddDays(3);
			receive.WD_RequiredDate = todayOffset.AddDays(4);
			receive.WD_ETA = todayOffset.AddDays(5);
			receive.WD_ETD = todayOffset.AddDays(6);
			receive.WD_CustomDate1 = today.AddDays(7);
			receive.WD_CustomDate2 = today.AddDays(8);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertExportDocketData(receive, xsdReceive);
		}

		void AssertExportDocketData(WhsDocket docket, SystemMergeWarehouseReceive xsdReceive)
		{
			AssertEquals("xsdReceive.PK", docket.PK.ToString(), xsdReceive.PK, xsdReceive.PKSpecified);
			AssertEquals("xsdReceive.ClientPK", docket.WD_OH_Client.ToString(), xsdReceive.ClientPK, xsdReceive.ClientPKSpecified);
			AssertEquals("xsdReceive.WarehousePK", docket.WD_WW_Whs.ToString(), xsdReceive.WarehousePK, xsdReceive.WarehousePKSpecified);
			AssertEquals("xsdReceive.StagingAreaPK", docket.WD_WL_CrossDock.ToString(), xsdReceive.StagingAreaPK, xsdReceive.StagingAreaPKSpecified);
			AssertEquals("xsdReceive.ExWhsJobGuid", docket.WD_ExWhsJobGuid.ToString(), xsdReceive.ExWhsJobGuid, xsdReceive.ExWhsJobGuidSpecified);

			AssertEquals("xsdReceive.DocketID", docket.WD_DocketID, xsdReceive.DocketID, xsdReceive.DocketIDSpecified);
			AssertEquals("xsdReceive.ExternalReference", docket.WD_ExternalReference, xsdReceive.ExternalReference, xsdReceive.ExternalReferenceSpecified);
			AssertEquals("xsdReceive.DocketType", DocketType.Codes.Receive, xsdReceive.DocketType, xsdReceive.DocketTypeSpecified);
			if (docket.WD_DocketType == DocketType.Codes.Receive)
			{
				AssertEquals("xsdReceive.DocketSubType", docket.WD_DocketSubType, xsdReceive.DocketSubType, xsdReceive.DocketSubTypeSpecified);
			}
			else
			{
				AssertEquals("xsdReceive.DocketSubType", ReceiveType.Codes.Receipt, xsdReceive.DocketSubType, xsdReceive.DocketSubTypeSpecified);
			}
			AssertEquals("xsdReceive.TotalWeightUnit", docket.WD_TotalWeightUnit, xsdReceive.TotalWeightUnit, xsdReceive.TotalWeightUnitSpecified);
			AssertEquals("xsdReceive.TotalCubicUnit", docket.WD_TotalCubicUnit, xsdReceive.TotalCubicUnit, xsdReceive.TotalCubicUnitSpecified);
			AssertEquals("xsdReceive.DocketStatus", docket.WD_DocketStatus, xsdReceive.DocketStatus, xsdReceive.DocketStatusSpecified);
			AssertEquals("xsdReceive.PickOption", docket.WD_PickOption, xsdReceive.PickOption, xsdReceive.PickOptionSpecified);
			AssertEquals("xsdReceive.CustomerReference", docket.WD_CustomerReference, xsdReceive.CustomerReference, xsdReceive.CustomerReferenceSpecified);
			AssertEquals("xsdReceive.TransportReference", docket.WD_TransportReference, xsdReceive.TransportReference, xsdReceive.TransportReferenceSpecified);
			AssertEquals("xsdReceive.DropMode", docket.WD_DropMode, xsdReceive.DropMode, xsdReceive.DropModeSpecified);
			AssertEquals("xsdReceive.CODPayMethod", docket.WD_CODPayMethod, xsdReceive.CODPayMethod, xsdReceive.CODPayMethodSpecified);
			AssertEquals("xsdReceive.INCO", docket.WD_INCO, xsdReceive.INCO, xsdReceive.INCOSpecified);
			AssertEquals("xsdReceive.F3_NKTotalPackType", docket.WD_F3_NKTotalPackType, xsdReceive.F3_NKTotalPackType, xsdReceive.F3_NKTotalPackTypeSpecified);
			AssertEquals("xsdReceive.PL_NKCarrierServiceLevel", docket.WD_PL_NKCarrierServiceLevel, xsdReceive.PL_NKCarrierServiceLevel, xsdReceive.PL_NKCarrierServiceLevelSpecified);
			AssertEquals("xsdReceive.RS_NKServiceLevel", docket.WD_RS_NKServiceLevel, xsdReceive.RS_NKServiceLevel, xsdReceive.RS_NKServiceLevelSpecified);
			AssertEquals("xsdReceive.CustomAttrib1", docket.WD_CustomAttrib1, xsdReceive.CustomAttrib1, xsdReceive.CustomAttrib1Specified);
			AssertEquals("xsdReceive.CustomAttrib2", docket.WD_CustomAttrib2, xsdReceive.CustomAttrib2, xsdReceive.CustomAttrib2Specified);
			AssertEquals("xsdReceive.CustomAttrib3", docket.WD_CustomAttrib3, xsdReceive.CustomAttrib3, xsdReceive.CustomAttrib3Specified);
			AssertEquals("xsdReceive.CustomAttrib4", docket.WD_CustomAttrib4, xsdReceive.CustomAttrib4, xsdReceive.CustomAttrib4Specified);
			AssertEquals("xsdReceive.CustomAttrib5", docket.WD_CustomAttrib5, xsdReceive.CustomAttrib5, xsdReceive.CustomAttrib5Specified);
			AssertEquals("xsdReceive.TotalUnits", docket.WD_TotalUnits, xsdReceive.TotalUnits, xsdReceive.TotalUnitsSpecified);
			AssertEquals("xsdReceive.UnitsSent", docket.WD_UnitsSent, xsdReceive.UnitsSent, xsdReceive.UnitsSentSpecified);
			AssertEquals("xsdReceive.CubicSent", docket.WD_CubicSent, xsdReceive.CubicSent, xsdReceive.CubicSentSpecified);
			AssertEquals("xsdReceive.LocalCartInsuranceCost", docket.WD_LocalCartInsuranceCost, xsdReceive.LocalCartInsuranceCost, xsdReceive.LocalCartInsuranceCostSpecified);
			AssertEquals("xsdReceive.ShipperCODAmount", docket.WD_ShipperCODAmount, xsdReceive.ShipperCODAmount, xsdReceive.ShipperCODAmountSpecified);
			AssertEquals("xsdReceive.WeightSent", docket.WD_WeightSent, xsdReceive.WeightSent, xsdReceive.WeightSentSpecified);
			AssertEquals("xsdReceive.WeightSentUserEntered", docket.WD_WeightSentUserEntered, xsdReceive.WeightSentUserEntered, xsdReceive.WeightSentUserEnteredSpecified);
			AssertEquals("xsdReceive.CustomDecimal1", docket.WD_CustomDecimal1, xsdReceive.CustomDecimal1, xsdReceive.CustomDecimal1Specified);
			AssertEquals("xsdReceive.CustomDecimal2", docket.WD_CustomDecimal2, xsdReceive.CustomDecimal2, xsdReceive.CustomDecimal2Specified);
			AssertEquals("xsdReceive.CustomDecimal3", docket.WD_CustomDecimal3, xsdReceive.CustomDecimal3, xsdReceive.CustomDecimal3Specified);
			AssertEquals("xsdReceive.CustomDecimal4", docket.WD_CustomDecimal4, xsdReceive.CustomDecimal4, xsdReceive.CustomDecimal4Specified);
			AssertEquals("xsdReceive.CustomDecimal5", docket.WD_CustomDecimal5, xsdReceive.CustomDecimal5, xsdReceive.CustomDecimal5Specified);
			AssertEquals("xsdReceive.PackagesSent", docket.WD_PackagesSent, xsdReceive.PackagesSent, xsdReceive.PackagesSentSpecified);
			AssertEquals("xsdReceive.TotalPallets", docket.WD_TotalPallets, xsdReceive.TotalPallets, xsdReceive.TotalPalletsSpecified);
			AssertEquals("xsdReceive.PalletsSent", docket.WD_PalletsSent, xsdReceive.PalletsSent, xsdReceive.PalletsSentSpecified);
			AssertEquals("xsdReceive.ExternalReferenceSplit", docket.WD_ExternalReferenceSplit, xsdReceive.ExternalReferenceSplit, xsdReceive.ExternalReferenceSplitSpecified);
			AssertEquals("xsdReceive.WeightVolSetFromImport", docket.WD_WeightVolSetFromImport, xsdReceive.WeightVolSetFromImport, xsdReceive.WeightVolSetFromImportSpecified);
			AssertEquals("xsdReceive.AddPalletWeightToOrder", docket.WD_AddPalletWeightToOrder, xsdReceive.AddPalletWeightToOrder, xsdReceive.AddPalletWeightToOrderSpecified);
			AssertEquals("xsdReceive.AutoFinaliseBOMIntoInventory", docket.WD_AutoFinaliseBOMIntoInventory, xsdReceive.AutoFinaliseBOMIntoInventory, xsdReceive.AutoFinaliseBOMIntoInventorySpecified);
			AssertEquals("xsdReceive.CustomFlag1", docket.WD_CustomFlag1, xsdReceive.CustomFlag1, xsdReceive.CustomFlag1Specified);
			AssertEquals("xsdReceive.CustomFlag2", docket.WD_CustomFlag2, xsdReceive.CustomFlag2, xsdReceive.CustomFlag2Specified);
			AssertEquals("xsdReceive.CustomFlag3", docket.WD_CustomFlag3, xsdReceive.CustomFlag3, xsdReceive.CustomFlag3Specified);
			AssertEquals("xsdReceive.CustomFlag4", docket.WD_CustomFlag4, xsdReceive.CustomFlag4, xsdReceive.CustomFlag4Specified);
			AssertEquals("xsdReceive.CustomFlag5", docket.WD_CustomFlag5, xsdReceive.CustomFlag5, xsdReceive.CustomFlag5Specified);
			AssertEquals("xsdReceive.BookingDate", docket.WD_BookingDate, xsdReceive.BookingDateTimeOffset, xsdReceive.BookingDateTimeOffsetSpecified);
			AssertEquals("xsdReceive.ArrivalDate", docket.WD_ArrivalDate, xsdReceive.ArrivalDateTimeOffset, xsdReceive.ArrivalDateTimeOffsetSpecified);
			AssertEquals("xsdReceive.FinalisedDate", docket.WD_FinalisedDate, xsdReceive.FinalisedDateTimeOffset, xsdReceive.FinalisedDateTimeOffsetSpecified);
			AssertEquals("xsdReceive.RequiredDate", docket.WD_RequiredDate, xsdReceive.RequiredDateTimeOffset, xsdReceive.RequiredDateTimeOffsetSpecified);
			AssertEquals("xsdReceive.ETA", docket.WD_ETA, xsdReceive.ETADateTimeOffset, xsdReceive.ETADateTimeOffsetSpecified);
			AssertEquals("xsdReceive.ETD", docket.WD_ETD, xsdReceive.ETDDateTimeOffset, xsdReceive.ETDDateTimeOffsetSpecified);
			AssertEquals("xsdReceive.CustomDate1", docket.WD_CustomDate1, xsdReceive.CustomDate1, xsdReceive.CustomDate1Specified);
			AssertEquals("xsdReceive.CustomDate2", docket.WD_CustomDate2, xsdReceive.CustomDate2, xsdReceive.CustomDate2Specified);
		}

		#endregion

		#region TestExport_Receive_PartialExport

		public void TestExport_Receive_PartialExport()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory_FullyInStock = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory_PartiallyPicked = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			receive.RunPreSaveValidation(); // to generate ReceiveLines.

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part2, 6m);
			Helper.CreateWhsOrderLine(order1, part3, 10m);
			Factory.Save();

			var pick1 = Helper.CreatePickByAttachingOrders(order1);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			Helper.CreateReservePickLine(order2Line, inventory_FullyInStock, 2m);

			Helper.CreatePickByAttachingOrders(order2);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Only Receive Lines that still have stock should be exported.", 2, xsdReceive.ReceiveLines.Count);
			AssertContainsInventory(xsdReceive.ReceiveLines, inventory_PartiallyPicked, 4m);
			AssertContainsInventory(xsdReceive.ReceiveLines, inventory_FullyInStock, 10m);
		}

		void AssertContainsInventory(SystemMergeReceiveLineCollection xsdReceiveLineCollection, WhsInventoryView expectedInventory, ZDecimal expectedAvailableQuantity)
		{
			var xsdReceiveLine = xsdReceiveLineCollection.Cast<SystemMergeReceiveLine>().Single(l => l.PK == expectedInventory.WI_WE_InDocketLine.ToString());
			var xsdInventory = xsdReceiveLine.Inventories[0];
			AssertEquals("Each receive Line should have only 1 inventory attached to it.", 1, xsdReceiveLine.Inventories.Count);

			AssertEquals("xsdReceiveLine.Units", expectedAvailableQuantity, xsdReceiveLine.Units);
			AssertEquals("xsdInventory.TotalUnits", expectedAvailableQuantity, xsdInventory.TotalUnits);
			AssertEquals("xsdInventory.InDocketLineUnits", expectedAvailableQuantity, xsdInventory.InDocketLineUnits);
			AssertEquals("xsdInventory.CommittedUnits", 0m, xsdInventory.CommittedUnits);
		}

		#endregion

		#region TestExport_Adjustment

		public void TestExport_Adjustment()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			adjustment.WD_DocketID = "W00000123";
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 1m, "A-1", "", today.ToZDateTime().ToOffset());
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2.PK, 1m, "A-2", "", today.ToZDateTime().ToOffset().AddDays(-1));
			adjustmentLine2.WE_PartAttrib1 = "PA1";
			adjustmentLine2.WE_PartAttrib2 = "PA2";
			adjustmentLine2.WE_PartAttrib3 = "PA3";
			adjustmentLine2.WE_SerialNumber = "SN";
			adjustmentLine2.WE_ExpiryDate = today.AddDays(50);
			adjustmentLine2.WE_PackingDate = today.AddDays(-5);
			adjustmentLine2.WE_PalletID = "PALLET123";
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);

			// adjustment finalise and create inventory in 2nd factory.
			Factory.Save();
			adjustmentLine1.Inventory.Load();
			adjustmentLine2.Inventory.Load();

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(adjustment, new ValueObjectExportContext(Notify));
			AssertExportDocketData(adjustment, xsdReceive);
			AssertEquals("1 Receive Line should be created from each Adjustment Line.", 2, xsdReceive.ReceiveLines.Count);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, adjustmentLine1);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, adjustmentLine2);
			AssertContainsInventory(xsdReceive.ReceiveLines, adjustmentLine1, 1m);
			AssertContainsInventory(xsdReceive.ReceiveLines, adjustmentLine2, 1m);
		}

		#endregion

		#region TestExport_Transfers_Inner

		public void TestExport_Transfers_Inner()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, locations[0]);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, locations[0], today.AddDays(50), today.AddDays(-5), "PA1", "PA2", "PA3", "BEK");
			inv.WI_SerialNumber = "SN1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save(); // Transfer uses ZDBOnlyQuery.

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_DocketID = "W00000123";
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, locations[0], locations[1]); // will be split into 2 lines under the hood.
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 1m, locations[0], locations[2]);
			transferLine2.WE_PartAttrib1 = "PA1";
			transferLine2.WE_PartAttrib2 = "PA2";
			transferLine2.WE_PartAttrib3 = "PA3";
			transferLine2.WE_SerialNumber = "SN1";
			transferLine2.WE_ExpiryDate = today.AddDays(50);
			transferLine2.WE_PackingDate = today.AddDays(-5);
			transferLine2.WE_BondedEntryKey = "BEK";
			transferLine2.WE_PalletID = "TRANSFER_PALLET";
			transferLine2.WE_LineComment = "COMMENT";
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(transfer, new ValueObjectExportContext(Notify));
			AssertExportDocketData(transfer, xsdReceive);
			AssertEquals("Should export 3 Transfer Lines.", 3, xsdReceive.ReceiveLines.Count);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, transferLine1);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, transferLine1.MatchingLines[0]);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, transferLine2);
			AssertContainsInventory(xsdReceive.ReceiveLines, transferLine1, 3m);
			AssertContainsInventory(xsdReceive.ReceiveLines, transferLine1.MatchingLines[0], 3m);
			AssertContainsInventory(xsdReceive.ReceiveLines, transferLine2, 1m);
		}

		#endregion

		#region	TestExport_Transfers_InterSource

		public void TestExport_Transfers_InterSource()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m);

			Factory.Save(); // Transfer uses ZDBOnlyQuery.

			var sourceTransfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var sourceTransferLine = Helper.CreateWhsTransferLine(sourceTransfer, data.Part1, 2m, "A", whs2.PK, "B");
			sourceTransferLine.WE_PalletID = "PLT-1";
			sourceTransfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(sourceTransfer);

			// transfer finalise and create inventory in 2nd factory.
			Factory.Save();
			sourceTransferLine.Inventory.Load();
			AssertEquals("Precondition - there should be no Inventory line attached to Source Transfer.", 0, sourceTransferLine.Inventory.Count);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_DocketSubType, TransferType.Codes.InterWhsDest);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var destTransfer = Factory.LoadTop1<WhsDocket>(query);
			AssertEquals("Precondition - ensure that Destination Transfer have inventory.", 1, destTransfer.Lines[0].Inventory.Count);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(destTransfer, new ValueObjectExportContext(Notify));
			AssertExportDocketData(destTransfer, xsdReceive);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, destTransfer.Lines[0]);
			AssertContainsInventory(xsdReceive.ReceiveLines, destTransfer.Lines[0], 2m);
		}

		#endregion

		#region	TestExport_Transfers_InterDest

		public void TestExport_Transfers_InterDest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m);

			Factory.Save(); // Transfer uses ZDBOnlyQuery.

			var destTransfer = Helper.CreateWhsTransfer(data.Org1, whs2, "TR1", Notify, TransferType.Codes.InterWhsDest);
			var destTransferLine = Helper.CreateWhsTransferLine(destTransfer, data.Part1, 2m, "A", whs1.PK, "B");
			destTransferLine.WE_PalletID = "PLT-1";
			destTransfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(destTransfer);

			// transfer finalise and create inventory in 2nd factory.
			Factory.Save();
			destTransferLine.Inventory.Load();
			AssertEquals("Precondition - ensure that Destination Transfer have inventory.", 1, destTransferLine.Inventory.Count);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(destTransfer, new ValueObjectExportContext(Notify));
			AssertExportDocketData(destTransfer, xsdReceive);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, destTransfer.Lines[0]);
			AssertContainsInventory(xsdReceive.ReceiveLines, destTransfer.Lines[0], 2m);
		}

		#endregion

		#region TestExport_DocketReferences

		public void TestExport_DocketReferences()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var reference1 = CreateDocketReference(receive, "OTH", "TEST REFERENCE");
			var reference2 = CreateDocketReference(receive, "CAN", "CUSTOMS NUMBER");

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Should export 2 DocketReference.", 2, xsdReceive.DocketReferences.Count);
			AssertContainsDocketReference(xsdReceive.DocketReferences, reference1);
			AssertContainsDocketReference(xsdReceive.DocketReferences, reference2);
		}

		WhsDocketReference CreateDocketReference(WhsReceive receive, ZString type, ZString reference)
		{
			var docketReference = receive.References.AddNew();
			docketReference.WX_RefType = type;
			docketReference.WX_Reference = reference;

			return docketReference;
		}

		void AssertContainsDocketReference(SystemMergeDocketReferenceCollection xsdDocketReferenceCollection, WhsDocketReference expectedReference)
		{
			var reference = xsdDocketReferenceCollection.Cast<SystemMergeDocketReference>().Single(r => r.PK == expectedReference.PK.ToString());
			AssertEquals("DocketReference.WX_RefType", expectedReference.WX_RefType, reference.RefType);
			AssertEquals("DocketReference.WX_Reference", expectedReference.WX_Reference, reference.Reference);
		}

		#endregion

		#region TestExport_ReceiveLine

		public void TestExport_ReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var today = ZDate.Today;
			var todayOffset = new ZDateTimeOffset(today);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT-2", today.AddDays(1), today.AddDays(2), "PA1", "PA2", "PA3", "BEK");
			inv.WI_SerialNumber = "SN";

			receive.RunPreSaveValidation(); // to generate Receive lines from Inventory.

			var receiveLine1 = receive.Lines.Single(dl => dl.WE_OP == data.Part1.PK);
			var receiveLine2 = receive.Lines.Single(dl => dl.WE_OP == data.Part2.PK);
			receiveLine2.WE_LineComment = "COMMENT";
			receiveLine2.WE_CustomAttrib1 = "CUSTOM ATTR1";
			receiveLine2.WE_CustomAttrib2 = "CUSTOM ATTR2";
			receiveLine2.WE_CustomAttrib3 = "CUSTOM ATTR3";
			receiveLine2.WE_DocketLineStatus = "FIN";
			receiveLine2.WE_RX_NKUnitPriceCurrency = "CCC";
			receiveLine2.WE_F3_NKPackType = "BAG";
			receiveLine2.WE_ReceiveCrossDockOrderNo = "CROSSDOCKORDERNO";
			receiveLine2.WE_CustomAttrib4 = "CUSTOM ATTR4";
			receiveLine2.WE_CustomAttrib5 = "CUSTOM ATTR5";
			receiveLine2.WE_CustomAttrib6 = "CUSTOM ATTR6";
			receiveLine2.WE_CustomTextBlob1 = "CUSTOM TEXT BLOB";

			receiveLine2.WE_AdjustmentArrivalDate = todayOffset.AddDays(1);
			receiveLine2.WE_FinalisedDate = todayOffset.AddDays(2);
			receiveLine2.WE_RequiredByDate = todayOffset.AddDays(3);
			receiveLine2.WE_CustomDate1 = today.AddDays(4);
			receiveLine2.WE_CustomDate2 = today.AddDays(5);
			receiveLine2.WE_CustomDate3 = today.AddDays(6);
			receiveLine2.WE_CustomDate4 = today.AddDays(7);
			receiveLine2.WE_CustomDate5 = today.AddDays(8);

			receiveLine2.WE_ClientOrderedUnits = 4m;
			receiveLine2.WE_RecommendedUnitPrice = 5m;
			receiveLine2.WE_UnitDiscountPercent = 6m;
			receiveLine2.WE_UnitDiscountAmount = 7m;
			receiveLine2.WE_UnitPriceAfterDiscount = 8m;
			receiveLine2.WE_ExtendedLinePrice = 9m;
			receiveLine2.WE_CustomDecimal1 = 10m;
			receiveLine2.WE_CustomDecimal2 = 11m;
			receiveLine2.WE_CustomDecimal3 = 12m;
			receiveLine2.WE_CustomDecimal4 = 13m;
			receiveLine2.WE_CustomDecimal5 = 14m;

			receiveLine2.WE_LineNo = 15;
			receiveLine2.WE_SubLineNo = 16;

			receiveLine2.WE_CustomFlag1 = true;
			receiveLine2.WE_CustomFlag2 = true;
			receiveLine2.WE_CustomFlag3 = true;
			receiveLine2.WE_CustomFlag4 = true;
			receiveLine2.WE_CustomFlag5 = true;

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Receive have 2 lines, so 2 lines should be exported", 2, xsdReceive.ReceiveLines.Count);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, receiveLine1);
			AssertContainsDocketLine(xsdReceive.ReceiveLines, receiveLine2);
		}

		void AssertContainsDocketLine(SystemMergeReceiveLineCollection systemMergeReceiveLineCollection, WhsDocketLine expectedDocketLine)
		{
			var xsdReceiveLine = systemMergeReceiveLineCollection.Cast<SystemMergeReceiveLine>().Single(l => l.PK == expectedDocketLine.PK.ToString());
			AssertEquals("xsdReceiveLine.ProductPK", expectedDocketLine.WE_OP.ToString(), xsdReceiveLine.ProductPK, xsdReceiveLine.ProductPKSpecified);
			AssertEquals("xsdReceiveLine.LocationPK", expectedDocketLine.WE_WL.ToString(), xsdReceiveLine.LocationPK, xsdReceiveLine.LocationPKSpecified);
			AssertEquals("xsdReceiveLine.PalletID", expectedDocketLine.WE_PalletID, xsdReceiveLine.PalletID, xsdReceiveLine.PalletIDSpecified);
			AssertEquals("xsdReceiveLine.PartAttrib1", expectedDocketLine.WE_PartAttrib1, xsdReceiveLine.PartAttrib1, xsdReceiveLine.PartAttrib1Specified);
			AssertEquals("xsdReceiveLine.PartAttrib2", expectedDocketLine.WE_PartAttrib2, xsdReceiveLine.PartAttrib2, xsdReceiveLine.PartAttrib2Specified);
			AssertEquals("xsdReceiveLine.PartAttrib3", expectedDocketLine.WE_PartAttrib3, xsdReceiveLine.PartAttrib3, xsdReceiveLine.PartAttrib3Specified);
			AssertEquals("xsdReceiveLine.SerialNumber", expectedDocketLine.WE_SerialNumber, xsdReceiveLine.SerialNumber, xsdReceiveLine.SerialNumberSpecified);
			AssertEquals("xsdReceiveLine.BondedEntryKey", expectedDocketLine.WE_BondedEntryKey, xsdReceiveLine.BondedEntryKey, xsdReceiveLine.BondedEntryKeySpecified);
			AssertEquals("xsdReceiveLine.LineComment", expectedDocketLine.WE_LineComment, xsdReceiveLine.LineComment, xsdReceiveLine.LineCommentSpecified);
			AssertEquals("xsdReceiveLine.CustomAttrib1", expectedDocketLine.WE_CustomAttrib1, xsdReceiveLine.CustomAttrib1, xsdReceiveLine.CustomAttrib1Specified);
			AssertEquals("xsdReceiveLine.CustomAttrib2", expectedDocketLine.WE_CustomAttrib2, xsdReceiveLine.CustomAttrib2, xsdReceiveLine.CustomAttrib2Specified);
			AssertEquals("xsdReceiveLine.CustomAttrib3", expectedDocketLine.WE_CustomAttrib3, xsdReceiveLine.CustomAttrib3, xsdReceiveLine.CustomAttrib3Specified);
			AssertEquals("xsdReceiveLine.DocketLineStatus", expectedDocketLine.WE_DocketLineStatus, xsdReceiveLine.DocketLineStatus, xsdReceiveLine.DocketLineStatusSpecified);
			AssertEquals("xsdReceiveLine.RX_NKUnitPriceCurrency", expectedDocketLine.WE_RX_NKUnitPriceCurrency, xsdReceiveLine.RX_NKUnitPriceCurrency, xsdReceiveLine.RX_NKUnitPriceCurrencySpecified);
			AssertEquals("xsdReceiveLine.F3_NKPackType", expectedDocketLine.WE_F3_NKPackType, xsdReceiveLine.F3_NKPackType, xsdReceiveLine.F3_NKPackTypeSpecified);
			AssertEquals("xsdReceiveLine.ReceiveCrossDockOrderNo", expectedDocketLine.WE_ReceiveCrossDockOrderNo, xsdReceiveLine.ReceiveCrossDockOrderNo, xsdReceiveLine.ReceiveCrossDockOrderNoSpecified);
			AssertEquals("xsdReceiveLine.CustomAttrib4", expectedDocketLine.WE_CustomAttrib4, xsdReceiveLine.CustomAttrib4, xsdReceiveLine.CustomAttrib4Specified);
			AssertEquals("xsdReceiveLine.CustomAttrib5", expectedDocketLine.WE_CustomAttrib5, xsdReceiveLine.CustomAttrib5, xsdReceiveLine.CustomAttrib5Specified);
			AssertEquals("xsdReceiveLine.CustomAttrib6", expectedDocketLine.WE_CustomAttrib6, xsdReceiveLine.CustomAttrib6, xsdReceiveLine.CustomAttrib6Specified);
			AssertEquals("xsdReceiveLine.CustomTextBlob1", expectedDocketLine.WE_CustomTextBlob1, xsdReceiveLine.CustomTextBlob1, xsdReceiveLine.CustomTextBlob1Specified);
			AssertEquals("xsdReceiveLine.ExpiryDate", expectedDocketLine.WE_ExpiryDate, xsdReceiveLine.ExpiryDate, xsdReceiveLine.ExpiryDateSpecified);
			AssertEquals("xsdReceiveLine.PackingDate", expectedDocketLine.WE_PackingDate, xsdReceiveLine.PackingDate, xsdReceiveLine.PackingDateSpecified);
			AssertEquals("xsdReceiveLine.AdjustmentArrivalDate", expectedDocketLine.WE_AdjustmentArrivalDate, xsdReceiveLine.AdjustmentArrivalDateTimeOffset, xsdReceiveLine.AdjustmentArrivalDateTimeOffsetSpecified);
			AssertEquals("xsdReceiveLine.FinalisedDate", expectedDocketLine.WE_FinalisedDate, xsdReceiveLine.FinalisedDateTimeOffset, xsdReceiveLine.FinalisedDateTimeOffsetSpecified);
			AssertEquals("xsdReceiveLine.RequiredByDate", expectedDocketLine.WE_RequiredByDate, xsdReceiveLine.RequiredByDateTimeOffset, xsdReceiveLine.RequiredByDateTimeOffsetSpecified);
			AssertEquals("xsdReceiveLine.CustomDate1", expectedDocketLine.WE_CustomDate1, xsdReceiveLine.CustomDate1, xsdReceiveLine.CustomDate1Specified);
			AssertEquals("xsdReceiveLine.CustomDate2", expectedDocketLine.WE_CustomDate2, xsdReceiveLine.CustomDate2, xsdReceiveLine.CustomDate2Specified);
			AssertEquals("xsdReceiveLine.CustomDate3", expectedDocketLine.WE_CustomDate3, xsdReceiveLine.CustomDate3, xsdReceiveLine.CustomDate3Specified);
			AssertEquals("xsdReceiveLine.CustomDate4", expectedDocketLine.WE_CustomDate4, xsdReceiveLine.CustomDate4, xsdReceiveLine.CustomDate4Specified);
			AssertEquals("xsdReceiveLine.CustomDate5", expectedDocketLine.WE_CustomDate5, xsdReceiveLine.CustomDate5, xsdReceiveLine.CustomDate5Specified);
			AssertEquals("xsdReceiveLine.Units", expectedDocketLine.WE_TransactionQuantity, xsdReceiveLine.Units, xsdReceiveLine.UnitsSpecified);
			AssertEquals("xsdReceiveLine.ClientOrderedUnits", expectedDocketLine.WE_ClientOrderedUnits, xsdReceiveLine.ClientOrderedUnits, xsdReceiveLine.ClientOrderedUnitsSpecified);
			AssertEquals("xsdReceiveLine.RecommendedUnitPrice", expectedDocketLine.WE_RecommendedUnitPrice, xsdReceiveLine.RecommendedUnitPrice, xsdReceiveLine.RecommendedUnitPriceSpecified);
			AssertEquals("xsdReceiveLine.UnitDiscountPercent", expectedDocketLine.WE_UnitDiscountPercent, xsdReceiveLine.UnitDiscountPercent, xsdReceiveLine.UnitDiscountPercentSpecified);
			AssertEquals("xsdReceiveLine.UnitDiscountAmount", expectedDocketLine.WE_UnitDiscountAmount, xsdReceiveLine.UnitDiscountAmount, xsdReceiveLine.UnitDiscountAmountSpecified);
			AssertEquals("xsdReceiveLine.UnitPriceAfterDiscount", expectedDocketLine.WE_UnitPriceAfterDiscount, xsdReceiveLine.UnitPriceAfterDiscount, xsdReceiveLine.UnitPriceAfterDiscountSpecified);
			AssertEquals("xsdReceiveLine.ExtendedLinePrice", expectedDocketLine.WE_ExtendedLinePrice, xsdReceiveLine.ExtendedLinePrice, xsdReceiveLine.ExtendedLinePriceSpecified);
			AssertEquals("xsdReceiveLine.CustomDecimal1", expectedDocketLine.WE_CustomDecimal1, xsdReceiveLine.CustomDecimal1, xsdReceiveLine.CustomDecimal1Specified);
			AssertEquals("xsdReceiveLine.CustomDecimal2", expectedDocketLine.WE_CustomDecimal2, xsdReceiveLine.CustomDecimal2, xsdReceiveLine.CustomDecimal2Specified);
			AssertEquals("xsdReceiveLine.CustomDecimal3", expectedDocketLine.WE_CustomDecimal3, xsdReceiveLine.CustomDecimal3, xsdReceiveLine.CustomDecimal3Specified);
			AssertEquals("xsdReceiveLine.CustomDecimal4", expectedDocketLine.WE_CustomDecimal4, xsdReceiveLine.CustomDecimal4, xsdReceiveLine.CustomDecimal4Specified);
			AssertEquals("xsdReceiveLine.CustomDecimal5", expectedDocketLine.WE_CustomDecimal5, xsdReceiveLine.CustomDecimal5, xsdReceiveLine.CustomDecimal5Specified);
			AssertEquals("xsdReceiveLine.LineNo", expectedDocketLine.WE_LineNo, xsdReceiveLine.LineNo, xsdReceiveLine.LineNoSpecified);
			AssertEquals("xsdReceiveLine.SubLineNo", expectedDocketLine.WE_SubLineNo, xsdReceiveLine.SubLineNo, xsdReceiveLine.SubLineNoSpecified);
			AssertEquals("xsdReceiveLine.CustomFlag1", expectedDocketLine.WE_CustomFlag1, xsdReceiveLine.CustomFlag1, xsdReceiveLine.CustomFlag1Specified);
			AssertEquals("xsdReceiveLine.CustomFlag2", expectedDocketLine.WE_CustomFlag2, xsdReceiveLine.CustomFlag2, xsdReceiveLine.CustomFlag2Specified);
			AssertEquals("xsdReceiveLine.CustomFlag3", expectedDocketLine.WE_CustomFlag3, xsdReceiveLine.CustomFlag3, xsdReceiveLine.CustomFlag3Specified);
			AssertEquals("xsdReceiveLine.CustomFlag4", expectedDocketLine.WE_CustomFlag4, xsdReceiveLine.CustomFlag4, xsdReceiveLine.CustomFlag4Specified);
			AssertEquals("xsdReceiveLine.CustomFlag5", expectedDocketLine.WE_CustomFlag5, xsdReceiveLine.CustomFlag5, xsdReceiveLine.CustomFlag5Specified);
		}

		#endregion

		#region TestExport_BondedWarehouseAttribute

		public void TestExport_BondedWarehouseAttribute()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var manufacturer = Helper.CreateClient("MANU", "Manufacturer");
			var manufacturerAddress = manufacturer.MainAddress;

			var customData = receive.Inventory[0].CustomsData;
			customData.WB_DeclarationReference = "DECLARATION";
			customData.WB_EntryKey = "KEY123";
			customData.WB_CustomsUnitOfQty = "KG";
			customData.WB_BondedWhsUnitOfQty = "BAG";
			customData.WB_RN_NKCountryOfOrigin = "AU";
			customData.WB_AddInfo = "ADDITIONAL INFO";
			customData.WB_RX_NKTILVCurrency = "USD";
			customData.WB_IsActive = true;
			customData.WB_EntryLineNo = 1;
			customData.WB_CustomsQty = 2m;
			customData.WB_BondedWhsQty = 3m;
			customData.WB_ValueForDuty = 4m;
			customData.WB_TILV = 5m;
			customData.WB_EntryDate = today;
			customData.WB_CustomsSecondQuantity = 22m;
			customData.WB_CustomsSecondUnitQty = "GRM";
			customData.WB_Tariff = "TRF";
			customData.WB_PrimaryPreference = "P0P";

			customData.WB_CustomsThirdQuantity = 12m;
			customData.WB_CustomsThirdUnitQty = "GRM";
			customData.WB_OA_ManufacturerAddress = manufacturerAddress.PK;

			receive.RunPreSaveValidation(); // to generate Receive lines from Inventory.

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			customData.WB_ZoneStatus = "Z";
			customData.WB_IsFromAnotherFTZWhs = true;
			customData.WB_OutwardType = "TOF";

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Receive should be exported as customs receive.", ReceiveType.Codes.Customs, xsdReceive.DocketSubType);
			AssertEquals("Receive have 1 line, so 1 line should be exported", 1, xsdReceive.ReceiveLines.Count);
			AssertContainsCustomData(xsdReceive.ReceiveLines, customData);
		}

		void AssertContainsCustomData(SystemMergeReceiveLineCollection xsdReceiveLineCollection, WhsBondedWarehouseAttribute expectedCustomData)
		{
			var xsdCustomData = xsdReceiveLineCollection.Cast<SystemMergeReceiveLine>().Single(l => l.PK == expectedCustomData.WB_ParentID.ToString()).CustomsData;
			AssertEquals("xsdCustomData.PK", expectedCustomData.PK.ToString(), xsdCustomData.PK, xsdCustomData.PKSpecified);
			AssertEquals("xsdCustomData.WB_InwardsEntry", expectedCustomData.WB_WB_InwardsEntry.ToString(), xsdCustomData.WB_InwardsEntry, xsdCustomData.WB_InwardsEntrySpecified);
			AssertEquals("xsdCustomData.ParentID", expectedCustomData.WB_ParentID.ToString(), xsdCustomData.ParentID, xsdCustomData.ParentIDSpecified);
			AssertEquals("xsdCustomData.DeclarationReference", expectedCustomData.WB_DeclarationReference, xsdCustomData.DeclarationReference, xsdCustomData.DeclarationReferenceSpecified);
			AssertEquals("xsdCustomData.EntryKey", expectedCustomData.WB_EntryKey, xsdCustomData.EntryKey, xsdCustomData.EntryKeySpecified);
			AssertEquals("xsdCustomData.CustomsUnitOfQty", expectedCustomData.WB_CustomsUnitOfQty, xsdCustomData.CustomsUnitOfQty, xsdCustomData.CustomsUnitOfQtySpecified);
			AssertEquals("xsdCustomData.BondedWhsUnitOfQty", expectedCustomData.WB_BondedWhsUnitOfQty, xsdCustomData.BondedWhsUnitOfQty, xsdCustomData.BondedWhsUnitOfQtySpecified);
			AssertEquals("xsdCustomData.RN_NKCountryOfOrigin", expectedCustomData.WB_RN_NKCountryOfOrigin, xsdCustomData.RN_NKCountryOfOrigin, xsdCustomData.RN_NKCountryOfOriginSpecified);
			AssertEquals("xsdCustomData.AddInfo", expectedCustomData.WB_AddInfo, xsdCustomData.AddInfo, xsdCustomData.AddInfoSpecified);
			AssertEquals("xsdCustomData.ParentTableCode", expectedCustomData.WB_ParentTableCode, xsdCustomData.ParentTableCode, xsdCustomData.ParentTableCodeSpecified);
			AssertEquals("xsdCustomData.RX_NKTILVCurrency", expectedCustomData.WB_RX_NKTILVCurrency, xsdCustomData.RX_NKTILVCurrency, xsdCustomData.RX_NKTILVCurrencySpecified);
			AssertEquals("xsdCustomData.IsActive", expectedCustomData.WB_IsActive, xsdCustomData.IsActive, xsdCustomData.IsActiveSpecified);
			AssertEquals("xsdCustomData.EntryLineNo", expectedCustomData.WB_EntryLineNo, xsdCustomData.EntryLineNo, xsdCustomData.EntryLineNoSpecified);
			AssertEquals("xsdCustomData.CustomsQty", expectedCustomData.WB_CustomsQty, xsdCustomData.CustomsQty, xsdCustomData.CustomsQtySpecified);
			AssertEquals("xsdCustomData.BondedWhsQty", expectedCustomData.WB_BondedWhsQty, xsdCustomData.BondedWhsQty, xsdCustomData.BondedWhsQtySpecified);
			AssertEquals("xsdCustomData.ValueForDuty", expectedCustomData.WB_ValueForDuty, xsdCustomData.ValueForDuty, xsdCustomData.ValueForDutySpecified);
			AssertEquals("xsdCustomData.TILV", expectedCustomData.WB_TILV, xsdCustomData.TILV, xsdCustomData.TILVSpecified);
			AssertEquals("xsdCustomData.EntryDate", expectedCustomData.WB_EntryDate, xsdCustomData.EntryDate, xsdCustomData.EntryDateSpecified);
			AssertEquals("xsdCustomData.CustomsSecondQuantity", expectedCustomData.WB_CustomsSecondQuantity, xsdCustomData.CustomsSecondQuantity, xsdCustomData.CustomsSecondQuantitySpecified);
			AssertEquals("xsdCustomData.CustomsSecondUnitQty", expectedCustomData.WB_CustomsSecondUnitQty, xsdCustomData.CustomsSecondUnitQty, xsdCustomData.CustomsSecondUnitQtySpecified);
			AssertEquals("xsdCustomData.Tariff", expectedCustomData.WB_Tariff, xsdCustomData.Tariff, xsdCustomData.TariffSpecified);
			AssertEquals("xsdCustomData.PrimaryPreference", expectedCustomData.WB_PrimaryPreference, xsdCustomData.PrimaryPreference, xsdCustomData.PrimaryPreferenceSpecified);
			AssertEquals("xsdCustomData.CustomsThirdQuantity", expectedCustomData.WB_CustomsThirdQuantity, xsdCustomData.CustomsThirdQuantity, xsdCustomData.CustomsThirdQuantitySpecified);
			AssertEquals("xsdCustomData.CustomsThirdUnitQty", expectedCustomData.WB_CustomsThirdUnitQty, xsdCustomData.CustomsThirdUnitQty, xsdCustomData.CustomsThirdUnitQtySpecified);
			AssertEquals("xsdCustomData.ManufacturerAddress", expectedCustomData.WB_OA_ManufacturerAddress.ToString(), xsdCustomData.ManufacturerAddress, xsdCustomData.ManufacturerAddressSpecified);
			AssertEquals("xsdCustomData.ZoneStatus", expectedCustomData.WB_ZoneStatus, xsdCustomData.ZoneStatus, xsdCustomData.ZoneStatusSpecified);
			AssertEquals("xsdCustomData.IsFromOtherFTZWarehouse", expectedCustomData.WB_IsFromAnotherFTZWhs, xsdCustomData.IsFromOtherFTZWarehouse, xsdCustomData.IsFromOtherFTZWarehouseSpecified);
			AssertEquals("xsdCustomData.OutwardType", expectedCustomData.WB_OutwardType, xsdCustomData.OutwardType, xsdCustomData.OutwardTypeSpecified);
		}

		#endregion

		#region TestExport_BondedWarehouseAttribute_PartialExport

		public void TestExport_BondedWarehouseAttribute_PartialExport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var row = Helper.CreateRow(data.Whs1, "RR1");
			Factory.Save();

			var bondedLocation = row.Locations[0];
			var area = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			bondedLocation.WLV_WA_PickingArea = area.PK;
			bondedLocation.WLV_WA_PutawayArea = area.PK;
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bondedLocation);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var manufacturer = Helper.CreateClient("MANU", "Manufacturer");
			var manufacturerAddress = manufacturer.MainAddress;

			var customData = receive.Inventory[0].CustomsData;
			customData.WB_DeclarationReference = "DECLARATION";
			customData.WB_EntryKey = "KEY123";
			customData.WB_EntryLineNo = 1;
			customData.WB_CustomsUnitOfQty = "KG";
			customData.WB_BondedWhsUnitOfQty = "BAG";
			customData.WB_RX_NKTILVCurrency = "USD";
			customData.WB_CustomsQty = 20m;
			customData.WB_ValueForDuty = 40m;
			customData.WB_TILV = 50m;
			customData.WB_CustomsSecondQuantity = 22m;
			customData.WB_CustomsSecondUnitQty = "GRM";
			customData.WB_Tariff = "TRF";
			customData.WB_PrimaryPreference = "P0P";
			customData.WB_CustomsThirdQuantity = 12m;
			customData.WB_CustomsThirdUnitQty = "GRM";
			customData.WB_OA_ManufacturerAddress = manufacturerAddress.PK;

			receive.RunPreSaveValidation(); // to generate Receive lines from Inventory.

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			order.Lines[0].WE_BondedEntryKey = "KEY123-1";
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				pick.CreatePick_OBSOLETE(order); // using OBSOLETE to simulate Bonded
			}
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - ensure pick is NOT finalised.", false, pick.IsFinalised);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Receive should be exported as customs receive.", ReceiveType.Codes.Customs, xsdReceive.DocketSubType);
			AssertEquals("Receive have 1 line, so 1 line should be exported", 1, xsdReceive.ReceiveLines.Count);
			AssertEquals("WB_CustomsQty should be updated proportionaly to stock remaining in warehouse.", 16m, xsdReceive.ReceiveLines[0].CustomsData.CustomsQty);
			AssertEquals("WB_BondedWhsQty should be updated proportionaly to stock remaining in warehouse.", 8m, xsdReceive.ReceiveLines[0].CustomsData.BondedWhsQty);
			AssertEquals("WB_ValueForDuty should be updated proportionaly to stock remaining in warehouse.", 32m, xsdReceive.ReceiveLines[0].CustomsData.ValueForDuty);
			AssertEquals("WB_TILV should be updated proportionaly to stock remaining in warehouse.", 40m, xsdReceive.ReceiveLines[0].CustomsData.TILV);
			AssertEquals("WB_CustomsSecondQuantity should be updated proportionaly to stock remaining in warehouse.", 22m, xsdReceive.ReceiveLines[0].CustomsData.CustomsSecondQuantity);
			AssertEquals("WB_CustomsSecondUnitQty should be updated proportionaly to stock remaining in warehouse.", "GRM", xsdReceive.ReceiveLines[0].CustomsData.CustomsSecondUnitQty);
			AssertEquals("WB_Tariff should be updated proportionaly to stock remaining in warehouse.", "TRF", xsdReceive.ReceiveLines[0].CustomsData.Tariff);
			AssertEquals("WB_PrimaryPreference should be updated proportionaly to stock remaining in warehouse.", "P0P", xsdReceive.ReceiveLines[0].CustomsData.PrimaryPreference);
			AssertEquals("WB_CustomsThirdQuantity should be updated proportionaly to stock remaining in warehouse.", 12m, xsdReceive.ReceiveLines[0].CustomsData.CustomsThirdQuantity);
			AssertEquals("WB_CustomsThirdUnitQty should be updated proportionaly to stock remaining in warehouse.", "GRM", xsdReceive.ReceiveLines[0].CustomsData.CustomsThirdUnitQty);
			AssertEquals("WB_OA_ManufacturerAddress should be updated proportionaly to stock remaining in warehouse.", manufacturerAddress.PK.ToString(), xsdReceive.ReceiveLines[0].CustomsData.ManufacturerAddress);
		}

		#endregion

		#region TestExport_Inventory

		public void TestExport_Inventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.DefaultLocation, "PLT-2", today.AddDays(1), today.AddDays(2), "PA1", "PA2", "PA3", "BEK");
			inventory2.WI_SerialNumber = "SN";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			inventory1.InDocketLine.HeldCodeChangeQuantity = 4m;
			inventory1.InDocketLine.HeldCodeToChangeTo = InventoryStatus.Codes.Held;

			inventory2.WI_InDocketLineUnits = 3m;
			inventory2.WI_TotalUnits = 1m;
			inventory2.WI_ArrivalDate = today.ToZDateTime().ToOffset().AddDays(3);
			inventory2.WI_IsOriginalReceiptLine = true;

			receive.RunPreSaveValidation(); // to generate WhsReceiveLines
			inventory1.InDocketLine.ChangeInventoryHeldCode(true); // simulate inventory split.
			var allReceiveLines = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK));
			var allInventories = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, receive.PK));
			AssertEquals("Receive should have 3 Receive Lines.", 3, allReceiveLines.Length);
			AssertEquals("Receive should have 3 Inventories.", 3, allInventories.Length);

			var receiveLine1 = receive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			var receiveLine2 = receive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			var receiveLine3 = allReceiveLines.Single(l => !l.WE_IsOriginalInventory);

			AssertNotNull(receiveLine3);
			AssertEquals(receiveLine3.WE_TransactionQuantity, 4m);
			AssertEquals(receiveLine3.Inventory[0].WI_InDocketLineUnits, 0m);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("New receive should have 3 receive lines.", 3, xsdReceive.ReceiveLines.Count);
			AssertContainsInventory(xsdReceive.ReceiveLines, receiveLine1, 6m);
			AssertContainsInventory(xsdReceive.ReceiveLines, receiveLine2, 1m);
			AssertContainsInventory(xsdReceive.ReceiveLines, receiveLine3, 4m);
		}

		void AssertContainsInventory(SystemMergeReceiveLineCollection systemMergeReceiveLineCollection, WhsDocketLine expectedDocketLine, ZDecimal expectedInDocketLineUnits)
		{
			var xsdReceiveLine = systemMergeReceiveLineCollection.Cast<SystemMergeReceiveLine>().Single(l => l.PK == expectedDocketLine.PK.ToString());
			AssertEquals("Each Inventory from docket line should be exported.", expectedDocketLine.Inventory.Count, xsdReceiveLine.Inventories.Count);
			foreach (WhsInventoryView inventory in expectedDocketLine.Inventory)
			{
				var xsdInventory = xsdReceiveLine.Inventories.Cast<SystemMergeInventory>().Single(i => i.PK == inventory.PK.ToString());
				AssertEquals("xsdInventory.InventoryStatus", inventory.WI_InventoryStatus, xsdInventory.InventoryStatus, xsdInventory.InventoryStatusSpecified);
				AssertEquals("xsdInventory.InDocketLineType", DocketType.Codes.Receive, xsdInventory.InDocketLineType, xsdInventory.InDocketLineTypeSpecified);
				AssertEquals("xsdInventory.F3_NKPackType", inventory.WI_F3_NKPackType, xsdInventory.F3_NKPackType, xsdInventory.F3_NKPackTypeSpecified);
				AssertEquals("xsdInventory.PalletID", inventory.WI_PalletID, xsdInventory.PalletID, xsdInventory.PalletIDSpecified);
				AssertEquals("xsdInventory.TotalUnits", inventory.WI_TotalUnits, xsdInventory.TotalUnits, xsdInventory.TotalUnitsSpecified);
				AssertEquals("xsdInventory.CommittedUnits", 0m, xsdInventory.CommittedUnits, xsdInventory.CommittedUnitsSpecified);
				AssertEquals("xsdInventory.InDocketLineUnits", expectedInDocketLineUnits, xsdInventory.InDocketLineUnits, xsdInventory.InDocketLineUnitsSpecified);
				AssertEquals("xsdInventory.ArrivalDate", inventory.WI_ArrivalDate, xsdInventory.ArrivalDateTimeOffset, xsdInventory.ArrivalDateTimeOffsetSpecified);
				AssertEquals("xsdInventory.IsOriginalReceiptLine", inventory.WI_IsOriginalReceiptLine, xsdInventory.IsOriginalReceiptLine, xsdInventory.IsOriginalReceiptLineSpecified);
			}
		}

		#endregion

		#endregion

		#region TestImport

		#region TestImport_Receive

		public void TestImport_Receive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save(); // to avoid need to import data into otherFactory.

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_ExWhsJobGuid = new ZGuid();
			receive.WD_WL_CrossDock = dockLocationA.PK;

			receive.WD_DocketID = "W00000123";
			receive.WD_TotalWeightUnit = "KG";
			receive.WD_TotalCubicUnit = "M3";
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			receive.WD_PickOption = WhsPickOption.Codes.Manual;
			receive.WD_CustomerReference = "CUSTOM REF";
			receive.WD_TransportReference = "TRANSPORT REF";
			receive.WD_DropMode = "ZZZ";
			receive.WD_CODPayMethod = "AAA";
			receive.WD_INCO = "BBB";
			receive.WD_F3_NKTotalPackType = "BOX";
			receive.WD_PL_NKCarrierServiceLevel = "D2D";
			receive.WD_RS_NKServiceLevel = "TSL";
			receive.WD_CustomAttrib1 = "CUSTOM 1";
			receive.WD_CustomAttrib2 = "CUSTOM 2";
			receive.WD_CustomAttrib3 = "CUSTOM 3";
			receive.WD_CustomAttrib4 = "CUSTOM 4";
			receive.WD_CustomAttrib5 = "CUSTOM 5";

			receive.WD_TotalUnits = 1m;
			receive.WD_UnitsSent = 4m;
			receive.WD_CubicSent = 6m;
			receive.WD_LocalCartInsuranceCost = 7m;
			receive.WD_ShipperCODAmount = 8m;
			receive.WD_WeightSent = 9m;
			receive.WD_WeightSentUserEntered = 10;
			receive.WD_CustomDecimal1 = 11m;
			receive.WD_CustomDecimal2 = 12m;
			receive.WD_CustomDecimal3 = 13m;
			receive.WD_CustomDecimal4 = 14m;
			receive.WD_CustomDecimal5 = 15m;

			receive.WD_PackagesSent = 16;

			receive.WD_TotalPallets = 17;
			receive.WD_PalletsSent = 19;

			receive.WD_ExternalReferenceSplit = 0;

			receive.WD_AddPalletWeightToOrder = true;
			receive.WD_AutoFinaliseBOMIntoInventory = true;
			receive.WD_CustomFlag1 = true;
			receive.WD_CustomFlag2 = true;
			receive.WD_CustomFlag3 = true;
			receive.WD_CustomFlag4 = true;
			receive.WD_CustomFlag5 = true;

			var today = ZDateTime.Today;
			var todayOffset = ZDateTimeOffset.Today;
			receive.WD_BookingDate = todayOffset.AddDays(1);
			receive.WD_ArrivalDate = todayOffset.AddDays(2);
			receive.WD_FinalisedDate = todayOffset.AddDays(3);
			receive.WD_RequiredDate = todayOffset.AddDays(4);
			receive.WD_ETA = todayOffset.AddDays(5);
			receive.WD_ETD = todayOffset.AddDays(6);
			receive.WD_CustomDate1 = today.AddDays(7);
			receive.WD_CustomDate2 = today.AddDays(8);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceiveInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory);
			AssertEquals("Receive.PK", receive.PK, importedReceiveInOtherFactory.PK);
			AssertEquals("Receive.WD_OH_Client", receive.WD_OH_Client, importedReceiveInOtherFactory.WD_OH_Client);
			AssertEquals("Receive.WD_WW_Whs", receive.WD_WW_Whs, importedReceiveInOtherFactory.WD_WW_Whs);
			AssertEquals("Receive.WD_WL_CrossDock", receive.WD_WL_CrossDock, importedReceiveInOtherFactory.WD_WL_CrossDock);
			AssertEquals("Receive.WD_ExWhsJobGuid", receive.WD_ExWhsJobGuid, importedReceiveInOtherFactory.WD_ExWhsJobGuid);
			AssertEquals("Receive.WD_DocketType", receive.WD_DocketType, importedReceiveInOtherFactory.WD_DocketType);
			AssertEquals("Receive.WD_DocketSubType", receive.WD_DocketSubType, importedReceiveInOtherFactory.WD_DocketSubType);
			AssertEquals("Receive.WD_TotalWeightUnit", receive.WD_TotalWeightUnit, importedReceiveInOtherFactory.WD_TotalWeightUnit);
			AssertEquals("Receive.WD_TotalCubicUnit", receive.WD_TotalCubicUnit, importedReceiveInOtherFactory.WD_TotalCubicUnit);
			AssertEquals("Receive.WD_DocketStatus", receive.WD_DocketStatus, importedReceiveInOtherFactory.WD_DocketStatus);
			AssertEquals("Receive.WD_PickOption", receive.WD_PickOption, importedReceiveInOtherFactory.WD_PickOption);
			AssertEquals("Receive.WD_CustomerReference", receive.WD_CustomerReference, importedReceiveInOtherFactory.WD_CustomerReference);
			AssertEquals("Receive.WD_TransportReference", receive.WD_TransportReference, importedReceiveInOtherFactory.WD_TransportReference);
			AssertEquals("Receive.WD_DropMode", receive.WD_DropMode, importedReceiveInOtherFactory.WD_DropMode);
			AssertEquals("Receive.WD_CODPayMethod", receive.WD_CODPayMethod, importedReceiveInOtherFactory.WD_CODPayMethod);
			AssertEquals("Receive.WD_INCO", receive.WD_INCO, importedReceiveInOtherFactory.WD_INCO);
			AssertEquals("Receive.WD_F3_NKTotalPackType", receive.WD_F3_NKTotalPackType, importedReceiveInOtherFactory.WD_F3_NKTotalPackType);
			AssertEquals("Receive.WD_PL_NKCarrierServiceLevel", receive.WD_PL_NKCarrierServiceLevel, importedReceiveInOtherFactory.WD_PL_NKCarrierServiceLevel);
			AssertEquals("Receive.WD_RS_NKServiceLevel", receive.WD_RS_NKServiceLevel, importedReceiveInOtherFactory.WD_RS_NKServiceLevel);
			AssertEquals("Receive.WD_CustomAttrib1", receive.WD_CustomAttrib1, importedReceiveInOtherFactory.WD_CustomAttrib1);
			AssertEquals("Receive.WD_CustomAttrib2", receive.WD_CustomAttrib2, importedReceiveInOtherFactory.WD_CustomAttrib2);
			AssertEquals("Receive.WD_CustomAttrib3", receive.WD_CustomAttrib3, importedReceiveInOtherFactory.WD_CustomAttrib3);
			AssertEquals("Receive.WD_CustomAttrib4", receive.WD_CustomAttrib4, importedReceiveInOtherFactory.WD_CustomAttrib4);
			AssertEquals("Receive.WD_CustomAttrib5", receive.WD_CustomAttrib5, importedReceiveInOtherFactory.WD_CustomAttrib5);
			AssertEquals("Receive.WD_TotalUnits", receive.WD_TotalUnits, importedReceiveInOtherFactory.WD_TotalUnits);
			AssertEquals("Receive.WD_TotalWeight", receive.WD_TotalWeight, importedReceiveInOtherFactory.WD_TotalWeight);
			AssertEquals("Receive.WD_TotalCubic", receive.WD_TotalCubic, importedReceiveInOtherFactory.WD_TotalCubic);
			AssertEquals("Receive.WD_UnitsSent", receive.WD_UnitsSent, importedReceiveInOtherFactory.WD_UnitsSent);
			AssertEquals("Receive.WD_CubicSent", receive.WD_CubicSent, importedReceiveInOtherFactory.WD_CubicSent);
			AssertEquals("Receive.WD_LocalCartInsuranceCost", receive.WD_LocalCartInsuranceCost, importedReceiveInOtherFactory.WD_LocalCartInsuranceCost);
			AssertEquals("Receive.WD_ShipperCODAmount", receive.WD_ShipperCODAmount, importedReceiveInOtherFactory.WD_ShipperCODAmount);
			AssertEquals("Receive.WD_WeightSent", receive.WD_WeightSent, importedReceiveInOtherFactory.WD_WeightSent);
			AssertEquals("Receive.WD_WeightSentUserEntered", receive.WD_WeightSentUserEntered, importedReceiveInOtherFactory.WD_WeightSentUserEntered);
			AssertEquals("Receive.WD_CustomDecimal1", receive.WD_CustomDecimal1, importedReceiveInOtherFactory.WD_CustomDecimal1);
			AssertEquals("Receive.WD_CustomDecimal2", receive.WD_CustomDecimal2, importedReceiveInOtherFactory.WD_CustomDecimal2);
			AssertEquals("Receive.WD_CustomDecimal3", receive.WD_CustomDecimal3, importedReceiveInOtherFactory.WD_CustomDecimal3);
			AssertEquals("Receive.WD_CustomDecimal4", receive.WD_CustomDecimal4, importedReceiveInOtherFactory.WD_CustomDecimal4);
			AssertEquals("Receive.WD_CustomDecimal5", receive.WD_CustomDecimal5, importedReceiveInOtherFactory.WD_CustomDecimal5);
			AssertEquals("Receive.WD_PackagesSent", receive.WD_PackagesSent, importedReceiveInOtherFactory.WD_PackagesSent);
			AssertEquals("Receive.WD_TotalPallets", receive.WD_TotalPallets, importedReceiveInOtherFactory.WD_TotalPallets);
			AssertEquals("Receive.WD_PalletsSent", receive.WD_PalletsSent, importedReceiveInOtherFactory.WD_PalletsSent);
			AssertEquals("Receive.WD_ExternalReferenceSplit", receive.WD_ExternalReferenceSplit, importedReceiveInOtherFactory.WD_ExternalReferenceSplit);
			AssertEquals("Receive.WD_WeightVolSetFromImport", receive.WD_WeightVolSetFromImport, importedReceiveInOtherFactory.WD_WeightVolSetFromImport);
			AssertEquals("Receive.WD_AddPalletWeightToOrder", receive.WD_AddPalletWeightToOrder, importedReceiveInOtherFactory.WD_AddPalletWeightToOrder);
			AssertEquals("Receive.WD_AutoFinaliseBOMIntoInventory", receive.WD_AutoFinaliseBOMIntoInventory, importedReceiveInOtherFactory.WD_AutoFinaliseBOMIntoInventory);
			AssertEquals("Receive.WD_CustomFlag1", receive.WD_CustomFlag1, importedReceiveInOtherFactory.WD_CustomFlag1);
			AssertEquals("Receive.WD_CustomFlag2", receive.WD_CustomFlag2, importedReceiveInOtherFactory.WD_CustomFlag2);
			AssertEquals("Receive.WD_CustomFlag3", receive.WD_CustomFlag3, importedReceiveInOtherFactory.WD_CustomFlag3);
			AssertEquals("Receive.WD_CustomFlag4", receive.WD_CustomFlag4, importedReceiveInOtherFactory.WD_CustomFlag4);
			AssertEquals("Receive.WD_CustomFlag5", receive.WD_CustomFlag5, importedReceiveInOtherFactory.WD_CustomFlag5);
			AssertEquals("Receive.WD_BookingDate", receive.WD_BookingDate, importedReceiveInOtherFactory.WD_BookingDate);
			AssertEquals("Receive.WD_ArrivalDate", receive.WD_ArrivalDate, importedReceiveInOtherFactory.WD_ArrivalDate);
			AssertEquals("Receive.WD_FinalisedDate", receive.WD_FinalisedDate, importedReceiveInOtherFactory.WD_FinalisedDate);
			AssertEquals("Receive.WD_RequiredDate", receive.WD_RequiredDate, importedReceiveInOtherFactory.WD_RequiredDate);
			AssertEquals("Receive.WD_ETA", receive.WD_ETA, importedReceiveInOtherFactory.WD_ETA);
			AssertEquals("Receive.WD_ETD", receive.WD_ETD, importedReceiveInOtherFactory.WD_ETD);
			AssertEquals("Receive.WD_CustomDate1", receive.WD_CustomDate1, importedReceiveInOtherFactory.WD_CustomDate1);
			AssertEquals("Receive.WD_CustomDate2", receive.WD_CustomDate2, importedReceiveInOtherFactory.WD_CustomDate2);

			AssertNotEquals("Receive.WD_DocketID", receive.WD_DocketID, importedReceiveInOtherFactory.WD_DocketID);
		}

		#endregion

		#region TestImport_Receive_MissingClient

		public void TestImport_Receive_MissingClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			// Do not import Client into new Factory.
			otherFactory.ImportFromAnotherFactory(data.Whs1);
			otherFactory.ImportFromAnotherFactory(data.Part1);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var expectedErrorMessage = string.Format("Receive [({0}) - {1}]\r\nCould not find Organization with PK = ({2}).\r\nPlease import it first and then retry the import operation.\r\n",
				receive.PK, receive.WD_ExternalReference, data.Org1.PK);

			AssertExceptionThrown(
				"Should not import receive if client is not in DB.",
				typeof(InvalidOperationException),
				expectedErrorMessage,
				() => dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory));

			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			AssertNull("If critical error occured, then receive should not be saved to DB.", receiveInOtherFactory);
		}

		#endregion

		#region TestImport_Receive_MissingWarehouse

		public void TestImport_Receive_MissingWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(data.Org1);
			// Do not import Warehouse into new Factory.
			otherFactory.ImportFromAnotherFactory(data.Part1);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var expectedErrorMessage = string.Format("Receive [({0}) - {1}]\r\nCould not find Warehouse with PK = ({2}).\r\nPlease import it first and then retry the import operation.\r\n",
				receive.PK, receive.WD_ExternalReference, data.Whs1.PK);

			AssertExceptionThrown(
				"Receive should not be imported if warehouse doesn't exist in DB.",
				typeof(InvalidOperationException),
				expectedErrorMessage,
				() => dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory));

			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			AssertNull("If critical error occured, then receive should not be saved to DB.", receiveInOtherFactory);
		}

		#endregion

		#region TestImport_Receive_ExternalReference

		public void TestImport_Receive_ExternalReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			receive1.WD_DocketID = "W00000123";
			receive2.WD_DocketID = "W00000456";

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive1 = adapter.ExportToValueObject(receive1, new ValueObjectExportContext(Notify));
			var xsdReceive2 = adapter.ExportToValueObject(receive2, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive1.IsInDatabase);
			AssertEquals("Precondition - Receive not in DB", false, receive2.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceive1InOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdReceive1, contextInOtherFactory);
			var importedReceive2InOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdReceive2, contextInOtherFactory);
			otherFactory.Save(); // making sure we could save into DB.
			AssertEquals("Receive reference should be changed to clearly identify merged jobs.", "System Merge W00000123", importedReceive1InOtherFactory.WD_ExternalReference);
			AssertEquals("Receive reference should be set in relation to original job.", "System Merge W00000456", importedReceive2InOtherFactory.WD_ExternalReference);
		}

		#endregion

		#region TestImport_Receive_DocketIDShoulNotBeImported

		public void TestImport_Receive_DocketIDShoulNotBeImported()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_DocketID = "W00000123";

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceiveInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory);
			otherFactory.Save(); // make sure we could save into DB.
			AssertNotEquals("DocketID shoud not be imported.", receive.WD_DocketID, importedReceiveInOtherFactory.WD_DocketID);
		}

		#endregion

		#region TestImport_DocketReferences

		public void TestImport_DocketReferences()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save(); // so we don't need to import this data into otherFactory.

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_DocketID = "W00000123";

			var reference1 = CreateDocketReference(receive, "OTH", "TEST REFERENCE");
			var reference2 = CreateDocketReference(receive, "CAN", "CUSTOMS NUMBER");

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceiveInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory);
			AssertEquals("Should import 4 references 2 from original job + original External Reference + original DocketID.", 4, importedReceiveInOtherFactory.References.Count);
			AssertContainsDocketReference(importedReceiveInOtherFactory.References, reference1);
			AssertContainsDocketReference(importedReceiveInOtherFactory.References, reference2);
			importedReceiveInOtherFactory.References.Cast<WhsDocketReference>().Single(r => r.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.PreMergeJobRefCode && r.WX_Reference == "R1");
			importedReceiveInOtherFactory.References.Cast<WhsDocketReference>().Single(r => r.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.PreMergeJobNoCode && r.WX_Reference == "W00000123");
		}

		void AssertContainsDocketReference(WhsDocketReferenceCollection docketReferenceCollection, WhsDocketReference expectedReference)
		{
			var reference = docketReferenceCollection.Cast<WhsDocketReference>().Single(r => r.PK == expectedReference.PK);
			AssertEquals("Reference.WX_RefType", expectedReference.WX_RefType, reference.WX_RefType);
			AssertEquals("Reference.WX_Reference", expectedReference.WX_Reference, reference.WX_Reference);
		}

		#endregion

		#region TestImport_DocketReferences_CuttingExternalReference

		public void TestImport_DocketReferences_CuttingExternalReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save(); // so we don't need to import this data into otherFactory.

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, new string('1', 35), data.Part1, 10m);
			receive.WD_DocketID = "W00000123";

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceiveInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory);
			AssertEquals("Should import 2 references from original job.", 2, importedReceiveInOtherFactory.References.Count);
			importedReceiveInOtherFactory.References.Cast<WhsDocketReference>().Single(r => r.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.PreMergeJobRefCode && r.WX_Reference == new string('1', 25));
			importedReceiveInOtherFactory.References.Cast<WhsDocketReference>().Single(r => r.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.PreMergeJobNoCode && r.WX_Reference == "W00000123");
			AssertEquals("Note need to be created when external reference could not be copied fully into references table.", 1, importedReceiveInOtherFactory.Notes.VisibleNotes.Count);
			AssertEquals("Note should have custom description", true, importedReceiveInOtherFactory.Notes.VisibleNotes[0].ST_IsCustomDescription);
			AssertEquals("System Merge Reference import", importedReceiveInOtherFactory.Notes.VisibleNotes[0].ST_Description);
			AssertEquals(
				string.Format("During System Merge, part of an old external reference was cut. Original reference '{0}' was shortened to '{1}'.", new string('1', 35), new string('1', 25)),
				importedReceiveInOtherFactory.Notes.VisibleNotes[0].ST_NoteDataAsText);
		}

		#endregion

		#region TestImport_ReceiveLine

		public void TestImport_ReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);
			Factory.Save(); // to avoid need to import data into otherFactory.

			var today = ZDate.Today;
			var todayOffset = new ZDateTimeOffset(today);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.DefaultLocation, "PLT-2", today.AddDays(1), today.AddDays(2), "PA1", "PA2", "PA3", "BEK");
			inv.WI_SerialNumber = "SN";

			receive.RunPreSaveValidation(); // to generate Receive lines from Inventory.

			var receiveLine1 = receive.Lines.Single(dl => dl.WE_OP == data.Part1.PK);
			var receiveLine2 = receive.Lines.Single(dl => dl.WE_OP == data.Part2.PK);
			receiveLine2.WE_LineComment = "COMMENT";
			receiveLine2.WE_CustomAttrib1 = "CUSTOM ATTR1";
			receiveLine2.WE_CustomAttrib2 = "CUSTOM ATTR2";
			receiveLine2.WE_CustomAttrib3 = "CUSTOM ATTR3";
			receiveLine2.WE_DocketLineStatus = "";
			receiveLine2.WE_RX_NKUnitPriceCurrency = "CCC";
			receiveLine2.WE_F3_NKPackType = "BAG";
			receiveLine2.WE_ReceiveCrossDockOrderNo = "CROSSDOCKORDERNO";
			receiveLine2.WE_CustomAttrib4 = "CUSTOM ATTR4";
			receiveLine2.WE_CustomAttrib5 = "CUSTOM ATTR5";
			receiveLine2.WE_CustomAttrib6 = "CUSTOM ATTR6";
			receiveLine2.WE_CustomTextBlob1 = "CUSTOM TEXT BLOB";

			receiveLine2.WE_AdjustmentArrivalDate = todayOffset.AddDays(1);
			receiveLine2.WE_RequiredByDate = todayOffset.AddDays(3);
			receiveLine2.WE_CustomDate1 = today.AddDays(4);
			receiveLine2.WE_CustomDate2 = today.AddDays(5);
			receiveLine2.WE_CustomDate3 = today.AddDays(6);
			receiveLine2.WE_CustomDate4 = today.AddDays(7);
			receiveLine2.WE_CustomDate5 = today.AddDays(8);

			receiveLine2.WE_RecommendedUnitPrice = 5m;
			receiveLine2.WE_UnitDiscountPercent = 6m;
			receiveLine2.WE_UnitDiscountAmount = 7m;
			receiveLine2.WE_UnitPriceAfterDiscount = 8m;
			receiveLine2.WE_ExtendedLinePrice = 9m;
			receiveLine2.WE_CustomDecimal1 = 10m;
			receiveLine2.WE_CustomDecimal2 = 11m;
			receiveLine2.WE_CustomDecimal3 = 12m;
			receiveLine2.WE_CustomDecimal4 = 13m;
			receiveLine2.WE_CustomDecimal5 = 14m;

			receiveLine2.WE_LineNo = 15;
			receiveLine2.WE_SubLineNo = 16;

			receiveLine2.WE_CustomFlag1 = true;
			receiveLine2.WE_CustomFlag2 = true;
			receiveLine2.WE_CustomFlag3 = true;
			receiveLine2.WE_CustomFlag4 = true;
			receiveLine2.WE_CustomFlag5 = true;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceiveInOtherFactory = (WhsReceive)dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory);
			AssertEquals("2 Receive lines should be imported.", 2, importedReceiveInOtherFactory.Lines.Count);
			AssertContainsReceiveLine(importedReceiveInOtherFactory.Lines, receiveLine1);
			AssertContainsReceiveLine(importedReceiveInOtherFactory.Lines, receiveLine2);
		}

		void AssertContainsReceiveLine(WhsReceiveLineCollection receiveLinesCollection, WhsDocketLine expectedReceiveLine)
		{
			var receiveLine = receiveLinesCollection.Single(l => l.PK == expectedReceiveLine.PK);
			AssertEquals("ReceiveLine.WE_WL", expectedReceiveLine.WE_WL, receiveLine.WE_WL);
			AssertEquals("ReceiveLine.WE_OP", expectedReceiveLine.WE_OP, receiveLine.WE_OP);
			AssertEquals("ReceiveLine.WE_PartAttrib1", expectedReceiveLine.WE_PartAttrib1, receiveLine.WE_PartAttrib1);
			AssertEquals("ReceiveLine.WE_PartAttrib2", expectedReceiveLine.WE_PartAttrib2, receiveLine.WE_PartAttrib2);
			AssertEquals("ReceiveLine.WE_PartAttrib3", expectedReceiveLine.WE_PartAttrib3, receiveLine.WE_PartAttrib3);
			AssertEquals("ReceiveLine.WE_SerialNumber", expectedReceiveLine.WE_SerialNumber, receiveLine.WE_SerialNumber);
			AssertEquals("ReceiveLine.WE_BondedEntryKey", expectedReceiveLine.WE_BondedEntryKey, receiveLine.WE_BondedEntryKey);
			AssertEquals("ReceiveLine.WE_LineComment", expectedReceiveLine.WE_LineComment, receiveLine.WE_LineComment);
			AssertEquals("ReceiveLine.WE_CustomAttrib1", expectedReceiveLine.WE_CustomAttrib1, receiveLine.WE_CustomAttrib1);
			AssertEquals("ReceiveLine.WE_CustomAttrib2", expectedReceiveLine.WE_CustomAttrib2, receiveLine.WE_CustomAttrib2);
			AssertEquals("ReceiveLine.WE_CustomAttrib3", expectedReceiveLine.WE_CustomAttrib3, receiveLine.WE_CustomAttrib3);
			AssertEquals("ReceiveLine.WE_DocketLineStatus", expectedReceiveLine.WE_DocketLineStatus, receiveLine.WE_DocketLineStatus);
			AssertEquals("ReceiveLine.WE_RX_NKUnitPriceCurrency", expectedReceiveLine.WE_RX_NKUnitPriceCurrency, receiveLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("ReceiveLine.WE_F3_NKPackType", expectedReceiveLine.WE_F3_NKPackType, receiveLine.WE_F3_NKPackType);
			AssertEquals("ReceiveLine.WE_PalletID", expectedReceiveLine.WE_PalletID, receiveLine.WE_PalletID);
			AssertEquals("ReceiveLine.WE_ReceiveCrossDockOrderNo", expectedReceiveLine.WE_ReceiveCrossDockOrderNo, receiveLine.WE_ReceiveCrossDockOrderNo);
			AssertEquals("ReceiveLine.WE_CustomAttrib4", expectedReceiveLine.WE_CustomAttrib4, receiveLine.WE_CustomAttrib4);
			AssertEquals("ReceiveLine.WE_CustomAttrib5", expectedReceiveLine.WE_CustomAttrib5, receiveLine.WE_CustomAttrib5);
			AssertEquals("ReceiveLine.WE_CustomAttrib6", expectedReceiveLine.WE_CustomAttrib6, receiveLine.WE_CustomAttrib6);
			AssertEquals("ReceiveLine.WE_CustomTextBlob1", expectedReceiveLine.WE_CustomTextBlob1, receiveLine.WE_CustomTextBlob1);
			AssertEquals("ReceiveLine.WE_ExpiryDate", expectedReceiveLine.WE_ExpiryDate, receiveLine.WE_ExpiryDate);
			AssertEquals("ReceiveLine.WE_PackingDate", expectedReceiveLine.WE_PackingDate, receiveLine.WE_PackingDate);
			AssertEquals("ReceiveLine.WE_AdjustmentArrivalDate", expectedReceiveLine.WE_AdjustmentArrivalDate, receiveLine.WE_AdjustmentArrivalDate);
			AssertEquals("ReceiveLine.WE_FinalisedDate", expectedReceiveLine.WE_FinalisedDate, receiveLine.WE_FinalisedDate);
			AssertEquals("ReceiveLine.WE_RequiredByDate", expectedReceiveLine.WE_RequiredByDate, receiveLine.WE_RequiredByDate);
			AssertEquals("ReceiveLine.WE_CustomDate1", expectedReceiveLine.WE_CustomDate1, receiveLine.WE_CustomDate1);
			AssertEquals("ReceiveLine.WE_CustomDate2", expectedReceiveLine.WE_CustomDate2, receiveLine.WE_CustomDate2);
			AssertEquals("ReceiveLine.WE_CustomDate3", expectedReceiveLine.WE_CustomDate3, receiveLine.WE_CustomDate3);
			AssertEquals("ReceiveLine.WE_CustomDate4", expectedReceiveLine.WE_CustomDate4, receiveLine.WE_CustomDate4);
			AssertEquals("ReceiveLine.WE_CustomDate5", expectedReceiveLine.WE_CustomDate5, receiveLine.WE_CustomDate5);
			AssertEquals("ReceiveLine.WE_TransactionQuantity", expectedReceiveLine.WE_TransactionQuantity, receiveLine.WE_TransactionQuantity);
			AssertEquals("ReceiveLine.WE_ClientOrderedUnits", expectedReceiveLine.WE_ClientOrderedUnits, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("ReceiveLine.WE_RecommendedUnitPrice", expectedReceiveLine.WE_RecommendedUnitPrice, receiveLine.WE_RecommendedUnitPrice);
			AssertEquals("ReceiveLine.WE_UnitDiscountPercent", expectedReceiveLine.WE_UnitDiscountPercent, receiveLine.WE_UnitDiscountPercent);
			AssertEquals("ReceiveLine.WE_UnitDiscountAmount", expectedReceiveLine.WE_UnitDiscountAmount, receiveLine.WE_UnitDiscountAmount);
			AssertEquals("ReceiveLine.WE_UnitPriceAfterDiscount", expectedReceiveLine.WE_UnitPriceAfterDiscount, receiveLine.WE_UnitPriceAfterDiscount);
			AssertEquals("ReceiveLine.WE_ExtendedLinePrice", expectedReceiveLine.WE_ExtendedLinePrice, receiveLine.WE_ExtendedLinePrice);
			AssertEquals("ReceiveLine.WE_CustomDecimal1", expectedReceiveLine.WE_CustomDecimal1, receiveLine.WE_CustomDecimal1);
			AssertEquals("ReceiveLine.WE_CustomDecimal2", expectedReceiveLine.WE_CustomDecimal2, receiveLine.WE_CustomDecimal2);
			AssertEquals("ReceiveLine.WE_CustomDecimal3", expectedReceiveLine.WE_CustomDecimal3, receiveLine.WE_CustomDecimal3);
			AssertEquals("ReceiveLine.WE_CustomDecimal4", expectedReceiveLine.WE_CustomDecimal4, receiveLine.WE_CustomDecimal4);
			AssertEquals("ReceiveLine.WE_CustomDecimal5", expectedReceiveLine.WE_CustomDecimal5, receiveLine.WE_CustomDecimal5);
			AssertEquals("ReceiveLine.WE_LineNo", expectedReceiveLine.WE_LineNo, receiveLine.WE_LineNo);
			AssertEquals("ReceiveLine.WE_SubLineNo", expectedReceiveLine.WE_SubLineNo, receiveLine.WE_SubLineNo);
			AssertEquals("ReceiveLine.WE_CustomFlag1", expectedReceiveLine.WE_CustomFlag1, receiveLine.WE_CustomFlag1);
			AssertEquals("ReceiveLine.WE_CustomFlag2", expectedReceiveLine.WE_CustomFlag2, receiveLine.WE_CustomFlag2);
			AssertEquals("ReceiveLine.WE_CustomFlag3", expectedReceiveLine.WE_CustomFlag3, receiveLine.WE_CustomFlag3);
			AssertEquals("ReceiveLine.WE_CustomFlag4", expectedReceiveLine.WE_CustomFlag4, receiveLine.WE_CustomFlag4);
			AssertEquals("ReceiveLine.WE_CustomFlag5", expectedReceiveLine.WE_CustomFlag5, receiveLine.WE_CustomFlag5);
		}

		#endregion

		#region TestImport_ReceiveLine_MissingProduct

		public void TestImport_ReceiveLine_MissingProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(data.Org1);
			otherFactory.ImportFromAnotherFactory(data.Whs1);
			// Do not import Product into new Factory.

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var expectedErrorMessage = string.Format("Receive [({0}) - {1}]\r\nCould not find Product with PK = ({2}).\r\nPlease import it first and then retry the import operation.\r\n",
				receive.PK, receive.WD_ExternalReference, data.Part1.PK);

			AssertExceptionThrown(
				"Should not import receive if product is not in DB.",
				typeof(ArgumentException),
				expectedErrorMessage,
				() => dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory));

			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			AssertNull("If critical error occured, then receive should not be saved to DB.", receiveInOtherFactory);
		}

		#endregion

		#region TestImport_ReceiveLine_MissingLocation

		public void TestImport_ReceiveLine_MissingLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rowB = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 1, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, rowB.Locations[0]);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(data.Org1);
			otherFactory.ImportFromAnotherFactory(data.Whs1);
			// Do not import RowB into new Factory.
			otherFactory.ImportFromAnotherFactory(data.Part1);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var expectedErrorMessage = string.Format("Receive [({0}) - {1}]\r\nCould not find Location with PK = ({2}).\r\nPlease import it first and then retry the import operation.\r\n",
					receive.PK, receive.WD_ExternalReference, rowB.Locations[0].PK);

			AssertExceptionThrown("When location doesn't exist in DB where we import data into, we should get an import exception.",
				typeof(ArgumentException),
				expectedErrorMessage,
				() => dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory));

			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			AssertNull("If critical error occured, then receive should not be saved to DB.", receiveInOtherFactory);
		}

		#endregion

		#region TestImport_BondedWarehouseAttribute

		public void TestImport_BondedWarehouseAttribute()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Factory.Save(); // so we don't need to import this data into another Factory.

			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var manufacturer = Helper.CreateClient("MANU", "Manufacturer");
			var manufacturerAddress = manufacturer.MainAddress;

			var customData = receive.Inventory[0].CustomsData;
			customData.WB_DeclarationReference = "DECLARATION";
			customData.WB_EntryKey = "KEY123";
			customData.WB_CustomsUnitOfQty = "KG";
			customData.WB_BondedWhsUnitOfQty = "BAG";
			customData.WB_RN_NKCountryOfOrigin = "AU";
			customData.WB_AddInfo = "ADDITIONAL INFO";
			customData.WB_RX_NKTILVCurrency = "USD";
			customData.WB_IsActive = true;
			customData.WB_EntryLineNo = 1;
			customData.WB_CustomsQty = 2m;
			customData.WB_BondedWhsQty = 3m;
			customData.WB_ValueForDuty = 4m;
			customData.WB_TILV = 5m;
			customData.WB_EntryDate = today;
			customData.WB_CustomsSecondQuantity = 22m;
			customData.WB_CustomsSecondUnitQty = "GRM";
			customData.WB_Tariff = "TRF";
			customData.WB_PrimaryPreference = "P0P";
			customData.WB_CustomsThirdQuantity = 12m;
			customData.WB_CustomsThirdUnitQty = "GRM";
			customData.WB_OA_ManufacturerAddress = manufacturerAddress.PK;

			receive.RunPreSaveValidation(); // to generate Receive lines from Inventory.

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceiveInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory);
			AssertEquals("Receive should be imported without subtype changing.", receive.WD_DocketSubType, importedReceiveInOtherFactory.WD_DocketSubType);
			AssertContainsCustomData(customData, importedReceiveInOtherFactory);
		}

		void AssertContainsCustomData(WhsBondedWarehouseAttribute expectedCustomData, WhsDocket receive)
		{
			var actualCustomData = receive.Lines.Single(l => l.PK == expectedCustomData.WB_ParentID).CustomsData;
			AssertEquals("CustomData.PK", expectedCustomData.PK, actualCustomData.PK);
			AssertEquals("CustomData.WB_WB_InwardsEntry", expectedCustomData.WB_WB_InwardsEntry, actualCustomData.WB_WB_InwardsEntry);
			AssertEquals("CustomData.WB_ParentID", expectedCustomData.WB_ParentID, actualCustomData.WB_ParentID);
			AssertEquals("CustomData.WB_DeclarationReference", expectedCustomData.WB_DeclarationReference, actualCustomData.WB_DeclarationReference);
			AssertEquals("CustomData.WB_EntryKey", expectedCustomData.WB_EntryKey, actualCustomData.WB_EntryKey);
			AssertEquals("CustomData.WB_CustomsUnitOfQty", expectedCustomData.WB_CustomsUnitOfQty, actualCustomData.WB_CustomsUnitOfQty);
			AssertEquals("CustomData.WB_BondedWhsUnitOfQty", expectedCustomData.WB_BondedWhsUnitOfQty, actualCustomData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomData.WB_RN_NKCountryOfOrigin", expectedCustomData.WB_RN_NKCountryOfOrigin, actualCustomData.WB_RN_NKCountryOfOrigin);
			AssertEquals("CustomData.WB_AddInfo", expectedCustomData.WB_AddInfo, actualCustomData.WB_AddInfo);
			AssertEquals("CustomData.WB_ParentTableCode", expectedCustomData.WB_ParentTableCode, actualCustomData.WB_ParentTableCode);
			AssertEquals("CustomData.WB_RX_NKTILVCurrency", expectedCustomData.WB_RX_NKTILVCurrency, actualCustomData.WB_RX_NKTILVCurrency);
			AssertEquals("CustomData.WB_IsActive", expectedCustomData.WB_IsActive, actualCustomData.WB_IsActive);
			AssertEquals("CustomData.WB_EntryLineNo", expectedCustomData.WB_EntryLineNo, actualCustomData.WB_EntryLineNo);
			AssertEquals("CustomData.WB_CustomsQty", expectedCustomData.WB_CustomsQty, actualCustomData.WB_CustomsQty);
			AssertEquals("CustomData.WB_BondedWhsQty", expectedCustomData.WB_BondedWhsQty, actualCustomData.WB_BondedWhsQty);
			AssertEquals("CustomData.WB_ValueForDuty", expectedCustomData.WB_ValueForDuty, actualCustomData.WB_ValueForDuty);
			AssertEquals("CustomData.WB_TILV", expectedCustomData.WB_TILV, actualCustomData.WB_TILV);
			AssertEquals("CustomData.WB_EntryDate", expectedCustomData.WB_EntryDate, actualCustomData.WB_EntryDate);
			AssertEquals("CustomData.WB_CustomsSecondQuantity", expectedCustomData.WB_CustomsSecondQuantity, actualCustomData.WB_CustomsSecondQuantity);
			AssertEquals("CustomData.WB_CustomsSecondUnitQty", expectedCustomData.WB_CustomsSecondUnitQty, actualCustomData.WB_CustomsSecondUnitQty);
			AssertEquals("CustomData.WB_Tariff", expectedCustomData.WB_Tariff, actualCustomData.WB_Tariff);
			AssertEquals("CustomData.WB_PrimaryPreference", expectedCustomData.WB_PrimaryPreference, actualCustomData.WB_PrimaryPreference);
			AssertEquals("CustomData.WB_CustomsThirdQuantity", expectedCustomData.WB_CustomsThirdQuantity, actualCustomData.WB_CustomsThirdQuantity);
			AssertEquals("CustomData.WB_CustomsThirdUnitQty", expectedCustomData.WB_CustomsThirdUnitQty, actualCustomData.WB_CustomsThirdUnitQty);
			AssertEquals("CustomData.WB_OA_ManufacturerAddress", expectedCustomData.WB_OA_ManufacturerAddress, actualCustomData.WB_OA_ManufacturerAddress);
		}

		#endregion

		#region TestImport_Inventory

		public void TestImport_Inventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);
			Factory.Save(); // to avoid need to import data into otherFactory.

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT-2", today.AddDays(1), today.AddDays(2), "PA1", "PA2", "PA3", "BEK");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			inventory1.InDocketLine.HeldCodeChangeQuantity = 4m;
			inventory1.InDocketLine.HeldCodeToChangeTo = InventoryStatus.Codes.Held;

			inventory2.WI_InDocketLineUnits = 3m;
			inventory2.WI_TotalUnits = 1m;
			inventory2.WI_ArrivalDate = today.ToZDateTime().ToOffset().AddDays(3);
			inventory2.WI_IsOriginalReceiptLine = true;

			receive.RunPreSaveValidation(); // to generate WhsReceiveLines
			inventory1.InDocketLine.ChangeInventoryHeldCode(true); // simulate inventory split.

			var allReceiveLines = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK));
			var allInventories = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, receive.PK));
			AssertEquals("Receive should have 3 Receive Lines.", 3, allReceiveLines.Length);
			AssertEquals("Receive should have 3 Inventories.", 3, allInventories.Length);

			var receiveLine1 = receive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			var receiveLine2 = receive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			var receiveLine3 = allReceiveLines.Single(l => !l.WE_IsOriginalInventory);

			AssertNotNull(receiveLine3);
			AssertEquals(receiveLine3.WE_TransactionQuantity, 4m);
			AssertEquals(receiveLine3.Inventory[0].WI_InDocketLineUnits, 0m);

			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var xsdReceive = adapter.ExportToValueObject(receive, new ValueObjectExportContext(Notify));
			AssertEquals("Precondition - Receive not in DB", false, receive.IsInDatabase);

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var importedReceiveInOtherFactory = (WhsReceive)dataAdapter.CreateOrUpdateFromValueObject(xsdReceive, contextInOtherFactory);
			var allReceiveLinesInOtherFactory = otherFactory.Load<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WD, importedReceiveInOtherFactory.PK));
			AssertContainsInventory(allReceiveLinesInOtherFactory, receiveLine1, 6m);
			AssertContainsInventory(allReceiveLinesInOtherFactory, receiveLine2, 1m);
			AssertContainsInventory(allReceiveLinesInOtherFactory, receiveLine3, 4m);
		}

		void AssertContainsInventory(WhsReceiveLine[] allReceiveLines, WhsDocketLine expectedReceiveLine, ZDecimal expectedInDocketLineUnits)
		{
			var receiveLine = allReceiveLines.Single(l => l.PK == expectedReceiveLine.PK);
			AssertEquals("All Inventory Lines should be imported.", expectedReceiveLine.Inventory.Count, receiveLine.Inventory.Count);
			foreach (WhsInventoryView expectedInventory in expectedReceiveLine.Inventory)
			{
				var inventory = receiveLine.Inventory.Cast<WhsInventoryView>().Single(i => i.PK == expectedInventory.PK);
				AssertEquals("Inventory.WI_WD", expectedInventory.WI_WD, inventory.WI_WD);
				AssertEquals("Inventory.WI_OH_Client", expectedInventory.WI_OH_Client, inventory.WI_OH_Client);
				AssertEquals("Inventory.WI_WE_OriginalInDocketLineForRating", expectedInventory.WI_WE_InDocketLine, inventory.WI_WE_OriginalInDocketLineForRating);
				AssertEquals("Inventory.WI_OP", expectedInventory.WI_OP, inventory.WI_OP);
				AssertEquals("Inventory.WI_WL", expectedInventory.WI_WL, inventory.WI_WL);
				AssertEquals("Inventory.WI_PartAttrib1", expectedInventory.WI_PartAttrib1, inventory.WI_PartAttrib1);
				AssertEquals("Inventory.WI_PartAttrib2", expectedInventory.WI_PartAttrib2, inventory.WI_PartAttrib2);
				AssertEquals("Inventory.WI_PartAttrib3", expectedInventory.WI_PartAttrib3, inventory.WI_PartAttrib3);
				AssertEquals("Inventory.WI_SerialNumber", expectedInventory.WI_SerialNumber, inventory.WI_SerialNumber);
				AssertEquals("Inventory.WI_ExpiryDate", expectedInventory.WI_ExpiryDate, inventory.WI_ExpiryDate);
				AssertEquals("Inventory.WI_PackingDate", expectedInventory.WI_PackingDate, inventory.WI_PackingDate);
				AssertEquals("Inventory.WI_BondedEntryKey", expectedInventory.WI_BondedEntryKey, inventory.WI_BondedEntryKey);
				AssertEquals("Inventory.WI_PalletID", expectedInventory.WI_PalletID, inventory.WI_PalletID);
				AssertEquals("Inventory.WI_InventoryStatus", expectedInventory.WI_InventoryStatus, inventory.WI_InventoryStatus);
				AssertEquals("Inventory.WI_InDocketLineType", expectedInventory.WI_InDocketLineType, inventory.WI_InDocketLineType);
				AssertEquals("Inventory.WI_F3_NKPackType", expectedInventory.WI_F3_NKPackType, inventory.WI_F3_NKPackType);
				AssertEquals("Inventory.WI_TotalUnits", expectedInventory.WI_TotalUnits, inventory.WI_TotalUnits);
				AssertEquals("Inventory.WI_InDocketLineUnits", expectedInDocketLineUnits, inventory.WI_InDocketLineUnits);
				AssertEquals("Inventory.WI_ArrivalDate", expectedInventory.WI_ArrivalDate, inventory.WI_ArrivalDate);
				AssertEquals("Inventory.WI_IsOriginalReceiptLine", expectedInventory.WI_IsOriginalReceiptLine, inventory.WI_IsOriginalReceiptLine);
			}
		}

		#endregion

		#endregion

		#region AssertEquals

		void AssertEquals(string message, ZString expectedValue, ZString actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", !expectedValue.IsEmpty, actualValueSpecified);
		}

		void AssertEquals(string message, ZDateTime expectedValue, ZDateTime actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", !expectedValue.IsEmpty, actualValueSpecified);
		}

		void AssertEquals(string message, ZDateTimeOffset expectedValue, ZDateTimeOffset actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", !expectedValue.IsEmpty, actualValueSpecified);
		}

		void AssertEquals(string message, ZBool expectedValue, ZBool actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", expectedValue, actualValueSpecified);
		}

		void AssertEquals(string message, ZShort expectedValue, ZShort actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", true, actualValueSpecified);
		}

		void AssertEquals(string message, ZInt expectedValue, ZInt actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", true, actualValueSpecified);
		}

		void AssertEquals(string message, ZDecimal expectedValue, ZDecimal actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", true, actualValueSpecified);
		}

		#endregion

		#region TestOverrides

		public void TestOverrides()
		{
			var adapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			AssertEquals("SystemMergeWarehouseReceives", adapter.RootCollectionElementName);
			AssertEquals("SystemMergeWarehouseReceive", adapter.RootElementName);
			AssertNull(adapter.CollectionSchema);
			AssertNull(adapter.Schema);
		}

		#endregion
	}
}
