using System;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WarehouseControllerTest : TestCaseWithFactory
	{
		public void TestMarkInventoriesLost_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestMarkInventoriesLostCore(true);
		}

		public void TestMarkInventoriesLost_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestMarkInventoriesLostCore(false);
		}

		void TestMarkInventoriesLostCore(bool isWebUser)
		{
			var isServiceCalled = false;
			var cycleCountPKs = new[] { ZGuid.NewZGuid() };

			cycleCountServiceMock
				.Setup(m => m.MarkInventoriesLostInCycleCount(It.IsAny<ZGuid[]>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(cycleCountServiceMock.Object))
			{
				var response = controller.MarkInventoriesLost(new[] { cycleCountPKs[0].ToGuid() }).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestMarkInventoriesLost_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					var cycleCountPKs = new[] { ZGuid.NewZGuid() };
					using (ObjectFactory.Substitute(cycleCountServiceMock.Object))
					{
						controller.MarkInventoriesLost(new[] { cycleCountPKs[0].ToGuid() });
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		public void TestCreateDockDoorTransfer_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestCreateDockDoorTransferCore(true);
		}

		public void TestCreateDockDoorTransfer_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestCreateDockDoorTransferCore(false);
		}

		void TestCreateDockDoorTransferCore(bool isWebUser)
		{
			var isServiceCalled = false;
			var pk = ZGuid.NewZGuid();

			packingConsolidationService
				.Setup(m => m.CreateDockDoorTransfer(It.IsAny<ZGuid>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var response = controller.CreateDockDoorTransfer(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestCreateDockDoorTransfer_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pk = ZGuid.NewZGuid();

			packingConsolidationService
				.Setup(m => m.CreateDockDoorTransfer(It.IsAny<ZGuid>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var response = controller.CreateDockDoorTransfer(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestCreateDockDoorTransfer_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pk = ZGuid.NewZGuid();

			packingConsolidationService
				.Setup(m => m.CreateDockDoorTransfer(It.IsAny<ZGuid>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var response = controller.CreateDockDoorTransfer(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestCreateDockDoorTransfer_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					var pk = ZGuid.NewZGuid();
					using (ObjectFactory.Substitute(packingConsolidationService.Object))
					{
						controller.CreateDockDoorTransfer(pk.ToGuid());
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		public void TestPutawayStockInDockDoor_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestPutawayStockInDockDoorCore(true);
		}

		public void TestPutawayStockInDockDoor_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestPutawayStockInDockDoorCore(false);
		}

		void TestPutawayStockInDockDoorCore(bool isWebUser)
		{
			var isServiceCalled = false;
			var pk = ZGuid.NewZGuid();

			packingConsolidationService
				.Setup(m => m.PutawayStockInDockDoor(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var response = controller.PutawayStockInDockDoor(pk.ToGuid(), pk.ToGuid(), pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestPutawayStockInDockDoor_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pk = ZGuid.NewZGuid();

			packingConsolidationService
				.Setup(m => m.PutawayStockInDockDoor(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var response = controller.PutawayStockInDockDoor(pk.ToGuid(), pk.ToGuid(), pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestPutawayStockInDockDoor_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pk = ZGuid.NewZGuid();

			packingConsolidationService
				.Setup(m => m.PutawayStockInDockDoor(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var response = controller.PutawayStockInDockDoor(pk.ToGuid(), pk.ToGuid(), pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestPutawayStockInDockDoor_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					var pk = ZGuid.NewZGuid();
					using (ObjectFactory.Substitute(packingConsolidationService.Object))
					{
						controller.PutawayStockInDockDoor(pk.ToGuid(), pk.ToGuid(), pk.ToGuid());
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		public void TestGenerateHandlingUnit_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestGenerateHandlingUnitCore(true);
		}

		public void TestGenerateHandlingUnit_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestGenerateHandlingUnitCore(false);
		}

		void TestGenerateHandlingUnitCore(bool isWebUser)
		{
			var isServiceCalled = false;

			packingConsolidationService
				.Setup(m => m.GenerateHandlingUnit(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var response = controller.GenerateHandlingUnit(pk.ToGuid(), pk.ToGuid(), pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestGenerateHandlingUnit_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			packingConsolidationService
				.Setup(m => m.GenerateHandlingUnit(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var response = controller.GenerateHandlingUnit(pk.ToGuid(), pk.ToGuid(), pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestGenerateHandlingUnit_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			packingConsolidationService
				.Setup(m => m.GenerateHandlingUnit(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(packingConsolidationService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var response = controller.GenerateHandlingUnit(pk.ToGuid(), pk.ToGuid(), pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestGenerateHandlingUnit_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					using (ObjectFactory.Substitute(packingConsolidationService.Object))
					{
						var pk = ZGuid.NewZGuid();
						controller.GenerateHandlingUnit(pk.ToGuid(), pk.ToGuid(), pk.ToGuid());
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#region TestCreateAdHocServiceJob

		public void TestCreateAdHocServiceJob_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestCreateAdHocServiceJobCore(true);
		}

		public void TestCreateAdHocServiceJob_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestCreateAdHocServiceJobCore(false);
		}

		void TestCreateAdHocServiceJobCore(bool isWebUser)
		{
			var isServiceCalled = false;

			glowServicesModuleService
				.Setup(m => m.CreateAdHocServiceJob(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDate>(), It.IsAny<ZString>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new CreateAdHocServiceJobArgs() { JobPK = pk.ToGuid(), ClientPK = pk.ToGuid(), WarehousePK = pk.ToGuid(), BillingDate = DateTime.Today };
				var response = controller.CreateAdHocServiceJob(args).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestCreateAdHocServiceJob_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CreateAdHocServiceJob(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDate>(), It.IsAny<ZString>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new CreateAdHocServiceJobArgs() { JobPK = pk.ToGuid(), ClientPK = pk.ToGuid(), WarehousePK = pk.ToGuid(), BillingDate = DateTime.Today };
				var response = controller.CreateAdHocServiceJob(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestCreateAdHocServiceJob_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CreateAdHocServiceJob(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDate>(), It.IsAny<ZString>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new CreateAdHocServiceJobArgs() { JobPK = pk.ToGuid(), ClientPK = pk.ToGuid(), WarehousePK = pk.ToGuid(), BillingDate = DateTime.Today };
				var response = controller.CreateAdHocServiceJob(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestCreateAdHocServiceJob_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					using (ObjectFactory.Substitute(glowServicesModuleService.Object))
					{
						var pk = ZGuid.NewZGuid();
						var args = new CreateAdHocServiceJobArgs() { JobPK = pk.ToGuid(), ClientPK = pk.ToGuid(), WarehousePK = pk.ToGuid(), BillingDate = DateTime.Today };
						controller.CreateAdHocServiceJob(args);
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestCreateWhsJobService

		public void TestCreateWhsJobService_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestCreateWhsJobServiceCore(true);
		}

		public void TestCreateWhsJobService_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestCreateWhsJobServiceCore(false);
		}

		void TestCreateWhsJobServiceCore(bool isWebUser)
		{
			var isServiceCalled = false;

			glowServicesModuleService
				.Setup(m => m.CreateWhsJobService(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDateTimeOffset>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var servicePk = ZGuid.NewZGuid();
				var jobPk = ZGuid.NewZGuid();
				var contractorPk = ZGuid.NewZGuid();
				var locationPk = ZGuid.NewZGuid();
				var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, JobPK = jobPk.ToGuid(), JobType = "WSJ", BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
				var response = controller.CreateWhsJobService(args).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestCreateWhsJobService_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CreateWhsJobService(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDateTimeOffset>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var servicePk = ZGuid.NewZGuid();
				var jobPk = ZGuid.NewZGuid();
				var contractorPk = ZGuid.NewZGuid();
				var locationPk = ZGuid.NewZGuid();
				var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, JobPK = jobPk.ToGuid(), JobType = "WSJ", BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
				var response = controller.CreateWhsJobService(args).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestCreateWhsJobService_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CreateWhsJobService(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDateTimeOffset>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var servicePk = ZGuid.NewZGuid();
				var jobPk = ZGuid.NewZGuid();
				var contractorPk = ZGuid.NewZGuid();
				var locationPk = ZGuid.NewZGuid();
				var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, JobPK = jobPk.ToGuid(), JobType = "WSJ", BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
				var response = controller.CreateWhsJobService(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestCreateWhsJobService_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					using (ObjectFactory.Substitute(glowServicesModuleService.Object))
					{
						var servicePk = ZGuid.NewZGuid();
						var jobPk = ZGuid.NewZGuid();
						var contractorPk = ZGuid.NewZGuid();
						var locationPk = ZGuid.NewZGuid();
						var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, JobPK = jobPk.ToGuid(), JobType = "WSJ", BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
						controller.CreateWhsJobService(args);
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestCompleteWhsJobService

		public void TestCompleteWhsJobService_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestCompleteWhsJobServiceCore(true);
		}

		public void TestCompleteWhsJobService_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestCompleteWhsJobServiceCore(false);
		}

		void TestCompleteWhsJobServiceCore(bool isWebUser)
		{
			var isServiceCalled = false;

			glowServicesModuleService
				.Setup(m => m.CompleteWhsJobService(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDateTimeOffset>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<bool>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var servicePk = ZGuid.NewZGuid();
				var jobPk = ZGuid.NewZGuid();
				var contractorPk = ZGuid.NewZGuid();
				var locationPk = ZGuid.NewZGuid();
				var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
				var response = controller.CompleteWhsJobService(args).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestCompleteWhsJobService_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CompleteWhsJobService(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDateTimeOffset>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<bool>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var servicePk = ZGuid.NewZGuid();
				var jobPk = ZGuid.NewZGuid();
				var contractorPk = ZGuid.NewZGuid();
				var locationPk = ZGuid.NewZGuid();
				var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
				var response = controller.CompleteWhsJobService(args).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestCompleteWhsJobService_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CompleteWhsJobService(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZDecimal>(), It.IsAny<ZDateTimeOffset>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<bool>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var servicePk = ZGuid.NewZGuid();
				var jobPk = ZGuid.NewZGuid();
				var contractorPk = ZGuid.NewZGuid();
				var locationPk = ZGuid.NewZGuid();
				var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
				var response = controller.CompleteWhsJobService(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestCompleteWhsJobService_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					using (ObjectFactory.Substitute(glowServicesModuleService.Object))
					{
						var servicePk = ZGuid.NewZGuid();
						var jobPk = ZGuid.NewZGuid();
						var contractorPk = ZGuid.NewZGuid();
						var locationPk = ZGuid.NewZGuid();
						var args = new WhsJobServiceArgs() { ServicePK = servicePk.ToGuid(), ServiceType = Core.Constants.FreightServiceType.Codes.Fumigation, ServiceCount = 5m, BookedDateTimeOffset = DateTimeOffset.Now, Contractor = contractorPk.ToGuid(), Duration = 36000, LocationPK = locationPk.ToGuid(), SubLocation = "Test Sub Location", Reference = "Test Reference", Note = "Test Note" };
						controller.CompleteWhsJobService(args);
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestCompleteAllWhsJobServices

		public void TestCompleteAllWhsJobServices_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestCompleteAllWhsJobServicesCore(true);
		}

		public void TestCompleteAllWhsJobServices_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestCompleteAllWhsJobServicesCore(false);
		}

		void TestCompleteAllWhsJobServicesCore(bool isWebUser)
		{
			var isServiceCalled = false;

			glowServicesModuleService
				.Setup(m => m.CompleteAllWhsJobServices(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<bool>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var jobPk = ZGuid.NewZGuid();
				var args = new CompleteAllWhsJobServicesArgs() { JobPK = jobPk.ToGuid(), JobType = WhsAdHocServiceJobSchema.Constants.Prefix, FinaliseServiceJob = true };
				var response = controller.CompleteAllWhsJobServices(args).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestCompleteAllWhsJobServices_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CompleteAllWhsJobServices(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<bool>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var jobPk = ZGuid.NewZGuid();
				var args = new CompleteAllWhsJobServicesArgs() { JobPK = jobPk.ToGuid(), JobType = WhsAdHocServiceJobSchema.Constants.Prefix, FinaliseServiceJob = true };
				var response = controller.CompleteAllWhsJobServices(args).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestCompleteAllWhsJobServices_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			glowServicesModuleService
				.Setup(m => m.CompleteAllWhsJobServices(It.IsAny<ZGuid>(), It.IsAny<ZString>(), It.IsAny<bool>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(glowServicesModuleService.Object))
			{
				var servicePk = ZGuid.NewZGuid();
				var jobPk = ZGuid.NewZGuid();
				var contractorPk = ZGuid.NewZGuid();
				var locationPk = ZGuid.NewZGuid();
				var args = new CompleteAllWhsJobServicesArgs() { JobPK = jobPk.ToGuid(), JobType = WhsAdHocServiceJobSchema.Constants.Prefix, FinaliseServiceJob = true };
				var response = controller.CompleteAllWhsJobServices(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestCompleteAllWhsJobServices_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					using (ObjectFactory.Substitute(glowServicesModuleService.Object))
					{
						var servicePk = ZGuid.NewZGuid();
						var jobPk = ZGuid.NewZGuid();
						var contractorPk = ZGuid.NewZGuid();
						var locationPk = ZGuid.NewZGuid();
						var args = new CompleteAllWhsJobServicesArgs() { JobPK = jobPk.ToGuid(), JobType = WhsAdHocServiceJobSchema.Constants.Prefix, FinaliseServiceJob = true };
						controller.CompleteAllWhsJobServices(args);
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestChangeTaskPlanningStatus

		public void TestChangeTaskPlanningStatus_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestChangeTaskPlanningStatusCore(true);
		}

		public void TestChangeTaskPlanningStatus_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestChangeTaskPlanningStatusCore(false);
		}

		void TestChangeTaskPlanningStatusCore(bool isWebUser)
		{
			var isServiceCalled = false;
			taskManagementServiceMock
				.Setup(m => m.ChangeTaskPlanningStatus(It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new ChangeTaskPlanningStatusArgs { JobPK = pk.ToGuid(), JobType = string.Empty, ChangeStatusToReady = true };
				var response = controller.ChangeTaskPlanningStatus(args).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestChangeTaskPlanningStatus_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			taskManagementServiceMock
				.Setup(m => m.ChangeTaskPlanningStatus(It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new ChangeTaskPlanningStatusArgs { JobPK = pk.ToGuid(), JobType = string.Empty, ChangeStatusToReady = true };
				var response = controller.ChangeTaskPlanningStatus(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestChangeTaskPlanningStatus_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			taskManagementServiceMock
				.Setup(m => m.ChangeTaskPlanningStatus(It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new ChangeTaskPlanningStatusArgs { JobPK = pk.ToGuid(), JobType = string.Empty, ChangeStatusToReady = true };
				var response = controller.ChangeTaskPlanningStatus(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestChangeTaskPlanningStatus_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			ExceptionDispatchInfo exceptionInfo = null;
			var thread = new Thread(() =>
			{
				try
				{
					using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
					{
						var pk = ZGuid.NewZGuid();
						var args = new ChangeTaskPlanningStatusArgs { JobPK = pk.ToGuid(), JobType = string.Empty, ChangeStatusToReady = true };
						controller.ChangeTaskPlanningStatus(args);
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestBatchChangeTaskPlanningStatus

		public void TestBatchChangeTaskPlanningStatus_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestBatchChangeTaskPlanningStatusCore(true);
		}

		public void TestBatchChangeTaskPlanningStatus_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestBatchChangeTaskPlanningStatusCore(false);
		}

		void TestBatchChangeTaskPlanningStatusCore(bool isWebUser)
		{
			var isServiceCalled = false;
			taskManagementServiceMock
				.Setup(m => m.BatchChangeTaskPlanningStatus(It.IsAny<ZGuid[]>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new BatchChangeTaskPlanningStatusArgs { JobPKs = [pk.ToGuid()], JobType = string.Empty, ChangeStatusToReady = true };
				var response = controller.BatchChangeTaskPlanningStatus(args).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestBatchChangeTaskPlanningStatus_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			taskManagementServiceMock
				.Setup(m => m.BatchChangeTaskPlanningStatus(It.IsAny<ZGuid[]>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new BatchChangeTaskPlanningStatusArgs { JobPKs = [pk.ToGuid()], JobType = string.Empty, ChangeStatusToReady = true };
				var response = controller.BatchChangeTaskPlanningStatus(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestBatchChangeTaskPlanningStatus_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			taskManagementServiceMock
				.Setup(m => m.BatchChangeTaskPlanningStatus(It.IsAny<ZGuid[]>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var pk = ZGuid.NewZGuid();
				var args = new BatchChangeTaskPlanningStatusArgs { JobPKs = [pk.ToGuid()], JobType = string.Empty, ChangeStatusToReady = true };
				var response = controller.BatchChangeTaskPlanningStatus(args).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestBatchChangeTaskPlanningStatus_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			ExceptionDispatchInfo exceptionInfo = null;
			var thread = new Thread(() =>
			{
				try
				{
					using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
					{
						var pk = ZGuid.NewZGuid();
						var args = new BatchChangeTaskPlanningStatusArgs { JobPKs = [pk.ToGuid()], JobType = string.Empty, ChangeStatusToReady = true };
						controller.BatchChangeTaskPlanningStatus(args);
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestAddPackageToHandlingUnit

		public void TestAddPackageToHandlingUnit_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestAddPackageToHandlingUnitCore(true);
		}

		public void TestAddPackageToHandlingUnit_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestAddPackageToHandlingUnitCore(false);
		}

		void TestAddPackageToHandlingUnitCore(bool isWebUser)
		{
			var isServiceCalled = false;
			var pkgPk = ZGuid.NewZGuid();
			var huPkgk = ZGuid.NewZGuid();

			pickAssignDockDoorService
				.Setup(m => m.AddPackageToHandlingUnit(It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var response = controller.AddPackageToHandlingUnit(pkgPk.ToGuid(), huPkgk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestAddPackageToHandlingUnit_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pkgPk = ZGuid.NewZGuid();
			var huPkgk = ZGuid.NewZGuid();

			pickAssignDockDoorService
				.Setup(m => m.AddPackageToHandlingUnit(It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var response = controller.AddPackageToHandlingUnit(pkgPk.ToGuid(), huPkgk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestAddPackageToHandlingUnit_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pkgPk = ZGuid.NewZGuid();
			var huPkgk = ZGuid.NewZGuid();

			pickAssignDockDoorService
				.Setup(m => m.AddPackageToHandlingUnit(It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var response = controller.AddPackageToHandlingUnit(pkgPk.ToGuid(), huPkgk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestAddPackageToHandlingUnit_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					var pkgPk = ZGuid.NewZGuid();
					var huPkgk = ZGuid.NewZGuid();
					using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
					{
						controller.AddPackageToHandlingUnit(pkgPk.ToGuid(), huPkgk.ToGuid());
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestRemovePackageFromHandlingUnit

		public void TestRemovePackageFromHandlingUnit_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestRemovePackageFromHandlingUnitCore(true);
		}

		public void TestRemovePackageFromHandlingUnit_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestRemovePackageFromHandlingUnitCore(false);
		}

		void TestRemovePackageFromHandlingUnitCore(bool isWebUser)
		{
			var isServiceCalled = false;
			var pk = ZGuid.NewZGuid();

			pickAssignDockDoorService
				.Setup(m => m.RemovePackageFromHandlingUnit(It.IsAny<ZGuid>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var response = controller.RemovePackageFromHandlingUnit(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestRemovePackageFromHandlingUnit_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pk = ZGuid.NewZGuid();

			pickAssignDockDoorService
				.Setup(m => m.RemovePackageFromHandlingUnit(It.IsAny<ZGuid>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var response = controller.RemovePackageFromHandlingUnit(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestRemovePackageFromHandlingUnit_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var pk = ZGuid.NewZGuid();

			pickAssignDockDoorService
				.Setup(m => m.RemovePackageFromHandlingUnit(It.IsAny<ZGuid>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var response = controller.RemovePackageFromHandlingUnit(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestRemovePackageFromHandlingUnit_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					var pk = ZGuid.NewZGuid();
					using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
					{
						controller.RemovePackageFromHandlingUnit(pk.ToGuid());
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestGetIsDockDoorOverrideAllowedForHandlingUnit

		public void TestGetIsDockDoorOverrideAllowedForHandlingUnit_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			TestGetIsDockDoorOverrideAllowedForHandlingUnitCore(true);
		}

		public void TestGetIsDockDoorOverrideAllowedForHandlingUnit_AsStaffUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			TestGetIsDockDoorOverrideAllowedForHandlingUnitCore(false);
		}

		void TestGetIsDockDoorOverrideAllowedForHandlingUnitCore(bool isWebUser)
		{
			var isServiceCalled = false;
			var pk = ZGuid.NewZGuid();

			pickAssignDockDoorService
				.Setup(m => m.GetIsDockDoorOverrideAllowedForHandlingUnit(It.IsAny<ZGuid>()))
				.Callback(() => isServiceCalled = true);

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var response = controller.GetIsDockDoorOverrideAllowedForHandlingUnit(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				if (isWebUser)
				{
					AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
					AssertEquals(false, isServiceCalled);
				}
				else
				{
					AssertEquals(response.StatusCode, HttpStatusCode.OK);
					AssertEquals(true, isServiceCalled);
				}
			}
		}

		public void TestGetIsDockDoorOverrideAllowedForHandlingUnit_Success()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			pickAssignDockDoorService
				.Setup(m => m.GetIsDockDoorOverrideAllowedForHandlingUnit(It.IsAny<ZGuid>()))
				.Returns(string.Empty);

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var response = controller.GetIsDockDoorOverrideAllowedForHandlingUnit(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestGetIsDockDoorOverrideAllowedForHandlingUnit_Fail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			pickAssignDockDoorService
				.Setup(m => m.GetIsDockDoorOverrideAllowedForHandlingUnit(It.IsAny<ZGuid>()))
				.Returns("error message");

			using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
			{
				var pk = ZGuid.NewZGuid();
				var response = controller.GetIsDockDoorOverrideAllowedForHandlingUnit(pk.ToGuid()).ExecuteAsync(CancellationToken.None).Result;

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("\"error message\"", response.Content.ReadAsStringAsync().Result);
			}
		}

		[ExpectNoExceptions]
		public void TestGetIsDockDoorOverrideAllowedForHandlingUnit_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					var pk = ZGuid.NewZGuid();
					using (ObjectFactory.Substitute(pickAssignDockDoorService.Object))
					{
						controller.GetIsDockDoorOverrideAllowedForHandlingUnit(pk.ToGuid());
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		#region TestGetNextTask

		public void TestGetNextTask_AsWebUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var args = new GetNextTaskArgs
			{
				TaskReference = "ABC",
				WarehousePK = Guid.NewGuid(),
				FormFlowType = WarehouseTaskFormFlowTypes.PutawayJob,
				LastFormFlowType = WarehouseTaskFormFlowTypes.UnloadJob,
				TasksToIgnore = [Guid.NewGuid(), Guid.NewGuid()],
			};

			var serviceMock = new Mock<IWhsTaskManagementService>();
			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var response = controller.GetNextTask(args).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
				serviceMock.VerifyNoOtherCalls();
			}
		}

		public void TestGetNextTask_AsStaffUser_SuccessfulInvocation()
		{
			TestGetNextTask_AsStaffUserCore(true);
		}

		public void TestGetNextTask_AsStaffUser_ErrorResponse()
		{
			TestGetNextTask_AsStaffUserCore(false);
		}

		void TestGetNextTask_AsStaffUserCore(bool isSuccessful)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			var args = new GetNextTaskArgs
			{
				TaskReference = "ABC",
				WarehousePK = Guid.NewGuid(),
				FormFlowType = WarehouseTaskFormFlowTypes.PutawayJob,
				LastFormFlowType = WarehouseTaskFormFlowTypes.UnloadJob,
				TasksToIgnore = [Guid.NewGuid(), Guid.NewGuid()],
			};

			var serviceMock = new Mock<IWhsTaskManagementService>();

			var getNextTaskResult = isSuccessful
				? new GetNextTaskResult(Guid.NewGuid(), WarehouseTaskFormFlowTypes.UnloadJob)
				: new GetNextTaskResult("Something went wrong.");

			serviceMock.Setup(m => m.GetNextTask(
				It.IsAny<BusinessObjectFactory>(),
				args.TaskReference,
				staff.PK.ToGuid(),
				args.WarehousePK,
				args.FormFlowType,
				args.LastFormFlowType,
				args.TasksToIgnore)).Returns(getNextTaskResult);

			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var response = controller.GetNextTask(args);

				var result = response.ExecuteAsync(CancellationToken.None).Result;
				AssertEquals(result.StatusCode, HttpStatusCode.OK);

				serviceMock.Verify(m => m.GetNextTask(
					It.IsAny<BusinessObjectFactory>(),
					args.TaskReference,
					staff.PK.ToGuid(),
					args.WarehousePK,
					args.FormFlowType,
					args.LastFormFlowType,
					args.TasksToIgnore));

				var jsonResult = (response as JsonResult<GetNextTaskResult>).Content;
				AssertEquals(getNextTaskResult, jsonResult);
				AssertEquals(getNextTaskResult.TaskPK, jsonResult.TaskPK);
				AssertEquals(getNextTaskResult.ErrorMessage, jsonResult.ErrorMessage);
			}
		}

		[ExpectNoExceptions]
		public void TestGetNextTask_RunSafelyFromAnotherThread()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			ExceptionDispatchInfo exceptionInfo = null;

			var args = new GetNextTaskArgs
			{
				TaskReference = "ABC",
				WarehousePK = Guid.NewGuid(),
				FormFlowType = WarehouseTaskFormFlowTypes.PutawayJob,
				LastFormFlowType = WarehouseTaskFormFlowTypes.UnloadJob,
				TasksToIgnore = [Guid.NewGuid(), Guid.NewGuid()],
			};

			var thread = new Thread(() =>
			{
				try
				{
					var serviceMock = new Mock<IWhsTaskManagementService>();
					using (ObjectFactory.Substitute(serviceMock.Object))
					{
						controller.GetNextTask(args);
					}
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			controller = new WarehouseController();
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};

			cycleCountServiceMock = new Mock<IWhsCycleCountLocationService>();
			packingConsolidationService = new Mock<IWhsPackingConsolidationService>();
			pickAssignDockDoorService = new Mock<IWhsPickDockDoorAssignmentService>();
			glowServicesModuleService = new Mock<IWhsGlowServicesModuleService>();
			taskManagementServiceMock = new Mock<IWhsTaskManagementService>();
		}

		WarehouseController controller;
		Mock<IWhsCycleCountLocationService> cycleCountServiceMock;
		Mock<IWhsPackingConsolidationService> packingConsolidationService;
		Mock<IWhsPickDockDoorAssignmentService> pickAssignDockDoorService;
		Mock<IWhsGlowServicesModuleService> glowServicesModuleService;
		Mock<IWhsTaskManagementService> taskManagementServiceMock;
	}
}
