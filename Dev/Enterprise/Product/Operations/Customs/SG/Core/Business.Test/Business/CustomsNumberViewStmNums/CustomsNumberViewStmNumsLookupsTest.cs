using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class CustomsNumberViewStmNumsLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			AssertEquals(Factory.GetCachedValue<NumberRangeTypeList>(), new SGCustomsNumberViewStmNumsLookups(Factory.New<CustomsNumberViewStmNums>()).TypeList);
		}
	}
}
