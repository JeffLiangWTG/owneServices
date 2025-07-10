using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PackingConflictTrackerTest : TestCaseWithDummy
	{
		public void TestAddWarning()
		{
			const string error1 = "Magic";
			const string error2 = "Smoke";
			Dummy.Z0_VarCharMax = "BOB";
			Dummy.RunPreSaveValidation();
			AssertNoNotifications("precondition", Dummy);
			ConflictNotificationTracker.AddWarning(Dummy.Z0_VarCharMaxInfo, error1);
			Dummy.Validation.ValidateZ0_VarCharMax();
			AssertHasWarning("Notification added (1)", Dummy.Z0_VarCharMaxInfo, error1);
			Dummy.Z0_VarCharMax = "CAT";
			AssertNoNotifications("remove notifications when the property changes.", Dummy);
			ConflictNotificationTracker.AddWarning(Dummy.Z0_VarCharMaxInfo, error1);
			ConflictNotificationTracker.AddWarning(Dummy.Z0_VarCharMaxInfo, error2);
			Dummy.Validation.ValidateZ0_VarCharMax();
			AssertHasWarning("Notification added (2)", Dummy.Z0_VarCharMaxInfo, error1);
			AssertHasWarning("Notification added (3)", Dummy.Z0_VarCharMaxInfo, error2);
			Factory.Save();
			AssertNoNotifications("Warnings should be removed on save.", Dummy);
		}
	}
}
