using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class BeginRFTransferTaskTest : WhsSecureServiceTestCase
	{
		public void TestBeginRFTransferTask_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(Guid.NewGuid(), isReplenishmentTask: true);

			AssertBusinessValidationError(webService, "Task could not be found.", response);
		}

		public void TestBeginRFTransferTask_StandardTransfer_WrongTaskType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: true);

			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFReplenishmentTask_Replenishment_WrongTaskType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);

			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFTransferTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Created = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine_HeldForTransfer = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_HeldForTransfer.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation(); // to commit inventory

			var transferLine_Finalised = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_Finalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine_Finalised);

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			AssertEquals(0, Helper.FindLogs(transfer.Logs, Events.ServiceCommenced).Length);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);

			AssertEquals("System should find transfer by task.", transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals("Only lines that need Allocation should be sent in the response.", 1, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine_Created.PK.ToGuid()));
			AssertEquals(1, Helper.FindLogs(transfer.Logs, Events.ServiceCommenced).Length);

			var newFactory = new BusinessObjectFactory();
			var transferLineInOtherFactory = newFactory.Load<WhsTransferLine>(transferLine_Created.PK);
			AssertEquals(staff.GS_Code, transferLineInOtherFactory.WE_GS_NKPutawayBy);
			AssertEquals(staff.GS_Code, transferLineInOtherFactory.GS_NKPickedBy);
		}

		public void TestBeginRFTransferTask_SomeLinesAssignedToOtherProcessTasks()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff1 = Helper.CreateGlbStaff("S2", "S2");
			var staff2 = Helper.CreateGlbStaff("S3", "S3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task1 = Helper.CreateProcessTaskForTransfer(transfer, staff1);
			var task2 = Helper.CreateProcessTaskForTransfer(transfer, staff2);
			transferLine1.WE_P9_Task = task1.PK;
			transferLine2.WE_P9_Task = task2.PK;
			Helper.Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals(0, Helper.FindLogs(transfer.Logs, Events.ServiceCommenced).Length);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.BeginRFTransferTask(task1.PK.ToGuid(), isReplenishmentTask: false);

			AssertEquals("System should find transfer by task.", transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals("Only lines assigned to the task should be sent in the response.", 1, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
		}

		public void TestBeginRFTransferTask_ShowStockOnHandWarningOnPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
				AssertEquals(transfer.WD_DocketID, response.Docket.DocketID);
				AssertEquals(true, response.ShowStockOnHandWarningOnPutaway);
			}

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
				AssertEquals(transfer.WD_DocketID, response.Docket.DocketID);
				AssertEquals(false, response.ShowStockOnHandWarningOnPutaway);
			}
		}

		public void TestBeginRFTransferTask_InvalidProcessFlowType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Created = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine_HeldForTransfer = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_HeldForTransfer.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation(); // to commit inventory

			var transferLine_Finalised = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_Finalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine_Finalised);

			var task = Helper.Factory.New<WhsTransferProcessTasks>();
			task.P9_ParentID = transfer.PK;
			task.P9_ParentTableCode = "WD";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_FormFlowType = "WRP";

			foreach (var line in transfer.Lines)
			{
				line.WE_P9_Task = task.PK;
			}

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFTransferTask_DockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition: Transfer is created.", 1, pick.Transfers.Count);

			var transfer = pick.Transfers.Single();
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFTransferTask_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPutawayTransfer = true;

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFTransferTask_SetsTaskToPlay()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestBeginRFTransferTask_FinalisedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Finalised = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_Finalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine_Finalised);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			AssertEquals(0, Helper.FindLogs(transfer.Logs, Events.ServiceCommenced).Length);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertBusinessValidationError(webService, "Transfer is already finalized or canceled.", response);
		}

		public void TestBeginRFTransferTask_NothingToTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Finalised = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_Finalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine_Finalised);

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertBusinessValidationError(webService, "Un-finalized transfer could not be found for this task.", response);
		}

		public void TestBeginRFTransferTask_InPickSequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["A-1", "A-2", "A-3", "A-4", "A-5"], response);
		}

		public void TestBeginRFTransferTask_InModifiedPickSequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].WLV_PickPathSequence = 3;
			sourceLocations[1].WLV_PickPathSequence = 5;
			sourceLocations[2].WLV_PickPathSequence = 1;
			sourceLocations[3].WLV_PickPathSequence = 4;
			sourceLocations[4].WLV_PickPathSequence = 2;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["A-3", "A-5", "A-1", "A-4", "A-2"], response);
		}

		public void TestBeginRFTransferTask_InModifiedPickSequence_IncludingSequenceNumberZero()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].WLV_PickPathSequence = 3;
			sourceLocations[1].WLV_PickPathSequence = 0;
			sourceLocations[2].WLV_PickPathSequence = 1;
			sourceLocations[3].WLV_PickPathSequence = 4;
			sourceLocations[4].WLV_PickPathSequence = 2;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["A-3", "A-5", "A-1", "A-4", "A-2"], response);
		}

		public void TestBeginRFTransferTask_InModifiedRowPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 5);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "D", 5);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "E", 5);

			Helper.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("B-2"),
				data.Whs1.FindLocation("C-3"),
				data.Whs1.FindLocation("D-4"),
				data.Whs1.FindLocation("E-5"),
			};

			sourceLocations[0].Row.WR_PickPathSequence = 5;
			sourceLocations[1].Row.WR_PickPathSequence = 4;
			sourceLocations[2].Row.WR_PickPathSequence = 1;
			sourceLocations[3].Row.WR_PickPathSequence = 2;
			sourceLocations[4].Row.WR_PickPathSequence = 3;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["C-3", "D-4", "E-5", "B-2", "A-1"], response);
		}

		public void TestBeginRFTransferTask_InModifiedRowPathSequence_AllSameSequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);

			Helper.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("B-1"),
				data.Whs1.FindLocation("B-2"),
				data.Whs1.FindLocation("B-3"),
				data.Whs1.FindLocation("B-4"),
				data.Whs1.FindLocation("B-5"),
			};

			sourceLocations[0].Row.WR_PickPathSequence = 5;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["B-1", "B-2", "B-3", "B-4", "B-5"], response);
		}

		public void TestBeginRFTransferTask_InPartiallyModifiedRowPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 5);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "D", 5);

			Helper.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("B-1"),
				data.Whs1.FindLocation("C-2"),
				data.Whs1.FindLocation("D-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].Row.WR_PickPathSequence = 5;
			sourceLocations[1].Row.WR_PickPathSequence = 4;
			sourceLocations[2].Row.WR_PickPathSequence = 1;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["D-3", "C-2", "B-1", "A-4", "A-5",], response);
		}

		public void TestBeginRFTransferTask_InModifiedPickSequence_AndModifiedRowPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);

			Helper.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("B-2"),
				data.Whs1.FindLocation("B-3"),
				data.Whs1.FindLocation("B-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].WLV_PickPathSequence = 3;
			sourceLocations[1].WLV_PickPathSequence = 5;
			sourceLocations[2].WLV_PickPathSequence = 1;
			sourceLocations[3].WLV_PickPathSequence = 4;
			sourceLocations[4].WLV_PickPathSequence = 2;

			sourceLocations[0].Row.WR_PickPathSequence = 2;
			sourceLocations[1].Row.WR_PickPathSequence = 3;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["A-5", "A-1", "B-3", "B-4", "B-2"], response);
		}

		public void TestBeginRFTransferTask_InPickSequence_AndPickMethod()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			// Setup Registry
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod = pickMethods.AddNew();
			pickMethod.Code = "TRB";
			pickMethod.Bool = true;
			pickMethod.Description = (NoResString)"Tractor Beam";

			sourceLocations[0].WLV_PickMethod = "TRB";
			sourceLocations[2].WLV_PickMethod = "TRB";
			sourceLocations[3].WLV_PickMethod = "TRB";

			var destinationLocation = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["A-2", "A-5", "A-1", "A-3", "A-4"], response);
		}

		public void TestBeginRFTransferTask_InPickSequence_WithDuplicatedLocations()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3")
			};

			var destinationLocation = data.Whs1.FindLocation("A-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(["A-1", "A-1", "A-2", "A-2", "A-3"], response);
		}

		public void TestBeginRFTransferTask_InPickSequence_WithSomeEmptyDestinationLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
			};

			var destinationLocation = data.Whs1.FindLocation("A-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), "");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), "");
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(new[] { "A-1", "A-2", "A-3" }, response);
		}

		public void TestBeginRFTransferTask_InPickSequence_DoesNotUseDestinationLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 6, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
			};

			var destinationLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
				data.Whs1.FindLocation("A-6"),
			};

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocations[0].ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocations[1].ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocations[2].ToLocationString());
			transfer.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertTransferResponseContainsSortedLines(new[] { "A-1", "A-2", "A-3" }, response);
		}

		public void TestBeginRFTransferTask_PickLineIsPicking()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 40m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transfer.RunPreSaveValidation(); // to commit inventory

			transferLine2.GS_NKPickedBy = staff.GS_Code;
			transferLine2.WE_GS_NKPutawayBy = staff.GS_Code;
			transferLine2.PickedTime = ZDateTimeOffset.Now;

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			AssertEquals("Precondition: transfer line is not Picked", false, transferLine1.IsPicked);
			AssertEquals("Precondition: transfer line has matching lines", true, transferLine1.MatchingLines.Any());
			AssertEquals("Precondition: transfer line is Picked", true, transferLine2.IsPicked);
			AssertEquals("Precondition: transfer line has matching lines", false, transferLine2.MatchingLines.Any());
			AssertEquals("Precondition: transfer line is not Picked", false, transferLine3.IsPicked);
			AssertEquals("Precondition: transfer line has no matching lines", false, transferLine3.MatchingLines.Any());

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertEquals("System should find only lines not allocated to anyone or lines allocated to current user.", 2, response.Docket.Lines.Count);
			AssertNotNull("Transfer line should in response", response.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid())));
			AssertNotNull("Transfer line should in response", response.Docket.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid())));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferLine1New = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			AssertEquals("Transfer Line has pick line that is picking", true, transferLine1New.PickLines.All(pl => pl.WZ_IsPicking));
			AssertEquals("Matching line in transfer Line has pick line that is picking", true, transferLine1New.MatchingLines.All(ml => ml.PickLines.All(pl => pl.WZ_IsPicking)));

			var transferLine2New = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals("Transfer Line has no pick line that is picking", false, transferLine2New.PickLines.All(pl => pl.WZ_IsPicking));

			var transferLine3New = newFactory.Load<WhsTransferLine>(transferLine3.PK);
			AssertEquals("Transfer Line has pick line that is picking", true, transferLine3New.PickLines.All(pl => pl.WZ_IsPicking));
		}

		public void AssertTransferResponseContainsSortedLines(string[] expectedLocationSequence, WhsDocketWebServiceResponse response)
		{
			var transferLines = response.Docket.Lines;
			AssertArrayEqualsByElements(expectedLocationSequence, transferLines.Select(l => l.Location).ToArray());
		}

		public void TestBeginRFTransferTask_InterWhs_Source() => TestBeginRFTransferTask_InterWhs(isSource: true);
		public void TestBeginRFTransferTask_InterWhs_Dest() => TestBeginRFTransferTask_InterWhs(isSource: false);

		public void TestBeginRFTransferTask_InterWhs(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m, null, "PLT");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 5m, "A", "PLT", warehouse2.PK, "A", "PLT", ZDateTimeOffset.Today);
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertEquals("System should find transfer by task.", transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals("Only lines that need Allocation should be sent in the response.", 1, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine.PK.ToGuid()));
		}

		public void TestBeginRFTransferTask_InterWhsChild_Source() => TestBeginRFTransferTask_InterWhsChild(isSource: true);
		public void TestBeginRFTransferTask_InterWhsChild_Dest() => TestBeginRFTransferTask_InterWhsChild(isSource: false);

		void TestBeginRFTransferTask_InterWhsChild(bool isSource)
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
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

			var task = Helper.CreateProcessTaskForTransfer(childTransfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: false);
			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFTransferTask_ReplenishmentTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: true);

			AssertEquals("System should find transfer by task.", transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals("Only lines that need Allocation should be sent in the response.", 1, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine.PK.ToGuid()));
			AssertEquals(1, Helper.FindLogs(transfer.Logs, Events.ServiceCommenced).Length);

			var newFactory = new BusinessObjectFactory();
			var transferLineInOtherFactory = newFactory.Load<WhsTransferLine>(transferLine.PK);
			AssertEquals(staff.GS_Code, transferLineInOtherFactory.WE_GS_NKPutawayBy);
			AssertEquals(staff.GS_Code, transferLineInOtherFactory.GS_NKPickedBy);
		}

		public void TestBeginRFTransferTask_ReplenishmentTask_InvalidProcessFlowType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.Factory.New<WhsTransferProcessTasks>();
			task.P9_ParentID = transfer.PK;
			task.P9_ParentTableCode = "WD";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_FormFlowType = "WTR";

			foreach (var line in transfer.Lines)
			{
				line.WE_P9_Task = task.PK;
			}

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: true);
			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFTransferTask_ReplenishmentTask_SetsTaskToPlay()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: true);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestBeginRFTransferTask_ReplenishmentTask_FinalisedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPickFaceReplenishment = true;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			task.P9_Description = "Test";
			task.P9_Type = "UDF";
			Helper.Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: true);
			AssertBusinessValidationError(webService, "Transfer is already finalized or canceled.", response);
		}

		public void TestBeginRFTransferTask_ReplenishmentTask_NothingToTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPickFaceReplenishment = true;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: true);
			AssertBusinessValidationError(webService, "Un-finalized replenishment transfer could not be found for this task.", response);
		}

		public void TestBeginRFTransferTask_ReplenishmentTask_DoesNotShowStockOnHandWarning()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.BeginRFTransferTask(task.PK.ToGuid(), isReplenishmentTask: true);
				AssertEquals(transfer.WD_DocketID, response.Docket.DocketID);
				AssertEquals(false, response.ShowStockOnHandWarningOnPutaway);
			}
		}
	}
}
