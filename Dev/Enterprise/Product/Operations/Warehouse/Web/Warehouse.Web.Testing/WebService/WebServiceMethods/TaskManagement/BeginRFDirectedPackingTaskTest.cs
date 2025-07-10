using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class BeginRFDirectedPackingTaskTest : WhsSecureServiceTestCase
	{
		public void TestBeginRFDirectedPackingTask_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFDirectedPackingTask(Guid.NewGuid());

			AssertBusinessValidationError(webService, "Task could not be found.", response);
		}

		public void TestBeginRFDirectedPackingTask_WrongTaskType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var packingStationLocationType = Helper.CreateLocationType("PST", "Test", false, 0, LocationClasses.Codes.PST);
			var packingLocation = data.Whs1.FindLocation("A-2");
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFDirectedPackingTask(task.PK.ToGuid());

			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFDirectedPackingTask_SetsTaskToPlay()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
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
			Helper.Factory.Save();

			AssertEquals(string.Empty, order.WD_GS_NKAssignedPacker);

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFDirectedPackingTask(task.PK.ToGuid());

			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(pick.PK, response.WhsPickPK);
			AssertEquals(packingLocation.PK, response.PackingStation.LocationPK);
			AssertEquals(packingLocation.WLV_LocationString, response.PackingStation.LocationString);
			AssertEquals(packingLocation.WLV_LocationString_UserFriendly, response.PackingStation.LocationString_UserFriendly);
			AssertEquals(order.PK, response.Orders.Single().PK);

			var newFactory = new BusinessObjectFactory();
			var taskInOtherFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskInOtherFactory.P9_Status);
			var orderInOtherFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals(staff.GS_Code, orderInOtherFactory.WD_GS_NKAssignedPacker);
		}

		public void TestBeginRFDirectedPackingTask_SetsTaskToPlay_TaskAlreadyWorking()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
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
			Helper.Factory.Save();

			AssertEquals(string.Empty, order.WD_GS_NKAssignedPacker);

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var saveCount = 0;
			var webService = GetNewWebService(data.Whs1, staff);
			webService.Factory.Saving += f => saveCount++;
			var response = webService.BeginRFDirectedPackingTask(task.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Factory Save should not be called.", 0, saveCount);

			AssertEquals(pick.PK, response.WhsPickPK);
			AssertEquals(packingLocation.PK, response.PackingStation.LocationPK);
			AssertEquals(packingLocation.WLV_LocationString, response.PackingStation.LocationString);
			AssertEquals(packingLocation.WLV_LocationString_UserFriendly, response.PackingStation.LocationString_UserFriendly);
			AssertEquals(order.PK, response.Orders.Single().PK);
		}

		public void TestBeginRFDirectedPackingTask_LoadPick()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
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
			Helper.Factory.Save();

			AssertEquals(string.Empty, order.WD_GS_NKAssignedPacker);

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFDirectedPackingTask(task.PK.ToGuid());

			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals(pick.PK, response.WhsPickPK);
			AssertEquals(packingLocation.PK, response.PackingStation.LocationPK);
			AssertEquals(packingLocation.WLV_LocationString, response.PackingStation.LocationString);
			AssertEquals(packingLocation.WLV_LocationString_UserFriendly, response.PackingStation.LocationString_UserFriendly);
			AssertEquals(order.PK, response.Orders.Single().PK);
		}

		public void TestBeginRFDirectedPackingTask_NoOrdersToPack()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
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

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Helper.Factory.Save();

			AssertEquals(string.Empty, order.WD_GS_NKAssignedPacker);

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFDirectedPackingTask(task.PK.ToGuid());

			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("No order to Pack.", response.ErrorMessage);
		}

		public void TestBeginRFDirectedPackingTask_NoPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			var packingLocation = data.Whs1.FindLocation("A-2");
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
			Helper.Factory.Save();

			AssertEquals(string.Empty, order.WD_GS_NKAssignedPacker);

			var task = Helper.CreateProcessTaskForDirectedPackingJob(pick, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFDirectedPackingTask(task.PK.ToGuid());

			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("No available Packing Station.", response.ErrorMessage);
		}

		#region Implementation

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Helper.Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}
