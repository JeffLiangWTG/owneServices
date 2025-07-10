using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageSealCollection))]
	class PkgPackageSealCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageSealCollection>
	{
		#region TestGetPackageHandlingUnitDivotCollection

		public void TestGetPkgPackageSealCollection()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("PLT", "HU");

			Helper.CreatePackageSeal(package, "Test Seal 1");
			Helper.CreatePackageSeal(package, "Test Seal 2", false);
			Factory.Save();

			var packageSeals = new PkgPackageSealCollection(package);
			AssertNotNull(packageSeals);
			AssertEquals("Should have 2 seals", 2, packageSeals.Count);

			var seal1 = package.PackageSeals.Single(d => d.KPE_IsSealOK);
			var seal2 = package.PackageSeals.Single(d => !d.KPE_IsSealOK);
			AssertContainsExactElementsInAnyOrder(new[] { seal1, seal2 }, packageSeals);
		}

		#endregion

		#region Implementation

		protected override PkgPackageSealCollection GetCollectionToTest()
		{
			var package = Factory.New<PkgPackage>();

			return new PkgPackageSealCollection(package);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory));
		TestDataForPacking data;

		protected PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory));
		PackingTestHelper helper;

		#endregion
	}
}
