using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.GBReferenceData.Business.ChiefHarmonisedDeclarationCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.ChiefHarmonisedDeclarationCode
{
	[TestFixture]
	class ChiefHarmonisedDeclarationCodeParserTests
	{
		[Test]
		public void DownloadAndConvertToRefCusCodeListXMLTest()
		{
			using (var expectedTestStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.GBReferenceData.Tests.ChiefHarmonisedDeclarationCode.TestFiles.Output.RefCusCodeListZZ_GB_DC44.xml"))
			{
				var inputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ChiefHarmonisedDeclarationCode\TestFiles\Input\HDCDownloadFile.zip");
				chiefHarmonisedDeclarationCodeParser.DownloadAndConvertToRefCusCodeListXML(outputPath, inputFile, new DateTime(2019, 6, 14));

				using (var resultStream = new FileStream(Path.Combine(outputPath, "RefCusCodeListZZ_GB_DC44.xml"), FileMode.Open))
				{
					using (TextReader trResult = new StreamReader(resultStream))
					using (TextReader trExpected = new StreamReader(expectedTestStream))
					{
						var result = trResult.ReadToEnd();
						var expected = trExpected.ReadToEnd();

						Assert.That(result, Is.EqualTo(expected));
					}
				}
			}
		}

		[Test]
		public void TestFileDownloadIsEmpty()
		{
			var inputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ChiefHarmonisedDeclarationCode\TestFiles\Input\EmptyHDCDownloadFile.zip");

			Assert.Throws(Is.TypeOf<FileNotFoundException>().And.Message.EqualTo("Downloaded Zip file for Chief Harmonised Data has changed structure and does not contain a content.xml file."),
				() => chiefHarmonisedDeclarationCodeParser.DownloadAndConvertToRefCusCodeListXML(outputPath, inputFile, new DateTime(2019, 6, 14)));
		}

		[Test]
		public void TestFileDownloadFailsCorruptFile()
		{
			Assert.Throws(Is.TypeOf<InvalidDataException>().And.Message.EqualTo("Central Directory corrupt."),
				() => chiefHarmonisedDeclarationCodeParser.DownloadAndConvertToRefCusCodeListXML(outputPath, Path.GetTempFileName(), new DateTime(2019, 6, 14)));
		}

		[Test]
		public void TestNoFileDownloaded()
		{
			var inputFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			Assert.Throws(Is.TypeOf<FileNotFoundException>().And.Message.EqualTo($"Downloaded Zip file for Chief Harmonised Data does not exist. Path: {inputFile}"),
				() => chiefHarmonisedDeclarationCodeParser.DownloadAndConvertToRefCusCodeListXML(outputPath, inputFile, new DateTime(2019, 6, 14)));
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"GB\ChiefHarmonisedDeclarationCodes\");
			chiefHarmonisedDeclarationCodeParser = new ChiefHarmonisedDeclarationCodeParser();
		}
		ChiefHarmonisedDeclarationCodeParser chiefHarmonisedDeclarationCodeParser;
		Assembly assembly;
		string outputPath;
	}
}
