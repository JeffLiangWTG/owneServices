using System;
using System.Configuration;
using CargoWise.RefDbRepo.PLReferenceData.Services;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Services.Taric4;

[TestFixture]
sealed class Taric4FileDownloaderConfigTest
{
	[Test]
	public void TestLoadIntervalInSeconds() => Assert.That(testDownloaderConfig.LoadIntervalInSeconds, Is.EqualTo(15));

	[Test]
	public void TestWaitIntervalInSeconds() => Assert.That(testDownloaderConfig.WaitIntervalInSeconds, Is.EqualTo(60));

	[Test]
	public void TestTaric4Url()
	{
		settingsIndexerMock.Setup(x => x["Taric4Url"]).Returns("http://example.com/taric4.html");
		Assert.That(testDownloaderConfig.Taric4Url, Is.EqualTo("http://example.com/taric4.html"));
	}

	[Test]
	public void TestTaric4CheckboxId()
	{
		settingsIndexerMock.Setup(x => x["Taric4CheckboxId"]).Returns("TestCheckboxId");
		Assert.That(testDownloaderConfig.Taric4CheckboxId, Is.EqualTo("TestCheckboxId"));
	}

	[Test]
	public void TestTaric4DownloadButtonId()
	{
		settingsIndexerMock.Setup(x => x["Taric4DownloadButtonId"]).Returns("TestDownloadButtonId");
		Assert.That(testDownloaderConfig.Taric4DownloadButtonId, Is.EqualTo("TestDownloadButtonId"));
	}

	[Test]
	public void TestFtpSeleniumAddress()
	{
		settingsIndexerMock.Setup(x => x["FtpSeleniumAddress"]).Returns("ftp://example.com:21");
		Assert.That(testDownloaderConfig.FtpSeleniumAddress, Is.EqualTo("ftp://example.com:21"));
	}

	[Test]
	public void TestFtpSeleniumDirecotry() => Assert.Multiple(() =>
	{
		settingsIndexerMock.Setup(x => x["FtpSeleniumAddress"]).Returns("ftp://example.com:21");
		settingsIndexerMock.Setup(x => x["FtpSeleniumDownloadsPLAddress"]).Returns("ftp://example.com:21/dir/");
		Assert.That(testDownloaderConfig.FtpSeleniumDirectory, Is.EqualTo("dir"), "Starts with the value specified for server address.");

		settingsIndexerMock.Setup(x => x["FtpSeleniumDownloadsPLAddress"]).Returns("ftp://example.com:21/dir/nested_dir/");
		Assert.That(testDownloaderConfig.FtpSeleniumDirectory, Is.EqualTo("dir/nested_dir"), "Directory includes nested level.");

		settingsIndexerMock.Setup(x => x["FtpSeleniumAddress"]).Returns("ftp://example.com:23");
		var error = Assert.Throws<ConfigurationErrorsException>(() => { _ = testDownloaderConfig.FtpSeleniumDirectory; }, "Doesn't start with the value specified for server address.");
		Assert.That(error.Message, Is.EqualTo("Invalid FtpSeleniumDownloadsPLAddress value. It must start with the value specified for FtpSeleniumAddress."), "Error message.");
	});

	[Test]
	public void TestSeleniumTariffDownloadFullPath() => Assert.Multiple(() =>
	{
		settingsIndexerMock.Setup(x => x["FtpSeleniumAddress"]).Returns("ftp://example.com");
		settingsIndexerMock.Setup(x => x["FtpSeleniumDownloadsPLAddress"]).Returns("ftp://example.com/dirPL");
		settingsIndexerMock.Setup(x => x["SeleniumTariffDownloadFullPath"]).Returns("C:\\downloads\\dirPL\\");
		Assert.That(testDownloaderConfig.SeleniumTariffDownloadFullPath, Is.EqualTo("C:\\downloads\\dirPL\\"), "Ends with the common parts of SeleniumDownloadsPLAddress.");

		settingsIndexerMock.Setup(x => x["SeleniumTariffDownloadFullPath"]).Returns("C:\\downloads\\dirPL");
		Assert.That(testDownloaderConfig.SeleniumTariffDownloadFullPath, Is.EqualTo("C:\\downloads\\dirPL"), "Ends with the common parts of SeleniumDownloadsPLAddress (without trailing slash).");

		settingsIndexerMock.Setup(x => x["SeleniumTariffDownloadFullPath"]).Returns("C:\\downloads\\dirEU\\");
		var error = Assert.Throws<ConfigurationErrorsException>(() => { _ = testDownloaderConfig.SeleniumTariffDownloadFullPath; }, "Doesn't end with the common parts of SeleniumDownloadsPLAddress.");
		Assert.That(error.Message, Is.EqualTo("Invalid SeleniumTariffDownloadFullPath value. It must end with the common parts of SeleniumDownloadsPLAddress."), "Error message.");
	});

	[SetUp]
	public void SetUp()
	{
		settingsIndexerMock = new Mock<ISettingsIndexer>();
		testDownloaderConfig = new Taric4FileDownloaderConfig(settingsIndexerMock.Object);
	}

	Mock<ISettingsIndexer> settingsIndexerMock;
	Taric4FileDownloaderConfig testDownloaderConfig;
}
