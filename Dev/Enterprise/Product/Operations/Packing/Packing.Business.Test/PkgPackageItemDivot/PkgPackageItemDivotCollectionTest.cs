using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageItemDivotCollection))]
	public class PkgPackageItemDivotCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPackageItemDivotCollection>
	{
		#region TestGetDivotWrappers

		public void TestGetDivotWrappers()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var packedItems1 = package.Pack(Data.DummyLine1, 10m).Single();
			var packedItems2 = package.Pack(Data.DummyLine1, 20m).Single();
			var packedItems3 = package.Pack(Data.DummyLine2, 40m).Single();
			var packedItems4 = package.Pack(Data.DummyLine3, 50m).Single();
			Factory.Save();

			var divotToDelete = package.PackedItemDivots.Single(d => d.PackedItem.Key == Data.DummyLine2.PackableItems.Single(p => p.Quantity == 40m).Key);
			divotToDelete.Delete(); // Deleted divot should not create any wrapper
			AssertEquals("divotToDelete should be deleted", true, divotToDelete.IsDeleted);

			var wrappers = new PkgPackageItemDivotCollection(package).GetDivotWrappers();
			AssertNotNull(wrappers);
			AssertEquals("Should create 2 wrappers", 2, wrappers.Count);

			var wrapper1 = wrappers.Single(w => w.PackedQty == 30m); // wrapper should have 2 divots for DummyLine1
			AssertEquals("Wrapper description shoulde be match", packedItems1.Description, wrapper1.Description);
			AssertEquals("Wrapper description shoulde be match", packedItems2.Description, wrapper1.Description);

			var wrapper2 = wrappers.Single(w => w.PackedQty == 50m); // wrapper should have 1 divot for DummyLine3
			AssertEquals("Wrapper description shoulde be match", packedItems4.Description, wrapper2.Description);
		}

		#endregion

		#region 

		public void TestLoadAllPackableItemParentsInOneHit()
		{
			Data.CreatePackingData();
			var dummyBizOWithPackageJob = Factory.New<DummyWithPacking>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob);
			var package = packageJob.Packages.AddNew();

			package.Pack(Data.DummyLine1, 10m);
			package.Pack(Data.DummyLine1, 20m);
			Factory.Save();

			AssertEquals("Precondition: LoadAllPackableItemParentsInOneHit() has not been called.", false, dummyBizOWithPackageJob.LoadAllPackableItemsWasHit);
			AssertNull("Precondition: LoadAllPackableItemParentsInOneHit() has not been called.", dummyBizOWithPackageJob.PackageJobForLoadAllPackableItemsInOneHit);
			var wrappers = new PkgPackageItemDivotCollection(package).GetDivotWrappers();
			AssertNotNull(wrappers);
			AssertEquals("Should have called LoadAllPackableItemParentsInOneHit() on Parent Job.", true, dummyBizOWithPackageJob.LoadAllPackableItemsWasHit);
			AssertEquals("Should have called LoadAllPackableItemParentsInOneHit() on Parent Job with PackageJob passed in.", packageJob, dummyBizOWithPackageJob.PackageJobForLoadAllPackableItemsInOneHit);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected override PkgPackageItemDivotCollection GetCollectionToTest()
		{
			var package = Factory.New<PkgPackage>();
			return new PkgPackageItemDivotCollection(package);
		}

		protected TestDataForPacking Data
		{
			get { return data ?? (data = new TestDataForPacking(Factory)); }
		}

		TestDataForPacking data;

		#endregion
	}
}
