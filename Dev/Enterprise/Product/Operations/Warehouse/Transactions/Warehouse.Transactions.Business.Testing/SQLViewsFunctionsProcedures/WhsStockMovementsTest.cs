using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStockMovementsTest : WhsTestCaseWithFactory
	{
		#region TestView_Adjustment

		[TestDate(2022, 02, 01)]
		public void TestView_Adjustment_NoBondedEntryKey()
		{
			TestView_AdjustmentCore(isWithBondedEntryKey: false);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_Adjustment_WithBondedEntryKey()
		{
			TestView_AdjustmentCore(isWithBondedEntryKey: true);
		}

		void TestView_AdjustmentCore(bool isWithBondedEntryKey)
		{
			var lastYear = ZDateTime.Today.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			if (isWithBondedEntryKey)
			{
				var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
				data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
				data.Whs1.DefaultLocation.WLV_WA_PutawayArea = bondedArea.PK;
			}

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "111", Helper.Notify);
			if (isWithBondedEntryKey)
			{
				receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			}

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 111m, data.Whs1.DefaultLocation, "PLT: 1-111",
				new ZDate(lastYear, 1, 1), new ZDate(lastYear, 2, 1), "PA1", "PA2", "PA3",
				isWithBondedEntryKey ? "BEK" : "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 121m, data.Whs1.DefaultLocation, "PLT: 2-111",
				new ZDate(lastYear, 1, 1), new ZDate(lastYear, 2, 1), "PA1", "PA2", "PA3",
				isWithBondedEntryKey ? "BEK" : "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 22m, data.Whs1.DefaultLocation, "PLT: 1-111",
				new ZDate(lastYear, 1, 1), new ZDate(lastYear, 2, 1), "PA1", "PA2", "PA3",
				isWithBondedEntryKey ? "BEK" : "");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			if (isWithBondedEntryKey)
			{
				adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			}

			var unit1 = -11m;
			var unit2 = 12m;
			var palletID1 = "PLT: 1-111";
			var palletID2 = "PLT: 2-";
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, unit1,
				data.Whs1.DefaultLocation.ToLocationString(), palletID1);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, unit2,
				data.Whs1.DefaultLocation.ToLocationString(), palletID2);
			Helper.SetDocketLineAttributes(adjustmentLine1, new ZDate(lastYear, 1, 1), new ZDate(lastYear, 2, 1), "PA1",
				"PA2", "PA3", "", isWithBondedEntryKey ? "BEK" : "");
			Helper.SetDocketLineAttributes(adjustmentLine2, new ZDate(lastYear, 1, 1), new ZDate(lastYear, 2, 1), "PA1",
				"PA2", "PA3", "", isWithBondedEntryKey ? "BEK" : "");

			adjustment.FinaliseDocket();
			AssertEquals("Adjust IsFinalised", true, adjustment.IsFinalised);
			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals(results1.Count, 2);
			AssertResult(adjustmentLine1, results1[0], unit1, CalculatePalletSpace(unit1, 5));

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2);
			AssertEquals(results2.Count, 2);
			AssertResult(adjustmentLine2, results2[0], unit2, 0m);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var cancelAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "112");
			Helper.CreateWhsAdjustmentLine(cancelAdjustment, data.Part2, unit2, "A-1", palletID2);

			Factory.Save();

			cancelAdjustment.CancelReactivateDocket();
			AssertEquals(true, cancelAdjustment.IsCancelled);

			var results3 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(results3.Count, 0);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_Adjustment_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, location);
			adjustmentLine.WE_SerialNumber = "SN1";
			adjustment.FinaliseDocket();
			AssertEquals("Adjust IsFinalised", true, adjustment.IsFinalised);
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals("ResultSet Count", 4, results.Count);
			AssertResult(adjustmentLine, results[0], -1m, 0m);
			AssertResult(receiveLine1, results[1], 1m, 0m);
			AssertResult(receiveLine2, results[2], 1m, 0m);
			AssertResult(receiveLine3, results[3], 1m, 0m);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_InnerTransfer

		[TestDate(2022, 02, 01)]
		public void TestView_InnerTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var today = ZDateTimeOffset.Today;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m,
				data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "111", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 11m, "A-1", "", "A-2", "");
			Helper.SetDocketLineAttributes(transferLine, new ZDate(today.Year - 1, 1, 1),
				new ZDate(today.Year - 1, 2, 1), "PA1", "PA2", "PA3", "", "BEK");

			var pickLine1 = Helper.CreateWhsPickLine(transferLine, inventory, 7m);
			var pickLine2 = Helper.CreateWhsPickLine(transferLine, inventory, 4m);
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-3);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-2);
			Factory.Save();

			//we need to manually finalise because we cannot normally setup multiple picklines on a single transfer line
			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_GS_NKPutawayBy = "ABC";
				transferLine.WE_PutawayTime = today.AddDays(-1);
				transferLine.WE_FinalisedDate = today.AddDays(-1);
				transferLine.WE_DocketLineStatus = "FIN";
				transferLine.WE_StockOnHand = 11m;
				transferLine.WE_AdjustmentArrivalDate = today;
			}

			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(results.Length, 3);
			AssertResult(transfer.Lines[0], results[0], -11m, CalculatePalletSpace(-11m, 5),
				locationOverride: data.Whs1.FindLocation("A-1"), finalisedDateOverride: today.AddDays(-2));
			AssertResult(transfer.Lines[0], results[1], 11m, CalculatePalletSpace(11m, 5),
				locationOverride: data.Whs1.FindLocation("A-2"));

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 11m,
				data.Whs1.FindLocation("A-1"), "PLT: 1-111");

			var notFinalisedTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "112");
			var transferLine3 = Helper.CreateWhsTransferLine(notFinalisedTransfer, part3, 11m, "A-1", "PLT: 1-111",
				"A-1", "PLT: 1-112");
			transferLine3.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var results3 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(results3.Count, 1);
			AssertResult(receive3.Lines[0], results3[0], 11m, 0m, palletIdOverride: "PLT: 1-111",
				locationOverride: data.Whs1.FindLocation("A-1"));
		}

		#endregion

		#region TestView_InnerTransfer_SerialNumber

		[TestDate(2022, 02, 01)]
		public void TestView_InnerTransfer_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "111", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "", "A-2", "");
			transferLine.WE_SerialNumber = "SN1";
			transfer.FinaliseDocket();
			AssertEquals("transfer IsFinalised", true, transfer.IsFinalised);
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals("ResultSet Count", 5, results.Count);
			AssertResult(receiveLine1, results[0], 1m, 0m);
			AssertResult(receiveLine2, results[1], 1m, 0m);
			AssertResult(receiveLine3, results[2], 1m, 0m);
			AssertResult(transferLine, results[3], -1m, 0m, locationOverride: location);
			AssertResult(transferLine, results[4], 1m, 0m);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_InnerTransfer_Unfinalised

		[TestDate(2022, 02, 01)]
		public void TestView_InnerTransfer_Unfinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var location = data.Whs1.FindLocation("A-1");
			var today = ZDateTimeOffset.Today;

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location, "");
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "111", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 11m, "A-1", "", "A-2", "");
			var pickLine1 = Helper.CreateWhsPickLine(transferLine, inventory, 7m);
			var pickLine2 = Helper.CreateWhsPickLine(transferLine, inventory, 4m);
			pickLine1.WZ_PickedDateTime = today.AddDays(-2);
			pickLine2.WZ_PickedDateTime = today.AddDays(-1);

			Assert("Precondition - Inter transfer is not finalised", !transfer.IsFinalised);
			Assert("Precondition - Inter transfer line is picked", transferLine.IsPicked);
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals("Destination line with QuantityActual = 11 should not appear because it is not finalised.", 2,
				results.Length);
			AssertResult(transfer.Lines[0], results[0], -11m, CalculatePalletSpace(-11m, 5), locationOverride: location,
				finalisedDateOverride: today.AddDays(-1));
			AssertResult(receive.Lines[0], results[1], 20m, CalculatePalletSpace(20m, 5));
		}

		#endregion

		#region TestView_InterTransferSource

		[TestDate(2022, 02, 01)]
		public void TestView_InterTransferSource()
		{
			var data = new TestEnviromentForStockMovement(Factory);
			Factory.Save();

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "111", Notify);
			interTransfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var interTransferLine1 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 11m, "A-1-1", data.Whs2.PK, "A-1-1");
			interTransferLine1.WE_TransferFromPalletId = "PLT: 1-111";
			interTransferLine1.WE_PalletID = "PLT: 1-112";
			Helper.SetDocketLineAttributes(interTransferLine1, data.DayOne, data.DayTwo, "PA1", "PA2", "PA3", "",
				"BEK");

			var interTransferLine2 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part2, 20m, "A-1-2", data.Whs2.PK, "A-1-2");
			interTransferLine2.WE_TransferFromPalletId = "PLT: 2-111";
			interTransferLine2.WE_PalletID = "PLT: 2-112";
			Helper.SetDocketLineAttributes(interTransferLine2, data.DayOne, data.DayTwo, "PA1", "PA2", "PA3", "",
				"BEK");

			interTransfer.FinaliseDocket();
			AssertEquals("Transfer IsFinalised", true, interTransfer.IsFinalised);

			Factory.Save();

			var resultsLine1_1 = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine1_1.Length, 2);
			AssertResult(interTransferLine1, resultsLine1_1[0], -11m, CalculatePalletSpace(-11m, 5),
				palletIdOverride: "PLT: 1-111");

			var resultsLine1_2 = LoadView(data.Whs2, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine1_2.Length, 1);
			AssertResult(interTransferLine1.ChildTransferLine, resultsLine1_2[0], 11m, CalculatePalletSpace(11m, 5),
				palletIdOverride: "PLT: 1-112");

			var resultsLine2_1 = LoadView(data.Whs1, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine2_1.Length, 2);
			AssertResult(interTransferLine2, resultsLine2_1[0], -20m, 0m, palletIdOverride: "PLT: 2-111");

			var resultsLine2_2 = LoadView(data.Whs2, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine2_2.Length, 1);
			AssertResult(interTransferLine2.ChildTransferLine, resultsLine2_2[0], 20m, 0m,
				palletIdOverride: "PLT: 2-112");

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 11m,
				data.Whs1.FindLocation("A-1-1"), "PLT: 1-111");

			var notFinalisedTransferSource = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "112",
				transferType: TransferType.Codes.InterWhsSource);
			var transferLine3 = Helper.CreateWhsTransferLine(notFinalisedTransferSource, part3, 11m, "A-1-1",
				"PLT: 1-111", "A-1-1", "PLT: 1-112");
			transferLine3.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var results3_1 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(results3_1.Count, 1);
			AssertResult(receive3.Lines[0], results3_1[0], 11m, 0m, palletIdOverride: "PLT: 1-111",
				locationOverride: data.Whs1.FindLocation("A-1-1"));

			var results3_2 = LoadView(data.Whs2, data.Org1, part3);
			AssertEquals(results3_2.Count, 0);
		}

		#endregion

		#region TestView_InterTransferSource_SerialNumber

		[TestDate(2022, 02, 01)]
		public void TestView_InterTransferSource_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "111", Notify);
			interTransfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var interTransferLine1 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 1m, "A-1", whs2.PK, "A-1-1");
			interTransferLine1.WE_SerialNumber = "SN1";

			var interTransferLine2 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 1m, "A-1", whs2.PK, "A-1-2");
			interTransferLine2.WE_SerialNumber = "SN2";

			interTransfer.FinaliseDocket();
			AssertEquals("Transfer IsFinalised", true, interTransfer.IsFinalised);

			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals("results1 Count", 5, results1.Count);
			AssertResult(receiveLine1, results1[0], 1m, 0m);
			AssertResult(receiveLine2, results1[1], 1m, 0m);
			AssertResult(receiveLine3, results1[2], 1m, 0m);
			AssertResult(interTransferLine1, results1[3], -1m, 0, locationOverride: location);
			AssertResult(interTransferLine2, results1[4], -1m, 0, locationOverride: location);

			var results2 = LoadView(whs2, data.Org1, data.Part1);
			AssertEquals("results2 Count", 2, results2.Count);
			AssertResult(interTransferLine1.ChildTransferLine, results2[0], 1m, 0);
			AssertResult(interTransferLine2.ChildTransferLine, results2[1], 1m, 0);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_InterTransferSource_Unfinalised

		[TestDate(2022, 02, 01)]
		public void TestView_InterTransferSource_Unfinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var whs2 = Helper.CreateWarehouse("2", "B", 2, 2);
			var today = ZDateTimeOffset.Today;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m,
				data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "111", Notify);
			interTransfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var interTransferLine =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 11m, "A-1", whs2.PK, "B-1-1");
			interTransferLine.PickedTime = today.AddDays(-2);

			interTransferLine.PickLines.DeleteAll();
			var pickLine1 = Helper.CreateWhsPickLine(interTransferLine, inventory, 7m);
			var pickLine2 = Helper.CreateWhsPickLine(interTransferLine, inventory, 4m);
			pickLine1.WZ_PickedDateTime = today.AddDays(-2);
			pickLine2.WZ_PickedDateTime = today.AddDays(-1);

			Assert("Inter transfer is not finalised", !interTransfer.IsFinalised);
			Assert("Inter transfer line is picked", interTransferLine.IsPicked);
			Factory.Save();

			var resultsLine = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(2, resultsLine.Length);
			AssertResult(interTransferLine, resultsLine[0], -11m, CalculatePalletSpace(-11m, 5),
				locationOverride: data.Whs1.FindLocation("A-1"), finalisedDateOverride: today.AddDays(-1));
		}

		#endregion

		#region TestView_InterTransferDestination

		[TestDate(2022, 02, 01)]
		public void TestView_InterTransferDestination()
		{
			var data = new TestEnviromentForStockMovement(Factory);
			Factory.Save();

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs2, "111", Notify);
			interTransfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var interTransferLine1 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 11m, "A-1-1", data.Whs1.PK, "A-2-1");
			interTransferLine1.WE_TransferFromPalletId = "PLT: 1-111";
			interTransferLine1.WE_PalletID = "PLT: 1-112";
			Helper.SetDocketLineAttributes(interTransferLine1, data.DayOne, data.DayTwo, "PA1", "PA2", "PA3", "",
				"BEK");

			var interTransferLine2 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part2, 20m, "A-1-2", data.Whs1.PK, "A-2-2");
			interTransferLine2.WE_TransferFromPalletId = "PLT: 2-111";
			interTransferLine2.WE_PalletID = "PLT: 2-112";
			Helper.SetDocketLineAttributes(interTransferLine2, data.DayOne, data.DayTwo, "PA1", "PA2", "PA3", "",
				"BEK");

			interTransfer.FinaliseDocket();
			AssertEquals("Transfer IsFinalised", true, interTransfer.IsFinalised);

			Factory.Save();

			var resultsLine1_1 = LoadView(data.Whs2, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine1_1.Length, 1);
			AssertResult(interTransferLine1, resultsLine1_1[0], 11m, CalculatePalletSpace(11m, 5),
				palletIdOverride: "PLT: 1-112");

			var resultsLine1_2 = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine1_2.Length, 2);
			AssertResult(interTransferLine1.ChildTransferLine, resultsLine1_2[0], -11m, CalculatePalletSpace(-11m, 5),
				locationOverride: data.Whs1.FindLocation("A-1-1"), palletIdOverride: "PLT: 1-111");

			var resultsLine2_1 = LoadView(data.Whs2, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine2_1.Length, 1);
			AssertResult(interTransferLine2, resultsLine2_1[0], 20m, 0m, palletIdOverride: "PLT: 2-112");

			var resultsLine2_2 = LoadView(data.Whs1, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(resultsLine2_2.Length, 2);
			AssertResult(interTransferLine2.ChildTransferLine, resultsLine2_2[0], -20m, 0m,
				locationOverride: data.Whs1.FindLocation("A-1-2"), palletIdOverride: "PLT: 2-111");

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 11m,
				data.Whs1.FindLocation("A-1-1"), "PLT: 1-111");

			var cancelTransferDestination = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "112",
				transferType: TransferType.Codes.InterWhsDest);
			var transferLine3 = Helper.CreateWhsTransferLine(cancelTransferDestination, part3, 11m, "A-1-1",
				"PLT: 1-111", "A-1-1", "PLT: 1-112");
			transferLine3.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var results3_1 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(results3_1.Count, 1);
			AssertResult(receive3.Lines[0], results3_1[0], 11m, 0m, palletIdOverride: "PLT: 1-111",
				locationOverride: data.Whs1.FindLocation("A-1-1"));

			var results3_2 = LoadView(data.Whs2, data.Org1, part3);
			AssertEquals(results3_2.Count, 0);
		}

		#endregion

		#region TestView_InterTransferDestination_SerialNumber

		[TestDate(2022, 02, 01)]
		public void TestView_InterTransferDestination_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1-111");
			var receiveLine2 = CreateReceiveLine("SN2-111");
			var receiveLine3 = CreateReceiveLine("SN3-111");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, whs2, "111", Notify);
			interTransfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var interTransferLine1 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 1m, "A-1", data.Whs1.PK, "A-1-1");
			interTransferLine1.WE_SerialNumber = "SN1-111";

			var interTransferLine2 =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 1m, "A-1", data.Whs1.PK, "A-1-2");
			interTransferLine2.WE_SerialNumber = "SN2-111";

			interTransfer.FinaliseDocket();
			AssertEquals("Transfer IsFinalised", true, interTransfer.IsFinalised);

			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals("results1 Count", 5, results1.Count);
			AssertResult(receiveLine1, results1[0], 1m, 0m);
			AssertResult(receiveLine2, results1[1], 1m, 0m);
			AssertResult(receiveLine3, results1[2], 1m, 0m);
			AssertResult(interTransferLine1.ChildTransferLine, results1[3], -1m, 0, locationOverride: location);
			AssertResult(interTransferLine2.ChildTransferLine, results1[4], -1m, 0, locationOverride: location);

			var results2 = LoadView(whs2, data.Org1, data.Part1);
			AssertEquals("results2 Count", 2, results2.Count);
			AssertResult(interTransferLine1, results2[0], 1m, 0);
			AssertResult(interTransferLine2, results2[1], 1m, 0);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_InterTransferDestination_Unfinalised

		[TestDate(2022, 02, 01)]
		public void TestView_InterTransferDestination_Unfinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var whs2 = Helper.CreateWarehouse("2", "B", 2, 2);
			var today = ZDateTimeOffset.Today;

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, locationA1, "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var interTransfer = Helper.CreateWhsTransfer(data.Org1, whs2, "111", Notify);
			interTransfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var interTransferLine =
				Helper.CreateWhsTransferLine(interTransfer, data.Part1, 11m, "A-1", data.Whs1.PK, "B-1-1");
			interTransferLine.PickedTime = today.AddDays(-2);

			interTransferLine.PickLines.DeleteAll();
			var pickLine1 = Helper.CreateWhsPickLine(interTransferLine, inventory, 7m);
			var pickLine2 = Helper.CreateWhsPickLine(interTransferLine, inventory, 4m);
			pickLine1.WZ_PickedDateTime = today.AddDays(-2);
			pickLine2.WZ_PickedDateTime = today.AddDays(-1);

			Assert("Transfer Is Not Finalised", !interTransfer.IsFinalised);
			Assert("Inter transfer line is picked", interTransferLine.IsPicked);
			AssertNotNull("Precondition - child transfer line exists.", interTransferLine.ChildTransferLine);
			Factory.Save();

			var resultsLine_1 = LoadView(whs2, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(0, resultsLine_1.Length);

			var resultsLine_2 = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"])
				.ToArray();
			AssertEquals(2, resultsLine_2.Length);
			AssertResult(interTransferLine.ChildTransferLine, resultsLine_2[0], -11m, CalculatePalletSpace(-11m, 5),
				locationOverride: locationA1, finalisedDateOverride: today.AddDays(-1));
		}

		#endregion

		#region TestStockMovementReportFilter

		[TestDate(2022, 02, 01)]
		public void TestStockMovementReportFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 2, 2);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 2, 2);
			var client1 = Helper.CreateClient("A1");
			var client2 = Helper.CreateClient("B1");

			var productTea = Helper.CreateProduct(client1, "Tea");
			var teaRelationship =
				productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client1,
					OrgPartRelation.RelationshipTypes.Owner);
			teaRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productCoke = Helper.CreateProduct(client1, "Coke");
			var cokeRelationship =
				productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client1,
					OrgPartRelation.RelationshipTypes.Owner);
			cokeRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productVB = Helper.CreateProduct(client2, "VB");
			var vbRelationship =
				productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client2,
					OrgPartRelation.RelationshipTypes.Owner);
			vbRelationship.OU_OPC_Category = categoryBeers.PK;

			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", productTea, 10m, true, true);
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R2", productCoke, 200m, true, true);
			var receive3 = Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R3", productVB, 100m, true, true);

			Factory.Save();

			AssertEquals("should return one for VB", 1,
				LoadView_WithProductCategory(categoryPK: categoryBeers.PK).Count);
			AssertEquals("Should return two for Coke and Tea", 2,
				LoadView_WithProductCategory(categoryPK: categorySoftDrinks.PK).Count);
			AssertEquals("Should return all three", 3,
				LoadView_WithProductCategory(categoryPK: categoryBeverages.PK).Count);
			AssertEquals("Should return all three", 3, LoadView_WithProductCategory().Count);
			AssertEquals(2, LoadView_WithProductCategory(clientPK: client1.PK).Count);
			AssertEquals(1, LoadView_WithProductCategory(clientPK: client2.PK).Count);
			AssertEquals(2, LoadView_WithProductCategory(warehousePK: warehouse1.PK).Count);
			AssertEquals(1, LoadView_WithProductCategory(warehousePK: warehouse2.PK).Count);
			AssertEquals(1, LoadView_WithProductCategory(productPK: productTea.PK).Count);
			AssertEquals(3, LoadView_WithProductCategory(finalisedDateFrom: ZDateTime.Now.AddDays(-10).Date).Count);
			AssertEquals(0, LoadView_WithProductCategory(finalisedDateFrom: ZDateTime.Now.AddDays(1).Date).Count);
			AssertEquals(0, LoadView_WithProductCategory(finalisedDateTo: ZDateTime.Now.AddDays(-1).Date).Count);
			AssertEquals(3, LoadView_WithProductCategory(finalisedDateTo: ZDateTime.Now.AddDays(10).Date).Count);
			AssertEquals(3, LoadView_WithProductCategory(docketType: "INW").Count);
			AssertEquals(0, LoadView_WithProductCategory(docketType: "AAA").Count);
			AssertEquals(3, LoadView_WithProductCategory(docketType: "INW, BBB").Count);
			AssertEquals(3, LoadView_WithProductCategory(docketType: "AAA, INW, BBB").Count);
			AssertEquals(3, LoadView_WithProductCategory(docketType: "AAA,INW, BBB").Count);
			AssertEquals(3, LoadView_WithProductCategory(docketType: "AAA, INW").Count);
		}

		[TestDate(2022, 01, 31, 22, 0, 0)]
		[TestTimeZoneUNLOCO("SGSIN")]
		public void TestStockMovementReportFilter_WithOffsets_HCC()
		{
			var branch1 = Helper.CreateGlbBranch("B1");
			branch1.GB_RL_NKHomePort = "AUSYD"; // +11
			var branch2 = Helper.CreateGlbBranch("B2");
			branch2.GB_RL_NKHomePort = "SGSIN"; // +8

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 2, 2);
			warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 2, 2);
			warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;

			var client = Helper.CreateClient("A1");
			var productTea = Helper.CreateProduct(client, "Tea");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", productTea, 10m, null, "P1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R2", productTea, 10m, null, "P2");

			Factory.Save();

			// UTC time is 2022-01-31 at 10pm

			// Change at 2022-02-01 at 9am +11
			receive1.Lines[0].HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receive1.Lines[0].ChangeInventoryHeldCode(true);

			// Change at 2022-02-01 at 6am +08
			receive2.Lines[0].HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receive2.Lines[0].ChangeInventoryHeldCode(true);

			Factory.Save();

			var results = LoadView_WithProductCategory(docketType: "HCC", finalisedDateFrom: new ZDateTime(2022, 2, 1), finalisedDateTo: new ZDateTime(2022, 2, 1));
			AssertEquals("Should return hold code changes for both branches.", 4, results.Count);

			var positiveHoldCodeResults = results.Where(line => (ZBool)line["IsPositiveHCC"]).OrderBy(line => line["PalletID"]).ToArray();
			AssertHoldCodeRow(positiveHoldCodeResults[0], receive1.Lines[0], "HEL", new ZDateTime(2022, 02, 01), 10m, expectedIsPositiveHCC: true);
			AssertHoldCodeRow(positiveHoldCodeResults[1], receive2.Lines[0], "HEL", new ZDateTime(2022, 02, 01), 10m, expectedIsPositiveHCC: true);

			var negativeHoldCodeResults = results.Where(line => !(ZBool)line["IsPositiveHCC"]).OrderBy(line => line["PalletID"]).ToArray();
			AssertHoldCodeRow(negativeHoldCodeResults[0], receive1.Lines[0], "", new ZDateTime(2022, 02, 01), -10m, expectedIsPositiveHCC: false);
			AssertHoldCodeRow(negativeHoldCodeResults[1], receive2.Lines[0], "", new ZDateTime(2022, 02, 01), -10m, expectedIsPositiveHCC: false);
		}

		[TestDate(2022, 01, 31, 22, 0, 0)]
		[TestTimeZoneUNLOCO("SGSIN")]
		public void TestStockMovementReportFilter_WithOffsets_ORD()
		{
			var branch1 = Helper.CreateGlbBranch("B1");
			branch1.GB_RL_NKHomePort = "AUSYD"; // +11
			var branch2 = Helper.CreateGlbBranch("B2");
			branch2.GB_RL_NKHomePort = "SGSIN"; // +8

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 2, 2);
			warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 2, 2);
			warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;

			var client = Helper.CreateClient("A1");
			var productTea = Helper.CreateProduct(client, "Tea");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", productTea, 10m, null, "P1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R2", productTea, 10m, null, "P2");

			Factory.Save();

			// UTC time is 2022-01-31 at 10pm

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "O1", productTea, 10m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse2, "O2", productTea, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			// Change at 2022-02-01 at 9am +11
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			// Change at 2022-02-01 at 6am +08
			pick2.FinaliseAllOrders();
			pick2.FinalisePick();
			AssertIsFinalisedPrecondition(pick1);
			AssertIsFinalisedPrecondition(pick2);

			Factory.Save();

			var results = LoadView_WithProductCategory(docketType: "ORD", finalisedDateFrom: new ZDateTime(2022, 2, 1), finalisedDateTo: new ZDateTime(2022, 2, 1));
			AssertEquals("Should show Picking for both branches.", 2, results.Count);

			var resultsOrdered = results.OrderBy(line => line["PalletID"]).ToArray();
			AssertResult(order1.Lines[0], resultsOrdered[0], -10m, 0m, palletIdOverride: "P1", locationOverride: receive1.Lines[0].Location);
			AssertResult(order2.Lines[0], resultsOrdered[1], -10m, 0m, palletIdOverride: "P2", locationOverride: receive2.Lines[0].Location);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZGuid? categoryPK = null, ZGuid? clientPK = null,
			ZGuid? warehousePK = null, ZGuid? productPK = null, ZDateTime? finalisedDateFrom = null,
			ZDateTime? finalisedDateTo = null, string docketType = "INW, ORD, ADJ, TFR, WOR, HCC")
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				$"SELECT * FROM WhsStockMovementReport('{categoryPK}','{clientPK}','{warehousePK}','{productPK}','{finalisedDateFrom?.ToString("yyyy-MM-dd")}','{finalisedDateTo?.ToString("yyyy-MM-dd")}','{docketType}')"
					.Replace("'',", "null,");
			result.Load(sql);
			return result;
		}

		#endregion

		#region TestView_ExcludesZeroUnitPickLines

		[TestDate(2022, 02, 01)]
		public void TestView_ExcludesZeroUnitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, setReleaseCaptured: true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			var inventory1 = receive1.Inventory[0];
			var inventory2 = receive2.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory1);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine1.ReservedQuantity);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine2.ReservedQuantity);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			pick.OrderedInventories[1].AvailableInventories[0].PickLineQuantity = 0m;
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			// should only return the Receives
			var results = LoadView();
			AssertEquals(2, results.Count);
			AssertEquals("INW", results[0]["DocketType"]);
			AssertEquals("INW", results[1]["DocketType"]);
		}

		#endregion

		#region TestView_Order

		[TestDate(2019, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_Order()
		{
			var data = new TestEnviromentForStockMovement(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			order.ConsigneePK = data.Consignee1.PK;
			order.ConsigneeAddressPK = data.Consignee1.MainAddress.PK;
			order.WD_RequiredDate = data.DayOne.ToZDateTime().ToOffset();
			order.WD_RS_NKServiceLevel = "TS2";

			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 12m);

			var pick1 = Helper.CreatePickNew(order);
			var pickLine1 = orderLine2.PickLines.Single();
			var pickLine2 = (WhsPickLine)pickLine1.Clone();
			pickLine1.WZ_Units = 8m;
			pickLine2.WZ_Units = 4m;

			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertEquals("Order IsFinalised", true, order.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick1.IsFinalised);
			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals(results1.Count, 2);
			AssertResult(orderLine1, results1[1], -11m, CalculatePalletSpace(-11m, 5), palletIdOverride: "PLT: 1-111",
				locationOverride: orderLine1.PickLines[0].InventoryLine.Location);

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2);
			AssertEquals(results2.Count, 2);
			AssertResult(orderLine2, results2[1], -12m, 0m, palletIdOverride: "PLT: 2-111",
				locationOverride: orderLine2.PickLines[0].InventoryLine.Location);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var cancelOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "112");
			var orderLine3 = Helper.CreateWhsOrderLine(cancelOrder, part3, 12m);
			Factory.Save();

			cancelOrder.CancelReactivateDocket();
			AssertEquals(true, cancelOrder.IsCancelled);

			var results3 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(results3.Count, 0);

			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "113", Notify);
			order4.ConsigneePK = data.Consignee1.PK;
			order4.ConsigneeAddressPK = data.Consignee1.MainAddress.PK;
			order4.ConsigneeDocAddress.E2_AddressOverride = ZBool.True;
			order4.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			order4.ConsigneeNameOrPK = "Overriden Consignee Company name";
			order4.ConsigneeDocAddress.E2_City = "SYD";

			var orderLine4 = Helper.CreateWhsOrderLine(order4, data.Part2, 16m);
			var pick2 = Helper.CreatePickNew(order4);
			pick2.FinaliseAllOrders();
			pick2.FinalisePick();
			AssertEquals("Order IsFinalised", true, order4.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick2.IsFinalised);
			Factory.Save();

			var result4 = LoadView(data.Whs1, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(result4.Length, 3);
			AssertResult(orderLine4, result4[0], -16m, 0m, palletIdOverride: "PLT: 2-111",
				locationOverride: orderLine4.PickLines[0].InventoryLine.Location);
		}

		#endregion

		#region TestView_Order_SerialNumber

		[TestDate(2022, 02, 01)]
		public void TestView_Order_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true);
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN2";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine3.WE_SerialNumber = "SN3";

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Order IsFinalised", true, order.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick.IsFinalised);
			Factory.Save();

			var result = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals("Should be 6 results", 6, result.Count);
			AssertResult(receiveLine1, result[0], 1m, 0m);
			AssertResult(receiveLine2, result[1], 1m, 0m);
			AssertResult(receiveLine3, result[2], 1m, 0m);
			AssertResult(orderLine1, result[3], -1m, 0m, locationOverride: inventoryLocation,
				expectedSerialNumber: "SN1");
			AssertResult(orderLine2, result[4], -1m, 0m, locationOverride: inventoryLocation,
				expectedSerialNumber: "SN2");
			AssertResult(orderLine3, result[5], -1m, 0m, locationOverride: inventoryLocation,
				expectedSerialNumber: "SN3");

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_Order_Unfinalised

		[TestDate(2022, 02, 01)]
		public void TestView_Order_Unfinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var today = ZDateTimeOffset.Today;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 12m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order);
			var pickLine1 = orderLine2.PickLines.Single();
			var pickLine2 = (WhsPickLine)pickLine1.Clone();
			pickLine1.WZ_Units = 8m;
			pickLine2.WZ_Units = 4m;

			order.Lines.ToList()
				.ForEach(orderLine => orderLine.PickLines.ToList()
					.ForEach(pickLine => pickLine.WZ_PickedDateTime = today.AddDays(-1)));

			Assert("Order is not finalised", !order.IsFinalised);
			Assert("Pick is not finalised", !pick1.IsFinalised);
			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals(2, results1.Count);

			// the in-Transit Transfer Line should be shown.
			var transferLine1 = orderLine1.PickLines[0].InventoryLine;
			AssertResult(transferLine1, results1[1], -11m, CalculatePalletSpace(-11m, 5),
				locationOverride: ((WhsTransferLine)transferLine1).TransferFromLocation,
				finalisedDateOverride: today.AddDays(-1));

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2);
			AssertEquals(2, results2.Count);

			// the in-Transit Transfer Line should be shown.
			var transferLine2 = orderLine2.PickLines[0].InventoryLine;
			AssertResult(transferLine2, results2[1], -12m, 0m,
				locationOverride: ((WhsTransferLine)transferLine2).TransferFromLocation,
				finalisedDateOverride: today.AddDays(-1));
		}

		#endregion

		#region TestView_Order_Unfinalised_PickedDirectly

		[TestDate(2022, 02, 01)]
		public void TestView_Order_Unfinalised_PickedDirectly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var today = ZDateTimeOffset.Today;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 12m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order);
			var pickLine1 = orderLine2.PickLines.Single();
			var pickLine2 = (WhsPickLine)pickLine1.Clone();
			pickLine1.WZ_Units = 8m;
			pickLine2.WZ_Units = 4m;

			order.Lines.ToList()
				.ForEach(orderLine => orderLine.PickLines.ToList()
					.ForEach(pickLine => pickLine.WZ_PickedDateTime = today.AddDays(-1)));

			Assert("Order is not finalised", !order.IsFinalised);
			Assert("Pick is not finalised", !pick1.IsFinalised);

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals(2, results1.Count);
			AssertResult(orderLine1, results1[1], -11m, CalculatePalletSpace(-11m, 5),
				locationOverride: orderLine1.PickLines[0].InventoryLine.Location,
				finalisedDateOverride: today.AddDays(-1));

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2);
			AssertEquals(2, results2.Count);
			AssertResult(orderLine2, results2[1], -12m, 0m,
				locationOverride: orderLine2.PickLines[0].InventoryLine.Location,
				finalisedDateOverride: today.AddDays(-1));
		}

		#endregion

		#region TestView_Order_PickedWithTime_WhenReportRangeDateNull

		[TestDate(2024, 12, 01)]
		public void TestView_Order_PickedWithTime_WhenReportRangeDateNull()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var whsPickLine = pick.GetAllPickLines().Single();
			var pickedDate = new ZDateTimeOffset(2024, 12, 01, 06, 00, 00, TimeSpan.FromHours(10));
			whsPickLine.WZ_PickedDateTime = pickedDate;
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Order IsFinalised", true, order.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick.IsFinalised);
			Factory.Save();

			AssertViewHasResults(fromDate: null, toDate: new DateTime(2024, 12, 01));
			AssertViewHasResults(fromDate: new DateTime(2024, 12, 01), toDate: null);

			void AssertViewHasResults(DateTime? fromDate, DateTime? toDate)
			{
				var result = new DynamicBusinessObjectCollection(Factory);

				string sql = @"select	*
						from	WhsStockMovementReport(null,null,null,null,@FromDate,@ToDate,'INW, ORD, ADJ, TFR, WOR, DWO, HCC, KAS')
						order by DocketType, Reference, Location";

				var sqlParams = new ZSqlParameterCollection
				{
					{ "@FromDate", fromDate, WhsDocketSchema.WD_FinalisedDate },
					{ "@ToDate", toDate, WhsDocketSchema.WD_FinalisedDate },
				};
				result.Load(sql, sqlParams);

				AssertEquals(result.Count, 2);
				AssertResult(receive.Lines[0], result[0], 10m, 0);
				AssertResult(order.Lines[0], result[1], -1m, 0, locationOverride: data.Whs1.DefaultLocation, finalisedDateOverride: pickedDate);
			}
		}

		#endregion

		#region TestView_DistributionCentre

		[TestDate(2019, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_DistributionCentre()
		{
			var data = new TestEnviromentForStockMovement(Factory);
			var distributionCentre = Helper.CreateClient("C3");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 11m);

			var pick1 = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_Units = 8m;

			pick1.FinaliseAllOrders();
			pick1.FinalisePick();

			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "113", Notify);
			order2.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK; 
			Helper.CreateWhsOrderLine(order2, data.Part1, 16m);

			var pick2 = Helper.CreatePickNew(order2);
			pick2.FinaliseAllOrders();
			pick2.FinalisePick();

			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1, data.Part1);
			var ordersWithDcAddress = results.Where(o => (CargoWise.Types.ZGuid)o["DistributionCentrePK"] == distributionCentre.PK).ToArray();

			AssertEquals(1, ordersWithDcAddress.Length);

			var businessObject = ordersWithDcAddress[0];

			AssertEquals(order2.PK, businessObject["DocketPK"]);
			AssertEquals(distributionCentre.PK, businessObject["DistributionCentrePK"]);
			AssertEquals(distributionCentre.OH_FullName, businessObject["DistributionCentreCoName"]);
		}

		#endregion

		#region TestView_Receive

		[TestDate(2022, 02, 01)]
		public void TestView_Receive()
		{
			var data = new TestEnviromentForStockMovement(Factory);

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals(results1.Count, 1);
			AssertResult(data.Receive.Lines[0], results1[0], 133m,
				CalculatePalletSpace(111m, 5) + CalculatePalletSpace(22m, 5));

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2);
			AssertEquals(results2.Count, 1);
			AssertResult(data.Receive.Lines[1], results2[0], 121m, 0m);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var cancelReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 10m,
				allocateLocations: false, finalise: false);
			Factory.Save();

			cancelReceive.CancelReactivateDocket();
			AssertEquals(true, cancelReceive.IsCancelled);

			var results3 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(results3.Count, 0);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_Receive_UnloadedInDockDoor_FindsUnloadedDDLStock()
		{
			// create a receive, unload to dock door location
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLTA");
			receiveLine.WE_UnloadedTime = new ZDateTimeOffset(new ZDateTime(2018, 1, 1));
			receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, new ZDateTimeOffset(2018, 1, 1));
			Factory.Save();

			var receiveResults1 = LoadView(data.Whs1, data.Org1, data.Part1)
				.Where(line => line["DocketType"].ToString() == "INW").ToArray();
			AssertEquals("Should get 1 unloaded receive line.", 1, receiveResults1.Length);
			AssertResult(receiveLine, receiveResults1[0], 10m, 0m, finalisedDateOverride: new ZDateTimeOffset(2018, 1, 1));
		}

		[TestDate(2022, 02, 01)]
		public void TestView_Receive_UnloadedInDockDoor_FindsPuttingAwayTransferStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLTA");
			receiveLine.WE_UnloadedTime = new ZDateTimeOffset(new ZDateTime(2018, 1, 1));
			receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, new ZDateTimeOffset(2018, 1, 1));
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLTA", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals($"Precondition: Receive Line status is {InventoryStatus.Codes.Received}.",
				InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);
			AssertEquals($"Precondition: Receive Line status is {InventoryStatus.Codes.PuttingAway}.",
				InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);

			var receiveResults2 = LoadView(data.Whs1, data.Org1, data.Part1)
				.Where(line => line["DocketType"].ToString() == "INW").ToArray();
			AssertEquals("Should get 1 Putting Away receive line.", 1, receiveResults2.Length);
			AssertResult(receiveLine, receiveResults2[0], 10m, 0m, finalisedDateOverride: new ZDateTimeOffset(2018, 1, 1));
		}

		[TestDate(2022, 02, 01)]
		public void TestView_Receive_UnloadedInDockDoor_FindsPutawayStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLTA");
			receiveLine.WE_UnloadedTime = new ZDateTimeOffset(new ZDateTime(2018, 1, 1));
			receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, new ZDateTimeOffset(2018, 1, 1));
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLTA", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals($"Precondition: Receive Line status is {InventoryStatus.Codes.Received}.",
				InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);
			AssertEquals($"Precondition: Receive Line status is {InventoryStatus.Codes.PuttingAway}.",
				InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();
			AssertEquals("Precondition: transferLine status is Putaway.", InventoryStatus.Codes.Putaway,
				transferLine.WE_CurrentInventoryStatus);

			var receiveResults3 = LoadView(data.Whs1, data.Org1, data.Part1).Where(line =>
				line["DocketType"].ToString() == "TFR" && line["Location"].ToString() == "A-1").ToArray();
			AssertEquals("Should get 1 Putaway transferLine.", 1, receiveResults3.Length);
			AssertResult(transferLine, receiveResults3[0], 10m, 0m);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_Receive_UnloadedInDockDoor_FindsPutawayAvaliableStock()
		{
			// create a receive, unload to dock door location
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLTA");
			receiveLine.WE_UnloadedTime = new ZDateTimeOffset(new ZDateTime(2018, 1, 1));
			receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, new ZDateTimeOffset(2018, 1, 1));
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLTA", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals($"Precondition: Receive Line status is {InventoryStatus.Codes.Received}.",
				InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);
			AssertEquals($"Precondition: Receive Line status is {InventoryStatus.Codes.PuttingAway}.",
				InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();
			AssertEquals("Precondition: transfer Line status is Putaway.", InventoryStatus.Codes.Putaway,
				transferLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();
			AssertEquals("Precondition: transfer Line status is Available.", InventoryStatus.Codes.Available,
				transferLine.WE_CurrentInventoryStatus);

			var receiveResults4 = LoadView(data.Whs1, data.Org1, data.Part1).Where(line =>
				line["DocketType"].ToString() == "TFR" && line["Location"].ToString() == "A-1").ToArray();
			AssertEquals("Should get 1 receive line.", 1, receiveResults4.Length);
			AssertResult(transferLine, receiveResults4[0], 10m, 0m);
		}

		[TestDate(2018, 8, 8, 11, 20, 33)]
		public void TestView_Receive_MultipleWHUEvents()
		{
			// create a receive, unload to dock door location
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var arrivedInventoryLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "A");
			var putawayTfrInventoryLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "C");
			Factory.Save();

			var transferForPalletC = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletC.WD_IsPutawayTransfer = true;
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transferForPalletC, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "C", 10m);
			transferForPalletC.RunPreSaveValidation();
			transferForPalletC.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, transferForPalletC.IsFinalised);
			AssertEquals("Precondition", InventoryStatus.Codes.Arrived,
				arrivedInventoryLine.WE_OriginalInventoryStatus);

			arrivedInventoryLine.WE_WL = nonDockDoorLocation.PK;
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway,
				arrivedInventoryLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received,
				putawayTfrInventoryLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway,
				transferLineForPalletC.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", nonDockDoorLocation.PK, transferLineForPalletC.WE_WL);

			// add a few WHU events (2 active, 1 inactive)
			receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, ZDateTimeOffset.Now);
			var inactiveLog = receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, ZDateTimeOffset.Now.AddHours(1));
			inactiveLog.Cancel();
			AssertEquals("Pre-condition: Log should be Cancelled.", true, inactiveLog.SL_IsCancelled);
			receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, ZDateTimeOffset.Now.AddHours(1));

			var whuEvents = receive.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "WHU"));
			AssertEquals("Unloaded event added successfully", 3, whuEvents.Length);

			// finalise receive, check stock movement report
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var receiveResults = LoadView(data.Whs1, data.Org1, data.Part1)
				.Where(line => line["DocketType"].ToString() == "INW").ToArray();
			AssertEquals(
				"Should get 2 lines for each receive line. (Should not get multiple lines for multiple WHU logs)", 2,
				receiveResults.Length);
		}

		#region TestView_Receive_UnloadLog

		[TestDate(2022, 02, 01)]
		public void TestView_Receive_UnloadLog()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now, Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLTA");
			receiveLine1.WE_UnloadedTime = new ZDateTimeOffset(new ZDateTime(2018, 7, 24, 11, 24, 30));

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLTB");
			receiveLine2.WE_UnloadedTime = new ZDateTimeOffset(new ZDateTime(2018, 7, 24, 11, 30, 30));
			receive.Logs.AddNew(Events.WarehouseReceiptUnloaded, new ZDateTimeOffset(2018, 7, 24, 11, 59, 00));
			Factory.Save();

			var receiveResults = LoadView(data.Whs1, data.Org1, data.Part1)
				.Where(line => line["DocketType"].ToString() == "INW").ToArray();
			AssertEquals("Should get 2 unloaded receive line.", 2, receiveResults.Length);

			var line1Result = receiveResults.Single(l => l["PalletID"].ToString() == "PLTA");
			var line2Result = receiveResults.Single(l => l["PalletID"].ToString() == "PLTB");
			AssertResult(receiveLine1, line1Result, 10m, 0m,
				finalisedDateOverride: new ZDateTimeOffset(2018, 7, 24, 11, 24, 30));
			AssertResult(receiveLine2, line2Result, 10m, 0m,
				finalisedDateOverride: new ZDateTimeOffset(2018, 7, 24, 11, 30, 30));
		}

		#endregion

		#endregion

		#region TestView_WithReleaseCapturedAttributes

		[TestDate(2008, 12, 7, 13, 57, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_WithReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);

			// setup another OrgPartRelation to make sure data is not doubled in the view
			var org2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(org2, data.Part1, "OWN");
			Helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "PA1-1";
			releaseLine1.PartAttribute2 = "PA2-1";
			releaseLine1.PartAttribute3 = "PA3-1";
			releaseLine1.Quantity = 2m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "PA1-2";
			releaseLine2.PartAttribute2 = "PA2-2";
			releaseLine2.PartAttribute3 = "PA3-2";
			releaseLine2.Quantity = 3m;

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView();
			AssertResult(receive.Lines[0], results[0], +5m, 0m);
			AssertResult(orderLine, results[1], -2m, 0m, releaseLineIndex: 0,
				locationOverride: orderLine.PickLines[0].InventoryLine.Location);
			AssertResult(orderLine, results[2], -3m, 0m, releaseLineIndex: 1,
				locationOverride: orderLine.PickLines[0].InventoryLine.Location);
		}

		[TestDate(2008, 12, 7, 13, 57, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_WithReleaseCapturedAttributes_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true,
				setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "PA1-1";
			releaseLine1.SerialNumber = "SER-1";
			releaseLine1.Quantity = 1m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "PA1-2";
			releaseLine2.SerialNumber = "SER-2";
			releaseLine2.Quantity = 1m;

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView();
			AssertResult(receive.Lines[0], results[0], 5m, 0m);
			AssertResult(orderLine, results[1], -1m, 0m, releaseLineIndex: 0,
				locationOverride: orderLine.PickLines[0].InventoryLine.Location, expectedSerialNumber: "SER-1");
			AssertResult(orderLine, results[2], -1m, 0m, releaseLineIndex: 1,
				locationOverride: orderLine.PickLines[0].InventoryLine.Location, expectedSerialNumber: "SER-2");
		}

		#endregion

		#region TestView_WithReleaseCapturedAttributesAndMultiplePallets

		[TestDate(2008, 12, 7)]
		public void TestView_WithReleaseCapturedAttributesAndMultiplePallets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			var loc1 = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, loc1, "PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, loc1, "PLT2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 13m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "PA1-1";
			releaseLine1.PartAttribute2 = "PA2-1";
			releaseLine1.PartAttribute3 = "PA3-1";
			releaseLine1.Quantity = 3m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "PA1-2";
			releaseLine2.PartAttribute2 = "PA2-2";
			releaseLine2.PartAttribute3 = "PA3-2";
			releaseLine2.Quantity = 10m;

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView();

			// Thing is that result data will be split between locations and part attributes.
			// And we cannot predict how exactly it will be split.
			// But we can check the totals

			AssertEquals(13m,
				results.Where(x => (ZString)x["DocketType"] == "INW").Sum(x => (ZDecimal)x["QuantityActual"]));
			AssertEquals(-13m,
				results.Where(x => (ZString)x["DocketType"] == "ORD").Sum(x => (ZDecimal)x["QuantityActual"]));

			// That's how data should be split per pallet/location
			AssertEquals(-5m, results.Where(x => (ZString)x["DocketType"] == "ORD" && (ZString)x["PalletID"] == "PLT1")
				.Sum(x => (ZDecimal)x["QuantityActual"]));
			AssertEquals(-8m, results.Where(x => (ZString)x["DocketType"] == "ORD" && (ZString)x["PalletID"] == "PLT2")
				.Sum(x => (ZDecimal)x["QuantityActual"]));

			// That's how data should be split by part attributes
			AssertEquals(-3m, results.Where(x => (ZString)x["DocketType"] == "ORD"
												 && (ZString)x["PartAttrib1"] == releaseLine1.PartAttribute1
												 && (ZString)x["PartAttrib2"] == releaseLine1.PartAttribute2
												 && (ZString)x["PartAttrib3"] == releaseLine1.PartAttribute3)
				.Sum(x => (ZDecimal)x["QuantityActual"]));
			AssertEquals(-10m, results.Where(x => (ZString)x["DocketType"] == "ORD"
												  && (ZString)x["PartAttrib1"] == releaseLine2.PartAttribute1
												  && (ZString)x["PartAttrib2"] == releaseLine2.PartAttribute2
												  && (ZString)x["PartAttrib3"] == releaseLine2.PartAttribute3)
				.Sum(x => (ZDecimal)x["QuantityActual"]));
		}

		[TestDate(2008, 12, 7)]
		public void TestView_WithReleaseCapturedAttributesAndMultiplePallets_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true,
				setReleaseCaptured: true);
			var loc1 = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, loc1, "PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, loc1, "PLT2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "PA1-1";
			releaseLine1.SerialNumber = "SER-1";
			releaseLine1.Quantity = 1m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "PA1-2";
			releaseLine2.SerialNumber = "SER-2";
			releaseLine2.Quantity = 1m;

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView();

			// Check the totals
			AssertEquals(2m,
				results.Where(x => (ZString)x["DocketType"] == "INW").Sum(x => (ZDecimal)x["QuantityActual"]));
			AssertEquals(-2m,
				results.Where(x => (ZString)x["DocketType"] == "ORD").Sum(x => (ZDecimal)x["QuantityActual"]));

			AssertEquals(-1m, results.Where(x => (ZString)x["DocketType"] == "ORD" && (ZString)x["PalletID"] == "PLT1")
				.Sum(x => (ZDecimal)x["QuantityActual"]));
			AssertEquals(-1m, results.Where(x => (ZString)x["DocketType"] == "ORD" && (ZString)x["PalletID"] == "PLT2")
				.Sum(x => (ZDecimal)x["QuantityActual"]));

			AssertEquals(-1m, results.Where(x => (ZString)x["DocketType"] == "ORD"
												 && (ZString)x["PartAttrib1"] == releaseLine1.PartAttribute1
												 && (ZString)x["SerialNumber"] == "SER-1")
				.Sum(x => (ZDecimal)x["QuantityActual"]));
			AssertEquals(-1m, results.Where(x => (ZString)x["DocketType"] == "ORD"
												 && (ZString)x["PartAttrib1"] == releaseLine2.PartAttribute1
												 && (ZString)x["SerialNumber"] == "SER-2")
				.Sum(x => (ZDecimal)x["QuantityActual"]));
		}

		#endregion

		#region TestView_WithHoldCodeChanges

		[TestDate(2022, 02, 01)]
		public void TestView_WithHoldCodeChanges()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			var holdCode = Helper.CreateInventoryHeldCode("SHATECLI", "Sharks ate the Client");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m,
				data.Whs1.FindLocation("A-1"), "PLT-1", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 15m,
				data.Whs1.FindLocation("A-2"), new ZDate(year, 1, 3), new ZDate(year, 1, 4), "SA1", "SA2", "SA3", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 7m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year - 1, 2, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year - 1, 2, 2);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 7m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-3"));
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year - 1, 3, 1);
			Factory.Save();

			// Change 4 of 7 Units to Held
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferLine.HeldCodeChangeQuantity = 4m;
			transferLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			// Change 5 of 10 Units to Damaged
			inventory1.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			inventory1.InDocketLine.HeldCodeChangeQuantity = 5m;
			inventory1.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			// Change all 15 Units to Held
			inventory2.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory2.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var newTransferLineQuery = new ZQuery();
			newTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, transfer.PK);
			newTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, false);
			var newTransferLine = Factory.LoadTop1<WhsTransferLine>(newTransferLineQuery);
			var firstHoldCodeChangeLog = newTransferLine.Logs.Find(l => l.IsHoldCodeChangeEvent()).Single();

			var receiveLineQuery = new ZQuery();
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, receive1.PK);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, false);
			var newReceiveLine = Factory.LoadTop1<WhsReceiveLine>(receiveLineQuery);
			var receiveLineHoldCodeChangeLog1 = newReceiveLine.Logs.Find(l => l.IsHoldCodeChangeEvent()).Single();
			var receiveLineHoldCodeChangeLog2 =
				inventory2.InDocketLine.Logs.Find(l => l.IsHoldCodeChangeEvent()).Single();

			// Change all 4 Units to Shark ate Client
			newTransferLine.HeldCodeToChangeTo = "SHATECLI";
			newTransferLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var timeIndex = ZDateTimeOffset.Now;
			var holdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery() { OrderBy = "WHL_SystemCreateTimeUtc" });

			// necessary to have realistic event times on tests, without this all event times are the same
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsInventoryHoldChangeLog_PreventUpdate", "dbo.WhsInventoryHoldChangeLog"))
			{
				foreach (var log in holdLogs)
				{
					log.WHL_EventTime = timeIndex = UpdateHoldChangeLogEventTime(timeIndex);
				}
				Factory.Save();
			}

			var sql = "select * from WhsStockMovementReport(null,null,null,null,null,null,'INW, ORD, ADJ, TFR, WOR, HCC') order by FinalisedDateTime, QuantityActual";
			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load(sql);
			AssertEquals(
				"3 Receive Lines + 2 Transfer Lines (From and To Location considered as two movements) + 8 Hold Code Changes.",
				3 + 2 + 8, results.Count);
			AssertResult(inventory1.InDocketLine, results[0], 10m, 0);
			AssertResult(inventory3.InDocketLine, results[1], 7m, 0);
			AssertResult(inventory2.InDocketLine, results[2], 15m, 0);

			var transferLineResults = results.Where(line => (ZString)line["DocketType"] == "TFR")
				.OrderBy(line => line["QuantityActual"]).ToArray();
			var mostRecentPickLineTime = transferLine.PickLines.OrderByDescending(pick => pick.WZ_PickedDateTime)
				.First().WZ_PickedDateTime;
			mostRecentPickLineTime = mostRecentPickLineTime.AddTicks(-(mostRecentPickLineTime.Ticks % TimeSpan.TicksPerMinute)); //truncate to whole minutes
			AssertResult(transferLine, transferLineResults[0], -7m, 0, locationOverride: data.Whs1.FindLocation("A-1"),
				finalisedDateOverride: mostRecentPickLineTime);
			AssertResult(transferLine, transferLineResults[1], 7m, 0, locationOverride: data.Whs1.FindLocation("A-3"));

			var holdCodeResults = results.Where(line => (ZString)line["DocketType"] == "HCC")
				.OrderBy(line => line["FinalisedDateTime"]).ToArray();

			var positiveHoldCodeResults = results.Where(line => (ZString)line["DocketType"] == "HCC" && (ZBool)line["IsPositiveHCC"])
				.OrderBy(line => line["FinalisedDateTime"]).ToArray();
			AssertHoldCodeRow(positiveHoldCodeResults[0], transferLine, "HEL",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), 4m, true);
			AssertHoldCodeRow(positiveHoldCodeResults[1], inventory1.InDocketLine, "DAM",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), 5m, true);
			AssertHoldCodeRow(positiveHoldCodeResults[2], inventory2.InDocketLine, "HEL",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), 15m, true);
			AssertHoldCodeRow(positiveHoldCodeResults[3], transferLine, "SHATECLI",
				((ZDateTimeOffset)holdLogs[3]["WHL_EventTime"]).ToDateTime(), 4m, true);

			var negativeHoldCodeResults = results.Where(line => (ZString)line["DocketType"] == "HCC" && !(ZBool)line["IsPositiveHCC"])
				.OrderBy(line => line["FinalisedDateTime"]).ToArray();
			AssertHoldCodeRow(negativeHoldCodeResults[0], transferLine, "",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), -4m, false);
			AssertHoldCodeRow(negativeHoldCodeResults[1], inventory1.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), -5m, false);
			AssertHoldCodeRow(negativeHoldCodeResults[2], inventory2.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), -15m, false);
			AssertHoldCodeRow(negativeHoldCodeResults[3], transferLine, "HEL",
				((ZDateTimeOffset)holdLogs[3]["WHL_EventTime"]).ToDateTime(), -4m, false);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_ChainedHoldChanges()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year - 1, 2, 1);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-3"));

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year - 1, 3, 1);
			Factory.Save();

			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferLine.HoldReasonToChangeTo = "I'm holding this now";
			transferLine.HeldCodeChangeQuantity = 10m;
			transferLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			transferLine.HeldCodeChangeQuantity = 10m;
			transferLine.HoldReasonToChangeTo = "Got bashed";
			transferLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			transferLine.HeldCodeToChangeTo = "";
			transferLine.HoldReasonToChangeTo = "";
			transferLine.HeldCodeChangeQuantity = 10m;
			transferLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var timeIndex = ZDateTimeOffset.Now;
			var holdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery() { OrderBy = "WHL_SystemCreateTimeUtc" });

			// necessary to have realistic event times on tests, without this all event times are the same
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsInventoryHoldChangeLog_PreventUpdate", "dbo.WhsInventoryHoldChangeLog"))
			{
				foreach (var log in holdLogs)
				{
					log.WHL_EventTime = timeIndex = UpdateHoldChangeLogEventTime(timeIndex);
				}
				Factory.Save();
			}

			var sql = "select * from WhsStockMovementReport(null,null,null,null,null,null,'HCC') Order By FinalisedDateTime";
			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load(sql);
			AssertEquals("6 Hold Code Logs(From and To Inventory considered 2 HCC logs).", 6, results.Count);

			var positiveHoldCodeResults = results.Where(line => (ZBool)line["IsPositiveHCC"])
				.OrderBy(line => line["FinalisedDateTime"])
				.ToArray();
			AssertHoldCodeRow(positiveHoldCodeResults[0], transferLine, "HEL",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), 10m, true);
			AssertHoldCodeRow(positiveHoldCodeResults[1], transferLine, "DAM",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), 10m, true);
			AssertHoldCodeRow(positiveHoldCodeResults[2], transferLine, "",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), 10m, true);

			var negativeHoldCodeResults = results.Where(line => !(ZBool)line["IsPositiveHCC"])
				.OrderBy(line => line["FinalisedDateTime"])
				.ToArray();
			AssertHoldCodeRow(negativeHoldCodeResults[0], transferLine, "",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), -10m, false);
			AssertHoldCodeRow(negativeHoldCodeResults[1], transferLine, "HEL",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), -10m, false);
			AssertHoldCodeRow(negativeHoldCodeResults[2], transferLine, "DAM",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), -10m, false);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_WithHoldCodeChanges_SerialNumber()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var holdCode = Helper.CreateInventoryHeldCode("SHATECLI", "Sharks ate the Client");
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year - 1, 2, 1);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-3"));
			transferLine.WE_SerialNumber = "SN1";
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year - 1, 3, 1);
			Factory.Save();

			// Change Transferred item to held
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferLine.HeldCodeChangeQuantity = 1m;
			transferLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			// Change Received item to damaged
			receiveLine2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine2.HeldCodeChangeQuantity = 1m;
			receiveLine2.ChangeInventoryHeldCode(true);
			Factory.Save();

			// Change transferred item to Shark ate Client
			transferLine.HeldCodeToChangeTo = "SHATECLI";
			transferLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var timeIndex = ZDateTimeOffset.Now;
			var holdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery() { OrderBy = "WHL_SystemCreateTimeUtc" });

			// necessary to have realistic event times on tests, without this all event times are the same
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsInventoryHoldChangeLog_PreventUpdate", "dbo.WhsInventoryHoldChangeLog"))
			{
				foreach (var log in holdLogs)
				{
					log.WHL_EventTime = timeIndex = UpdateHoldChangeLogEventTime(timeIndex);
				}
				Factory.Save();
			}

			var sql = "select * from WhsStockMovementReport(null,null,null,null,null,null,'INW, ORD, ADJ, TFR, WOR, HCC') order by FinalisedDateTime";
			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load(sql);
			AssertEquals(
				"3 Receive Lines + 2 Transfer Lines (From and To Location considered as two movements) + 6 Hold Code Changes(From and To Inventory considered 2 HCC logs).",
				3 + 2 + 6, results.Count);
			AssertResult(receiveLine1, results[0], 1m, 0m);
			AssertResult(receiveLine2, results[1], 1m, 0m);
			AssertResult(receiveLine3, results[2], 1m, 0m);

			var transferLineResults = results.Where(line => (ZString)line["DocketType"] == "TFR")
				.OrderBy(line => line["QuantityActual"]).ToArray();
			var mostRecentPickLineTime = transferLine.PickLines.OrderByDescending(pick => pick.WZ_PickedDateTime)
				.First().WZ_PickedDateTime;
			mostRecentPickLineTime = mostRecentPickLineTime.AddTicks(-(mostRecentPickLineTime.Ticks % TimeSpan.TicksPerMinute)); //truncate to whole minutes
			AssertResult(transferLine, transferLineResults[0], -1m, 0, locationOverride: data.Whs1.FindLocation("A-1"),
				finalisedDateOverride: mostRecentPickLineTime);
			AssertResult(transferLine, transferLineResults[1], 1m, 0, locationOverride: data.Whs1.FindLocation("A-3"));

			var holdCodeResults = results.Where(line => (ZString)line["DocketType"] == "HCC")
				.OrderBy(line => line["IsPositiveHCC"]).ToArray();

			var positiveHoldCodeResults = results.Where(line => (ZString)line["DocketType"] == "HCC" && (ZBool)line["IsPositiveHCC"])
				.OrderBy(line => line["FinalisedDateTime"]).ToArray();
			AssertHoldCodeRow(positiveHoldCodeResults[0], transferLine, "HEL",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), 1m, true);
			AssertHoldCodeRow(positiveHoldCodeResults[1], receiveLine2, "DAM",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), 1m, true);
			AssertHoldCodeRow(positiveHoldCodeResults[2], transferLine, "SHATECLI",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), 1m, true);

			var negativeHoldCodeResults = results.Where(line => (ZString)line["DocketType"] == "HCC" && !(ZBool)line["IsPositiveHCC"])
				.OrderBy(line => line["FinalisedDateTime"]).ToArray();
			AssertHoldCodeRow(negativeHoldCodeResults[0], transferLine, "",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), -1m, false);
			AssertHoldCodeRow(negativeHoldCodeResults[1], receiveLine2, "",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), -1m, false);
			AssertHoldCodeRow(negativeHoldCodeResults[2], transferLine, "HEL",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), -1m, false);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		[TestDate(2023, 3, 4)]
		public void TestView_HoldCodeChangeDataFromCorrectTable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			receive.WD_FinalisedDate = new ZDateTimeOffset(2023, 2, 16);

			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = receiveLine.PK;
				log.SL_Table = "WhsDocketLine";
				log.SL_Reference = "Status Changed to LCC|NEW=LCC|TYP=Hold Code";
				log.SL_SE_NKEvent = "CID";
			}

			var sql = "select * from WhsStockMovementReport(null,null,null,null,null,null,'HCC') order by FinalisedDateTime";
			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load(sql);

			AssertEquals("2 Hold Code Changes(From and To Inventory considered 2 HCC logs).", 2, results.Count);
			AssertHoldCodeRow(results[0], receiveLine, "", ZDateTime.Now, -5m, false);
			AssertHoldCodeRow(results[1], receiveLine, "DAM", ZDateTime.Now, 5m, true);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_WithHoldCodeChanges_NoFromDate()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m,
				data.Whs1.FindLocation("A-1"), "PLT-1", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 15m,
				data.Whs1.FindLocation("A-2"), new ZDate(year, 1, 3), new ZDate(year, 1, 4), "SA1", "SA2", "SA3", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 7m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year - 1, 2, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year - 1, 2, 2);
			Factory.Save();

			inventory1.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			inventory1.InDocketLine.HeldCodeChangeQuantity = 3m;
			inventory1.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			inventory2.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory2.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			inventory3.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			inventory3.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var receiveLineQuery = new ZQuery();
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, receive1.PK);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, false);
			var newReceiveLine = Factory.LoadTop1<WhsReceiveLine>(receiveLineQuery);
			var receiveLineHoldCodeChangeLog1 = newReceiveLine.Logs.Find(l => l.IsHoldCodeChangeEvent()).Single();
			var receiveLineHoldCodeChangeLog2 =
				inventory2.InDocketLine.Logs.Find(l => l.IsHoldCodeChangeEvent()).Single();

			var holdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery() { OrderBy = "WHL_SystemCreateTimeUtc" });

			// necessary to have specific event times to test boundaries
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsInventoryHoldChangeLog_PreventUpdate", "dbo.WhsInventoryHoldChangeLog"))
			{
				// Inventory 1
				holdLogs[0].WHL_EventTime = new ZDateTimeOffset(2022, 02, 01, 06, 00, 00, TimeSpan.FromHours(10));
				// Inventory 2
				holdLogs[1].WHL_EventTime = new ZDateTimeOffset(2022, 02, 01, 12, 00, 00, TimeSpan.FromHours(10));
				// Inventory 3
				holdLogs[2].WHL_EventTime = new ZDateTimeOffset(2022, 02, 02, 06, 00, 00, TimeSpan.FromHours(10));
				Factory.Save();
			}

			var sql = @"select * from WhsStockMovementReport
						(
							null,
							null,
							null,
							null,
							null,
							@DateTo,
							'INW, ORD, ADJ, TFR, WOR, HCC'
						) order by FinalisedDateTime, QuantityActual";

			var results = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@DateTo", new ZDate(2022, 02, 02), WhsInventoryHoldChangeLogSchema.WHL_EventTime);
			results.Load(sql, sqlParams);

			AssertEquals(
				"3 Receive Lines + 6 Hold Code Changes.",
				3 + 6, results.Count);

			AssertResult(inventory1.InDocketLine, results[0], 10m, 0);
			AssertResult(inventory3.InDocketLine, results[1], 7m, 0);
			AssertResult(inventory2.InDocketLine, results[2], 15m, 0);

			AssertHoldCodeRow(results[3], inventory1.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), -3m, false);
			AssertHoldCodeRow(results[4], inventory1.InDocketLine, InventoryHoldCodes.Codes.Damaged,
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), 3m, true);

			AssertHoldCodeRow(results[5], inventory2.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), -15m, false);
			AssertHoldCodeRow(results[6], inventory2.InDocketLine, InventoryHoldCodes.Codes.Held,
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), 15m, true);

			AssertHoldCodeRow(results[7], inventory3.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), -7m, false);
			AssertHoldCodeRow(results[8], inventory3.InDocketLine, InventoryHoldCodes.Codes.LostInCycleCount,
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), 7m, true);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_WithHoldCodeChanges_NoToDate()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m,
				data.Whs1.FindLocation("A-1"), "PLT-1", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 15m,
				data.Whs1.FindLocation("A-2"), new ZDate(year, 1, 3), new ZDate(year, 1, 4), "SA1", "SA2", "SA3", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 7m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			receive1.WD_FinalisedDate = new ZDateTimeOffset(2022, 2, 1, 00, 00, 00, TimeSpan.FromHours(10));
			receive2.WD_FinalisedDate = new ZDateTimeOffset(2022, 2, 1, 01, 00, 00, TimeSpan.FromHours(10));
			Factory.Save();

			inventory1.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			inventory1.InDocketLine.HeldCodeChangeQuantity = 3m;
			inventory1.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			inventory2.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory2.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			inventory3.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			inventory3.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var receiveLineQuery = new ZQuery();
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, receive1.PK);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, false);
			var newReceiveLine = Factory.LoadTop1<WhsReceiveLine>(receiveLineQuery);
			var receiveLineHoldCodeChangeLog1 = newReceiveLine.Logs.Find(l => l.IsHoldCodeChangeEvent()).Single();
			var receiveLineHoldCodeChangeLog2 =
				inventory2.InDocketLine.Logs.Find(l => l.IsHoldCodeChangeEvent()).Single();

			var holdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery() { OrderBy = "WHL_SystemCreateTimeUtc" });

			// necessary to have specific event times to test boundaries
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsInventoryHoldChangeLog_PreventUpdate", "dbo.WhsInventoryHoldChangeLog"))
			{
				// Inventory 1
				holdLogs[0].WHL_EventTime = new ZDateTimeOffset(2022, 02, 01, 06, 00, 00, TimeSpan.FromHours(10));
				// Inventory 2
				holdLogs[1].WHL_EventTime = new ZDateTimeOffset(2022, 02, 01, 12, 00, 00, TimeSpan.FromHours(10));
				// Inventory 3
				holdLogs[2].WHL_EventTime = new ZDateTimeOffset(2022, 02, 02, 06, 00, 00, TimeSpan.FromHours(10));
				Factory.Save();
			}
			
			var sql = @"select * from WhsStockMovementReport
						(
							null,
							null,
							null,
							null,
							@DateFrom,
							null,
							'INW, ORD, ADJ, TFR, WOR, HCC'
						) order by FinalisedDateTime, QuantityActual";

			var results = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@DateFrom", new ZDate(2022, 01, 01), WhsInventoryHoldChangeLogSchema.WHL_EventTime);
			results.Load(sql, sqlParams);

			AssertEquals(
				"3 Receive Lines + 6 Hold Code Changes.",
				3 + 6, results.Count);

			AssertResult(inventory1.InDocketLine, results[0], 10m, 0);
			AssertResult(inventory3.InDocketLine, results[1], 7m, 0);
			AssertResult(inventory2.InDocketLine, results[2], 15m, 0);

			AssertHoldCodeRow(results[3], inventory1.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), -3m, false);
			AssertHoldCodeRow(results[4], inventory1.InDocketLine, InventoryHoldCodes.Codes.Damaged,
				((ZDateTimeOffset)holdLogs[0]["WHL_EventTime"]).ToDateTime(), 3m, true);

			AssertHoldCodeRow(results[5], inventory2.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), -15m, false);
			AssertHoldCodeRow(results[6], inventory2.InDocketLine, InventoryHoldCodes.Codes.Held,
				((ZDateTimeOffset)holdLogs[1]["WHL_EventTime"]).ToDateTime(), 15m, true);

			AssertHoldCodeRow(results[7], inventory3.InDocketLine, "",
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), -7m, false);
			AssertHoldCodeRow(results[8], inventory3.InDocketLine, InventoryHoldCodes.Codes.LostInCycleCount,
				((ZDateTimeOffset)holdLogs[2]["WHL_EventTime"]).ToDateTime(), 7m, true);
		}

		void AssertHoldCodeRow(DynamicBusinessObject result, WhsDocketLine inDocketLine, ZString expectedHoldCodeChange,
			ZDateTime expectedMovementTime, ZDecimal expectedChange, bool expectedIsPositiveHCC)
		{
			CombineAssertions(() =>
			{
				var docket = inDocketLine.Docket;
				AssertEquals("Location", inDocketLine.LocationString, result["Location"]);
				AssertEquals("Pallet ID", inDocketLine.WE_PalletID, result["PalletID"]);
				AssertEquals("Docket", inDocketLine.WE_WD, result["DocketPK"]);
				AssertEquals("Docket ID", docket.WD_DocketID, result["DocketID"]);
				AssertEquals("Hold Code Change", expectedHoldCodeChange, result["Reference"]);
				AssertEquals("Client", docket.WD_OH_Client, result["ClientPK"]);
				AssertEquals("Warehouse", docket.WD_WW_Whs, result["WarehousePK"]);
				AssertEquals("Hold Code Changes should be Shown as 'HCC'", "HCC", result["DocketType"]);
				AssertEquals("Hold Code Changes should be Shown as 'HCC'", "HCC", result["DocketSubType"]);
				AssertEquals("Service Level", docket.WD_RS_NKServiceLevel, result["ServiceLevel"]);
				AssertEquals("Movement Time", expectedMovementTime.Date, ((ZDateTime)result["FinalisedDateTime"]).Date);
				AssertEquals("Product", inDocketLine.WE_OP, result["ProductPK"]);
				AssertEquals("Part Attribute 1", inDocketLine.WE_PartAttrib1, result["PartAttrib1"]);
				AssertEquals("Part Attribute 2", inDocketLine.WE_PartAttrib2, result["PartAttrib2"]);
				AssertEquals("Part Attribute 3", inDocketLine.WE_PartAttrib3, result["PartAttrib3"]);
				AssertEquals("Serial Number", inDocketLine.WE_SerialNumber, result["SerialNumber"]);
				AssertEquals("Expiry Date", inDocketLine.WE_ExpiryDate, result["ExpiryDate"]);
				AssertEquals("Packing Date", inDocketLine.WE_PackingDate, result["PackingDate"]);
				AssertEquals("Quantity Changed", expectedChange, result["QuantityActual"]);
				AssertEquals("IsPositiveHCC", expectedIsPositiveHCC, result["IsPositiveHCC"]);
			});
		}

		ZDateTimeOffset UpdateHoldChangeLogEventTime(ZDateTimeOffset eventTime)
		{
			return eventTime.AddHours(1);
		}

		#endregion

		#region TestView_WorkOrder

		[TestDate(2019, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_WorkOrder()
		{
			var data = new TestEnviromentForStockMovement(Factory);

			var bomProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(bomProduct1, data.Part1);
			var bomProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(bomProduct2, data.Part2);
			var bomProduct3 = Helper.CreateProduct(data.Org1, "BOM3");
			Helper.CreateProductBOM(bomProduct3, data.Part1);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "115");
			workOrder.WD_RequiredDate = data.DayOne.ToZDateTime().ToOffset();

			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct1, 16m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct2, 20m);
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct3, 11m);

			var pick = Helper.CreatePickNew(workOrder);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(results1.Length, 2);
			AssertResult(workOrderLine1.BOM.ChildComponentLines.ElementAt(0), results1[0], -27m,
				CalculatePalletSpace(-16m, 5) + CalculatePalletSpace(-11m, 5),
				palletIdOverride: "PLT: 1-111",
				locationOverride: workOrderLine1.BOM.ChildComponentLines.ElementAt(0).PickLines[0].InventoryLine
					.Location);

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(results2.Length, 2);
			AssertResult(workOrderLine2.BOM.ChildComponentLines.ElementAt(0), results2[0], -20m, 0m,
				palletIdOverride: "PLT: 2-111",
				locationOverride: workOrderLine2.BOM.ChildComponentLines.ElementAt(0).PickLines[0].InventoryLine
					.Location);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var bomProduct4 = Helper.CreateProduct(data.Org1, "BOM4");
			Helper.CreateProductBOM(bomProduct4, part3);
			var cancelWorkOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "112");
			var orderLine4 = Helper.CreateWhsOrderLine(cancelWorkOrder, part3, 12m);

			Factory.Save();

			cancelWorkOrder.CancelReactivateDocket();
			AssertEquals(true, cancelWorkOrder.IsCancelled);

			var results3 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(results3.Count, 0);
		}

		#endregion

		#region TestView_WorkOrder_SerialNumber

		[TestDate(2022, 02, 01)]
		public void TestView_WorkOrder_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true);

			var bomProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(bomProduct, data.Part1);
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SN1");
			var receiveLine2 = CreateReceiveLine("SN2");
			var receiveLine3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "115");
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);

			var pick = Helper.CreatePickNew(workOrder);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Order IsFinalised", true, workOrder.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick.IsFinalised);
			Factory.Save();

			var result = LoadView(data.Whs1, data.Org1, data.Part1);
			AssertEquals("Should be 4 results", 4, result.Count);
			AssertResult(receiveLine1, result[0], 1m, 0m);
			AssertResult(receiveLine2, result[1], 1m, 0m);
			AssertResult(receiveLine3, result[2], 1m, 0m);
			AssertResult(workOrderLine1.BOM.ChildComponentLines.ElementAt(0), result[3], -1m, 0,
				locationOverride: inventoryLocation, expectedSerialNumber: "SN1");

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_WorkOrder_Unfinalised

		[TestDate(2022, 02, 01)]
		public void TestView_WorkOrder_Unfinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var today = ZDateTimeOffset.Today;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var bomProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomProduct3 = Helper.CreateProduct(data.Org1, "BOM3");
			Helper.CreateProductBOM(bomProduct1, data.Part1);
			Helper.CreateProductBOM(bomProduct2, data.Part2);
			Helper.CreateProductBOM(bomProduct3, data.Part1);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "115");
			workOrder.WD_RequiredDate = new ZDateTimeOffset(today.Year - 1, 1, 1);

			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct1, 16m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct2, 20m);
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct3, 11m);

			var pick = Helper.CreatePickNew(workOrder);

			foreach (WhsWorkOrderLine docketLine in workOrder.Lines)
			{
				docketLine.BOM.ChildComponentLines.ToList()
					.ForEach(componentLine => componentLine.PickLines.ToList()
						.ForEach(pickLine => pickLine.WZ_PickedDateTime = today.AddDays(-1)));
			}

			Assert("Work Order is not finalised", !workOrder.IsFinalised);
			Assert("Pick is not finalised", !pick.IsFinalised);
			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(2, results1.Length);

			// the in-Transit Transfer Line should be shown.
			var componentLine1 = workOrderLine1.BOM.ChildComponentLines.First();
			var transferLine1 = componentLine1.PickLines.Single().InventoryLine;
			AssertResult(transferLine1, results1[0], -27m,
				CalculatePalletSpace(-16m, 5) + CalculatePalletSpace(-11m, 5),
				locationOverride: ((WhsTransferLine)transferLine1).TransferFromLocation,
				finalisedDateOverride: today.AddDays(-1));

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(2, results2.Length);

			var componentLine2 = workOrderLine2.BOM.ChildComponentLines.First();
			var transferLine2 = componentLine2.PickLines.Single().InventoryLine;
			AssertResult(transferLine2, results2[0], -20m, 0m,
				locationOverride: ((WhsTransferLine)transferLine2).TransferFromLocation,
				finalisedDateOverride: today.AddDays(-1));
		}

		#endregion

		#region TestView_WorkOrder_Unfinalised_PickedDirectly

		[TestDate(2022, 02, 01)]
		public void TestView_WorkOrder_Unfinalised_PickedDirectly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var today = ZDateTimeOffset.Today;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var bomProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomProduct3 = Helper.CreateProduct(data.Org1, "BOM3");
			Helper.CreateProductBOM(bomProduct1, data.Part1);
			Helper.CreateProductBOM(bomProduct2, data.Part2);
			Helper.CreateProductBOM(bomProduct3, data.Part1);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "115");
			workOrder.WD_RequiredDate = new ZDateTimeOffset(today.Year - 1, 1, 1);

			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct1, 16m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct2, 20m);
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct3, 11m);

			var pick = Helper.CreatePickNew(workOrder);

			foreach (WhsWorkOrderLine docketLine in workOrder.Lines)
			{
				docketLine.BOM.ChildComponentLines.ToList()
					.ForEach(componentLine => componentLine.PickLines.ToList()
						.ForEach(pickLine => pickLine.WZ_PickedDateTime = today.AddDays(-1)));
			}

			Assert("Work Order is not finalised", !workOrder.IsFinalised);
			Assert("Pick is not finalised", !pick.IsFinalised);

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(2, results1.Length);

			var componentLine1 = workOrderLine1.BOM.ChildComponentLines.First();
			AssertResult(componentLine1, results1[0], -27m,
				CalculatePalletSpace(-16m, 5) + CalculatePalletSpace(-11m, 5),
				locationOverride: componentLine1.PickLines[0].InventoryLine.Location,
				finalisedDateOverride: today.AddDays(-1));

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(2, results2.Length);

			var componentLine2 = workOrderLine2.BOM.ChildComponentLines.First();
			AssertResult(componentLine2, results2[0], -20m, 0m,
				locationOverride: componentLine2.PickLines[0].InventoryLine.Location,
				finalisedDateOverride: today.AddDays(-1));
		}

		#endregion

		#region TestView_DynamicWorkOrder

		[TestDate(2019, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_DynamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var product1 = Helper.CreateProduct(data.Org1, "PRD1");
			var product2 = Helper.CreateProduct(data.Org1, "PRD2");
			var product3 = Helper.CreateProduct(data.Org1, "PRD3");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 75m, inwardProcessingLocation);
			recLine1.CustomsData.WB_EntryKey = "ENT1";
			receive1.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 75m, inwardProcessingLocation);
			recLine2.CustomsData.WB_EntryKey = "ENT2";
			receive2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive2);

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
			dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
			dynamicWorkOrder.WD_RequiredDate = new ZDateTimeOffset(ZDateTime.Today.Year, 1, 2);
			dynamicWorkOrder.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			var kitLine1 = Factory.New<WhsDynamicWorkOrderLine>();
			kitLine1.WE_WD = dynamicWorkOrder.PK;
			kitLine1.WE_OP = product1.PK;
			kitLine1.WE_TransactionQuantity = 16m;
			kitLine1.WE_F3_NKPackType = product1.OP_StockKeepingUnit;
			kitLine1.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var componentLine1 = Factory.New<WhsDynamicWorkOrderLine>();
			componentLine1.WE_WD = dynamicWorkOrder.PK;
			componentLine1.WE_OP = data.Part1.PK;
			componentLine1.WE_TransactionQuantity = 16m;
			componentLine1.WE_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
			componentLine1.WE_WE_ParentDocketLine = kitLine1.PK;

			var componentLine2 = Factory.New<WhsDynamicWorkOrderLine>();
			componentLine2.WE_WD = dynamicWorkOrder.PK;
			componentLine2.WE_OP = data.Part2.PK;
			componentLine2.WE_TransactionQuantity = 26m;
			componentLine2.WE_F3_NKPackType = data.Part2.OP_StockKeepingUnit;
			componentLine2.WE_WE_ParentDocketLine = kitLine1.PK;

			var kitLine2 = Factory.New<WhsDynamicWorkOrderLine>();
			kitLine2.WE_WD = dynamicWorkOrder.PK;
			kitLine2.WE_OP = product2.PK;
			kitLine2.WE_TransactionQuantity = 20m;
			kitLine2.WE_F3_NKPackType = product2.OP_StockKeepingUnit;
			kitLine2.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;

			var componentLine3 = Factory.New<WhsDynamicWorkOrderLine>();
			componentLine3.WE_WD = dynamicWorkOrder.PK;
			componentLine3.WE_OP = data.Part2.PK;
			componentLine3.WE_TransactionQuantity = 20m;
			componentLine3.WE_F3_NKPackType = data.Part2.OP_StockKeepingUnit;
			componentLine3.WE_WE_ParentDocketLine = kitLine2.PK;

			var kitLine3 = Factory.New<WhsDynamicWorkOrderLine>();
			kitLine3.WE_WD = dynamicWorkOrder.PK;
			kitLine3.WE_OP = product3.PK;
			kitLine3.WE_TransactionQuantity = 11m;
			kitLine3.WE_F3_NKPackType = product3.OP_StockKeepingUnit;
			kitLine3.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;

			var componentLine4 = Factory.New<WhsDynamicWorkOrderLine>();
			componentLine4.WE_WD = dynamicWorkOrder.PK;
			componentLine4.WE_OP = data.Part1.PK;
			componentLine4.WE_TransactionQuantity = 11m;
			componentLine4.WE_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
			componentLine4.WE_WE_ParentDocketLine = kitLine3.PK;

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Order IsFinalised", true, dynamicWorkOrder.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick.IsFinalised);
			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, data.Part1).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(2, results1.Length);
			AssertResult(componentLine1, results1[0], -16m, 0m,
				locationOverride: componentLine1.PickLines[0].InventoryLine.Location);

			var results2 = LoadView(data.Whs1, data.Org1, data.Part2).OrderBy(line => line["QuantityActual"]).ToArray();
			AssertEquals(2, results2.Length);
			AssertResult(componentLine2, results2[0], -26m, 0m,
				locationOverride: componentLine2.PickLines[0].InventoryLine.Location);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var product4 = Helper.CreateProduct(data.Org1, "BOM4");
			var cancelWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			cancelWorkOrder.WD_OH_Client = data.Org1.PK;
			cancelWorkOrder.WD_WW_Whs = data.Whs1.PK;
			cancelWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			cancelWorkOrder.ConsigneeAddressPK = data.Org1.MainAddress.PK;
			cancelWorkOrder.WD_DocketStatus = DocketStatus.Codes.Entered;

			var cancelKitLine1 = Factory.New<WhsDynamicWorkOrderLine>();
			var cancelComponentLine1 = Factory.New<WhsDynamicWorkOrderLine>();

			cancelKitLine1.WE_WD = cancelWorkOrder.PK;
			cancelKitLine1.WE_OP = product4.PK;
			cancelKitLine1.WE_TransactionQuantity = 12m;
			cancelKitLine1.WE_F3_NKPackType = product4.OP_StockKeepingUnit;
			cancelKitLine1.CustomsData.WB_IsMainInwardsProcessedItem = true;

			cancelComponentLine1.WE_WD = cancelWorkOrder.PK;
			cancelComponentLine1.WE_OP = part3.PK;
			cancelComponentLine1.WE_TransactionQuantity = 12m;
			cancelComponentLine1.WE_F3_NKPackType = part3.OP_StockKeepingUnit;
			cancelComponentLine1.WE_WE_ParentDocketLine = cancelKitLine1.PK;

			Factory.Save();

			cancelWorkOrder.CancelReactivateDocket();
			AssertEquals(true, cancelWorkOrder.IsCancelled);

			var results3 = LoadView(data.Whs1, data.Org1, part3);
			AssertEquals(0, results3.Count);
		}

		#endregion

		#region TestView_DocketLine_ProductPackType

		[TestDate(2022, 02, 01)]
		public void TestView_DocketLine_ProductPLTPackType_StockKeepingUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Part1.OP_StockKeepingUnit = Constants.PkgUnit.Pallet;
			var today = ZDateTime.Today;

			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "").Lines[0];
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "").Lines[0];
			Factory.Save();

			var receiveResults = LoadView().ToArray();
			AssertEquals("Should get 2 lines for each receive line.", 2, receiveResults.Length);

			AssertResult(receiveLine1, receiveResults.Where(line => line["Product"].ToString() == "P1").First(), 50m,
				50m);
			AssertResult(receiveLine2, receiveResults.Where(line => line["Product"].ToString() == "P2").First(), 50m,
				0m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 12m);

			var pick1 = Helper.CreatePickNew(order);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertEquals("Order IsFinalised", true, order.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick1.IsFinalised);
			Factory.Save();

			var allResults = LoadView().ToArray();
			AssertEquals("Should get 4 lines for receive and order lines.", 4, allResults.Length);
			AssertResult(receiveLine1,
				allResults.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 50m);
			AssertResult(receiveLine2,
				allResults.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 0m);

			var orderLine1Result = allResults
				.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine1 ExpectedActualQuantity", -11m, orderLine1Result["QuantityActual"]);
			AssertEquals("OrderLine1 Pallet Spaces", -11m, orderLine1Result["TotalPalletSpaces"]);

			var orderLine2Result = allResults
				.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine2 ExpectedActualQuantity", -12m, orderLine2Result["QuantityActual"]);
			AssertEquals("OrderLine2 Pallet Spaces", 0m, orderLine2Result["TotalPalletSpaces"]);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_DocketLine_ProductPLTPackType_PackType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, Constants.PkgUnit.Unit, 5m);
			var today = ZDateTime.Today;

			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "").Lines[0];
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "").Lines[0];
			Factory.Save();

			var receiveResults = LoadView().ToArray();
			AssertEquals("Should get 2 lines for each receive line.", 2, receiveResults.Length);

			AssertResult(receiveLine1, receiveResults.Where(line => line["Product"].ToString() == "P1").First(), 50m,
				250m);
			AssertResult(receiveLine2, receiveResults.Where(line => line["Product"].ToString() == "P2").First(), 50m,
				0m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 12m);

			var pick1 = Helper.CreatePickNew(order);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertEquals("Order IsFinalised", true, order.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick1.IsFinalised);
			Factory.Save();

			var allResults = LoadView().ToArray();
			AssertEquals("Should get 4 lines for receive and order lines.", 4, allResults.Length);
			AssertResult(receiveLine1,
				allResults.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 250m);
			AssertResult(receiveLine2,
				allResults.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 0m);

			var orderLine1Result = allResults
				.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine1 ExpectedActualQuantity", -11m, orderLine1Result["QuantityActual"]);
			AssertEquals("OrderLine1 Pallet Spaces", -55m, orderLine1Result["TotalPalletSpaces"]);

			var orderLine2Result = allResults
				.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine2 ExpectedActualQuantity", -12m, orderLine2Result["QuantityActual"]);
			AssertEquals("OrderLine2 Pallet Spaces", 0m, orderLine2Result["TotalPalletSpaces"]);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_DocketLine_ProductPLTPackType_ParentPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var today = ZDateTime.Today;

			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "").Lines[0];
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "").Lines[0];
			Factory.Save();

			var receiveResults = LoadView().ToArray();
			AssertEquals("Should get 2 lines for each receive line.", 2, receiveResults.Length);

			AssertResult(receiveLine1, receiveResults.Where(line => line["Product"].ToString() == "P1").First(), 50m,
				10m);
			AssertResult(receiveLine2, receiveResults.Where(line => line["Product"].ToString() == "P2").First(), 50m,
				0m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 12m);

			var pick1 = Helper.CreatePickNew(order);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertEquals("Order IsFinalised", true, order.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick1.IsFinalised);
			Factory.Save();

			var allResults = LoadView().ToArray();
			AssertEquals("Should get 4 lines for receive and order lines.", 4, allResults.Length);
			AssertResult(receiveLine1,
				allResults.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 10m);
			AssertResult(receiveLine2,
				allResults.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 0m);

			var orderLine1Result = allResults
				.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine1 ExpectedActualQuantity", -10m, orderLine1Result["QuantityActual"]);
			AssertEquals("OrderLine1 Pallet Spaces", -2m, orderLine1Result["TotalPalletSpaces"]);

			var orderLine2Result = allResults
				.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine2 ExpectedActualQuantity", -12m, orderLine2Result["QuantityActual"]);
			AssertEquals("OrderLine2 Pallet Spaces", 0m, orderLine2Result["TotalPalletSpaces"]);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_DocketLine_ProductPLTPackType_MultiplePLTConversions()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, Constants.PkgUnit.Unit, 5m);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Container, Constants.PkgUnit.Pallet, 12m);
			var today = ZDateTime.Today;

			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				data.Whs1.FindLocation("A-1"), "").Lines[0];
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m,
				data.Whs1.FindLocation("A-2"), "").Lines[0];
			Factory.Save();

			var receiveResults = LoadView().ToArray();
			AssertEquals("Should get 2 lines for each receive line.", 2, receiveResults.Length);

			AssertResult(receiveLine1, receiveResults.Where(line => line["Product"].ToString() == "P1").First(), 50m,
				250m);
			AssertResult(receiveLine2, receiveResults.Where(line => line["Product"].ToString() == "P2").First(), 50m,
				0m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "111", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 12m);

			var pick1 = Helper.CreatePickNew(order);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertEquals("Order IsFinalised", true, order.IsFinalised);
			AssertEquals("Pick IsFinalised", true, pick1.IsFinalised);
			Factory.Save();

			var allResults = LoadView().ToArray();
			AssertEquals("Should get 4 lines for receive and order lines.", 4, allResults.Length);
			AssertResult(receiveLine1,
				allResults.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 250m);
			AssertResult(receiveLine2,
				allResults.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "INW")
					.First(), 50m, 0m);

			var orderLine1Result = allResults
				.Where(line => line["Product"].ToString() == "P1" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine1 ExpectedActualQuantity", -10m, orderLine1Result["QuantityActual"]);
			AssertEquals("OrderLine1 Pallet Spaces", -50m, orderLine1Result["TotalPalletSpaces"]);

			var orderLine2Result = allResults
				.Where(line => line["Product"].ToString() == "P2" && line["DocketType"].ToString() == "ORD").First();
			AssertEquals("OrderLine2 ExpectedActualQuantity", -12m, orderLine2Result["QuantityActual"]);
			AssertEquals("OrderLine2 Pallet Spaces", 0m, orderLine2Result["TotalPalletSpaces"]);
		}

		#endregion

		#region TestView_PickByBOM_ShowKitReceive

		[TestDate(2022, 02, 01)]
		public void TestView_PickByBOM_KitReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);
			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			Factory.Save();

			var kitReceiveLine = orderLine1.PickLines[0].InventoryLine;

			var receiveResults1 = LoadView(data.Whs1, data.Org1, wheel)
				.Where(line => line["DocketType"].ToString() == "INW").ToArray();
			AssertEquals("Should get 1 receive line.", 1, receiveResults1.Length);
			AssertResult(receive1.Lines[0], receiveResults1[0], 50m, 0m);

			var receiveResults2 = LoadView(data.Whs1, data.Org1, frame)
				.Where(line => line["DocketType"].ToString() == "INW").ToArray();
			AssertEquals("Should get 1 receive line.", 1, receiveResults2.Length);
			AssertResult(receive2.Lines[0], receiveResults2[0], 25m, 0m);

			var receiveResults3 = LoadView(data.Whs1, data.Org1, bike).ToArray();
			AssertEquals("The WE_WL on Kit Receive Line is empty at the moment.", 0, receiveResults3.Length);

			pick.FinaliseAllOrders();
			pick.NotificationManager.Push(Notify);
			Notify.PreQueryUser += (sender, e) => ((DefaultableQueryUserEventArgs)e).Response = true;
			pick.FinalisePick();

			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			var kitReceive = orderLine1.PickLines[0].InventoryLineForAvailableInventory.Docket;
			AssertIsFinalisedPrecondition(kitReceive);

			var receiveResults4 = LoadView(data.Whs1, data.Org1, bike)
				.Where(line => line["DocketType"].ToString() == "KAS").ToArray();
			AssertResult(kitReceiveLine, receiveResults4[0], 5m, 0m, docketTypeOverride: "KAS");

			var receiveResults5 = LoadView(data.Whs1, data.Org1, wheel)
				.Where(line => line["DocketType"].ToString() == "KAS").ToArray();
			AssertResult(wheelOrderLine, receiveResults5[0], -10m, 0m, locationOverride: location, docketTypeOverride: "KAS");

			var receiveResults6 = LoadView(data.Whs1, data.Org1, frame)
				.Where(line => line["DocketType"].ToString() == "KAS").ToArray();
			AssertResult(frameOrderLine, receiveResults6[0], -5m, 0m, locationOverride: location, docketTypeOverride: "KAS");
		}

		#endregion

		#region TestView_PickByBOM_ComponentsPicked_InTransit

		[TestDate(2022, 02, 01)]
		public void TestView_PickByBOM_ComponentsPicked_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-1");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m, location1, palletID: "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 25m, location2, palletID: "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);

			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			var date1 = new ZDateTimeOffset(2022, 02, 01, 1, 1, 1);
			var date2 = new ZDateTimeOffset(2022, 02, 01, 1, 1, 2);
			wheelPickLine.WZ_PickedDateTime = date1;
			framePickLine.WZ_PickedDateTime = date2;
			Factory.Save();

			var kitReceiveLine = orderLine1.PickLines[0].InventoryLine;
			var wheelTransferLine = wheelOrderLine.PickLines[0].InventoryLine;
			var frameTransferLine = frameOrderLine.PickLines[0].InventoryLine;

			var results1 = LoadView(data.Whs1, data.Org1, wheel).ToArray();
			AssertEquals("Should get 1 receive line, and 1 transfer line.", 2, results1.Length);
			AssertResult(receive1.Lines[0], results1.Single(line => line["DocketType"].ToString() == "INW"), 50m, 0m);
			AssertResult(wheelTransferLine, results1.Single(line => line["DocketType"].ToString() == "TFR"), -10m, 0m, locationOverride: location1, finalisedDateOverride: date1);

			var results2 = LoadView(data.Whs1, data.Org1, frame).ToArray();
			AssertEquals("Should get 1 receive line, and 1 transfer line.", 2, results2.Length);
			AssertResult(receive2.Lines[0], results2.Single(line => line["DocketType"].ToString() == "INW"), 25m, 0m);
			AssertResult(frameTransferLine, results2.Single(line => line["DocketType"].ToString() == "TFR"), -5m, 0m, locationOverride: location2, finalisedDateOverride: date2);

			var results3 = LoadView(data.Whs1, data.Org1, bike).ToArray();
			AssertEquals("The WE_WL on Kit Receive Line is empty at the moment.", 0, results3.Length);
		}

		#endregion

		#region TestView_PickByBOM_ComponentsPicked_Putawayed

		[TestDate(2022, 02, 01)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_PickByBOM_ComponentsPicked_Putawayed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m, location1, palletID: "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 25m, location2, palletID: "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);

			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			var date1 = new ZDateTimeOffset(2022, 02, 01, 1, 1, 1);
			var date2 = new ZDateTimeOffset(2022, 02, 01, 1, 1, 2);
			wheelPickLine.WZ_PickedDateTime = date1;
			framePickLine.WZ_PickedDateTime = date2;
			Factory.Save();

			// create kit transfer line
			var wheelTransferLine = (WhsTransferLine)wheelOrderLine.PickLines[0].InventoryLine;
			var frameTransferLine = (WhsTransferLine)frameOrderLine.PickLines[0].InventoryLine;
			var componentTransferLines = new WhsTransferLine[] { wheelTransferLine, frameTransferLine };
			var (transferLinesToIgnoreWhenSettingLocation, kitPackages) = WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(Factory, data.Whs1.WW_DefaultOutboundDockDoor, componentTransferLines, GlbStaff.CurrentUser.GS_Code);

			// putaway by finalising component transfer lines
			var errorMessage = WhsPackingConsolidationService.PutawayTransferLines(data.Whs1.DefaultOutboundDockDoorLocation, componentTransferLines, transferLinesToIgnoreWhenSettingLocation);
			AssertEquals("Precondition", true, string.IsNullOrEmpty(errorMessage));
			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1, wheel).ToArray();
			AssertEquals("Should get 1 receive line, 2 transfer lines and 1 KAS line.", 4, results1.Length);
			AssertResult(receive1.Lines[0], results1.Single(line => line["DocketType"].ToString() == "INW"), 50m, 0m);
			AssertResult(wheelTransferLine, results1.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] < 0), -10m, 0m, locationOverride: location1, finalisedDateOverride: date1);
			AssertResult(wheelTransferLine, results1.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] > 0), 10m, 0m, locationOverride: location2);
			AssertResult(wheelOrderLine, results1.Single(line => line["DocketType"].ToString() == "KAS"), -10m, 0m, locationOverride: location2, finalisedDateOverride: ZDateTimeOffset.Now, docketTypeOverride: "KAS");

			var results2 = LoadView(data.Whs1, data.Org1, frame).ToArray();
			AssertEquals("Should get 1 receive line, 2 transfer lines and 1 KAS line.", 4, results2.Length);
			AssertResult(receive2.Lines[0], results2.Single(line => line["DocketType"].ToString() == "INW" && line["DocketType"].ToString() == "INW"), 25m, 0m);
			AssertResult(frameTransferLine, results2.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] < 0), -5m, 0m, locationOverride: location2, finalisedDateOverride: date2);
			AssertResult(frameTransferLine, results2.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] > 0), 5m, 0m, locationOverride: location2);
			AssertResult(frameOrderLine, results2.Single(line => line["DocketType"].ToString() == "KAS"), -5m, 0m, locationOverride: location2, finalisedDateOverride: ZDateTimeOffset.Now, docketTypeOverride: "KAS");

			var results3 = LoadView(data.Whs1, data.Org1, bike).ToArray();
			AssertEquals("Should get 1 KAS line, and 2 tranfser lines", 3, results3.Length);
			var kitPickLine = orderLine1.PickLines.Single();
			var kitReceiveLine = kitPickLine.InventoryLineForAvailableInventory;
			var kitTransferLine = kitPickLine.InventoryLine;
			AssertResult(kitReceiveLine, results3.Single(line => line["DocketType"].ToString() == "KAS"), 5m, 0m, finalisedDateOverride: date2, docketTypeOverride: "KAS");
			AssertResult(kitTransferLine, results3.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] < 0), -5m, 0m, locationOverride: location2, finalisedDateOverride: date2);
			AssertResult(kitTransferLine, results3.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] > 0), 5m, 0m);
		}

		#endregion

		#region TestView_PickByBOM_ComponentsPicked_OrderFinalised

		[TestDate(2022, 02, 01)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_PickByBOM_ComponentsPicked_OrderFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m, location1, palletID: "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 25m, location2, palletID: "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);

			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			var date1 = new ZDateTimeOffset(2022, 02, 01, 1, 1, 1);
			var date2 = new ZDateTimeOffset(2022, 02, 01, 1, 1, 2);
			wheelPickLine.WZ_PickedDateTime = date1;
			framePickLine.WZ_PickedDateTime = date2;
			Factory.Save();

			// create kit transfer line
			var wheelTransferLine = (WhsTransferLine)wheelOrderLine.PickLines[0].InventoryLine;
			var frameTransferLine = (WhsTransferLine)frameOrderLine.PickLines[0].InventoryLine;
			var componentTransferLines = new WhsTransferLine[] { wheelTransferLine, frameTransferLine };
			var (transferLinesToIgnoreWhenSettingLocation, kitPackages) = WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(Factory, data.Whs1.WW_DefaultOutboundDockDoor, componentTransferLines, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			// putaway by finalising component transfer lines
			var dockdoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var errorMessage = WhsPackingConsolidationService.PutawayTransferLines(dockdoor, componentTransferLines, transferLinesToIgnoreWhenSettingLocation);
			AssertEquals("Precondition", true, string.IsNullOrEmpty(errorMessage));
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			var kitReceive = orderLine1.PickLines[0].InventoryLineForAvailableInventory.Docket;
			AssertIsFinalisedPrecondition(kitReceive);

			var results1 = LoadView(data.Whs1, data.Org1, wheel).ToArray();
			AssertEquals("Should get 1 receive line, 2 transfer lines and 1 KAS line.", 4, results1.Length);
			AssertResult(receive1.Lines[0], results1.Single(line => line["DocketType"].ToString() == "INW"), 50m, 0m);
			AssertResult(wheelTransferLine, results1.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] < 0), -10m, 0m, locationOverride: location1, finalisedDateOverride: date1);
			AssertResult(wheelTransferLine, results1.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] > 0), 10m, 0m, locationOverride: location2);
			AssertResult(wheelOrderLine, results1.Single(line => line["DocketType"].ToString() == "KAS"), -10m, 0m, locationOverride: location2, docketTypeOverride: "KAS", finalisedDateOverride: wheelPickLine.WZ_PickedDateTime);

			var results2 = LoadView(data.Whs1, data.Org1, frame).ToArray();
			AssertEquals("Should get 1 receive line, 2 transfer lines and 1 KAS line.", 4, results2.Length);
			AssertResult(receive2.Lines[0], results2.Single(line => line["DocketType"].ToString() == "INW"), 25m, 0m);
			AssertResult(frameTransferLine, results2.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] < 0), -5m, 0m, locationOverride: location2, finalisedDateOverride: date2);
			AssertResult(frameTransferLine, results2.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] > 0), 5m, 0m, locationOverride: location2);
			AssertResult(frameOrderLine, results2.Single(line => line["DocketType"].ToString() == "KAS"), -5m, 0m, locationOverride: location2, docketTypeOverride: "KAS", finalisedDateOverride: framePickLine.WZ_PickedDateTime);

			var results3 = LoadView(data.Whs1, data.Org1, bike).ToArray();
			AssertEquals("Should get 1 KAS line, 2 tranfser lines, and 1 order line", 4, results3.Length);
			var kitPickLine = orderLine1.PickLines.Single();
			var kitReceiveLine = kitPickLine.InventoryLineForAvailableInventory;
			var kitTransferLine = kitPickLine.InventoryLine;
			AssertResult(kitReceiveLine, results3.Single(line => line["DocketType"].ToString() == "KAS"), 5m, 0m, finalisedDateOverride: date2, docketTypeOverride: "KAS");
			AssertResult(kitTransferLine, results3.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] < 0), -5m, 0m, locationOverride: location2, finalisedDateOverride: date2);
			AssertResult(kitTransferLine, results3.Single(line => line["DocketType"].ToString() == "TFR" && (ZDecimal)line["QuantityActual"] > 0), 5m, 0m);
			AssertResult(orderLine1, results3.Single(line => line["DocketType"].ToString() == "ORD"), -5m, 0m, locationOverride: dockdoor, finalisedDateOverride: kitPickLine.WZ_PickedDateTime);
		}

		#endregion

		#region TestView_PickByBOM_ComponentsPickedDirectedly_OrderFinalised

		[TestDate(2022, 02, 01)]
		[TestUtcOffset(10, 0, 0)]
		public void TestView_PickByBOM_ComponentsPickedDirectedly_OrderFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 25m);
			var location1 = receive2.Lines[0].WE_WL;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);
			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.NotificationManager.Push(Notify);
			Notify.PreQueryUser += (sender, e) => ((DefaultableQueryUserEventArgs)e).Response = true;
			pick.FinalisePick();

			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			var kitReceive = orderLine1.PickLines[0].InventoryLineForAvailableInventory.Docket;
			AssertIsFinalisedPrecondition(kitReceive);

			var results1 = LoadView(data.Whs1, data.Org1, wheel).ToArray();
			AssertEquals("Should get 1 receive line, 1 KAS line", 2, results1.Length);
			AssertResult(receive1.Lines[0], results1.Single(line => line["DocketType"].ToString() == "INW"), 50m, 0m);
			AssertResult(wheelOrderLine, results1.Single(line => line["DocketType"].ToString() == "KAS"), -10m, 0m, docketTypeOverride: "KAS", locationOverride: location);

			var results2 = LoadView(data.Whs1, data.Org1, frame).ToArray();
			AssertEquals("Should get 1 receive line, 1 KAS line", 2, results2.Length);
			AssertResult(receive2.Lines[0], results2.Single(line => line["DocketType"].ToString() == "INW"), 25m, 0m);
			AssertResult(frameOrderLine, results2.Single(line => line["DocketType"].ToString() == "KAS"), -5m, 0m, docketTypeOverride: "KAS", locationOverride: location);

			var results3 = LoadView(data.Whs1, data.Org1, bike).ToArray();
			AssertEquals("Should get 1 KAS line, 1 order line", 2, results3.Length);
			var kitPickLine = orderLine1.PickLines.Single();
			var kitReceiveLine = kitPickLine.InventoryLine;
			AssertResult(kitReceiveLine, results3.Single(line => line["DocketType"].ToString() == "KAS"), 5m, 0m, docketTypeOverride: "KAS", locationOverride: location);
			AssertResult(orderLine1, results3.Single(line => line["DocketType"].ToString() == "ORD"), -5m, 0m, locationOverride: location);
		}

		#endregion

		#region TestView_FixedWidthLocationWarehouse

		[TestDate(2022, 02, 01)]
		public void TestView_FixedWidthLocationWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 4, 3, 2);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, warehouse, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 111m, warehouse.FindLocation("LOCZ0040302"), "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var results = LoadView(warehouse, data.Org1, data.Part1);
			var result = results.Single();
			AssertEquals("LOCZ-004-03-02", result["Location"].ToString());
			AssertResult(receive.Lines[0], result, 111m, CalculatePalletSpace(111m, 5));
		}

		#endregion

		#region CalculatePalletSpace

		decimal CalculatePalletSpace(decimal units, int unitsPerPallet)
		{
			return Math.Sign(units) * Math.Ceiling(Math.Abs(units / unitsPerPallet));
		}

		#endregion

		#region AssertResult

		void AssertResult(WhsDocketLine docketLine, DynamicBusinessObject result, decimal expectedActualQuantity,
			decimal expectedPalletSpaces, int releaseLineIndex = 0, string palletIdOverride = null,
			WhsLocation locationOverride = null, ZDateTimeOffset? finalisedDateOverride = null,
			string expectedSerialNumber = "", WhsPickLine pickLine = null, string docketTypeOverride = null)
		{
			var docket = docketLine.Docket;
			var tranType = docket.WD_DocketType;
			var supplierPart = docketLine.SupplierPart;
			var location = locationOverride ?? docketLine.Location;

			// product category
			var category = GetProductCategoryFromDocketLine(docketLine);
			var expectedCategoryCode = category?.OPC_CategoryCode ?? ZString.Empty;
			var expectedCategoryDescription = category?.OPC_CategoryDescription ?? ZString.Empty;

			var expectedCommodityPK = (!supplierPart.OP_RH_NKCommodityCode.IsEmpty)
				? supplierPart.CommodityCode.PK
				: ZGuid.Empty;
			var expectedFinalisedDate = finalisedDateOverride ?? (tranType == DocketType.Codes.Transfer
				? docketLine.WE_FinalisedDate
				: docket.WD_FinalisedDate);

			AssertEquals("WarehousePK", docket.Warehouse.PK, result["WarehousePK"]);
			AssertEquals("WarehouseName", docket.Warehouse.WW_WarehouseName, result["WarehouseName"]);
			AssertEquals("ClientPK", docket.Client.PK, result["ClientPK"]);
			AssertEquals("ClientCode", docket.Client.OH_Code, result["ClientCode"]);
			AssertEquals("ClientFullName", docket.Client.OH_FullName, result["ClientFullName"]);
			AssertEquals("PalletID", palletIdOverride ?? docketLine.WE_PalletID, result["PalletID"]);
			AssertEquals("LocationString", location?.WLV_LocationString_UserFriendly, result["Location"]);
			AssertEquals("LocationIndexForSort", WhsSqlViewHelper.GetLocationIndexForSort(location),
				result["LocationIndexForSort"]);
			AssertEquals("Reference", docket.WD_ExternalReference, result["Reference"]);
			AssertEquals("ServiceLevel", docket.WD_RS_NKServiceLevel, result["ServiceLevel"]);
			AssertEquals("FinalisedDate", expectedFinalisedDate.Date, ((ZDateTime)result["FinalisedDate"]));
			AssertEquals("FinalisedDateTime", expectedFinalisedDate.ToString("dd-MMM-yyyy hh:mm"), ((ZDateTime)result["FinalisedDateTime"]).ToString("dd-MMM-yyyy hh:mm"));

			AssertEquals("ProductPK", supplierPart.PK, result["ProductPK"]);
			AssertEquals("Product", supplierPart.OP_PartNum, result["Product"]);
			AssertEquals("ProductDesc", supplierPart.OP_Desc, result["ProductDesc"]);
			AssertEquals("ProductCategoryCode", expectedCategoryCode, result["ProductCategoryCode"]);
			AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
				result["ProductCategoryDescription"]);
			AssertEquals("CommodityCode", supplierPart.OP_RH_NKCommodityCode, result["CommodityCode"]);
			AssertEquals("CommodityPK", expectedCommodityPK, result["CommodityPK"]);
			AssertEquals("DocketType", docketTypeOverride ?? tranType, result["DocketType"]);
			AssertEquals("IsPositiveHCC", false, result["IsPositiveHCC"]);

			if (tranType == DocketType.Codes.Order)
			{
				var order = (WhsOrder)docket;
				if (order.ConsigneeDocAddress.E2_AddressOverride)
				{
					AssertEquals("ConsigneePK", ZGuid.Empty, result["ConsigneePK"]);
					AssertEquals("ConsigneeName", order.ConsigneeDocAddress.E2_CompanyName, result["ConsigneeName"]);
				}
				else
				{
					AssertEquals("ConsigneePK", order.Consignee.PK, result["ConsigneePK"]);
					AssertEquals("ConsigneeName", order.Consignee.OH_FullName, result["ConsigneeName"]);
				}

				if (docketLine.WE_WE_ParentDocketLine.IsEmpty)
				{
					AssertEquals("ExpiryDate", ((WhsOrderLine)docketLine).ReleaseLines[releaseLineIndex].ExpiryDate,
					((ZDateTime)result["ExpiryDate"]));
					AssertEquals("PackingDate", ((WhsOrderLine)docketLine).ReleaseLines[releaseLineIndex].PackingDate,
						((ZDateTime)result["PackingDate"]));
					AssertEquals("PartAttrib1", pickLine == null ? ((WhsOrderLine)docketLine).ReleaseLines[releaseLineIndex].PartAttribute1 : pickLine.WZ_ReleaseCapturedPartAttrib1,
						result["PartAttrib1"]);
					AssertEquals("PartAttrib2", pickLine == null ? ((WhsOrderLine)docketLine).ReleaseLines[releaseLineIndex].PartAttribute2 : pickLine.WZ_ReleaseCapturedPartAttrib2,
						result["PartAttrib2"]);
					AssertEquals("PartAttrib3", pickLine == null ? ((WhsOrderLine)docketLine).ReleaseLines[releaseLineIndex].PartAttribute3 : pickLine.WZ_ReleaseCapturedPartAttrib3,
						result["PartAttrib3"]);
					AssertEquals("SerialNumber", expectedSerialNumber, result["SerialNumber"]);
				}
			}
			else if (tranType == DocketType.Codes.WorkOrder || tranType == DocketType.Codes.DynamicWorkOrder)
			{
				var pickLineInventory = ((WhsComponentOrderLine)docketLine).PickLines[0].Inventory;
				AssertEquals("ExpiryDate", pickLineInventory.WI_ExpiryDate, ((ZDateTime)result["ExpiryDate"]));
				AssertEquals("PackingDate", pickLineInventory.WI_PackingDate, ((ZDateTime)result["PackingDate"]));
				AssertEquals("PartAttrib1", pickLineInventory.WI_PartAttrib1, result["PartAttrib1"]);
				AssertEquals("PartAttrib2", pickLineInventory.WI_PartAttrib2, result["PartAttrib2"]);
				AssertEquals("PartAttrib3", pickLineInventory.WI_PartAttrib3, result["PartAttrib3"]);
				AssertEquals("SerialNumber", pickLineInventory.WI_SerialNumber, result["SerialNumber"]);
			}
			else
			{
				AssertEquals("ExpiryDate", docketLine.WE_ExpiryDate, ((ZDateTime)result["ExpiryDate"]));
				AssertEquals("PackingDate", docketLine.WE_PackingDate, ((ZDateTime)result["PackingDate"]));
				AssertEquals("PartAttrib1", docketLine.WE_PartAttrib1, result["PartAttrib1"]);
				AssertEquals("PartAttrib2", docketLine.WE_PartAttrib2, result["PartAttrib2"]);
				AssertEquals("PartAttrib3", docketLine.WE_PartAttrib3, result["PartAttrib3"]);
				AssertEquals("SerialNumber", docketLine.WE_SerialNumber, result["SerialNumber"]);
			}

			AssertEquals("expectedActualQuantity", expectedActualQuantity, result["QuantityActual"]);
			AssertEquals("QuantityUQ", supplierPart.OP_StockKeepingUnit, result["QuantityUQ"]);
			AssertEquals("Pallet Spaces", expectedPalletSpaces, result["TotalPalletSpaces"]);
		}

		OrgPartCategory GetProductCategoryFromDocketLine(WhsDocketLine docketLine)
		{
			var docket = docketLine.Docket;
			var client = docket.Client;
			var supplierPart = docketLine.SupplierPart;
			var orgRelation = supplierPart.RelatedOrganisations.Cast<OrgPartRelation>()
				.Single(r => r.OU_OH == client.PK && (r.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ||
													  r.OU_Relationship == OrgPartRelation.RelationshipTypes.Both));
			return orgRelation.Category;
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = @"
select
	*
from
	WhsStockMovementReport(null,null,null,null,null,null,'INW, ORD, ADJ, TFR, WOR, DWO, HCC, KAS')
where
	WarehousePK = @WarehousePK and
	ClientPK = @ClientPK and
	ProductPK = @ProductPK
order by
	DocketType,
	Reference,
	Location,
	SerialNumber";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client },
				{ "@ProductPK", part.PK, WhsDocketLineSchema.WE_OP }
			};
			result.Load(sql, sqlParams);

			return result;
		}

		DynamicBusinessObjectCollection LoadView()
		{
			var sql =
				@"select * from WhsStockMovementReport(null,null,null,null,null,null,'INW, ORD, ADJ, TFR, WOR, DWO, HCC') order by QuantityActual desc";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			return result;
		}

		#endregion

		#region Implementation

		class TestEnviromentForStockMovement : TestDataSimpleEnvironment
		{
			#region Constructors

			public TestEnviromentForStockMovement(BusinessObjectFactory factory)
				: base(factory, 2, 2)
			{
			}

			#endregion

			#region CreateEnvironment

			protected override void CreateEnvironment()
			{
				base.CreateEnvironment();

				Consignee1 = Helper.CreateClient("3", "3");
				Consignee2 = Helper.CreateClient("4", "4");
				Whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
				Factory.Save();

				var year = ZDateTime.Today.Year;
				DayOne = new ZDate(year, 1, 2);
				DayTwo = new ZDate(year, 2, 1);

				Helper.SetClientAllAttributeType(Org1, false);
				Helper.SetProductAllAttributeUse(Org1, Part1, use: true, setReleaseCaptured: false,
					useSerialNumber: false);
				Helper.SetProductAllAttributeUse(Org1, Part2, use: true, setReleaseCaptured: false,
					useSerialNumber: false);

				var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
				Part1.OP_RH_NKCommodityCode = commodity.RH_Code;

				Helper.CreateProductUnit(Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);

				Receive = Helper.CreateWhsReceive(Org1, Whs1, "111", Helper.Notify);
				Helper.CreateWhsReceiveInventoryLine(Receive, Part1, 111m, null, "PLT: 1-" + "111", DayOne, DayTwo,
					"PA1", "PA2", "PA3", "BEK");
				Helper.CreateWhsReceiveInventoryLine(Receive, Part2, 121m, null, "PLT: 2-" + "111", DayOne, DayTwo,
					"PA1", "PA2", "PA3", "BEK");
				Helper.CreateWhsReceiveInventoryLine(Receive, Part1, 22m, null, "PLT: 1-" + "111", DayOne, DayTwo,
					"PA1", "PA2", "PA3", "BEK");

				Receive.AllocateLocationsWithMock();
				Receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(Receive);

				Factory.Save();
			}

			#endregion

			public WhsWarehouse Whs2;
			public WhsReceive Receive;
			public OrgHeader Consignee1;
			public OrgHeader Consignee2;
			public ZDate DayOne;
			public ZDate DayTwo;
		}

		#endregion
	}
}
