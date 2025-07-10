using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPackageJobFetchStrategyTest : PackingTestCaseWithFactory
	{
		#region TestFetchForLoad_PackagesInMultiplePackageJobs

		public void TestFetchForLoad_PackagesInMultiplePackageJobs()
		{
			var dummyBizOWithPackageJob1 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob1);
			packageJob1.Packages.AddNew();
			packageJob1.Packages.AddNew();
			packageJob1.Packages.AddNew().Packages.AddNew();

			var dummyBizOWithPackageJob2 = Factory.New<DummyWithPacking>();
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob2);
			packageJob2.Packages.AddNew();
			packageJob2.Packages.AddNew().Packages.AddNew().Packages.AddNew();
			packageJob2.Packages.AddNew();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var packageJob1InNewFactory = newFactory.Load<PkgPackageJob>(packageJob1.PK);
			var packageJob2InNewFactory = newFactory.Load<PkgPackageJob>(packageJob2.PK);
			PokeAllPackages(packageJob1InNewFactory.Packages);
			PokeAllPackages(packageJob2InNewFactory.Packages);
			AssertMaxDbHits("Should be Two DB Hits for Package Jobs, one DB hit for two top packages and 1 for child packages.", 2 + 2, newFactory);
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

		#region TestFetchForLoad_PackageIDs

		public void TestFetchForLoad_PackageIDs()
		{
			var dummyBizOWithPackageJob1 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob1);
			packageJob1.Packages.AddNew().KP_PackageID = "123";
			packageJob1.Packages.AddNew().KP_PackageID = "234";
			var pkg345 = packageJob1.Packages.AddNew();
			pkg345.KP_PackageID = "345";
			pkg345.Packages.AddNew().KP_PackageID = "456";

			var dummyBizOWithPackageJob2 = Factory.New<DummyWithPacking>();
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob2);
			packageJob2.Packages.AddNew().KP_PackageID = "123";
			var pkg234b = packageJob2.Packages.AddNew();
			var pkg345b = pkg234b.Packages.AddNew();
			pkg234b.KP_PackageID = "234";
			pkg345b.KP_PackageID = "345";
			pkg345b.Packages.AddNew().KP_PackageID = "456";
			packageJob2.Packages.AddNew().KP_PackageID = "567";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var packageJob1InNewFactory = newFactory.Load<PkgPackageJob>(packageJob1.PK);
			var packageJob2InNewFactory = newFactory.Load<PkgPackageJob>(packageJob2.PK);
			PokeAllPackageIDs(packageJob1InNewFactory.Packages);
			PokeAllPackageIDs(packageJob2InNewFactory.Packages);

			// PkgPackage: 2
			// PkgPackageJob: 2
			// PkgPackageHeader: 1

			// Hits: 5
			AssertMaxDbHits("Should be Two DB Hits for Package Jobs, one DB hit for two top packages and 1 for child packages + 1 for Headers with id.", 2 + 2 + 1, newFactory);
		}

		void PokeAllPackageIDs(PkgPackageCollection packages)
		{
			foreach (var package in packages)
			{
				var poke = package.KP_PackageID;
				PokeAllPackageIDs(package.Packages);
			}
		}

		#endregion
	}
}
