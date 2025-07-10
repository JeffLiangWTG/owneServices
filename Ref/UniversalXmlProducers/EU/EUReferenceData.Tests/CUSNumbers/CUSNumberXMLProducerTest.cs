using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Tests
{
	sealed class CUSNumberXMLProducerTest
	{
		[Test]
		public void DownloadAndCreateCUSNumbersXML()
		{
			var errors = producer.DownloadAndConvertToCUSNumberXML(outputPath, 1);
			var expectedXml1 = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Output.{BatchOutputFile1}");
			var expectedXml2 = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Output.{BatchOutputFile2}");
			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.Empty, "No Errors");
				Assert.That(ReadAllText(BatchOutputFile1), Is.EqualTo(expectedXml1), "Batch 1 Contents");
				Assert.That(ReadAllText(BatchOutputFile2), Is.EqualTo(expectedXml2), "Batch 2 Contents");
			});
		}

		[Test]
		public void DownloadAndCreateCUSNumbersXMLOneBatch()
		{
			const string outputFile = "RefCusCodeListZZ_CUSNumbers_20210802_Batch00001.xml";
			var errors = producer.DownloadAndConvertToCUSNumberXML(outputPath, 999);
			var expectedXml = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Output.OneBatch_{outputFile}");
			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.Empty, "No Errors");
				Assert.That(ReadAllText(outputFile), Is.EqualTo(expectedXml), "Batch 1 Contents");
			});
		}

		[Test]
		public void DownloadAndCreateCUSNumbersXMLErrorsOnPage()
		{
			//There are a lot of errors put I don't think there is enough information to determine which page the error came from. The time taken to run all the files
			//we need the details to go exactly to the page with a problem

			var errorPage = BaseResourcePath + "errorpage.html";
			httpClientMock.Setup(x => x.GetWebPageAsync(string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CUSNumberListUrlPattern, 25))).Returns(Task.FromResult(errorPage));

			var errors = producer.DownloadAndConvertToCUSNumberXML(outputPath, 1);
			var expectedXml1 = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Output.{BatchOutputFile1}");

			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.EqualTo("error getting page https://ec.europa.eu/taxation_customs/dds2/ecics/chemicalsubstance_list.jsp?Lang=en&offset=25&LangNm=en&sortOrder=1 for index 25: No table found\r\n"), "1 Error shown");
				Assert.That(ReadAllText(BatchOutputFile1), Is.EqualTo(expectedXml1), "Batch 1 Contents");
				Assert.That(Path.Combine(outputPath, BatchOutputFile2), Does.Not.Exist, "Batch 2 does not exist");
			});
		}

		[Test]
		public void DownloadAndCreateCUSNumbersXMLExceptionThrown()
		{
			var dateTime = DateTime.UtcNow;
			var indexPage = $"{BaseResourcePath}errorpage.html";
			var expectedHtml = TestHelper.ReadManifestResourceContentAsString(indexPage);

			httpClientMock.Setup(x => x.GetWebPageAsync(ApplicationConfig.Instance.CUSNumberEntryPointUrl)).Returns(Task.FromResult(expectedHtml));
			var producer = new CUSNumberXMLProducerForTest(httpClientMock.Object)
			{
				TimeStamp = dateTime
			};

			var dumpFileName = $"RefCusCodeListZZ_DownloadAndConvertToCUSNumberXML_{dateTime:yyyyMMdd_HHmmssfffff}_IndexPage_Error.htm";
			var path = Path.Combine(outputPath, "DumpFiles", dumpFileName);

			Assert.Multiple(() =>
			{
				var exception = Assert.Throws<CUSNumbersException>(() => producer.DownloadAndConvertToCUSNumberXML(outputPath, 1));
				Assert.That(exception.Message, Does.Contain(dumpFileName));
				Assert.That(path, Does.Exist, "Index Page dump file exists");
				Assert.That(File.ReadAllText(path), Is.EqualTo(expectedHtml), "Index page and dump file contents match");
			});
		}

		[Test]
		public void DownloadAndCreateCUSNumbersXMLExceptionThrown_CleanUp()
		{
			var indexPage = $"{BaseResourcePath}errorpage.html";
			var expectedHtml = TestHelper.ReadManifestResourceContentAsString(indexPage);

			httpClientMock.Setup(x => x.GetWebPageAsync(ApplicationConfig.Instance.CUSNumberEntryPointUrl)).Returns(Task.FromResult(expectedHtml));
			var producer = new CUSNumberXMLProducerForTest(httpClientMock.Object)
			{
				TimeStamp = DateTime.UtcNow
			};

			var dirInfo = Directory.CreateDirectory(Path.Combine(outputPath, "DumpFiles"));
			for (int i = 0; i <= 1; i++)
			{
				var timeStamp = DateTime.UtcNow;
				var path = Path.Combine(dirInfo.FullName, $"RefCusCodeListZZ_DownloadAndConvertToCUSNumberXML_{timeStamp:yyyyMMdd_HHmmssfffff}_IndexPage_Error.htm");
				File.WriteAllText(path, "OLD DUMP FILE");
				var file = new FileInfo(path);
				if (file.Exists)
				{
					file.CreationTime = timeStamp.AddDays(-15);
				}
			}
			Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(2), "Pre-requisite: 2 old dump files");

			var dumpFile = Path.Combine(outputPath, "DumpFiles", $"RefCusCodeListZZ_DownloadAndConvertToCUSNumberXML_{producer.TimeStamp:yyyyMMdd_HHmmssfffff}_IndexPage_Error.htm");
			Assert.Multiple(() =>
			{
				var exception = Assert.Throws<CUSNumbersException>(() => producer.DownloadAndConvertToCUSNumberXML(outputPath, 1));
				Assert.That(dumpFile, Does.Exist, "Index Page dump file exists");
				Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(1), "Only 1 dump file should exist");
			});
		}

		[SetUp]
		public void SetUp()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\CUSNumbers");
			httpClientMock = new Mock<IHttpClientHelper>();
			SetupHttpMappings(httpClientMock);
			producer = new CUSNumberXMLProducer(httpClientMock.Object);
		}

		string outputPath;
		Mock<IHttpClientHelper> httpClientMock;
		CUSNumberXMLProducer producer;

		[TearDown]
		public void Teardown()
		{
			if (Directory.Exists(outputPath))
			{
				Directory.Delete(outputPath, true);
			}
		}

		void SetupHttpMappings(Mock<IHttpClientHelper> httpClientMock)
		{
			Dictionary<string, string> mappings = new Dictionary<string, string>()
			{
				{ ApplicationConfig.Instance.CUSNumberEntryPointUrl, BaseResourcePath + "Index.html" },
				{ string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CUSNumberListUrlPattern, 0), BaseResourcePath + "page1.html" },
				{ string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CUSNumberListUrlPattern, 25), BaseResourcePath + "page2.html" },
				{ string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CUSNumberListUrlPattern, 50), BaseResourcePath + "emptypage.html" },
			};
			foreach (var mapping in mappings)
			{
				var pageContent = TestHelper.ReadManifestResourceContentAsString(mapping.Value);
				httpClientMock.Setup(x => x.GetWebPageAsync(mapping.Key)).Returns(Task.FromResult(pageContent));
			}

			Dictionary<string, string> soapmappings = new Dictionary<string, string>()
			{
				{ BaseResourcePath + "Soap_1_1_Request.xml", BaseResourcePath + "Soap_1_1.xml" },
				{ BaseResourcePath + "Soap_1_2_Request.xml", BaseResourcePath + "Soap_1_2.xml" },
				{ BaseResourcePath + "Soap_1_3_Request.xml", BaseResourcePath + "Soap_1_3.xml" },
				{ BaseResourcePath + "Soap_2_1_Request.xml", BaseResourcePath + "Soap_2_1.xml" },
				{ BaseResourcePath + "Soap_2_2_Request.xml", BaseResourcePath + "Soap_2_2.xml" },
				{ BaseResourcePath + "Soap_2_3_Request.xml", BaseResourcePath + "Soap_2_3.xml" },
				{ BaseResourcePath + "Soap_3_1_Request.xml", BaseResourcePath + "Soap_3_1.xml" },
				{ BaseResourcePath + "Soap_3_2_Request.xml", BaseResourcePath + "Soap_3_2.xml" },
				{ BaseResourcePath + "Soap_3_3_Request.xml", BaseResourcePath + "Soap_3_3.xml" },
			};

			foreach (var mapping in soapmappings)
			{
				var requestContent = TestHelper.ReadManifestResourceContentAsString(mapping.Key);
				var resultContent = TestHelper.ReadManifestResourceContentAsString(mapping.Value);
				httpClientMock.Setup(x => x.PostAndReadAsAsyncString("https://ec.europa.eu/taxation_customs/dds2/ecics/cs/services/chemical-substance", requestContent, "text/xml")).Returns(Task.FromResult(resultContent));
			}

			Dictionary<string, string> zipFileMappings = new Dictionary<string, string>()
			{
				{ "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_CUSCode.zip", BaseResourcePath + "RD_NCTS-P5_CUSCode.zip" },
			};

			foreach (var mapping in zipFileMappings)
			{
				var zipStream = TestHelper.ReadManifestResourceContentAsStream(mapping.Value);
				httpClientMock.Setup(x => x.GetAsync(mapping.Key)).Returns(Task.FromResult(zipStream));
			}
		}

		string ReadAllText(string fileName)
		{
			var path = Path.Combine(outputPath, fileName);
			return File.ReadAllText(path);
		}

		const string BatchOutputFile1 = "RefCusCodeListZZ_CUSNumbers_20210802_Batch00001.xml";
		const string BatchOutputFile2 = "RefCusCodeListZZ_CUSNumbers_20210802_Batch00002.xml";
		const string BaseResourcePath = "CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Input.";

		sealed class CUSNumberXMLProducerForTest : CUSNumberXMLProducer
		{
			public CUSNumberXMLProducerForTest(IHttpClientHelper httpClientHelper) : base(httpClientHelper)
			{
			}

			protected override DateTime ProcessInvalidDataTimeStamp => TimeStamp;
			public DateTime TimeStamp;
		}
	}
}
