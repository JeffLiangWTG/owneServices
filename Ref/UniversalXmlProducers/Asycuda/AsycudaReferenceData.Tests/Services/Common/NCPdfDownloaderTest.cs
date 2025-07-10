using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AsycudaReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Tests.Services;

[TestFixture]
public class NCPdfDownloaderTest
{
	[Test]
	public async Task FindAndDownloadLastPdfAsync_ShouldDownloadLastPdf()
	{
		// Arrange
		var url = new Uri("https://example.com");
		var destinationFolder = Path.GetTempPath();
		var htmlContent = "<html><body><a href='https://example.com/file1.pdf'>PDF1</a><a href='https://example.com/file2.pdf'>PDF2</a></body></html>";

		using var mockHttpMessageHandler = new MockHttpMessageHandler(htmlContent);
		using var httpClient = new HttpClient(mockHttpMessageHandler);

		var exchangeRateDownloader = new NCPdfDownloader(httpClient);

		// Act
		var downloadedFilePath = await exchangeRateDownloader.DownloadLatestPdf(url, destinationFolder);

		// Assert
		Assert.NotNull(downloadedFilePath);
		Assert.IsTrue(File.Exists(downloadedFilePath));
		Assert.IsTrue(downloadedFilePath.EndsWith("file2.pdf"));

		// Dispose
		File.Delete(downloadedFilePath);
	}

	[Test]
	public async Task FindAndDownloadLastPdfAsync_ShouldHandleNoPdfFound()
	{
		// Arrange
		var consoleOut = Console.Out;
		using var newStdOut = new StringWriter();
		Console.SetOut(newStdOut);

		var url = new Uri("https://example.com");
		var destinationFolder = Path.GetTempPath();
		var htmlContent = "<html><body><a href='https://example.com/file1.txt'>Text File</a></body></html>";

		using var mockHttpMessageHandler = new MockHttpMessageHandler(htmlContent);
		using var httpClient = new HttpClient(mockHttpMessageHandler);

		var exchangeRateDownloader = new NCPdfDownloader(httpClient);

		// Act
		var downloadedFilePath = await exchangeRateDownloader.DownloadLatestPdf(url, destinationFolder);

		// Assert
		Assert.IsNull(downloadedFilePath);
		Assert.IsTrue(newStdOut.ToString().Contains("No Exchange Rate PDF files were found on the provided page."));

		// Dispose
		Console.SetOut(consoleOut);
	}

	[Test]
	public async Task FindAndDownloadLastPdfAsync_ShouldHandleHttpFailure()
	{
		// Arrange
		var consoleOut = Console.Out;
		using var newStdOut = new StringWriter();
		Console.SetOut(newStdOut);

		var url = new Uri("https://example.com");
		var destinationFolder = Path.GetTempPath();

		using var mockHttpMessageHandler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
		using var httpClient = new HttpClient(mockHttpMessageHandler);

		var exchangeRateDownloader = new NCPdfDownloader(httpClient);

		// Act
		var downloadedFilePath = await exchangeRateDownloader.DownloadLatestPdf(url, destinationFolder);

		// Assert
		Assert.IsNull(downloadedFilePath);
		Assert.IsTrue(newStdOut.ToString().Contains("Error downloading file"));

		// Dispose
		Console.SetOut(consoleOut);
	}
}
