using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class CusPackageFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var collection = CreateCollectionToTest(Factory);
			TestFetchForView((CusPackage)collection[0], (CusPackage)collection[1]);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			var jobDeclaration = factory.Load<BaseJobDeclaration>(this.jobDeclaration.PK);
			return jobDeclaration.LoadCusPackingList(factory).PackageJob.Packages;
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var packingList = jobDeclaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var collection = packingList.PackageJob.Packages;
			collection.AddNew();
			collection.AddNew();
			Factory.Save();
		}

		BaseJobDeclaration jobDeclaration;
	}
}
