using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class SuspendPickingTest : WhsSecureServiceTestCase
	{
		#region TestSuspendPicking

		public void TestSuspendPicking()
		{
			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = "A";
			pickLine.WZ_IsPicking = true;
			Helper.Factory.Save();
			AssertEquals("Precondition", true, pickLine.WZ_IsPicking);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SuspendPicking();
			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals("Should be still assigned to same picker.", "A", pickLine.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking should be false.", false, pickLine.WZ_IsPicking);
				AssertEquals("Ensure saved.", false, pickLine.HasChanges);
			});
		}

		public void TestSuspendPicking_ConcurrencyError()
		{
			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = "A";
			pickLine.WZ_IsPicking = true;
			Helper.Factory.Save();
			AssertEquals("Precondition", true, pickLine.WZ_IsPicking);

			var webService = GetNewWebService(data.Whs1, staff);

			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLine).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.SuspendPicking();
			AssertBusinessValidationError(webService, "While you have been working with this job another user has made changes. Please restart the operation and try again.", response);
			AssertEquals("Should be still assigned to same picker.", "A", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Is picking should still be true.", true, pickLine.WZ_IsPicking);
		}

		#endregion
	}
}
