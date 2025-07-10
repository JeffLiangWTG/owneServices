using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageSeal))]
	class PkgPackageSealTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var package = Factory.New<PkgPackage>();
			var packageSeal = Helper.CreatePackageSeal(package, "Test Seal");

			return packageSeal;
		}

		PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory));
		PackingTestHelper helper;
	}
}
