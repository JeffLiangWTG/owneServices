using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class CNExchangeRateParserFixture
	{
		[Test]
		public void TestExportToXml()
		{
			var inputHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.index.html");
			using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(inputHtml) })
			{
				httpHandlerMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

				var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.CNExchangeRate_xmlexpected.xml");

				var today = new DateTime(2024, 10, 16);
				var parser = new CNExchangeRateParser(SourceUrl, today);
				parser.ExportToXMLFile(dumpPath, httpHandlerMock.Object);

				var generatedXml = File.ReadAllText(dumpPath);
				Assert.That(generatedXml != null);
				Assert.That(generatedXml.Trim() == expectedXml.Trim());
			}
		}

		[Test]
		public void TestParse_ThrowExceptionIfPublishIsNotToday()
		{
			var inputHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.index.html");
			using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(inputHtml) })
			{
				httpHandlerMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

				var collectionDate = new DateTime(2024, 10, 17);
				var parser = new CNExchangeRateParser(SourceUrl, collectionDate);
				Assert.That(() => parser.ExportToXMLFile(dumpPath, httpHandlerMock.Object), Throws.Exception.Message.EqualTo("Publish Date 2024-10-16 11:18:45 is not today."));
			}
		}

		[Test]
		public void TestParse_ThrowExceptionIfNoReturnFromService()
		{
			using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) })
			{
				httpHandlerMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

				var collectionDate = new DateTime(2024, 10, 16);
				var parser = new CNExchangeRateParser(SourceUrl, collectionDate);
				Assert.That(() => parser.ExportToXMLFile(dumpPath, httpHandlerMock.Object), Throws.Exception.Message.EqualTo("Exchange rate table is not found, Not Found element."));
			}
		}

		[Test]
		public void TestParse_ThrowExceptionIfNoRateReturned()
		{
			var expectedHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.index_error.html");
			using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedHtml) })
			{
				httpHandlerMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

				var collectionDate = new DateTime(2018, 5, 8);
				var parser = new CNExchangeRateParser(SourceUrl, collectionDate);
				Assert.That(() => parser.ExportToXMLFile(dumpPath, httpHandlerMock.Object), Throws.Exception.Message.EqualTo("Could not find rates for 阿联酋迪拉姆,澳大利亚元,加拿大元,瑞士法郎,丹麦克朗,欧元,英镑,港币,印尼卢比,日元,韩国元,澳门元,林吉特,挪威克朗,新西兰元,菲律宾比索,卢布,沙特里亚尔,瑞典克朗,新加坡元,泰国铢,土耳其里拉,新台币,美元,南非兰特."));
			}
		}

		[Test]
		public void TestParse_ThrowExceptionIfFailToConnect()
		{
			using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK))
			{
				httpHandlerMock.Setup(x => x.GetAsync(It.IsAny<string>())).Throws<HttpRequestException>();

				var collectionDate = new DateTime(2018, 5, 8);
				var parser = new CNExchangeRateParser(SourceUrl, collectionDate)
				{
					RetryInterval = TimeSpan.FromMilliseconds(1)
				};
				Assert.Throws<AggregateException>(() => parser.ExportToXMLFile(dumpPath, httpHandlerMock.Object), "Exception thrown after 3 retries");
			}
		}

		[SetUp]
		public void Setup()
		{
			binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			dumpPath = Path.Combine(binPath, Guid.NewGuid() + "_dumps", "cnrate.xml");
			httpHandlerMock = new Mock<IHttpHandler>();
		}

		[TearDown]
		public void TearDown()
		{
			var dumpFolder = Path.GetDirectoryName(dumpPath);
			if (Directory.Exists(dumpFolder))
			{
				Directory.Delete(dumpFolder, true);
			}
		}

		string dumpPath;
		string binPath;
		const string SourceUrl = "https://www.boc.cn/sourcedb/whpj/index.html";
		Mock<IHttpHandler> httpHandlerMock;
	}
}
