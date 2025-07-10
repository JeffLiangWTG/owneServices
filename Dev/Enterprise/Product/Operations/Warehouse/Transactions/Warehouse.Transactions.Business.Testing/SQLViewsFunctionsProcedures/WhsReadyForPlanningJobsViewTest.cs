using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReadyForPlanningJobsViewTest : WhsTestCaseWithFactory
	{
		#region TestReceive

		public void TestReceive_EmptyPlanningStatus()
			=> TestReceive(string.Empty, false);

		public void TestReceive_ReadyForPlanning()
			=> TestReceive(TaskPlanningStatus.Codes.Ready, true);

		public void TestReceive_NotReadyForPlanning()
			=> TestReceive(TaskPlanningStatus.Codes.NotReady, false);

		public void TestReceive_NotReadyForPlanning_Planned()
			=> TestReceive(TaskPlanningStatus.Codes.Planned, false);

		void TestReceive(string taskPlanningStatus, bool shouldAppearInView)
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: false);
				Factory.Save();

				receive.WD_TaskPlanningStatus = taskPlanningStatus;
				Factory.Save();
				AssertEquals("Precondition.", taskPlanningStatus, receive.WD_TaskPlanningStatus);

				var jobsReadyForPlanning = LoadDataFromView();

				if (shouldAppearInView)
				{
					AssertEquals("Should have 1 job ready for planning.", 1, jobsReadyForPlanning.Count);

					var jobReadyForPlanning = jobsReadyForPlanning[0];
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.PK, receive.PK, ((ZGuid)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.PK]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType, receive.WD_DocketType, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo, receive.WD_DocketID, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc, ZDateTime.TruncateSeconds(receive.WD_SystemCreateTimeUtc), ((ZDateTime)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc]));
				}
				else
				{
					AssertEquals("Should have no jobs ready for planning.", 0, jobsReadyForPlanning.Count);
				}
			}
		}

		#endregion

		#region TestTransfer

		public void TestTransfer_EmptyPlanningStatus()
			=> TestTransfer(string.Empty, false);

		public void TestTransfer_ReadyForPlanning()
			=> TestTransfer(TaskPlanningStatus.Codes.Ready, true);

		public void TestTransfer_NotReadyForPlanning()
			=> TestTransfer(TaskPlanningStatus.Codes.NotReady, false);

		public void TestTransfer_NotReadyForPlanning_Planned()
			=> TestTransfer(TaskPlanningStatus.Codes.Planned, false);

		void TestTransfer(string taskPlanningStatus, bool shouldAppearInView)
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location1, "");
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
				var tranferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
				transfer.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = taskPlanningStatus;
				Factory.Save();
				AssertEquals("Precondition.", taskPlanningStatus, transfer.WD_TaskPlanningStatus);

				var jobsReadyForPlanning = LoadDataFromView();

				if (shouldAppearInView)
				{
					AssertEquals("Should have 1 job ready for planning.", 1, jobsReadyForPlanning.Count);

					var jobReadyForPlanning = jobsReadyForPlanning[0];
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.PK, transfer.PK, ((ZGuid)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.PK]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType, transfer.WD_DocketType, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo, transfer.WD_DocketID, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc, ZDateTime.TruncateSeconds(transfer.WD_SystemCreateTimeUtc), ((ZDateTime)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc]));
				}
				else
				{
					AssertEquals("Should have no jobs ready for planning.", 0, jobsReadyForPlanning.Count);
				}
			}
		}

		#endregion

		#region TestReplenishmentTransfer

		public void TestReplenishmentTransfer_EmptyPlanningStatus()
			=> TestReplenishmentTransfer(string.Empty, false);

		public void TestReplenishmentTransfer_ReadyForPlanning()
			=> TestReplenishmentTransfer(TaskPlanningStatus.Codes.Ready, true);

		public void TestReplenishmentTransfer_NotReadyForPlanning()
			=> TestReplenishmentTransfer(TaskPlanningStatus.Codes.NotReady, false);

		public void TestReplenishmentTransfer_NotReadyForPlanning_Planned()
			=> TestReplenishmentTransfer(TaskPlanningStatus.Codes.Planned, false);

		void TestReplenishmentTransfer(string taskPlanningStatus, bool shouldAppearInView)
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location1, "");
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
				transfer.WD_IsPickFaceReplenishment = true;

				var tranferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
				transfer.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = taskPlanningStatus;
				Factory.Save();
				AssertEquals("Precondition.", taskPlanningStatus, transfer.WD_TaskPlanningStatus);

				var jobsReadyForPlanning = LoadDataFromView();

				if (shouldAppearInView)
				{
					AssertEquals("Should have 1 job ready for planning.", 1, jobsReadyForPlanning.Count);

					var jobReadyForPlanning = jobsReadyForPlanning[0];
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.PK, transfer.PK, ((ZGuid)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.PK]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType, NonPersistentTransferType.Codes.AutoCreatedReplenishment, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo, transfer.WD_DocketID, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc, ZDateTime.TruncateSeconds(transfer.WD_SystemCreateTimeUtc), ((ZDateTime)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc]));
				}
				else
				{
					AssertEquals("Should have no jobs ready for planning.", 0, jobsReadyForPlanning.Count);
				}
			}
		}

		#endregion

		#region TestPick

		public void TestPick_EmptyPlanningStatus()
			=> TestPick(string.Empty, false);

		public void TestPick_ReadyForPlanning()
			=> TestPick(TaskPlanningStatus.Codes.Ready, true);

		public void TestPick_NotReadyForPlanning()
			=> TestPick(TaskPlanningStatus.Codes.NotReady, false);

		public void TestPick_NotReadyForPlanning_Planned()
			=> TestPick(TaskPlanningStatus.Codes.Planned, false);

		void TestPick(string taskPlanningStatus, bool shouldAppearInView)
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetPickTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				pick.WP_TaskPlanningStatus = taskPlanningStatus;
				Factory.Save();
				AssertEquals("Precondition.", taskPlanningStatus, pick.WP_TaskPlanningStatus);

				var jobsReadyForPlanning = LoadDataFromView();

				if (shouldAppearInView)
				{
					AssertEquals("Should have 1 job ready for planning.", 1, jobsReadyForPlanning.Count);

					var jobReadyForPlanning = jobsReadyForPlanning[0];
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.PK, pick.PK, ((ZGuid)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.PK]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType, Constants.Pick, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo, pick.WP_PickNo, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo]));
					AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc, ZDateTime.TruncateSeconds(pick.WP_SystemCreateTimeUtc), ((ZDateTime)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc]));
				}
				else
				{
					AssertEquals("Should have no jobs ready for planning.", 0, jobsReadyForPlanning.Count);
				}
			}
		}

		#endregion

		#region TestLoad

		public void TestLoad_EmptyPlanningStatus()
			=> TestLoad(string.Empty, false);

		public void TestLoad_ReadyForPlanning()
			=> TestLoad(TaskPlanningStatus.Codes.Ready, true);

		public void TestLoad_NotReadyForPlanning()
			=> TestLoad(TaskPlanningStatus.Codes.NotReady, false);

		public void TestLoad_NotReadyForPlanning_Planned()
			=> TestLoad(TaskPlanningStatus.Codes.Planned, false);

		void TestLoad(string taskPlanningStatus, bool shouldAppearInView)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;
			Factory.Save();

			load.WLO_TaskPlanningStatus = taskPlanningStatus;
			Factory.Save();
			AssertEquals("Precondition.", taskPlanningStatus, load.WLO_TaskPlanningStatus);

			var jobsReadyForPlanning = LoadDataFromView();

			if (shouldAppearInView)
			{
				AssertEquals("Should have 1 job ready for planning.", 1, jobsReadyForPlanning.Count);

				var jobReadyForPlanning = jobsReadyForPlanning[0];
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.PK, load.PK, ((ZGuid)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.PK]));
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType, Constants.Load, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType]));
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo, load.WLO_JobID, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo]));
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc, ZDateTime.TruncateSeconds(load.WLO_SystemCreateTimeUtc), ((ZDateTime)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc]));
			}
			else
			{
				AssertEquals("Should have no jobs ready for planning.", 0, jobsReadyForPlanning.Count);
			}
		}

		#endregion

		#region TestCycleCount

		public void TestCycleCount_EmptyPlanningStatus()
			=> TestCycleCount(string.Empty, false);

		public void TestCycleCount_ReadyForPlanning()
			=> TestCycleCount(TaskPlanningStatus.Codes.Ready, true);

		public void TestCycleCount_NotReadyForPlanning()
			=> TestCycleCount(TaskPlanningStatus.Codes.NotReady, false);

		public void TestCycleCount_NotReadyForPlanning_Planned()
			=> TestCycleCount(TaskPlanningStatus.Codes.Planned, false);

		void TestCycleCount(string taskPlanningStatus, bool shouldAppearInView)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 5m, location2, "PLT");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, ZDateTimeOffset.Empty, "TTT");
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, ZDateTimeOffset.Empty, "TTT");
			Factory.Save();

			cycleCount1.WCL_TaskPlanningStatus = taskPlanningStatus;
			cycleCount2.WCL_TaskPlanningStatus = taskPlanningStatus;
			Factory.Save();
			AssertEquals("Precondition.", taskPlanningStatus, cycleCount1.WCL_TaskPlanningStatus);
			AssertEquals("Precondition.", taskPlanningStatus, cycleCount2.WCL_TaskPlanningStatus);

			cycleCount1.WCL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-7);
			cycleCount2.WCL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-14);
			Factory.Save();

			var jobsReadyForPlanning = LoadDataFromView();

			if (shouldAppearInView)
			{
				AssertEquals("Should have 1 job per warehouse ready for planning.", 1, jobsReadyForPlanning.Count);

				var jobReadyForPlanning = jobsReadyForPlanning[0];
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.PK, data.Whs1.PK, ((ZGuid)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.PK]));
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType, Constants.CycleCount, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobType]));
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo, data.Whs1.WW_WarehouseCode, ((ZString)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_JobNo]));
				AssertEquals(WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc, ZDateTime.TruncateSeconds(cycleCount2.WCL_SystemCreateTimeUtc), ((ZDateTime)jobReadyForPlanning[WhsReadyForPlanningJobsViewSchema.Constants.WRV_SystemCreateTimeUtc]));
			}
			else
			{
				AssertEquals("Should have no jobs ready for planning.", 0, jobsReadyForPlanning.Count);
			}
		}

		#endregion

		#region Implementation

		DynamicBusinessObjectCollection LoadDataFromView()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load("SELECT * FROM dbo.WhsReadyForPlanningJobsView");
			return result;
		}

		static class Constants
		{
			public const string Pick = "PIC";
			public const string Load = "LOA";
			public const string CycleCount = "CCL";
		}

		#endregion
	}
}
