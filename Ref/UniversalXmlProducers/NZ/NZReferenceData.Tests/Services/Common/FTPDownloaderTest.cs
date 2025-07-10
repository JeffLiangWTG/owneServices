using System;
using System.IO;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using FluentFTP;
using FluentFTP.Exceptions;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	sealed class FTPDownloaderTest
	{
		[Test]
		public void TestDownload()
		{
			var ftpClientMock = new Mock<IFtpClient>();
			ftpClientMock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FtpLocalExists>(), It.IsAny<FtpVerify>(), It.IsAny<Action<FtpProgress>>()))
				.Returns(FtpStatus.Failed);
			Exception throwedException = null;
			try
			{
				FTPDownloader.Download(ftpClientMock.Object, "test.txt", "stest.txt");
			}
			catch (Exception ex)
			{
				throwedException = ex;
			}
			Assert.That(throwedException.GetType(), Is.EqualTo(typeof(FtpException)));
			Assert.That(throwedException.Message, Is.EqualTo("Unable to download file stest.txt to test.txt, status: Failed"));
		}

		[Test]
		public void TestDownloadWithFallback()
		{
			const string fileName = "test.txt";
			const string remotePath = "NZOnFtp";
			var testBasePath = Path.Combine(Path.GetTempPath(), "NZFTPDownloaderTest" + Guid.NewGuid().ToString());
			var localPath = Path.Combine(testBasePath, "Local");
			var localFilePath = Path.Combine(localPath, fileName);
			var portalPath = Path.Combine(testBasePath, "Portal");
			var portalFilePath = Path.Combine(portalPath, fileName);

			try
			{
				Directory.CreateDirectory(testBasePath);
				Directory.CreateDirectory(localPath);
				Directory.CreateDirectory(portalPath);

				File.WriteAllText(portalFilePath, "portal");

				var ftpClientMock = new Mock<IFtpClient>();
				ftpClientMock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FtpLocalExists>(), It.IsAny<FtpVerify>(), It.IsAny<Action<FtpProgress>>()))
					.Callback((string localPath, string remotePath, FtpLocalExists existsMode, FtpVerify verify, Action<FtpProgress> progress) =>
					{
						File.WriteAllText(localPath, "123456");
					})
					.Returns(FtpStatus.Success);
				var filePath = FTPDownloader.DownloadWithFallback(ftpClientMock.Object, fileName, localPath, remotePath, portalPath, (Exception ex) => { });
				StringAssert.AreEqualIgnoringCase(localFilePath, filePath, "Download from FTP succeed");
				Assert.That(File.Exists(localFilePath));

				ftpClientMock = new Mock<IFtpClient>();
				ftpClientMock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FtpLocalExists>(), It.IsAny<FtpVerify>(), It.IsAny<Action<FtpProgress>>()))
					.Throws(new Exception("Some Exception"));
				var utcTimeNow = DateTime.UtcNow;
				File.SetLastWriteTimeUtc(portalFilePath, utcTimeNow.AddMinutes(-1));
				File.SetLastWriteTimeUtc(localFilePath, utcTimeNow);
				filePath = FTPDownloader.DownloadWithFallback(ftpClientMock.Object, fileName, localPath, remotePath, portalPath, (Exception ex) => { });
				StringAssert.AreEqualIgnoringCase(localFilePath, filePath, "Download from FTP failed, last downloaded cache is newer than portal file");

				Exception lastThrowedException = null;
				File.SetLastWriteTimeUtc(portalFilePath, utcTimeNow);
				File.SetLastWriteTimeUtc(localFilePath, utcTimeNow.AddMinutes(-1));
				filePath = FTPDownloader.DownloadWithFallback(ftpClientMock.Object, fileName, localPath, remotePath, portalPath, (Exception ex) => lastThrowedException = ex);
				StringAssert.AreEqualIgnoringCase(portalFilePath, filePath, "Download from FTP failed, portal file is newer than last downloaded cache");
				Assert.That(lastThrowedException, Is.Not.Null);
				Assert.That(lastThrowedException.Message, Is.EqualTo("Some Exception"));

				File.Delete(localFilePath);
				File.Delete(portalFilePath);
				Assert.Throws<FileNotFoundException>(() =>
				{
					FTPDownloader.DownloadWithFallback(ftpClientMock.Object, fileName, localPath, remotePath, portalPath, (Exception ex) => lastThrowedException = ex);
				}, "Download from FTP failed, no portal file or last downloaded cache");
			}
			finally
			{
				if (Directory.Exists(testBasePath))
				{
					Directory.Delete(testBasePath, true);
				}
			}
		}
	}
}
