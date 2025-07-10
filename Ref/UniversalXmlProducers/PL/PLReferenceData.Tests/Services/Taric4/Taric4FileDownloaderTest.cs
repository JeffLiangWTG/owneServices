using System;
using System.IO;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Services.Taric4;

[TestFixture]
sealed class Taric4FileDownloaderTest
{
	[Test]
	public void TestDownloadTaric4BaseFile()
	{
		const string taric4TestUrl = "http://test.url";
		const int testLoadInterval = 1;
		const string taric4CheckboxId = "taric4CheckboxId";
		const string taric4DownloadButtonId = "taric4DownloadButtonId";
		const string testFileName = "testFile";
		const string testFtpDirectory = "testDirectory";
		const string testDownloadFullPath = "testDownloadFullPath"+"//"+testFtpDirectory;


		var downloaderConfigMock = Mock.Of<ITaric4FileDownloaderConfig>(c =>
			c.Taric4Url == taric4TestUrl &&
			c.Taric4CheckboxId == taric4CheckboxId &&
			c.Taric4DownloadButtonId == taric4DownloadButtonId &&
			c.LoadIntervalInSeconds == testLoadInterval &&
			c.FtpSeleniumDirectory == testFtpDirectory &&
			c.SeleniumTariffDownloadFullPath == testDownloadFullPath);

		var testDestinationDir = Path.Combine(Path.GetTempPath(), "testLocalDirectory");
		CleanUpTestDirectory(testDestinationDir);

		var webDriverHelperMock = new Mock<IWebDriverHelper>() { CallBase = true };
		var webDriverHelperFactoryMock = new Mock<IWebDriverHelperFactory>();
		webDriverHelperFactoryMock.Setup(x => x.Create(testDownloadFullPath)).Returns(webDriverHelperMock.Object);

		var ftpClientMock = new Mock<IFtpClient>() { CallBase = true };
		ftpClientMock.SetupSequence(f => f.GetDirectoryContent(It.IsAny<string>()))
			.Returns([])
			.Returns([$"{testFileName}.tmp"])
			.Returns([$"{testFileName}.zip"]);

		var testFileDownloader = new Taric4FileDownloader(downloaderConfigMock, ftpClientMock.Object, webDriverHelperFactoryMock.Object);
		testFileDownloader.DownloadTaric4BaseFile(testDestinationDir);

		Assert.That(Directory.Exists(testDestinationDir), Is.True, "Local directory created.");
		ftpClientMock.Verify(f => f.GetDirectoryContent(null), Times.Once, "FTP server validated for directory");
		ftpClientMock.Verify(f => f.CreateDirectory(testFtpDirectory), Times.Once, "FTP directory created.");
		webDriverHelperMock.Verify(f => f.GetWebPage(taric4TestUrl, testLoadInterval), Times.Once, "Web page opened.");
		webDriverHelperMock.Verify(f => f.GetWebPageByLinkId(taric4CheckboxId), Times.Once, "Checkbox clicked.");
		webDriverHelperMock.Verify(f => f.GetWebPageByLinkId(taric4DownloadButtonId), Times.Once, "Download button clicked.");
		ftpClientMock.Verify(f => f.GetDirectoryContent(testFtpDirectory), Times.Exactly(2), "FTP directory validated for .tmp then for .zip");
		ftpClientMock.Verify(f => f.DownloadFile($"{testFileName}.zip", testFtpDirectory, testDestinationDir), Times.Once, "FTP file downloaded.");
		ftpClientMock.Verify(f => f.DeleteFile($"{testFileName}.zip", testFtpDirectory), Times.Once, "FTP file deleted.");

		CleanUpTestDirectory(testDestinationDir);
	}

	[Test]
	public void TestDownloadTaric4BaseFile_FtpDirectoryContainsZipFiles()
	{
		const string taric4TestUrl = "http://test.url";
		const string testFileName = "testFile";
		const string testFtpDirectory = "testDirectory";

		var downloaderConfigMock = Mock.Of<ITaric4FileDownloaderConfig>(c =>
			c.Taric4Url == taric4TestUrl &&
			c.FtpSeleniumDirectory == testFtpDirectory);

		var testDestinationDir = Path.Combine(Path.GetTempPath(), "testLocalDirectory");
		CleanUpTestDirectory(testDestinationDir);

		var ftpClientMock = new Mock<IFtpClient>() { CallBase = true };
		ftpClientMock.SetupSequence(f => f.GetDirectoryContent(It.IsAny<string>()))
			.Returns([testFtpDirectory])
			.Returns([$"{testFileName}.zip"])
			.Returns([$"{testFileName}.zip"]);

		var testFileDownloader = new Taric4FileDownloader(downloaderConfigMock, ftpClientMock.Object);
		Assert.Throws<InvalidOperationException>(() =>
			{ testFileDownloader.DownloadTaric4BaseFile(testDestinationDir); },
			"There are zip files in the FTP directory testDirectory. Please remove them manually.");

		ftpClientMock.Verify(f => f.GetDirectoryContent(null), Times.Once, "FTP server validated for directory exists.");
		ftpClientMock.Verify(f => f.GetDirectoryContent(testFtpDirectory), Times.Exactly(2), "FTP server validated for directory content.");
		ftpClientMock.Verify(f => f.DeleteFile($"{testFileName}.zip", testFtpDirectory), Times.Once, "The attempt to delete zip file from FTP.");
	}

	static void CleanUpTestDirectory(string testDestinationDir)
	{
		if (Directory.Exists(testDestinationDir))
		{
			Directory.Delete(testDestinationDir, true);
		}
	}
}
