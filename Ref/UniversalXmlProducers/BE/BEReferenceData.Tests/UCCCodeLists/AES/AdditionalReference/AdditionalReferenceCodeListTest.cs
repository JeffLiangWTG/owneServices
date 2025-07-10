using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class AdditionalReferenceCodeListTest
	{
		string testFilesOutputPath;
		string testFilesInputPath;
		string outputPath;

		[Test]
		public void TestExtractAdditionalReferenceFile()
		{
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, "RD_AES_AdditionalReference.xml"));
			var extractedList = UCCCodeListHelper.ExtractUCCCodeListFile(Path.Combine(testFilesInputPath, "RD_AES_AdditionalReference.zip"), new ExportAdditionalReference(), outputPath);

			var resultContent = File.ReadAllText(extractedList.FileName).Replace("\n", "\r\n");
			Assert.AreEqual(expectedContent, resultContent);
		}

		[Test]
		public void TestGenerateUniversalXML()
		{
			GenerateUniversalXML(@"RD_AES_AdditionalReference_[2022-03-15'10h41]_''.xml", "RefCusCodeListZZ_BE_AR44E.xml");
		}

		void GenerateUniversalXML(string inputTestFileName, string outputTestFileName)
		{
			var xmlOutputFile = Path.Combine(outputPath, outputTestFileName);

			var cusCodeList = UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(Path.Combine(testFilesInputPath, inputTestFileName), Constants.ZZRefCusCodeList.UccAdditionalReference, new List<(string, string)> { (Constants.AttributeNames.Level, Constants.AttributeValues.Header ), (Constants.AttributeNames.Level, Constants.AttributeValues.Item) });
			XMLGeneration.ExportToXMLFile($"BE Additional Reference", xmlOutputFile, XMLGeneration.GetRefNctsCodesWriterConfiguration(Constants.ZZRefCusCodeList.UccAdditionalReference), UCCCodeListHelper.GetPublicationDateTime(new string[] { inputTestFileName }), cusCodeList);

			var effectiveOutputXmlDocument = new XmlDocument();
			effectiveOutputXmlDocument.Load(Path.Combine(outputPath, outputTestFileName));

			var expectedOutputXmlDocument = new XmlDocument();
			expectedOutputXmlDocument.Load(Path.Combine(testFilesOutputPath, outputTestFileName));

			BEReferenceDataTestHelper.AssertEqualXML(effectiveOutputXmlDocument.OuterXml, expectedOutputXmlDocument.OuterXml);
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\AdditionalReference\TestFiles\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\AdditionalReference\TestFiles\Input");
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\AdditionalReference\Output");
		}
	}
}
