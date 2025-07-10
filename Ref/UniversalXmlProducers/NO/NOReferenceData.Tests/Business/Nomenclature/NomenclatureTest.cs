using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Business.Nomenclature;
using CargoWise.RefDbRepo.NOReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Customstariffstructure;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tolltariffstruktur;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tests.Nomenclature
{
	sealed class NomenclatureTest
	{
		[Test]
		public void TestCompositeKey()
		{
			var customstariffData = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.customstariffstructure.xml");

			var nomenclature = NomenclatureParser.CreateNomenclature(customstariffData);
			AssertTestNomenclature(true, nomenclature, "010121", "01012100", "01.01..01.2.1");
			AssertTestNomenclature(true, nomenclature, "020714", "02071490", "01.02..07.1.14.90");
			AssertTestNomenclature(false, nomenclature, "012345", "02071490", "01.02..03.04.05");
		}

		void AssertTestNomenclature(bool isValid, List<RefCusNomenclatureGroup> nomenclature, string hsNumber, string tariffId, string compositeKey)
		{
			TariffParser.ErrorBuilder.Clear();
			var errMsg = $@"Unable to find correct nomenclature.
DETAILS:
HSNumber: {hsNumber}
Tariff: {tariffId}
";

			var key = Tariff.Nomenclature.GetCompositeKey(nomenclature, hsNumber, tariffId);

			if (isValid)
			{
				Assert.AreEqual(compositeKey, key, $"{hsNumber} - {tariffId} - {compositeKey}");
			}
			else
			{
				Assert.That(TariffParser.ErrorBuilder.ToString(), Is.EqualTo(errMsg).NoClip);
			}
		}

		[Test]
		public void TestCreateNomenclatureNO()
		{
			var customstariffNoData = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.tolltariffstruktur.xml");

			var nomenclature = NomenclatureParserNO.CreateNomenclature(customstariffNoData);
			var norwegianTranslatedString = Tariff.Nomenclature.GetTranslatedString(nomenclature, "01.01..03.9.1");
			Assert.That("med vekt under 50 kg", Is.EqualTo(norwegianTranslatedString));
		}

		[Test]
		public void TestNomenclatureXml()
		{
			var errors = new StringBuilder();
			var (generalTariffStructreXML, generalTariffStructreErrors) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), customstariffstructureUri);
			var (tariffStructreXmlForNorway, tariffStructreXmlForNorwayErrors) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), tolltariffstrukturUri);
			errors.Append(generalTariffStructreErrors).Append(tariffStructreXmlForNorwayErrors);

			var modified = DateTime.Parse("01/01/2023 00:00:00", CultureInfo.InvariantCulture);
			errors.Append(NomenclatureParser.ConvertToXMLFile("NO Customs Nomenclature", generalTariffStructreXML.FileContents, tariffStructreXmlForNorway.FileContents, modified, outputTempFileForTest));

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Nomenclature.Testfiles.Output.Nomenclature_NO.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.Multiple(() =>
			{
				Assert.That(errors.ToString, Is.Empty);
				Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
			});
		}

		[Test]
		public void TestNomenclatureXmlEmptyGeneralData()
		{
			var errors = new StringBuilder();
			var modified = DateTime.Parse("01/01/2023 00:00:00", CultureInfo.InvariantCulture);
			errors.Append(NomenclatureParser.ConvertToXMLFile("NO Customs Nomenclature", string.Empty, string.Empty, modified, outputTempFileForTest));

			Assert.That(errors.ToString(), Is.EqualTo($"Unable to parse Nomenclature data, empty or NULL file.{Environment.NewLine}"));
		}

		[Test]
		public void TestNomenclatureXmlEmptyNorwegianData()
		{
			var errors = new StringBuilder();
			var (generalTariffStructreXML, generalTariffStructreErrors) = DownloadResourceData.Download(mockHttpMessageHandler.ToHttpClient(), customstariffstructureUri);
			errors.Append(generalTariffStructreErrors);

			var modified = DateTime.Parse("01/01/2023 00:00:00", CultureInfo.InvariantCulture);
			errors.Append(NomenclatureParser.ConvertToXMLFile("NO Customs Nomenclature", generalTariffStructreXML.FileContents, string.Empty, modified, outputTempFileForTest));
			Assert.That(errors.ToString, Is.EqualTo($"Unable to parse Nomenclature data (norwegian language), empty or NULL file.{Environment.NewLine}"));
		}

		[SetUp]
		public void Setup()
		{
			outputTempFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			mockHttpMessageHandler = new MockHttpMessageHandler();

			var customstariffstructureUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceCustomstariffstructureFilename;
			var customstariffstructureResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_customstariffstructure.json");
			mockHttpMessageHandler.When(customstariffstructureUrl).Respond("application/json", customstariffstructureResponse);
			var customstariffstructure = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.customstariffstructure.xml");
			var customstariffstructureJsonUrl = "https://data.toll.no/dataset/aeb26da2-f75a-440d-8d16-44de39775e43/resource/c91c5587-5370-41c7-abd9-16063b0e8119/download/customstariffstructure.xml";
			mockHttpMessageHandler.When(customstariffstructureJsonUrl).Respond("application/xml", customstariffstructure);
			customstariffstructureUri = new Uri(customstariffstructureUrl);

			var tolltariffstrukturUrl = ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceTollsatsFilename;
			var tolltariffstrukturResponse = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.api_generic_tolltariffstruktur.json");
			mockHttpMessageHandler.When(tolltariffstrukturUrl).Respond("application/json", tolltariffstrukturResponse);
			var tolltariffstruktur = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.tolltariffstruktur.xml");
			var tolltariffstrukturJsonUrl = "https://data.toll.no/dataset/0e3bbee9-4dd1-41e1-b7d8-1bafb41315e6/resource/5cfeb068-534f-42bc-a13c-d1944732d8d5/download/tolltariffstruktur.xml";
			mockHttpMessageHandler.When(tolltariffstrukturJsonUrl).Respond("application/xml", tolltariffstruktur);
			tolltariffstrukturUri = new Uri(tolltariffstrukturUrl);
		}

		[TearDown]
		public void TearDown()
		{
			outputTempFileForTest.DeleteTestOutput();
		}
		string outputTempFileForTest;
		MockHttpMessageHandler mockHttpMessageHandler;
		Uri customstariffstructureUri;
		Uri tolltariffstrukturUri;
	}
}
