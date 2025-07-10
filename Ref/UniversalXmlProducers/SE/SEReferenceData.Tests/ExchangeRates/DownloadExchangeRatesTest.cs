using System.IO;
using System.Net;
using CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.SEReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Tests
{
	[TestFixture]
	class DownloadExchangeRatesTests
	{
		private const string MEDIA_TYPE = "text/plain";
		private const string EXCHANGE_RATE_URL = "http://example.com/filename.xml.gz.pgp";
		const string InputTestFilesPath = @"UniversalXmlProducers\SE\SEReferenceData.Tests\ExchangeRates\TestFiles\Input\";

		[Test]
		public void DownloadXML()
		{
			var filepath = Path.Combine(TestHelper.BaseSourcePath, InputTestFilesPath, "IncrementalObjectTraderExport_201130.xml.gz.pgp");
			using (var fileStream = File.OpenRead(filepath))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(EXCHANGE_RATE_URL).Respond(MEDIA_TYPE, fileStream);

				var exchangePeriods = new DownloadExchangeRates(mockHttp.ToHttpClient()).DownloadLatestIncrementalAndExtract(EXCHANGE_RATE_URL);
				Assert.That(exchangePeriods.Length, Is.EqualTo(1));
				Assert.That(exchangePeriods[0].monetaryExchangeRate.Length, Is.EqualTo(25));
			}
		}

		[Test]
		public void DownloadInvalidXML()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(EXCHANGE_RATE_URL).Respond(HttpStatusCode.NotFound);

				var exception = Assert.Throws<ObjectTraderExportException>(() => new DownloadExchangeRates(mockHttp.ToHttpClient()).DownloadLatestIncrementalAndExtract("InvalidURL"));
				Assert.That(exception.Message, Does.StartWith("Unable to Load IncrementalObjectTraderExport XML from the following URL: InvalidURL"));
			}
		}
	}
}
