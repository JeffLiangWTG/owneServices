using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsInventoryViewAsAtDate : WhsTestCaseWithFactory
	{
		#region TestView_Transfers

		public void TestView_Transfers()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1",
				new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5), data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2");
			transfer.RunPreSaveValidation();

			transferLine2.PickedTime = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 7);
			transferLine3.PickedTime = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);
			transferLine2.FinaliseDocketLine();
			transferLine3.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);
			AssertIsFinalisedPrecondition(transferLine3);

			// hack to get finalised dates
			transferLine2.WE_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 7);
			transferLine3.WE_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);

			Factory.Save();

			var results1 = LoadSQLFunction(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 1));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			var results2 = LoadSQLFunction(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8));
			AssertEquals("System should find 2 inventory records, for original location and for 1 transferred line.", 2,
				results2.Count);
			AssertLineMatch(receive.Lines[0], data.Whs1.FindLocation("A-1").PK, 85m, results2);
			AssertLineMatch(transferLine2, data.Whs1.FindLocation("A-2").PK, 15m, results2);

			var results3 = LoadSQLFunction(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 15));
			AssertEquals("System should find 2 inventory records, for original location and for 2 transferred lines.",
				2, results3.Count);
			AssertLineMatch(receive.Lines[0], data.Whs1.FindLocation("A-1").PK, 65m, results3);
			AssertLineMatch(transferLine2, data.Whs1.FindLocation("A-2").PK, 35m,
				results3); // transfer line 2 and 3 should be rolled up into 1.
			AssertLineMatch(transferLine3, data.Whs1.FindLocation("A-2").PK, 35m, results3);
		}

		public void TestView_Transfers_SerialNumber()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5);
			var receiveLine1 = CreateReceiveLine("SER1");
			CreateReceiveLine("SER2");
			var receiveLine3 = CreateReceiveLine("SER3");
			CreateReceiveLine("SER4");
			var receiveLine5 = CreateReceiveLine("SER5");
			var receiveLine6 = CreateReceiveLine("SER6");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			CreateTransferLine("SER1");
			var transferLine2 = CreateTransferLine("SER2");
			var transferLine3 = CreateTransferLine("SER3");
			var transferLine4 = CreateTransferLine("SER4");
			var transferLine5 = CreateTransferLine("SER5");
			transfer.RunPreSaveValidation();

			transferLine2.PickedTime = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 7);
			transferLine3.PickedTime = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);
			transferLine4.PickedTime = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 7);
			transferLine5.PickedTime = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 17);
			transferLine2.FinaliseDocketLine();
			transferLine3.FinaliseDocketLine();
			transferLine4.FinaliseDocketLine();
			transferLine5.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);
			AssertIsFinalisedPrecondition(transferLine3);
			AssertIsFinalisedPrecondition(transferLine4);
			AssertIsFinalisedPrecondition(transferLine5);

			// hack to get finalised dates
			transferLine2.WE_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 7);
			transferLine3.WE_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);
			transferLine4.WE_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 7);
			transferLine5.WE_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 17);

			Factory.Save();

			var loc1PK = data.Whs1.FindLocation("A-1").PK;
			var loc2PK = data.Whs1.FindLocation("A-2").PK;

			var results1 = LoadSQLFunction(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 1));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			var results2 = LoadSQLFunction(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8));
			AssertEquals("System should find 6 inventory records, for 4 original location and for 2 transferred line.",
				6, results2.Count);
			AssertLineMatch(receiveLine1, loc1PK, 1m, results2);
			AssertLineMatch(receiveLine3, loc1PK, 1m, results2);
			AssertLineMatch(receiveLine5, loc1PK, 1m, results2);
			AssertLineMatch(receiveLine6, loc1PK, 1m, results2);
			AssertLineMatch(transferLine2, loc2PK, 1m, results2);
			AssertLineMatch(transferLine4, loc2PK, 1m, results2);

			var results3 = LoadSQLFunction(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 15));
			AssertEquals("System should find 6 inventory records, for 3 original location and for 3 transferred lines.",
				6, results3.Count);
			AssertLineMatch(receiveLine1, loc1PK, 1m, results3);
			AssertLineMatch(receiveLine5, loc1PK, 1m, results3);
			AssertLineMatch(receiveLine6, loc1PK, 1m, results3);
			AssertLineMatch(transferLine2, loc2PK, 1m, results3);
			AssertLineMatch(transferLine3, loc2PK, 1m, results3);
			AssertLineMatch(transferLine4, loc2PK, 1m, results3);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
				line.WE_PartAttrib1 = "PA1";
				line.WE_SerialNumber = serialNumber;

				return line;
			}

			WhsTransferLine CreateTransferLine(string serialNumber)
			{
				var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "A-2");
				line.WE_PartAttrib1 = "PA1";
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestFunction_FinalisedOrder

		public void TestFunction_FinalisedOrder()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			// Pick the line
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 7);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results1 = LoadSQLFunction(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			// Add received quantity
			var results2 = LoadSQLFunction(new ZDateTime(year, 1, 1));
			AssertEquals("Precondition - only one inventory grouped line.", 1, results2.Count);
			AssertLineMatch(receive.Lines[0], location.PK, 20m, results2);

			// Should still appear in location after picked
			var results3 = LoadSQLFunction(new ZDateTime(year, 1, 3));
			AssertEquals("ResultSet Count", 1, results3.Count);
			AssertLineMatch(receive.Lines[0], location.PK, 20m, results3);

			// Reduce stock by finalising pick
			var results5 = LoadSQLFunction(new ZDateTime(year, 1, 8));
			AssertEquals("ResultSet Count", 1, results5.Count);
			AssertLineMatch(receive.Lines[0], location.PK, 12m, results5);
		}

		#endregion

		#region TestFunction_PickedButUnfinalisedOrder_WithDockDoorTransfer

		public void TestFunction_PickedButUnfinalisedOrder_WithDockDoorTransfer()
		{
			TestFunction_PickedButUnfinalisedOrder_WithDockDoorTransfer(dockDoorTransferFinalised: false);
		}

		public void TestFunction_PickedButUnfinalisedOrder_WithDockDoorTransfer_FinalisedDockDoorTransfer()
		{
			TestFunction_PickedButUnfinalisedOrder_WithDockDoorTransfer(dockDoorTransferFinalised: true);
		}

		void TestFunction_PickedButUnfinalisedOrder_WithDockDoorTransfer(bool dockDoorTransferFinalised)
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			// Pick the line
			var pickLine = pick.GetAllPickLines().Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
			Factory.Save();

			if (dockDoorTransferFinalised)
			{
				// Finalise the In-Transit Transfer
				transferLine.FinaliseDocketLine();
				transferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
				Factory.Save();
			}

			var results1 = LoadSQLFunction(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			// Add received quantity
			var results2 = LoadSQLFunction(new ZDateTime(year, 1, 1));
			AssertEquals("Precondition - only one inventory grouped line.", 1, results2.Count);
			AssertLineMatch(receive.Lines[0], location.PK, 20m, results2);

			// Should still appear in location after picked by Dock Door Transfer
			var results3 = LoadSQLFunction(new ZDateTime(year, 1, 3));
			AssertEquals("ResultSet Count", 1, results3.Count);
			AssertLineMatch(receive.Lines[0], location.PK, 20m, results3);

			// Should still appear in location after In-Transit stock created by Dock Door Transfer (if finalised)
			var results4 = LoadSQLFunction(new ZDateTime(year, 1, 5));
			AssertEquals("ResultSet Count", 1, results4.Count);
			AssertLineMatch(receive.Lines[0], location.PK, 20m, results4);
		}

		#endregion

		#region TestView_PickedButUnfinalisedOrder_DifferentPickedDates

		public void TestView_PickedButUnfinalisedOrder_DifferentPickedDates()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 40m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 60m);
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines();
			pickLines.Single(pl => pl.WZ_Units == 60m).WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 5);
			AssertEquals("Precondition: 60 picked units.", 60m,
				pickLines.Where(pl => pl.IsPicked).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 40 units committed but unpicked.", 40m,
				pickLines.Where(pl => !pl.IsPicked).Sum(pl => pl.WZ_Units));

			pickLines.Single(pl => pl.WZ_Units == 40m).WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 10);
			AssertEquals("Precondition: 100 picked units.", 100m,
				pickLines.Where(pl => pl.IsPicked).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 0 units committed but unpicked.", 0m,
				pickLines.Where(pl => !pl.IsPicked).Sum(pl => pl.WZ_Units));
			Factory.Save();

			var results1 = LoadSQLFunction(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			// Add received quantity
			var results2 = LoadSQLFunction(new ZDateTime(year, 1, 2));
			AssertEquals("Precondition - only one inventory grouped line.", 1, results2.Count);
			AssertLineMatch(receive1.Lines[0], location.PK, 100m, results2);

			// Should show 100 units, 40 in the location, 60 in transit (but represented in the src location).
			var results3 = LoadSQLFunction(new ZDateTime(year, 1, 6));
			AssertEquals("ResultSet Count", 1, results3.Count);
			AssertLineMatch(receive1.Lines[0], location.PK, 100m, results3);

			// Should show 100 units, 0 in the location, 100 in transit (but represented in the src location).
			var results4 = LoadSQLFunction(new ZDateTime(year, 1, 11));
			AssertEquals("ResultSet Count", 1, results4.Count);
			AssertLineMatch(receive1.Lines[0], location.PK, 100m, results4);
		}

		#endregion

		#region TestFunction_FinalisedWorkOrder

		public void TestFunction_FinalisedWorkOrder()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
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

			// Pick the lines
			foreach (var pickLine in workOrder.Lines[0].ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);
			}

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 7);
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrderPick);
			Factory.Save();

			// Add received quantity
			var results1 = LoadSQLFunction(new ZDateTime(year, 1, 1));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results1.Count);
			AssertLineMatch(frameInventory, location.PK, 10m, results1);
			AssertLineMatch(wheelInventory, location.PK, 20m, results1);

			// Reduce stock by finalising pick
			var results2 = LoadSQLFunction(new ZDateTime(year, 1, 8));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results2.Count);
			AssertLineMatch(frameInventory, location.PK, 2m, results2);
			AssertLineMatch(wheelInventory, location.PK, 4m, results2);
		}

		#endregion

		#region TestFunction_PickedButUnfinalisedWorkOrder_WithDockDoorTransfer

		public void TestFunction_PickedButUnfinalisedWorkOrder_WithDockDoorTransfer()
		{
			TestFunction_PickedButUnfinalisedWorkOrder_WithDockDoorTransfer(dockDoorTransferFinalised: false);
		}

		public void TestFunction_PickedButUnfinalisedWorkOrder_WithDockDoorTransfer_FinalisedDockDoorTransfer()
		{
			TestFunction_PickedButUnfinalisedWorkOrder_WithDockDoorTransfer(dockDoorTransferFinalised: true);
		}

		public void TestFunction_PickedButUnfinalisedWorkOrder_WithDockDoorTransfer(bool dockDoorTransferFinalised)
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
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

			// Pick the lines
			foreach (var pickLine in workOrder.Lines[0].ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
			}

			Factory.Save();

			if (dockDoorTransferFinalised)
			{
				// Finalise the In-Transit Transfer
				var transfer =
					Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer,
						workOrderPick.PK));
				AssertNotNull("Should have created a Dock Door Transfer.", transfer);

				transfer.FinaliseDocketWithoutUserConfirmation();
				foreach (var line in transfer.Lines)
				{
					line.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
				}

				Factory.Save();
			}

			// Add received quantity
			var results1 = LoadSQLFunction(new ZDateTime(year, 1, 1));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results1.Count);
			AssertLineMatch(frameInventory, location.PK, 10m, results1);
			AssertLineMatch(wheelInventory, location.PK, 20m, results1);

			// Should still appear in location after picked by Dock Door Transfer
			var results2 = LoadSQLFunction(new ZDateTime(year, 1, 3));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results2.Count);
			AssertLineMatch(frameInventory, location.PK, 10m, results2);
			AssertLineMatch(wheelInventory, location.PK, 20m, results2);

			// Should still appear in location after In-Transit stock created by Dock Door Transfer
			var results3 = LoadSQLFunction(new ZDateTime(year, 1, 5));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results3.Count);
			AssertLineMatch(frameInventory, location.PK, 10m, results3);
			AssertLineMatch(wheelInventory, location.PK, 20m, results3);
		}

		#endregion

		#region TestView_PickedButUnfinalisedInnerTransfers

		public void TestView_PickedButUnfinalisedInnerTransfers()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, locationA1, "");
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, locationA1, "");
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 110m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // commit transfer line
			AssertEquals("Precondition - committed 110 units to transfer.", 110m,
				transferLine.QtyCommittedIncludingMatchingLines);
			transferLine.PickedTime = new ZDateTimeOffset(year, 1, 5);

			// Make sure we use the pick line's values
			transferLine.MatchingLines[0].PickLines[0].WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 7);
			Factory.Save();

			var results1 = LoadSQLFunction(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			// Add received quantity
			var results2 = LoadSQLFunction(new ZDateTime(year, 1, 2));
			AssertEquals("Precondition - only one inventory grouped line.", 1, results2.Count);
			AssertLineMatch(receive1.Lines[0], locationA1.PK, 200m, results2);

			// Should show 200 units, 100 in the location, 100 in transit (but represented in the src location).
			var results3 = LoadSQLFunction(new ZDateTime(year, 1, 6));
			AssertEquals("ResultSet Count", 1, results3.Count);
			AssertLineMatch(receive1.Lines[0], locationA1.PK, 200m, results3);

			// Should show 200 units, 90 in the location, 110 in transit (but represented in the src location).
			var results4 = LoadSQLFunction(new ZDateTime(year, 1, 8));
			AssertEquals("ResultSet Count", 1, results4.Count);
			AssertLineMatch(receive1.Lines[0], locationA1.PK, 200m, results4);

			transfer.FinaliseDocket();
			transfer.Lines[0].WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transfer.Lines[0].MatchingLines[0].WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			// Add transferred quantity
			var results5 = LoadSQLFunction(new ZDateTime(year, 1, 11));
			AssertEquals("Precondition - two grouped inventory lines.", 2, results5.Count);
			AssertLineMatch(receive1.Lines[0], locationA1.PK, 90m, results5);
			AssertLineMatch(receive1.Lines[0], locationA2.PK, 110m, results5);
		}

		#endregion

		#region TestView_PickedButUnfinalisedInnerTransfer_WithUnpickedLine

		public void TestView_PickedButUnfinalisedInnerTransfer_WithUnpickedLine()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, inventory.LocationString, "");
			var transferedLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, inventory.LocationString, "");
			AssertEquals("Precondition: Transfer Line should not be in transit.", InventoryStatus.Codes.Available,
				transferedLine.WE_CurrentInventoryStatus);
			transfer.RunPreSaveValidation();
			transferedLine.PickedTime = new ZDateTimeOffset(year, 1, 5);
			AssertEquals("Precondition: Transfer Line should be in transit.", InventoryStatus.Codes.InTransit,
				transferedLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var results = LoadSQLFunction(ZDateTime.Empty);
			AssertLineMatch(receive.Lines[0], data.Whs1.DefaultLocation.PK, 10m,
				results); // Result should include nonTransfered line
		}

		#endregion

		#region TestView_PickedButUnfinalisedInterWhsTransfers

		public void TestView_PickedButUnfinalisedInterWhsTransfers()
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
			transferInterWhsSourceLine.PickedTime = new ZDateTimeOffset(year, 1, 5);

			var transferInterWhsDest =
				Helper.CreateWhsTransfer(client, whs2, "TR3", Notify, TransferType.Codes.InterWhsDest);
			var transferInterWhsDestLine =
				Helper.CreateWhsTransferLine(transferInterWhsDest, part, 25m, "A-1", whs1.PK, "B");
			transferInterWhsDest.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 25m,
				transferInterWhsDestLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsDestLine.PickedTime = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var results1 = LoadSQLFunction(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			// Add received quantity
			var results2 = LoadSQLFunction(new ZDateTime(year, 1, 2));
			AssertEquals("Precondition - only one inventory grouped line.", 1, results2.Count);
			AssertLineMatch(receive.Lines[0], whs1.FindLocation("A-1").PK, 100m, results2);

			// Should show 100 units, 60 in the location, 40 in transit (but represented in the src location).
			var results3 = LoadSQLFunction(new ZDateTime(year, 1, 6));
			AssertEquals("ResultSet Count", 1, results3.Count);
			AssertLineMatch(receive.Lines[0], whs1.FindLocation("A-1").PK, 100m, results3);

			transferInterWhsSourceLine.FinaliseDocketLine();
			transferInterWhsDestLine.FinaliseDocketLine();
			transferInterWhsSourceLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transferInterWhsSourceLine.ChildTransferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transferInterWhsDestLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transferInterWhsDestLine.ChildTransferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			Factory.Save();
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine);
			AssertIsFinalisedPrecondition(transferInterWhsDestLine);
			Factory.Save();

			// Add transferred quantity
			var results4 = LoadSQLFunction(new ZDateTime(year, 1, 11));
			AssertEquals("Precondition - two grouped inventory lines.", 2, results4.Count);
			AssertLineMatch(receive.Lines[0], whs1.FindLocation("A-1").PK, 60m, results4);
			AssertLineMatch(transferInterWhsDestLine, whs2.FindLocation("B").PK, 40m, results4);
		}

		#endregion

		#region TestView_WhenFinaliseDateIsDifferentToPickTime_InnerTransfer

		public void TestView_WhenFinaliseDateIsDifferentToPickTime_InnerTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receiveDate = new ZDateTime(year, 10, 1);
			var pickingDate = new ZDateTime(year, 10, 7);
			var finaliseTransferDate = new ZDateTime(year, 10, 10);

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, locationA1, "");
			receive.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(receiveDate);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // commit transfer line
			transferLine.PickedTime = data.Whs1.GetWarehouseBranchDateTimeOffset(pickingDate);
			transfer.FinaliseDocket();
			transfer.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(finaliseTransferDate);
			transferLine.WE_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(finaliseTransferDate);
			Factory.Save();

			var results = LoadSQLFunction(receiveDate.AddDays(-1).Date);
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results.Count);

			results = LoadSQLFunction(pickingDate.Date); // pickingDate before finaliseTransferDate
			AssertEquals("Should have only receive.", 1, results.Count);
			AssertLineMatch(receive.Lines[0], locationA1.PK, 20m, results);

			results = LoadSQLFunction(finaliseTransferDate.Date
				.AddDays(-1)); // after pickingDate before finaliseTransferDate
			AssertEquals("Should product still be in source location when is in_transit at this time.", 1,
				results.Count);
			AssertLineMatch(receive.Lines[0], locationA1.PK, 20m, results);

			results = LoadSQLFunction(finaliseTransferDate.Date);
			AssertEquals("Transfer is finalise at this time so should have product in both locations.", 2,
				results.Count);
			AssertLineMatch(receive.Lines[0], locationA1.PK, 20m - 8m, results);
			AssertLineMatch(receive.Lines[0], locationA2.PK, 8m, results);
		}

		#endregion

		#region TestView_WhenFinaliseDateIsDifferentToPickTime_InterWhsTransfers

		public void TestView_WhenFinaliseDateIsDifferentToPickTime_InterWhsTransfers()
		{
			var year = ZDateTime.Now.Year;
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var whs3 = Helper.CreateWarehouse("WH3", "C");
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();
			var locationA = whs1.FindLocation("A");
			var locationB = whs2.FindLocation("B");
			var locationC = whs3.FindLocation("C");

			var receiveFinalisedDate = new ZDateTime(year, 10, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", part, 100m, locationA, "");
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(receiveFinalisedDate);
			Factory.Save();

			var sourceTransferPickedTime = new ZDateTime(year, 10, 7);
			var transferInterWhsSource =
				Helper.CreateWhsTransfer(client, whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var transferInterWhsSourceLine =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 15m, "A", whs2.PK, "B");
			transferInterWhsSource.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 15m,
				transferInterWhsSourceLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsSourceLine.PickedTime = whs1.GetWarehouseBranchDateTimeOffset(sourceTransferPickedTime);

			var transferInterWhsDest =
				Helper.CreateWhsTransfer(client, whs3, "TR3", Notify, TransferType.Codes.InterWhsDest);
			var transferInterWhsDestLine =
				Helper.CreateWhsTransferLine(transferInterWhsDest, part, 25m, "A", whs1.PK, "C");
			transferInterWhsDest.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 25m,
				transferInterWhsDestLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsDestLine.PickedTime = whs1.GetWarehouseBranchDateTimeOffset(sourceTransferPickedTime).AddDays(1);
			Factory.Save();

			var results = LoadSQLFunction(receiveFinalisedDate.AddDays(-1));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results.Count);

			results = LoadSQLFunction(receiveFinalisedDate);
			AssertEquals("Precondition - only inventory for receive.", 1, results.Count);
			AssertLineMatch(receive.Lines[0], locationA.PK, 100m, results);

			results = LoadSQLFunction(sourceTransferPickedTime.AddDays(-1));
			AssertEquals("Should only have inventory for receive.", 1, results.Count);
			AssertLineMatch(receive.Lines[0], locationA.PK, 100m, results);

			var transferFinalisedDate = new ZDateTime(year, 10, 10);
			transferInterWhsSourceLine.FinaliseDocketLine();
			transferInterWhsDestLine.FinaliseDocketLine();
			transferInterWhsSourceLine.WE_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(transferFinalisedDate);
			transferInterWhsSourceLine.ChildTransferLine.WE_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(transferFinalisedDate);
			transferInterWhsDestLine.WE_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(transferFinalisedDate);
			transferInterWhsDestLine.ChildTransferLine.WE_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(transferFinalisedDate);
			Factory.Save();
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine);
			AssertIsFinalisedPrecondition(transferInterWhsDestLine);
			Factory.Save();

			// Add transferred quantity
			results = LoadSQLFunction(receiveFinalisedDate.AddDays(1));
			AssertEquals("Should have only receive.", 1, results.Count);
			AssertLineMatch(receive.Lines[0], locationA.PK, 100m, results);

			results = LoadSQLFunction(transferFinalisedDate.AddDays(-1));
			AssertEquals("Product should still be in source location when is in-transit at this time.", 1,
				results.Count);
			AssertLineMatch(receive.Lines[0], locationA.PK, 100m, results);

			results = LoadSQLFunction(transferFinalisedDate);
			AssertEquals("Transfers are finalised at this time so should have product in three locations.", 3,
				results.Count);
			AssertLineMatch(receive.Lines[0], locationA.PK, 100m - 15m - 25m, results);
			AssertLineMatch(transferInterWhsSourceLine.ChildTransferLine, locationB.PK, 15m, results);
			AssertLineMatch(transferInterWhsDestLine, locationC.PK, 25m, results);
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObjectCollection LoadSQLFunction(ZDateTime date)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"SELECT * FROM WhsInventoryAsAtDate(@Date)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@Date", date, WhsDocketSchema.WD_FinalisedDate);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region AssertLineMatch

		void AssertLineMatch(WhsDocketLine expectedLine, ZGuid expectedLocationPK, ZDecimal expectedQty,
			DynamicBusinessObjectCollection results)
		{
			var docket = expectedLine.Docket;
			var expectedSerialNumber = expectedLine.WE_SerialNumber;
			AssertLineMatch(docket.Warehouse, docket.Client, expectedLine.SupplierPart, expectedSerialNumber,
				expectedLocationPK, expectedQty, results);

			var actualLine = results.Single(l =>
				(ZDecimal)l["Quantity"] == expectedQty && (ZString)l["SerialNumber"] == expectedSerialNumber);
			AssertEquals("PartAttrib1", expectedLine.WE_PartAttrib1, actualLine["PartAttrib1"]);
			AssertEquals("PartAttrib2", expectedLine.WE_PartAttrib2, actualLine["PartAttrib2"]);
			AssertEquals("PartAttrib3", expectedLine.WE_PartAttrib3, actualLine["PartAttrib3"]);
		}

		void AssertLineMatch(WhsWarehouse expectedWhs, OrgHeader expectedClient, OrgSupplierPart expectedPart,
			ZString expectedSerialNumber, ZGuid expectedLocationPK, ZDecimal expectedQty,
			DynamicBusinessObjectCollection results)
		{
			var actualLine = results.Single(l =>
				(ZDecimal)l["Quantity"] == expectedQty && (ZString)l["SerialNumber"] == expectedSerialNumber);
			AssertEquals("WarehousePK", expectedWhs.PK, actualLine["WarehousePK"]);
			AssertEquals("ClientPK", expectedClient.PK, actualLine["ClientPK"]);
			AssertEquals("ProductPK", expectedPart.PK, actualLine["ProductPK"]);
			AssertEquals("LocationPK", expectedLocationPK, actualLine["LocationPK"]);
			AssertEquals("Quantity", expectedQty, actualLine["Quantity"]);
		}

		#endregion
	}
}
