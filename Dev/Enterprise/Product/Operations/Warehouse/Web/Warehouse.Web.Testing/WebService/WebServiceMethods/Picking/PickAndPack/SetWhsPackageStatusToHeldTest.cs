using System;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class SetWhsPackageStatusToHeldTest : WhsSecureServiceTestCase
	{
		#region TestSetWhsPackageStatusToHeld

		public void TestSetWhsPackageStatusToHeld()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.Factory.Save();

			helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE02";
			package.Pack(orderLine.ReleaseLines[0], 5m);
			AssertEquals("Precondition", false, package.KP_IsHeld);

			var response = webService.SetWhsPackageStatusToHeld(package.PK.ToGuid());
			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Package should be held.", true, package.KP_IsHeld);
		}

		#endregion

		#region TestSetWhsPackageStatusToHeld_NoPackageMatched

		public void TestSetWhsPackageStatusToHeld_NoPackageMatched()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var randomGuid = new Guid("12345678-1111-1234-1111-123456789012");

			var package = helper.Factory.Load<PkgPackage>(randomGuid);
			AssertNull(package); //Precondition

			var response = webService.SetWhsPackageStatusToHeld(randomGuid);
			AssertEquals("Package does not exist.", response.ErrorMessage);
		}

		#endregion
	}
}
