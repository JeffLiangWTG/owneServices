using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPackageAudit))]
	public class WhsPackageAuditTest : WhsBusinessObjectTestCase
	{
		#region TestDefaultValues

		[TestDate(1992, 11, 10, 5, 1, 3)]
		public void TestDefaultValues()
		{
			var audit = Factory.New<WhsPackageAudit>();

			AssertEquals("The audit complete time is setted to now with local time", ZDateTimeOffset.Now, audit.WPA_AuditCompleteTime);
			AssertEquals("The auditor is setted to the current user because when a audit is created with the device he's already logged in", GlbStaff.CurrentUser.GS_Code, audit.WPA_GS_NKAuditor);
		}

		#endregion

		#region Related Entities

		#region Order

		public void TestOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var audit = Helper.CreateWhsPackageAudit(order);
			AssertEquals("When the order.WPA_WD_Order is setted the property loads correctly the Order", order.PK, audit.Order.PK);
		}

		#endregion

		#region PackageNumAndCompleteTime

		public void TestPackageNumAndCompleteTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var audit = Helper.CreateWhsPackageAudit(order, "PKG", completeTimeoffset);
			AssertEquals("Package ID must be set to 'PKG'", "PKG", audit.WPA_PackageID);
			AssertEquals("Completed Audit Time must be set to Brett's Birthday", completeTimeoffset, audit.WPA_AuditCompleteTime);
		}

		#endregion

		#region PackageAuditFailureLines

		public void TestPackageAuditFailureLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var audit = Helper.CreateWhsPackageAudit(order);
			var auditFailure = audit.PackageAuditFailureLines.AddNew();

			AssertContainsExactElementsInAnyOrder("When adding an auditFailure it's correctly assigned to the audit's failure collection", new[] { auditFailure }, audit.PackageAuditFailureLines);
		}

		public void TestCreateWhsPackageAuditWithFailureLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(order, "PKG", data.Part1, 5m, 10m);
			var auditFailure = audit.PackageAuditFailureLines[0];

			AssertEquals("Audit must have a failure line.", 1, audit.PackageAuditFailureLines.Count);
			AssertEquals("Failure line must containe Part1 PK.", data.Part1.PK, auditFailure.WPF_OP);
			AssertEquals("Failure line must containe expected quantity of 5.", 5m, auditFailure.WPF_ExpectedQty);
			AssertEquals("Failure line must containe audited quantity of 5.", 10m, auditFailure.WPF_AuditedQty);
		}

		#endregion

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var audit = Helper.CreateWhsPackageAudit(order);
			var auditFailure = audit.PackageAuditFailureLines.AddNew();

			audit.Delete();
			AssertEquals("The audit is deleted", true, audit.IsDeleted);
			AssertEquals("The failure lines are deleted", true, auditFailure.IsDeleted);
			AssertEquals("The failure lines are deleted", 0, audit.PackageAuditFailureLines.Count);
		}

		#endregion

		#region implementation

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			var audit = (WhsPackageAudit)base.GetNewBusinessObject();

			var order = Helper.CreateWhsOrder(Helper.CreateClient(), Helper.CreateWarehouse("TST"));
			audit.WPA_WD_Order = order.PK;
			audit.WPA_PackageID = "TEST";
			foreach (var auditFailure in audit.PackageAuditFailureLines)
			{
				auditFailure.WPF_ExpectedQty = 2;
			}
			return audit;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		#endregion

		#endregion

	}
}
