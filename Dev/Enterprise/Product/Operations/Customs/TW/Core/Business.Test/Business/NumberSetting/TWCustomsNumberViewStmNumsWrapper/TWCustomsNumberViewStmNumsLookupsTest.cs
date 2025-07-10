using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWCustomsNumberViewStmNumsLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var lookups1 = new TWCustomsNumberViewStmNumsLookups(Factory.New<CustomsNumberViewStmNums>());
			var typeList = lookups1.TypeList;
			AssertEquals(1, typeList.Count);
			AssertEquals("CUS", typeList.CodesAsString);
		}
	}
}
