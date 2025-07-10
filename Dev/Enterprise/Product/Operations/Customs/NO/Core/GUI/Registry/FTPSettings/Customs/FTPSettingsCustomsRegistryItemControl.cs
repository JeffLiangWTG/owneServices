using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.NO.GUI;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Registry.GUI;

partial class FTPSettingsCustomsRegistryItemControl : RegistryZUserControl
{
	public FTPSettingsCustomsRegistryItemControl()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && ViewButton is not null)
		{
			ViewButton.Click -= ViewButton_Click;
		}
		base.Dispose(disposing);
	}

	#region Viewing the Password

	void ViewButton_Click(object sender, System.EventArgs e) => ViewPassword();

	void ViewPassword()
	{
		using var loginForm = new DeveloperLoginForm();
		if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) != DialogResult.OK)
		{
			return;
		}

		if (IsValidPassword(loginForm))
		{
			Globals.Message.ShowInformation(PasswordTextBox.Text, Res.GetString("64E7AF7A-72F6-4151-93D4-2DA45B5AA192", "Password"));
			return;
		}

		loginForm.ShowIncorrectPasswordMessage();
	}

#if DEBUG
	protected virtual
#endif
		bool IsValidPassword(DeveloperLoginForm loginForm)
	{
		return loginForm.IsValidPassword;
	}

	#endregion
}
