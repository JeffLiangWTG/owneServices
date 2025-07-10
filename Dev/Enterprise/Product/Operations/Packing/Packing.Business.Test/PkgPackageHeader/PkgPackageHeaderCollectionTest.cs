using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageHeaderCollection))]
	class PkgPackageHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageHeaderCollection>
	{
		protected override PkgPackageHeaderCollection GetCollectionToTest()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			return new PkgPackageHeaderCollection(packageJob);
		}
	}
}
