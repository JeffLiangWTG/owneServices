using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Common.Testing
{
	[TestedType(typeof(WhsOrgSupplierPartCollectionBOM))]
	public class WhsOrgSupplierPartCollectionBOMTest : OrgSupplierPartCollectionBOMTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsOrgSupplierPartCollectionBOM(Factory);
		}
	}
}
