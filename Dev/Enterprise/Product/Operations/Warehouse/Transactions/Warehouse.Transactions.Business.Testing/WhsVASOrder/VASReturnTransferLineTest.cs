using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class VASReturnTransferLineTest : WhsTestCaseWithFactory
	{
		#region TestGetLinesToPutawayFromTransfer

		public void TestGetLinesToPutawayFromTransfer_Null()
		{
			AssertEquals(0, VASReturnTransferLine.GetLinesToPutawayFromTransfer(null).Count());
		}

		public void TestGetLinesToPutawayFromTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m,
				data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m,
				data.Whs1.FindLocation("A-1"), "PLT-1");

			// make both locations non-empty to make sure service area is excluded in putaway algorithm.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 5m,
				data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 5m,
				data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			var putawayEngineMock = new Mock<IPutawayEngineManagerForVASTransferLine>(MockBehavior.Strict);
			putawayEngineMock.Setup(putawayEngine => putawayEngine.Putaway(It.IsAny<IEnumerable<WhsVASOrder>>(),
					It.IsAny<IEnumerable<VASReturnTransferLine>>(), It.IsNotNull<INotifications>(), null))
				.Verifiable();

			WhsTransfer returnTransfer;
			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			AssertExceptionThrown<InvalidOperationException>(
				"Should not do Putaway for the Into Service Area VAS Order Transfer.",
				() => VASReturnTransferLine.GetLinesToPutawayFromTransfer(initialTransfer));

			returnTransfer.Lines[0].WE_WL = ZGuid.Empty;
			returnTransfer.Lines[0].PickedTime =
				ZDateTimeOffset.Now; // hack to make it look like service area location is empty to test it won't putaway to the service area.
			var linesToPutaway = VASReturnTransferLine.GetLinesToPutawayFromTransfer(returnTransfer)
				.Cast<ILineToPutaway>();
			var putawayLine = linesToPutaway.Single();
			AssertEquals("putawayLine.TransferLine", returnTransfer.Lines[0],
				((VASReturnTransferLine)putawayLine).TransferLine);
			AssertEquals("putawayLine.CheckPalletIDExists", false, putawayLine.CheckPalletIDExists);
			AssertEquals("putawayLine.DocketPK", returnTransfer.PK, putawayLine.DocketPK);
			AssertEquals("putawayLine.Factory", Factory, putawayLine.Factory);
			AssertEquals("putawayLine.InventoryStatus", InventoryStatus.Codes.Available, putawayLine.InventoryStatus);
			AssertEquals("putawayLine.InventoryHeldCode", "", putawayLine.InventoryHeldCode);
			AssertEquals("putawayLine.IsValidToPutaway", true, putawayLine.IsValidToPutaway);
			AssertEquals("putawayLine.Location", null, putawayLine.Location);
			AssertEquals("putawayLine.LocationPK", ZGuid.Empty, putawayLine.LocationPK);
			AssertEquals("putawayLine.PackType", "UNT", putawayLine.PackType);
			AssertEquals("putawayLine.PalletID", "PLT-1", putawayLine.PalletID);
			AssertEquals("putawayLine.Product", data.Part1, putawayLine.Product);
			AssertEquals("putawayLine.ProductPK", data.Part1.PK, putawayLine.ProductPK);
			AssertEquals("putawayLine.QuantityToPutaway", 12m, putawayLine.QuantityToPutaway);
			AssertEquals("putawayLine.PackQuantity", 12m, putawayLine.PackQuantity);
			AssertEquals("putawayLine.WarehousePK", data.Whs1.PK, putawayLine.WarehousePK);

			putawayLine.PackType = "CTN";
			putawayLine.LocationPK = data.Whs1.FindLocation("A-2").PK;
			AssertEquals("CTN", putawayLine.PackType);
			AssertEquals("putawayLine.QuantityToPutaway", 144m, putawayLine.QuantityToPutaway);
			AssertEquals("putawayLine.PackQuantity", 12m, putawayLine.PackQuantity);
			AssertEquals(data.Whs1.FindLocation("A-2").PK, putawayLine.LocationPK);

			returnTransfer.Lines[0].PickLines.DeleteAll();
			AssertEquals("Should not be valid to putaway if line is not fully committed.", false,
				putawayLine.IsValidToPutaway);
		}

		#endregion

		#region TestSplit

		public void TestSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var part5 = Helper.CreateProduct(data.Org1, "P5");
			var part6 = Helper.CreateProduct(data.Org1, "P6");
			Factory.Save();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.DefaultLocation,
				"PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.DefaultLocation,
				"PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", part3, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", part4, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", part4, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", part5, 3m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R9", part5, 3m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R10", part5, 3m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R11", part6, 3m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R12", part6, 3m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R13", part6, 3m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 5m);
			Helper.CreateWhsVASOrderLine(vasOrder, part3, 8m);
			Helper.CreateWhsVASOrderLine(vasOrder, part4, 8m);
			Helper.CreateWhsVASOrderLine(vasOrder, part5, 9m);
			Helper.CreateWhsVASOrderLine(vasOrder, part6, 9m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			var returnTransferLine1 = (WhsTransferLine)returnTransfer.Lines.Single(l => l.SupplierPart == data.Part1);
			var returnTransferLine2 = (WhsTransferLine)returnTransfer.Lines.Single(l => l.SupplierPart == data.Part2);
			var returnTransferLine3 = (WhsTransferLine)returnTransfer.Lines.Single(l => l.SupplierPart == part3);
			var returnTransferLine4 = (WhsTransferLine)returnTransfer.Lines.Single(l => l.SupplierPart == part4);
			var returnTransferLine5 = (WhsTransferLine)returnTransfer.Lines.Single(l => l.SupplierPart == part5);
			var returnTransferLine6 = (WhsTransferLine)returnTransfer.Lines.Single(l => l.SupplierPart == part6);

			var putawayLines = VASReturnTransferLine.GetLinesToPutawayFromTransfer(returnTransfer)
				.Cast<ILineToPutaway>();
			var putawayLine1 = putawayLines.Single(l => l.Product == data.Part1);
			var putawayLine2 = putawayLines.Single(l => l.Product == data.Part2);
			var putawayLine3 = putawayLines.Single(l => l.Product == part3);
			var putawayLine4 = putawayLines.Single(l => l.Product == part4);
			var putawayLine5 = putawayLines.Single(l => l.Product == part5);
			var putawayLine6 = putawayLines.Single(l => l.Product == part6);

			// Putaway Line 1 has a total quantity of 10.
			AssertExceptionThrown<ArgumentException>(
				"Quantity for splitting Transfer line should be greater than Zero and less than the Total Units.",
				() => putawayLine1.Split(10m));
			AssertExceptionThrown<ArgumentException>(
				"Quantity for splitting Transfer line should be greater than Zero and less than the Total Units.",
				() => putawayLine1.Split(11m));
			AssertExceptionThrown<ArgumentException>(
				"Quantity for splitting Transfer line should be greater than Zero and less than the Total Units.",
				() => putawayLine1.Split(0m));
			AssertExceptionThrown<ArgumentException>(
				"Quantity for splitting Transfer line should be greater than Zero and less than the Total Units.",
				() => putawayLine1.Split(-1m));

			// test splitting a transfer line with one matching line matching split quantity
			var newPutawayLine1 = putawayLine1.Split(5m);
			AssertEquals("newPutawayLine1.DocketPK", returnTransfer.PK, newPutawayLine1.DocketPK);
			AssertEquals("newPutawayLine1.IsValidToPutaway", true, newPutawayLine1.IsValidToPutaway);
			AssertEquals("newPutawayLine1.PackType", "UNT", newPutawayLine1.PackType);
			AssertEquals("newPutawayLine1.PalletID", "PLT-1", newPutawayLine1.PalletID);
			AssertEquals("newPutawayLine1.Product", data.Part1, newPutawayLine1.Product);
			AssertEquals("newPutawayLine1.QuantityToPutaway", 5m, newPutawayLine1.QuantityToPutaway);
			AssertEquals("Matching should have been Line moved out.", 0, returnTransferLine1.MatchingLines.Count);

			var newTransferLine1 =
				returnTransfer.Lines.Single(l => l.SupplierPart == data.Part1 && l != returnTransferLine1);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine1.WE_OriginalInventoryStatus);
			AssertEquals("Should have copied Transfer from Pallet ID.", "PLT-1",
				newTransferLine1.WE_TransferFromPalletId);
			AssertEquals("Should have copied Transfer from Location.", serviceArea.PutawayLocations[0].PK,
				newTransferLine1.WE_WL_TransferFrom);
			AssertEquals("Original Transfer Line should have correct quantity.", 5m,
				returnTransferLine1.QtyToMoveIncludingMatchingLines);

			// test splitting a single top level transfer line
			var newPutawayLine2 = putawayLine2.Split(3m);
			AssertEquals("newPutawayLine2.DocketPK", returnTransfer.PK, newPutawayLine2.DocketPK);
			AssertEquals("newPutawayLine2.IsValidToPutaway", true, newPutawayLine2.IsValidToPutaway);
			AssertEquals("newPutawayLine2.PackType", "UNT", newPutawayLine2.PackType);
			AssertEquals("newPutawayLine2.PalletID", "", newPutawayLine2.PalletID);
			AssertEquals("newPutawayLine2.Product", data.Part2, newPutawayLine2.Product);
			AssertEquals("newPutawayLine2.QuantityToPutaway", 3m, newPutawayLine2.QuantityToPutaway);

			var newTransferLine2 =
				returnTransfer.Lines.Single(l => l.SupplierPart == data.Part2 && l != returnTransferLine2);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine2.WE_OriginalInventoryStatus);
			AssertEquals("Should have copied Transfer from Pallet ID.", "", newTransferLine2.WE_TransferFromPalletId);
			AssertEquals("Should have copied Transfer from Location.", serviceArea.PutawayLocations[0].PK,
				newTransferLine2.WE_WL_TransferFrom);
			AssertEquals("Original Transfer Line should have correct quantity.", 2m,
				returnTransferLine2.QtyToMoveIncludingMatchingLines);

			// test splitting a transfer line with one matching line with quantity less than matching line quantity
			var newPutawayLine3 = putawayLine3.Split(2m);
			AssertEquals("newPutawayLine3.DocketPK", returnTransfer.PK, newPutawayLine3.DocketPK);
			AssertEquals("newPutawayLine3.IsValidToPutaway", true, newPutawayLine3.IsValidToPutaway);
			AssertEquals("newPutawayLine3.PackType", "UNT", newPutawayLine3.PackType);
			AssertEquals("newPutawayLine3.PalletID", "", newPutawayLine3.PalletID);
			AssertEquals("newPutawayLine3.Product", part3, newPutawayLine3.Product);
			AssertEquals("newPutawayLine3.QuantityToPutaway", 2m, newPutawayLine3.QuantityToPutaway);

			var newTransferLine3 =
				returnTransfer.Lines.Single(l => l.SupplierPart == part3 && l != returnTransferLine3);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine3.WE_CurrentInventoryStatus);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine3.WE_OriginalInventoryStatus);
			AssertEquals("Should have copied Transfer from Pallet ID.", "", newTransferLine3.WE_TransferFromPalletId);
			AssertEquals("Should have copied Transfer from Location.", serviceArea.PutawayLocations[0].PK,
				newTransferLine3.WE_WL_TransferFrom);
			AssertEquals("Original Transfer Line should have correct quantity.", 6m,
				returnTransferLine3.QtyToMoveIncludingMatchingLines);

			// test splitting a transfer line with one matching line with quantity greater than matching line quantity
			var newPutawayLine4 = putawayLine4.Split(5m);
			AssertEquals("newPutawayLine4.DocketPK", returnTransfer.PK, newPutawayLine4.DocketPK);
			AssertEquals("newPutawayLine4.IsValidToPutaway", true, newPutawayLine4.IsValidToPutaway);
			AssertEquals("newPutawayLine4.PackType", "UNT", newPutawayLine4.PackType);
			AssertEquals("newPutawayLine4.PalletID", "", newPutawayLine4.PalletID);
			AssertEquals("newPutawayLine4.Product", part4, newPutawayLine4.Product);
			AssertEquals("newPutawayLine4.QuantityToPutaway", 5m, newPutawayLine4.QuantityToPutaway);

			var newTransferLine4 =
				returnTransfer.Lines.Single(l => l.SupplierPart == part4 && l != returnTransferLine4);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine4.WE_CurrentInventoryStatus);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine4.WE_OriginalInventoryStatus);
			AssertEquals("Should have copied Transfer from Pallet ID.", "", newTransferLine4.WE_TransferFromPalletId);
			AssertEquals("Should have copied Transfer from Location.", serviceArea.PutawayLocations[0].PK,
				newTransferLine4.WE_WL_TransferFrom);
			AssertEquals("Original Transfer Line should have correct quantity.", 3m,
				returnTransferLine4.QtyToMoveIncludingMatchingLines);

			// test splitting a transfer line with multiple matching lines with quantity greater than matching lines quantity
			var newPutawayLine5 = putawayLine5.Split(8m);
			AssertEquals("newPutawayLine5.DocketPK", returnTransfer.PK, newPutawayLine5.DocketPK);
			AssertEquals("newPutawayLine5.IsValidToPutaway", true, newPutawayLine5.IsValidToPutaway);
			AssertEquals("newPutawayLine5.PackType", "UNT", newPutawayLine5.PackType);
			AssertEquals("newPutawayLine5.PalletID", "", newPutawayLine5.PalletID);
			AssertEquals("newPutawayLine5.Product", part5, newPutawayLine5.Product);
			AssertEquals("newPutawayLine5.QuantityToPutaway", 8m, newPutawayLine5.QuantityToPutaway);

			var newTransferLine5 =
				returnTransfer.Lines.Single(l => l.SupplierPart == part5 && l != returnTransferLine5);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine5.WE_CurrentInventoryStatus);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine5.WE_OriginalInventoryStatus);
			AssertEquals("Should have copied Transfer from Pallet ID.", "", newTransferLine5.WE_TransferFromPalletId);
			AssertEquals("Should have copied Transfer from Location.", serviceArea.PutawayLocations[0].PK,
				newTransferLine5.WE_WL_TransferFrom);
			AssertEquals("Original Transfer Line should have correct quantity.", 1m,
				returnTransferLine5.QtyToMoveIncludingMatchingLines);

			// test splitting a transfer line with multiple matching lines with quantity less than matching lines quantity
			var newPutawayLine6 = putawayLine6.Split(4m);
			AssertEquals("newPutawayLine6.DocketPK", returnTransfer.PK, newPutawayLine6.DocketPK);
			AssertEquals("newPutawayLine6.IsValidToPutaway", true, newPutawayLine6.IsValidToPutaway);
			AssertEquals("newPutawayLine6.PackType", "UNT", newPutawayLine6.PackType);
			AssertEquals("newPutawayLine6.PalletID", "", newPutawayLine6.PalletID);
			AssertEquals("newPutawayLine6.Product", part6, newPutawayLine6.Product);
			AssertEquals("newPutawayLine6.QuantityToPutaway", 4m, newPutawayLine6.QuantityToPutaway);

			var newTransferLine6 =
				returnTransfer.Lines.Single(l => l.SupplierPart == part6 && l != returnTransferLine6);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine6.WE_CurrentInventoryStatus);
			AssertEquals("Should have copied Inventory Status.", InventoryStatus.Codes.Available,
				newTransferLine6.WE_OriginalInventoryStatus);
			AssertEquals("Should have copied Transfer from Pallet ID.", "", newTransferLine6.WE_TransferFromPalletId);
			AssertEquals("Should have copied Transfer from Location.", serviceArea.PutawayLocations[0].PK,
				newTransferLine6.WE_WL_TransferFrom);
			AssertEquals("Original Transfer Line should have correct quantity.", 5m,
				returnTransferLine6.QtyToMoveIncludingMatchingLines);
			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct.

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Transfer should be created in a finalisable state.", true, returnTransfer.IsFinalised);
		}

		#endregion
	}
}
