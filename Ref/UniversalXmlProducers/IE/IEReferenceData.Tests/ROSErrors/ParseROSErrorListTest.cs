using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Business;
using CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Tests
{
	class ParseROSErrorListTest
	{
		[Test]
		public void ROSErrorsXml()
		{
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ROSErrors\TestFiles\Input\error-spec1.txt");
			var exchangeRates = DownloadROSErrorList.Download(inputFilePath);
			parser.ConvertToXmlFile((Dictionary<string, string>)exchangeRates.ExtractedErrorList, dateTimeProviderMock.Object.CurrentLocalDate, outputPath);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IEReferenceData.Tests.ROSErrors.TestFiles.Output.RefROSErrorsListZZ_IE.xml");
			var actualXml = File.ReadAllText(outputFile);
			Assert.That(actualXml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void InvaildErrorCode()
		{
			var errorWithInvaildCode = new Dictionary<string, string>
			{
				{ "invalidErrorCode", "" }
			};
			var errors = parser.ConvertToXmlFile(errorWithInvaildCode, DateTime.Today, outputPath);

			var expectedError = @"Unable to import record due to empty Code or Description. DETAILS:
Code: invalidErrorCode
Description: EMPTY
";
			Assert.That(errors.Contains(expectedError));
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"IE\ROSErrors\TestFiles\Output");
			outputFile = Path.Combine(outputPath, "RefCusCodeListZZ_IE_IEROS.xml");
			downloader = new DownloadROSErrorList();
			parser = new ROSErrorsListProducer();
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 1, 13));
		}
		Assembly assembly;
		string outputPath;
		string outputFile;
		DownloadROSErrorList downloader;
		ROSErrorsListProducer parser;
		Mock<IDateTimeProvider> dateTimeProviderMock;
	}
}
