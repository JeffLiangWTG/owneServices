using System;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.XmlService;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class NoteA148RateGenerationEndToEndFixture
	{
		[TestCase("ITTariffPage2713200000_WithNoteA148", "ExpectedProduced2713200000_WithNoteA148", TestName = "GenerateUniversalReferenceDateaXmlForTariffWithNoteA148")]
		[TestCase("ITTariffPage2713200000_WithoutNoteA148", "ExpectedProduced2713200000_WithoutNoteA148", TestName = "GenerateUniversalReferenceDateaXmlForTariffWithoutNoteA148")]
		public void GenerateUniversalReferenceDataXml(string inputFilename, string outputFilename)
		{
			const string dummyUrl = "http://dummyurl";
			const string tariffCode = "2713200000";
			var inputTariffPageResourceName = "CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles." + inputFilename + ".html";
			var outputForTariffResourceName = "CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles." + outputFilename + ".xml";

			var publicationDate = new DateTime(2024, 01, 17);
			_xmlService.UpdatePublicationDate(publicationDate);

			var inputTariffPageHtml = EmbeddedResourceHelper.ReadManifestResourceContent(inputTariffPageResourceName);
			_webResponseHandlerMock.Setup(x => x.GetWebRepsonseForTariffAsync(dummyUrl, tariffCode))
				.Returns(Task.FromResult(new ResponseResult(true, inputTariffPageHtml, string.Empty)));

			_taxOrFeeCodeLookupMock.Setup(x => x.Lookup("22"))
				.Returns("ORD");

			_taxOrFeeCodeLookupMock.Setup(x => x.Lookup("10"))
				.Returns("RID");

			_taxOrFeeCodeLookupMock.Setup(x => x.Lookup("4"))
				.Returns("MIN");

			_tradeGroupLookupMock.Setup(x => x.Lookup(It.IsAny<string>()))
				.Returns("1011");

			_rateCodeDataLookupMock.Setup(x => x.Lookup("Imposta di consumo", tariffCode))
				.Returns(new RateCodeDataLookupResult { Code = "125", RateType = "MSC", RateTypeDataGrouping = "IT" });

			_rateCodeDataLookupMock.Setup(x => x.Lookup("Contributo Stazione Sperimentale Combustibili", tariffCode))
				.Returns(new RateCodeDataLookupResult { Code = "912", RateType = "LEV", RateTypeDataGrouping = "IT" });

			_preferenceDataLookupMock.Setup(x => x.Preferences)
				.Returns(new[] { new PreferenceData() { Code = "100", DataGrouping = "EUN" }, new PreferenceData() { Code = "200", DataGrouping = "EUN" } });

			var scrappedRecordParser = new ScrappedRecordParser(
				_tradeGroupLookupMock.Object,
				_taxOrFeeCodeLookupMock.Object,
				_rateCodeDataLookupMock.Object,
				publicationDate);

			var parser = new WebContentParser(
				_xmlService,
				_webResponseHandlerMock.Object,
				dummyUrl,
				scrappedRecordParser,
				_preferenceDataLookupMock.Object,
				_loggerMock.Object);
			parser.GenerateXmlFromWebContent(new[] { tariffCode }).Wait();

			var actualOutputXmlDocument = _xmlService.GetUniversalXml();

			var expectedOutputXmlContent = EmbeddedResourceHelper.ReadManifestResourceContent(outputForTariffResourceName);
			var expectedUniversalXmlDocument = new XmlDocument();
			expectedUniversalXmlDocument.LoadXml(expectedOutputXmlContent);

			Assert.AreEqual(expectedUniversalXmlDocument.OuterXml, actualOutputXmlDocument.OuterXml, "Produced XML");
		}

		[SetUp]
		protected void Setup()
		{
			_loggerMock = new Mock<ILogger>();
			_xmlService = new XmlService.XmlService(_loggerMock.Object);
			_webResponseHandlerMock = new Mock<IWebResponseHandler>();
			_tradeGroupLookupMock = new Mock<IDataLookup>();
			_taxOrFeeCodeLookupMock = new Mock<IDataLookup>();
			_rateCodeDataLookupMock = new Mock<IRateCodeDataLookup>();
			_preferenceDataLookupMock = new Mock<IPreferenceDataLookup>();
		}

		Mock<ILogger> _loggerMock;
		IXmlService _xmlService;
		Mock<IWebResponseHandler> _webResponseHandlerMock;
		Mock<IDataLookup> _tradeGroupLookupMock;
		Mock<IDataLookup> _taxOrFeeCodeLookupMock;
		Mock<IRateCodeDataLookup> _rateCodeDataLookupMock;
		Mock<IPreferenceDataLookup> _preferenceDataLookupMock;
	}
}
