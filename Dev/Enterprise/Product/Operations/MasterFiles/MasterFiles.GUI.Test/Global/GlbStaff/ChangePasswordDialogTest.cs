using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ChangePasswordDialog))]
	sealed class ChangePasswordDialogTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestChangePasswordDialog()
		{
			using (ChangePasswordDialog dialog = new ChangePasswordDialog(GlbStaff.CurrentUser, true))
			{
				dialog.Show();
				dialog.OKButton.PerformClick();
				Assert("Old Password incorrect message",
					dialog.ErrorLabel.Text.IndexOf("Old Password is incorrect") >= 0);

				dialog.OldPasswordTextBox.Text = User.MasterPassword;
				dialog.NewPasswordTextBox.Text = "NewPassword_1";
				dialog.ConfirmPasswordTextBox.Text = "wrongpassword";
				dialog.OKButton.PerformClick();
				Assert("Confirm password mistmatch",
					dialog.ErrorLabel.Text.IndexOf("Confirm Password does not match") >= 0);
			}
		}

		public void TestChangePasswordDialog_HandleADNotLinkedOrObjectNotExists()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = "something";
			Factory.Save();
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			using (var dialog = new ChangePasswordDialog(staff, false))
			{
				dialog.Show();
				dialog.NewPasswordTextBox.Text = "nnn";
				dialog.ConfirmPasswordTextBox.Text = "nnn";
				dialog.OKButton.PerformClick();
				AssertEquals("The staff has not been synchronized to Active Directory yet, please try again later.",
					dialog.ErrorLabel.Text);
			}

			staff.GS_LoginName = "coffeepot";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			using (var dialog = new ChangePasswordDialog(staff, false))
			{
				dialog.Show();
				dialog.NewPasswordTextBox.Text = "nnn";
				dialog.ConfirmPasswordTextBox.Text = "nnn";
				dialog.OKButton.PerformClick();
				AssertEquals(
					$"Active Directory User '{staff.GS_LoginName}' is missing, please contact your system administrator.",
					dialog.ErrorLabel.Text);
			}
		}

		public void TestChangePasswordHistoryValidation()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 2;
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.StaffPlainTextPassword = "pAssword_0";
			Factory.Save();
			using (ChangePasswordDialog dialog = new ChangePasswordDialog(user, true))
			{
				dialog.Show();
				dialog.OldPasswordTextBox.Text = "pAssword_0";
				dialog.NewPasswordTextBox.Text = "one";
				dialog.ConfirmPasswordTextBox.Text = "one";
				dialog.OKButton.PerformClick();
				AssertEquals("Password change error: " + dialog.ErrorLabel.Text, DialogResult.OK, dialog.DialogResult);
				user.Factory.Save();
			}

			using (ChangePasswordDialog dialog = new ChangePasswordDialog(user, true))
			{
				dialog.Show();
				dialog.OldPasswordTextBox.Text = "one";
				dialog.NewPasswordTextBox.Text = "one";
				dialog.ConfirmPasswordTextBox.Text = "one";
				dialog.OKButton.PerformClick();
				Assert("Should display password previously used error",
					dialog.ErrorLabel.Text.IndexOf(
						"You previously used this password, please choose a new password.") >= 0);
				dialog.NewPasswordTextBox.Text = "two";
				dialog.ConfirmPasswordTextBox.Text = "two";
				dialog.OKButton.PerformClick();
				AssertEquals("Password change error: " + dialog.ErrorLabel.Text, DialogResult.OK, dialog.DialogResult);
				user.Factory.Save();
			}

			using (ChangePasswordDialog dialog = new ChangePasswordDialog(user, true))
			{
				dialog.Show();
				dialog.OldPasswordTextBox.Text = "two";
				dialog.NewPasswordTextBox.Text = "one";
				dialog.ConfirmPasswordTextBox.Text = "one";
				dialog.OKButton.PerformClick();
				Assert("Should display password previously used error",
					dialog.ErrorLabel.Text.IndexOf(
						"You previously used this password, please choose a new password.") >= 0);
				EnvProxy.Instance.Registry.PasswordHistoryCount = 1;
				dialog.OKButton.PerformClick();
				AssertEquals("Password change error: " + dialog.ErrorLabel.Text, DialogResult.OK, dialog.DialogResult);
				user.Factory.Save();
			}
		}

		public void TestChangePasswordDialog_ResetPassword()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Test User";
			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();
			using (ChangePasswordDialog dialog = new ChangePasswordDialog(staff, false))
			{
				dialog.Show();
				dialog.NewPasswordTextBox.Text = "NewPassword_1";
				dialog.ConfirmPasswordTextBox.Text = "NewPassword_1";
				dialog.OKButton.PerformClick();
				AssertEquals("Password change error: " + dialog.ErrorLabel.Text, dialog.DialogResult, DialogResult.OK);
				staff.Factory.Save();
			}

			AssertEquals(true, staff.VerifyPassword("NewPassword_1"));
			AssertEquals(false, GlbStaff.CurrentUser.VerifyPassword("NewPassword_1"));
			AssertEquals("Reset password always requires ChangePasswordAtNextLogin after password reset", true,
				staff.GS_ChangePasswordAtNextLogin);
		}

		public void TestChangePasswordDialog_ChangePassword()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Test User";
			staff.StaffPlainTextPassword = "OldPassword";
			staff.GS_ChangePasswordAtNextLogin = true;
			Factory.Save();
			using (ChangePasswordDialog dialog = new ChangePasswordDialog(staff, true))
			{
				dialog.Show();
				dialog.OldPasswordTextBox.Text = "OldPassword";
				dialog.NewPasswordTextBox.Text = "NewPassword_1";
				dialog.ConfirmPasswordTextBox.Text = "NewPassword_1";
				dialog.OKButton.PerformClick();
				AssertEquals("Password change error: " + dialog.ErrorLabel.Text, dialog.DialogResult, DialogResult.OK);
				staff.Factory.Save();
			}

			AssertEquals(true, staff.VerifyPassword("NewPassword_1"));
			AssertEquals(false, GlbStaff.CurrentUser.VerifyPassword("NewPassword_1"));
			AssertEquals("Change password does not required ChangePasswordAtNextLogin", false,
				staff.GS_ChangePasswordAtNextLogin);
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ChangePasswordDialog(Factory.New<GlbStaff>(), true);
			MissingResourceStringChecker.ExcludeFromTest(form.ErrorLabel);
			return form;
		}
	}
}
