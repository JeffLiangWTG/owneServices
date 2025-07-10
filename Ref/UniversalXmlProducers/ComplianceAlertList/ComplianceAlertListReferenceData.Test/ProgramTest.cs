using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.ComplianceAlertListReferenceData.CmdLine;
using CargoWise.RefDbRepo.ComplianceCommodityAlertListReferenceData.Test;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Test
{
	[TestFixture]
	public class ProgramTest
	{
		string OutputFile => Path.Combine(OutputPath, "RefComplianceCommodityAlertList.xml");

		string OutputPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");

		[Test]
		public void GetRefComplianceCommodityAlertListTest()
		{
			var complianceAlertList = Program.GetRefComplianceCommodityAlertList();

			Assert.IsNotNull(complianceAlertList);
			Assert.AreEqual(1, complianceAlertList.Count());
			Assert.AreEqual("CA", complianceAlertList.First().CountryCode);
			Assert.AreEqual(2, complianceAlertList.First().ExportAlerts.Count);
			Assert.AreEqual(2, complianceAlertList.First().ImportAlerts.Count);
		}

		[Test]
		public void TestProduceXML_Success_DoesNotThrowAnyException()
		{
			Assert.DoesNotThrow(() => Program.ProduceXml(Array.Empty<string>()));
			Assert.IsTrue(File.Exists(OutputFile));
		}

		[Test]
		public void TestProduceXML_Failure_EmptyPath_ThrowsArgumentException()
		{
			AppConfigurationProvider.AppConfiguration.OutputPath = string.Empty;

			Assert.That(() => Program.ProduceXml(Array.Empty<string>()), Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void TestExecute_ValidSourceInput_ReturnStatusSuccess()
		{
			Assert.DoesNotThrow(() => Program.ProduceXml(Array.Empty<string>()));
			Assert.IsTrue(File.Exists(OutputFile));
			var regexToRemovePublicationTime = new Regex("<PublicationTime>.+</PublicationTime>");
			var expectedDoc = new XmlDocument();
			expectedDoc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "ComplianceAlertListParserTestExpected.xml"));
			var resultDoc = new XmlDocument();
			resultDoc.Load(OutputFile);
			var expectedXml = regexToRemovePublicationTime.Replace(resultDoc.InnerXml, "<PublicationTime></PublicationTime>");
			Assert.AreEqual(expectedDoc.InnerXml.Replace("<PublicationTime>2025-01-01T00:00:00</PublicationTime>", "<PublicationTime></PublicationTime>"), expectedXml);
		}

		[Test]
		public void TestExecute_OnProduceXmlError_ThrowsExceptionAndStatusFailure()
		{
			AppConfigurationProvider.AppConfiguration.OutputPath = string.Empty;
			var exceptionMessage = "";
			try
			{
				Program.ProduceXml(Array.Empty<string>());
			}
			catch (ArgumentException ex)
			{
				exceptionMessage = ex.Message;
			}
			StringAssert.Contains("The value cannot be an empty string. (Parameter 'path')",
				exceptionMessage);
		}

		string rawBaseUrl;
		string rawOutputPath;

		[SetUp]
		public void SetUp()
		{
			rawBaseUrl = AppConfigurationProvider.AppConfiguration.BWBaseUrl;
			rawOutputPath = AppConfigurationProvider.AppConfiguration.OutputPath;

			var expectedResultList = ComplianceAlertListParserTestFixture.TestSourceList();
			var baseUrl = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";
			AppConfigurationProvider.AppConfiguration.BWBaseUrl = baseUrl;
			AppConfigurationProvider.AppConfiguration.OutputPath = OutputPath;
			httpService = GetHttpResponseService(new Uri(baseUrl), JsonConvert.SerializeObject(expectedResultList), 200);
			httpService.Start();
		}

		[TearDown]
		public void TearDown()
		{
			AppConfigurationProvider.AppConfiguration.BWBaseUrl = rawBaseUrl;
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
