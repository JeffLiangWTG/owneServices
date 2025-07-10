using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class BeginRFPutawayTaskTest : WhsSecureServiceTestCase
	{
		public void TestBeginRFPutawayTask_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPutawayTask(Guid.NewGuid());

			AssertBusinessValidationError(webService, "Task could not be found.", response);
		}

		public void TestBeginRFPutawayTask_WrongTaskType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receiveWithDockDoor =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 5));
			var inventory = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory.InDocketLine.WE_PalletID = "12345";
			inventory.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;

			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "12345", 10m);
			putawayTransferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receiveWithDockDoor, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPutawayTask(task.PK.ToGuid());

			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFPutawayTask_LoadsSinglePutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var receiveWithDockDoor =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 5));
			var inventory = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory.InDocketLine.WE_PalletID = "12345";
			inventory.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;

			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "12345", 10m);
			putawayTransferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPutawayTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var palletsOnReponse = response.PalletInfos;
			AssertEquals("Response has 1 Pallet", 1, palletsOnReponse.Length);
			AssertEquals("Response has correct PalletID", "12345", palletsOnReponse[0].PalletID);

			var newFactory = new BusinessObjectFactory();
			var transferLineInOtherFactory = newFactory.Load<WhsTransferLine>(putawayTransferLine.PK);
			AssertEquals(staff.GS_Code, transferLineInOtherFactory.WE_GS_NKPutawayBy);
		}

		public void TestBeginRFPutawayTask_LoadsSinglePutaway_TaskAlreadyStarted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var receiveWithDockDoor =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 5));
			var inventory = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory.InDocketLine.WE_PalletID = "12345";
			inventory.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;

			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "12345", 10m);
			putawayTransferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var saveCount = 0;
			var webService = GetNewWebService(data.Whs1, staff);
			webService.Factory.Saving += f => saveCount++;
			var response = webService.BeginRFPutawayTask(task.PK.ToGuid());
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Factory Save should not be called.", 0, saveCount);

			var palletsOnReponse = response.PalletInfos;
			AssertEquals("Response has 1 Pallet", 1, palletsOnReponse.Length);
			AssertEquals("Response has correct PalletID", "12345", palletsOnReponse[0].PalletID);
		}

		public void TestBeginRFPutawayTask_LoadsMultiPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var receiveWithDockDoor =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 5));
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory1.InDocketLine.WE_PalletID = "12345";
			inventory1.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory1.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory2.InDocketLine.WE_PalletID = "9876";
			inventory2.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "12345", 10m);
			putawayTransferLine1.RunPreSaveValidation();
			var putawayTransferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "9876", 10m);
			putawayTransferLine2.RunPreSaveValidation();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPutawayTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var palletsOnReponse = response.PalletInfos;
			AssertEquals("Response has 2 Pallets", 2, palletsOnReponse.Length);
			AssertContainsExactElementsInAnyOrder("Response has correct PalletIDs", ["12345", "9876"], palletsOnReponse.Select(l => l.PalletID));

			var newFactory = new BusinessObjectFactory();
			var transferLine1InOtherFactory = newFactory.Load<WhsTransferLine>(putawayTransferLine1.PK);
			AssertEquals(staff.GS_Code, transferLine1InOtherFactory.WE_GS_NKPutawayBy);
			var transferLine2InOtherFactory = newFactory.Load<WhsTransferLine>(putawayTransferLine2.PK);
			AssertEquals(staff.GS_Code, transferLine2InOtherFactory.WE_GS_NKPutawayBy);
		}

		public void TestBeginRFPutawayTask_LoadsMultiPutaway_MultiProductsPerPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var receiveWithDockDoor =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 5));
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory1.InDocketLine.WE_PalletID = "12345";
			inventory1.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory1.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m);
			inventory2.InDocketLine.WE_PalletID = "9876";
			inventory2.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part2, 10m);
			inventory3.InDocketLine.WE_PalletID = "9876";
			inventory3.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory3.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "12345", 10m);
			putawayTransferLine1.RunPreSaveValidation();
			var putawayTransferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, "9876", 10m);
			putawayTransferLine2.RunPreSaveValidation();
			var putawayTransferLine3 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part2,
				dockDoorLocation, nonDockDoorLocation, "9876", 10m);
			putawayTransferLine3.RunPreSaveValidation();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPutawayTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var palletsOnReponse = response.PalletInfos;
			AssertEquals("Response has 2 Pallets", 2, palletsOnReponse.Length);
			AssertContainsExactElementsInAnyOrder("Response has correct PalletIDs", ["12345", "9876"], palletsOnReponse.Select(l => l.PalletID));

			var newFactory = new BusinessObjectFactory();
			var transferLine1InOtherFactory = newFactory.Load<WhsTransferLine>(putawayTransferLine1.PK);
			AssertEquals(staff.GS_Code, transferLine1InOtherFactory.WE_GS_NKPutawayBy);
			var transferLine2InOtherFactory = newFactory.Load<WhsTransferLine>(putawayTransferLine2.PK);
			AssertEquals(staff.GS_Code, transferLine2InOtherFactory.WE_GS_NKPutawayBy);
			var transferLine3InOtherFactory = newFactory.Load<WhsTransferLine>(putawayTransferLine3.PK);
			AssertEquals(staff.GS_Code, transferLine3InOtherFactory.WE_GS_NKPutawayBy);
		}

		public void TestBeginRFPutawayTask_DbHitsTest()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var palletIDs = new List<string>();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 20; i++)
			{
				var palletID = $"PLT{i}";
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
				inventory.InDocketLine.WE_PalletID = palletID;
				inventory.InDocketLine.WE_WL = dockDoorLocation.PK;
				inventory.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
				palletIDs.Add(palletID);
			}
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			for (var i = 0; i < 20; i++)
			{
				var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation, nonDockDoorLocation, $"PLT{i}", 10m);
				putawayTransferLine.RunPreSaveValidation();
			}
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			Helper.Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 3 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 4 },
			};

			PutawayMultiplePalletsWebServiceResponse response = null;
			var webService = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService.Factory))
			using (RowFactory.SetCachedTables())
			{
				response = webService.BeginRFPutawayTask(task.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			var palletsOnReponse = response.PalletInfos;
			AssertEquals("Response has 20 Pallets", 20, palletsOnReponse.Length);
			AssertContainsExactElementsInAnyOrder("Response has correct PalletIDs", palletIDs, palletsOnReponse.Select(l => l.PalletID));
		}
	}
}
