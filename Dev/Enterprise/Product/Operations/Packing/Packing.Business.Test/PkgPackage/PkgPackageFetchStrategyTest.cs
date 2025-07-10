using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageFetchStrategyTest : PackingTestCaseWithFactory
	{
		#region TestFetchForLoad_Packages

		public void TestFetchForLoad_Packages()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			// level1
			var container1 = packageJob.Packages.AddNew();
			var container2 = packageJob.Packages.AddNew();

			// level2
			var pallet1 = container1.Packages.AddNew();
			var pallet2 = container2.Packages.AddNew();

			// level 3
			var box1 = pallet1.Packages.AddNew();
			var box2 = pallet2.Packages.AddNew();

			// level 4 - no data, but a db hit will occur to determine this

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var otherPackageJob = otherFactory.Load<PkgPackageJob>(packageJob.PK);
			AssertMaxDbHits("Should not have loaded anything other than the PackageJob.", 1, otherFactory);

			PokeAllPackages(otherPackageJob.Packages);
			AssertMaxDbHits("Should be one DB hit per package level.", 1 + 4, otherFactory);
		}

		void PokeAllPackages(PkgPackageCollection packages)
		{
			foreach (var package in packages)
			{
				var poke = package.PK;
				PokeAllPackages(package.Packages);
			}
		}

		#endregion

		#region TestFetchForLoad_Divots

		public void TestFetchForLoad_Divots()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			// level1
			var container1 = packageJob.Packages.AddNew();
			var container2 = packageJob.Packages.AddNew();
			container1.Pack(Data.DummyLine1, 5m);
			container2.Pack(Data.DummyLine2, 5m);

			// level2
			var pallet1 = container1.Packages.AddNew();
			var pallet2 = container2.Packages.AddNew();
			pallet1.Pack(Data.DummyLine1, 5m);
			pallet2.Pack(Data.DummyLine2, 5m);

			// level 3
			var box1 = pallet1.Packages.AddNew();
			var box2 = pallet2.Packages.AddNew();
			box1.Pack(Data.DummyLine1, 5m);
			box2.Pack(Data.DummyLine2, 5m);

			// level 4 - no packages, but a db hit will occur to determine this

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var otherPackageJob = otherFactory.Load<PkgPackageJob>(packageJob.PK);
			AssertMaxDbHits("Should not have loaded anything other than the PackageJob.", 1, otherFactory);

			PokeAllDivots(otherPackageJob.Packages);
			AssertMaxDbHits("Should be one DB hit per package level, and one DB hit per divot level.", 1 + 4 + 3, otherFactory);
		}

		void PokeAllDivots(PkgPackageCollection packages)
		{
			foreach (var package in packages)
			{
				foreach (var divot in package.PackedItemDivots)
				{
					var poke = divot.PK;
				}
				PokeAllDivots(package.Packages);
			}
		}

		#endregion
	}
}
