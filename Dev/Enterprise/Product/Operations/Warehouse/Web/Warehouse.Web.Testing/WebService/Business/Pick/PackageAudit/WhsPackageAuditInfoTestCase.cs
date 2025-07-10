using System;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsPackageAuditInfoTestCase : WhsTestCaseWithFactory
	{
		public void TestConstructors()
		{
			var auditPackInfo = new WhsPackageProductInfo();
			AssertNotNull(auditPackInfo);
			AssertEquals(Guid.Empty, auditPackInfo.ProductPK);
			AssertEquals(0m, auditPackInfo.Quantity);
			AssertEquals(0m, auditPackInfo.ExpectedQty);
			AssertEquals(0m, auditPackInfo.ProductWeight);
			AssertEquals(string.Empty, auditPackInfo.ProductWeightUQ);

			var pk = Guid.NewGuid();
			var auditPackInfoWithData = new WhsPackageProductInfo(pk, "NOKIA PHONE", "INDESTRUCTIBLE", 111.283m, 573.283m, 1.23m, "KG");
			AssertNotNull(auditPackInfoWithData);
			AssertEquals(pk, auditPackInfoWithData.ProductPK);
			AssertEquals(111.283m, auditPackInfoWithData.Quantity);
			AssertEquals(573.283m, auditPackInfoWithData.ExpectedQty);
			AssertEquals("NOKIA PHONE", auditPackInfoWithData.ProductCode);
			AssertEquals("INDESTRUCTIBLE", auditPackInfoWithData.ProductDescription);
			AssertEquals(1.23m, auditPackInfoWithData.ProductWeight);
			AssertEquals("KG", auditPackInfoWithData.ProductWeightUQ);
		}
	}
}
