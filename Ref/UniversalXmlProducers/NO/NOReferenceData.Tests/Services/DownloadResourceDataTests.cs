using System;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tests
{
	sealed class DownloadResourceDataTests
	{
		[Test]
		public void TestMissingJsonUrlField()
		{
			var resourceUrl = resourceSearchUrl + "generic_test.xml";
			var queryResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_missingurlfield.json");
			mockHttpMessageHandler.When(resourceUrl).Respond("application/json", queryResponse);
			var (resourceData, errorString) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), new Uri(resourceUrl));

			var expectedErrorMessage = "We have incomplete Resource Data information to download the content. The query response was: " + queryResponse + Environment.NewLine;
			Assert.Multiple(() =>
			{
				Assert.That(resourceData.Url, Is.Null, "No Url value");
				Assert.That(errorString, Is.EqualTo(expectedErrorMessage).NoClip, "Incomplete Json details");
			});
		}

		[Test]
		public void TestBlankJsonUrlField()
		{
			var resourceUrl = resourceSearchUrl + "generic_test.xml";
			var queryResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_blankurl.json");
			mockHttpMessageHandler.When(resourceUrl).Respond("application/json", queryResponse);
			var (resourceData, errorString) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), new Uri(resourceUrl));
			var expectedErrorMessage = "We have incomplete Resource Data information to download the content. The query response was: " + queryResponse + Environment.NewLine;

			Assert.Multiple(() =>
			{
				Assert.That(resourceData.Url, Is.Empty, "Url is empty");
				Assert.That(errorString, Is.EqualTo(expectedErrorMessage).NoClip, "Blank Json details");
			});
		}

		[Test]
		public void TestValidResourceDataFromQuery()
		{
			var resourceUrl = resourceSearchUrl + "api_generic_complete_request.xml";
			var finalUrl = "https://data.toll.no/dataset/a44e51b3-1404-40ae-abac-07964bc56282/resource/f7c90308-dd53-460d-95be-f34bd9cdb7ab/download/valutakurs.xml";
			mockHttpMessageHandler.When(resourceUrl).Respond("application/json", EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_complete_request.json"));
			mockHttpMessageHandler.When(finalUrl).Respond("application/xml", "Test File Contents");
			var (resourceData, errorString) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), new Uri(resourceUrl));

			Assert.Multiple(() =>
			{
				Assert.That(resourceData.FileName, Is.EqualTo("valutakurs.xml"), "FileName");
				Assert.That(resourceData.LastModified, Is.EqualTo("02/02/2022 00:00:36"), "LastModified");
				Assert.That(resourceData.Url, Is.EqualTo(finalUrl), "Content Url");
				Assert.That(resourceData.FileContents, Is.EqualTo("Test File Contents"), "Contents");
				Assert.That(errorString, Is.Empty, "Errors");
			});
		}

		[Test]
		public void TestZeroByteFileContents()
		{
			var resourceUrl = resourceSearchUrl + "generic_test_zeroByte.xml";
			mockHttpMessageHandler.When(resourceUrl).Respond("application/json", EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_zeroByte.json"));
			mockHttpMessageHandler.When("https://data.toll.no/zeroByte").Respond("application/xml", string.Empty);
			var (resourceData, errorString) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), new Uri(resourceUrl));

			Assert.Multiple(() =>
			{
				Assert.That(resourceData.FileContents, Is.Empty, "Empty File Contents");
				Assert.That(errorString, Is.EqualTo("Downloaded content from 'https://data.toll.no/zeroByte', was zero bytes." + Environment.NewLine).NoClip, "Errors");
			});
		}

		[Test]
		public void TestInvalidDownloadUrl()
		{
			var resourceUrl = resourceSearchUrl + "generic_test_zeroByte.xml";
			mockHttpMessageHandler.When(resourceUrl).Respond("application/json", EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_downloaderror.json"));
			var (_, errorString) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), new Uri(resourceUrl));

			Assert.That(errorString, Is.EqualTo("Error requesting resource content for, 'https://data.toll.no/downloaderror' the response status code was: NotFound." + Environment.NewLine));
		}

		[SetUp]
		public void Setup()
		{
			resourceSearchUrl = ApplicationConfig.ResourceSearchUrl;
			mockHttpMessageHandler = new MockHttpMessageHandler();
		}
		string resourceSearchUrl;
		MockHttpMessageHandler mockHttpMessageHandler;
	}
}
