using Enterprise.Security.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class LoginForm_Test : TransactionedTestCase
	{
		public void TestInvalidLogin()
		{
			using (LoginForm testForm = new LoginForm())
			{
				testForm.Show();

				testForm.LoginTextBox.Text = "username";
				testForm.PasswordTextBox.Text = "password";
				testForm.OKButton.PerformClick();

				AssertNull(testForm.Credentials.UserSecurity);
			}
		}

		[RequiresSTA]
		public void TestValidLogin()
		{
			SecurityTestObject.CreateTestUser(true, "", "tst", "username", "password");

			using (LoginForm testForm = new LoginForm())
			{
				testForm.Show();

				testForm.LoginTextBox.Text = "username";
				testForm.PasswordTextBox.Text = "password";
				testForm.OKButton.PerformClick();

				AssertNotNull(testForm.Credentials.UserSecurity);
			}
		}

		[RequiresSTA]
		public void TestSecurityMessage()
		{
			using (LoginForm testForm = new LoginForm())
			{
				testForm.Message = "test message";
				AssertEquals("test message", testForm.Message);
			}
		}

		[RequiresSTA]
		public void TestPasswordTextBoxCharacterCasing()
		{
			using (LoginForm testForm = new LoginForm())
			{
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, testForm.PasswordTextBox.CharacterCasing);
			}
		}
	}
}
