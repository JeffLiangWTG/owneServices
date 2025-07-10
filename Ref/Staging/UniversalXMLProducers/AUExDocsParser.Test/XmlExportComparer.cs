using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser.Test
{
	[TestFixture]
	public class XmlExportComparer
	{
		[Test]
		public void XmlFileCompareForE01()
		{
			string fileCode = "E01Test";
			var e01 = new E01_AqisPlaceParser();
			var e01Result = e01.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var exportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExportE01.xml");
			var expectedExportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExpectedExportE01.xml");
			e01.ExportToXml(e01Result, exportFilepath);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void XmlFileCompareForE07()
		{
			string fileCode = "E07Test";
			var e07 = new E07_CutCodeParser();
			var e07Result = e07.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var exportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExportE07.xml");
			var expectedExportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExpectedExportE07.xml");
			e07.ExportToXml(e07Result, exportFilepath);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void XmlFileCompareForE21()
		{
			string fileCode = "E21Test";
			var e21 = new E21_ProductTypeParser();
			var e21Result = e21.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var exportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExportE21.xml");
			var expectedExportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExpectedExportE21.xml");
			e21.ExportToXml(e21Result, exportFilepath);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void XmlFileCompareForE25()
		{
			string fileCode = "E25Test";
			var e25 = new E25_SupplementaryCodeParser();
			var e25Result = e25.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var exportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExportE25.xml");
			var expectedExportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExpectedExportE25.xml");
			e25.ExportToXml(e25Result, exportFilepath);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void XmlFileCompareForE29()
		{
			string fileCode = "E29Test";
			var e29 = new E29_AqisPlaceParser();
			var e29Result = e29.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var exportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExportE29.xml");
			var expectedExportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExpectedExportE29.xml");
			e29.ExportToXml(e29Result, exportFilepath);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void XmlFileCompareForE38()
		{
			string fileCode = "E38Test";
			var e38 = new E38_DominantProductParser();
			var e38Result = e38.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var exportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExportE38.xml");
			var expectedExportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExpectedExportE38.xml");
			e38.ExportToXml(e38Result, exportFilepath);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void XmlFileCompareForE39()
		{
			string fileCode = "E39Test";
			var e39 = new E39_ApprovedCertifierParser();
			var e39Result = e39.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var exportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExportE39.xml");
			var expectedExportFilepath = Path.Combine(FolderHelper.GetBinFolder(), @"Res\ExpectedExportE39.xml");
			e39.ExportToXml(e39Result, exportFilepath);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser.Test.config.json");
		}

		string BinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
