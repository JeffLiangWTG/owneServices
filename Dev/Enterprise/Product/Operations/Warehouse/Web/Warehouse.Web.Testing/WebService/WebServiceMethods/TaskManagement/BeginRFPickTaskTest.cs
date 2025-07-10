using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class BeginRFPickTaskTest : WhsPickingSecureServiceTestCase
	{
		public void TestBeginRFPickTask_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(Guid.NewGuid());

			AssertBusinessValidationError(webService, "Picking Task could not be found.", response);
			AssertEquals(Guid.Empty, response.TaskPK);
		}

		public void TestBeginRFPickTask_WrongTaskType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);

			var pickLine = order.Lines[0].PickLines[0];
			pickLine.WZ_P9_Task = task.PK;
			AssertEquals(string.Empty, pickLine.WZ_GS_NKAssignedTo);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertBusinessValidationError(webService, "Picking Task could not be found.", response);
			AssertEquals(Guid.Empty, response.TaskPK);
		}

		public void TestBeginRFPickTask_SetsTaskToPlay()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var pickLine = order.Lines[0].PickLines[0];
			AssertEquals(string.Empty, pickLine.WZ_GS_NKAssignedTo);

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(task.PK.ToGuid(), response.TaskPK);

			var newFactory = new BusinessObjectFactory();
			var pickLineInOtherFactory = newFactory.Load<WhsPickLine>(pickLine.PK);
			AssertEquals(staff.GS_Code, pickLineInOtherFactory.WZ_GS_NKAssignedTo);
		}

		public void TestBeginRFPickTask_LoadsPickDetails()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(task.PK.ToGuid(), response.TaskPK);

			var pickLogs = Helper.FindLogs(pick.Logs, Events.ServiceCommenced);
			CombineAssertions(() =>
			{
				AssertEquals(1, pickLogs.Length);
				AssertEquals(pick.WP_PickNo, pickLogs[0].ReferenceFreeText);
				AssertEquals(pick.WP_PickNo, response.Pick.Reference);
			});

			AssertPickLines(new WhsPickLineInfoCollection(pick.GetAllPickLines(), new WhsPickInfo()), response.Pick.Lines);
		}

		public void TestBeginRFPickTask_OnlyLoadsLinesAssignedToUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			var staff2 = Helper.CreateGlbStaff("US2", "User2");

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var pickLine1 = order.Lines[0].PickLines[0];
			var pickLine2 = order.Lines[0].PickLines[1];

			pickLine2.WZ_GS_NKAssignedTo = staff2.GS_Code;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(task.PK.ToGuid(), response.TaskPK);

			var pickLogs = Helper.FindLogs(pick.Logs, Events.ServiceCommenced);
			CombineAssertions(() =>
			{
				AssertEquals(1, pickLogs.Length);
				AssertEquals(pick.WP_PickNo, pickLogs[0].ReferenceFreeText);
				AssertEquals(pick.WP_PickNo, response.Pick.Reference);
				
				AssertEquals("Pickline returned to the gun should be marked as IsPicking.", true, pickLine1.WZ_IsPicking);
				AssertEquals("Pickline *NOT* returned to the gun should *NOT* be marked as IsPicking.", false, pickLine2.WZ_IsPicking);
			});

			AssertPickLines(new WhsPickLineInfoCollection([pickLine1], new WhsPickInfo()), response.Pick.Lines);
		}

		public void TestBeginRFPickTask_PutawayOnly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines[0];
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(task.PK.ToGuid(), response.TaskPK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			var pickLogs = Helper.FindLogs(pick.Logs, Events.ServiceCommenced);
			CombineAssertions(() =>
			{
				AssertEquals(1, pickLogs.Length);
				AssertEquals(pick.WP_PickNo, pickLogs[0].ReferenceFreeText);
				AssertEquals(pick.WP_PickNo, response.Pick.Reference);

				AssertEquals("Should be a putaway only pick.", false, response.Pick.Lines.Any());
				AssertEquals("Should be a putaway only pick.", true, response.Pick.IsPutawayOnly);
			});
		}

		public void TestBeginRFPickTask_LinesArePickedAndPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines[0];
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertBusinessValidationError(webService, "No Pick Lines could be found for this task for current user.", response);
			AssertEquals(Guid.Empty, response.TaskPK);
		}

		public void TestBeginRFPickTask_MixOfPickedAndUnpickedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = order.Lines[0].PickLines[0];
			var pickLine2 = order.Lines[0].PickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(task.PK.ToGuid(), response.TaskPK);

			var pickLogs = Helper.FindLogs(pick.Logs, Events.ServiceCommenced);
			CombineAssertions(() =>
			{
				AssertEquals(1, pickLogs.Length);
				AssertEquals(pick.WP_PickNo, pickLogs[0].ReferenceFreeText);
				AssertEquals(pick.WP_PickNo, response.Pick.Reference);
			});

			AssertPickLines(new WhsPickLineInfoCollection([pickLine2], new WhsPickInfo()), response.Pick.Lines);
		}

		public void TestBeginRFPickTask_MixOfPutawayAndUnpickedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = order.Lines[0].PickLines[0];
			var pickLine2 = order.Lines[0].PickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(task.PK.ToGuid(), response.TaskPK);

			var pickLogs = Helper.FindLogs(pick.Logs, Events.ServiceCommenced);
			CombineAssertions(() =>
			{
				AssertEquals(1, pickLogs.Length);
				AssertEquals(pick.WP_PickNo, pickLogs[0].ReferenceFreeText);
				AssertEquals(pick.WP_PickNo, response.Pick.Reference);
			});

			AssertPickLines(new WhsPickLineInfoCollection([pickLine2], new WhsPickInfo()), response.Pick.Lines);
		}

		public void TestBeginRFPickTask_SortsPickLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 2);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var org2 = Helper.CreateClient("2");
			var part3 = Helper.CreateProduct(org2, "P3");
			var warehouse = Helper.CreateWarehouse("WH2");
			var row1 = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "B", 2, 2, 2);
			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse, "1", Notify);
			var receive2 = Helper.CreateWhsReceive(org2, warehouse, "2", Notify);
			Helper.Factory.Save();

			// Create Varied Inventories across different locations
			SetInventory(receive1, warehouse.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetInventory(receive1, warehouse.FindLocation("A-1-1-1").PK, data.Part2.PK);
			SetInventory(receive1, warehouse.FindLocation("A-1-1-2").PK, data.Part1.PK, pickPathSequence: 2);
			SetInventory(receive1, warehouse.FindLocation("A-1-2-1").PK, data.Part1.PK);
			SetInventory(receive1, warehouse.FindLocation("A-1-2-1").PK, data.Part2.PK);
			SetInventory(receive2, warehouse.FindLocation("A-1-2-1").PK, part3.PK);
			SetInventory(receive2, warehouse.FindLocation("A-1-2-2").PK, part3.PK, pickPathSequence: 1);
			SetInventory(receive1, warehouse.FindLocation("A-2-1-1").PK, data.Part1.PK);
			SetInventory(receive1, warehouse.FindLocation("A-2-1-2").PK, data.Part1.PK);
			SetInventory(receive1, warehouse.FindLocation("A-2-2-1").PK, data.Part2.PK);

			SetInventory(receive2, warehouse.FindLocation("B-1-2-1").PK, part3.PK, pickPathSequence: 3);
			SetInventory(receive2, warehouse.FindLocation("B-1-2-2").PK, part3.PK, pickPathSequence: 4);
			SetInventory(receive1, warehouse.FindLocation("B-2-1-1").PK, data.Part1.PK);
			SetInventory(receive1, warehouse.FindLocation("B-2-1-1").PK, data.Part2.PK);

			receive1.FinaliseDocket();
			receive2.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, warehouse);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 40m);

			var order2 = Helper.CreateWhsOrder(org2, warehouse);
			var orderLine6 = Helper.CreateWhsOrderLine(order2, part3, 40m);

			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickTask(task.PK.ToGuid());

			var pickLines = pick.GetAllPickLines().ToArray();
			AssertPickLines(new WhsPickLineInfoCollection(pick.GetAllPickLines(), new WhsPickInfo()), response.Pick.Lines);

			Array.Sort(pickLines, new SortPickLinesForPickingSlip());
			var responseLines = response.Pick.Lines;

			for (var i = 0; i < pickLines.Length; i++)
			{
				AssertEquals("Lines should be in sorted order.", pickLines[i].PK, responseLines[i].PKs.Single());
			}

			void SetInventory(WhsReceive receive, ZGuid location, ZGuid part, short pickPathSequence = 0)
			{
				var line = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
				line.WI_WL = location;
				line.Location.WLV_PickPathSequence = pickPathSequence;
			}
		}

		public void TestBeginRFPickTask_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			}
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var orders = new List<WhsOrder>();
			for (var i = 0; i < 10; i++)
			{
				orders.Add(Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 5m));
			}
			var pick = Helper.CreatePickNew(orders.ToArray());
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
				{
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 4 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ StmEventSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 3 },
					{ WhsDocketLineSchema.Constants.TableName, 2 },
					{ WhsInventoryViewSchema.Constants.TableName, 2 },
					{ WhsLocationViewSchema.Constants.TableName, 3 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsPickFaceSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 2 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
				};

			var webService = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.BeginRFPickTask(task.PK.ToGuid());

				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}
	}
}
