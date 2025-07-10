using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PutawayTransferTriggerTest : TestCase
	{
		#region TestPreventConcurrentUsersCreatingPutawayTransferLinesForTheSameInventory

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestPreventConcurrentUsersCreatingPutawayTransferLinesForTheSameInventory()
		{
			var factoryForFirstUser = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factoryForFirstUser);
			var data = new TestDataSimpleEnvironment(factoryForFirstUser, 2, 1);
			factoryForFirstUser.Save();

			var dockDoorLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var rec1InRec1ForPart1 = helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "A-1", 1m);
			var rec2InRec1ForPart1 = helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "A-1", 2m);
			factoryForFirstUser.Save();

			var transferForFirstUser = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForFirstUser.WD_IsPutawayTransfer = true;
			var transferLineForFirstUser = SetupTransferLineForDockDoorLocation(helper, transferForFirstUser, data.Part1, dockDoorLocation1, "A-1", 3m);
			transferForFirstUser.RunPreSaveValidation();

			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				var factoryForSecondUser = new BusinessObjectFactory(anotherConnection) { RefreshEnabled = false };
				var helperForSecondUser = new WhsTestHelperFunctions(factoryForSecondUser);
				var transferForSecondUser = helperForSecondUser.CreateWhsTransfer(data.Org1.PK, data.Whs1.PK);
				transferForSecondUser.WD_IsPutawayTransfer = true;
				var transferLineForSecondUser = SetupTransferLineForDockDoorLocation(helperForSecondUser, transferForSecondUser, data.Part1, dockDoorLocation1, "A-1", 3m);
				transferForSecondUser.RunPreSaveValidation();

				factoryForFirstUser.Save();

				((IDbConnected)factoryForSecondUser).Connection.BeginTransaction();
				NUnit.Framework.Assert.That(delegate
				{
					factoryForSecondUser.Save();
				}, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}"), "Save should be prevented.");
			}
		}

		WhsTransferLine SetupTransferLineForDockDoorLocation(WhsTestHelperFunctions helper, WhsTransfer transfer, OrgSupplierPart part, WhsLocation dockDoorLocation, string palletID, ZDecimal quantity)
		{
			var line = helper.CreateWhsTransferLine(transfer, part, quantity, dockDoorLocation.ToLocationString(), palletID);
			line.WE_TransferFromPalletId = palletID;
			line.WE_PalletID = palletID;
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			AssertEquals("Precondition - ensure no stock is committed.", 0m, line.GetQtyCommittedToThisLine());
			return line;
		}

		#endregion
	}
}
