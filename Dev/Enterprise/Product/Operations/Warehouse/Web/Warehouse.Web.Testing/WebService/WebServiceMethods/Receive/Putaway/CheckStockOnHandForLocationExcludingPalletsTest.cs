using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CheckStockOnHandForLocationExcludingPalletsTest : WhsSecureServiceTestCase
	{
		#region TestCheckStockOnHandForLocationExcludingPallets

		public void TestCheckStockOnHandForLocationExcludingPallets_MatchingPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10, null, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseMatchingPallet = webService.CheckStockOnHandForLocationExcludingPallets(new[] { "PLT-1" }, "A-1");
			Assert("Both R1 and R2 has PLT-1, and no other receives put any stock in A-1, so no warning should be returned", string.IsNullOrEmpty(responseMatchingPallet.ErrorMessage));
			AssertEquals("Both R1 and R2 has PLT-1, and no other receives put any stock in A-1, so no warning should be returned", ErrorTypes.None, responseMatchingPallet.Error);
		}

		public void TestCheckStockOnHandForLocationExcludingPallets_DifferentPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10, null, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseForDifferentPallet = webService.CheckStockOnHandForLocationExcludingPallets(new[] { "PLT-2" }, "A-1");
			AssertEquals("Location A-1 has stock available from R1 which does not have PLT-2", "Location A-1 has stock on hand.", responseForDifferentPallet.ErrorMessage);
			AssertEquals("Location A-1 has stock available from R1 which does not have PLT-2", ErrorTypes.BusinessValidationError, responseForDifferentPallet.Error);
		}

		public void TestCheckStockOnHandForLocationExcludingPallets_LocationWithStock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10, data.Whs1.FindLocation("A-1"), "IrrelevantPallet");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseForLocationWithStock = webService.CheckStockOnHandForLocationExcludingPallets(new[] { "PLT-1" }, "A-1");
			AssertEquals("Location A-1 has stock from receive R3 (which doesn't contain PLT-1), so we must have a message", "Location A-1 has stock on hand.", responseForLocationWithStock.ErrorMessage);
			AssertEquals("Location A-1 has stock from receive R3 (which doesn't contain PLT-1), so we must have a message", ErrorTypes.BusinessValidationError, responseForLocationWithStock.Error);
		}

		public void TestCheckStockOnHandForLocationExcludingPallets_OneMatchingPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10, location, "IrrelevantPallet");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseForLocationWithStock = webService.CheckStockOnHandForLocationExcludingPallets(new[] { "PLT-1", "PLT-3" }, "A-1");
			AssertEquals("Location A-1 has stock from receive R3 (which doesn't contain PLT-1), so we must have a message", "Location A-1 has stock on hand.", responseForLocationWithStock.ErrorMessage);
			AssertEquals("Location A-1 has stock from receive R3 (which doesn't contain PLT-1), so we must have a message", ErrorTypes.BusinessValidationError, responseForLocationWithStock.Error);
		}

		public void TestCheckStockOnHandForLocationExcludingPallets_MultipleMatchingPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10, location, "IrrelevantPallet");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10, location, "PLT-3");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseForLocationWithStock = webService.CheckStockOnHandForLocationExcludingPallets(new[] { "PLT-1", "PLT-3" }, "A-1");
			AssertEquals("Location A-1 has stock from receive R3 (which doesn't contain PLT-1), so we must have a message", "Location A-1 has stock on hand.", responseForLocationWithStock.ErrorMessage);
			AssertEquals("Location A-1 has stock from receive R3 (which doesn't contain PLT-1), so we must have a message", ErrorTypes.BusinessValidationError, responseForLocationWithStock.Error);
		}

		public void TestCheckStockOnHandForLocationExcludingPallets_Multiple()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10, null, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10, location, "PLT-3");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var responseMatchingPallet = webService.CheckStockOnHandForLocationExcludingPallets(new[] { "PLT-1", "PLT-3" }, "A-1");
			Assert("Both R1, R2 and R3 has PLT-1/PLT-3, and no other receives put any stock in A-1, so no warning should be returned", string.IsNullOrEmpty(responseMatchingPallet.ErrorMessage));
			AssertEquals("Both R1, R2 and R3 has PLT-1/PLT-3, and no other receives put any stock in A-1, so no warning should be returned", ErrorTypes.None, responseMatchingPallet.Error);
		}

		#endregion
	}
}
