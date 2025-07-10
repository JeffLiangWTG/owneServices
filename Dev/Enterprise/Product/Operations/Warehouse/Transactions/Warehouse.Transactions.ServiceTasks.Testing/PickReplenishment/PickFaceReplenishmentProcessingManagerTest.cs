using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class PickFaceReplenishmentProcessingManagerTest : WhsTestCaseWithFactory
	{
		#region TestCreateTransfersForPickfaceReplenishment_WithCommittedInventory

		public void TestCreateTransfersForPickfaceReplenishment_WithCommittedInventory()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staff = Helper.CreateGlbStaff("STF", "Staff");
				var data = new TestDataSimpleEnvironment(Factory, 6, 1);
				var locationWithNoAllocatedInv = data.Whs1.FindLocation("A-1");
				var locationWithAllocatedInv = data.Whs1.FindLocation("A-2");
				var locationWithPickerName = data.Whs1.FindLocation("A-3");
				var locationWithPickerNameAndDate = data.Whs1.FindLocation("A-4");
				var bulkLocation = data.Whs1.FindLocation("A-5");
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithNoAllocatedInv, 1m, 3m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithAllocatedInv, 1m, 3m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithPickerName, 1m, 3m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithPickerNameAndDate, 1m, 3m);

				// create a receive, order and pick
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithNoAllocatedInv);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithAllocatedInv);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithPickerName);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithPickerNameAndDate);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, bulkLocation);
				receive.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);

				var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
				var availbleInventories = ((WhsPickOrderedInventory)pick.OrderedInventories.Single()).AvailableInventories.Cast<WhsPickAvailableInventory>();

				// allocate available inventory
				var avlInvNoAllocatedStock = availbleInventories.Single(a => a.Location == locationWithNoAllocatedInv);
				var avlInvAllocatedStock = availbleInventories.Single(a => a.Location == locationWithAllocatedInv);
				var avlInvWithPickerName = availbleInventories.Single(a => a.Location == locationWithPickerName);
				var avlInvWithPickerNameAndDate = availbleInventories.Single(a => a.Location == locationWithPickerNameAndDate);
				SetAvailableInventory(avlInvNoAllocatedStock, false, null, ZDateTimeOffset.Empty);
				SetAvailableInventory(avlInvAllocatedStock, true, null, ZDateTimeOffset.Empty);
				SetAvailableInventory(avlInvWithPickerName, true, staff, ZDateTimeOffset.Empty);
				SetAvailableInventory(avlInvWithPickerNameAndDate, true, staff, ZDateTimeOffset.Now);
				Factory.Save();

				var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager.CreateTransfersForPickfaceReplenishment();

				AssertEquals(0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, locationWithNoAllocatedInv)).Length);
				AssertEquals(0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, locationWithAllocatedInv)).Length);
				AssertEquals(0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, locationWithPickerName)).Length);
				AssertEquals(1, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, locationWithPickerNameAndDate)).Length);
				var transferForLocationWithLocationWithPickerNameAndDate = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, locationWithPickerNameAndDate)).Single();

				AssertSingleTransferLine(transferForLocationWithLocationWithPickerNameAndDate, bulkLocation, locationWithPickerNameAndDate, data.Org1, data.Whs1, data.Part1, 3m, ZDate.Empty, ZDate.Empty);
				AssertContains(@"Information|Transfer W00000004: was created successfully.
Information|Pick Face: A-4 is to be replenished with 3 Product: P1 for Client: 111 from Warehouse: 1 Location: A-5.", Logger.ToString().Trim());
			}
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_WithCommittedAdjustments

		[TestDate(2013, 11, 27)]
		public void TestCreateTransfersForPickfaceReplenishment_WithCommittedAdjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			Factory.Save();

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var b_1 = data.Whs1.FindLocation("B-1");
			var b_2 = data.Whs1.FindLocation("B-2");

			// create pickface and define replenishment criteria
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m, replenishMax: 45m, replenishMultiple: 10m);
			Factory.Save();

			ZDate.Today.AddDays(-4);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, b_1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, b_2); // only 10 will be available as we will reserve 30 units
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -30m, inventory3.Location);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, inventory1.Location);
			adjustment.RunPreSaveValidation();
			AssertEquals("Precondition - Committed Quantity is correct.", 30m, adjustmentLine1.CommittedQuantity);
			AssertEquals("Precondition - Committed Quantity is correct.", 2m, adjustmentLine2.CommittedQuantity);
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			AssertEquals(0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).Length);
			AssertEquals(@"Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_WithCrossDocking

		[TestDate(2013, 11, 27)]
		public void TestCreateTransfersForPickfaceReplenishment_WithCrossDocking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			Factory.Save();

			var pickFaceLocation = data.Whs1.FindLocation("A");
			var b_1 = data.Whs1.FindLocation("B-1");
			var b_2 = data.Whs1.FindLocation("B-2");

			// create pickface and define replenishment criteria
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, replenishMin: 5m, replenishMax: 45m, replenishMultiple: 10m);
			Factory.Save();

			ZDate.Today.AddDays(-4);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, pickFaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, b_1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, b_2); // only 10 will be available as we will reserve 30 units
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory3);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory1); // cross dock 2 to bring pickFace Qty below replenish min
			((IBusinessObjectInternals)reservedPickLine1).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 25m;
			((IBusinessObjectInternals)reservedPickLine2).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 1m;
			AssertEquals("Precondition - Reserved Quantity is correct.", 30m, reservedPickLine1.ReservedQuantity);
			AssertEquals("Precondition - Reserved Quantity is correct.", 2m, reservedPickLine2.ReservedQuantity);
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			AssertEquals(0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).Length);
			AssertEquals(@"Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());
		}

		#endregion

		#region  TestCreateTransfersForPickfaceReplenishment_WithInventoryAllocatedToPicks

		public void TestCreateTransfersForPickfaceReplenishment_WithInventoryAllocatedToPicks()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var bulkLocation = data.Whs1.FindLocation("A-1");
				var pickFace = data.Whs1.FindLocation("A-2");
				Helper.CreateProductPickFace(data.Part1, data.Org1, pickFace, 5m, 10m);
				Helper.CreateProductPickFace(data.Part2, data.Org1, pickFace, 5m, 10m);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, bulkLocation);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, bulkLocation);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, pickFace);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, pickFace);
				receive.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, WhsPickOption.Codes.Manual);
				Helper.CreateWhsOrderLine(order, data.Part1, 3m);
				Helper.CreateWhsOrderLine(order, data.Part2, 3m);

				var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
				var availableInventoryWithPickerDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1).
					AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == pickFace);
				availableInventoryWithPickerDate.PickLineQuantity = 2m;
				Helper.SetPickedDate(availableInventoryWithPickerDate, ZDateTimeOffset.Now);

				var availableInventoryWithoutPickerDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2).
					AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == pickFace);
				availableInventoryWithoutPickerDate.PickLineQuantity = 2m;

				Factory.Save();

				var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager.CreateTransfersForPickfaceReplenishment();

				var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFace)).Single();
				AssertTransferLine(transfer, (WhsTransferLine)transfer.Lines.Single(l => l.SupplierPart == data.Part1), pickFace, data.Org1, data.Whs1, data.Part1, 6m, ZDate.Empty, ZDate.Empty);
				AssertEquals(@"Information|Transfer W00000004: was created successfully.
Information|Pick Face: A-2 is to be replenished with 6 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
			}
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_WithReplenishMultiples

		public void TestCreateTransfersForPickfaceReplenishment_WithReplenishMultiples()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");
			var pickFaceLocation = data.Whs1.FindLocation("A-5");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 5m, 48m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, bulkLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, bulkLocation2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, bulkLocation3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, bulkLocation4);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, pickFaceLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).Single();
			AssertEquals(3, transfer.Lines.Count);
			AssertTransferLine(transfer, transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation == bulkLocation1), pickFaceLocation, data.Org1, data.Whs1, data.Part1, 12m, ZDate.Empty, ZDate.Empty);
			AssertTransferLine(transfer, transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation == bulkLocation2), pickFaceLocation, data.Org1, data.Whs1, data.Part1, 12m, ZDate.Empty, ZDate.Empty);
			AssertTransferLine(transfer, transfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation == bulkLocation3), pickFaceLocation, data.Org1, data.Whs1, data.Part1, 12m, ZDate.Empty, ZDate.Empty);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-5 is to be replenished with 12 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.
Information|Pick Face: A-5 is to be replenished with 12 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.
Information|Pick Face: A-5 is to be replenished with 12 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_WithIncorrectReplenishMultiples

		public void TestCreateTransfersForPickfaceReplenishment_WithIncorrectReplenishMultiples()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			var pickFaceLocation1 = data.Whs1.FindLocation("A-1");
			var pickFaceLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation1 = data.Whs1.FindLocation("A-3");
			var bulkLocation2 = data.Whs1.FindLocation("A-4");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1, 2m, 10m, 10m);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation2, 2m, 10m, 10m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, pickFaceLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation2);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation2));
			AssertEquals("This pick face location is not empty and so cannot be replenished by 10 UNT (replen. multiple).", 0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation1)).Length);
			AssertEquals("This pick face location is empty and so should be replenished.", 1, transfers.Length);
			AssertTransferLine(transfers[0], (WhsTransferLine)transfers[0].Lines.Single(l => l.SupplierPart == data.Part1), pickFaceLocation2, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_UnassignedPickFaceWithoutStock

		public void TestCreateTransfersForPickfaceReplenishment_UnassignedPickFace_NoStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.FindLocation("A");
			var fixedLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			location.WLV_WLT_LocationType = fixedLocationType.PK;
			Factory.Save();

			var pickFaceView = new WhsPickFaceViewCollection(Factory);
			AssertEquals("Precondition", fixedLocationType, location.LocationType);
			AssertEquals("Precondition: Pick Face view should have records.", 1, pickFaceView.Count);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			AssertEquals("No Transfers should be created for un-assigned pick face location.", 0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, location)).Length);
			AssertEquals("Log is incorrect.", "Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_UnassignedPickFaceWithStock

		public void TestCreateTransfersForPickfaceReplenishment_UnassignedPickFace_ExistingStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixedLocation = data.Whs1.FindLocation("A-2");
			var fixedLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, fixedLocation);
			receive.FinaliseDocket();
			fixedLocation.WLV_WLT_LocationType = fixedLocationType.PK;

			Factory.Save();

			var pickFaceView = new WhsPickFaceViewCollection(Factory);
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition", fixedLocationType, fixedLocation.LocationType);
			AssertEquals("Precondition: Pick Face view should have records.", 1, pickFaceView.Count);
			AssertEquals("Precondition: Record should have stock on hand.", 15m, pickFaceView.Single().WPV_TotalQuantity);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			AssertEquals("No Transfers should be created for un-assigned pick face location.", 0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, fixedLocation)).Length);
			AssertEquals("Log is incorrect.", "Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Today, ZDate.Today.AddDays(2), data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now), "Z", "Z", "Z", "", "Z", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_ProductChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			data.Part1.OP_IsActive = false;
			Factory.Save();

			TestDateAttribute.AddDays(1);
			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertNull(Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).SingleOrDefault());

			data.Part1.OP_IsActive = true;
			Factory.Save();

			TestDateAttribute.AddDays(1);
			Logger.ClearLog();
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, inventory.WI_PackingDate, inventory.WI_ExpiryDate, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now.AddDays(-2)), "Z", "Z", "Z", "", "Z", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_PickFaceChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			TestDateAttribute.AddDays(1);
			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertNull(Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).SingleOrDefault());

			pickFace.WF_OP = data.Part1.PK;
			Factory.Save();

			TestDateAttribute.AddDays(1);
			Logger.ClearLog();
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, inventory.WI_PackingDate, inventory.WI_ExpiryDate, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now.AddDays(-2)), "Z", "Z", "Z", "", "Z", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_LocationAllocatedOrChanged()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var bulkLocation = data.Whs1.FindLocation("A-1");
				var pickFaceLocation = data.Whs1.FindLocation("A-2");
				var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
				Factory.Save();

				Helper.SetClientAllAttributeType(data.Org1, false);
				Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

				var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
				var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, pickFaceLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "A", "A", "A", "A");
				var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "Z", "Z", "Z", "Z");
				receive.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m, pickOption: WhsPickOption.Codes.Manual);
				var pick = Helper.CreatePickNew(order);
				pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(availInv => availInv.LocationPK == pickFaceLocation.PK).Allocate = true;
				Factory.Save();

				TestDateAttribute.AddDays(1);
				var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager.CreateTransfersForPickfaceReplenishment();
				AssertNull(Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).SingleOrDefault());

				var pickLine = pick.GetAllPickLines().First();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				Factory.Save();

				TestDateAttribute.AddDays(1);
				Logger.ClearLog();
				processingManager.CreateTransfersForPickfaceReplenishment();
				AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, inventory2.WI_PackingDate, inventory2.WI_ExpiryDate, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now.AddDays(-2)), "Z", "Z", "Z", "", "Z", expectedLineCount: 1);
				AssertEquals(@"Information|Transfer W00000004: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
			}
		}

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_InventoryReceivedAfterRunningServiceTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			TestDateAttribute.AddDays(1);
			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertNull(Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).SingleOrDefault());

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			TestDateAttribute.AddDays(1);
			Logger.ClearLog();
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, inventory1.WI_PackingDate, inventory1.WI_ExpiryDate, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Now.AddDays(-1)), "Z", "Z", "Z", "", "Z", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 2m);
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

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).Single();
			AssertEquals(2, transfer.Lines.Count);

			var lines = transfer.Lines;
			Assert(lines.Cast<WhsTransferLine>().All(line => line.TransferFromLocation == bulkLocation));
			AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2" }, lines.Select(line => line.WE_SerialNumber));
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 1 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.
Information|Pick Face: A-2 is to be replenished with 1 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		[TestDate(2024, 11, 18)]
		public void TestCreateTransfersForPickfaceReplenishment_DepartedOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Order line status should be departed", DocketLineStatus.Codes.Departed, order.Lines.Single().WE_DocketLineStatus);
				AssertEquals("Pick should be waiting for replenishment", false, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
			AssertEquals("Should filter out departed order lines", 0, transfers.Length);
		}

		public void TestCreateTransfersForPickfaceReplenishment_FixedAndDynamicPickFaceReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, bulkLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, bulkLocation, "");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part2, 10m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 10m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000003: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.
