using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageJobPackageHeaderPivotCollection))]
	class PkgPackageJobPackageHeaderPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageJobPackageHeaderPivotCollection>
	{
		protected override PkgPackageJobPackageHeaderPivotCollection GetCollectionToTest()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			return new PkgPackageJobPackageHeaderPivotCollection(packageJob);
		}
	}
}
