using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class CustomsOfficesXmlWriterTest
	{
		[Test]
		public void TestDownloadAndConvertToXML()
		{
			var downloadUrl = AppConfig.NACCS.CodeLists.CustomsOfficesFileDownloadUrl;
			var basePageHtmlFilePath = "CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.NACCSCodeListIndexHtml.html";
			var  basePageDownloadUrl = AppConfig.NACCS.CodeLists.BaseUrl;
			var csvStream = TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.JPReferenceData.Tests.CustomsOffices.TestFiles.Input.zeika.csv");
			httpClientMock.Setup(x => x.GetAsync(downloadUrl)).Returns(Task.FromResult(csvStream));
			var htmlContent = TestHelper.ReadManifestResourceContent(basePageHtmlFilePath);
			httpClientMock.Setup(x => x.GetWebPageAsync(basePageDownloadUrl)).Returns(Task.FromResult(htmlContent));

			var errors = CustomsOfficesXmlWriter.WriteXml(httpClientMock.Object);
			Assert.That(errors, Is.Empty);

			var expectedXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.JPReferenceData.Tests.CustomsOffices.TestFiles.Output.{RefCusCodeListFileName}");

			var actualXml = File.ReadAllText(outputFile);

			Assert.That(actualXml, Is.EqualTo(expectedXML));
		}

		[SetUp]
		public void SetUp()
		{
			outputFile = Path.Combine(AppConfig.Shared.OutputDirectory, RefCusCodeListFileName);
			httpClientMock = new Mock<IHttpClientHelper>();
		}

		string outputFile;
		Mock<IHttpClientHelper> httpClientMock;
		const string RefCusCodeListFileName = "RefCusCodeList_JP_CustomsOffices.xml";

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
