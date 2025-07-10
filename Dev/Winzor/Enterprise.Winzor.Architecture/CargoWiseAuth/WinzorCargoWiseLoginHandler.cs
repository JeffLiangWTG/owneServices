using System;
using CargoWise.Blazor.Common;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enterprise.Winzor.Architecture;

public class WinzorCargoWiseLoginHandler : IWinzorCargoWiseLoginHandler
{
	readonly CargoWiseAuthOptions cargoWiseAuthOptions;
	readonly ICargoWiseAuthStateProvider authStateProvider;
	readonly ILogger<WinzorCargoWiseLoginHandler> logger;

	public WinzorCargoWiseLoginHandler(ICargoWiseAuthStateProvider authStateProvider, IOptions<CargoWiseAuthOptions> cargoWiseAuthOptions, ILogger<WinzorCargoWiseLoginHandler> logger)
	{
		this.authStateProvider = authStateProvider;
		this.cargoWiseAuthOptions = cargoWiseAuthOptions.Value;
		this.logger = logger;
	}

	public bool Login()
	{
		return Login(authStateProvider, cargoWiseAuthOptions, logger);
	}

	internal static bool Login(
		ICargoWiseAuthStateProvider authStateProvider,
		CargoWiseAuthOptions cargoWiseAuthOptions,
		ILogger<WinzorCargoWiseLoginHandler> logger)
	{
		try
		{
			Initialization.SplashFormInstance.UpdateText(MSG_LoginStarted);
			var loginResult = authStateProvider.Login(authStateProvider, cargoWiseAuthOptions);

			if (!loginResult.IsSuccess)
			{
				Initialization.SplashFormInstance.UpdateText(MSG_LoginFailed);
			}
			else
			{
				Initialization.SplashFormInstance.UpdateText(MSG_LoginSuccess);
			}
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			logger?.LogError(ex, (NoResString)"Error login.");
			Initialization.SplashFormInstance.UpdateText(MSG_LoginError_Unexpected);
			ShowLoginFailedMessage(CargoWiseAuthResult.Failed(MSG_LoginError_Unexpected));
			return false;
		}

		return true;
	}

	static void ShowLoginFailedMessage(CargoWiseAuthResult failedResult)
	{
		if (failedResult.IsSuccess)
		{
			throw new ArgumentOutOfRangeException(nameof(failedResult));
		}

		if (!failedResult.CanRetryLogin)
		{
			Globals.Message.Show($"{failedResult.FailedMessage}", MSG_LoginFailed, ZMessageBoxButtons.OK, ZDialogResult.None);
			return;
		}

		var dialogResult = Globals.Message.Show(message: $"{failedResult.FailedMessage}{System.Environment.NewLine}{MSG_RetryLogin_Question}",
												caption: MSG_LoginFailed,
												buttons: ZMessageBoxButtons.OKCancel,
												defaultResult: ZDialogResult.None);

		if (dialogResult == ZDialogResult.OK)
		{
			var clientServices = Initialization.SplashFormInstance.CargoWiseClientServices;
			Initialization.SplashFormInstance.EndInvoke(clientServices.LifecycleService.StartNewApplicationAsync(new Uri($"{clientServices.ServerBaseUri}?loginPrompt=SelectAccount")));
		}
	}

	static string MSG_LoginStarted => Res.GetString("26ef9eec-6229-4eca-8258-4cc7228efda0", "Login started");
	static string MSG_LoginSuccess => Res.GetString("ba318c57-e8f5-4298-8807-5965186f8737", "Login success");
	static string MSG_LoginFailed => Res.GetString("c09c8206-9a68-4b6e-ba60-914cba43d04b", "Login failed");
	static string MSG_LoginError_Unexpected => Res.GetString("d75b3848-7cc7-458d-b201-72192f2bc390", "Unexpected error encountered during login attempt.");
	static string MSG_RetryLogin_Question => Res.GetString("8221ca0a-8bf0-468d-8f9a-b12787bef97c", "Would you like to restart and try to login again?");
}
