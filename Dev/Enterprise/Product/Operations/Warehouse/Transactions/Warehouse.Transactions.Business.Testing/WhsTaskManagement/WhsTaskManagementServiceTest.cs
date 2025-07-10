using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using GlowIndexQueryService.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsTaskManagementServiceTest : WhsTestCaseWithFactory
	{
		#region TestChangeTaskPlanningStatus

		#region TestChangeTaskPlanningStatus_TypeAndPK_DifferentJobTypeAndStatus

		public void TestChangeTaskPlanningStatus_TypeAndPK_Receive()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			Factory.Save();

			var whs = Helper.CreateWarehouse("WH1", "A");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var org = Helper.CreateClient("CLI");
			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			Factory.Save();

			TestChangeTaskPlanningStatus_TypeAndPKCore(receive.PK, TaskManagementJobType.WhsReceive, receive);
		}

		public void TestChangeTaskPlanningStatus_TypeAndPK_CycleCount()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			Factory.Save();

			var whs = Helper.CreateWarehouse("WH1", "A", 1, 1);
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var cycleCount = Helper.CreateWhsCycleCountLocation(whs.DefaultLocation, CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			Factory.Save();

			TestChangeTaskPlanningStatus_TypeAndPKCore(cycleCount.PK, TaskManagementJobType.WhsCycleCountLocation, cycleCount);
		}

		public void TestChangeTaskPlanningStatus_TypeAndPK_Pick()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(false, false, order);
			Factory.Save();

			TestChangeTaskPlanningStatus_TypeAndPKCore(pick.PK, TaskManagementJobType.WhsPick, pick);
		}

		public void TestChangeTaskPlanningStatus_TypeAndPK_Transfer()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			TestChangeTaskPlanningStatus_TypeAndPKCore(transfer.PK, TaskManagementJobType.WhsTransfer, transfer);
		}

		public void TestChangeTaskPlanningStatus_TypeAndPK_Load()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			Factory.Save();

			var whs = Helper.CreateWarehouse("WH1", "A");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var org = Helper.CreateClient("CLI");
			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation);
			Factory.Save();

			TestChangeTaskPlanningStatus_TypeAndPKCore(load.PK, TaskManagementJobType.WhsLoad, load);
		}

		void TestChangeTaskPlanningStatus_TypeAndPKCore(ZGuid jobPK, string jobType, ITaskPlanningJob job)
		{
			TestChangeStatus(true, string.Empty, TaskPlanningStatus.Codes.Ready, string.Empty);
			TestChangeStatus(true, TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.Ready, string.Empty);
			TestChangeStatus(true, TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.Ready, "Cannot change Task Planning Status to Ready as the status is currently 'Ready For Planning'");
			TestChangeStatus(true, TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.Planned, "Cannot change Task Planning Status to Ready as the status is currently 'Planned'");
			TestChangeStatus(true, TaskPlanningStatus.Codes.Error, TaskPlanningStatus.Codes.Ready, string.Empty);

			TestChangeStatus(false, string.Empty, string.Empty, "Cannot change Task Planning Status to Not Ready as it is already Unplanned");
			TestChangeStatus(false, TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.NotReady, "Cannot change Task Planning Status to Not Ready as it is already Unplanned");
			TestChangeStatus(false, TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.NotReady, string.Empty);
			TestChangeStatus(false, TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.NotReady, string.Empty);
			TestChangeStatus(false, TaskPlanningStatus.Codes.Error, TaskPlanningStatus.Codes.Error, "Cannot change Task Planning Status to Not Ready as it is already Unplanned");

			void TestChangeStatus(bool changeToReady, string fromStatus, string expectedStatus, string expectedMessage)
			{
				job.TaskPlanningStatus = fromStatus;
				Factory.Save();

				var message = string.Empty;
				var service = new WhsTaskManagementService();
				message = service.ChangeTaskPlanningStatus(jobPK, jobType, changeToReady);
				CombineAssertions(() =>
				{
					AssertEquals(expectedStatus, job.TaskPlanningStatus);
					AssertEquals(expectedMessage, message);
				});
			}
		}

		#endregion

		#region TestChangeTaskPlanningStatus_TypeAndPK_InvalidPK

		public void TestChangeTaskPlanningStatus_TypeAndPK_InvalidPK()
		{
			var service = new WhsTaskManagementService();
			var message = service.ChangeTaskPlanningStatus(ZGuid.Empty, TaskManagementJobType.WhsReceive, true);
			AssertEquals("PK '00000000-0000-0000-0000-000000000000', Job Type 'WhsReceive' is not found", message);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_TypeAndPK_TaskManagementNotEnabled

		public void TestChangeTaskPlanningStatus_TypeAndPK_TaskManagementNotEnabled()
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			var org = Helper.CreateClient("CLI");
			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			Factory.Save();

			var service = new WhsTaskManagementService();
			var message = service.ChangeTaskPlanningStatus(receive.PK, TaskManagementJobType.WhsReceive, true);
			AssertEquals("Task management is not enabled for the Warehouse Receipt", message);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_ITaskPlanningJob_DifferentStatus

		public void TestChangeTaskPlanningStatus_ITaskPlanningJob_Empty()
		{
			TestChangeTaskPlanningStatus_ITaskPlanningJobCore(string.Empty);
		}

		public void TestChangeTaskPlanningStatus_ITaskPlanningJob_NotReady()
		{
			TestChangeTaskPlanningStatus_ITaskPlanningJobCore(TaskPlanningStatus.Codes.NotReady);
		}

		public void TestChangeTaskPlanningStatus_ITaskPlanningJob_Ready()
		{
			TestChangeTaskPlanningStatus_ITaskPlanningJobCore(TaskPlanningStatus.Codes.Ready);
		}

		public void TestChangeTaskPlanningStatus_ITaskPlanningJob_Planned()
		{
			TestChangeTaskPlanningStatus_ITaskPlanningJobCore(TaskPlanningStatus.Codes.Planned);
		}

		void TestChangeTaskPlanningStatus_ITaskPlanningJobCore(string status)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.WarehousePK).Returns(data.Whs1.PK);
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.TaskPlanningStatus).Returns(status);
			mock.SetupSet(s => s.TaskPlanningStatus = It.IsAny<ZString>())
				.Callback((ZString value) => mock.Setup(s => s.TaskPlanningStatus).Returns(value));
			mock.Setup(s => s.IsInDatabase).Returns(true);
			mock.Setup(s => s.HasChanges).Returns(false);
			mock.Setup(s => s.IsFinalisedOrCancelled).Returns(false);

			var service = new WhsTaskManagementService();
			var message = service.ChangeTaskPlanningStatus(mock.Object);
			AssertNullOrEmpty(message);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_ITaskPlanningJob_TaskManagementNotEnabled

		public void TestChangeTaskPlanningStatus_ITaskPlanningJob_TaskManagementNotEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.HumanReadableNameWithoutID).Returns("mock");
			AssertEquals(false, mock.Object.IsTaskManagementEnabled());

			var service = new WhsTaskManagementService();
			var message = service.ChangeTaskPlanningStatus(mock.Object);
			AssertEquals("Task management is not enabled for the mock", message);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady

		#region TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsReceive

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsReceive()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			var whs = Helper.CreateWarehouse("WH1", "A");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var org = Helper.CreateClient("CLI");
			var receive = Helper.CreateWhsReceive(org, whs, "REC");
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.UnloadJob;
			Factory.Save();

			TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(receive.PK, TaskManagementJobType.WhsReceive, receive);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsTransfer

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_TransferJob()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			Factory.Save();

			TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(transfer.PK, TaskManagementJobType.WhsTransfer, transfer);
		}

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_PutawayTransfer()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			transfer.WD_IsPutawayTransfer = true;
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.PutawayJob;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var message = service.ChangeTaskPlanningStatus(transfer.PK, TaskManagementJobType.WhsTransfer, false);

			AssertEquals("PutawayTransfer do not support Task Planning Status", "The job does not support Task Planning Status.", message);
		}

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_ReplenishmentTransfer()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			transfer.WD_IsPickFaceReplenishment = true;
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.ReplenishmentJob;
			Factory.Save();

			TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(transfer.PK, TaskManagementJobType.WhsTransfer, transfer);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsPick

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_PickByLabelJob()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.PickByLabelJob;
			Factory.Save();

			TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(pick.PK, TaskManagementJobType.WhsPick, pick);
		}

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_DirectedPackingJob()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.DirectedPackingJob;
			Factory.Save();

			TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(pick.PK, TaskManagementJobType.WhsPick, pick);
		}

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_PickJob()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.PickJob;
			Factory.Save();

			TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(pick.PK, TaskManagementJobType.WhsPick, pick);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsLoad

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsLoad()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			var whs = Helper.CreateWarehouse("WH1", "A");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var org = Helper.CreateClient("CLI");
			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, jobID: "LOAD001");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForLoad(load, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.LoadJob;
			Factory.Save();

			TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(load.PK, TaskManagementJobType.WhsLoad, load);
		}

		#endregion

		#region TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsCycleCount

		public void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReady_WhsCycleCount()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			var whs = Helper.CreateWarehouse("WH1", "A");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			var org = Helper.CreateClient("CLI");
			Factory.Save();

			var wave = Factory.New<WhsCycleCountWave>();
			var location = whs.FindLocation("A");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);

			cycleCount.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var header = Factory.New<IProcessHeader>();
			header.FH_WorkflowType = "ZZZ";
			var task = Helper.CreateProcessTaskForCycleCountWave(wave, staff);
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.CycleCountJob;
			cycleCount.WCL_P9_Task = task.PK;
			Factory.Save();

			var job = (ITaskPlanningJobWithExternalTasks)cycleCount;

			AssertEquals("Process Task should created for this job.", 1, job.GetRelatedProcessTasks().Length);

			var service = new WhsTaskManagementService();
			var message = service.ChangeTaskPlanningStatus(cycleCount.PK, TaskManagementJobType.WhsCycleCountLocation, false);

			AssertEquals(TaskPlanningStatus.Codes.NotReady, job.TaskPlanningStatus);
			AssertEquals("Should delete related process task(s) when Task Planning Status set to Not Ready.", 0, job.GetRelatedProcessTasks().Length);
			AssertEquals(string.Empty, message);
		}

		#endregion

		void TestChangeTaskPlanningStatus_DeleteRelatedProcessTasksAfterSetToNotReadyCore(ZGuid jobPK, string jobType, ITaskPlanningJob job)
		{
			var task = job.GetRelatedProcessTasksOffJob().Single();
			var header = job.Factory.Load<IProcessHeader>(task.P9_FH_ProcessHeader);
			AssertEquals(false, header.IsDeleted);

			var service = new WhsTaskManagementService();
			var message = service.ChangeTaskPlanningStatus(jobPK, jobType, false);

			AssertEquals(TaskPlanningStatus.Codes.NotReady, job.TaskPlanningStatus);
			AssertEquals("Should delete related process task(s) when Task Planning Status set to Not Ready.", 0, job.GetRelatedProcessTasksOffJob().Length);
			AssertEquals("Should delete process header(s) for the deleted Process task(s)", true, header.IsDeleted);
			AssertEquals(string.Empty, message);
		}

		#endregion

		#endregion

		#region TestBatchChangeTaskPlanningStatus_CycleCount

		public void TestBatchChangeTaskPlanningStatus_CycleCount_ChangToReady()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-1"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount1.WCL_TaskPlanningStatus = string.Empty;

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-2"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;

			var cycleCount3 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-3"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount3.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var cycleCount4 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-4"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount4.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var cycleCount5 = Helper.CreateWhsCycleCountLocation(whs2.FindLocation("B-1"), CycleCountGranularity.Codes.PalletIDOnly);
			Factory.Save();

			var service = new WhsTaskManagementService();
			ZGuid[] jobPKs = [cycleCount1.PK, cycleCount2.PK, cycleCount3.PK, cycleCount4.PK, cycleCount5.PK];
			var message = service.BatchChangeTaskPlanningStatus(jobPKs, TaskManagementJobType.WhsCycleCountLocation, true);
			AssertEquals(@"A-1 - Cycle Count has been updated
A-2 - Cycle Count has been updated
A-3 - Cannot change Task Planning Status to Ready as the status is currently 'Ready For Planning'
A-4 - Cannot change Task Planning Status to Ready as the status is currently 'Planned'
B-1 - Task management is not enabled for the Cycle Count
", message);
		}

		public void TestBatchChangeTaskPlanningStatus_CycleCount_ChangToNotReady()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-1"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount1.WCL_TaskPlanningStatus = string.Empty;

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-2"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;

			var cycleCount3 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-3"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount3.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var cycleCount4 = Helper.CreateWhsCycleCountLocation(whs1.FindLocation("A-4"), CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount4.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var cycleCount5 = Helper.CreateWhsCycleCountLocation(whs2.FindLocation("B-1"), CycleCountGranularity.Codes.PalletIDOnly);
			Factory.Save();

			var service = new WhsTaskManagementService();
			ZGuid[] jobPKs = [cycleCount1.PK, cycleCount2.PK, cycleCount3.PK, cycleCount4.PK, cycleCount5.PK];
			var message = service.BatchChangeTaskPlanningStatus(jobPKs, TaskManagementJobType.WhsCycleCountLocation, false);
			AssertEquals(@"A-1 - Cannot change Task Planning Status to Not Ready as it is already Unplanned
A-2 - Cannot change Task Planning Status to Not Ready as it is already Unplanned
A-3 - Cycle Count has been updated
A-4 - Cycle Count has been updated
B-1 - Task management is not enabled for the Cycle Count
", message);
		}

		#endregion

		#region TestBatchChangeTaskPlanningStatus_Load

		public void TestBatchChangeTaskPlanningStatus_Load_ChangToReady()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var org = Helper.CreateClient("CLI");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var load1 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-1"), "WL00000001");
			load1.WLO_TaskPlanningStatus = string.Empty;

			var load2 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-2"), "WL00000002");
			load2.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;

			var load3 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-3"), "WL00000003");
			load3.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var load4 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-4"), "WL00000004");
			load4.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var load5 = Helper.CreateWhsLoad(org, whs2.FindLocation("B-1"), "WL00000005");
			Factory.Save();

			var service = new WhsTaskManagementService();
			ZGuid[] jobPKs = [load1.PK, load2.PK, load3.PK, load4.PK, load5.PK];
			var message = service.BatchChangeTaskPlanningStatus(jobPKs, TaskManagementJobType.WhsLoad, true);
			AssertEquals(@"WL00000001 - Load Planning has been updated
WL00000002 - Load Planning has been updated
WL00000003 - Cannot change Task Planning Status to Ready as the status is currently 'Ready For Planning'
WL00000004 - Cannot change Task Planning Status to Ready as the status is currently 'Planned'
WL00000005 - Task management is not enabled for the Load Planning
", message);
		}

		public void TestBatchChangeTaskPlanningStatus_Load_ChangToNotReady()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var org = Helper.CreateClient("CLI");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var load1 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-1"), "WL00000001");
			load1.WLO_TaskPlanningStatus = string.Empty;

			var load2 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-2"), "WL00000002");
			load2.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;

			var load3 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-3"), "WL00000003");
			load3.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var load4 = Helper.CreateWhsLoad(org, whs1.FindLocation("A-4"), "WL00000004");
			load4.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var load5 = Helper.CreateWhsLoad(org, whs2.FindLocation("B-1"), "WL00000005");
			Factory.Save();

			var service = new WhsTaskManagementService();
			ZGuid[] jobPKs = [load1.PK, load2.PK, load3.PK, load4.PK, load5.PK];
			var message = service.BatchChangeTaskPlanningStatus(jobPKs, TaskManagementJobType.WhsLoad, false);
			AssertEquals(@"WL00000001 - Cannot change Task Planning Status to Not Ready as it is already Unplanned
WL00000002 - Cannot change Task Planning Status to Not Ready as it is already Unplanned
WL00000003 - Load Planning has been updated
WL00000004 - Load Planning has been updated
WL00000005 - Task management is not enabled for the Load Planning
", message);
		}

		#endregion

		#region TestGetNextTask

		#region TestGetNextTask_Success

		public void TestGetNextTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			var searchResult = new GlowIndexQueryResult(task.PK.ToString(), WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType);
			luceneResultCollection.Results = new[] { searchResult };
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

				AssertEquals(task.PK.ToGuid(), result.TaskPK);
				Assert(string.IsNullOrEmpty(result.ErrorMessage));
			}
		}

		public void TestGetNextTask_PrioritizesInProgressTasks()
		{
			// Test relevance can be revisited when PRE integrated
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task1 = Helper.CreateProcessTaskForReceive(receive1, staff);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var task2 = Helper.CreateProcessTaskForReceive(receive2, staff);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			var searchResult = new GlowIndexQueryResult(task2.PK.ToString(), WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType);
			luceneResultCollection.Results = new[] { searchResult };
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

				AssertEquals(task2.PK.ToGuid(), result.TaskPK);
				Assert(string.IsNullOrEmpty(result.ErrorMessage));
			}
		}

		public void TestGetNextTask_PrioritizesTasksAssignedToUser()
		{
			// Test relevance can be revisited when PRE integrated
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task1 = Helper.CreateProcessTaskForReceive(receive1, staff);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var task2 = Helper.CreateProcessTaskForReceive(receive2, staff);

			Factory.Save();

			task1.P9_GS_NKAssignedStaffMember = string.Empty;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			var searchResult = new GlowIndexQueryResult(task2.PK.ToString(), WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType);
			luceneResultCollection.Results = new[] { searchResult };
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					It.IsAny<string>(),
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

				AssertEquals(task2.PK.ToGuid(), result.TaskPK);
				Assert(string.IsNullOrEmpty(result.ErrorMessage));
			}
		}

		public void TestGetNextTask_PrioritizesEarliestTasks()
		{
			// Test relevance can be revisited when PRE integrated
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			var now = DateTime.UtcNow;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task1 = Helper.CreateProcessTaskForReceive(receive1, staff);
			task1.P9_SystemCreateTimeUtc = now.AddMinutes(-1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var task2 = Helper.CreateProcessTaskForReceive(receive2, staff);
			task2.P9_SystemCreateTimeUtc = now;

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			var searchResult = new GlowIndexQueryResult(task1.PK.ToString(), WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType);
			luceneResultCollection.Results = new[] { searchResult };
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

				AssertEquals(task1.PK.ToGuid(), result.TaskPK);
				Assert(string.IsNullOrEmpty(result.ErrorMessage));
			}
		}

		#endregion

		#region TestGetNextTask_InvalidArguments

		public void TestGetNextTask_NullFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var service = new WhsTaskManagementService();
			AssertExceptionThrown<ArgumentNullException>(() => service.GetNextTask(null, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>()));
		}

		public void TestGetNextTask_InvalidStaffPK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.GetNextTask(Factory, string.Empty, Guid.NewGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

			AssertEquals(Guid.Empty, result.TaskPK);
			AssertEquals("Staff could not be found. Please try again.", result.ErrorMessage);
		}

		public void TestGetNextTask_InvalidWarehousePK()
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), Guid.NewGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

			AssertEquals(Guid.Empty, result.TaskPK);
			AssertEquals("Warehouse could not be found. Please check branch details and try again.", result.ErrorMessage);
		}

		public void TestGetNextTask_WarehouseDisabledTaskManagement()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

			AssertEquals(Guid.Empty, result.TaskPK);
			Assert(string.IsNullOrEmpty(result.TaskFormFlowType));
			AssertEquals("This warehouse does not support Task Management. Please check you are logged in with the correct branch.", result.ErrorMessage);
		}

		public void TestGetNextTask_NullFormFlowType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			var searchResult = new GlowIndexQueryResult(task.PK.ToString(), WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType);
			luceneResultCollection.Results = new[] { searchResult };
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), null, string.Empty, Array.Empty<Guid>());

				AssertEquals(task.PK.ToGuid(), result.TaskPK);
				Assert(string.IsNullOrEmpty(result.ErrorMessage));
			}
		}

		public void TestGetNextTask_NullTasksToIgnore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			var searchResult = new GlowIndexQueryResult(task.PK.ToString(), WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType);
			luceneResultCollection.Results = new[] { searchResult };
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, null);

				AssertEquals(task.PK.ToGuid(), result.TaskPK);
				Assert(string.IsNullOrEmpty(result.ErrorMessage));
			}
		}

		#endregion

		#region TestGetNextTask_NoTasksFound

		public void TestGetNextTask_NoTasksFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			luceneResultCollection.Results = Array.Empty<GlowIndexQueryResult>();
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

				AssertEquals(Guid.Empty, result.TaskPK);
				AssertEquals("No available task could be found. Please try again.", result.ErrorMessage);
			}
		}

		public void TestGetNextTask_NoTasksFound_DueToTasksToIgnore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			luceneResultCollection.Results = Array.Empty<GlowIndexQueryResult>();
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					new[] { task.PK.ToGuid() }))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, [task.PK.ToGuid()]);

				AssertEquals(Guid.Empty, result.TaskPK);
				AssertEquals("No available task could be found. Please try again.", result.ErrorMessage);
			}
		}

		#endregion

		#region TestGetNextTask_ByReference

		public void TestGetNextTask_ByReference()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var task1 = Helper.CreateProcessTaskForReceive(receive1, staff);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var task2 = Helper.CreateProcessTaskForReceive(receive2, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			var searchResult = new GlowIndexQueryResult(task2.PK.ToString(), WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType);
			luceneResultCollection.Results = new[] { searchResult };
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					task2.P9_TaskID,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, task2.P9_TaskID, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), string.Empty, string.Empty, Array.Empty<Guid>());

				AssertEquals(task2.PK.ToGuid(), result.TaskPK);
				Assert(string.IsNullOrEmpty(result.ErrorMessage));
			}
		}

		#endregion

		#region TestGetNextTask_SpecificFormFlow

		public void TestGetNextTask_SpecificFormFlow_Unload()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.UnloadJob);
		}

		public void TestGetNextTask_SpecificFormFlow_Putaway()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.PutawayJob);
		}

		public void TestGetNextTask_SpecificFormFlow_Pick()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.PickJob);
		}

		public void TestGetNextTask_SpecificFormFlow_DirectedPacking()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.DirectedPackingJob);
		}

		public void TestGetNextTask_SpecificFormFlow_Transfer()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.TransferJob);
		}

		public void TestGetNextTask_SpecificFormFlow_Replenishment()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.ReplenishmentJob);
		}

		public void TestGetNextTask_SpecificFormFlow_Load()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.LoadJob);
		}

		public void TestGetNextTask_SpecificFormFlow_CycleCount()
		{
			TestGetNextTask_SpecificFormFlowCore(WarehouseTaskFormFlowTypes.CycleCountJob);
		}

		void TestGetNextTask_SpecificFormFlowCore(string formFlowType)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var luceneTaskSearchMock = new Mock<IWhsLuceneTaskSearch>();
			var luceneResultCollection = new GlowIndexQueryResultCollection();
			luceneResultCollection.Results = Array.Empty<GlowIndexQueryResult>();
			luceneTaskSearchMock
				.Setup(x => x.QueryLuceneForTasks(
					string.Empty,
					It.Is<GlbStaff>(s => s.PK == staff.PK),
					It.Is<WhsRFRegistry>(r => r.WRR_GS_NKAssignedTo == staff.GS_Code && r.WRR_WW_Whs == data.Whs1.PK),
					It.Is<WhsWarehouse>(w => w.PK == data.Whs1.PK),
					new[] { formFlowType },
					Array.Empty<Guid>()))
				.Returns(luceneResultCollection);

			using (ObjectFactory.Substitute(luceneTaskSearchMock.Object))
			{
				var service = new WhsTaskManagementService();
				var result = service.GetNextTask(Factory, string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), formFlowType, string.Empty, Array.Empty<Guid>());

				AssertEquals(Guid.Empty, result.TaskPK);
				AssertEquals("No available task could be found. Please try again.", result.ErrorMessage);
			}
		}

		#endregion

		#endregion

		#region TestSetTaskToPlayIfValid

		public void TestSetTaskToPlayIfValid_ReceiveJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			TestSetTaskToPlayIfValidCore(task, WarehouseTaskFormFlowTypes.UnloadJob, staff);
		}

		public void TestSetTaskToPlayIfValid_PutawayTransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

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
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			TestSetTaskToPlayIfValidCore(task, WarehouseTaskFormFlowTypes.PutawayJob, staff);
		}

		public void TestSetTaskToPlayIfValid_PickJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			TestSetTaskToPlayIfValidCore(task, WarehouseTaskFormFlowTypes.PickJob, staff);
		}

		public void TestSetTaskToPlayIfValid_PickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			TestSetTaskToPlayIfValidCore(task, WarehouseTaskFormFlowTypes.PickByLabelJob, staff);
		}

		public void TestSetTaskToPlayIfValid_DirectedPackingJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			TestSetTaskToPlayIfValidCore(task, WarehouseTaskFormFlowTypes.DirectedPackingJob, staff);
		}

		public void TestSetTaskToPlayIfValid_TransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			TestSetTaskToPlayIfValidCore(task, WarehouseTaskFormFlowTypes.TransferJob, staff);
		}

		public void TestSetTaskToPlayIfValid_ReplenishmentJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			TestSetTaskToPlayIfValidCore(task, WarehouseTaskFormFlowTypes.ReplenishmentJob, staff);
		}

		void TestSetTaskToPlayIfValidCore(ProcessTask task, string taskType, GlbStaff staff)
		{
			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, taskType, staff.GS_Code);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.Success, result);
			AssertEquals("Valid task should set actual date success.", true, task.P9_ActualDateUtc.IsValid);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_AlreadyWorking_ReceiveJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToPlayIfValid_AlreadyWorkingCore(task, WarehouseTaskFormFlowTypes.UnloadJob, staff);
		}

		public void TestSetTaskToPlayIfValid_AlreadyWorking_PutawayTransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

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
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToPlayIfValid_AlreadyWorkingCore(task, WarehouseTaskFormFlowTypes.PutawayJob, staff);
		}

		public void TestSetTaskToPlayIfValid_AlreadyWorking_PickJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToPlayIfValid_AlreadyWorkingCore(task, WarehouseTaskFormFlowTypes.PickJob, staff);
		}

		public void TestSetTaskToPlayIfValid_AlreadyWorking_PickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToPlayIfValid_AlreadyWorkingCore(task, WarehouseTaskFormFlowTypes.PickByLabelJob, staff);
		}

		public void TestSetTaskToPlayIfValid_AlreadyWorking_DirectedPackingJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToPlayIfValid_AlreadyWorkingCore(task, WarehouseTaskFormFlowTypes.DirectedPackingJob, staff);
		}

		public void TestSetTaskToPlayIfValid_AlreadyWorking_TransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToPlayIfValid_AlreadyWorkingCore(task, WarehouseTaskFormFlowTypes.TransferJob, staff);
		}

		public void TestSetTaskToPlayIfValid_AlreadyWorking_ReplenishmentJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToPlayIfValid_AlreadyWorkingCore(task, WarehouseTaskFormFlowTypes.ReplenishmentJob, staff);
		}

		void TestSetTaskToPlayIfValid_AlreadyWorkingCore(ProcessTask task, string taskType, GlbStaff staff)
		{
			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, taskType, staff.GS_Code);

			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.SuccessWithNoChanges, result);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_NullArguments()
		{
			var processTask = Factory.New<ProcessTask>();
			var staff = Helper.CreateGlbStaff("US1", "User");

			var service = new WhsTaskManagementService();
			AssertExceptionThrown<ArgumentNullException>(() => service.SetTaskToPlayIfValid(null, WarehouseTaskFormFlowTypes.UnloadJob, staff.GS_Code));
			AssertExceptionThrown<ArgumentNullException>(() => service.SetTaskToPlayIfValid(processTask, WarehouseTaskFormFlowTypes.UnloadJob, null));
			AssertExceptionThrown<ArgumentException>(() => service.SetTaskToPlayIfValid(processTask, WarehouseTaskFormFlowTypes.UnloadJob, string.Empty));
		}

		public void TestSetTaskToPlayIfValid_SuspendedTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, WarehouseTaskFormFlowTypes.UnloadJob, staff.GS_Code);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.Success, result);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_OpenTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, WarehouseTaskFormFlowTypes.UnloadJob, staff.GS_Code);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.Success, result);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals("User should be assigned.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_WrongUserSet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff1 = Helper.CreateGlbStaff("US1", "User1");
			var staff2 = Helper.CreateGlbStaff("US2", "User2");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff1);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, WarehouseTaskFormFlowTypes.UnloadJob, staff2.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.AssignedUserIsDifferent, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff1.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_BlankFormFlowType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, WarehouseTaskFormFlowTypes.UnloadJob, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskIsWrongFormFlowType, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_DoesNotMatchExpectedTaskType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, WarehouseTaskFormFlowTypes.TransferJob, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskIsWrongFormFlowType, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_TaskIsCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			task.P9_CompletedTime = DateTime.Now;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, WarehouseTaskFormFlowTypes.UnloadJob, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskStatusIsCompleted, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToPlayIfValid_TaskIsCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToPlayIfValid(task, WarehouseTaskFormFlowTypes.UnloadJob, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskStatusIsCancelled, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		#endregion

		#region TestSetTaskToSuspendedIfValid

		public void TestSetTaskToSuspendedIfValid_ReceiveJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToSuspendedIfValidCore(task, staff);
		}

		public void TestSetTaskToSuspendedIfValid_PutawayTransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

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
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToSuspendedIfValidCore(task, staff);
		}

		public void TestSetTaskToSuspendedIfValid_PickJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToSuspendedIfValidCore(task, staff);
		}

		public void TestSetTaskToSuspendedIfValid_PickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToSuspendedIfValidCore(task, staff);
		}

		public void TestSetTaskToSuspendedIfValid_DirectedPackingJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToSuspendedIfValidCore(task, staff);
		}

		public void TestSetTaskToSuspendedIfValid_TransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToSuspendedIfValidCore(task, staff);
		}

		public void TestSetTaskToSuspendedIfValid_ReplenishmentJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToSuspendedIfValidCore(task, staff);
		}

		void TestSetTaskToSuspendedIfValidCore(ProcessTask task, GlbStaff staff)
		{
			var service = new WhsTaskManagementService();
			var result = service.SetTaskToSuspendedIfValid(task, staff.GS_Code);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.Success, result);
			AssertEquals("Valid task should set suspended time success.", true, task.P9_SuspendedAtUtc.IsValid);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Suspended, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToSuspendedIfValid_AlreadySuspended_ReceiveJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(task, staff, task.P9_SuspendedAtUtc);
		}

		public void TestSetTaskToSuspendedIfValid_AlreadySuspended_PutawayTransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

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
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(task, staff, task.P9_SuspendedAtUtc);
		}

		public void TestSetTaskToSuspendedIfValid_AlreadySuspended_PickJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(task, staff, task.P9_SuspendedAtUtc);
		}

		public void TestSetTaskToSuspendedIfValid_AlreadySuspended_PickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(task, staff, task.P9_SuspendedAtUtc);
		}

		public void TestSetTaskToSuspendedIfValid_AlreadySuspended_DirectedPackingJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(task, staff, task.P9_SuspendedAtUtc);
		}

		public void TestSetTaskToSuspendedIfValid_AlreadySuspended_TransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(task, staff, task.P9_SuspendedAtUtc);
		}

		public void TestSetTaskToSuspendedIfValid_AlreadySuspended_ReplenishmentJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(task, staff, task.P9_SuspendedAtUtc);
		}

		void TestSetTaskToSuspendedIfValid_AlreadyCancelledCore(ProcessTask task, GlbStaff staff, ZDateTime expectedSuspendedTime)
		{
			var service = new WhsTaskManagementService();
			var result = service.SetTaskToSuspendedIfValid(task, staff.GS_Code);

			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.SuccessWithNoChanges, result);
			AssertEquals("Valid task should set suspended time success.", expectedSuspendedTime, task.P9_SuspendedAtUtc);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Suspended, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToSuspendedIfValid_NullArguments()
		{
			var processTask = Factory.New<ProcessTask>();
			var staff = Helper.CreateGlbStaff("US1", "User");

			var service = new WhsTaskManagementService();
			AssertExceptionThrown<ArgumentNullException>(() => service.SetTaskToSuspendedIfValid(null, staff.GS_Code));
			AssertExceptionThrown<ArgumentNullException>(() => service.SetTaskToSuspendedIfValid(processTask, null));
			AssertExceptionThrown<ArgumentException>(() => service.SetTaskToSuspendedIfValid(processTask, string.Empty));
		}

		public void TestSetTaskToSuspendedIfValid_WrongUserSet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff1 = Helper.CreateGlbStaff("US1", "User1");
			var staff2 = Helper.CreateGlbStaff("US2", "User2");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff1);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToSuspendedIfValid(task, staff2.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.AssignedUserIsDifferent, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff1.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToSuspendedIfValid_IsNotWarehouseTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToSuspendedIfValid(task, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskIsNotValidWarehouseJob, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToSuspendedIfValid_OpenTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToSuspendedIfValid(task, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskStatusIsOpen, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			AssertEquals("User should not be assigned.", string.Empty, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToSuspendedIfValid_TaskIsCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			task.P9_CompletedTime = DateTime.Now;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToSuspendedIfValid(task, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskStatusIsCompleted, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToSuspendedIfValid_TaskIsCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToSuspendedIfValid(task, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskStatusIsCancelled, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		#endregion

		#region TestSetTaskToCompletedIfValid

		public void TestSetTaskToCompletedIfValid_ReceiveJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToCompletedIfValidCore(task, staff);
		}

		public void TestSetTaskToCompletedIfValid_PutawayTransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

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
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToCompletedIfValidCore(task, staff);
		}

		public void TestSetTaskToCompletedIfValid_PickJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToCompletedIfValidCore(task, staff);
		}

		public void TestSetTaskToCompletedIfValid_PickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestSetTaskToCompletedIfValidCore(task, staff);
		}

		public void TestSetTaskToCompletedIfValid_DirectedPackingJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToCompletedIfValidCore(task, staff);
		}

		public void TestSetTaskToCompletedIfValid_TransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToCompletedIfValidCore(task, staff);
		}

		public void TestSetTaskToCompletedIfValid_ReplenishmentJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestSetTaskToCompletedIfValidCore(task, staff);
		}

		void TestSetTaskToCompletedIfValidCore(ProcessTask task, GlbStaff staff)
		{
			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff.GS_Code);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.Success, result);
			AssertEquals("Valid task should set complete time success.", true, task.P9_CompletedTimeUtc.IsValid);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToCompletedIfValid_AlreadyClosed_ReceiveJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestSetTaskToCompletedIfValid_AlreadyCompletedCore(task, staff, task.P9_CompletedTimeUtc);
		}

		public void TestSetTaskToCompletedIfValid_AlreadyCompleted_PutawayTransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

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
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestSetTaskToCompletedIfValid_AlreadyCompletedCore(task, staff, task.P9_CompletedTimeUtc);
		}

		public void TestSetTaskToCompletedIfValid_AlreadyCompleted_PickJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestSetTaskToCompletedIfValid_AlreadyCompletedCore(task, staff, task.P9_CompletedTimeUtc);
		}

		public void TestSetTaskToCompletedIfValid_AlreadyCompleted_PickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Factory.Save();

			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestSetTaskToCompletedIfValid_AlreadyCompletedCore(task, staff, task.P9_CompletedTimeUtc);
		}

		public void TestSetTaskToCompletedIfValid_AlreadyCompleted_DirectedPackingJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestSetTaskToCompletedIfValid_AlreadyCompletedCore(task, staff, task.P9_CompletedTimeUtc);
		}

		public void TestSetTaskToCompletedIfValid_AlreadyCompleted_TransferJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestSetTaskToCompletedIfValid_AlreadyCompletedCore(task, staff, task.P9_CompletedTimeUtc);
		}

		public void TestSetTaskToCompletedIfValid_AlreadyCompleted_ReplenishmentJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, pickfaceLocation);
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestSetTaskToCompletedIfValid_AlreadyCompletedCore(task, staff, task.P9_CompletedTimeUtc);
		}

		void TestSetTaskToCompletedIfValid_AlreadyCompletedCore(ProcessTask task, GlbStaff staff, ZDateTime expectedCompletedTime)
		{
			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff.GS_Code);

			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.SuccessWithNoChanges, result);
			AssertEquals("Valid task should set complete time success.", expectedCompletedTime, task.P9_CompletedTimeUtc);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToCompletedIfValid_NullArguments()
		{
			var processTask = Factory.New<ProcessTask>();
			var staff = Helper.CreateGlbStaff("US1", "User");

			var service = new WhsTaskManagementService();
			AssertExceptionThrown<ArgumentNullException>(() => service.SetTaskToCompletedIfValid(null, staff.GS_Code));
			AssertExceptionThrown<ArgumentNullException>(() => service.SetTaskToCompletedIfValid(processTask, null));
			AssertExceptionThrown<ArgumentException>(() => service.SetTaskToCompletedIfValid(processTask, string.Empty));
		}

		public void TestSetTaskToCompletedIfValid_AssignedTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff.GS_Code);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.Success, result);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToCompletedIfValid_AlreadySuspendedTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff.GS_Code);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Valid task should return success.", UpdateTaskStatusResult.Success, result);
			AssertEquals("Valid task should update status.", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToCompletedIfValid_WrongUserSet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff1 = Helper.CreateGlbStaff("US1", "User1");
			var staff2 = Helper.CreateGlbStaff("US2", "User2");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff1);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff2.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.AssignedUserIsDifferent, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff1.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToCompletedIfValid_IsNotWarehouseTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskIsNotValidWarehouseJob, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToCompletedIfValid_OpenTask()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskStatusIsOpen, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			AssertEquals("User should not be assigned.", string.Empty, task.P9_GS_NKAssignedStaffMember);
		}

		public void TestSetTaskToCompletedIfValid_TaskIsCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var service = new WhsTaskManagementService();
			var result = service.SetTaskToCompletedIfValid(task, staff.GS_Code);

			AssertEquals("Invalid task should return appropriate result.", UpdateTaskStatusResult.TaskStatusIsCancelled, result);
			AssertEquals("Invalid task should not update status.", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
			AssertEquals("Assigned User should not be changed.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		#endregion
	}
}
