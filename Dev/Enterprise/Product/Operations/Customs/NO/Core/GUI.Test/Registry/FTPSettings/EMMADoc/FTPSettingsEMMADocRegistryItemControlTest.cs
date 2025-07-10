using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.GUI.Testing;

[TestedType(typeof(FTPSettingsEMMADocRegistryItemControl))]
sealed class FTPSettingsEMMADocRegistryItemControlTest : RegistryZUserControlTestCase
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new FTPSettingsEMMADocRegistryItemControl();
		_ = userControl.AssertThisControl(x => x
			.WithCaptionRenderingEnabled());

		_ = userControl.AssertContainsControl<ZGroupBox>(nameof(userControl.FtpSettingsEMMADocGroupBox), x => x
			.WithCaption("FTP Server settings"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.UserNameTextBox), x => x
			.WithBindTo(nameof(FTPSettingsRegistry.Username))
			.WithCaption("Username"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.PasswordTextBox), x => x
			.WithBindTo(nameof(FTPSettingsRegistry.Password))
			.WithCharacterCasing(System.Windows.Forms.CharacterCasing.Normal)
			.WithPasswordChar('*')
			.WithCaption("Password"));

		_ = userControl.AssertContainsControl<ZButton>(nameof(userControl.ViewButton), x => x
			.WithCaption("View"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.UrlAddressTextBox), x => x
			.WithBindTo(nameof(FTPSettingsRegistry.Url))
			.WithCaption("URL"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.PortTextBox), x => x
			.WithBindTo(nameof(FTPSettingsRegistry.Port))
			.WithCaption("Port"));
	});

	public void TestViewPassword()
	{
		using var control = new FTPSettingsEMMADocRegistryItemControlForTest();
		control.Show();
		control.PasswordTextBox.Text = "ThisIsThePassword";

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		control.ViewButton.PerformClick();
		AssertNull("no message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		control.LoginPassword = "ThisIsTheWrongPwd";
		control.ViewButton.PerformClick();
		AssertEquals("Incorrect password, an error message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		control.LoginPassword = User.MasterPassword;
		control.ViewButton.PerformClick();
		AssertEquals("Correct password, the password should be displayed.", "ThisIsThePassword", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	protected override IBusiness GetNewBusinessEntity() => new FTPSettingsRegistry();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		=> control.ReadOnly || businessEntity.IsReadOnly;

	sealed class FTPSettingsEMMADocRegistryItemControlForTest : FTPSettingsEMMADocRegistryItemControl
	{
		protected override bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			((ZTextBox)typeof(DeveloperLoginForm).GetField("PasswordTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loginForm)).Text = LoginPassword;
			return base.IsValidPassword(loginForm);
		}

		public string LoginPassword;
	}
}

