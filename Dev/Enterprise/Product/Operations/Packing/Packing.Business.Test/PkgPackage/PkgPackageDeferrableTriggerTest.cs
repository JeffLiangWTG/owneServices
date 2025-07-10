using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackage))]
	public class PkgPackageDeferrableTriggerTest : DeferrableTriggerTestCase<PkgPackage>
	{
		#region TestDeferTriggerAndRunBeforeCommit

		public void TestDeferredTrigger_PkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnit()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_ParentID = Guid.NewGuid();

			var singleLevelHandlingUnit = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "HU1");
			var singleLevelHandlingUnitInner1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "PKG1");
			var singleLevelHandlingUnitInner2 = packageJob.Packages.AddNew(Constants.PkgUnit.Bag, "PKG2");

			var topLevelHandlingUnit = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "HU2");
			var subLevelHandlingUnit = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "HU3");
			var topLevelHandlingUnitInner = packageJob.Packages.AddNew(Constants.PkgUnit.Bag, "PKG4");
			var subLevelHandlingUnitInner = packageJob.Packages.AddNew(Constants.PkgUnit.Bag, "PKG5");

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var singleLevelHandlingUnitAfterReloading = factory2.Load<PkgPackage>(singleLevelHandlingUnit.PK);
			var singleLevelHandlingUnitInner1AfterReloading = factory2.Load<PkgPackage>(singleLevelHandlingUnitInner1.PK);
			var singleLevelHandlingUnitInner2AfterReloading = factory2.Load<PkgPackage>(singleLevelHandlingUnitInner2.PK);
			var topLevelHandlingUnitAfterReloading = factory2.Load<PkgPackage>(topLevelHandlingUnit.PK);
			var subLevelHandlingUnitAfterReloading = factory2.Load<PkgPackage>(subLevelHandlingUnit.PK);
			var topLevelHandlingUnitInnerAfterReloading = factory2.Load<PkgPackage>(topLevelHandlingUnitInner.PK);
			var subLevelHandlingUnitInnerAfterReloading = factory2.Load<PkgPackage>(subLevelHandlingUnitInner.PK);

			var packingHelper = new PackingTestHelper(factory2);

			// pack single level
			packingHelper.PackHandlingUnit(singleLevelHandlingUnitAfterReloading, singleLevelHandlingUnitInner1AfterReloading, singleLevelHandlingUnitAfterReloading);
			packingHelper.PackHandlingUnit(singleLevelHandlingUnitAfterReloading, singleLevelHandlingUnitInner2AfterReloading, singleLevelHandlingUnitAfterReloading);

			// pack multi level
			packingHelper.PackHandlingUnit(topLevelHandlingUnitAfterReloading, subLevelHandlingUnitAfterReloading, topLevelHandlingUnitAfterReloading);
			packingHelper.PackHandlingUnit(topLevelHandlingUnitAfterReloading, topLevelHandlingUnitInnerAfterReloading, topLevelHandlingUnitAfterReloading);
			packingHelper.PackHandlingUnit(subLevelHandlingUnitAfterReloading, subLevelHandlingUnitInnerAfterReloading, topLevelHandlingUnitAfterReloading);

			AssertNoExceptionThrown("trigger deferral should allow this", () => factory2.Save());

			CombineAssertions("packages should have the correct top handling unit package", () =>
			{
				AssertEquals(singleLevelHandlingUnitAfterReloading.PK, singleLevelHandlingUnitInner1AfterReloading.KP_KP_TopHandlingUnitPackage);
				AssertEquals(singleLevelHandlingUnitAfterReloading.PK, singleLevelHandlingUnitInner2AfterReloading.KP_KP_TopHandlingUnitPackage);
				AssertEquals(topLevelHandlingUnitAfterReloading.PK, subLevelHandlingUnitAfterReloading.KP_KP_TopHandlingUnitPackage);
				AssertEquals(topLevelHandlingUnitAfterReloading.PK, topLevelHandlingUnitInnerAfterReloading.KP_KP_TopHandlingUnitPackage);
				AssertEquals(topLevelHandlingUnitAfterReloading.PK, subLevelHandlingUnitInnerAfterReloading.KP_KP_TopHandlingUnitPackage);
			});
		}

		#endregion
	}
}
