using System;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CheckStockOnHandForLocationExcludingReceiveTest : WhsSecureServiceTestCase
	{
		#region TestCheckStockOnHandForLocationExcludingReceive

		public void TestCheckStockOnHandForLocationExcludingReceive_CurrentReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseForCurrentReceive = webService.CheckStockOnHandForLocationExcludingReceive(receive.PK.ToGuid(), "A-1");
			Assert(string.IsNullOrEmpty(responseForCurrentReceive.ErrorMessage));
		}

		public void TestCheckStockOnHandForLocationExcludingReceive_LocationWithStock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseForLocationWithStock = webService.CheckStockOnHandForLocationExcludingReceive(Guid.NewGuid(), "A-1");
			AssertEquals("Location A-1 has stock on hand.", responseForLocationWithStock.ErrorMessage);
		}

		public void TestCheckStockOnHandForLocationExcludingReceive_LocationNoStock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("OP1", "Test1");
			var staff2 = Helper.CreateGlbStaff("OP2", "Test2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff1);
			var responseForAnotherLocation = webService1.CheckStockOnHandForLocationExcludingReceive(receive.PK.ToGuid(), "A-2");
			Assert(string.IsNullOrEmpty(responseForAnotherLocation.ErrorMessage));

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff2);
			var responseForLocationA3 = webService2.CheckStockOnHandForLocationExcludingReceive(Guid.NewGuid(), "A-3");
			Assert(string.IsNullOrEmpty(responseForLocationA3.ErrorMessage));
		}

		public void TestCheckStockOnHandForLocationExcludingReceive_LocationAfterTransferPendingExcludeTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("OP1", "Test1");
			var staff2 = Helper.CreateGlbStaff("OP2", "Test2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Created = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transferLine_Created.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff1);
			var responseForLocationA3_AfterTransferPending = webService1.CheckStockOnHandForLocationExcludingReceive(Guid.NewGuid(), "A-3");
			AssertEquals("Location A-3 now has stock pending to be transfered to A3", "Location A-3 has stock on hand.", responseForLocationA3_AfterTransferPending.ErrorMessage);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff2);
			var responseForLocationA3_AfterTransferPendingExcludeTransfer = webService2.CheckStockOnHandForLocationExcludingReceive(transfer.PK.ToGuid(), "A-3");
			Assert(string.IsNullOrEmpty(responseForLocationA3_AfterTransferPendingExcludeTransfer.ErrorMessage));
		}

		#endregion
	}
}
