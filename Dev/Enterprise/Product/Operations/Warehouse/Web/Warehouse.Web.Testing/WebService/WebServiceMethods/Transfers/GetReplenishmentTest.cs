using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetReplenishmentTest : WhsSecureServiceTestCase
	{
		#region TestGetReplenishment

		WhsTransfer SetupPickFaceReplenishmentTransfer(TestDataSimpleEnvironment data, bool withTransferLine)
		{
			var locationA1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			if (withTransferLine)
			{
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, pickfaceLocation);
			}
			transfer.RunPreSaveValidation();
			transfer.WD_IsPickFaceReplenishment = true;

			return transfer;
		}

		public void TestGetReplenishment()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var transfer = SetupPickFaceReplenishmentTransfer(data, true);
			Helper.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals("System should find transfer without external reference.", transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(1, response.Docket.Lines.Count);
		}

		public void TestGetReplenishment_TransferWithoutLine()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var transfer = SetupPickFaceReplenishmentTransfer(data, false);
			Helper.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals("The Error type should be right.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should get the right Error message", "Replenishment transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.", response.ErrorMessage);
		}

		public void TestGetReplenishment_Suspend_ThenFindByReference()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var transfer = SetupPickFaceReplenishmentTransfer(data, true);
			Helper.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals("System should find transfer without external reference.", transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(1, response.Docket.Lines.Count);

			var suspendTransferResponse = webService.SuspendPickingForTransfer(transfer.PK.ToGuid());
			AssertNull("Should has no error", suspendTransferResponse.ErrorMessage);
			AssertEquals("Validation error.", ErrorTypes.None, suspendTransferResponse.Error);

			response = webService.GetReplenishment("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, new[] { response.Docket.PK });
			AssertEquals(0, response.Docket.Lines.Count);

			response = webService.GetReplenishment(response.Docket.ExternalReference, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, new[] { response.Docket.PK });
			AssertEquals("System should find the transfer with the external reference.", transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(1, response.Docket.Lines.Count);
		}

		public void TestGetReplenishment_PickNum()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 4, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			var part3 = helper.CreateProduct(data.Org1, "P3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var pickfaceLocation1 = data.Whs1.FindLocation("A-4");

			helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation1, 0m, 50m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation1, 0m, 50m);
			helper.CreateProductPickFace(part3, data.Org1, pickfaceLocation1, 0m, 50m);

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locationA2, "");
			var receive3 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, locationA3, "");
			var receive4 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 10m, locationA1, "");
			var receive5 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 10m, locationA2, "");
			var receive6 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", part3, 10m, locationA1, "");
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m, WhsPickOption.Codes.Manual);
			var pick1 = helper.CreatePickNew(order1);
			pick1.WP_IsAwaitingReplenishment = true;

			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m, WhsPickOption.Codes.Manual);
			var pick2 = helper.CreatePickNew(order2);
			pick2.WP_IsAwaitingReplenishment = true;

			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", part3, 10m, WhsPickOption.Codes.Manual);
			var pick3 = helper.CreatePickNew(order3);
			pick3.WP_IsAwaitingReplenishment = true;

			var transfer1 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA1, pickfaceLocation1);
			var transferLine2 = helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA2, pickfaceLocation1);
			var transferLine3 = helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA3, pickfaceLocation1);
			transfer1.WD_IsPickFaceReplenishment = true;
			transfer1.RunPreSaveValidation();

			var transfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLine4 = helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, locationA1, pickfaceLocation1);
			var transferLine5 = helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, locationA2, pickfaceLocation1);
			transfer2.WD_IsPickFaceReplenishment = true;
			transfer2.RunPreSaveValidation();
			webService.Factory.Save();

			var response1 = webService.GetReplenishment(pick1.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);

			AssertEquals("System should find transfer for selected Pick Number", transfer1.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(3, response1.Docket.Lines.Count);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid()));

			var response2 = webService.GetReplenishment(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, null);

			AssertEquals("System should find transfer for selected Pick Number", transfer2.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(2, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine4.PK.ToGuid()));
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine5.PK.ToGuid()));

			var response3 = webService.GetReplenishment(pick3.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);

			AssertEquals("The Error type should be right for no Tranfer for selected Pick Number", ErrorTypes.BusinessValidationError, response3.Error);
			AssertEquals("Should get the right Error message", "Can't find Replenishment Transfer with Pick Number/Transfer ID: P00000003.", response3.ErrorMessage);
		}

		public void TestGetReplenishment_PickNum_DynamicPickFace()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);
			var staff = helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea = helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-3");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;
			webService.Factory.Save();

			var productParams1 = helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var productParams2 = helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, normalLocation, "");
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m, normalLocation, "");
			webService.Factory.Save();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m, WhsPickOption.Codes.Manual);
			var pick1 = helper.CreatePickNew(order1);
			pick1.WP_IsAwaitingReplenishment = true;
			pick1.WP_WW_Whs = data.Whs1.PK;
			pick1.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;

			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m, WhsPickOption.Codes.Manual);
			var pick2 = helper.CreatePickNew(order2);
			pick2.WP_IsAwaitingReplenishment = true;
			pick2.WP_WW_Whs = data.Whs1.PK;
			pick2.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;

			webService.Factory.Save();

			var transfer1 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer1.WD_IsPickFaceReplenishment = true;
			transfer1.WD_WP_PickBeingReplenished = pick1.PK;
			var transferLine1 = helper.CreateWhsTransferLine(transfer1, data.Part1, 30m, normalLocation, dynamicLocation);
			transfer1.RunPreSaveValidation();

			webService.Factory.Save();

			var transfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			transfer2.WD_IsPickFaceReplenishment = true;
			transfer2.WD_WP_PickBeingReplenished = pick2.PK;
			var transferLine2 = helper.CreateWhsTransferLine(transfer2, data.Part2, 20m, normalLocation, dynamicLocation);
			transfer2.RunPreSaveValidation();

			webService.Factory.Save();

			var response1 = webService.GetReplenishment(pick1.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);

			AssertEquals("System should find transfer for selected Pick Number", transfer1.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(1, response1.Docket.Lines.Count);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));

			var response2 = webService.GetReplenishment(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, null);

			AssertEquals("System should find transfer for selected Pick Number", transfer2.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(1, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
		}

		#endregion

		#region TestGetReplenishment_TaskManagement

		public void TestGetReplenishment_TaskManagementEnabled_NoProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			AssertNotEquals(TaskPlanningStatus.Codes.Planned, transfer.WD_TaskPlanningStatus);
			AssertEquals("Precondition", Guid.Empty, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", Guid.Empty, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", 0, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(WarehouseTaskFormFlowTypes.ReplenishmentJob, transferProcessTask.P9_FormFlowType);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask.P9_Status);
			AssertEquals("S2", transferProcessTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(transferProcessTask.PK, response.Docket.TaskPK);

			var transferLine1InNewFactory = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			AssertEquals(transferProcessTask.PK, transferLine1InNewFactory.WE_P9_Task);
			var transferLine2InNewFactory = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals(transferProcessTask.PK, transferLine2InNewFactory.WE_P9_Task);

			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals(TaskPlanningStatus.Codes.Planned, transferInNewFactory.WD_TaskPlanningStatus);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask.PK, response.Docket.TaskPK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasProcessTask_SomeLinesHasNoTaskAssigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = Guid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasProcessTask_LinesHasNoTaskAssigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = Guid.Empty;
			transferLine2.WE_P9_Task = Guid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertBusinessValidationError(webService, "Replenishment transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.", response);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasProcessTask_Assigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff1 = Helper.CreateGlbStaff("S2", "S2");
			var staff2 = Helper.CreateGlbStaff("S3", "S3");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff2);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertBusinessValidationError(webService, "This transfer is assigned to another user.", response);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasProcessTask_Completed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			var webServiceToComplete = GetNewWebService(data.Whs1, staff);
			webServiceToComplete.CompleteTask(transferProcessTask.PK.ToGuid());
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertBusinessValidationError(webService, "This transfer is already completed.", response);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasMultipleProcessTasks_MultipleAssignedToUser_TakesSuspendedFirst()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			var webServiceToComplete = GetNewWebService(data.Whs1, staff);
			webServiceToComplete.SuspendTask(transferProcessTask1.PK.ToGuid());
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Suspended, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasMultipleProcessTasks_MultipleAssignedToUser_TakesAssignedOverOpen()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasMultipleProcessTasks_NothingAssigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer);
			transferProcessTask1.P9_SystemCreateTimeUtc = DateTime.Now.AddDays(-1);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferProcessTask2.P9_SystemCreateTimeUtc = DateTime.Now;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasMultipleProcessTasks_AllAssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff1 = Helper.CreateGlbStaff("S3", "S3");
			var otherStaff2 = Helper.CreateGlbStaff("S4", "S4");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff1);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff2);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertBusinessValidationError(webService, "This transfer is assigned to another user.", response);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);
			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_HasMultipleProcessTasks_OtherLinesWithProcessTaskAssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementNotEnabled_HasProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementNotEnabled_HasProcessTask_AssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff1 = Helper.CreateGlbStaff("S3", "S3");
			var otherStaff2 = Helper.CreateGlbStaff("S4", "S4");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff1);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff2);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);
			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementNotEnabled_HasProcessTask_Completed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			var webServiceToCompleteTask = GetNewWebService(data.Whs1, staff);
			webServiceToCompleteTask.CompleteTask(transferProcessTask.PK.ToGuid());
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementNotEnabled_HasProcessTask_LinesWithNoTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = Guid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementNotEnabled_HasProcessTask_SomeLinesAssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetReplenishment(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);
			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
				AssertEquals(2, response.Docket.Lines.Count);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_ErrorReturnedFromTaskManagementService()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult("Error Here"));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Error Here", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_SomeLinesOnAnotherTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var task2 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = task1.PK;
			transferLine2.WE_P9_Task = task2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", task1.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task2.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals("Precondition", 2, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
				AssertEquals(data.Part1.PK, response.Docket.Lines.Single().Product.PK);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(task1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_NoLinesToTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var task2 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = task2.PK;
			transferLine2.WE_P9_Task = task2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", task2.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task2.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals("Precondition", 2, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(task1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_NoAssociatedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(ZGuid.BrettsGuid.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_TaskParentIsNotTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var receiveTask = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, receiveTask.P9_Status);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(receiveTask.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var receiveProcessTaskInNewFactory = newFactory.Load<ProcessTask>(receiveTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, receiveProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_TaskParentIsNull()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var dummyTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			dummyTask.P9_ParentID = ZGuid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", dummyTask.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", dummyTask.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, dummyTask.P9_Status);
			AssertEquals("Precondition", 0, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(dummyTask.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var dummyTaskInNewFactory = newFactory.Load<ProcessTask>(dummyTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, dummyTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_FinalisedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);
			transfer.WD_IsPickFaceReplenishment = true;

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_DockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

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

			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_InvalidProcessFlowType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.LoadJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_InterWhsChild_Source() => TestGetReplenishment_TaskManagementEnabled_EmptyReference_InterWhsChild(isSource: true);
		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_InterWhsChild_Dest() => TestGetReplenishment_TaskManagementEnabled_EmptyReference_InterWhsChild(isSource: false);

		void TestGetReplenishment_TaskManagementEnabled_EmptyReference_InterWhsChild(bool isSource)
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
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

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Replenishment transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_TaskIsClosed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
				})
				.Returns(UpdateTaskStatusResult.TaskStatusIsCompleted);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "The task is already closed and cannot be updated. Please check the task status and try again.", response);
			}
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_TaskIsAssignedToAnotherUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_GS_NKAssignedStaffMember = "S3";
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.ReplenishmentJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					AssertEquals("S3", task.P9_GS_NKAssignedStaffMember);
				})
				.Returns(UpdateTaskStatusResult.AssignedUserIsDifferent);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "This task is not assigned to the current user. Please perform a different task.", response);
			}
		}

		public void TestGetReplenishment_TaskManagementEnabled_EmptyReference_ConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 5m, 15m);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.WD_IsPickFaceReplenishment = true;
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.ReplenishmentJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var innerException = new Exception();
				var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)task).Row, Db.Connection);
				webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

				var response = webService.GetReplenishment("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, null);
				AssertBusinessValidationError(webService, "Another user has changed the Transfer Job while you have been working on it. Please restart the operation and try again.", response);
			}
		}

		#endregion
	}
}
