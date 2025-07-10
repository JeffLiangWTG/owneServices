using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.ComplianceListReferenceData.Business;
using CargoWise.RefDbRepo.ComplianceListReferenceData.CmdLine;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Test
{
	[TestFixture]
	public class ProgramTest
	{
		string OutputFile => Path.Combine(OutputPath, "RefComplianceList.xml");

		string OutputPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");

		[Test]
		public async Task GetRefComplianceListTest()
		{
			var mockTokenProvider = new Mock<ITokenProvider>();
			mockTokenProvider.Setup(tp => tp.GetAuthorizationTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("DummyValue");
			var refComplianceList = await Program.GetRefComplianceListAsync(mockTokenProvider.Object);

			Assert.IsNotNull(refComplianceList);
			Assert.AreEqual(3, refComplianceList.Count());
		}

		[Test]
		public void TestProduceXML_Success_DoesNotThrowAnyException()
		{
			var mockTokenProvider = new Mock<ITokenProvider>();
			mockTokenProvider.Setup(tp => tp.GetAuthorizationTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("DummyValue");

			Assert.DoesNotThrowAsync(() => Program.ProduceXmlAsync(Array.Empty<string>(), mockTokenProvider.Object));
			Assert.IsTrue(File.Exists(OutputFile));
		}

		[Test]
		public void TestProduceXML_Failure_EmptyPath_ThrowsArgumentException()
		{
			AppConfigurationProvider.AppConfiguration.OutputPath = string.Empty;
			var mockTokenProvider = new Mock<ITokenProvider>();
			mockTokenProvider.Setup(tp => tp.GetAuthorizationTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("DummyValue");
			Assert.That(() => Program.ProduceXmlAsync(Array.Empty<string>(), mockTokenProvider.Object), Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void TestExecute_ValidSourceInput_ReturnStatusSuccess()
		{
			var mockTokenProvider = new Mock<ITokenProvider>();
			mockTokenProvider.Setup(tp => tp.GetAuthorizationTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("DummyValue");
			Assert.DoesNotThrowAsync(() => Program.ProduceXmlAsync(Array.Empty<string>(), mockTokenProvider.Object));

			Assert.IsTrue(File.Exists(OutputFile));

			var regexToRemovePublicationTime = new Regex("<PublicationTime>.+</PublicationTime>");
			var expectedDoc = new XmlDocument();
			expectedDoc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "ComplianceListParserTestExpected.xml"));
			var resultDoc = new XmlDocument();
			resultDoc.Load(OutputFile);
			var expectedXml = regexToRemovePublicationTime.Replace(resultDoc.InnerXml, "<PublicationTime></PublicationTime>");

			Assert.AreEqual(expectedDoc.InnerXml.Replace("<PublicationTime>2020-01-01T00:00:00</PublicationTime>", "<PublicationTime></PublicationTime>"), expectedXml);
		}

		[Test]
		public async Task TestExecute_OnProduceXmlError_ThrowsExceptionAndStatusFailureAsync()
		{
			var mockTokenProvider = new Mock<ITokenProvider>();
			mockTokenProvider.Setup(tp => tp.GetAuthorizationTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("DummyValue");
			AppConfigurationProvider.AppConfiguration.OutputPath = string.Empty;
			var exceptionMessage = "";
			try
			{
				await Program.ProduceXmlAsync(Array.Empty<string>(), mockTokenProvider.Object);
			}
			catch (ArgumentException ex)
			{
				exceptionMessage = ex.Message;
			}
			StringAssert.Contains("The value cannot be an empty string. (Parameter 'path')",
				exceptionMessage);
		}

		[Test]
		public async Task TestGetRefComplianceList_WhenValidAuthTokenIsNotPassed_ShouldFail()
		{
			var mockTokenProvider = new Mock<ITokenProvider>();
			mockTokenProvider.Setup(tp => tp.GetAuthorizationTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("NotValidToken");
			var exceptionMessage = "";
			try
			{
				await Program.GetRefComplianceListAsync(mockTokenProvider.Object);
			}
			catch (AggregateException ex)
			{
				exceptionMessage = ex.Message;
			}
			StringAssert.Contains("Response status code does not indicate success: 401 (Unauthorized).",
				exceptionMessage);
		}

		string rawBaseUrl;
		string rawOutputPath;

		[SetUp]
		public void SetUp()
		{
			rawBaseUrl = AppConfigurationProvider.AppConfiguration.DpsWebServiceBaseUrl;
			rawOutputPath = AppConfigurationProvider.AppConfiguration.OutputPath;

			var expectedResultList = ComplianceListParserTestFixture.TestSourceList();
			var baseUrl = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";
			AppConfigurationProvider.AppConfiguration.DpsWebServiceBaseUrl = baseUrl;
			AppConfigurationProvider.AppConfiguration.OutputPath = OutputPath;
			httpService = GetHttpResponseService(new Uri(baseUrl), JsonConvert.SerializeObject(expectedResultList), 200);
			httpService.ExpectedBearerToken = "DummyValue";
			httpService.Start();
		}

		[TearDown]
		public void TearDown()
		{
			AppConfigurationProvider.AppConfiguration.DpsWebServiceBaseUrl = rawBaseUrl;
			AppConfigurationProvider.AppConfiguration.OutputPath = rawOutputPath;

			if (httpService.IsStarted)
			{
				httpService.Stop();
				httpService.Dispose();
			}
			if (File.Exists(OutputFile))
			{
				try
				{
					File.Delete(OutputFile);
				}
				catch
				{
					// Ignore
				}
			}
		}

		HttpServiceForTest httpService;

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response, int statusCode)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "GET" },
				Processor = (uri, request) => new Tuple<int, string>(statusCode, response),
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}
	}
}