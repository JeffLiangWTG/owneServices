using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class AdditionalInformationListTest
	{
		string testFilesOutputPath;
		string testFilesInputPath;
		string outputPath;

		[Test]
		public void TestExtractAdditionalInformationFile()
		{
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, "RD_AES_AdditionalInformation.xml"));
			var extractedList = UCCCodeListHelper.ExtractUCCCodeListFile(Path.Combine(testFilesInputPath, "RD_AES_AdditionalInformation.zip"), new ExportAdditionalInformation(), outputPath);

			var resultContent = File.ReadAllText(extractedList.FileName).Replace("\n", "\r\n");
			Assert.AreEqual(expectedContent, resultContent);
		}

		[Test]
		public void TestGenerateUniversalXML()
		{
			GenerateUniversalXML("RD_AES_AdditionalInformation.xml", "RefCusCodeListZZ_BE_AI44E.xml");
		}

		void GenerateUniversalXML(string inputTestFileName, string outputTestFileName)
		{
			var xmlOutputFile = Path.Combine(outputPath, outputTestFileName);

			var cusCodeList = UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(Path.Combine(testFilesInputPath, inputTestFileName), Constants.ZZRefCusCodeList.UccAdditionalInformation, new List<(string, string)> { (Constants.AttributeNames.Level, Constants.AttributeValues.Header ), (Constants.AttributeNames.Level, Constants.AttributeValues.Item) });
			XMLGeneration.ExportToXMLFile(Constants.DataSources.BeAdditionalInformation, xmlOutputFile, XMLGeneration.GetRefNctsCodesWriterConfiguration(Constants.ZZRefCusCodeList.UccAdditionalInformation), new System.DateTime(2022, 3, 15, 8, 14, 15), cusCodeList);

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
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\AdditionalInformation\TestFiles\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\AdditionalInformation\TestFiles\Input");
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\AES\\AdditionalInformation\Output");
		}
	}
}
