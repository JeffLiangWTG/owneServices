using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.VNReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.VNReferenceData.Tests
{
	/// <summary>
	/// Main business logic test
	/// </summary>
	[TestFixture]
	public class CustomsOfficeParserFixture
	{
		const string MetadataDownloadUrl =
			"http://10.224.128.185:8080/resources/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang ma don vi HQ 2025 14052025.xlsx";

		private const string ActualDownloadUrl =
			"https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang%20ma%20don%20vi%20HQ%202025%2014052025.xlsx";

		const string MetadataUpdatedDate = "May 14, 2025";

		string downloadFilePath;
		string actualXmlFilePath;
		string expectedXmlFilePath;
		Mock<IHttpHandler> httpHandlerMock;

		[SetUp]
		public void SetUp()
		{
			var testDir = TestContext.CurrentContext.TestDirectory;
			downloadFilePath = Path.Join(testDir, "VNTestFiles", "CustomsOffice", "Input_RealFileFromVNCustoms.xlsx");
			actualXmlFilePath = Path.GetTempFileName();
			expectedXmlFilePath =
				Path.Join(testDir, "VNTestFiles", "CustomsOffice", "Output_RealFileFromVNCustoms.xml");

			httpHandlerMock = new Mock<IHttpHandler>();
			httpHandlerMock.Setup(h => h.DownloadCodeListMetadata())
				.ReturnsAsync(new CodeListMetadata
				{
					DataItems = new List<CodeListMetadataItem>
					{
						new() { Id = 1 },
						new()
						{
							Id = ApplicationConfig.CustomsOfficeMetadataId,
							FileDownloadUrl = MetadataDownloadUrl,
							UpdatedDate = MetadataUpdatedDate
						}
					}
				});
		}

		[Test]
		public async Task TestSuccess()
		{
			var collector = new CustomsOfficeParser(
				downloadFilePath, actualXmlFilePath, httpHandlerMock.Object, DateTime.UtcNow);
			await collector.DownloadAndExportToXmlFile();

			httpHandlerMock.Verify(h => h.DownloadCodeListMetadata(), Times.Once);
			httpHandlerMock.Verify(h => h.DownloadToFile(
				It.Is<Uri>(u => u.AbsoluteUri == ActualDownloadUrl),
				downloadFilePath
			), Times.Once);

			var contentEqual = TestHelper.AreXmlFilesEqual(
				actualXmlFilePath, expectedXmlFilePath, new[] { "AppName", "AppProgramArgs" });
			Assert.That(contentEqual, "The output xml doesn't match the expected xml");
		}
	}
}
