using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CheckReceiveASNQuantityMetTest : WhsSecureServiceTestCase
	{
		public void TestCheckReceiveASNQuantityMet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.Factory.Save();

			PopulateASNLines(receive);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "PLT1", data.Whs1.DefaultLocation.ToLocationString());
			var responseUnload = webService1.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(responseUnload, webService1);

			var webService2 = GetNewWebService(data.Whs1);
			var checkResponse = webService2.CheckReceiveASNQuantityMet(receive.PK.ToGuid());
			AssertSuccessfulResponse(checkResponse, webService2);
			AssertEquals(ErrorTypes.YesNoEnquiry, checkResponse.Error);
			AssertEquals($"Has the unload been completed for Receive {receive.WD_ExternalReference}?", checkResponse.ErrorMessage);
		}

		public void TestCheckReceiveASNQuantityMet_NoASNLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "PLT1", data.Whs1.DefaultLocation.ToLocationString());
			var responseUnload = webService1.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(responseUnload, webService1);

			var webService2 = GetNewWebService(data.Whs1);
			var checkResponse = webService2.CheckReceiveASNQuantityMet(receive.PK.ToGuid());
			AssertSuccessfulResponse(checkResponse, webService2);
			AssertNoResponseError(checkResponse);
		}

		public void TestCheckReceiveASNQuantityMet_InvalidPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.Factory.Save();

			PopulateASNLines(receive);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var checkResponse = webService.CheckReceiveASNQuantityMet(Guid.NewGuid());
			AssertBusinessValidationError(webService, "Receive not found.", checkResponse);
		}

		public void TestCheckReceiveASNQuantityMet_MissingPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.Factory.Save();

			PopulateASNLines(receive);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "PLT1", data.Whs1.DefaultLocation.ToLocationString());
			var responseUnload = webService1.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(responseUnload, webService1);

			var webService2 = GetNewWebService(data.Whs1);
			var checkResponse = webService2.CheckReceiveASNQuantityMet(receive.PK.ToGuid());
			AssertSuccessfulResponse(checkResponse, webService2);
			AssertNoResponseError(checkResponse);
		}

		public void TestCheckReceiveASNQuantityMet_QuantityNotMet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Helper.Factory.Save();

			PopulateASNLines(receive);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var checkResponse = webService.CheckReceiveASNQuantityMet(receive.PK.ToGuid());
			AssertSuccessfulResponse(checkResponse, webService);
			AssertNoResponseError(checkResponse);
		}

		public void TestCheckReceiveASNQuantityMet_Over()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.Factory.Save();

			PopulateASNLines(receive);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var receiveLine1 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT1", data.Whs1.DefaultLocation.ToLocationString());
			var responseUnload1 = webService1.UnloadWhsReceiveLines(new[] { receiveLine1 }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(responseUnload1, webService1);
			AssertNoResponseError(responseUnload1);

			var webService2 = GetNewWebService(data.Whs1);
			var receiveLine2 = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT2", data.Whs1.DefaultLocation.ToLocationString());
			var responseUnload2 = webService2.UnloadWhsReceiveLines(new[] { receiveLine2 }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(responseUnload2, webService2);
			AssertNoResponseError(responseUnload2);

			var webService3 = GetNewWebService(data.Whs1);
			var checkResponse = webService3.CheckReceiveASNQuantityMet(receive.PK.ToGuid());
			AssertSuccessfulResponse(checkResponse, webService3);
			AssertEquals(ErrorTypes.YesNoEnquiry, checkResponse.Error);
			AssertEquals($"Has the unload been completed for Receive {receive.WD_ExternalReference}?", checkResponse.ErrorMessage);
		}

		public void TestCheckReceiveASNQuantityMet_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var count = 10;
			var palletIDs = new List<string>();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (var i = 0; i < count; i++)
			{
				var palletID = $"PLT{i}";
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, palletID);
				palletIDs.Add(palletID);
			}
			Helper.Factory.Save();

			PopulateASNLines(receive);
			Helper.Factory.Save();

			foreach (var palletID in palletIDs)
			{
				var webServiceX = GetNewWebService(data.Whs1);
				var receiveLineInfo = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", palletID, data.Whs1.DefaultLocation.ToLocationString());
				var responseUnload = webServiceX.UnloadWhsReceiveLines(new[] { receiveLineInfo }, Array.Empty<Guid>(), false);
				AssertSuccessfulResponse(responseUnload, webServiceX);
				AssertNoResponseError(responseUnload);
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
			};

			var webService2 = GetNewWebService(data.Whs1);
			var newFactory = webService2.Factory;
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				var checkResponse = webService2.CheckReceiveASNQuantityMet(receive.PK.ToGuid());
				AssertSuccessfulResponse(checkResponse, webService2);
				AssertEquals(ErrorTypes.YesNoEnquiry, checkResponse.Error);
				AssertEquals($"Has the unload been completed for Receive {receive.WD_ExternalReference}?", checkResponse.ErrorMessage);
			}
		}

		public void TestCheckReceiveASNQuantityMet_PlannedReceiveWithTask_WorkingTask()
			=> TestCheckReceiveASNQuantityMet_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskAssignedToOtherUser: true);

		public void TestCheckReceiveASNQuantityMet_PlannedReceiveWithTask_WorkingTask_NotUnloadTask()
			=> TestCheckReceiveASNQuantityMet_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: false, isTaskAssignedToOtherUser: true);

		public void TestCheckReceiveASNQuantityMet_PlannedReceiveWithTask_NotWorkingTask()
			=> TestCheckReceiveASNQuantityMet_WithTaskManagement(isPlannedReceive: true, isWorkingTask: false, isUnloadTask: true, isTaskAssignedToOtherUser: true);

		public void TestCheckReceiveASNQuantityMet_NotPlannedReceiveWithTask_WorkingTask()
			=> TestCheckReceiveASNQuantityMet_WithTaskManagement(isPlannedReceive: false, isWorkingTask: true, isUnloadTask: true, isTaskAssignedToOtherUser: true);

		public void TestCheckReceiveASNQuantityMet_PlannedReceiveWithTask_WorkingTask_AssignedToCurrentUser()
			=> TestCheckReceiveASNQuantityMet_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskAssignedToOtherUser: false);

		void TestCheckReceiveASNQuantityMet_WithTaskManagement(bool isPlannedReceive, bool isWorkingTask, bool isUnloadTask, bool isTaskAssignedToOtherUser)
		{
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");
			Helper.Factory.Save();

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Helper.Factory.Save();

			PopulateASNLines(receive);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, isTaskAssignedToOtherUser ? otherStaff : staff);
			if (isWorkingTask)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			}
			if (!isUnloadTask)
			{
				task.P9_FormFlowType = string.Empty;
			}
			Helper.Factory.Save();

			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", isUnloadTask, task.P9_FormFlowType.EqualsIgnoringCase(WarehouseTaskFormFlowTypes.UnloadJob));
			AssertEquals("Precondition", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));

			var webService1 = GetNewWebService(data.Whs1, staff);
			var receiveLine = CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10m, "UNT", "", "", "", "", ZDate.Today, ZDate.Today, "", "PLT1", data.Whs1.DefaultLocation.ToLocationString());
			var responseUnload = webService1.UnloadWhsReceiveLines(new[] { receiveLine }, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(responseUnload, webService1);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var checkResponse = webService2.CheckReceiveASNQuantityMet(receive.PK.ToGuid());
			AssertSuccessfulResponse(checkResponse, webService2);

			var cannotSetUnloadCompleteTime = isPlannedReceive && isWorkingTask && isUnloadTask && isTaskAssignedToOtherUser;
			AssertEquals(cannotSetUnloadCompleteTime ? ErrorTypes.None : ErrorTypes.YesNoEnquiry, checkResponse.Error);
			AssertEquals(cannotSetUnloadCompleteTime ? null : $"Has the unload been completed for Receive {receive.WD_ExternalReference}?", checkResponse.ErrorMessage);
		}

		#region PopulateASNLines

		void PopulateASNLines(WhsReceive receive, bool hasUnloaded = false)
		{
			var expectAsnCount = receive.Lines.Count;
			// create ASN Lines for receive
			receive.PopulateASNLines();
			if (!hasUnloaded)
			{
				receive.Inventory.RemoveAndDeleteAll();
			}

			AssertEquals("Precondition:", expectAsnCount, receive.AsnLines.Count);
		}

		#endregion
	}
}
