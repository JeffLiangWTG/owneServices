using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class TransportDocumentTypeCodeListTest
	{
		string testFilesOutputPath;
		string testFilesInputPath;
		string outputPath;

		[Test]
		public void TestExtractTransportDocumentTypeFile()
		{
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, "RD_AES_TransportDocumentType.xml"));
			var extractedList = UCCCodeListHelper.ExtractUCCCodeListFile(Path.Combine(testFilesInputPath, "RD_AES_TransportDocumentType.zip"), new ExportTransportDocumentType(), outputPath);

			var resultContent = File.ReadAllText(extractedList.FileName).Replace("\n", "\r\n");
			Assert.AreEqual(expectedContent, resultContent);
		}

		[Test]
		public void TestGenerateUniversalXML()
		{
			GenerateUniversalXML(@"RD_AES_TransportDocumentType_[2022-03-10'10h18]_''.xml", "RefCusCodeListZZ_BE_TD44E.xml");
		}

		void GenerateUniversalXML(string inputTestFileName, string outputTestFileName)
		{
			var xmlOutputFile = Path.Combine(outputPath, outputTestFileName);

			var cusCodeList = UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(Path.Combine(testFilesInputPath, inputTestFileName), Constants.ZZRefCusCodeList.UccTransportDocumentType, new List<(string, string)> { (Constants.AttributeNames.Level, Constants.AttributeValues.Header ), (Constants.AttributeNames.Level, Constants.AttributeValues.Item) });
			XMLGeneration.ExportToXMLFile($"BE Transport Documents", xmlOutputFile, XMLGeneration.GetRefNctsCodesWriterConfiguration(Constants.ZZRefCusCodeList.UccTransportDocumentType), UCCCodeListHelper.GetPublicationDateTime(new string[] { inputTestFileName }), cusCodeList);

			var effectiveOutputXmlDocument = new XmlDocument();
			effectiveOutputXmlDocument.Load(Path.Combine(outputPath, outputTestFileName));

			var expectedOutputXmlDocument = new XmlDocument();
			expectedOutputXmlDocument.Load(Path.Combine(Path.Combine(testFilesOutputPath, outputTestFileName)));

			BEReferenceDataTestHelper.AssertEqualXML(effectiveOutputXmlDocument.OuterXml, expectedOutputXmlDocument.OuterXml);
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\TransportDocumentType\TestFiles\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\TransportDocumentType\TestFiles\Input");
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\TransportDocumentType\Output");
		}
	}
}
