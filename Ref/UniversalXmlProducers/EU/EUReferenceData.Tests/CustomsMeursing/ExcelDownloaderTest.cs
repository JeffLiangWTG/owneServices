using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Services.Circabc;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Tests
{
	[TestFixture]
	class ExcelDownloaderTest
	{
		[Test]
		public void AgricultureDataInfo()
		{
			var data = ExcelDownloader.GetWebFileInfo(service, ApplicationConfig.Instance.CirabcDutiesAndRelatedRootId, ApplicationConfig.Instance.CircabcDownloadUrlTemplate, @"[Aa]dditional[\s]+[Aa]gricultural[\s]+[Dd]uties[\s-]+[Mm]eursing[\s.]+[Xx]lsx", "2022");
			Assert.Multiple(() =>
			{
				var expectedDateModified = DateTime.Parse("2021-12-30T17:05Z", CultureInfo.InvariantCulture);
				Assert.AreEqual("Additional agricultural duties - Meursing.xlsx", data.FileName);
				Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CircabcDownloadUrlTemplate, "709f7afa-c1fd-4341-b2bf-de9067fd7dbb"), data.DownloadPath);
				Assert.AreEqual("01 - January", data.Month);
				Assert.AreEqual("2022", data.Year);
				Assert.AreEqual(expectedDateModified, data.ModifiedDate);
			});
		}

		[Test]
		public void GeographicalAreasInfo()
		{
			var data = ExcelDownloader.GetWebFileInfo(service, ApplicationConfig.Instance.CirabcDutiesAndRelatedRootId, ApplicationConfig.Instance.CircabcDownloadUrlTemplate, @"[Gg]eographical[\s]+[Aa]reas[\s]+[Cc]omposition[\s.]+[Xx][Ll][Ss][Xx]", "2022");
			Assert.Multiple(() =>
			{
				var expectedDateModified = DateTime.Parse("2021-12-30T17:02Z", CultureInfo.InvariantCulture);
				Assert.AreEqual("Geographical areas composition.xlsx", data.FileName);
				Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CircabcDownloadUrlTemplate, "3e8a978b-5db2-4518-95c0-c13f4fe92ab0"), data.DownloadPath);
				Assert.AreEqual("01 - January", data.Month);
				Assert.AreEqual("2022", data.Year);
				Assert.AreEqual(expectedDateModified, data.ModifiedDate);
			});
		}

		[SetUp]
		public void Setup()
		{
			httpClientMock = new Mock<IHttpClientHelper>();
			SetupHttpMappings();

			service = new CircabcService(httpClientMock.Object, ApplicationConfig.Instance.CircabcServiceUrlTemplate);
		}

		Mock<IHttpClientHelper> httpClientMock;
		CircabcService service;

		void SetupHttpMappings()
		{
			Dictionary<string, string> mappings = new Dictionary<string, string>()
			{
				{ string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CircabcServiceUrlTemplate, ApplicationConfig.Instance.CirabcDutiesAndRelatedRootId), BaseResourcePath + "Index.json" },
				{ string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CircabcServiceUrlTemplate, "c2f7d247-efae-43ee-ab12-b8b468479bc7"), BaseResourcePath + "2022.json" },
				{ string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CircabcServiceUrlTemplate, "fc9ddf3a-cc89-4450-a50b-fb1efa3ac604"), BaseResourcePath + "01 - January.json" },
			};
			foreach (var mapping in mappings)
			{
				var pageContent = TestHelper.ReadManifestResourceContentAsString(mapping.Value);
				httpClientMock.Setup(x => x.GetWebPageAsync(mapping.Key)).Returns(Task.FromResult(pageContent));
			}
		}
		const string BaseResourcePath = "CargoWise.RefDbRepo.EUReferenceData.Tests.CustomsMeursing.TestFiles.";
	}
}
