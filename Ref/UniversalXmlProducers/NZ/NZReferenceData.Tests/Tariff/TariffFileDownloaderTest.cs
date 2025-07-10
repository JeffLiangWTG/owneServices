using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	sealed class TariffFileDownloaderTest
	{
		[Test]
		public void TestDownloadFile()
		{
			var assemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
			var resourcePath = assembly.GetName().Name + ".Tariff.TestFiles.Input.Tariff.Tariff.tar.gz";
			var outputFolderPath = Path.GetTempPath();

			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, "https://www.customs.govt.nz/api/datafiles/tariff").Respond("application/zip", assembly.GetManifestResourceStream(resourcePath));

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffListFileDownloaderForTest("Tariff.tar.gz");
					downloader.Download(client, "https://www.customs.govt.nz/api/datafiles/tariff", outputFolderPath);
				}
			}

			Assert.AreEqual(File.ReadAllLines(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Output\Tariff\Tariff_Details.csv")).Length, File.ReadAllLines(Path.Combine(outputFolderPath, "Tariff_Details.csv")).Length, "Should successfully download and unzip file");
			Assert.AreEqual(File.ReadAllLines(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Output\Tariff\Tariff_Levies.csv")).Length, File.ReadAllLines(Path.Combine(outputFolderPath, "Tariff_Levies.csv")).Length, "Should successfully download and unzip file");
			Assert.AreEqual(File.ReadAllLines(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Output\Tariff\Tariff_Levy_Formulas.csv")).Length, File.ReadAllLines(Path.Combine(outputFolderPath, "Tariff_Levy_Formulas.csv")).Length, "Should successfully download and unzip file");
			Assert.AreEqual(File.ReadAllLines(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Output\Tariff\Tariff_Rates.csv")).Length, File.ReadAllLines(Path.Combine(outputFolderPath, "Tariff_Rates.csv")).Length, "Should successfully download and unzip file");
			Assert.AreEqual(File.ReadAllLines(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Output\Tariff\time_stamp.txt")).Length, File.ReadAllLines(Path.Combine(outputFolderPath, "time_stamp.txt")).Length, "Should successfully download and unzip file");
		}

		[Test]
		public void TestDownloadWithInvalidUrl()
		{
			var localFilePath = Path.GetTempFileName();

			using (var mockHttp = new MockHttpMessageHandler())
			using (var client = mockHttp.ToHttpClient())
			{
				var exception = Assert.Throws<InvalidOperationException>(() => new TariffListFileDownloaderForTest("Tariff.tar.gz").Download(client, "InvalidURL", localFilePath));
				Assert.That(exception.Message, Does.StartWith("Unable to Load NZ Tariff List from the following URL: InvalidURL"));
			}
		}

		public sealed class TariffListFileDownloaderForTest : TariffListFileDownloader
		{
			public TariffListFileDownloaderForTest(string fileName)
			{
				this.fileName = fileName;
			}

			readonly string fileName;

			protected override string GetDownloadFileName(HttpResponseMessage httpResponseMessage)
			{
				var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("ContentDisposition");
				contentDisposition.FileName = fileName;

				httpResponseMessage.Content.Headers.ContentDisposition = contentDisposition;
				return base.GetDownloadFileName(httpResponseMessage);
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}

		Assembly assembly;
	}
}
