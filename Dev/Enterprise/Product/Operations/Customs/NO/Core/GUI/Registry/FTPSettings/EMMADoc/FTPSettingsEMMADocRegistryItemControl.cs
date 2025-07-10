using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.NO.GUI;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Registry.GUI;

partial class FTPSettingsEMMADocRegistryItemControl : RegistryZUserControl
{
	public FTPSettingsEMMADocRegistryItemControl()
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
			Globals.Message.ShowInformation(PasswordTextBox.Text, Res.GetString("9A010911-A45A-431D-B09C-0E9340AA5847", "Password"));
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
