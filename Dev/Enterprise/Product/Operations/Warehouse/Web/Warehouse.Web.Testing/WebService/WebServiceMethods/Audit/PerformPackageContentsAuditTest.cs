using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PerformPackageContentsAuditTest : WhsSecureServiceTestCase
	{
		#region TestPerformPackageContentsAudit

		public void TestPerformPackageContentsAudit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 12m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var auditLines = new[] { new WhsPackageProductInfo(data.Part1.PK.ToGuid(), string.Empty, string.Empty, 10m, 0m, 0m, string.Empty) };
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.PerformPackageContentsAudit(package.PK.ToGuid(), auditLines);
			AssertEquals(ErrorTypes.Information, response.Error);
			AssertEquals(WhsPackageAuditManagerForWebServices.PackageAuditManagerStrings.Success_Variance, response.ErrorMessage);
		}

		#endregion
	}
}
