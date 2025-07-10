using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Packing;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageActionStrategyTest : TestCaseWithFactory
	{
		public void TestAllProperties()
		{
			var package = Factory.New<PkgPackage>();

			var actionStrategy = new PackageActionStrategy(package, PackageAction.Delete | PackageAction.PackUnpack, "Cannot delete or modify.");
			AssertActionStrategy(actionStrategy, false, false, true, "Cannot delete or modify.");

			actionStrategy = new PackageActionStrategy(package, PackageAction.Delete, "Cannot delete.");
			AssertActionStrategy(actionStrategy, false, true, true, "Cannot delete.");

			actionStrategy = new PackageActionStrategy(package, PackageAction.PackUnpack, "Cannot modify.");
			AssertActionStrategy(actionStrategy, true, false, true, "Cannot modify.");

			actionStrategy = new PackageActionStrategy(package, PackageAction.Edit, "Cannot edit.");
			AssertActionStrategy(actionStrategy, true, true, false, "Cannot edit.");
		}

		void AssertActionStrategy(PackageActionStrategy actionStrategy, bool allowDelete, bool allowPackUnpack, bool allowEdit, ZString reasonForNotAllowingAction)
		{
			AssertEquals("AllowDelete should be " + allowDelete, allowDelete, actionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("AllowPackUnpack should be " + allowPackUnpack, allowPackUnpack, actionStrategy.IsActionAllowed(PackageAction.PackUnpack));
			AssertEquals("AllowEdit should be " + allowEdit, allowEdit, actionStrategy.IsActionAllowed(PackageAction.Edit));
			AssertEquals("ReasonForNotAllowingAction should be : " + reasonForNotAllowingAction, reasonForNotAllowingAction, actionStrategy.ReasonForNotAllowingAction);
		}
	}
}
