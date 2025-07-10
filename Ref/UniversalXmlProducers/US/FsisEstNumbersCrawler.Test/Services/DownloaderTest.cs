using System.IO;
using System.Net;
using FsisEstNumbersCrawler.Services;
using NUnit.Framework;

namespace FsisEstNumbersCrawler.Test.Services
{
	[TestFixture]
	public class DownloaderTest
	{
		static readonly IDownloader Downloader = new Downloader();

		void DownloadTestFile()
		{
			var url = Utilities.CurrentFolder() + "\\Resources\\testfile.txt";
			var localPath = Path.GetTempFileName();
			Downloader.Download(url, localPath);
			Assert.True(File.Exists(localPath));
			Assert.True(File.ReadAllText(localPath).Contains("Hello World!"));
			File.Delete(localPath);
		}

		void DownloadWrongTestFile()
		{
			var url = Utilities.CurrentFolder() + "\\Resources\\wrongtestfile.txt";
			var localPath = Path.GetTempFileName();
			Downloader.Download(url, localPath);
		}

		[Test]
		public void DownloadShouldReturnExceptionWithWrongUrl()
		{
			Assert.That(DownloadWrongTestFile, Throws.TypeOf<WebException>());
		}

		[Test]
		public void DownloadShouldWork()
		{
			Assert.That(DownloadTestFile, Throws.Nothing);
		}
	}
}
