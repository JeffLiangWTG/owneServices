namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Services.OperationalActions.Support.Testing;
	using Enterprise.Warehouse.Environment.Business;
	using Enterprise.Warehouse.Environment.CodeLists;
	using Enterprise.Warehouse.Transactions.Business;
	using Enterprise.Warehouse.Transactions.Business.Common;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using Enterprise.Warehouse.Transactions.Business.Testing.Common;
	using Enterprise.Warehouse.Transactions.CodeLists;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(ForceReplenishmentToPickFaceActionMethodApplicator))]
	public class ForceReplenishmentToPickFaceActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestApplicator_CreatesTransfersForNormalPickfaceSituationsCorrectly

		public void TestApplicator_CreatesTransfersForNormalPickfaceSituationsCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-1-3");
			var bulkLocation = data.Whs1.FindLocation("A-1-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 500m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Now, data.Part1, 20m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Now, data.Part1, 80m, location3, "");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 10m, 250m); // 150
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2, 50m, 150m); // 130
			var pickFace3 = Helper.CreateProductPickFace(data.Part1, data.Org1, location3, 10m, 80m);  // 0
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			var expected = new[]
			{
				"INFO: Transfer [HL W00000005]: was created successfully.",
				"INFO: Pick Face: A-1-1 is to be replenished with 150 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-4.",
				"INFO: Transfer [HL W00000006]: was created successfully.",
				"INFO: Pick Face: A-1-2 is to be replenished with 130 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-4."
			};

			ApplyApplicatorLogOrderIsUnimportantIgnoreString(new[] { pickFaceView1, pickFaceView2 }, expected, "\n");

			var transferLine1 = Factory.LoadTop1<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_TransactionQuantity, 150));
			AssertNotNull(transferLine1);
			AssertEquals("Product", data.Part1.PK, transferLine1.WE_OP);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferLine1.WE_DocketLineStatus);
			AssertEquals("Inventory status", InventoryStatus.Codes.Available, transferLine1.WE_OriginalInventoryStatus);

			var transfer1 = transferLine1.Docket;
			AssertEquals("Precondition: Client", data.Org1.PK, transfer1.WD_OH_Client);
			AssertEquals("Precondition: Warehouse", data.Whs1.PK, transfer1.WD_WW_Whs);
			AssertEquals("Precondition: 1 Transfer lines", 1, transfer1.Lines.Count);

			var transferLine2 = Factory.LoadTop1<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_TransactionQuantity, 130));
			AssertEquals("Product", data.Part1.PK, transferLine2.WE_OP);
			AssertEquals("Qty", 130m, transferLine2.QtyToMoveIncludingMatchingLines);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferLine2.WE_DocketLineStatus);
			AssertEquals("Inventory status", InventoryStatus.Codes.Available, transferLine2.WE_OriginalInventoryStatus);

			var transfer2 = transferLine2.Docket;
			AssertEquals("Precondition: Client", data.Org1.PK, transfer2.WD_OH_Client);
			AssertEquals("Precondition: Warehouse", data.Whs1.PK, transfer2.WD_WW_Whs);
			AssertEquals("Precondition: 1 Transfer lines", 1, transfer2.Lines.Count);

			transfer1.FinaliseDocketWithoutUserConfirmation();
			Assert("Transfer1 is finalised.", transfer1.IsFinalised);
			transfer2.FinaliseDocketWithoutUserConfirmation();
			Assert("Transfer2 is finalised.", transfer2.IsFinalised);
			AssertNoExceptionThrown("Save successfully without exception thrown.", Factory.Save);
		}

		#endregion

		#region TestApplicator_ReplenishmentMultiple

		public void TestApplicator_ReplenishmentMultiple() => TestApplicator_ReplenishmentMultiple(lessThanMultipleInInventory: false, differentArrivalDates: false);
		public void TestApplicator_ReplenishmentMultiple_DifferentArrivalDates() => TestApplicator_ReplenishmentMultiple(lessThanMultipleInInventory: false, differentArrivalDates: true);
		public void TestApplicator_ReplenishmentMultiple_LessThanMultipleInInventory() => TestApplicator_ReplenishmentMultiple(lessThanMultipleInInventory: true, differentArrivalDates: false);

		void TestApplicator_ReplenishmentMultiple(bool lessThanMultipleInInventory, bool differentArrivalDates)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var pickFaceLocation = data.Whs1.FindLocation("A-1-1");
			var bulkLocation = data.Whs1.FindLocation("A-1-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 20m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-1) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 1m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-2) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 5m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-3) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 7m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-4) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, lessThanMultipleInInventory ? 6m : 7m, bulkLocation, "");

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 21m, replenishMultiple: 20m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			var expected =
@"WARNING: Pickface Location: A-1-1 for Client: 111 and Product: P1 does not have capacity for a single Replenish Multiple, therefore it cannot be replenished and has been ignored.
WARNING: Did not find any Pick Faces that needed replenishing.";

			ApplyApplicator(new[] { pickFaceView }, expected);

			var transferLine1 = Factory.LoadTop1<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
			AssertNull(transferLine1);
		}

		public void TestApplicator_ReplenishmentMultiple_Transfer() => TestApplicator_ReplenishmentMultiple_Transfer(lessThanMultipleInInventory: false, differentArrivalDates: false);
		public void TestApplicator_ReplenishmentMultiple_Transfer_DifferentArrivalDates() => TestApplicator_ReplenishmentMultiple_Transfer(lessThanMultipleInInventory: false, differentArrivalDates: true);
		public void TestApplicator_ReplenishmentMultiple_Transfer_LessThanMultipleInInventory() => TestApplicator_ReplenishmentMultiple_Transfer(lessThanMultipleInInventory: true, differentArrivalDates: false);

		void TestApplicator_ReplenishmentMultiple_Transfer(bool lessThanMultipleInInventory, bool differentArrivalDates)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var pickFaceLocation = data.Whs1.FindLocation("A-1-1");
			var bulkLocation = data.Whs1.FindLocation("A-1-4");
			var otherLoc = data.Whs1.FindLocation("A-1-3");
			var otherLoc2 = data.Whs1.FindLocation("A-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", ZDateTimeOffset.Now, data.Part1, 15m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", ZDateTimeOffset.Now, data.Part1, 15m, otherLoc2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 2m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-2) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 5m, otherLoc, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-3) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, 7m, otherLoc, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", differentArrivalDates ? ZDateTimeOffset.Now.AddDays(-4) : ZDateTimeOffset.Now.AddDays(-1), data.Part1, lessThanMultipleInInventory ? 6m : 7m, otherLoc, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, otherLoc, pickFaceLocation);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 7m, otherLoc, pickFaceLocation);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, lessThanMultipleInInventory ? 6m : 7m, otherLoc, pickFaceLocation);
			transfer.RunPreSaveValidation();

			transferLine1.PickLines.Single().WZ_GS_NKAssignedTo = "AAA";
			transferLine2.PickLines.Single().WZ_GS_NKAssignedTo = "AAA";
			transferLine3.PickLines.Single().WZ_GS_NKAssignedTo = "AAA";

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, 21m, replenishMultiple: 20m);
			Factory.Save();

			AssertEquals(lessThanMultipleInInventory ? 4 : 3, Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer)).Length);

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			var expected =
@"WARNING: Pickface Location: A-1-1 for Client: 111 and Product: P1 does not have capacity for a single Replenish Multiple, therefore it cannot be replenished and has been ignored.
WARNING: Did not find any Pick Faces that needed replenishing.";

			ApplyApplicator(new[] { pickFaceView }, expected);

			AssertEquals(lessThanMultipleInInventory ? 4 : 3, Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer)).Length);
		}

		public void TestApplicator_ReplenishmentMultiple_IncomingStock_Warning() => TestApplicator_ReplenishmentMultiple_IncomingStock(testIncoming: false);
		public void TestApplicator_ReplenishmentMultiple_IncomingStock() => TestApplicator_ReplenishmentMultiple_IncomingStock(testIncoming: true);

		void TestApplicator_ReplenishmentMultiple_IncomingStock(bool testIncoming)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var pickFaceLocation = data.Whs1.FindLocation("A-1-1");
			var bulkLocation = data.Whs1.FindLocation("A-1-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 20m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now.AddDays(-1), data.Part1, 1m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Now.AddDays(-2), data.Part1, 5m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Now.AddDays(-3), data.Part1, 7m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", ZDateTimeOffset.Now.AddDays(-4), data.Part1, 60m, bulkLocation, "");

			if (testIncoming)
			{
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				Helper.CreateWhsTransferLine(transfer, data.Part1, 40m, bulkLocation, pickFaceLocation);
				transfer.RunPreSaveValidation();
				AssertEquals("Should be correct transferLine count", 4, Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer)).Length);
			}

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 1m, testIncoming ? 61m : 21m, replenishMultiple: 20m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			if (testIncoming)
			{
				AssertEquals("Precondition: Incoming is 40", 40m, pickFaceView.WPV_Incoming);
			}

			var replenishWarning =
