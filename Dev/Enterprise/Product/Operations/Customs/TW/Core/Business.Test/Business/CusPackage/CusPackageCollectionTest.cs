using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackageCollection))]
	sealed class CusPackageCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPackageCollection>
	{
		protected override CusPackageCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			var cusPackingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			return (CusPackageCollection)cusPackingList.PackageJob.Packages;
		}
	}
}
