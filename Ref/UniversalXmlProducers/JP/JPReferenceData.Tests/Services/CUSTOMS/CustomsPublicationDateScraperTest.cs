using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class CustomsPublicationDateScraperTest
	{
		static CustomsPublicationDateScraperTest()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		[Test]
		public void TestTryGetPublicationDateUsingValidDownloadUrl()
		{
			var isRetrieved = CustomsPublicationDateScraper.TryGetPublicationDate(httpClientHelper.Object, baseUrl, validDownloadUrl, out var publicationDate);
			Assert.That(isRetrieved, Is.True);
			Assert.That(publicationDate, Is.EqualTo(new DateTime(2025, 1, 7)));
		}

		[Test]
		public void TestTryGetPublicationDateUsingInvalidDownloadUrl()
		{
			var ex = Assert.Throws<InvalidOperationException>(() => CustomsPublicationDateScraper.TryGetPublicationDate(httpClientHelper.Object, baseUrl, invalidDownloadUrl, out var publicationDate));
			Assert.That(ex.Message, Is.EqualTo($"Failed to find a tr element that matches //tr[td[a[@href='{invalidDownloadUrl}']]]"));
		}

		[SetUp]
		public void SetUp()
		{
			var htmlContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.JapanCustomsIndex.html", "shift_jis");
			httpClientHelper = new Mock<IHttpClientHelper>();
			httpClientHelper.Setup(x => x.GetWebPageAsync(baseUrl, "shift_jis")).Returns(Task.FromResult(htmlContent));

			stringWriter = new StringWriter();
			Console.SetError(stringWriter);
		}

		readonly string validDownloadUrl = AppConfig.Customs.CodeLists.FSBTxtFileDownloadUrl;
		readonly string invalidDownloadUrl = "ThisIsAnInvalidDownloadUrl";
		readonly string baseUrl = AppConfig.Customs.CodeLists.BaseUrl;

		StringWriter stringWriter;
		Mock<IHttpClientHelper> httpClientHelper;
	}
}
