using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPackageAuditLineFailure))]
	public class WhsPackageAuditLineFailureTest : WhsBusinessObjectTestCase
	{
		#region Related Entities

		#region Product

		public void TestProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = Helper.CreateProduct(data.Org1, "P1");
			var auditFailure = Factory.New<WhsPackageAuditLineFailure>();
			AssertNull(auditFailure.Product);
			auditFailure.WPF_OP = product.PK;
			AssertEquals("When the auditFailure.WPF_OP is setted the property loads correctly the Product", product.PK, auditFailure.Product.PK);
		}

		#endregion

		#region PackageAudit

		public void TestPackageAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var audit = Helper.CreateWhsPackageAudit(order);
			var auditFailure = Factory.New<WhsPackageAuditLineFailure>();
			AssertNull(auditFailure.PackageAudit);
			auditFailure.WPF_WPA_WhsPackageAudit = audit.PK;
			AssertEquals("When the auditFailure.WPF_WPA_WhsPackageAudit is setted the property loads correctly the PackageAudit", audit.PK, auditFailure.PackageAudit.PK);
		}

		#endregion

		#endregion

		#region implementation

		#region GetNewBusinessObject overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var client = Helper.CreateClient();
			var order = Helper.CreateWhsOrder(client, Helper.CreateWarehouse("TST"));
			var packageAudit = Helper.CreateWhsPackageAudit(order);
			packageAudit.WPA_PackageID = "TEST";
			var lineFailure = (WhsPackageAuditLineFailure)base.GetNewBusinessObject();
			lineFailure.WPF_WPA_WhsPackageAudit = packageAudit.PK;
			lineFailure.WPF_OP = Helper.CreateProduct(client, "P1").PK;
			lineFailure.WPF_AuditedQty = 1;
			lineFailure.WPF_ExpectedQty = 2;
			return lineFailure;
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
