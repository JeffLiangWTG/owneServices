using System;
using System.Configuration;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;

sealed class Taric4FileDownloaderConfig(ISettingsIndexer settings = null) : ITaric4FileDownloaderConfig
{
	readonly ISettingsIndexer appSettings = settings;

	public string Taric4Url => AppConfigHelper.GetAppSettingsValue("Taric4Url", appSettings);

	public string Taric4CheckboxId => AppConfigHelper.GetAppSettingsValue("Taric4CheckboxId", appSettings);

	public string Taric4DownloadButtonId => AppConfigHelper.GetAppSettingsValue("Taric4DownloadButtonId", appSettings);

	public string FtpSeleniumAddress => AppConfigHelper.GetAppSettingsValue("FtpSeleniumAddress", appSettings);

	public string FtpSeleniumDirectory => ValidateAndGetFtpDownloadDirectory(FtpSeleniumAddress, AppConfigHelper.GetAppSettingsValue("FtpSeleniumDownloadsPLAddress", appSettings));

	public string SeleniumTariffDownloadFullPath => ValidateAndGetSeleniumTariffDownloadFullPath(FtpSeleniumDirectory, AppConfigHelper.GetAppSettingsValue("SeleniumTariffDownloadFullPath", appSettings));

	public int LoadIntervalInSeconds => 15;

	public int WaitIntervalInSeconds => 60;

	static string ValidateAndGetFtpDownloadDirectory(string ftpAddress, string ftpDownloadAddress) =>
		ftpDownloadAddress.StartsWith(ftpAddress, StringComparison.Ordinal)
			? ftpDownloadAddress.Substring(ftpAddress.Length).Trim('/')
			: throw new ConfigurationErrorsException("Invalid FtpSeleniumDownloadsPLAddress value. It must start with the value specified for FtpSeleniumAddress.");

	static string ValidateAndGetSeleniumTariffDownloadFullPath(string ftpDirectory, string downloadFullPath) =>
		NormalizePath(downloadFullPath).EndsWith(NormalizePath(ftpDirectory), StringComparison.Ordinal)
			? downloadFullPath
			: throw new ConfigurationErrorsException("Invalid SeleniumTariffDownloadFullPath value. It must end with the common parts of SeleniumDownloadsPLAddress.");

	static string NormalizePath(string path) => path.Replace("/", "\\") + (path.EndsWith("\\", StringComparison.OrdinalIgnoreCase) ? "" : "\\");
}
