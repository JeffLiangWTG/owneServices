using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageJobWithAutoPackageBreakdownTest : PackingWithAutoPackageBreakdownTestCase
	{
		[ExpectNoExceptions]
		public void TestOnFactorySave_SplittingPackagesAndGeneratePackageIDs()
		{
			var dummyBizOWithPackageJob = Factory.New<DummyWithAutoPackageBreakdown>();
			dummyBizOWithPackageJob.PackageSequenceType = PackageSequenceType.Outer;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob);
			var firstPackage = packageJob.Packages.AddNew();
			AssertEquals("Before save it should have one Package", 1, packageJob.Packages.Count);
			AssertEquals("Before save it should one have package with empty PackageID", true, packageJob.Packages.Single().KP_PackageID.IsEmpty);
			AssertEquals("Sequence should be 0 if package has no ID", (ZShort)0, firstPackage.KP_Sequence);

			var expectedExceptionMessage = @"Package IDs could not be successfully generated. Errors below:
The Dummy does not yet have a Job Number. Try saving the Dummy first.";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZCannotSaveException), expectedExceptionMessage, true), "");
			AssertEquals("After save package should have ShouldGenerateIDOnSaving true", true, ((ISupportPackageIDGeneration)packageJob.Packages.Single()).ShouldGenerateIDOnSaving);

			firstPackage.KP_PackageQty = 3;
			dummyBizOWithPackageJob.JobNoForPackingParent = "abc";
			Factory.Save();
			AssertEquals("After save it should split the package into 3 packages.", 3, packageJob.Packages.Count);
			AssertEquals("After save it should generate PackageID for all packages.", 0, packageJob.Packages.Count(p => p.KP_PackageID.IsEmpty));
			AssertContainsExactElementsInAnyOrder("After save sequence should be populated for all packages.", new ZShort[] { 1, 2, 3 }, packageJob.Packages.Select(p => p.KP_Sequence));

			var newPackage = packageJob.Packages.AddNew();
			newPackage.KP_PackageQty = 1;
			AssertEquals("Package Job should have 4 Packages.", 4, packageJob.Packages.Count);
			AssertEquals("New Package should have no ID.", 1, packageJob.Packages.Count(p => p.KP_PackageID.IsEmpty));

			Factory.Save();
			AssertEquals("After save it should not split Packages again.", 4, packageJob.Packages.Count);
			AssertEquals("After save it should generate PackageIDs for new Packages.", 0, packageJob.Packages.Count(p => p.KP_PackageID.IsEmpty));
			AssertEquals("After save all Packages should have ShouldGenerateIDOnSaving true.", 4, packageJob.Packages.Count(p => ((ISupportPackageIDGeneration)p).ShouldGenerateIDOnSaving));
			AssertContainsExactElementsInAnyOrder("After save sequence should be populated for all packages.", new ZShort[] { 1, 2, 3, 4 }, packageJob.Packages.Select(p => p.KP_Sequence));

			var container = packageJob.Packages.AddNew("CNT");
			container.KP_PackageQty = 3;
			AssertEquals("Should not split container package.", 5, packageJob.Packages.Count);
			AssertEquals("After save all Packages Except container should have ShouldGenerateIDOnSaving true.", 4, packageJob.Packages.Count(p => ((ISupportPackageIDGeneration)p).ShouldGenerateIDOnSaving));
		}

		public void TestOnFactorySave_SplittingPackagesAndGeneratePackageIDs_DoneOutsideTransaction()
		{
			var dummyBizOWithPackageJob = Factory.New<DummyWithAutoPackageBreakdown>();
			dummyBizOWithPackageJob.JobNoForPackingParent = "abc";
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob);
			packageJob.Packages.AddNew("PLT", 10);
			packageJob.Packages.AddNew("BOX", 5);
			packageJob.Packages.AddNew("CTN");
			var packageWithID = packageJob.Packages.AddNew("BOX", "123");

			var isFactoryInTransaction = true;
			var isSemaphoreSuspendedDuringBreakdown = true;
			packageJob.Packages.CollectionCountChange += (sender, e) =>
			{
				isFactoryInTransaction = Factory.IsInTransaction;
				isSemaphoreSuspendedDuringBreakdown = isSemaphoreSuspendedDuringBreakdown && dummyBizOWithPackageJob.AutoBreakdownSemaphore.IsSuspended;
			};

			Factory.Save();
			AssertEquals("Should have broken down the packages outsides of Transaction.", false, isFactoryInTransaction);
			AssertEquals("Semaphore should be suspended throughout whole duration of Auto Breakdown.", true, isSemaphoreSuspendedDuringBreakdown);
			AssertContainsExactElementsInAnyOrder(packageJob.Packages.Where(p => p != packageWithID),
				dummyBizOWithPackageJob.PackagesPassedIntoRunAfterAllAutoCreatedPackagesAreAdded);
		}
	}
}