@"WARNING: Pickface Location: A-1-1 for Client: 111 and Product: P1 does not have capacity for a single Replenish Multiple, therefore it cannot be replenished and has been ignored.
WARNING: Did not find any Pick Faces that needed replenishing.";

			ApplyApplicator(new[] { pickFaceView }, replenishWarning);

			var transferLines = Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
			AssertEquals("Should be correct transferLine count", testIncoming ? 4 : 0, transferLines.Length);
		}

		#endregion

		#region TestApplicator_DeadlockedPickFace

		public void TestApplicator_DeadlockedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			var replenishFromLocation = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 20m, 40m, 5m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, pickfaceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, replenishFromLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertEquals("Receive is finalised", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 30m);
			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			AssertEquals("Precondition: Pick should be waiting for pick face to be replenished.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: 25 units allocated from the pick face.", 25m, pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			var expected = new[]
			{
				"INFO: Transfer [HL W00000003]: was created successfully.",
				"INFO: Pick Face: A-2 is to be replenished with 40 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1."
			};
			ApplyApplicatorLogOrderIsUnimportantIgnoreString(new[] { pickFaceView }, expected, "\n");
		}

		#endregion

		#region TestApplicator_UnAssignedPickFace

		public void TestApplicator_UnAssignedPickFaces()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location = data.Whs1.FindLocation("A-1-1");
			var fixedLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			location.WLV_WLT_LocationType = fixedLocationType.PK;
			Factory.Save();

			var pickFaceView = new WhsPickFaceViewCollection(Factory);
			AssertEquals("Precondition", fixedLocationType, location.LocationType);
			AssertEquals("Precondition: Pick Face view should have 1 record.", 1, pickFaceView.Count);
			var pickFaceView1 = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, location.PK));

			ApplyApplicator(new[] { pickFaceView1 },
				@"WARNING: Location: A-1-1 is not assigned to a Pick Face therefore it cannot be replenished and has been ignored.
WARNING: Did not find any Pick Faces that needed replenishing.");

			AssertNull(Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer)));
		}

		#endregion

		#region TestApplicator_WithHeldInventory

		public void TestApplicator_WithHeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var bulkLocation = data.Whs1.FindLocation("A-1-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 50m, bulkLocation, "");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 7m, location1, "");
			var receiveLine = receive.Lines.Single();
			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.ChangeInventoryHeldCode(true);

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 5m, 25m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView },
				@"INFO: Transfer [HL W00000003]: was created successfully.
