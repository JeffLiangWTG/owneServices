using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class BeginRFUnloadTaskTest : WhsSecureServiceTestCase
	{
		public void TestBeginRFUnloadTask_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(Guid.NewGuid());

			AssertBusinessValidationError(webService, "Task could not be found.", response);
		}

		public void TestBeginRFUnloadTask_WrongTaskType()
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
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());

			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFUnloadTask_SetsTaskToPlay()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestBeginRFUnloadTask_LoadsReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());

			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals("PK is correct.", response.Dockets[0].PK, receive.PK);
			AssertEquals("DocketID is correct", response.Dockets[0].DocketID, receive.WD_DocketID);
		}

		public void TestBeginRFUnloadTask_LoadsReceive_HoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var heldCode1 = Helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var heldCode2 = Helper.CreateInventoryHeldCode("BBB", "BBB for client 1", data.Org1.PK);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());

			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(7, response.HeldCodes.Length);
			response.HeldCodes.Single(p => string.IsNullOrEmpty(p.Code) && string.IsNullOrEmpty(p.Description));
			response.HeldCodes.Single(p => p.Code == "HEL" && p.Description == "Held");
			response.HeldCodes.Single(p => p.Code == "DAM" && p.Description == "Damaged");
			response.HeldCodes.Single(p => p.Code == "LCC" && p.Description == "Lost in Cycle Count");
			response.HeldCodes.Single(p => p.Code == "SHORT" && p.Description == "Short Picked");
			response.HeldCodes.Single(p => p.Code == "AAA" && p.Description == "AAA for system");
			response.HeldCodes.Single(p => p.Code == "BBB" && p.Description == "BBB for client 1");
		}

		public void TestBeginRFUnloadTask_ASNReceive()
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertNotNull(response.Dockets);
			AssertEquals("Receive PK should be correct", receive.PK, response.Dockets[0].PK);
			AssertEquals("ASNUnloadState should be correct", ASNState.NoPallets, response.Dockets[0].ASNUnloadState);
		}

		public void TestBeginRFUnloadTask_CustomsReceive()
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());
			AssertEquals("Cannot unload Customs Receives with RF device.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestBeginRFUnloadTask_FinalisedReceive()
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());
			AssertEquals("Cannot unload finalized or canceled Receives.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestBeginRFUnloadTask_CancelledReceive()
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Helper.Factory.Save();
			AssertEquals("Receive is cancelled", true, receive.IsCancelled);

			var taskPK = ZGuid.BrettsGuid;
			var insertProcessTaskQuery = @$"INSERT INTO ProcessTasks (P9_PK, P9_TaskID, P9_Description, P9_Type, P9_Status, P9_GS_NKAssignedStaffMember, P9_ParentID, P9_ParentTableCode, P9_FormFlowType)
VALUES (@taskPK, 'TASK-0001', 'Some Description', 'UDF', 'ASN', '{staff.GS_Code}', @parentID, 'WD', 'WUL')";
			using (var command = Db.Connection.Command(insertProcessTaskQuery))
			{
				command.AddParameter("@taskPK", SqlDbType.UniqueIdentifier, taskPK.ToGuid());
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, receive.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			var unloadTask = Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, receive.PK)).Single();
			AssertEquals("Precondition", WarehouseTaskFormFlowTypes.UnloadJob, unloadTask.P9_FormFlowType);
			AssertNotEquals("Precondition", ProcessTaskStatusCodeList.Codes.Cancelled, unloadTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(taskPK.ToGuid());
			AssertEquals("Cannot unload finalized or canceled Receives.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestBeginRFUnloadTask_NullReceive()
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_ParentID = Guid.NewGuid();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFUnloadTask(task.PK.ToGuid());
			AssertEquals("Receive not found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestBeginRFUnloadTask_DbHitsTest()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 15; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, i % 2 == 0 ? $"PLT{i}" : string.Empty);
			}
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 4 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 2 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsClientParameterByWarehouseSchema.Constants.TableName, 1 },
				{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
			};

			WhsDocketsWebServiceResponse response = null;
			var webService = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService.Factory))
			using (RowFactory.SetCachedTables())
			{
				response = webService.BeginRFUnloadTask(task.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			var responseDocket = response.Dockets[0];
			AssertEquals("PK is correct.", responseDocket.PK, receive.PK);
			AssertEquals("DocketID is correct", responseDocket.DocketID, receive.WD_DocketID);
			AssertEquals("ASNUnloadState should be correct", ASNState.NoPallets, responseDocket.ASNUnloadState);
		}
	}
}
