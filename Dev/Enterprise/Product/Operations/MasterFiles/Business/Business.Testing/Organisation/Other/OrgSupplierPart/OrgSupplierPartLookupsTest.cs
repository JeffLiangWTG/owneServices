using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupplierPartLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNMFCList()
		{
			RefNMFC nmfc1 = Factory.New<RefNMFC>();
			nmfc1.FN_ItemNo = "999";
			nmfc1.FN_Class = "999";
			RefNMFC nmfc2 = Factory.New<RefNMFC>();
			nmfc2.FN_ItemNo = "888";
			nmfc2.FN_Class = "888";
			RefNMFC nmfc3 = Factory.New<RefNMFC>();
			nmfc3.FN_IsActive = false;
			nmfc3.FN_ItemNo = "777";
			nmfc3.FN_Class = "777";
			Factory.Save();

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			RefNMFCCollection collection = part.Lookups.NMFCList;
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(nmfc1));
			Assert(collection.Contains(nmfc2));
		}
	}
}