INFO: Pick Face: A-1-1 is to be replenished with 23 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-2.");

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("1 Transfer line", 1, transfer.Lines.Count);

			var transferLine1 = transfer.Lines.Cast<WhsTransferLine>().Single();
			AssertEquals("Qty", 23m, transferLine1.QtyToMoveIncludingMatchingLines);
		}

		#endregion

		#region TestApplicator_NoStockOnHand

		public void TestApplicator_NoStockOnHand()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var bulklocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 10m, location1, "");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 5m, 25m);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);

			ApplyApplicator(new[] { pickFaceView1 },
				@"WARNING: No transfers have been generated to replenish any Pick Faces.
WARNING: Two possible reasons for this are the Pick Faces don't need replenishment or there is no stock available to transfer.");

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNull(transfer);

			//Add Stock
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, bulklocation, "");
			Factory.Save();

			ApplyApplicator(new[] { pickFaceView1 },
	@"INFO: Transfer [HL W00000003]: was created successfully.
INFO: Pick Face: A-1-1 is to be replenished with 15 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-2.");

			AssertNotNull(Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer)));
		}

		#endregion

		#region TestApplicator_MultiplePickFaces

		public void TestApplicator_MultiplePickFaces_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 1, 2);
			Factory.Save();

			var whs1location = data.Whs1.FindLocation("A-1-1");
			var whs1BulkLocation = data.Whs1.FindLocation("A-1-2");
			var whs2location = whs2.FindLocation("B-1-1");
			var whs2BulkLocation = whs2.FindLocation("B-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 10m, whs1location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, whs1BulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R3", ZDateTimeOffset.Now, data.Part1, 15m, whs2location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R4", ZDateTimeOffset.Now, data.Part1, 100m, whs2BulkLocation, "");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, whs1location, 5m, 25m);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, whs2location, 5m, 25m);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			var expected = new[]
			{
				"INFO: Transfer [HL W00000005]: was created successfully.",
				"INFO: Pick Face: A-1-1 is to be replenished with 15 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-2.",
				"INFO: Transfer [HL W00000006]: was created successfully.",
				"INFO: Pick Face: B-1-1 is to be replenished with 10 Product: P1 for Client: 111 from Warehouse: WHS2 Location: B-1-2."
			};

			ApplyApplicatorLogOrderIsUnimportantIgnoreString(new[] { pickFaceView1, pickFaceView2 }, expected, "\n");

			var transfer1 = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00000005"));
			AssertNotNull(transfer1);

			var transfer2 = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00000006"));
			AssertNotNull(transfer2);
		}

		public void TestApplicator_MultiplePickFaces_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var client2 = Helper.CreateClient("LIM");
			var location1 = data.Whs1.FindLocation("A-1-1");
			var bulkLocation = data.Whs1.FindLocation("A-1-2");
			var location2 = data.Whs1.FindLocation("A-1-3");
			var bulkLocation2 = data.Whs1.FindLocation("A-1-4");
			var part2 = Helper.CreateProduct(client2, "Stuff");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 7m, location1, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", ZDateTimeOffset.Now, part2, 12m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Now, data.Part1, 100m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R4", ZDateTimeOffset.Now, part2, 100m, bulkLocation2, "");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 5m, 25m);
			var pickFace2 = Helper.CreateProductPickFace(part2, client2, location2, 5m, 35m);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNull(transfer);

			var expected = new[]
			{
				"INFO: Transfer [HL W00000005]: was created successfully.",
				"INFO: Pick Face: A-1-1 is to be replenished with 18 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-2.",
				"INFO: Transfer [HL W00000006]: was created successfully.",
				"INFO: Pick Face: A-1-3 is to be replenished with 23 Product: STUFF for Client: LIM from Warehouse: 1 Location: A-1-4.",
			};

			ApplyApplicatorLogOrderIsUnimportantIgnoreString(new[] { pickFaceView1, pickFaceView2 }, expected, "\n");

			var checkTransfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNotNull(checkTransfer);
			AssertEquals("Should be only be two transfers found.", 2, checkTransfer.Length);
		}

		public void TestApplicator_MultiplePickFaces_DifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var bulkLocation1 = data.Whs1.FindLocation("A-1-2");
			var bulkLocation2 = data.Whs1.FindLocation("A-1-3");
			var location2 = data.Whs1.FindLocation("A-1-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 7m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part2, 12m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Now, data.Part1, 100m, bulkLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Now, data.Part2, 100m, bulkLocation2, "");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 5m, 25m);
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2, 5m, 35m);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNull(transfer);

			var expected = new[]
			{
				"INFO: Transfer [HL W00000005]: was created successfully.",
				"INFO: Pick Face: A-1-4 is to be replenished with 23 Product: P2 for Client: 111 from Warehouse: 1 Location: A-1-3.",
				"INFO: Transfer [HL W00000006]: was created successfully.",
				"INFO: Pick Face: A-1-1 is to be replenished with 18 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-2.",
			};

			ApplyApplicatorLogOrderIsUnimportantIgnoreString(new[] { pickFaceView1, pickFaceView2 }, expected, "\n");

			var transfers = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("Should be only one transfer found.", 2, transfers.Length);
		}

		#endregion

		#region TestApplicator_DifferentClients

		public void TestApplicator_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var bulklocation = data.Whs1.FindLocation("A-1-2");
			var client2 = Helper.CreateClient("LIM");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 5m, 25m);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);

			ApplyApplicator(new[] { pickFaceView1 }, @"WARNING: No transfers have been generated to replenish any Pick Faces.
WARNING: Two possible reasons for this are the Pick Faces don't need replenishment or there is no stock available to transfer.");

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNull(transfer);
		}

		#endregion

		#region TestApplicator_DifferentWarehouses

		public void TestApplicator_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WHS2", "B");
			Factory.Save();

			var location1 = data.Whs1.DefaultLocation;
			var location2 = whs2.DefaultLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 5m, 25m);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);

			ApplyApplicator(new[] { pickFaceView1 }, @"WARNING: No transfers have been generated to replenish any Pick Faces.
WARNING: Two possible reasons for this are the Pick Faces don't need replenishment or there is no stock available to transfer.");

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNull(transfer);
		}

		#endregion

		#region TestApplicator_InventoryIsCommitted

		public void TestApplicator_InventoryIsCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-1-3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 70m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, location1, location3);
			transfer.RunPreSaveValidation();

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1, 10m, 87m);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			AssertEquals("Precondition: Committed is 30.", 30m, pickFaceView1.WPV_Committed);

			ApplyApplicator(new[] { pickFaceView1 },
				@"INFO: Transfer [HL W00000004]: was created successfully.
INFO: Pick Face: A-1-1 is to be replenished with 47 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-2.");

			AssertNotNull(Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer)));
		}

		#endregion

		#region TestApplicator_Incoming

		public void TestApplicator_IncomingTransferInProcess()
		{
			TestApplicator_IncomingTransferlineCore(true);
		}

		public void TestApplicator_IncomingTransferlineNotStarted()
		{
			TestApplicator_IncomingTransferlineCore(false);
		}

		void TestApplicator_IncomingTransferlineCore(ZBool isStarted)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 20m, pickFaceLocation, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 45m, location1, pickFaceLocation);
			transfer.RunPreSaveValidation();
			if (isStarted)
			{
				transferLine1.PickLines.Single().WZ_GS_NKAssignedTo = "AAA";
			}

			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 15m, 70m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickface.PK);

			AssertEquals("Precondition: Incoming is 45", 45m, pickFaceView.WPV_Incoming);

			ApplyApplicator(new[] { pickFaceView },
				@"INFO: Transfer [HL W00000004]: was created successfully.
INFO: Pick Face: A-1-2 is to be replenished with 5 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-1.");

			AssertEquals("Only 2 transfers in db", 2, Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer)).Length);
		}

		#endregion

		#region TestApplicator_NoInventoriesInThePickFace

		public void TestApplicator_NoInventoriesInThePickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var pfLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pfLocation, 10m, 50m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView },
				@"INFO: Transfer [HL W00000002]: was created successfully.
