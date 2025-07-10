using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class TariffParserTest
	{
		readonly string inputFolderPath = @"TestFiles";
		readonly string expectedOutputFilePathExport = @"TestFiles\Expected_JP_ExportTariff.xml";
		readonly string actualOutputFilePathExport = "JPExportTariff.xml";
		readonly string inputTahourFilePath = @"TestFiles\tahour.csv";
		readonly string inputBoukanFilePath = @"TestFiles\boukan-e1.csv";

		[Test]
		public void TestParseExportTariffData()
		{
			var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedFilePath = Path.Combine(dirPath, expectedOutputFilePathExport);
			var actualFilePath = Path.Combine(AppConfig.Shared.OutputDirectory, actualOutputFilePathExport);
			var expectedXmlAsString = File.ReadAllText(expectedFilePath);
			var mockHttpClientHelper = new Mock<IHttpClientHelper>();

			using (var mockTahourFileStream = new FileStream(inputTahourFilePath, FileMode.Open))
			using (var mockBoukanFileStream = new FileStream(inputBoukanFilePath, FileMode.Open))
			{
				mockHttpClientHelper.Setup(x => x.GetAsync(AppConfig.NACCS.CodeLists.OtherLawCodeCsvFileDownloadUrl)).Returns(Task.FromResult<Stream>(mockTahourFileStream));
				mockHttpClientHelper.Setup(x => x.GetAsync(AppConfig.NACCS.CodeLists.ExportTradeControlOrdinanceAppendixCsvFileDownloadUrl)).Returns(Task.FromResult<Stream>(mockBoukanFileStream));
				var httpClientHelper = mockHttpClientHelper.Object;
				try
				{
					var parser = new TariffParser(httpClientHelper, new DateTime(2024, 1, 1), false);
					parser.Parse(inputFolderPath);
					var actualXmlAsString = File.ReadAllText(actualFilePath);

					Assert.That(actualXmlAsString, Is.EqualTo(expectedXmlAsString));
				}
				finally
				{
					if (File.Exists(actualFilePath))
					{
						File.Delete(actualFilePath);
					}
				}
			}
		}
	}
}
