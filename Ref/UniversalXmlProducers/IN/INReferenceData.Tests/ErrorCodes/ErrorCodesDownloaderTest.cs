using System;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class ErrorCodesDownloaderTest
	{
		[Test]
		public void GetErrorCodes_ValidResponseWithCodes()
		{
			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			{
				response.Content = new StringContent(ValidCodesResponse);
				HttpClientMock.Setup(m => m.Get(It.IsAny<Uri>())).Returns(response);

				var result = Downloader.GetErrorCodes(SampleType);
				LoggerMock.Verify(m => m.Log(LogType.Info, It.Is<string>(x => x.Contains("Downloading Error Codes for Type:")), It.IsAny<object>()), Times.AtLeastOnce);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("E001", result[0].ErrorCode);
				Assert.AreEqual("Test Error", result[0].Description);
				Assert.AreEqual("Fresh", result[0].MessageType);

				LoggerMock.Verify(m => m.Log(LogType.Info, It.Is<string>(x => x.Contains("Codes starts with:")), It.IsAny<object>()), Times.AtLeastOnce);
			}
		}

		[Test]
		public void GetErrorCodes_ValidResponseMissingTable()
		{
			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			{
				response.Content = new StringContent(TableMissingResponse);
				HttpClientMock.Setup(m => m.Get(It.IsAny<Uri>())).Returns(response);

				var result = Downloader.GetErrorCodes(SampleType);
				Assert.AreEqual(0, result.Count);
				LoggerMock.Verify(m => m.Log(LogType.ReviewRequired, It.Is<string>(x => x.Contains("Table not found for code:")), It.IsAny<object>()), Times.AtLeastOnce);
			}
		}

		[Test]
		public void GetErrorCodes_WithEmptyResponse()
		{
			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			{
				response.Content = new StringContent("");
				HttpClientMock.Setup(m => m.Get(It.IsAny<Uri>())).Returns(response);

				var result = Downloader.GetErrorCodes(SampleType);
				Assert.AreEqual(0, result.Count);
				LoggerMock.Verify(m => m.Log(LogType.ReviewRequired, It.Is<string>(x => x.Contains("No response from source for code:")), It.IsAny<object>()), Times.AtLeastOnce);
			}
		}

		[Test]
		public void GetErrorCodes_WithInvalidTableSize()
		{
			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			{
				response.Content = new StringContent(InValidTableSizeResponse);
				HttpClientMock.Setup(m => m.Get(It.IsAny<Uri>())).Returns(response);

				var result = Downloader.GetErrorCodes(SampleType);
				Assert.AreEqual(0, result.Count);
				LoggerMock.Verify(m => m.Log(LogType.ReviewRequired, It.Is<string>(x => x.Contains("Unexpected number of columns in the table for code:")), It.IsAny<object>()), Times.AtLeastOnce);
			}
		}

		[Test]
		public void GetErrorCodes_FailedHttpRequest()
		{

			using (var response = new HttpResponseMessage(HttpStatusCode.InternalServerError))
			{
				HttpClientMock.Setup(m => m.Get(It.IsAny<Uri>())).Returns(response);
				Assert.Throws<UnhandledApplicationException>(() => Downloader.GetErrorCodes(SampleType));
			}
		}

		ErrorCodesDownloader Downloader => downloader ?? (downloader = new ErrorCodesDownloader(HttpClientMock.Object, DownloadContextProviderMock.Object, 1, 1, 0, 0, LoggerMock.Object));
		ErrorCodesDownloader downloader;

		Mock<IHttpClient> HttpClientMock => httpClientMock ?? (httpClientMock = new Mock<IHttpClient>());
		Mock<IHttpClient> httpClientMock;

		Mock<IErrorCodeDownloadContextProvider> DownloadContextProviderMock
		{
			get
			{
				if (downloadContextProviderMock == null)
				{
					downloadContextProviderMock = new Mock<IErrorCodeDownloadContextProvider>();
					var context = new ErrorCodeDownloadContext(SampleType, "SampleSubUrl", "//table");
					downloadContextProviderMock.Setup(m => m.GetContext(SampleType)).Returns(context);
				}
				return downloadContextProviderMock;
			}
		}
		Mock<IErrorCodeDownloadContextProvider> downloadContextProviderMock;

		Mock<ILogger> LoggerMock => loggerMock ?? (loggerMock = new Mock<ILogger>());
		Mock<ILogger> loggerMock;

		const ErrorCodeType SampleType = ErrorCodeType.BE;
		const string ValidCodesResponse = "<html><table><tr></tr><tr></tr><tr><td>E001</td><td>Test Error</td><td>Fresh</td></tr><tr></tr></table></html>";
		const string InValidTableSizeResponse = "<html><table><tr></tr><tr></tr><tr><td>1</td><td>E001</td><td>Test Error</td><td>Fresh</td></tr><tr></tr></table></html>";
		const string TableMissingResponse = "<html>Welcome to page</html>";
	}
}
