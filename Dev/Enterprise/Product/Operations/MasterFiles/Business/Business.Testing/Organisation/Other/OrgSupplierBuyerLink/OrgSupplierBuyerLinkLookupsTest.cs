using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupplierBuyerLinkLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRelationTypeList()
		{
			var lookups = Factory.New<OrgSupplierBuyerLink>().Lookups;
			AssertNotNull(lookups.RelationTypeList);
			AssertEquals(new OrgRelationTypeList().Count, lookups.RelationTypeList.Count);  // Count = 2
			AssertEquals("RelationTypeList", typeof(OrgRelationTypeList), lookups.RelationTypeList.GetType());
			Assert(!lookups.RelationTypeList.ContainsCode("REL"));
		}
	}
}
