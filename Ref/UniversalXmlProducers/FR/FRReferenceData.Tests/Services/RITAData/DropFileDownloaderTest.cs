using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services
{
	[TestFixture]
	public class DropFileDownloaderTest
	{
		[Test]
		public void TestDownloadZipFromCustoms()
		{
			var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var downloadPath = Path.GetTempFileName();
			var baseUrl = ApplicationConfig.Instance.DeltaGBaseUrl;
			var downloadDirectory = Path.GetDirectoryName(downloadPath);
			var downloadFileName = Path.GetFileName(downloadPath);
			var error = Errors.No;

			var fileDownloaderMock = new Mock<IFileDownloader>();
			fileDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((address, downloadFilePath) =>
			{
				File.Copy(currentDir + "\\TestFiles\\" + ApplicationConfig.Instance.DownloadFileName, downloadFilePath);
			});
			DropFileDownloader.DownloadZipFromCustoms(fileDownloaderMock.Object, baseUrl, downloadDirectory, downloadFileName, ref error);

			Assert.AreEqual(Errors.No, error);
			Assert.AreEqual(true, File.Exists(downloadPath));

			File.Delete(downloadPath);
		}

		[Test]
		public void TestExtractZipFiles()
		{
			var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var downloadDirectory = currentDir + "\\TestFiles";
			var downloadFileName = ApplicationConfig.Instance.DownloadFileName;
			var currenciesFileName = "COURS_DEVISES.xml";
			var currencyPricesFileName = "DEVISES.XML";
			var filesToExtract = new string[] { currenciesFileName, currencyPricesFileName };
			var extractToDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(extractToDirectory);
			var error = Errors.No;

			DropFileDownloader.ExtractZipFile(downloadDirectory, downloadFileName, extractToDirectory, filesToExtract, out var publicationDate, ref error);

			Assert.AreEqual(Errors.No, error);
			Assert.AreEqual(new DateTime(2023, 12, 29), publicationDate.Date);
			Assert.AreEqual(true, File.Exists(extractToDirectory + "\\DEVISES.XML"));
			Assert.AreEqual(true, File.Exists(extractToDirectory + "\\COURS_DEVISES.XML"));

			var expectedDevisesContent = File.ReadAllText(currentDir + "\\TestFiles\\DEVISES.XML");
			var actualDevisesContent = File.ReadAllText(extractToDirectory + "\\DEVISES.XML");
			Assert.AreEqual(expectedDevisesContent, actualDevisesContent);

			var expectedCoursDevisesContent = File.ReadAllText(currentDir + "\\TestFiles\\COURS_DEVISES.XML");
			var actualCoursDevisesContent = File.ReadAllText(extractToDirectory + "\\COURS_DEVISES.XML");
			Assert.AreEqual(expectedCoursDevisesContent, actualCoursDevisesContent);

			Directory.Delete(extractToDirectory, true);
		}
	}
}
