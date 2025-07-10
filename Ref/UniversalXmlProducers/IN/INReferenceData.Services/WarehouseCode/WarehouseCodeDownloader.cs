using System;
using System.IO;
using System.Threading;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.INReferenceData.Services;

public class WarehouseCodeDownloader
{
	public string DownloadData()
	{
		var retryCount = 0;
		while (true)
		{
			try
			{
				return DownloadDataCore();
			}
			catch (Exception ex)
			{
				if (ex is UnhandledApplicationException)
				{
					Logger.Log(LogType.Warning, ex.Message);

				}
				else
				{
					Logger.Log(LogType.Warning, $"Failed to load warehouse codes from the source at {retryCount} retry", ex.Message);
				}
				if (++retryCount >= AppConfig.WarehouseCode.MaxRetry)
				{
					var message = $"Failed to load warehouse codes from the source after {retryCount} retries";
					Logger.Log(LogType.ReviewRequired, message, ex);
					throw new UnhandledApplicationException(message);
				}

				Thread.Sleep(SleepInterval);
			}
		}
	}

	string DownloadDataCore()
	{
		using var client = GetHttpClient();
		var captcha = HandleCaptcha(client);

		var codeUrl = $"{AppConfig.WarehouseCode.CodeUrl}?captchaValue={captcha}";
		var codesResponse = client.Get(new Uri(codeUrl));
		Helper.Assume(codesResponse.IsSuccessStatusCode, $"Failed to load warehouse codes from the source, State Code:{codesResponse.StatusCode}");

		var responseContent = codesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
		Helper.Assume(!responseContent.IsNullOrEmpty(), "Failed to load warehouse codes from the source, Response is empty");
		Helper.Assume(!responseContent.Contains("Invalid Captcha!"), "Failed to load warehouse codes from the source, Wrong Captcha");

		return responseContent;
	}

	protected virtual string HandleCaptcha(IHttpClient client)
	{
		var captchaUrl = AppConfig.WarehouseCode.CaptchaUrl;
		var captchaResponse = client.Get(new Uri(captchaUrl));
		Helper.Assume(captchaResponse.IsSuccessStatusCode, $"Failed to load warehouse codes from the source, State Code:{captchaResponse.StatusCode}");

		var captchaImg = captchaResponse.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
		Helper.Assume(captchaImg is { Length: > 0 }, "Failed to load warehouse codes from the source, Captcha is empty");

		var captcha = CaptchaSolver.SolveCaptcha(captchaImg);
		Helper.Assume(!captcha.IsNullOrEmpty(), "Failed to solve Captcha, which is empty");
		Logger.Log(LogType.Info, "Captcha solved", captcha);
		return captcha;
	}

	Logger Logger => logger ??= new Logger(new DateTimeProvider());
	Logger logger;

	protected virtual IHttpClient GetHttpClient() => new HttpClientWithCookieSupport();
	protected virtual int SleepInterval => AppConfig.WarehouseCode.SleepInterval;
}
