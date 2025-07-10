using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class LocationOverReduceTriggerTest : TransactionedTestCase
	{
		#region TestTriggerPreventsOverreduceLocationCapacity

		public void TestTriggerPreventsOverreduceLocationCapacity()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m, true, finalise: false);
			var locationPK = data.Whs1.DefaultLocation.PK;
			factory.Save();

			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(locationPK, 10m)));
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(locationPK, 0m)));
			AssertExceptionThrown<SqlException>(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(locationPK, 9m)));
		}

		public void TestTriggerPreventsOverreduceLocationCapacity_InTransit()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_MaxQuantity = 20m;
			location2.WLV_MaxQuantity = 20m;

			factory.Save();

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location1, "");
			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 20m, location1.ToLocationString(), location2.ToLocationString());

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 20m, transferLine.QtyCommittedIncludingMatchingLines);
			factory.Save();

			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(location1.PK, 15m)));
			AssertExceptionThrown<SqlException>(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(location2.PK, 10m)));
		}

		public void TestTriggerPreventsOverreduceLocationCapacity_Staged()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			factory.Save();

			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(receive.Lines[0].Location.PK, 5m)));
			AssertExceptionThrown<SqlException>(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(data.Whs1.DefaultOutboundDockDoorLocation.PK, 5m)));
		}

		public void TestTriggerPreventsOverreduceLocationCapacity_ExcludePFUDocketLines()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receiveLine = inventory.InDocketLine;
			factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.DefaultLocation, "PLT-1", 10m);
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.PickedForUnload, receiveLine.WE_DocketLineStatus);
			factory.Save();

			// Picked for Unload receive lines should not take any location capacity.
			var ddlPK = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(ddlPK, 10m)));
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(ddlPK, 0m)));
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(ddlPK, 9m)));

			// Putaway transfer should be taking location capacity in destination location
			var locationPK = data.Whs1.DefaultLocation.PK;
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(locationPK, 10m)));
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(locationPK, 0m)));
			AssertExceptionThrown<SqlException>(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(locationPK, 9m)));
		}

		public void TestTriggerPreventsOverreduceLocationCapacity_ExcludesCancelledReceiveLines()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);

			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive1 = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			var receiveLine1 = helper.CreateWhsReceiveLine(receive1, data.Part1, 6m, dockdoorLocation);

			var receive2 = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2");
			var receiveLine2 = helper.CreateWhsReceiveLine(receive2, data.Part1, 4m, dockdoorLocation);

			var cancelledReceive = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R3");
			var cancelledReceiveLine = helper.CreateWhsReceiveLine(cancelledReceive, data.Part1, 5m, dockdoorLocation);
			cancelledReceive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_StockOnHand = 0;
			factory.Save();

			AssertEquals(DocketStatus.Codes.Cancelled, cancelledReceive.WD_DocketStatus);
			AssertEquals(DocketLineStatus.Codes.Cancelled, cancelledReceiveLine.WE_DocketLineStatus);

			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(dockdoorLocation.PK, 10m)));
			AssertExceptionThrown<SqlException>(() => Db.Connection.ExecuteNonQuery(GetRawSQLToUpdateLocationCapacity(dockdoorLocation.PK, 9m)));
		}

		#endregion

		#region GetRawSQLToUpdateLocationCapacity

		string GetRawSQLToUpdateLocationCapacity(ZGuid locationPK, decimal targetCapacity) => CargoWise.Database.TestFramework.ObjectModel.WhsLocation.UpdateWhere(locationPK.ToGuid()).Set(l => l.WL_MaxQuantity, targetCapacity).AsSQL();

		#endregion
	}
}
