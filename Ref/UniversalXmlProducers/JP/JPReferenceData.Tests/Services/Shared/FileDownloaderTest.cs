using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class FileDownloaderTest
	{
		[Test]
		public void TestTryDownloadUsingValidDownloadUrl()
		{
			
			var isDownloaded = FileDownloader.TryDownload(httpClientHelper, validDownloadUrl, filePath).GetAwaiter().GetResult();
			Assert.That(isDownloaded, Is.True);
			Assert.That(File.Exists(filePath), Is.True);

			var errors = stringWriter?.ToString();
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void TestTryDownloadUsingInvalidDownloadUrl()
		{
			var invalidDownloadUrl = "ThisIsAnInvalidDownloadUrl";
			var isDownloaded = FileDownloader.TryDownload(httpClientHelper, invalidDownloadUrl, filePath).GetAwaiter().GetResult();
			Assert.That(isDownloaded, Is.False);
			Assert.That(File.Exists(filePath), Is.False);

			var errors = stringWriter?.ToString();
			Assert.That(errors, Contains.Substring($"Failed to download. Download URL: {invalidDownloadUrl}. File Path: {filePath}."));
		}

		[SetUp]
		public void Setup()
		{
			filePath = Path.GetTempFileName();

			validDownloadUrl = "https://bbs.naccscenter.com/naccs/dfw/web/data/code/naccs/naimen.csv";
			mockValidDownloadUrl = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"TestFiles\naimen.csv");
			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockFileStream = new FileStream(mockValidDownloadUrl, FileMode.Open);
			mockHttpClientHelper.Setup(x => x.GetAsync(validDownloadUrl)).Returns(Task.FromResult<Stream>(mockFileStream));
			httpClientHelper = mockHttpClientHelper.Object;

			stringWriter = new StringWriter();
			Console.SetError(stringWriter);
		}

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			mockFileStream.Dispose();
		}

		string filePath;
		string validDownloadUrl;
		string mockValidDownloadUrl;
		FileStream mockFileStream;
		IHttpClientHelper httpClientHelper;
		StringWriter stringWriter;
	}
}
