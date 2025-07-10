using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PkgPackageCollection))]
	public class PkgPackageCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageCollection>
	{
		protected override PkgPackageCollection GetCollectionToTest()
		{
			var parent = Factory.New<PackLine>();
			return new PkgPackageCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<PkgPackage>();
		}
	}
}
