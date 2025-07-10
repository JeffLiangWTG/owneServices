using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(UpdateInventoryHeldCodeActionMethodApplicator))]
	class UpdateInventoryHeldCodeActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestSelectedInventoryHeldCode

		public void TestSelectedInventoryHeldCode()
		{
			Applicator.SelectedInventoryHeldCode = "";
			AssertNoErrors(Applicator.SelectedInventoryHeldCodeInfo);

			Applicator.SelectedInventoryHeldCode = string.Empty;
			AssertEquals(string.Empty, Applicator.SelectedInventoryHeldCode);
			AssertNoErrors(Applicator.SelectedInventoryHeldCodeInfo);

			Applicator.SelectedInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertEquals(InventoryHoldCodes.Codes.Damaged, Applicator.SelectedInventoryHeldCode);
			AssertNoErrors(Applicator.SelectedInventoryHeldCodeInfo);

			Applicator.SelectedInventoryHeldCode = "ABC";
			AssertEquals("ABC", Applicator.SelectedInventoryHeldCode);
			AssertHasError(Applicator.SelectedInventoryHeldCodeInfo, "Enter a valid selection.");
		}

		#endregion

		#region TestAction_HoldCodeReason

		public void TestAction_HoldCodeReason()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var unFinalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, allocateLocations: true, finalise: false);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m);
			Factory.Save();

			var inventoryForUnFinalisedReceive = (WhsInventoryView)unFinalisedReceive.Inventory.Single();
			var inventoryForFinalisedReceive = (WhsInventoryView)finalisedReceive.Inventory.Single();

			Applicator.SelectedInventoryHeldCode = InventoryStatus.Codes.Held;
			Applicator.HoldReason = "Whatever";
			ApplyApplicator(new[] { inventoryForUnFinalisedReceive },
@"WARNING: 1 Inventory was not updated with the new Hold Code.
This could be because the Receipt is not finalized, there is no available stock to be updated or new Hold Code is not valid for the client.".Trim());
			AssertEquals("Hold Reason should not be set.", "", inventoryForUnFinalisedReceive.InDocketLine.WE_CurrentHoldReason);

			ApplyApplicator(new[] { inventoryForFinalisedReceive },
@"INFO: 1 Inventory was updated with the new Hold Code.".Trim());
			AssertEquals("Hold Reason should be set.", "Whatever", inventoryForFinalisedReceive.InDocketLine.WE_CurrentHoldReason);
		}

		#endregion

		#region TestAction

		public void TestAction()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var unFinalisedReceiveLocation = data.Whs1.FindLocation("A-1");
			var finalisedReceiveLocation = data.Whs1.FindLocation("A-2");
			var committedToPickLocation = data.Whs1.FindLocation("A-3");
			var committedToTransferLocation = data.Whs1.FindLocation("A-4");

			var unFinalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, unFinalisedReceiveLocation, "", false, false);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m, finalisedReceiveLocation, "", false, true);
			var finalisedReceiveForPick = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m, committedToPickLocation, "", false, true);
			var finalisedReceiveForTransfer = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 6m, committedToTransferLocation, "", false, true);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == committedToPickLocation);
			availableInventory.Allocate = true;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, committedToTransferLocation, data.Whs1.DefaultLocation);
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var inventoryForUnFinalisedReceive = (WhsInventoryView)unFinalisedReceive.Inventory.Single();
			var inventoryForFinalisedReceive = (WhsInventoryView)finalisedReceive.Inventory.Single();
			var inventoryForCommittedToPick = (WhsInventoryView)finalisedReceiveForPick.Inventory.Single();
			var inventoryForCommittedToTransfer = (WhsInventoryView)finalisedReceiveForTransfer.Inventory.Single();

			Applicator.SelectedInventoryHeldCode = InventoryStatus.Codes.Held;
			ApplyApplicator(new[] { inventoryForUnFinalisedReceive },
@"WARNING: 1 Inventory was not updated with the new Hold Code.
This could be because the Receipt is not finalized, there is no available stock to be updated or new Hold Code is not valid for the client.".Trim());

			Applicator.SelectedInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			ApplyApplicator(new[] { inventoryForFinalisedReceive, inventoryForCommittedToPick, inventoryForCommittedToTransfer },
@"INFO: 3 Inventory was updated with the new Hold Code.".Trim());

			Applicator.SelectedInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			ApplyApplicator(new[] { inventoryForUnFinalisedReceive, inventoryForFinalisedReceive, inventoryForCommittedToPick, inventoryForCommittedToTransfer },
@"WARNING: 3 Inventory was not updated with the new Hold Code.
This could be because the Receipt is not finalized, there is no available stock to be updated or new Hold Code is not valid for the client.
INFO: 1 Inventory was updated with the new Hold Code.".Trim());

			Applicator.SelectedInventoryHeldCode = string.Empty;
			ApplyApplicator(new[] { inventoryForFinalisedReceive },
@"INFO: 1 Inventory was updated with the new Hold Code.".Trim());
		}

		public void TestAction_SelectedHoldCodeIsNotValidForSomeClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var finalisedReceiveLocation = data.Whs1.FindLocation("A-2");

			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var product1 = Helper.CreateProduct(client1, "Product1");
			var product2 = Helper.CreateProduct(client2, "Product2");
			var product3 = Helper.CreateProduct(client3, "Product3");
			Helper.CreateInventoryHeldCode("ABC", "ABC for client 1", client1.PK);
			Helper.CreateInventoryHeldCode("ABC2", "ABC for client 2", client2.PK);
			Helper.CreateInventoryHeldCode("DEF", "DEF for client 3", client3.PK);

			var receive1ForClient1 = Helper.CreateWhsReceiveWithInventory(client1, data.Whs1, "R1", product1, 3m, finalisedReceiveLocation, "", false, true);
			var receive1ForClient2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", product2, 3m, finalisedReceiveLocation, "", false, true);
			var receive1ForClient3 = Helper.CreateWhsReceiveWithInventory(client3, data.Whs1, "R3", product3, 3m, finalisedReceiveLocation, "", false, true);
			var receive2ForClient3 = Helper.CreateWhsReceiveWithInventory(client3, data.Whs1, "R4", product3, 3m, finalisedReceiveLocation, "", false, true);
			Factory.Save();

			var inventory1ForClient1 = (WhsInventoryView)receive1ForClient1.Inventory.Single();
			var inventory1ForClient2 = (WhsInventoryView)receive1ForClient2.Inventory.Single();
			var inventory1ForClient3 = (WhsInventoryView)receive1ForClient3.Inventory.Single();
			var inventory2ForClient3 = (WhsInventoryView)receive2ForClient3.Inventory.Single();

			Applicator.SelectedInventoryHeldCode = "ABC";
			ApplyApplicator(new[] { inventory1ForClient1, inventory1ForClient2, inventory1ForClient3, inventory2ForClient3 },
@"WARNING: 3 Inventory was not updated with the new Hold Code.
This could be because the Receipt is not finalized, there is no available stock to be updated or new Hold Code is not valid for the client.
INFO: 1 Inventory was updated with the new Hold Code.".Trim());
		}

		#endregion

		#region TestAction_DbHits

		public void TestAction_DbHits()
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

			var expectedDBHits = new Dictionary<string, int>();
			expectedDBHits.Add(OrgAddressSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsDocketSchema.Constants.TableName, 3);
			expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, 4);
			expectedDBHits.Add(WhsDocketLineSchema.Constants.TableName, 5);
			expectedDBHits.Add(WhsInventoryHoldChangeLogSchema.Constants.TableName, 4);
			expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			expectedDBHits.Add(ProcessTasksSchema.Constants.TableName, 4);
			expectedDBHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);
			expectedDBHits.Add(StmEventSchema.Constants.TableName, 1);
			expectedDBHits.Add(RefUNLOCOSchema.Constants.TableName, 1); // due to assigning event time based on warehouse branch time zone/offset

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				Applicator.SelectedInventoryHeldCode = string.Empty;
				ApplyApplicator(newFactory.Load<WhsInventoryView>(new ZQuery()), @"WARNING: 90 Inventory was not updated with the new Hold Code.
This could be because the Receipt is not finalized, there is no available stock to be updated or new Hold Code is not valid for the client.
INFO: 110 Inventory was updated with the new Hold Code.".Trim());

				var dbHitsOnStmALogTable = newFactory.TableSelects.Single(x => x.TableName == StmALogSchema.Constants.TableName).Value;
				AssertEquals(string.Format("DB Hits on table StmALog should be less than 8. (Actual: {0})", dbHitsOnStmALogTable), true, dbHitsOnStmALogTable <= 8);

				expectedDBHits.Add(StmALogSchema.Constants.TableName, dbHitsOnStmALogTable); // 1 hit per receive with available inventory when calling "FindActiveLog" to create/update Propagated event + 1 from the parent event.
			}
		}

		#endregion

		#region TestLookups

		public void TestLookups()
		{
			AssertEquals(typeof(UpdateInventoryHeldCodesApplicatorLookups), Applicator.Lookups.GetType());
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			AssertEquals(typeof(UpdateInventoryHeldCodeValidation), Applicator.Validation.GetType());
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		new UpdateInventoryHeldCodeActionMethodApplicator Applicator => (UpdateInventoryHeldCodeActionMethodApplicator)base.Applicator;
	}
}
