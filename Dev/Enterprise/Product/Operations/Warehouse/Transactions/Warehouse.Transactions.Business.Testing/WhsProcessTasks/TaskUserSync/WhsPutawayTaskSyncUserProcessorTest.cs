using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPutawayTaskSyncUserProcessor))]
	public class WhsPutawayTaskSyncUserProcessorTest : WhsTestCaseWithFactory
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<WhsPutawayTaskSyncUserProcessor>(ObjectFactory.Get<IProcessTaskSyncUserProcessor>("WhsPutawayTaskSyncUserProcessor"));
		}

		public void TestSyncUser()
		{
			TestSyncUser(existingUser: false);
		}

		public void TestSyncUser_ExistingUser()
		{
			TestSyncUser(existingUser: true);
		}

		void TestSyncUser(bool existingUser)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("TST", "TSTName");
			GlbStaff existing = null;

			var receiveWithDockDoor =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receiveWithDockDoor, data.Part1, 10m);
			inventory1.WE_PalletID = "12345";
			inventory1.WE_WL = dockDoorLocation.PK;
			inventory1.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			var inventory2 = Helper.CreateWhsReceiveLine(receiveWithDockDoor, data.Part2, 10m);
			inventory2.WE_PalletID = "875";
			inventory2.WE_WL = dockDoorLocation.PK;
			inventory2.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine1 = Helper.SetupTransferLineForDockDoorLocation(
				putawayTransfer,
				data.Part1,
				dockDoorLocation,
				nonDockDoorLocation,
				"12345",
				10m);
			putawayTransferLine1.RunPreSaveValidation();

			var putawayTransferLine2 = Helper.SetupTransferLineForDockDoorLocation(
				putawayTransfer,
				data.Part2,
				dockDoorLocation,
				nonDockDoorLocation,
				"875",
				10m);
			putawayTransferLine2.RunPreSaveValidation();

			if (existingUser)
			{
				existing = Helper.CreateGlbStaff("EXR", "EXRname");
				putawayTransferLine1.WE_GS_NKPutawayBy = existing.GS_Code;
				putawayTransferLine2.WE_GS_NKPutawayBy = existing.GS_Code;
			}
			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, existing);
			Factory.Save();

			AssertEquals("Precondition: putawayTransferLine1 user", existing?.GS_Code ?? string.Empty, putawayTransferLine1.WE_GS_NKPutawayBy);
			AssertEquals("Precondition: putawayTransferLine2 user", existing?.GS_Code ?? string.Empty, putawayTransferLine2.WE_GS_NKPutawayBy);
			AssertEquals("Precondition: task user", existing?.GS_Code ?? string.Empty, task.P9_GS_NKAssignedStaffMember);

			// changing the user and saving triggers processor
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			AssertEquals("WE_GS_NKPutawayBy should be correct", staff.GS_Code, putawayTransferLine1.WE_GS_NKPutawayBy);
			AssertEquals("WZ_GS_NKAssignedTo should be correct", staff.GS_Code, putawayTransferLine1.PickLines[0].WZ_GS_NKAssignedTo);
			AssertEquals("WE_GS_NKPutawayBy should be correct", staff.GS_Code, putawayTransferLine2.WE_GS_NKPutawayBy);
			AssertEquals("WZ_GS_NKAssignedTo should be correct", staff.GS_Code, putawayTransferLine2.PickLines[0].WZ_GS_NKAssignedTo);
		}

		public void TestSyncUser_MultipleUsersOnTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("TST", "TSTName");
			var other = Helper.CreateGlbStaff("EXR", "EXRName");

			var receive =
				Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			inventory1.WE_PalletID = "12345";
			inventory1.WE_WL = dockDoorLocation.PK;
			inventory1.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m);
			inventory2.WE_PalletID = "875";
			inventory2.WE_WL = dockDoorLocation.PK;
			inventory2.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine1 = Helper.SetupTransferLineForDockDoorLocation(
				putawayTransfer,
				data.Part1,
				dockDoorLocation,
				nonDockDoorLocation,
				"12345",
				10m);
			putawayTransferLine1.RunPreSaveValidation();

			var putawayTransferLine2 = Helper.SetupTransferLineForDockDoorLocation(
				putawayTransfer,
				data.Part2,
				dockDoorLocation,
				nonDockDoorLocation,
				"875",
				10m);
			putawayTransferLine2.RunPreSaveValidation();

			var task1 = Helper.CreateProcessTaskForTransfer(putawayTransfer, null);
			var task2 = Helper.CreateProcessTaskForTransfer(putawayTransfer, other);
			putawayTransferLine1.WE_P9_Task = task2.PK;
			putawayTransferLine2.WE_P9_Task = task1.PK;
			Factory.Save();

			AssertEquals("Precondition: putawayTransferLine1 user", string.Empty, putawayTransferLine1.WE_GS_NKPutawayBy);
			AssertEquals("Precondition: putawayTransferLine2 user", string.Empty, putawayTransferLine2.WE_GS_NKPutawayBy);
			AssertEquals("Precondition: task1 user", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Precondition: task2 user", other.GS_Code, task2.P9_GS_NKAssignedStaffMember);

			// changing the user and saving triggers processor
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			AssertEquals("WE_GS_NKPutawayBy should be correct", string.Empty, putawayTransferLine1.WE_GS_NKPutawayBy);
			AssertEquals("WE_GS_NKPutawayBy should be correct", staff.GS_Code, putawayTransferLine2.WE_GS_NKPutawayBy);
		}

		public void TestSyncUser_Picked()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("TST", "TSTName");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine =
				Helper.CreateWhsReceiveLine(receive, data.Part1, 33m, dockDoorLocation, "PLT-3");
			receiveLine.WE_AdjustmentArrivalDate = today;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(
				putawayTransfer,
				data.Part1,
				dockDoorLocation,
				nonDockDoorLocation,
				"PLT-3",
				33m);
			putawayTransfer.RunPreSaveValidation();
			putawayTransferLine.PickedTime = today;
			putawayTransferLine.WE_GS_NKPutawayBy = GlbStaff.CurrentUser.GS_Code;

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, null);
			Factory.Save();

			AssertEquals("Precondition: WZ_GS_NKAssignedTo", GlbStaff.CurrentUser.GS_Code, putawayTransferLine.PickLines[0].WZ_GS_NKAssignedTo);
			AssertEquals("Precondition: WZ_PickedDateTime", today, putawayTransferLine.PickLines[0].WZ_PickedDateTime);

			// changing the user and saving triggers processor
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			AssertEquals("WE_GS_NKPutawayBy should still be correct", GlbStaff.CurrentUser.GS_Code, putawayTransferLine.WE_GS_NKPutawayBy);
		}

		public void TestSyncUser_DBHits()
		{
			var count = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("TST", "TSTName");

			var receiveWithDockDoor = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			for (var i = 0; i < count; i++)
			{
				var inventory1 = Helper.CreateWhsReceiveLine(receiveWithDockDoor, data.Part1, 10m);
				inventory1.WE_PalletID = $"12345{i}";
				inventory1.WE_WL = dockDoorLocation.PK;
				inventory1.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
				var inventory2 = Helper.CreateWhsReceiveLine(receiveWithDockDoor, data.Part2, 10m);
				inventory2.WE_PalletID = $"875{i}";
				inventory2.WE_WL = dockDoorLocation.PK;
				inventory2.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			}
			Factory.Save();

			var putawayTransferLines = new List<WhsTransferLine>();
			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			for (var i = 0; i < count; i++)
			{
				var putawayTransferLine1 = Helper.SetupTransferLineForDockDoorLocation(
					putawayTransfer,
					data.Part1,
					dockDoorLocation,
					nonDockDoorLocation,
					$"12345{i}",
					10m);
				putawayTransferLine1.RunPreSaveValidation();

				var putawayTransferLine2 = Helper.SetupTransferLineForDockDoorLocation(
					putawayTransfer,
					data.Part2,
					dockDoorLocation,
					nonDockDoorLocation,
					$"875{i}",
					10m);
				putawayTransferLine2.RunPreSaveValidation();

				putawayTransferLines.Add(putawayTransferLine1);
				putawayTransferLines.Add(putawayTransferLine2);
			}

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, null);
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 4 },
			};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				// changing the user and saving triggers processor
				taskInNewFactory.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				newFactory.Save();
			}

			CombineAssertions(() =>
			{
				var putawayTransferLinesInNewFactory = newFactory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.PK, putawayTransferLines.Select(l => l.PK)));
				foreach (var line in putawayTransferLinesInNewFactory)
				{
					AssertEquals("WE_GS_NKPutawayBy should be correct", staff.GS_Code, line.WE_GS_NKPutawayBy);
				}
			});
		}
	}
}
