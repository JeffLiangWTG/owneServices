namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

interface ITaric4FileDownloaderConfig
{
	string Taric4Url { get; }
	string Taric4CheckboxId { get; }
	string Taric4DownloadButtonId { get; }
	string SeleniumTariffDownloadFullPath { get; }
	string FtpSeleniumAddress { get; }
	string FtpSeleniumDirectory { get; }
	int LoadIntervalInSeconds { get; }
	int WaitIntervalInSeconds { get; }
}
