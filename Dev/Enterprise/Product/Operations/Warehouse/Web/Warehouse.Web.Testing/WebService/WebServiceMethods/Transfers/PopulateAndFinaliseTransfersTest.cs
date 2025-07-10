using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PopulateAndFinaliseTransfersTest : WhsTransferSecureServiceTestCase
	{
		#region TestPopulateAndFinaliseTransfers

		[TestDate(2012, 2, 22, 5, 5, 0)]
		public void TestPopulateAndFinaliseTransfers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0], "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].ToLocationString(), "");
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.GS_NKPickedBy = "A.A";
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new Guid[] { transfer.PK.ToGuid() }, locations[1].ToLocationString(), Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = transferInOtherFactory.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should be finalised.", true, transferInOtherFactory.IsFinalised);
				AssertEquals("Transfer should have only 1 line.", 1, transferInOtherFactory.Lines.Count);
				AssertEquals("PalletID should stay the same.", "PLT-1", transferLineInOtherFactory.WE_PalletID);
				AssertEquals("Destination locations should be updated.", locations[1].PK, transferLineInOtherFactory.WE_WL);
				AssertEquals("Putaway By should be updated.", "A.A", transferLineInOtherFactory.WE_GS_NKPutawayBy);
				AssertEquals("Putaway Time should be updated.", ZDateTimeOffset.Now, transferLineInOtherFactory.WE_PutawayTime);
			});

			AssertTransferEventsCreated(transferInOtherFactory, 1, 0, 0, 0, 1, 1);
		}

		#endregion

		#region TestPopulateAndFinaliseTransfers_PickFaceDestination

		[TestDate(2012, 2, 22, 5, 5, 0)]
		public void TestPopulateAndFinaliseTransfers_PickFaceDestination_RetainPalletIDFlagIsTrue()
		{
			TestPopulateAndFinaliseTransfers_PickFaceDestination_RetainPalletIDFlagCore(shouldRetainPalletIDs: true);
		}

		[TestDate(2012, 2, 22, 5, 5, 0)]
		public void TestPopulateAndFinaliseTransfers_PickFaceDestination_RetainPalletIDFlagIsFalse()
		{
			TestPopulateAndFinaliseTransfers_PickFaceDestination_RetainPalletIDFlagCore(shouldRetainPalletIDs: false);
		}

		void TestPopulateAndFinaliseTransfers_PickFaceDestination_RetainPalletIDFlagCore(bool shouldRetainPalletIDs)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, locationA2);
			locationA2.LocationType.WLT_RetainPalletIDsInFixedPickFaces = shouldRetainPalletIDs;

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.GS_NKPickedBy = "A.A";
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new[] { transfer.PK.ToGuid() }, "A-2", Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = transferInOtherFactory.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should be finalised.", true, transferInOtherFactory.IsFinalised);
				AssertEquals("Transfer should have only 1 line.", 1, transferInOtherFactory.Lines.Count);
				AssertEquals("PalletID should be cleared because destination is a pickface.", shouldRetainPalletIDs ? "PLT-1" : "", transferLineInOtherFactory.WE_PalletID);
				AssertEquals("Destination locations should be updated.", locations[1].PK, transferLineInOtherFactory.WE_WL);
				AssertEquals("Putaway By should be updated.", "A.A", transferLineInOtherFactory.WE_GS_NKPutawayBy);
				AssertEquals("Putaway Time should be updated.", ZDateTimeOffset.Now, transferLineInOtherFactory.WE_PutawayTime);
			});
		}

		[TestDate(2012, 2, 22, 5, 5, 0)]
		public void TestPopulateAndFinaliseTransfers_PickFaceDestination_DifferentClientPickFaces()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var differentClient = Helper.CreateClient("C2", "C2");
			Helper.CreateProductClientRelationShip(differentClient, data.Part1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, differentClient, pickfaceLocation);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.GS_NKPickedBy = "A.A";
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new[] { transfer.PK.ToGuid() }, "A-2", Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = transferInOtherFactory.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should be finalised.", true, transferInOtherFactory.IsFinalised);
				AssertEquals("Transfer should have only 1 line.", 1, transferInOtherFactory.Lines.Count);
				AssertEquals("PalletID should be dropped because A-2 is pickface location and it doesn't allow pallets.", "", transferLineInOtherFactory.WE_PalletID);
				AssertEquals("Destination locations should be updated.", locations[1].PK, transferLineInOtherFactory.WE_WL);
				AssertEquals("Putaway By should be updated.", "A.A", transferLineInOtherFactory.WE_GS_NKPutawayBy);
				AssertEquals("Putaway Time should be updated.", ZDateTimeOffset.Now, transferLineInOtherFactory.WE_PutawayTime);
			});
		}

		[TestDate(2012, 2, 22, 5, 5, 0)]
		public void TestPopulateAndFinaliseTransfers_PickFaceDestination_DifferentWarehousePickFaces()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var differentWarehouse = Helper.CreateWarehouse("Whs2", "A", 2, 1);
			Helper.Factory.Save();

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var pickfaceLocationInDifferentWarehouse = differentWarehouse.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocationInDifferentWarehouse);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.GS_NKPickedBy = "A.A";
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new[] { transfer.PK.ToGuid() }, "A-2", Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = transferInOtherFactory.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should be finalised.", true, transferInOtherFactory.IsFinalised);
				AssertEquals("Transfer should have only 1 line.", 1, transferInOtherFactory.Lines.Count);
				AssertEquals("PalletID should be not be dropped because pickface location is in another warehouse.", "PLT-1", transferLineInOtherFactory.WE_PalletID);
				AssertEquals("Destination locations should be updated.", locations[1].PK, transferLineInOtherFactory.WE_WL);
				AssertEquals("Putaway By should be updated.", "A.A", transferLineInOtherFactory.WE_GS_NKPutawayBy);
				AssertEquals("Putaway Time should be updated.", ZDateTimeOffset.Now, transferLineInOtherFactory.WE_PutawayTime);
			});
		}

		#endregion

		#region TestPopulateAndFinaliseTransfers_InterWhsChild

		public void TestPopulateAndFinaliseTransfers_InterWhsChild_Dest()
		{
			TestPopulateAndFinaliseTransfers_InterWhsChild(isSource: false);
		}

		public void TestPopulateAndFinaliseTransfers_InterWhsChild_Source()
		{
			TestPopulateAndFinaliseTransfers_InterWhsChild(isSource: true);
		}

		void TestPopulateAndFinaliseTransfers_InterWhsChild(bool isSource)
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

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			Helper.Factory.Save();

			var transferLineInfo = CreatePickingTransferLineInfo("A", "", data.Org1, data.Part1, "", 10m);
			transferLineInfo.PK = transferLine.PK.ToGuid();

			var webService = GetNewWebService(warehouse2, staff);
			AssertBusinessValidationError(webService, "Should have thrown an exception as child inter  Warehouse transfers are not supported.",
				"Cannot transfer an Inter-Warehouse Transfer using the child job.", webService.PopulateAndFinaliseTransfers(new[] { childTransfer.PK.ToGuid() }, "A", Guid.Empty));
		}

		#endregion

		#region TestPopulateAndFinaliseTransfers_FixedWidthLocation

		[TestDate(2012, 2, 22, 5, 5, 0)]
		public void TestPopulateAndFinaliseTransfers_FixedWidthLocation_UserFriendlyString()
		{
			TestPopulateAndFinaliseTransfers_FixedWidthLocationCore(true);
		}

		[TestDate(2012, 2, 22, 5, 5, 0)]
		public void TestPopulateAndFinaliseTransfers_FixedWidthLocation_LocationString()
		{
			TestPopulateAndFinaliseTransfers_FixedWidthLocationCore(false);
		}

		void TestPopulateAndFinaliseTransfers_FixedWidthLocationCore(bool usingUserFriendlyString)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 2);
			var warehouse = data.Whs1;
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 2;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			warehouse.WW_LocationTraysFixedWidth = 2;

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0], "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].ToLocationString(), locations[1].ToLocationString());
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.GS_NKPickedBy = "A.A";
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var destination = usingUserFriendlyString ? locations[1].WLV_LocationString_UserFriendly : locations[1].WLV_LocationString;
			var response = webService.PopulateAndFinaliseTransfers([transfer.PK.ToGuid()], destination, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = transferInOtherFactory.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should be finalised.", true, transferInOtherFactory.IsFinalised);
				AssertEquals("Transfer should have only 1 line.", 1, transferInOtherFactory.Lines.Count);
				AssertEquals("PalletID should stay the same.", "PLT-1", transferLineInOtherFactory.WE_PalletID);
				AssertEquals("Destination locations should not be updated.", locations[1].PK, transferLineInOtherFactory.WE_WL);
				AssertEquals("Putaway By should be updated.", "A.A", transferLineInOtherFactory.WE_GS_NKPutawayBy);
				AssertEquals("Putaway Time should be updated.", ZDateTimeOffset.Now, transferLineInOtherFactory.WE_PutawayTime);
			});

			AssertTransferEventsCreated(transferInOtherFactory, 1, 0, 0, 0, 1, 1);

			var cidLogs = Helper.FindLogs(transfer.Logs, ZArchitecture.Business.Events.ChangeOfIdentifier);
			AssertEquals(0, cidLogs.Length);
		}

		#endregion

		#region TestTransferEvents

		public void TestTransferEvents()
		{
			// create inventory
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			// 1. choose Loc to Loc, after create transfer then suspend
			// 1a) CreateNewWhsTransfer
			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.CreateNewWhsTransfers("A-1");
			AssertSuccessfulResponse(response1, webService1);

			AssertEquals("Only 1 transfer should be created.", 1, response1.Transfers.Length);

			var transfer = Helper.Factory.Load<WhsTransfer>(new ZGuid(response1.Transfers[0].PK));
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);

			// 1b) Suspend
			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.AddWhsEventLog(AutoEvents.ServiceSuspended.Code, WhsDocketSchema.Constants.Prefix, transfer.PK.ToGuid());
			AssertSuccessfulResponse(response2, webService2);

			AssertTransferEventsCreated(transfer, 1, 1, 1, 0, 0, 0);

			// 2. choose transfer unfinished
			// 2a) GetWhsTransfer
			var webService3 = GetNewWebService(data.Whs1, user);
			var response3 = webService3.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertSuccessfulResponse(response3, webService3);

			AssertEquals("System should find transfer by its external reference.", transfer.PK.ToGuid(), response3.Docket.PK);
			AssertTransferEventsCreated(transfer, 1, 2, 1, 0, 0, 0);

			// 2b) enter destination location and destination PalletID
			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-1", "A-2");
			var webService4 = GetNewWebService(data.Whs1, user);
			var response4 = webService4.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response4, webService4);

			// 2c) finalise
			var webService5 = GetNewWebService(data.Whs1, user);
			var response5 = webService5.FinaliseDocket(transfer.PK.ToGuid(), RFDocketType.WhsTransfer);
			AssertSuccessfulResponse(response5, webService5);

			AssertTransferEventsCreated(transfer, 1, 2, 1, 0, 1, 1);
		}

		public void TestTransferEvents_ChangeOfIdentifier()
		{
			// create inventory
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// create transfer
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "PLT-1", "A-2", "PLT-2");
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			// 2. choose transfer unfinished
			// 2a) GetWhsTransfer
			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("System should find transfer by its external reference.", transfer.PK.ToGuid(), response1.Docket.PK);
				AssertEquals("Only lines that need Allocation should be send to device.", 1, response1.Docket.Lines.Count);
			});

			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);

			// 2b) enter destination location and destination PalletID
			var transferLineInfo = CreatePutawayTransferLineInfo(null, "PLT-1", "PLT-3", "A-3");

			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			AssertTransferEventsCreated(transfer, 1, 1, 0, 1, 0, 0);

			var cidLog = Helper.FindLogs(transfer.Logs, ZArchitecture.Business.Events.ChangeOfIdentifier)[0];
			var expectedMessage = "RF: Line No. 1 - [Dest. Location changed from 'A-2';Dest. Pallet ID changed from 'PLT-2';]";
			AssertEquals(expectedMessage, cidLog.ReferenceFreeText);

			var otherFactory = new BusinessObjectFactory();
			var transfer2 = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLine2 = transfer2.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("PLT-3", transferLine2.WE_PalletID);
				AssertEquals("A-3", transferLine2.LocationString);
			});
		}

		public void TestTransferEvents_ChangeOfIdentifier_QtyChange()
		{
			// create inventory
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var now = ZDateTimeOffset.Now;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// create transfer
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var line1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			var line2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "", "");
			transfer.RunPreSaveValidation(); // to commit stock
			line1.PickedTime = now;
			line2.PickedTime = now;
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "PLT-2", "A-2", 25m);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			AssertEquals("One of the Transfer Lines should had splitted into 2 lines.", 3, transfer.Lines.Count);

			var transferLine1 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 5m);
			var transferLine2 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 15m);
			var transferLine3 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 20m);

			AssertTransferEventsCreated(transfer, 1, 0, 0, 1, 0, 0);

			var cidLog = Helper.FindLogs(transfer.Logs, ZArchitecture.Business.Events.ChangeOfIdentifier)[0];
			var expectedMessage = "RF: Line No. 2 - [Qty changed from 20.000]";

			AssertEquals(expectedMessage, cidLog.ReferenceFreeText);
		}

		#endregion

		#region TestPopulateAndFinaliseTransfers_PickFaceDestination_PartOfTheDestinationLocationEntered

		[TestDate(2014, 12, 2)]
		public void TestPopulateAndFinaliseTransfers_PickFaceDestination_PartOfTheDestinationLocationEntered()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, locationA1);
			Helper.Factory.Save();

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA2, "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-2", "PLT-1", "", "");
			transferLine.GS_NKPickedBy = "A.A";
			transferLine.PickedTime = now;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new[] { transfer.PK.ToGuid() }, "A-1", Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = transferInOtherFactory.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should be finalised.", true, transferInOtherFactory.IsFinalised);
				AssertEquals("Transfer should have only 1 line.", 1, transferInOtherFactory.Lines.Count);
				AssertEquals("PalletID should be cleared because destination is a pickface.", "", transferLineInOtherFactory.WE_PalletID);
				AssertEquals("Destination locations should be updated.", locationA1.PK, transferLineInOtherFactory.WE_WL);
				AssertEquals("Putaway By should be updated.", "A.A", transferLineInOtherFactory.WE_GS_NKPutawayBy);
				AssertEquals("Putaway Time should be updated.", ZDateTimeOffset.Now, transferLineInOtherFactory.WE_PutawayTime);
			});
		}

		#endregion

		#region TestPopulateAndFinaliseTransfers_TaskManagementEnabled

		[TestDate(2025, 5, 19)]
		public void TestPopulateAndFinaliseTransfers_WithProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var sourceLocation = locations[0];
			var destinationLocation = locations[1];
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT-1");
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer1Line = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer1Line.WE_TransferFromPalletId = "PLT-1";
			transfer1Line.GS_NKPickedBy = "A.A";
			transfer1Line.PickedTime = ZDateTimeOffset.Now;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, sourceLocation, "PLT-2");
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer2Line = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer2Line.WE_TransferFromPalletId = "PLT-2";
			transfer2Line.GS_NKPickedBy = "A.A";
			transfer2Line.PickedTime = ZDateTimeOffset.Now;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, sourceLocation, "PLT-3");
			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer3Line = Helper.CreateWhsTransferLine(transfer3, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer3Line.WE_TransferFromPalletId = "PLT-3";
			transfer3Line.GS_NKPickedBy = "A.A";
			transfer3Line.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var processTask1 = Helper.CreateProcessTaskForTransfer(transfer1, user);
			AssertEquals(processTask1.PK, transfer1Line.WE_P9_Task);
			var processTask2 = Helper.CreateProcessTaskForTransfer(transfer2, user);
			AssertEquals(processTask2.PK, transfer2Line.WE_P9_Task);
			var processTask3 = Helper.CreateProcessTaskForTransfer(transfer3, user);
			AssertEquals(processTask3.PK, transfer3Line.WE_P9_Task);
			Helper.Factory.Save();

			var processTaskPKToPass = processTask2.PK;
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new Guid[] { transfer1.PK.ToGuid(), transfer2.PK.ToGuid(), transfer3.PK.ToGuid() }, destinationLocation.ToLocationString(), processTaskPKToPass.ToGuid());
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			AssertTransferAndTransferLineWithProcessTaskPK(otherFactory, transfer1, processTaskPKToPass, destinationLocation.PK, "PLT-1", "A.A");
			AssertTransferAndTransferLineWithProcessTaskPK(otherFactory, transfer2, processTaskPKToPass, destinationLocation.PK, "PLT-2", "A.A");
			AssertTransferAndTransferLineWithProcessTaskPK(otherFactory, transfer3, processTaskPKToPass, destinationLocation.PK, "PLT-3", "A.A");

			AssertNull("Other process tasks are deleted.", otherFactory.Load<ProcessTask>(processTask1.PK));
			AssertNull("Other process tasks are deleted.", otherFactory.Load<ProcessTask>(processTask3.PK));
		}

		[TestDate(2025, 5, 19)]
		public void TestPopulateAndFinaliseTransfers_NoProcessTask_LinesWithTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var sourceLocation = locations[0];
			var destinationLocation = locations[1];
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT-1");
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer1Line = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer1Line.WE_TransferFromPalletId = "PLT-1";
			transfer1Line.GS_NKPickedBy = "A.A";
			transfer1Line.PickedTime = ZDateTimeOffset.Now;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, sourceLocation, "PLT-2");
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer2Line = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer2Line.WE_TransferFromPalletId = "PLT-2";
			transfer2Line.GS_NKPickedBy = "A.A";
			transfer2Line.PickedTime = ZDateTimeOffset.Now;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, sourceLocation, "PLT-3");
			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer3Line = Helper.CreateWhsTransferLine(transfer3, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer3Line.WE_TransferFromPalletId = "PLT-3";
			transfer3Line.GS_NKPickedBy = "A.A";
			transfer3Line.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var processTask1 = Helper.CreateProcessTaskForTransfer(transfer1, user);
			AssertEquals(processTask1.PK, transfer1Line.WE_P9_Task);
			var processTask2 = Helper.CreateProcessTaskForTransfer(transfer2, user);
			AssertEquals(processTask2.PK, transfer2Line.WE_P9_Task);
			var processTask3 = Helper.CreateProcessTaskForTransfer(transfer3, user);
			AssertEquals(processTask3.PK, transfer3Line.WE_P9_Task);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new Guid[] { transfer1.PK.ToGuid(), transfer2.PK.ToGuid(), transfer3.PK.ToGuid() }, destinationLocation.ToLocationString(), Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			AssertTransferAndTransferLineWithProcessTaskPK(otherFactory, transfer1, processTask1.PK, destinationLocation.PK, "PLT-1", "A.A");
			AssertTransferAndTransferLineWithProcessTaskPK(otherFactory, transfer2, processTask2.PK, destinationLocation.PK, "PLT-2", "A.A");
			AssertTransferAndTransferLineWithProcessTaskPK(otherFactory, transfer3, processTask3.PK, destinationLocation.PK, "PLT-3", "A.A");
		}

		public void TestPopulateAndFinaliseTransfers_ProcessTaskDoesNotMatchTransfers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var sourceLocation = locations[0];
			var destinationLocation = locations[1];
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT-1");
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer1Line = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer1Line.WE_TransferFromPalletId = "PLT-1";
			transfer1Line.GS_NKPickedBy = "A.A";
			transfer1Line.PickedTime = ZDateTimeOffset.Now;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, sourceLocation, "PLT-2");
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer2Line = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer2Line.WE_TransferFromPalletId = "PLT-2";
			transfer2Line.GS_NKPickedBy = "A.A";
			transfer2Line.PickedTime = ZDateTimeOffset.Now;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, sourceLocation, "PLT-3");
			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer3Line = Helper.CreateWhsTransferLine(transfer3, data.Part1, 10m, sourceLocation.ToLocationString(), "");
			transfer3Line.WE_TransferFromPalletId = "PLT-3";
			transfer3Line.GS_NKPickedBy = "A.A";
			transfer3Line.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			var processTask1 = Helper.CreateProcessTaskForTransfer(transfer1, user);
			AssertEquals(processTask1.PK, transfer1Line.WE_P9_Task);
			var processTask2 = Helper.CreateProcessTaskForTransfer(transfer2, user);
			AssertEquals(processTask2.PK, transfer2Line.WE_P9_Task);
			var processTask3 = Helper.CreateProcessTaskForTransfer(transfer3, user);
			AssertEquals(processTask3.PK, transfer3Line.WE_P9_Task);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.PopulateAndFinaliseTransfers(new Guid[] { transfer1.PK.ToGuid(), transfer2.PK.ToGuid(), transfer3.PK.ToGuid() }, destinationLocation.ToLocationString(), ZGuid.BrettsGuid.ToGuid());
			AssertBusinessValidationError(webService, "Process task does not match any of the transfers to finalize.", response);
		}

		void AssertTransferAndTransferLineWithProcessTaskPK(BusinessObjectFactory factory, WhsTransfer transfer, ZGuid processTaskPK, ZGuid locationPK, string palletID, string staffCode)
		{
			var transferInOtherFactory = factory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = transferInOtherFactory.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should be finalised.", true, transferInOtherFactory.IsFinalised);
				AssertEquals("Transfer should have only 1 line.", 1, transferInOtherFactory.Lines.Count);
				AssertEquals("PalletID should stay the same.", palletID, transferLineInOtherFactory.WE_PalletID);
				AssertEquals("Destination locations should be updated.", locationPK, transferLineInOtherFactory.WE_WL);
				AssertEquals("Putaway By should be updated.", staffCode, transferLineInOtherFactory.WE_GS_NKPutawayBy);
				AssertEquals("Putaway Time should be updated.", ZDateTimeOffset.Now, transferLineInOtherFactory.WE_PutawayTime);
				AssertEquals("Process task PK is correct.", processTaskPK, transferLineInOtherFactory.WE_P9_Task);
			});

			AssertTransferEventsCreated(transferInOtherFactory, 1, 0, 0, 0, 1, 1);
		}

		#endregion
	}
}