Information|Transfer W00000004: was created successfully.
Information|Pick Face: A-3 is to be replenished with 10 Product: P2 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_SetsUserContext()
		{
			var warehouse1 = Helper.CreateWarehouse("WH1", "A", 4, 1);
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = Helper.CreateClient("CL1");
			var client2 = Helper.CreateClient("CL2");
			var product1 = Helper.CreateProduct("PROD1", client1);
			var product2 = Helper.CreateProduct("PROD2", client2);
			Factory.Save();

			var bulkLocation1Whs1 = warehouse1.FindLocation("A-1");
			var bulkLocation2Whs1 = warehouse1.FindLocation("A-2");
			var pickFaceLocation1Whs1 = warehouse1.FindLocation("A-3");
			var pickFaceLocation2Whs1 = warehouse1.FindLocation("A-4");
			Helper.CreateProductPickFace(product1, client1, pickFaceLocation1Whs1, 1m, 10m);
			Helper.CreateProductPickFace(product2, client2, pickFaceLocation2Whs1, 1m, 10m);

			var bulkLocationWhs2 = warehouse2.FindLocation("B-1");
			var pickFaceLocationWhs2 = warehouse2.FindLocation("B-2");
			Helper.CreateProductPickFace(product1, client1, pickFaceLocationWhs2, 1m, 10m);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client1.PK, warehouse1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, product1, 10m, bulkLocation1Whs1, "");
			receive1.FinaliseDocket();

			var receive2 = Helper.CreateWhsReceive(client2.PK, warehouse1.PK, "R2", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, product2, 10m, bulkLocation2Whs1, "");
			receive2.FinaliseDocket();

			var receive3 = Helper.CreateWhsReceive(client1.PK, warehouse2.PK, "R2", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive3, product1, 10m, bulkLocationWhs2, "");
			receive3.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(receive3);

			var contextChangeCount = 0;
			var branchContexts = new HashSet<ZGuid>();
			var testBranchCode = Env.CurrentBranch.Code;

			try
			{
				Env.Instance.UserContextChanged += OnContextChanged;

				var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager.CreateTransfersForPickfaceReplenishment();
				AssertTransfer(pickFaceLocation1Whs1, bulkLocation1Whs1, client1, warehouse1, product1, 10m, ZDate.Empty, ZDate.Empty, warehouse1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
				AssertTransfer(pickFaceLocation2Whs1, bulkLocation2Whs1, client2, warehouse1, product2, 10m, ZDate.Empty, ZDate.Empty, warehouse1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
				AssertTransfer(pickFaceLocationWhs2, bulkLocationWhs2, client1, warehouse2, product1, 10m, ZDate.Empty, ZDate.Empty, warehouse2.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);

				AssertEquals("Changed context twice.", 2, contextChangeCount);
				AssertContainsExactElementsInAnyOrder("Changed to 2 warehouse branch contexts.", new[] { warehouse1.WW_GB_RelatedCompanyBranch, warehouse2.WW_GB_RelatedCompanyBranch }, branchContexts);
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnContextChanged;
			}

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				if (e.NewUserContext.Branch.Code != testBranchCode) // do not count if it's just reverting to the previous context on temp context dispose
				{
					contextChangeCount++;
					branchContexts.Add(e.NewUserContext.Branch.PK);
				}
			}
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase() => TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase(lessThanMultipleInInventory: false, differentArrivalDates: false);

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_DifferentArrivalDates() => TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase(lessThanMultipleInInventory: false, differentArrivalDates: true);

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_LessThanMultipleInInventory() => TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase(lessThanMultipleInInventory: true, differentArrivalDates: false);

		void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase(bool lessThanMultipleInInventory, bool differentArrivalDates)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var location = data.Whs1.FindLocation("A-1-1");
			var bulkLocation = data.Whs1.FindLocation("A-1-4");

			Helper.CreateProductPickFace(data.Part1, data.Org1, location, 20m, 21m, 20m); // This setup makes no sense, just doing it to test the edge case
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 15m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-1) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 1m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-2) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 5m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-3) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 7m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-4) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, lessThanMultipleInInventory ? 6m : 7m, bulkLocation, "");
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertEquals(@"Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			var transferLine1 = Factory.LoadTop1<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
			AssertNull(transferLine1);
		}

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_MatchingLines()
			=> TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_MatchingLines(lessThanMultipleInInventory: false, differentArrivalDates: false);

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_DifferentArrivalDates_MatchingLines()
			=> TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_MatchingLines(lessThanMultipleInInventory: false, differentArrivalDates: true);

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_LessThanMultipleInInventory_MatchingLines()
			=> TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_MatchingLines(lessThanMultipleInInventory: true, differentArrivalDates: false);

		void TestCreateTransfersForPickFaceReplenishment_ReplenishmentMultipleEdgeCase_MatchingLines(bool lessThanMultipleInInventory, bool differentArrivalDates)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var bulkLocation = data.Whs1.FindLocation("A-1-4");

			Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 1m, 21m, 20m); // This setup makes no sense, just doing it to test the edge case
			Helper.CreateProductPickFace(data.Part1, data.Org1, location2, 1m, 21m, 20m); // This setup makes no sense, just doing it to test the edge case
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 20m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-1) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 1m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-2) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 5m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-3) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 5m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-4) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, lessThanMultipleInInventory ? 9m : 10m, bulkLocation, "");
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			if (differentArrivalDates && !lessThanMultipleInInventory)
			{
				AssertEquals(
@"Information|Transfer W00000006: was created successfully.
Information|Pick Face: A-1-1 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-4.
Information|Pick Face: A-1-1 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-4.
Information|Pick Face: A-1-1 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-4.
Information|Pick Face: A-1-1 is to be replenished with 1 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-4.",
Logger.ToString().Trim());

				var transferLines = Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
				AssertNotNull(transferLines);
				AssertEquals(21m, transferLines.Sum(tr => tr.WE_TransactionQuantity + tr.MatchingLines.Sum(t => t.WE_TransactionQuantity)));
			}
			else
			{
				AssertEquals(
@"Information|Transfer W00000006: was created successfully.
Information|Pick Face: A-1-1 is to be replenished with 20 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-4.",
			Logger.ToString().Trim());

				var transferLine = Factory.LoadTop1<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
				AssertNotNull(transferLine);
				AssertEquals(20m, transferLine.WE_TransactionQuantity + transferLine.MatchingLines.Sum(t => t.WE_TransactionQuantity));
			}
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);
			AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_GroupMultiplePicksPerArea

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_MultiplePicksPerArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocations = new[] { bulkLocation1, bulkLocation2, bulkLocation3 };

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var part1 = Helper.CreateProduct("PRODUCT1", data.Org1);
			var part2 = Helper.CreateProduct("PRODUCT2", data.Org1);
			var part3 = Helper.CreateProduct("PRODUCT3", data.Org1);
			Helper.CreateProductParamsByWhsAndClient(part1, data.Org1, data.Whs1).W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Helper.CreateProductParamsByWhsAndClient(part2, data.Org1, data.Whs1).W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Helper.CreateProductParamsByWhsAndClient(part3, data.Org1, data.Whs1).W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part1, 5m, bulkLocation1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, part2, 5m, bulkLocation2, "");
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 5m, bulkLocation3, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var products = new[] { part1, part2, part3 };
			var picks = new WhsPick[products.Length];
			for (int i = 0; i < products.Length; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i + 1}", products[i], 5m);

				picks[i] = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
				var orderedInventory = picks[i].OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
					AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
					AssertEquals("Pick should be waiting for replenishment", true, picks[i].WP_IsAwaitingReplenishment);
				});
			}

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			CombineAssertions(() =>
			{
				for (int i = 0; i < products.Length; i++)
				{
					var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation, products[i])).Single();
					AssertEquals("Transfer WD_WP_PickBeingReplenished correct", picks[i].PK, transfer.WD_WP_PickBeingReplenished);
					AssertContains($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.", Logger.ToString().Trim());

					AssertEquals("Number of transfer lines correct.", 1, transfer.Lines.Count);
					var lineForProduct = (WhsTransferLine)transfer.Lines.Single();
					AssertTransferLine(transfer, lineForProduct, dynamicLocation, data.Org1, data.Whs1, products[i], 5m, ZDate.Empty, ZDate.Empty);
					AssertEquals(bulkLocations[i], lineForProduct.TransferFromLocation);
					AssertContains($"Information|Pick Face: A-4 is to be replenished with 5 Product: PRODUCT{i + 1} for Client: 111 from Warehouse: 1 Location: A-{i + 1}.", Logger.ToString().Trim());
				}
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_MultipleDynamicAreas

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_MultipleDynamicAreas()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);

			var bulkLocations = new[]
			{
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3")
			};

			var numberOfProducts = bulkLocations.Length;
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicAreas = new WhsArea[numberOfProducts];
			var dynamicLocations = new WhsLocation[numberOfProducts];

			for (int i = 0; i < numberOfProducts; i++)
			{
				dynamicAreas[i] = Helper.CreateArea(data.Whs1, $"DYNAMIC{i + 1}", AreaTypes.Codes.DynamicPickFace, isPutawayArea: false);

				var location = data.Whs1.FindLocation($"A-{i + 4}");
				location.WLV_WLT_LocationType = dynamicLocationType.PK;
				location.WLV_WA_PickingArea = dynamicAreas[i].PK;
				dynamicLocations[i] = location;
			}

			Factory.Save();

			var products = new OrgSupplierPart[numberOfProducts];

			for (int i = 0; i < numberOfProducts; i++)
			{
				products[i] = Helper.CreateProduct($"PRODUCT{i + 1}", data.Org1);
				var productParams = Helper.CreateProductParamsByWhsAndClient(products[i], data.Org1, data.Whs1);
				productParams.W3_WA_DynamicPickFaceArea = dynamicAreas[i].PK;
			}

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			for (int i = 0; i < numberOfProducts; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, products[i], 5m, bulkLocations[i], "");
			}
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var picks = new WhsPick[numberOfProducts];
			for (int i = 0; i < numberOfProducts; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i + 1}", products[i], 5m);
				picks[i] = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
				var orderedInventories = picks[i].OrderedInventories.Cast<WhsPickOrderedInventory>();
				var orderedInventory = orderedInventories.Single();

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
					AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
					AssertEquals("Pick should be waiting for replenishment", true, picks[i].WP_IsAwaitingReplenishment);
				});
			}

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			CombineAssertions(() =>
			{
				for (int i = 0; i < numberOfProducts; i++)
				{
					var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocations[i])).Single();
					AssertEquals("Transfer WD_WP_PickBeingReplenished correct", picks[i].PK, transfer.WD_WP_PickBeingReplenished);
					AssertSingleTransferLine(transfer, bulkLocations[i], dynamicLocations[i], data.Org1, data.Whs1, products[i], 5m, ZDate.Empty, ZDate.Empty);
					AssertContains($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-{i + 4} is to be replenished with 5 Product: PRODUCT{i + 1} for Client: 111 from Warehouse: 1 Location: A-{i + 1}.", Logger.ToString().Trim());
				}
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AreaWithMultipleLocations_TransfersToLeastFullLocation

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AreaWithMultipleLocations_TransfersToLeastFullLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation = data.Whs1.FindLocation("A-1");

			var dynamicLocation1 = data.Whs1.FindLocation($"A-2");
			var dynamicLocation2 = data.Whs1.FindLocation($"A-3");
			var dynamicLocation3 = data.Whs1.FindLocation($"A-4");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation3.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;
			dynamicLocation3.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var dynamicPart = data.Part1;
			var productParams1 = Helper.CreateProductParamsByWhsAndClient(dynamicPart, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var fillerPart = data.Part2;
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(fillerPart, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, $"R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicPart, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive, fillerPart, 5m, dynamicLocation1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, fillerPart, 5m, dynamicLocation2, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", dynamicPart, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer to empty location
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation3)).Single();
			AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation3, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);

			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-4 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AreaWithNoEmptyLocations

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AreaWithNoEmptyLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");

			var dynamicLocation1 = data.Whs1.FindLocation($"A-2");
			var dynamicLocation2 = data.Whs1.FindLocation($"A-3");
			var dynamicLocation3 = data.Whs1.FindLocation($"A-4");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation3.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;
			dynamicLocation3.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var dynamicPart = data.Part1;
			var productParams1 = Helper.CreateProductParamsByWhsAndClient(dynamicPart, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var fillerPart = data.Part2;
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(fillerPart, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicPart, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive, fillerPart, 5m, dynamicLocation1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, fillerPart, 2m, dynamicLocation2, "");
			Helper.CreateWhsReceiveInventoryLine(receive, fillerPart, 10m, dynamicLocation3, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", dynamicPart, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer to non-empty location
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation2)).Single();
			AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation2, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);

			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-3 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_IgnoreLocationThatIsBothDynamicAndFixed

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_IgnoreLocationThatIsBothDynamicAndFixed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFifoRule(ruleSet, 10);

			var dynamicPart = data.Part1;
			var fillerPart = data.Part2;

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			var fixedAndDynamicLocation = data.Whs1.FindLocation("A-3");
			Helper.CreateProductPickFace(dynamicPart, data.Org1, fixedAndDynamicLocation); // fixed PLUS dynamic is not a valid use case, but we test this to make sure it's handled gracefully
			Factory.Save();

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(dynamicPart, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var productParams2 = Helper.CreateProductParamsByWhsAndClient(fillerPart, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicPart, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicPart, 5m, fixedAndDynamicLocation, ""); //Raise above pick face replenishment min
			Helper.CreateWhsReceiveInventoryLine(receive, fillerPart, 10m, dynamicLocation, "");

			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", dynamicPart, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			// Use direct SQL to add a fixed PickFace to a Dynamic area (cannot be done via business layer as it is guarded against).
			CargoWise.Database.TestFramework.ObjectModel.WhsLocationView.UpdateWhere(fixedAndDynamicLocation.PK.ToGuid())
					.Set(l => l.WLV_WA_PickingArea, dynamicArea.PK.ToGuid())
					.Post(Db.Connection);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer to the normal dynamic location (even though the other is less full)
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);
			AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, dynamicPart, 5m, ZDate.Empty, ZDate.Empty);
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_IgnoreLocationThatIsAlsoFixed_EvenWhenNoOtherLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFifoRule(ruleSet, 10);

			var dynamicPart = data.Part1;
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var fixedAndDynamicLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(dynamicPart, data.Org1, fixedAndDynamicLocation);
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(dynamicPart, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicPart, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicPart, 5m, fixedAndDynamicLocation, ""); //Raise above pick face replenishment min

			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create transfer to ensure this is also ignored
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, bulkLocation, fixedAndDynamicLocation);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is committed.", 5m, transferLine.QtyCommittedIncludingMatchingLines);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", dynamicPart, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			CargoWise.Database.TestFramework.ObjectModel.WhsLocationView.UpdateWhere(fixedAndDynamicLocation.PK.ToGuid())
					.Set(l => l.WLV_WA_PickingArea, dynamicArea.PK.ToGuid())
					.Post(Db.Connection);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var createdTransfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, fixedAndDynamicLocation)).Where(t => t.PK != transfer.PK);
			var logs = Logger.ToString().Trim();

			// No dynamic locations available for transfer
			CombineAssertions(() =>
			{
				AssertEquals("No transfer should be created", 0, createdTransfer.Count());

				AssertNotContains("Information|Transfer W00000004: was created successfully.", logs);
				AssertNotContains("Information|Pick Face: A-3 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1", logs);
				AssertContains("Error|Product: P1 for Client: 111 from Warehouse 1 needs replenishment, but there are no locations available to transfer to.", logs);
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectAttributes

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.One, true);
			var productParams = Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation1, ZDate.Empty, ZDate.Empty, "Red", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation2, ZDate.Empty, ZDate.Empty, "Blue", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation3, ZDate.Empty, ZDate.Empty, "Green", "", "", "");
			receive.FinaliseDocket();

			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 5m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer correct attributes
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);
			AssertSingleTransferLine(transfer, bulkLocation2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "Blue", "", "");
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-4 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectAttributes_ExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.ExpiryDate, true);
			var productParams = Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation1, today.AddDays(-5), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation2, today, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation3, today.AddDays(5), ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 5m, today, ZDate.Empty, "", "", "", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer correct attributes
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertSingleTransferLine(transfer, bulkLocation2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, today);
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-4 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectAttributes_PackingDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			dynamicArea.WA_IsPickingArea = true;
			dynamicArea.WA_IsPutawayArea = false;
			dynamicArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.PackingDate, true);
			var productParams = Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation1, ZDate.Empty, today.AddDays(-5), "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation2, ZDate.Empty, today.AddDays(-10), "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation3, ZDate.Empty, today.AddDays(-15), "", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 5m, ZDate.Empty, today.AddDays(-15), "", "", "", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer correct attributes
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertSingleTransferLine(transfer, bulkLocation3, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, today.AddDays(-15), ZDate.Empty);
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-4 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectAttributes_MultipleAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 8, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");
			var bulkLocation5 = data.Whs1.FindLocation("A-5");
			var bulkLocation6 = data.Whs1.FindLocation("A-6");

			var dynamicLocation = data.Whs1.FindLocation("A-7");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, product, use: true, setReleaseCaptured: false, useSerialNumber: false);
			var productParams = Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today.ToZDateTime().ToOffset(), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation1, today.AddDays(5), today.AddDays(-5), "red", "large", "old", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation2, today.AddDays(5), today.AddDays(-5), "blue", "large", "old", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation3, today.AddDays(5), today.AddDays(-5), "red", "small", "old", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation4, today.AddDays(5), today.AddDays(-5), "red", "large", "new", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation5, today.AddDays(5), today.AddDays(-10), "red", "large", "old", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation6, today.AddDays(10), today.AddDays(-5), "red", "large", "old", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 5m, today.AddDays(5), today.AddDays(-5), "red", "large", "new", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer all correct attributes
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertSingleTransferLine(transfer, bulkLocation4, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, today.AddDays(-5), today.AddDays(5), "red", "large", "new");
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-7 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-4.", Logger.ToString().Trim());
		}

		#endregion

		#region  TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_MultiplePicks

		public void TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_MultiplePicks_ForSameInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, bulkLocation1, "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, bulkLocation2, "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 7m, bulkLocation3, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 6m);

			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);

			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var orderedInventory2 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("Pick1: PickLineQuantity", 0m, orderedInventory1.PickLineQuantity);
				AssertEquals("Pick1: QuantityShort", 4m, orderedInventory1.QuantityShort);
				AssertEquals("Pick1: Pick should be waiting for replenishment", true, pick1.WP_IsAwaitingReplenishment);

				AssertEquals("Pick2: PickLineQuantity", 0m, orderedInventory2.PickLineQuantity);
				AssertEquals("Pick2: QuantityShort", 6m, orderedInventory2.QuantityShort);
				AssertEquals("Pick2: Pick should be waiting for replenishment", true, pick2.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
			var transfer1 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick1.PK);
			AssertEquals(2, transfer1.Lines.Count);
			var line1 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var line2 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 1m);

			var transfer2 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick2.PK);
			AssertEquals(1, transfer2.Lines.Count);
			var line3 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 6m);

			// Transfers are made from locations in descending order of most available inventory
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertEquals("Line 1 location correct", bulkLocation2.PK, line1.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 3m, ZDate.Empty, ZDate.Empty);
				AssertEquals("Line 2 location correct", bulkLocation3.PK, line2.WE_WL_TransferFrom);
				AssertTransferLine(transfer2, line2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 1m, ZDate.Empty, ZDate.Empty);

				AssertEquals("Line 3 location correct", bulkLocation3.PK, line2.WE_WL_TransferFrom);
				AssertTransferLine(transfer2, line3, dynamicLocation, data.Org1, data.Whs1, data.Part1, 6m, ZDate.Empty, ZDate.Empty);

				AssertContains($"Information|Transfer {transfer1.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 6 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", logs);

				AssertContains($"Information|Transfer {transfer2.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 1 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 3 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", logs);
			});
		}

		public void TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_MultipleOrders_ForSameInventory_InsufficientQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, bulkLocation1, "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, bulkLocation2, "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 7m, bulkLocation3, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 7m);

			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);

			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var orderedInventory2 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("Pick1: PickLineQuantity", 0m, orderedInventory1.PickLineQuantity);
				AssertEquals("Pick1: QuantityShort", 10m, orderedInventory1.QuantityShort);
				AssertEquals("Pick1: Pick should be waiting for replenishment", true, pick1.WP_IsAwaitingReplenishment);

				AssertEquals("Pick2: PickLineQuantity", 0m, orderedInventory2.PickLineQuantity);
				AssertEquals("Pick2: QuantityShort", 7m, orderedInventory2.QuantityShort);
				AssertEquals("Pick2: Pick should be waiting for replenishment", true, pick2.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
			var transfer1 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick1.PK);
			AssertEquals(2, transfer1.Lines.Count);

			var transfer2 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick2.PK);
			AssertEquals(2, transfer2.Lines.Count);

			var line1 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var line2 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 7m);
			var line3 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 2m);
			var line4 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);

			// Just transfer what we can
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertEquals("Line 1 pick location is correct.", bulkLocation2.PK, line1.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 3m, ZDate.Empty, ZDate.Empty);
				AssertEquals("Line 2 pick location is correct.", bulkLocation3.PK, line2.WE_WL_TransferFrom);
				AssertTransferLine(transfer2, line2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty);

				AssertEquals("Line 3 pick location is correct.", bulkLocation2.PK, line3.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line3, dynamicLocation, data.Org1, data.Whs1, data.Part1, 2m, ZDate.Empty, ZDate.Empty);
				AssertEquals("Line 4 pick location is correct.", bulkLocation1.PK, line4.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line4, dynamicLocation, data.Org1, data.Whs1, data.Part1, 3m, ZDate.Empty, ZDate.Empty);

				AssertContains($"Information|Transfer {transfer1.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 3 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", logs);

				AssertContains($"Information|Transfer {transfer2.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 2 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 3 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
			});
		}

		public void TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_MultipleOrders_ForDifferentAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");

			var dynamicLocation = data.Whs1.FindLocation("A-5");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, bulkLocation1, ZDate.Empty, ZDate.Empty, "Red", "Small", "", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 8m, bulkLocation2, ZDate.Empty, ZDate.Empty, "Red", "Small", "", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, bulkLocation3, ZDate.Empty, ZDate.Empty, "Blue", "Large", "", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, bulkLocation4, ZDate.Empty, ZDate.Empty, "Blue", "Large", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 11m, ZDate.Empty, ZDate.Empty, "Red", "", "", "", "");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 7m, ZDate.Empty, ZDate.Empty, "", "Large", "", "", "");

			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);

			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var orderedInventory2 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("Pick1: PickLineQuantity", 0m, orderedInventory1.PickLineQuantity);
				AssertEquals("Pick1: QuantityShort", 11m, orderedInventory1.QuantityShort);
				AssertEquals("Pick1: Pick should be waiting for replenishment", true, pick1.WP_IsAwaitingReplenishment);

				AssertEquals("Pick2: PickLineQuantity", 0m, orderedInventory2.PickLineQuantity);
				AssertEquals("Pick2: QuantityShort", 7m, orderedInventory2.QuantityShort);
				AssertEquals("Pick2: Pick should be waiting for replenishment", true, pick2.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
			AssertEquals("Number of transfers correct", 2, transfers.Length);

			var transfer1 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick1.PK);
			AssertEquals(2, transfer1.Lines.Count);

			var transfer2 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick2.PK);
			AssertEquals(1, transfer2.Lines.Count);

			var line1 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var line2 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 8m);
			var line3 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 7m);

			// Transfers are made from locations in descending order of most available inventory
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertEquals(bulkLocation1.PK, line1.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "Red", "Small");
				AssertEquals(bulkLocation2.PK, line2.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 8m, ZDate.Empty, ZDate.Empty, "Red", "Small");
				AssertEquals(bulkLocation4.PK, line3.WE_WL_TransferFrom);
				AssertTransferLine(transfer2, line3, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty, "Blue", "Large");

				AssertContains($"Information|Transfer {transfer1.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-5 is to be replenished with 3 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
				AssertContains("Information|Pick Face: A-5 is to be replenished with 8 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", logs);
				AssertContains($"Information|Transfer {transfer2.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-5 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-4.", logs);
			});
		}

		public void TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_MultipleOrders_ForDifferentAttributes_InsufficientQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");

			var dynamicLocation = data.Whs1.FindLocation("A-5");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, bulkLocation1, ZDate.Empty, ZDate.Empty, "Red", "Small", "", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 8m, bulkLocation2, ZDate.Empty, ZDate.Empty, "Red", "Small", "", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, bulkLocation3, ZDate.Empty, ZDate.Empty, "Blue", "Large", "", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, bulkLocation4, ZDate.Empty, ZDate.Empty, "Blue", "Large", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 15m, ZDate.Empty, ZDate.Empty, "Red", "", "", "", "");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 15m, ZDate.Empty, ZDate.Empty, "", "Large", "", "", "");

			var pick1 = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			var pick2 = Helper.CreatePickNew_WithoutAllocationEngineMock(order2);

			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var orderedInventory2 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("Pick1: PickLineQuantity", 0m, orderedInventory1.PickLineQuantity);
				AssertEquals("Pick1: QuantityShort", 15m, orderedInventory1.QuantityShort);
				AssertEquals("Pick1: Pick should be waiting for replenishment", true, pick1.WP_IsAwaitingReplenishment);

				AssertEquals("Pick2: PickLineQuantity", 0m, orderedInventory2.PickLineQuantity);
				AssertEquals("Pick2: QuantityShort", 15m, orderedInventory2.QuantityShort);
				AssertEquals("Pick2: Pick should be waiting for replenishment", true, pick2.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
			var transfer1 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick1.PK);
			AssertEquals(2, transfer1.Lines.Count);
			var line1 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 5m);
			var line2 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 8m);

			var transfer2 = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick2.PK);
			AssertEquals(2, transfer2.Lines.Count);
			var line3 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 2m);
			var line4 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 10m);

			// Transfers are made from locations in descending order of most available inventory
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertEquals(bulkLocation1.PK, line1.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "Red", "Small");
				AssertEquals(bulkLocation2.PK, line2.WE_WL_TransferFrom);
				AssertTransferLine(transfer1, line2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 8m, ZDate.Empty, ZDate.Empty, "Red", "Small");
				AssertEquals(bulkLocation3.PK, line3.WE_WL_TransferFrom);
				AssertTransferLine(transfer2, line3, dynamicLocation, data.Org1, data.Whs1, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "Blue", "Large");
				AssertEquals(bulkLocation4.PK, line4.WE_WL_TransferFrom);
				AssertTransferLine(transfer2, line4, dynamicLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "Blue", "Large");

				AssertContains($"Information|Transfer {transfer1.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-5 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
				AssertContains("Information|Pick Face: A-5 is to be replenished with 8 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", logs);

				AssertContains($"Information|Transfer {transfer2.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-5 is to be replenished with 2 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", logs);
				AssertContains("Information|Pick Face: A-5 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-4.", logs);
			});
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_MultipleOrders_WithCompetingAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 8, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");
			var bulkLocation5 = data.Whs1.FindLocation("A-5");
			var bulkLocation6 = data.Whs1.FindLocation("A-6");

			var dynamicLocation = data.Whs1.FindLocation("A-7");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.Three, true);

			var productParams = Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 5m, bulkLocation1, ZDate.Empty, ZDate.Empty, "red", "large", "old", ""); // This inventory will be contested, see below assertions
			Helper.CreateWhsReceiveInventoryLine(receive, product, 5m, bulkLocation2, ZDate.Empty, ZDate.Empty, "red", "small", "old", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 5m, bulkLocation3, ZDate.Empty, ZDate.Empty, "blue", "large", "old", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 5m, bulkLocation4, ZDate.Empty, ZDate.Empty, "red", "small", "new", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 5m, bulkLocation5, ZDate.Empty, ZDate.Empty, "blue", "small", "new", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 5m, bulkLocation6, ZDate.Empty, ZDate.Empty, "blue", "small", "old", "");

			receive.FinaliseDocket();

			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 10m, ZDate.Empty, ZDate.Empty, "red", "", "", "", "");
			Helper.CreateWhsOrderLine(order, product, 10m, ZDate.Empty, ZDate.Empty, "", "large", "", "", "");
			Helper.CreateWhsOrderLine(order, product, 5m, ZDate.Empty, ZDate.Empty, "blue", "", "new", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", true, orderedInventories.All(l => l.PickLineQuantity == 0m));
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer all correct attributes
			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
			var transfer = transfers.Single();

			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-7 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
				AssertContains("Information|Pick Face: A-7 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", logs);
				AssertContains("Information|Pick Face: A-7 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", logs);
				AssertContains("Information|Pick Face: A-7 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-4.", logs);
				AssertContains("Information|Pick Face: A-7 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-5.", logs);
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectlyCombinesMultipleOrderLines

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectlyCombinesMultipleOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.ManualWithAutoAllocate);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part1, 7m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single().PickLineQuantity += 4m;
			var releaseLine1 = orderline1.ReleaseLines.Cast<WhsReleaseLine>().Single(l => l.OrderedQuantity == 3m);
			releaseLine1.Quantity = 1m;
			var releaseLine2 = orderline2.ReleaseLines.Cast<WhsReleaseLine>().Single(l => l.OrderedQuantity == 7m);
			releaseLine2.Quantity = 3m;

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 4m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 6m, orderedInventory.QuantityShort);

				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Transfer should only have one line even though there are multiple order/pick lines with shortfall
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 6m, ZDate.Empty, ZDate.Empty);
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-2 is to be replenished with 6 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectlyCombinesMultipleOrderLines_WithOrderedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, bulkLocation1, ZDate.Empty, ZDate.Empty, "red", "", "", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, bulkLocation2, ZDate.Empty, ZDate.Empty, "blue", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.ManualWithAutoAllocate);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "red", "", "", "", "");
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part1, 7m, ZDate.Empty, ZDate.Empty, "red", "", "", "", "");
			var orderline3 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "blue", "", "", "", "");
			var orderline4 = Helper.CreateWhsOrderLine(order, data.Part1, 6m, ZDate.Empty, ZDate.Empty, "blue", "", "", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInventories.Single(l => l.PartAttrib1 == "red");
			var orderedInventory2 = orderedInventories.Single(l => l.PartAttrib1 == "blue");

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity1", 0m, orderedInventory1.PickLineQuantity);
				AssertEquals("QuantityShort1", 10m, orderedInventory1.QuantityShort);
				AssertEquals("PickLineQuantity2", 0m, orderedInventory2.PickLineQuantity);
				AssertEquals("QuantityShort2", 10m, orderedInventory2.QuantityShort);

				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single().PickLineQuantity += 3m;
			orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single().PickLineQuantity += 6m;

			var releaseLine1 = orderline1.ReleaseLines.Cast<WhsReleaseLine>().Single();
			releaseLine1.Quantity = 1m;
			var releaseLine2 = orderline2.ReleaseLines.Cast<WhsReleaseLine>().Single();
			releaseLine2.Quantity = 2m;
			var releaseLine3 = orderline3.ReleaseLines.Cast<WhsReleaseLine>().Single();
			releaseLine3.Quantity = 2m;
			var releaseLine4 = orderline4.ReleaseLines.Cast<WhsReleaseLine>().Single();
			releaseLine4.Quantity = 4m;

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity1", 3m, orderedInventory1.PickLineQuantity);
				AssertEquals("QuantityShort1", 7m, orderedInventory1.QuantityShort);
				AssertEquals("PickLineQuantity2", 6m, orderedInventory2.PickLineQuantity);
				AssertEquals("QuantityShort2", 4m, orderedInventory2.QuantityShort);

				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Transfer should only have one line even though there are multiple order/pick lines with shortfall
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);
			AssertEquals(2, transfer.Lines.Count);

			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				var line1 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 7);
				var line2 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 4);

				AssertTransferLine(transfer, line1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty, "red");
				AssertEquals(line1.WE_WL_TransferFrom, bulkLocation1.PK);
				AssertTransferLine(transfer, line2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "blue");
				AssertEquals(line2.WE_WL_TransferFrom, bulkLocation2.PK);

				AssertContains($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-3 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
				AssertContains("Information|Pick Face: A-3 is to be replenished with 4 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", logs);
			});
		}

		#endregion TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectlyCombinesOrderLines

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AttributesNotAvailable

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AttributesNotAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.One, true);
			var productParams = Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation1, ZDate.Empty, ZDate.Empty, "Red", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation2, ZDate.Empty, ZDate.Empty, "Blue", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation3, ZDate.Empty, ZDate.Empty, "Green", "", "", "");
			receive.FinaliseDocket();

			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 5m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var adjust = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			Helper.CreateWhsAdjustmentLine(adjust, data.Part1.PK, -10m, bulkLocation2.ToLocationString(), "Blue", "", "", "", ZDate.Empty, ZDate.Empty);
			adjust.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjust);
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Correct attributes not available
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).SingleOrDefault();
			var logs = Logger.ToString().Trim();

			CombineAssertions(() =>
			{
				AssertNull("No transfer should be created", transfer);

				AssertNotContains("Information|Transfer W00000004: was created successfully.", logs);
				AssertNotContains("Information|Pick Face: A-4 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1", logs);
				AssertContains("Warning|No transfers have been generated to replenish any Pick Faces.", logs);
				AssertContains("Warning|Two possible reasons for this are the Pick Faces don't need replenishment or there is no stock available to transfer.", logs);
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AttributesPartiallyAvailable

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AttributesPartiallyAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.One, true);
			var productParams = Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation1, ZDate.Empty, ZDate.Empty, "Red", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 3m, bulkLocation2, ZDate.Empty, ZDate.Empty, "Blue", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation3, ZDate.Empty, ZDate.Empty, "Green", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 5m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer what stock there is but not touch incorrect attributes
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).SingleOrDefault();
			var logs = Logger.ToString().Trim();

			CombineAssertions(() =>
			{
				AssertSingleTransferLine(transfer, bulkLocation2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 3m, ZDateTime.Empty, ZDateTime.Empty, "Blue");
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 3 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2", logs);
				AssertNotContains("Should not use Incorrect Attribute", "Product: P1 for Client: 111 from Warehouse: 1 Location: A-1", logs);
				AssertNotContains("Should not use Incorrect Attribute", "Product: P1 for Client: 111 from Warehouse: 1 Location: A-3", logs);
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_MixOfFixedAndDynamicTransfers

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_MixOfFixedAndDynamicTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 9, 1);

			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");

			var fixedProduct1 = data.Part1;
			var fixedProduct2 = data.Part2;

			var dynamicProduct1 = Helper.CreateProduct("P3", data.Org1);
			var productParams1 = Helper.CreateProductParamsByWhsAndClient(dynamicProduct1, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;

			var dynamicProduct2 = Helper.CreateProduct("P4", data.Org1);
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(dynamicProduct2, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;

			var fixedLocation1 = data.Whs1.FindLocation("A-5");
			Helper.CreateProductPickFace(fixedProduct1, data.Org1, fixedLocation1, 1m, 10m);

			var fixedLocation2 = data.Whs1.FindLocation("A-6");
			Helper.CreateProductPickFace(fixedProduct2, data.Org1, fixedLocation2, 1m, 10m);

			var dynamicLocation1 = data.Whs1.FindLocation("A-7");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-8");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, fixedProduct1, 10m, bulkLocation1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, fixedProduct2, 10m, bulkLocation2, "");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicProduct1, 10m, bulkLocation3, "");
			Helper.CreateWhsReceiveInventoryLine(receive, dynamicProduct2, 10m, bulkLocation4, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, dynamicProduct1, 5m);
			Helper.CreateWhsOrderLine(order, dynamicProduct2, 5m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();

			CombineAssertions("Preconditions: ", () =>
			{
				foreach (var orderedInventory in orderedInventories)
				{
					AssertEquals($"{orderedInventory.ProductCode}: PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
					AssertEquals($"{orderedInventory.ProductCode}: QuantityShort", 5m, orderedInventory.QuantityShort);
				}
				AssertEquals($"Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			CombineAssertions(() =>
			{
				var fixedTransfer1 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, fixedLocation1)).Single();
				AssertSingleTransferLine(fixedTransfer1, bulkLocation1, fixedLocation1, data.Org1, data.Whs1, fixedProduct1, 10m, ZDate.Empty, ZDate.Empty);
				AssertContains($@"Information|Transfer {fixedTransfer1.WD_DocketID}: was created successfully.", Logger.ToString().Trim());
				AssertContains(@"Information|Pick Face: A-5 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

				var fixedTransfer2 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, fixedLocation2)).Single();
				AssertSingleTransferLine(fixedTransfer2, bulkLocation2, fixedLocation2, data.Org1, data.Whs1, fixedProduct2, 10m, ZDate.Empty, ZDate.Empty);
				AssertContains($@"Information|Transfer {fixedTransfer2.WD_DocketID}: was created successfully.", Logger.ToString().Trim());
				AssertContains($@"Information|Pick Face: A-6 is to be replenished with 10 Product: P2 for Client: 111 from Warehouse: 1 Location: A-2.", Logger.ToString().Trim());

				var dynamicTransfer1 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation1)).Single();
				AssertSingleTransferLine(dynamicTransfer1, bulkLocation3, dynamicLocation1, data.Org1, data.Whs1, dynamicProduct1, 5m, ZDate.Empty, ZDate.Empty);
				AssertContains($@"Information|Transfer {dynamicTransfer1.WD_DocketID}: was created successfully.", Logger.ToString().Trim());
				AssertContains($@"Information|Pick Face: A-7 is to be replenished with 5 Product: P3 for Client: 111 from Warehouse: 1 Location: A-3.", Logger.ToString().Trim());

				var dynamicTransfer2 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation2)).Single();
				AssertSingleTransferLine(dynamicTransfer2, bulkLocation4, dynamicLocation2, data.Org1, data.Whs1, dynamicProduct2, 5m, ZDate.Empty, ZDate.Empty);
				AssertContains($@"Information|Transfer {dynamicTransfer2.WD_DocketID}: was created successfully.", Logger.ToString().Trim());
				AssertContains($@"Information|Pick Face: A-8 is to be replenished with 5 Product: P4 for Client: 111 from Warehouse: 1 Location: A-4.", Logger.ToString().Trim());
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity_AreaAlreadyContainsSomeCorrectInventory

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity_AreaAlreadyContainsSomeCorrectInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");

			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var extraDynamicLocation = data.Whs1.FindLocation("A-3");
			extraDynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			extraDynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 10m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 3m, extraDynamicLocation, "");
			AssertIsFinalisedPrecondition(extraReceive);
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();

			// Existing inventory should be discounted
			CombineAssertions(() =>
			{
				AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty);
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-2 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
			});
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity_AreaAlreadyContainsSomeCorrectInventory_MultipleInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");

			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var extraDynamicLocation1 = data.Whs1.FindLocation("A-4");
			extraDynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			extraDynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;

			var extraDynamicLocation2 = data.Whs1.FindLocation("A-5");
			extraDynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			extraDynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 7m, bulkLocation1, "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, bulkLocation2, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 8m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("Pick: PickLineQuantity", 0m, orderedInventory1.PickLineQuantity);
				AssertEquals("Pick: QuantityShort", 8m, orderedInventory1.QuantityShort);
				AssertEquals("Pick: Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			extraReceive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 2m, extraDynamicLocation1, "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 1m, extraDynamicLocation2, "");
			extraReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(extraReceive);
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
			AssertEquals("Number of transfers correct", 1, transfers.Length);

			var transfer = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick.PK);
			AssertEquals("Sum of transferred qty correct.", 5m, transfer.Lines.Sum(l => l.WE_TransactionQuantity));

			var bulkLocationStrings = new Dictionary<ZGuid, string>
			{
				[bulkLocation1.PK] = bulkLocation1.ToLocationString(),
				[bulkLocation2.PK] = bulkLocation2.ToLocationString()
			};
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				foreach (WhsTransferLine line in transfer.Lines)
				{
					AssertTransferLine(transfer, line, dynamicLocation, data.Org1, data.Whs1, data.Part1, line.WE_TransactionQuantity, ZDate.Empty, ZDate.Empty);
					AssertEquals("Line pick location is correct.", true, bulkLocationStrings.Keys.Contains(line.WE_WL_TransferFrom));
					AssertContains($"Information|Pick Face: A-3 is to be replenished with {line.WE_TransactionQuantity.ToStringTrimZeros()} Product: P1 for Client: 111 from Warehouse: 1 Location: {bulkLocationStrings[line.WE_WL_TransferFrom]}.", logs);
				}
			});
		}

		#endregion

		#region TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity_ExistingDynamicTransferAreAccountedFor

		public void TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity_ExistingDynamicTransferAreAccountedFor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");

			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation1, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 10m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 3m, bulkLocation2, "");
			AssertIsFinalisedPrecondition(extraReceive);
			Factory.Save();

			var extraTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 3m, bulkLocation2, dynamicLocation);
			extraTransfer.RunPreSaveValidation();
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single(t => t.PK != extraTransfer.PK);

			// Existing in-transit inventory should be discounted
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertSingleTransferLine(transfer, bulkLocation1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty);
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-3 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
			});
		}

		public void TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity_ExistingDynamicTransferAreAccountedFor_DifferentLocationInDynamicArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");

			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var extraDynamicLocation = data.Whs1.FindLocation("A-4");
			extraDynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			extraDynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var fillerPart = data.Part2;
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(fillerPart, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation1, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 10m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			extraReceive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 3m, bulkLocation2, "");
			Helper.CreateWhsReceiveLine(extraReceive, fillerPart, 5m, extraDynamicLocation, ""); //Prevent autogenerated transfer from being placed here
			extraReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(extraReceive);
			Factory.Save();

			var extraTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 3m, bulkLocation2, extraDynamicLocation);
			extraTransfer.RunPreSaveValidation();
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();

			// Existing in-transit inventory should be discounted
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertSingleTransferLine(transfer, bulkLocation1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty);
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-3 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
			});
		}

		public void TestCreateTransfersForPickFaceReplenishment_DynamicPickFaceArea_CorrectTransferQuantity_ExistingDynamicTransfersAreAccountedFor_MultipleTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");

			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, bulkLocation1, "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 27m, bulkLocation2, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 31m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order1);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("Pick: PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("Pick: QuantityShort", 31m, orderedInventory.QuantityShort);
				AssertEquals("Pick: Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			extraReceive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 12m, bulkLocation1, "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 3m, bulkLocation2, "");
			extraReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(extraReceive);
			Factory.Save();

			var extraTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 12m, bulkLocation1, dynamicLocation);
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 3m, bulkLocation2, dynamicLocation);
			extraTransfer.RunPreSaveValidation();
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Where(t => t.PK != extraTransfer.PK);
			AssertEquals("Number of transfers correct", 1, transfers.Count());

			var transfer = transfers.Single(t => t.WD_WP_PickBeingReplenished == pick.PK);
			AssertEquals("Transfer transferlines transaction qty sum correct.", 16m, transfer.Lines.Sum(l => l.WE_TransactionQuantity));

			var bulkLocationStrings = new Dictionary<ZGuid, string>
			{
				[bulkLocation1.PK] = bulkLocation1.ToLocationString(),
				[bulkLocation2.PK] = bulkLocation2.ToLocationString()
			};
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				foreach (WhsTransferLine line in transfer.Lines)
				{
					AssertTransferLine(transfer, line, dynamicLocation, data.Org1, data.Whs1, data.Part1, line.WE_TransactionQuantity, ZDate.Empty, ZDate.Empty);
					AssertEquals("Line pick location is correct.", true, bulkLocationStrings.Keys.Contains(line.WE_WL_TransferFrom));
					AssertContains($"Information|Pick Face: A-3 is to be replenished with {line.WE_TransactionQuantity.ToStringTrimZeros()} Product: P1 for Client: 111 from Warehouse: 1 Location: {bulkLocationStrings[line.WE_WL_TransferFrom]}.", logs);
				}
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickfaceArea_CorrectTransferQuantity_AccountForExistingStockAndTransfers_WhereSingleInventorySatifiesOneShortfallAndPartOfAnotherShortfall

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickfaceArea_CorrectTransferQuantity_AccountForExistingStockAndTransfers_WhereSingleInventorySatifiesOneShortfallAndPartOfAnotherShortfall()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, bulkLocation1, "", ZDate.Empty, ZDate.Empty, "Blue", "z", "z", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, bulkLocation2, "", ZDate.Empty, ZDate.Empty, "Blue", "Large", "z", "");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "Blue", "Large", "", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(2, orderedInventories.Count());

				AssertEquals("PickLineQuantity", 0m, orderedInventories.Sum(i => i.PickLineQuantity));
				AssertEquals("QuantityShort", 40m, orderedInventories.Sum(i => i.QuantityShort));
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			extraReceive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 30m, dynamicLocation, ZDate.Empty, ZDate.Empty, "Blue", "Large", "New", "");
			extraReceive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(extraReceive);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals(1, transfer.Lines.Count);
			// Existing inventory should be discounted
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				// Since 30 'Blue' & 'Large' is already in the dynamic Area, only 10 need to be transferred. Since the Blue & Large shortfall takes precedence
				// the remaining 10 in shortfall is actually the the just Blue Shortfall, evidenced by the the replenishment only taking the Blue Stock from A-1
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-3 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickfaceArea_CorrectTransferQuantity_AccountForExistingStockAndTransfers_WithConflictingAttributes

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickfaceArea_CorrectTransferQuantity_AccountForExistingStockAndTransfers_WithConflictingAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");

			var tempLocation = data.Whs1.FindLocation("A-5");

			var dynamicLocation = data.Whs1.FindLocation("A-6");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation1, "", ZDate.Empty, ZDate.Empty, "Blue", "-", "-", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation2, "", ZDate.Empty, ZDate.Empty, "-", "Large", "-", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation3, "", ZDate.Empty, ZDate.Empty, "Blue", "Large", "-", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation4, "", ZDate.Empty, ZDate.Empty, "Red", "-", "Old", "");

			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 25m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 25m, ZDate.Empty, ZDate.Empty, "", "Large", "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "Blue", "Large", "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "Red", "", "Old", "", "");

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(4, orderedInventories.Count());

				AssertEquals("PickLineQuantity", 0m, orderedInventories.Sum(i => i.PickLineQuantity));
				AssertEquals("QuantityShort", 90m, orderedInventories.Sum(i => i.QuantityShort));
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			extraReceive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Red", "Large", "Old", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Red", "Small", "Old", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Blue", "Large", "New", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Blue", "Giant", "Old", "");

			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, dynamicLocation, ZDate.Empty, ZDate.Empty, "Blue", "Small", "New", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, dynamicLocation, ZDate.Empty, ZDate.Empty, "Red", "Large", "New", "");
			extraReceive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(extraReceive);

			var extraTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Red", "Large", "Old");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Red", "Small", "Old");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Blue", "Large", "New");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Blue", "Giant", "Old");
			extraTransfer.RunPreSaveValidation();
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single(t => t.PK != extraTransfer.PK);
			AssertEquals(4, transfer.Lines.Count);
			// Existing inventory should be discounted
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-6 is to be replenished with 15 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-6 is to be replenished with 20 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-6 is to be replenished with 15 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-6 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-4.", Logger.ToString().Trim());
			});
		}

		#endregion

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_StockFromOtherDynamicLocationsIgnored

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_StockFromOtherDynamicLocationsIgnored()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var dynamicLocation1 = data.Whs1.FindLocation("A-1");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-2");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;

			var bulkLocation = data.Whs1.FindLocation("A-3");

			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 2m, bulkLocation, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 3m, dynamicLocation1, "");
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation2)).SingleOrDefault();
			var logs = Logger.ToString().Trim();

			// Should ignore stock already in dynamic locations
			CombineAssertions(() =>
			{
				AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation2, data.Org1, data.Whs1, data.Part1, 2m, ZDateTime.Empty, ZDateTime.Empty);
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertNotContains("Information|Pick Face: A-2 is to be replenished with 3 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1", logs);
				AssertContains("Information|Pick Face: A-2 is to be replenished with 2 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3", logs);
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AreaContainsNoLocations

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AreaContainsNoLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			CargoWise.Database.TestFramework.ObjectModel.WhsLocation.DeleteInDB(Db.Connection, dynamicLocation.PK.ToGuid());

			var newFactory = new BusinessObjectFactory();
			AssertNull("Precondition: location deleted", newFactory.Load<WhsLocation>(dynamicLocation.PK));

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// No locations available in dynamic area so cannot transfer
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).SingleOrDefault();
			var logs = Logger.ToString().Trim();

			CombineAssertions(() =>
			{
				AssertNull("No transfer should be created", transfer);

				AssertNotContains($"Information|Transfer W00000003: was created successfully.", logs);
				AssertNotContains("Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1", logs);
				AssertContains("Error|Product: P1 for Client: 111 from Warehouse 1 needs replenishment, but there are no locations available to transfer to.", logs);
			});
		}

		[TestDate(2018, 6, 20)]
		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_AreaContainsNoLocations_DoesNotBlockOtherTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");

			var dynamicLocation1 = data.Whs1.FindLocation("A-3");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-4");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			Factory.Save();

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;

			var productParams2 = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, bulkLocation2, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			CargoWise.Database.TestFramework.ObjectModel.WhsLocation.DeleteInDB(Db.Connection, dynamicLocation1.PK.ToGuid());

			var newFactory = new BusinessObjectFactory();
			AssertNull("Precondition: location deleted", newFactory.Load<WhsLocation>(dynamicLocation1.PK));

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventories[0].PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventories[0].QuantityShort);

				AssertEquals("PickLineQuantity", 0m, orderedInventories[1].PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventories[1].QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// No locations available in dynamic area so cannot transfer, but other transfer should continue as normal
			var transfer1 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation1)).SingleOrDefault();
			var transfer2 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation2)).SingleOrDefault();

			var logs = Logger.ToString().Trim();

			CombineAssertions(() =>
			{
				AssertNull("No transfer should be created", transfer1);
				AssertSingleTransferLine(transfer2, bulkLocation2, dynamicLocation2, data.Org1, data.Whs1, data.Part2, 5m, ZDateTime.Empty, ZDateTime.Empty);
				AssertContains($"Information|Transfer W00000003: was created successfully.", logs);
				AssertNotContains($"Information|Transfer W0000004: was created successfully.", logs);

				AssertNotContains("Information|Pick Face: A-3 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1", logs);
				AssertContains("Information|Pick Face: A-4 is to be replenished with 5 Product: P2 for Client: 111 from Warehouse: 1 Location: A-2", logs);

				AssertContains("Error|Product: P1 for Client: 111 from Warehouse 1 needs replenishment, but there are no locations available to transfer to.", logs);
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Factory.Save();

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItems();
			Factory.Save();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);
			AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts_MultipleProductsOrdersAreas()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);

			var bulkLocations = new[]
			{
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3")
			};

			var numberOfProducts = bulkLocations.Length;
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicAreas = new WhsArea[numberOfProducts];
			var dynamicLocations = new WhsLocation[numberOfProducts];

			for (int m = 0; m < numberOfProducts; m++)
			{
				dynamicAreas[m] = Helper.CreateArea(data.Whs1, $"DYNAMIC{m + 1}", AreaTypes.Codes.DynamicPickFace, isPutawayArea: false);

				var location = data.Whs1.FindLocation($"A-{m + 4}");
				location.WLV_WLT_LocationType = dynamicLocationType.PK;
				location.WLV_WA_PickingArea = dynamicAreas[m].PK;
				dynamicLocations[m] = location;
			}

			Factory.Save();

			var products = new OrgSupplierPart[numberOfProducts];

			for (int h = 0; h < numberOfProducts; h++)
			{
				products[h] = Helper.CreateProduct($"PRODUCT{h + 1}", data.Org1);
			}

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			for (int b = 0; b < numberOfProducts; b++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, products[b], 5m, bulkLocations[b], "");
			}
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var picks = new WhsPick[numberOfProducts];
			for (int f = 0; f < numberOfProducts; f++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{f + 1}", products[f], 5m);
				picks[f] = Factory.New<WhsPick>();
				picks[f].WP_WW_Whs = data.Whs1.PK;
				picks[f].WP_WA_DynamicPickAreaOverride = dynamicAreas[f].PK;
				Factory.Save();

				picks[f].AddOrders(new[] { order });
				picks[f].AutoAllocateItems();
				Factory.Save();
				var orderedInventories = picks[f].OrderedInventories.Cast<WhsPickOrderedInventory>();
				var orderedInventory = orderedInventories.Single();

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
					AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
					AssertEquals("Pick should be waiting for replenishment", true, picks[f].WP_IsAwaitingReplenishment);
				});
			}

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			CombineAssertions(() =>
			{
				for (int i = 0; i < numberOfProducts; i++)
				{
					var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocations[i])).Single();
					AssertEquals("Transfer WD_WP_PickBeingReplenished correct", picks[i].PK, transfer.WD_WP_PickBeingReplenished);
					AssertSingleTransferLine(transfer, bulkLocations[i], dynamicLocations[i], data.Org1, data.Whs1, products[i], 5m, ZDate.Empty, ZDate.Empty);
					AssertContains($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-{i + 4} is to be replenished with 5 Product: PRODUCT{i + 1} for Client: 111 from Warehouse: 1 Location: A-{i + 1}.", Logger.ToString().Trim());
				}
			});
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts_MultipleSimilarOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);

			var bulkLocation = data.Whs1.FindLocation("A-1");

			var numberOfProducts = 3;
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = Helper.CreateArea(data.Whs1, $"DYNAMIC", AreaTypes.Codes.DynamicPickFace, isPutawayArea: false);
			var dynamicLocation = data.Whs1.FindLocation("A-5");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var products = new OrgSupplierPart[numberOfProducts];

			for (int i = 0; i < numberOfProducts; i++)
			{
				products[i] = Helper.CreateProduct($"PRODUCT{i + 1}", data.Org1);
			}

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			for (int i = 0; i < numberOfProducts; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, products[i], 5m, bulkLocation, "");
				Helper.CreateWhsReceiveInventoryLine(receive, products[i], 5m, bulkLocation, "");
			}
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var pick1 = Factory.New<WhsPick>();
			pick1.WP_WW_Whs = data.Whs1.PK;
			pick1.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			pick1.WP_PickNo = "111";
			Factory.Save();

			for (int i = 0; i < numberOfProducts; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O1{i + 1}", products[i], 5m);
				pick1.AddOrders(new[] { order });
			}
			pick1.AutoAllocateItems();

			var pick2 = Factory.New<WhsPick>();
			pick2.WP_WW_Whs = data.Whs1.PK;
			pick2.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			pick2.WP_PickNo = "222";
			Factory.Save();

			for (int i = 0; i < numberOfProducts; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O2{i + 1}", products[i], 5m);
				pick2.AddOrders(new[] { order });
			}
			pick2.AutoAllocateItems();

			Factory.Save();

			CombineAssertions("Preconditions: ", () =>
			{
				var orderedInventories1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>();
				foreach (var orderedInventory in orderedInventories1)
				{
					AssertEquals($"Pick 1 - {orderedInventory.ProductCode}: QuantityShort", 5m, orderedInventory.QuantityShort);
					AssertEquals($"Pick 1 - {orderedInventory.ProductCode}: PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
					AssertEquals($"Pick 1 - {orderedInventory.ProductCode}: Pick should be waiting for replenishment", true, pick1.WP_IsAwaitingReplenishment);
				}

				var orderedInventories2 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>();
				foreach (var orderedInventory in orderedInventories2)
				{
					AssertEquals($"Pick 2 - {orderedInventory.ProductCode}: QuantityShort", 5m, orderedInventory.QuantityShort);
					AssertEquals($"Pick 2 - {orderedInventory.ProductCode}: PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
					AssertEquals($"Pick 2 - {orderedInventory.ProductCode}: Pick should be waiting for replenishment", true, pick2.WP_IsAwaitingReplenishment);
				}
			});
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			CombineAssertions(() =>
			{
				var pickPKs = new[] { pick1.PK, pick2.PK };
				var transfers = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation));
				AssertEquals("Created Transfer count correct", 2, transfers.Length);

				var j = 0;
				var expectedLogs = new List<string>();
				foreach (var transfer in transfers.OrderBy(t => t.PickBeingReplenished.WP_PickNo))
				{
					AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pickPKs[j++], transfer.WD_WP_PickBeingReplenished);
					expectedLogs.Add($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.");
					for (int i = 0; i < numberOfProducts; i++)
					{
						var transferLine = transfer.Lines.Where(l => l.WE_OP == products[i].PK).Cast<WhsTransferLine>().Single();
						AssertEquals("Source Location is correct.", bulkLocation, transferLine.TransferFromLocation);
						AssertTransferLine(transfer, transferLine, dynamicLocation, data.Org1, data.Whs1, products[i], 5m, ZDate.Empty, ZDate.Empty,
							string.Empty, string.Empty, string.Empty, string.Empty);
						expectedLogs.Add($@"Information|Pick Face: A-5 is to be replenished with 5 Product: PRODUCT{i + 1} for Client: 111 from Warehouse: 1 Location: A-1.");
					}
				}
				var loggerStrings = Logger.ToString().Trim().Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
				AssertContainsExactElementsInAnyOrder("Transfer logs are correct.", new[] { loggerStrings[0], loggerStrings[4] }, new[] { expectedLogs[0], expectedLogs[4] });
				AssertContainsExactElementsInAnyOrder(expectedLogs, loggerStrings);
			});
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");

			var dynamicLocation = data.Whs1.FindLocation("A-4");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var product = data.Part1;
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation1, ZDate.Empty, ZDate.Empty, "Red", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation2, ZDate.Empty, ZDate.Empty, "Blue", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, bulkLocation3, ZDate.Empty, ZDate.Empty, "Green", "", "", "");
			receive.FinaliseDocket();

			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, product, 5m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "", "");

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Factory.Save();

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItems();
			Factory.Save();
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			// Should transfer correct attributes
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);
			AssertSingleTransferLine(transfer, bulkLocation2, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "Blue", "", "");
			AssertEquals($@"Information|Transfer {transfer.WD_DocketID}: was created successfully.
Information|Pick Face: A-4 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts_MultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client2 = Helper.CreateClient("312");
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part2, 10m, bulkLocation, "");
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);
			AssertIsFinalisedPrecondition(receive2);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part2, 7m);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Factory.Save();

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItems();
			Factory.Save();

			CombineAssertions("Preconditions: ", () =>
			{
				var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
				AssertEquals("OrderedInventories Count", 2, orderedInventories.Count());
				var orderedInventory1 = orderedInventories.First();
				var orderedInventory2 = orderedInventories.Last();

				AssertEquals("PickLineQuantity: orderedInventory1", 0m, orderedInventory1.PickLineQuantity);
				AssertEquals("QuantityShort: orderedInventory1", 5m, orderedInventory1.QuantityShort);

				AssertEquals("PickLineQuantity: orderedInventory2", 0m, orderedInventory2.PickLineQuantity);
				AssertEquals("QuantityShort: orderedInventory2", 7m, orderedInventory2.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer1 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer1 WD_WP_PickBeingReplenished correct", pick.PK, transfer1.WD_WP_PickBeingReplenished);
			AssertSingleTransferLine(transfer1, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);
			AssertContains($@"Information|Transfer {transfer1.WD_DocketID}: was created successfully.
Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			var transfer2 = Factory.Load<WhsTransfer>(GetTransferQuery(client2, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer2 WD_WP_PickBeingReplenished correct", pick.PK, transfer2.WD_WP_PickBeingReplenished);
			AssertSingleTransferLine(transfer2, bulkLocation, dynamicLocation, client2, data.Whs1, data.Part2, 7m, ZDate.Empty, ZDate.Empty);
			AssertContains($@"Information|Transfer {transfer2.WD_DocketID}: was created successfully.
Information|Pick Face: A-2 is to be replenished with 7 Product: P2 for Client: 312 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts_AreaAlreadyContainsSomeCorrectInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var testLocation = data.Whs1.FindLocation("A-4");

			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var extraDynamicLocation = data.Whs1.FindLocation("A-3");
			extraDynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			extraDynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Factory.Save();

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItems();
			Factory.Save();
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 10m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 3m, testLocation, "");
			AssertIsFinalisedPrecondition(extraReceive);
			Factory.Save();

			var setUpTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			setUpTransfer.WD_WP_PickBeingReplenished = pick.PK;
			Factory.Save();

			Helper.CreateWhsTransferLine(setUpTransfer, data.Part1, 3m, testLocation, extraDynamicLocation);
			setUpTransfer.FinaliseDocket();
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);

			// Existing inventory should be discounted
			CombineAssertions(() =>
			{
				AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty);
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-2 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
			});
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickFaceArea_NonDynamicProducts_ExistingDynamicTransferAreAccountedFor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");

			var dynamicLocation = data.Whs1.FindLocation("A-3");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation1, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Factory.Save();

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItems();
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 10m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 3m, bulkLocation2, "");
			AssertIsFinalisedPrecondition(extraReceive);
			Factory.Save();

			var extraTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 3m, bulkLocation2, dynamicLocation);
			extraTransfer.RunPreSaveValidation();
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single(t => t.PK != extraTransfer.PK);
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);

			// Existing in-transit inventory should be discounted
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertSingleTransferLine(transfer, bulkLocation1, dynamicLocation, data.Org1, data.Whs1, data.Part1, 7m, ZDate.Empty, ZDate.Empty);
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-3 is to be replenished with 7 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", logs);
			});
		}

		public void TestCreateTransfersForPickfaceReplenishment_DynamicPickfaceArea_NonDynamicProducts_AccountForExistingStockAndTransfers_WithConflictingAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation3 = data.Whs1.FindLocation("A-3");
			var bulkLocation4 = data.Whs1.FindLocation("A-4");

			var tempLocation = data.Whs1.FindLocation("A-5");

			var dynamicLocation = data.Whs1.FindLocation("A-6");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation1, "", ZDate.Empty, ZDate.Empty, "Blue", "-", "-", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation2, "", ZDate.Empty, ZDate.Empty, "-", "Large", "-", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation3, "", ZDate.Empty, ZDate.Empty, "Blue", "Large", "-", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation4, "", ZDate.Empty, ZDate.Empty, "Red", "-", "Old", "");

			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 25m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 25m, ZDate.Empty, ZDate.Empty, "", "Large", "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "Blue", "Large", "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "Red", "", "Old", "", "");

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Factory.Save();

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItems();
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(4, orderedInventories.Count());

				AssertEquals("PickLineQuantity", 0m, orderedInventories.Sum(i => i.PickLineQuantity));
				AssertEquals("QuantityShort", 90m, orderedInventories.Sum(i => i.QuantityShort));
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			var extraReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			extraReceive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Red", "Large", "Old", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Red", "Small", "Old", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Blue", "Large", "New", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Blue", "Giant", "Old", "");

			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Blue", "Small", "New", "");
			Helper.CreateWhsReceiveLine(extraReceive, data.Part1, 5m, tempLocation, ZDate.Empty, ZDate.Empty, "Red", "Large", "New", "");
			extraReceive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(extraReceive);

			var testTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TT");
			testTransfer.WD_WP_PickBeingReplenished = pick.PK;
			Helper.CreateWhsTransferLine(testTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Blue", "Small", "New");
			Helper.CreateWhsTransferLine(testTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Red", "Large", "New");
			testTransfer.RunPreSaveValidation();
			testTransfer.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(testTransfer);

			var extraTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			extraTransfer.WD_WP_PickBeingReplenished = pick.PK;
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Red", "Large", "Old");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Red", "Small", "Old");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Blue", "Large", "New");
			Helper.CreateWhsTransferLine(extraTransfer, data.Part1, 5m, tempLocation, "", dynamicLocation, ZDate.Empty, ZDate.Empty, "Blue", "Giant", "Old");
			extraTransfer.RunPreSaveValidation();
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single(t => t.PK != extraTransfer.PK);
			AssertEquals("Transfer WD_WP_PickBeingReplenished correct", pick.PK, transfer.WD_WP_PickBeingReplenished);
			AssertEquals(4, transfer.Lines.Count);
			// Existing inventory should be discounted
			var logs = Logger.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertContains($"Information|Transfer {transfer.WD_DocketID}: was created successfully.", logs);
				AssertContains("Information|Pick Face: A-6 is to be replenished with 15 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-6 is to be replenished with 20 Product: P1 for Client: 111 from Warehouse: 1 Location: A-2.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-6 is to be replenished with 15 Product: P1 for Client: 111 from Warehouse: 1 Location: A-3.", Logger.ToString().Trim());
				AssertContains("Information|Pick Face: A-6 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-4.", Logger.ToString().Trim());
			});
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_LogsErrorMessages

		public void TestCreateTransfersForPickfaceReplenishment_LogsErrorMessages()
		{
			// creating test data so 'CreateTransfersForPickfaceReplenishment' will run
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation, "", ZDate.Today.AddDays(2), ZDate.Today, "Z", "Z", "Z", "Z");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK).DropRelateObjects(TestConnection);
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, "WhsPickLine");
			TestConnection.ExecuteNonQuery("DROP TABLE WhsPickLine");

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Error|Invalid object name 'dbo.WhsCommittedStock'.\r\nInformation|Auto-creation of Replenishment Transfers failed.\r\n", Logger.ToString());
		}

		public void TestCreateTransfersForPickfaceReplenishment_ShortCircuitQuery_LogsErrorMessages()
		{
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.PK);
			TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.WhsDocket
