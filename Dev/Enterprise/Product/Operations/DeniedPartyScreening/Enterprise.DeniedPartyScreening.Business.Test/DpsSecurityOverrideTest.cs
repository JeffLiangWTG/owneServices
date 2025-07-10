using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	class DeniedPartySecurityOverrideTest : TestCaseWithFactory
	{
		public void TestCheckIfOverridingUserHasPrivilegeSucceed()
		{
			var password = "topsecret";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = password;
			var securityAcceptanceOfClear = staff.StaffSecurityPermissionsCollection.AddNew();
			securityAcceptanceOfClear.GU_SecurityRight = "DpsAllowUpdateToClear";
			securityAcceptanceOfClear.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			var checkSecurityFunctions = new Func<SecurityCore, SecurityCheckpoint>[] { s => s.DpsAllowUpdateToClear };
			var dpsSecurityOverride = new DpsSecurityOverride(checkSecurityFunctions);
			var errorMessage = dpsSecurityOverride.CheckUserPrivilege(staff.GS_LoginName, password);

			AssertEquals(string.Empty, errorMessage);
			Assert(dpsSecurityOverride.IsAllowed);
		}

		public void TestCheckIfOverridingUserHasPrivilegeFail_UsernamePasswordIsInvalid()
		{
			var password = "topsecret";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = password;
			Factory.Save();

			var checkSecurityFunctions = new Func<SecurityCore, SecurityCheckpoint>[] { s => s.DpsAllowUpdateToClear };
			var dpsSecurityOverride = new DpsSecurityOverride(checkSecurityFunctions);
			var errorMessage = dpsSecurityOverride.CheckUserPrivilege(staff.GS_LoginName, password + "InvalidPassword");

			AssertEquals("Username/password is invalid", errorMessage);
			Assert(!dpsSecurityOverride.IsAllowed);
		}

		public void TestCheckIfOverridingUserHasPrivilegeFail_InsufficientPrivilege()
		{
			var password = "topsecret";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = password;
			Factory.Save();

			var checkSecurityFunctions = new Func<SecurityCore, SecurityCheckpoint>[] { s => s.DpsAllowUpdateToClear };
			var dpsSecurityOverride = new DpsSecurityOverride(checkSecurityFunctions);
			var errorMessage = dpsSecurityOverride.CheckUserPrivilege(staff.GS_LoginName, password);

			AssertEquals("", errorMessage);
			Assert(!dpsSecurityOverride.IsAllowed);
			AssertEquals(1, dpsSecurityOverride.NotAllowedSecurityCheckFunctions.Length);
			AssertEquals(checkSecurityFunctions[0], dpsSecurityOverride.NotAllowedSecurityCheckFunctions[0]);
		}
	}
}
