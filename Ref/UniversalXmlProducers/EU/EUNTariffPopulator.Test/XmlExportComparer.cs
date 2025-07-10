using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator;
using Moq;
using NUnit.Framework;

namespace EUNTariffPopulator.Test
{
	[TestFixture]
	public class XmlExportComparer
	{
		[TestCase("2401109591ResultPage.html", "2401109591SinglePage.html", "2401109591MeasureConditions.html", "2401109591Expected.xml", "2401109591Result.xml")]
		[TestCase("0702000099ResultPage.html", "0702000099SinglePage.html", "0702000099MeasureConditions.html", "0702000099Expected.xml", "0702000099Result.xml")]
		[TestCase("0201100010ResultPage.html", "0201100010SinglePage.html", "0201100010MeasureConditions.html", "0201100010Expected.xml", "0201100010Result.xml")]
		public void XmlFileComparerTest(string resultPage, string singlePage, string conditionPage, string expectedResultXml, string outputFile)
		{
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

			var webDriverHelperMock = new Mock<IWebDriverHelper>();
			var publishedDate = new DateTime(2018, 1, 1, 0, 0, 0);
						
			var ResultPage = HttpUtility.HtmlDecode(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + resultPage + "")));
			webDriverHelperMock.Setup(x => x.GetWebPage(It.Is<string>(v => v.StartsWith(@"http://ec.europa.eu/taxation_customs/dds2/taric/measures.jsp?")), It.IsAny<int>())).Returns(ResultPage);

			var singleTariffPagePage = HttpUtility.HtmlDecode(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + singlePage + "")));
			webDriverHelperMock.Setup(x => x.GetWebPage(It.Is<string>(v => v.StartsWith(@"http://ec.europa.eu/taxation_customs/dds2/taric/measures_details.jsp?")), It.IsAny<int>())).Returns(singleTariffPagePage);

			var conditionPage1 = HttpUtility.HtmlDecode((File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + conditionPage + ""))));
			webDriverHelperMock.Setup(x => x.GetWebPage(It.Is<string>(v => v.StartsWith(@"http://ec.europa.eu/taxation_customs/dds2/taric/measures_conditions.jsp?")), It.IsAny<int>())).Returns(conditionPage1);

			var exportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + outputFile + "");
			var expectedExportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + expectedResultXml + "");

			new EUNTariffWebsiteParser(webDriverHelperMock.Object, exportFilepath, publishedDate, publishedDate, publishedDate).ProduceXML();

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(xmlDoc.InnerXml, expectedXmlDoc.InnerXml);
		}
	}
}
