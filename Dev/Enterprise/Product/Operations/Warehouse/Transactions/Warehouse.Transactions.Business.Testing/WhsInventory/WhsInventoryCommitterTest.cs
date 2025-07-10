using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsInventoryCommitterTest<TLine, TParent> : WhsTestCaseWithFactory
			where TLine : BusinessObject, ILineWithCommittedPickLines
			where TParent : BusinessObject
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetCommitter(null));
		}

		public void TestConstructor_UseCorrectClass()
		{
			var transferLine = Factory.New<WhsTransferLine>();
			AssertExceptionThrown<InvalidOperationException>("Use WhsInventoryCommitterWithMatchingLines<> when you have a transaction line with matching lines.",
				() => new WhsInventoryCommitter<WhsTransferLine>(transferLine));

			AssertNoExceptionThrown(() => new WhsInventoryCommitterWithMatchingLines<WhsTransferLine>(transferLine));
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory

		public void TestUncommitOverPickedOrNotMatchingInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "");
			var inventory = receive.Inventory[0];

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 20m,
				Location = location
			});
			var inventoryCommitmentBuilder = GetCommitter(transactionLine);

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("Precondition - ensure stock is committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure stock is committed.", 20m, transactionLine.GetQtyCommittedToThisLine());

			transactionLine.TransactionQty = 30m;
			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommit inventory should not commit additional stock.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should not commit additional stock.", 20m, transactionLine.GetQtyCommittedToThisLine());

			transactionLine.TransactionQty = 10m;
			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommit inventory should uncommit stock if committed more that required.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should uncommit stock if committed more that required.", 10m, transactionLine.GetQtyCommittedToThisLine());

			SetProduct(transactionLine, data.Part2);
			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommit inventory should uncommit all stock if one of parameters doesn't match anymore.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should uncommit all stock if one of parameters doesn't match anymore.", 0m, transactionLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory_Picked

		public void TestUncommitOverPickedOrNotMatchingInventory_Picked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "");
			var inventory = receive.Inventory[0];

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 20m,
				Location = location
			});
			var inventoryCommitmentBuilder = GetCommitter(transactionLine);

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("Precondition - ensure stock is committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure stock is committed.", 20m, transactionLine.GetQtyCommittedToThisLine());

			var pickLine = transactionLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			SetProduct(transactionLine, data.Part2);
			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommit inventory should *not* uncommit all stock if the pick line is already picked, even if one of parameters doesn't match anymore.", 20m, transactionLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory_PickedFromPutawayLocation

		public void TestUncommitOverPickedOrNotMatchingInventory_PickedFromPutawayLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "");
			var inventory = receive.Inventory[0];

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 20m,
				Location = location
			});
			var inventoryCommitmentBuilder = GetCommitter(transactionLine);

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("Precondition - ensure stock is committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure stock is committed.", 20m, transactionLine.GetQtyCommittedToThisLine());

			var pickLine = transactionLine.PickLines.Single();
			pickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.NewZGuid();

			SetProduct(transactionLine, data.Part2);
			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Uncommit inventory should *not* uncommit all stock if the pick line is already picked, even if one of parameters doesn't match anymore.", 20m, transactionLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestUncommitOverPickedOrNonMatchingInventory_ChecksPackageGroupId

		public void TestUncommitOverPickedOrNonMatchingInventory_ChecksPackageGroupId()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PutawayArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(year, 01, 01, 05, 55, 55));
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "ABC", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "XYZ", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				ArrivalDate = new ZDateTimeOffset(year, 01, 01),
				Product = data.Part1,
				QuantityToCommit = 10m,
				Location = location,
				DestLocation = data.Whs1.FindLocation("A-2"),
				BondedEntryKey = "123-1",
				PackGroupId = "ABC"
			});

			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals(10m, transactionLine.GetQtyCommittedToThisLine());
			AssertEquals(1, transactionLine.PickLines.Count);

			var pickLineToInventory1 = transactionLine.PickLines[0];
			AssertEquals(inventory1, pickLineToInventory1.Inventory);

			SetPackGroupId(transactionLine, "XYZ");
			AssertEquals("Precondition", false, pickLineToInventory1.IsDeleted);

			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals(true, pickLineToInventory1.IsDeleted);
		}

		#endregion

		#region TestUncommitOverPickedOrNonMatchingInventory_ArrivalDatesComparedWithoutTimePart

		public void TestUncommitOverPickedOrNonMatchingInventory_ArrivalDatesComparedWithoutTimePart()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(year, 01, 01, 05, 55, 55));
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				ArrivalDate = new ZDateTimeOffset(year, 01, 01),
				Product = data.Part1,
				QuantityToCommit = 10m,
				Location = location,
				DestLocation = data.Whs1.FindLocation("A-2")
			});
			parent.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", 10m, transactionLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", 1, transactionLine.PickLines.Count);

			var pickLineForInventory = transactionLine.PickLines[0];
			AssertEquals("Precondition", inventory, pickLineForInventory.Inventory);

			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("Pick line should not be deleted since arrival date's time part doesn't match.", false, pickLineForInventory.IsDeleted);
		}

		#endregion

		#region TestDoesPickLineNotMatchTransactionLine_PalletIDCaseInsensitive

		public void TestDoesPickLineNotMatchTransactionLine_PalletIDCaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "palletID");
			var inventory = receive.Inventory[0];

			AssertIsFinalisedPrecondition(receive);

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 10m,
				Location = location,
				PalletId = "palletID"
			});
			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, transactionLine.GetQtyCommittedToThisLine());

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit stock.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 10m, transactionLine.GetQtyCommittedToThisLine());

			var pickLineToInventory = transactionLine.PickLines[0];
			AssertEquals(inventory, pickLineToInventory.Inventory);

			SetPalletId(transactionLine, "PALLETID");
			AssertEquals("Precondition", false, pickLineToInventory.IsDeleted);

			inventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			AssertEquals("PalletID case insensitive. Pickline should not be deleted.", false, pickLineToInventory.IsDeleted);
		}

		#endregion

		#region TestCommitInventory

		#region TestCommitInventory

		public void TestCommitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "");
			var inventory = receive.Inventory[0];

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 10m,
				Location = location
			});
			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, transactionLine.GetQtyCommittedToThisLine());

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit stock.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 10m, transactionLine.GetQtyCommittedToThisLine());

			transactionLine.TransactionQty = 100m;
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit no more that available stock.", 50m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit no more that available stock.", 50m, transactionLine.GetQtyCommittedToThisLine());

			transactionLine.TransactionQty = 20m;
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should uncommit stock when required.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should uncommit stock when required.", 20m, transactionLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestCommitInventory_Attributes

		public void TestCommitInventory_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.PackingDate, true);
			Factory.Save();

			var packingDate = ZDate.Today.AddYears(-2);
			var expiryDate = ZDate.Today.AddDays(2);
			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			Helper.SetDocketLineAttributes(receiveLine1, expiryDate, packingDate, "PA1", "PA2", "PA3", "SN1");

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			Helper.SetDocketLineAttributes(receiveLine2, expiryDate, packingDate, "PA11", "PA22", "PA3", "SN2");

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 4m, location);
			Helper.SetDocketLineAttributes(receiveLine3, today, today, "PA11", "PA2", "PA33", "");

			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			Helper.SetDocketLineAttributes(receiveLine4, today, today, "PA11", "PA22", "PA3", "SN4");

			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location);
			Helper.SetDocketLineAttributes(receiveLine5, expiryDate, packingDate, "PA11", "PA22", "PA33", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "1");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 1m,
				Location = location,
				PartAttrib1 = "PA11",
				PartAttrib2 = "PA22",
				PartAttrib3 = "PA3",
				SerialNumber = "SN4",
				PackingDate = today,
				ExpiryDate = today
			});
			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, transactionLine.GetQtyCommittedToThisLine());

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit stock.", 1m, transactionLine.GetQtyCommittedToThisLine());

			transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part2,
				QuantityToCommit = 5m,
				Location = location,
				PartAttrib1 = "PA11",
				PartAttrib2 = "PA2",
				PartAttrib3 = "PA33",
				PackingDate = today,
				ExpiryDate = today
			});

			inventoryCommitmentBuilder = GetCommitter(transactionLine);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit no more than available stock.", 4m, transactionLine.GetQtyCommittedToThisLine());

			transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 15m,
				Location = location,
				PartAttrib1 = "PA1",
				PartAttrib2 = "PA2",
				PartAttrib3 = "PA3",
				SerialNumber = "SN",
				PackingDate = packingDate,
				ExpiryDate = expiryDate
			});

			inventoryCommitmentBuilder = GetCommitter(transactionLine);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit no more than available stock.", 0m, transactionLine.GetQtyCommittedToThisLine());

			transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 15m,
				Location = location,
				PartAttrib1 = "PA1",
				PartAttrib2 = "PA2",
				PartAttrib3 = "PA3",
				SerialNumber = "SN1",
				PackingDate = packingDate,
				ExpiryDate = expiryDate
			});

			inventoryCommitmentBuilder = GetCommitter(transactionLine);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit no more than available stock.", 1m, transactionLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestCommitInventory_DifferentInventoryStatuses

		public void TestCommitInventory_DifferentInventoryStatuses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryAVL = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 20m, data.Whs1.FindLocation("A-1").PK, "", "");
			var inventoryHEL = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 25m, data.Whs1.FindLocation("A-1").PK, "", "", InventoryStatus.Codes.Held);
			var inventoryDMG = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 50m,
				Location = data.Whs1.FindLocation("A-1"),
				InventoryStatus = InventoryStatus.Codes.Available
			});
			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryAVL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, transactionLine.GetQtyCommittedToThisLine());

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit stock.", 20m, inventoryAVL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 20m, transactionLine.GetQtyCommittedToThisLine());

			SetInventoryStatusAndHeldCode(transactionLine, InventoryStatus.Codes.Held);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryAVL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 25m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 25m, transactionLine.GetQtyCommittedToThisLine());

			SetInventoryStatusAndHeldCode(transactionLine, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryAVL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 30m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 30m, transactionLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestCommitInventory_DifferentInventoryHeldCodes

		public void TestCommitInventory_DifferentInventoryHeldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventoryHeldCode = Factory.NewWithValidTestData<WhsInventoryHeldCode>();
			inventoryHeldCode.WHC_Code = "AAA";
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryAAA = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 20m, data.Whs1.FindLocation("A-1").PK, "", "");
			var inventoryHEL = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 25m, data.Whs1.FindLocation("A-1").PK, "", "");
			var inventoryDMG = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");

			inventoryAAA.OriginalInventoryHeldCode = inventoryHeldCode.WHC_Code;
			inventoryHEL.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			inventoryDMG.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 30m,
				Location = data.Whs1.FindLocation("A-1"),
				InventoryStatus = InventoryStatus.Codes.Held,
				InventoryHeldCode = inventoryHeldCode.WHC_Code
			});

			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryAAA.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, transactionLine.GetQtyCommittedToThisLine());

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit stock.", 20m, inventoryAAA.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 20m, transactionLine.GetQtyCommittedToThisLine());

			SetInventoryStatusAndHeldCode(transactionLine, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryAAA.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 25m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 25m, transactionLine.GetQtyCommittedToThisLine());

			SetInventoryStatusAndHeldCode(transactionLine, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryAAA.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 30m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 30m, transactionLine.GetQtyCommittedToThisLine());

			inventoryDMG.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "AAA";
			Factory.Save();
			transactionLine.TransactionQty = 50m;

			SetInventoryStatusAndHeldCode(transactionLine, InventoryStatus.Codes.Held, "BBB");
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals(0m, transactionLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryAAA.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, transactionLine.GetQtyCommittedToThisLine());

			SetInventoryStatusAndHeldCode(transactionLine, InventoryStatus.Codes.Held, "AAA");
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit stock.", 20m, inventoryAAA.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("InventoryDMG is now *not damaged* but Held with AAA code - CommitInventory should commit stock.", 30m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);

			inventoryDMG.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			Factory.Save();

			SetInventoryStatusAndHeldCode(transactionLine, InventoryStatus.Codes.Held, ""); // Special case for adjustments created from stocktake
			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should commit stock.", 20m, inventoryAAA.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory status doesn't match so stock should not be committed.", 0m, inventoryDMG.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 25m, inventoryHEL.CommittedQuantityIncludingUnfinalisedReceipt);
		}

		#endregion

		#region TestCommitInventory_CommitsOnlyAvailableToTransferStock

		public void TestCommitInventory_CommitsOnlyAvailableToTransferStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, locationA1, "");
			var inventory = receive.Inventory[0];

			// commit to transfer
			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1");
			GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 1m,
				Location = data.Whs1.FindLocation("A-1"),
				DestLocation = data.Whs1.FindLocation("A-2")
			});
			parent.RunPreSaveValidation(); // to committ stock

			// reserve
			var order_Reserve = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine_Reserve = Helper.CreateWhsOrderLine(order_Reserve, data.Part1, 2m);
			Helper.CreateReservePickLine(orderLine_Reserve, inventory, 2m);
			Factory.Save();

			// commit to order
			var order_Commit = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
			var orderLine_Commit = Helper.CreateWhsOrderLine(order_Commit, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order_Commit);

			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 10m,
				Location = data.Whs1.FindLocation("A-1"),
				DestLocation = data.Whs1.FindLocation("A-2")
			});
			var inventoryCommitter = GetCommitter(transactionLine);
			AssertEquals("Precondition", 0m, transactionLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", 3m, inventory.WI_AvailableToTransferQuantity);

			inventoryCommitter.CommitInventory();
			AssertEquals("Committer should commit only stock available to transfer.", 3m, transactionLine.GetQtyCommittedToThisLine());
			AssertEquals(0m, inventory.WI_AvailableToTransferQuantity);
		}

		#endregion

		#region TestCommitInventory_DoesNotCreatesEmptyPicklines

		public void TestCommitInventory_DoesNotCreateEmptyPicklines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 15m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 15m, locationA1, "");
			Factory.Save();

			// commit to order
			var orderCommit = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
			Helper.CreateWhsOrderLine(orderCommit, data.Part1, 15m);
			Helper.CreatePickNew(orderCommit);
			Factory.Save();

			// commit to transfer
			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 20m, // pick more than we have to force it to pick all inventory
				Location = locationA1,
				DestLocation = data.Whs1.FindLocation("A-2")
			});
			var inventoryCommitter = GetCommitter(transactionLine);
			AssertEquals("Precondition", 0m, transactionLine.GetQtyCommittedToThisLine());

			inventoryCommitter.CommitInventory();
			AssertEquals("Committer should commit only stock available to transfer.", 15m, transactionLine.GetQtyCommittedToThisLine());
			AssertEquals("Should have 15 units picked.", 15m, transactionLine.PickLines.Cast<WhsPickLine>().Sum(p => p.WZ_Units));
			Assert("All picklines should have positive quantity.", transactionLine.PickLines.Cast<WhsPickLine>().All(p => p.WZ_Units > 0));
		}

		#endregion

		#region TestCommitInventory_GetNotEnoughStockMessage

		public void TestCommitInventory_GetNotEnoughStockMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 12m,
				Location = data.Whs1.DefaultLocation
			});
			var inventoryCommittmentHelper = GetCommitter(transactionLine);

			var expectedErrorMessage1 = string.Format(
@"Attempted to {1} 12 Units, but only 10 Units are available for {0} out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the {0} line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to {1}.
If you are trying to {1} stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to {1} stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", transactionLine.Noun, transactionLine.Verb);

			inventoryCommittmentHelper.CommitInventory();
			transactionLine.RunPreSaveValidation();
			AssertHasError(GetTotalTransactionQtyInfo(transactionLine), expectedErrorMessage1);

			var expectedErrorMessage2 = string.Format(
@"Attempted to {1} 12 Units, but no Units are available for {0} out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the {0} line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to {1}.
If you are trying to {1} stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to {1} stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", transactionLine.Noun, transactionLine.Verb);
			SetPalletIdToCommit(transactionLine, "PLT-1");

			inventoryCommittmentHelper.CommitInventory();
			transactionLine.RunPreSaveValidation();
			AssertHasError(GetTotalTransactionQtyInfo(transactionLine), expectedErrorMessage2);
		}

		#endregion

		#region TestCommitInventory_GetNotEnoughStockMessage_USBonded

		public void TestCommitInventory_GetNotEnoughStockMessage_USBonded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1", docketSubType: AdjustmentType.Codes.Customs);
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 12m,
				Location = data.Whs1.DefaultLocation
			});
			var inventoryCommittmentHelper = GetCommitter(transactionLine);

			var expectedErrorMessage1 = string.Format(
@"Attempted to {1} 12 Units, but no Units are available for {0} out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the {0} line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to {1}.
If you are trying to {1} stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to {1} stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", transactionLine.Noun, transactionLine.Verb);

			inventoryCommittmentHelper.CommitInventory();
			transactionLine.RunPreSaveValidation();
			AssertHasError(GetTotalTransactionQtyInfo(transactionLine), expectedErrorMessage1);

			var expectedErrorMessage2 = string.Format(
@"Attempted to {1} 12 Units, but no Units are available for {0} out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the {0} line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to {1}.
If you are trying to {1} stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to {1} stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.
If you are trying to {1} stock with a Package Group ID you must enter it exactly. Blank Package Group IDs will only match to inventory with blank Package Group IDs.", transactionLine.Noun, transactionLine.Verb);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			inventoryCommittmentHelper.CommitInventory();
			transactionLine.RunPreSaveValidation();
			AssertHasError(GetTotalTransactionQtyInfo(transactionLine), expectedErrorMessage2);
		}

		#endregion

		#region TestCommitInventory_GetNotEnoughStockMessage_WithAttributes

		public void TestCommitInventory_GetNotEnoughStockMessage_WithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A"), ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 12m,
				Location = data.Whs1.FindLocation("A"),
				PartAttrib1 = "PA1"
			});
			var inventoryCommittmentHelper = GetCommitter(transactionLine);

			var expectedErrorMessage1 = string.Format(
@"Attempted to {1} 12 Units, but only 10 Units are available for {0} out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the {0} line.
Check that all the attributes exactly match the attributes on the inventory you are trying to {1}.
If you are trying to {1} stock with attributes, you must enter the attribute exactly. Blank non mandatory attributes will only match to inventory with blank attributes.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to {1}.
If you are trying to {1} stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to {1} stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", transactionLine.Noun, transactionLine.Verb);

			inventoryCommittmentHelper.CommitInventory();
			transactionLine.RunPreSaveValidation();
			AssertHasError(GetTotalTransactionQtyInfo(transactionLine), expectedErrorMessage1);

			var expectedErrorMessage2 = string.Format(
@"Attempted to {1} 12 Units, but no Units are available for {0} out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the {0} line.
Check that all the attributes exactly match the attributes on the inventory you are trying to {1}.
If you are trying to {1} stock with attributes, you must enter the attribute exactly. Blank non mandatory attributes will only match to inventory with blank attributes.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to {1}.
If you are trying to {1} stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to {1} stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", transactionLine.Noun, transactionLine.Verb);
			SetPalletIdToCommit(transactionLine, "PLT-1");

			inventoryCommittmentHelper.CommitInventory();
			transactionLine.RunPreSaveValidation();
			AssertHasError(GetTotalTransactionQtyInfo(transactionLine), expectedErrorMessage2);
		}

		#endregion

		#region TestCommitInventory_GetNotEnoughStockMessage_WithCustomAttributes

		public void TestCommitInventory_GetNotEnoughStockMessage_WithCustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var today = ZDateTime.Today;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			Helper.SetInventoryCustomAttributes(inventory1, "CA11", "CA21", "CA31", "CA41", "CA51", "CA61", 11m, 12m, 13m, 14m, 15m, today.AddDays(1), today.AddDays(2), today.AddDays(3), today.AddDays(4), today.AddDays(5), true, true, true, true, true, "TB1");
			Helper.SetInventoryCustomAttributes(inventory2, "CA12", "CA22", "CA32", "CA42", "CA52", "CA62", 21m, 22m, 23m, 24m, 25m, today.AddDays(11), today.AddDays(12), today.AddDays(13), today.AddDays(14), today.AddDays(15), true, true, true, true, true, "TB2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 20m,
				Location = data.Whs1.FindLocation("A-1")
			});
			var customAttribs = new TestILineCustomAttributes("CA11", "CA21", "CA31", "CA41", "CA51", "CA61", 11m, 12m, 13m, 14m, 15m, today.AddDays(1), today.AddDays(2), today.AddDays(3), today.AddDays(4), today.AddDays(5), true, true, true, true, true, "TB1");
			((ILineCustomAttributes)transactionLine).SetCustomAttributes(customAttribs);
			var inventoryCommittmentHelper = GetCommitter(transactionLine);
			inventoryCommittmentHelper.CommitInventory();
			AssertEquals("Custom Attributes are job level attributes, so should not affect stock allocation.", 20m, inventoryCommittmentHelper.TotalQtyCommitted);
			AssertNoErrors("Custom Attributes are job level attributes, so should not affect stock allocation.", GetTotalTransactionQtyInfo(transactionLine));
		}

		#endregion

		#region TestCommitInventory_WithPackageGroupIDAndPerPackageQty

		public void TestCommitInventory_WithPackageGroupIDAndPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON");
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = "CUS";
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "ABC", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "XYZ", 2m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "XYZ", 2m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location.PK, "123-1", "", 1m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "", docketSubType: AdjustmentType.Codes.Customs);
			var transactionLine1 = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 20m,
				Location = location,
				DestLocation = data.Whs1.FindLocation("A-2"),
				BondedEntryKey = "123-1",
				PackGroupId = "ABC"
			});
			var inventoryCommitmentBuilder1 = GetCommitter(transactionLine1);
			inventoryCommitmentBuilder1.CommitInventory();
			AssertEquals(10m, transactionLine1.GetQtyCommittedToThisLine()); // cannot commit 20 as there are only 10 available with Package Group ID 'ABC'
			AssertEquals(5m, GetPerPackageQty(transactionLine1));

			var transactionLine2 = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 4m,
				Location = location,
				DestLocation = data.Whs1.FindLocation("A-2"),
				BondedEntryKey = "123-1",
				PackGroupId = "XYZ"
			});
			var inventoryCommitmentBuilder2 = GetCommitter(transactionLine2);
			inventoryCommitmentBuilder2.CommitInventory();
			AssertEquals(4m, transactionLine2.GetQtyCommittedToThisLine());
			AssertEquals(2m, GetPerPackageQty(transactionLine2));

			inventory3.PerPackageQty = 4m; // Simulate having the same matching inventory with two different per package quantities, should never occur.
			transactionLine2.PickLines.DeleteAll(); // Uncommit everything
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not have multiple matching inventory for Docket Line 2 with different Per Package Quantities.",
				() => inventoryCommitmentBuilder2.CommitInventory());

			var transactionLine3 = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 3m,
				Location = location,
				DestLocation = data.Whs1.FindLocation("A-2"),
				BondedEntryKey = "123-1"
			});
			var inventoryCommitmentBuilder3 = GetCommitter(transactionLine3);
			inventoryCommitmentBuilder3.CommitInventory();
			AssertEquals(2m, transactionLine3.GetQtyCommittedToThisLine()); // cannot commit 3 as there are only 2 available with no Package Group ID
			AssertEquals(1m, GetPerPackageQty(transactionLine3));
		}

		#endregion

		#region TestCommitInventoryWithInvalidData

		public void TestCommitInventoryWithInvalidData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var parent = GetNewTransactionLineParent(null, data.Whs1, "T1");
			var lineWithoutPartAndLocations = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData { QuantityToCommit = 1m });
			var lineWithPartAndWithoutLocations = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData { Product = data.Part1, QuantityToCommit = 1m });
			var lineWithPartAndWithLocations = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 1m,
				Location = data.Whs1.FindLocation("A-1"),
				DestLocation = data.Whs1.FindLocation("A-2")
			});

			var commiterForLineWithoutPartAndLocations = GetCommitter(lineWithoutPartAndLocations);
			AssertNoExceptionThrown(() => commiterForLineWithoutPartAndLocations.CommitInventory());

			var commiterForLineWithPartAndWithoutLocations = GetCommitter(lineWithPartAndWithoutLocations);
			AssertNoExceptionThrown(() => commiterForLineWithPartAndWithoutLocations.CommitInventory());

			var commiterForLineWithPartAndWithLocations = GetCommitter(lineWithPartAndWithLocations);
			AssertNoExceptionThrown(() => commiterForLineWithPartAndWithLocations.CommitInventory());
		}

		#endregion

		#region TestCommitInventory_PickInformation

		public void TestCommitInventory_PickInformation()
		{
			if (typeof(ILineWithPickAndPutawayDetails).IsAssignableFrom(typeof(TLine)))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
				var staff2 = Helper.CreateGlbStaff("S2", "Staff2");
				var location = data.Whs1.DefaultLocation;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "");
				var inventory = receive.Inventory[0];

				var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
				var lineWithPickedBy1 = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
				{
					Product = data.Part1,
					QuantityToCommit = 10m,
					Location = location,
					PickedBy = staff1.GS_Code
				});
				var lineWithPickedBy2 = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
				{
					Product = data.Part1,
					QuantityToCommit = 10m,
					Location = location,
					PickedBy = staff2.GS_Code
				});
				var lineWithoutPickedBy = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
				{
					Product = data.Part1,
					QuantityToCommit = 10m,
					Location = location
				});

				AssertPickLine(lineWithPickedBy1, staff1.GS_Code);
				AssertPickLine(lineWithPickedBy2, staff2.GS_Code);
				AssertPickLine(lineWithoutPickedBy, "");
			}
			else
			{
				// this test is not relevant for this subclass
				Assert(true);
			}
		}

		void AssertPickLine(TLine transactionLineWithPickedTime, string expectedStaffCode)
		{
			var inventoryCommitmentBuilder = GetCommitter(transactionLineWithPickedTime);
			AssertEquals("Precondition - No pick lines should be generated.", 0, transactionLineWithPickedTime.PickLines.Count);

			inventoryCommitmentBuilder.CommitInventory();
			var pickLine = transactionLineWithPickedTime.PickLines.Single();
			AssertEquals(expectedStaffCode, pickLine.WZ_GS_NKAssignedTo);
		}

		#endregion

		#region TestCommitInventory_WithInvalidDateTime

		public void TestCommitInventory_WithInvalidDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				ArrivalDate = ZDateTimeOffset.Invalid,
				DestLocation = data.Whs1.FindLocation("A-2"),
				ExpiryDate = ZDate.Invalid,
				Location = data.Whs1.FindLocation("A-1"),
				PackingDate = ZDate.Invalid,
				Product = data.Part1,
				QuantityToCommit = 2m
			});

			AssertNoExceptionThrown(() => GetCommitter(transactionLine).CommitInventory());
		}

		#endregion

		#region TestCommitInventoryWithVASOrderTransferInInventory

		public void TestCommitInventoryWithVASOrderTransferInInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var inventory = intoServiceAreaTransfer.Lines.Single().Inventory[0];

			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "");
			var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
			{
				Product = data.Part1,
				QuantityToCommit = 5m,
				Location = inventory.Location
			});

			var inventoryCommitmentBuilder = GetCommitter(transactionLine);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, transactionLine.GetQtyCommittedToThisLine());

			inventoryCommitmentBuilder.CommitInventory();
			AssertEquals("CommitInventory should not commit stock.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should not commit stock.", 0m, transactionLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestCommitInventory_DbHits

		public void TestCommitInventory_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			for (var i = 0; i < 10; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i, Helper.Notify);
				for (var j = 0; j < 20; j++)
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "");
				}

				receive.FinaliseDocket();
			}
			Factory.Save();

			var rfUser = Helper.CreateGlbStaff("S1", "S1");
			var sourceLocation = data.Whs1.DefaultLocation;
			var destinationLocation = data.Whs1.FindLocation("A-2");
			for (int count = 0; count < 30; count++)
			{
				// Adjustment Out
				var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A" + count.ToString());
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -1m, data.Whs1.DefaultLocation);
				adjustmentLine.RunPreSaveValidation(); // to commit stock;
				AssertEquals("Precondition: Stock is committed.", 1m, adjustmentLine.CommittedQuantity);

				// Order + Pick
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + count.ToString(), data.Part1, 1m);
				Helper.CreatePickNew(order);

				// Transfer
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T" + count.ToString());
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation, rfUser);
				transferLine.RunPreSaveValidation(); // to commit stock;
				AssertEquals("Precondition: Stock is committed.", 1m, transferLine.QtyCommittedIncludingMatchingLines);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newFactoryHelper = new WhsTestHelperFunctions(newFactory);
			var parent = GetNewTransactionLineParent(data.Org1, data.Whs1, "TR1", newFactoryHelper);

			using (RowFactory.SetCachedTables())
			{
				var transactionLine = GetNewTransactionLine(parent, new LineWithCommittedPickLinesData
				{
					DestLocation = newFactory.Load<WhsLocation>(data.Whs1.FindLocation("A-2").PK),
					Location = newFactory.Load<WhsLocation>(data.Whs1.DefaultLocation.PK),
					Product = newFactory.Load<OrgSupplierPart>(data.Part1.PK),
					QuantityToCommit = 10m
				}, newFactoryHelper);

				GetCommitter(transactionLine).CommitInventory();
			}

			var expectedDBHits = new Dictionary<string, int>();
			expectedDBHits.Add(OrgAddressSchema.Constants.TableName, 1);
			expectedDBHits.Add(OrgCompanyDataSchema.Constants.TableName, 1);
			expectedDBHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			expectedDBHits.Add(OrgMiscServSchema.Constants.TableName, 1);
			expectedDBHits.Add(OrgPartRelationSchema.Constants.TableName, 1);
			expectedDBHits.Add(OrgPartUnitSchema.Constants.TableName, 1);
			expectedDBHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			expectedDBHits.Add(RefUNLOCOSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsLocationViewSchema.Constants.TableName, 3);
			expectedDBHits.Add(WhsAreaSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsRowSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsDocketSchema.Constants.TableName, 2);
			expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, ExpectedWhsPickLineHitCountForCommitInventory);
			expectedDBHits.Add(WhsDocketLineSchema.Constants.TableName, 5);
			expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1);
			expectedDBHits.Add(RefPacksSchema.Constants.TableName, 1);
			AddToExpectedDBHits(expectedDBHits);
			AssertDbHits(expectedDBHits, newFactory);
		}

		protected virtual int ExpectedWhsPickLineHitCountForCommitInventory => 4;

		protected virtual void AddToExpectedDBHits(Dictionary<string, int> dbHits)
		{
		}

		#endregion

		#endregion

		#region Implementation

		TParent GetNewTransactionLineParent(OrgHeader client, WhsWarehouse warehouse, ZString reference, WhsTestHelperFunctions helper = null, string docketSubType = "")
		{
			return GetNewTransactionLineParentCore(client, warehouse, reference, docketSubType, helper ?? Helper);
		}

		protected abstract TParent GetNewTransactionLineParentCore(OrgHeader client, WhsWarehouse warehouse, ZString reference, ZString docketSubType, WhsTestHelperFunctions helper);

		protected class LineWithCommittedPickLinesData
		{
			public LineWithCommittedPickLinesData()
			{
				InventoryStatus = CodeLists.InventoryStatus.Codes.Available;
			}

			public ZString LocationString
			{
				get { return Location != null ? Location.ToLocationString() : ZString.Empty; }
			}

			public ZString DestLocationString
			{
				get { return DestLocation != null ? DestLocation.ToLocationString() : ZString.Empty; }
			}

			public ZGuid ProductPK
			{
				get { return Product != null ? Product.PK : ZGuid.Empty; }
			}

			public OrgSupplierPart Product { get; set; }
			public ZDecimal QuantityToCommit { get; set; }
			public WhsLocation Location { get; set; }
			public string PalletId { get; set; }
			public WhsLocation DestLocation { get; set; }
			public ZString InventoryStatus { get; set; }
			public ZString InventoryHeldCode { get; set; }
			public ZDateTimeOffset ArrivalDate { get; set; }
			public ZString BondedEntryKey { get; set; }
			public ZString PackGroupId { get; set; }
			public ZString PartAttrib1 { get; set; }
			public ZString PartAttrib2 { get; set; }
			public ZString PartAttrib3 { get; set; }
			public ZString SerialNumber { get; set; }
			public ZDate ExpiryDate { get; set; }
			public ZDate PackingDate { get; set; }

			// Picking
			public ZString PickedBy { get; set; }
		}

		protected abstract ZPropertyInfo GetTotalTransactionQtyInfo(TLine transactionLine);
		protected abstract ZDecimal GetPerPackageQty(TLine transactionLine);

		#region SetInventoryStatusAndHeldCode

		void SetInventoryStatusAndHeldCode(TLine transactionLine, ZString status, string heldCode = "")
		{
			SetInventoryStatusAndHeldCodeCore(transactionLine, status, heldCode);
			AssertEquals("Precondition: Status was set correctly.", status, transactionLine.TransactionInventoryStatus);
			AssertEquals("Precondition: HeldCode was set correctly.", heldCode, transactionLine.TransactionInventoryHeldCode);
		}

		protected abstract void SetInventoryStatusAndHeldCodeCore(TLine transactionLine, ZString status, string heldCode);

		#endregion

		#region SetPalletIdToCommit

		void SetPalletIdToCommit(TLine transactionLine, ZString palletId)
		{
			SetPalletIdToCommitCore(transactionLine, palletId);
			AssertEquals("Precondition: Pallet Id was set correctly.", palletId, transactionLine.PalletIDToCommit);
		}

		protected abstract void SetPalletIdToCommitCore(TLine transactionLine, ZString palletId);

		#endregion

		#region SetProduct

		void SetProduct(TLine transactionLine, OrgSupplierPart product)
		{
			SetProductCore(transactionLine, product);
			AssertEquals("Precondition: Product was set correctly.", product != null ? product.PK : ZGuid.Empty, transactionLine.ProductPK);
		}

		protected abstract void SetProductCore(TLine transactionLine, OrgSupplierPart product);

		#endregion

		#region SetPackGroupId

		void SetPackGroupId(TLine transactionLine, ZString packGroupId)
		{
			SetPackGroupIdCore(transactionLine, packGroupId);
			AssertEquals("Precondition: Package Group Id was set correctly.", packGroupId, transactionLine.PackageGroupID);
		}

		protected abstract void SetPackGroupIdCore(TLine transactionLine, ZString packGroupId);

		#endregion

		#region SetPalletId

		void SetPalletId(TLine transactionLine, ZString palletId)
		{
			SetPalletIdCore(transactionLine, palletId);
			AssertEquals("Precondition: Pallet Id was set correctly.", palletId, transactionLine.PalletIDToCommit);
		}

		protected abstract void SetPalletIdCore(TLine transactionLine, ZString palletId);

		#endregion

		#region GetNewTransactionLine

		TLine GetNewTransactionLine(TParent parent, LineWithCommittedPickLinesData data, WhsTestHelperFunctions helper = null)
		{
			var result = GetNewTransactionLineCore(parent, data, helper ?? Helper);
			AssertEquals("Precondition: Arrival Date is correct.", data.ArrivalDate, result.ArrivalDate);
			AssertEquals("Precondition: Bonded Entry Key is correct.", data.BondedEntryKey, result.BondedEntryKey);
			AssertEquals("Precondition: Expiry Date is correct.", data.ExpiryDate, result.ExpiryDate);
			AssertEquals("Precondition: Location is correct.", data.Location, result.LocationToCommit);
			AssertEquals("Precondition: Package Group ID is correct.", data.PackGroupId, result.PackageGroupID);
			AssertEquals("Precondition: Packing Date is correct.", data.PackingDate, result.PackingDate);
			AssertEquals("Precondition: Parent is correct.", parent.PK, result.ParentDocketPK);
			AssertEquals("Precondition: PartAttrib1 is correct.", data.PartAttrib1, result.PartAttrib1);
			AssertEquals("Precondition: PartAttrib2 is correct.", data.PartAttrib2, result.PartAttrib2);
			AssertEquals("Precondition: PartAttrib3 is correct.", data.PartAttrib3, result.PartAttrib3);
			AssertEquals("Precondition: SerialNumber is correct.", data.SerialNumber, result.SerialNumber);
			AssertEquals("Precondition: Product is correct.", data.ProductPK, result.ProductPK);
			AssertEquals("Precondition: Inventory Status is correct.", data.InventoryStatus, result.TransactionInventoryStatus);
			AssertEquals("Precondition: InventoryHeldCode is correct.", data.InventoryHeldCode, result.TransactionInventoryHeldCode);
			AssertEquals("Precondition: Quantity is correct.", data.QuantityToCommit, result.TransactionQty);

			var lineWithPickingDetails = result as ILineWithPickAndPutawayDetails;
			if (lineWithPickingDetails != null)
			{
				AssertEquals("Precondition: Picked By is correct.", data.PickedBy, lineWithPickingDetails.PickedBy);
			}

			return result;
		}

		protected abstract TLine GetNewTransactionLineCore(TParent parent, LineWithCommittedPickLinesData data, WhsTestHelperFunctions helper);

		#endregion

		#region GetCommitter

		protected virtual WhsInventoryCommitter<TLine> GetCommitter(TLine transactionLine)
		{
			return new WhsInventoryCommitter<TLine>(transactionLine);
		}

		#endregion

		#endregion
	}
}
