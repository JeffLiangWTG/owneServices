using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackageLookupsTest : PkgPackageLookupsTest
	{
		protected override PkgPackageJob GetNewPackageJobForPackTypeTest() => Factory.New<CusPackageJob>();

		public void TestPackTypesExcludeCNT()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = CusPackageJob.LoadOrCreatePackageJob(cusPackingList);
			var package = packageJob.Packages.AddNew();
			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Factory, excludeCNT: true), package.Lookups.PackTypes);
		}
	}
}
