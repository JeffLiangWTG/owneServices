using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGCustomsNumberViewStmNumsWrapperLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).CustomsNumberProvider;
			AssertEquals(true, object.ReferenceEquals(Factory.GetCachedValue<NumberRangeTypeList>(), stmNum.Lookups.TypeList));
		}
	}
}
