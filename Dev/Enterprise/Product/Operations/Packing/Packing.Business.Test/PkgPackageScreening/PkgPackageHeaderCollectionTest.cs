using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageScreeningCollection))]
	class PkgPackageScreeningCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageScreeningCollection>
	{
		#region TestGetScreeningCollection

		public void TestGetScreeningCollection()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var screening1 = Helper.CreatePackageScreening(package, "ABC");
			var screening2 = Helper.CreatePackageScreening(package, "CBA");
			Factory.Save();

			var screenings = new PkgPackageScreeningCollection(package);
			AssertNotNull(screenings);
			AssertEquals("Should have 2 screenings", 2, screenings.Count);

			AssertEquals("Method should match", screening1.KPS_Method, screenings.Single(s => s.KPS_Method == "ABC").KPS_Method);
			AssertEquals("Method should match", screening2.KPS_Method, screenings.Single(s => s.KPS_Method == "CBA").KPS_Method);
		}

		#endregion

		#region Implementation

		protected override PkgPackageScreeningCollection GetCollectionToTest()
		{
			var package = Factory.New<PkgPackage>();
			return new PkgPackageScreeningCollection(package);
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
