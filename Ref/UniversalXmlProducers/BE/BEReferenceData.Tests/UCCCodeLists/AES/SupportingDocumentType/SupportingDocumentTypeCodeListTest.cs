using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class SupportingDocumentTypeCodeListTest
	{
		string testFilesOutputPath;
		string testFilesInputPath;
		string outputPath;

		[Test]
		public void TestExtractSupportingDocumentTypeFile()
		{
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, "RD_AES_SupportingDocumentType.xml"));
			var extractedList = UCCCodeListHelper.ExtractUCCCodeListFile(Path.Combine(testFilesInputPath, "RD_AES_SupportingDocumentType.zip"), new ExportSupportingDocumentType(), outputPath);

			var resultContent = File.ReadAllText(extractedList.FileName).Replace("\n", "\r\n");
			Assert.AreEqual(expectedContent, resultContent);
		}

		[Test]
		public void TestGenerateUniversalXML()
		{
			GenerateUniversalXML(@"RD_AES_SupportingDocumentType_[2022-03-11'11h19]_''.xml", "RefCusCodeListZZ_BE_DC44E.xml");
		}

		void GenerateUniversalXML(string inputTestFileName, string outputTestFileName)
		{
			var xmlOutputFile = Path.Combine(outputPath, outputTestFileName);

			var cusCodeList = UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(Path.Combine(testFilesInputPath, inputTestFileName), Constants.ZZRefCusCodeList.UccSupportingDocumentType, new List<(string, string)> { (Constants.AttributeNames.Level, Constants.AttributeValues.Header ), (Constants.AttributeNames.Level, Constants.AttributeValues.Item) });
			XMLGeneration.ExportToXMLFile($"BE Supporting Documents", xmlOutputFile, XMLGeneration.GetRefNctsCodesWriterConfiguration(Constants.ZZRefCusCodeList.UccSupportingDocumentType), UCCCodeListHelper.GetPublicationDateTime(new string[] { inputTestFileName }), cusCodeList);

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
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\SupportingDocumentType\TestFiles\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\SupportingDocumentType\TestFiles\Input");
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\SupportingDocumentType\Output");
		}
	}
}
