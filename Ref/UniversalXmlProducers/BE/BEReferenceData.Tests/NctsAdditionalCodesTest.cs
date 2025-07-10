using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class NctsAdditionalCodesTest
	{
		string testFilesOutputPath;
		string testFilesInputPath;
		string outputPath;

		[Test]
		public void TestExtractAdditionalCodesFile()
		{
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, @"CL380_ExtractRD_NCTS-P5_[2020-08-03'9h29]_''.xml"));
			var xmlOutputFilePath = UCCCodeListHelper.ExtractCodeListFiles(Path.Combine(testFilesInputPath, "codelijsten_NCTSP5.zip"), new string[] { Constants.NctsCodeListNames.AdditionalReferenceCodes }, outputPath)[Constants.NctsCodeListNames.AdditionalReferenceCodes];

			var resultContent = File.ReadAllText(xmlOutputFilePath).Replace("\n", "\r\n");
			Assert.AreEqual(expectedContent, resultContent);
		}

		[Test]
		public void TestExtractMultipleCodeListFiles()
		{
			var anotherCodeList = "CL008";
			var xmlOutputFilePath = UCCCodeListHelper.ExtractCodeListFiles(Path.Combine(testFilesInputPath, "codelijsten_NCTSP5.zip"), new string[] { Constants.NctsCodeListNames.AdditionalReferenceCodes, anotherCodeList }, outputPath);

			Assert.AreEqual(2, xmlOutputFilePath.Count);
			Assert.IsTrue(Path.GetFileName(xmlOutputFilePath[Constants.NctsCodeListNames.AdditionalReferenceCodes]).StartsWith(Constants.NctsCodeListNames.AdditionalReferenceCodes));
			Assert.IsTrue(Path.GetFileName(xmlOutputFilePath[anotherCodeList]).StartsWith(anotherCodeList));
		}

		[Test]
		public void TestMissingAdditionalCodesFile()
		{
			var ex = Assert.Throws<System.IO.IOException>(() => { UCCCodeListHelper.ExtractCodeListFiles(Path.Combine(testFilesInputPath, "codelijsten_NCTSP5_withoutCL380.zip"), new string[] { Constants.NctsCodeListNames.AdditionalReferenceCodes }, outputPath); });

			Assert.That(ex.Message, Does.StartWith("Invalid zip file content"));
		}

		[Test]
		public void TestGenerateUniversalXML()
		{
			GenerateUniversalXML(@"CL380_ExtractRD_NCTS-P5_[2020-08-03'9h29]_''.xml", $"RefCusCodeListZZ_BE_NCTS_{Constants.ZZRefCusCodeList.NctsAdditionalCode}.xml");
		}

		[Test]
		public void TestGenerateUniversalXMLTakeOnlyValidCodes()
		{
			GenerateUniversalXML(@"CL380_ExtractRD_NCTS-P5_[2020-08-03'9h29]_1st_item_not_valid.xml", $"RefCusCodeListZZ_BE_NCTS_{Constants.ZZRefCusCodeList.NctsAdditionalCode}_1st_item_not_valid.xml");
		}

		[Test]
		public void TestGenerateUniversalXMLMissingLanguageDescription()
		{
			GenerateUniversalXML(@"CL380_ExtractRD_NCTS-P5_[2020-08-03'9h29]_no_lang_DE.xml", $"RefCusCodeListZZ_BE_NCTS_{Constants.ZZRefCusCodeList.NctsAdditionalCode}_no_lang_DE.xml");
		}

		[Test]
		public void TestGetPubblicationDateTime()
		{
			var xmlInputFile = @"CL380_ExtractRD_NCTS-P5_[2020-08-03'9h29]_''.xml";
			var expectedDateTime = new DateTime(2020, 08, 03, 9, 29,0);

			var publicationDate = UCCCodeListHelper.GetPublicationDateTime(new string[] { xmlInputFile });
			Assert.AreEqual(expectedDateTime, publicationDate);

			xmlInputFile = @"CL380_ExtractRD_NCTS-P5_[202008030929]_''.xml";
			expectedDateTime = DateTime.Now;
			publicationDate = UCCCodeListHelper.GetPublicationDateTime(new string[] { xmlInputFile });

			Assert.AreEqual(new DateTime(expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day, expectedDateTime.Hour, expectedDateTime.Minute, expectedDateTime.Second, 0, expectedDateTime.Kind), new DateTime(publicationDate.Year, publicationDate.Month, publicationDate.Day, publicationDate.Hour, publicationDate.Minute, publicationDate.Second, 0, publicationDate.Kind));
		}

		void GenerateUniversalXML(string inputTestFileName, string outputTestFileName)
		{
			var xmlOutputFile = Path.Combine(outputPath, outputTestFileName);
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, outputTestFileName));

			var cusCodeList = UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(Path.Combine(testFilesInputPath, inputTestFileName), Constants.ZZRefCusCodeList.NctsAdditionalCode, new List<(string, string)> { (Constants.AttributeNames.Level, Constants.AttributeValues.Header), (Constants.AttributeNames.Level, Constants.AttributeValues.House), (Constants.AttributeNames.Level, Constants.AttributeValues.Item) });
			XMLGeneration.ExportToXMLFile($"BE Additional References NCTS - {Constants.ZZRefCusCodeList.NctsAdditionalCode}", xmlOutputFile, XMLGeneration.GetRefNctsAddCodesWriterConfiguration(), UCCCodeListHelper.GetPublicationDateTime(new[] { inputTestFileName }), cusCodeList);

			var resultContent = File.ReadAllText(xmlOutputFile);

			BEReferenceDataTestHelper.AssertEqualXML(resultContent, expectedContent);
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsAdditionalCodes\TestFiles\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsAdditionalCodes\TestFiles\Input");
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"NctsAdditionalCodes\Output");
		}
	}
}
