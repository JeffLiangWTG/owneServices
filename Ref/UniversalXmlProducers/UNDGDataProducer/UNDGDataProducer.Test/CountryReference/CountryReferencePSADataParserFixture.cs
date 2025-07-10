using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	class CountryReferencePSADataParserFixture
	{
		[Test]
		public void Output()
		{
			var additionalReferences = new List<UNDGCountryReference>()
			{
				new UNDGCountryReference() { DCR_Type = "ICPE", DCR_RN_NKCountry = "FR", DCR_Code = "4736", DCR_Description = "Stockage de trifluorure de bore" },
				new UNDGCountryReference() { DCR_Type = "ICPE", DCR_RN_NKCountry = "FR", DCR_Code = "1185.3", DCR_Description = "Stockage de fluides vierges, recyclés ou régénérés contenant du gaz à effet de serre fluorés ou substances qui appauvrissent la couche d’ozone" },
				new UNDGCountryReference() { DCR_Type = "ICPE", DCR_RN_NKCountry = "FR", DCR_Code = "QHSE", DCR_Description = "Contacter votre QHSE" }
			};

			var parser = new CountryReferencePSAParser(new FileDownloaderWrapper(), string.Empty, additionalReferences);
			var rawIMORecords = new List<UNDGSubstance>();

			var expectedHtmlFor0004 = File.ReadAllText(Path.Combine(_binPath, @"TestFiles\UNDG PSA Sample 0004.html"));
			var expectedHtmlFor2993 = File.ReadAllText(Path.Combine(_binPath, @"TestFiles\UNDG PSA Sample 2993.html"));
			var expectedHtmlFor3080 = File.ReadAllText(Path.Combine(_binPath, @"TestFiles\UNDG PSA Sample 3080.html"));
			var expectedHtmlFor1040 = File.ReadAllText(Path.Combine(_binPath, @"TestFiles\UNDG PSA Sample 1040.html"));
			var expectedHtmlFor3082 = File.ReadAllText(Path.Combine(_binPath, @"TestFiles\UNDG PSA Sample 3082.html"));

			_webScraperMock.Setup(x => x.ScrapeWithRetriesAsync(It.Is<Uri>(s => s == new Uri(string.Format(CultureInfo.InvariantCulture, Constants.DataSources.PSAGroupURL, "0004"))), 3)).ReturnsAsync(expectedHtmlFor0004);
			_webScraperMock.Setup(x => x.ScrapeWithRetriesAsync(It.Is<Uri>(s => s == new Uri(string.Format(CultureInfo.InvariantCulture, Constants.DataSources.PSAGroupURL, "2993"))), 3)).ReturnsAsync(expectedHtmlFor2993);
			_webScraperMock.Setup(x => x.ScrapeWithRetriesAsync(It.Is<Uri>(s => s == new Uri(string.Format(CultureInfo.InvariantCulture, Constants.DataSources.PSAGroupURL, "3080"))), 3)).ReturnsAsync(expectedHtmlFor3080);
			_webScraperMock.Setup(x => x.ScrapeWithRetriesAsync(It.Is<Uri>(s => s == new Uri(string.Format(CultureInfo.InvariantCulture, Constants.DataSources.PSAGroupURL, "1040"))), 3)).ReturnsAsync(expectedHtmlFor1040);
			_webScraperMock.Setup(x => x.ScrapeWithRetriesAsync(It.Is<Uri>(s => s == new Uri(string.Format(CultureInfo.InvariantCulture, Constants.DataSources.PSAGroupURL, "3082"))), 3)).ReturnsAsync(expectedHtmlFor3082);

			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "0004",
				DG_Variant = "a",
				DG_Class = "1.1D",
				DG_PSN = "AMMONIUM PICRATE",
				DG_PG = string.Empty
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "0004",
				DG_Variant = "b",
				DG_Class = "1.1D",
				DG_PSN = "AMMONIUM PICRATE",
				DG_PG = string.Empty
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "0004",
				DG_Variant = "c",
				DG_Class = "1.1D",
				DG_PSN = "AMMONIUM PICRATE",
				DG_PG = string.Empty
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "1040",
				DG_Variant = "a",
				DG_Class = "2.3",
				DG_PSN = "ETHYLENE OXIDE",
				DG_PG = string.Empty
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "1040",
				DG_Variant = "b",
				DG_Class = "2.3",
				DG_PSN = "ETHYLENE OXIDE WITH NITROGEN",
				DG_PG = string.Empty
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "1040",
				DG_Variant = "c",
				DG_Class = "2.3",
				DG_PSN = "ETHYLENE OXIDE",
				DG_PG = string.Empty
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "1040",
				DG_Variant = "d",
				DG_Class = "2.3",
				DG_PSN = "ETHYLENE OXIDE WITH NITROGEN",
				DG_PG = string.Empty
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "2993",
				DG_Variant = "a",
				DG_Class = "6.1",
				DG_PSN = "ARSENICAL PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				DG_PG = "I"
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "2993",
				DG_Variant = "b",
				DG_Class = "6.1",
				DG_PSN = "ARSENICAL PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				DG_PG = "II"
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "2993",
				DG_Variant = "c",
				DG_Class = "6.1",
				DG_PSN = "ARSENICAL PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				DG_PG = "II"
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "3080",
				DG_Variant = "a",
				DG_Class = "6.1",
				DG_PSN = "ISOCYANATES, TOXIC, FLAMMABLE, N.O.S",
				DG_PG = "II"
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "3080",
				DG_Variant = "b",
				DG_Class = "6.1",
				DG_PSN = "ISOCYANATE SOLUTION, TOXIC, FLAMMABLE, N.O.S.",
				DG_PG = "II"
			});
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "3082",
				DG_Variant = string.Empty,
				DG_Class = "9",
				DG_PSN = "ENVIRONMENTALLY HAZARDOUS SUBSTANCE, LIQUID, N.O.S.",
				DG_PG = "III"
			});

			var pSARecords = CountryReferencePSAParser.GetPSARecordsAsync(rawIMORecords, _webScraperMock.Object).GetAwaiter().GetResult();
			var uNDGWithPSARecords = parser.MergePSARecordsToUNDGSubstances(rawIMORecords, pSARecords);
			var writer = CountryReferenceXmlWriterConfiguration.GetXmlWriter(DateTime.MinValue);
			XmlWriterHelper.ExportToXml(writer, uNDGWithPSARecords, _dumpPath);

			var result = File.ReadAllText(_dumpPath);
			var expected = File.ReadAllText(_expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		[SetUp]
		public void Setup()
		{
			_binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_dumpPath = Path.Combine(_binPath, @"TestFiles\Country Reference PSA UNDG Result.xml");
			_expectedXmlFilePath = Path.Combine(_binPath, @"TestFiles\Country Reference PSA Expected.xml");
			_webScraperMock = new Mock<IWebScraper>();
		}

		string _dumpPath;
		string _binPath;
		string _expectedXmlFilePath;
		Mock<IWebScraper> _webScraperMock;
	}
}
