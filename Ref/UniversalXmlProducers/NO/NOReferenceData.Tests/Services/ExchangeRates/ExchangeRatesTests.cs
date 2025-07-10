using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.ExchangeRates.Tests
{
	sealed class ExchangeRatesTests
	{
		[Test]
		public void TestExchangeRateCurrencyList()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<omregningKursListe>(mockHttpMessageHandler.ToHttpClient(), exchangeRatesUri, EmbeddedSchemaResources.ExchangeRateSchema);

			var currencyList = xmlData.ConversionRate.GroupBy(x => new
			{
				currency = x.CurrencyCode,
				fromdate = x.DateStart
			});
			foreach (var curr in currencyList)
			{
				sb.Append(curr.Key.currency + ",");
			}
			const string expectedCurrencyList = "AUD,AUD,CAD,CAD,CHF,CHF,CNY,CNY,CZK,CZK,DKK,DKK,EUR,EUR,GBP,GBP,HKD,HKD,INR,INR,JPY,JPY,NOK,NZD,NZD,PKR,PKR,PLN,PLN,SEK,SEK,SGD,SGD,THB,THB,USD,USD,ZAR,ZAR,";
			Assert.That(sb.ToString(), Is.EqualTo(expectedCurrencyList));
		}

		[Test]
		public void TestExchangeRateNumberOfEntries()
		{
			var (_, xmlData, _) = DownloadContent.Download<omregningKursListe>(mockHttpMessageHandler.ToHttpClient(), exchangeRatesUri, EmbeddedSchemaResources.ExchangeRateSchema);
			Assert.That(xmlData.ConversionRate.Length, Is.EqualTo(39));
		}

		[SetUp]
		public void Setup()
		{
			mockHttpMessageHandler = new MockHttpMessageHandler();
			var exchangeRatesUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceExchangeRatesFilename;
			var queryResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_complete_request.json");
			mockHttpMessageHandler.When(exchangeRatesUrl).Respond("application/json", queryResponse);

			var valutakurser = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ExchangeRates.Testfiles.Input.valutakurs.xml");
			const string resourceJsonUrl = "https://data.toll.no/dataset/a44e51b3-1404-40ae-abac-07964bc56282/resource/f7c90308-dd53-460d-95be-f34bd9cdb7ab/download/valutakurs.xml";
			mockHttpMessageHandler.When(resourceJsonUrl).Respond("application/xml", valutakurser);
			exchangeRatesUri = new Uri(exchangeRatesUrl);
		}
		MockHttpMessageHandler mockHttpMessageHandler;
		Uri exchangeRatesUri;
	}
}
