using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing.PackageState
{
	[TestedType(typeof(WhsItemPackageStateProcessTaskCollection))]
	public class WhsItemPackageStateProcessTaskCollectionTest : ProcessTaskCollectionTest<WhsItemPackageStateProcessTaskCollection>
	{
		protected override WhsItemPackageStateProcessTaskCollection GetCollectionToTestCore()
		{
			var packageState = Factory.NewWithValidTestData<WhsItemPackageState>();
			return new WhsItemPackageStateProcessTaskCollection(packageState);
		}
	}
}
