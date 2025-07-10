using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	internal class Tariff4PGAParserTests
	{
		[Test]
		public void TestParse_NotFoundDownloadUrl()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() => null);

			var parser = new Tariff4PGAParser(mock.Object).Parse();
			var logMessage = string.Format(CultureInfo.InvariantCulture,
				"{0} : Can not find the PDF file for downloading (url: {1}).\r\n" +
				"{0} : Can not find the XLSX file for downloading (url: {1}).\r\n" +
				"{0} : Can not find the EV1 link for downloading (url: {2}).\r\n" +
				"Processed 0 tariffs for PGA codes.\r\n",
				DateTime.UtcNow.AddHours(-5).Date.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture),
				ApplicationConfig.Instance.ACEHTSCodesForPGAURL,
				ApplicationConfig.Instance.ACEHTSCodesForEV1URL);
			Assert.AreEqual(logMessage, parser.logMessage);
			Assert.Null(parser.tariff4PGACollect);
		}

		[Test]
		public void TestParse_NotFoundPublishDate()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var node = new HtmlNode(HtmlNodeType.Element, new HtmlDocument(), 0) { Name = "a" };

				node.Attributes.Add("href", "Test:Appendix_X_PGA_07202022_508c_0.pdf");
				node.InnerHtml = "ACE AESTIR Appendix X - HTS Codes for PGAs: PDF";

				return node;
			});

			var parser = new Tariff4PGAParser(mock.Object).Parse();
			var logMessage = string.Format(CultureInfo.InvariantCulture,
				"{0} : Can not find the publish date (url: {1}).\r\n" +
				"{0} : Can not find the publish date (url: {1}).\r\n" +
				"{0} : Can not find the EV1 publish date (url: {2}).\r\n" +
				"Processed 0 tariffs for PGA codes.\r\n",
				DateTime.UtcNow.AddHours(-5).Date.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture),
				ApplicationConfig.Instance.ACEHTSCodesForPGAURL,
				ApplicationConfig.Instance.ACEHTSCodesForEV1URL);
			Assert.AreEqual(logMessage, parser.logMessage);
			Assert.Null(parser.tariff4PGACollect);
		}

		[Test]
		public void TestParse_FailedDownloadingAppndixFile()
		{
			var pdfFileUrl = ApplicationConfig.Instance.CustomsBorderProtectionGoverment + @"\Appendix_X_PGA_07202022_508c_0.pdf";
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var trNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "tr" };
				var thNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "th" };
				var downloadNode = new HtmlNode(HtmlNodeType.Element, document, 1) { Name = "a" };
				downloadNode.Attributes.Add("href", pdfFileUrl);
				downloadNode.InnerHtml = "ACE AESTIR Appendix X - HTS Codes for PGAs: PDF";

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 2) { Name = "span" };
				dateNode.InnerHtml = "07/21/2022";
				trNode.AppendChild(thNode);
				thNode.AppendChild(downloadNode);
				trNode.AppendChild(dateNode);
				return downloadNode;
			});
			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => false);

			var parser = new Tariff4PGAParser(mock.Object).Parse();
			var logMessage = $"Download PDF file publish date: 07-21-2022.\r\n" +
				$"Download PDF file from {pdfFileUrl} failed.\r\n" +
				$"Download XLSX file publish date: 07-21-2022.\r\n" +
				$"Download XLSX file from {pdfFileUrl} failed.\r\n" +
				$"Download EV1 file publish date: 07-21-2022.\r\n" +
				$"Download EV1 file from {pdfFileUrl} failed.\r\n" +
				$"\r\nProcessed 0 tariffs for PGA codes.\r\n";
			Assert.AreEqual(logMessage, parser.logMessage);
			Assert.Null(parser.tariff4PGACollect);
		}

		[Test]
		public void TestParse_DownloadingFullPage()
		{
			var site = new HtmlDocument();
			var sitehtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Input.ACE AESTIR Appendix X — HTS Codes for PGAs.html");
			site.LoadHtml(sitehtml);

			var downloadNode = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.A && node.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains("ACE AESTIR APPENDIX X - HTS CODES FOR PGAS"));
			var dateNode = downloadNode.ParentNode.ParentNode.ChildNodes.FirstOrDefault(node => node.InnerText.Contains("07/21/2022"));

			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var trNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "tr" };
				var thNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "th" };
				trNode.AppendChild(thNode);
				thNode.AppendChild(downloadNode);
				trNode.AppendChild(dateNode);
				return downloadNode;
			});
			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => false);

			var parser = new Tariff4PGAParser(mock.Object).Parse();
			var logMessage = string.Format(CultureInfo.InvariantCulture,
				"Download PDF file publish date: {0}.\r\n" +
				"Download PDF file from {1} failed.\r\n" +
				"Download XLSX file publish date: {0}.\r\n" +
				"Download XLSX file from {1} failed.\r\n" +
				"Download EV1 file publish date: {0}.\r\n" +
				"Download EV1 file from {1} failed.\r\n" +
				"\r\nProcessed 0 tariffs for PGA codes.\r\n",
				"07-21-2022",
				"https://www.cbp.gov/sites/default/files/assets/documents/2022-Jul/Appendix_X_PGA_07202022_508c_0.pdf");
			Assert.AreEqual(logMessage, parser.logMessage);
			Assert.Null(parser.tariff4PGACollect);
		}
	}
}
