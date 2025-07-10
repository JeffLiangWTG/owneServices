using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class NaccsPublicationDateScraperTest
	{
		[Test]
		public void TestTryGetPublicationDateUsingValidDownloadUrl()
		{
			var isRetrieved = NaccsPublicationDateScraper.TryGetPublicationDate(httpClientHelper.Object, baseUrl, validDownloadUrl, out var publicationDate);
			Assert.That(isRetrieved, Is.True);
			Assert.That(publicationDate, Is.EqualTo(new DateTime(2021, 1, 4)));
		}

		[Test]
		public void TestTryGetPublicationDateUsingInvalidDownloadUrl()
		{
			var ex = Assert.Throws<InvalidOperationException>(() => NaccsPublicationDateScraper.TryGetPublicationDate(httpClientHelper.Object, baseUrl, invalidDownloadUrl, out var publicationDate));
			Assert.That(ex.Message, Is.EqualTo($"Failed to find a tr element that matches //tr[td[a[@href='{invalidDownloadUrl}']]]"));
		}

		[SetUp]
		public void SetUp()
		{
			var htmlContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.NACCSCodeListIndexHtml.html");
			httpClientHelper = new Mock<IHttpClientHelper>();
			httpClientHelper.Setup(x => x.GetWebPageAsync(baseUrl)).Returns(Task.FromResult(htmlContent));
			stringWriter = new StringWriter();
			Console.SetError(stringWriter);
		}

		readonly string validDownloadUrl = AppConfig.NACCS.CodeLists.ExportApprovalCertificateCsvFileDownloadUrl;
		readonly string invalidDownloadUrl = "ThisIsAnInvalidDownloadUrl";
		readonly string baseUrl = AppConfig.NACCS.CodeLists.BaseUrl;

		StringWriter stringWriter;
		Mock<IHttpClientHelper> httpClientHelper;
	}
}
