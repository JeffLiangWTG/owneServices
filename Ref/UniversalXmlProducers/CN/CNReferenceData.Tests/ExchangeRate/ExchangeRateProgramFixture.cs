using System;
using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using CargoWise.RefDbRepo.CNReferenceData.CmdLine;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRateProgramFixture : TestBase
	{
		[Test]
		public void TestRun_TodayIsCollectionDate()
		{
			var inputHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.index.html");
			using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(inputHtml) })
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var httpHandlerMock = new Mock<IHttpHandler>();
				httpHandlerMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

				var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.CNExchangeRate_xmlexpected.xml");
				{
					var today = new DateTime(2024, 10, 16, 12, 34, 56);
					ExchangeRateProgram.Run(today, httpHandlerMock.Object);

					var outputFilePath = Path.Combine(GlobalOption.Instance.Setting.OutputFileFolderPath, "RefExchangeRateZZ_CN_20241016.xml");
					var generatedXml = File.ReadAllText(outputFilePath);
					Assert.That(generatedXml != null);
					Assert.That(generatedXml.Trim() == expectedXml.Trim());
					Assert.AreEqual("Finish", sw.ToString());
				}
			}
		}

		[Test]
		public void TestRun_TodayIsNotCollectionDate()
		{
			var inputHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.index.html");
			using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(inputHtml) })
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var httpHandlerMock = new Mock<IHttpHandler>();
				httpHandlerMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

				var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.ExchangeRate.Res.CNExchangeRate_xmlexpected.xml");
				{
					var today = new DateTime(2024, 10, 17);
					ExchangeRateProgram.Run(today, httpHandlerMock.Object);

					Assert.AreEqual("Today 2024-10-17 is not collection date (2024-10-16).", sw.ToString());
				}
			}
		}

		public override void SetUp()
		{
			defOut = Console.Out;
			base.SetUp();
		}

		public override void TearDown()
		{
			Console.SetOut(defOut);
			base.TearDown();
		}

		TextWriter defOut;
	}
}
