using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	[TestFixture]
	sealed class EDILocationXMLProducerTest
	{
		const string TestInputFilesDir = "EDILocation\\INTestFiles\\Input";
		const string TestOutputFilesDir = "EDILocation\\INTestFiles\\Output";
		const string ValidInputFile = "SMTP_Mail_List_ICEGATE.html";
		const string OutputFileName = "RefEDILocationZZ_IN.xml";
		readonly string sourceUrl = AppConfig.EDILocation.Url;

		[Test]
		public void TestGenerateEDILocationXMLFile()
		{
			var sourcePage = TestHelper.ReadContentString($"{TestInputFilesDir}\\{ValidInputFile}");
			httpClientMock.Setup(x => x.GetWebPageAsync(sourceUrl)).Returns(Task.FromResult(sourcePage));

			var producer = new EDILocationXMLProducer(httpClientMock.Object);
			var errors = producer.GenerateEDILocationXMLFile(sourceUrl, outputFile);
			var expectedXML = TestHelper.ReadContentString($"{TestOutputFilesDir}\\{OutputFileName}");
			var actualXml = TestHelper.RemoveIgnoredArgsFromXml(File.ReadAllText(outputFile));
			Assert.AreEqual(expectedXML, actualXml);
		}

		[Test]
		public void TestGenerateEDILocationXMLFile_UnexpectedSourceUrl()
		{
			AssertException("", $"EDILocation Source Url ({sourceUrl}) is incorrect");
			AssertException("<!DOCTYPE html><html></html>", $"Unable to fetch table with EDILocation data from source {sourceUrl}");
		}

		void AssertException(string sourcePage, string exceptionMessage)
		{
			httpClientMock.Setup(x => x.GetWebPageAsync(sourceUrl)).Returns(Task.FromResult(sourcePage));

			var producer = new EDILocationXMLProducer(httpClientMock.Object);
			Assert.Throws(Is.TypeOf<UnhandledApplicationException>()
							.And.Message.EqualTo(exceptionMessage),
							() => producer.GenerateEDILocationXMLFile(sourceUrl, outputFile));
		}

		[Test]
		public void TestGenerateEDILocationXMLFile_InvalidEDILocation()
		{
			var sourcePage =
				@"<!DOCTYPE html>
				<html>
					<table id=""pagetable"">
						<tr></tr>
						<tr>
							<td>1</td>
							<td>ABC</td>
							<td>INAPL9<\td>
							<td>abc@xyz</td>
						</tr>
					</table>
				</html>";

			httpClientMock.Setup(x => x.GetWebPageAsync(sourceUrl)).Returns(Task.FromResult(sourcePage));

			var producer = new EDILocationXMLProducer(httpClientMock.Object);
			var errors = producer.GenerateEDILocationXMLFile(sourceUrl, outputFile);
			Assert.That(errors, Does.StartWith("Invalid location code found! Skipped record for INAPL9"));
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"IN\TestFiles\EDILocation");
			outputFile = Path.Combine(outputPath, OutputFileName);
			httpClientMock = new Mock<IHttpClientHelper>();
		}

		[TearDown]
		public void Teardown()
		{
			if (Directory.Exists(outputPath))
			{
				Directory.Delete(outputPath, true);
			}
		}
		string outputPath;
		string outputFile;
		Mock<IHttpClientHelper> httpClientMock;
	}
}
