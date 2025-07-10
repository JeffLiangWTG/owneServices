using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser.Test
{
	class AESExportPortCodesParserFixture
	{
		[Test]
		public void TestParse()
		{
			var fileDownloaderMock = new Mock<IFileDownloaderWrapper>();
			fileDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			var httpClientMock = new Mock<IHttpClientHelper>();
			httpClientMock.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns(
				Task.FromResult(@"<a href=""https://www.cbp.gov/sites/default/files/assets/documents/2019-Sep/APPENDIX_D_Port_Cd_09042019_0.pdf"" />
<div  id=""node-document-74423"" class=""ds-1col node node-document view-mode-full node-published node-not-promoted node-not-sticky author-editor odd clearfix clearfix"">
<div class=""field field-name-field-date-release field-type-datetime field-label-inline clearfix""><div class=""field-label"">Document Posting Date:&nbsp;</div><div class=""field-items""><div class=""field-item even""><span class=""date-display-single"">September 4, 2019</span></div></div></div><div class=""field field-name-body field-type-text-with-summary field-label-hidden""><div class=""field-items""><div class=""field-item even""><p>In CERT: Sept. 05, 2019</p>
<p>In PROD: Sept. 06, 2019</p>
</div></div></div><div class=""field field-name-changed-date field-type-ds field-label-inline clearfix""><div class=""field-label"">Last modified:&nbsp;</div><div class=""field-items""><div class=""field-item even"">September 4, 2019</div></div></div><div class=""field field-name-field-tags field-type-taxonomy-term-reference field-label-inline clearfix""><div class=""field-label"">Tags:&nbsp;</div><div class=""field-items""><div class=""field-item even""><a href=""/tags/trade"">Trade</a></div><div class=""field-item odd""><a href=""/tags/automated-commercial-environment-ace"">Automated Commercial Environment (ACE)</a></div></div></div></div>
</div>")
			);

			var parser = new AESExportPortCodesParser(fileDownloaderMock.Object, httpClientMock.Object, "any", _pdfFilePath);
			parser.Parse(_xmlDumpPath);

			var xml = new XmlDocument();
			xml.Load(_xmlDumpPath);

			Assert.NotNull(xml);

			var publishDate = xml.GetElementsByTagName("PublicationTime");
			Assert.AreEqual("2019-09-04T00:00:00", publishDate[0].InnerText);

			var codeLists = xml.GetElementsByTagName(nameof(RefCusCodeList));
			Assert.Greater(codeLists.Count, 0);

			var firstRecord = codeLists[0];
			Assert.AreEqual("0101", firstRecord[nameof(RefCusCodeList.ZZD_Code)].InnerText);
			Assert.AreEqual("PORTLAND, ME", firstRecord[nameof(RefCusCodeList.ZZD_Description)].InnerText);

			var transports = firstRecord.ChildNodes.Cast<XmlNode>().Where(o => o.Name == nameof(RefCusCodeOrAttributeTransportMode));
			Assert.Greater(transports.Count(), 0);

			var firstTransport = transports.First();
			Assert.AreEqual("SEA", firstTransport[nameof(RefCusCodeOrAttributeTransportMode.ZZU_TransportMode)].InnerText);

			var attribute = firstRecord.ChildNodes.Cast<XmlNode>().Where(o => o.Name == nameof(RefCusCodeListAttribute));
			Assert.Greater(attribute.Count(), 0);

			var firstAttribute = attribute.Cast<XmlElement>().First();
			Assert.True(firstAttribute.IsEmpty);
		}

		[Test]
		public void TestParseThrowExWhenDownloadPDFFail()
		{
			var fileDownloaderMock = new Mock<IFileDownloaderWrapper>();
			fileDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

			var httpClientMock = new Mock<IHttpClientHelper>();
			httpClientMock.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns(
				Task.FromResult(@"<a href=""https://www.cbp.gov/sites/default/files/assets/documents/2019-Sep/APPENDIX_D_Port_Cd_09042019_0.pdf"" />
<div  id=""node-document-74423"" class=""ds-1col node node-document view-mode-full node-published node-not-promoted node-not-sticky author-editor odd clearfix clearfix"">
<div class=""field field-name-field-date-release field-type-datetime field-label-inline clearfix""><div class=""field-label"">Document Posting Date:&nbsp;</div><div class=""field-items""><div class=""field-item even""><span class=""date-display-single"">September 4, 2019</span></div></div></div><div class=""field field-name-body field-type-text-with-summary field-label-hidden""><div class=""field-items""><div class=""field-item even""><p>In CERT: Sept. 05, 2019</p>
<p>In PROD: Sept. 06, 2019</p>
</div></div></div><div class=""field field-name-changed-date field-type-ds field-label-inline clearfix""><div class=""field-label"">Last modified:&nbsp;</div><div class=""field-items""><div class=""field-item even"">September 4, 2019</div></div></div><div class=""field field-name-field-tags field-type-taxonomy-term-reference field-label-inline clearfix""><div class=""field-label"">Tags:&nbsp;</div><div class=""field-items""><div class=""field-item even""><a href=""/tags/trade"">Trade</a></div><div class=""field-item odd""><a href=""/tags/automated-commercial-environment-ace"">Automated Commercial Environment (ACE)</a></div></div></div></div>
</div>")
			);

			var parser = new AESExportPortCodesParser(fileDownloaderMock.Object, httpClientMock.Object, "any", _pdfFilePath);

			Assert.Throws<Exception>(() => parser.Parse(_xmlDumpPath));
		}

		[SetUp]
		public void SetUp()
		{
			_binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_xmlDumpPath = Path.Combine(_binPath, "317685BC-6D7F-4FDD-9C49-D891E5967CBB_dump", "test.xml");
			_pdfFilePath = Path.Combine(_binPath, "Res", "APPENDIX_D_Port_Cd.PDF");
		}

		string _binPath;
		string _pdfFilePath;
		string _xmlDumpPath;
	}
}
