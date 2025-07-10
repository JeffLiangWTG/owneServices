using System;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Definitions;
using CargoWiseNext.Infrastructure.Installations;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.JSInterop;

namespace Enterprise.Winzor.Architecture;
public class WinzorStartupOpenMainFormTask : AbstractApplicationStartupTask
{
	public override string TaskDescription => Res.GetString("f14fd383-d425-4d61-8d2d-25ce7fd75370", "Opening Main Form");

	public override int FailureExitCode => ExitCodes.WinzorStartupOpenMainFormTaskError;

	readonly IUrlHandlerProvider urlHandlerProvider = new UrlHandlerFromInstallerFilesHandler();

	protected override bool DoExecute(CommandLineArguments arguments)
	{
		if (LoginDirector.Instance.AuthenticatedUser.IsOK)
		{
			StartupOpenMainFormTask.MainFormInstance = new MainForm();

			StartupOpenMainFormTask.MainFormInstance.Shown += (s, e) =>
			{
				StartupOpenMainFormTask.MainFormInstance.Invoke(async () =>
				{
					var jsRuntime = StartupOpenMainFormTask.MainFormInstance.CargoWiseClientServices?.JSRuntime;
					if (jsRuntime != null)
					{
						await jsRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageItemKeys.LaunchProtocol, urlHandlerProvider.GetUrlHandler());
					}
				});
			};

			if (LoginDirector.Instance.LoggedInLocation)
			{
				MainForm.InitializeAfterLogin();
			}
			else
			{
				StartupOpenMainFormTask.MainFormInstance.ShowLoginLocationControl();
			}
			return true;
		}
		else
		{
			var dialogResult = Globals.Message.Show(message: Res.GetString("8221ca0a-8bf0-468d-8f9a-b12787bef97c", "Would you like to restart and try to login again?"),
												caption: Res.GetString("c09c8206-9a68-4b6e-ba60-914cba43d04b", "Login failed"),
												buttons: ZMessageBoxButtons.OKCancel,
												defaultResult: ZDialogResult.None);

			if (dialogResult == ZDialogResult.OK)
			{
				var clientServices = Initialization.SplashFormInstance.CargoWiseClientServices;
				Initialization.SplashFormInstance.EndInvoke(clientServices.LifecycleService.StartNewApplicationAsync(new Uri($"{clientServices.ServerBaseUri}?loginPrompt=SelectAccount")));
			}
			return false;
		}
	}
}
