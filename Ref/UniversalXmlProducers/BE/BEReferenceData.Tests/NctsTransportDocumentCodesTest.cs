using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class NctsTransportDocumentCodesTest
	{
		string testFilesOutputPath;
		string testFilesInputPath;
		string outputPath;

		[Test]
		public void TestExtractTransportDocumentCodesFile()
		{
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, @"CL754_ExtractRD_NCTS-P5_[2020-08-07'15h20]_''.xml"));
			var xmlOutputFilePath = UCCCodeListHelper.ExtractCodeListFiles(Path.Combine(testFilesInputPath, "codelijsten_NCTSP5.zip"), new string[] { Constants.NctsCodeListNames.TransportDocumentCodes }, outputPath)[Constants.NctsCodeListNames.TransportDocumentCodes];

			var resultContent = File.ReadAllText(xmlOutputFilePath).Replace("\n", "\r\n");
			Assert.AreEqual(expectedContent, resultContent);
		}

		[Test]
		public void TestMissingTransportDocumentCodesFile()
		{
			var ex = Assert.Throws<System.IO.IOException>(() => { UCCCodeListHelper.ExtractCodeListFiles(Path.Combine(testFilesInputPath, "codelijsten_NCTSP5_withoutCL754.zip"), new string[] { Constants.NctsCodeListNames.TransportDocumentCodes }, outputPath); });

			Assert.That(ex.Message, Does.StartWith("Invalid zip file content"));
		}

		[Test]
		public void TestGenerateUniversalXMLTakeOnlyValidCodes()
		{
			GenerateUniversalXML(@"CL754_ExtractRD_NCTS-P5_[2020-08-07'15h20]_1st_item_not_valid.xml", "RefCusCodeListZZ_BE_NCTS_TD44N_1st_item_not_valid.xml");
		}

		[Test]
		public void TestGenerateUniversalXML()
		{
			GenerateUniversalXML(@"CL754_ExtractRD_NCTS-P5_[2020-08-07'15h20]_''.xml", "RefCusCodeListZZ_BE_NCTS_TD44N.xml");
		}

		[Test]
		public void TestGetPubblicationDateTime()
		{
			var xmlInputFile = @"CL754_ExtractRD_NCTS-P5_[2020-08-07'15h20]_''.xml";
			var expectedDateTime = new DateTime(2020, 08, 07, 15, 20, 0);

			var publicationDate = UCCCodeListHelper.GetPublicationDateTime(new string[] { xmlInputFile });
			Assert.AreEqual(expectedDateTime, publicationDate);
		}

		void GenerateUniversalXML(string inputTestFileName, string outputTestFileName)
		{
			var xmlOutputFile = Path.Combine(outputPath, outputTestFileName);
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, outputTestFileName));

			var cusCodeList = UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(Path.Combine(testFilesInputPath, inputTestFileName), Constants.ZZRefCusCodeList.NctsTransportDocumentCode, new List<(string, string)> { (Constants.AttributeNames.Level, Constants.AttributeValues.Header), (Constants.AttributeNames.Level, Constants.AttributeValues.House) });
			XMLGeneration.ExportToXMLFile($"BE Transport Documents - {Constants.ZZRefCusCodeList.NctsTransportDocumentCode}", xmlOutputFile, XMLGeneration.GetRefNctsTraDocCodesWriterConfiguration(), UCCCodeListHelper.GetPublicationDateTime(new[] { inputTestFileName }), cusCodeList);

			var resultContent = File.ReadAllText(xmlOutputFile);

			BEReferenceDataTestHelper.AssertEqualXML(resultContent, expectedContent);
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsTransportDocumentCodes\TestFiles\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsTransportDocumentCodes\TestFiles\Input");
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsTransportDocumentCodes\Output");
		}
	}
}
