using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.GUI;
using Enterprise.Customs.NO.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Testing;

[TestedType(typeof(FTPSettingsUserControl))]
sealed class FTPSettingsUserControlTest : RegistryZUserControlTestCase
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new FTPSettingsUserControl();
		_ = userControl.AssertThisControl(x => x
			.WithCaptionRenderingEnabled());

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.UserNameTextBox), x => x
			.WithBindTo(nameof(FTPSettingsCustomsRegistry.Username))
			.WithCaption("Username"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.PasswordTextBox), x => x
			.WithBindTo(nameof(FTPSettingsCustomsRegistry.Password))
			.WithCharacterCasing(System.Windows.Forms.CharacterCasing.Normal)
			.WithPasswordChar('*')
			.WithCaption("Password"));

		_ = userControl.AssertContainsControl<ZButton>(nameof(userControl.ViewButton), x => x
			.WithCaption("View"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.UrlAddressTextBox), x => x
			.WithBindTo(nameof(FTPSettingsCustomsRegistry.Url))
			.WithCaption("URL"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.PortTextBox), x => x
			.WithBindTo(nameof(FTPSettingsCustomsRegistry.Port))
			.WithCaption("Port"));
	});

	public void TestViewPassword() => CombineAssertions(() =>
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
	});

	protected override IBusiness GetNewBusinessEntity() => new FTPSettingsCustomsRegistry();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		=> control.ReadOnly || businessEntity.IsReadOnly;

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
