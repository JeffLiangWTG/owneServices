using System.IO;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class FSBXmlWriterTest
	{
		static FSBXmlWriterTest()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		[Test]
		public void TestDownloadAndConvertToXML()
		{
			var downloadUrl = AppConfig.Customs.CodeLists.FSBTxtFileDownloadUrl;
			var basePageHtmlFilePath = "CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.JapanCustomsIndex.html";
			var  basePageDownloadUrl = AppConfig.Customs.CodeLists.BaseUrl;
			var txtStream = TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.shidashishimukelist.txt");
			httpClientMock.Setup(x => x.GetAsync(downloadUrl)).Returns(Task.FromResult(txtStream));
			var htmlContent = TestHelper.ReadManifestResourceContent(basePageHtmlFilePath, "shift_jis");
			httpClientMock.Setup(x => x.GetWebPageAsync(basePageDownloadUrl, "shift_jis")).Returns(Task.FromResult(htmlContent));
			var fsbXmlWriter = new FSBXmlWriter();
			fsbXmlWriter.WriteXml(httpClientMock.Object);

			var expectedXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.{ExpectedRefCusCodeListFileName}");

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
		const string RefCusCodeListFileName = "RefCusCodeList_JP_FSBCode.xml";
		const string ExpectedRefCusCodeListFileName = "ExpectedRefCusCodeList_JP_FSBCode.xml";

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
