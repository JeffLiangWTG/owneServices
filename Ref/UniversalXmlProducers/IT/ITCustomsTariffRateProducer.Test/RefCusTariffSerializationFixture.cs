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
	sealed class RefCusTariffSerializationFixture
	{
		[Test]
		public void SerializeTariffWithChildElements()
		{
			const string dummyUrl = "http://dummyurl";
			const string tariffCode = "2204219631";
			const string inputTariffPageResourceName = "CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffPage2204219631.html";
			const string outputForTariffResourceName = "CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ExpectedProduced2204219631.xml";

			var publicationDate = new DateTime(2024, 01, 17);
			_xmlService.UpdatePublicationDate(publicationDate);

			var inputTariffPageHtml = EmbeddedResourceHelper.ReadManifestResourceContent(inputTariffPageResourceName);
			_webResponseHandlerMock.Setup(x => x.GetWebRepsonseForTariffAsync(dummyUrl, tariffCode))
				.Returns(Task.FromResult(new ResponseResult(true, inputTariffPageHtml, string.Empty)));

			_taxOrFeeCodeLookupMock.Setup(x => x.Lookup("22"))
				.Returns("ORD");

			_tradeGroupLookupMock.Setup(x => x.Lookup(It.IsAny<string>()))
				.Returns("1011");

			_rateCodeDataLookupMock.Setup(x => x.Lookup("Accise", tariffCode))
				.Returns(new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = "IT" });

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
		public void Setup()
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
