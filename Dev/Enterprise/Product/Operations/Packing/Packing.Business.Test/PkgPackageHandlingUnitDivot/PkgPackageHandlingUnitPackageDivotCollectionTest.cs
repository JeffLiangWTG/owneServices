using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageHandlingUnitPackageDivotCollection))]
	class PkgPackageHandlingUnitPackageDivotCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageHandlingUnitPackageDivotCollection>
	{
		#region TestGetPackageHandlingUnitDivotCollection

		public void TestGetPackageHandlingUnitDivotCollection()
		{
			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("PLT", "HU");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "HU");
			var package3 = Data.PackageJob.Packages.AddNew("BOX", "BOX");

			var divot1 = Helper.CreatePackageHandlingUnitDivot(package1, package3);
			var divot2 = Helper.CreatePackageHandlingUnitDivot(package2, package3);
			Factory.Save();

			var divots = new PkgPackageHandlingUnitPackageDivotCollection(package3);
			AssertEquals("Should have 2 divots", 2, divots.Count);

			AssertNotNull("Method should match", divots.Single(d => d.KPD_KP_HandlingUnit == package1.PK && d.KPD_KP_Package == package3.PK));
			AssertNotNull("Method should match", divots.Single(d => d.KPD_KP_HandlingUnit == package2.PK && d.KPD_KP_Package == package3.PK));
		}

		#endregion

		#region Implementation

		protected override PkgPackageHandlingUnitPackageDivotCollection GetCollectionToTest()
		{
			var package = Factory.New<PkgPackage>();
			return new PkgPackageHandlingUnitPackageDivotCollection(package);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected TestDataForPacking Data
		{
			get { return data ?? (data = new TestDataForPacking(Factory)); }
		}

		TestDataForPacking data;

		protected PackingTestHelper Helper
		{
			get { return helper ?? (helper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper helper;

		#endregion
	}
}
