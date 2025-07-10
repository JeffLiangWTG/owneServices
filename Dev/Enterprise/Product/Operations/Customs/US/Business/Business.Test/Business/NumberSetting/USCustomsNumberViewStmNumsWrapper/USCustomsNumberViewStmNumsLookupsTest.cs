using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCustomsNumberViewStmNumsLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var lookups1 = new USCustomsNumberViewStmNumsLookups(Factory.New<CustomsNumberViewStmNums>());
			var lookups2 = new USCustomsNumberViewStmNumsLookups(Factory.New<CustomsNumberViewStmNums>());
			AssertSame(lookups1.TypeList, lookups2.TypeList);
		}
	}
}
