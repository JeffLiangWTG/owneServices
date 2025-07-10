using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PasswordControlTest : TransactionedTestCase
	{
		public PasswordControlTest()
			: base()
		{
		}

		public void TestCheckPasswordHistory()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "testuser";
			staff.StaffPlainTextPassword = "pAassword_0";
			factory.Save();

			staff.ChangePassword("pAassword_0", "pAassword_1");
			factory.Save();
			staff.ChangePassword("pAassword_1", "pAassword_2");
			factory.Save();
			staff.ChangePassword("pAassword_2", "pAassword_3");
			factory.Save();

			var passwordHistories = PasswordHistoryHelper.GetPasswordHistories(staff);
			AssertEquals("Password history count", EnvProxy.Instance.Registry.PasswordHistoryCount - 1, passwordHistories.Count); // should be PasswordHistoryCount - 1 as GS_PasswordHash is the latest history
			AssertEquals("Pre-condition: pAassword_3 should have been used", true, staff.HasPasswordBeenUsed("pAassword_3"));
			AssertEquals("Pre-condition: pAassword_2 should have been used", true, staff.HasPasswordBeenUsed("pAassword_2"));
			AssertEquals("Pre-condition: pAassword_1 should have been used", true, staff.HasPasswordBeenUsed("pAassword_1"));
			AssertEquals("Pre-condition: pAassword_0 should have not been used", false, staff.HasPasswordBeenUsed("pAassword_0"));

			var passwordControl = new PasswordControl();
			AssertEquals("Change password - new password", true, passwordControl.NewPasswordIsValid(staff, "pAassword_3", "pAassword_X1", "pAassword_X1", out var _));
			AssertEquals("Change password - reuse password", false, passwordControl.NewPasswordIsValid(staff, "pAassword_3", "pAassword_2", "pAassword_2", out var _));
			AssertEquals("Reset password - new password", true, passwordControl.NewPasswordIsValid(staff, null, "pAassword_X2", "pAassword_X2", out var _));
			AssertEquals("Reset password - reuse password", true, passwordControl.NewPasswordIsValid(staff, null, "pAassword_2", "pAassword_2", out var _));
		}

		public void TestIsValidPassword()
		{
			PasswordControl pswdCtrl = new PasswordControl();
			Assert("Invalid password", !pswdCtrl.IsValidPassword("abc"));
			AssertEquals("Error count", 4, pswdCtrl.Errors.Count);
			Assert("Valid password", pswdCtrl.IsValidPassword("Password_1"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			fSavePasswordMinLength = EnvProxy.Instance.Registry.PasswordMinLength;
			fSavePasswordMinUpperAlphas = EnvProxy.Instance.Registry.PasswordMinUpperAlphas;
			fSavePasswordMinLowerAlphas = EnvProxy.Instance.Registry.PasswordMinLowerAlphas;
			fSavePasswordMinNumeric = EnvProxy.Instance.Registry.PasswordMinNumeric;
			fSavePasswordMinNonAlphNums = EnvProxy.Instance.Registry.PasswordMinNonAlphNums;

			EnvProxy.Instance.Registry.PasswordMinLength = 8;
			EnvProxy.Instance.Registry.PasswordMinUpperAlphas = 1;
			EnvProxy.Instance.Registry.PasswordMinLowerAlphas = 1;
			EnvProxy.Instance.Registry.PasswordMinNumeric = 1;
			EnvProxy.Instance.Registry.PasswordMinNonAlphNums = 1;
			EnvProxy.Instance.Registry.PasswordHistoryCount = 3;
		}

		protected override void TearDown()
		{
			base.TearDown();
			//Even though we are in a TransactionedTestCase, 
			//still need to restore Rgistry values as they are cached.
			EnvProxy.Instance.Registry.PasswordMinLength = fSavePasswordMinLength;
			EnvProxy.Instance.Registry.PasswordMinUpperAlphas = fSavePasswordMinUpperAlphas;
			EnvProxy.Instance.Registry.PasswordMinLowerAlphas = fSavePasswordMinLowerAlphas;
			EnvProxy.Instance.Registry.PasswordMinNumeric = fSavePasswordMinNumeric;
			EnvProxy.Instance.Registry.PasswordMinNonAlphNums = fSavePasswordMinNonAlphNums;
		}

		public void TestChangePasswordDoesNotAcceptTokenAsOldPassword()
		{
			var passwordControl = new PasswordControl();
			var token = CWSupportLoginToken.TokenForTest;
			Assert(!passwordControl.NewPasswordIsValid(GlbStaff.CurrentUser, token, "New$Passw0rd", "New$Passw0rd", out var error));
			AssertEquals("Old Password is incorrect. Passwords are case-sensitive.", error);
		}

		public void TestNewPasswordIsValid()
		{
			var passwordControl = new PasswordControl();

			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "testuser";
			staff.StaffPlainTextPassword = "pAssword_0";
			factory.Save();

			string error;
			Assert(!passwordControl.NewPasswordIsValid(staff, "XXX", "", "", out error));
			AssertEquals("Old Password is incorrect. Passwords are case-sensitive.", error);

			var masterPassword = CWSupportLoginToken.TokenForTest;

			Assert(!passwordControl.NewPasswordIsValid(staff, "pAssword_0", "", "", out error));
			AssertEquals("Please enter New Password.", error);

			Assert(!passwordControl.NewPasswordIsValid(staff, "pAssword_0", "new$passw0rd", "different", out error));
			AssertEquals("Confirm Password does not match New Password.", error);

			Assert(passwordControl.NewPasswordIsValid(staff, "pAssword_0", "New$Passw0rd", "New$Passw0rd", out error));
			AssertEquals("", error);
		}

		public void TestNewPasswordIsValid_ADEnabled()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "testuser";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.StaffPlainTextPassword = "pAssword_0";
			factory.Save();

			staff.ChangePassword("pAssword_0", "pAssword_1");
			factory.Save();

			PasswordControl passwordControl = new PasswordControl();

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

			AssertEquals("Non-AD, Old password is wrong", false, passwordControl.NewPasswordIsValid(staff, "XXX", "", "", out var error));
			AssertEquals("Old Password is incorrect. Passwords are case-sensitive.", error);

			AssertEquals("Non-AD, empty new password", false, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "", "", out error));
			AssertEquals("Please enter New Password.", error);

			AssertEquals("Non-AD, new passwords are different", false, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "new$passw0rd", "different", out error));
			AssertEquals("Confirm Password does not match New Password.", error);

			AssertEquals("Non-AD, new password has been used before", false, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "pAssword_1", "pAssword_1", out error));
			AssertEquals("You previously used this password, please choose a new password.", error);

			AssertEquals("Non-AD, change password success", true, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "New$Passw0rd", "New$Passw0rd", out error));
			AssertEquals("", error);

			// AD Enabled - should ignore local password policy and return true regardlessly
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			AssertEquals("AD, Old password is wrong - don't care", true, passwordControl.NewPasswordIsValid(staff, "XXX", "", "", out error));
			AssertEquals("", error);

			AssertEquals("AD, empty new password - don't care", true, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "", "", out error));
			AssertEquals("", error);

			AssertEquals("AD, new passwords are different - still need to check this", false, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "new$passw0rd", "different", out error));
			AssertEquals("Confirm Password does not match New Password.", error);

			AssertEquals("AD, new password has been used before - don't care", true, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "pAssword_2", "pAssword_2", out error));
			AssertEquals("", error);

			AssertEquals("AD, change password success", true, passwordControl.NewPasswordIsValid(staff, "pAssword_1", "New$Passw0rd", "New$Passw0rd", out error));
			AssertEquals("", error);
		}

		int fSavePasswordMinLength;
		int fSavePasswordMinUpperAlphas;
		int fSavePasswordMinLowerAlphas;
		int fSavePasswordMinNumeric;
		int fSavePasswordMinNonAlphNums;
	}
}
