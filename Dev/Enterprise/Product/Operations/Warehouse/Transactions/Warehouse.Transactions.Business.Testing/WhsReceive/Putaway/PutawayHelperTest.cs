using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PutawayHelperTest : WhsTestCaseWithFactory
	{
		public void TestFindInventoryQuery()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive1.WD_ExternalReference = "";
			var receiveLine11 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");
			var receiveLine12 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT-2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive2.WD_ExternalReference = "";
			var receiveLine21 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m);
			var receiveLine22 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m);
			receiveLine21.WE_PalletID = "PLT-3";
			receiveLine22.WE_PalletID = "PLT-4";

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive3.WD_ExternalReference = "";
			var receiveLine31 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-5");
			var receiveLine32 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT-6");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation.ToLocationString(), "PLT-5", location2.ToLocationString(), "PLT-5");
			var pickLine = Helper.CreateWhsPickLine(transferLine, receiveLine31.Inventory[0], 10m);

			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.CancelReactivateDocket();
			Factory.Save();

			var inventories = Factory.Load<WhsInventoryView>(PutawayHelper.FindInventoryQuery(new[] { "PLT-1", "PLT-2", "PLT-3", "PLT-4", "PLT-5", "PLT-6" }, data.Whs1.PK));
			AssertEquals(2, inventories.Length);
			AssertArrayEqualsByElements(new[] { "PLT-5", "PLT-6" }, inventories.Select(s => s.WI_PalletID.ToString()).OrderBy(s => s).ToArray());

			var lineType = inventories.Select(s => s.WI_InDocketLineType).Distinct().ToArray();
			AssertEquals(1, lineType.Length);
			AssertEquals(DocketType.Codes.Receive, lineType[0]);
		}

		public void TestGetPutawayTransferLineFromInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, dockDoorLocation1, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "PLT1", 15m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			var inventories = Factory.Load<WhsInventoryView>(new ZQuery());
			AssertEquals(2, inventories.Length);

			var inventoryViewReceiveLine = inventories.Single(s => s.WI_InDocketLineType == "INW");
			var inventoryViewTransferLine = inventories.Single(s => s.WI_InDocketLineType == "TFR");
			AssertEquals(transferLine.PK, PutawayHelper.GetPutawayTransferLineFromInventory(inventoryViewReceiveLine).PK);
			AssertEquals(transferLine.PK, PutawayHelper.GetPutawayTransferLineFromInventory(inventoryViewTransferLine).PK);
		}
	}
}
