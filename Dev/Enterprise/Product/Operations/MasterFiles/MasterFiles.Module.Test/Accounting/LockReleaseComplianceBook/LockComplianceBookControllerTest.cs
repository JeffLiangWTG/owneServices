using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(LockComplianceBookController))]
	sealed class LockComplianceBookControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LockComplianceBook;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new LockComplianceBook(Factory);
		}

		public void TestSecurityRight()
		{
			var oldvalue = Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed;

			try
			{
				var propertyInfo = typeof(LockComplianceBookController).GetProperty("CheckPointForNew", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;

				AssertEquals(true, ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).IsAllowed);
				AssertEquals("Lock / Release Counter Compliance Book", ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).HumanReadableName);

				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;

				AssertEquals(false, ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).IsAllowed);
				AssertEquals("Lock / Release Counter Compliance Book", ((SecurityCheckpoint)propertyInfo.GetValue(Controller)).HumanReadableName);
			}
			finally
			{
				Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = oldvalue;
			}
		}
	}
}
