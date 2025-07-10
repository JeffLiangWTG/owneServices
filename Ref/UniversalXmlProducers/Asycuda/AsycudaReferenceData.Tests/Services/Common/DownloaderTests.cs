using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AsycudaReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Tests.Services;

[TestFixture]
public class DownloaderTests
{
	[Test]
	public async Task DownloadFileAsync_ShouldDownloadFile()
	{
		// Arrange  
		var fileUrl = new Uri("https://example.com/file.pdf");
		var destinationFolder = Path.GetTempPath();
		var expectedFileName = Path.Combine(destinationFolder, "file.pdf");
		using var mockHandler = new MockHttpMessageHandler("PDF content");
		using var httpClient = new HttpClient(mockHandler);

		// Act  
		var downloadedFilePath = await new Downloader(httpClient).DownloadFileAsync(fileUrl, destinationFolder);

		// Assert  
		Assert.IsTrue(File.Exists(downloadedFilePath));
		Assert.AreEqual(expectedFileName, downloadedFilePath);

		// Cleanup  
		File.Delete(downloadedFilePath);
	}
}
