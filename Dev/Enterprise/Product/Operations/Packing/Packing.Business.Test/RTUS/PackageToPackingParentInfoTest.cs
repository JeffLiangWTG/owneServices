using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageToPackingParentInfoTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PackageToPackingParentInfo(null, Factory.New<PkgPackage>(), "ABC"));
			AssertExceptionThrown<ArgumentNullException>(() => new PackageToPackingParentInfo(new Mock<IPackingParent>().Object, null, "ABC"));
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentException), "Packing Parent ID should not be empty.\r\nParameter name: packingParentId",
				() => new PackageToPackingParentInfo(new Mock<IPackingParent>().Object, Factory.New<PkgPackage>(), ""));
#else
			AssertExceptionThrown(typeof(ArgumentException), "Packing Parent ID should not be empty. (Parameter 'packingParentId')",
				() => new PackageToPackingParentInfo(new Mock<IPackingParent>().Object, Factory.New<PkgPackage>(), ""));
#endif
		}

		public void TestPackingParent()
		{
			var packingParent = new Mock<IPackingParent>().Object;
			var packageToPackingParentInfo = new PackageToPackingParentInfo(packingParent, Factory.New<PkgPackage>(), "ABC");
			AssertEquals(packingParent, packageToPackingParentInfo.PackingParent);
		}

		public void TestPackage()
		{
			var package = Factory.New<PkgPackage>();
			var packageToPackingParentInfo = new PackageToPackingParentInfo(new Mock<IPackingParent>().Object, package, "ABC");
			AssertEquals(package.PK, packageToPackingParentInfo.Package.PK);
		}

		public void TestPackingParentId()
		{
			var packageToPackingParentInfo = new PackageToPackingParentInfo(new Mock<IPackingParent>().Object, Factory.New<PkgPackage>(), "ABC");
			AssertEquals("ABC", packageToPackingParentInfo.PackingParentId);
		}
	}
}
