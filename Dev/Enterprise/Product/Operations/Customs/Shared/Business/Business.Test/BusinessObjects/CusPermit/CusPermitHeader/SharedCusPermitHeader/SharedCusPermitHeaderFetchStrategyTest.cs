using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class SharedCusPermitHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var header1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var header2 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			Factory.Save();

			TestFetchForView(header1, header2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return new CusPermitHeaderCollection(factory);
		}
	}
}
