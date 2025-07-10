using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageOrderReference))]
	class PkgPackageOrderReferenceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var package = Factory.New<PkgPackage>();
			var packageOrderReference = Helper.CreatePackageOrderReference(package, "123");

			return packageOrderReference;
		}

		PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory));
		PackingTestHelper helper;
	}
}
