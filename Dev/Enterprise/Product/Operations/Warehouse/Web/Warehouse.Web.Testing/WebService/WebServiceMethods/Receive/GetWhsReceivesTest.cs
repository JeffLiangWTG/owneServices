using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetWhsReceivesTest : WhsSecureServiceTestCase
	{
		#region GetWhsReceives

		#region TestGetWhsReceives_WithEmptyRef

		public void TestGetWhsReceives_WithEmptyRef()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref");
			CreateContainerForDocket(receive1, "Ref");
			CreateReferenceForDocket(receive1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			CreateContainerForDocket(receive2, "Ref");
			CreateReferenceForDocket(receive2, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertBusinessValidationError(webService, "Please provide Receive reference.", webService.GetWhsReceives("", true));
		}

		public void TestGetWhsReceives_WithEmptyRef_TaskManagementEnabled()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("Red", "Red");

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			var task = Helper.CreateProcessTaskForReceive(receive1, staff);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var taskManagementServiceMock = new Mock<IWhsTaskManagementService>();

			var taskResult = new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.UnloadJob);
			taskManagementServiceMock.Setup(
				t => t.GetNextTask(
					webService.Factory,
					string.Empty,
					staff.PK.ToGuid(),
					data.Whs1.PK.ToGuid(),
					WarehouseTaskFormFlowTypes.UnloadJob,
					string.Empty,
					null)
				).Returns(taskResult);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				AssertCorrectDocketIsReturned(webService, "", receive1, task);
				taskManagementServiceMock.Verify(
					t => t.GetNextTask(
						webService.Factory,
						string.Empty,
						staff.PK.ToGuid(),
						data.Whs1.PK.ToGuid(),
						WarehouseTaskFormFlowTypes.UnloadJob,
						string.Empty,
						null));
				taskManagementServiceMock.Verify(t => t.SetTaskToPlayIfValid(It.IsAny<WhsReceiveProcessTasks>(), "WUL", "Red"));
				taskManagementServiceMock.VerifyNoOtherCalls();
			}
		}

		public void TestLoadExistingReceiveJob_EmptyReceiveReference_TaskManagementEnabled_Error()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("Red", "Red");

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			var task = Helper.CreateProcessTaskForReceive(receive1, staff);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var taskManagementServiceMock = new Mock<IWhsTaskManagementService>();

			var taskResult = new GetNextTaskResult("Staff could not be found. Please try again.");
			taskManagementServiceMock.Setup(
				t => t.GetNextTask(
					webService.Factory,
					string.Empty,
					staff.PK.ToGuid(),
					data.Whs1.PK.ToGuid(),
					WarehouseTaskFormFlowTypes.UnloadJob,
					string.Empty,
					null)
				).Returns(taskResult);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				AssertBusinessValidationError(webService, "Staff could not be found. Please try again.", webService.GetWhsReceives("", true));
				taskManagementServiceMock.Verify(
					t => t.GetNextTask(
						webService.Factory,
						string.Empty,
						staff.PK.ToGuid(),
						data.Whs1.PK.ToGuid(),
						WarehouseTaskFormFlowTypes.UnloadJob,
						string.Empty,
						null));
				taskManagementServiceMock.VerifyNoOtherCalls();
			}
		}

		public void TestGetWhsReceives_WithEmptyRef_TaskManagementEnabled_IsInvokedInTemporaryUserContext()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("Red", "Red");

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			var task = Helper.CreateProcessTaskForReceive(receive1, staff);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var glowUserDataManagerMock = new Mock<IGlowUserDataManager>();
			var disposableMock = new Mock<IDisposable>();
			glowUserDataManagerMock.Setup(g => g.IncreaseTempUserCount()).Returns(disposableMock.Object);

			var envBranch = GlbBranch.CurrentBranch.PK.ToGuid();
			var envDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();

			var webService = GetNewWebService(data.Whs1, staff);
			var taskManagementServiceMock = new Mock<IWhsTaskManagementService>();

			var taskResult = new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.UnloadJob);
			taskManagementServiceMock.Setup(
				t => t.GetNextTask(
					webService.Factory,
					string.Empty,
					staff.PK.ToGuid(),
					data.Whs1.PK.ToGuid(),
					WarehouseTaskFormFlowTypes.UnloadJob,
					string.Empty,
					null)
				)
				.Callback(() =>
				{
					glowUserDataManagerMock.Verify(g => g.IncreaseTempUserCount());

					AssertEquals(staff.PK, GlbStaff.CurrentUser.PK);
					AssertEquals(envBranch, GlbBranch.CurrentBranch.PK);
					AssertEquals(envDepartment, GlbDepartment.CurrentDepartment.PK);
				})
				.Returns(taskResult);

			using (ObjectFactory.Substitute("IGlowServiceClientFactory", glowUserDataManagerMock.Object))
			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				AssertCorrectDocketIsReturned(webService, "", receive1, task);
				taskManagementServiceMock.Verify(
					t => t.GetNextTask(
						webService.Factory,
						string.Empty,
						staff.PK.ToGuid(),
						data.Whs1.PK.ToGuid(),
						WarehouseTaskFormFlowTypes.UnloadJob,
						string.Empty,
						null));
				taskManagementServiceMock.Verify(t => t.SetTaskToPlayIfValid(It.IsAny<WhsReceiveProcessTasks>(), "WUL", "Red"));
				taskManagementServiceMock.VerifyNoOtherCalls();

				disposableMock.Verify(d => d.Dispose());
			}
		}

		#endregion

		#region TestGetWhsReceives_WithEmptyRef_ReturnTask

		public void TestLoadExistingReceiveJob_EmptyReceiveReference_ReturnTaskAssignedToUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("Red", "Red");
			var staff2 = Helper.CreateGlbStaff("Grn", "Grn");

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			var task1 = Helper.CreateProcessTaskForReceive(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			var task2 = Helper.CreateProcessTaskForReceive(receive2, staff1);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref3");
			Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m);
			var task3 = Helper.CreateProcessTaskForReceive(receive3, staff2);
			Helper.Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.P9_GS_NKAssignedStaffMember = string.Empty;
			Helper.Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("OPN", task1.P9_Status);
				AssertEquals(string.Empty, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals("ASN", task2.P9_Status);
				AssertEquals("Red", task2.P9_GS_NKAssignedStaffMember);
				AssertEquals("ASN", task3.P9_Status);
				AssertEquals("Grn", task3.P9_GS_NKAssignedStaffMember);
			});

			var webService = GetNewWebService(data.Whs1, staff1);
			var taskManagementServiceMock = new Mock<IWhsTaskManagementService>();

			var taskResult = new GetNextTaskResult(task2.PK.ToGuid(), WarehouseTaskFormFlowTypes.UnloadJob);
			taskManagementServiceMock.Setup(
				t => t.GetNextTask(
					webService.Factory,
					string.Empty,
					staff1.PK.ToGuid(),
					data.Whs1.PK.ToGuid(),
					WarehouseTaskFormFlowTypes.UnloadJob,
					string.Empty,
					null)
			).Returns(taskResult);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				AssertCorrectDocketIsReturned(webService, "", receive2, task2);
				taskManagementServiceMock.Verify(
					t => t.GetNextTask(
						webService.Factory,
						string.Empty,
						staff1.PK.ToGuid(),
						data.Whs1.PK.ToGuid(),
						WarehouseTaskFormFlowTypes.UnloadJob,
						string.Empty,
						null));
				taskManagementServiceMock.Verify(t => t.SetTaskToPlayIfValid(It.IsAny<WhsReceiveProcessTasks>(), "WUL", "Red"));
				taskManagementServiceMock.VerifyNoOtherCalls();
			}
		}

		public void TestLoadExistingReceiveJob_EmptyReceiveReference_ReturnOpenTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("Red", "Red");
			var staff2 = Helper.CreateGlbStaff("Grn", "Grn");

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			var task1 = Helper.CreateProcessTaskForReceive(receive1, staff2);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			var task2 = Helper.CreateProcessTaskForReceive(receive2);
			Helper.Factory.Save();

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.P9_GS_NKAssignedStaffMember = string.Empty;
			Helper.Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("ASN", task1.P9_Status);
				AssertEquals("Grn", task1.P9_GS_NKAssignedStaffMember);
				AssertEquals("OPN", task2.P9_Status);
				AssertEquals(string.Empty, task2.P9_GS_NKAssignedStaffMember);
			});

			var webService = GetNewWebService(data.Whs1, staff1);
			var taskManagementServiceMock = new Mock<IWhsTaskManagementService>();

			var taskResult = new GetNextTaskResult(task2.PK.ToGuid(), WarehouseTaskFormFlowTypes.UnloadJob);
			taskManagementServiceMock.Setup(
				t => t.GetNextTask(
					webService.Factory,
					string.Empty,
					staff1.PK.ToGuid(),
					data.Whs1.PK.ToGuid(),
					WarehouseTaskFormFlowTypes.UnloadJob,
					string.Empty,
					null)
			).Returns(taskResult);

			taskManagementServiceMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<WhsReceiveProcessTasks>(), "WUL", "Red"))
				.Callback((IProcessTask task, string expectedRFType, string staffCode) =>
				{
					task.P9_GS_NKAssignedStaffMember = staffCode;
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				});

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				AssertCorrectDocketIsReturned(webService, "", receive2, task2);
				taskManagementServiceMock.Verify(
					t => t.GetNextTask(
						webService.Factory,
						string.Empty,
						staff1.PK.ToGuid(),
						data.Whs1.PK.ToGuid(),
						WarehouseTaskFormFlowTypes.UnloadJob,
						string.Empty,
						null));
				taskManagementServiceMock.Verify(t => t.SetTaskToPlayIfValid(It.IsAny<WhsReceiveProcessTasks>(), "WUL", "Red"));
				taskManagementServiceMock.VerifyNoOtherCalls();
			}
		}

		#endregion

		#region TestGetWhsReceives_RefCheckSequence

		public void TestGetWhsReceives_RefCheckSequence()
		{
			// Reference sequence: WD_ExternalReference > WX_Reference > WC_ContainerNum
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef1");
			CreateReferenceForDocket(receive1, "Ref1");
			CreateContainerForDocket(receive1, "Container1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef2");
			CreateReferenceForDocket(receive2, "Ref2");
			CreateContainerForDocket(receive2, "Container2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef3");
			CreateReferenceForDocket(receive3, "Ref3");
			CreateContainerForDocket(receive3, "Container3");
			Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, "ExtRef1", receive1);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService2, "Ref2", receive2);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService3, "Container3", receive3);

			var webService4 = GetNewWebService();
			webService4.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService4, "Ref");
		}

		public void TestGetWhsReceives_RefCheckSequence_ExternalReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref");
			CreateReferenceForDocket(receive1, "Ref");
			CreateContainerForDocket(receive1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef2");
			CreateReferenceForDocket(receive2, "Ref");
			CreateContainerForDocket(receive2, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, "Ref", receive1);

			receive1.WD_ExternalReference = "Smth New";
			Helper.Factory.Save();
			receive2.WD_ExternalReference = "Ref";
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService2, "Ref", receive2);
		}

		public void TestGetWhsReceives_RefCheckSequence_References()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef1");
			var reference1 = CreateReferenceForDocket(receive1, "Ref");
			CreateContainerForDocket(receive1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef2");
			CreateReferenceForDocket(receive2, "Ref");
			CreateContainerForDocket(receive2, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, "Ref", receive1);

			reference1.WX_Reference = "Smth New";
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService2, "Ref", receive2);
		}

		public void TestGetWhsReceives_RefCheckSequence_ContainerNum()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef1");
			CreateReferenceForDocket(receive1, "Ref1");
			var container1 = CreateContainerForDocket(receive1, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef2");
			CreateReferenceForDocket(receive2, "Ref2");
			CreateContainerForDocket(receive2, "Ref");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, "Ref", receive1);

			container1.WC_ContainerNum = "Smth New";
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService2, "Ref", receive2);
		}

		public void TestGetWhsReceives_RefCheckSequence_ExternalReference_DocketID()
		{
			// Reference sequence: WD_ExternalReference > WD_DocketID
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef2");
			receive2.WD_ExternalReference = receive1.WD_DocketID;
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			AssertCorrectDocketIsReturned(webService1, receive1.WD_DocketID, receive2);
		}

		#endregion

		#region TestGetWhsReceives_ByContainerNo

		#region TestGetWhsReceives_ByContainerNo

		public void TestGetWhsReceives_ByContainerNo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateContainerForDocket(receive1, "Con1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			CreateContainerForDocket(receive1, "Con2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService, "Con1", receive1);
		}

		#endregion

		#region TestGetWhsReceives_ByContainerNo_InMultipleWarehouses

		public void TestGetWhsReceives_ByContainerNo_InMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			warehouse2.WW_WarehouseCode = "WH2";
			Helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateContainerForDocket(receive1, "Con1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, warehouse2, "Ref2");
			CreateContainerForDocket(receive2, "Con2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, "Con1", receive1);
			AssertNoDocketIsReturned(webService1, "Con2");

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = warehouse2.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService2, "Con2", receive2);
		}

		#endregion

		#region TestGetWhsReceives_ByContainerNo_ReceiveIsFinalised

		public void TestGetWhsReceives_ByContainerNo_ReceiveIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateContainerForDocket(receive, "Con1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, "Con1");
		}

		#endregion

		#region TestGetWhsReceives_ByContainerNo_ContainerNoIsNotExisted

		public void TestGetWhsReceives_ByContainerNo_ContainerNoIsNotExisted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateContainerForDocket(receive, "Con1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, "NONEXISTINGCONNO");
		}

		#endregion

		#region TestGetWhsReceives_ByContainerNo_LoadOnlyReceives

		public void TestGetWhsReceives_ByContainerNo_LoadOnlyReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "123", Notify);
			CreateContainerForDocket(order, "CNT001");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var response = webService.GetWhsReceives("CNT001", true);
				AssertSuccessfulResponse(response, webService);
			});
		}

		#endregion

		#endregion

		#region TestGetWhsReceives_PartAttributes

		public void TestGetWhsReceives_PartAttributes()
		{
			var response = new WhsDocketsWebServiceResponse();
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			response = webService.GetWhsReceives("Ref", true);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Dockets);
			var docket = response.Dockets.Single();
			AssertEquals(receive.PK.ToGuid(), docket.PK);
			AssertEquals(receive.WD_DocketID, docket.DocketID);
			AssertNotNull(docket.PartAttributes);
			AssertEquals("Attr1", docket.PartAttributes.Attribute1Caption);
			AssertEquals(true, docket.PartAttributes.Attribute1IsMandatory);
			AssertEquals("Attr2", docket.PartAttributes.Attribute2Caption);
			AssertEquals(false, docket.PartAttributes.Attribute2IsMandatory);
			AssertEquals("", docket.PartAttributes.Attribute3Caption);
			AssertEquals(false, docket.PartAttributes.Attribute3IsMandatory);
		}

		#endregion

		#region TestGetWhsReceive_ByPK

		public void TestGetWhsReceive_ByPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateReferenceForDocket(receive1, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			CreateReferenceForDocket(receive2, "HAWB2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceive(receive1.PK.ToGuid(), true);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);
			var docket = response.Dockets.Single();
			AssertEquals("DocketID should be equal", receive1.WD_DocketID, docket.DocketID);
			AssertEquals("TaskPK should be empty", ZGuid.Empty, docket.TaskPK);
		}

		#region TestGetWhsReceive_ByPK_CheckDockDoorLocations

		public void TestGetWhsReceive_ByPK_CheckDockDoorLocations_SingleDDL_IsUnload()
		{
			TestGetWhsReceive_ByPK_CheckDockDoorLocations_SingleDDLCore(true);
		}

		public void TestGetWhsReceive_ByReference_CheckDockDoorLocations_SingleDDL_IsNotUnload()
		{
			TestGetWhsReceive_ByPK_CheckDockDoorLocations_SingleDDLCore(false);
		}

		void TestGetWhsReceive_ByPK_CheckDockDoorLocations_SingleDDLCore(bool isUnloadProcess)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceive(receive.PK.ToGuid(), isUnloadProcess);

			var expectedDDL = isUnloadProcess ? data.Whs1.DefaultInboundDockDoorLocation : null;
			AssertEquals("Response should only check DDL during Unload.", isUnloadProcess, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should retrieve appropriate DDL PK.", expectedDDL?.PK ?? Guid.Empty, response.SingleDockDoorLocationPK);
			AssertEquals("Response should retrieve appropriate DDL string.", expectedDDL?.WLV_LocationString, response.SingleDockDoorLocation);
		}

		public void TestGetWhsReceive_ByPK_CheckDockDoorLocations_MultipleDDL()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("TS1", "Test1", false, 0, LocationClasses.Codes.DDL);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[2].WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceive(receive.PK.ToGuid(), true);

			AssertEquals("Response should only return false if multiple DDL.", false, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should not populate DDL PK if multiple DDL.", Guid.Empty, response.SingleDockDoorLocationPK);
			AssertEquals("Response should not populate DDL string if multiple DDL.", null, response.SingleDockDoorLocation);
		}

		#endregion

		#region TestGetWhsReceive_ByPK_InMultipleWarehouses

		public void TestGetWhsReceive_ByPK_InMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			warehouse2.WW_WarehouseCode = "WH2";
			Helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateReferenceForDocket(receive1, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, warehouse2, "Ref2");
			CreateReferenceForDocket(receive2, "HAWB2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsReceive(receive1.PK.ToGuid(), true);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Dockets should not be null", response1.Dockets);
			AssertEquals("DocketID should be equal", receive1.WD_DocketID, response1.Dockets.Single().DocketID);

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetWhsReceive(receive2.PK.ToGuid(), true);
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull("Dockets should not be null", response2.Dockets);
			AssertEquals("No dockets be return", 0, response2.Dockets.Length);

			var webService3 = GetNewWebService(warehouse2);
			var response3 = webService3.GetWhsReceive(receive2.PK.ToGuid(), true);
			AssertSuccessfulResponse(response3, webService3);
			AssertNotNull("Dockets should not be null", response3.Dockets);
			AssertEquals("DocketID should be equal", receive2.WD_DocketID, response3.Dockets.Single().DocketID);
		}

		#endregion

		#region TestGetWhsReceive_ByPK_ReceiveIsFinalised

		public void TestGetWhsReceive_ByPK_ReceiveIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceive(receive.PK.ToGuid(), true);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);
			AssertEquals("No dockets be return", 0, response.Dockets.Length);
		}

		#endregion

		#region TestGetWhsReceive_ByPK_PKIsNotExisted

		public void TestGetWhsReceive_ByPK_PKIsNotExisted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceive(ZGuid.NewZGuid().ToGuid(), true);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);
			AssertEquals("No dockets be return", 0, response.Dockets.Length);
		}

		#endregion

		#endregion

		#region TestGetWhsReceives_ByReference

		#region TestGetWhsReceives_ByReference

		public void TestGetWhsReceives_ByReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			CreateReferenceForDocket(receive1, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			CreateReferenceForDocket(receive2, "HAWB2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService, "HAWB1", receive1);
		}

		#endregion

		#region TestGetWhsReceives_ByReference_CheckDockDoorLocations

		public void TestGetWhsReceives_ByReference_CheckDockDoorLocations_SingleDDL_IsUnload()
		{
			TestGetWhsReceives_ByReference_CheckDockDoorLocations_SingleDDLCore(true);
		}

		public void TestGetWhsReceives_ByReference_CheckDockDoorLocations_SingleDDL_IsNotUnload()
		{
			TestGetWhsReceives_ByReference_CheckDockDoorLocations_SingleDDLCore(false);
		}

		void TestGetWhsReceives_ByReference_CheckDockDoorLocations_SingleDDLCore(bool isUnloadProcess)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives("HAWB1", isUnloadProcess);

			var expectedDDL = isUnloadProcess ? data.Whs1.DefaultInboundDockDoorLocation : null;
			AssertEquals("Response should only check DDL during Unload.", isUnloadProcess, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should retrieve appropriate DDL PK.", expectedDDL?.PK ?? Guid.Empty, response.SingleDockDoorLocationPK);
			AssertEquals("Response should retrieve appropriate DDL string.", expectedDDL?.WLV_LocationString, response.SingleDockDoorLocation);
		}

		public void TestGetWhsReceives_ByReference_CheckDockDoorLocations_MultipleDDL()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("TS1", "Test1", false, 0, LocationClasses.Codes.DDL);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[2].WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives("HAWB1", true);

			AssertEquals("Response should only return false if multiple DDL.", false, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should not populate DDL PK if multiple DDL.", Guid.Empty, response.SingleDockDoorLocationPK);
			AssertEquals("Response should not populate DDL string if multiple DDL.", null, response.SingleDockDoorLocation);
		}

		#endregion

		#region TestGetWhsReceives_ByReference_InMultipleWarehouses

		public void TestGetWhsReceives_ByReference_InMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			warehouse2.WW_WarehouseCode = "WH2";
			Helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			CreateReferenceForDocket(receive1, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, warehouse2, "Ref2");
			CreateReferenceForDocket(receive2, "HAWB2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, "HAWB1", receive1);
			AssertNoDocketIsReturned(webService1, "HAWB2");

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = warehouse2.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService2, "HAWB2", receive2);
		}

		#endregion

		#region TestGetWhsReceives_ByReference_ReceiveIsFinalised

		public void TestGetWhsReceives_ByReference_ReceiveIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, "HAWB1");
		}

		#endregion

		#region TestGetWhsReceives_ByReference_ReferenceIsNotExisted

		public void TestGetWhsReceives_ByReference_ReferenceIsNotExisted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateReferenceForDocket(receive, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, "NONEXISTINGREFERENCE");
		}

		#endregion

		#region TestGetWhsReceives_ByReference_RecentDockDoorLocation

		[TestDate(2016, 02, 16)]
		public void TestGetWhsReceives_ByReference_RecentDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);

			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1"); // No Recent Dock Door location
			var lineInReceive1WithNoDockDoorLocation = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2"); // Single Dock Door locations
			var receive2InventoryLine1 = CreateLineAndAssignDockDoor(receive2, data.Part1, dockDoorLocation1, ZDateTime.Now);
			var lineInReceive2WithNoDockDoorLocation = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref3"); // Multiple Dock Door locations
			var receive3InventoryLine1 = CreateLineAndAssignDockDoor(receive3, data.Part1, dockDoorLocation2, ZDateTime.Now.AddDays(-2));
			var receive3InventoryLine2 = CreateLineAndAssignDockDoor(receive3, data.Part1, dockDoorLocation1, ZDateTime.Now.AddDays(-1));
			var lineInReceive3WithNoDockDoorLocation = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertResponseForRecentlyUsedDockDoorLocation(webService1, receive1, "Ref1", "", ZGuid.Empty);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertResponseForRecentlyUsedDockDoorLocation(webService2, receive2, "Ref2", "A-1", dockDoorLocation1.PK);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertResponseForRecentlyUsedDockDoorLocation(webService3, receive3, "Ref3", "A-1", dockDoorLocation1.PK);
		}

		void AssertResponseForRecentlyUsedDockDoorLocation(WhsSecureService webService, WhsReceive expectedReceive, string referenceNumber,
			string expectedDockDoorLocationString, ZGuid expectedDockDoorLocationPK)
		{
			webService.AllowedToRunServiceHasBeenCalled = false;
			var response = webService.GetWhsReceives(referenceNumber, true);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(expectedReceive.PK.ToGuid(), response.Dockets[0].PK);
			AssertEquals(expectedDockDoorLocationString, response.Dockets[0].RecentlyUsedDockDoorLocation);
			AssertEquals(expectedDockDoorLocationPK, response.Dockets[0].RecentlyUsedDockDoorLocationPK);
		}

		WhsInventoryView CreateLineAndAssignDockDoor(WhsReceive receive, OrgSupplierPart part, WhsLocation dockDoorLocation, ZDateTime logAddedTime)
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			inventory.InDocketLine.WE_WL = dockDoorLocation.PK;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			inventory.InDocketLine.Logs.AddNew(Events.AddedARecordToTheSystem, "RF", logAddedTime.ToOffset());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			return inventory;
		}

		public void TestGetWhsReceives_ByReference_RecentDockDoorLocation_PutawayLocationOnLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");
			AssertEquals("Precondition", false, location.IsDockDoorLocation);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			inventory.InDocketLine.Logs.AddNew(Events.AddedARecordToTheSystem, "RF", ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceives("R1", true);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(receive.PK.ToGuid(), response.Dockets[0].PK);
			AssertEquals("No dockdoor location is returned as location on receive line is not a dockdoor location.", "", response.Dockets[0].RecentlyUsedDockDoorLocation);
			AssertEquals("No dockdoor location is returned as location on receive line is not a dockdoor location.", ZGuid.Empty, response.Dockets[0].RecentlyUsedDockDoorLocationPK);
		}

		public void TestGetWhsReceives_ByReference_RecentDockDoorLocation_LogIsEditedARecord()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, dockDoorLocation);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			inventory.InDocketLine.Logs.AddNew(Events.EditedARecord, "RF", ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceives("R1", true);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(receive.PK.ToGuid(), response.Dockets[0].PK);
			AssertEquals("Dockdoor location is returned.", "A-1", response.Dockets[0].RecentlyUsedDockDoorLocation);
			AssertEquals("Dockdoor location is returned.", dockDoorLocation.PK, response.Dockets[0].RecentlyUsedDockDoorLocationPK);
		}

		public void TestGetWhsReceives_ByReference_RecentDockDoorLocation_LinesWithAddedAndEditedARecordLogs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, dockDoorLocation1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, dockDoorLocation2);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			inventory1.InDocketLine.Logs.AddNew(Events.EditedARecord, "RF", ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			inventory2.InDocketLine.Logs.AddNew(Events.AddedARecordToTheSystem, "RF", ZDateTimeOffset.Now.AddDays(-2));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceives("R1", true);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(receive.PK.ToGuid(), response.Dockets[0].PK);
			AssertEquals("Dockdoor location is returned.", "A-1", response.Dockets[0].RecentlyUsedDockDoorLocation);
			AssertEquals("Dockdoor location is returned.", dockDoorLocation1.PK, response.Dockets[0].RecentlyUsedDockDoorLocationPK);
		}

		public void TestGetWhsReceives_RecentDockDoorLocation_FixedWidthLocation()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			factory.Save();

			var dockdoorLocationType = factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			var location1 = warehouse.FindLocation("Z030201");
			location1.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location1.WLV_LocationStatus = "NOR";
			var location2 = warehouse.FindLocation("Z040302");
			location2.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location2.WLV_LocationStatus = "NOR";
			factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, warehouse, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			inventory1.InDocketLine.Logs.AddNew(Events.EditedARecord, "RF", ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			inventory2.InDocketLine.Logs.AddNew(Events.AddedARecordToTheSystem, "RF", ZDateTimeOffset.Now.AddDays(-2));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.GetWhsReceives("R1", true);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(receive.PK.ToGuid(), response.Dockets[0].PK);
			AssertEquals("Dockdoor location is returned.", "Z030201", response.Dockets[0].RecentlyUsedDockDoorLocation);
			AssertEquals("Dockdoor location is returned.", "Z-03-02-01", response.Dockets[0].RecentlyUsedDockDoorLocation_UserFriendly);
			AssertEquals("Dockdoor location is returned.", location1.PK, response.Dockets[0].RecentlyUsedDockDoorLocationPK);
		}

		#endregion

		#region TestGetWhsReceives_PopulateASNLines

		public void TestGetWhsReceives_PopulateASNLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			webService.GetWhsReceives("Ref1", true);
			AssertEquals("ASN lines are created.", 1, receive.AsnLines.Count);
		}

		public void TestGetWhsReceives_PopulateASNLines_NoneCreatedForReceives_WithAddedARecordToTheSystemLogOnReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receiveHasLog = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			var receiveHasLogInventoryLine = CreateLineAndAssignDockDoor(receiveHasLog, data.Part1, dockDoorLocation, ZDateTime.Now);
			AssertNotNull(WebServiceHelper.FindExistingLog(receiveHasLogInventoryLine.InDocketLine, AutoEvents.AddedARecordToTheSystem, "RF")); //Precondition
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			webService.GetWhsReceives("Ref2", true);
			AssertEquals("No ASN lines created correctly", 0, receiveHasLog.AsnLines.Count);
		}

		public void TestGetWhsReceives_PopulateASNLines_NoneCreatedForReceiveWithEditedARecordLogOnReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");

			var receiveHasLog = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receiveHasLog.Logs.AddNew(Events.EditedARecord, "RF", DateTime.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Helper.CreateWhsReceiveLine(receiveHasLog, data.Part1, 10m);
			AssertNotNull(WebServiceHelper.FindExistingLog(receiveHasLog, AutoEvents.EditedARecord, "RF")); //Precondition
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			webService.GetWhsReceives("Ref2", true);
			AssertEquals("No ASN lines are created.", false, receiveHasLog.AsnLines.Any());
		}

		public void TestGetWhsReceives_PopulateASNLines_NoneCreatedForReceiveWithPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.DefaultLocation;

			var receivePutawayTransfer = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref3");
			Helper.CreateWhsReceiveInventoryLine(receivePutawayTransfer, data.Part2, 10m, dockDoorLocation, "A");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, dockDoorLocation, normalLocation, "A", 10m);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			webService.GetWhsReceives("Ref3", true);
			AssertEquals("No ASN lines are created.", false, receivePutawayTransfer.AsnLines.Any());
		}

		public void TestGetWhsReceives_PopulateASNLines_NoneCreatedForReceiveCreatedFromWebService()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.CreateNewWhsReceive("R1", data.Org1.OH_Code, DateTime.Today, ReceiveType.Codes.Receipt);

			var receive = Helper.Factory.Load<WhsReceive>(response1.Docket.PK);
			AssertNotNull("Precondition: Receive created from web service call.", receive);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Precondition: Receive has lines.", true, receive.Lines.Any());
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			webService2.GetWhsReceives("R1", true);
			AssertEquals("No ASN lines are created.", false, receive.AsnLines.Any());
			AssertEquals("Existing receive line retains transaction quantity.", 10m, receiveLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestGetWhsReceives_ByReference_LoadOnlyReceives

		public void TestGetWhsReceives_ByReference_LoadOnlyReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "123", Notify);
			CreateReferenceForDocket(order, "REF001");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var response = webService.GetWhsReceives("REF001", true);
				AssertSuccessfulResponse(response, webService);
			});
		}

		#endregion

		#endregion

		#region TestGetWhsReceives_LoadUsedSerialNumbers

		public void TestGetWhsReceives_LoadUsedSerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			receive.PopulateASNHasBeenCalled_TestsOnly = false;
			var docketResponse1 = webService.GetWhsReceives("Rev1", true);

			AssertSuccessfulResponse(docketResponse1, webService);
			AssertNotNull(docketResponse1.Dockets);
			AssertEquals(receive.PK.ToGuid(), docketResponse1.Dockets[0].PK);
			AssertEquals(0, docketResponse1.Dockets[0].Lines.Count);
			AssertEquals(0, docketResponse1.Dockets[0].UsedSerialNumbers.Length);       // no usedSerialNumbers yet

			var year = ZDateTime.Today.Year - 1;
			// create 1 line through RF, and serial number SN01
			var service2 = GetNewWebService();
			service2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var inventoryResponse = service2.UnloadWhsReceiveLines(new[] {
					CreateWhsDocketLineInfo(receive.PK.ToGuid(), data.Part1.OP_PartNum, 1, "KG", "NonMandatoryAttribute", "NonMandatoryAttribute", "", "SN01", new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "")
				}, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(inventoryResponse, service2);
			AssertEquals(ErrorTypes.None, inventoryResponse.Error);
			AssertEquals(true, string.IsNullOrEmpty(inventoryResponse.ErrorMessage));
			AssertUnloadWhsReceiveLine(webService.Factory, Helper, inventoryResponse.InventoryLinePK, "P1", 2m, "KG", "NONMANDATORYATTRIBUTE", "NONMANDATORYATTRIBUTE", "", "SN01",
				new ZDate(year, 10, 22), new ZDate(year, 10, 23), "", "123", "", receive.PK, 1, 1m, "UNT", null);

			// load receive again
			var service3 = GetNewWebService();
			service3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			// so we have to recreate factory here.
			var docketResponse2 = service3.GetWhsReceives("Rev1", true);
			AssertSuccessfulResponse(docketResponse2, service3);
			AssertNotNull(docketResponse2.Dockets);
			AssertEquals(receive.PK.ToGuid(), docketResponse2.Dockets[0].PK);
			AssertEquals(0, docketResponse2.Dockets[0].Lines.Count);                 // still do not load existing line
			AssertEquals(1, docketResponse2.Dockets[0].UsedSerialNumbers.Length);
			AssertEquals("SN01|P1", docketResponse2.Dockets[0].UsedSerialNumbers[0]);
		}

		public void TestGetWhsReceives_LoadUsedSerialNumbers_SerialNumberOnEmptyInventoryNotIncluded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			line1.WE_SerialNumber = "SER1";
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			line2.WE_SerialNumber = "SER2";
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var docketResponse = webService.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(docketResponse, webService);
			AssertEquals("Docket only has 1 used serial numbers.", 1, docketResponse.Dockets[0].UsedSerialNumbers.Length);
			AssertEquals("SER1|P1", docketResponse.Dockets[0].UsedSerialNumbers[0]);
		}

		public void TestGetWhsReceives_LoadUsedSerialNumbers_SerialNumberOnPutawayInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var lineWithPutaway = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			lineWithPutaway.WE_SerialNumber = "SER1";
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			line2.WE_SerialNumber = "SER2";
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var docketResponse1 = webService1.ValidatePalletIDOnPutaway("PLT1", false);
			AssertSuccessfulResponse(docketResponse1, webService1);
			AssertEquals("Precondition", true, lineWithPutaway.HasPutawayTransfer);
			AssertEquals("Precondition", 0m, lineWithPutaway.WE_StockOnHand);

			var webService2 = GetNewWebService(data.Whs1);
			var docketResponse2 = webService2.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(docketResponse2, webService2);
			AssertEquals("Docket has 2 used serial numbers.", 2, docketResponse2.Dockets[0].UsedSerialNumbers.Length);
			AssertEquals(true, docketResponse2.Dockets[0].UsedSerialNumbers.Contains("SER1|P1"));
			AssertEquals(true, docketResponse2.Dockets[0].UsedSerialNumbers.Contains("SER2|P1"));
		}

		#endregion

		#region TestGetWhsReceives_ShowStockOnHandWarningOnPutaway

		public void TestGetWhsReceives_ShowStockOnHandWarningOnPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10, false, false);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);

			var response1 = webService.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals(1, response1.Dockets.Length);
			AssertEquals(true, response1.ShowStockOnHandWarningOnPutaway);

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var response2 = webService.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response2, webService);
			AssertEquals(1, response2.Dockets.Length);
			AssertEquals(false, response2.ShowStockOnHandWarningOnPutaway);
		}

		#endregion

		#region TestGetWhsReceives_ByExtReference

		#region TestGetWhsReceives_ByExtReference

		public void TestGetWhsReceives_ByExtReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			receive2.WD_ExternalReference = "Ref2";
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService, "Ref1", receive1);
		}

		#endregion

		#region TestGetWhsReceives_ByExtReference_InMultipleWarehouses

		public void TestGetWhsReceives_ByExtReference_InMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			warehouse2.WW_WarehouseCode = "WH2";
			Helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, warehouse2, "Ref2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, "Ref1", receive1);
			AssertNoDocketIsReturned(webService1, "Ref2");

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = warehouse2.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService2, "Ref1");
			AssertCorrectDocketIsReturned(webService2, "Ref2", receive2);
		}

		#endregion

		#region TestGetWhsReceives_ByExtReference_ReceiveIsFinalised

		public void TestGetWhsReceives_ByExtReference_ReceiveIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Ref1", data.Part1, 10m);
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, "Ref1");
		}

		#endregion

		#region TestGetWhsReceives_ByExtReference_ExtReferenceIsNotExisted

		public void TestGetWhsReceives_ByExtReference_ExtReferenceIsNotExisted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, "NONEXISTINGEXTREF");
		}

		#endregion

		#region TestGetWhsReceives_ByExtReference_LoadReceivesOnly

		public void TestGetWhsReceives_ByExtReference_LoadReceivesOnly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "123", Notify);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var response = webService.GetWhsReceives("123", true);
				AssertSuccessfulResponse(response, webService);
			});
		}

		#endregion

		#region TestGetWhsReceives_ByExtReference_SplitByQuantity

		public void TestGetWhsReceives_ByExtReference_WithSplittedReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "ExtRef2");
			receive2.WD_ExternalReference = receive1.WD_DocketID;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			inventory.WI_SplitQuantity = 5m;

			Helper.Factory.Save();

			receive2.SplitReceiptByQuantity();
			Helper.Factory.Save();

			var notify = (NotificationBuffer)receive2.NotificationManager.Peek;
			AssertEquals("Should have no error", false, notify.HasErrors);

			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, receive1.WD_DocketID);
			AssertEquals("Should now be 2 receive (1 new split)", 2, Helper.Factory.Load<WhsReceive>(query).Length);

			var webService1 = GetNewWebService(data.Whs1);
			AssertDocketsIsReturned(webService1, receive1.WD_DocketID, 2);
		}

		#endregion

		#endregion

		#region TestGetWhsReceives_ByDocketID

		#region TestGetWhsReceives_ByDocketID

		public void TestGetWhsReceives_ByDocketID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			receive2.WD_ExternalReference = "Ref2";
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService, receive1.WD_DocketID, receive1);
		}

		#endregion

		#region TestGetWhsReceives_ByDocketID_InMultipleWarehouses

		public void TestGetWhsReceives_ByDocketID_InMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			warehouse2.WW_WarehouseCode = "WH2";
			Helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, warehouse2, "Ref2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService1, receive1.WD_DocketID, receive1);
			AssertNoDocketIsReturned(webService1, receive2.WD_DocketID);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = warehouse2.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService2, receive1.WD_DocketID);
			AssertCorrectDocketIsReturned(webService2, receive2.WD_DocketID, receive2);
		}

		#endregion

		#region TestGetWhsReceives_ByDocketID_ReceiveIsFinalised

		public void TestGetWhsReceives_ByDocketID_ReceiveIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Ref1", data.Part1, 10m);
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, receive.WD_DocketID);
		}

		#endregion

		#region TestGetWhsReceives_ByDocketID_DocketIDIsNotExisted

		public void TestGetWhsReceives_ByDocketID_DocketIDIsNotExisted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoDocketIsReturned(webService, "NONEXISTINGDOCKETID");
		}

		#endregion

		#region TestGetWhsReceives_ByDocketID_LoadReceivesOnly

		public void TestGetWhsReceives_ByDocketID_LoadReceivesOnly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "123", Notify);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var response = webService.GetWhsReceives(order.WD_DocketID, true);
				AssertSuccessfulResponse(response, webService);
			});
		}

		#endregion

		#endregion

		#region TestGetWhsReceives_IsUnloadProcess

		public void TestGetWhsReceives_IsUnloadProcess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			Helper.Factory.Save();

			receive2.PopulateASNLines();
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.GetWhsReceives("R1", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("Inventory lines should not be transferred into ASN lines", 1, receive1.Inventory.Count);
			AssertEquals("No ASN lines should be created", 0, receive1.AsnLines.Count);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("First RF load, existing inventories are retained.", 1, receive1.Inventory.Count);
			AssertEquals("First RF load, ASN Lines are generated from inventories.", 1, receive1.AsnLines.Count);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response3 = webService3.GetWhsReceives("R2", false);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals("Inventory lines should not be changed into ASN lines", 2, receive2.Inventory.Count);
			AssertEquals("No additional ASN lines should be created", 1, receive2.AsnLines.Count);

			var webService4 = GetNewWebService();
			webService4.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response4 = webService4.GetWhsReceives("R2", true);
			AssertSuccessfulResponse(response4, webService4);
			AssertEquals("Not First RF load, Inventory lines should be unchanged", 2, receive2.Inventory.Count);
			AssertEquals("Not First RF load, ASN lines should be unchanged", 1, receive2.AsnLines.Count);
		}

		#endregion

		#region TestGetWhsReceives_WithEmptyArrivalDate

		[TestDate(2012, 06, 21)]
		public void TestGetWhsReceives_WithEmptyArrivalDate()
		{
			var arrivalDate = ZDateTimeOffset.Now.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receiveWithEmptyArrivalDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty, Notify);
			var receiveWithNonEmptyArrivalDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", arrivalDate, Notify);

			Helper.Factory.Save();

			AssertEquals("Pre-condition", ZDateTimeOffset.Empty, receiveWithEmptyArrivalDate.WD_ArrivalDate);
			AssertEquals("Pre-condition", arrivalDate, receiveWithNonEmptyArrivalDate.WD_ArrivalDate);

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService1.GetWhsReceives("R1", false);
			AssertEquals("Since arrival date is empty, is should be set.", ZDateTimeOffset.Now, receiveWithEmptyArrivalDate.WD_ArrivalDate);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.GetWhsReceives("R2", false);
			AssertEquals("Since arrival date is not empty, it should not be overridden.", arrivalDate, receiveWithNonEmptyArrivalDate.WD_ArrivalDate);
		}

		#endregion

		#region TestGetWhsReceives_ReturnsMultipleReceives

		public void TestGetWhsReceives_ReturnsMultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.GetWhsReceives("NONEXISTANTREF", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(0, response1.Dockets.Length);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "REF1");
			Helper.Factory.Save();
			var response2 = webService2.GetWhsReceives("REF1", false);
			AssertContainsExactElementsInAnyOrder(new[] { receive1.WD_DocketID }, response2.Dockets.Select(d => d.DocketID));

			var webService3 = GetNewWebService();
			SetupSecurityHeader(webService3, data.Whs1, staff);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF2");
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1");
			receive3.WD_ExternalReferenceSplit = 1;
			var receive4 = Helper.CreateWhsReceive(Helper.CreateClient("C2"), data.Whs1, "REF1");
			Helper.Factory.Save();
			var response3 = webService3.GetWhsReceives("REF1", false);
			AssertContainsExactElementsInAnyOrder(new[] { receive1.WD_DocketID, receive3.WD_DocketID, receive4.WD_DocketID }, response3.Dockets.Select(d => d.DocketID));
		}

		#endregion

		#region TestGetWhsReceives_OneReceiveOrder_HeldCodesAndLineDuplicateSecurity

		public void TestGetWhsReceives_OneReceiveOrder_HeldCodesAndLineDuplicateSecurity()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var heldCode2 = Helper.CreateInventoryHeldCode("BBB", "BBB for client 1", data.Org1.PK);
			var heldCode3 = Helper.CreateInventoryHeldCode("CCC", "test", data.Org1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("DDD", "test", client2.PK);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			Env.Security.WhsRFScanningUnloadDuplicatePreviousLine.IsAllowed = true;
			var response1 = webService1.GetWhsReceives("R1", false);
			AssertEquals(true, response1.CanDuplicatePreviousLine);
			AssertEquals(8, response1.HeldCodes.Length);
			response1.HeldCodes.Single(p => string.IsNullOrEmpty(p.Code) && string.IsNullOrEmpty(p.Description));
			response1.HeldCodes.Single(p => p.Code == "HEL" && p.Description == "Held");
			response1.HeldCodes.Single(p => p.Code == "DAM" && p.Description == "Damaged");
			response1.HeldCodes.Single(p => p.Code == "LCC" && p.Description == "Lost in Cycle Count");
			response1.HeldCodes.Single(p => p.Code == "SHORT" && p.Description == "Short Picked");
			response1.HeldCodes.Single(p => p.Code == "AAA" && p.Description == "AAA for system");
			response1.HeldCodes.Single(p => p.Code == "BBB" && p.Description == "BBB for client 1");
			response1.HeldCodes.Single(p => p.Code == "CCC" && p.Description == "test");

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			Env.Security.WhsRFScanningUnloadDuplicatePreviousLine.IsAllowed = false;
			var response2 = webService2.GetWhsReceives("R1", false);
			AssertEquals(false, response2.CanDuplicatePreviousLine);
			AssertEquals(8, response2.HeldCodes.Length);
			response2.HeldCodes.Single(p => string.IsNullOrEmpty(p.Code) && string.IsNullOrEmpty(p.Description));
			response2.HeldCodes.Single(p => p.Code == "HEL" && p.Description == "Held");
			response2.HeldCodes.Single(p => p.Code == "DAM" && p.Description == "Damaged");
			response2.HeldCodes.Single(p => p.Code == "LCC" && p.Description == "Lost in Cycle Count");
			response2.HeldCodes.Single(p => p.Code == "SHORT" && p.Description == "Short Picked");
			response2.HeldCodes.Single(p => p.Code == "AAA" && p.Description == "AAA for system");
			response2.HeldCodes.Single(p => p.Code == "BBB" && p.Description == "BBB for client 1");
			response2.HeldCodes.Single(p => p.Code == "CCC" && p.Description == "test");
		}

		#endregion

		#region TestGetWhsReceives_MultipleReceiveOrders_HeldCodesAndLineDuplicateSecurity

		public void TestGetWhsReceives_MultipleReceiveOrders_HeldCodesAndLineDuplicateSecurity()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.GetWhsReceives("NONEXISTANTREF", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(0, response1.HeldCodes.Length);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF2");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1");
			var receive3 = Helper.CreateWhsReceive(Helper.CreateClient("C2"), data.Whs1, "REF1");
			Helper.Factory.Save();
			var response2 = webService2.GetWhsReceives("REF1", false);
			AssertContainsExactElementsInAnyOrder(new[] { receive2.WD_DocketID, receive3.WD_DocketID }, response2.Dockets.Select(d => d.DocketID));
			AssertEquals(0, response2.HeldCodes.Length);
		}

		#endregion

		#region TestGetWhsReceives_ASNUnloadState

		#region TestGetWhsReceives_ASNUnloadState_NoReceiveLinesAndASNLines

		public void TestGetWhsReceives_ASNUnloadState_NoReceiveLinesAndASNLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			// receive has no receive lines and ASN lines, ASNUnloadState: ASNState.NoPallets
			var docketsResponse = webService.GetWhsReceives("Rev1", true);
			AssertSuccessfulResponse(docketsResponse, webService);
			AssertNotNull(docketsResponse.Dockets);
			AssertEquals(receive.PK, docketsResponse.Dockets[0].PK);
			AssertEquals(ASNState.NoPallets, docketsResponse.Dockets[0].ASNUnloadState);
		}

		#endregion

		#region TestGetWhsReceives_ASNUnloadState_HasASNPalletsUnload

		public void TestGetWhsReceives_ASNUnloadState_HasASNPalletsUnload()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			// recevie has ASN line with Pallet, ASNUnloadState: ASNState.HasASNPalletsToUnload
			var docketsResponse = webService.GetWhsReceives("Rev1", true);
			AssertSuccessfulResponse(docketsResponse, webService);
			AssertNotNull(docketsResponse.Dockets);
			AssertEquals(receive.PK, docketsResponse.Dockets[0].PK);
			AssertEquals(ASNState.HasASNPalletsToUnload, docketsResponse.Dockets[0].ASNUnloadState);
		}

		#endregion

		#region TestGetWhsReceives_ASNUnloadState_HasASNLinesButNoPallets

		public void TestGetWhsReceives_ASNUnloadState_HasASNLinesButNoPallets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			// receive has ASN line without Pallets, ASNUnloadState: ASNState.NoPallets
			var docketsResponse = webService.GetWhsReceives("Rev1", true);
			AssertSuccessfulResponse(docketsResponse, webService);
			AssertNotNull(docketsResponse.Dockets);
			AssertEquals(receive.PK, docketsResponse.Dockets[0].PK);
			AssertEquals(ASNState.NoPallets, docketsResponse.Dockets[0].ASNUnloadState);
		}

		#endregion

		#region TestGetWhsReceives_ASNUnloadState_OtherPalletsHaveUnloadedLines

		public void TestGetWhsReceives_ASNUnloadState_OtherPalletsHaveUnloadedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService1 = GetNewWebService(data.Whs1);
			var year = ZDateTime.Now.Year - 1;
			// unload one line through RF, and serial number SN01, Pallet PLT1
			var inventoryResponse = webService1.UnloadWhsReceiveLines(new[] {
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT1", data.Whs1.DefaultLocation.ToLocationString())
			}, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(inventoryResponse, webService1);
			AssertEquals(ErrorTypes.None, inventoryResponse.Error);
			AssertEquals(true, string.IsNullOrEmpty(inventoryResponse.ErrorMessage));

			// PLT2 not unloaded, ASNUnloadState: ASNState.HasASNPalletsToUnload
			var service2 = GetNewWebService(data.Whs1);
			var docketsResponse = service2.GetWhsReceives("Rev1", true);
			AssertSuccessfulResponse(docketsResponse, service2);
			AssertNotNull(docketsResponse.Dockets);
			AssertEquals(receive.PK, docketsResponse.Dockets[0].PK);
			AssertEquals(ASNState.HasASNPalletsToUnload, docketsResponse.Dockets[0].ASNUnloadState);
		}

		#endregion

		#region TestGetWhsReceives_ASNUnloadState_AllPalletsUnloaded

		public void TestGetWhsReceives_ASNUnloadState_AllPalletsUnloaded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "");

			Helper.Factory.Save();

			PopluateASNLines(receive);
			var year = ZDateTime.Now.Year - 1;
			// unload two lines through RF, Pallet PLT1 and PLT2
			var service1 = GetNewWebService(data.Whs1);
			var inventoryResponse = service1.UnloadWhsReceiveLines(new[] {
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT1", data.Whs1.DefaultLocation.ToLocationString()),
				CreateWhsDocketLineInfo(receive.PK.ToGuid(), "P1", 10, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", "PLT2", data.Whs1.DefaultLocation.ToLocationString())
			}, Array.Empty<Guid>(), false);
			AssertSuccessfulResponse(inventoryResponse, service1);
			AssertEquals(ErrorTypes.None, inventoryResponse.Error);
			AssertEquals(true, string.IsNullOrEmpty(inventoryResponse.ErrorMessage));

			// ASNUnloadState: ASNState.AllASNPalletsUnloaded
			var service2 = GetNewWebService(data.Whs1);
			var docketsResponse = service2.GetWhsReceives("Rev1", true);
			AssertSuccessfulResponse(docketsResponse, service2);
			AssertNotNull(docketsResponse.Dockets);
			AssertEquals(receive.PK, docketsResponse.Dockets[0].PK);
			AssertEquals(ASNState.AllASNPalletsUnloaded, docketsResponse.Dockets[0].ASNUnloadState);
		}

		#endregion

		#endregion

		#region TestGetWhsReceives_RFLogAdded

		public void TestGetWhsReceives_UnloadProcess_RFLogAdded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("First RF load, inventories are retained.", 1, receive.Inventory.Count);
			AssertEquals("First RF load, ASN lines are generated from inventories.", 1, receive.AsnLines.Count);
			AssertEquals("Receive has RF logs.", true, receive.Logs.GetAllLogs().Cast<StmALog>().Any(log => log.SL_Reference == "RF: Unloading job commenced."));
			AssertEquals("Receive has 1 RF log.", 1, receive.Logs.GetAllLogs().Cast<StmALog>().Count(log => log.SL_Reference == "RF: Unloading job commenced."));

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Receive still has 1 RF log.", 1, receive.Logs.GetAllLogs().Cast<StmALog>().Count(log => log.SL_Reference == "RF: Unloading job commenced."));
		}

		public void TestGetWhsReceives_UnloadProcess_NoReceiveLines_RFLogAdded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("Receive has no ASN lines.", 0, receive.AsnLines.Count);
			AssertEquals("Receive has RF logs.", true, receive.Logs.GetAllLogs().Cast<StmALog>().Any(log => log.SL_Reference == "RF: Unloading job commenced."));
			AssertEquals("Receive has 1 RF log.", 1, receive.Logs.GetAllLogs().Cast<StmALog>().Count(log => log.SL_Reference == "RF: Unloading job commenced."));

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Receive still has 1 RF log.", 1, receive.Logs.GetAllLogs().Cast<StmALog>().Count(log => log.SL_Reference == "RF: Unloading job commenced."));
		}

		#endregion

		#region TestGetWhsReceives_Unload_BeforeReceiveStartedReceiving

		public void TestGetWhsReceives_Unload_BeforeReceiveStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receive.StartedReceiving);

			var service = GetNewWebService(data.Whs1);
			var response = service.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response, service);
			AssertEquals(true, receive.StartedReceiving);
			AssertEquals("Receive lines are not deleted.", false, receiveLine1.IsDeleted);
			AssertEquals("Transaction quantity is cleared.", 0m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive lines are not deleted.", false, receiveLine2.IsDeleted);
			AssertEquals("Transaction quantity is cleared.", 0m, receiveLine2.WE_TransactionQuantity);
		}

		#endregion

		#region TestGetWhsReceives_Unload_AfterReceiveStartedReceiving

		public void TestGetWhsReceives_Unload_AfterReceiveStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			Helper.Factory.Save();

			receive.PopulateASNLines();
			AssertEquals("Precondition", true, receive.StartedReceiving);
			Helper.Factory.Save();

			var service = GetNewWebService(data.Whs1);
			var response = service.GetWhsReceives("R1", true);
			AssertSuccessfulResponse(response, service);
			AssertEquals("Receive lines are not deleted.", false, receiveLine1.IsDeleted);
			AssertEquals("Transaction quantity is cleared.", 10m, receiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive lines are not deleted.", false, receiveLine2.IsDeleted);
			AssertEquals("Transaction quantity is cleared.", 20m, receiveLine2.WE_TransactionQuantity);
		}

		#endregion

		#region TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled

		public void TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_AllAsnLinesWithNoPalletId_UnloadProcess()
		{
			TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_AllAsnLinesWithNoPalletIdCore(isUnloadProcess: true, expectedIsEmptyAsnPalletMatchingEnabled: true);
		}

		public void TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_AllAsnLinesWithNoPalletId_NotUnloadProcess()
		{
			TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_AllAsnLinesWithNoPalletIdCore(isUnloadProcess: false, expectedIsEmptyAsnPalletMatchingEnabled: false);
		}

		void TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_AllAsnLinesWithNoPalletIdCore(bool isUnloadProcess, bool expectedIsEmptyAsnPalletMatchingEnabled)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsReceives("R1", isUnloadProcess);

			AssertEquals(isUnloadProcess, receive.AsnLines.Any());

			if (isUnloadProcess)
			{
				AssertEquals("All ASN lines don't have pallet id.", true, receive.AsnLines.Cast<WhsAsnLine>().Any(asnLine => asnLine.WN_PalletId.IsEmpty));
			}
			AssertEquals("Correct IsEmptyAsnPalletMatchingEnabled info is returned.", expectedIsEmptyAsnPalletMatchingEnabled, response1.Dockets[0].IsEmptyAsnPalletMatchingEnabledForUnload);

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetWhsReceives("R1", isUnloadProcess);
			AssertEquals("The same correct IsEmptyAsnPalletMatchingEnabled info is returned.", expectedIsEmptyAsnPalletMatchingEnabled, response2.Dockets[0].IsEmptyAsnPalletMatchingEnabledForUnload);
		}

		public void TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_ReceiveWithNoAsnLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceives("R1", true);

			AssertEquals("Receive has no ASN lines.", false, receive.AsnLines.Any());
			AssertEquals("Correct IsEmptyAsnPalletMatchingEnabled info is returned.", false, response.Dockets[0].IsEmptyAsnPalletMatchingEnabledForUnload);
		}

		public void TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_SomeAsnLinesHavePalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "ABC");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceives("R1", true);

			AssertEquals("Receive has ASN lines populated.", true, receive.AsnLines.Any());
			AssertEquals("Not all ASN lines have empty pallet id.", false, receive.AsnLines.Cast<WhsAsnLine>().All(asnLine => asnLine.WN_PalletId.IsEmpty));
			AssertEquals("Correct IsEmptyAsnPalletMatchingEnabled info is returned.", false, response.Dockets[0].IsEmptyAsnPalletMatchingEnabledForUnload);
		}

		public void TestGetWhsReceive_IsEmptyAsnPalletMatchingEnabled_AllAsnLinesHavePalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "ABC");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, null, "DEF");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceives("R1", true);

			AssertEquals("Receive has ASN lines populated.", true, receive.AsnLines.Any());
			AssertEquals("Not all ASN lines have empty pallet id.", false, receive.AsnLines.Cast<WhsAsnLine>().All(asnLine => asnLine.WN_PalletId.IsEmpty));
			AssertEquals("Correct IsEmptyAsnPalletMatchingEnabled info is returned.", false, response.Dockets[0].IsEmptyAsnPalletMatchingEnabledForUnload);
		}

		#endregion

		#region TestGetWhsReceive_LoadProductsWhichMayFulfillAsnLinesWithStockUnit

		public void TestGetWhsReceive_LoadProductsWhichMayFulfillAsnLinesWithStockUnit_AsnLinesUseStockUnit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = "PID";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_F3_NKPackType = "UNT";
			receiveLine2.WE_PalletID = "PID";

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "UNT";
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsReceives("R1", true);

			AssertEquals(true, receive.AsnLines.Any());

			AssertContainsExactElementsInAnyOrder(new[] { data.Part1.PK }, response1.Dockets.Single().ProductsWhichMayFulfillAsnLinesWithStockUnit);
		}

		public void TestGetWhsReceive_LoadProductsWhichMayFulfillAsnLinesWithStockUnit_AsnLinesDoNotUseStockUnit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			Helper.CreateProductUnit(data.Part1, "PLT", 5m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = "PID";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_F3_NKPackType = "PLT";
			receiveLine2.WE_PalletID = "PID";

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part1, 10m);
			asnLine2.WN_QuantityUQ = "PLT";
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsReceives("R1", true);

			AssertEquals(true, receive.AsnLines.Any());
			AssertContainsExactElementsInAnyOrder(Array.Empty<Guid>(), response1.Dockets.Single().ProductsWhichMayFulfillAsnLinesWithStockUnit);
		}

		public void TestGetWhsReceive_LoadProductsWhichMayFulfillAsnLinesWithStockUnit_MultipleProductsAsnLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);

			data.Part1.OP_StockKeepingUnit = "UNT";
			data.Part2.OP_StockKeepingUnit = "PLT";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine1.WE_F3_NKPackType = "UNT";
			receiveLine1.WE_PalletID = "PID";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m);
			receiveLine2.WE_F3_NKPackType = "PLT";
			receiveLine2.WE_PalletID = "PID";

			var asnLine1 = Helper.CreateAsnLine(receive, data.Part1, 5m);
			asnLine1.WN_QuantityUQ = "UNT";
			var asnLine2 = Helper.CreateAsnLine(receive, data.Part2, 10m);
			asnLine2.WN_QuantityUQ = "PLT";
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsReceives("R1", true);

			AssertEquals(true, receive.AsnLines.Any());

			AssertContainsExactElementsInAnyOrder(new[] { data.Part1.PK, data.Part2.PK }, response1.Dockets.Single().ProductsWhichMayFulfillAsnLinesWithStockUnit);
		}

		#endregion

		#region TestGetWhsReceive_HeldCodes

		public void TestGetWhsReceive_HeldCodes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var heldCode2 = Helper.CreateInventoryHeldCode("BBB", "BBB for client 1", data.Org1.PK);
			var heldCode3 = Helper.CreateInventoryHeldCode("CCC", "test", data.Org1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("DDD", "test", client2.PK);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateReferenceForDocket(receive1, "HAWB1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			CreateReferenceForDocket(receive2, "HAWB2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsReceive(receive1.PK.ToGuid(), true);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(8, response.HeldCodes.Length);
			response.HeldCodes.Single(p => string.IsNullOrEmpty(p.Code) && string.IsNullOrEmpty(p.Description));
			response.HeldCodes.Single(p => p.Code == "HEL" && p.Description == "Held");
			response.HeldCodes.Single(p => p.Code == "DAM" && p.Description == "Damaged");
			response.HeldCodes.Single(p => p.Code == "LCC" && p.Description == "Lost in Cycle Count");
			response.HeldCodes.Single(p => p.Code == "SHORT" && p.Description == "Short Picked");
			response.HeldCodes.Single(p => p.Code == "AAA" && p.Description == "AAA for system");
			response.HeldCodes.Single(p => p.Code == "BBB" && p.Description == "BBB for client 1");
			response.HeldCodes.Single(p => p.Code == "CCC" && p.Description == "test");
		}

		#endregion

		#region TestGetWhsReceives_PackageIdReference

		public void TestGetWhsReceives_PackageIdReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 2.", 2, package.PackedItemDivots.Count);
			var divot1 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 10m);
			var divot2 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 20m);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice = GetNewWebService(data.Whs1);
			var result = webservice.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result.Error);
			AssertEquals("Precondition: there are no errors.", null, result.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive's details are correct.", true, newReceive.StartedReceiving);
			AssertEquals("New receive has 2 lines.", 2, newReceive.Lines.Count);

			var reference = newReceive.References.Cast<WhsDocketReference>().Single();
			AssertEquals("Receive reference details are correct.", WarehouseAdditionalReferenceTypes.Codes.Other, reference.WX_RefType);
			AssertEquals("Receive reference details are correct.", "Pkg1", reference.WX_Reference);

			var newReceiveLine1 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 10m);
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);

			var newReceiveLine2 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceiveLine2.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine2.WE_TransactionQuantity);

			AssertEquals("New receive has 2 asn lines.", 2, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 10m);
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);

			var newReceiveAsnLine2 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 20m);
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceiveAsnLine2.WN_OP);

			AssertContainsExactElementsInAnyOrder("Receive and Order are linked.", new[] { newReceive }, order.RelatedJobs);
			AssertContainsExactElementsInAnyOrder("Receive and Order are linked.", new[] { order }, newReceive.RelatedJobs);
		}

		#region TestGetWhsReceives_PackageIdReference_CheckDockDoorLocations

		public void TestGetWhsReceive_PackageIdReference_CheckDockDoorLocations_SingleDDL()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 2.", 2, package.PackedItemDivots.Count);
			var divot1 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 10m);
			var divot2 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 20m);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var result = webService.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result.Error);
			AssertEquals("Precondition: there are no errors.", null, result.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive's details are correct.", true, newReceive.StartedReceiving);
			AssertEquals("New receive has 2 lines.", 2, newReceive.Lines.Count);

			var reference = newReceive.References.Cast<WhsDocketReference>().Single();
			AssertEquals("Receive reference details are correct.", WarehouseAdditionalReferenceTypes.Codes.Other, reference.WX_RefType);
			AssertEquals("Receive reference details are correct.", "Pkg1", reference.WX_Reference);

			var expectedDDL = data.Whs1.DefaultInboundDockDoorLocation;
			AssertEquals("Response should only check DDL during Unload.", true, result.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should retrieve appropriate DDL PK.", expectedDDL.PK, result.SingleDockDoorLocationPK);
			AssertEquals("Response should retrieve appropriate DDL string.", expectedDDL.WLV_LocationString, result.SingleDockDoorLocation);
		}

		public void TestGetWhsReceive_PackageIdReference_CheckDockDoorLocations_MultipleDDL()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("TS1", "Test1", false, 0, LocationClasses.Codes.DDL);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[2].WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 2.", 2, package.PackedItemDivots.Count);
			var divot1 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 10m);
			var divot2 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 20m);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var result = webService.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result.Error);
			AssertEquals("Precondition: there are no errors.", null, result.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive's details are correct.", true, newReceive.StartedReceiving);
			AssertEquals("New receive has 2 lines.", 2, newReceive.Lines.Count);

			var reference = newReceive.References.Cast<WhsDocketReference>().Single();
			AssertEquals("Receive reference details are correct.", WarehouseAdditionalReferenceTypes.Codes.Other, reference.WX_RefType);
			AssertEquals("Receive reference details are correct.", "Pkg1", reference.WX_Reference);

			AssertEquals("Response should only return false if multiple DDL.", false, result.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should not populate DDL PK if multiple DDL.", Guid.Empty, result.SingleDockDoorLocationPK);
			AssertEquals("Response should not populate DDL string if multiple DDL.", null, result.SingleDockDoorLocation);
		}

		#endregion

		public void TestGetWhsReceives_PackageIdReference_ReturnReceiveExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg2";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 2 packages.", 2, order.PackageJob.Packages.Count);
			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceiveAsnLine1.WN_Quantity);

			var webservice2 = GetNewWebService(data.Whs1);
			var result2 = webservice2.GetWhsReceives("Pkg2", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertEquals("Existing return receive is returned.", newReceive.PK, newReceive2.PK);
			AssertEquals("New receive has 2 lines.", 2, newReceive2.Lines.Count);

			var newReceive2Line1 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 10m);
			var newReceive2Line2 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceive2Line2.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line2.WE_TransactionQuantity);

			AssertEquals("New receive has 2 asn lines.", 2, newReceive.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 10m);
			var newReceive2AsnLine2 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 20m);
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceive2AsnLine2.WN_OP);
		}

		public void TestGetWhsReceives_PackageIdReference_ReturnReceiveExists_SamePackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceiveAsnLine1.WN_Quantity);

			var webservice2 = GetNewWebService(data.Whs1);
			var result2 = webservice2.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertEquals("Existing return receive is returned.", newReceive.PK, newReceive2.PK);
			AssertEquals("New receive still has 1 line.", 1, newReceive2.Lines.Count);

			var newReceive2Line1 = newReceive2.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceive2Line1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceive2Line1.WE_ClientOrderedUnits);

			AssertEquals("New receive still has 1 asn line.", 1, newReceive2.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive2.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceive2AsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceive2AsnLine1.WN_Quantity);
		}

		public void TestGetWhsReceives_PackageIdReference_WithInnerPackages()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 10m);

			var innerPackage = package.Packages.AddNew();
			innerPackage.KP_PackageID = "Inner1";
			innerPackage.Pack(orderLine2.ReleaseLines[0], 20m);

			var innerInnerPackage = innerPackage.Packages.AddNew();
			innerInnerPackage.KP_PackageID = "Inner2";
			innerInnerPackage.Pack(orderLine1.ReleaseLines[0], 30m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 3 packages.", 3, order.PackageJob.GetAllPackagesOnJob().Length);
			var divot1 = package.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			var divot2 = innerPackage.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);

			var divot3 = innerInnerPackage.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 30m, divot3.KI_PackedQty);

			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice = GetNewWebService(data.Whs1);
			var result = webservice.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result.Error);
			AssertEquals("Precondition: there are no errors.", null, result.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 3 lines.", 3, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 10m);
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);

			var newReceiveLine2 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceiveLine2.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine2.WE_TransactionQuantity);

			var newReceiveLine3 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 30m);
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine3.WE_TransactionQuantity);

			AssertEquals("New receive has 3 asn lines.", 2, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 40m);
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);

			var newReceiveAsnLine2 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 20m);
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceiveAsnLine2.WN_OP);
		}

		public void TestGetWhsReceives_PackageIdReference_WithInnerPackages_InnerPackageFirst()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 10m);

			var innerPackage = package.Packages.AddNew();
			innerPackage.KP_PackageID = "Inner1";
			innerPackage.Pack(orderLine2.ReleaseLines[0], 20m);

			var innerInnerPackage = innerPackage.Packages.AddNew();
			innerInnerPackage.KP_PackageID = "Inner2";
			innerInnerPackage.Pack(orderLine1.ReleaseLines[0], 30m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 3 packages.", 3, order.PackageJob.GetAllPackagesOnJob().Length);
			var divot1 = package.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			var divot2 = innerPackage.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);

			var divot3 = innerInnerPackage.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 30m, divot3.KI_PackedQty);

			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("Inner1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive1 = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive1.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive1.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive1.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive1.WD_DocketSubType);
			AssertEquals("New receive has 2 lines.", 2, newReceive1.Lines.Count);

			var newReceiveLine1 = newReceive1.Lines.Single(line => line.WE_ClientOrderedUnits == 30m);
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);

			var newReceiveLine2 = newReceive1.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceiveLine2.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine2.WE_TransactionQuantity);

			AssertEquals("New receive has 2 asn lines.", 2, newReceive1.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive1.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 30m);
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);

			var newReceiveAsnLine2 = newReceive1.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 20m);
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceiveAsnLine2.WN_OP);

			var webservice2 = GetNewWebService(data.Whs1);
			var result2 = webservice2.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertEquals("Returned receives are the same.", newReceive1.PK, newReceive2.PK);
			AssertEquals("New receive has 3 lines.", 3, newReceive2.Lines.Count);

			var newReceive2Line1 = newReceive2.Lines.Single(line => line.WE_ClientOrderedUnits == 30m);
			var newReceive2Line2 = newReceive2.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			var newReceive2Line3 = newReceive2.Lines.Single(line => line.WE_ClientOrderedUnits == 10m);
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceive2Line3.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line3.WE_TransactionQuantity);

			AssertEquals("New receive has 2 asn lines.", 2, newReceive2.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive1.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 40m);
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceive2AsnLine1.WN_OP);
			var newReceive2AsnLine2 = newReceive1.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 20m);
		}

		public void TestGetWhsReceives_PackageIdReference_WithInnerPackages_InnerPackageOnFinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 40m);

			var innerPackage = package.Packages.AddNew();
			innerPackage.KP_PackageID = "Inner1";
			innerPackage.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 2 packages.", 2, order.PackageJob.GetAllPackagesOnJob().Length);
			var divot1 = package.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 40m, divot1.KI_PackedQty);

			var divot2 = innerPackage.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);

			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("Inner1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive1 = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive1.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive1.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive1.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive1.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive1.Lines.Count);

			var newReceiveLine1 = newReceive1.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 20m, newReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive1.AsnLines.Count);
			var newReceiveAsnLine2 = newReceive1.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceiveAsnLine2.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 20m, newReceiveAsnLine2.WN_Quantity);

			newReceiveLine1.WE_TransactionQuantity = 20m;
			newReceive1.AllocateLocationsWithMock();
			newReceive1.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(newReceive1);
			var webservice2 = GetNewWebService(data.Whs1);
			var result2 = webservice2.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertNotEquals("Returned receives are not the same.", newReceive1.PK, newReceive2.PK);
			AssertEquals("New receive has 1 line.", 1, newReceive2.Lines.Count);

			var newReceive2Line1 = newReceive2.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceive2Line1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 40m, newReceive2Line1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive2.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive2.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceive2AsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 40m, newReceive2AsnLine1.WN_Quantity);
		}

		public void TestGetWhsReceives_PackageIdReference_ReceiveWithPackageIdReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Pkg1");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 1.", 1, package.PackedItemDivots.Count);
			var divot = package.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot from orderline1.", 10m, divot.KI_PackedQty);

			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			AssertEquals("Precondition: There are 2 receives.", 2, Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, "INW")));

			var webservice1 = GetNewWebService(data.Whs1);
			var result = webservice1.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result.Error);
			AssertEquals("Precondition: there are no errors.", null, result.ErrorMessage);

			AssertEquals("Returned receive is the existing receive.", result.Dockets.Single().PK, receive2.PK);
			AssertEquals("Receive still has 1 line.", 1, receive2.Lines.Count);
			AssertEquals("No new receives added.", 2, Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, "INW")));
		}

		public void TestGetWhsReceives_PackageIdReference_PackageDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertEquals("There are no packages.", 0, Helper.Factory.GetDatabaseCount(typeof(PkgPackage)));
			AssertNoDocketIsReturned(GetNewWebService(data.Whs1), "AnyUnknownPackage");
		}

		public void TestGetWhsReceives_PackageIdReference_UnfinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 1.", 1, package.PackedItemDivots.Count);
			var divot1 = package.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot details is correct.", 10m, divot1.KI_PackedQty);
			Assert("Precondition: pick is not finalised.", !pick.IsFinalised);
			Assert("Precondition: order is not finalised.", !order.IsFinalised);

			AssertNoDocketIsReturned(GetNewWebService(data.Whs1), "Pkg1");
		}

		public void TestGetWhsReceives_PackageIdReference_ExistingReceiveIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg2";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 2 packages.", 2, order.PackageJob.Packages.Count);
			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceiveAsnLine1.WN_Quantity);

			newReceiveLine1.WE_TransactionQuantity = 10m;
			newReceive.AllocateLocationsWithMock();
			newReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			Assert("Receive is finalised.", newReceive.IsFinalised);

			var webservice2 = GetNewWebService(data.Whs1);
			var result2 = webservice2.GetWhsReceives("Pkg2", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertNotEquals("New receive is created.", newReceive2.PK, newReceive.PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive2.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", (ZByte)1, newReceive2.WD_ExternalReferenceSplit);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive2.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive2.WD_OH_Client);
			AssertEquals("New receive has 1 line.", 1, newReceive2.Lines.Count);

			var newReceive2Line1 = newReceive2.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceive2Line1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 20m, newReceive2Line1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive2.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive2.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceive2AsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 20m, newReceive2AsnLine1.WN_Quantity);
		}

		public void TestGetWhsReceives_PackageIdReference_ExistingReceiveInAnotherWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg2";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 2 packages.", 2, order.PackageJob.Packages.Count);
			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceiveAsnLine1.WN_Quantity);

			var whs2 = Helper.CreateWarehouse("Warehouse2");
			whs2.WW_WarehouseCode = "WH2";
			Helper.Factory.Save();

			var webservice2 = GetNewWebService(whs2);
			var result2 = webservice2.GetWhsReceives("Pkg2", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertNotEquals("New receive is created.", newReceive2.PK, newReceive.PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive2.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", (ZByte)1, newReceive2.WD_ExternalReferenceSplit);
			AssertEquals("New receive's details are correct.", whs2.PK, newReceive2.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive2.WD_OH_Client);
			AssertEquals("New receive has 1 line.", 1, newReceive2.Lines.Count);

			var newReceive2Line1 = newReceive2.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceive2Line1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 20m, newReceive2Line1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive2.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive2.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceive2AsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 20m, newReceive2AsnLine1.WN_Quantity);

			var webservice3 = GetNewWebService(whs2);
			var result3 = webservice3.GetWhsReceives("Pkg1", true);
			AssertEquals("There are no errors.", ErrorTypes.None, result3.Error);
			AssertEquals("There are no errors.", null, result3.ErrorMessage);
			AssertEquals("There are no dockets returned.", 0, result3.Dockets.Length);
		}

		public void TestGetWhsReceives_PackageIdReference_MultiplePackages()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);
			Helper.Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg1";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();
			order2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();
			Helper.Factory.Save();

			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);

			AssertIsFinalisedPrecondition(pick1);
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick2);
			AssertIsFinalisedPrecondition(order2);

			var webService = GetNewWebService(data.Whs1);
			var result = webService.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result.Error);
			AssertEquals("Precondition: there are no errors.", null, result.ErrorMessage);

			AssertEquals("2 docket infos are returned.", 2, result.Dockets.Length);
			var docketInfo1 = result.Dockets.Where(docket => docket.DocketID == order1.WD_ExternalReference).Single();
			AssertEquals("Package details are correct.", "Pkg1", docketInfo1.ExternalReference);
			AssertEquals("Package details are correct.", package1.PK, docketInfo1.PK);
			AssertEquals("Package details are correct.", data.Org1.OH_Code, docketInfo1.ClientCode);

			var docketInfo2 = result.Dockets.Where(docket => docket.DocketID == order2.WD_ExternalReference).Single();
			AssertEquals("Package details are correct.", "Pkg1", docketInfo2.ExternalReference);
			AssertEquals("Package details are correct.", package2.PK, docketInfo2.PK);
			AssertEquals("Package details are correct.", data.Org1.OH_Code, docketInfo2.ClientCode);
		}

		public void TestGetWhsReceives_PackageIdReference_MultiplePackages_OneWithFinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();
			Helper.Factory.Save();

			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			AssertIsFinalisedPrecondition(pick1);
			AssertIsFinalisedPrecondition(order1);

			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			var newReceiveLine1 = newReceive.Lines.Single();
			newReceiveLine1.WE_TransactionQuantity = 10m;
			newReceive.AllocateLocationsWithMock();
			newReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			Assert("Receive is finalised.", newReceive.IsFinalised);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part2, 10m);
			Helper.Factory.Save();

			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg1";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);

			var pick3 = Helper.CreatePickNew(order3);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "Pkg1";
			package3.Pack(orderLine3.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			order2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();

			order3.FinaliseDocketWithoutUserConfirmation();
			pick3.FinalisePick();
			Helper.Factory.Save();

			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);

			var divot3 = package3.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot3.KI_PackedQty);

			AssertIsFinalisedPrecondition(pick2);
			AssertIsFinalisedPrecondition(order2);
			AssertIsFinalisedPrecondition(pick3);
			AssertIsFinalisedPrecondition(order3);

			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			AssertEquals("2 docket infos are returned.", 2, result2.Dockets.Length);
			var docketInfo1 = result2.Dockets.Where(docket => docket.DocketID == order2.WD_ExternalReference).Single();
			AssertEquals("Package details are correct.", "Pkg1", docketInfo1.ExternalReference);
			AssertEquals("Package details are correct.", package2.PK, docketInfo1.PK);
			AssertEquals("Package details are correct.", data.Org1.OH_Code, docketInfo1.ClientCode);

			var docketInfo2 = result2.Dockets.Where(docket => docket.DocketID == order3.WD_ExternalReference).Single();
			AssertEquals("Package details are correct.", "Pkg1", docketInfo2.ExternalReference);
			AssertEquals("Package details are correct.", package3.PK, docketInfo2.PK);
			AssertEquals("Package details are correct.", data.Org1.OH_Code, docketInfo2.ClientCode);
		}

		public void TestGetWhsReceives_PackageIdReference_WithReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var inventoryLine = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20);
			var pick = Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(orderLine, inventoryLine, 20);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "A1";
			releaseLine1.PartAttribute2 = "B1";
			releaseLine1.PartAttribute3 = "C1";
			releaseLine1.Quantity = 15m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "A2";
			releaseLine2.PartAttribute2 = "B2";
			releaseLine2.PartAttribute3 = "C2";
			releaseLine2.Quantity = 5m;

			var package = order.PackageJob.Packages.AddNew("PLT", "Pkg1");
			var pickLine1 = orderLine.PickLines.Single(l => l.WZ_Units == 15m);
			var pickLine2 = orderLine.PickLines.Single(l => l.WZ_Units == 5m);

			var divot1 = package.PackedItemDivots.AddNew();
			divot1.KI_PackedQty = 15;
			divot1.KI_ParentTableCode = "WZ";
			divot1.KI_ParentID = pickLine1.PK;

			var divot2 = package.PackedItemDivots.AddNew();
			divot2.KI_PackedQty = 5;
			divot2.KI_ParentTableCode = "WZ";
			divot2.KI_ParentID = pickLine2.PK;
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webService = GetNewWebService(data.Whs1);
			var result = webService.GetWhsReceives("Pkg1", true);

			var newReceive = Helper.Factory.Load<WhsReceive>(result.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 20m, newReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Receive line details are correct.", string.Empty, newReceiveLine1.WE_PartAttrib1);
			AssertEquals("Receive line details are correct.", string.Empty, newReceiveLine1.WE_PartAttrib2);
			AssertEquals("Receive line details are correct.", string.Empty, newReceiveLine1.WE_PartAttrib3);

			AssertEquals("New receive has 1 asn line.", 1, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 20m, newReceiveAsnLine1.WN_Quantity);
			AssertEquals("New receive's asn line details are correct.", string.Empty, newReceiveAsnLine1.WN_PartAttrib1);
			AssertEquals("New receive's asn line details are correct.", string.Empty, newReceiveAsnLine1.WN_PartAttrib2);
			AssertEquals("New receive's asn line details are correct.", string.Empty, newReceiveAsnLine1.WN_PartAttrib3);
		}

		public void TestGetWhsReceives_PackageIdReference_PackageContentsWithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.Factory.Save();

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, today.AddYears(2), today.AddDays(1), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, today.AddYears(1), today, "PB1", "PB2", "PB3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, today.AddYears(2), today.AddDays(1), "PA1", "PA2", "PA3", "", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m, today.AddYears(1), today, "PB1", "PB2", "PB3", "", "");

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			var divot1 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 10m);
			var divot2 = package.PackedItemDivots.Single(divot => divot.KI_PackedQty == 20m);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webService = GetNewWebService(data.Whs1);
			var result = webService.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result.Error);
			AssertEquals("Precondition: there are no errors.", null, result.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 2 lines.", 2, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 10m);
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", "PA1", newReceiveLine1.WE_PartAttrib1);
			AssertEquals("Receive line details are correct.", "PA2", newReceiveLine1.WE_PartAttrib2);
			AssertEquals("Receive line details are correct.", "PA3", newReceiveLine1.WE_PartAttrib3);
			AssertEquals("Receive line details are correct.", today.AddYears(2), newReceiveLine1.WE_ExpiryDate);
			AssertEquals("Receive line details are correct.", today.AddDays(1), newReceiveLine1.WE_PackingDate);

			var newReceiveLine2 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine2.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine2.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", "PB1", newReceiveLine2.WE_PartAttrib1);
			AssertEquals("Receive line details are correct.", "PB2", newReceiveLine2.WE_PartAttrib2);
			AssertEquals("Receive line details are correct.", "PB3", newReceiveLine2.WE_PartAttrib3);
			AssertEquals("Receive line details are correct.", today.AddYears(1), newReceiveLine2.WE_ExpiryDate);
			AssertEquals("Receive line details are correct.", today, newReceiveLine2.WE_PackingDate);

			AssertEquals("New receive has 2 ASN lines.", 2, newReceive.AsnLines.Count);
			var asnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(asnLine => asnLine.WN_Quantity == 10m);
			AssertEquals("ASN line details are correct.", data.Part1.PK, asnLine1.WN_OP);
			AssertEquals("ASN line details are correct.", "PA1", asnLine1.WN_PartAttrib1);
			AssertEquals("ASN line details are correct.", "PA2", asnLine1.WN_PartAttrib2);
			AssertEquals("ASN line details are correct.", "PA3", asnLine1.WN_PartAttrib3);
			AssertEquals("ASN line details are correct.", today.AddDays(1), asnLine1.WN_PackingDate);
			AssertEquals("ASN line details are correct.", today.AddYears(2), asnLine1.WN_ExpiryDate);

			var asnLine2 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(asnLine => asnLine.WN_Quantity == 20m);
			AssertEquals("ASN line details are correct.", data.Part1.PK, asnLine2.WN_OP);
			AssertEquals("ASN line details are correct.", "PB1", asnLine2.WN_PartAttrib1);
			AssertEquals("ASN line details are correct.", "PB2", asnLine2.WN_PartAttrib2);
			AssertEquals("ASN line details are correct.", "PB3", asnLine2.WN_PartAttrib3);
			AssertEquals("ASN line details are correct.", today, asnLine2.WN_PackingDate);
			AssertEquals("ASN line details are correct.", today.AddYears(1), asnLine2.WN_ExpiryDate);
		}

		public void TestGetWhsReceives_PackageIdReference_MaxPackageIdLength()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			AssertEquals("Precondition: package id max length is 46. Update this test if max length is updated.", 46, PkgPackageHeaderSchema.KPH_PackageID.MaxLength);
			AssertEquals("Precondition: reference max length is 25. Update this test if max length is updated.", 25, WhsDocketReferenceSchema.WX_Reference.MaxLength);

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "1234567890123456789012345678901234567890123456"; // length is 46
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "1234567890123456789012345678901234567890123457"; // length is 46
			package2.Pack(orderLine2.ReleaseLines[0], 20m);

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 2 packages.", 2, order.PackageJob.Packages.Count);
			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);
			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("1234567890123456789012345678901234567890123456", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive.Lines.Count);
			AssertEquals("New receive has 1 references.", 1, newReceive.References.Count);

			var reference = newReceive.References.Cast<WhsDocketReference>().Single();
			AssertEquals("Receive reference details are correct.", WarehouseAdditionalReferenceTypes.Codes.Other, reference.WX_RefType);
			AssertEquals("Receive reference details are correct.", "2345678901234567890123456", reference.WX_Reference);

			var newReceiveLine1 = newReceive.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceiveAsnLine1.WN_Quantity);

			var webservice2 = GetNewWebService(data.Whs1);
			var result2 = webservice2.GetWhsReceives("1234567890123456789012345678901234567890123456", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertEquals("Existing return receive is returned.", newReceive.PK, newReceive2.PK);
			AssertEquals("New receive still has 1 line.", 1, newReceive2.Lines.Count);
			AssertEquals("New receive still has 1 reference.", 1, newReceive2.References.Count);

			var newReceive2Line1 = newReceive2.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceive2Line1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceive2Line1.WE_ClientOrderedUnits);

			AssertEquals("New receive still has 1 asn line.", 1, newReceive2.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive2.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceive2AsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceive2AsnLine1.WN_Quantity);

			var webservice3 = GetNewWebService(data.Whs1);
			var result3 = webservice3.GetWhsReceives("1234567890123456789012345678901234567890123457", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive3 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertEquals("Existing return receive is returned.", newReceive.PK, newReceive3.PK);
			AssertEquals("New receive now has 2 lines.", 2, newReceive3.Lines.Count);
			AssertEquals("New receive now has 2 references.", 2,
				newReceive3.References.Cast<WhsDocketReference>().Count(docketRef => docketRef.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.Other));

			AssertContainsExactElementsInAnyOrder("Multiple references are created for the package id with long length.",
				new[] { "2345678901234567890123456", "2345678901234567890123457" },
				newReceive3.References.Cast<WhsDocketReference>().Select(docketRef => docketRef.WX_Reference.ToString())
				);

			var newReceive3Line1 = newReceive3.Lines.Single(line => line.WE_ClientOrderedUnits == 10m);
			var newReceive3Line2 = newReceive3.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceive3Line2.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line1.WE_TransactionQuantity);

			AssertEquals("Receive now has 2 asn lines.", 2, newReceive3.AsnLines.Count);
			var newReceive3AsnLine1 = newReceive3.AsnLines.Cast<WhsAsnLine>().Single(asnLine => asnLine.WN_Quantity == 10m);
			var newReceive3AsnLine2 = newReceive3.AsnLines.Cast<WhsAsnLine>().Single(asnLine => asnLine.WN_Quantity == 20m);
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceive3AsnLine2.WN_OP);
		}

		#endregion

		#region TestGetWhsReceive_PackageIdPK

		public void TestGetWhsReceive_PackageIdPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg2";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 2 packages.", 2, order.PackageJob.Packages.Count);
			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);
			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.GetWhsReceive(package1.PK.ToGuid(), true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			AssertEquals("New receive's details are correct.", "O1", newReceive.WD_ExternalReference);
			AssertEquals("New receive's details are correct.", data.Whs1.PK, newReceive.WD_WW_Whs);
			AssertEquals("New receive's details are correct.", data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals("New receive's details are correct.", ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertEquals("New receive has 1 line.", 1, newReceive.Lines.Count);

			var newReceiveLine1 = newReceive.Lines.Single();
			AssertEquals("Receive line details are correct.", data.Part1.PK, newReceiveLine1.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive line details are correct.", 10m, newReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("New receive has 1 asn line.", 1, newReceive.AsnLines.Count);
			var newReceiveAsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single();
			AssertEquals("New receive's asn line details are correct.", data.Part1.PK, newReceiveAsnLine1.WN_OP);
			AssertEquals("New receive's asn line details are correct.", 10m, newReceiveAsnLine1.WN_Quantity);

			var webservice2 = GetNewWebService(data.Whs1);
			var result2 = webservice2.GetWhsReceive(package2.PK.ToGuid(), true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result2.Error);
			AssertEquals("Precondition: there are no errors.", null, result2.ErrorMessage);

			var newReceive2 = Helper.Factory.Load<WhsReceive>(result2.Dockets.Single().PK);
			AssertEquals("Existing return receive is returned.", newReceive.PK, newReceive2.PK);
			AssertEquals("New receive has 2 lines.", 2, newReceive2.Lines.Count);

			var newReceive2Line1 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 10m);
			var newReceive2Line2 = newReceive.Lines.Single(line => line.WE_ClientOrderedUnits == 20m);
			AssertEquals("Receive line details are correct.", data.Part2.PK, newReceive2Line2.WE_OP);
			AssertEquals("Receive line details are correct.", 0m, newReceive2Line2.WE_TransactionQuantity);

			AssertEquals("New receive has 2 asn lines.", 2, newReceive.AsnLines.Count);
			var newReceive2AsnLine1 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 10m);
			var newReceive2AsnLine2 = newReceive.AsnLines.Cast<WhsAsnLine>().Single(line => line.WN_Quantity == 20m);
			AssertEquals("New receive's asn line details are correct.", data.Part2.PK, newReceive2AsnLine2.WN_OP);
		}

		#endregion

		#region TestGetWhsReceive_ReceiveIsReadonly

		public void TestGetWhsReceive_ReceiveIsReadonly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 2m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			Helper.Factory.Save();

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals(true, createdReceive.IsCreatedFromPickByBOM);

			var webService = GetNewWebService(data.Whs1);
			AssertNoDocketIsReturned(webService, createdReceive.WD_ExternalReference);
		}

		#endregion

		#region TestCopyBOMComponentLinks_PickLinesFromMultipleTypesOfInventoryPackedToOnePackage

		public void TestCopyBOMComponentLinks_PickLinesFromMultipleTypesOfInventoryPackedToOnePackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			part3.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");

			var receiveForPart3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receiveForPart3, part3, 1m);
			receiveForPart3.AllocateLocationsWithMock();
			receiveForPart3.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 1");
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder1, part3, 1m);
			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 2");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder2, part3, 1m);

			Helper.CreatePickNew(workOrder1);
			workOrder1.FinaliseDocketAlwaysFinalisingPick();

			Helper.CreatePickNew(workOrder2);
			workOrder2.FinaliseDocketAlwaysFinalisingPick();

			workOrder1.Receive.FinaliseDocketWithoutUserConfirmation();
			workOrder2.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, part3, 4m);
			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives(package.KP_PackageID, true);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);

			var returnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);
			AssertEquals("Should contain 3 receive lines", 3, returnReceive.Lines.Count);

			var receiveLinesFromPBBAndNormalReceiveLine = returnReceive.Lines.Where(l => l.WE_ClientOrderedUnits == 2 && l.WE_OP == part3.PK && !l.BOMComponentLinks.Any()).ToArray();
			AssertEquals(1, receiveLinesFromPBBAndNormalReceiveLine.Length);

			var expectedComponentsInventoryPKs = receiveForComponents.Lines.Select(l => l.PK).ToList();
			AssertReturnReceiveLines(returnReceive, 1, 2, part3.PK, expectedComponentsInventoryPKs);
		}

		#endregion

		#region TestCopyBOMComponentLinks_HaveMultipleGroupReleaseLinesFromWorkOrderInventory

		public void TestCopyBOMComponentLinks_HaveMultipleGroupReleaseLinesFromWorkOrderInventoryPackedToOnePackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForCompentPart = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForCompentPart, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForCompentPart, data.Part2, 10m);
			receiveForCompentPart.AllocateLocationsWithMock();
			receiveForCompentPart.FinaliseDocketWithoutUserConfirmation();

			var mainProduct1 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(mainProduct1, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(mainProduct1, data.Part2, 1m, "UNT");
			mainProduct1.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			mainProduct1.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			var mainProduct2 = Helper.CreateProduct(data.Org1, "P4");
			Helper.CreateProductBOM(mainProduct2, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(mainProduct2, data.Part2, 1m, "UNT");
			mainProduct2.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			mainProduct2.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct1, 1m);
			Helper.SetDocketLineAttributes(workOrderLine1, ZDate.Empty, ZDate.Empty, "color 1", "size 1", "", "");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct1, 2m);
			Helper.SetDocketLineAttributes(workOrderLine2, ZDate.Empty, ZDate.Empty, "color 2", "size 2", "", "");
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct2, 3m);
			Helper.SetDocketLineAttributes(workOrderLine3, ZDate.Empty, ZDate.Empty, "color 3", "size 3", "", "");
			var workOrderLine4 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct2, 4m);
			Helper.SetDocketLineAttributes(workOrderLine4, ZDate.Empty, ZDate.Empty, "color 4", "size 4", "", "");

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, mainProduct1, 3m);
			Helper.CreateWhsOrderLine(order, mainProduct2, 7m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(order.Lines[0].ReleaseLines[0], 1m);
			package.Pack(order.Lines[0].ReleaseLines[1], 2m);
			package.Pack(order.Lines[1].ReleaseLines[0], 3m);
			package.Pack(order.Lines[1].ReleaseLines[1], 4m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives(package.KP_PackageID, true);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);

			var returnnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);
			AssertEquals("Should contain 4 receive lines", 4, returnnReceive.Lines.Count);

			var expectedComponetsPartPKs = receiveForCompentPart.Lines.Select(l => l.PK).ToList();
			AssertReturnReceiveLines(returnnReceive, 1, 1, mainProduct1.PK, expectedComponetsPartPKs);
			AssertReturnReceiveLines(returnnReceive, 2, 1, mainProduct1.PK, expectedComponetsPartPKs);
			AssertReturnReceiveLines(returnnReceive, 3, 1, mainProduct2.PK, expectedComponetsPartPKs);
			AssertReturnReceiveLines(returnnReceive, 4, 1, mainProduct2.PK, expectedComponetsPartPKs);
		}

		#endregion

		#region TestCopyBOMComponentLinks_MultipleOrderLinesFromSaveInventoryPackedToOnePackage

		public void TestCopyBOMComponentLinks_MultipleOrderLinesFromSameInventoryToOnePackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 1m);
			Helper.CreateWhsOrderLine(order, part3, 1m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(order.Lines[0].ReleaseLines[0], 1m);
			package.Pack(order.Lines[1].ReleaseLines[0], 1m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives(package.KP_PackageID, true);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);

			var returnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);
			AssertEquals("Should contain 1 receive lines", 1, returnReceive.Lines.Count);

			AssertReturnReceiveLines(returnReceive, 2, 1, part3.PK, receive.Lines.Select(l => l.PK).ToList());
		}

		#endregion

		#region TestCopyBOMComponentLinks_OrderLineFromSameInventoryToTwoPackages

		public void TestCopyBOMComponentLinks_OrderLineFromSameInventoryToTwoPackages()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 2m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(order.Lines[0].ReleaseLines[0], 1m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg2";
			package2.Pack(order.Lines[0].ReleaseLines[0], 1m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			AssertResult(package1, 1, 1);
			AssertResult(package2, 1, 2);

			void AssertResult(PkgPackage package, int expectedQuantity, int expectedLinesCount)
			{
				var response = webService.GetWhsReceives(package.KP_PackageID, true);

				AssertSuccessfulResponse(response, webService);
				AssertNotNull("Dockets should not be null", response.Dockets);

				var returnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);
				AssertEquals($"Should contain {expectedLinesCount} receive lines", expectedLinesCount, returnReceive.Lines.Count);

				AssertReturnReceiveLines(returnReceive, expectedQuantity, expectedLinesCount, part3.PK, receive.Lines.Select(l => l.PK).ToList());
			}
		}

		#endregion

		#region TestCopyBOMComponentLinks_AllAttributesBeenRetained

		[TestDate(2021, 8, 20)]
		public void TestCopyBOMComponentLinks_AllAttributesBeenRetained()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var expiryDate = ZDate.Today;
			var packingDate = ZDate.Today;
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "PLT-1", expiryDate, packingDate, "A", "B", "C", "S1", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].Allocate = true;

			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(order.Lines[0].ReleaseLines[0], 1m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives(package.KP_PackageID, true);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);

			var returnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);

			Assert("All attributes are retained for new inventory", returnReceive.Lines.All(l => l.WE_PartAttrib1 == "A" && l.WE_PartAttrib2 == "B" && l.WE_PartAttrib3 == "C" && l.WE_SerialNumber == "S1" && l.WE_ExpiryDate == expiryDate && l.WE_PackingDate == packingDate));
		}

		#endregion

		#region TestDeleteBomLinksWhenReturnToOnePackage

		public void TestDeleteBomLinksWhenReturnToOnePackage_FullReturn()
		{
			TestDeleteBomLinksWhenReturnToOnePackageCore(true);
		}

		public void TestDeleteBomLinksWhenReturnToOnePackage_PartialReturn()
		{
			TestDeleteBomLinksWhenReturnToOnePackageCore(false);
		}

		void TestDeleteBomLinksWhenReturnToOnePackageCore(bool isFullReturn)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, part3, 3m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 3m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(order.Lines[0].ReleaseLines[0], 3m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives(package.KP_PackageID, true);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);

			var returnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);
			AssertEquals("Should contain 1 receive lines", 1, returnReceive.Lines.Count);

			var returnReceiveLine = returnReceive.Lines.Where(line => line.WE_ClientOrderedUnits == 3 && line.WE_OP == part3.PK && line.BOMComponentLinks.Any()).ToList();
			AssertEquals("Should be 1 receive line with BOMLinks", 1, returnReceiveLine.Count);

			IReceiveLinesUpdater receiveLinesUpdater = new ReceiveLinesUpdaterForUnload();

			AssertNoExceptionThrown(() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty((WhsReceive)returnReceive, returnReceiveLine.Cast<WhsReceiveLine>().ToList(), isFullReturn ? 3 : 1, "UNT", workOrder.Receive.Lines[0].WE_WL, string.Empty, false));

			returnReceive.FinaliseDocketWithoutUserConfirmation();
			if (isFullReturn)
			{
				AssertEquals(2, returnReceiveLine[0].BOMComponentLinks.Count());
			}
			else
			{
				AssertEquals(0, returnReceiveLine[0].BOMComponentLinks.Count());
			}
		}

		#endregion

		#region TestDeleteBomLinksForPartialReturnedReceiveLinesIfHaveMultipleLines

		public void TestDeleteBomLinksForPartialReturnedReceiveLinesIfHaveMultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 1m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 1m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 2m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(order.Lines[0].ReleaseLines[0], 2m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives(package.KP_PackageID, true);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);

			var returnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);
			AssertEquals("Should contain 2 receive lines", 2, returnReceive.Lines.Count);

			var returnReceiveLines = returnReceive.Lines.Where(line => line.WE_ClientOrderedUnits == 1 && line.WE_OP == part3.PK && line.BOMComponentLinks.Any()).ToList();
			AssertEquals("Should be 2 receive lines with BOMLinks", 2, returnReceiveLines.Count);

			IReceiveLinesUpdater receiveLinesUpdater = new ReceiveLinesUpdaterForUnload();
			AssertNoExceptionThrown(() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty((WhsReceive)returnReceive, returnReceiveLines.Cast<WhsReceiveLine>().ToList(), 1, "UNT", workOrder.Receive.Lines[0].WE_WL, string.Empty, false));

			returnReceive.FinaliseDocketWithoutUserConfirmation();

			AssertEquals(1, returnReceive.Lines.Count);
			AssertEquals(false, returnReceive.Lines[0].BOMComponentLinks.Any());
		}

		#endregion

		#region TestNotDeleteBomLinksForFullReturnedReceiveLinesWhenReturnedInManyTimes

		public void TestNotDeleteBomLinksForFullReturnedReceiveLinesWhenReturnedInManyTimes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, part3, 3m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 3m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(order.Lines[0].ReleaseLines[0], 3m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsReceives(package.KP_PackageID, true);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);

			var returnReceive = Helper.Factory.Load<WhsDocket>(response.Dockets[0].PK);
			AssertEquals("Should contain 1 receive lines", 1, returnReceive.Lines.Count);

			var returnReceiveLine = returnReceive.Lines.Where(line => line.WE_ClientOrderedUnits == 3 && line.WE_OP == part3.PK && line.BOMComponentLinks.Any()).ToList();
			AssertEquals("Should be 1 receive line with BOMLinks", 1, returnReceiveLine.Count);

			IReceiveLinesUpdater receiveLinesUpdater = new ReceiveLinesUpdaterForUnload();
			AssertNoExceptionThrown(() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty((WhsReceive)returnReceive, returnReceiveLine.Cast<WhsReceiveLine>().ToList(), 1, "UNT", workOrder.Receive.Lines[0].WE_WL, string.Empty, false));
			AssertNoExceptionThrown(() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty((WhsReceive)returnReceive, returnReceiveLine.Cast<WhsReceiveLine>().ToList(), 2, "UNT", workOrder.Receive.Lines[0].WE_WL, string.Empty, false));

			returnReceive.FinaliseDocketWithoutUserConfirmation();
			Assert("BOMLinks should be retained for fully returned receive line even if it's returned in several times", returnReceiveLine[0].BOMComponentLinks.Any());
		}

		#endregion

		#region TestGetWhsReceives_TaskManagement

		public void TestGetWhsReceives_TaskManagement()
		{
			TestGetWhsReceives_TaskManagementCore(hasReleaseGroup: true);
		}

		public void TestGetWhsReceives_TaskManagement_NoReleaseGroup()
		{
			TestGetWhsReceives_TaskManagementCore(hasReleaseGroup: false);
		}

		void TestGetWhsReceives_TaskManagementCore(bool hasReleaseGroup)
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
				var staff = Helper.CreateGlbStaff("TST", "Test");
				if (hasReleaseGroup)
				{
					var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
					data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
				}
				Helper.Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

				var task = Helper.CreateProcessTaskForReceive(receive, staff);
				Helper.Factory.Save();

				var webService = GetNewWebService(data.Whs1, staff);
				AssertCorrectDocketIsReturned(webService, "Ref1", receive, hasReleaseGroup ? task : null);
			}
		}

		public void TestGetWhsReceives_TaskManagementTaskError()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
				var staff1 = Helper.CreateGlbStaff("TST", "Test");
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
				Helper.Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

				var task = Helper.CreateProcessTaskForReceive(receive, staff1);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Helper.Factory.Save();

				var webService = GetNewWebService(data.Whs1, staff1);
				var response = webService.GetWhsReceives("Ref1", true);
				AssertBusinessValidationError(webService, "The task is already closed and cannot be updated. Please check the task status and try again.", response);
				AssertNull("Dockets should be null", response.Dockets);
			}
		}

		public void TestGetWhsReceives_TaskManagement_TaskOnJobAssignToAnotherUser()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
				var staff1 = Helper.CreateGlbStaff("TST", "Test");
				var staff2 = Helper.CreateGlbStaff("TS2", "Tes2");
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
				Helper.Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

				Helper.CreateProcessTaskForReceive(receive, staff2);
				Helper.Factory.Save();

				var webService = GetNewWebService(data.Whs1, staff1);
				var response = webService.GetWhsReceives("Ref1", true);
				var task = webService.Factory.LoadTop1<WhsReceiveProcessTasks>(new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff1.GS_Code));
				AssertCorrectDocketIsReturned(webService, "Ref1", receive, task);
			}
		}

		public void TestGetWhsReceives_TaskManagement_SuspendedTaskAssignedToUser()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
				var staff = Helper.CreateGlbStaff("TST", "Test");

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
				Helper.Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

				var task = Helper.CreateProcessTaskForReceive(receive, staff);
				task.P9_Status = "SUS";
				Helper.Factory.Save();

				Assert("Precondition: Task is suspended", task.IsSuspended);

				var webService = GetNewWebService(data.Whs1, staff);
				AssertCorrectDocketIsReturned(webService, "Ref1", receive, task);
			}
		}

		public void TestGetWhsReceives_TaskManagement_MultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsReceives("NONEXISTANTREF", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(0, response1.Dockets.Length);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1");
			var task1 = Helper.CreateProcessTaskForReceive(receive1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "REF1");
			Helper.Factory.Save();

			var response2 = webService2.GetWhsReceives("REF1", false);
			AssertContainsExactElementsInAnyOrder(new[] { receive1.WD_DocketID }, response2.Dockets.Select(d => d.DocketID));
			AssertEquals("Task1 PK should be correct", task1.PK, response2.Dockets[0].TaskPK);

			var webService3 = GetNewWebService(data.Whs1, staff);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF2");
			var task2 = Helper.CreateProcessTaskForReceive(receive2);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1");
			receive3.WD_ExternalReferenceSplit = 1;

			var receive4 = Helper.CreateWhsReceive(Helper.CreateClient("C2"), data.Whs1, "REF1");
			var task4 = Helper.CreateProcessTaskForReceive(receive4);
			Helper.Factory.Save();

			var response3 = webService3.GetWhsReceives("REF1", false);

			var orderedDockets = response3.Dockets.OrderBy(d => d.DocketID).ToArray();
			AssertEquals("Expect 3 receives", 3, orderedDockets.Length);
			AssertEquals("Receive1 DocketID", receive1.WD_DocketID, orderedDockets[0].DocketID);
			AssertEquals("Receive1 TaskPK", task1.PK, orderedDockets[0].TaskPK);
			AssertEquals("Receive3 DocketID", receive3.WD_DocketID, orderedDockets[1].DocketID);
			AssertEquals("Receive3 TaskPK", Guid.Empty, orderedDockets[1].TaskPK);
			AssertEquals("Receive4 DocketID", receive4.WD_DocketID, orderedDockets[2].DocketID);
			AssertEquals("Receive4 TaskPK", task4.PK, orderedDockets[2].TaskPK);
		}

		#endregion

		#region TestGetWhsReceive_UnloadCompleteTimeSet

		public void TestGetWhsReceive_UnloadCompleteTimeSet_Docket()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Ref1", ZDateTimeOffset.Empty);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			receive2.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webService, "Ref1", receive1);
			AssertUnloadTimeSetErrorIsReturned(webService, "Ref2");

			var response = webService.GetWhsReceives("Ref2", false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(receive2.PK.ToGuid(), response.Dockets[0].PK);
		}

		public void TestGetWhsReceive_UnloadCompleteTimeSet_PackageId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg2";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Helper.Factory.Save();

			AssertEquals("Precondition: there are 2 packages.", 2, order.PackageJob.Packages.Count);
			var divot1 = package1.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 10m, divot1.KI_PackedQty);

			var divot2 = package2.PackedItemDivots.Single();
			AssertEquals("Precondition: package divot is correct.", 20m, divot2.KI_PackedQty);
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);

			var webservice1 = GetNewWebService(data.Whs1);
			var result1 = webservice1.GetWhsReceives("Pkg1", true);
			AssertEquals("Precondition: there are no errors.", ErrorTypes.None, result1.Error);
			AssertEquals("Precondition: there are no errors.", null, result1.ErrorMessage);

			var newReceive = Helper.Factory.Load<WhsReceive>(result1.Dockets.Single().PK);
			newReceive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var webservice2 = GetNewWebService(data.Whs1);
			AssertUnloadTimeSetErrorIsReturned(webservice2, "Pkg2");

			var response = webservice2.GetWhsReceives("Pkg2", false);
			AssertSuccessfulResponse(response, webservice2);
		}

		public void TestGetWhsReceive_UnloadCompleteTimeSet_ContainerNo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1");
			CreateContainerForDocket(receive1, "Con1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref2");
			receive2.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			CreateContainerForDocket(receive2, "Con2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20m);
			Helper.Factory.Save();

			var webservice = GetNewWebService();
			webservice.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertCorrectDocketIsReturned(webservice, "Con1", receive1);
			AssertUnloadTimeSetErrorIsReturned(webservice, "Con2");

			var response = webservice.GetWhsReceives("Con2", false);
			AssertSuccessfulResponse(response, webservice);
		}

		void AssertUnloadTimeSetErrorIsReturned(WhsSecureService webService, string reference)
		{
			var response = webService.GetWhsReceives(reference, true);
			CombineAssertions(() =>
			{
				AssertEquals("The Receive is already unloaded.", response.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should not load the receive", true, response.Dockets.IsNullOrEmpty());
			});
		}

		#endregion

		public void TestDbHitsForWhsBOMInventoryPivot()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");

			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			for (int i = 0; i < 10; i++)
			{
				Helper.CreateWhsWorkOrderLine(workOrder, part3, 1m);
			}
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Helper.Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DtbBookingConsolidationSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 }, //Implementation of denied party resynchronization screening status
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsClientParameterByWarehouseSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 10 }, //CheckExternalReferenceForDuplicates when a new docket is setting property WD_ExternalReference
				{ WhsDocketJobPivotSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 3 }, //Implementation of denied party resynchronization screening status
				{ WhsPickLineSchema.Constants.TableName, 2 },
			};

			var webService = GetNewWebService();

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService.Factory))
			using (RowFactory.SetCachedTables())
			{
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				webService.GetWhsReceives(package.KP_PackageID, true);
			}
		}

		void AssertReturnReceiveLines(WhsDocket returnReceive, int expectedQuantity, int expectedLineCount, ZGuid originalPartPK, List<ZGuid> componentLinePKs)
		{
			var returnReceiveLine = returnReceive.Lines.Where(line => line.WE_ClientOrderedUnits == expectedQuantity && line.WE_OP == originalPartPK && line.BOMComponentLinks.Any()).ToList();
			AssertEquals($"Should be {expectedLineCount} receive line", expectedLineCount, returnReceiveLine.Count);
			for (int i = 0; i < expectedLineCount; i++)
			{
				var receiveLine = returnReceiveLine[i];
				AssertEquals(2, receiveLine.ComponentInventoryLines.Count);
				Assert(receiveLine.BOMComponentLinks.All(link => link.WIP_ComponentQuantity == expectedQuantity));
				Assert(receiveLine.ComponentInventoryLines.All(l => componentLinePKs.Contains(l.PK)));
			}
		}

		[TestDate(2025, 03, 19)]
		public void TestGetWhsReceives_ConcurrencyException()
		{
			var arrivalDate = ZDateTimeOffset.Now.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receiveWithEmptyArrivalDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty, Notify);
			Helper.Factory.Save();

			AssertEquals("Pre-condition", ZDateTimeOffset.Empty, receiveWithEmptyArrivalDate.WD_ArrivalDate);

			var webService = GetNewWebService(data.Whs1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)receiveWithEmptyArrivalDate).Row, TestConnection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.GetWhsReceives("R1", false);
			AssertBusinessValidationError(webService, "Another user has made changes while you have been loading this receive. Please restart the operation and try again.", response);

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsDocket>(receiveWithEmptyArrivalDate.PK);
			AssertEquals("Arrival date is empty", ZDateTimeOffset.Empty, receiveInNewFactory.WD_ArrivalDate);
		}

		#region Implementation

		#region CreateReferenceForDocket

		WhsDocketReference CreateReferenceForDocket(WhsDocket docket, string reference)
		{
			var refdocketReference = docket.References.AddNew();
			refdocketReference.WX_Reference = reference;
			refdocketReference.WX_RefType = refdocketReference.Lookups.ReferenceTypes[0].Code;

			return refdocketReference;
		}

		#endregion

		#region CreateContainerForDocket

		WhsDocketContainer CreateContainerForDocket(WhsDocket docket, string containerNum)
		{
			var container = docket.Containers.AddNew();
			container.WC_ContainerNum = containerNum;

			return container;
		}

		#endregion

		#region AssertResponse

		void AssertCorrectDocketIsReturned(WhsSecureService webService, string reference, WhsDocket docket, ProcessTask task = null)
		{
			var response = webService.GetWhsReceives(reference, true);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);
			AssertEquals("DocketPK should be equal", docket.PK.ToGuid(), response.Dockets[0].PK);
			AssertEquals("DocketID should be equal", docket.WD_DocketID, response.Dockets[0].DocketID);
			AssertEquals("TaskPK should be correct", task?.PK ?? ZGuid.Empty, response.Dockets[0].TaskPK);
		}

		void AssertNoDocketIsReturned(WhsSecureService webService, string reference)
		{
			var response = webService.GetWhsReceives(reference, true);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);
			AssertEquals("No dockets be return", 0, response.Dockets.Length);
		}

		void AssertDocketsIsReturned(WhsSecureService webService, string reference, int count)
		{
			var response = webService.GetWhsReceives(reference, true);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Dockets should not be null", response.Dockets);
			AssertEquals($"{count} dockets should be returned", count, response.Dockets.Length);
		}

		#endregion

		#endregion

		#endregion
	}
}
