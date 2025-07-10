using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class NctsAdditionalInformationCodesTest
	{
		string testFilesOutputPath;
		string testFilesInputPath;
		string outputPath;

		[Test]
		public void TestExtractAdditionalInformationCodesFile()
		{
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, @"CL239_ExtractRD_NCTS-P5_[2020-08-03'9h25]_''.xml"));
			var xmlOutputFilePath = UCCCodeListHelper.ExtractCodeListFiles(Path.Combine(testFilesInputPath, "codelijsten_NCTSP5.zip"), new string[] { Constants.NctsCodeListNames.AdditionalInformationCodes }, outputPath)[Constants.NctsCodeListNames.AdditionalInformationCodes];

			var resultContent = File.ReadAllText(xmlOutputFilePath).Replace("\n", "\r\n");
			Assert.AreEqual(expectedContent, resultContent);
		}

		[Test]
		public void TestMissingAdditionalInformationCodesFile()
		{
			var ex = Assert.Throws<System.IO.IOException>(() => { UCCCodeListHelper.ExtractCodeListFiles(Path.Combine(testFilesInputPath, "codelijsten_NCTSP5_withoutCL239.zip"), new string[] { Constants.NctsCodeListNames.AdditionalInformationCodes }, outputPath); });

			Assert.That(ex.Message, Does.StartWith("Invalid zip file content"));
		}

		[Test]
		public void TestGenerateUniversalXMLTakeOnlyValidCodes()
		{
			GenerateUniversalXML(@"CL239_ExtractRD_NCTS-P5_[2020-08-03'9h25]_1st_item_invalid.xml", "RefCusCodeListZZ_BE_NCTS_AI44N_1st_item_not_valid.xml");
		}

		[Test]
		public void TestGenerateUniversalXML()
		{
			GenerateUniversalXML(@"CL239_ExtractRD_NCTS-P5_[2020-08-03'9h25]_''.xml", "RefCusCodeListZZ_BE_NCTS_AI44N.xml");
		}

		[Test]
		public void TestGetPubblicationDateTime()
		{
			var xmlInputFile = @"CL239_ExtractRD_NCTS-P5_[2020-08-03'9h25]_''.xml";
			var expectedDateTime = new DateTime(2020, 08, 03, 9, 25, 0);

			var publicationDate = UCCCodeListHelper.GetPublicationDateTime(new string[] { xmlInputFile });
			Assert.AreEqual(expectedDateTime, publicationDate);
		}

		void GenerateUniversalXML(string inputTestFileName, string outputTestFileName)
		{
			var xmlOutputFile = Path.Combine(outputPath, outputTestFileName);
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, outputTestFileName));

			var cusCodeList = UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(Path.Combine(testFilesInputPath, inputTestFileName), Constants.ZZRefCusCodeList.NctsAdditionalInfoCode, new List<(string, string)> { (Constants.AttributeNames.Level, Constants.AttributeValues.Header ), (Constants.AttributeNames.Level, Constants.AttributeValues.Item) });
			XMLGeneration.ExportToXMLFile($"BE Additional Information Codes - {Constants.ZZRefCusCodeList.NctsAdditionalInfoCode}", xmlOutputFile, XMLGeneration.GetRefNctsAddInfoCodesWriterConfiguration(), UCCCodeListHelper.GetPublicationDateTime(new string[] { inputTestFileName }), cusCodeList);

			var resultContent = File.ReadAllText(xmlOutputFile);

			BEReferenceDataTestHelper.AssertEqualXML(resultContent, expectedContent);
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsAdditionalInformationCodes\TestFiles\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsAdditionalInformationCodes\TestFiles\Input");
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsAdditionalInformationCodes\Output");
		}
	}
}
