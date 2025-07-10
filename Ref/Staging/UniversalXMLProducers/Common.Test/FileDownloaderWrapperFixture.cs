using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	public class FileDownloaderWrapperFixture
	{
		[Test]
		public void TestHttpClientOverride()
		{
			using (var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(1) })
			{
				var fileDownloaderWrapper = new FileDownloaderWrapper(client, null);
				Assert.That(() => fileDownloaderWrapper.DownloadFile("http://HttpClientOverride"), Throws.TypeOf<TaskCanceledException>().With.Message.Match("The request was canceled due to the configured HttpClient\\.Timeout of 0.001 seconds elapsing\\."));
			}
		}

		[Test]
		public void DownloadFile()
		{
			var tempPath = Path.Combine(Path.GetTempPath(), "{11C1FC70-075D-4D16-AF2D-D2A0CDAA4B81}");
			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
			Directory.CreateDirectory(tempPath);
			var tempFilePath = Path.Combine(tempPath, "TaricTest.zip");
			var fileDownloader = new Mock<IFileDownloader>();
			var fileDownloaderWrapper = new FileDownloaderWrapperTest(fileDownloader);
			var result = fileDownloaderWrapper.DownloadFile("", tempFilePath);
			Assert.True(result);
			Assert.True(File.Exists(tempFilePath));
			fileDownloader.Verify(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>()), Times.Once);
			fileDownloader.Verify(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>()), Times.Once);

			fileDownloader.Invocations.Clear();
			fileDownloaderWrapper.DownloadFile("", tempFilePath, DateTime.Now.AddYears(-1));
			fileDownloader.Verify(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>()), Times.Never);
			fileDownloader.Verify(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>()), Times.Never);

			fileDownloader.Invocations.Clear();
			fileDownloaderWrapper.DownloadFile("", tempFilePath, DateTime.Now.AddYears(1));
			fileDownloader.Verify(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>()), Times.Never);
			fileDownloader.Verify(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>()), Times.Once);
			Directory.Delete(tempPath, true);
		}

		[Test]
		public void DownloadFileToByteArray()
		{
			var testZipFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\TaricTest.zip");
			var fileBytes = File.ReadAllBytes(testZipFile);
			var fileDownloader = new Mock<IFileDownloader>();
			var fileDownloaderWrapper = new FileDownloaderWrapperTest(fileDownloader);
			var downloadBytes = fileDownloaderWrapper.DownloadFile("");
			Assert.AreEqual(fileBytes, downloadBytes);
			fileDownloader.Verify(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>()), Times.Never);
			fileDownloader.Verify(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>()), Times.Once);
		}

		[Test]
		public void DownloadAndExtract()
		{
			var tempPath = Path.Combine(Path.GetTempPath(), "{C3B17935-44A7-430A-B8C6-8177E67A19B5}");
			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
			Directory.CreateDirectory(tempPath);
			var tempFilePath = Path.Combine(tempPath, "TaricTest.zip");
			var fileDownloader = new Mock<IFileDownloader>();
			var fileDownloaderWrapper = new FileDownloaderWrapperTest(fileDownloader);
			var files = fileDownloaderWrapper.DownloadAndExtract("", tempFilePath);
			Assert.True(File.Exists(tempFilePath));
			Assert.AreEqual(5, files.Length);
			Assert.True(files.Contains(Path.Combine(tempPath, "Measures.csv")));

			fileDownloader.Verify(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>()), Times.Once);
			fileDownloader.Verify(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>()), Times.Once);

			fileDownloader.Invocations.Clear();
			fileDownloaderWrapper.DownloadAndExtract("", tempFilePath, DateTime.Now.AddYears(-1));
			fileDownloader.Verify(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>()), Times.Never);
			fileDownloader.Verify(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>()), Times.Never);

			fileDownloader.Invocations.Clear();
			fileDownloaderWrapper.DownloadAndExtract("", tempFilePath, DateTime.Now.AddYears(1));
			fileDownloader.Verify(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>()), Times.Never);
			fileDownloader.Verify(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>()), Times.Once);
			Directory.Delete(tempPath, true);
		}
	}

	class FileDownloaderWrapperTest : FileDownloaderWrapper
	{
		public FileDownloaderWrapperTest(Mock<IFileDownloader> fileDownloader)
		{
			var testZipFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\TaricTest.zip");
			var fileInfo = new FileInfo(testZipFile);

			fileDownloader.Setup(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>())).Returns(fileInfo.LastWriteTime);

			var stream = new MemoryStream(File.ReadAllBytes(testZipFile));
			var responseStream = new Mock<IResponseStream>();
			responseStream.Setup(x => x.GetResponseStream()).Returns(stream);

			fileDownloader.Setup(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>())).Returns(responseStream.Object);
			_fileDownloader = fileDownloader;
		}

		protected override IFileDownloader GetFileDownloader(string remoteUrl)
		{
			return _fileDownloader.Object;
		}

		readonly Mock<IFileDownloader> _fileDownloader;
	}
}
