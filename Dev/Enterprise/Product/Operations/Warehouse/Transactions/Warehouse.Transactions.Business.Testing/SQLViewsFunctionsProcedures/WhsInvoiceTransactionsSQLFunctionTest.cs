using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInvoiceTransactionsSQLFunctionTest : WhsTestCaseWithFactory
	{
		#region TestFunction

		public void TestFunction()
		{
			var client = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 5, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 5, 1);
			var part = Helper.CreateProduct(client, "P1");
			var locations1 = whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var locations2 = whs2.Rows.Single(r => r.WR_Name == "B").Locations;

			CreateWhsReceive(client, whs1, "R1", part, locations1[0], 300m, false);
			var receive1 = CreateWhsReceive(client, whs1, "R2", part, locations1[1], 310m, true);
			var receive2 = CreateWhsReceive(client, whs2, "R3", part, locations2[0], 320m, true);

			CreateWhsOrder(client, whs1, "OR1", part, 50m, false, false);
			CreateWhsOrder(client, whs1, "OR2", part, 45m, true, false);
			var order = CreateWhsOrder(client, whs1, "OR3", part, 40m, true, true);

			var adjustment1 = CreateWhsAdjustment(client, whs1, "AD1", part, 35m, locations1[1], false);
			var adjustment2 = CreateWhsAdjustment(client, whs1, "AD2", part, 30m, locations1[1], true);

			var transfer1 = CreateWhsTransfer(client, TransferType.Codes.InterWhsSource, whs1, "TR3", part,
				locations1[1], 15m, whs2, locations2[1], false);
			var transfer2 = CreateWhsTransfer(client, TransferType.Codes.InterWhsSource, whs1, "TR4", part,
				locations1[1], 10m, whs2, locations2[1], true); // 2 finalised jobs
			var transfer3 = CreateWhsTransfer(client, TransferType.Codes.InterWhsDest, whs1, "TR5", part, locations2[0],
				5m, whs2, locations1[3], false);
			var transfer4 = CreateWhsTransfer(client, TransferType.Codes.InterWhsDest, whs1, "TR6", part, locations2[0],
				1m, whs2, locations1[3], true); // 2 finalised jobs

			var transfer2_2 = GetRelatedInternalTransfer(transfer2);
			var transfer4_2 = GetRelatedInternalTransfer(transfer4);

			var functionLoadResults = LoadSQLFunction();

			AssertEquals(8, functionLoadResults.Count);
			AssertCorrectData(adjustment2, functionLoadResults[0], receive1);
			AssertCorrectData(receive1, functionLoadResults[1], receive1);
			AssertCorrectData(receive2, functionLoadResults[2], receive1);
			AssertCorrectData(order, functionLoadResults[3], receive1);
			AssertCorrectData(transfer2, functionLoadResults[4], receive1);
			AssertCorrectData(transfer4_2, functionLoadResults[5], receive1);
			AssertCorrectData(transfer4, functionLoadResults[6], receive1);
			AssertCorrectData(transfer2_2, functionLoadResults[7], receive1);
		}

		void AssertCorrectData(WhsDocket docket, DynamicBusinessObject result, WhsReceive receive)
		{
			var docketLine = docket.Lines[0];

			AssertEquals(docket.WD_OH_Client, result["ClientPK"]);
			AssertEquals(docket.WD_WW_Whs, result["WarehousePK"]);
			AssertLocationPK(docket.WD_DocketType,
				docket.WD_DocketType == DocketType.Codes.Transfer &&
				docket.WD_DocketSubType == TransferType.Codes.InterWhsSource
					? docketLine.WE_WL_TransferFrom
					: docketLine.WE_WL, (ZGuid)result["LocationPK"], receive);
			AssertUnits(docket.WD_DocketType, docket.WD_DocketSubType, docketLine.WE_TransactionQuantity,
				(ZDecimal)result["Units"]);
			AssertEquals(docketLine.WE_OP, result["ProductPK"]);
			AssertEquals(docketLine.WE_PartAttrib1, result["PartAttrib1"]);
			AssertEquals(docketLine.WE_PartAttrib2, result["PartAttrib2"]);
			AssertEquals(docketLine.WE_PartAttrib3, result["PartAttrib3"]);
			AssertEquals(docketLine.WE_SerialNumber, result["SerialNumber"]);
		}

		void AssertLocationPK(ZString docketType, ZGuid expectedLocationPK, ZGuid actualLocationPK, WhsReceive receive)
		{
			if (docketType == DocketType.Codes.Order)
			{
				AssertEquals(receive.Lines[0].WE_WL, actualLocationPK);
			}
			else
			{
				AssertEquals(expectedLocationPK, actualLocationPK);
			}
		}

		void AssertUnits(ZString docketType, ZString docketSubType, ZDecimal expectedUnits, ZDecimal actualUnits)
		{
			if (docketType == DocketType.Codes.Order || (docketType == DocketType.Codes.Transfer &&
														 docketSubType == TransferType.Codes.InterWhsSource))
			{
				AssertEquals(-expectedUnits, actualUnits);
			}
			else
			{
				AssertEquals(expectedUnits, actualUnits);
			}
		}

		#endregion

		#region TestFunction_InnerTransfers

		public void TestFunction_InnerTransfers()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, locationA1, "");

			Factory.Save();

			var transferFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transferFinalised, data.Part1, 10m, "A-1", "A-2");
			transferFinalised.Lines[0].PickedTime = new ZDateTimeOffset(year, 1, 9);
			transferFinalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferFinalised);

			var transferPartiallyFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLineFinalised =
				Helper.CreateWhsTransferLine(transferPartiallyFinalised, data.Part1, 15m, "A-1", "A-2");
			var transferLineNotFinalised =
				Helper.CreateWhsTransferLine(transferPartiallyFinalised, data.Part1, 20m, "A-1", "A-2");
			transferLineNotFinalised.RunPreSaveValidation(); // to commit inventory
			transferLineFinalised.PickedTime = new ZDateTimeOffset(year, 1, 6);
			transferLineFinalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineFinalised);

			var transferNotFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			Helper.CreateWhsTransferLine(transferNotFinalised, data.Part1, 25m, "A-1", "A-2");
			transferNotFinalised.RunPreSaveValidation(); // to commit inventory

			// hack to setup Finalised dates.
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			transferFinalised.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 15);
			transferFinalised.Lines[0].WE_FinalisedDate = new ZDateTimeOffset(year, 1, 9);
			transferLineFinalised.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 6);
			Factory.Save();

			var results = LoadSQLFunction();
			AssertEquals(
				"System should find 1 transaction for Receive and 2 transactions for finalised Inner Transfer.", 5,
				results.Count);
			AssertContainsData(receive, receive.Lines[0], locationA1, 100m, results);
			AssertContainsData(transferFinalised, transferFinalised.Lines[0], locationA1, -10m, results);
			AssertContainsData(transferFinalised, transferFinalised.Lines[0], locationA2, 10m, results);
			AssertContainsData(transferPartiallyFinalised, transferLineFinalised, locationA1, -15m, results);
			AssertContainsData(transferPartiallyFinalised, transferLineFinalised, locationA2, 15m, results);
		}

		void AssertContainsData(WhsDocket expectedDocket, WhsDocketLine expectedOriginalDocketLine,
			WhsLocation expectedLocation, ZDecimal expectedQuantity, DynamicBusinessObjectCollection results,
			string expectedPalletID = "", string expectedSerialNumber = "")
		{
			var actualLine = results.Cast<DynamicBusinessObject>().Single(l =>
				(ZGuid)l["LocationPK"] == expectedLocation.PK && (ZDecimal)l["Units"] == expectedQuantity &&
				(ZString)l["SerialNumber"] == expectedSerialNumber);
			AssertEquals("ClientPK", expectedDocket.WD_OH_Client, actualLine["ClientPK"]);
			AssertEquals("WarehousePK", expectedDocket.WD_WW_Whs, actualLine["WarehousePK"]);
			AssertEquals("DocketType", expectedDocket.WD_DocketType, actualLine["DocketType"]);
			AssertEquals("ProductPK", expectedOriginalDocketLine.WE_OP, actualLine["ProductPK"]);
			AssertEquals("PartAttrib1", expectedOriginalDocketLine.WE_PartAttrib1, actualLine["PartAttrib1"]);
			AssertEquals("PartAttrib2", expectedOriginalDocketLine.WE_PartAttrib2, actualLine["PartAttrib2"]);
			AssertEquals("PartAttrib3", expectedOriginalDocketLine.WE_PartAttrib3, actualLine["PartAttrib3"]);
			AssertEquals("SerialNumber", expectedOriginalDocketLine.WE_SerialNumber, actualLine["SerialNumber"]);
			AssertEquals("Units", expectedQuantity, actualLine["Units"]);
			AssertEquals("LocationPK", expectedLocation.PK, actualLine["LocationPK"]);
			AssertEquals("PalletID", expectedPalletID, actualLine["PalletID"]);
			AssertEquals("SerialNumber", expectedSerialNumber, actualLine["SerialNumber"]);
		}

		#endregion

		#region TestFunction_PickedButUnfinalisedInnerTransfers

		public void TestFunction_PickedButUnfinalisedInnerTransfers()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, locationA1, "");
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, locationA1, "");
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 110m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // commit transfer line
			AssertEquals("Precondition - committed 110 units to transfer.", 110m,
				transferLine.QtyCommittedIncludingMatchingLines);
			transferLine.PickedTime = new ZDateTimeOffset(year, 1, 2);

			// HACK: Unpick the matching line to make sure we use the pick line's values
			transferLine.MatchingLines[0].PickedTime = ZDateTimeOffset.Empty;
			Factory.Save();

			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for Receive and 1 transaction for Inner Transfer.", 3,
				results1.Count);
			AssertContainsData(receive1, receive1.Lines[0], locationA1, 100m, results1);
			AssertContainsData(receive2, receive2.Lines[0], locationA1, 50m, results1);
			// Should not contain the balancing positive WE_TransactionQuantity line until the transfer is finalised
			AssertContainsData(transfer, transferLine, locationA1, -50m,
				results1); // Empty finalised date means it is In-Transit

			// HACK: Pick the matching line to make sure we use the pick line's values
			transferLine.MatchingLines[0].PickLines[0].WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 3);
			Factory.Save();

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for Receive and 2 transactions for Inner Transfer.", 4,
				results2.Count);
			AssertContainsData(receive1, receive1.Lines[0], locationA1, 100m, results2);
			AssertContainsData(receive2, receive2.Lines[0], locationA1, 50m, results2);
			// Should not contain the balancing positive WE_TransactionQuantity line until the transfer is finalised
			AssertContainsData(transfer, transferLine, locationA1, -50m,
				results2); // Empty finalised date means it is In-Transit
			AssertContainsData(transfer, transferLine.MatchingLines[0], locationA1, -60m,
				results2); // Empty finalised date means it is In-Transit

			// Finalise the transfer, ensure the balancing positive lines are included
			transfer.FinaliseDocket();
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
			transferLine.MatchingLines[0].WE_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			var results3 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for Receive and 4 transactions for Inner Transfer.", 6,
				results3.Count);
			AssertContainsData(receive1, receive1.Lines[0], locationA1, 100m, results3);
			AssertContainsData(receive2, receive2.Lines[0], locationA1, 50m, results3);

			AssertContainsData(transfer, transferLine, locationA1, -50m,
				results3); // Until we have In-Transit location we need to consider still is in source location if is not finalised
			AssertContainsData(transfer, transferLine, locationA2, 50m, results3);

			AssertContainsData(transfer, transferLine.MatchingLines[0], locationA1, -60m,
				results3); // Until we have In-Transit location we need to consider still is in source location if is not finalised
			AssertContainsData(transfer, transferLine.MatchingLines[0], locationA2, 60m, results3);
		}

		#endregion

		#region TestFunction_PickedButUnfinalisedInterWhsTransfers

		public void TestFunction_PickedButUnfinalisedInterWhsTransfers()
		{
			var year = ZDateTime.Now.Year;
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var receive =
				Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", part, 100m, whs1.FindLocation("A-1"), "");
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var transferInterWhsSource =
				Helper.CreateWhsTransfer(client, whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var transferInterWhsSourceLine =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 15m, "A-1", whs2.PK, "B");
			transferInterWhsSource.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 15m,
				transferInterWhsSourceLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsSourceLine.PickedTime = new ZDateTimeOffset(year, 1, 2);

			var transferInterWhsDest =
				Helper.CreateWhsTransfer(client, whs2, "TR3", Notify, TransferType.Codes.InterWhsDest);
			var transferInterWhsDestLine =
				Helper.CreateWhsTransferLine(transferInterWhsDest, part, 25m, "A-1", whs1.PK, "B");
			transferInterWhsDest.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 25m,
				transferInterWhsDestLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsDestLine.PickedTime = new ZDateTimeOffset(year, 1, 2);

			Factory.Save();

			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("ResultSet Count.", 3, results1.Count);
			AssertContainsData(receive, receive.Lines[0], whs1.FindLocation("A-1"), 100m, results1);
			// Should not contain the balancing positive WE_TransactionQuantity line until the transfer is finalised
			AssertContainsData(transferInterWhsSource, transferInterWhsSource.Lines[0], whs1.FindLocation("A-1"), -15m,
				results1); // Empty finalised date means it is In-Transit

			// even though we're testing destination transfer, this line should have the original warehouse
			AssertContainsData(transferInterWhsSource, transferInterWhsDest.Lines[0], whs1.FindLocation("A-1"), -25m,
				results1); // Empty finalised date means it is In-Transit

			// Finalise docket lines
			transferInterWhsSourceLine.FinaliseDocketLine();
			transferInterWhsDestLine.FinaliseDocketLine();

			transferInterWhsSourceLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 3);
			transferInterWhsSourceLine.ChildTransferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 3);
			transferInterWhsDestLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 3);
			transferInterWhsDestLine.ChildTransferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 3);
			Factory.Save();
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine);
			AssertIsFinalisedPrecondition(transferInterWhsDestLine);

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 1 transactions for Receive and 4 transactions for InterWhs Transfers.", 5,
				results2.Count);
			AssertContainsData(receive, receive.Lines[0], whs1.FindLocation("A-1"), 100m, results2);
			AssertContainsData(transferInterWhsSource, transferInterWhsSource.Lines[0], whs1.FindLocation("A-1"), -15m,
				results2); // Until we have In-Transit location we need to consider still is in source location if is not finalised
			AssertContainsData(transferInterWhsSource, transferInterWhsDest.Lines[0].ChildTransferLine,
				whs1.FindLocation("A-1"), -25m,
				results2); // Testing transferInterWhsDest, but pass in transferInterWhsSource as it will have same Whs as we need to assert
			AssertContainsData(transferInterWhsSource.ChildTransfers.ElementAt(0),
				transferInterWhsSource.Lines[0].ChildTransferLine, whs2.FindLocation("B"), 15m, results2);
			AssertContainsData(transferInterWhsDest, transferInterWhsDest.Lines[0], whs2.FindLocation("B"), 25m,
				results2);
		}

		#endregion

		#region TestFunction_PickedButNotFinalisedOrder

		public void TestFunction_PickedButNotFinalisedOrder()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 40m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 60m);
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines();
			pickLines.Single(pl => pl.WZ_Units == 60m).WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);
			AssertEquals("Precondition: 60 picked units.", 60m,
				pickLines.Where(pl => pl.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 40 units committed but unpicked.", 40m,
				pickLines.Where(pl => !pl.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units));
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for the Receive and 1 transaction for the picked order.", 3,
				results1.Count);
			AssertContainsData(receive1, receive1.Lines[0], location, 40m, results1);
			AssertContainsData(receive2, receive2.Lines[0], location, 60m, results1);
			AssertContainsData(order, receive2.Lines[0], location, -60m,
				results1); // Empty finalised date means it is In-Transit

			pickLines.Single(pl => pl.WZ_Units == 40m).WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 3);
			AssertEquals("Precondition: 100 picked units.", 100m,
				pickLines.Where(pl => pl.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 0 units committed but unpicked.", 0m,
				pickLines.Where(pl => !pl.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units));
			Factory.Save();

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for the Receive and 2 transactions for the picked order.",
				4, results2.Count);
			AssertContainsData(receive1, receive1.Lines[0], location, 40m, results2);
			AssertContainsData(receive2, receive2.Lines[0], location, 60m, results2);
			AssertContainsData(order, receive2.Lines[0], location, -60m,
				results2); // Empty finalised date means it is In-Transit
			AssertContainsData(order, receive1.Lines[0], location, -40m,
				results2); // Empty finalised date means it is In-Transit

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 15);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results3 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for the Receive and 2 transactions for the picked order.",
				4, results3.Count);
			AssertContainsData(receive1, receive1.Lines[0], location, 40m, results3);
			AssertContainsData(receive2, receive2.Lines[0], location, 60m, results3);
			AssertContainsData(order, receive2.Lines[0], location, -60m, results3);
			AssertContainsData(order, receive1.Lines[0], location, -40m, results3);
		}

		#endregion

		#region TestFunction_PickedButNotFinalisedWorkOrder

		public void TestFunction_PickedButNotFinalisedWorkOrder()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m);
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bomBike, 10m);
			var workOrderPick = Helper.CreatePickNew(workOrder);

			foreach (var pickLine in workOrderLine.ChildComponentLines.SelectMany(cl => cl.PickLines))
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);
				AssertEquals("Precondition: Work Order Component line is Picked.", true, pickLine.IsPicked);
			}

			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals(
				"System should find 2 transactions for the Receive and 2 transactions for the picked work order.", 4,
				results1.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), location, 10m,
				results1);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), location, 20m,
				results1);
			AssertContainsData(workOrder, frameInventory, location, -10m,
				results1); // Empty finalised date means it is In-Transit
			AssertContainsData(workOrder, wheelInventory, location, -20m,
				results1); // Empty finalised date means it is In-Transit

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 3);
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrderPick);
			Factory.Save();

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals(
				"System should find 2 transactions for the Receive and 2 transactions for the picked work order.", 4,
				results2.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), location, 10m,
				results2);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), location, 20m,
				results2);
			AssertContainsData(workOrder, frameInventory, location, -10m, results2);
			AssertContainsData(workOrder, wheelInventory, location, -20m, results2);
		}

		#endregion

		#region TestFunction_PickedOrder

		public void TestFunction_PickedOrder()
		{
			TestFunction_PickedOrder_Core(testOriginalDocketLineFK: false);
		}

		public void TestFunction_PickedOrder_WithOriginalDocketLineFK()
		{
			TestFunction_PickedOrder_Core(testOriginalDocketLineFK: true);
		}

		public void TestFunction_PickedOrder_InTransit()
		{
			TestFunction_PickedOrder_UsingInTransitTransfer_Core(testOriginalDocketLineFK: false);
		}

		public void TestFunction_PickedOrder_InTransit_WithOriginalDocketLineFK()
		{
			TestFunction_PickedOrder_UsingInTransitTransfer_Core(testOriginalDocketLineFK: true);
		}

		void TestFunction_PickedOrder_Core(bool testOriginalDocketLineFK)
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			if (testOriginalDocketLineFK)
			{
				PopulateOriginalDocketLineFK(receive.Lines[0]);
			}

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			// Pick the line
			var pickLine = pick.GetAllPickLines().Single();

			pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
				AssertEquals("Precondition: Picked Directly.", ZGuid.Empty,
					order.Lines[0].PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			}

			pick.FinaliseAllOrders();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0);
			AssertContainsData(receive, receive.Lines[0], location, 20m, results);
			AssertContainsData(order, receive.Lines[0], location, -10m, results);
		}

		void TestFunction_PickedOrder_UsingInTransitTransfer_Core(bool testOriginalDocketLineFK)
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			if (testOriginalDocketLineFK)
			{
				PopulateOriginalDocketLineFK(receive.Lines[0]);
			}

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			// Pick the line / create In-Transit Transfer
			var pickLine = pick.GetAllPickLines().Single();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
			Factory.Save();
			var transfer = transferLine.Docket;

			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertContainsData(receive, receive.Lines[0], location, 20m, results1);
			AssertContainsData(order, receive.Lines[0], location, -10m,
				results1); // For picked transfer to DDL, empty finalised date as it is in transit

			// Finalise transfer, pick/finalise the dock door stock
			transferLine.FinaliseDocketLine();
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 4);

			pick.FinaliseAllOrders();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertContainsData(receive, receive.Lines[0], location, 20m, results2);
			AssertContainsData(order, receive.Lines[0], location, -10m, results2);
		}

		void PopulateOriginalDocketLineFK(WhsReceiveLine receiveLine)
		{
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.HeldCodeChangeQuantity = 15m;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var holdCodeChangedLine =
				Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 15m));
			holdCodeChangedLine.HeldCodeToChangeTo = "";
			holdCodeChangedLine.ChangeInventoryHeldCode(true);
			Factory.Save();
			AssertEquals("Precondition.", InventoryStatus.Codes.Available,
				holdCodeChangedLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition.", receiveLine.PK, holdCodeChangedLine.WE_WE_OriginalDocketLineForRating);

			// Damage other stock so we pick the hold code changed line
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();
		}

		#endregion

		#region TestFunction_PickedWorkOrder

		public void TestFunction_PickedWorkOrder()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m);
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, bomBike, 8m);
			var workOrderPick = Helper.CreatePickNew(workOrder);

			// Pick the line
			foreach (var pickLine in workOrder.Lines[0].ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);
			}

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
				AssertEquals("Precondition: No dock door transfers created.", 0,
					Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer,
						SQLComparisonOperator.NotEqual, ZGuid.Empty)).Length);
			}

			var location = data.Whs1.DefaultLocation;
			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for the Receive.", 2, results1.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), location, 10m,
				results1);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), location, 20m,
				results1);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrderPick);
			Factory.Save();

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals(
				"System should find 2 transactions for the Receive and 2 transactions for the picked work order.", 4,
				results2.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), location, 10m,
				results2);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), location, 20m,
				results2);
			AssertContainsData(workOrder, frameInventory, location, -8m, results2);
			AssertContainsData(workOrder, wheelInventory, location, -16m, results2);
		}

		public void TestFunction_PickedWorkOrder_WithInTransitTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m);
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, bomBike, 8m);
			var workOrderPick = Helper.CreatePickNew(workOrder);

			// Pick the line / create In-Transit Transfer
			foreach (var pickLine in workOrder.Lines[0].ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
			}

			Factory.Save();
			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer,
				SQLComparisonOperator.NotEqual, ZGuid.Empty)).Single();

			var location = data.Whs1.DefaultLocation;
			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals(
				"System should find 2 transactions for the Receive and 2 transactions for the picked work order.", 4,
				results1.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), location, 10m,
				results1);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), location, 20m,
				results1);
			AssertContainsData(workOrder, frameInventory, location, -8m,
				results1); // Empty finalised date means it is In-Transit
			AssertContainsData(workOrder, wheelInventory, location, -16m,
				results1); // Empty finalised date means it is In-Transit

			// Finalise transfer, pick/finalise the dock door stock
			transfer.FinaliseDocket();

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrderPick);
			Factory.Save();

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals(
				"System should find 2 transactions for the Receive and 2 transactions for the picked work order.", 4,
				results2.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), location, 10m,
				results2);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), location, 20m,
				results2);
			AssertContainsData(workOrder, frameInventory, location, -8m, results2);
			AssertContainsData(workOrder, wheelInventory, location, -16m, results2);
		}

		#endregion

		#region TestFunction_DynamicWorkOrder

		public void TestFunction_DynamicWorkOrder()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m, inwardProcessingLocation);
			frameInventory.CustomsData.WB_EntryKey = "ENT - 1";
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m, inwardProcessingLocation);
			wheelInventory.CustomsData.WB_EntryKey = "ENT - 1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
			dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			var dynamicWorkOrderLine1 = Factory.New<WhsDynamicWorkOrderLine>();
			var dynamicWorkOrderLine2 = Factory.New<WhsDynamicWorkOrderLine>();
			var dynamicWorkOrderLine3 = Factory.New<WhsDynamicWorkOrderLine>();

			dynamicWorkOrderLine1.WE_WD = dynamicWorkOrder.PK;
			dynamicWorkOrderLine1.WE_OP = bike.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 5m;
			dynamicWorkOrderLine1.WE_F3_NKPackType = bike.OP_StockKeepingUnit;
			dynamicWorkOrderLine1.CustomsData.WB_IsMainInwardsProcessedItem = true;

			dynamicWorkOrderLine2.WE_WD = dynamicWorkOrder.PK;
			dynamicWorkOrderLine2.WE_OP = bikeFrame.PK;
			dynamicWorkOrderLine2.WE_TransactionQuantity = 5m;
			dynamicWorkOrderLine2.WE_F3_NKPackType = bikeFrame.OP_StockKeepingUnit;
			dynamicWorkOrderLine2.WE_WE_ParentDocketLine = dynamicWorkOrderLine1.PK;

			dynamicWorkOrderLine3.WE_WD = dynamicWorkOrder.PK;
			dynamicWorkOrderLine3.WE_OP = bikeWheel.PK;
			dynamicWorkOrderLine3.WE_TransactionQuantity = 10m;
			dynamicWorkOrderLine3.WE_F3_NKPackType = bikeWheel.OP_StockKeepingUnit;
			dynamicWorkOrderLine3.WE_WE_ParentDocketLine = dynamicWorkOrderLine1.PK;

			var pick = Helper.CreatePickNew(dynamicWorkOrder);

			// Pick the line
			foreach (var pickLine in dynamicWorkOrderLine1.ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);
			}

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
				AssertEquals("Precondition: No dock door transfers created.", 0,
					Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer,
						SQLComparisonOperator.NotEqual, ZGuid.Empty)).Length);
			}

			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for the Receive.", 2, results1.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), inwardProcessingLocation, 10m,
				results1);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), inwardProcessingLocation, 20m,
				results1);

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(dynamicWorkOrder);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results2 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals(
				"System should find 2 transactions for the Receive, 2 transactions for the picked dynamic work order and 1 for the created receive.", 5,
				results2.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), inwardProcessingLocation, 10m,
				results2);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), inwardProcessingLocation, 20m,
				results2);
			AssertContainsData(dynamicWorkOrder.Receive, dynamicWorkOrder.Receive.Lines[0], inwardProcessingLocation, 5m, results2);
			AssertContainsData(dynamicWorkOrder, frameInventory, inwardProcessingLocation, -5m, results2);
			AssertContainsData(dynamicWorkOrder, wheelInventory, inwardProcessingLocation, -10m, results2);
		}

		public void TestFunction_DynamicWorkOrder_InTransit()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m, inwardProcessingLocation);
			frameInventory.CustomsData.WB_EntryKey = "ENT - 1";
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m, inwardProcessingLocation);
			wheelInventory.CustomsData.WB_EntryKey = "ENT - 1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
			dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			var dynamicWorkOrderLine1 = Factory.New<WhsDynamicWorkOrderLine>();
			var dynamicWorkOrderLine2 = Factory.New<WhsDynamicWorkOrderLine>();
			var dynamicWorkOrderLine3 = Factory.New<WhsDynamicWorkOrderLine>();

			dynamicWorkOrderLine1.WE_WD = dynamicWorkOrder.PK;
			dynamicWorkOrderLine1.WE_OP = bike.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 5m;
			dynamicWorkOrderLine1.WE_F3_NKPackType = bike.OP_StockKeepingUnit;
			dynamicWorkOrderLine1.CustomsData.WB_IsMainInwardsProcessedItem = true;

			dynamicWorkOrderLine2.WE_WD = dynamicWorkOrder.PK;
			dynamicWorkOrderLine2.WE_OP = bikeFrame.PK;
			dynamicWorkOrderLine2.WE_TransactionQuantity = 5m;
			dynamicWorkOrderLine2.WE_F3_NKPackType = bikeFrame.OP_StockKeepingUnit;
			dynamicWorkOrderLine2.WE_WE_ParentDocketLine = dynamicWorkOrderLine1.PK;

			dynamicWorkOrderLine3.WE_WD = dynamicWorkOrder.PK;
			dynamicWorkOrderLine3.WE_OP = bikeWheel.PK;
			dynamicWorkOrderLine3.WE_TransactionQuantity = 10m;
			dynamicWorkOrderLine3.WE_F3_NKPackType = bikeWheel.OP_StockKeepingUnit;
			dynamicWorkOrderLine3.WE_WE_ParentDocketLine = dynamicWorkOrderLine1.PK;

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			var pickLines = pick.GetAllPickLines();

			pickLines.Single(pl => pl.WZ_Units == 5m).WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);
			AssertEquals("Precondition: 5 picked units.", 5m, pickLines.Where(pl => pl.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 10 units unpicked.", 10m, pickLines.Where(pl => !pl.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units));

			Factory.Save();
			AssertEquals("Precondition: 1 dock door transfers created.", 1,
					Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer,
						SQLComparisonOperator.NotEqual, ZGuid.Empty)).Length);

			var results1 = LoadSQLFunction(freeStoreDays: 0);
			AssertEquals("System should find 2 transactions for the Receive and 1 transactions for the picked dynamic work order.", 3, results1.Count);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 10m), inwardProcessingLocation, 10m, results1);
			AssertContainsData(receive, receive.Lines.Single(l => l.WE_TransactionQuantity == 20m), inwardProcessingLocation, 20m, results1);
			AssertContainsData(dynamicWorkOrder, frameInventory, inwardProcessingLocation, -5m, results1);
		}

		#endregion

		#region TestFunction_PutawayTransfersAreIncluded_ReceivedReceiveLineAreIgnored

		public void TestFunction_PutawayTransfersAreIncluded_ReceivedReceiveLineAreIgnored()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs = data.Whs1;
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.DefaultLocation;
			var client = data.Org1;
			var product = data.Part1;
			Factory.Save();

			var receivedReceive = Helper.CreateWhsReceive(client, whs, "R1");
			var receivedReceiveLine =
				Helper.CreateWhsReceiveLine(receivedReceive, product, 30m, dockDoorLocation, "PLT-1");
			receivedReceiveLine.WE_AdjustmentArrivalDate = today;

			var finalisedPutawayReceive = Helper.CreateWhsReceive(client, whs, "R2");
			var finalisedPutawayReceiveLine =
				Helper.CreateWhsReceiveLine(finalisedPutawayReceive, product, 17m, dockDoorLocation, "PLT-2");
			finalisedPutawayReceiveLine.WE_AdjustmentArrivalDate = today;
			Factory.Save();

			var putawayTransferForFinRec = Helper.CreateWhsTransfer(client, whs, "T1");
			putawayTransferForFinRec.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(putawayTransferForFinRec,
				product, dockDoorLocation, normalLocation, "PLT-2", 17m);
			putawayTransferForFinRec.RunPreSaveValidation();

			putawayTransferForFinRec.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("putawayTransferForFinRec is finalised.", true, putawayTransferForFinRec.IsFinalised);
			finalisedPutawayReceive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("finalisedPutawayReceive is finalised.", true, finalisedPutawayReceive.IsFinalised);
			Factory.Save();

			var unfinalisedPutawayReceive = Helper.CreateWhsReceive(client, whs, "R3");
			var unfinalisedPutawayReceiveLine =
				Helper.CreateWhsReceiveLine(unfinalisedPutawayReceive, product, 33m, dockDoorLocation, "PLT-3");
			unfinalisedPutawayReceiveLine.WE_AdjustmentArrivalDate = today;
			Factory.Save();

			var finalisedPutawayTransfer = Helper.CreateWhsTransfer(client, whs, "T2");
			finalisedPutawayTransfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(finalisedPutawayTransfer,
				product, dockDoorLocation, normalLocation, "PLT-3", 33m);
			finalisedPutawayTransfer.RunPreSaveValidation();

			finalisedPutawayTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("FinalisedPutawayTransfer is finalised.", true, finalisedPutawayTransfer.IsFinalised);
			Factory.Save();

			var unfinalisedPickedReceive = Helper.CreateWhsReceive(client, whs, "R7");
			var unfinalisedPickedReceiveLine =
				Helper.CreateWhsReceiveLine(unfinalisedPickedReceive, product, 43m, dockDoorLocation, "PLT-4");
			unfinalisedPickedReceiveLine.WE_AdjustmentArrivalDate = today;
			Factory.Save();

			var pickedPutawayTransfer = Helper.CreateWhsTransfer(client, whs, "T7");
			pickedPutawayTransfer.WD_IsPutawayTransfer = true;
			var pickedPutawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(pickedPutawayTransfer, product,
				dockDoorLocation, normalLocation, "PLT-4", 43m);
			pickedPutawayTransfer.RunPreSaveValidation();
			pickedPutawayTransferLine.PickedTime = today;
			Factory.Save();

			var results = LoadSQLFunction(0);
			AssertEquals("Only transfers with finalised receives should be shown.", 2, results.Count);
			AssertContainsData(putawayTransferForFinRec, finalisedPutawayReceive.Lines.Single(), normalLocation, 17m,
				results, expectedPalletID: "PLT-2");
			AssertContainsData(finalisedPutawayTransfer, unfinalisedPutawayReceive.Lines.Single(), normalLocation, 33m,
				results, expectedPalletID: "PLT-3");
		}

		#endregion

		#region TestFunction_PutawayTransfersAreIncluded_HonoursFreeStorageDates

		public void TestFunction_PutawayTransfersAreIncluded_HonoursFreeStorageDates()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.DefaultLocation;
			var client = data.Org1;
			var product = data.Part1;
			var whs = data.Whs1;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			Factory.Save();

			var putawayReceive1 = Helper.CreateWhsReceive(client, whs, "R1");
			putawayReceive1.WD_ArrivalDate = today.ToOffset();
			Helper.CreateWhsReceiveLine(putawayReceive1, product, 1m, dockDoorLocation, "PLT-1");
			Factory.Save();

			var putawayTransfer1 = Helper.CreateWhsTransfer(client, whs, "T1");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, product, dockDoorLocation, normalLocation,
				"PLT-1", 1m);
			putawayTransfer1.RunPreSaveValidation();

			putawayTransfer1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("putawayTransferForFinRec is finalised.", true, putawayTransfer1.IsFinalised);
			putawayReceive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("finalisedPutawayReceive is finalised.", true, putawayReceive1.IsFinalised);
			Factory.Save();

			var putawayReceive2 = Helper.CreateWhsReceive(client, whs, "R2");
			putawayReceive2.WD_ArrivalDate = today.AddDays(-10).ToOffset();
			Helper.CreateWhsReceiveLine(putawayReceive2, product, 3m, dockDoorLocation, "PLT-2");
			Factory.Save();

			var putawayTransfer2 = Helper.CreateWhsTransfer(client, whs, "T2");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, product, dockDoorLocation, normalLocation,
				"PLT-2", 3m);
			putawayTransfer2.RunPreSaveValidation();

			putawayTransfer2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("putawayTransferForFinRec is finalised.", true, putawayTransfer2.IsFinalised);
			putawayReceive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("finalisedPutawayReceive is finalised.", true, putawayTransfer2.IsFinalised);
			Factory.Save();

			var putawayReceive3 = Helper.CreateWhsReceive(client, whs, "R3");
			putawayReceive3.WD_ArrivalDate = today.AddDays(-11).ToOffset();
			Helper.CreateWhsReceiveLine(putawayReceive3, product, 5m, dockDoorLocation, "PLT-3");
			Factory.Save();

			var putawayTransfer3 = Helper.CreateWhsTransfer(client, whs, "T3");
			putawayTransfer3.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(putawayTransfer3, product, dockDoorLocation, normalLocation,
				"PLT-3", 5m);
			putawayTransfer3.RunPreSaveValidation();

			putawayTransfer3.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("putawayTransferForFinRec is finalised.", true, putawayTransfer3.IsFinalised);
			putawayReceive3.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("finalisedPutawayReceive is finalised.", true, putawayTransfer3.IsFinalised);
			Factory.Save();

			var putawayReceive4 = Helper.CreateWhsReceive(client, whs, "R4");
			putawayReceive4.WD_ArrivalDate = today.AddDays(-20).ToOffset();
			Helper.CreateWhsReceiveLine(putawayReceive4, product, 7m, dockDoorLocation, "PLT-4");
			Factory.Save();

			var putawayTransfer4 = Helper.CreateWhsTransfer(client, whs, "T4");
			putawayTransfer4.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(putawayTransfer4, product, dockDoorLocation, normalLocation,
				"PLT-4", 7m);
			putawayTransfer4.RunPreSaveValidation();

			putawayTransfer4.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("putawayTransferForFinRec is finalised.", true, putawayTransfer4.IsFinalised);
			putawayReceive4.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("finalisedPutawayReceive is finalised.", true, putawayTransfer4.IsFinalised);
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 10, today);
			AssertEquals("Only transfers finalised within 10 days of current date should be included.", 2,
				results.Count);
			AssertContainsData(putawayTransfer1, putawayReceive1.Lines.Single(), normalLocation, 1m, results,
				expectedPalletID: "PLT-1");
			AssertContainsData(putawayTransfer2, putawayReceive2.Lines.Single(), normalLocation, 3m, results,
				expectedPalletID: "PLT-2");
		}

		#endregion

		#region TestFunction_PalletID

		public void TestFunction_PalletID()
		{
			var year = ZDateTime.Now.Year;
			var finalisedDate = new ZDateTimeOffset(year, 8, 31);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "B", 2, 1);
			var bom = Helper.CreateProduct("COM", data.Org1);
			Helper.CreateProductBOM(bom, data.Part1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationB1 = whs2.FindLocation("B-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 200m, locationA1, "P01");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine =
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, locationA1.ToLocationString(), "P01");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			var innerTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR1");
			var innerTransferLine = Helper.CreateWhsTransferLine(innerTransfer, data.Part1, 15m,
				locationA1.ToLocationString(), "P01", locationA2.ToLocationString(), "P02");
			innerTransfer.RunPreSaveValidation(); // commit transfer line
			innerTransferLine.PickedTime = finalisedDate;
			innerTransferLine.FinaliseDocketLine();
			innerTransferLine.WE_FinalisedDate = finalisedDate;

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR2",
				transferType: TransferType.Codes.InterWhsSource);
			var interTransferLine = Helper.CreateWhsTransferLine(interTransfer, data.Part1, 20m,
				locationA1.ToLocationString(), whs2.PK, locationB1.ToLocationString());
			interTransferLine.WE_TransferFromPalletId = "P01";
			interTransferLine.WE_PalletID = "P03";
			interTransferLine.RunPreSaveValidation(); // commit transfer line
			interTransferLine.PickedTime = finalisedDate;
			interTransferLine.FinaliseDocketLine();
			interTransferLine.WE_FinalisedDate = finalisedDate;
			interTransferLine.ChildTransferLine.WE_FinalisedDate = finalisedDate;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			order.WD_FinalisedDate = finalisedDate;
			pick.FinalisePick();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "O2", bom, 30m);
			var pick2 = Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketWithoutUserConfirmation();
			workOrder.WD_FinalisedDate = finalisedDate;
			pick2.FinalisePick();
			Factory.Save();

			var results = LoadSQLFunction(0);
			AssertEquals(8, results.Count);
			AssertContainsData(receive, receiveLine, locationA1, 200m, results, expectedPalletID: "P01");
			AssertContainsData(adjustment, adjustmentLine, locationA1, -5m, results, expectedPalletID: "P01");
			AssertContainsData(innerTransfer, innerTransferLine, locationA1, -15m, results, expectedPalletID: "P01");
			AssertContainsData(innerTransfer, innerTransferLine, locationA2, 15m, results, expectedPalletID: "P02");
			AssertContainsData(interTransfer, interTransferLine, locationA1, -20m, results, expectedPalletID: "P01");
			AssertContainsData(interTransfer.ChildTransfers.Single(), interTransferLine.ChildTransferLine, locationB1,
				20m, results, expectedPalletID: "P03");
			AssertContainsData(order, receiveLine, locationA1, -25m, results, expectedPalletID: "P01");
			AssertContainsData(workOrder, receiveLine, locationA1, -30m, results, expectedPalletID: "P01");
		}

		#endregion

		#region TestFunction_SerialNumber

		public void TestFunction_SerialNumber()
		{
			var year = ZDateTime.Now.Year;
			var finalisedDate = new ZDateTimeOffset(year, 8, 31);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "B", 2, 1);
			var bom = Helper.CreateProduct("COM", data.Org1);
			Helper.CreateProductBOM(bom, data.Part2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationB1 = whs2.FindLocation("B-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine(data.Part1, "SN1");
			var receiveLine2 = CreateReceiveLine(data.Part1, "SN2");
			var receiveLine3 = CreateReceiveLine(data.Part1, "SN3");
			var receiveLine4 = CreateReceiveLine(data.Part1, "SN4");
			var receiveLine5 = CreateReceiveLine(data.Part1, "SN5");
			var receiveLine6 = CreateReceiveLine(data.Part2, "SN6");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = finalisedDate;
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine =
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, locationA1.ToLocationString());
			adjustmentLine.WE_SerialNumber = "SN1";
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			var innerTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR1");
			var innerTransferLine = Helper.CreateWhsTransferLine(innerTransfer, data.Part1, 1m,
				locationA1.ToLocationString(), "", locationA2.ToLocationString(), "");
			innerTransferLine.WE_SerialNumber = "SN3";
			innerTransfer.RunPreSaveValidation(); // commit transfer line
			innerTransferLine.PickedTime = finalisedDate;
			innerTransferLine.FinaliseDocketLine();
			innerTransferLine.WE_FinalisedDate = finalisedDate;
			Factory.Save();

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR2",
				transferType: TransferType.Codes.InterWhsSource);
			var interTransferLine = Helper.CreateWhsTransferLine(interTransfer, data.Part1, 1m,
				locationA1.ToLocationString(), whs2.PK, locationB1.ToLocationString());
			interTransferLine.WE_SerialNumber = "SN2";
			interTransferLine.RunPreSaveValidation(); // commit transfer line
			interTransferLine.PickedTime = finalisedDate;
			interTransferLine.FinaliseDocketLine();
			interTransferLine.WE_FinalisedDate = finalisedDate;
			interTransferLine.ChildTransferLine.WE_FinalisedDate = finalisedDate;
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "O2", bom, 1m);
			var pick2 = Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketWithoutUserConfirmation();
			workOrder.WD_FinalisedDate = finalisedDate;
			pick2.FinalisePick();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			order.WD_FinalisedDate = finalisedDate;
			pick.FinalisePick();
			Factory.Save();

			var results = LoadSQLFunction(0);
			AssertEquals("Sql should produce correct number of results", 13, results.Count);
			AssertContainsData(receive, receiveLine1, locationA1, 1m, results, expectedSerialNumber: "SN1");
			AssertContainsData(receive, receiveLine2, locationA1, 1m, results, expectedSerialNumber: "SN2");
			AssertContainsData(receive, receiveLine3, locationA1, 1m, results, expectedSerialNumber: "SN3");
			AssertContainsData(receive, receiveLine4, locationA1, 1m, results, expectedSerialNumber: "SN4");
			AssertContainsData(receive, receiveLine5, locationA1, 1m, results, expectedSerialNumber: "SN5");
			AssertContainsData(receive, receiveLine6, locationA1, 1m, results, expectedSerialNumber: "SN6");
			AssertContainsData(adjustment, adjustmentLine, locationA1, -1m, results, expectedSerialNumber: "SN1");
			AssertContainsData(innerTransfer, innerTransferLine, locationA1, -1m, results, expectedSerialNumber: "SN3");
			AssertContainsData(innerTransfer, innerTransferLine, locationA2, 1m, results, expectedSerialNumber: "SN3");
			AssertContainsData(interTransfer, interTransferLine, locationA1, -1m, results, expectedSerialNumber: "SN2");
			AssertContainsData(interTransfer.ChildTransfers.Single(), interTransferLine.ChildTransferLine, locationB1,
				1m, results, expectedSerialNumber: "SN2");
			AssertContainsData(order, receiveLine4, locationA1, -1m, results, expectedSerialNumber: "SN4");
			AssertContainsData(workOrder, receiveLine6, locationA1, -1m, results, expectedSerialNumber: "SN6");

			WhsReceiveLine CreateReceiveLine(OrgSupplierPart part, string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, part, 1m, locationA1);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestFunction_FinalisedDateParameter

		public void TestFunction_FinalisedDateParameter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var currentDateTime = ZDateTime.Now;
			var currentDateTimeOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(currentDateTime);

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, locationA1, "");
			receive1.WD_FinalisedDate = currentDateTimeOffset;
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, locationA1, "");
			receive2.WD_FinalisedDate = currentDateTimeOffset.AddDays(-7);

			Factory.Save();

			var order1 = CreateWhsOrder(data.Org1, data.Whs1, "OR1", data.Part1, 50m, true, true);
			order1.WD_FinalisedDate = currentDateTimeOffset;
			var order2 = CreateWhsOrder(data.Org1, data.Whs1, "OR2", data.Part1, 50m, true, true);
			order2.WD_FinalisedDate = currentDateTimeOffset.AddDays(-7);

			Factory.Save();

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 35m, locationA1);
			adjustment1.FinaliseDocket();
			adjustment1.WD_FinalisedDate = currentDateTimeOffset;

			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD2", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 35m, locationA1);
			adjustment2.FinaliseDocket();
			adjustment2.WD_FinalisedDate = currentDateTimeOffset.AddDays(-7);

			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, "A-1", "A-2");
			transfer1.Lines[0].PickedTime = currentDateTimeOffset;
			transfer1.FinaliseDocket();

			transfer1.WD_FinalisedDate = currentDateTimeOffset;
			transfer1.Lines[0].WE_FinalisedDate = currentDateTimeOffset;

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 20m, "A-1", "A-2");
			transfer2.Lines[0].PickedTime = currentDateTimeOffset.AddDays(-7);
			transfer2.FinaliseDocket();

			transfer2.WD_FinalisedDate = currentDateTimeOffset.AddDays(-7);
			transfer2.Lines[0].WE_FinalisedDate = transfer2.WD_FinalisedDate;

			Factory.Save();

			var functionLoadResults = LoadSQLFunction(0, currentDateTime);

			AssertEquals(5, functionLoadResults.Count);
			AssertContainsData(receive1, receive1.Lines[0], locationA1, 100m, functionLoadResults);
			AssertContainsData(order1, order1.Lines[0], locationA1, -50m, functionLoadResults);
			AssertContainsData(adjustment1, adjustment1.Lines[0], locationA1, 35m, functionLoadResults);
			AssertContainsData(transfer1, transfer1.Lines[0], locationA1, -10m, functionLoadResults);
			AssertContainsData(transfer1, transfer1.Lines[0], locationA2, 10m, functionLoadResults);
		}

		#endregion

		#region TestFunction_WithPartAttributes

		public void TestFunction_WithPartAttributes_ReceiveLines()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var freeFinalisedReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(freeFinalisedReceive, data.Part1.PK, 1m, location.PK, "PLT1", ZDate.Empty,
				ZDate.Empty, "ATT1", "ATT2", "ATT3", "SN1", "");
			freeFinalisedReceive.FinaliseDocketWithoutUserConfirmation();
			freeFinalisedReceive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 10);

			var unfinalisedReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(unfinalisedReceive, data.Part1.PK, 1m, location.PK, "PLT2", ZDate.Empty,
				ZDate.Empty, "ATT1", "ATT2", "ATT3", "SN2", "");

			var finalisedReceiveOutOfFreeStorageDays = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveLine(finalisedReceiveOutOfFreeStorageDays, data.Part1.PK, 1m, location.PK, "PLT3",
				ZDate.Empty, ZDate.Empty, "ATT1", "ATT2", "ATT3", "SN3", "");
			finalisedReceiveOutOfFreeStorageDays.FinaliseDocketWithoutUserConfirmation();
			finalisedReceiveOutOfFreeStorageDays.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 1, finalisedDate: new ZDateTime(year, 1, 10));
			AssertEquals("Only 1 result is returned.", 1, results.Count);
			var result = results[0];
			AssertEquals("SerialNumber", "SN1", result["SerialNumber"]);
			AssertEquals("PartAttrib1", "ATT1", result["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ATT2", result["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ATT3", result["PartAttrib3"]);
			AssertEquals("Units", 1m, result["Units"]);
			AssertEquals("PalletID", "PLT1", result["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_AdjustmentLines()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var date = new ZDateTimeOffset(year, 1, 10);
			var freeFinalisedAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			Helper.CreateWhsAdjustmentLine(freeFinalisedAdjustment, data.Part1.PK, 1m, location.WLV_LocationString,
				"PLT1", date, "", "ATT1", "ATT2", "ATT3", "SN1", ZDate.Empty, ZDate.Empty);
			freeFinalisedAdjustment.FinaliseDocketWithoutUserConfirmation();
			freeFinalisedAdjustment.WD_FinalisedDate = date;

			var unfinalisedAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A2");
			Helper.CreateWhsAdjustmentLine(unfinalisedAdjustment, data.Part1.PK, 1m, location.WLV_LocationString,
				"PLT2", date, "", "ATT1", "ATT2", "ATT3", "SN2", ZDate.Empty, ZDate.Empty);

			var oldDate = new ZDateTimeOffset(year, 1, 5);
			var finalisedAdjustmentOutOfFreeStorageDays = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A3");
			Helper.CreateWhsAdjustmentLine(finalisedAdjustmentOutOfFreeStorageDays, data.Part1.PK, 1m,
				location.WLV_LocationString, "PLT3", oldDate, "", "ATT1", "ATT2", "ATT3", "SN3", ZDate.Empty,
				ZDate.Empty);
			finalisedAdjustmentOutOfFreeStorageDays.FinaliseDocketWithoutUserConfirmation();
			finalisedAdjustmentOutOfFreeStorageDays.WD_FinalisedDate = oldDate;
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.ToDateTime());
			AssertEquals("Only 1 result is returned.", 1, results.Count);
			var result = results[0];
			AssertEquals("SerialNumber", "SN1", result["SerialNumber"]);
			AssertEquals("PartAttrib1", "ATT1", result["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ATT2", result["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ATT3", result["PartAttrib3"]);
			AssertEquals("Units", 1m, result["Units"]);
			AssertEquals("PalletID", "PLT1", result["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_PickedOrder()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, location.PK, "PLT1", ZDate.Empty, ZDate.Empty,
				"ABC1", "ABC2", "ABC3", "SN1", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, location.PK, "PLT2", ZDate.Empty, ZDate.Empty,
				"ABC2", "ABC3", "ABC4", "SN2", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, location.PK, "PLT3", ZDate.Empty, ZDate.Empty,
				"ABC3", "ABC4", "ABC5", "SN3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var pickedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(pickedOrder, data.Part1, 1m);
			orderLine1.WE_PartAttrib1 = "ABC1";
			orderLine1.WE_PartAttrib2 = "ABC2";
			orderLine1.WE_PartAttrib3 = "ABC3";
			orderLine1.WE_SerialNumber = "SN1";

			var pick = Helper.CreatePickNew(pickedOrder);

			// Pick the line
			var date = new ZDateTimeOffset(year, 1, 10);
			pick.GetAllPickLines().Single().WZ_PickedDateTime = date;

			var oldPickedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(oldPickedOrder, data.Part1, 1m);
			orderLine2.WE_PartAttrib1 = "ABC2";
			orderLine2.WE_PartAttrib2 = "ABC3";
			orderLine2.WE_PartAttrib3 = "ABC4";
			orderLine2.WE_SerialNumber = "SN2";

			var pick2 = Helper.CreatePickNew(oldPickedOrder);

			// Pick the line
			var oldDate = new ZDateTimeOffset(year, 1, 5);
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = oldDate;

			var unPickedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(unPickedOrder, data.Part1, 1m);
			orderLine3.WE_PartAttrib1 = "ABC3";
			orderLine3.WE_PartAttrib2 = "ABC4";
			orderLine3.WE_PartAttrib3 = "ABC5";
			orderLine3.WE_SerialNumber = "SN3";

			Factory.Save();

			pick.FinaliseAllOrders();
			pickedOrder.WD_FinalisedDate = date;
			AssertIsFinalisedPrecondition(pickedOrder);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			pick2.FinaliseAllOrders();
			oldPickedOrder.WD_FinalisedDate = oldDate;
			AssertIsFinalisedPrecondition(oldPickedOrder);
			pick2.FinalisePick();
			AssertIsFinalisedPrecondition(pick2);
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals("Only 1 result is returned.", 1, results.Count);
			var result = results[0];
			AssertEquals("SerialNumber", "SN1", result["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result["PartAttrib3"]);
			AssertEquals("Units", -1m, result["Units"]);
			AssertEquals("PalletID", "PLT1", result["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_PickedOrder_NoOriginalPickedInventoryLine()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, location.PK, "PLT1", ZDate.Empty, ZDate.Empty,
				"ABC1", "ABC2", "ABC3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_PartAttrib1 = "ABC1";
			orderLine1.WE_PartAttrib2 = "ABC2";
			orderLine1.WE_PartAttrib3 = "ABC3";
			orderLine1.WE_SerialNumber = "SN1";

			var pick = Helper.CreatePickNew(order);

			// Pick the line
			var date = new ZDateTimeOffset(year, 1, 10);
			pick.GetAllPickLines().Single().WZ_PickedDateTime = date;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
				AssertEquals("Precondition: Picked Directly.", ZGuid.Empty,
					order.Lines[0].PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			}

			pick.FinaliseAllOrders();
			order.WD_FinalisedDate = date;
			AssertIsFinalisedPrecondition(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals("Only 1 result is returned.", 1, results.Count);
			var result = results[0];
			AssertEquals("SerialNumber", "SN1", result["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result["PartAttrib3"]);
			AssertEquals("Units", -1m, result["Units"]);
			AssertEquals("PalletID", "PLT1", result["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_PickedOrder_InTransitTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, location.PK, "PLT1", ZDate.Empty, ZDate.Empty,
				"ABC1", "ABC2", "ABC3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var date = new ZDateTimeOffset(year, 1, 10);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_PartAttrib1 = "ABC1";
			orderLine.WE_PartAttrib2 = "ABC2";
			orderLine.WE_PartAttrib3 = "ABC3";
			orderLine.WE_SerialNumber = "SN1";

			var pick = Helper.CreatePickNew(order);

			// Pick the line / create In-Transit Transfer
			var pickLine = pick.GetAllPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, date);
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals("Only 1 result is returned.", 1, results.Count);
			var result = results[0];
			AssertEquals("SerialNumber", "SN1", result["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result["PartAttrib3"]);
			AssertEquals("Units", -1m, result["Units"]);
			AssertEquals("PalletID", "PLT1", result["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_InterWhsAndDestinationWhsTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 5, 1);
			var whs1Location = data.Whs1.DefaultLocation;
			var whs2Location = whs2.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, whs1Location.PK, "PLT1", ZDate.Empty, ZDate.Empty,
				"ABC1", "ABC2", "ABC3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var date = new ZDateTimeOffset(year, 1, 10);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, whs1Location.ToLocationString(),
				"PLT1", whs2.PK, whs2Location.ToLocationString(), "PLT1", ZDateTimeOffset.Empty, ZDate.Empty, ZDate.Empty, "ABC1",
				"ABC2", "ABC3");
			line.WE_SerialNumber = "SN1";

			transfer.FinaliseDocket();
			transfer.WD_FinalisedDate = date;
			line.WE_FinalisedDate = date;

			var destTransfer = transfer.ChildTransfers.Single();
			destTransfer.WD_FinalisedDate = date;
			destTransfer.Lines[0].WE_FinalisedDate = date;

			AssertEquals(true, transfer.IsFinalised);
			AssertEquals(true, destTransfer.IsFinalised);
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals("Only 2 results are returned.", 2, results.Count);
			var result1 = results.Single(result => (ZGuid)result["LocationPK"] == whs1Location.PK);
			AssertEquals("SerialNumber", "SN1", result1["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result1["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result1["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result1["PartAttrib3"]);
			AssertEquals("Units", -1m, result1["Units"]);
			AssertEquals("PalletID", "PLT1", result1["PalletID"]);

			var result2 = results.Single(result => (ZGuid)result["LocationPK"] == whs2Location.PK);
			AssertEquals("SerialNumber", "SN1", result2["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result2["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result2["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result2["PartAttrib3"]);
			AssertEquals("Units", 1m, result2["Units"]);
			AssertEquals("PalletID", "PLT1", result2["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_UnfinalisedInterWhsAndDestinationWhsTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 5, 1);
			var whs1Location = data.Whs1.DefaultLocation;
			var whs2Location = whs2.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, whs1Location.PK, "PLT1", ZDate.Empty, ZDate.Empty,
				"ABC1", "ABC2", "ABC3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var date = new ZDateTimeOffset(year, 1, 10);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var sourceLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, whs1Location.ToLocationString(),
				"PLT1", whs2.PK, whs2Location.ToLocationString(), "PLT1", ZDateTimeOffset.Empty, ZDate.Empty, ZDate.Empty, "ABC1",
				"ABC2", "ABC3");
			sourceLine.WE_SerialNumber = "SN1";

			sourceLine.PickedTime = date;
			AssertEquals("Source Transfer Line should not be Finalised.", false, sourceLine.IsFinalised);

			var destinationTransfer = transfer.ChildTransfers.Single();
			var destinationLine = destinationTransfer.Lines.Single();
			AssertEquals("Destination Transfer Line should not be Finalised.", false, destinationLine.IsFinalised);
			Factory.Save();

			var results1 = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals("Only 1 result is returned.", 1, results1.Count);
			var result1 = results1.Single();
			AssertEquals("SerialNumber", "SN1", result1["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result1["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result1["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result1["PartAttrib3"]);
			AssertEquals("Units", -1m, result1["Units"]);
			AssertEquals("PalletID", "PLT1", result1["PalletID"]);
			AssertEquals("LocationPK", whs1Location.PK, result1["LocationPK"]);

			transfer.FinaliseDocket();
			transfer.WD_FinalisedDate = date;
			sourceLine.WE_FinalisedDate = date;
			destinationLine.WE_FinalisedDate = date;
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AssertEquals("Source Transfer Line should be Finalised.", true, sourceLine.IsFinalised);
			AssertEquals("Destination Transfer Line should be Finalised.", true, destinationLine.IsFinalised);

			var results2 = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals("2 results are returned.", 2, results2.Count);
			var result21 = results2.Single(result => (ZGuid)result["LocationPK"] == whs1Location.PK);
			AssertEquals("SerialNumber", "SN1", result21["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result21["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result21["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result21["PartAttrib3"]);
			AssertEquals("Units", -1m, result21["Units"]);
			AssertEquals("PalletID", "PLT1", result21["PalletID"]);

			var result22 = results2.Single(result => (ZGuid)result["LocationPK"] == whs2Location.PK);
			AssertEquals("SerialNumber", "SN1", result22["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result22["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result22["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result22["PartAttrib3"]);
			AssertEquals("Units", 1m, result22["Units"]);
			AssertEquals("PalletID", "PLT1", result22["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_PutawayTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, dockDoorLocation.PK, "PLT1", ZDate.Empty,
				ZDate.Empty, "ABC1", "ABC2", "ABC3", "SN1", "");
			Factory.Save();

			var date = new ZDateTimeOffset(year, 1, 10);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, dockDoorLocation.ToLocationString(),
				nonDockDoorLocation.ToLocationString());
			line.WE_TransferFromPalletId = "PLT1";
			line.WE_PalletID = "PLT1";
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			line.WE_SerialNumber = "SN1";
			line.WE_PartAttrib1 = "ABC1";
			line.WE_PartAttrib2 = "ABC2";
			line.WE_PartAttrib3 = "ABC3";
			AssertEquals("Precondition - ensure no stock is committed.", 0m, line.GetQtyCommittedToThisLine());
			line.WE_SerialNumber = "SN1";

			transfer.RunPreSaveValidation();
			line.PickedTime = date;
			line.FinaliseDocketLine();
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals("Only 1 resulta are returned.", 1, results.Count);
			var result = results[0];
			AssertEquals("Docket Type", "TFR", result["DocketType"]);
			AssertEquals("SerialNumber", "SN1", result["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", result["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", result["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", result["PartAttrib3"]);
			AssertEquals("Units", 1m, result["Units"]);
			AssertEquals("PalletID", "PLT1", result["PalletID"]);
		}

		public void TestFunction_WithPartAttributes_InnerTransfers()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, locationA1.PK, "PLT1", ZDate.Empty, ZDate.Empty,
				"ABC1", "ABC2", "ABC3", "SN1", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, locationA1.PK, "PLT2", ZDate.Empty, ZDate.Empty,
				"ABC2", "ABC3", "ABC4", "SN2", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, locationA1.PK, "PLT3", ZDate.Empty, ZDate.Empty,
				"ABC3", "ABC4", "ABC5", "SN3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, locationA1.PK, "PLT4", ZDate.Empty, ZDate.Empty,
				"ABC4", "ABC5", "ABC6", "SN4", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var date = new ZDateTimeOffset(year, 1, 9);
			var transferFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transferFinalised, data.Part1, 1m, "A-1", "A-2");
			transferLine.WE_TransferFromPalletId = "PLT1";
			transferLine.WE_PalletID = "PLT1";
			transferLine.WE_PartAttrib1 = "ABC1";
			transferLine.WE_PartAttrib2 = "ABC2";
			transferLine.WE_PartAttrib3 = "ABC3";
			transferLine.WE_SerialNumber = "SN1";
			transferFinalised.Lines[0].PickedTime = date;
			transferFinalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferFinalised);
			transferFinalised.WD_FinalisedDate = date;
			transferFinalised.Lines[0].WE_FinalisedDate = date;
			Factory.Save();

			var transferPartiallyFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLineFinalised =
				Helper.CreateWhsTransferLine(transferPartiallyFinalised, data.Part1, 1m, "A-1", "A-2");
			transferLineFinalised.WE_TransferFromPalletId = "PLT2";
			transferLineFinalised.WE_PalletID = "PLT2";
			transferLineFinalised.WE_PartAttrib1 = "ABC2";
			transferLineFinalised.WE_PartAttrib2 = "ABC3";
			transferLineFinalised.WE_PartAttrib3 = "ABC4";
			transferLineFinalised.WE_SerialNumber = "SN2";
			transferLineFinalised.PickedTime = date;
			transferLineFinalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineFinalised);
			transferLineFinalised.WE_FinalisedDate = date;

			var transferLinePickedNotFinalised =
				Helper.CreateWhsTransferLine(transferPartiallyFinalised, data.Part1, 1m, "A-1", "A-2");
			transferLinePickedNotFinalised.WE_TransferFromPalletId = "PLT3";
			transferLinePickedNotFinalised.WE_PalletID = "PLT3";
			transferLinePickedNotFinalised.WE_PartAttrib1 = "ABC3";
			transferLinePickedNotFinalised.WE_PartAttrib2 = "ABC4";
			transferLinePickedNotFinalised.WE_PartAttrib3 = "ABC5";
			transferLinePickedNotFinalised.WE_SerialNumber = "SN3";
			transferLinePickedNotFinalised.RunPreSaveValidation(); // to commit inventory
			transferLinePickedNotFinalised.PickedTime = date;
			Factory.Save();

			var transferNotFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			var transferLineNotFinalised =
				Helper.CreateWhsTransferLine(transferNotFinalised, data.Part1, 1m, "A-1", "A-2");
			transferLineNotFinalised.WE_TransferFromPalletId = "PLT4";
			transferLineNotFinalised.WE_PalletID = "PLT4";
			transferLineNotFinalised.WE_PartAttrib1 = "ABC4";
			transferLineNotFinalised.WE_PartAttrib2 = "ABC5";
			transferLineNotFinalised.WE_PartAttrib3 = "ABC6";
			transferLineNotFinalised.WE_SerialNumber = "SN4";
			transferNotFinalised.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: date.Date);
			AssertEquals(
				"System should find 2 transactions each for every finalised Inner Transfer and 1 for in-transit transfer.",
				5, results.Count);

			var finalisedTransferSourceResult = results.Single(result =>
				(ZString)result["PalletID"] == "PLT1" && (ZGuid)result["LocationPK"] == locationA1.PK);
			AssertEquals("SerialNumber", "SN1", finalisedTransferSourceResult["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", finalisedTransferSourceResult["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", finalisedTransferSourceResult["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", finalisedTransferSourceResult["PartAttrib3"]);
			AssertEquals("Units", -1m, finalisedTransferSourceResult["Units"]);

			var finalisedTransferDestinationResult = results.Single(result =>
				(ZString)result["PalletID"] == "PLT1" && (ZGuid)result["LocationPK"] == locationA2.PK);
			AssertEquals("SerialNumber", "SN1", finalisedTransferDestinationResult["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC1", finalisedTransferDestinationResult["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC2", finalisedTransferDestinationResult["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC3", finalisedTransferDestinationResult["PartAttrib3"]);
			AssertEquals("Units", 1m, finalisedTransferDestinationResult["Units"]);

			var partiallyFinalisedTransferSourceResult = results.Single(result =>
				(ZString)result["PalletID"] == "PLT2" && (ZGuid)result["LocationPK"] == locationA1.PK);
			AssertEquals("SerialNumber", "SN2", partiallyFinalisedTransferSourceResult["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC2", partiallyFinalisedTransferSourceResult["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC3", partiallyFinalisedTransferSourceResult["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC4", partiallyFinalisedTransferSourceResult["PartAttrib3"]);
			AssertEquals("Units", -1m, partiallyFinalisedTransferSourceResult["Units"]);

			var partiallyFinalisedTransferDestinationResult = results.Single(result =>
				(ZString)result["PalletID"] == "PLT2" && (ZGuid)result["LocationPK"] == locationA2.PK);
			AssertEquals("SerialNumber", "SN2", partiallyFinalisedTransferDestinationResult["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC2", partiallyFinalisedTransferDestinationResult["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC3", partiallyFinalisedTransferDestinationResult["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC4", partiallyFinalisedTransferDestinationResult["PartAttrib3"]);
			AssertEquals("Units", 1m, partiallyFinalisedTransferDestinationResult["Units"]);

			var inTransitTransferResult = results.Single(result =>
				(ZString)result["PalletID"] == "PLT3" && (ZGuid)result["LocationPK"] == locationA1.PK);
			AssertEquals("SerialNumber", "SN3", inTransitTransferResult["SerialNumber"]);
			AssertEquals("PartAttrib1", "ABC3", inTransitTransferResult["PartAttrib1"]);
			AssertEquals("PartAttrib2", "ABC4", inTransitTransferResult["PartAttrib2"]);
			AssertEquals("PartAttrib3", "ABC5", inTransitTransferResult["PartAttrib3"]);
			AssertEquals("Units", -1m, inTransitTransferResult["Units"]);
		}

		#endregion

		#region TestFunction_Adjustment_AdjustmentArrivalDate

		public void TestFunction_AdjustmentArrivalDate_InternalWarehouseAdjustment_ApplyInPast()
		{
			TestFunction_AdjustmentArrivalDate_Core(adjustWithReceiveArrival: true, AdjustmentType.Codes.InternalWarehouseAdjustment,
				expectedResult: Array.Empty<(string, decimal)>());
		}

		public void TestFunction_AdjustmentArrivalDate_InternalWarehouseAdjustment_ApplyNow()
		{
			TestFunction_AdjustmentArrivalDate_Core(adjustWithReceiveArrival: false, AdjustmentType.Codes.InternalWarehouseAdjustment,
				("ADJ", 10), ("ADJ", -10));
		}

		public void TestFunction_AdjustmentArrivalDate_InternalWarehouseAdjustment_NotSet()
		{
			TestFunction_AdjustmentArrivalDate_Core(adjustWithReceiveArrival: null, AdjustmentType.Codes.InternalWarehouseAdjustment,
				("ADJ", 10), ("ADJ", -10));
		}

		public void TestFunction_AdjustmentArrivalDate_Adjustment_ApplyInPast()
		{
			TestFunction_AdjustmentArrivalDate_Core(adjustWithReceiveArrival: true, AdjustmentType.Codes.Adjustment,
				("ADJ", 10), ("ADJ", -10));
		}

		public void TestFunction_AdjustmentArrivalDate_Adjustment_ApplyNow()
		{
			TestFunction_AdjustmentArrivalDate_Core(adjustWithReceiveArrival: false, AdjustmentType.Codes.Adjustment,
				("ADJ", 10), ("ADJ", -10));
		}

		public void TestFunction_AdjustmentArrivalDate_Adjustment_NotSet()
		{
			TestFunction_AdjustmentArrivalDate_Core(adjustWithReceiveArrival: null, AdjustmentType.Codes.Adjustment,
				("ADJ", 10), ("ADJ", -10));
		}

		void TestFunction_AdjustmentArrivalDate_Core(bool? adjustWithReceiveArrival, string adjustmentSubType, params (string DocketType, decimal Units)[] expectedResult)
		{
			var now = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var otherLocation = data.Whs1.FindLocation("A-2");
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now.AddMonths(-1), data.Part1, 10, location, "");
			Factory.Save();

			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN");
			adjustmentIn.WD_DocketSubType = adjustmentSubType;
			Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1.PK.ToGuid(), 10, otherLocation.ToLocationString());
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			ZDateTimeOffset? adjustmentArrivalDate = null;
			if (adjustWithReceiveArrival.HasValue)
			{
				adjustmentArrivalDate = adjustWithReceiveArrival.Value ? receive.WD_ArrivalDate : now;
			}

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT");
			adjustmentOut.WD_DocketSubType = adjustmentSubType;
			Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1.PK, -10, location.ToLocationString());
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();

			if (adjustWithReceiveArrival.HasValue)
			{
				adjustmentIn.Lines[0].WE_AdjustmentArrivalDate = adjustmentArrivalDate.Value;
				adjustmentOut.Lines[0].WE_AdjustmentArrivalDate = adjustmentArrivalDate.Value;
			}
			else
			{
				//adjustmentIn WE_AdjustmentArrivalDate  should have a value when there is stock on hand
				adjustmentOut.Lines[0].WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;
			}
			Factory.Save();

			AssertResult(now, expectedResult);
			AssertResult(now.AddMonths(-1), ("INW", 10), ("ADJ", 10), ("ADJ", -10));

			void AssertResult(ZDateTimeOffset reportDate, params (string DocketType, decimal Units)[] expectedRowInResult)
			{
				var results = LoadSQLFunction(freeStoreDays: 0, finalisedDate: reportDate.ToDateTime());
				AssertEquals("Result count", expectedRowInResult.Length, results.Count);
				foreach (var item in expectedRowInResult)
				{
					var expectedLocationPK = item.DocketType == "ADJ" && item.Units > 0 ? otherLocation.PK : location.PK;
					var resultRow = results.Single(result => (ZString)result["DocketType"] == item.DocketType && (ZGuid)result["LocationPK"] == expectedLocationPK);
					AssertEquals("Units", item.Units, resultRow["Units"]);
				}
			}
		}

		#endregion

		#region SetupData

		WhsReceive CreateWhsReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart part,
			WhsLocation location, ZDecimal units, bool finaliseReceive)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, units);
			inventory.WI_WL = location.PK;

			if (finaliseReceive)
			{
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);

				var receiveLine = receive.Lines[0];
				receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				receiveLine.HeldCodeChangeQuantity = 1;
				receiveLine.ChangeInventoryHeldCode(true);
			}

			Factory.Save();
			return receive;
		}

		WhsOrder CreateWhsOrder(OrgHeader client, WhsWarehouse warehouse, string reference, OrgSupplierPart part,
			ZDecimal units, bool finaliseOrder, bool finalisePick)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			Helper.CreateWhsOrderLine(order, part, units);
			var pick = Helper.CreatePickNew(order);

			if (finaliseOrder)
			{
				pick.FinaliseAllOrders();
				AssertIsFinalisedPrecondition(order);

				if (finalisePick)
				{
					pick.FinalisePick();
					AssertIsFinalisedPrecondition(pick);
				}
			}
			else if (finalisePick)
			{
				throw new ArgumentException(
					"wtf -- cannot finalise a pick without finalising the order."); // for retard programmers
			}

			Factory.Save();
			return order;
		}

		WhsAdjustment CreateWhsAdjustment(OrgHeader client, WhsWarehouse warehouse, string reference,
			OrgSupplierPart part, ZDecimal units, WhsLocation location, bool finaliseAdjustment)
		{
			var adjustment = Helper.CreateWhsAdjustment(client, warehouse, reference, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, part, units, location);

			if (finaliseAdjustment)
			{
				adjustment.FinaliseDocket();
				AssertIsFinalisedPrecondition(adjustment);

				line.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				line.HeldCodeChangeQuantity = 1;
				line.ChangeInventoryHeldCode(true);
			}
			else
			{
				line.RunPreSaveValidation(); // to commit inventory
			}

			Factory.Save();
			return adjustment;
		}

		WhsTransfer CreateWhsTransfer(OrgHeader client, string transferSubType, WhsWarehouse sourceWhs,
			string reference, OrgSupplierPart part, WhsLocation sourceLocation, ZDecimal units, WhsWarehouse destWhs,
			WhsLocation destLocation, bool finaliseTransfer)
		{
			var transfer = Helper.CreateWhsTransfer(client, sourceWhs, reference, Notify);
			transfer.WD_DocketSubType = transferSubType;
			var line = Helper.CreateWhsTransferLine(transfer, part, units, sourceLocation.ToLocationString(),
				destWhs.PK, destLocation.ToLocationString());

			if (finaliseTransfer)
			{
				transfer.FinaliseDocket();
				AssertEquals(true, transfer.IsFinalised);

				if (transferSubType != TransferType.Codes.InterWhsSource)
				{
					line.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
					line.HeldCodeChangeQuantity = 1;
					line.ChangeInventoryHeldCode(true);
				}
			}
			else
			{
				line.RunPreSaveValidation(); // to commit inventory
			}

			Factory.Save();
			return transfer;
		}

		WhsTransfer GetRelatedInternalTransfer(WhsTransfer masterTransfer)
		{
			return Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, masterTransfer.PK));
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObjectCollection LoadSQLFunction(byte freeStoreDays, ZDateTime finalisedDate)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql =
				@"
							select   *
							from     WhsInvoiceTransactions(@FreeStoreDays, @FinalisedDate)
							order by DocketType, Units
						";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@FreeStoreDays", freeStoreDays, OrgCompanyDataSchema.OB_WhsClientFreeStorageDays);
			parameters.Add("@FinalisedDate", finalisedDate, WhsDocketSchema.WD_FinalisedDate);

			result.Load(sql, parameters);
			return result;
		}

		DynamicBusinessObjectCollection LoadSQLFunction(byte freeStoreDays = 7)
		{
			return LoadSQLFunction(freeStoreDays, ZDateTime.MinSmallDateTimeValue);
		}

		#endregion
	}
}
