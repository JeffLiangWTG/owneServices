using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Common.Test
{
	[TestFixture]
	public class FileDownloaderFixture
	{
		[Test]
		public void TestHttpClientOverride()
		{
			using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(1) })
			{
				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					var fileDownloader = new FileDownloader(new Uri("http://ClientOverride"), httpClientOverride: httpClient);
					Assert.That(() => fileDownloader.GetFileStream(), Throws.TypeOf<TaskCanceledException>().With.Message.Match("The request was canceled due to the configured HttpClient\\.Timeout of 0.001 seconds elapsing\\."));
				}
			}
		}

		[Test]
		public void TestHttpClientRetryHandlerOverride()
		{
			using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(1) })
			{
				var httpClientRetryHandler = new HttpClientRetryHandler(5, 45);
				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					var fileDownloader = new FileDownloader(new Uri("http://RetryHandler"), httpClientOverride: httpClient, httpClientRetryHandlerOverride: httpClientRetryHandler);
					Assert.Throws<TaskCanceledException>(() => fileDownloader.GetFileStream());
					var output = sw.ToString();
					Assert.True(output.Contains("The 5 attempt to request http://retryhandler/ failed."));
				}
			}
		}

		[Test]
		public void CreateZipDirectoryIfMissing()
		{
			var zipFile = Path.Combine(testFilesFolderPath, "Any.zip");
			FileDownloader.ExtractZipFile(zipFile);

			Assert.True(Directory.Exists(expectedFolderPath));
			Assert.True(File.Exists(expectedFilePath));
		}

		[Test]
		public void ExtractZipFileRelativePath()
		{
			var relativePathZipFile = @"TestFiles\Any.zip";
			FileDownloader.ExtractZipFile(relativePathZipFile);

			Assert.True(Directory.Exists(testFilesFolderPath));
			Assert.True(File.Exists(expectedFilePath));
		}

		[Test]
		public void RetryGetCreationTime()
		{
			var fileDownloader = new FileDownloader(new Uri("http://lastmodifiedTime"));
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				Assert.Throws<HttpRequestException>(() => fileDownloader.GetCreationTime());
				var output = sw.ToString();
				Assert.True(output.Contains("The 3 attempt to request http://lastmodifiedtime/ failed. Exception message"));
			}
		}

		[Test]
		public void RetryGetFileStream()
		{
			var fileDownloader = new FileDownloader(new Uri("http://filestream"));
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				Assert.Throws<HttpRequestException>(() => fileDownloader.GetFileStream());
				var output = sw.ToString();
				Assert.True(output.Contains("The 3 attempt to request http://filestream/ failed. Exception message"));
			}
		}

		string testFilesFolderPath;
		string expectedFolderPath;
		string expectedFilePath;
		TextWriter defOut;

		[SetUp]
		public void SetUp()
		{
			var binFolder = AppDomain.CurrentDomain.BaseDirectory;
			testFilesFolderPath = Path.Combine(binFolder, "TestFiles");
			expectedFolderPath = Path.Combine(testFilesFolderPath, "Any");
			expectedFilePath = Path.Combine(expectedFolderPath, "any.txt");
			defOut = Console.Out;
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(expectedFolderPath))
			{
				Directory.Delete(expectedFolderPath, true);
			}
			Console.SetOut(defOut);
		}
	}
}
