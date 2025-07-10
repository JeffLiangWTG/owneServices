using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;
public class PasswordViewerLayoutExtension : ILayoutExtension
{
	IPasswordControlHost passwordControlHost;
	void ILayoutExtension.Initialize(IControlHost host)
	{
		passwordControlHost = (IPasswordControlHost)host;
		RegisterEventsForPasswordControls();
	}

	void ILayoutExtension.Cleanup()
	{
		if (passwordControlHost?.ViewButton != null)
		{
			passwordControlHost.ViewButton.Click -= ViewButton_Click;
		}
	}

	void RegisterEventsForPasswordControls()
	{
		if (passwordControlHost != null)
		{
			ZButton viewButton = passwordControlHost.ViewButton;
			if (viewButton != null)
			{
				viewButton.Click += ViewButton_Click;
			}
		}
	}

	void ViewButton_Click(object sender, EventArgs e) => ViewPassword();

	void ViewPassword()
	{
		using var loginForm = new DeveloperLoginForm();
		if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) != DialogResult.OK)
		{
			return;
		}

		if (loginForm.IsValidPassword)
		{
			Globals.Message.ShowInformation(passwordControlHost.PasswordTextBox.Text, Res.GetString("9A010911-A45A-431D-B09C-0E9340AA5847", "Password"));
			return;
		}

		loginForm.ShowIncorrectPasswordMessage();
	}
}
