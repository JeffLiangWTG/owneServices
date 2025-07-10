using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class BaseCusGuaranteeHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var header1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			header1.CPH_Number = "123";
			var header2 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			header2.CPH_Number = "456";
			Factory.Save();

			TestFetchForView(header1, header2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return new CusGuaranteeHeaderCollection(factory);
		}
	}
}
