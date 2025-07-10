using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPackageAuditManagerTest : WhsTestCaseWithFactory
	{
		#region TestPackageWithInvalidParent

		public void TestPackageWithInvalidParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "P1", 5, "PLT");
			packageJob.KJ_ParentTableCode = "KB";

			var audit = WhsPackageAuditManager.AuditPackage(package);
			AssertNull("Should not return an audit for a package not related to WhsOrder.", audit);
			AssertNull("Should not return any audits for a package not related to WhsOrder.", WhsPackageAuditManager.GetLastWhsPackageAudit(package));
			AssertEquals("Should not detect audits passed for a package not related to WhsOrder.", false, WhsPackageAuditManager.HasPassedAudit(package));
			AssertEquals("Should not detect audits for a package not related to WhsOrder.", false, WhsPackageAuditManager.HasBeenAudited(package));
		}

		#endregion

		#region TestAuditPackage

		public void TestAuditPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			AssertNull("There should not be audits for this package", WhsPackageAuditManager.GetLastWhsPackageAudit(package));

			WhsPackageAuditManager.AuditPackage(package);
			AssertNotNull("There should be an audit for this package now", WhsPackageAuditManager.GetLastWhsPackageAudit(package));
		}

		#endregion

		#region TestGetLastWhsPackageAudit

		[TestDate(2016, 11, 10)]
		public void TestGetLastWhsPackageAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();

			AssertNull("It should return null if the package doesn't have a PackageID", WhsPackageAuditManager.GetLastWhsPackageAudit(package));

			package.KP_PackageID = "P1";
			AssertNull("There should not be audits for this package", WhsPackageAuditManager.GetLastWhsPackageAudit(package));

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			WhsPackageAuditManager.AuditPackage(package, completeTimeoffset, GlbStaff.CurrentUser);
			AssertEquals("There should be an audit for this package now with Brett's birthday as date", completeTimeoffset, WhsPackageAuditManager.GetLastWhsPackageAudit(package).WPA_AuditCompleteTime);

			WhsPackageAuditManager.AuditPackage(package);
			AssertEquals("There should be an audit for this package now with Today date", ZDateTimeOffset.UtcNow, WhsPackageAuditManager.GetLastWhsPackageAudit(package).WPA_AuditCompleteTime);
		}

		#endregion

		#region TestGetLastWhsPackageAuditReturnsNullWhenPackageDoesNotHaveID

		public void TestGetLastWhsPackageAuditReturnsNullWhenPackageDoesNotHaveID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			AssertNull("It should return null if the package doesn't have a PackageID", WhsPackageAuditManager.GetLastWhsPackageAudit(package));
		}

		#endregion

		#region TestHasPassedAudit

		[TestDate(2016, 12, 29)]
		public void TestHasPassedAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			AssertEquals("Since the package doesn't have an ID it should be false", false, WhsPackageAuditManager.HasPassedAudit(package));

			package.KP_PackageID = "P1";
			AssertEquals("Since there is not audits it should be false", false, WhsPackageAuditManager.HasPassedAudit(package));

			var audit = WhsPackageAuditManager.AuditPackage(package);
			AssertEquals("Since there is an audit without problems it should be true", true, WhsPackageAuditManager.HasPassedAudit(package));

			var failure = audit.PackageAuditFailureLines.AddNew();
			AssertEquals("Since there is an audit but has problems it should be false again", false, WhsPackageAuditManager.HasPassedAudit(package));
			var znow = ZDateTimeOffset.Now;

			WhsPackageAuditManager.AuditPackage(package, znow.AddHours(1), GlbStaff.CurrentUser);
			AssertEquals("Since it was reaudited and there were no problems it should be true again", true, WhsPackageAuditManager.HasPassedAudit(package));
		}

		#endregion

		#region TestHasBeenAudited

		public void TestHasBeenAudited()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			AssertEquals("Since the package doesn't have an ID it should be false", false, WhsPackageAuditManager.HasBeenAudited(package));

			package.KP_PackageID = "P1";
			AssertEquals("Since there is not audits it should be false", false, WhsPackageAuditManager.HasBeenAudited(package));

			var audit = WhsPackageAuditManager.AuditPackage(package);
			AssertEquals("Since there is an audit it should be true now", true, WhsPackageAuditManager.HasBeenAudited(package));

			var failure = audit.PackageAuditFailureLines.AddNew();
			AssertEquals("Even if the audit has problems the package was already audited", true, WhsPackageAuditManager.HasBeenAudited(package));
		}

		#endregion

		#region TestHasAuditFailures

		public void TestHasAuditFailures()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			AssertEquals("Since the package doesn't have an ID it should be false", false, WhsPackageAuditManager.HasAuditFailures(package));

			package.KP_PackageID = "P1";
			AssertEquals("Since there is not audits it should be false", false, WhsPackageAuditManager.HasAuditFailures(package));

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var audit = WhsPackageAuditManager.AuditPackage(package, completeTimeoffset, GlbStaff.CurrentUser);
			AssertEquals("Since there is an audit but without failures it should be false", false, WhsPackageAuditManager.HasAuditFailures(package));

			var failure = audit.PackageAuditFailureLines.AddNew();
			AssertEquals("Since there is an audit with failures it should be true", true, WhsPackageAuditManager.HasAuditFailures(package));

			WhsPackageAuditManager.AuditPackage(package);
			AssertEquals("Since it was reaudited without problems it should be false now", false, WhsPackageAuditManager.HasAuditFailures(package));
		}

		#endregion
	}
}
