using System;
using System.IO;
using System.Net;
using NUnit.Framework;

namespace ZAReferenceData.Services
{
	class Downloader : IDownloader
	{
		public void Download(string url, string localPath)
		{
			using (var client = new WebClient())
			{
				try
				{
					ServicesProvider.Logger.Info($"Downloading from {url} to {localPath}");
					client.DownloadFile(new Uri(url), localPath);
				}
				catch (Exception e)
				{
					throw new WebException("Error occured when downloading file, see the inner exception for details.", e);
				}
			}
		}
	}
}


namespace ZAReferenceData.Services.Test
{
	[TestFixture]
	public class DownloaderTest
	{
		static readonly IDownloader Downloader = new Downloader();

		void DownloadTestFile()
		{
			var url = Utilities.CurrentFolder() + "\\TestResources\\testfile.txt";
			var localPath = Path.GetTempFileName();
			Downloader.Download(url, localPath);
			Assert.True(File.Exists(localPath));
			Assert.True(File.ReadAllText(localPath).Contains("Hello World!"));
			File.Delete(localPath);
		}

		void DownloadWrongTestFile()
		{
			var url = Utilities.CurrentFolder() + "\\TestResources\\wrongtestfile.txt";
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
