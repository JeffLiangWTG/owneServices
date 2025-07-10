using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class HarmonizedTariffParserTest
	{
		[Test]
		public void ConvertToXMLFile()
		{
			var parser = new HarmonizedTariffParser(new NomenclatureTariffParserForTest(), new HsnTariffBanDataParserForTest());
			var outputFolder = GetOutputFolder();
			parser.ConvertToXMLFile(outputFolder);

			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Output.HarmonizedTariff.RefCusNomenclatureGroupZZ_TR.xml");
			var actualXML = File.ReadAllText(Path.Combine(outputFolder, "RefCusNomenclatureGroupZZ_TR.xml"));
			Assert.That(actualXML, Is.EqualTo(expectedXML), "RefCusNomenclatureGroupZZ_TR.xml");

			expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Output.HarmonizedTariff.RefCusTariffZZ_TR_HSN.xml");
			actualXML = File.ReadAllText(Path.Combine(outputFolder, "RefCusTariffZZ_TR_HSN.xml"));
			Assert.That(actualXML, Is.EqualTo(expectedXML), "RefCusTariffZZ_TR_HSN.xml");
		}

		[Test]
		public void ConvertToXMLFileWithDtyRates()
		{
			var parserMock =
				new Mock<HarmonizedTariffParser>(new NomenclatureTariffParserForTest(),
					new HsnTariffBanDataParserForTest())
				{ CallBase = true };
			parserMock.Setup(p => p.RunAdditionalProcessors(It.IsAny<IEnumerable<RefCusTariff>>()))
				.Callback<IEnumerable<RefCusTariff>>(tariffs => HsnTariffDTYRatesProcessor.AttachDTYRates(tariffs));

			var parser = parserMock.Object;
			var outputFolder = GetOutputFolder();
			parser.ConvertToXMLFile(outputFolder);

			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Output.HarmonizedTariff.RefCusNomenclatureGroupZZ_TR.xml");
			var actualXML = File.ReadAllText(Path.Combine(outputFolder, "RefCusNomenclatureGroupZZ_TR.xml"));
			Assert.That(actualXML, Is.EqualTo(expectedXML), "RefCusNomenclatureGroupZZ_TR.xml");

			expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Output.HarmonizedTariff.RefCusTariffZZ_TR_HSN_DTYRates.xml");
			actualXML = File.ReadAllText(Path.Combine(outputFolder, "RefCusTariffZZ_TR_HSN.xml"));
			Assert.That(actualXML, Is.EqualTo(expectedXML), "RefCusTariffZZ_TR_HSN.xml");
		}

		string GetOutputFolder()
		{
			var outputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(outputFolder);
			return outputFolder;
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
	}
}
