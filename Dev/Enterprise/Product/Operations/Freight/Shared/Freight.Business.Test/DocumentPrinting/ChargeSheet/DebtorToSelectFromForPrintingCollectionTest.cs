using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DebtorToSelectFromForPrintingCollectionTest : TestCaseWithFactory
	{
		public void TestCollectionDoesNotAllowNew()
		{
			OrgHeaderCollection orgs = new OrgHeaderCollection(Factory);
			orgs.AddNew();

			DebtorToSelectFromForPrintingCollection collection = new DebtorToSelectFromForPrintingCollection(orgs);
			AssertEquals("Collection should not allow new", false, collection.AllowNew);
		}
	}
}
