using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StaffViewStmNumsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var lookups = Factory.New<StaffViewStmNums>().Lookups;
			var list = lookups.TypeList;

			AssertSame("Should be cached", list, lookups.TypeList);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(OrgConstants.NumberFountains.Code.PatentNumber));
		}
	}
}
