using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackageJob))]
	sealed class CusPackageJobTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.NewWithValidTestData<JobDeclaration>();
			var cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(factory);
			var packageJob = cusPackingList.PackageJob;
			packageJob.Packages.AddNew();
			return packageJob;
		}

		[ExpectNoExceptions]
		public void TestPackagesType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cusPackingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackageJob = cusPackingList.PackageJob;
			NUnit.Framework.Assert.That(cusPackageJob.Packages, NUnit.Framework.Is.TypeOf(typeof(CusPackageCollection)));
		}
	}
}
