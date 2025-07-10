using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSMiscData;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CFIAMiscData
{
	[TestFixture]
	public class CFIAMiscDataTest
	{
		class CFIAAIRSMiscDataFileDownloaderForTest : CFIAAIRSMiscDataFileDownloader
		{
			public CFIAAIRSMiscDataFileDownloaderForTest(string rootURLForTest, PreProcessChecker checker) : base(checker)
			{
				RootURLForTest = rootURLForTest;
			}

			public string RootURLForTest { get; set; }

			protected override string RootURL => RootURLForTest;
		}

		const string parentPath = "CFIAMiscData";

		[Test]
		public void TestExportExpecteXMLwithDifferenctDateFormat()
		{
			var path = Path.GetTempFileName();
			var exportFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\UniversalXmlProducers\CA\CAReferenceData.Tests\TestFiles\Output\CFIAMiscDataOutputXML.xml");
			exportFilePath = exportFilePath.Replace(@"..\..\UniversalXmlProducers\", "");
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.airs_miscellaneous_codes_20230224_eng.pdf", parentPath)))
			using (var expectedResultXml = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.CFIAMiscDataExpected20230224.xml", parentPath)))
			using (var file = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(file);
				stream.Close();
				file.Close();
				var reader = new DataReader(path, exportFilePath);
				reader.ReadPDFAndExportXML();
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(exportFilePath);
				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(expectedResultXml);
				Assert.That(xmlDoc.InnerXml, Is.EqualTo(expectedXmlDoc.InnerXml));
			}
		}

		[Test]
		public void TestExportExpecteXML()
		{
			var path = Path.GetTempFileName();
			var exportFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\UniversalXmlProducers\CA\CAReferenceData.Tests\TestFiles\Output\CFIAMiscDataOutputXML.xml");
			exportFilePath = exportFilePath.Replace(@"..\..\UniversalXmlProducers\", "");
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.airs_miscellaneous_codes_1494610954514_eng.pdf", parentPath)))
			using (var expectedResultXml = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.CFIAMiscDataExpected.xml", parentPath)))
			using (var file = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(file);
				stream.Close();
				file.Close();
				var reader = new DataReader(path, exportFilePath);
				reader.ReadPDFAndExportXML();
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(exportFilePath);
				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(expectedResultXml);
				Assert.That(xmlDoc.InnerXml, Is.EqualTo(expectedXmlDoc.InnerXml));
			}
		}

		[Test]
		public void TestDownloadFile()
		{
			checker.MarkAsProcessRequired();
			using (var stream2 = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.airs_miscellaneous_codes_1494610954514_eng.pdf", parentPath)))
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.DownloadPage.html", parentPath)))
			using (var htmlStream = new StreamReader(stream))
			{
				var html = htmlStream.ReadToEnd();
				var downloader = new CFIAAIRSMiscDataFileDownloaderForTest("", checker);
				var fileName = Path.GetTempFileName();
				var downLoadPDFPath = Path.Combine(TestHelper.GetCurrentFolder(), @"TestFiles\airs_miscellaneous_codes_1494610954514_eng.pdf");
				SaveFileTo(stream2, downLoadPDFPath);
				html = html.Replace(@"https://inspection.canada.ca/DAM/DAM-aboutcfia-sujetacia/STAGING/text-texte/airs_miscellaneous_codes_1494610954514_eng.pdf", downLoadPDFPath);
				var result = downloader.DownloadFile("", fileName, false, html);
				Assert.IsTrue(result);
				Assert.True(string.IsNullOrEmpty(downloader.ErrorBuilder.ToString()));
				Assert.That(downloader.PublicationTime, Is.EqualTo(new DateTime(2022, 05, 16)));
			}
		}

		[Test]
		public void TestFailedDownloadFile()
		{
			checker.MarkAsProcessRequired();
			using (var stream2 = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.airs_miscellaneous_codes_1494610954514_eng.pdf", parentPath)))
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.DownloadPage.html", parentPath)))
			using (var htmlStream = new StreamReader(stream))
			{
				var html = htmlStream.ReadToEnd();
				var downloader = new CFIAAIRSMiscDataFileDownloaderForTest(html, checker);
				var fileName = Path.GetTempFileName();
				var downLoadPDFPath = Path.Combine(TestHelper.GetCurrentFolder(), @"TestFiles\airs_miscellaneous_codes_1494610954514_eng.pdf");
				SaveFileTo(stream2, downLoadPDFPath);
				html = html.Replace(@"https://inspection.canada.ca/DAM/DAM-aboutcfia-sujetacia/STAGING/text-texte/airs_miscellaneous_codes_1494610954514_eng.pdf", downLoadPDFPath);
				var result = downloader.DownloadFile(html, fileName, false, html);
				Assert.IsTrue(!result);
				Assert.IsTrue(downloader.ErrorBuilder.ToString().Contains("File downloads failed. URL:"));

				checker.MarkAsProcessRequired();
				html = html.Replace(downLoadPDFPath, "Test");
				result = downloader.DownloadFile("", fileName, false, html);
				Assert.IsTrue(!result);
				Assert.IsTrue(downloader.ErrorBuilder.ToString().Contains("The data file node is not found in the web page, the page layout may have changed."));
			}
		}

		void SaveFileTo(Stream stream, string path)
		{
			byte[] srcBuf = new byte[stream.Length];
			stream.Read(srcBuf, 0, srcBuf.Length);
			stream.Seek(0, SeekOrigin.Begin);
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				fs.Write(srcBuf, 0, srcBuf.Length);
				fs.Close();
			}
		}

		[Test]
		public void TestReadPDFAndExportXML()
		{
			var path = Path.GetTempFileName();
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.airs_miscellaneous_codes_1494610954514_eng.pdf", parentPath)))
			using (var file = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(file);
				stream.Close();
				file.Close();
				var exportFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\UniversalXmlProducers\CA\CAReferenceData.Tests\TestFiles\Output\CFIAMiscData.xml");
				var reader = new DataReader(path, exportFilePath);
				var result = reader.ReadPDFAndExportXML();
				var file1 = new FileInfo(exportFilePath);
				Assert.IsTrue(result);
				Assert.IsTrue(file1.Exists);
				file1.Delete();
			}
		}

		[SetUp]
		public void Setup()
		{
			checker = new PreProcessChecker(Constants.ProgramFunctions.CFIAAIRSMiscData);
		}
		PreProcessChecker checker;
	}
}
