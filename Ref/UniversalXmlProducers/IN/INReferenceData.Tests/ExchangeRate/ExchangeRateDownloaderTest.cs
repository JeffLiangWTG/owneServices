using System;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRateDownloaderTest
	{
		[Test]
		public void TestDownloadData()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Post, AppConfig.ExchangeRate.Url).Respond("application/json", "FOO");

				var downloader = new ExchangeRateDownloaderForTest();
				downloader.MockClient = mockHttp.ToHttpClient();

				var downloadData = downloader.DownloadData(DateTime.Now);
				Assert.AreEqual("FOO", downloadData);
			}
		}

		[Test]
		public void TestDownloadData_RequestFail()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Post, AppConfig.ExchangeRate.Url).Respond(HttpStatusCode.InternalServerError, "application/json", "");

				var downloader = new ExchangeRateDownloaderForTest { MockClient = mockHttp.ToHttpClient() };

				Assert.Throws<UnhandledApplicationException>(() => downloader.DownloadData(DateTime.Now),
					"Failed to load exchange rate from the source after 3 retries");
			}
		}
	}

	class ExchangeRateDownloaderForTest : ExchangeRateDownloader
	{
		public HttpClient MockClient { get; set; }

		protected override HttpClient GetHttpClient() => MockClient;

		protected override int SleepInterval => 1;
	}
}
