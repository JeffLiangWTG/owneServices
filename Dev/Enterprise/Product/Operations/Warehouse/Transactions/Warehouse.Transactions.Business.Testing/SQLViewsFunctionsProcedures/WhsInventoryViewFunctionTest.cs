using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryViewFunctionTest : WhsTestCaseWithFactory
	{
		#region TestView_WhsInventoryView_InDocketLineUnits

		public void TestView_WhsInventoryView_InDocketLineUnits_ReceiveLineReturnsTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var arrivalDate = new ZDateTimeOffset(DateTime.Now.Year - 1, 1, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1",
				   arrivalDate, data.Part1, 100m);
			Factory.Save();

			var receiveDynamicBizO = new DynamicBusinessObjectCollection(Factory);
			receiveDynamicBizO.Load($"SELECT * FROM dbo.WhsInventoryView WHERE [WI_InDocketLineType] = '{DocketType.Codes.Receive}'");

			AssertEquals("One receive line should be found.", 1, receiveDynamicBizO.Count);
			AssertEquals("Original receive line should have WE_TransactionQuantity.",
				receive.Lines[0].WE_TransactionQuantity,
				receiveDynamicBizO[0]["WI_InDocketLineUnits"]
			);

			AssertExhaustiveMatch(receive.Lines[0], receiveDynamicBizO[0]);
		}

		public void TestView_WhsInventoryView_InDocketLineUnits_TransferLineReturnsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location_A_1_1 = data.Whs1.FindLocation("A-1-1");
			var location_A_1_2 = data.Whs1.FindLocation("A-1-2");

			var arrivalDate = new ZDateTimeOffset(DateTime.Now.Year - 1, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1",
				arrivalDate, data.Part1, 100m, location_A_1_1, "Pallet_11");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50M,
				location_A_1_1.ToLocationString(), "Pallet_11", location_A_1_2.ToLocationString(), "Pallet_12");

			transferLine.RunPreSaveValidation();
			transferLine.FinaliseDocketLine();

			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();
			var internalTransferDynamicBizO = new DynamicBusinessObjectCollection(Factory);
			internalTransferDynamicBizO.Load($"SELECT * FROM dbo.WhsInventoryView WHERE [WI_InDocketLineType] = '{DocketType.Codes.Transfer}'");

			AssertEquals("One transfer line should be found.", 1, internalTransferDynamicBizO.Count);
			AssertEquals("Transfer line should have 0 WI_InDocketLineUnits.",
				ZDecimal.Zero,
				internalTransferDynamicBizO[0]["WI_InDocketLineUnits"]
			);

			AssertExhaustiveMatch(transferLine, internalTransferDynamicBizO[0]);
		}

		public void TestView_WhsInventoryView_InDocketLineUnits_AdjustmentLineReturnsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 25M,
				data.Whs1.FindLocation("A"));

			adjustmentLine.RunPreSaveValidation();
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustmentLine);
			Factory.Save();

			var adjustmentDynamicBizO = new DynamicBusinessObjectCollection(Factory);
			adjustmentDynamicBizO.Load($"SELECT * FROM dbo.WhsInventoryView WHERE [WI_InDocketLineType] = '{DocketType.Codes.Adjustment}'");

			AssertEquals("One adjustment line should be found.", 1, adjustmentDynamicBizO.Count);
			AssertEquals("Adjustment line should have 0 WI_InDocketLineUnits",
				ZDecimal.Zero,
				adjustmentDynamicBizO[0]["WI_InDocketLineUnits"]
			);

			AssertExhaustiveMatch(adjustmentLine, adjustmentDynamicBizO[0]);
		}

		public void TestView_WhsInventoryView_InDocketLineUnits_HoldCodeChangedReceiveLineReturnsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var arrivalDate = new ZDateTimeOffset(DateTime.Now.Year - 1, 1, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1",
				   arrivalDate, data.Part1, 100m);

			var receiveLine = receive.Lines[0];
			receiveLine.IsInventoryEditForm = true;
			receiveLine.HeldCodeChangeQuantity = 25M;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			Factory.Save();

			var splitLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, false));

			var originalReceiveDynamicBizO = new DynamicBusinessObjectCollection(Factory);
			originalReceiveDynamicBizO.Load(
				  $"SELECT * FROM dbo.WhsInventoryView WHERE [WI_InDocketLineType] = '{DocketType.Codes.Receive}' AND [WI_IsOriginalReceiptLine] = 1");

			AssertEquals("One original receive line should be found", 1, originalReceiveDynamicBizO.Count);
			AssertEquals("Original receive has 75 on hand", 75M, originalReceiveDynamicBizO[0]["WI_TotalUnits"]);
			AssertEquals("Original receive has WE_TransactionQuantity as InDocketLineUnits.", receiveLine.WE_TransactionQuantity, originalReceiveDynamicBizO[0]["WI_InDocketLineUnits"]);

			AssertExhaustiveMatch(receiveLine, originalReceiveDynamicBizO[0]);

			var splitReceiveDynamicBizO = new DynamicBusinessObjectCollection(Factory);
			splitReceiveDynamicBizO.Load(
				  $"SELECT * FROM dbo.WhsInventoryView WHERE [WI_InDocketLineType] = '{DocketType.Codes.Receive}' AND [WI_IsOriginalReceiptLine] = 0");

			AssertEquals("One split receive line should be found.", 1, splitReceiveDynamicBizO.Count);
			AssertEquals("The split receive has 25 on hand.", 25M, splitReceiveDynamicBizO[0]["WI_TotalUnits"]);
			AssertEquals("Split receive should have 0 InDocketLineUnits.", ZDecimal.Zero, splitReceiveDynamicBizO[0]["WI_InDocketLineUnits"]);

			AssertExhaustiveMatch(splitLine, splitReceiveDynamicBizO[0]);
		}

		#endregion

		#region TestView_WhsInventoryView_WHERE_Clause

		public void TestView_WhsInventoryView_ShouldIncludeFinalisedOrUnfinalisedReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA = data.Whs1.FindLocation("A");
			var arrivalDate = new ZDateTimeOffset(DateTime.Now.Year - 1, 1, 1);

			var receiveUnfinalised = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW-UNFIN", arrivalDate,
				data.Part1, 0, locationA, "", allocateLocations: true, finalise: false);
			var receiveFinalised = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW-FIN", arrivalDate,
				data.Part1, 0, locationA, "", allocateLocations: true, finalise: true);

			Factory.Save();

			var collection = LoadInventoryLineForTest();

			AssertContainsExactElementsInAnyOrder("Precondition: No WI with StockOnHand greater than zero", Array.Empty<string>(),
				collection.Where(x => x.WI_TotalUnits > 0).Select(x => $"{x.WD_ExternalReference}.StockOnHand = {x.WI_TotalUnits}"));

			AssertContainsExactElementsInAnyOrder("All receives should return", new string[] { "INW-FIN", "INW-UNFIN" },
				collection.Select(x => x.WD_ExternalReference));
		}

		public void TestView_WhsInventoryView_ShouldIncludeFinalisedInternalTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var arrivalDate = new ZDateTimeOffset(DateTime.Now.Year - 1, 1, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", arrivalDate,
				data.Part1, 2, locationA1, "", allocateLocations: true, finalise: true);
			Factory.Save();

			var transferUnfinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR-UNFIN");
			Helper.CreateWhsTransferLine(transferUnfinalised, data.Part1, 1, locationA1, locationA2);
			transferUnfinalised.RunPreSaveValidation();

			var transferFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR-FIN");
			Helper.CreateWhsTransferLine(transferFinalised, data.Part1, 1, locationA1, locationA2);
			transferFinalised.RunPreSaveValidation();
			transferFinalised.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transferFinalised);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-OUT");
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1, locationA2);
			adjustment.RunPreSaveValidation();
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var collection = LoadInventoryLineForTest().Where(x => (ZString)x["WI_InDocketLineType"] == DocketType.Codes.Transfer).ToArray();

			AssertContainsExactElementsInAnyOrder("Precondition: No WI with StockOnHand greater than zero", Array.Empty<string>(),
				collection.Where(x => x.WI_TotalUnits > 0).Select(x => $"{x.WD_ExternalReference}.StockOnHand = {x.WI_TotalUnits}"));

			AssertContainsExactElementsInAnyOrder("Only the finalised transfer should return", new string[] { "TFR-FIN" },
				collection.Select(x => x.WD_ExternalReference));
		}

		public void TestView_WhsInventoryView_ShouldIncludeFinalisedIWDTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationDest = data.Whs1.FindLocation("A");

			var whsSource = Helper.CreateWarehouse("whs2", "B", 1, 1);
			Factory.Save();

			var locationSource = whsSource.FindLocation("B");

			var arrivalDate = new ZDateTimeOffset(DateTime.Now.Year - 1, 1, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, whsSource, "R1", arrivalDate,
				data.Part1, 2, locationSource, "", allocateLocations: true, finalise: true);
			Factory.Save();

			var transferUnfinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "IWD-UNFIN", transferType: TransferType.Codes.InterWhsDest);
			Helper.CreateWhsTransferLine(transferUnfinalised, data.Part1, 1, "B", whsSource.PK, "A");
			transferUnfinalised.RunPreSaveValidation();

			var transferFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "IWD-FIN", transferType: TransferType.Codes.InterWhsDest);
			Helper.CreateWhsTransferLine(transferFinalised, data.Part1, 1, "B", whsSource.PK, "A");
			transferFinalised.RunPreSaveValidation();
			transferFinalised.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transferFinalised);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-OUT");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1, locationDest);
			adjustment.RunPreSaveValidation();
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var collection = LoadInventoryLineForTest()
				.Where(x => (ZString)x["WI_InDocketLineType"] == DocketType.Codes.Transfer).ToArray();

			AssertContainsExactElementsInAnyOrder("Precondition: No WI with StockOnHand greater than zero", Array.Empty<string>(),
				collection.Where(x => x.WI_TotalUnits > 0).Select(x => $"{x.WD_ExternalReference}.StockOnHand = {x.WI_TotalUnits}"));

			AssertContainsExactElementsInAnyOrder("Only the finalised transfer should return", new string[] { "IWD-FIN" },
				collection.Select(x => x.WD_ExternalReference));
		}

		public void TestView_WhsInventoryView_ShouldExcludeFinalisedIWSTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA = data.Whs1.FindLocation("A");

			var whsDest = Helper.CreateWarehouse("whs2", "B", 1, 1);
			Factory.Save();

			var locationDest = whsDest.FindLocation("B");

			var arrivalDate = new ZDateTimeOffset(DateTime.Now.Year - 1, 1, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", arrivalDate,
				data.Part1, 2, locationA, "", allocateLocations: true, finalise: true);
			Factory.Save();

			var transferFinalised = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "IWS-FROM-Whs1", transferType: TransferType.Codes.InterWhsSource);
			Helper.CreateWhsTransferLine(transferFinalised, data.Part1, 1, "A", whsDest.PK, "B");
			transferFinalised.RunPreSaveValidation();
			transferFinalised.FinaliseDocketWithoutUserConfirmation();
			transferFinalised.ChildTransfers.First().WD_ExternalReference = "IWD-TO-Whs2";
			AssertIsFinalisedPrecondition(transferFinalised);
			Factory.Save();

			var collection = LoadInventoryLineForTest()
				.Where(x => (ZString)x["WI_InDocketLineType"] == DocketType.Codes.Transfer)
				.Where(x => (ZGuid)x["WI_WW_Whs"] == data.Whs1.PK).ToArray();

			AssertContainsExactElementsInAnyOrder("Precondition: No WI with StockOnHand greater than zero", Array.Empty<string>(),
				collection.Where(x => x.WI_TotalUnits > 0).Select(x => $"{x.WD_ExternalReference}.StockOnHand = {x.WI_TotalUnits}"));

			AssertContainsExactElementsInAnyOrder("IWS-FIN should not return", Array.Empty<string>(),
				collection.Select(x => x.WD_ExternalReference));
		}

		public void TestView_WhsInventoryView_ShouldIncludeFinalisedAdjustmentInLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA = data.Whs1.FindLocation("A");
			var docketDate = new ZDate(DateTime.Now.Year, 1, 1);

			var adjustmentInUnfinalised = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-IN-UNFIN");
			var adjustmentInUnfinalisedLine = Helper.CreateWhsAdjustmentLine(adjustmentInUnfinalised, data.Part1, 1M, locationA);

			var adjustmentInFinalised = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-IN-FIN");
			var adjustmentInFinalisedLine = Helper.CreateWhsAdjustmentLine(adjustmentInFinalised, data.Part1, 1M, locationA);
			adjustmentInFinalised.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentInFinalised);
			Factory.Save();

			var adjustmentOutUnfinalised = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-OUT-UNFIN");
			var adjustmentOutUnfinalisedLine = Helper.CreateWhsAdjustmentLine(adjustmentOutUnfinalised, data.Part1, 1M, locationA);
			adjustmentOutUnfinalised.RunPreSaveValidation();

			var adjustmentOutFinalised = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-OUT-FIN");
			var adjustmentOutFinalisedLine = Helper.CreateWhsAdjustmentLine(adjustmentOutFinalised, data.Part1, -1M, locationA);
			adjustmentOutFinalised.RunPreSaveValidation();

			adjustmentOutFinalised.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentOutFinalised);
			Factory.Save();

			var collection = LoadInventoryLineForTest();

			AssertContainsExactElementsInAnyOrder("Precondition: No WI with StockOnHand greater than zero", Array.Empty<string>(),
				collection.Where(x => x.WI_TotalUnits > 0).Select(x => $"{x.WD_ExternalReference}.StockOnHand = {x.WI_TotalUnits}"));

			AssertContainsExactElementsInAnyOrder("Only ADJ-IN-FIN should return", new string[] { "ADJ-IN-FIN" },
				collection.Select(x => x.WD_ExternalReference));
		}

		#endregion

		#region TestView_WhsInventoryView_All_Fields
		public void TestView_WhsInventoryView_All_Fields()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true);
			Helper.SetClientAllAttributeType(data.Org1, mandatoryAttributeType: true);
			var location = data.Whs1.FindLocation("A");

			var arrivalDate = ZDateTimeOffset.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Receive1", arrivalDate);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1M, location.PK, "PALLET-999", "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);

			receiveLine.WE_AllocationKey = "ALLOC-999";
			receiveLine.WE_PartAttrib1 = "HOT";
			receiveLine.WE_PartAttrib2 = "HARD";
			receiveLine.WE_PartAttrib3 = "ROCKY";
			receiveLine.WE_F3_NKPackType = "CTN";
			receiveLine.WE_ClientOrderedUnits = 1M;
			receiveLine.WE_SerialNumber = "SN-999";
			var packingDate = new ZDate(DateTime.Now.Year, 1, 1);
			receiveLine.WE_PackingDate = packingDate;
			var expiryDate = new ZDate(DateTime.Now.Year, 12, 31);
			receiveLine.WE_ExpiryDate = expiryDate;

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Hacking the entry key assignment to avoid setting up bonded warehouse
			// Do not put this before finalising - will break validation
			receiveLine.WE_BondedEntryKey = "ENTRY-999";

			Factory.Save();

			var collection = LoadDataFromView();
			AssertEquals("One inventory entered, so 1 records should be found.", 1, collection.Count);

			var row = collection[0];
			var matcher = new ExhaustiveMatcher(row);

			// PK & FKs
			matcher.AddColumnMatcher("WI_PK", receiveLine.PK);
			matcher.AddColumnMatcher("IsOriginal should be true for a Receive", "WI_IsOriginalReceiptLine", true);
			matcher.AddColumnMatcher("WI_WE_OriginalInDocketLineForRating", receiveLine.PK);
			matcher.AddColumnMatcher("WI_WE_InDocketLine", receiveLine.PK);
			matcher.AddColumnMatcher("WI_WW_Whs", data.Whs1.PK);
			matcher.AddColumnMatcher("WI_WL", data.Whs1.FindLocation("A").PK);
			matcher.AddColumnMatcher("WI_OH_Client", data.Org1.PK);
			matcher.AddColumnMatcher("WI_WD", receive.PK);
			matcher.AddColumnMatcher("WI_OP", data.Part1.PK);
			// Values
			matcher.AddColumnMatcher("WI_IsValid, Light validation, should be false for a receive", "WI_IsValid", false);
			matcher.AddColumnMatcher("WI_TotalUnits", 1M);
			matcher.AddColumnMatcher("WI_PackingDate", packingDate);
			matcher.AddColumnMatcher("WI_ArrivalDate", arrivalDate);
			matcher.AddColumnMatcher("WI_ExpiryDate", expiryDate);
			matcher.AddColumnMatcher("WI_InventoryStatus", InventoryStatus.Codes.Held);
			matcher.AddColumnMatcher("WI_HeldCode", InventoryHoldCodes.Codes.Damaged);
			matcher.AddColumnMatcher("WI_PartAttrib1", "HOT");
			matcher.AddColumnMatcher("WI_PartAttrib2", "HARD");
			matcher.AddColumnMatcher("WI_PartAttrib3", "ROCKY");
			matcher.AddColumnMatcher("WI_SerialNumber", "SN-999");
			matcher.AddColumnMatcher("WI_F3_NKPackType", "CTN");
			matcher.AddColumnMatcher("WI_BondedEntryKey", "ENTRY-999");
			matcher.AddColumnMatcher("WI_AllocationKey", "ALLOC-999");
			matcher.AddColumnMatcher("WI_PalletID", "PALLET-999");
			matcher.AddColumnMatcher("WI_InDocketLineType", DocketType.Codes.Receive);
			matcher.AddColumnMatcher("WI_InDocketLineUnits", 1M);

			matcher.AssertAll();
		}

		#endregion

		#region TestView_WhsInventoryView_AllocationKey

		[TestDate(2023, 6, 1)]
		public void TestView_WhsInventoryView_AllocationKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2023, 1, 5),
				data.Part1, 100m, data.Whs1.FindLocation("A"), "");
			var inventory = receive.Inventory[0];
			inventory.InDocketLine.WE_AllocationKey = "ABC123";
			Factory.Save();

			var results = LoadDataFromView();
			AssertEquals("One iventory entered, so 1 records should be found.", 1, results.Count);
			AssertExhaustiveMatch(inventory.InDocketLine, results[0]);
		}

		#endregion

		#region TestView_WhsInventoryView_WithPutawayTransfers

		public void TestView_WhsInventoryView_WithPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receiveWithDockDoor =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 5));
			var inventory = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory.InDocketLine.WE_PalletID = "12345";
			inventory.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;

			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "12345", 10m);
			putawayTransferLine.RunPreSaveValidation();
			putawayTransferLine.FinaliseDocketLine();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var putawayTransferFromNewFactory = newFactory.Load<WhsTransferLine>(putawayTransferLine.PK);

			AssertEquals("Precondition: There is a Put away transfer line.", true,
				putawayTransferFromNewFactory.Docket.WD_IsPutawayTransfer);
			var results = LoadDataFromView();
			AssertEquals("WhsInverntoryView should both receive line and putaway transfer line.", 2, results.Count);

			var receiveLineInv = results.Where(r => (ZGuid)r["WI_PK"] == inventory.PK).Single();
			var putawayTransferLineInv = results.Where(r => (ZGuid)r["WI_PK"] == putawayTransferLine.PK).Single();
			AssertExhaustiveMatch(inventory.InDocketLine, receiveLineInv);
			AssertExhaustiveMatch(putawayTransferLine, putawayTransferLineInv);
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObjectCollection LoadDataFromView()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"SELECT * FROM dbo.WhsInventoryView";
			result.Load(sql);

			return result;
		}

		class InventoryLineForTest : DynamicBusinessObject
		{
			public InventoryLineForTest(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public string WD_ExternalReference => ((ZString)this[nameof(WD_ExternalReference)]).Split(' ')[0];

			public decimal WI_TotalUnits => (ZDecimal)this[WhsInventoryViewSchema.WI_TotalUnits];
		}

		DynamicBusinessObjectCollection<InventoryLineForTest> LoadInventoryLineForTest()
		{
			var result = new DynamicBusinessObjectCollection<InventoryLineForTest>(Factory);
			var sql = @"SELECT WI.*, WD.WD_ExternalReference FROM dbo.WhsInventoryView WI INNER JOIN dbo.WhsDocket WD ON WI.WI_WD = WD.WD_PK";
			result.Load(sql);

			return result;
		}

		#endregion

		#region AssertInventoryMatch

		void AssertExhaustiveMatch(WhsDocketLine docketLine, DynamicBusinessObject resultRow)
		{
			var matcher = new ExhaustiveMatcher(resultRow);
			var docket = docketLine.Docket;

			matcher.AddColumnMatcher("WI_PK", docketLine.PK);
			matcher.AddColumnMatcher("WI_IsValid", docketLine.LightValidationEnabled && docket.LightValidationIsValid);
			matcher.AddColumnMatcher("WI_WW_Whs", docketLine.WarehousePK);
			matcher.AddColumnMatcher("WI_IsOriginalReceiptLine", docketLine.WE_IsOriginalInventory);
			matcher.AddColumnMatcher("WI_WL", docketLine.WE_WL);
			matcher.AddColumnMatcher("WI_OP", docketLine.WE_OP);
			matcher.AddColumnMatcher("WI_TotalUnits", docketLine.WE_StockOnHand);
			matcher.AddColumnMatcher("WI_PackingDate", docketLine.WE_PackingDate);
			matcher.AddColumnMatcher("WI_ExpiryDate", docketLine.WE_ExpiryDate);
			matcher.AddColumnMatcher("WI_ArrivalDate", docketLine.WE_AdjustmentArrivalDate);
			matcher.AddColumnMatcher("WI_InventoryStatus", docketLine.WE_CurrentInventoryStatus);
			matcher.AddColumnMatcher("WI_HeldCode", docketLine.WE_WHC_NKCurrentInventoryHeldCode);
			matcher.AddColumnMatcher("WI_PartAttrib1", docketLine.WE_PartAttrib1);
			matcher.AddColumnMatcher("WI_PartAttrib2", docketLine.WE_PartAttrib2);
			matcher.AddColumnMatcher("WI_PartAttrib3", docketLine.WE_PartAttrib3);
			matcher.AddColumnMatcher("WI_SerialNumber", docketLine.WE_SerialNumber);
			matcher.AddColumnMatcher("WI_BondedEntryKey", docketLine.WE_BondedEntryKey);
			matcher.AddColumnMatcher("WI_AllocationKey", docketLine.WE_AllocationKey);
			matcher.AddColumnMatcher("WI_PalletID", docketLine.WE_PalletID);
			matcher.AddColumnMatcher("WI_WE_InDocketLine", docketLine.PK);
			matcher.AddColumnMatcher("WI_WE_OriginalInDocketLineForRating", docketLine.WE_WE_OriginalDocketLineForRating);
			matcher.AddColumnMatcher("WI_InDocketLineType", docket.WD_DocketType);

			if (docketLine.WE_DocketLineType == DocketType.Codes.Receive && docketLine.WE_IsOriginalInventory)
			{
				// This WI_InDocketLineUnits only matters on original receives
				matcher.AddColumnMatcher("WI_InDocketLineUnits", docketLine.WE_TransactionQuantity);
			}
			else
			{
				matcher.AddColumnMatcher("WI_InDocketLineUnits of TFR and ADJ is zero", "WI_InDocketLineUnits", ZDecimal.Zero);
			}

			matcher.AddColumnMatcher("WI_F3_NKPackType", docketLine.WE_F3_NKPackType);
			matcher.AddColumnMatcher("WI_OH_Client", docket.WD_OH_Client);
			matcher.AddColumnMatcher("WI_WD", docketLine.WE_WD);

			matcher.AssertAll();
		}

		#endregion
	}
}
