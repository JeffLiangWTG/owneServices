using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Customstariffstructure;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Varenummer;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff.Tests
{
	sealed class TariffTest
	{
		[Test]
		public void TestTariffXml()
		{
			var customstariffData = XmlHelper.ReadDeserializedManifestResourceContent<CustomsTariffStructure>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.customstariffstructure.xml");
			var innfoerselsavgiftData = XmlHelper.ReadDeserializedManifestResourceContent<AvgiftListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.innfoerselsavgift.xml");
			var utfoerselsavgiftData = XmlHelper.ReadDeserializedManifestResourceContent<AvgiftListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.utfoerselsavgift.xml");
			var tollsatsData = XmlHelper.ReadDeserializedManifestResourceContent<vareListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.tollavgiftssats.xml");
			var raavareTollsatsData = XmlHelper.ReadDeserializedManifestResourceContent<vareListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.raavaretollavgiftssats.xml");
			var varenummerData = XmlHelper.ReadDeserializedManifestResourceContent<VarenummerListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.varenummer.xml");
			var landData = XmlHelper.ReadDeserializedManifestResourceContent<LandgruppeListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.TradeGroups.Testfiles.Input.landgruppe.xml");

			var ENcustomstariffStructure = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.customstariffstructure.xml");

			var refCusConditions = CusConditionCodes.GetCusConditionData();

			var modified = DateTime.Parse("01/01/2023 00:00:00", CultureInfo.InvariantCulture);
			var errors = TariffParser.ConvertToXMLFile("NO Customs Tariff", tollsatsData, raavareTollsatsData, customstariffData, innfoerselsavgiftData, utfoerselsavgiftData, varenummerData, landData, refCusConditions, ENcustomstariffStructure, modified, outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Output.RefCusTariff_NO.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.Empty);
				Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
			});
		}

		[SetUp]
		public void Setup()
		{
			outputTempFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		[TearDown]
		public void TearDown()
		{
			outputTempFileForTest.DeleteTestOutput();
		}
		string outputTempFileForTest;
	}
}
