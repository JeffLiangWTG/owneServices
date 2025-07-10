using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;
using WhsPickLineDo = CargoWise.Database.TestFramework.ObjectModel.WhsPickLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class GeneratePickManagerTest : WhsTestCaseWithFactory
	{
		#region TestPickOrders_ConcurrencyTest

		public void TestPickOrders_ConcurrencyTest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();

			order.Factory.Saving += (f) =>
				AssertNoExceptionThrown(() => WhsDocketLineDO.DeleteInDB(Db.Connection, order.Lines[0].PK.ToGuid()));

			var logger = new DummyGeneratePickLogger();
			GeneratePickWithAllocateMock(order, logger);
			AssertMultilineASCIIEquals("Should have concurrency error.",
				$"Order: {order.WD_DocketID} was modified by another user while attempting to automatically generate a Pick. Create the Pick manually.",
				logger.LastMessage);
			AssertEquals(LogType.Error, logger.LastLogType);
		}

		#endregion

		#region TestPickOrders_CreatePickOnlyIfNotExists

		public void TestPickOrders_CreatePickOnlyIfNotExists()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();

			var logger = new DummyGeneratePickLogger();
			GeneratePickWithAllocateMock(order, logger);
			AssertEquals("Should create one pick.", 1, Factory.GetDatabaseCount(typeof(WhsPick)));

			AssertNoExceptionThrown("Should not have exception if run again.",
				() => GeneratePickManager.GeneratePick(order, logger));
			AssertEquals("Should not create new pick.", 1, Factory.GetDatabaseCount(typeof(WhsPick)));
			AssertEquals("Log should has error.", "Order already has a Pick.\r\n", logger.LastMessage);
			AssertEquals(LogType.Error, logger.LastLogType);
			AssertEquals(PickType.Codes.Order, order.Pick.WP_PickType);
		}

		#endregion

		#region TestPickOrders_NoStockWasAllocated

		public void TestPickOrders_NoStockWasAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			Factory.Save();

			var logger = new DummyGeneratePickLogger();
			GeneratePickWithAllocateMock(order1, logger);
			AssertEquals(string.Empty, logger.LastMessage);

			logger = new DummyGeneratePickLogger();
			GeneratePickWithAllocateMock(order2, logger);
			AssertEquals("Log should has error.", "No Stock was allocated.\r\n", logger.LastMessage);
			AssertEquals(LogType.Error, logger.LastLogType);
			AssertEquals("Should only have one pick for order 1", 1, Factory.GetDatabaseCount(typeof(WhsPick)));
		}

		#endregion

		#region TestPickOrders_PickAwaitingReplenishment

		public void TestPickOrders_PickAwaitingReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var logger = new DummyGeneratePickLogger();
			GeneratePickWithAllocateMock(order, logger);
			AssertEquals("Log should have warning.",
				$"Pick {order.Pick.WP_PickNo} has been created and is awaiting replenishment.\r\n", logger.LastMessage);
			AssertEquals(LogType.Warning, logger.LastLogType);
			AssertEquals("Should have a pick for the order", 1, Factory.GetDatabaseCount(typeof(WhsPick)));
			AssertEquals(PickType.Codes.Order, order.Pick.WP_PickType);
		}

		#endregion

		#region TestPickOrders_OnPickOrdersFailed

		public void TestPickOrders_OnPickOrdersFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();

			order.WD_DocketStatus = DocketStatus.Codes.Held;

			var logger = new DummyGeneratePickLogger();
			GeneratePickWithAllocateMock(order, logger);
			AssertEquals("This Order is Held. Only Entered (Saved) Orders can be attached to a Pick.\r\n",
				logger.LastMessage);
			AssertEquals(LogType.Error, logger.LastLogType);
			AssertEquals("Should not create pick.", 0, Factory.GetDatabaseCount(typeof(WhsPick)));
		}

		#endregion

		#region TestPickOrders_ManyOrderPickFromInventory

		[StressTest]
		public void TestPickOrders_ManyOrderPickFromInventory()
		{
			var numberOfOrders = 31000;
			var data = new TestDataSimpleEnvironment(Factory);
			var now = DateTime.Now.Date;
			Factory.Save();

			var receive =
				new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), "INW", "REC", "FIN", "R1")
				{
					WD_FinalisedDate = now
				}.InsertAndReturnObject(TestConnection);
			var receiveLine = new WhsDocketLineDO(receive, data.Part1.PK.ToGuid(), numberOfOrders + 1)
			{
				WE_F3_NKPackType = "PKG",
				WE_WL = data.Whs1.DefaultLocation.PK.ToGuid(),
				WE_DocketLineStatus = "FIN",
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_FinalisedDate = now,
				WE_AdjustmentArrivalDate = now,
				WE_StockOnHand = numberOfOrders + 1
			}.InsertAndReturnObject(TestConnection);

			var existingOrderCounts = new BusinessObjectFactory().GetDatabaseCount(typeof(WhsOrder));

			TestConnection.ExecuteNonQuery(@"
			alter table dbo.WhsDocket disable trigger all;
			alter table dbo.WhsDocketLine disable trigger all;
			alter table dbo.WhsPickLine disable trigger all;");

			var chunkSize = 200;
			for (var i = 0; i < numberOfOrders; i += chunkSize)
			{
				var sql = new SqlQueryBuilder();
				for (var j = i; j < i + chunkSize; j++)
				{
					var order = new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), "ORD", "ORD", "ENT",
						$"O{j}").AppendInsertAndReturnObject(sql);
					var orderLine =
						new WhsDocketLineDO(order, data.Part1.PK.ToGuid(), 1m).AppendInsertAndReturnObject(sql);
					new WhsPickLineDo(receiveLine, orderLine, 1m) { WZ_OriginalReservedQty = 1m }
						.AppendInsertAndReturnObject(sql);
				}

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			TestConnection.ExecuteNonQuery(@"
			alter table dbo.WhsDocket enable trigger all;
			alter table dbo.WhsDocketLine enable trigger all;
			alter table dbo.WhsPickLine enable trigger all;");

			AssertEquals("Precondition: Make sure data inserted as expected.", numberOfOrders + existingOrderCounts,
				new BusinessObjectFactory().GetDatabaseCount(typeof(WhsOrder)));
			var orderToTest =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OOO", data.Part1, numberOfOrders);
			Helper.CreateReservePickLine(orderToTest.Lines[0],
				Factory.Load<WhsReceiveLine>(receiveLine.PK).Inventory[0], 1m);
			Factory.Save();

			var logger = new DummyGeneratePickLogger();
			AssertNoExceptionThrown(() =>
				GeneratePickWithAllocateMock(new BusinessObjectFactory().Load<WhsOrder>(orderToTest.PK), logger));
		}

		#endregion

		#region TestGeneratePick_NoFactorySave

		public void TestGeneratePick_NoFactorySave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Factory.Save();

			var logger = new DummyGeneratePickLogger();
			GeneratePickWithAllocateMock(order, logger, saveFactory: false);
			AssertNull("Should have not saved the Pick in Factory.",
				new BusinessObjectFactory().Load<WhsOrder>(order.PK).Pick);

			Factory.Save();
			AssertNotNull("Should create Pick and attach order to it.",
				new BusinessObjectFactory().Load<WhsOrder>(order.PK).Pick);
		}

		#endregion

		#region GeneratePickWithAllocateMock

		static void GeneratePickWithAllocateMock(WhsOrder order, IGeneratePickLogger logger, bool saveFactory = true)
		{
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				GeneratePickManager.GeneratePick(order, logger, saveFactory);
			}
		}

		#endregion

		#region DummyGeneratePickLogger

		enum LogType
		{
			Nothing,
			Warning,
			Error
		}

		class DummyGeneratePickLogger : IGeneratePickLogger
		{
			readonly List<(string Message, LogType Type)> logs = new List<(string, LogType)>();
			const string newLine = "\r\n";

			public void LogWarning(string message)
			{
				logs.Add((message + newLine, LogType.Warning));
			}

			public void LogError(string message)
			{
				logs.Add((message + newLine, LogType.Error));
			}

			public void LogError(WhsPick pick, string message)
			{
				logs.Add((message + newLine, LogType.Error));
			}

			public void LogSaveConcurrencyException(WhsPick pick, string docketID)
			{
				logs.Add((
					$"Order: {docketID} was modified by another user while attempting to automatically generate a Pick. Create the Pick manually." +
					newLine, LogType.Error));
			}

			public string LastMessage => (logs.Count > 0) ? logs.Last().Message : string.Empty;
			public LogType LastLogType => (logs.Count > 0) ? logs.Last().Type : LogType.Nothing;
		}

		#endregion
	}
}
