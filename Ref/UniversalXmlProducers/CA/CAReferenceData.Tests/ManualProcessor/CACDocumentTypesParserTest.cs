using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.ManualProcessor;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.ManualProcessor
{
	[TestFixture]
	public class CACDocumentTypesParserTest
	{
		[Test]
		public void TestParseCusCodeListIntoXML()
		{
			var builder = new StringBuilder();
			var path = Path.GetTempFileName();
			using (var stream = TestHelper.GetTestInputFile("ManualProcessor.CACDocumentTypes.csv"))
			using (var expectedResultXml = TestHelper.GetTestInputFile("CACDocumentTypes.xml"))
			using (var inputFile = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(inputFile);
				stream.Close();
				inputFile.Close();

				var publicationTime = new DateTime(2023, 08, 05, 00, 00, 00);
				var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "CACDocumentTypes.xml");
				var parser = new CACDocumentTypesParser(exportFilePath, publicationTime, path, builder);
				parser.ParseCusCodeListIntoXML();

				var exportFile = new FileInfo(exportFilePath);
				Assert.IsTrue(exportFile.Exists);
				var documentTypesXmlDoc = new XmlDocument();
				documentTypesXmlDoc.Load(exportFilePath);
				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(expectedResultXml);
				Assert.AreEqual(documentTypesXmlDoc.InnerXml, expectedXmlDoc.InnerXml);
				exportFile.Delete();
			}
		}

		[Test]
		public void TestParseCusCodeListIntoXML_WithErrorFormat()
		{
			var builder = new StringBuilder();
			var path = Path.GetTempFileName();
			using (var stream = TestHelper.GetTestInputFile("ManualProcessor.CACDocumentTypes_WithErrorFormat.csv"))
			using (var expectedResultXml = TestHelper.GetTestInputFile("CACDocumentTypes.xml"))
			using (var inputFile = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(inputFile);
				stream.Close();
				inputFile.Close();

				var publicationTime = new DateTime(2023, 08, 05, 00, 00, 00);
				var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "CACDocumentTypes.xml");
				var parser = new CACDocumentTypesParser(exportFilePath, publicationTime, path, builder);
				parser.ParseCusCodeListIntoXML();

				var message = builder.ToString();
				Assert.That(message, Does.Contain("Error processing CA CDocument Types data :\r\nHeader with name 'ID Code' was not found. If you are expecting some headers to be missing and want to ignore this validation, set the configuration HeaderValidated to null. You can also change the functionality to do something else, like logging the issue."));
			}
		}

		[Test]
		public void TestParseCusCodeListIntoXML_WithMissingData()
		{
			var builder = new StringBuilder();
			var path = Path.GetTempFileName();
			using (var stream = TestHelper.GetTestInputFile("ManualProcessor.CACDocumentTypes_WithMissingData.csv"))
			using (var expectedResultXml = TestHelper.GetTestInputFile("CACDocumentTypes.xml"))
			using (var inputFile = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(inputFile);
				stream.Close();
				inputFile.Close();

				var publicationTime = new DateTime(2023, 08, 05, 00, 00, 00);
				var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "CACDocumentTypes.xml");
				var parser = new CACDocumentTypesParser(exportFilePath, publicationTime, path, builder);
				parser.ParseCusCodeListIntoXML();

				var message = builder.ToString();
				Assert.That(message, Does.Contain("Missing data in line 10."));
				Assert.That(message, Does.Contain("Missing data in line 21."));
				Assert.That(message, Does.Contain("Missing data in line 22."));
				Assert.That(message, Does.Contain("Not populate successfully, please verify if the file contains any content or check the missing data."));
			}
		}
	}
}
