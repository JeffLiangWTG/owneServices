using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class TryToAllocateStockForTransferTest : WhsTransferSecureServiceTestCase
	{
		#region TestTryToAllocateStockForTransfer

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			AssertEquals("Precondition", false, data.Whs1.IsTaskManagementEnabled);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");

			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.NewTransferLines);

			CombineAssertions(() =>
			{
				AssertEquals("New transfer line should be passed back.", 1, response.NewTransferLines.Count);

				AssertEquals("IsValidLocation", true, response.IsValidLocation);
				AssertEquals("IsValidPalletID", false, response.IsValidPalletID);
				AssertEquals("IsValidProduct", true, response.IsValidProduct);
				AssertEquals("IsValidOriginalInventoryHeldCode", true, response.IsValidInventoryHeldCode);
				AssertEquals("IsValidAttributes", true, response.IsValidAttributes);
				AssertEquals("TotalQuantityAvailableToPick", 50m, response.TotalQuantityAvailableToPick);
				AssertEquals("TotalPickLineQuantity", 10m, response.TotalPickLineQuantity);
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transfer = newFactory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.PK, new ZGuid(response.Transfer.PK)));
			AssertNotNull(transfer);

			AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "PLT-1", 10m, "", user, now);

			var pickedTransferLine = transfer.Lines.Single(l => l.WE_TransactionQuantity == 10m);

			CombineAssertions(() =>
			{
				AssertEquals("In-Transit Transfer lines should have an Arrival Date set.", ZDateTimeOffset.Now, pickedTransferLine.WE_AdjustmentArrivalDate);
				AssertEquals("Transfer line that was picked should be returned in response.", pickedTransferLine.PK.ToGuid(), response.NewTransferLines[0].PK);
			});

			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);

			AssertNull("No process task created.", newFactory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)));
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_ShowStockOnHandWarningOnPutaway

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_ShowStockOnHandWarningOnPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");

			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.NewTransferLines);
			AssertEquals("SOH", true, response1.ShowStockOnHandWarningOnPutaway);

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("SOH", false, response2.ShowStockOnHandWarningOnPutaway);
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_FixedWidthLocation

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_FixedWidthLocation_ByUserFriendlyLocationString()
		{
			TestTryToAllocateStockForTransfer_FixedWidthLocationCore(true);
		}

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_FixedWidthLocation_ByLocationString()
		{
			TestTryToAllocateStockForTransfer_FixedWidthLocationCore(false);
		}

		void TestTryToAllocateStockForTransfer_FixedWidthLocationCore(bool searchWithUserFriendlyString)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 2);
			var warehouse = data.Whs1;
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 2;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			warehouse.WW_LocationTraysFixedWidth = 2;

			Helper.Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			var now = ZDate.Today;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var sourceLocation = data.Whs1.FindLocation("A-01-01");
			var destinationLocation = data.Whs1.FindLocation("A-01-02");

			var expiryDate = now.AddDays(10);
			var packingDate = now.AddDays(-1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, sourceLocation, "PLT-1", expiryDate, packingDate, "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, "PLT-1", destinationLocation, expiryDate, packingDate, "", "", "");
			transfer.RunPreSaveValidation(); // to commit stock.

			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo(searchWithUserFriendlyString ? sourceLocation.WLV_LocationString_UserFriendly : sourceLocation.WLV_LocationString, "PLT-1", data.Org1, data.Part1, "", 1m, expiryDate, packingDate, "", "", "");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InactiveProductWithMatchingTransferLines

		public void TestTryToAllocateStockForTransfer_InactiveProductWithMatchingTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			var now = ZDate.Today;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var expiryDate = now.AddDays(10);
			var packingDate = now.AddDays(-1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, sourceLocation, "PLT-1", expiryDate, packingDate, "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, "PLT-1", destinationLocation, expiryDate, packingDate, "", "", "");
			transfer1.RunPreSaveValidation(); // to commit stock.

			Helper.Factory.Save();

			var transferLineInfo1 = CreatePickingTransferLineInfo("A-1", "PLT-1", data.Org1, data.Part1, "", 1m, expiryDate, packingDate, "", "", "");
			data.Part1.OP_IsActive = false;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer1.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response given should have business validation error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response should have error message.", "Error - WE_OP: This product cannot be selected because it is inactive", response.ErrorMessage);
				AssertNull("New transfer line should not be passed back.", response.NewTransferLines);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InactiveProductWithNonNullNonPickedTransferLine

		public void TestTryToAllocateStockForTransfer_InactiveProductWithExistingNonPickedTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 3m);
			transferLineInfo.PK = transferLine.PK.ToGuid();
			data.Part1.OP_IsActive = false;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response given should have business validation error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response should have error message.", "Error - WE_OP: This product cannot be selected because it is inactive\nError - WE_OP: This Product Code is inactive - it may not be used.", response.ErrorMessage);
				AssertNull("New transfer line should not be passed back.", response.NewTransferLines);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_NoPackingDateOrExpiryDate

		[ExpectNoExceptions]
		public void TestTryToAllocateStockForTransfer_NoPackingDateOrExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var today = ZDate.Today;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, sourceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, sourceLocation, today.AddDays(1), today.AddDays(-1), "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var transferLineInfoForPart1 = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);
			var transferLineInfoForPart2 = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part2, "", 10m);
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfoForPart1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfoForPart2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_RaiseExceptionWhenTransferAlreadyFinalised

		[TestDate(2016, 2, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_RaiseExceptionWhenTransferAlreadyFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"), user);
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Helper.Factory.Save();

			var transferLineInfo1 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 60m);

			var webService = GetNewWebService(data.Whs1, user);
			AssertBusinessValidationError(webService, "Transfer record has been finalized.",
				webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo1, false, Guid.Empty));
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_CompareExpiryAndPackingDateAttribute

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_CompareExpiryAndPackingDateAttribute()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var expiryDate = ZDate.Today.AddDays(5);
			var packingDate = ZDate.Today.AddDays(-5);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), expiryDate, packingDate, "PA1", "PA2", "PA3", "");
			inventory1.WI_PalletID = "PLT-1";

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "PLT-1", data.Org1, data.Part1, "", 10m, expiryDate, packingDate, "PA1", "PA2", "PA3");

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.NewTransferLines);

			CombineAssertions(() =>
			{
				AssertEquals("New transfer line should be passed back.", 1, response.NewTransferLines.Count);

				AssertEquals("IsValidLocation", true, response.IsValidLocation);
				AssertEquals("IsValidPalletID", true, response.IsValidPalletID);
				AssertEquals("IsValidProduct", true, response.IsValidProduct);
				AssertEquals("IsValidOriginalInventoryHeldCode", true, response.IsValidInventoryHeldCode);
				AssertEquals("IsValidAttributes", true, response.IsValidAttributes);
				AssertEquals("TotalQuantityAvailableToPick", 50m, response.TotalQuantityAvailableToPick);
				AssertEquals("TotalPickLineQuantity", 10m, response.TotalPickLineQuantity);
				AssertEquals("IsValidLocation", expiryDate, response.Transfer.Lines[0].ExpiryDate);
				AssertEquals("IsValidLocation", packingDate, response.Transfer.Lines[0].PackingDate);
			});

			var transfer = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.PK, new ZGuid(response.Transfer.PK)));
			AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "PLT-1", 10m, "", user, now);

			var pickedTransferLine = transfer.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			CombineAssertions(() =>
			{
				AssertEquals("In-Transit Transfer lines should have an Arrival Date set.", ZDateTimeOffset.Now, pickedTransferLine.WE_AdjustmentArrivalDate);
				AssertEquals("Transfer line that was picked should be returned in response.", pickedTransferLine.PK.ToGuid(), response.NewTransferLines[0].PK);
			});

			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_ReusingSameTransferAndCreatesNewLines

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_ReusingSameTransferAndCreatesNewLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");

			Helper.Factory.Save();

			var transferLineInfo1 = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.NewTransferLines);

			CombineAssertions(() =>
			{
				AssertEquals("New transfer line should be passed back.", 1, response1.NewTransferLines.Count);

				AssertEquals("IsValidLocation", true, response1.IsValidLocation);
				AssertEquals("IsValidPalletID", false, response1.IsValidPalletID);
				AssertEquals("IsValidProduct", true, response1.IsValidProduct);
				AssertEquals("IsValidOriginalInventoryHeldCode", true, response1.IsValidInventoryHeldCode);
				AssertEquals("IsValidAttributes", true, response1.IsValidAttributes);
				AssertEquals("TotalQuantityAvailableToPick", 50m, response1.TotalQuantityAvailableToPick);
				AssertEquals("TotalPickLineQuantity", 10m, response1.TotalPickLineQuantity);
			});

			var transfer = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.PK, new ZGuid(response1.Transfer.PK)));
			AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "PLT-1", 10m, "", user, now);

			var transferLineInfo2 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 15m);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.NewTransferLines);

			CombineAssertions(() =>
			{
				AssertEquals("New transfer line should be passed back.", 1, response2.NewTransferLines.Count);
				AssertEquals("IsValidLocation", true, response2.IsValidLocation);
				AssertEquals("IsValidPalletID", false, response2.IsValidPalletID);
				AssertEquals("IsValidProduct", true, response2.IsValidProduct);
				AssertEquals("IsValidOriginalInventoryHeldCode", true, response2.IsValidInventoryHeldCode);
				AssertEquals("IsValidAttributes", true, response2.IsValidAttributes);
				AssertEquals("TotalQuantityAvailableToPick", 40m, response2.TotalQuantityAvailableToPick);
				AssertEquals("TotalPickLineQuantity", 15m, response2.TotalPickLineQuantity);
			});

			transfer = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.PK, new ZGuid(response1.Transfer.PK)));
			AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "PLT-1", 15m, "", user, now);
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_PackTypesAreSet

		[TestDate(2012, 9, 25, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_PackTypesAreSet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");

			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 12m);
			transferLineInfo.PackUQ = Core.Constants.PkgUnit.Carton; // 1 CTN = 12 UNT
			transferLineInfo.Packs = 1m;
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var transfer = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.PK, new ZGuid(response.Transfer.PK)));
			AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "PLT-1", 12m, "", user, now);

			CombineAssertions(() =>
			{
				AssertEquals("Pack Type", Core.Constants.PkgUnit.Carton, transfer.Lines[0].WE_F3_NKPackType);
				AssertEquals("Packs", 1m, transfer.Lines[0].WE_PackQuantity);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_AllocateStockForExistingLine

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_AllocateStockForExistingLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.GS_NKPickedBy = user.GS_Code;
			transferLine.WE_GS_NKPutawayBy = user.GS_Code;
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 3m);
			transferLineInfo.PK = transferLine.PK.ToGuid();

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.NewTransferLines);

			CombineAssertions(() =>
			{
				AssertEquals("If transfer line is partially picked, then it should be split into 2 lines.", 2, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "", 3m, "", user, now);
				AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "", 7m, "", user, ZDateTimeOffset.Empty);

				AssertEquals("New transfer line should be passed back.", 1, response1.NewTransferLines.Count);
				AssertEquals("IsValidLocation", false, response1.IsValidLocation);
				AssertEquals("IsValidPalletID", false, response1.IsValidPalletID);
				AssertEquals("IsValidProduct", false, response1.IsValidProduct);
				AssertEquals("IsValidOriginalInventoryHeldCode", false, response1.IsValidInventoryHeldCode);
				AssertEquals("IsValidAttributes", false, response1.IsValidAttributes);
				AssertEquals("TotalQuantityAvailableToPick", 0m, response1.TotalQuantityAvailableToPick);
				AssertEquals("TotalPickLineQuantity", 3m, response1.TotalPickLineQuantity);
			});

			var pickedTransferLine1 = transfer.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			CombineAssertions(() =>
			{
				AssertEquals("Transfer line that was picked should be returned in response.", pickedTransferLine1.PK.ToGuid(), response1.NewTransferLines[0].PK);
				AssertNotEquals("When transfer line is split, then new line should be picked, not existing line.", transferLine.PK, pickedTransferLine1.PK);
			});

			var allTransferLines = transfer.Lines.Cast<WhsTransferLine>();
			CombineAssertions(() =>
			{
				AssertEquals("All picked (In-Transit) transfer lines should have TotalUnits = Units.", true, allTransferLines.Where(l => l.IsPicked).All(l => l.WE_StockOnHand == l.WE_TransactionQuantity));
				AssertEquals("All unpicked (*not* In-Transit) transfer lines should have TotalUnits = 0.", true, allTransferLines.Where(l => !l.IsPicked).All(l => l.WE_StockOnHand == 0m));
			});

			var transferLineInfo2 = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 7m);
			transferLineInfo2.PK = transferLine.PK.ToGuid();

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.NewTransferLines);

			CombineAssertions(() =>
			{
				AssertEquals("If transfer line is fully picked, then it should not be split into 2 lines.", 2, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "", 3m, "", user, now);
				AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "", 7m, "", user, now);

				AssertEquals("IsValidLocation", false, response2.IsValidLocation);
				AssertEquals("IsValidPalletID", false, response2.IsValidPalletID);
				AssertEquals("IsValidProduct", false, response2.IsValidProduct);
				AssertEquals("IsValidOriginalInventoryHeldCode", false, response2.IsValidInventoryHeldCode);
				AssertEquals("IsValidAttributes", false, response2.IsValidAttributes);
				AssertEquals("TotalQuantityAvailableToPick", 0m, response2.TotalQuantityAvailableToPick);
				AssertEquals("TotalPickLineQuantity", 7m, response2.TotalPickLineQuantity);
			});

			var pickedTransferLine2 = transfer.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			CombineAssertions(() =>
			{
				AssertEquals("New transfer line should be passed back.", 1, response2.NewTransferLines.Count);
				AssertEquals("Transfer line that was picked should be returned in response.", pickedTransferLine2.PK.ToGuid(), response2.NewTransferLines[0].PK);
				AssertEquals("When transfer line is not split, then specified line should be picked.", transferLine.PK, pickedTransferLine2.PK);
			});

			allTransferLines = transfer.Lines.Cast<WhsTransferLine>();
			CombineAssertions(() =>
			{
				AssertEquals("All picked (In-Transit) transfer lines be set IsPicking = false.", true, allTransferLines.Where(l => l.IsPicked).All(l => l.PickLines.All(pl => !pl.WZ_IsPicking)));
				AssertEquals("All unpicked (*not* In-Transit) transfer lines should have TotalUnits = 0.", true, allTransferLines.Where(l => !l.IsPicked).All(l => l.WE_StockOnHand == 0m));
				AssertEquals("All unpicked (*not* In-Transit) transfer lines be set IsPicking = true.", true, allTransferLines.Where(l => !l.IsPicked).All(l => l.PickLines.All(pl => pl.WZ_IsPicking)));
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_AutomaticallyChooseAttributes_DifferentAttributes

		[TestDate(2013, 1, 1)]
		public void TestTryToAllocateStockForTransfer_AutomaticallyChooseAttributes_DifferentAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA11", "PA21", "PA31", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 5), new ZDate(2012, 1, 5), "PA12", "PA22", "PA32", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			//  RF Confirm Attribute Specified
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			// Attributes not specified and Qty to pick < Qty available. Inventory Attributes are different.
			var transferLineInfo1 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 60m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo1, false, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("System should not select any stock if it cannot do a valid inventory choice.", false, transfer.Lines.Any());
				AssertEquals("All matching stock should be counted as available, independent of its attributes.", 100m, response1.TotalQuantityAvailableToPick);
				AssertEquals("Nothing should be picked", 0m, response1.TotalPickLineQuantity);
			});

			// Attributes not specified and Qty to pick = Qty available. Inventory Attributes are different.
			var transferLineInfo2 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 100m);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo2, false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("System should not select any stock if its Product has RF Confirm Attribute, even if it can do a valid inventory choice.", false, transfer.Lines.Any());
				AssertEquals("All matching stock should be counted as available, independent of its attributes.", 100m, response2.TotalQuantityAvailableToPick);
				AssertEquals("Nothing should be picked", 0m, response2.TotalPickLineQuantity);
			});

			//  RF Confirm Attribute Not Specified
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;

			// Attributes not specified and Qty to pick < Qty available. Inventory Attributes are different.
			var transferLineInfo3 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 60m);
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1, user);
			var response3 = webService3.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo3, false, Guid.Empty);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response3.Error);
				AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
				AssertEquals("System should not select any stock if it cannot do a valid inventory choice.", false, transfer.Lines.Any());
				AssertEquals("All matching stock should be counted as available, independent of its attributes.", 100m, response3.TotalQuantityAvailableToPick);
				AssertEquals("Nothing should be picked", 0m, response3.TotalPickLineQuantity);
			});

			// Attributes not specified and Qty to pick = Qty available. Inventory Attributes are different. 
			var transferLineInfo4 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 100m);
			Helper.Factory.Save();

			var webService4 = GetNewWebService(data.Whs1, user);
			var response4 = webService4.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo4, false, Guid.Empty);
			AssertSuccessfulResponse(response4, webService4);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response4.Error);
				AssertEquals(true, string.IsNullOrEmpty(response4.ErrorMessage));
				AssertEquals("System should do the picking if it can do a valid inventory choice if product has no RF Confirm attribute, even if attributes are not specified.", 2, transfer.Lines.Count);
				transfer.Lines.Single(l => AttributeComparer.Compare(receive.Inventory[0], l));
				transfer.Lines.Single(l => AttributeComparer.Compare(receive.Inventory[1], l));
				AssertEquals("All matching stock should be counted as available, independent of its attributes.", 100m, response4.TotalQuantityAvailableToPick);
				AssertEquals("All stock should be picked if Qty to Pick = Qty Available.", 100m, response4.TotalPickLineQuantity);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_AutomaticallyChooseAttributes_SameAttributes

		[TestDate(2013, 1, 1)]
		public void TestTryToAllocateStockForTransfer_AutomaticallyChooseAttributes_SameAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			//  RF Confirm Attribute Specified
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			// Attributes not specified and Qty to pick < Qty available. Inventory Attributes are same.
			var transferLineInfo1 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 60m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo1, false, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("System should not select any stock if product has RF Confirm attribute, even if it can do a valid inventory choice.", false, transfer.Lines.Any());
				AssertEquals("All matching stock should be caunted as available, independent of its attributes.", 100m, response1.TotalQuantityAvailableToPick);
				AssertEquals("Nothing should be picked", 0m, response1.TotalPickLineQuantity);
			});

			//  RF Confirm Attribute Not Specified
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;

			// Attributes not specified and Qty to pick < Qty available. Inventory Attributes are same.
			var transferLineInfo2 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 60m);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo2, false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("System should do the picking if it can do a valid inventory choice and Product has no RF Confirm Attribute, even if attributes are not specified.", 1, transfer.Lines.Count);
				AssertNoExceptionThrown(() => transfer.Lines.Single(l => AttributeComparer.Compare(receive.Inventory[0], l)));
				AssertEquals("All matching stock should be counted as available, independent of its attributes.", 100m, response2.TotalQuantityAvailableToPick);
				AssertEquals("Any stock should be picked if it all has same attributes.", 60m, response2.TotalPickLineQuantity);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_CaseInsensitiveAttributes

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_CaseInsensitiveAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 2);
			var warehouse = data.Whs1;
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 2;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			warehouse.WW_LocationTraysFixedWidth = 2;

			Helper.Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			var now = ZDate.Today;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var sourceLocation = data.Whs1.FindLocation("A-01-01");
			var destinationLocation = data.Whs1.FindLocation("A-01-02");

			var expiryDate = now.AddDays(10);
			var packingDate = now.AddDays(-1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, sourceLocation, "PLT-1", expiryDate, packingDate, "A1", "A2", "A3", "");
			inventoryLine.WI_SerialNumber = "S1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, "PLT-1", destinationLocation, expiryDate, packingDate, "A1", "A2", "A3");
			transferLine.WE_SerialNumber = "S1";
			transfer.RunPreSaveValidation(); // to commit stock.

			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo(sourceLocation.WLV_LocationString_UserFriendly, "PLT-1", data.Org1, data.Part1, "", 1m, expiryDate, packingDate, "a1", "a2", "a3");
			transferLineInfo.SerialNumber = "s1";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_TryToTransferAllStockForOneSingleLine

		[TestDate(2013, 1, 1)]
		public void TestTryToAllocateStockForTransfer_TryToTransferAllStockForOneSingleLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var location = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT1", new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT1", new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT1", new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA2", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT1", new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA3", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT1", new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA4", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT1", new ZDate(2012, 10, 2), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT1", new ZDate(2012, 10, 1), new ZDate(2012, 1, 2), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "PLT2", new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, location, "PLT1", new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLineWithoutPalletId = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3");
			var transferLine1OnPLT1WithSameAttributes = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3");
			var transferLine2OnPLT1WithSameAttributes = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3");
			var transferLineWithDifferentAttribute1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA2", "PA2", "PA3");
			var transferLineWithDifferentAttribute2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA3", "PA3");
			var transferLineWithDifferentAttribute3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA4");
			var transferLineWithDifferentPackingDate = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 2), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3");
			var transferLineWithDifferentExpiryDate = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 2), "PA1", "PA2", "PA3");
			var transferLineWithDifferentPalletID = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location, "PLT2", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3");
			var transferLineWithDifferentProduct = Helper.CreateWhsTransferLine(transfer, data.Part2, 1m, location, "PLT1", destinationLocation, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3");
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "PLT1", data.Org1, data.Part1, "", 0m, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA1", "PA2", "PA3");
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			AssertContainsExactElementsInAnyOrder(new[] { transferLine1OnPLT1WithSameAttributes.PK.ToGuid(), transferLine2OnPLT1WithSameAttributes.PK.ToGuid() }, response.PickedTransferLinePks);

			var newFactory = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithoutPalletId.PK).PickedTime);
				AssertEquals(now, newFactory.Load<WhsTransferLine>(transferLine1OnPLT1WithSameAttributes.PK).PickedTime);
				AssertEquals(now, newFactory.Load<WhsTransferLine>(transferLine2OnPLT1WithSameAttributes.PK).PickedTime);
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithDifferentAttribute1.PK).PickedTime);
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithDifferentAttribute2.PK).PickedTime);
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithDifferentAttribute3.PK).PickedTime);
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithDifferentPackingDate.PK).PickedTime);
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithDifferentExpiryDate.PK).PickedTime);
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithDifferentPalletID.PK).PickedTime);
				AssertEquals(ZDateTimeOffset.Empty, newFactory.Load<WhsTransferLine>(transferLineWithDifferentProduct.PK).PickedTime);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_WithPackingDateOrExpiryDate

		public void TestTryToAllocateStockForTransfer_WithPackingDateOrExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.PackingDate, true);
			var now = ZDate.Today;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var expiryDate = now.AddDays(10);
			var packingDate = now.AddDays(-1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, sourceLocation, "PLT-1", expiryDate, packingDate, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, sourceLocation, "PLT-1", expiryDate, packingDate, "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, "PLT-1", destinationLocation, expiryDate, packingDate, "", "", "");
			transfer1.RunPreSaveValidation(); // to commit stock.

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer2, data.Part2, 1m, sourceLocation, "PLT-1", destinationLocation, expiryDate, packingDate, "", "", "");
			transfer2.RunPreSaveValidation(); // to commit stock.

			Helper.Factory.Save();

			var transferLineInfo1 = CreatePickingTransferLineInfo("A-1", "PLT-1", data.Org1, data.Part1, "", 1m, expiryDate, packingDate, "", "", "");
			var transferLineInfo2 = CreatePickingTransferLineInfo("A-1", "PLT-1", data.Org1, data.Part2, "", 1m);

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService1 = GetNewWebService(data.Whs1, user);
				var response1 = webService1.TryToAllocateStockForTransfer(transfer1.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
				AssertSuccessfulResponse(response1, webService1);

				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.None, response1.Error);
					AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
					AssertEquals("Transfer Line should be picked.", true, transfer1.Lines.Cast<WhsTransferLine>().Single().PickedTime.IsValid);
				});

				var webService2 = GetNewWebService(data.Whs1, user);
				var response2 = webService2.TryToAllocateStockForTransfer(transfer2.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
				AssertSuccessfulResponse(response2, webService2);

				CombineAssertions(() =>
				{
					AssertEquals("None of the stock available to transfer match entered attributes.", response2.ErrorMessage);
					AssertEquals("Transfer Line should not be Picked.", false, transfer2.Lines.Cast<WhsTransferLine>().Single().PickedTime.IsValid);
				});
			}
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidLocation

		public void TestTryToAllocateStockForTransfer_InvalidLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedErrorMessage = "No stock can be transferred from this Location.";

			var transferLineInfo1 = CreatePickingTransferLineInfo("TEST", "", null, data.Part1, "", 10m);
			var transferLineInfo2 = CreatePickingTransferLineInfo("A-2", "", null, data.Part1, "", 10m);
			var transferLineInfo3 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 10m);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(expectedErrorMessage, response1.ErrorMessage);
				AssertEquals("When location doesn't exist, no new transfer lines should be created.", 0, transfer.Lines.Count);
				AssertEquals(false, response1.IsValidLocation);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(expectedErrorMessage, response2.ErrorMessage);
				AssertEquals("When location have no stock, no new transfer lines should be created.", 0, transfer.Lines.Count);
				AssertEquals(false, response2.IsValidLocation);
			});

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo3, true, Guid.Empty);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response3.Error);
				AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
				AssertEquals("New transfer lines should be created with correct location.", 1, transfer.Lines.Count);
				AssertEquals(true, response3.IsValidLocation);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidLocation_PalletIDRequired

		[TestDate(2012, 5, 25, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_InvalidLocation_PalletIDRequired_WithPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locationString = "A-1";
			var palletID = "PLT-1";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			// stock with Pallet ID
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), palletID);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			// Test stock with a Pallet ID.
			var transferLineInfo = CreatePickingTransferLineInfo(locationString, "", null, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("System should transfer stock with a Pallet ID, if no stock without Pallet ID exist.", 1, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, locationString, palletID, 10m, "", user, now);
				AssertEquals(true, response.IsValidLocation);
			});
		}

		[TestDate(2012, 5, 25, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_InvalidLocation_PalletIDRequired_WithoutPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locationString = "A-1";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			// stock without Pallet ID
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			// Test stock without Pallet ID.
			var transferLineInfo = CreatePickingTransferLineInfo(locationString, "", null, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("System should transfer stock without Pallet ID if it exist.", 1, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, locationString, "", 10m, "", user, now);
				AssertEquals(true, response.IsValidLocation);
			});
		}

		[TestDate(2012, 5, 25, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_InvalidLocation_PalletIDRequired_MultiplePalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locationString = "A-1";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			// stock with multiple Pallet ID's only
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-4");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-5");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedErrorMessage = "This product exist on multiple pallets in this location. Please scan a Pallet ID that you want to transfer.";

			// Test stock with multiple Pallet ID's only
			var transferLineInfo = CreatePickingTransferLineInfo(locationString, "", null, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("System should not create a transfer line if it is unsure from which pallet ID it should take stock.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidLocation);
			});
		}

		[TestDate(2012, 5, 25, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_InvalidLocation_PalletIDRequired_MultipleAndMissingPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locationString = "A-1";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			// stock with multiple Pallet ID's and without Pallet ID
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation(locationString), "PLT-3");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			// Test stock with multiple Pallet ID's and without Pallet ID
			var transferLineInfo = CreatePickingTransferLineInfo(locationString, "", null, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("System should transfer stock without Pallet ID if it exist, even if another stock with Pallet ID exist.", 1, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, locationString, "", 10m, "", user, now);
				AssertEquals(true, response.IsValidLocation);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidPalletID

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidPalletID_MissingID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-2"), "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedNoLocationOrPalletIDErrorMessage = "Please enter a Location or Pallet ID to transfer inventory from.";

			// Test stock without Pallet ID.
			var transferLineInfo = CreatePickingTransferLineInfo("", "", null, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedNoLocationOrPalletIDErrorMessage, response.ErrorMessage);
				AssertEquals("If no Location or Pallet ID specified, system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidLocation);
				AssertEquals(false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidPalletID_IDWithoutStock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-2"), "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedErrorMessage = "No stock can be transferred from this Pallet ID.";

			// Test stock with Pallet ID without stock.
			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-2", null, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("If entered Pallet ID doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidLocation);
				AssertEquals(false, response.IsValidPalletID);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidProduct

		[TestDate(2012, 6, 7)]
		public void TestTryToAllocateStockForTransfer_InvalidProduct_ProductNotSpecified()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var client2 = Helper.CreateClient("CLIENT2");
			var part3 = Helper.CreateProduct(client2, "PART3");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", part3, 50m, data.Whs1.FindLocation("A-2"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedNoProductErrorMessage = "Please provide a valid product code or barcode.";

			// Test product is not specified.
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, null, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedNoProductErrorMessage, response.ErrorMessage);
				AssertEquals("If product not specified, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidProduct);
			});
		}

		public void TestTryToAllocateStockForTransfer_InvalidProduct_MissingProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var client2 = Helper.CreateClient("CLIENT2");
			var part3 = Helper.CreateProduct(client2, "PART3");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", part3, 50m, data.Whs1.FindLocation("A-2"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedErrorMessage = $"Product could not be found for client: {data.Org1.OH_Code}. Please provide a valid product code or barcode.";

			// Test product doesn't exist.
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 10m);

			transferLineInfo.Product.Code = "TEST";
			transferLineInfo.Product.PK = ZGuid.NewZGuid().ToGuid(); // product that doesn't exist.

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("If product cannot be found, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidProduct);
			});
		}

		public void TestTryToAllocateStockForTransfer_InvalidProduct_NoStockForProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var client2 = Helper.CreateClient("CLIENT2");
			var part3 = Helper.CreateProduct(client2, "PART3");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", part3, 50m, data.Whs1.FindLocation("A-2"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedNoStockErrorMessage = "No inventory of this Product is available to transfer.";

			// Test no stock for a product.
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part2, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedNoStockErrorMessage, response.ErrorMessage);
				AssertEquals("If no stock for a product can be found, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidProduct);
			});
		}

		public void TestTryToAllocateStockForTransfer_InvalidProduct_WrongClient()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var client2 = Helper.CreateClient("CLIENT2");
			var part3 = Helper.CreateProduct(client2, "PART3");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", part3, 50m, data.Whs1.FindLocation("A-2"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedErrorMessage = $"Product could not be found for client: {data.Org1.OH_Code}. Please provide a valid product code or barcode.";

			// Test product for a wrong client.
			var transferLineInfo = CreatePickingTransferLineInfo("A-2", "", null, part3, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("If product is for a wrong client, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidProduct);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidInventoryHeldCode

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryCode_NoStockWithAvailableStatus()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var damagedLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Damaged);
			var heldCodeChangeLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", abc_HeldCode.WHC_Code);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			heldCodeChangeLine.InDocketLine.IsInventoryEditForm = true;
			Helper.Factory.Save();

			var expectedErrorMessage = $"No inventory with Hold Code '' available to Transfer.";

			// Test no stock with Available status.
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("If inventory with desired status cannot be found, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidInventoryHeldCode);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryCode_NoStockWithHeldStatus()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var damagedLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Damaged);
			var heldCodeChangeLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", abc_HeldCode.WHC_Code);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			heldCodeChangeLine.InDocketLine.IsInventoryEditForm = true;
			Helper.Factory.Save();

			var expectedErrorMessage = $"No inventory with Hold Code '{InventoryStatus.Codes.Held}' available to Transfer.";

			// Test no stock with Held status.
			var transferLineInfo = CreatePickingTransferLineInfo("A-2", "", null, data.Part1, InventoryStatus.Codes.Held, 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("If inventory with desired status cannot be found, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidInventoryHeldCode);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryCode_NoStockWithDamagedStatus()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var damagedLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Damaged);
			var heldCodeChangeLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", abc_HeldCode.WHC_Code);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			heldCodeChangeLine.InDocketLine.IsInventoryEditForm = true;
			Helper.Factory.Save();

			var expectedErrorMessage = $"No inventory with Hold Code '{InventoryHoldCodes.Codes.Damaged}' available to Transfer.";

			// Test no stock with Damaged status.
			var transferLineInfo = CreatePickingTransferLineInfo("A-2", "", null, data.Part1, InventoryHoldCodes.Codes.Damaged, 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("If inventory with desired status cannot be found, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response.IsValidInventoryHeldCode);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryCode_TransferDamagedStock()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var damagedLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Damaged);
			var heldCodeChangeLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", abc_HeldCode.WHC_Code);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			heldCodeChangeLine.InDocketLine.IsInventoryEditForm = true;
			Helper.Factory.Save();

			// Test Transfer stock with Damaged status.
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, InventoryHoldCodes.Codes.Damaged, 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("If inventory with desired status is found, then system should create a Transfer Line.", 1, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "", 10m, InventoryHoldCodes.Codes.Damaged, user, now);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryCode_ChangedHeldCode()
		{
			var abc_HeldCode = Helper.CreateInventoryHeldCode("ABC", "ABC");
			var def_HeldCode = Helper.CreateInventoryHeldCode("DEF", "DEF");

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var damagedLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Damaged);
			var heldCodeChangeLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", abc_HeldCode.WHC_Code);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-1").PK, "", "", "", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 50m, data.Whs1.FindLocation("A-2").PK, "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			heldCodeChangeLine.InDocketLine.HeldCodeToChangeTo = def_HeldCode.WHC_Code;
			heldCodeChangeLine.InDocketLine.IsInventoryEditForm = true;
			Helper.Factory.Save();

			var expectedErrorMessage = $"No inventory with Hold Code '{abc_HeldCode.WHC_Code}' available to Transfer.";

			// Test Transfer stock with Held Code that has since been changed
			var transferLineInfo1 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, abc_HeldCode.WHC_Code, 10m);

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals(expectedErrorMessage, response1.ErrorMessage);
				AssertEquals("If inventory with desired status cannot be found, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(false, response1.IsValidInventoryHeldCode);
			});

			var transferLineInfo2 = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, def_HeldCode.WHC_Code, 10m);
			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("If inventory with desired status is found, then system should create a Transfer Line.", 1, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "", 10m, def_HeldCode.WHC_Code, user, now);
				AssertEquals(true, response2.IsValidInventoryHeldCode);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidQuantity

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidQuantity_MoreThanAvailable()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1"); // when inventory with pallet ID and without it exist, then only invntory without pallet ID should be considered.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			// Try to transfer more stock than available
			var pickAmount = 200m;
			var initialQuantityAvailble = 100m;
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", pickAmount);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals($"Cannot pick {pickAmount} Unit only {initialQuantityAvailble} Unit available to transfer.", response.ErrorMessage);
				AssertEquals("If Qty to transfer exceeds available to transfer stock, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals(initialQuantityAvailble, response.TotalQuantityAvailableToPick);
				AssertEquals(0m, response.TotalPickLineQuantity);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidQuantity_LessThanAvailable()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1"); // when inventory with pallet ID and without it exist, then only invntory without pallet ID should be considered.
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			// Try to transfer less stock than available
			var pickAmount = 60m;
			var initialQuantityAvailble = 100m;
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", pickAmount);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("If Qty to transfer equal or less that available to transfer stock, then system should create a Transfer Line.", 1, transfer.Lines.Count);
				AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "", pickAmount, "", user, now);
				AssertEquals(initialQuantityAvailble, response.TotalQuantityAvailableToPick);
				AssertEquals(pickAmount, response.TotalPickLineQuantity);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidAttributes

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidAttributes_NoAttributes()
		{
			// Try to transfer stock without attributes.
			TestTryToAllocateStockForTransfer_InvalidAttributesCore((info) =>
			{
				info.ExpiryDate = DateTime.MinValue;
				info.PackingDate = DateTime.MinValue;
				info.Attribute1 = "";
				info.Attribute2 = "";
				info.Attribute3 = "";
				info.SerialNumber = "";
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidAttributes_InvalidExpiryDate()
		{
			// Try to transfer stock with invalid Expiry Date.
			TestTryToAllocateStockForTransfer_InvalidAttributesCore((info) => info.ExpiryDate = new ZDate(2012, 10, 8).ToDateTime());
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidAttributes_InvalidPackingDate()
		{
			// Try to transfer stock with invalid Packing Date.
			TestTryToAllocateStockForTransfer_InvalidAttributesCore((info) => info.PackingDate = new ZDate(2012, 1, 25).ToDateTime());
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidAttributes_InvalidPartAttribute1()
		{
			// Try to transfer stock with invalid Part Attribute 1.
			TestTryToAllocateStockForTransfer_InvalidAttributesCore((info) => info.Attribute1 = "Test");
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidAttributes_InvalidPartAttribute2()
		{
			// Try to transfer stock with invalid Part Attribute 2.
			TestTryToAllocateStockForTransfer_InvalidAttributesCore((info) => info.Attribute2 = "Test");
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidAttributes_InvalidPartAttribute3()
		{
			// Try to transfer stock with invalid Part Attribute 3.
			TestTryToAllocateStockForTransfer_InvalidAttributesCore((info) => info.Attribute3 = "Test");
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidAttributes_InvalidSerialNumber()
		{
			// Try to transfer stock with invalid Serial Number.
			TestTryToAllocateStockForTransfer_InvalidAttributesCore((info) => info.SerialNumber = "Test");
		}

		void TestTryToAllocateStockForTransfer_InvalidAttributesCore(Action<WhsDocketLineInfo> changeLineToInvalidAttribute)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA11", "PA21", "PA31", "");
			line1.WI_SerialNumber = "SER1";
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 5), new ZDate(2012, 1, 5), "PA12", "PA22", "PA32", "");
			line2.WI_SerialNumber = "SER2";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var expectedErrorMessage = "None of the stock available to transfer match entered attributes.";

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 1m, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA11", "PA21", "PA31");
			transferLineInfo.SerialNumber = "SER1";
			changeLineToInvalidAttribute(transferLineInfo);

			// Try to transfer stock with invalid Expiry Date.
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals("When attributes are not correct, then system should not create a Transfer Line.", false, transfer.Lines.Any());
				AssertEquals("All matching stock should be counted as available, independent of its attributes.", 2m, response.TotalQuantityAvailableToPick);
				AssertEquals(0m, response.TotalPickLineQuantity);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_ValidAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA11", "PA21", "PA31", "");
			line1.WI_SerialNumber = "SER1";
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), new ZDate(2012, 10, 5), new ZDate(2012, 1, 5), "PA12", "PA22", "PA32", "");
			line2.WI_SerialNumber = "SER2";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			// Try to transfer stock with all attributes correct.
			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", null, data.Part1, "", 1m, new ZDate(2012, 10, 1), new ZDate(2012, 1, 1), "PA11", "PA21", "PA31");
			transferLineInfo.SerialNumber = "SER1";
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("When attributes are correct, then system should create a Transfer Line.", 1, transfer.Lines.Count);
				AssertEquals("All matching stock should be counted as available, independent of its attributes.", 2m, response.TotalQuantityAvailableToPick);
				AssertEquals("Only inventory that match attributes completely should be picked.", 1m, response.TotalPickLineQuantity);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_CreatesPickLinesOnSplit

		public void TestTryToAllocateStockForTransfer_CreatesPickLinesOnSplit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);
			transferLineInfo.PK = transferLine.PK.ToGuid();
			var webService = GetNewWebService();
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer line should be split in two.", 2, transfer.Lines.Count);
				AssertEquals("All transfer lines should have picklines attached.", 2, transfer.Lines.Cast<WhsTransferLine>().Count(l => l.PickLines.Count == 1));
				AssertEquals("All transfer lines should have correct committed qty.", true, transfer.Lines.Cast<WhsTransferLine>().All(l => l.QtyCommittedIncludingMatchingLines == l.QtyToMoveIncludingMatchingLines));
				AssertEquals("All stock should be committed to transfers.", 40m, receive.Inventory[0].WI_TotalUnits);
				AssertEquals("All stock should be committed to transfers.", 40m, receive.Inventory[0].WI_CommittedToTransferQuantity);
			});

			var inTransitLine = transfer.Lines.Single(l => l.WE_OriginalInventoryStatus == InventoryStatus.Codes.InTransit);
			var otherLine = transfer.Lines.Single(l => l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Available);
			CombineAssertions(() =>
			{
				AssertEquals("In-Transit line should have WE_StockOnHand correctly set.", 10m, inTransitLine.WE_StockOnHand);
				AssertEquals("WZ_IsPicking of In-transit line should be set to false", false, inTransitLine.PickLines.Single().WZ_IsPicking);
				AssertEquals("Unpicked line should *not* have WE_StockOnHand set.", 0m, otherLine.WE_StockOnHand);
				AssertEquals("WZ_IsPicking of Unpicked line should be set to true", true, otherLine.PickLines.Single().WZ_IsPicking);
				AssertNoExceptionThrown("Should have a valid datashape.", Helper.Factory.Save);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_CreatesPickLinesOnSplit_MatchingLines

		public void TestTryToAllocateStockForTransfer_CreatesPickLinesOnSplit_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();
			AssertEquals("Precondition: Line should be Committed.", 50m, transferLine.QtyCommittedIncludingMatchingLines);

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 40m);
			transferLineInfo.PK = transferLine.PK.ToGuid();
			var webService = GetNewWebService();
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer line should be split in two.", 2, transfer.Lines.Count);
				AssertEquals("Transfer Lines should be Split correctly with one Picked.", 1, transfer.Lines.Cast<WhsTransferLine>().Count(l => l.QtyToMoveIncludingMatchingLines == 40m && l.IsPicked));
				AssertEquals("Transfer Lines should be Split correctly.", 1, transfer.Lines.Cast<WhsTransferLine>().Count(l => l.QtyToMoveIncludingMatchingLines == 10m));
			});

			var allTransferLines = transfer.Lines.Cast<WhsTransferLine>()
				.Concat(transfer.Lines.Cast<WhsTransferLine>().SelectMany(l => l.MatchingLines.Cast<WhsTransferLine>())).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("All transfer lines should have picklines attached.", 3, allTransferLines.Count(l => l.PickLines.Count == 1));
				AssertEquals("All transfer lines should have correct committed qty.", true, allTransferLines.Where(l => l.IsMainTransactionLine()).All(l => l.QtyCommittedIncludingMatchingLines == l.QtyToMoveIncludingMatchingLines));
				AssertEquals("All transfer lines should have correct committed qty.", true, allTransferLines.Where(l => !l.IsMainTransactionLine()).All(l => l.GetQtyCommittedToThisLine() == l.WE_TransactionQuantity));
				AssertEquals("All picked (In-Transit) transfer lines should have TotalUnits = Units.", true, allTransferLines.Where(l => l.IsPicked).All(l => l.WE_StockOnHand == l.WE_TransactionQuantity));
				AssertEquals("All picked (In-Transit) transfer lines should have a Status of In-Transit.", true, allTransferLines.Where(l => l.IsPicked)
					.All(l => l.WE_OriginalInventoryStatus == InventoryStatus.Codes.InTransit && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));
				AssertEquals("The WZ_IsPicking in all picked (In-Transit) transfer lines should be set false", false, allTransferLines.Where(l => l.IsPicked).All(l => l.PickLines.All(p => p.WZ_IsPicking)));
			});

			var unpickedTransferLines = allTransferLines.Where(l => !l.IsPicked);
			CombineAssertions(() =>
			{
				AssertEquals("All unpicked (*not* In-Transit) transfer lines should have TotalUnits = 0.", true, unpickedTransferLines.All(l => l.WE_StockOnHand == 0m));
				AssertEquals("All unpicked (*not* In-Transit) transfer lines should have a Status of Available.", true, unpickedTransferLines
					.All(l => l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Available && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Available));
				AssertEquals("The WZ_IsPicking in all unpicked (*not* In-Transit) transfer lines should be set true", true, unpickedTransferLines.All(l => l.PickLines.All(p => p.WZ_IsPicking)));
				AssertNoExceptionThrown("Should have a valid datashape.", Helper.Factory.Save);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_TransferLineAlreadyPicked

		public void TestTryToAllocateStockForTransfer_TransferLineAlreadyPicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is Picked.", true, transferLine.PickedTime.IsValid);
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 12m);
			transferLineInfo.PK = transferLine.PK.ToGuid();
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should have correct Error.", "Transfer Line is already Picked.", response.ErrorMessage);
				AssertEquals("Response should have correct Error.", ErrorTypes.BusinessValidationError, response.Error);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InterWhsChild

		public void TestTryToAllocateStockForTransfer_InterWhsChild_Dest()
		{
			TestTryToAllocateStockForTransfer_InterWhsChild(isSource: false);
		}

		public void TestTryToAllocateStockForTransfer_InterWhsChild_Source()
		{
			TestTryToAllocateStockForTransfer_InterWhsChild(isSource: true);
		}

		void TestTryToAllocateStockForTransfer_InterWhsChild(bool isSource)
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

			var webService = GetNewWebService(warehouse2, staff);
			var transferLineInfo = CreatePickingTransferLineInfo("A", "", data.Org1, data.Part1, "", 10m);
			transferLineInfo.PK = transferLine.PK.ToGuid();
			AssertBusinessValidationError(webService, "Should have thrown an exception as child inter Warehouse transfers are not supported.",
				"Cannot transfer an Inter-Warehouse Transfer using the child job.",
				webService.TryToAllocateStockForTransfer(childTransfer.PK.ToGuid(), transferLineInfo, false, Guid.Empty));
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_InvalidInventoryStatus

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_ReceivedToDDL_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-1", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Pallet ID.", response.ErrorMessage);
				AssertEquals("If entered Pallet ID doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_ReceivedToDDL_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("DOCKDOOR", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Location.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_InTransit_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locA1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var othertransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TRO");
			var otherTransferLine = Helper.CreateWhsTransferLineWithInTransitInventory(othertransfer, data.Part1, 50m, locA1, "PLT-1", locA2, "PLT-1", user);
			AssertEquals("Precondition - ensure inventory is In-Transit.", "INT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-2", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Location.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_InTransit_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locA1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var othertransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TRO");
			var otherTransferLine = Helper.CreateWhsTransferLineWithInTransitInventory(othertransfer, data.Part1, 50m, locA1, "PLT-1", locA2, "PLT-1", user);
			AssertEquals("Precondition - ensure inventory is In-Transit.", "INT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-1", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Pallet ID.", response.ErrorMessage);
				AssertEquals("If entered Pallet ID doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_PuttingAway_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putting Away.", "PTA", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-2", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Location.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_PuttingAway_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putting Away.", "PTA", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-1", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Pallet ID.", response.ErrorMessage);
				AssertEquals("If entered Pallet ID doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_Putaway_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			otherTransfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(otherTransfer);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-2", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Location.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_Putaway_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			otherTransfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(otherTransfer);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-1", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Pallet ID.", response.ErrorMessage);
				AssertEquals("If entered Pallet ID doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_Staged_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var inventoryLine = receive.Lines[0];
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var stagedInventoryLine = Helper.PickAndMakeInTransitTransfer(order.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);

			var ddlTransfer = pick.Transfers.Single();
			ddlTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure inventory is Putaway.", InventoryStatus.Codes.Staged, stagedInventoryLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("DOCKDOOR", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Location.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_Staged_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var inventoryLine = receive.Lines[0];
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var stagedInventoryLine = Helper.PickAndMakeInTransitTransfer(order.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);

			var ddlTransfer = pick.Transfers.Single();
			ddlTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure inventory is Putaway.", InventoryStatus.Codes.Staged, stagedInventoryLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-1", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Pallet ID.", response.ErrorMessage);
				AssertEquals("If entered Pallet ID doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_ReceivedToDDLNotDefaultDDL_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDDL = data.Whs1.FindLocation("A-1");
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure otherDDL is DDL.", true, otherDDL.IsDockDoorLocation);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, otherDDL, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Location.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_ReceivedToDDLNotDefaultDDL_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDDL = data.Whs1.FindLocation("A-1");
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure otherDDL is DDL.", true, otherDDL.IsDockDoorLocation);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, otherDDL, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-1", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Pallet ID.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_DirectPutaway_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Location.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestTryToAllocateStockForTransfer_InvalidInventoryStatus_DirectPutaway_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("", "PLT-1", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock can be transferred from this Pallet ID.", response.ErrorMessage);
				AssertEquals("If entered Location doesn't exist or not available for transfer, then new transfer line should not be created.", false, transfer.Lines.Any());
				AssertEquals("IsValidLocation should be false", false, response.IsValidLocation);
				AssertEquals("IsValidPalletID should be false", false, response.IsValidPalletID);
			});
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_LocationFormattedCheckDigit

		[TestDate(2012, 5, 24, 5, 5, 0)]
		public void TestTryToAllocateStockForTransfer_LocationFormattedCheckDigit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.FormattedCheckDigit = "11";
			location2.FormattedCheckDigit = "22";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.GS_NKPickedBy = user.GS_Code;
			transferLine.WE_GS_NKPutawayBy = user.GS_Code;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 3m);
			transferLineInfo.PK = transferLine.PK.ToGuid();

			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.NewTransferLines);
			AssertEquals("11", response1.NewTransferLines[0].LocationFormattedCheckDigit);
			AssertEquals("22", response1.NewTransferLines[0].DestLocationFormattedCheckDigit);
		}

		#endregion

		#region TestTryToAllocateStockForTransfer_NoTransferPK_TaskManagementEnabled

		[TestDate(2025, 5, 28)]
		public void TestTryToAllocateStockForTransfer_NoTransferPK_TaskManagementEnabled()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo, false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.NewTransferLines);

			var transferTaskPK = response.Transfer.TaskPK;
			CombineAssertions(() =>
			{
				AssertNotEquals(Guid.Empty, transferTaskPK);
				AssertEquals("New transfer line should be passed back.", 1, response.NewTransferLines.Count);

				AssertEquals("IsValidLocation", true, response.IsValidLocation);
				AssertEquals("IsValidPalletID", false, response.IsValidPalletID);
				AssertEquals("IsValidProduct", true, response.IsValidProduct);
				AssertEquals("IsValidOriginalInventoryHeldCode", true, response.IsValidInventoryHeldCode);
				AssertEquals("IsValidAttributes", true, response.IsValidAttributes);
				AssertEquals("TotalQuantityAvailableToPick", 50m, response.TotalQuantityAvailableToPick);
				AssertEquals("TotalPickLineQuantity", 10m, response.TotalPickLineQuantity);
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transfer = newFactory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.PK, new ZGuid(response.Transfer.PK)));
			AssertNotNull(transfer);

			AssertAllocateTransferLineData(transfer, data.Part1, "A-1", "PLT-1", 10m, "", user, ZDateTimeOffset.Now);

			var pickedTransferLine = transfer.Lines.Single(l => l.WE_TransactionQuantity == 10m);

			CombineAssertions(() =>
			{
				AssertEquals(transferTaskPK, pickedTransferLine.WE_P9_Task);
				AssertEquals("In-Transit Transfer lines should have an Arrival Date set.", ZDateTimeOffset.Now, pickedTransferLine.WE_AdjustmentArrivalDate);
				AssertEquals("Transfer line that was picked should be returned in response.", pickedTransferLine.PK.ToGuid(), response.NewTransferLines[0].PK);
			});

			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);

			var transferTask = newFactory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK));
			AssertEquals(transferTaskPK, transferTask.PK);
			AssertEquals("A.A", transferTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferTask.P9_Status);
		}

		public void TestTryToAllocateStockForTransfer_NoTransferPK_WithTransferTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 10m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(Guid.Empty, transferLineInfo, false, ZGuid.BrettsGuid.ToGuid());
			AssertBusinessValidationError(webService, "Task started with no transfer started yet.", response);
		}

		#endregion

		#region TestTestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK

		public void TestTestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK() => TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPkCore(isSpecifiedLine: true);
		public void TestTestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_MatchedLine() => TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPkCore(isSpecifiedLine: false);

		void TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPkCore(bool isSpecifiedLine)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.GS_NKPickedBy = user.GS_Code;
			transferLine.WE_GS_NKPutawayBy = user.GS_Code;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 3m);
			if (isSpecifiedLine)
			{
				transferLineInfo.PK = transferLine.PK.ToGuid();
			}

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, transferProcessTask.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			if (isSpecifiedLine)
			{
				AssertNotNull(response.NewTransferLines);
			}
			else
			{
				AssertCollectionContains(transferLine.PK.ToGuid(), response.PickedTransferLinePks);
			}
		}

		public void TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_DifferentTaskAssignedToTheLine() => TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_DifferentTaskAssignedToTheLineCore(isSpecifiedLine: true);
		public void TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_DifferentTaskAssignedToTheLine_MatchedLine() => TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_DifferentTaskAssignedToTheLineCore(isSpecifiedLine: false);

		void TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_DifferentTaskAssignedToTheLineCore(bool isSpecifiedLine)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.GS_NKPickedBy = user.GS_Code;
			transferLine.WE_GS_NKPutawayBy = user.GS_Code;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 3m);
			if (isSpecifiedLine)
			{
				transferLineInfo.PK = transferLine.PK.ToGuid();
			}

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, ZGuid.BrettsGuid.ToGuid());
			AssertBusinessValidationError(webService, "Transfer Line is assigned to another user.", response);
		}

		public void TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_SpecifiedLineHasTask_TaskCopiedOnSplitLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();
			AssertEquals("Precondition: Line should be Committed.", 50m, transferLine.QtyCommittedIncludingMatchingLines);

			var transferTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part1, "", 40m);
			transferLineInfo.PK = transferLine.PK.ToGuid();
			var webService = GetNewWebService();
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, true, transferTask.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer line should be split in two.", 2, transfer.Lines.Count);
				AssertEquals("Transfer Lines should be Split correctly with one Picked.", 1, transfer.Lines.Cast<WhsTransferLine>().Count(l => l.QtyToMoveIncludingMatchingLines == 40m && l.IsPicked));
				AssertEquals("Transfer Lines should be Split correctly.", 1, transfer.Lines.Cast<WhsTransferLine>().Count(l => l.QtyToMoveIncludingMatchingLines == 10m));
			});

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			Assert(transferInNewFactory.Lines.All(l => l.WE_P9_Task == transferTask.PK));
		}

		[TestDate(2025, 5, 28)]
		public void TestTryToAllocateStockForTransfer_WithTransferPK_WithTransferTaskPK_NewLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, user);
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, transfer.Lines.Count);
			AssertEquals("Precondition", transferLine.WE_P9_Task, task.PK);

			var transferLineInfo = CreatePickingTransferLineInfo("A-1", "", data.Org1, data.Part2, "", 15m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToAllocateStockForTransfer(transfer.PK.ToGuid(), transferLineInfo, false, task.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(2, transfer.Lines.Count);
				AssertEquals(task.PK.ToGuid(), response.Transfer.TaskPK);
				AssertEquals("New transfer line should be passed back.", 1, response.NewTransferLines.Count);
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);

			AssertAllocateTransferLineData(transferInNewFactory, data.Part2, "A-1", "", 15m, "", user, ZDateTimeOffset.Now);

			var pickedTransferLine = transferInNewFactory.Lines.Single(l => l.WE_TransactionQuantity == 15m);
			AssertEquals(task.PK.ToGuid(), pickedTransferLine.WE_P9_Task);
		}

		#endregion

		#region AssertAllocateTransferLineData

		void AssertAllocateTransferLineData(WhsTransfer transfer, OrgSupplierPart part, string location, string palletID, decimal quantity, string heldCode, GlbStaff user, ZDateTimeOffset pickedTime)
		{
			AssertNoExceptionThrown(() => transfer.Lines.Cast<WhsTransferLine>().Single(l =>
				l.WE_OP == part.PK &&
				l.TransferFromLocationString.EqualsIgnoringCase(location) &&
				l.WE_TransferFromPalletId.EqualsIgnoringCase(palletID) &&
				l.QtyToMoveIncludingMatchingLines == quantity &&
				l.WE_WHC_NKOriginalInventoryHeldCode.EqualsIgnoringCase(heldCode) &&
				((user == null && l.GS_NKPickedBy.IsEmpty) || l.GS_NKPickedBy == user.GS_Code) &&
				l.PickedTime == pickedTime
			));
		}

		#endregion
	}
}
