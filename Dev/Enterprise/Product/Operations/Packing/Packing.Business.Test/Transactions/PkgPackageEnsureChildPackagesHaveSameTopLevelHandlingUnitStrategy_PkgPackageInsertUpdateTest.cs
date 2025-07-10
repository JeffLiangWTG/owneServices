using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdateTest : TestCaseWithFactory
	{
		public void TestShouldDeferTriggerWhenInsert()
		{
			var package = CreatePackage();
			package.KP_KP_TopHandlingUnitPackage = ZGuid.NewZGuid();

			AssertEquals("Package has changes", true, package.HasChanges);

			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate));
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(package));
		}

		public void TestShouldNotDeferTriggerWhenUpdateWithoutChanges()
		{
			var package = CreatePackage();
			AssertEquals("Package has changes", true, package.HasChanges);

			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate));
			AssertEquals("Trigger should be deferred at this stage of setup.", true, strategy.ShouldDeferTrigger(package));
			Factory.Save();

			AssertEquals("Package has no changes", false, package.HasChanges);
			AssertEquals("Trigger should not be deferred for update without changes.", false, strategy.ShouldDeferTrigger(package));
		}

		public void TestShouldDeferTriggerWhenUpdateWithChanges()
		{
			var package = CreatePackage();
			AssertEquals("Package has changes", true, package.HasChanges);

			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate));
			AssertEquals("Trigger should be deferred at this stage of setup.", true, strategy.ShouldDeferTrigger(package));
			Factory.Save();

			package.KP_KP_TopHandlingUnitPackage = ZGuid.NewZGuid();
			AssertEquals("Package has changes", true, package.HasChanges);
			AssertEquals("Trigger should be deferred for update with changes.", true, strategy.ShouldDeferTrigger(package));
		}

		PkgPackage CreatePackage()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			return package;
		}
	}
}
