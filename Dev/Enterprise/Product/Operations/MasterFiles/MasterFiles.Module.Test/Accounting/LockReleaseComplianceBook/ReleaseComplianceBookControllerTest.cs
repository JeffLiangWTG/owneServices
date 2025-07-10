using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ReleaseComplianceBookController))]
	sealed class ReleaseComplianceBookControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ReleaseComplianceBook;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ReleaseComplianceBook(Factory);
		}

		public void TestSecurityRight()
		{
			var oldvalue1 = Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed;
			var oldvalue2 = Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed;

			try
			{
				var propertyInfo = typeof(ReleaseComplianceBookController).GetProperty("CheckPointForNew", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
				Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

				AssertEquals(true, ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).IsAllowed);
				AssertEquals("Lock / Release Counter Compliance Book", ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).HumanReadableName);

				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
				Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

				AssertEquals(true, ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).IsAllowed);
				AssertEquals("Release Counter Compliance Book Locked By Other Staff", ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).HumanReadableName);

				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
				Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

				AssertEquals(true, ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).IsAllowed);
				AssertEquals("Lock / Release Counter Compliance Book", ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).HumanReadableName);

				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
				Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

				AssertEquals(false, ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).IsAllowed);
				AssertEquals("Release Counter Compliance Book Locked By Other Staff", ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).HumanReadableName);
			}
			finally
			{
				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = oldvalue1;
				Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = oldvalue2;
			}
		}
	}
}
