using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class TryToTransferStockTest : WhsTransferSecureServiceTestCase
	{
		#region TestTryToTransferStock

		[TestDate(2012, 4, 19, 5, 5, 0)]
		public void TestTryToTransferStock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = now;

			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			AssertPutawayTransferLineData(transferLine, true, "A-2", "PLT-1", now, "A.A");
			AssertTransferPutawayResponse(response, null, true, true, false, true, true, false, true);
			AssertEquals("Even if all Transfer Lines are finalised, the job itself should not be finalised.", false, transfer.IsFinalised);

			AssertTransferEventsCreated(transfer, 1, 0, 0, 0, 0, 0);
		}

		#endregion

		#region TestTryToTransferStock_PutawayPartialStock

		[TestDate(2014, 11, 13)]
		public void TestTryToTransferStock_PutawayPartialStock()
		{
			TestTryToTransferStock_PutawayPartialStock(setPutawayByAndLocationPriorToCallingWebService: false);
		}

		[TestDate(2014, 11, 13)]
		public void TestTryToTransferStock_PutawayPartialStock_WithPutawayBySet()
		{
			TestTryToTransferStock_PutawayPartialStock(setPutawayByAndLocationPriorToCallingWebService: true);
		}

		void TestTryToTransferStock_PutawayPartialStock(bool setPutawayByAndLocationPriorToCallingWebService)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = now;

			if (setPutawayByAndLocationPriorToCallingWebService)
			{
				transferLine.WE_GS_NKPutawayBy = user.GS_Code;
				transferLine.LocationString = "A-2";
			}

			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", 1m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("Precondition - Transfer must not be finalised.", false, transfer.IsFinalised);
				AssertEquals("Precondition - Transfer must have two lines.", 2, transfer.Lines.Count);
			});

			AssertPutawayTransferLineData(transferLine, false, setPutawayByAndLocationPriorToCallingWebService ? "A-2" : "", "", ZDateTimeOffset.Empty, setPutawayByAndLocationPriorToCallingWebService ? "A.A" : "");
			AssertPutawayTransferLineData((WhsTransferLine)transfer.Lines.Single(l => l.PK != transferLine.PK), true, "A-2", "", now, "A.A");

			AssertTransferPutawayResponse(response, null, false, false, true, true, true, true, false);
		}

		#endregion

		#region TestTryToTransferStock_PutawayPartialStock_InterWhs

		[TestDate(2014, 11, 13)]
		public void TestTryToTransferStock_PutawayPartialStock_InterWhs_Dest()
		{
			TestTryToTransferStock_PutawayPartialStock_InterWhs_Core(isSource: false);
		}

		[TestDate(2014, 11, 13)]
		public void TestTryToTransferStock_PutawayPartialStock_InterWhs_Source()
		{
			TestTryToTransferStock_PutawayPartialStock_InterWhs_Core(isSource: true);
		}

		void TestTryToTransferStock_PutawayPartialStock_InterWhs_Core(bool isSource)
		{
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = now;
			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created child line.", childLine);
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A", 1m);

			var webService = GetNewWebService(transferWhs, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("Precondition - Transfer must not be finalised.", false, transfer.IsFinalised);
				AssertEquals("Precondition - ChildTransfer must not be finalised.", false, childTransfer.IsFinalised);
				AssertEquals("Precondition - Transfer must have two lines.", 2, transfer.Lines.Count);
				AssertEquals("Precondition - ChildTransfer must have two lines.", 2, childTransfer.Lines.Count);
			});

			AssertPutawayTransferLineData(transferLine, false, "A", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData((WhsTransferLine)transfer.Lines.Single(l => l.PK != transferLine.PK), true, "A", "", now, "A.A");
			AssertTransferPutawayResponse(response, null, false, false, true, true, true, true, false);

			CombineAssertions(() =>
			{
				AssertEquals("Finalised line should have 1 unit.", true, transfer.Lines.Any(l => l.WE_TransactionQuantity == 1m && l.IsFinalised));
				AssertEquals("Finalised line should have 1 unit.", true, childTransfer.Lines.Any(l => l.WE_TransactionQuantity == 1m && l.IsFinalised));
				AssertEquals("Unfinalised line should have 49 unit.", true, transfer.Lines.Any(l => l.WE_TransactionQuantity == 49m && ((WhsTransferLine)l).IsPicked && !l.IsFinalised));
				AssertEquals("Unfinalised line should have 49 unit.", true, childTransfer.Lines.Any(l => l.WE_TransactionQuantity == 49m && ((WhsTransferLine)l).IsPicked && !l.IsFinalised));
			});

			var destinationTransfer = isSource ? childTransfer : transfer;
			CombineAssertions(() =>
			{
				AssertEquals("Finalised line should have 1 unit.", true, destinationTransfer.Lines.Any(l => l.WE_StockOnHand == 1m && l.WE_TransactionQuantity == 1m));
				AssertEquals("Unfinalised line should have 49 unit.", true, destinationTransfer.Lines.Any(l => l.WE_StockOnHand == 49m && l.WE_TransactionQuantity == 49m));
			});
		}

		#endregion

		#region TestTryToTransferStock_SpecifiedLine

		[TestDate(2012, 7, 30, 5, 5, 0)]
		public void TestTryToTransferStock_SpecifiedLine_LineIsPutawayFully()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.PickedTime = now;

			Helper.Factory.Save();

			// Line is putaway fully
			var transferLineInfo = new WhsDocketLineInfo(transferLine);
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			AssertPutawayTransferLineData(transferLine, true, "A-2", "", now, "A.A");
			AssertTransferPutawayResponse(response, null, true, false, true, true, true, true, false);

			CombineAssertions(() =>
			{
				AssertEquals(10m, response.TotalQuantityAvailableForPutaway);
				AssertEquals(10m, response.TotalQuantityTransferred);
			});
		}

		[TestDate(2012, 7, 30, 5, 5, 0)]
		public void TestTryToTransferStock_SpecifiedLine_LineIsPutawayPartially()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.PickedTime = now;

			Helper.Factory.Save();

			// Line is putaway partially
			var transferLineInfo1 = new WhsDocketLineInfo(transferLine) { Qty = 3m };

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

				AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, transfer.Lines.Count);
				AssertEquals("When transfer line is partially transferred, then original lines Qty should be reduced.", 7m, transferLine.WE_TransactionQuantity);
			});

			var transferLineCreated = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);
			AssertPutawayTransferLineData(transferLine, false, "A-2", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLineCreated, true, "A-2", "", now, "A.A");

			AssertTransferPutawayResponse(response1, null, false, false, true, true, true, true, false);

			CombineAssertions(() =>
			{
				AssertEquals(10m, response1.TotalQuantityAvailableForPutaway);
				AssertEquals(3m, response1.TotalQuantityTransferred);
			});

			// Last Line is putaway fully
			var transferLineInfo2 = new WhsDocketLineInfo(transferLine);

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			AssertPutawayTransferLineData(transferLine, true, "A-2", "", now, "A.A");
			AssertTransferPutawayResponse(response2, null, true, false, true, true, true, true, false);

			CombineAssertions(() =>
			{
				AssertEquals(7m, response2.TotalQuantityAvailableForPutaway);
				AssertEquals(7m, response2.TotalQuantityTransferred);
			});
		}

		#endregion

		#region TestTryToTransferStock_MatchingLines

		[TestDate(2012, 7, 30, 5, 5, 0)]
		public void TestTryToTransferStock_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			Helper.CreateMatchingLine(transferLine, 7m);
			transferLine.PickedTime = now;

			AssertEquals("Precondition: ", 10m + 7m, transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("Precondition: ", 10m + 7m, transferLine.QtyCommittedIncludingMatchingLines);
			Helper.Factory.Save();

			var transferLineInfo = new WhsDocketLineInfo(transferLine) { Qty = 3m };

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, transfer.Lines.Count);
				AssertEquals("When transfer line is partially transferred, then original lines Qty should be reduced.", 10m + 7m - 3m, transferLine.QtyToMoveIncludingMatchingLines);
			});

			var transferLineCreated = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);
			AssertEquals("Created line should not have matching lines", ZGuid.Empty, transferLineCreated.WE_WE_MatchingLine);

			AssertPutawayTransferLineData(transferLine, false, "A-2", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLineCreated, true, "A-2", "", now, "A.A");
			AssertTransferPutawayResponse(response, null, false, false, true, true, true, true, false);

			CombineAssertions(() =>
			{
				AssertEquals(17m, response.TotalQuantityAvailableForPutaway);
				AssertEquals(3m, response.TotalQuantityTransferred);
			});
		}

		[TestDate(2012, 7, 30, 5, 5, 0)]
		public void TestTryToTransferStock_MatchingLines_WithAttribute()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "A-2");
			transferLine.WE_PartAttrib1 = "PA1";
			Helper.CreateMatchingLine(transferLine, 5m);
			Helper.CreateMatchingLine(transferLine, 1m);
			transferLine.PickedTime = now;
			Helper.Factory.Save();

			AssertEquals("Precondition: ", 5m + 5m + 1m, transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("Precondition: ", 5m + 5m + 1m, transferLine.QtyCommittedIncludingMatchingLines);

			var transferLineInfo = new WhsDocketLineInfo(transferLine) { Qty = 4m };

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, transfer.Lines.Count);
			AssertEquals("When transfer line is partially transferred, then QtyToMove should be reduced.", 5m + 5m + 1m - 4m, transferLine.QtyToMoveIncludingMatchingLines);

			AssertEquals(11m, response.TotalQuantityAvailableForPutaway);
			AssertEquals(4m, response.TotalQuantityTransferred);

			AssertTransferPutawayResponse(response, null, false, false, true, true, true, true, false);
			AssertPutawayTransferLineData(transferLine, false, "A-2", "", ZDateTimeOffset.Empty, "");

			var transferLineCreated = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 1m);
			AssertEquals("Created line should not have matching lines", ZGuid.Empty, transferLineCreated.WE_WE_MatchingLine);

			var matchingLine = (WhsTransferLine)transferLineCreated.MatchingLines.Single();
			AssertNotNull("Created line should have one matching lines", matchingLine);
			AssertEquals("Created line should have correct quantity", 3m, matchingLine.WE_TransactionQuantity);

			AssertPutawayTransferLineData(transferLineCreated, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(matchingLine, true, "A-2", "", now, "A.A");
		}

		#endregion

		#region TestTryToTransferStock_SpecifiedLine_PalletOverMultipleLines

		[TestDate(2012, 10, 9, 5, 5, 0)]
		public void TestTryToTransferStock_SpecifiedLine_PalletOverMultipleLines()
		{
			// This test ensures that transferring a pallet ID split over multiple lines is valid in RF
			// In previous builds this required interesting "workarounds" to finalise all lines at the end (this is no longer necessary)
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 7m, "A-1", "PLT-1", "A-2", "PLT-1");
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var transferLineInfo1 = new WhsDocketLineInfo(transferLine1);
			var transferLineInfo2 = new WhsDocketLineInfo(transferLine2);

			// Make this resemble an "End-To-End" test, these methods are called prior to TryToTransferStock
			var preconditionsWebService = GetNewWebService(data.Whs1, user);
			preconditionsWebService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			preconditionsWebService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			preconditionsWebService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertEquals("Precondition: All lines should be picked.", true, transfer.Lines.Cast<WhsTransferLine>().All(l => l.IsPicked));

			// Transfer the first part of the pallet ID.
			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			});

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals("No new transfer lines should be created.", 2, transferInOtherFactory.Lines.Count);

			var transferLine1InOtherFactory = transferInOtherFactory.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var transferLine2InOtherFactory = transferInOtherFactory.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 7m);

			AssertPutawayTransferLineData(transferLine1InOtherFactory, true, "A-2", "PLT-1", now, "A.A");
			AssertPutawayTransferLineData(transferLine2InOtherFactory, false, "A-2", "PLT-1", ZDateTimeOffset.Empty, "A.A");
			AssertTransferPutawayResponse(response1, null, isAllTransferLinesTransferredOrFinalised: false, isFullPalletIDTransfered: false, isSingleProductTransfered: true, isValidDestLocation: true, isValidDestPalletID: true, isValidProduct: true, isValidSourcePalletID: false);

			CombineAssertions(() =>
			{
				AssertEquals(3m, response1.TotalQuantityAvailableForPutaway);
				AssertEquals(3m, response1.TotalQuantityTransferred);
			});

			Helper.Factory.Save();

			// Transfer the second line

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			var otherFactory2 = new BusinessObjectFactory();
			var transferInOtherFactory2 = otherFactory2.Load<WhsTransfer>(transfer.PK);
			AssertEquals("When transfer line is fully transferred, then it should not be split.", 2, transferInOtherFactory2.Lines.Count);

			var transferLine1InOtherFactory2 = transferInOtherFactory2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 7m);
			var transferLine2InOtherFactory2 = transferInOtherFactory2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);

			AssertPutawayTransferLineData(transferLine1InOtherFactory2, true, "A-2", "PLT-1", now, "A.A");
			AssertPutawayTransferLineData(transferLine2InOtherFactory2, true, "A-2", "PLT-1", now, "A.A");
			AssertTransferPutawayResponse(response2, null, isAllTransferLinesTransferredOrFinalised: true, isFullPalletIDTransfered: false, isSingleProductTransfered: true, isValidDestLocation: true, isValidDestPalletID: true, isValidProduct: true, isValidSourcePalletID: false);

			CombineAssertions(() =>
			{
				AssertEquals(7m, response2.TotalQuantityAvailableForPutaway);
				AssertEquals(7m, response2.TotalQuantityTransferred);
			});
		}

		#endregion

		#region TestTryToTransferStock_SpecifiedLine_WithSplit

		[TestDate(2012, 10, 9, 5, 5, 0)]
		public void TestTryToTransferStock_SpecifiedLine_WithSplit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-1", "A-2", "PLT-1");
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			// A Specified Transfer Line should be Put Away even if there is an Error.
			var transferLineInfo1 = new WhsDocketLineInfo(transferLine) { Qty = 3m };

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			});

			var otherFactory = new BusinessObjectFactory();
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, transferInOtherFactory.Lines.Count);

			var transferLine1InOtherFactory = transferInOtherFactory.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 7m);
			var transferLine2InOtherFactory = transferInOtherFactory.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);

			AssertPutawayTransferLineData(transferLine1InOtherFactory, false, "A-2", "PLT-1", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2InOtherFactory, true, "A-2", "PLT-1", now, "A.A");
			AssertTransferPutawayResponse(response1, null, isAllTransferLinesTransferredOrFinalised: false, isFullPalletIDTransfered: false, isSingleProductTransfered: true, isValidDestLocation: true, isValidDestPalletID: true, isValidProduct: true, isValidSourcePalletID: false);

			CombineAssertions(() =>
			{
				AssertEquals(10m, response1.TotalQuantityAvailableForPutaway);
				AssertEquals(3m, response1.TotalQuantityTransferred);
			});

			// When putting away some lines, the system should try to finalise not only the specified line, but also all lines with transferred status.
			var transferLineInfo2 = new WhsDocketLineInfo(transferLine1InOtherFactory);

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			var otherFactory2 = new BusinessObjectFactory();
			var transferInOtherFactory2 = otherFactory2.Load<WhsTransfer>(transfer.PK);
			AssertEquals("When transfer line is fully transferred, then it should not be split.", 2, transferInOtherFactory2.Lines.Count);

			var transferLine1InOtherFactory2 = transferInOtherFactory2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 7m);
			var transferLine2InOtherFactory2 = transferInOtherFactory2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 3m);

			AssertPutawayTransferLineData(transferLine1InOtherFactory2, true, "A-2", "PLT-1", now, "A.A");
			AssertPutawayTransferLineData(transferLine2InOtherFactory2, true, "A-2", "PLT-1", now, "A.A");
			AssertTransferPutawayResponse(response2, null, isAllTransferLinesTransferredOrFinalised: true, isFullPalletIDTransfered: false, isSingleProductTransfered: true, isValidDestLocation: true, isValidDestPalletID: true, isValidProduct: true, isValidSourcePalletID: false);

			CombineAssertions(() =>
			{
				AssertEquals(7m, response2.TotalQuantityAvailableForPutaway);
				AssertEquals(7m, response2.TotalQuantityTransferred);
			});
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseFullPallet_MultipleLines

		[TestDate(2012, 4, 18, 5, 5, 0)]
		public void TestTryToTransferStock_PopulateAndFinaliseFullPallet_MultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var now = ZDateTimeOffset.Now;

			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var differentUser = Helper.CreateGlbStaff("B.B", "BBB");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 2m, locationA1.PK, "PLT-1", "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "PLT-1", "", "", InventoryHoldCodes.Codes.Damaged);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 4m, "A-1", "PLT-1", "A-2", "PLT-2");
			var transferLine5 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "PLT-1", "", "");
			var transferLine6 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-2", "", "");
			var transferLineAssignedToDifferentUser = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, "A-1", "PLT-1", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			transferLine4.PickedTime = now;
			transferLine5.PickedTime = now;
			transferLine6.PickedTime = now;
			transferLineAssignedToDifferentUser.PickedTime = now;
			transferLineAssignedToDifferentUser.WE_GS_NKPutawayBy = differentUser.GS_Code;
			Helper.Factory.Save();

			var transferLineInfo1 = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-3", "A-3");

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			});

			AssertTransferPutawayResponse(response1, null, false, true, false, true, true, false, true);

			AssertPutawayTransferLineData(transferLine1, true, "A-3", "PLT-3", now, "A.A");
			AssertPutawayTransferLineData(transferLine2, true, "A-3", "PLT-3", now, "A.A");
			AssertPutawayTransferLineData(transferLine3, true, "A-3", "PLT-3", now, "A.A");
			AssertPutawayTransferLineData(transferLine4, true, "A-3", "PLT-3", now, "A.A");
			AssertPutawayTransferLineData(transferLine5, true, "A-3", "PLT-3", now, "A.A");
			AssertPutawayTransferLineData(transferLine6, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLineAssignedToDifferentUser, false, "", "", ZDateTimeOffset.Empty, "B.B");

			var transferLineInfo2 = CreatePutawayTransferLineInfo(null, "PLT-2", "PLT-4", "A-2");

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			AssertTransferPutawayResponse(response2, null, false, true, false, true, true, false, true);
			AssertPutawayTransferLineData(transferLine6, true, "A-2", "PLT-4", now, "A.A");
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseFullPallet_PartOfPalletIDTransferredBeforehand

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_PopulateAndFinaliseFullPallet_PartOfPalletIDTransferredBeforehand()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "A-2", "PLT-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, "A-1", "PLT-1", "A-2", "PLT-1");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			AssertTransferPutawayResponse(response, null, true, true, false, true, true, false, true);
			AssertPutawayTransferLineData(transferLine2, true, "A-2", "PLT-1", now, "A.A");
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseASingleProduct

		[TestDate(2011, 5, 1, 5, 5, 0)]
		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, "A-1", "PLT-1", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;

			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-1", "A-2", 50m);
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			AssertTransferPutawayResponse(response, null, true, false, true, true, true, true, false);
			AssertPutawayTransferLineData(transferLine1, true, "A-2", "PLT-1", now, "A.A");
			AssertPutawayTransferLineData(transferLine2, true, "A-2", "PLT-1", now, "A.A");
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplittingTransferLines

		[TestDate(2011, 5, 1, 5, 5, 0)]
		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplittingTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			var arrivalDate = now.AddMonths(-4);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = arrivalDate;
			receive.TransportCoPK = data.Org1.PK;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1", ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var line1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			var line2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(line1, ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "");
			Helper.SetDocketLineAttributes(line2, ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "");
			AssertEquals("Precondition - transfer should have only 2 lines.", 2, transfer.Lines.Count);

			transfer.RunPreSaveValidation(); // to commit stock
			line1.PickedTime = now;
			line2.PickedTime = now;
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 25m, "", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(-5), "PA1", "PA2", "PA3");
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("One of the Transfer Lines should had splitted into 2 lines.", 3, transfer.Lines.Count);
				AssertEquals("TotalUnits should equal WE_TransactionQuantity for all lines.", true, transfer.Lines.All(l => l.WE_StockOnHand == l.WE_TransactionQuantity));
			});

			var transferLine1 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 5m);
			var transferLine2 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 15m);
			var transferLine3 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 20m);
			AssertTransferLineSplit(transferLine1, transferLine2, false);

			CombineAssertions(() =>
			{
				// finalised or in transit lines should have arrival date set
				AssertEquals(arrivalDate, transferLine1.WE_AdjustmentArrivalDate);
				AssertEquals(arrivalDate, transferLine2.WE_AdjustmentArrivalDate);
				AssertEquals(arrivalDate, transferLine3.WE_AdjustmentArrivalDate);

				AssertEquals("Committed Qty after split should match Qty to transfer.", 5m, transferLine1.GetQtyCommittedToThisLine());
				AssertEquals("Committed Qty after split should match Qty to transfer.", 15m, transferLine2.GetQtyCommittedToThisLine());
			});

			AssertTransferPutawayResponse(response, null, false, false, true, true, true, true, false);
			AssertPutawayTransferLineData(transferLine1, true, "A-2", "PLT-2", now, "A.A");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, true, "A-2", "PLT-2", now, "A.A");
		}

		void AssertTransferLineSplit(WhsTransferLine expectedLine, WhsTransferLine actualLine, bool isPicking)
		{
			CombineAssertions(() =>
			{
				AssertEquals("WE_ExpiryDate", expectedLine.WE_ExpiryDate, actualLine.WE_ExpiryDate);
				AssertEquals("WE_F3_NKPackType", expectedLine.WE_F3_NKPackType, actualLine.WE_F3_NKPackType);
				AssertEquals("GS_NKPickedBy", expectedLine.GS_NKPickedBy, actualLine.GS_NKPickedBy);
				AssertEquals("WE_OriginalInventoryStatus", InventoryStatus.Codes.InTransit, actualLine.WE_OriginalInventoryStatus);
				AssertEquals("WE_CurrentInventoryStatus", InventoryStatus.Codes.InTransit, actualLine.WE_CurrentInventoryStatus);
				AssertEquals("WE_LineComment", expectedLine.WE_LineComment, actualLine.WE_LineComment);
				AssertEquals("WE_OP", expectedLine.WE_OP, actualLine.WE_OP);
				AssertEquals("WE_PackingDate", expectedLine.WE_PackingDate, actualLine.WE_PackingDate);
				AssertEquals("WE_TransferFromPalletId", expectedLine.WE_TransferFromPalletId, actualLine.WE_TransferFromPalletId);
				AssertEquals("WE_PartAttrib1", expectedLine.WE_PartAttrib1, actualLine.WE_PartAttrib1);
				AssertEquals("WE_PartAttrib2", expectedLine.WE_PartAttrib2, actualLine.WE_PartAttrib2);
				AssertEquals("WE_PartAttrib3", expectedLine.WE_PartAttrib3, actualLine.WE_PartAttrib3);
				AssertEquals("PickedTime", expectedLine.PickedTime, actualLine.PickedTime);
				AssertEquals("WE_UnitsUQ", expectedLine.ProductUQ, actualLine.ProductUQ);
				AssertEquals("WE_WD", expectedLine.WE_WD, actualLine.WE_WD);
				AssertEquals("WE_WL_TransferFrom", expectedLine.WE_WL_TransferFrom, actualLine.WE_WL_TransferFrom);
				AssertEquals("WE_DocketLineStatus", DocketLineStatus.Codes.HeldForTransfer, actualLine.WE_DocketLineStatus);
				if (!isPicking)
				{
					AssertEquals("GS_NKPickedBy", expectedLine.GS_NKPickedBy, actualLine.GS_NKPickedBy);
					AssertEquals("PickedTime", expectedLine.PickedTime, actualLine.PickedTime);
				}
			});
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MatchingLines

		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MatchingLines_Dest()
		{
			TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MatchingLines_Core(isSource: false);
		}

		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MatchingLines_Source()
		{
			TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MatchingLines_Core(isSource: true);
		}

		void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MatchingLines_Core(bool isSource)
		{
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var now = ZDateTimeOffset.Now;

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;
			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created child line.", childLine);
			Helper.Factory.Save();

			var transferLineInfo = new WhsDocketLineInfo(transferLine) { Qty = 40m };

			// Hook on to ensure docket line status is set correctly prior to finalisation
			var webService = GetNewWebService(transferWhs, user);
			webService.CreateTestDataForIntergrityTestDuringTransferPutaway += (s, e) =>
			{
				var allParentLines = transfer.Lines.Concat(transfer.Lines.Cast<WhsTransferLine>().SelectMany(l => l.MatchingLines));
				var allChildLines = childTransfer.Lines.Concat(childTransfer.Lines.Cast<WhsTransferLine>().SelectMany(l => l.MatchingLines));
				var allTransferLines = allParentLines.Concat(allChildLines);
				AssertEquals("All docket lines should be HFT.", true, allTransferLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.HeldForTransfer));
			};

			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, transfer.Lines.Count);
				AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, childTransfer.Lines.Count);

				AssertEquals("Transfer Lines should be Split correctly with one Finalised.", 1, CountTransferLines(transfer, 40m, true, InventoryStatus.Codes.Available));
				AssertEquals("Transfer Lines should be Split correctly with one Finalised.", 1, CountTransferLines(childTransfer, 40m, true, InventoryStatus.Codes.Available));
				AssertEquals("Transfer Lines should be Split correctly.", 1, CountTransferLines(transfer, 10m, false, InventoryStatus.Codes.InTransit));
				AssertEquals("Transfer Lines should be Split correctly.", 1, CountTransferLines(childTransfer, 10m, false, InventoryStatus.Codes.InTransit));
			});

			var destinationTransfer = isSource ? childTransfer : transfer;
			AssertEquals("TotalUnits should equal Units for every line.", true, destinationTransfer.Lines.Concat(destinationTransfer.Lines.Cast<WhsTransferLine>().SelectMany(l => l.MatchingLines))
				.All(l => l.WE_StockOnHand == l.WE_TransactionQuantity)
			);
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MultipleMatchingLines

		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MultipleMatchingLines_Dest()
		{
			TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MultipleMatchingLines_Core(isSource: false);
		}

		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MultipleMatchingLines_Source()
		{
			TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MultipleMatchingLines_Core(isSource: true);
		}

		void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_InterWhs_MultipleMatchingLines_Core(bool isSource)
		{
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var now = ZDateTimeOffset.Now;

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;
			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created child line.", childLine);
			Helper.Factory.Save();

			var transferLineInfo = new WhsDocketLineInfo(transferLine) { Qty = 40m };
			var webService = GetNewWebService(transferWhs, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, transfer.Lines.Count);
				AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, childTransfer.Lines.Count);

				AssertEquals("Transfer Lines should be Split correctly.", 1, CountTransferLines(transfer, 40m, true, InventoryStatus.Codes.Available));
				AssertEquals("Transfer Lines should be Split correctly.", 1, CountTransferLines(childTransfer, 40m, true, InventoryStatus.Codes.Available));
				AssertEquals("Transfer Lines should be Split correctly.", 1, CountTransferLines(transfer, 10m, false, InventoryStatus.Codes.InTransit));
				AssertEquals("Transfer Lines should be Split correctly.", 1, CountTransferLines(childTransfer, 10m, false, InventoryStatus.Codes.InTransit));
			});

			var destinationTransfer = isSource ? childTransfer : transfer;
			AssertEquals("TotalUnits should equal Units for every line.", true, destinationTransfer.Lines.Concat(destinationTransfer.Lines.Cast<WhsTransferLine>().SelectMany(l => l.MatchingLines))
				.All(l => l.WE_StockOnHand == l.WE_TransactionQuantity)
			);
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_MatchingLines

		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_SplitingTransferLines_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");

			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = now;
			Helper.Factory.Save();
			AssertEquals("Precondition: Line should be Committed.", 50m, transferLine.QtyCommittedIncludingMatchingLines);

			var transferLineInfo = new WhsDocketLineInfo(transferLine) { Qty = 40m };
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("When transfer line is partially transferred, then it should be split into 2 lines.", 2, transfer.Lines.Count);
				AssertEquals("Transfer Lines should be Split correctly with one Finalised.", 1, CountTransferLines(transfer, 40m, true, InventoryStatus.Codes.Available));
				AssertEquals("Transfer Lines should be Split correctly.", 1, CountTransferLines(transfer, 10m, false, InventoryStatus.Codes.InTransit));
				AssertEquals("TotalUnits should equal Units for every line.", true, transfer.Lines.Concat(transfer.Lines.Cast<WhsTransferLine>().SelectMany(l => l.MatchingLines))
					.All(l => l.WE_StockOnHand == l.WE_TransactionQuantity)
				);
			});
		}

		#endregion

		#region TestTryToTransferStock_PopulateAndFinaliseASingleProduct_WithConfirmAttributes

		[TestDate(2011, 5, 1, 5, 5, 0)]
		public void TestTryToTransferStock_PopulateAndFinaliseASingleProduct_WithConfirmAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA11", "PA21", "PA31", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA12", "PA22", "PA32", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "");
			Helper.SetDocketLineAttributes(transferLine1, ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA11", "PA21", "PA31", "");
			Helper.SetDocketLineAttributes(transferLine2, ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA12", "PA22", "PA32", "");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var isAttributesSet = true;

				// correct attributes are set.
				var transferLineInfo1 = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 20m, "",
					ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(-5), "PA11", "PA21", "PA31");

				var webService1 = GetNewWebService(data.Whs1, user);
				var response1 = webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, isAttributesSet, Guid.Empty);
				AssertSuccessfulResponse(response1, webService1);

				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.None, response1.Error);
					AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				});

				AssertTransferPutawayResponse(response1, null, false, false, true, true, true, true, false);
				AssertPutawayTransferLineData(new BusinessObjectFactory().Load<WhsTransferLine>(transferLine1.PK), true, "A-2", "PLT-2", ZDateTimeOffset.Now, "A.A");
				AssertPutawayTransferLineData(new BusinessObjectFactory().Load<WhsTransferLine>(transferLine2.PK), false, "", "", ZDateTimeOffset.Empty, "");

				// correct attributes but not set correctly
				var transferLineInfo2 = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 20m, "",
					ZDateTime.Today.AddDays(6), ZDateTime.Today.AddDays(-6), "PA12", "PA22", "PA32");

				var webService2 = GetNewWebService(data.Whs1, user);
				var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo2, !isAttributesSet, Guid.Empty);
				AssertSuccessfulResponse(response2, webService2);

				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.None, response2.Error);
					AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				});

				AssertTransferPutawayResponse(response2, null, false, false, false, true, true, true, false);
				AssertPutawayTransferLineData(new BusinessObjectFactory().Load<WhsTransferLine>(transferLine1.PK), true, "A-2", "PLT-2", ZDateTimeOffset.Now, "A.A");
				AssertPutawayTransferLineData(new BusinessObjectFactory().Load<WhsTransferLine>(transferLine2.PK), false, "", "", ZDateTimeOffset.Empty, "");

				// correct attributes are set.
				var transferLineInfo3 = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 20m, "",
					ZDateTime.Today.AddDays(6), ZDateTime.Today.AddDays(-6), "PA12", "PA22", "PA32");

				var webService3 = GetNewWebService(data.Whs1, user);
				var response3 = webService3.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo3, isAttributesSet, Guid.Empty);
				AssertSuccessfulResponse(response3, webService3);

				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.None, response3.Error);
					AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
				});

				AssertTransferPutawayResponse(response3, null, true, false, true, true, true, true, false);
				AssertPutawayTransferLineData(new BusinessObjectFactory().Load<WhsTransferLine>(transferLine1.PK), true, "A-2", "PLT-2", ZDateTimeOffset.Now, "A.A");
				AssertPutawayTransferLineData(new BusinessObjectFactory().Load<WhsTransferLine>(transferLine2.PK), true, "A-2", "PLT-2", ZDateTimeOffset.Now, "A.A");
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestTryToTransferStock_PalletIDPickedAndDestPalletIDTheSame

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_PalletIDPickedAndDestPalletIDTheSame()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-2", "PLT-1", "A-2");

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			AssertTransferPutawayResponse(response, null, isAllTransferLinesTransferredOrFinalised: false, isFullPalletIDTransfered: true, isSingleProductTransfered: false, isValidDestLocation: true, isValidDestPalletID: true, isValidProduct: false, isValidSourcePalletID: true);
			AssertPutawayTransferLineData(Helper.Factory.Load<WhsTransferLine>(transferLine2.PK), true, "A-2", "PLT-1", now, "A.A");
		}

		#endregion

		#region TestTryToTransferStock_IsValidDestLocation

		public void TestTryToTransferStock_IsValidDestLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var locationString = "A-1";
			var destLocationString = "A-2";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationString, "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationString, "PLT-2", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;

			Helper.Factory.Save();

			// Putting away into valid destination location
			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-2", "PLT-2", destLocationString);
			var webService = GetNewWebService(data.Whs1);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidDestLocation", true, response.IsValidDestLocation);
			});
		}

		#endregion

		#region TestTryToTransferStock_IsOriginalLocation

		public void TestTryToTransferStock_IsOriginalLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var locationString = "A-1";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationString, "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationString, "PLT-2", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;

			Helper.Factory.Save();

			// Putting away back into original location.
			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", locationString);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidDestLocation", true, response.IsValidDestLocation);
			});
		}

		#endregion

		#region TestTryToTransferStock_InvalidDestLocation

		public void TestTryToTransferStock_InvalidDestLocation_MissingLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var locationString = "A-1";
			var missingLocationString = "A-3";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationString, "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationString, "PLT-2", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;

			Helper.Factory.Save();

			var locationCannotBeFoundErrorMessage = $"Location '{missingLocationString}' cannot be found.";

			// Putting away into non existing location
			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-2", "PLT-2", missingLocationString);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", locationCannotBeFoundErrorMessage, response.ErrorMessage);
				AssertEquals("IsValidDestLocation", false, response.IsValidDestLocation);
			});
		}

		#endregion

		#region TestTryToTransferStock_InvalidTransferPK

		public void TestTryToTransferStock_InvalidTransferPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var randomGuid = Guid.NewGuid();

			var transferLineInfo1 = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");
			var webService1 = GetNewWebService(data.Whs1);
			AssertNoExceptionThrown("System should be able to find transfer by PK.", () => webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty));

			var transferLineInfo2 = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");
			var webService2 = GetNewWebService(data.Whs1);
			AssertBusinessValidationError(webService2, $"Transfer with PK = '{randomGuid}' cannot be found.", webService2.TryToTransferStock(randomGuid, transferLineInfo2, true, Guid.Empty));
		}

		#endregion

		#region TestTryToTransferStock_InvalidDestPalletID

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_SourcePalletNotSpecified_DestNotAwaitingFullTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var expectedErrorMessage = "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.";
			var expectedSameSourceAndDestPalletErrorMessage = "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.";

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			WhsEnvironment.IsRF = true;
			try
			{
				// dest Pallet ID exist in different from Dest Location. Source Pallet ID is not specified yet, and dest pallet is not awaiting full transfer.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "", "PLT-2", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				var errorMessage = (response.ErrorMessage == expectedErrorMessage)
					? expectedErrorMessage
					: expectedSameSourceAndDestPalletErrorMessage; // error message taken from random line.
				AssertTransferPutawayResponse(response, errorMessage, false, false, false, true, !isValidDestPalletID, false, false);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_SourcePalletNotSpecified_DestAwaitingFullTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			WhsEnvironment.IsRF = true;
			try
			{
				// dest Pallet ID exist in different from Dest Location. Source Pallet ID is not specified yet, however dest pallet is awaiting full transfer.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "", "PLT-1", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, null, false, false, false, true, isValidDestPalletID, false, false);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_DestPalletUsedPreviously_CurrentlyEmpty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			WhsEnvironment.IsRF = true;
			try
			{
				// dest Pallet ID was used previously in different location. But there are no current stock for it.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "", "PLT-4", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, null, false, false, false, true, isValidDestPalletID, false, false);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_SourcePalletNotSpecified_DestPalletEmpty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			WhsEnvironment.IsRF = true;
			try
			{
				// empty dest Pallet ID is valid dest Pallet ID. Source Pallet ID is not specified.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "", "", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, null, false, false, false, true, isValidDestPalletID, false, false);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_SamePallets_DestNotAwaitingFullTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var expectedSameSourceAndDestPalletErrorMessage = "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.";

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			WhsEnvironment.IsRF = true;
			try
			{
				// dest Pallet ID exist in different from Dest Location. Source Pallet ID is same as Dest Pallet ID, but dest pallet is not awaiting full transfer.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-2", "PLT-2", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, expectedSameSourceAndDestPalletErrorMessage, false, false, false, true, !isValidDestPalletID, false, true);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_DifferentPallets_DestNotAwaitingFullTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var expectedErrorMessage = "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.";

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			WhsEnvironment.IsRF = true;
			try
			{
				// dest Pallet ID exist in different from Dest Location. Source Pallet ID is different from Dest Pallet ID, and dest pallet is not awaiting full transfer.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-2", "A-2");
				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, expectedErrorMessage, false, false, false, true, !isValidDestPalletID, false, true);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_DifferentPallets_DestAwaitingFullTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var expectedErrorMessage = "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.";

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			// create 10 more units for PLT-1 in A-1 and commit it to the transfer.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-1", "", "");
			transferLine4.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine4.QtyCommittedIncludingMatchingLines);
			transferLine4.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// dest Pallet ID exist in different from Dest Location. Source Pallet ID is different from Dest Pallet ID, and dest pallet is awaiting full transfer.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-2", "PLT-1", "A-2");
				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);

				AssertTransferPutawayResponse(response, expectedErrorMessage, false, false, false, true, !isValidDestPalletID, false, true);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_SamePallets_DestAwaitingFullTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			// create 10 more units for PLT-1 in A-1 and commit it to the transfer.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-1"),
				"PLT-1");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-1", "", "");
			transferLine4.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine4.QtyCommittedIncludingMatchingLines);
			transferLine4.Factory.Save();

			// cannot Putaway non-picked Transfer Lines, so pick the Transfer Line.
			transferLine4.PickedTime = now;
			transferLine4.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// dest pallet ID exist in different from Dest Location. Source Pallet ID is same as Dest Pallet ID, and dest pallet is awaiting full Pallet Transfer.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");
				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, null, false, true, false, true, isValidDestPalletID, false, true);
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine1.PK), true, "A-2", "PLT-1", now, "A.A");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine2.PK), false, "", "", ZDateTimeOffset.Empty, "");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine3.PK), false, "", "", ZDateTimeOffset.Empty, "");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine4.PK), true, "A-2", "PLT-1", now, "A.A");
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_DestPalletUsedPreviously_PartialTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			// create 10 more units for PLT-1 in A-1 and commit it to the transfer.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-1"),
				"PLT-1");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-1", "", "");
			transferLine4.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine4.QtyCommittedIncludingMatchingLines);
			Helper.Factory.Save();

			// cannot Putaway non-picked Transfer Lines, so pick the Transfer Line.
			transferLine4.PickedTime = now;
			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// dest Pallet ID was used previously in different location. Partial pallet id Transfer into that location.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-2", "PLT-4", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, null, false, true, false, true, isValidDestPalletID, false, true);
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine1.PK), false, "", "", ZDateTimeOffset.Empty, "");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine2.PK), true, "A-2", "PLT-4", now, "A.A");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine3.PK), false, "", "", ZDateTimeOffset.Empty, "");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine4.PK), false, "", "", ZDateTimeOffset.Empty, "");
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		[TestDate(2012, 4, 20, 5, 5, 0)]
		public void TestTryToTransferStock_InvalidDestPalletID_EmptyDestPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-4"); // inventory with 0 total units.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "PLT-3", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;

			Helper.Factory.Save();

			var isValidDestPalletID = true;
			AssertPutawayTransferLineData(transferLine1, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine3, false, "", "", ZDateTimeOffset.Empty, "");

			// create 10 more units for PLT-1 in A-1 and commit it to the transfer.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-1"),
				"PLT-1");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-1", "", "");
			transferLine4.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine4.QtyCommittedIncludingMatchingLines);
			transferLine4.Factory.Save();

			// cannot Putaway non-picked Transfer Lines, so pick the Transfer Line.
			transferLine4.PickedTime = now;
			transferLine4.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// empty dest Pallet ID is valid dest Pallet ID.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-3", "", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				AssertTransferPutawayResponse(response, null, false, true, false, true, isValidDestPalletID, false, true);
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine1.PK), false, "", "", ZDateTimeOffset.Empty, "");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine2.PK), false, "", "", ZDateTimeOffset.Empty, "");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine3.PK), true, "A-2", "", now, "A.A");
				AssertPutawayTransferLineData(webService.Factory.Load<WhsTransferLine>(transferLine4.PK), false, "", "", ZDateTimeOffset.Empty, "");
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestTryToTransferStock_IsValidProductByCode

		public void TestTryToTransferStock_IsValidProductByCode_EmptySourcePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			// Product Code
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2");
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidProduct", true, response.IsValidProduct);
			});

			AssertPutawayTransferLineData(transferLine, false, "", "", ZDateTimeOffset.Empty, "");
		}

		#endregion

		#region TestTryToTransferStock_InvalidProductByCode

		public void TestTryToTransferStock_InvalidProductByCode_EmptyProductAndSourcePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			// Empty Product or Source Pallet ID.
			var transferLineInfo = CreatePutawayTransferLineInfo(null, "", "PLT-2", "A-2");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidProduct", false, response.IsValidProduct);
			});
		}

		public void TestTryToTransferStock_InvalidProductByCode_InvalidProductAndSourcePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			// Neither Product nor Source Pallet ID.
			var invalidPalletID = "TEST";
			var transferLineInfo = CreatePutawayTransferLineInfo(null, invalidPalletID, "PLT-2", "A-2");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals($"Picked Source Pallet ID '{invalidPalletID}' could not be found.", response.ErrorMessage);
				AssertEquals("IsValidProduct", false, response.IsValidProduct);
			});
		}

		public void TestTryToTransferStock_InvalidProductByCode_EmptyProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// Source Pallet ID.
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-2", "A-2");
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.None, response.Error);
					AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertEquals("IsValidProduct", false, response.IsValidProduct);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestTryToTransferStock_InvalidProductByLineStatus

		public void TestTryToTransferStock_InvalidProductByLineStatus_TransferLineNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var locationA1 = data.Whs1.FindLocation("A-1");
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, locationA1, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLineNotPicked = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			var transferLinePicked = Helper.CreateWhsTransferLine(transfer, data.Part2, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLinePicked.PickedTime = now;

			Helper.Factory.Save();

			// transferLineNotPicked
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", $"Picked Product '{data.Part1.OP_PartNum}' could not be found.", response.ErrorMessage);
				AssertEquals("IsValidProduct", false, response.IsValidProduct);
			});
		}

		public void TestTryToTransferStock_InvalidProductByLineStatus_TransferLineTransferred()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var now = ZDateTimeOffset.Now;
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, locationA1, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLinePicked = Helper.CreateWhsTransferLine(transfer, data.Part2, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLinePicked.PickedTime = now;

			var transferLineTransferred = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "A-2");
			transferLineTransferred.PickedTime = now;
			transferLineTransferred.FinaliseDocketLine();

			Helper.Factory.Save();

			// transferLineTransferred
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", $"Picked Product '{data.Part1.OP_PartNum}' could not be found.", response.ErrorMessage);
				AssertEquals("IsValidProduct", false, response.IsValidProduct);
			});
		}

		public void TestTryToTransferStock_InvalidProductByLineStatus_TransferLineFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var now = ZDateTimeOffset.Now;
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, locationA1, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLinePicked = Helper.CreateWhsTransferLine(transfer, data.Part2, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLinePicked.PickedTime = now;

			var transferLineFinalised = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLineFinalised.PickedTime = now;
			transferLineFinalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineFinalised);

			Helper.Factory.Save();

			// transferLineFinalised
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", $"Picked Product '{data.Part1.OP_PartNum}' could not be found.", response.ErrorMessage);
				AssertEquals("IsValidProduct", false, response.IsValidProduct);
			});
		}

		public void TestTryToTransferStock_InvalidProductByLineStatus_TransferLinePicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var now = ZDateTimeOffset.Now;
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, locationA1, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLinePicked = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLinePicked.PickedTime = now;

			Helper.Factory.Save();

			// transferLinePicked
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidProduct", true, response.IsValidProduct);
			});
		}

		#endregion

		#region TestTryToTransferStock_InvalidSourcePalletID

		public void TestTryToTransferStock_InvalidSourcePalletID_EmptyProductAndSourcePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-2");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// Empty Product or Source Pallet ID
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "", "PLT-3", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertEquals("IsValidSourcePalletID", false, response.IsValidSourcePalletID);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidSourcePalletID_EmptySourcePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-2");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// Product Code
				var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-3", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertEquals("IsValidSourcePalletID", false, response.IsValidSourcePalletID);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidSourcePalletID_InvalidProductAndSourcePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-2");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			var invalidPalletID = "TEST";

			WhsEnvironment.IsRF = true;
			try
			{
				// Neither Product nor Source Pallet ID
				var transferLineInfo = CreatePutawayTransferLineInfo(null, invalidPalletID, "PLT-3", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
					AssertEquals("ErrorMessage", $"Picked Source Pallet ID '{invalidPalletID}' could not be found.", response.ErrorMessage);
					AssertEquals("IsValidSourcePalletID", false, response.IsValidSourcePalletID);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidSourcePalletID_InvalidProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-2");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				// Source Pallet ID
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-3", "A-2");

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertEquals("IsValidSourcePalletID", true, response.IsValidSourcePalletID);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidSourcePalletID_InvalidProduct_RepeatedTransferToSamePallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-2");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-2", "", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			var repeatedTransferPalletID = "PLT-1";

			WhsEnvironment.IsRF = true;
			try
			{
				// Source Pallet ID
				var transferLineInfo1 = CreatePutawayTransferLineInfo(null, repeatedTransferPalletID, "PLT-3", "A-2");

				var webService1 = GetNewWebService(data.Whs1, user);
				var response1 = webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
				AssertSuccessfulResponse(response1, webService1);

				// Try to Transfer same Pallet ID second time.
				var transferLineInfo2 = CreatePutawayTransferLineInfo(null, repeatedTransferPalletID, "PLT-3", "A-2");

				var webService2 = GetNewWebService(data.Whs1, user);
				var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
				AssertSuccessfulResponse(response2, webService2);

				CombineAssertions(() =>
				{
					AssertEquals("IsValidSourcePalletID", false, response2.IsValidSourcePalletID);
					AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response2.Error);
					AssertEquals("ErrorMessage", $"Picked Source Pallet ID '{repeatedTransferPalletID}' could not be found.", response2.ErrorMessage);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestTryToTransferStock_IsValidInventoryHeldCode

		public void TestTryToTransferStock_IsValidInventoryHeldCode_DamagedHeldCode()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1", "", InventoryHoldCodes.Codes.Damaged);
			var abcHeld_Inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1", "", abc_HeldCode.WHC_Code);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "", InventoryHoldCodes.Codes.Damaged);
			transferLine.PickedTime = ZDateTimeOffset.Now;

			abcHeld_Inventory.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 50m, InventoryHoldCodes.Codes.Damaged);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidInventoryHeldCode", true, response.IsValidInventoryHeldCode);
			});
		}

		#endregion

		#region TestTryToTransferStock_InvalidInventoryHeldCode

		public void TestTryToTransferStock_InvalidInventoryHeldCode()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1", "", InventoryHoldCodes.Codes.Damaged);
			var abcHeld_Inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1", "", abc_HeldCode.WHC_Code);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "", InventoryHoldCodes.Codes.Damaged);
			transferLine.PickedTime = ZDateTimeOffset.Now;

			abcHeld_Inventory.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 50m, abc_HeldCode.WHC_Code);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("IsValidInventoryHeldCode", false, response.IsValidInventoryHeldCode);
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", $"No inventory with Hold Code '{abc_HeldCode.WHC_Code}' await putting away.", response.ErrorMessage);
			});
		}

		public void TestTryToTransferStock_InvalidInventoryHeldCode_BlankHeldCode()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1", "", InventoryHoldCodes.Codes.Damaged);
			var abcHeld_Inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1", "", abc_HeldCode.WHC_Code);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "", InventoryHoldCodes.Codes.Damaged);
			transferLine.PickedTime = ZDateTimeOffset.Now;

			abcHeld_Inventory.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 50m);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("IsValidInventoryHeldCode", false, response.IsValidInventoryHeldCode);
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", "No inventory with Hold Code '' await putting away.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestTryToTransferStock_InvalidQuantity

		public void TestTryToTransferStock_InvalidQuantity_MoreThanAvailable()
		{
			var targetQuantity = 50m;
			var availableQuantity = 20m;

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, availableQuantity, "A-1", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-1", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-2", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transferLine3.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", targetQuantity);

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals("TotalQuantityAvailableForPutaway", availableQuantity, response.TotalQuantityAvailableForPutaway);
					AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
					AssertEquals("ErrorMessage", $"Cannot putaway {targetQuantity} {data.Part1.OP_StockKeepingUnit} only {availableQuantity} {data.Part1.OP_StockKeepingUnit} available to put away.", response.ErrorMessage);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidQuantity_LessThanAvailable()
		{
			var targetQuantity = 10m;
			var availableQuantity = 20m;

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, availableQuantity, "A-1", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-1", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-2", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transferLine3.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", targetQuantity);

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertEquals("TotalQuantityAvailableForPutaway", availableQuantity, response.TotalQuantityAvailableForPutaway);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidQuantity_MoreThanAvailable_MultipleLocations()
		{
			var targetQuantity = 50m;
			var availableQuantity = 20m;

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, availableQuantity, "A-1", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-1", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-2", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transferLine3.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var transferLineInfo = CreatePutawayTransferLineInfo(data.Part2, "", "", "A-2", targetQuantity);

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals("TotalQuantityAvailableForPutaway", availableQuantity * 2, response.TotalQuantityAvailableForPutaway);
					AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
					AssertEquals("ErrorMessage", $"Cannot putaway {targetQuantity} {data.Part1.OP_StockKeepingUnit} only {availableQuantity * 2} {data.Part2.OP_StockKeepingUnit} available to put away.", response.ErrorMessage);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidQuantity_ExactAmount_MultipleLocations()
		{
			var targetQuantity = 40m;
			var availableQuantity = 20m;

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 30m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, availableQuantity, "A-1", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-1", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, availableQuantity, "A-2", "");
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transferLine3.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var transferLineInfo = CreatePutawayTransferLineInfo(data.Part2, "", "", "A-3", targetQuantity);

				var webService = GetNewWebService(data.Whs1, user);
				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);

				CombineAssertions(() =>
				{
					AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertEquals("TotalQuantityAvailableForPutaway", availableQuantity * 2, response.TotalQuantityAvailableForPutaway);
				});
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestTryToTransferStock_InvalidAttributes

		public void TestTryToTransferStock_InvalidAttributes_InvalidPartAttribute1()
		{
			TestTryToTransferStock_InvalidAttributesCore((info) => info.Attribute1 = "TEST");
		}

		public void TestTryToTransferStock_InvalidAttributes_InvalidPartAttribute2()
		{
			TestTryToTransferStock_InvalidAttributesCore((info) => info.Attribute2 = "TEST");
		}

		public void TestTryToTransferStock_InvalidAttributes_InvalidPartAttribute3()
		{
			TestTryToTransferStock_InvalidAttributesCore((info) => info.Attribute3 = "TEST");
		}

		public void TestTryToTransferStock_InvalidAttributes_InvalidSerialNumber()
		{
			TestTryToTransferStock_InvalidAttributesCore((info) => info.SerialNumber = "TEST");
		}

		public void TestTryToTransferStock_InvalidAttributes_IncorrectExpiryDate()
		{
			TestTryToTransferStock_InvalidAttributesCore((info) => info.ExpiryDate = ZDateTime.Today.AddDays(15).ToDateTime());
		}

		public void TestTryToTransferStock_InvalidAttributes_IncorrectPackingDate()
		{
			TestTryToTransferStock_InvalidAttributesCore((info) => info.PackingDate = ZDateTime.Today.AddDays(-15).ToDateTime());
		}

		void TestTryToTransferStock_InvalidAttributesCore(Action<WhsDocketLineInfo> changeLineToInvalidAttribute)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA4", "PA5", "PA6", "");
			inventory1.WI_PalletID = "PLT-1";
			inventory1.WI_SerialNumber = "SER1";
			inventory2.WI_PalletID = "PLT-1";
			inventory2.WI_SerialNumber = "SER2";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(transferLine1, ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "SER1");
			Helper.SetDocketLineAttributes(transferLine2, ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA4", "PA5", "PA6", "SER2");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			var expectedErrorMessage = "None of the stock for putaway match entered attributes.";

			// Incorrect Packing Date
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 1m, "", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(-15), "PA1", "PA2", "PA3");
			transferLineInfo.SerialNumber = "SER1";
			changeLineToInvalidAttribute(transferLineInfo);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", expectedErrorMessage, response.ErrorMessage);
				AssertEquals("IsValidAttributes", false, response.IsValidAttributes);
			});
		}

		public void TestTryToTransferStock_ValidAttributes_AllAttributesCorrect()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA4", "PA5", "PA6", "");
			inventory1.WI_PalletID = "PLT-1";
			inventory2.WI_PalletID = "PLT-1";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(transferLine1, ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "");
			Helper.SetDocketLineAttributes(transferLine2, ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA4", "PA5", "PA6", "");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			// All attributes are correct.
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 1m, "", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(-5), "PA1", "PA2", "PA3");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorMessage", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidAttributes", true, response.IsValidAttributes);
				AssertEquals("Only 1 unit with set attributes could be transferred, system should notify user about that.", 1m, response.TotalQuantityTransferred);
				AssertEquals(true, webService.Factory.Load<WhsTransferLine>(transferLine1.PK).IsFinalised);
			});
		}

		public void TestTryToTransferStock_ValidAttributes_AllAttributesCorrect_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA4", "PA5", "PA6", "");
			inventory1.WI_PalletID = "PLT-1";
			inventory1.WI_SerialNumber = "SER1";
			inventory2.WI_PalletID = "PLT-1";
			inventory2.WI_SerialNumber = "SER2";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(transferLine1, ZDate.Today.AddDays(5), ZDate.Today.AddDays(-5), "PA1", "PA2", "PA3", "SER1");
			Helper.SetDocketLineAttributes(transferLine2, ZDate.Today.AddDays(6), ZDate.Today.AddDays(-6), "PA4", "PA5", "PA6", "SER2");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			// All attributes are correct.
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 1m, "", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(-5), "PA1", "PA2", "PA3");
			transferLineInfo.SerialNumber = "SER1";

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorMessage", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidAttributes", true, response.IsValidAttributes);
				AssertEquals("Only 1 unit with set attributes could be transferred, system should notify user about that.", 1m, response.TotalQuantityTransferred);
				AssertEquals(true, webService.Factory.Load<WhsTransferLine>(transferLine1.PK).IsFinalised);
			});
		}

		#endregion

		#region TestTryToTransferStock_IgnoreTimeInCompareExpiryAndPackingDateAttribute

		[TestDate(2015, 08, 20, 9, 30, 00)]
		public void TestTryToTransferStock_IgnoreTimeInCompareExpiryAndPackingDateAttribute_ExpiryDate_DifferentDays()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var expiryDate1 = ZDate.Today.AddDays(5);
			var packingDate1 = ZDate.Today.AddDays(-5);
			var expiryDate2 = ZDate.Today.AddDays(6);
			var packingDate2 = ZDate.Today.AddDays(-6);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			inventory1.WI_PalletID = "PLT-1";
			inventory2.WI_PalletID = "PLT-1";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			receive1.Factory.Save();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(transferLine1, expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			Helper.SetDocketLineAttributes(transferLine2, expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			var expectedErrorMessage = "None of the stock for putaway match entered attributes.";

			// Expiry Date in different day should not match
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 20m, "", expiryDate1.AddDays(1), packingDate1, "PA1", "PA2", "PA3");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", expectedErrorMessage, response.ErrorMessage);
				AssertEquals("IsValidAttributes", false, response.IsValidAttributes);
			});
		}

		[TestDate(2015, 08, 20, 9, 30, 00)]
		public void TestTryToTransferStock_IgnoreTimeInCompareExpiryAndPackingDateAttribute_ExpiryDate_NoTime()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var expiryDate1 = ZDate.Today.AddDays(5);
			var packingDate1 = ZDate.Today.AddDays(-5);
			var expiryDate2 = ZDate.Today.AddDays(6);
			var packingDate2 = ZDate.Today.AddDays(-6);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			inventory1.WI_PalletID = "PLT-1";
			inventory2.WI_PalletID = "PLT-1";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			receive1.Factory.Save();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(transferLine1, expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			Helper.SetDocketLineAttributes(transferLine2, expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			// Expiry Date with no time should match
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 20m, "", expiryDate1, packingDate1, "PA1", "PA2", "PA3");

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidAttributes", true, response.IsValidAttributes);
				AssertEquals(true, webService.Factory.Load<WhsTransferLine>(transferLine1.PK).IsFinalised);
			});
		}

		[TestDate(2015, 08, 20, 9, 30, 00)]
		public void TestTryToTransferStock_IgnoreTimeInCompareExpiryAndPackingDateAttribute_PackingDate_DifferentDay()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var expiryDate1 = ZDate.Today.AddDays(5);
			var packingDate1 = ZDate.Today.AddDays(-5);
			var expiryDate2 = ZDate.Today.AddDays(6);
			var packingDate2 = ZDate.Today.AddDays(-6);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			inventory1.WI_PalletID = "PLT-1";
			inventory2.WI_PalletID = "PLT-1";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			receive1.Factory.Save();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(transferLine1, expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			Helper.SetDocketLineAttributes(transferLine2, expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			var expectedErrorMessage = "None of the stock for putaway match entered attributes.";

			// Packing Date in different day should not match
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 20m, "", expiryDate2, packingDate2.AddDays(1), "PA4", "PA5", "PA6"); // .AddDays(1)

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("ErrorType", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("ErrorMessage", expectedErrorMessage, response.ErrorMessage);
				AssertEquals("IsValidAttributes", false, response.IsValidAttributes);
			});
		}

		[TestDate(2015, 08, 20, 9, 30, 00)]
		public void TestTryToTransferStock_IgnoreTimeInCompareExpiryAndPackingDateAttribute_PackingDate_NoTime()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var expiryDate1 = ZDate.Today.AddDays(5);
			var packingDate1 = ZDate.Today.AddDays(-5);
			var expiryDate2 = ZDate.Today.AddDays(6);
			var packingDate2 = ZDate.Today.AddDays(-6);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			inventory1.WI_PalletID = "PLT-1";
			inventory2.WI_PalletID = "PLT-1";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			receive1.Factory.Save();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			Helper.SetDocketLineAttributes(transferLine1, expiryDate1, packingDate1, "PA1", "PA2", "PA3", "");
			Helper.SetDocketLineAttributes(transferLine2, expiryDate2, packingDate2, "PA4", "PA5", "PA6", "");
			transfer.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", false, transfer.HasErrors);

			Helper.Factory.Save();

			// Packing Date with no time should match
			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 20m, "", expiryDate2, packingDate2, "PA4", "PA5", "PA6");
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("IsValidAttributes", true, response.IsValidAttributes);
				AssertEquals(true, webService.Factory.Load<WhsTransferLine>(transferLine2.PK).IsFinalised);
			});
		}

		#endregion

		#region TestTryToTransferStock_TransferLineIsNotPicked

		public void TestTryToTransferStock_TransferLineIsNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "PLT-1", "", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "", "A-2");

			transferLineInfo.PK = transferLine.PK.ToGuid();
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			AssertTransferPutawayResponse(response, "Transfer Line must be Picked.", isAllTransferLinesTransferredOrFinalised: false, isFullPalletIDTransfered: false, isSingleProductTransfered: false, isValidDestLocation: false, isValidDestPalletID: false, isValidProduct: false, isValidSourcePalletID: false);
			AssertPutawayTransferLineData(Helper.Factory.Load<WhsTransferLine>(transferLine.PK), false, "", "", ZDateTimeOffset.Empty, "");
		}

		#endregion

		#region TestTryToTransferStock_InterWhsChild

		public void TestTryToTransferStock_InterWhsChild_Dest()
		{
			TestTryToTransferStock_InterWhsChild(isSource: false);
		}

		public void TestTryToTransferStock_InterWhsChild_Source()
		{
			TestTryToTransferStock_InterWhsChild(isSource: true);
		}

		void TestTryToTransferStock_InterWhsChild(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A", "", data.Org1, data.Part1, "", 10m);
			transferLineInfo.PK = transferLine.PK.ToGuid();

			var webService = GetNewWebService(warehouse2, staff);
			AssertBusinessValidationError(webService, "Should have thrown an exception as child inter Warehouse transfers are not supported.",
				"Cannot transfer an Inter-Warehouse Transfer using the child job.",
				webService.TryToTransferStock(childTransfer.PK.ToGuid(), transferLineInfo, false, Guid.Empty));
		}

		#endregion

		#region TestTryToTransferStock_MaxPalletIdLengthExceeded

		public void TestTryToTransferStock_MaxPalletIdLengthExceeded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = now;

			Helper.Factory.Save();

			var destinationPalletId = "1234567890123456789012345678901";
			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", destinationPalletId, "A-2");

			Assert("Precondition: Destination pallet id exceeds maximum length for pallet ids.", destinationPalletId.Length > WhsDocketLineSchema.WE_PalletID.MaxLength);

			var webService = GetNewWebService(data.Whs1, user);
			TransferPutawayWebServiceResponse response = null;
			AssertNoExceptionThrown("No exception is thrown.", () => response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty));

			AssertEquals("No error reported.", string.Empty, ErrorReporter.LastMessageReported);
			AssertNotNull("Response is not null.", response);
			CombineAssertions(() =>
			{
				AssertEquals("Error response is returned.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error response is returned.",
					"WE_PalletID exceeds maximum length allowed. The maximum length of this property is 30 characters, but 31 were entered.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestTryToTransferStock_PalletCapacityCheck

		public void TestTryToTransferStock_PalletCapacityCheck()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var user = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = user.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(user.StaffPlainTextPassword);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_PalletFloorSpaces = 1;
			location2.WLV_PalletStackHeight = 1;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1, "PLT-1");
			helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location2, "PLT-2");
			receive.FinaliseDocket();
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");

				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Total required Pallets (1) exceeds the maximum available Pallets (0) for this location.", response.ErrorMessage);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestTryToTransferStock_InvalidLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var user = Helper.CreateGlbStaff("ST1", "Staff1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1, "PLT-1");
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var webService = GetNewWebService(data.Whs1, user);
				var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");

				var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("You cannot putaway to Dock Door locations.", response.ErrorMessage);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestTryToTransferStock_WithTransferTaskPK

		[TestDate(2025, 5, 14)]
		public void TestTryToTransferStock_WithTransferTaskPK_AllLinesOnSameProcessTaskPK() => TestTryToTransferStock_WithTransferTaskPKCore(allLinesOnSameProcessTaskPK: true);

		[TestDate(2025, 5, 14)]
		public void TestTryToTransferStock_WithTransferTaskPK_NotAllLinesOnSameProcessTaskPK() => TestTryToTransferStock_WithTransferTaskPKCore(allLinesOnSameProcessTaskPK: false);

		void TestTryToTransferStock_WithTransferTaskPKCore(bool allLinesOnSameProcessTaskPK)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			if (!allLinesOnSameProcessTaskPK)
			{
				transferLine2.WE_P9_Task = Guid.Empty;
			}
			Helper.Factory.Save();

			AssertEquals(transferLine1.WE_P9_Task, transferProcessTask.PK);
			AssertEquals(transferLine2.WE_P9_Task, allLinesOnSameProcessTaskPK ? transferProcessTask.PK : Guid.Empty);

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", 50m);
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, transferProcessTask.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("Precondition - Transfer must not be finalised.", false, transfer.IsFinalised);
				AssertEquals("Precondition - Transfer must have two lines.", 2, transfer.Lines.Count);
			});

			AssertPutawayTransferLineData(transferLine2, false, "", "", ZDateTimeOffset.Empty, "");
			AssertPutawayTransferLineData(transferLine1, true, "A-2", "", now, "A.A");

			AssertTransferPutawayResponse(response, null, !allLinesOnSameProcessTaskPK, false, true, true, true, true, false);
		}

		public void TestTryToTransferStock_WithTransferTaskPK_LinesWithDifferentTaskPK() => TestTryToTransferStock_WithTransferTaskPK_LinesWithDifferentTaskPKCore(isEmptyPK: false);
		public void TestTryToTransferStock_WithTransferTaskPK_LinesWithDifferentTaskPK_NoTaskAssigned() => TestTryToTransferStock_WithTransferTaskPK_LinesWithDifferentTaskPKCore(isEmptyPK: true);

		void TestTryToTransferStock_WithTransferTaskPK_LinesWithDifferentTaskPKCore(bool isEmptyPK)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;

			Helper.CreateProcessTaskForTransfer(transfer, user);
			if (isEmptyPK)
			{
				transferLine1.WE_P9_Task = Guid.Empty;
				transferLine2.WE_P9_Task = Guid.Empty;
			}
			Helper.Factory.Save();

			var mockTransferTaskPK = ZGuid.BrettsGuid.ToGuid();
			AssertNotEquals(transferLine1.WE_P9_Task, mockTransferTaskPK);
			AssertNotEquals(transferLine2.WE_P9_Task, mockTransferTaskPK);

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", 50m);
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, mockTransferTaskPK);
			AssertBusinessValidationError(webService, "None of the Transfer Lines are assigned to the user.", response);
		}

		[TestDate(2025, 5, 14)]
		public void TestTryToTransferStock_WithTransferTaskPK_SpecifiedLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = now;

			transferLine.WE_GS_NKPutawayBy = user.GS_Code;
			transferLine.LocationString = "A-2";

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			Helper.Factory.Save();

			AssertEquals(transferLine.WE_P9_Task, transferProcessTask.PK);

			var transferLineInfo = new WhsDocketLineInfo(transferLine);
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, transferProcessTask.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			AssertPutawayTransferLineData(transferLine, true, "A-2", "", now, "A.A");
			AssertTransferPutawayResponse(response, null, true, false, true, true, true, true, false);
		}

		public void TestTryToTransferStock_WithTransferTaskPK_SpecifiedLine_DifferentTransferTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = now;

			transferLine.WE_GS_NKPutawayBy = user.GS_Code;
			transferLine.LocationString = "A-2";

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			Helper.Factory.Save();

			AssertEquals(transferLine.WE_P9_Task, transferProcessTask.PK);

			var mockTransferTaskPK = ZGuid.BrettsGuid.ToGuid();
			var transferLineInfo = new WhsDocketLineInfo(transferLine);
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, mockTransferTaskPK);
			AssertBusinessValidationError(webService, "Transfer Line is assigned to another user.", response);
		}

		[TestDate(2025, 5, 14)]
		public void TestTryToTransferStock_LineWithTransferTaskPK_PutawayPartialStock_TaskIsCopiedToNewLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = now;

			transferLine.WE_GS_NKPutawayBy = user.GS_Code;
			transferLine.LocationString = "A-2";

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			Helper.Factory.Save();

			AssertEquals(transferLine.WE_P9_Task, transferProcessTask.PK);

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", 1m);
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("Precondition - Transfer must not be finalised.", false, transfer.IsFinalised);
				AssertEquals("Precondition - Transfer must have two lines.", 2, transfer.Lines.Count);
			});

			AssertPutawayTransferLineData(transferLine, false, "A-2", "", ZDateTimeOffset.Empty, "A.A");
			var newTransferLine = (WhsTransferLine)transfer.Lines.Single(l => l.PK != transferLine.PK);
			AssertPutawayTransferLineData(newTransferLine, true, "A-2", "", now, "A.A");

			AssertEquals(transferLine.WE_P9_Task, transferProcessTask.PK);
			AssertEquals(newTransferLine.WE_P9_Task, transferProcessTask.PK);

			AssertTransferPutawayResponse(response, null, false, false, true, true, true, true, false);
		}

		[TestDate(2025, 5, 15)]
		public void TestTryToTransferStock_WithTransferTaskPK_PopulateAndFinaliseFullPalletWithMultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var now = ZDateTimeOffset.Now;

			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var differentUser = Helper.CreateGlbStaff("B.B", "BBB");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, locationA1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "PLT-1", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 15m, "A-1", "PLT-1", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-3", "A-3");
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, transferProcessTask.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertTransferPutawayResponse(response, null, true, true, false, true, true, false, true);

			AssertPutawayTransferLineData(transferLine1, true, "A-3", "PLT-3", now, "A.A");
			AssertPutawayTransferLineData(transferLine2, true, "A-3", "PLT-3", now, "A.A");
			AssertPutawayTransferLineData(transferLine3, true, "A-3", "PLT-3", now, "A.A");
		}

		public void TestTryToTransferStock_WithTransferTaskPK_PopulateAndFinaliseFullPalletWithMultipleLines_SomeLinesOnDifferentProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var now = ZDateTimeOffset.Now;

			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var differentUser = Helper.CreateGlbStaff("B.B", "BBB");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, locationA1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, "A-1", "PLT-1", "", "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "PLT-1", "", "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 15m, "A-1", "PLT-1", "", "");
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			transferLine2.WE_P9_Task = Guid.Empty;
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-3", "A-3");
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, transferProcessTask.PK.ToGuid());
			AssertBusinessValidationError(webService, "Some of the transfer lines on the full pallet transfer are not assigned to the user.", response);
		}

		#endregion

		#region Implementation 

		#region CountTransferLines

		int CountTransferLines(WhsTransfer transfer, Decimal qtyToMove, bool isFinalised, string originalInventoryStatus)
		{
			return transfer.Lines.Cast<WhsTransferLine>().Count(l =>
				l.QtyToMoveIncludingMatchingLines == qtyToMove &&
				l.IsPicked &&
				l.IsFinalised == isFinalised &&
				l.WE_OriginalInventoryStatus == originalInventoryStatus
			);
		}

		#endregion

		#endregion
	}
}
