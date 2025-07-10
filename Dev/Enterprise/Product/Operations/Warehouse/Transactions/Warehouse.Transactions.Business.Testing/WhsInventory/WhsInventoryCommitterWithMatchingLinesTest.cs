using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using WhsInventoryCommitter = Enterprise.Warehouse.Transactions.Business.WhsInventoryCommitterWithMatchingLines<Enterprise.Warehouse.Transactions.Business.WhsTransferLine>;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryCommitterWithMatchingLinesTest : WhsInventoryCommitterTest<WhsTransferLine, WhsTransfer>
	{
		#region TestConstructor_MainLineOnly

		public void TestConstructor_MainLineOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "");
			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			AssertNoExceptionThrown("Main transfer line should be passed into committer.", () => new WhsInventoryCommitter(transferLine));
			AssertExceptionThrown("Matching transfer line should NOT be passed into committer.", typeof(ArgumentException), () => new WhsInventoryCommitter(matchingLine));
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory_ReassignQtyAndRemovesMatchingLines

		public void TestUncommitOverPickedOrNotMatchingInventory_ReassignQtyAndRemovesMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);

			// transfer line partially committed + 1 pick line not matching (no matching lines)
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			var pickLine1_1 = Helper.CreateWhsPickLine(transferLine1, inventory1, 2m);
			var pickLine1_2 = Helper.CreateWhsPickLine(transferLine1, inventory2, 1m);
			var pickLine1_3 = Helper.CreateWhsPickLine(transferLine1, inventory3, 1m);
			var committer1 = new WhsInventoryCommitter(transferLine1);
			committer1.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommitting qty on main transfer line should not modify qty.", 5m, transferLine1.WE_TransactionQuantity);
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", 2m, transferLine1.GetQtyCommittedToThisLine());
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", true, pickLine1_2.IsDeleted);
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", true, pickLine1_3.IsDeleted);

			// transfer line with matching lines
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			var pickLine2_1 = Helper.CreateWhsPickLine(transferLine2, inventory1, 3m);

			// mathcing line with picklines to multiple inventories
			var matchingLine1 = Helper.CreateMatchingLine(transferLine2, 3m);
			var pickLine2_2 = Helper.CreateWhsPickLine(matchingLine1, inventory1, 2m);
			var pickLine2_3 = Helper.CreateWhsPickLine(matchingLine1, inventory2, 1m);

			// matching line with not matching pick line
			var matchingLine2 = Helper.CreateMatchingLine(transferLine2, 2m);
			var pickLine2_4 = Helper.CreateWhsPickLine(matchingLine2, inventory3, 2m);

			// matching line without pick lines
			var matchingLine3 = Helper.CreateMatchingLine(transferLine2, 2m);

			var committer2 = new WhsInventoryCommitter(transferLine2);
			committer2.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommitting qty from matching lines should modify qty of main transfer line.", 10m, transferLine2.WE_TransactionQuantity);
			AssertEquals("Qty committed should not be modified on main line.", 3m, transferLine2.GetQtyCommittedToThisLine());
			AssertEquals("Uncommitting qty from matching line should modify its qty.", 2m, matchingLine1.WE_TransactionQuantity);
			AssertEquals("Pick lines to multiple inventories should be removed.", 2m, matchingLine1.GetQtyCommittedToThisLine());
			AssertEquals("Uncommitting all qty from matching line should delete the matching line.", true, matchingLine2.IsDeleted);
			AssertEquals("Uncommitting all qty from matching line should delete the matching line.", true, matchingLine3.IsDeleted);
			AssertEquals("Not matching or pick lines to multiple inventories should be deleted.", true, pickLine2_3.IsDeleted);
			AssertEquals("Not matching or pick lines to multiple inventories should be deleted.", true, pickLine2_4.IsDeleted);
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory_DoesNotUncommitInTransitInventory

		// This shouldn't happen in production as we don't uncommit InTransit (Master) lines, but i've handled it anyway.
		// There are tests which separately pick MatchingLines (for example) which would fail without this
		// e.g. TestPickedTime_SetsAllMatchingLines
		public void TestUncommitOverPickedOrNotMatchingInventory_DoesNotUncommitInTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, locationA1);
			var inventory_DifferentProduct = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m, locationA1);
			var inventory_DifferentHoldCode = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m, locationA1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			inventory_DifferentHoldCode.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory_DifferentHoldCode.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition.", InventoryHoldCodes.Codes.Held, inventory_DifferentHoldCode.WE_WHC_NKCurrentInventoryHeldCode);
			Factory.Save();

			// transfer line committed + 2 pick lines not matching (no matching lines)
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition - InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			// HACK: Tweak pick lines so some don't match
			transferLine.PickLines.DeleteAll();
			var pickLine1 = Helper.CreateWhsPickLine(transferLine, inventory1.Inventory[0], 2m);
			var pickLine2 = Helper.CreateWhsPickLine(transferLine, inventory_DifferentProduct.Inventory[0], 1m);
			var pickLine3 = Helper.CreateWhsPickLine(transferLine, inventory_DifferentHoldCode.Inventory[0], 1m);
			AssertEquals("Precondition.", 4m, transferLine.GetQtyCommittedToThisLine());

			var committer = new WhsInventoryCommitter(transferLine);
			committer.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommitting qty on main transfer line should not modify qty.", 5m, transferLine.WE_TransactionQuantity);
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", 2m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", true, pickLine2.IsDeleted);
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", true, pickLine3.IsDeleted);
			AssertEquals("Should *not* remove lines that are matching besides inventory status.", false, pickLine1.IsDeleted);
		}

		// This shouldn't happen in production as we don't uncommit InTransit (Master) lines, but i've handled it anyway.
		// There are tests which separately pick MatchingLines (for example) which would fail without this
		// e.g. TestPickedTime_SetsAllMatchingLines
		public void TestUncommitOverPickedOrNotMatchingInventory_DoesNotUncommitInTransitInventory_Held()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, locationA1);
			var inventory_DifferentHoldCode = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, locationA1);
			var inventory_NotHeld = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, locationA1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			inventory1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory1.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition.", InventoryHoldCodes.Codes.Held, inventory1.WE_WHC_NKCurrentInventoryHeldCode);

			inventory_DifferentHoldCode.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			inventory_DifferentHoldCode.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition.", InventoryHoldCodes.Codes.Damaged, inventory_DifferentHoldCode.WE_WHC_NKCurrentInventoryHeldCode);
			Factory.Save();

			// transfer line committed + 2 pick lines not matching (no matching lines)
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1.WLV_LocationString, "", locationA2.WLV_LocationString, "", InventoryHoldCodes.Codes.Held);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition - InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			// HACK: Tweak pick lines so some don't match
			transferLine.PickLines.DeleteAll();
			var pickLine1 = Helper.CreateWhsPickLine(transferLine, inventory1.Inventory[0], 3m);
			var pickLine2 = Helper.CreateWhsPickLine(transferLine, inventory_NotHeld.Inventory[0], 1m);
			var pickLine3 = Helper.CreateWhsPickLine(transferLine, inventory_DifferentHoldCode.Inventory[0], 1m);
			AssertEquals("Precondition.", 5m, transferLine.GetQtyCommittedToThisLine());

			var committer = new WhsInventoryCommitter(transferLine);
			committer.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommitting qty on main transfer line should not modify qty.", 5m, transferLine.WE_TransactionQuantity);
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", 3m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", true, pickLine2.IsDeleted);
			AssertEquals("Unmatching pick lines or pick lines linked to different inventories should be removed.", true, pickLine3.IsDeleted);
			AssertEquals("Should *not* remove lines that are matching besides inventory status.", false, pickLine1.IsDeleted);
		}

		#endregion

		#region TestCommitInventory

		#region TestCommitInventory_SplitIntoMultipleTransferLines

		public void TestCommitInventory_SplitIntoMultipleTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-1"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m, data.Whs1.FindLocation("A-1"));
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-1"));
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, "A-1", "A-2");
			AssertEquals("Precondition", 2, transfer.Lines.Count);
			AssertEquals("Precondition - no pick lines were allocated.", 0, transferLine1.PickLines.Count);
			AssertEquals("Precondition - no pick lines were allocated.", 0, transferLine2.PickLines.Count);
			AssertEquals("Precondition - no matching transfer lines should exist.", 0, transferLine1.MatchingLines.Count);
			AssertEquals("Precondition - no matching transfer lines should exist.", 0, transferLine2.MatchingLines.Count);

			var committer1 = new WhsInventoryCommitter(transferLine1);
			committer1.CommitInventory();
			AssertEquals("No new transfer lines should be created.", 2, transfer.Lines.Count);
			AssertEquals("The total quantity of transfer line should not be modified.", 2m, transferLine1.QtyToMoveIncludingMatchingLines);
			AssertEquals("Single pick line should be linked to main transfer line.", 1, transferLine1.PickLines.Count);
			AssertEquals("No inventory should be created for main tranfser line.", 0, transferLine1.Inventory.Count);
			AssertEquals("No matching transfer lines should be created.", 0, transferLine1.MatchingLines.Count);

			var committer2 = new WhsInventoryCommitter(transferLine2);
			committer2.CommitInventory();
			AssertEquals("No new transfer lines should be created.", 2, transfer.Lines.Count);
			AssertEquals("2 new matching transfer lines should be created.", 2, transferLine2.MatchingLines.Count);
			AssertEquals("The total quantity of transfer line should not be modified.", 5m, transferLine2.QtyToMoveIncludingMatchingLines);
			var allTransferLines = new List<WhsTransferLine>();
			allTransferLines.Add(transferLine2);
			allTransferLines.AddRange(transferLine2.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			var line3 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);
			AssertEquals("No inventory should be created for the line.", 0, line1.Inventory.Count);
			AssertEquals("No inventory should be created for the line.", 0, line2.Inventory.Count);
			AssertEquals("No inventory should be created for the line.", 0, line3.Inventory.Count);
		}

		public void TestCommitInventory_SplitIntoMultipleTransferLines_WithReserveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "ABC123");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "ABC123");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine.ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition", 20m, inventory1.InDocketLine.ReservedQuantity);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory1);
			transferLine.QtyToMoveIncludingMatchingLines += inventory2.WI_AvailableToTransferQuantity;

			var reservePickLine = inventory1.ReservedPickLines[0];
			reservePickLine.WZ_WE_InventoryLine = transferLine.PK; // re-assign reserve pick line to transferline
			AssertEquals("Precondition", 1, transfer.Lines.Count);
			AssertEquals("Precondition", 50m, transferLine.WE_TransactionQuantity);
			AssertEquals("Precondition", 20m, transferLine.ReservedQuantity);
			AssertEquals("Precondition - no pick lines were allocated.", 0, transferLine.PickLines.Count);
			AssertEquals("Precondition - no matching transfer lines should exist.", 0, transferLine.MatchingLines.Count);

			var committer = new WhsInventoryCommitter(transferLine);
			committer.CommitInventory();
			AssertEquals("No new transfer lines should be created.", 1, transfer.Lines.Count);
			AssertEquals("The total quantity of transfer line should not be modified.", 50m, transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("1 new matching transfer line should be created.", 1, transferLine.MatchingLines.Count);
			var allTransferLines = new List<WhsTransferLine>();
			allTransferLines.Add(transferLine);
			allTransferLines.AddRange(transferLine.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("No inventory should be created for the line.", 0, line1.Inventory.Count);
			AssertEquals("Transfer lines should have reserved quantity.", true, line1.ReservedPickLines.Count > 0);
			AssertEquals("Transfer lines should have reserved quantity.", 20m, line1.ReservedQuantity);

			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("No inventory should be created for the line.", 0, line2.Inventory.Count);
			AssertEquals("Transfer line should not have reserved quantity.", false, line2.ReservedPickLines.Count > 0);
		}

		public void TestCommitInventory_SplitIntoMultipleTransferLines_WithReserveLines_ReserveLinesAreSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "ABC123");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "ABC123");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine.ReserveStockIfAbleTo(inventory1);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			orderLine2.ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 20m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory2.InDocketLine.ReservedQuantity);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory1);
			transferLine.QtyToMoveIncludingMatchingLines += inventory2.WI_AvailableToTransferQuantity;

			var reservePickLine1 = inventory1.ReservedPickLines[0];
			reservePickLine1.WZ_WE_InventoryLine = transferLine.PK; // re-assign reserve pick line to transferline
			var reservePickLine2 = inventory2.ReservedPickLines[0];
			reservePickLine2.WZ_WE_InventoryLine = transferLine.PK; // re-assign reserve pick line to transferline
			AssertEquals("Precondition", 1, transfer.Lines.Count);
			AssertEquals("Precondition", 50m, transferLine.WE_TransactionQuantity);
			AssertEquals("Precondition", 50m, transferLine.ReservedQuantity);
			AssertEquals("Precondition - no pick lines were allocated.", 0, transferLine.PickLines.Count);
			AssertEquals("Precondition - no matching transfer lines should exist.", 0, transferLine.MatchingLines.Count);

			var committer = new WhsInventoryCommitter(transferLine);
			committer.CommitInventory();
			AssertEquals("No new transfer lines should be created.", 1, transfer.Lines.Count);
			AssertEquals("The total quantity of transfer line should not be modified.", 50m, transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("1 new matching transfer line should be created.", 1, transferLine.MatchingLines.Count);
			var allTransferLines = new List<WhsTransferLine>();
			allTransferLines.Add(transferLine);
			allTransferLines.AddRange(transferLine.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("No inventory should be created for the line.", 0, line1.Inventory.Count);
			AssertEquals("Transfer lines should have reserved quantity.", true, line1.ReservedPickLines.Count > 0);
			AssertEquals("Transfer lines should have reserved quantity.", 20m, line1.ReservedQuantity);

			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("No inventory should be created for the line.", 0, line2.Inventory.Count);
			AssertEquals("Transfer lines should have reserved quantity.", true, line2.ReservedPickLines.Count > 0);
			AssertEquals("Transfer lines should have reserved quantity.", 30m, line2.ReservedQuantity);
		}

		#endregion

		#region TestCommitInventory_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLines

		public void TestCommitInventory_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLines_Source()
		{
			TestCommitInventory_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLinesCore(TransferType.Codes.InterWhsSource);
		}

		public void TestCommitInventory_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLines_Dest()
		{
			TestCommitInventory_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLinesCore(TransferType.Codes.InterWhsDest);
		}

		void TestCommitInventory_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLinesCore(ZString transferSubType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2", "B");

			var jobWhs = (transferSubType == TransferType.Codes.InterWhsSource) ? data.Whs1 : whs2;
			var lineWhs = (transferSubType != TransferType.Codes.InterWhsSource) ? data.Whs1 : whs2;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m, data.Whs1.FindLocation("A"));
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A"));
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, jobWhs, "TR1", Notify, transferSubType);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A", lineWhs.PK, "B");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, "A", lineWhs.PK, "B");
			AssertEquals("Precondition", 2, transfer.Lines.Count);
			AssertEquals("Precondition - no pick lines were allocated.", 0, transferLine1.PickLines.Count);
			AssertEquals("Precondition - no pick lines were allocated.", 0, transferLine2.PickLines.Count);
			AssertEquals("Precondition - no matching transfer lines should exist.", 0, transferLine1.MatchingLines.Count);
			AssertEquals("Precondition - no matching transfer lines should exist.", 0, transferLine2.MatchingLines.Count);

			var committer1 = new WhsInventoryCommitter(transferLine1);
			committer1.CommitInventory();
			AssertEquals("No new transfer lines should be created.", 2, transfer.Lines.Count);
			AssertEquals("The total quantity of transfer line should not be modified.", 2m, transferLine1.QtyToMoveIncludingMatchingLines);
			AssertEquals("Single pick line should be linked to main transfer line.", 1, transferLine1.PickLines.Count);
			AssertEquals("No inventory should be created for main tranfser line.", 0, transferLine1.Inventory.Count);
			AssertEquals("No matching transfer lines should be created.", 0, transferLine1.MatchingLines.Count);

			var committer2 = new WhsInventoryCommitter(transferLine2);
			committer2.CommitInventory();
			AssertEquals("No new transfer lines should be created.", 2, transfer.Lines.Count);
			AssertEquals("The total quantity of transfer line should not be modified.", 5m, transferLine2.QtyToMoveIncludingMatchingLines);
			AssertEquals("2 new matching transfer lines should be created.", 2, transferLine2.MatchingLines.Count);
			var allTransferLines = new List<WhsTransferLine>();
			allTransferLines.Add(transferLine2);
			allTransferLines.AddRange(transferLine2.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			var line3 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);
			AssertEquals("No inventory should be created for the line.", 0, line1.Inventory.Count);
			AssertEquals("No inventory should be created for the line.", 0, line2.Inventory.Count);
			AssertEquals("No inventory should be created for the line.", 0, line3.Inventory.Count);
		}

		#endregion

		#endregion

		#region Implementation

		protected override int ExpectedWhsPickLineHitCountForCommitInventory => 5;

		protected override void AddToExpectedDBHits(Dictionary<string, int> dbHits)
		{
			base.AddToExpectedDBHits(dbHits);

			dbHits.Add(WhsVASOrderSchema.Constants.TableName, 1);
			dbHits.Add(WhsPickFaceSchema.Constants.TableName, 1);
			dbHits.Add(WhsLocationTypeSchema.Constants.TableName, 1);
			dbHits[WhsLocationViewSchema.Constants.TableName] += 1;
		}

		protected override WhsTransfer GetNewTransactionLineParentCore(OrgHeader client, WhsWarehouse warehouse, ZString reference, ZString docketSubType, WhsTestHelperFunctions helper)
		{
			var transfer = helper.CreateWhsTransfer(client != null ? client.PK : ZGuid.Empty, warehouse.PK, reference, Notify);
			if (!docketSubType.IsEmpty)
			{
				transfer.WD_DocketSubType = docketSubType;
			}

			return transfer;
		}

		protected override ZPropertyInfo GetTotalTransactionQtyInfo(WhsTransferLine transactionLine)
		{
			return transactionLine.QtyToMoveIncludingMatchingLinesInfo;
		}

		protected override ZDecimal GetPerPackageQty(WhsTransferLine transactionLine)
		{
			return ((ILineWithMatchingLines<WhsTransferLine>)transactionLine).PerPackageQty;
		}

		protected override void SetInventoryStatusAndHeldCodeCore(WhsTransferLine transactionLine, ZString status, string heldCode)
		{
			transactionLine.WE_WHC_NKOriginalInventoryHeldCode = heldCode;
			transactionLine.WE_OriginalInventoryStatus = status;
		}

		protected override void SetPalletIdToCommitCore(WhsTransferLine transactionLine, ZString palletId)
		{
			transactionLine.WE_TransferFromPalletId = palletId;
		}

		protected override void SetProductCore(WhsTransferLine transactionLine, OrgSupplierPart product)
		{
			transactionLine.WE_OP = product != null ? product.PK : ZGuid.Empty;
		}

		protected override void SetPackGroupIdCore(WhsTransferLine transactionLine, ZString packGroupId)
		{
			transactionLine.WE_PackageGroupId = packGroupId;
		}

		protected override void SetPalletIdCore(WhsTransferLine transactionLine, ZString palletId)
		{
			transactionLine.WE_TransferFromPalletId = palletId;
		}

		protected override WhsTransferLine GetNewTransactionLineCore(WhsTransfer parent, LineWithCommittedPickLinesData data, WhsTestHelperFunctions helper)
		{
			var result = helper.CreateWhsTransferLine(parent, data.ProductPK, data.QuantityToCommit, data.LocationString, "", ZGuid.Empty, data.DestLocationString, "",
				data.ArrivalDate, data.ExpiryDate, data.PackingDate, data.PartAttrib1, data.PartAttrib2, data.PartAttrib3, data.InventoryHeldCode);
			result.WE_SerialNumber = data.SerialNumber;
			result.WE_BondedEntryKey = data.BondedEntryKey;
			result.WE_PackageGroupId = data.PackGroupId;
			result.WE_TransferFromPalletId = data.PalletId;
			result.GS_NKPickedBy = data.PickedBy;
			result.WE_OriginalInventoryStatus = data.InventoryStatus;

			return result;
		}

		protected override WhsInventoryCommitter<WhsTransferLine> GetCommitter(WhsTransferLine transactionLine)
		{
			return new WhsInventoryCommitterWithMatchingLines<WhsTransferLine>(transactionLine);
		}

		#endregion
	}
}
