using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageExtensionCollection))]
	class PkgPackageExtensionCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageExtensionCollection>
	{
		protected override PkgPackageExtensionCollection GetCollectionToTest()
		{
			var package = Factory.New<PkgPackage>();
			return new PkgPackageExtensionCollection(package);
		}

		public void TestGetPackageExtension_IsActive()
		{
			TestGetPackageExtension_IsActiveCore(true);
		}

		public void TestGetPackageExtension_IsInactive()
		{
			TestGetPackageExtension_IsActiveCore(false);
		}

		public void TestGetPackageExtension_IsActiveCore(bool isActive)
		{
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "PKG";
			var packageExtension = Factory.New<PkgPackageExtension>();
			packageExtension.KPN_KP_Package = package.PK;
			packageExtension.KPN_IsActive = isActive;
			var collection = new PkgPackageExtensionCollection(package);
			if (isActive)
			{
				Assertion.Assert("package extension collection should have one element", collection.Count == 1);
			}
			else
			{
				Assertion.Assert("package extension collection should have no element", collection.Count == 0);
			}
		}
	}
}
