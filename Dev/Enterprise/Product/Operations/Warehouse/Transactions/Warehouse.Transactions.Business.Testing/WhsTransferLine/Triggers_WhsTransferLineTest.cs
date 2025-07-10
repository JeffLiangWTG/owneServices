using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class Triggers_WhsTransferLineTest : WhsDocketLineTriggerTestCase
	{
		#region TestUpdatingLineWithLocationFromAnotherWarehouse_IWS

		[ExpectNoExceptions]
		public void TestUpdatingLineWithLocationFromAnotherWarehouse_IWS()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var whs3 = Helper.CreateWarehouse("W2", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", transferType: TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			transferLine.WE_WL = whs3.DefaultLocation.PK;
			AssertNoExceptionThrown("Destination Warehouse is not checked on an Inter Whs Source Parent, so it should be fine to change.", Factory.Save);

			transferLine.WE_WL_TransferFrom = whs3.DefaultLocation.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Locations should be in the warehouse specified on the parent Docket."), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
		}

		#endregion

		#region TestUpdatingLineWithLocationFromAnotherWarehouse_IWS_WithChild

		public void TestUpdatingLineWithLocationFromAnotherWarehouse_IWS_WithChild()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var whs3 = Helper.CreateWarehouse("W2", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", transferType: TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Today;
			Factory.Save();

			transferLine.WE_WL = whs3.DefaultLocation.PK;
			AssertExceptionThrown<ZSaveException>("InterWhs Sync trigger should prevent changing the destination warehouse. If this is no longer the case, we should consider extending this location/warehouse check trigger!", Factory.Save);
		}

		#endregion

		#region TestUpdatingLineWithLocationFromAnotherWarehouse_IWD

		[ExpectNoExceptions]
		public void TestUpdatingLineWithLocationFromAnotherWarehouse_IWD()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var whs3 = Helper.CreateWarehouse("W2", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", transferType: TransferType.Codes.InterWhsDest);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			transferLine.WE_WL_TransferFrom = whs3.DefaultLocation.PK;
			AssertNoExceptionThrown("Source Warehouse is not checked on an Inter Whs Destination Parent, so it should be fine to change.", Factory.Save);

			transferLine.WE_WL = whs3.DefaultLocation.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Locations should be in the warehouse specified on the parent Docket."), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
		}

		#endregion

		#region TestUpdatingLineWithLocationFromAnotherWarehouse_IWD_WithChild

		public void TestUpdatingLineWithLocationFromAnotherWarehouse_IWD_WithChild()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var whs3 = Helper.CreateWarehouse("W2", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", transferType: TransferType.Codes.InterWhsDest);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Today;
			AssertNotNull("Precondition: Created Child.", transferLine.ChildTransferLine);
			Factory.Save();

			transferLine.WE_WL_TransferFrom = whs3.DefaultLocation.PK;
			AssertExceptionThrown<ZConcurrencyCheckFailureException>(
				"InterWhs Sync trigger should prevent changing the destination warehouse. If this is no longer the case, we should consider extending this location/warehouse check trigger!",
				Factory.Save);
		}

		#endregion

		#region TestUpdatingLineWithLocationFromAnotherWarehouse_TransferFrom

		[ExpectNoExceptions]
		public void TestUpdatingLineWithLocationFromAnotherWarehouse_TransferFrom()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var otherWhs = Helper.CreateWarehouse("W2", "A", 1, 1);
			var line = GetNewDocketLineForNewNonFinalisedDocket(data);
			Factory.Save();

			line.WE_WL_TransferFrom = otherWhs.DefaultLocation.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Locations should be in the warehouse specified on the parent Docket."), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
		}

		#endregion

		#region TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrect

		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert()
		{
			TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrectCore((transferLine) =>
			{
				Helper.CreateWhsTransferLine(transferLine.Docket, transferLine.SupplierPart, 10m, transferLine.TransferFromLocation, transferLine.Location);
			});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Update()
		{
			TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrectCore((transferLine) =>
			{
				transferLine.WE_TransactionQuantity = 13m;
			});
		}

		void TestTG_WhsDocketLine_TransactionAndPickedQtyIsCorrectCore(Action<WhsTransferLine> setDataForTriggerToFail)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			// doggy insert / modify tranfser line to ensure trigger fails
			setDataForTriggerToFail(transferLine);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID}", true), "When trying to over-pick transfer line the trigger should fail.");
		}

		#endregion

		#region TestTG_PreventOverfillLocationWithUnitsCapacity

		#region TestTG_PreventOverfillLocationWithUnitsCapacity_InTransitInventory_SourceLocation

		public void TestTG_PreventOverfillLocationWithUnitsCapacity_InTransitInventory_SourceLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, sourceLocation, "");
			Factory.Save();

			// suspend trigger to pre-set bad data (over filled location) SOH = 20, WLV_MaxQuantity = 10
			using (WhsTestHelperFunctions.SuspendTrigger("TG_PreventOverReduceLocationUnitsCapacity", WhsLocationSchema.Constants.TableName))
			{
				sourceLocation.WLV_MaxQuantity = 10m;
				Factory.Save();
			}

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, sourceLocation, destLocation);
			transferLine1.RunPreSaveValidation(); // to commit inventory
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown("Picking from overfilled source location more than max allowed capacity is allowed (if it also fixes the over-fill, will fail if we try to pick say 7).", Factory.Save);
		}

		#endregion

		#region TestTG_PreventOverfillLocationWithUnitsCapacity_InTransitInventory_DestLocation

		[ExpectNoExceptions]
		public void TestTG_PreventOverfillLocationWithUnitsCapacity_InTransitInventory_DestLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");
			destLocation.WLV_MaxQuantity = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, sourceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 7m, sourceLocation, destLocation);
			transferLine1.RunPreSaveValidation(); // to commit inventory
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown("We can have less than max allowed in transit to a dest location.", Factory.Save);

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, sourceLocation, destLocation);
			transferLine2.RunPreSaveValidation(); // to commit inventory
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocketLine.PreventOverflowLocationQuantityTriggerID), "We cannot have in-transit more than destination location capacity.");
		}

		#endregion

		#region TestTG_PreventOverfillLocationWithUnitsCapacity_StagedInventory

		[ExpectNoExceptions]
		public void TestTG_PreventOverfillLocationWithUnitsCapacity_StagedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.DefaultOutboundDockDoorLocation.WLV_MaxQuantity = 15m;
			var differentClient = Helper.CreateClient("C2", "C2");
			Helper.CreateProductClientRelationShip(differentClient, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(differentClient, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(differentClient, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine1.WE_CurrentInventoryStatus);
			transferLine1.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine1.WE_CurrentInventoryStatus);

			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);
			transferLine2.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine2.WE_CurrentInventoryStatus);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocketLine.PreventOverflowLocationQuantityTriggerID), "We cannot have more inventory than the dock door location capacity.");
		}

		#endregion

		#endregion

		#region Implementation

		protected override WhsDocketLine CreateUnfinalisedDocketLineOnFinalisedDocket(WhsDocketLine finalisedLine)
		{
			var line = base.CreateUnfinalisedDocketLineOnFinalisedDocket(finalisedLine);
			line.WE_WL_TransferFrom = finalisedLine.WE_WL;
			return line;
		}

		protected override WhsDocketLine GetNewDocketLineForNewNonFinalisedDocket(TestDataSimpleEnvironment data)
		{
			var locationA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC", data.Part1, 20m, locationA2, "");

			var docket = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var docketLine = GetNewDocketLineForDocket(docket, data.Part1);
			docketLine.RunPreSaveValidation(); // to commit inventory

			return docketLine;
		}

		protected override WhsDocketLine GetNewDocketLineForNewFinalisedDocket(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, ZDecimal qty)
		{
			var transfer = Helper.CreateWhsTransfer(client, whs);
			var transferLine = GetNewDocketLineForDocket(transfer, product);
			transferLine.WE_TransactionQuantity = qty;
			transfer.FinaliseDocketWithoutUserConfirmation();

			return transferLine;
		}

		protected override WhsDocketLine GetNewDocketLineForDocket(WhsDocket docket, OrgSupplierPart product)
		{
			var whs = docket.Warehouse;
			return Helper.CreateWhsTransferLine((WhsTransfer)docket, product, 1m, whs.FindLocation("A-2"), whs.FindLocation("A-1"));
		}

		protected override IDbCommand GetUpdateStatusAndDateCommandCore(string status, ZDateTimeOffset? finaliseDate, Guid pk)
		{
			var updateStatusSql = @"
UPDATE dbo.WhsPickLine 
SET 
	WZ_GS_NKAssignedTo = 'E', 
	WZ_PickedDateTime = @Date,
	WZ_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WZ_SystemLastEditUser = '~BP'
WHERE 
	WZ_WE_TransactionLine = @PK
	AND WZ_PickedDateTime is null

UPDATE dbo.WhsDocketLine 
SET 
	WE_DocketLineStatus = @Status, 
	WE_FinalisedDate = @Date, 
	WE_PutawayTime = @Date, 
	WE_GS_NKPutawayBy = 'E',
	WE_StockOnHand = WE_TransactionQuantity,
	WE_AdjustmentArrivalDate = @Date,
	WE_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WE_SystemLastEditUser = '~BP'
WHERE 
	WE_PK = @PK

UPDATE dbo.WhsDocketLine
SET
	WE_StockOnHand = WE_StockOnHand - WZ_Units,
	WE_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WE_SystemLastEditUser = '~BP'
FROM
	dbo.WhsPickLine
WHERE
	WZ_WE_TransactionLine = @PK
	AND WE_PK = WZ_WE_InventoryLine
";

			var updateStatusCommand = TestConnection.Command(updateStatusSql);
			updateStatusCommand.AddParameterBasedOnDbColumn("@Status", status, WhsDocketLineSchema.WE_DocketLineStatus);
			updateStatusCommand.AddParameterBasedOnDbColumn("@Date", finaliseDate?.ToDateTimeOffset(), WhsDocketLineSchema.WE_FinalisedDate);
			updateStatusCommand.AddParameterBasedOnDbColumn("@PK", pk, WhsDocketLineSchema.PK);

			return updateStatusCommand;
		}

		protected override bool DoesCreateNewStock
		{
			get { return true; }
		}

		protected override bool CanBeCancelled => false;

		#endregion
	}
}