INFO: Pick Face: A-1-2 is to be replenished with 50 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-1.");

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNotNull("Pick Face Replenishment Transfer", transfer);

			var transferLine = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_WL == pfLocation.PK);
			AssertEquals("TransferLine Qty is correct", 50m, transferLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestApplicator_WithUnFinalisedTransferOut

		public void TestApplicator_WithUnFinalisedTransferOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var pfLocation = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var storageLocation = data.Whs1.FindLocation("A-1-3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 40m, pfLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, storageLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var picker = Helper.CreateGlbStaff("AAA", "StaffA");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, pfLocation, location2, picker);
			transferLine.PickedTime = ZDateTimeOffset.Today;
			transfer.RunPreSaveValidation();
			Assert("Precondition: Transfer line is not finalised.", !transferLine.IsFinalised);
			AssertEquals("Precondition: Transfer line status is HFT.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pfLocation, 5m, 75m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView },
@"INFO: Transfer [HL W00000004]: was created successfully.
INFO: Pick Face: A-1-1 is to be replenished with 65 Product: P1 for Client: 111 from Warehouse: 1 Location: A-1-3.");

			var checkTransfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00000004"));
			AssertNotNull("Pick Face Replenishment Transfer", checkTransfer);

			var checkTransferLine = checkTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_WL == pfLocation.PK);
			AssertEquals("checkTransferLine Qty is correct", 65m, checkTransferLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestApplicator_TransferHasErrors

		public void TestApplicator_TransferHasErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var pfLocation = data.Whs1.FindLocation("A-1-2");
			var locationbulk = data.Whs1.FindLocation("A-1-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, locationbulk, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 70m, pfLocation, "");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, pfLocation, 50m, 200m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			var pickFaceCreationForTest = new PickFaceCreateTransfersTesting();
			pickFaceCreationForTest.RunActionBeforeTransferValidation += (transfer) =>
			{
				transfer.AddRowError("Only For Testing 1");
				transfer.AddRowError("Only For Testing 2");
				transfer.AddRowError("Only For Testing Last One");
			};

			using (ObjectFactory.Substitute<IPickFaceCreateTransfers>(pickFaceCreationForTest))
			{
				ApplyApplicator(new[] { pickFaceView },
	@"WARNING: Creating a transfer for Client: 111, Warehouse: 1, Product: P1, Pick-face location A-1-2 failed.
Validation errors:
Error - Warehouse Transfer: Only For Testing 1
Error - Warehouse Transfer: Only For Testing 2
Error - Warehouse Transfer: Only For Testing Last One");
			}

			var checkTransfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNull("Pick Face Replenishment Transfer", checkTransfer);
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
