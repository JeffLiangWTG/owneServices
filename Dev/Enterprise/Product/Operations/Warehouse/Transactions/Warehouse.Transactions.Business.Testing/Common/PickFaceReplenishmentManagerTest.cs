using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PickFaceReplenishmentManagerTest : WhsTestCaseWithFactory
	{
		#region TestCreateTransfersForPickFaceReplenishment

		#region TestCreateTransfersForPickFaceReplenishment_DifferentWarehouses

		public void TestCreateTransfersForPickFaceReplenishment_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var differentWarehouse = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			Factory.Save();

			var bulkLocation = differentWarehouse.FindLocation("A");
			var pickfaceNeedReplenishIngWitExistingStock = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceNeedReplenishIngWitExistingStock,
				10m, 20m);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, differentWarehouse, "R1", data.Part1, 15m, bulkLocation,
				"");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m,
				pickfaceNeedReplenishIngWitExistingStock, "");
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 5m, false);
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray();

			AssertEquals("No transfers created.", 0, transfers.Length);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_OnlyBulkLocationsAreSelectedToTransferFrom

		public void TestCreateTransfersForPickFaceReplenishment_OnlyBulkLocationsAreSelectedToTransferFrom()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var pickfaceLocation = data.Whs1.FindLocation("A-1");
			var pickfaceNeedReplenishIngWitExistingStock = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceNeedReplenishIngWitExistingStock,
				10m, 20m);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 10m, 20m);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m, pickfaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m,
				pickfaceNeedReplenishIngWitExistingStock, "");
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickFace, data.Whs1, 5m, false);
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray();

			AssertEquals("No transfers created.", 0, transfers.Length);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_WithoutExistingStock

		[TestDate(2018, 6, 22, 10, 0, 0)]
		public void TestCreateTransfersForPickFaceReplenishment_WithoutExistingStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceNeedReplenishIngWitExistingStock = data.Whs1.FindLocation("A-2");
			var pickfaceNeedReplenishIngWithoutExistingStock = data.Whs1.FindLocation("A-3");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1,
				pickfaceNeedReplenishIngWitExistingStock, 5m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1,
				pickfaceNeedReplenishIngWithoutExistingStock, 5m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, pickfaceNeedReplenishIngWitExistingStock);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo1 = CreatePickFaceInfo(pickFace1, data.Whs1, 6m, false);
			var pickFaceInfo2 = CreatePickFaceInfo(pickFace2, data.Whs1, 10m, false);
			var transfer = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray().Single();
			AssertSingleTransferLine(transfer, bulkLocation, pickfaceNeedReplenishIngWitExistingStock, data.Org1,
				data.Whs1, data.Part1, 6m, ZDate.Empty, ZDate.Empty);
			AssertEquals("Booking date should be set when replenishment transfer is created.", ZDateTimeOffset.Now,
				transfer.WD_BookingDate);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_ForDifferentProductsOnSamePickface

		public void TestCreateTransfersForPickFaceReplenishment_ForDifferentProductsOnSamePickface()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var differentWarehouse = Helper.CreateWarehouse("W2", "B", 2, 1);
			var differentClient = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(differentClient, data.Part1);
			Factory.Save();

			var bulkLocationForPart1 = data.Whs1.FindLocation("A-1");
			var bulkLocationForPart2 = data.Whs1.FindLocation("A-2");
			var pickfaceLocation = data.Whs1.FindLocation("A-3");
			var bulkLocationInDifferentWarehouse = differentWarehouse.FindLocation("B-1");
			var pickfaceInDifferentWarehouse = differentWarehouse.FindLocation("B-2");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 1m, 2m);
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 1m, 2m);
			var pickFace3 = Helper.CreateProductPickFace(data.Part1, differentClient, pickfaceLocation, 2m, 4m);
			var pickFace4 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceInDifferentWarehouse, 3m, 6m);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m, bulkLocationForPart1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 3m, bulkLocationForPart2, "");
			Helper.CreateWhsReceiveWithInventory(differentClient, data.Whs1, "R3", data.Part1, 4m, bulkLocationForPart1,
				"");
			Helper.CreateWhsReceiveWithInventory(data.Org1, differentWarehouse, "R4", data.Part1, 8m,
				bulkLocationInDifferentWarehouse, "");
			Factory.Save();

			var pickFaceInfo1 = CreatePickFaceInfo(pickFace1, data.Whs1, 2m, false);
			var pickFaceInfo2 = CreatePickFaceInfo(pickFace2, data.Whs1, 2m, false);
			var pickFaceInfo3 = CreatePickFaceInfo(pickFace3, data.Whs1, 4m, false);
			var pickFaceInfo4 = CreatePickFaceInfo(pickFace4, differentWarehouse, 6m, false);
			var transfers = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[]
				{
					pickFaceInfo1, pickFaceInfo2, pickFaceInfo3, pickFaceInfo4
				}).ToArray();

			AssertEquals("Should create 4 transfers", 4, transfers.Length);
			var transferForOrg1Whs1Part1Pickface = transfers.Single(t =>
				t.WD_OH_Client == data.Org1.PK &&
				t.Lines.Any(l => l.WE_OP == data.Part1.PK && l.WE_WL == pickfaceLocation.PK));
			var transferForOrg1Whs1Part2Pickface = transfers.Single(t =>
				t.WD_OH_Client == data.Org1.PK &&
				t.Lines.Any(l => l.WE_OP == data.Part2.PK && l.WE_WL == pickfaceLocation.PK));
			AssertEquals("There should be one line for Part1 for pickface location", 1,
				transferForOrg1Whs1Part1Pickface.Lines.Count);
			AssertEquals("There should be one line for Part2 for pickface location", 1,
				transferForOrg1Whs1Part2Pickface.Lines.Count);
			AssertTransferLine(transferForOrg1Whs1Part1Pickface,
				transferForOrg1Whs1Part1Pickface.Lines.Cast<WhsTransferLine>().Single(l =>
					l.TransferFromLocation.PK == bulkLocationForPart1.PK),
				pickfaceLocation, data.Org1, data.Whs1, data.Part1, 2m, ZDate.Empty, ZDate.Empty);
			AssertTransferLine(transferForOrg1Whs1Part2Pickface,
				transferForOrg1Whs1Part2Pickface.Lines.Cast<WhsTransferLine>().Single(l =>
					l.TransferFromLocation.PK == bulkLocationForPart2.PK),
				pickfaceLocation, data.Org1, data.Whs1, data.Part2, 2m, ZDate.Empty, ZDate.Empty);

			var transferForDifferentOrgWhs1Pickface = transfers.Single(t =>
				t.WD_OH_Client == differentClient.PK &&
				t.Lines.Any(l => l.WE_OP == data.Part1.PK && l.WE_WL == pickfaceLocation.PK));
			AssertSingleTransferLine(transferForDifferentOrgWhs1Pickface, bulkLocationForPart1, pickfaceLocation,
				differentClient, data.Whs1, data.Part1, 4m, ZDate.Empty, ZDate.Empty);

			var transferForPickfaceInDifferentWhs = transfers.Single(t =>
				t.WD_WW_Whs == differentWarehouse.PK && t.Lines.Any(l =>
					l.WE_OP == data.Part1.PK && l.WE_WL == pickfaceInDifferentWarehouse.PK));
			AssertSingleTransferLine(transferForPickfaceInDifferentWhs, bulkLocationInDifferentWarehouse,
				pickfaceInDifferentWarehouse, data.Org1, differentWarehouse, data.Part1, 6m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_SummationOfMultipleLocationsNeededForReplenishment

		public void TestCreateTransfersForPickFaceReplenishment_SummationOfMultipleLocationsNeededForReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var pickFaceLocation = data.Whs1.FindLocation("A-3");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 2m, 5m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, bulkLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, bulkLocation2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, pickFaceLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickFace, data.Whs1, 4m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			AssertEquals("Should create 2 transfer lines", 2, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(l => l.TransferFromLocation.PK == bulkLocation1.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 3m, ZDate.Empty, ZDate.Empty);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(l => l.TransferFromLocation.PK == bulkLocation2.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 1m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DatesGrouping

		public void TestCreateTransfersForPickFaceReplenishment_DatesGrouping()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 2m, 10m);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 10m, "A-1", new ZDateTimeOffset(2014, 4, 1, 5, 30, 0));
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 10m, "A-1", new ZDateTimeOffset(2014, 4, 1, 7, 0, 0));
			adjustment.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(adjustment);

			var pickFaceInfo = CreatePickFaceInfo(pickFace, data.Whs1, 10m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertTransferLine(transfer,
				(WhsTransferLine)transfer.Lines.Single(l => l.SupplierPart.PK == data.Part1.PK), pickFaceLocation,
				data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);
		}

		#region TestCreateTransfersForPickFaceReplenishment_MultiplePickfaceLocationsFortheSameProductWithOneBulkLocation

		public void
			TestCreateTransfersForPickFaceReplenishment_MultiplePickfaceLocationsFortheSameProductWithOneBulkLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceLocation1 = data.Whs1.FindLocation("A-2");
			var pickfaceLocation2 = data.Whs1.FindLocation("A-3");
			var pickfaceLocation3 = data.Whs1.FindLocation("A-4");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation1, 2m, 3m);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation2, 2m, 5m);
			var pickFace3 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation3, 2m, 6m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, pickfaceLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, pickfaceLocation2);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo1 = CreatePickFaceInfo(pickFace1, data.Whs1, 2m, false);
			var pickFaceInfo2 = CreatePickFaceInfo(pickFace2, data.Whs1, 4m, false);
			var pickFaceInfo3 = CreatePickFaceInfo(pickFace3, data.Whs1, 6m, false);
			var transfers = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2, pickFaceInfo3 })
				.ToArray();

			AssertEquals("Should create 2 transfers", 2, transfers.Length);

			var transferForPickface3 = transfers.Single(t => t.Lines.Any(l => l.WE_WL == pickfaceLocation3.PK));
			AssertSingleTransferLine(transferForPickface3, bulkLocation, pickfaceLocation3, data.Org1, data.Whs1,
				data.Part1, 6m, ZDate.Empty, ZDate.Empty);

			var transferForPickface2 = transfers.Single(t => t.Lines.Any(l => l.WE_WL == pickfaceLocation2.PK));
			AssertSingleTransferLine(transferForPickface2, bulkLocation, pickfaceLocation2, data.Org1, data.Whs1,
				data.Part1, 1m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_FIFOInventory

		public void TestCreateTransfersForPickFaceReplenishment_FIFOInventory()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var c_1_2_2 = data.Whs1.FindLocation("C-1-2-2");
			var c_2_1_2 = data.Whs1.FindLocation("C-2-1-2");
			var c_2_2_1 = data.Whs1.FindLocation("C-2-2-1");
			var c_2_2_2 = data.Whs1.FindLocation("C-2-2-2");
			var b_2_2_2 = data.Whs1.FindLocation("B-2-2-2");

			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 0m, 1m);
			Factory.Save();

			var receiveWithNewBookingDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_2_2_2, "",
				today.AddDays(-4), today, "Z", "Z", "Z", "Z"); // not transferred
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_2_2_2, "",
				today.AddDays(-4), today, "Z", "Z", "A", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_2_2_2, "",
				today.AddDays(-4), today, "Z", "A", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_2_2_2, "",
				today.AddDays(-4), today, "A", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_2_2_2, "",
				today.AddDays(-4), today, "Z", "Z", "Z", "A");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_2_2_1, "",
				today.AddDays(-4), today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_2_1_2, "",
				today.AddDays(-4), today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, c_1_2_2, "",
				today.AddDays(-4), today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 5m, b_2_2_2, "",
				today.AddDays(-4), today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 2m, c_2_2_2, "",
				today.AddDays(-4), today.AddDays(-1), "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receiveWithNewBookingDate, data.Part1, 1m, c_2_2_2, "",
				today.AddDays(-5), today, "Z", "Z", "Z", "Z");
			receiveWithNewBookingDate.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receiveWithNewBookingDate);

			var receiveWithOldBookingDate =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", today.AddDays(-1).ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receiveWithOldBookingDate, data.Part1, 3m, c_2_2_2, "",
				today.AddDays(-4), today, "Z", "Z", "Z", "Z");
			receiveWithOldBookingDate.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receiveWithOldBookingDate);

			AssertPickfaceTransfer(pickFaceLocation, c_2_2_2, pickface, 1m, data.Org1, data.Whs1, data.Part1, 1m,
				today.AddDays(-5), today, today, "Z", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_2_2_2, pickface, 3m, data.Org1, data.Whs1, data.Part1, 2m,
				today.AddDays(-4), today.AddDays(-1), today, "Z", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_2_2_2, pickface, 6m, data.Org1, data.Whs1, data.Part1, 3m,
				today.AddDays(-4), today, today.AddDays(-1), "Z", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, b_2_2_2, pickface, 11m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "Z", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_1_2_2, pickface, 16m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "Z", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_2_1_2, pickface, 21m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "Z", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_2_2_1, pickface, 26m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "Z", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_2_2_2, pickface, 31m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "Z", "Z", "Z", "", "A");
			AssertPickfaceTransfer(pickFaceLocation, c_2_2_2, pickface, 36m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "A", "Z", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_2_2_2, pickface, 41m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "Z", "A", "Z", "", "Z");
			AssertPickfaceTransfer(pickFaceLocation, c_2_2_2, pickface, 46m, data.Org1, data.Whs1, data.Part1, 5m,
				today.AddDays(-4), today, today, "Z", "Z", "A", "", "Z");
		}

		void AssertPickfaceTransfer(WhsLocation pickFaceLocation, WhsLocation bulkLocation, WhsPickFace pickface,
			ZDecimal replenishmentMax,
			OrgHeader org, WhsWarehouse warehouse, OrgSupplierPart part, ZDecimal expectedTransferQty,
			ZDate expiryDate, ZDate packingDate, ZDateTime arrivalDate, ZString attribute1, ZString attribute2,
			ZString attribute3, ZString serialNumber, ZString bondedEntryKey)
		{
			pickface.WF_ReplenishMaximum = replenishmentMax;
			pickface.WF_ReplenishMinimum = pickface.WF_ReplenishMaximum - 1;
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, warehouse, expectedTransferQty, false);
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray();

			var transfer = transfers.Single(t => t.Lines.Any(l => l.WE_TransactionQuantity == expectedTransferQty));
			AssertSingleTransferLine(transfer, bulkLocation, pickFaceLocation, org, warehouse, part,
				expectedTransferQty,
				packingDate, expiryDate, attribute1, attribute2, attribute3, serialNumber, bondedEntryKey);

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Transfer is finalised", true, transfer.IsFinalised);
			transfer.Factory.Save();
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenOldestStockIsLessThanReplenishmentMultiple

		[TestDate(2013, 9, 28)]
		public void
			TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenOldestStockIsLessThanReplenishmentMultiple()
		{
			//	We have inventory from oldest to newest as follows:
			//	Location			Quantity
			//	B-1						9
			//	C-1						40
			//	B-2						40
			//	C-2						40
			//
			//	We should pick in this order:
			//
			//	B-1			Pick *All* 9
			//	C-1			Pick Only  30

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var b_1 = data.Whs1.FindLocation("B-1");
			var b_2 = data.Whs1.FindLocation("B-2");
			var c_1 = data.Whs1.FindLocation("C-1");
			var c_2 = data.Whs1.FindLocation("C-2");

			// create pickface and define replenishment criteria
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m,
				replenishMax: 45m, replenishMultiple: 10m);
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(-4);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, pickFaceLocation, "", expiryDate.AddDays(2),
				ZDate.Today, "Z", "Z", "Z", "Z"); // not transferred
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, c_1, "PID456", expiryDate.AddDays(-2),
				ZDate.Today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, b_2, "PID457", expiryDate.AddDays(-1),
				ZDate.Today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, c_2, "PID458", expiryDate, ZDate.Today, "Z",
				"Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 9m, b_1, "PID123", expiryDate.AddDays(-3),
				ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 41m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			var expectedArrivalDate = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now);
			AssertTransfer(transfer, pickFaceLocation, b_1, data.Org1, data.Whs1, data.Part1, 9m, ZDate.Today,
				expiryDate.AddDays(-3), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 2);
			AssertTransfer(transfer, pickFaceLocation, c_1, data.Org1, data.Whs1, data.Part1, 30m, ZDate.Today,
				expiryDate.AddDays(-2), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 2);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenFragmentedStockIsLessThanReplenishmentMultiple

		[TestDate(2013, 11, 27)]
		public void
			TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenFragmentedStockIsLessThanReplenishmentMultiple()
		{
			//	We have inventory from oldest to newest as follows:
			//	Location			Quantity
			//	B-1						4
			//	C-1						20
			//	B-2						5
			//	C-2						20
			//
			//	We should pick in this order:
			//
			//	B-1			Pick *All* 4
			//	C-1			Pick *All* 20
			//	B-2			Pick *All* 5
			//	C-2			Pick Only  10

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var b_1 = data.Whs1.FindLocation("B-1");
			var b_2 = data.Whs1.FindLocation("B-2");
			var c_1 = data.Whs1.FindLocation("C-1");
			var c_2 = data.Whs1.FindLocation("C-2");

			// create pickface and define replenishment criteria
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m,
				replenishMax: 45m, replenishMultiple: 10m);
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(-4);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, pickFaceLocation, "", expiryDate.AddDays(2),
				ZDate.Today, "Z", "Z", "Z", "Z"); // not transferred
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, c_1, "PID456", expiryDate.AddDays(-2),
				ZDate.Today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, b_2, "PID457", expiryDate.AddDays(-1),
				ZDate.Today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, c_2, "PID458", expiryDate, ZDate.Today, "Z",
				"Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, b_1, "PID123", expiryDate.AddDays(-3),
				ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 41m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			var expectedArrivalDate = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now);
			AssertTransfer(transfer, pickFaceLocation, b_1, data.Org1, data.Whs1, data.Part1, 4m, ZDate.Today,
				expiryDate.AddDays(-3), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
			AssertTransfer(transfer, pickFaceLocation, c_1, data.Org1, data.Whs1, data.Part1, 20m, ZDate.Today,
				expiryDate.AddDays(-2), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
			AssertTransfer(transfer, pickFaceLocation, b_2, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Today,
				expiryDate.AddDays(-1), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
			AssertTransfer(transfer, pickFaceLocation, c_2, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Today,
				expiryDate, expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenFragmentedStockIncludingNewestIsLessThanReplenishmentMultiple

		[TestDate(2013, 11, 27)]
		public void
			TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenFragmentedStockIncludingNewestIsLessThanReplenishmentMultiple()
		{
			//	We have inventory from oldest to newest as follows:
			//	Location			Quantity
			//	B-1						5
			//	B-2						20
			//	B-3						4
			//	C-1						20
			//	C-2						6
			//	C-3						30
			//	C-4						1
			//	C-5						3
			//	C-6						1
			//	C-7						1
			//
			//	We should pick in this order:
			//
			//	B-1			Pick *All* 5
			//	B-2			Pick *All* 20
			//	B-3			Pick *All* 4
			//	C-1			Pick *All* 20
			//	C-2			Pick *All* 6
			//	C-3			Pick Only  10
			//	C-4			Pick *All* 1
			//	C-5			Pick *All* 3
			//	C-6			Pick *All* 1

			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 7, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 3, 1);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var b_1 = data.Whs1.FindLocation("B-1");
			var b_2 = data.Whs1.FindLocation("B-2");
			var b_3 = data.Whs1.FindLocation("B-3");
			var c_1 = data.Whs1.FindLocation("C-1");
			var c_2 = data.Whs1.FindLocation("C-2");
			var c_3 = data.Whs1.FindLocation("C-3");
			var c_4 = data.Whs1.FindLocation("C-4");
			var c_5 = data.Whs1.FindLocation("C-5");
			var c_6 = data.Whs1.FindLocation("C-6");
			var c_7 = data.Whs1.FindLocation("C-7");

			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m,
				replenishMax: 75m, replenishMultiple: 10m);
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(-4);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, b_1, "PID123", expiryDate.AddDays(-5), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, b_2, "PID124", expiryDate.AddDays(-4), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, b_3, "PID125", expiryDate.AddDays(-3), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, c_1, "PID456", expiryDate.AddDays(-2), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, c_2, "PID457", expiryDate.AddDays(-1), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, c_3, "PID458", expiryDate, today, "Z", "Z",
				"Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, c_4, "PID459", expiryDate.AddDays(1), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, c_5, "PID461", expiryDate.AddDays(2), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, c_6, "PID462", expiryDate.AddDays(3), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, c_7, "PID463", expiryDate.AddDays(4), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, pickFaceLocation, "", expiryDate.AddDays(2),
				today, "Z", "Z", "Z", "Z"); // not transferred
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 70m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			var expectedArrivalDate = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(today.ToZDateTime());
			AssertTransfer(transfer, pickFaceLocation, b_1, data.Org1, data.Whs1, data.Part1, 5m, today,
				expiryDate.AddDays(-5), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, b_2, data.Org1, data.Whs1, data.Part1, 20m, today,
				expiryDate.AddDays(-4), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, b_3, data.Org1, data.Whs1, data.Part1, 4m, today,
				expiryDate.AddDays(-3), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, c_1, data.Org1, data.Whs1, data.Part1, 20m, today,
				expiryDate.AddDays(-2), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, c_2, data.Org1, data.Whs1, data.Part1, 6m, today,
				expiryDate.AddDays(-1), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, c_3, data.Org1, data.Whs1, data.Part1, 10m, today,
				expiryDate, expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, c_4, data.Org1, data.Whs1, data.Part1, 1m, today,
				expiryDate.AddDays(1), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, c_5, data.Org1, data.Whs1, data.Part1, 3m, today,
				expiryDate.AddDays(2), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
			AssertTransfer(transfer, pickFaceLocation, c_6, data.Org1, data.Whs1, data.Part1, 1m, today,
				expiryDate.AddDays(3), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 9);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenMultipleOldestStockIsLessThanReplenishmentMultiple

		[TestDate(2013, 11, 27)]
		public void
			TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenMultipleOldestStockIsLessThanReplenishmentMultiple()
		{
			//	We have inventory from oldest to newest as follows:
			//	Location			Quantity
			//	B-1						5
			//	C-2						4
			//	B-2						3
			//	C-1						40
			//
			//	We should pick in this order:
			//
			//	B-1			Pick *All* 5
			//	C-2			Pick *All* 4
			//	B-2			Pick *All* 3
			//	C-1			Pick Only  30

			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var b_1 = data.Whs1.FindLocation("B-1");
			var b_2 = data.Whs1.FindLocation("B-2");
			var c_1 = data.Whs1.FindLocation("C-1");
			var c_2 = data.Whs1.FindLocation("C-2");

			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m,
				replenishMax: 55m, replenishMultiple: 10m);
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(-4);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, pickFaceLocation, "", expiryDate.AddDays(2),
				today, "Z", "Z", "Z", "Z"); // not transferred
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, c_1, "PID456", expiryDate, today, "Z", "Z",
				"Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, c_2, "PID458", expiryDate.AddDays(-2), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, b_1, "PID123", expiryDate.AddDays(-3), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, b_2, "PID457", expiryDate.AddDays(-1), today,
				"Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 51m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			var expectedArrivalDate = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(today.ToZDateTime());
			AssertTransfer(transfer, pickFaceLocation, b_1, data.Org1, data.Whs1, data.Part1, 5m, today,
				expiryDate.AddDays(-3), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
			AssertTransfer(transfer, pickFaceLocation, c_2, data.Org1, data.Whs1, data.Part1, 4m, today,
				expiryDate.AddDays(-2), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
			AssertTransfer(transfer, pickFaceLocation, b_2, data.Org1, data.Whs1, data.Part1, 3m, today,
				expiryDate.AddDays(-1), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
			AssertTransfer(transfer, pickFaceLocation, c_1, data.Org1, data.Whs1, data.Part1, 30m, today, expiryDate,
				expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 4);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenOldestStockDoesNotFullyReplenish

		[TestDate(2013, 11, 27)]
		public void TestCreateTransfersForPickFaceReplenishment_FIFOInventory_WhenOldestStockDoesNotFullyReplenish()
		{
			//	We have inventory from oldest to newest as follows:
			//	Location			Quantity
			//	B-1						11
			//	C-1						12
			//	B-2						40
			//	C-2						40
			//
			//	We should pick in this order:
			//
			//	B-1			Pick *All* 11
			//	C-1			Pick *All* 12
			//	B-2			Pick Only  10

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var b_1 = data.Whs1.FindLocation("B-1");
			var b_2 = data.Whs1.FindLocation("B-2");
			var c_1 = data.Whs1.FindLocation("C-1");
			var c_2 = data.Whs1.FindLocation("C-2");

			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m,
				replenishMax: 45m, replenishMultiple: 10m);
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(-4);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, pickFaceLocation, "", expiryDate.AddDays(2),
				ZDate.Today, "Z", "Z", "Z", "Z"); // not transferred
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, c_1, "PID456", expiryDate.AddDays(-2),
				ZDate.Today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, b_2, "PID457", expiryDate.AddDays(-1),
				ZDate.Today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, c_2, "PID458", expiryDate, ZDate.Today, "Z",
				"Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m, b_1, "PID123", expiryDate.AddDays(-3),
				ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 41m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			var expectedArrivalDate = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now);
			AssertTransfer(transfer, pickFaceLocation, b_1, data.Org1, data.Whs1, data.Part1, 11m, ZDate.Today,
				expiryDate.AddDays(-3), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 3);
			AssertTransfer(transfer, pickFaceLocation, c_1, data.Org1, data.Whs1, data.Part1, 12m, ZDate.Today,
				expiryDate.AddDays(-2), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 3);
			AssertTransfer(transfer, pickFaceLocation, b_2, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Today,
				expiryDate.AddDays(-1), expectedArrivalDate, "Z", "Z", "Z", "", "Z", expectedLineCount: 3);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_ExistingTransfers

		public void TestCreateTransfersForPickFaceReplenishment_ExistingTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceWithTransfer = data.Whs1.FindLocation("A-2");
			var pickfaceWithoutTransfer = data.Whs1.FindLocation("A-3");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceWithoutTransfer, 10m, 15m);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceWithTransfer, 10m, 15m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, pickfaceWithTransfer);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, pickfaceWithoutTransfer);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, bulkLocation, pickfaceWithTransfer);
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 10m, false);
			var transferForPickfaceReplenishment = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo }).ToArray().Single();
			AssertEquals(1, transferForPickfaceReplenishment.Lines.Count);
			AssertSingleTransferLine(transferForPickfaceReplenishment, bulkLocation, pickfaceWithoutTransfer, data.Org1,
				data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_ExistingTransferHasInTransitInventory

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_ExistingTransferHasInTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceWithTransferHasInTransitInv = data.Whs1.FindLocation("A-2");
			var pickface =
				Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceWithTransferHasInTransitInv, 10m, 15m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, pickfaceWithTransferHasInTransitInv);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, bulkLocation,
				pickfaceWithTransferHasInTransitInv);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition, created In-Transit Inventory.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 10m, false);
			var transferForPickfaceReplenishment = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo }).ToArray();
			AssertEquals("If the PickFace Location contains In-Transit Inventory, we should not create a new one.", 0,
				transferForPickfaceReplenishment.Length);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_BiggestLocationsFirst

		public void TestCreateTransfersForPickFaceReplenishment_BiggestLocationsFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var bulkLocationWithPartOfMultiple = data.Whs1.FindLocation("A-1");
			var smallestBulkLocation = data.Whs1.FindLocation("A-2");
			var bulkLocation = data.Whs1.FindLocation("A-3");
			var biggestBulkLocation = data.Whs1.FindLocation("A-4");
			var pickFaceLocation = data.Whs1.FindLocation("A-5");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 0m, 60m, 50m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 105m, biggestBulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 65m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 55m, smallestBulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, bulkLocationWithPartOfMultiple);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 50m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == biggestBulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 50m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_LocationsWithPartOfMultiples

		public void TestCreateTransfersForPickFaceReplenishment_LocationsWithPartOfMultiples()
		{
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");
			var pickFaceLocation = data.Whs1.FindLocation("A-5");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 0m, 60m, 50m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 35m, bulkLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, bulkLocation2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, bulkLocation3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation4);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 60m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation1.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 35m, ZDate.Empty, ZDate.Empty);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation2.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 25m, ZDate.Empty, ZDate.Empty);

			transfer.FinaliseDocketWithoutUserConfirmation();
			transfer.Factory.Save();

			var newPickFaceLocation = data.Whs1.FindLocation("A-6");
			var newPickface =
				Helper.CreateProductPickFace(data.Part1, data.Org1, newPickFaceLocation, 0m, 60m,
					50m); // Second pick face to transfer all remaining stock
			Factory.Save();

			var newPickFaceInfo = CreatePickFaceInfo(newPickface, data.Whs1, 60m, false);
			var transferRemainingStock = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { newPickFaceInfo }).ToArray().Single();
			AssertTransferLine(transferRemainingStock,
				transferRemainingStock.Lines.Cast<WhsTransferLine>().Single(t =>
					t.TransferFromLocation.PK == bulkLocation3.PK), newPickFaceLocation, data.Org1, data.Whs1,
				data.Part1, 15m, ZDate.Empty, ZDate.Empty);
			AssertTransferLine(transferRemainingStock,
				transferRemainingStock.Lines.Cast<WhsTransferLine>().Single(t =>
					t.TransferFromLocation.PK == bulkLocation4.PK), newPickFaceLocation, data.Org1, data.Whs1,
				data.Part1, 10m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultiples

		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultiples()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var pickFaceLocation = data.Whs1.FindLocation("A-3");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 0m, 50m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 35m, bulkLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 35m, bulkLocation2);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 50m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation1.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 35m, ZDate.Empty, ZDate.Empty);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation2.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks -- Early replenishment / overfill

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks

		public void TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks()
		{
			// Create simple dead lock state
			// ReplenishMinimum = 20, Maximum = 100
			// In Pick Face = 25
			// Ordered 30, picks 25 but needs 5 more units - replenishment should be automatically triggered even though the pick face doesn't fall below the replenish minimum
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 500m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 30m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: 25 units allocated from the pick face.", 25m,
				pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 75m, true);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals("Should have created 1 transfer line.", 1, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 75m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForReadyPicksFiltersByClient

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForReadyPicksFiltersByClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, bulkLocation);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part1, 25m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("25 units allocated from the pick face.", 25m,
				pick1.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part1, 30m);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);
			AssertEquals("Precondition: Pick should be ready.", PickStatus.Codes.Created, pick2.WP_PickStatus);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 75m, true);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals(
				"Should have created 1 transfer line. The existing ready pick does not help prevent deadlock because it is for another Client.",
				1, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 75m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForReadyPicksFiltersByProduct

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForReadyPicksFiltersByProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			var pickface2 = Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, bulkLocation);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("25 units allocated from the pick face.", 25m,
				pick1.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 30m);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);
			AssertEquals("Precondition: Pick should be ready.", PickStatus.Codes.Created, pick2.WP_PickStatus);

			var pickFaceInfo1 = CreatePickFaceInfo(pickface1, data.Whs1, 75m, true);
			var pickFaceInfo2 = CreatePickFaceInfo(pickface2, data.Whs1, 75m, true);
			var transfer = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray().Single();
			AssertEquals(
				"Should have created 1 transfer line. The existing ready pick does not help prevent deadlock because it is for another Product.",
				1, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 75m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForReadyPicksFiltersByLocation

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForReadyPicksFiltersByLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			var pickface2 = Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, bulkLocation);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("25 units allocated from the pick face.", 25m,
				pick1.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 30m,
				WhsPickOption.Codes.Manual);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);
			var availInv = pick2.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>()
				.Single(ai => ai.Location == bulkLocation);
			availInv.Allocate = true; // Manually pick from bulk
			AssertEquals("Precondition: Pick should be ready to pick.", PickStatus.Codes.Created, pick2.WP_PickStatus);

			var pickFaceInfo1 = CreatePickFaceInfo(pickface1, data.Whs1, 75m, true);
			var pickFaceInfo2 = CreatePickFaceInfo(pickface2, data.Whs1, 75m, true);
			var transfer = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray().Single();
			AssertEquals(
				"Should have created 1 transfer line. The existing ready pick does not help prevent deadlock because it is for another Location.",
				1, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 75m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForAvailableStockFiltersHeldInventory

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_CheckForAvailableStockFiltersHeldInventory()
		{
			// Should overfill the location if there is held inventory in the pick face
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 0m, 50m, 10m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, bulkLocation, "");
			var heldReceive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 55m, pickFaceLocation, "");
			Factory.Save();

			var inventoryLine = heldReceive.Lines[0];
			inventoryLine.HeldCodeChangeQuantity = 5m;
			inventoryLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventoryLine.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition: Split line.", 50m, inventoryLine.WE_StockOnHand);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 60m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: Allocations.", 50m, pick.OrderedInventories[0].PickLineQuantity);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 10m, true);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleProducts

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleProducts_SingleProductLocked()
		{
			// 1 Pickface shared by two products, only one product is locked
			// Should only trigger early replenishment of the locked product
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleProducts(
				bothProductsLocked: false);
		}

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleProducts_MultipleProductsLocked()
		{
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleProducts(
				bothProductsLocked: true);
		}

		[TestDate(2018, 6, 20)]
		void TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleProducts(
			bool bothProductsLocked)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			var pickface2 = Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation, 20m, 100m, 1m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, bothProductsLocked ? 25m : 30m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 30m);
			Helper.CreateWhsOrderLine(order, data.Part2, 30m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickFaceInfo1 = CreatePickFaceInfo(pickface1, data.Whs1, 75m, true);
			var pickFaceInfo2 = CreatePickFaceInfo(pickface2, data.Whs1, 75m, true);
			var transfers = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray();
			var transfer1 = transfers.Single(t => t.Lines.Any(l => l.WE_OP == data.Part1.PK));
			AssertTransferLine(transfer1,
				transfer1.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 75m, ZDate.Empty, ZDate.Empty);
			if (bothProductsLocked)
			{
				var transfer2 = transfers.Single(t => t.Lines.Any(l => l.WE_OP == data.Part2.PK));
				AssertTransferLine(transfer2,
					transfer2.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
					pickFaceLocation, data.Org1, data.Whs1, data.Part2, 75m, ZDate.Empty, ZDate.Empty);
			}
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_OnlyOverfillsByOneReplenishmentMultiple

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_OnlyOverfillsByOneReplenishmentMultiple_MultipleOfOne()
		{
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_OnlyOverfillsByOneReplenishmentMultiple(
				1m);
		}

		public void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_OnlyOverfillsByOneReplenishmentMultiple_MultipleOfMoreThanOne()
		{
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_OnlyOverfillsByOneReplenishmentMultiple(
				10m);
		}

		[TestDate(2018, 6, 20)]
		void
			TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_OnlyOverfillsByOneReplenishmentMultiple(
				ZDecimal replenMultiple)
		{
			// The pickface should only be overfilled by the replenishment multiple
			// This may result in small replenishment transfers, but alternatives are complex and have problems with edge cases.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface =
				Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 0m, 50m, replenMultiple);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10000m, bulkLocation);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1000m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Committed all units in the pick face.", 50m,
				pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, replenMultiple, true);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals("Should have created 1 transfer line.", 1, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, replenMultiple, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleOrdersAndLines

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_DetectsAndPreventsDeadlocks_MultipleOrdersAndLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("C2");
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 100m, 120m, 5m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			// Receive into bulk location
			var receiveBulk =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RB", data.Part1, 1000m, bulkLocation, "");
			var receivePickFace =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RP", data.Part1, 100m, pickFaceLocation,
					"");
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 51m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 25m);
			Helper.CreateWhsOrderLine(order3, data.Part1, 25m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order2, order3);
			AssertEquals("Precondition.", 100m, pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			// should allocate the pick face up to the maximum using multiples
			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 20m, true);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			AssertEquals("Should have created 1 transfer line.", 1, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 20m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_AllowMultipleTransfersToReplenish

		public void TestCreateTransfersForPickFaceReplenishment_AllowMultipleTransfersToReplenish_False()
		{
			TestCreateTransfersForPickFaceReplenishment_AllowMultipleTransfersToReplenish_Core(false);
		}

		public void TestCreateTransfersForPickFaceReplenishment_AllowMultipleTransfersToReplenish_True()
		{
			TestCreateTransfersForPickFaceReplenishment_AllowMultipleTransfersToReplenish_Core(true);
		}

		void TestCreateTransfersForPickFaceReplenishment_AllowMultipleTransfersToReplenish_Core(
			bool allowMultipleTransfersToReplenish)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var fromLocation = data.Whs1.FindLocation("A-1");
			var pickfaceWithTransfer = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceWithTransfer, 10m, 15m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, fromLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, pickfaceWithTransfer);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, fromLocation, pickfaceWithTransfer);
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 10m, false);
			var transfers = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo }, allowMultipleTransfersToReplenish)
				.ToArray();
			AssertEquals($"There should be {(allowMultipleTransfersToReplenish ? "1" : "no")} transfer/s returned.",
				allowMultipleTransfersToReplenish, transfers.Length > 0);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DoesNotReplenishEarlyIfAPickCanBePicked

		public void TestCreateTransfersForPickFaceReplenishment_DoesNotReplenishEarlyIfAPickCanBePicked()
		{
			TestCreateTransfersForPickFaceReplenishment_DoesNotReplenishEarlyIfAPickCanBePicked(false);
		}

		public void
			TestCreateTransfersForPickFaceReplenishment_DoesNotReplenishEarlyIfAPickCanBePicked_PickSlipPrinted()
		{
			TestCreateTransfersForPickFaceReplenishment_DoesNotReplenishEarlyIfAPickCanBePicked(true);
		}

		void TestCreateTransfersForPickFaceReplenishment_DoesNotReplenishEarlyIfAPickCanBePicked(bool pickSlipPrinted)
		{
			// Create multiple picks, similar to deadlock state but not actually deadlocked as one pick can be picked.
			// Assert doesn't replenish beyond the maximum
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 5m, 25m, 1m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);

			AssertEquals("Precondition.", 20m, pick1.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Precondition.", 5m, pick2.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Precondition: Pick should be ready for picking.", PickStatus.Codes.Created,
				pick1.WP_PickStatus);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick2.WP_IsAwaitingReplenishment);

			if (pickSlipPrinted)
			{
				pick1.WP_PickStatus = PickStatus.Codes.PickSlip;
			}

			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 0m, false);
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray();

			AssertEquals(
				"Should not have created a replenishment transfer. We are waiting on the user to execute one of the picks.",
				0, transfers.Length);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DoesNotOverfillIfAPickCanBePicked

		public void TestCreateTransfersForPickFaceReplenishment_DoesNotOverfillIfAPickCanBePicked()
		{
			TestCreateTransfersForPickFaceReplenishment_DoesNotOverfillIfAPickCanBePicked(false);
		}

		public void TestCreateTransfersForPickFaceReplenishment_DoesNotOverfillIfAPickCanBePicked_PickSlipPrinted()
		{
			TestCreateTransfersForPickFaceReplenishment_DoesNotOverfillIfAPickCanBePicked(true);
		}

		void TestCreateTransfersForPickFaceReplenishment_DoesNotOverfillIfAPickCanBePicked(bool pickSlipPrinted)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 60m, 300m, 240m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 290m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 24000m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 75m);
			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			AssertEquals("Precondition: Should *not* be waiting replenishment because there is plenty of stock.",
				PickStatus.Codes.Created, pick1.WP_PickStatus);

			if (pickSlipPrinted)
			{
				pick1.WP_PickStatus = PickStatus.Codes.PickSlip;
			}

			AssertEquals("75 units should have been allocated from the pick face.", 75m,
				pick1.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 220m);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);
			AssertEquals("Precondition: Should be waiting replenishment.", true,
				pick2.WP_IsAwaitingReplenishment);
			AssertEquals("215 units should have been allocated from the pick face.", 215m,
				pick2.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 0m, false);
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray();
			AssertEquals("Should not have created a replenishment transfer", 0, transfers.Length);

			// execute the pick
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertIsFinalisedPrecondition(pick1);
			Factory.Save();

			var pickFaceInfo2 = CreatePickFaceInfo(pickface, data.Whs1, 240m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo2 })
				.ToArray().Single();
			AssertEquals("Should have created 1 transfer line.", 1, transfer.Lines.Count);
			AssertTransferLine(transfer,
				transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation.PK == bulkLocation.PK),
				pickFaceLocation, data.Org1, data.Whs1, data.Part1, 240m, ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DoesNotOverFillIfThereIsAvailableStock

		public void TestCreateTransfersForPickFaceReplenishment_DoesNotOverFillIfThereIsAvailableStock()
		{
			// Should overfill the location if there is held inventory in the pick face
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 0m, 50m, 10m);
			SetPickAlgorithmsToPickOnlyFromPickFace();
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 60m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.",
				true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: Allocations.", 50m, pick.OrderedInventories[0].PickLineQuantity);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m, pickFaceLocation, "");
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 0m, false);
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray();
			AssertEquals("Should *not* have created a transfer as there is unallocated available stock.", 0,
				transfers.Length);
		}

		#endregion

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_ReplenishmentListIsEmpty

		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentListIsEmpty()
		{
			AssertEquals("No transfers be created", false,
				PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new List<IPickFaceInfo>()).Any());
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_Location

		public void
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_On_Location()
		{
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_Location(
				true);
		}

		public void
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_Off_Location()
		{
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_Location(
				false);
		}

		void
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_Location(
				bool registrySetting)
		{
			using (WarehouseDataRegistry.Instance.SingleLineAutoPickFaceReplenishmentTransfers.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, registrySetting))
			{
				var data = new TestDataSimpleEnvironment(Factory, 4, 1);
				var pickFaceLocation = data.Whs1.FindLocation("A-1");
				var location1 = data.Whs1.FindLocation("A-2");
				var location2 = data.Whs1.FindLocation("A-3");
				var location3 = data.Whs1.FindLocation("A-4");
				var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 3m, 4m);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location1);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location2);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location3);
				receive.FinaliseDocket();
				Factory.Save();

				AssertIsFinalisedPrecondition(receive);

				var pickfaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 4, false);
				AssertSplitTransfer(registrySetting, data, pickFaceLocation, pickfaceInfo);
			}
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_TransferIsSplitForMultipleProducts

		public void
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_On_PalletID()
		{
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_PalletID(
				true);
		}

		public void
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_Off_PalletID()
		{
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_PalletID(
				false);
		}

		void
			TestCreateTransfersForPickFaceReplenishment_WithSingleLineAutoPickFaceReplenishmentTransfersRegistrySetting_PalletID(
				bool registrySetting)
		{
			using (WarehouseDataRegistry.Instance.SingleLineAutoPickFaceReplenishmentTransfers.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, registrySetting))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var bulkLocation = data.Whs1.FindLocation("A-1");
				var pickfaceLocation = data.Whs1.FindLocation("A-2");
				var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 3m, 4m);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, bulkLocation, "PLT1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, bulkLocation, "PLT2");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, bulkLocation, "PLT3");
				receive.FinaliseDocket();
				Factory.Save();

				AssertIsFinalisedPrecondition(receive);

				var pickfaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 4, false);
				AssertSplitTransfer(registrySetting, data, pickfaceLocation, pickfaceInfo);
			}
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_TransferIsSplitForMultipleProducts

		public void TestCreateTransfersForPickFaceReplenishment_TransferIsSplitForMultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 3m, 4m);
			var pickface2 = Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 3m, 4m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 4m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var pickfaceInfo1 = CreatePickFaceInfo(pickface1, data.Whs1, 4, false);
			var pickfaceInfo2 = CreatePickFaceInfo(pickface2, data.Whs1, 4, false);
			AssertSplitTransfer(true, data, pickfaceLocation, pickfaceInfo1, pickfaceInfo2);
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DynamicTransfer

		public void TestCreateTransfersForPickFaceReplenishment_DynamicTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2),
				ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var pickFaceInfo = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part1, dynamicLocation, 10m,
				ZDate.Empty, ZDate.Empty, "", "", "", "");
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			CombineAssertions(() =>
			{
				AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True,
					transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Should have 1 transfer line", 1, transfer.Lines.Count);
				AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 10m,
					ZDate.Today, ZDate.Today.AddDays(2), "Z", "Z", "Z", "", "Z");
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_MultipleDPFsDontOverPick

		public void TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_MultipleDPFsDontOverPick()
		{
			var expectedAmountStored = 10m;

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");

			var dynamicLocation1 = data.Whs1.FindLocation("A-2");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-3");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, expectedAmountStored, bulkLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var dpfInfo1 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part1, dynamicLocation1, expectedAmountStored, ZDate.Empty, ZDate.Empty, "", "", "", "");
			var dpfInfo2 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part1, dynamicLocation2, expectedAmountStored, ZDate.Empty, ZDate.Empty, "", "", "", "");
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { dpfInfo1, dpfInfo2 }).Single();

			CombineAssertions(() =>
			{
				AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True, transfer.WD_IsPickFaceReplenishment);
				var actualAmountTransferred = transfer.Lines.Sum(l => l.WE_TransactionQuantity);
				AssertLessThanOrEqualTo("Should not pick more inventory than exists", actualAmountTransferred, expectedAmountStored);
				AssertEquals("Should have 1 transfer line", 1, transfer.Lines.Count);
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_GroupsLinesWithSameArea

		public void TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_GroupsLinesWithSameArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");

			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation1, "", today.AddDays(2), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, bulkLocation2, "", today.AddDays(2), today,
				"Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var pickFaceInfo1 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part1, dynamicLocation, 10m,
				ZDate.Empty, ZDate.Empty, "", "", "", "");
			var pickFaceInfo2 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part2, dynamicLocation, 10m,
				ZDate.Empty, ZDate.Empty, "", "", "", "");

			var transfer = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray().Single();
			CombineAssertions(() =>
			{
				AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True,
					transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Should be two grouped lines", 2, transfer.Lines.Count);
				var transferline1 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK);
				var transferline2 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part2.PK);

				AssertTransferLine(transfer, transferline1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 10m,
					today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
				AssertTransferLine(transfer, transferline2, dynamicLocation, data.Org1, data.Whs1, data.Part2, 10m,
					today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_MultipleTransfers

		public void TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_MultipleTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var dynamicLocation1 = data.Whs1.FindLocation("A-3");
			var dynamicLocation2 = data.Whs1.FindLocation("A-4");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			Factory.Save();

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation1, "", today.AddDays(2), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, bulkLocation2, "", today.AddDays(2), today,
				"Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var pickFaceInfo1 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part1, dynamicLocation1, 10m,
				ZDate.Empty, ZDate.Empty, "", "", "", "");
			var pickFaceInfo2 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part2, dynamicLocation2, 10m,
				ZDate.Empty, ZDate.Empty, "", "", "", "");

			var transfers = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray();
			AssertEquals("Should have created two seperate transfers", 2, transfers.Length);

			CombineAssertions(() =>
			{
				var transfer1 = transfers.Single(t => t.Lines.Single().WE_OP == data.Part1.PK);
				var transfer2 = transfers.Single(t => t.Lines.Single().WE_OP == data.Part2.PK);
				AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True,
					transfer1.WD_IsPickFaceReplenishment);
				AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True,
					transfer2.WD_IsPickFaceReplenishment);
				AssertSingleTransferLine(transfer1, bulkLocation1, dynamicLocation1, data.Org1, data.Whs1, data.Part1,
					10m, today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
				AssertSingleTransferLine(transfer2, bulkLocation2, dynamicLocation2, data.Org1, data.Whs1, data.Part2,
					10m, today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_TransfersCorrectAttributes

		public void TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_TransfersCorrectAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);
			var dynamicProduct = data.Part1;

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, dynamicProduct, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");
			var bulkLocation5 = data.Whs1.FindLocation("A-5");
			var dynamicLocation = data.Whs1.FindLocation("A-6");

			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(dynamicProduct, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicProduct, 10m, bulkLocation1, "", today.AddDays(2),
				today, "A", "B", "C", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicProduct, 10m, bulkLocation2, "", today.AddDays(2),
				today, "P", "B", "C", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicProduct, 10m, bulkLocation3, "", today.AddDays(2),
				today, "A", "Q", "C", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicProduct, 10m, bulkLocation4, "", today.AddDays(2),
				today, "A", "B", "R", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicProduct, 10m, bulkLocation5, "", today.AddDays(5),
				today, "A", "Q", "C", "Z");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var pickFaceInfo = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, dynamicProduct, dynamicLocation, 10m,
				today.AddDays(2), today, "A", "Q", "C", "");
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();

			CombineAssertions(() =>
			{
				AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True,
					transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Should have 1 transfer line", 1, transfer.Lines.Count);
				AssertSingleTransferLine(transfer, bulkLocation3, dynamicLocation, data.Org1, data.Whs1, dynamicProduct,
					10m, today, today.AddDays(2), "A", "Q", "C", "", "Z");
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_MixOfFixedAndDynamicTransfers

		public void TestCreateTransfersForPickFaceReplenishment_DynamicTransfer_MixOfFixedAndDynamicTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 8, 1);
			var fixedProduct1 = Helper.CreateProduct("P3", data.Org1);
			var fixedProduct2 = Helper.CreateProduct("P4", data.Org1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, fixedProduct1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, fixedProduct2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");

			var dynamicLocation = data.Whs1.FindLocation("A-5");
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var fixedPickFaceLocation1 = data.Whs1.FindLocation("A-6");
			var fixedPickFaceLocation2 = data.Whs1.FindLocation("A-7");
			var pickFace1 = Helper.CreateProductPickFace(fixedProduct1, data.Org1, fixedPickFaceLocation1, 1m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(fixedProduct2, data.Org1, fixedPickFaceLocation2, 1m, 10m);

			Factory.Save();

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation1, "", today.AddDays(2), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, bulkLocation2, "", today.AddDays(2), today,
				"Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, fixedProduct1, 10m, bulkLocation3, "", today.AddDays(2),
				today, "Z", "Z", "Z", "Z");
			Helper.CreateWhsReceiveInventoryLine(receive, fixedProduct2, 10m, bulkLocation4, "", today.AddDays(2),
				today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var pickFaceInfo1 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part1, dynamicLocation, 10m,
				ZDate.Empty, ZDate.Empty, "", "", "", "");
			var pickFaceInfo2 = CreateDynamicPickFaceInfo(data.Whs1, data.Org1, data.Part2, dynamicLocation, 10m,
				ZDate.Empty, ZDate.Empty, "", "", "", "");
			var pickFaceInfo3 = CreatePickFaceInfo(pickFace1, data.Whs1, 5m, false);
			var pickFaceInfo4 = CreatePickFaceInfo(pickFace2, data.Whs1, 5m, false);

			var transfers = PickFaceReplenishmentManager
				.CreateTransfersForPickFaceReplenishment(new[]
				{
					pickFaceInfo1, pickFaceInfo2, pickFaceInfo3, pickFaceInfo4
				}).ToArray();
			AssertEquals("Should have created three seperate transfers", 3, transfers.Length);

			var dynamicTransfer = transfers.Single(t => t.Lines.Count == 2);
			var fixedTransfers = transfers.Where(t => t.Lines.Count == 1).ToArray();
			AssertEquals("Should be two fixed transfers", 2, fixedTransfers.Length);
			CombineAssertions(() =>
			{
				AssertEquals(true, transfers.All(t => t.WD_IsPickFaceReplenishment));
				var dynamicLine1 = dynamicTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK);
				var dynamicLine2 = dynamicTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part2.PK);
				var fixedTransfer1 = fixedTransfers.Where(t => t.Lines.Single().WE_OP == fixedProduct1.PK).Single();
				var fixedTransfer2 = fixedTransfers.Where(t => t.Lines.Single().WE_OP == fixedProduct2.PK).Single();

				AssertTransferLine(dynamicTransfer, dynamicLine1, dynamicLocation, data.Org1, data.Whs1, data.Part1,
					10m, today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
				AssertTransferLine(dynamicTransfer, dynamicLine2, dynamicLocation, data.Org1, data.Whs1, data.Part2,
					10m, today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
				AssertSingleTransferLine(fixedTransfer1, bulkLocation3, fixedPickFaceLocation1, data.Org1, data.Whs1,
					fixedProduct1, 5m, today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
				AssertSingleTransferLine(fixedTransfer2, bulkLocation4, fixedPickFaceLocation2, data.Org1, data.Whs1,
					fixedProduct2, 5m, today, today.AddDays(2), "Z", "Z", "Z", "", "Z");
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_FixedPickfaceReplenishmentDoesNotConsiderStockInDynamicPickFaces

		public void
			TestCreateTransfersForPickFaceReplenishment_FixedPickfaceReplenishmentDoesNotConsiderStockInDynamicPickFaces()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace,
				isPutawayArea: false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m,
				replenishMax: 15m);
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, dynamicLocation, "");
			Factory.Save();

			var pickFaceInfo = CreatePickFaceInfo(pickFace, data.Whs1, 10m, false);
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(
					"Should have created no Transfers because the only stock is in a Dynamic Pick Face Location.", 0,
					transfers.Length);
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2),
				ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 10m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True, transfer.WD_IsPickFaceReplenishment);
			AssertEquals("Should have 1 transfer line", 1, transfer.Lines.Count);

			var line = (WhsTransferLine)transfer.Lines.Single();
			AssertEquals("Line - Client", data.Org1.PK, transfer.Client.PK);
			AssertEquals("Line - Warehouse", data.Whs1.PK, transfer.Warehouse.PK);
			AssertEquals("Line - TransferFromLocation", bulkLocation.PK, line.TransferFromLocation.PK);
			AssertEquals("Line - TransferToLocation", pickFaceLocation.PK, line.Location.PK);
			AssertEquals("Line - Product", data.Part1.PK, line.SupplierPart.PK);
			AssertEquals("Line - PackQuantity", 10m, line.WE_PackQuantity);
			AssertEquals("Line - PackingDate", ZDate.Today, line.WE_PackingDate);
			AssertEquals("Line - ExpiryDate", ZDate.Today.AddDays(2), line.WE_ExpiryDate);
			AssertEquals("Line - Attribute1", "Z", line.WE_PartAttrib1);
			AssertEquals("Line - Attribute2", "Z", line.WE_PartAttrib2);
			AssertEquals("Line - Attribute3", "Z", line.WE_PartAttrib3);
			AssertEquals("Line - BondedEntryKey", "Z", line.WE_BondedEntryKey);
			AssertEquals("Line - PickLineUnits", 10m, line.PickLines.Single().WZ_Units);
			AssertEquals("Line - ArrivalDate", data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), line.WE_AdjustmentArrivalDate);
		}

		public void TestCreateTransfersForPickFaceReplenishment_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 2m);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			var inventory1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, bulkLocation);
			inventory1.WE_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, bulkLocation);
			inventory2.WE_SerialNumber = "SN2";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickface, data.Whs1, 2m, false);
			var transfer = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals("Should have the IsPickFaceReplenishment ZBool", ZBool.True, transfer.WD_IsPickFaceReplenishment);
			AssertEquals("Should have 2 transfer lines", 2, transfer.Lines.Count);

			AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2" },
				transfer.Lines.Select(l => l.WE_SerialNumber));
			AssertEquals("Line - Client", data.Org1.PK, transfer.Client.PK);
			AssertEquals("Line - Warehouse", data.Whs1.PK, transfer.Warehouse.PK);
			Assert("Line - TransferFromLocation",
				transfer.Lines.Cast<WhsTransferLine>().All(l => l.TransferFromLocation.PK == bulkLocation.PK));
			Assert("Line - TransferToLocation", transfer.Lines.All(l => l.Location.PK == pickFaceLocation.PK));
			Assert("Line - Product", transfer.Lines.All(l => l.SupplierPart.PK == data.Part1.PK));
			Assert("Line - PackQuantity", transfer.Lines.All(l => l.WE_PackQuantity == 1m));
			Assert("Line - PickLineUnits",
				transfer.Lines.SelectMany(l => l.PickLines).All(pickLine => pickLine.WZ_Units == 1m));
			var expectedArrivalDate = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today);
			Assert("Line - ArrivalDate", transfer.Lines.All(l => l.WE_AdjustmentArrivalDate == expectedArrivalDate));
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFaces

		public void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFacesTrue()
		{
			TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFacesCore(retain: true);
		}

		public void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFacesFalse()
		{
			TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFacesCore(retain: false);
		}

		void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFacesCore(bool retain)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var fixedLocation = data.Whs1.FindLocation("A-1");
			var normalLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocation, 5m, 15m);
			fixedLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = retain;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, normalLocation, "PLT-01");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickFace, data.Whs1, 15m, false);
			var transferForPickfaceReplenishment = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals("Should have 1 transfer line", 1, transferForPickfaceReplenishment.Lines.Count);

			var line = transferForPickfaceReplenishment.Lines.Single();
			AssertEquals("Line - WLT_RetainPalletIDsInFixedPickFaces", retain, line.Location.LocationType.WLT_RetainPalletIDsInFixedPickFaces);
			AssertEquals("Line - WE_PalletID", retain ? "PLT-01" : "", line.WE_PalletID);
		}

		public void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFaces_MultipleTransferLines_SameDest()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var fixedLocation = data.Whs1.FindLocation("A-1");
			var normalLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocation, 5m, 15m);
			fixedLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, normalLocation, "PLT-01");
			receiveLine1.WI_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, normalLocation, "PLT-01");
			receiveLine2.WI_SerialNumber = "SN2";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickFace, data.Whs1, 2m, false);
			var transferForPickfaceReplenishment = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals("Should have 2 transfer line", 2, transferForPickfaceReplenishment.Lines.Count);
			AssertEquals("Line - WE_PalletID", true, transferForPickfaceReplenishment.Lines.All(l => l.WE_PalletID == "PLT-01"));
			AssertEquals("Line - Transfer To", true, transferForPickfaceReplenishment.Lines.All(l => l.WE_WL == fixedLocation.PK));
		}

		public void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFaces_MultipleTransferLines_SameDest_NotAllTransferd()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var fixedLocation = data.Whs1.FindLocation("A-1");
			var normalLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocation, 5m, 15m);
			fixedLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, normalLocation, "PLT-01");
			receiveLine1.WI_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, normalLocation, "PLT-01");
			receiveLine2.WI_SerialNumber = "SN2";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo = CreatePickFaceInfo(pickFace, data.Whs1, 1m, false);
			var transferForPickfaceReplenishment = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo })
				.ToArray().Single();
			AssertEquals("Should have 1 transfer line", 1, transferForPickfaceReplenishment.Lines.Count);
			AssertEquals("Pallet ID not retained.", "", transferForPickfaceReplenishment.Lines.Single().WE_PalletID);
		}

		public void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFaces_MultipleTransferLines_DifferentDest()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var fixedLocation1 = data.Whs1.FindLocation("A-1");
			var fixedLocation2 = data.Whs1.FindLocation("A-2");
			var normalLocation = data.Whs1.FindLocation("A-3");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocation1, 5m, 15m);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocation2, 5m, 15m);
			fixedLocation1.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			fixedLocation2.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, normalLocation, "PLT-01");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo1 = CreatePickFaceInfo(pickFace1, data.Whs1, 1m, false);
			var pickFaceInfo2 = CreatePickFaceInfo(pickFace2, data.Whs1, 1m, false);
			var transferForPickfaceReplenishments = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray();
			AssertEquals("Should have 2 transfers.", 2, transferForPickfaceReplenishments.Length);
			var transfer1 = transferForPickfaceReplenishments.Single(t => t.Lines.Any(l => l.WE_WL == fixedLocation1.PK));
			var transfer2 = transferForPickfaceReplenishments.Single(t => t.Lines.Any(l => l.WE_WL == fixedLocation2.PK));
			var transferLine1 = transfer1.Lines.Single();
			var transferLine2 = transfer2.Lines.Single();
			AssertEquals("Shouldn't retain Pallet ID because tansfered to different Locations.", "", transferLine1.WE_PalletID);
			AssertEquals("Shouldn't retain Pallet ID because tansfered to different Locations.", "", transferLine2.WE_PalletID);
		}

		public void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFaces_MultipleTransferLines_TwoWarehouseWithSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var fixedLocation = data.Whs1.FindLocation("A-1");
			var normalLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocation, 5m, 15m);
			fixedLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			Factory.Save();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, normalLocation, "PLT-01");
			receiveLine1.WI_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, normalLocation, "PLT-01");
			receiveLine2.WI_SerialNumber = "SN2";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var whs2 = Helper.CreateWarehouse("TOT", "B", 3, 1);
			Factory.Save();
			var fixedLocationInWhs2 = whs2.FindLocation("B-1");
			var normalLocationInWhs2 = whs2.FindLocation("B-2");
			var pickFaceInWhs2 = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocationInWhs2, 5m, 15m);
			fixedLocationInWhs2.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			Factory.Save();
			var receiveInWhs2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			var receiveLine1InWhs2 = Helper.CreateWhsReceiveInventoryLine(receiveInWhs2, data.Part1, 1m, normalLocationInWhs2, "PLT-01");
			receiveLine1InWhs2.WI_SerialNumber = "SN3";
			var receiveLine2InWhs2 = Helper.CreateWhsReceiveInventoryLine(receiveInWhs2, data.Part1, 1m, normalLocationInWhs2, "PLT-01");
			receiveLine2InWhs2.WI_SerialNumber = "SN4";
			receiveInWhs2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receiveInWhs2);

			var pickFaceInfo1 = CreatePickFaceInfo(pickFace, data.Whs1, 2m, false);
			var pickFaceInfo2 = CreatePickFaceInfo(pickFaceInWhs2, whs2, 2m, false);
			var transferForPickfaceReplenishments = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray();

			AssertEquals("Should have 1 transfer each warehouse.", 2, transferForPickfaceReplenishments.Length);

			var transfer1 = transferForPickfaceReplenishments.Single(t => t.Lines.All(l => l.WE_WL == fixedLocation.PK));
			var transfer2 = transferForPickfaceReplenishments.Single(t => t.Lines.All(l => l.WE_WL == fixedLocationInWhs2.PK));
			AssertEquals("Should have 2 transfer line for Whs1.", 2, transfer1.Lines.Count);
			AssertEquals("Line - WE_PalletID in Whs1", true, transfer1.Lines.All(l => l.WE_PalletID == "PLT-01"));
			AssertEquals("Should have 2 transfer line for Whs2.", 2, transfer2.Lines.Count);
			AssertEquals("Line - WE_PalletID in Whs2", true, transfer2.Lines.All(l => l.WE_PalletID == "PLT-01"));
		}

		public void TestCreateTransfersForPickFaceReplenishment_RetainPalletIDsInFixedPickFaces_MultipleTransferLines_DifferentProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var fixedLocation = data.Whs1.FindLocation("A-1");
			var normalLocation = data.Whs1.FindLocation("A-3");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, fixedLocation, 5m, 15m);
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, fixedLocation, 5m, 15m);
			fixedLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, normalLocation, "PLT-01");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, normalLocation, "PLT-01");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceInfo1 = CreatePickFaceInfo(pickFace1, data.Whs1, 1m, false);
			var pickFaceInfo2 = CreatePickFaceInfo(pickFace2, data.Whs1, 1m, false);
			var transferForPickfaceReplenishments = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(new[] { pickFaceInfo1, pickFaceInfo2 }).ToArray();
			AssertEquals("Should have 2 transfers.", 2, transferForPickfaceReplenishments.Length);
			var transfer1 = transferForPickfaceReplenishments.Single(t => t.Lines.Any(l => l.WE_OP == data.Part1.PK));
			var transfer2 = transferForPickfaceReplenishments.Single(t => t.Lines.Any(l => l.WE_OP == data.Part2.PK));
			var transferLine1 = transfer1.Lines.Single();
			var transferLine2 = transfer2.Lines.Single();
			AssertEquals("Transfer to same dest.", fixedLocation.PK, transferLine1.WE_WL);
			AssertEquals("Transfer to same dest.", fixedLocation.PK, transferLine2.WE_WL);
			AssertEquals("Shouldn't retain Pallet ID because Pallet is split into two Transfers.", "", transferLine1.WE_PalletID);
			AssertEquals("Shouldn't retain Pallet ID because Pallet is split into two Transfers.", "", transferLine2.WE_PalletID);
		}

		#endregion

		#endregion

		#region Implementation

		void AssertSplitTransfer(bool transferShouldBeSplit, TestDataSimpleEnvironment data, WhsLocation location,
			params PickFaceInfo[] pickfaceInfos)
		{
			var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(pickfaceInfos)
				.ToArray();
			var expectedTransferCount = transferShouldBeSplit ? 2 : 1;
			var expectedTransferLineNumber = transferShouldBeSplit ? 1 : 2;
			var transfersText = transferShouldBeSplit ? "transfers" : "transfer";
			var linesText = transferShouldBeSplit ? "line" : "lines";
			var assertionText = "There should be {0} {1} if the registry is set to {2}";
			AssertEquals(
				string.Format(assertionText, expectedTransferCount, transfersText, transferShouldBeSplit.ToString()),
				expectedTransferCount, transfers.Length);
			for (int i = 0; i < expectedTransferCount; i++)
			{
				AssertEquals(
					string.Format(assertionText, expectedTransferLineNumber, linesText,
						transferShouldBeSplit.ToString()), expectedTransferLineNumber, transfers[i].Lines.Count);
			}
		}

		void AssertTransfer(WhsTransfer transfer, WhsLocation pickFaceLocation, WhsLocation bulkLocation,
			OrgHeader org, WhsWarehouse warehouse, OrgSupplierPart part, ZDecimal expectedTransferQty,
			ZDate packingDate, ZDate expiryDate, ZDateTimeOffset arrivalDate, ZString attribute1, ZString attribute2,
			ZString attribute3, ZString serialNumber, ZString bondedEntryKey, int expectedLineCount)
		{
			AssertEquals(expectedLineCount, transfer.Lines.Count);

			var line = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.TransferFromLocation.PK == bulkLocation.PK);
			AssertTransferLine(transfer, line, pickFaceLocation, org, warehouse, part, expectedTransferQty,
				packingDate, expiryDate, attribute1, attribute2, attribute3, serialNumber, bondedEntryKey, arrivalDate);
		}

		static void AssertTransferLine(WhsTransfer transfer, WhsTransferLine line, WhsLocation expectedToLocation,
			OrgHeader expectedClient, WhsWarehouse expectedWarehouse, OrgSupplierPart expectedPart,
			ZDecimal expectedPackQty,
			ZDate expectedPackingDate, ZDate expectedExpiryDate, string expectedPartAttribute1 = "",
			string expectedPartAttribute2 = "", string expectedPartAttribute3 = "", string expectedSerialNumber = "",
			string expectedBondedEntryKey = "", ZDateTimeOffset? expectedArrivalDate = null)
		{
			AssertEquals(expectedClient.PK, transfer.Client.PK);
			AssertEquals(expectedWarehouse.PK, transfer.Warehouse.PK);
			AssertEquals(expectedToLocation.PK, line.Location.PK);
			AssertEquals(expectedPart.PK, line.SupplierPart.PK);
			AssertEquals(expectedPackQty, line.WE_PackQuantity);
			AssertEquals(expectedPackingDate, line.WE_PackingDate);
			AssertEquals(expectedExpiryDate, line.WE_ExpiryDate);
			AssertEquals(expectedPartAttribute1, line.WE_PartAttrib1);
			AssertEquals(expectedPartAttribute2, line.WE_PartAttrib2);
			AssertEquals(expectedPartAttribute3, line.WE_PartAttrib3);
			AssertEquals(expectedSerialNumber, line.WE_SerialNumber);
			AssertEquals(expectedBondedEntryKey, line.WE_BondedEntryKey);
			AssertEquals(expectedPackQty, line.PickLines.Single().WZ_Units);

			if (expectedArrivalDate.HasValue)
			{
				AssertEquals(expectedArrivalDate, line.WE_AdjustmentArrivalDate);
			}
		}

		static void AssertSingleTransferLine(WhsTransfer transfer, WhsLocation expectedSourceLocation,
			WhsLocation expectedTransferToLocation,
			OrgHeader expectedClient, WhsWarehouse expectedWarehouse, OrgSupplierPart expectedPart,
			ZDecimal expectedPackQty,
			ZDate expectedPackingDate, ZDate expectedExpiryDate, string expectedPartAttribute1 = "",
			string expectedPartAttribute2 = "", string expectedPartAttribute3 = "", string expectedSerialNumber = "",
			string expectedBondedEntryKey = "")
		{
			var line = transfer.Lines.Cast<WhsTransferLine>().Single();
			AssertEquals(expectedSourceLocation.PK, line.TransferFromLocation.PK);
			AssertTransferLine(transfer, line, expectedTransferToLocation, expectedClient, expectedWarehouse,
				expectedPart, expectedPackQty, expectedPackingDate, expectedExpiryDate,
				expectedPartAttribute1, expectedPartAttribute2, expectedPartAttribute3, expectedSerialNumber,
				expectedBondedEntryKey);
		}

		void SetPickAlgorithmsToPickOnlyFromPickFace()
		{
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 100);
			AllocationRulesHelper.AddFifoRule(ruleSet, 200, preventPickingPickFacesFromBulk: true);
		}

		PickFaceInfo CreatePickFaceInfo(WhsPickFace pickFace, WhsWarehouse warehouse, ZDecimal replenishQuantity,
			ZBool isDeadLocked)
		{
			return new PickFaceInfo(warehouse.PK, pickFace.WF_OH_Client, pickFace.WF_OP, pickFace.WF_WL,
				replenishQuantity, pickFace.WF_ReplenishmentMultiple, isDeadLocked);
		}

		PickFaceInfo CreateDynamicPickFaceInfo(WhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part,
			WhsLocation location, ZDecimal shortfall,
			ZDate orderedExpiryDate, ZDate orderedPackingDate, ZString orderedAttribute1, ZString orderedAttribute2,
			ZString orderedAttribute3, ZString orderedSerialNumber)
		{
			return new PickFaceInfo(warehouse.PK, ZGuid.Empty, client.PK, part.PK, location.PK, shortfall, 1m, false,
				true, orderedExpiryDate, orderedPackingDate, orderedAttribute1, orderedAttribute2, orderedAttribute3,
				orderedSerialNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			// Allocation rules typically pops up a GUI, so all tests in this class would need GuiTest for their setup.
			// This class is intended to be run in a service task anyway, so make the tests run non-user interactive to avoid this.
			isUserInteractiveDisposable = Globals.SetIsUserInteractiveForTest(false);
		}

		protected override void TearDown()
		{
			base.TearDown();

			isUserInteractiveDisposable?.Dispose();
		}

		IDisposable isUserInteractiveDisposable;

		#endregion
	}
}
