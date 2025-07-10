using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(PasswordViewerLayoutExtension))]
sealed class PasswordViewerLayoutExtensionTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestPasswordViewerLayoutExtensionRegisterAndCleanup()
	{
		var layoutProvider = new FTPSettingsCustomsLayout();
	}

	public void TestViewPassword()
	{
		using var control = new FTPSettingsUserControlForTest();
		control.Show();
		control.PasswordTextBox.Text = "ThisIsThePassword";

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		control.ViewButton.PerformClick();
		AssertNull("No message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		control.LoginPassword = "ThisIsTheWrongPwd";
		control.ViewButton.PerformClick();
		AssertEquals("An error message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		control.LoginPassword = User.MasterPassword;
		control.ViewButton.PerformClick();
		AssertEquals("The password should be displayed.", "ThisIsThePassword", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	sealed class FTPSettingsUserControlForTest : FTPSettingsUserControl
	{
		protected override bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			((ZTextBox)typeof(DeveloperLoginForm).GetField("PasswordTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loginForm)).Text = LoginPassword;
			return base.IsValidPassword(loginForm);
		}

		public string LoginPassword;
	}
}
