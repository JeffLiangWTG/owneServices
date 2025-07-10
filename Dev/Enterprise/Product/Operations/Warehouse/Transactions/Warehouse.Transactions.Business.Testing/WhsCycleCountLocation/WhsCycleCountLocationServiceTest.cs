using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCycleCountLocationServiceTest : WhsTestCaseWithFactory
	{
		#region TestMarkInventoriesLostInCycleCount

		#region TestMarkInventoriesLostInCycleCount_CannotHoldInventory

		public void TestMarkInventoriesLostInCycleCount_CannotHoldInventory_CycleCountIsNotFinished()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m);
			Factory.Save();

			// EndTime of cycle count is empty
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "TTT");
			AssertEquals("Cycle Count is not finished", true, cycleCount.WCL_EndTime.IsEmpty);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNull("Should not held any inventory if cycle count is not finished", heldCodeChangeLine);

			// Variance of cycle count is rejected
			var now = DateTimeOffset.Now;
			cycleCount.WCL_StartTime = now;
			cycleCount.WCL_EndTime = now.AddHours(1);
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Rejected,
				varianceQty: -5, expectedQty: 10, client: data.Org1, part: data.Part1);
			Factory.Save();

			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			heldCodeChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNull("Should not held any inventory if no open variances existed", heldCodeChangeLine);

			// Valid Cycle Count
			var cycleCount2 =
				Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation, "PID", now, now.AddDays(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 10, client: data.Org1, part: data.Part1);
			Factory.Save();

			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount2.PK });

			heldCodeChangeLine = Factory
				.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode,
					InventoryHoldCodes.Codes.LostInCycleCount)).Single();
			AssertEquals("Should held matched inventory", 5m, heldCodeChangeLine.WE_StockOnHand);
		}

		public void TestMarkInventoriesLostInCycleCount_CannotHoldInventory_CycleCountIsPLT()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.PalletCount, now, now.AddDays(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, true, -5,
				expectedQty: 10);
			Factory.Save();

			AssertEquals("Cycle Count is finished", false, cycleCount.WCL_EndTime.IsEmpty);

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNull("Should not held any inventory", heldCodeChangeLine);
		}

		public void TestMarkInventoriesLostInCycleCount_CannotHoldInventory_CycleCountIsPRD()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductOnly, now, now.AddDays(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -5,
				expectedQty: 10, client: data.Org1, part: data.Part1);
			Factory.Save();

			AssertEquals("Cycle Count is finished", false, cycleCount.WCL_EndTime.IsEmpty);

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNull("Should not held any inventory", heldCodeChangeLine);
		}

		public void TestMarkInventoriesLostInCycleCount_InventoryIsInTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1.ToLocationString(),
				"PLT1", location2.ToLocationString(), "PLT1");
			transferLine.RunPreSaveValidation();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -5,
				expectedQty: 10, client: data.Org1, part: data.Part1);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNull("Should not held any inventory", heldCodeChangeLine);
		}

		public void TestMarkInventoriesLostInCycleCount_InventoryIsCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -5,
				expectedQty: 10, client: data.Org1, part: data.Part1);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNull("Should not held any inventory", heldCodeChangeLine);
		}

		public void TestMarkInventoriesLostInCycleCount_PropertiesNotMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var part3 = Helper.CreateProduct("PART3", data.Org1);
			var org2 = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2, "PLTY");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLTX");
			Helper.CreateWhsReceiveLine(receive, part3, 10m, location1, "PLT1");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1, "PLT1", ZDate.Today, ZDate.Today, "XX",
				"BB", "CC", ZString.Empty);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1, "PLT1", ZDate.Today, ZDate.Today, "AA",
				"XX", "CC", ZString.Empty);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1, "PLT1", ZDate.Today, ZDate.Today, "AA",
				"BB", "XX", ZString.Empty);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1, "PLT1", ZDate.Today.AddDays(1),
				ZDate.Today, "AA", "BB", "CC", ZString.Empty);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1, "PLT1", ZDate.Today,
				ZDate.Today.AddDays(1), "AA", "BB", "CC", ZString.Empty);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1, "PLT1", ZDate.Today, ZDate.Today, "AA",
				"BB", "CC", ZString.Empty);
			receive.FinaliseDocketWithoutUserConfirmation();

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "REC2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, location1, "PLT1");
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -100, expectedQty: 100, client: data.Org1, part: data.Part2, palletID: "Plt1",
				partAttrib1: "aA", partAttrib2: "bb", partAttrib3: "cC", expiryDate: ZDate.Today,
				packingDate: ZDate.Today);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Should put held code for matched inventory", 1, heldCodeLCCChangeLines.Length);
			AssertEquals(10m, heldCodeLCCChangeLines.Single().WE_StockOnHand);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount

		public void TestMarkInventoriesLostInCycleCount()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m,
				data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeLCCChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNotNull("Held Inventory should not be null", holdCodeLCCChangeLine);
			AssertEquals("The inventory should be held", 10m, holdCodeLCCChangeLine.WE_StockOnHand);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_MultipleInventories

		public void TestMarkInventoriesLostInCycleCount_MultipleInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -15, expectedQty: 15, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Should put held code for all matched inventory", 2, holdCodeChangeLines.Length);

			AssertEquals("Should put held code for matched inventory", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("The inventory should be held", 5m, receiveLine1.WE_StockOnHand);
			AssertEquals("Should put held code for matched inventory", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("The inventory should be held", 10m, receiveLine2.WE_StockOnHand);
		}

		public void TestMarkInventoriesLostInCycleCount_MultipleInventories_MatchSerialNumbers_InventoryMatches()
		{
			TestMarkInventoriesLostInCycleCount_MultipleInventories_MatchSerialNumbersCore(true);
		}

		public void TestMarkInventoriesLostInCycleCount_MultipleInventories_MatchSerialNumbers_InventoryMismatches()
		{
			TestMarkInventoriesLostInCycleCount_MultipleInventories_MatchSerialNumbersCore(false);
		}

		void TestMarkInventoriesLostInCycleCount_MultipleInventories_MatchSerialNumbersCore(bool isMatchingSerial)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1");
			receiveLine1.WE_SerialNumber = isMatchingSerial ? "Serial" : "S1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1");
			receiveLine2.WE_SerialNumber = "S2";
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -2,
				expectedQty: 2, client: data.Org1, part: data.Part1, serialNumber: "Serial", palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			if (isMatchingSerial)
			{
				AssertEquals("Should put held code for all matched inventory", 1, holdCodeChangeLines.Length);
				AssertEquals("Should put held code for matched inventory", InventoryHoldCodes.Codes.LostInCycleCount,
					receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("The inventory should be held", 1m, receiveLine1.WE_StockOnHand);
				AssertEquals("Non matching Serial should not be held.", InventoryStatus.Codes.Available,
					receiveLine2.WE_CurrentInventoryStatus);
			}
			else
			{
				AssertEquals("Should not put held code for any inventory", 0, holdCodeChangeLines.Length);
				AssertEquals("Should not change held code to LCC for inventory where serial number mismatches",
					InventoryStatus.Codes.Available, receiveLine1.WE_CurrentInventoryStatus);
				AssertEquals("Should not change held code to LCC for inventory where serial number mismatches",
					InventoryStatus.Codes.Available, receiveLine2.WE_CurrentInventoryStatus);
			}
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_InventoryHasPartialCommitted

		public void TestMarkInventoriesLostInCycleCount_InventoryHasPartialCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -7,
				expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Should put held code for matched inventory", 1, holdCodeChangeLines.Length);
			AssertEquals(4m, holdCodeChangeLines.Single().WE_StockOnHand);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_InventoryHasPartialHeld

		public void TestMarkInventoriesLostInCycleCount_InventoryHasPartialHeld()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var existingHoldCodeLine = Factory.LoadTop1<WhsDocketLine>(
				new ZQuery(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.Held));
			AssertNotNull("Precondition: Inventory has been held partially", existingHoldCodeLine);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -7, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Should put held code for matched inventory", 2, heldCodeLCCChangeLines.Length);
			AssertNotNull("Should held correct qty",
				heldCodeLCCChangeLines.SingleOrDefault(l => l.WE_StockOnHand == 5m));
			AssertNotNull("Should held correct qty",
				heldCodeLCCChangeLines.SingleOrDefault(l => l.WE_StockOnHand == 2m));

			AssertEquals("Should not change held code of the existing held code line", InventoryHoldCodes.Codes.Held,
				existingHoldCodeLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should update the qty of exiting held code line", 3m, existingHoldCodeLine.WE_StockOnHand);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_InventoryHasPartialHeld

		public void TestMarkInventoriesLostInCycleCount_CachesInventoryHoldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");

			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -1, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");

			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery());
			AssertEquals("Should put held code for matched inventory", 2, heldCodeLCCChangeLines.Length);
			var line1 = heldCodeLCCChangeLines.SingleOrDefault(l => l.WE_StockOnHand == 9m);
			var line2 = heldCodeLCCChangeLines.SingleOrDefault(l => l.WE_StockOnHand == 1m);
			AssertNotNull("Should held correct qty", line1);
			AssertNotNull("Should held correct qty", line2);

			AssertEquals("Should only cache held code for lines lost in Cycle Count", string.Empty, line1.PreviousHeldCodeForCycleCount);
			AssertEquals("Should cache correct held code for matched inventory", InventoryHoldCodes.Codes.Damaged, line2.PreviousHeldCodeForCycleCount);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_InventoryIsNotFinalised

		public void TestMarkInventoriesLostInCycleCount_InventoryIsNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m,
				data.Whs1.DefaultLocation, "PLT1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC2");
			var receiveLine2 =
				Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			AssertIsFinalisedPrecondition(receive1);
			AssertEquals("Precondition: ReceiveLine is not finalised", false, receiveLine2.IsFinalised);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -15, expectedQty: 15, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeLCCChangeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNotNull("Should held the rest of inventory", holdCodeLCCChangeLine);
			AssertEquals("The rest of inventory should be held", 10m, holdCodeLCCChangeLine.WE_StockOnHand);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_InventoryHasPartialLostInCycleCount

		public void TestMarkInventoriesLostInCycleCount_InventoryHasPartialLostInCycleCount_LostQtyGreaterThanExisted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var existingHoldCodeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNotNull("Precondition: Inventory has been held partially", existingHoldCodeLine);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -7, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Should create a new held inventory", 2, holdCodeLCCChangeLines.Length);

			AssertEquals("Shoult not change the qty for existing line", 5m, existingHoldCodeLine.WE_StockOnHand);

			var newHeldCodeLine = holdCodeLCCChangeLines.Single(l => l.PK != existingHoldCodeLine.PK);
			AssertEquals("Shoult create a new held inventory", 2m, newHeldCodeLine.WE_StockOnHand);

			AssertEquals("The StockOnHand should be updated", 3m, receiveLine.WE_StockOnHand);
		}

		public void TestMarkInventoriesLostInCycleCount_InventoryHasPartialLostInCycleCount_LostQtyLessThanExisted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var existingHoldCodeLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertNotNull("Precondition: Inventory has been held partially", existingHoldCodeLine);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -3,
				expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Should not create new held inventory", existingHoldCodeLine.PK,
				holdCodeLCCChangeLines.Single().PK);
			AssertEquals("Shoult not change the qty for existing line", 5m, existingHoldCodeLine.WE_StockOnHand);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocation

		public void TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocation()
		{
			TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocationCore(true);
		}

		public void TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocation_NoExpectedLocation()
		{
			TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocationCore(false);
		}

		void TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocationCore(bool hasExpectedLocation)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 15m, location1, "PLT1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 5,
				expectedLocation: hasExpectedLocation ? location1 : null);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			if (hasExpectedLocation)
			{
				AssertEquals("Should put held code for PalletID matched inventory",
					InventoryHoldCodes.Codes.LostInCycleCount, receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(10m, receiveLine1.WE_StockOnHand);
				AssertEquals("Should put held code for PalletID matched inventory",
					InventoryHoldCodes.Codes.LostInCycleCount, receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(15m, receiveLine2.WE_StockOnHand);
				AssertEquals("Should put held code for PalletID matched inventory",
					InventoryHoldCodes.Codes.LostInCycleCount, receiveLine3.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(20m, receiveLine3.WE_StockOnHand);
			}
			else
			{
				var heldCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
					WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
				AssertEquals("Should not create any held inventory", 0, heldCodeLCCChangeLines.Length);
			}
		}

		public void TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocation_InventoryHasHeld()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 15m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine1.HeldCodeChangeQuantity = 5m;
			receiveLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine1.ChangeInventoryHeldCode(true);
			Factory.Save();

			var existingHeldCodeLine = Factory.LoadTop1<WhsDocketLine>(
				new ZQuery(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.Held));
			AssertNotNull("Precondition: Inventory has been held partially", existingHeldCodeLine);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 5, expectedLocation: location1);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("All inventory with unexpected location should be changed to LCC", 3,
				heldCodeLCCChangeLines.Length);

			var heldCodeLine1 =
				heldCodeLCCChangeLines.Single(l => l.PK != receiveLine2.PK && l.PK != existingHeldCodeLine.PK);
			AssertEquals("The rest of inventory should be held", 5m, heldCodeLine1.WE_StockOnHand);
			var heldCodeLine2 = heldCodeLCCChangeLines.Single(l => l.PK == receiveLine2.PK);
			AssertEquals("The inventory should be held", 15m, heldCodeLine2.WE_StockOnHand);
			var heldCodeLine3 = heldCodeLCCChangeLines.Single(l => l.PK == existingHeldCodeLine.PK);
			AssertEquals("The held inventory should be changed to LCC", 5m, heldCodeLine3.WE_StockOnHand);
		}

		public void TestMarkInventoriesLostInCycleCount_PalletIDExistingDifferentLocationInDifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("XXX", "A", 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = whs2.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, whs2);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2, "PLT1");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 15m, location2, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 5, expectedLocation: location2);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var heldCodeLCCChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Should not held any inventories if Pallet ID existing in different warehouse", 0,
				heldCodeLCCChangeLines.Length);
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_HasPalletID_SerialNumberInDifferentLocation

		public void TestMarkInventoriesLostInCycleCount_HasPalletID_SerialNumberInDifferentLocation()
		{
			TestMarkInventoriesLostInCycleCount_HasPalletID_SerialNumberInDifferentLocationCore("S11", true);
		}

		public void TestMarkInventoriesLostInCycleCount_HasPalletID_SerialNumberInDifferentLocation_NoExpectedLocation()
		{
			TestMarkInventoriesLostInCycleCount_HasPalletID_SerialNumberInDifferentLocationCore("S11", false);
		}

		void TestMarkInventoriesLostInCycleCount_HasPalletID_SerialNumberInDifferentLocationCore(string serial,
			bool hasExpectedLocation)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT2", ZDate.Today,
				ZDate.Today, "S11", "S12", "S13", ZString.Empty);
			receiveLine1.WE_SerialNumber = serial;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2, "PLT1", ZDate.Today,
				ZDate.Today, "S21", "S22", "S23", ZString.Empty);
			receiveLine2.WE_SerialNumber = "Random";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT3", ZDate.Today,
				ZDate.Today, "S31", "S32", "S33", ZString.Empty);
			receiveLine3.WE_SerialNumber = "UniqueSerial";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location3,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S11",
				partAttrib2: "S12", partAttrib3: "S13", serialNumber: serial, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: hasExpectedLocation ? location1 : null);
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S11",
				partAttrib2: "S12", partAttrib3: "S13", serialNumber: "UniqueSerial", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: hasExpectedLocation ? location1 : null);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			if (hasExpectedLocation)
			{
				AssertEquals("Should put held code for Serial Number matched inventory",
					InventoryHoldCodes.Codes.LostInCycleCount, receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(1m, receiveLine1.WE_StockOnHand);

				AssertEquals("Should put held code for PalletID matched inventory",
					InventoryHoldCodes.Codes.LostInCycleCount, receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(1m, receiveLine2.WE_StockOnHand);

				AssertEquals("Should put held code for Serial matched inventory",
					InventoryHoldCodes.Codes.LostInCycleCount, receiveLine3.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(1m, receiveLine3.WE_StockOnHand);
			}
			else
			{
				AssertEquals(
					"Should not put held code for Serial Number matched inventory if has no expected stock location",
					InventoryStatus.Codes.Available, receiveLine1.WE_CurrentInventoryStatus);
				AssertEquals("Should not put held code for PalletID matched inventory", InventoryStatus.Codes.Available,
					receiveLine2.WE_CurrentInventoryStatus);
			}
		}

		#endregion

		#region TestMarkInventoriesLostInCycleCount_SerialNumberExistingDifferentLocation_InventoryHasHeld

		public void TestMarkInventoriesLostInCycleCount_SerialNumberExistingDifferentLocation_InventoryHasHeld()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2, "PLT1", ZDate.Today,
				ZDate.Today, "S11", "S12", "S13", ZString.Empty);
			receiveLine1.WE_SerialNumber = "S11";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2, "PLT1", ZDate.Today,
				ZDate.Today, "S21", "S22", "S23", ZString.Empty);
			receiveLine2.WE_SerialNumber = "UNIQUESERIAL";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine1.HeldCodeChangeQuantity = 1m;
			receiveLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine1.ChangeInventoryHeldCode(true);
			Factory.Save();

			var existingHeldCodeLine = Factory.LoadTop1<WhsDocketLine>(
				new ZQuery(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.Held));
			AssertNotNull("Precondition: Inventory has been held partially", existingHeldCodeLine);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S11",
				partAttrib2: "S12", partAttrib3: "S13", serialNumber: "s11", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location2);
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S21",
				partAttrib2: "S22", partAttrib3: "S23", serialNumber: "UniqueSerial", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location2);
			Factory.Save();

			var cycleCountLocationService = new WhsCycleCountLocationService();
			cycleCountLocationService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertEquals("Should change held code to LCC", InventoryHoldCodes.Codes.LostInCycleCount,
				existingHeldCodeLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should not change qty of line", 1m, existingHeldCodeLine.WE_StockOnHand);

			AssertEquals(
				"Should not change held code to LCC for inventory where serial number mismatches part attribute",
				InventoryStatus.Codes.Held, receiveLine2.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestDBHits_MarkInventoriesLostInCycleCount

		public void TestDBHits_MarkInventoriesLostInCycleCount()
		{
			var now = DateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var org3 = Helper.CreateClient("CLIENT3");
			var org4 = Helper.CreateClient("CLIENT4");
			var part3 = Helper.CreateProduct("PART3", org3);
			var part4 = Helper.CreateProduct("PART4", org4);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: true);

			Helper.SetClientAllAttributeType(org3, true);
			Helper.SetProductAllAttributeUse(org3, part3, true, useSerialNumber: true);

			Helper.SetClientAllAttributeType(org4, true);
			Helper.SetProductAllAttributeUse(org4, part4, true, useSerialNumber: true);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC1");
			var receive2 = Helper.CreateWhsReceive(org3, data.Whs1, "REC2");
			var receive3 = Helper.CreateWhsReceive(org4, data.Whs1, "REC3");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			for (int i = 0; i < 100; i++)
			{
				var part = Helper.CreateProduct("XPART" + i.ToString(), data.Org1);
				// Lost inventory
				Helper.CreateWhsReceiveLine(receive1, part, 10m, location1, "PLT1");
				Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
					varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");

				// Unexpected Pallet ID
				Helper.CreateWhsReceiveLine(receive1, data.Part1, 5m, location2, "XPLT" + i.ToString());
				Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
					palletID: "XPLT" + i.ToString(), client: data.Org1, part: data.Part1, varianceQty: 1,
					expectedLocation: location2);

				// Unexpected Serial Number
				Helper.CreateWhsReceiveLine(receive1, data.Part2.PK, 1m, location2.PK, "PLT3", ZDate.Today, ZDate.Today,
					"AA", "BB", "CC", "S" + i.ToString(), "");
				Helper.CreateWhsReceiveLine(receive2, part3.PK, 1m, location2.PK, "PLT3", ZDate.Today, ZDate.Today,
					"AA", "BB", "CC", "X" + i.ToString(), "");
				Helper.CreateWhsReceiveLine(receive3, part4.PK, 1m, location2.PK, "PLT3", ZDate.Today, ZDate.Today,
					"AA", "BB", "CC", "Z" + i.ToString(), "");
				Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
					palletID: "PLT3", client: data.Org1, part: data.Part2, varianceQty: 1, partAttrib1: "AA",
					partAttrib2: "BB", partAttrib3: "CC", serialNumber: "S" + i.ToString(), expiryDate: ZDate.Today,
					packingDate: ZDate.Today, expectedLocation: location2);
				Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
					palletID: "PLT3", client: data.Org1, part: part3, varianceQty: 1, partAttrib1: "AA",
					partAttrib2: "BB", partAttrib3: "CC", serialNumber: "X" + i.ToString(), expiryDate: ZDate.Today,
					packingDate: ZDate.Today, expectedLocation: location2);
				Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
					palletID: "PLT3", client: data.Org1, part: part4, varianceQty: 1, partAttrib1: "AA",
					partAttrib2: "BB", partAttrib3: "CC", serialNumber: "Z" + i.ToString(), expiryDate: ZDate.Today,
					packingDate: ZDate.Today, expectedLocation: location2);
			}

			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive3.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 3 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 6 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 7 },
				{ WhsCycleCountLocationSchema.Constants.TableName, 1 },
				{ WhsCycleCountLocationVarianceSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 3 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 7 },
				{ WhsInventoryHoldChangeLogSchema.Constants.TableName, 7 },
				{ WhsRowSchema.Constants.TableName, 1 }, // Due to location update on hold code change
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 }, // due to assigning event time based on warehouse branch time zone/offset
				{
					GenAddOnColumnSchema.Constants.TableName, 7
				} // Temporary for defect fix, to be removed in the HCS project final WI
			};

			var cycleCountLocationService = new WhsCycleCountLocationService();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			{
				cycleCountLocationService.MarkInventoriesLostInCycleCountCore(newfactory, new[] { cycleCount.PK });
			}
		}

		#endregion

		#endregion
	}
}
