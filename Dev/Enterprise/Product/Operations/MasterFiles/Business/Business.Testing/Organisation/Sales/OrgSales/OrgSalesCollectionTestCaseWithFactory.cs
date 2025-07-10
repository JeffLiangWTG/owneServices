using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	sealed class OrgSalesCollectionTestCaseWithFactory : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestFetchOnlyFromLocalCacheNonDependent()
		{
			OrgSalesCollectionForTest coll = new OrgSalesCollectionForTest(Factory);
			AssertEquals(false, coll.FetchOnlyFromLocalCache);
		}

		public void TestFetchOnlyFromLocalCacheDependent()
		{
			OrgHeader orgheader2 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgSalesCollectionForTest coll2 = new OrgSalesCollectionForTest(orgheader2);
			AssertEquals(false, coll2.FetchOnlyFromLocalCache);

			OrgHeader orgheader = Factory.New<OrgHeader>();
			OrgSalesCollectionForTest coll = new OrgSalesCollectionForTest(orgheader);

			AssertEquals(true, coll.FetchOnlyFromLocalCache);
		}

		class OrgSalesCollectionForTest : OrgSalesCollection
		{
			public OrgSalesCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public OrgSalesCollectionForTest(OrgHeader header) : base(header)
			{
			}

			internal new bool FetchOnlyFromLocalCache => base.FetchOnlyFromLocalCache;
		}
	}
}
