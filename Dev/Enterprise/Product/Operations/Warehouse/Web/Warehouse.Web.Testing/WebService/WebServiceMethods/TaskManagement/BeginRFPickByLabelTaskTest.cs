using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class BeginRFPickByLabelTaskTest : WhsSecureServiceTestCase
	{
		public void TestBeginRFPickByLabelTask_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickByLabelTask(Guid.NewGuid());

			AssertBusinessValidationError(webService, "Task could not be found.", response);
		}

		public void TestBeginRFPickByLabelTask_WrongTaskType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickByLabelTask(task.PK.ToGuid());

			AssertBusinessValidationError(webService, "This task is not valid for the current operation.", response);
		}

		public void TestBeginRFPickByLabelTask_SetsTaskToPlay()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines.Single());
			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickByLabelJob(pick, staff, pickByLabelJob);
			pickByLabelJob.WTK_GS_NKAssignedTo = String.Empty;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.BeginRFPickByLabelTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferLineInOtherFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals(staff.GS_Code, transferLineInOtherFactory.WTK_GS_NKAssignedTo);
		}
	}
}
