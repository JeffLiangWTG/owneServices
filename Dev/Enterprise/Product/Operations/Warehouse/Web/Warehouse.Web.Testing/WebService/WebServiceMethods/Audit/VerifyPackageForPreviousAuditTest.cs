using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class VerifyPackageForPreviousAuditTest : WhsSecureServiceTestCase
	{
		#region TestVerifyPackageForPreviousAudit

		public void TestVerifyPackageForPreviousAudit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 12m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.CreateWhsPackageAuditWithLineFailure(order, "P1", data.Part1, 10, 9);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.VerifyPackageForPreviousAudit(package.PK.ToGuid());
			AssertEquals("It must return enquire for re-audit.", ErrorTypes.YesNoEnquiry, response.Error);
			AssertEquals("Question for re-auditing failed package must be returned.", WhsPackageAuditManagerForWebServices.PackageAuditManagerStrings.YesNoEnquiry_AuditedFailed, response.ErrorMessage);
		}

		#endregion
	}
}
