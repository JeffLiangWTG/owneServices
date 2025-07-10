namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageComparerTest : PackingTestCaseWithFactory
	{
		public void TestComparer_InnerPackages()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var parentPackage = Helper.CreatePackage(packageJob, "ID0001", 1, "BOX");
			var boxWithID1 = Helper.CreatePackage(parentPackage, 1, "BOX", "ID0001");
			var boxWithID2 = Helper.CreatePackage(parentPackage, 1, "BOX", "ID0002");
			var boxWithoutID = Helper.CreatePackage(parentPackage, 1, "BOX");
			var palletWithoutID1 = Helper.CreatePackage(parentPackage, 1, "PLT");
			var palletWithoutID2 = Helper.CreatePackage(parentPackage, 1, "PLT");

			// cache description
			_ = boxWithID1.Description;
			_ = boxWithID2.Description;
			_ = boxWithoutID.Description;
			_ = palletWithoutID1.Description;
			_ = palletWithoutID2.Description;

			var comparer = new PkgPackageComparer();
			AssertPersistentPropertiesHitCount("Without Caching properties, should hit same properties multiple times.", 36, () =>
			{
				AssertEquals("Box should be before Pallet", -1, comparer.Compare(boxWithoutID, palletWithoutID1));
				AssertEquals("Box with ID should be before Box without ID.", -1, comparer.Compare(boxWithID1, boxWithoutID));
				AssertEquals("Box with lower ID should be first.", -1, comparer.Compare(boxWithID1, boxWithID2));
				AssertEquals("Two pallets with no ID should be equal.", 0, comparer.Compare(palletWithoutID1, palletWithoutID2));
				AssertEquals("Same package with same ID should be equal.", 0, comparer.Compare(boxWithID1, boxWithID1));
			});
		}

		public void TestComparer_InnerPackages_WithCaching()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var parentPackage = Helper.CreatePackage(packageJob, "ID0001", 1, "BOX");
			var boxWithID1 = Helper.CreatePackage(parentPackage, 1, "BOX", "ID0001");
			var boxWithID2 = Helper.CreatePackage(parentPackage, 1, "BOX", "ID0002");
			var boxWithoutID = Helper.CreatePackage(parentPackage, 1, "BOX");
			var palletWithoutID1 = Helper.CreatePackage(parentPackage, 1, "PLT");
			var palletWithoutID2 = Helper.CreatePackage(parentPackage, 1, "PLT");

			// cache description
			_ = boxWithID1.Description;
			_ = boxWithID2.Description;
			_ = boxWithoutID.Description;
			_ = palletWithoutID1.Description;
			_ = palletWithoutID2.Description;

			var comparer = new PkgPackageComparer();
			using (comparer.CacheSortingProperties())
			{
				AssertPersistentPropertiesHitCount("With Caching properties, should only hit each property once per package.", 19, () =>
				{
					AssertEquals("Box should be before Pallet", -1, comparer.Compare(boxWithoutID, palletWithoutID1));
					AssertEquals("Box with ID should be before Box without ID.", -1, comparer.Compare(boxWithID1, boxWithoutID));
					AssertEquals("Box with lower ID should be first.", -1, comparer.Compare(boxWithID1, boxWithID2));
					AssertEquals("Two pallets with no ID should be equal.", 0, comparer.Compare(palletWithoutID1, palletWithoutID2));
					AssertEquals("Same package with same ID should be equal.", 0, comparer.Compare(boxWithID1, boxWithID1));
				});
			}
		}

		public void TestComparer_OuterPackage()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var boxWithID1 = Helper.CreatePackage(packageJob, "ID0001", 1, "BOX");
			var boxWithID2 = Helper.CreatePackage(packageJob, "ID0002", 1, "BOX");
			var boxWithoutID = Helper.CreatePackage(packageJob, "", 1, "BOX");
			var palletWithoutID1 = Helper.CreatePackage(packageJob, "", 1, "PLT");
			var palletWithID2 = Helper.CreatePackage(packageJob, "ID003", 1, "PLT");

			var comparer = new PkgPackageComparer();
			AssertPersistentPropertiesHitCount("Without Caching properties, should hit same properties multiple times.", 8, () =>
			{
				AssertEquals(-1, comparer.Compare(boxWithoutID, palletWithoutID1));
				AssertEquals(-1, comparer.Compare(boxWithID1, boxWithoutID));
				AssertEquals(-1, comparer.Compare(boxWithID1, boxWithID2));
				AssertEquals(-1, comparer.Compare(palletWithoutID1, palletWithID2));
			});

			using (comparer.CacheSortingProperties())
			{
				AssertPersistentPropertiesHitCount("With Caching properties, should only hit each property once per package.", 5, () =>
				{
					AssertEquals(-1, comparer.Compare(boxWithoutID, palletWithoutID1));
					AssertEquals(-1, comparer.Compare(boxWithID1, boxWithoutID));
					AssertEquals(-1, comparer.Compare(boxWithID1, boxWithID2));
					AssertEquals(-1, comparer.Compare(palletWithoutID1, palletWithID2));
				});
			}
		}
	}
}
