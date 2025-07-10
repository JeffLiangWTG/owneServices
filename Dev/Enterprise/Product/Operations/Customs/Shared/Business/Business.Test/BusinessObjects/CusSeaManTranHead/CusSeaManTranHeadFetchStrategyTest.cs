using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class CusSeaManTranHeadFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var header1 = Factory.NewWithValidTestData<CusSeaManTranHead>();
			var header2 = Factory.NewWithValidTestData<CusSeaManTranHead>();
			Factory.Save();

			TestFetchForView(header1, header2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return new CusSeaManTranHeadCollection(factory);
		}
	}
}
