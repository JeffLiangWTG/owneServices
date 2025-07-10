using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.SEReferenceData.Business;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
{
	[TestFixture]
	internal abstract class XmlParserTest<TXmlObject> where TXmlObject : class
	{
		const string Media_type_plain = "text/plain";
		const string Media_type_html = "text/html";
		const string Tulltaxan_Xml_tot_response = "CargoWise.RefDbRepo.SEReferenceData.Tests.Input.Tulltaxan_Xml_tot.html";

		protected abstract string GetInputFileUrl();
		protected abstract string GetInputFileLocalPath();
		protected abstract string GetUniversalXmlOutputPath();
		protected abstract string GetNoErrorsMessage();

		protected abstract DownloadExportXml CreateXmlProducer(HttpClient client);
		protected abstract XmlParser<TXmlObject> CreateXmlParser();

		protected abstract string GetFilePrefix();

		[Test]
		public void TestDownloadAndConvertToXml()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(ApplicationConfig.CompleteMonthlyRepositoryUrl).Respond(Media_type_html, TestHelper.ReadManifestResourceContent(Tulltaxan_Xml_tot_response));
				mockHttp.When(GetInputFileUrl()).Respond(Media_type_plain, TestHelper.ReadManifestResourceContent(GetInputFileLocalPath()));
				var xmlProducer = CreateXmlProducer(mockHttp.ToHttpClient());
				var (xmlData, modified) = xmlProducer.CombinedDownloadFile<TXmlObject>(GetFilePrefix());

				var xmlParser  = CreateXmlParser();
				var parseError = xmlParser.ConvertToXMLFile(xmlData, modified, outputTempFileForTest);

				var actualUniversalXml = File.ReadAllText(outputTempFileForTest);
				var expectedUniversalXml = TestHelper.ReadManifestResourceContent(GetUniversalXmlOutputPath());

				Assert.That(parseError, Is.EqualTo(GetNoErrorsMessage()), "No errors");
				Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml), "Xml should be identical");
			}
		}

		[Test]
		public void TestParseWhenEmptyData()
		{
			TXmlObject[] xmlObjects = null;

			var tradegroupParser = CreateXmlParser();
			var parseError = tradegroupParser.ConvertToXMLFile(xmlObjects, "230901", outputTempFileForTest);
			Assert.That(parseError, Is.EqualTo("Not able to extract any data"));
		}

		[SetUp]
		public void Setup()
		{
			outputTempFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		[TearDown]
		public void TearDown()
		{
			outputTempFileForTest.DeleteTestOutput();
		}

		string outputTempFileForTest;
	}
}
