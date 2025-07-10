using System;
using CargoWise.RefDbRepo.NOReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tests
{
	sealed class DownloadContentTests
	{
		[Test]
		public void TestDownloadFails()
		{
			var (errors, _, _) = DownloadContent.Download<omregningKursListe>(mockHttpMessageHandler.ToHttpClient(), new Uri("https://data.toll.no/downloaderror"), string.Empty);
			Assert.That(errors, Is.EqualTo("Error requesting resource content for, 'https://data.toll.no/downloaderror' the response status code was: NotFound." + Environment.NewLine));
		}

		[Test]
		public void TestValidationFails()
		{
			var resourceSearchInvalidUrl = ApplicationConfig.ResourceSearchUrl + "api_generic_invalidXml.json";
			var queryInvalidResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_invalidXml.json");
			mockHttpMessageHandler.When(resourceSearchInvalidUrl).Respond("application/json", queryInvalidResponse);

			var resourceInvalidXmlUrl = "https://data.toll.no/valutakurs_notValid.xml";
			var invalidvalutakurser = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.valutakurs_notValid.xml");
			mockHttpMessageHandler.When(resourceInvalidXmlUrl).Respond("application/xml", invalidvalutakurser);

			var (errors, _, _) = DownloadContent.Download<omregningKursListe>(mockHttpMessageHandler.ToHttpClient(), new Uri(resourceSearchInvalidUrl), EmbeddedSchemaResources.ExchangeRateSchema);
			Assert.That(errors, Is.EqualTo("Error validating XML downloaded from https://data.toll.no/api/3/action/resource_search?query=name:api_generic_invalidXml.json: The element 'omregningskurser' has invalid child element 'omregningskurs_ZZ'. List of possible elements expected: 'omregningskurs'.").NoClip);
		}

		[SetUp]
		public void Setup()
		{
			mockHttpMessageHandler = new MockHttpMessageHandler();
		}
		MockHttpMessageHandler mockHttpMessageHandler;
	}
}
