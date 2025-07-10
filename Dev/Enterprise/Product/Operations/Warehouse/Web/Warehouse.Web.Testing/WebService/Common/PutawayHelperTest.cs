using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	public class PutawayHelperTest : TestCaseWithFactory
	{
		#region TestAddScannedReleaseCapturedSerialNumbers

		public void TestFindReceiveLineQuery()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_3");
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_4");

			var whs2 = helper.CreateWarehouse("WH2", "A", 2, 1);
			var receive2 = helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, whs2.DefaultInboundDockDoorLocation, "PLT_1");
			helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, whs2.DefaultInboundDockDoorLocation, "PLT_2");
			helper.Factory.Save();

			var loadedReceives = helper.Factory.Load<WhsReceiveLine>(PutawayHelper.FindReceiveLineQuery(new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4" }, data.Whs1.PK));
			AssertEquals("ReceiveLine count correct", 4, loadedReceives.Length);
			AssertEquals("ReceiveLines only from receive in WHS1", true, loadedReceives.All(r => r.WE_WD == receive1.PK));
			AssertContainsExactElementsInAnyOrder("ReceiveLine count correct", new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4" }, loadedReceives.Select(r => r.WE_PalletID));
		}

		public void TestFindReceiveLineQuery_WithCancelledPalletIDs()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PLT_1");
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PLT_2");
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PLT_3");
			helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PLT_4");

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var cancelledReceiveLine1 = helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, dockdoorLocation, "PLT_1");
			var cancelledReceiveLine2 = helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, dockdoorLocation, "PLT_2");
			var cancelledReceiveLine3 = helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, dockdoorLocation, "PLT_5");
			helper.Factory.Save();

			var loadedReceivesBefore = helper.Factory.Load<WhsReceiveLine>(PutawayHelper.FindReceiveLineQuery(new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4", "PLT_5" }, data.Whs1.PK));
			AssertEquals("ReceiveLine count correct", 7, loadedReceivesBefore.Length);
			AssertContainsExactElementsInAnyOrder("ReceiveLine count correct", new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4", "PLT_1", "PLT_2", "PLT_5" }, loadedReceivesBefore.Select(r => r.WE_PalletID));

			receive2.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			cancelledReceiveLine1.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			cancelledReceiveLine2.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			cancelledReceiveLine3.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			helper.Factory.Save();

			var loadedReceivesAfter = helper.Factory.Load<WhsReceiveLine>(PutawayHelper.FindReceiveLineQuery(new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4", "PLT_5" }, data.Whs1.PK));
			AssertEquals("ReceiveLine count correct", 4, loadedReceivesAfter.Length);
			AssertContainsExactElementsInAnyOrder("Should have filter out cancelled receive lines", new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4" }, loadedReceivesAfter.Select(r => r.WE_PalletID));
		}

		#endregion

		#region TestHasUnfinalisedPutawayTransfersForMultiplePallets

		public void TestHasUnfinalisedPutawayTransfersForMultiplePallets_SomeInventoryAlreadyFnalised()
		{
			var factory = new BusinessObjectFactory();
			var response = new WhsPalletWebServiceResponse();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			var receiveLine2 = helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");
			factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_1", 10m);
			transferLine.RunPreSaveValidation();

			var putawayTransfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_2", 10m);
			transferLine2.RunPreSaveValidation();

			factory.Save();
			receiveLine2.PutawayTransferLine.FinaliseDocketLine();
			AssertEquals(false, receiveLine1.PutawayTransferLine.IsFinalised);
			AssertEquals(true, receiveLine2.PutawayTransferLine.IsFinalised);
			var inventories = new WhsInventoryView[] { (WhsInventoryView)receiveLine1.Inventory.First(), (WhsInventoryView)receiveLine2.Inventory.First() };
			helper.Factory.Save();

			var transferLines = new[] { transferLine, transferLine2 };
			var result1 = PutawayHelper.AreAllPutawayTransfersUnfinalized(response, transferLines);

			AssertEquals(false, result1);
			AssertEquals(false, response.NoError());
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Putaway has been already completed for one or more pallet ids", response.ErrorMessage);

			response.Error = ErrorTypes.None;
			response.ErrorMessage = null;
			var result2 = PutawayHelper.AreAllPutawayTransfersUnfinalized(response, transferLines, "PLT_1");
			AssertEquals(false, result2);
			AssertEquals(false, response.NoError());
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Putaway has been already completed for the Pallet ID PLT_1.", response.ErrorMessage);

			response.Error = ErrorTypes.None;
			response.ErrorMessage = null;
			var result3 = PutawayHelper.AreAllPutawayTransfersUnfinalized(response, inventories, "PLT_1");
			AssertEquals(false, result3);
			AssertEquals(false, response.NoError());
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Putaway has been already completed for the Pallet ID PLT_1.", response.ErrorMessage);
		}

		public void TestHasUnfinalisedPutawayTransfersForMultiplePallets_AllInventoryUnFinalised()
		{
			var factory = new BusinessObjectFactory();
			var response = new WhsPalletWebServiceResponse();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			var receiveLine2 = helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");
			factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_1", 10m);
			transferLine.RunPreSaveValidation();

			var putawayTransfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_2", 10m);
			transferLine2.RunPreSaveValidation();

			factory.Save();
			AssertEquals(false, receiveLine1.PutawayTransferLine.IsFinalised);
			AssertEquals(false, receiveLine2.PutawayTransferLine.IsFinalised);
			var inventories = new WhsInventoryView[] { (WhsInventoryView)receiveLine1.Inventory.First(), (WhsInventoryView)receiveLine2.Inventory.First() };
			helper.Factory.Save();

			var transferLines = new[] { transferLine, transferLine2 };
			var result1 = PutawayHelper.AreAllPutawayTransfersUnfinalized(response, transferLines);

			AssertEquals(result1, true);
			AssertEquals(true, response.NoError());
			AssertEquals(null, response.ErrorMessage);

			response.Error = ErrorTypes.None;
			response.ErrorMessage = null;
			var result2 = PutawayHelper.AreAllPutawayTransfersUnfinalized(response, transferLines, "PLT_1");
			AssertEquals(result2, true);
			AssertEquals(true, response.NoError());
			AssertEquals(null, response.ErrorMessage);

			response.Error = ErrorTypes.None;
			response.ErrorMessage = null;
			var result3 = PutawayHelper.AreAllPutawayTransfersUnfinalized(response, inventories, "PLT_1");
			AssertEquals(result3, true);
			AssertEquals(true, response.NoError());
			AssertEquals(null, response.ErrorMessage);
		}

		#endregion

		#region Implementation

		TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		TestNotificationBuffer notify;

		#endregion
	}
}
