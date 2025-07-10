using CargoWise.RefDbRepo.AUReferenceData.Business;
using System;
using NUnit.Framework;
using System.IO;
using System.Globalization;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISProducerCodesParserTest : CommonCMRDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISProducerCodes";

		protected override string TextFileName => "AQSPRDCR-P1-EDMAIN-2304120141.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISProducerCodes_{0}.xml";

		DateTime PublishedDate => new DateTime(2023, 04, 12, 01, 41, 00);

		ICMRDataParser Parser => new AQISProducerCodesParser();

		[Test]
		public void TestAQISProducerCodesOutputFilesByLeadingCountryCodes()
		{
			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", TestFileFolderName);
			using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, TextFileName)))
			using (var txtReader = new StreamReader(txtStream))
			{
				var outputFolderPath = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "TestFiles");
				Directory.CreateDirectory(outputFolderPath);

				Parser.Parse(txtReader.ReadToEnd(), PublishedDate, outputFolderPath);
				Assert.Multiple(() =>
				{
					for (var countryCode = 'A'; countryCode <= 'Z'; countryCode++)
					{
						AssertFileContentEqual(countryCode);
					}
				});

				void AssertFileContentEqual(char countryCode)
				{
					var outputFileName = string.Format(CultureInfo.InvariantCulture, XMLFileName, countryCode);
					var outputFilePath = Path.Combine(outputFolderPath, outputFileName);
					if (!File.Exists(outputFilePath))
					{
						Assert.Fail($"Output file for country code {countryCode} does not exist at {outputFilePath}");
						return;
					}

					using (var expectedXmlStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, outputFileName)))
					using (var expectedXmlReader = new StreamReader(expectedXmlStream))
					using (var actualXmlStream = new FileStream(outputFilePath, FileMode.Open))
					using (var actualXmlReader = new StreamReader(actualXmlStream))
					{
						Assert.That(actualXmlReader.ReadToEnd(), Is.EqualTo(expectedXmlReader.ReadToEnd()), $"{outputFileName} content is not equal to expected output");
					}

					File.Delete(outputFilePath);
				}
			}
		}
	}
}
