using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Tests
{
	[TestFixture]
	class OfficeCodeXMLProducerTest
	{
		[Test]
		public void DownloadAndCreateRefCusCodeListXML()
		{
			var downloadUrl = "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/COL-Generic-20220520.zip";
			var zipStream = TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.COL-Generic-20201222.zip");
			var htmlContent = TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.DownloadCOLAndRDMessages.html");
			httpClientMock.Setup(x => x.GetWebPageAsync(ApplicationConfig.Instance.DownloadPageUrl)).Returns(Task.FromResult(htmlContent));
			httpClientMock.Setup(x => x.GetAsync(downloadUrl)).Returns(Task.FromResult(zipStream));
			var errors = OfficeCodeXMLProducer.DownloadAndConvertToRefCusCodeListXML(httpClientMock.Object, outputPath);
			var expectedXML = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Output.{RefCusCodeListFileName}");
			Assert.That(errors, Is.Empty);
			var actualXml = File.ReadAllText(outputFile);
			Assert.That(actualXml, Is.EqualTo(expectedXML));
		}

		[SetUp]
		public void SetUp()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\OfficeCodes");
			outputFile = Path.Combine(outputPath, RefCusCodeListFileName);
			httpClientMock = new Mock<IHttpClientHelper>();
		}
		string outputPath;
		string outputFile;
		Mock<IHttpClientHelper> httpClientMock;
		const string RefCusCodeListFileName = "RefCusCodeListZZ_EU_OfficeCodes_20201222.xml";

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}
	}
}