DROP CONSTRAINT WhsDocket_WD_WP_FK2_WhsPick_RRR_120N
ALTER TABLE dbo.WhsDocket
DROP CONSTRAINT WhsDocket_WD_WP_ParentPickForTransfer_FK2_WhsPick_RRR_120N
ALTER TABLE dbo.WhsDocket
DROP CONSTRAINT WhsDocket_WD_WP_ParentPickForReceive_FK2_WhsPick_RRR_120N
ALTER TABLE dbo.WhsDocket
DROP CONSTRAINT WhsDocket_WD_WP_PickBeingReplenished_FK2_WhsPick_RRR_120N
DROP TABLE WhsPick");

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Error|Invalid object name 'dbo.WhsPick'.\r\nInformation|Auto-creation of Replenishment Transfers failed.\r\n", Logger.ToString());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_LogsWhenNoLocationsNeedReplenishingWereFound

		public void TestCreateTransfersForPickfaceReplenishment_LogsWhenNoLocationsNeedReplenishingWereFound()
		{
			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCreateTransfersForPickfaceReplenishment_LogsWhenCreatedTransferHasErrors

		public void TestCreateTransfersForPickfaceReplenishment_LogsWhenCreatedTransferHasErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceNeedReplenishIngWitExistingStock = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceNeedReplenishIngWitExistingStock, 5m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, bulkLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, pickfaceNeedReplenishIngWitExistingStock);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickFaceCreationForTest = new PickFaceCreateTransfersTesting();
			pickFaceCreationForTest.RunActionBeforeTransferValidation += (transfer) =>
			{
				transfer.AddRowError("Test Error1");
			};

			using (ObjectFactory.Substitute<IPickFaceCreateTransfers>(pickFaceCreationForTest))
			{
				var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager.CreateTransfersForPickfaceReplenishment();
			}

			AssertEquals("No Transfer is created.", 0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickfaceNeedReplenishIngWitExistingStock)).Length);
			AssertEquals(@"Error|Creating a transfer for Client: 111, Warehouse: 1, Product: P1, Pick-face location A-2 failed.
Validation errors:
Error - Warehouse Transfer: Test Error1", Logger.ToString().Trim());
		}

		#endregion

		#region TestGetPickFaceInfos

		public void TestGetPickFaceInfos()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var pickface1Location = data.Whs1.FindLocation("A-1");
			var pickface2Location = data.Whs1.FindLocation("A-2");
			var pickface3Location = data.Whs1.FindLocation("A-3");
			var product3 = Helper.CreateProduct(data.Org1, "Test");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickface1Location, 1m, 2m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickface2Location, 1m, 3m);
			Helper.CreateProductPickFace(product3, data.Org1, pickface3Location, 1m, 5m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, pickface1Location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, pickface2Location);
			Helper.CreateWhsReceiveInventoryLine(receive, product3, 5m, pickface3Location);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			var pickFaceInfos = processingManager.LoadPickFaceInfos().OrderBy(pickFace => pickFace.ReplenishQuantity).ToArray(); // calls LoadPickFaceInfos

			AssertEquals("There should be 2 PickFaceInfos needing replenishment returned.", 2, pickFaceInfos.Length);
			AssertPickFaceInfo(pickFaceInfos[0], data.Whs1.PK, ZGuid.Empty, data.Org1.PK, data.Part1.PK, pickface1Location.PK,
				expectedReplenishQuantity: 1m, expectedReplenishMultiple: 1m, expectedIsDeadlocked: false);
			AssertPickFaceInfo(pickFaceInfos[1], data.Whs1.PK, ZGuid.Empty, data.Org1.PK, data.Part2.PK, pickface2Location.PK,
				expectedReplenishQuantity: 2m, expectedReplenishMultiple: 1m, expectedIsDeadlocked: false);
		}

		public void TestGetPickFaceInfos_DeadlockedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			var replenishFromLocation = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 20m, 40m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, pickfaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, replenishFromLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 30m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: 25 units allocated from the pick face.", 25m, pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			Factory.Save();

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			var pickfaceInfos = processingManager.LoadPickFaceInfos().ToArray(); // calls LoadPickFaceInfos

			AssertEquals("There should be a PickFaceInfo returned.", true, pickfaceInfos.Length > 0);
			AssertPickFaceInfo(pickfaceInfos[0], data.Whs1.PK, pick.PK, data.Org1.PK, data.Part1.PK, pickfaceLocation.PK,
				expectedReplenishQuantity: 15m, expectedReplenishMultiple: 1m, expectedIsDeadlocked: true);
		}

		void AssertPickFaceInfo(PickFaceInfo pickFaceInfo, ZGuid expectedWarehousePK, ZGuid expectedpickToReplenishPK, ZGuid expectedClientPK, ZGuid expectedProductPK,
			ZGuid expectedTransferToLocationPK, ZDecimal expectedReplenishQuantity, ZDecimal expectedReplenishMultiple, ZBool expectedIsDeadlocked)
		{
			AssertEquals("Warehouse should be correct.", expectedWarehousePK, pickFaceInfo.WarehousePK);
			AssertEquals("Pick To Replenish should be correct.", expectedpickToReplenishPK, pickFaceInfo.PickToReplenishPK);
			AssertEquals("Client should be correct.", expectedClientPK, pickFaceInfo.ClientPK);
			AssertEquals("Product should be correct.", expectedProductPK, pickFaceInfo.ProductPK);
			AssertEquals("Location should be correct.", expectedTransferToLocationPK, pickFaceInfo.TransferToLocationPK);
			AssertEquals("Replenish quantity should be correct.", expectedReplenishQuantity, pickFaceInfo.ReplenishQuantity);
			AssertEquals("Replenish multiple be correct.", expectedReplenishMultiple, pickFaceInfo.ReplenishMultiple);
			AssertEquals("Is deadlocked should be correct.", expectedIsDeadlocked, pickFaceInfo.IsDeadLocked);
		}

		#endregion

		#region TestDetectsAndPreventsDeadlocks -- Early replenishment / overfill

		#region TestDetectsAndPreventsDeadlocks_PickOrderingMoreThanReplenishmentMax

		public void TestDetectsAndPreventsDeadlocks_PickOrderingMoreThanReplenishmentMax()
		{
			TestDetectsAndPreventsDeadlocks_PickOrderingMoreThanReplenishmentMax(testRoundingUpToTheNextMultiple: false);
		}

		public void TestDetectsAndPreventsDeadlocks_PickOrderingMoreThanReplenishmentMax_RoundsUpToMultiple()
		{
			TestDetectsAndPreventsDeadlocks_PickOrderingMoreThanReplenishmentMax(testRoundingUpToTheNextMultiple: true);
		}

		// T0D0 : Geoff did not review this correctly LEAVE THIS COMMENT HERE GE TO DELETE IT.
		void TestDetectsAndPreventsDeadlocks_PickOrderingMoreThanReplenishmentMax(bool testRoundingUpToTheNextMultiple)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFace = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFace, 0m, 50m, 10m);

			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 100m, bulkLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, testRoundingUpToTheNextMultiple ? 59m : 60m); // multiple is 10
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: Allocations.", 0m, pick.OrderedInventories[0].PickLineQuantity);

			// First replenishment should fill to max
			new PickFaceReplenishmentProcessingManager(Logger).CreateTransfersForPickfaceReplenishment();
			var transfer1 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFace)).Single();
			AssertEquals("Should have created 1 transfer line.", 1, transfer1.Lines.Count);
			AssertTransferLine(transfer1, transfer1.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation == bulkLocation), pickFace, data.Org1, data.Whs1, data.Part1, 50m, ZDate.Empty, ZDate.Empty);

			transfer1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer1);
			Factory.Save();

			// Run allocation service task
			AllocateAwaitingReplenishmentPicksForTest();
			AssertEquals("Pick should be waiting for replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			// Second replenishment should overfill
			new PickFaceReplenishmentProcessingManager(Logger).CreateTransfersForPickfaceReplenishment();
			var transfer2 = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFace)).Single();
			AssertEquals("Should have created 1 transfer line.", 1, transfer2.Lines.Count);
			AssertTransferLine(transfer2, transfer2.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation == bulkLocation), pickFace, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);

			// Finalise transfer
			transfer2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer2);
			Factory.Save();

			// Run replenishment task, should not overfill as there is available inventory
			// (This is the simplest way of filtering out "deadlocks" which are actually waiting for allocation to be run)
			new PickFaceReplenishmentProcessingManager(Logger).CreateTransfersForPickfaceReplenishment();
			AssertEquals("Should not have created another transfer.", 0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFace)).Length);

			// Run allocation service task
			AllocateAwaitingReplenishmentPicksForTest();
			AssertEquals("Pick should no longer be waiting replenishment.", PickStatus.Codes.Created, pick.WP_PickStatus);
			Factory.Save();

			// Run again, should not need to overfill
			new PickFaceReplenishmentProcessingManager(Logger).CreateTransfersForPickfaceReplenishment();
		}

		#endregion

		#region TestDetectsAndPreventsDeadlocks_RelievesDeadLockViaMultipleTransfersOfOneMultiple

		public void TestDetectsAndPreventsDeadlocks_RelievesDeadLockViaMultipleTransfersOfOneMultiple()
		{
			// This solution is not ideal, but trying to calculate the shortfall quantity is problematic as it may have to consider
			// multiple pickfaces (as combination of pick faces may fulfil a pick) AND be careful not to excessively overload a pickface.
			//
			// We may wish to revist this solution in the future, but this may require the service task to be aware of picks and ordered quantities.
			// This will likely have to be done for supporting Part Attributes.
			//
			// Note that this is only a problem when there are lower priority picks in shortfall OR picks ordering more than the pickface qty
			// AND the replenishment multiple is small. These should both be edge cases, so the main priority is just handling them.
			//
			// If they are not edge cases then it likely means the customer has setup their pickfaces inefficiently. we do not code for inefficient processes.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFace = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFace, 0m, 50m, 10m);

			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive1.WD_ArrivalDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, pickFace);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10000m, bulkLocation);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 100m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Committed all units in the pick face.", 50m, pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.", true, pick.WP_IsAwaitingReplenishment);

			// Should overfill in transfer increments of 10 (because 10 is the multiple)
			var currentExpectedPickLineQty = pick.OrderedInventories[0].PickLineQuantity;
			while (pick.OrderedInventories[0].QuantityShort > 0m)
			{
				// Run replenishment service task -- should overfill by one multiple
				new PickFaceReplenishmentProcessingManager(Logger).CreateTransfersForPickfaceReplenishment();

				// Finalise transfer
				var newTransfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFace)).Single();
				AssertEquals("Should have created 1 transfer line.", 1, newTransfer.Lines.Count);
				AssertTransferLine(newTransfer, newTransfer.Lines.Cast<WhsTransferLine>().Single(t => t.TransferFromLocation == bulkLocation), pickFace, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);

				newTransfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(newTransfer);
				Factory.Save();

				// Run allocation service task
				currentExpectedPickLineQty += 10m;
				AllocateAwaitingReplenishmentPicksForTest();
				if (currentExpectedPickLineQty != pick.OrderedInventories[0].PickLineQuantity)
				{
					AssertEquals("Pick2 should have been allocated to.", currentExpectedPickLineQty, pick.OrderedInventories[0].PickLineQuantity);
				}

				AssertEquals("Pick2 should have been allocated to.", currentExpectedPickLineQty, pick.OrderedInventories[0].PickLineQuantity);
				Factory.Save();
			}

			AssertEquals("Pick should no longer be waiting for for replenishment.", PickStatus.Codes.Created, pick.WP_PickStatus);

			// Run replenishment service task, should not overfill as there is no longer a deadlock
			new PickFaceReplenishmentProcessingManager(Logger).CreateTransfersForPickfaceReplenishment();
			AssertEquals("Should not have created a transfer because an existing pick can be picked.", 0, Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFace)).Length);
		}

		#endregion

		#endregion

		#region TestLastReplenishmentRun

		public void TestLastReplenishmentRun_RegistryUpdated()
		{
			AssertEquals("Precondition: last replenish run is default value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			receive.TransportCoPK = data.Org1.PK;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a value after a replenishment run.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_NewStockForProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(2));
			Assert("Finalised datetime is earlier than the initial last run datetime.", receive.WD_FinalisedDate < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-2));
			Assert("Finalised datetime is later than the initial last run datetime.", receive.WD_FinalisedDate > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			Logger.ClearLog();
			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			var processingManager2 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager2.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_NewStockForProductPicked()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var bulkLocation = data.Whs1.FindLocation("A-1");
				var pickFaceLocation = data.Whs1.FindLocation("A-2");
				Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);

				var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
				AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
				AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, bulkLocation);
				receive.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager1.CreateTransfersForPickfaceReplenishment();
				var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation)).Single();
				AssertSingleTransferLine(transfer, bulkLocation, pickFaceLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty);
				AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

				transfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(transfer);
				Factory.Save();

				Logger.ClearLog();
				var processingManager2 = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager2.CreateTransfersForPickfaceReplenishment();
				AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 9m);
				var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
				pick.FinaliseOrder(order);
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(order);
				AssertIsFinalisedPrecondition(pick);
				Factory.Save();

				Logger.ClearLog();
				var processingManager3 = new PickFaceReplenishmentProcessingManager(Logger);
				processingManager3.CreateTransfersForPickfaceReplenishment();

				AssertEquals(@"Information|Transfer W00000004: was created successfully.
Information|Pick Face: A-2 is to be replenished with 9 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());
			}
		}

		public void TestLastReplenishmentRun_NewStockForProduct_FinalisedDateBeforeLastRun()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
			Assert("Finalised datetime is earlier than the initial last run datetime.", receive.WD_FinalisedDate < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestLastReplenishmentRun_NewStockForProduct_FinalisedDateAfterLastRun()
		{
			TestDateAttribute.UseUNLOCO = true;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
			Assert("Finalised datetime is after the initial last run datetime.", receive.WD_FinalisedDate > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_ProductUpdatedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
			Assert("OP_SystemLastEditTimeUtc of product is not later than the initial last run datetime.", data.Part1.OP_SystemLastEditTimeUtc < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5));
			Assert("OP_SystemLastEditTimeUtc of product is later than the initial last run datetime.", data.Part1.OP_SystemLastEditTimeUtc > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			Logger.ClearLog();
			var processingManager2 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager2.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_LocationUpdatedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation);
			Factory.Save();

			Assert("WLV_LastAllocatedOrChangedDateUtc of location has value.", !bulkLocation.WLV_LastAllocatedOrChangedDateUtc.IsEmpty);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
			Assert("WLV_LastAllocatedOrChangedDateUtc of location is not later than the initial last run datetime.", bulkLocation.WLV_LastAllocatedOrChangedDateUtc < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5));
			Assert("WLV_LastAllocatedOrChangedDateUtc of location is later than the initial last run datetime.", bulkLocation.WLV_LastAllocatedOrChangedDateUtc > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			Logger.ClearLog();
			var processingManager2 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager2.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_PickFaceUpdatedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, bulkLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
			Assert("WF_SystemLastEditTimeUtc of pickface is not later than the initial last run datetime.", pickFace.WF_SystemLastEditTimeUtc < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5));
			Assert("WF_SystemLastEditTimeUtc of pickface is later than the initial last run datetime.", pickFace.WF_SystemLastEditTimeUtc > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			Logger.ClearLog();
			var processingManager2 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager2.CreateTransfersForPickfaceReplenishment();
			AssertTransfer(pickFaceLocation, bulkLocation, data.Org1, data.Whs1, data.Part1, 10m, ZDate.Empty, ZDate.Empty, data.Whs1.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today), "", "", "", "", "", expectedLineCount: 1);
			AssertEquals(@"Information|Transfer W00000002: was created successfully.
Information|Pick Face: A-2 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_DynamicPickFace_NewStockForProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();
			Factory.Save();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			AssertIsFinalisedPrecondition(receive);
			Assert("Finalised datetime is not later than the initial last run datetime.", receive.WD_FinalisedDate < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5));
			Assert("Finalised datetime is later than the initial last run datetime.", receive.WD_FinalisedDate > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			Logger.ClearLog();
			var processingManager2 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager2.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertSingleTransferLine(transfer, bulkLocation, dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);
			AssertEquals($@"Information|Transfer W00000003: was created successfully.
Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_DynamicPickFace_PickUpdatedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();
			Factory.Save();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
			Assert("WP_SystemLastEditTimeUtc of pick is not later than the initial last run datetime.", pick.WP_SystemLastEditTimeUtc < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5));
			Assert("WP_SystemLastEditTimeUtc of pick is later than the initial last run datetime.", pick.WP_SystemLastEditTimeUtc > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			Logger.ClearLog();
			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertTransferLine(transfer, (WhsTransferLine)transfer.Lines.Single(l => l.SupplierPart == data.Part1), dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);
			AssertEquals(@"Information|Transfer W00000003: was created successfully.
Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		public void TestLastReplenishmentRun_DynamicPickFace_OrderUpdatedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory = orderedInventories.Single();
			Factory.Save();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("PickLineQuantity", 0m, orderedInventory.PickLineQuantity);
				AssertEquals("QuantityShort", 5m, orderedInventory.QuantityShort);
				AssertEquals("Pick should be waiting for replenishment", true, pick.WP_IsAwaitingReplenishment);
			});

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(5));
			AssertNotEquals("Precondition: last replenish run has value.", WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.DefaultValue, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
			Assert("WD_SystemLastEditTimeUtc of order is not later than the initial last run datetime.", order.WD_SystemLastEditTimeUtc < WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var processingManager1 = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager1.CreateTransfersForPickfaceReplenishment();
			AssertEquals("Information|Did not find any Locations that needed replenishing.", Logger.ToString().Trim());

			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5));
			Assert("WD_SystemLastEditTimeUtc of order is later than the initial last run datetime.", order.WD_SystemLastEditTimeUtc > WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);

			var initialLastRunDateTime = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			Logger.ClearLog();
			var processingManager = new PickFaceReplenishmentProcessingManager(Logger);
			processingManager.CreateTransfersForPickfaceReplenishment();

			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, dynamicLocation)).Single();
			AssertTransferLine(transfer, (WhsTransferLine)transfer.Lines.Single(l => l.SupplierPart == data.Part1), dynamicLocation, data.Org1, data.Whs1, data.Part1, 5m, ZDate.Empty, ZDate.Empty);
			AssertEquals(@"Information|Transfer W00000003: was created successfully.
Information|Pick Face: A-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1.", Logger.ToString().Trim());

			AssertNotEquals("PickFaceReplenishmentLastRunUTC is assigned a new value after a replenishment run.", initialLastRunDateTime, WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value);
		}

		#endregion

		#region Implementation

		#region AllocateAwaitingReplenishmentPicksForTest

		void AllocateAwaitingReplenishmentPicksForTest()
			=> ObjectFactory
				.Get<IAllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager>()
				.AllocateAwaitingReplenishmentPicks(Logger, new CancellationToken());

		#endregion

		void AssertTransfer(WhsLocation pickFaceLocation, WhsLocation bulkLocation,
			OrgHeader org, WhsWarehouse warehouse, OrgSupplierPart part, ZDecimal expectedTransferQty,
			ZDate packingDate, ZDate expiryDate, ZDateTimeOffset arrivalDate, ZString attribute1, ZString attribute2, ZString attribute3, ZString serialNumber, ZString bondedEntryKey, int expectedLineCount)
		{
			var transfer = Factory.Load<WhsTransfer>(GetTransferQuery(org, warehouse, pickFaceLocation)).Single();
			AssertEquals(expectedLineCount, transfer.Lines.Count);

			var line = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.TransferFromLocation == bulkLocation);
			AssertTransferLine(transfer, line, pickFaceLocation, org, warehouse, part, expectedTransferQty,
				packingDate, expiryDate, attribute1, attribute2, attribute3, serialNumber, bondedEntryKey, arrivalDate);
		}

		static void AssertTransferLine(WhsTransfer transfer, WhsTransferLine line, WhsLocation expectedToLocation,
			OrgHeader expectedClient, WhsWarehouse expectedWarehouse, OrgSupplierPart expectedPart, ZDecimal expectedPackQty,
			ZDateTime expectedPackingDate, ZDateTime expectedExpiryDate, string expectedPartAttribute1 = "", string expectedPartAttribute2 = "", string expectedPartAttribute3 = "", string expectedSerialNumber = "",
			string expectedBondedEntryKey = "", ZDateTimeOffset? expectedArrivalDate = null)
		{
			AssertEquals(expectedClient, transfer.Client);
			AssertEquals(expectedWarehouse, transfer.Warehouse);
			AssertEquals(expectedToLocation, line.Location);
			AssertEquals(expectedPart, line.SupplierPart);
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

		static void AssertSingleTransferLine(WhsTransfer transfer, WhsLocation expectedSourceLocation, WhsLocation expectedTransferToLocation,
			OrgHeader expectedClient, WhsWarehouse expectedWarehouse, OrgSupplierPart expectedPart, ZDecimal expectedPackQty,
			ZDateTime expectedPackingDate, ZDateTime expectedExpiryDate, string expectedPartAttribute1 = "", string expectedPartAttribute2 = "", string expectedPartAttribute3 = "",
			string expectedBondedEntryKey = "")
		{
			var line = transfer.Lines.Cast<WhsTransferLine>().Single();
			AssertEquals(expectedSourceLocation, line.TransferFromLocation);
			AssertTransferLine(transfer, line, expectedTransferToLocation, expectedClient, expectedWarehouse, expectedPart, expectedPackQty, expectedPackingDate, expectedExpiryDate,
				expectedPartAttribute1, expectedPartAttribute2, expectedPartAttribute3, expectedBondedEntryKey);
		}

		ZQuery GetTransferQuery(OrgHeader org, WhsWarehouse warehouse, WhsLocation pickfaceLocation, OrgSupplierPart product = null)
		{
			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, DocketLineStatus.Codes.Entered);
			transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WL, pickfaceLocation.PK);

			if (product != null)
			{
				transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_OP, product.PK);
			}

			var transferQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, org.PK);
			transferQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehouse.PK);
			transferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);

			return transferQuery;
		}

		void SetAvailableInventory(WhsPickAvailableInventory availableInventory, bool allocateInventory, GlbStaff picker, ZDateTimeOffset pickedDate)
		{
			availableInventory.Allocate = allocateInventory;
			Helper.SetAssignToPk(availableInventory, picker?.PK ?? ZGuid.Empty);
			Helper.SetPickedDate(availableInventory, pickedDate);
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

		TestServiceLogger Logger => logger ?? (logger = new TestServiceLogger());
		TestServiceLogger logger;

		#endregion
	}

	#region PickFaceReplenishmentProcessingManager_TriggersTest class

	public class PickFaceReplenishmentProcessingManager_TriggersTest : TestCase
	{
		#region TestCreateTransfersForPickfaceReplenishment_OverPickingTrigger

		[UseSnapshotProtection]
		public void TestCreateTransfersForPickfaceReplenishment_OverPickingTrigger()
		{
			// temporary we have 2 imbalance check triggers, so remove one at a time to test the other one is handled.
			Action removeSecondTrigger = () =>
			{
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsTestHelperFunctions.WhsCheckTransactionAndPickQtyIsCorrect} (@TransactionLinePKs dbo.TVP_uniqueidentifier READONLY)", "0");
				WhsTestHelperFunctions.SuspendDatabaseProcForTest("WhsCheckTransactionAndPickQtyIsCorrect_ForInsert (@TransactionLinePKs dbo.TVP_uniqueidentifier READONLY)", "0");
			};

			var expectedErrorLog = new List<ZString>
			{
				"Error|Creating a transfer for Client: 111, Warehouse: 1, Product: P1, Pick-face location A-2 failed.",
				"Save errors: Another job has taken some of the stock that this service task tried to allocate.",
				"Information|Transfer W00000003: was created successfully.",
				"Information|Pick Face: A-3 is to be replenished with 10 Product: P2 for Client: 111 from Warehouse: 1 Location: A-1."
			};

			TestCreateTransfersForPickfaceReplenishment_OverPickingTriggerCore(removeSecondTrigger, expectedErrorLog);
		}

		[UseSnapshotProtection]
		public void TestTG_WhsDocketAndPickLine_TransactionAndPickedQtyAreCorrect()
		{
			// temporary we have 2 imbalance check triggers, so remove one at a time to test the other one is handled.
			Action removeSecondTrigger = () =>
			{
				WhsTestHelperFunctions.SuspendTrigger("TG_PreventOverCommitOfStockViaPickLine", WhsPickLineSchema.Constants.TableName);
			};

			var expectedErrorLog = new List<ZString>
			{
				"Information|Transfer W00000003: was created successfully.",
				"Information|Pick Face: A-3 is to be replenished with 10 Product: P2 for Client: 111 from Warehouse: 1 Location: A-1.",
				"Error|Creating a transfer for Client: 111, Warehouse: 1, Product: P1, Pick-face location A-2 failed.",
				"Save errors: Attempt to set Transaction and Pick qty out of sync by the current process."
			};

			TestCreateTransfersForPickfaceReplenishment_OverPickingTriggerCore(removeSecondTrigger, expectedErrorLog);
		}

		void TestCreateTransfersForPickfaceReplenishment_OverPickingTriggerCore(Action removeSecondTrigger, List<ZString> expectedErrorLog)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation1 = data.Whs1.FindLocation("A-2");
			var pickFaceLocation2 = data.Whs1.FindLocation("A-3");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1, 2m, 10m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation2, 2m, 10m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var pickFaceCreationForTest = new PickFaceCreateTransfersTesting();
			pickFaceCreationForTest.RunActionBeforeTransferValidation += (transfer) =>
			{
				var query = new ZQuery();
				query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);

				if (transfer.Lines.Any(l => l.WE_OP == data.Part1.PK))
				{
					transfer.Lines[0].PickLines[0].WZ_Units = 15m; // over-pick of inventory by the transfer
				}
			};

			using (ObjectFactory.Substitute<IPickFaceCreateTransfers>(pickFaceCreationForTest))
			{
				var processingManager = new PickFaceReplenishmentProcessingManager(Logger);

				// temporary we have 2 imbalance check triggers, so remove one at a time to test the other one is handled.
				removeSecondTrigger();

				processingManager.CreateTransfersForPickfaceReplenishment();
			}

			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				var factoryWithSecondConnection = new BusinessObjectFactory(secondConnection);
				AssertEquals("No Transfer is created.", 0,
					factoryWithSecondConnection.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation1)).Length);
				AssertEquals("Should have created Transfer for Pick Face 2.", 1,
					factoryWithSecondConnection.Load<WhsTransfer>(GetTransferQuery(data.Org1, data.Whs1, pickFaceLocation2)).Length);

				var producedLog = Logger.ToString().Trim();
				foreach (var log in expectedErrorLog)
				{
					Assert($"Log should contain {log}", producedLog.Contains(log));
				}
			}
		}

		ZQuery GetTransferQuery(OrgHeader org, WhsWarehouse warehouse, WhsLocation pickfaceLocation)
		{
			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, DocketLineStatus.Codes.Entered);
			transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WL, pickfaceLocation.PK);

			var transferQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, org.PK);
			transferQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehouse.PK);
			transferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);

			return transferQuery;
		}

		#endregion

		#region Implementation

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#region Logger

		TestServiceLogger Logger
		{
			get { return logger ?? (logger = new TestServiceLogger()); }
		}

		TestServiceLogger logger;

		#endregion

		#endregion
	}

	#endregion
}
