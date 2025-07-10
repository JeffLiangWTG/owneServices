using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdateTest : TestCaseWithFactory
	{
		public void TestShouldDeferTriggerWhenInsert()
		{
			var packageHeader = CreatePackageHeader();
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate));
			AssertEquals("Trigger should not be deferred when inserting.", false, strategy.ShouldDeferTrigger(packageHeader));
		}

		public void TestShouldNotDeferTriggerWhenUpdateWithoutChanges()
		{
			var packageHeader = CreatePackageHeader();
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate));
			AssertEquals("Trigger should not be deferred at this stage of setup.", false, strategy.ShouldDeferTrigger(packageHeader));
			Factory.Save();

			AssertEquals("Trigger should not be deferred for update without changes.", false, strategy.ShouldDeferTrigger(packageHeader));
		}

		public void TestShouldDeferTriggerWhenUpdateWithChanges()
		{
			var packageHeader = CreatePackageHeader();
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate));
			AssertEquals("Trigger should not be deferred at this stage of setup.", false, strategy.ShouldDeferTrigger(packageHeader));
			Factory.Save();

			packageHeader.KPH_PackageID = "DEF";
			AssertEquals("Trigger should be deferred for update with changes.", true, strategy.ShouldDeferTrigger(packageHeader));
		}

		PkgPackageHeader CreatePackageHeader()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			return package.PackageID;
		}
	}
}
