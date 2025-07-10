using System;
using System.Net.Http;
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
	public class WebParserTest
	{
		[Test]
		public void WebContentParser_SuccessfullyGetsPublicationDate()
		{
			const string dummyUrl = "http://dummyurl";

			_webResponseHandlerMock.Setup(x => x.GetWebResponseForPublicationDateAsync(dummyUrl))
				.Returns(Task.FromResult(new ResponseResult(true, HtmlDataExtractorTest.SampleHtml, string.Empty)));

			var parser = new WebContentParser(_webResponseHandlerMock.Object, dummyUrl);
			var publicationDate = parser.GetPublicationDate();

			Assert.AreEqual(publicationDate.ToString("s"), "2017-08-30T00:00:00");
		}

		[Test]
		public async Task WebContentParser_SuccessfullyParseHTMLToXML()
		{
			const string dummyUrl = "http://dummyurl";
			var tariffCodes = new[] { "8703101800" };
			var rateCode1 = new RateCodeDataLookupResult { Code = "912", RateType = "LEV", RateTypeDataGrouping = "IT" };
			var rateCode2 = new RateCodeDataLookupResult { Code = "125", RateType = "EXC", RateTypeDataGrouping = "IT" };
			var rateCode3 = new RateCodeDataLookupResult { Code = "128", RateType = "EXC", RateTypeDataGrouping = "IT" };

			var expectedXmlContent = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ExpectedProduced8703101800.xml");
			var expectedUniversalXml = new XmlDocument();
			expectedUniversalXml.LoadXml(expectedXmlContent);

			var resourceDetail = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffPage8703101800.html");

			_webResponseHandlerMock.Setup(x => x.GetWebRepsonseForTariffAsync(dummyUrl, "8703101800"))
				.Returns(Task.FromResult(new ResponseResult(true, resourceDetail, string.Empty)));

			_taxOrFeeCodeLookupMock.Setup(x => x.Lookup(It.IsAny<string>())).Returns("ORD");
			_tradeGroupLookupMock.Setup(x => x.Lookup(It.IsAny<string>())).Returns("1011");
			_rateCodeDataLookupMock.Setup(x => x.Lookup("Contributo obbligatorio consorzio oli usati", "8703101800")).Returns(rateCode1);
			_rateCodeDataLookupMock.Setup(x => x.Lookup("Imposta di consumo", "8703101800")).Returns(rateCode2);
			_rateCodeDataLookupMock.Setup(x => x.Lookup("Accise", "8703101800")).Returns(rateCode3);

			var scrappedRecordParser = new ScrappedRecordParser(_tradeGroupLookupMock.Object, _taxOrFeeCodeLookupMock.Object, _rateCodeDataLookupMock.Object, DateTime.MinValue);
			var parser = new WebContentParser(_xmlService, _webResponseHandlerMock.Object, dummyUrl, scrappedRecordParser, _preferenceDataLookupMock.Object, _loggerMock.Object);
			await parser.GenerateXmlFromWebContent(tariffCodes);

			var actualXml = _xmlService.GetUniversalXml();

			Assert.AreEqual(expectedUniversalXml.OuterXml, actualXml.OuterXml);
		}

		[Test]
		public async Task WebContentParser_UnableToRetrieveTariffDescriptionError()
		{
			const string tariffCode = "0307211011";
			const string dummyUrl = "http://dummyurl";

			_webResponseHandlerMock
				.Setup(x => x.GetWebRepsonseForTariffAsync(dummyUrl, tariffCode))
				.Returns(Task.FromResult(new ResponseResult(true, HtmlDataExtractorTest.SampleHtml, string.Empty)));

			var scrappedRecordParser = new ScrappedRecordParser(_tradeGroupLookupMock.Object, _taxOrFeeCodeLookupMock.Object, _rateCodeDataLookupMock.Object, DateTime.MinValue);
			var webContentParser = new WebContentParser(_xmlService, _webResponseHandlerMock.Object, dummyUrl, scrappedRecordParser, _preferenceDataLookupMock.Object, _loggerMock.Object);
			await webContentParser.GenerateXmlFromWebContent([tariffCode]);

			_loggerMock.Verify(x => x.Log(It.IsAny<ParsingResult>()));
		}

		[Test]
		public async Task WebContentParser_PartialProcessingShouldIncludeAllTariffs()
		{
			const string dummyUrl = "http://dummyurl";
			var tariffCodes = new[] { "8481809980", "8703101800", "8802601900" };
			var rateCode1 = new RateCodeDataLookupResult { Code = "912", RateType = "LEV", RateTypeDataGrouping = "IT" };
			var rateCode2 = new RateCodeDataLookupResult { Code = "125", RateType = "EXC", RateTypeDataGrouping = "IT" };
			var rateCode3 = new RateCodeDataLookupResult { Code = "128", RateType = "EXC", RateTypeDataGrouping = "IT" };

			var expectedXmlContent = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ExpectedProducedFailures.xml");
			var expectedUniversalXml = new XmlDocument();
			expectedUniversalXml.LoadXml(expectedXmlContent);

			var resourceDetail = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffPage8703101800.html");

			_webResponseHandlerMock.Setup(x => x.GetWebRepsonseForTariffAsync(dummyUrl, "8481809980"))
				.Throws(new HttpRequestException());

			_webResponseHandlerMock.Setup(x => x.GetWebRepsonseForTariffAsync(dummyUrl, "8703101800"))
				.Returns(Task.FromResult(new ResponseResult(true, resourceDetail, string.Empty)));

			_webResponseHandlerMock.Setup(x => x.GetWebRepsonseForTariffAsync(dummyUrl, "8802601900"))
				.Returns(Task.FromResult(new ResponseResult(false, ErrorMessagesConstant.TariffCodeNoResponseFromWeb, "Dummy StackTrace")));

			_taxOrFeeCodeLookupMock.Setup(x => x.Lookup(It.IsAny<string>())).Returns("ORD");
			_tradeGroupLookupMock.Setup(x => x.Lookup(It.IsAny<string>())).Returns("1011");
			_rateCodeDataLookupMock.Setup(x => x.Lookup("Contributo obbligatorio consorzio oli usati", "8703101800")).Returns(rateCode1);
			_rateCodeDataLookupMock.Setup(x => x.Lookup("Imposta di consumo", "8703101800")).Returns(rateCode2);
			_rateCodeDataLookupMock.Setup(x => x.Lookup("Accise", "8703101800")).Returns(rateCode3);

			var scrappedRecordParser = new ScrappedRecordParser(_tradeGroupLookupMock.Object, _taxOrFeeCodeLookupMock.Object, _rateCodeDataLookupMock.Object, DateTime.MinValue);
			var parser = new WebContentParser(_xmlService, _webResponseHandlerMock.Object, dummyUrl, scrappedRecordParser, _preferenceDataLookupMock.Object, _loggerMock.Object);
			await parser.GenerateXmlFromWebContent(tariffCodes);

			var actualXml = _xmlService.GetUniversalXml();

			Assert.AreEqual(expectedUniversalXml.OuterXml, actualXml.OuterXml);
			_loggerMock.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<HttpRequestException>()), Times.Exactly(1));
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
