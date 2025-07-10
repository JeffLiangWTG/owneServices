using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class CusDispositionFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var cusDisposition1 = Factory.NewWithValidTestData<CusDisposition>();
			cusDisposition1.Parent = Master;
			var cusDisposition2 = Factory.NewWithValidTestData<CusDisposition>();
			cusDisposition2.Parent = Master;
			Factory.Save();

			TestFetchForView(cusDisposition1, cusDisposition2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			var parent = factory.Load<CusDispositionTestBO>(Master.PK);
			return new CusDispositionCollection(parent);
		}

		CusDispositionTestBO Master
		{
			get
			{
				return master ?? (master = Factory.New<CusDispositionTestBO>());
			}
		}
		CusDispositionTestBO master;
	}
}
