using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NOReferenceData.Services.ErrorCodes;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes.Tests
{
	sealed class ErrorCodesTests
	{
		[Test]
		public void TestErrorCodeList()
		{
			var sb = new StringBuilder();
			var (_, xmlData, _) = DownloadContent.Download<FeilmeldingListe>(mockHttpMessageHandler.ToHttpClient(), errorCodesUri, EmbeddedSchemaResources.ErrorCodesSchema);

			var errorCodeList = xmlData.ErrorMessage.GroupBy(x => new
			{
				code = x.MessageNumber
			}).Take(10);
			foreach (var errCode in errorCodeList)
			{
				sb.Append(errCode.Key.code + ",");
			}
			const string expectedErrCodeList = "9017,9018,9066,9067,9068,9069,9070,9071,9072,9362,";
			Assert.That(sb.ToString(), Is.EqualTo(expectedErrCodeList));
		}

		[Test]
		public void TestErrorCodeNumberOfEntries()
		{
			var (_, xmlData, _) = DownloadContent.Download<FeilmeldingListe>(mockHttpMessageHandler.ToHttpClient(), errorCodesUri, EmbeddedSchemaResources.ErrorCodesSchema);
			Assert.That(xmlData.ErrorMessage.Length, Is.EqualTo(13));
		}

		[SetUp]
		public void Setup()
		{
			mockHttpMessageHandler = new MockHttpMessageHandler();
			var errorCodesUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceErrorCodesFilename;
			var queryResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_errorcodes.json");
			mockHttpMessageHandler.When(errorCodesUrl).Respond("application/json", queryResponse);
			var errorCodes = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Input.feilmelding.xml");
			const string resourceJsonUrl = "https://data.toll.no/dataset/18cb963c-4dad-4766-8c50-809a2c4b1a89/resource/1c4ac0f8-2c6c-456e-ad84-9beb4188e3ed/download/feilmelding.xml";
			mockHttpMessageHandler.When(resourceJsonUrl).Respond("application/xml", errorCodes);
			errorCodesUri = new Uri(errorCodesUrl);
		}
		MockHttpMessageHandler mockHttpMessageHandler;
		Uri errorCodesUri;
	}
}
