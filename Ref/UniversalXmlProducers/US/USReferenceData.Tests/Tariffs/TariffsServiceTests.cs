using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	class TariffsServiceTests
	{
		[TestCase("July 3, 2019", 7, 3)]
		[TestCase("APRIL 23, 2019", 4, 23)]
		[TestCase("dec. 4, 2019", 12, 4)]
		[TestCase("november 18, 2019", 11, 18)]
		[TestCase("JAN. 7, 2019", 1, 7)]

		public void TestGetDateTimeFromHtml(string testSiteDate, int month, int day)
		{
			var site = new HtmlDocument();
			var sitehtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Input.AESUpdateInfo.html").Replace("{DateInfoPlaceHolder}", testSiteDate);
			site.LoadHtml(sitehtml);
			var htmlNode = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.H3 && node.InnerText.Contains(Constants.FileStructureAndUpdateInfoNodeKeywords.EXP));
			var tariffsService = new DownLoadService();
			var (successfullyParsed, dateTime) = tariffsService.GetDateTimeFromHtmlNode(htmlNode);
			Assert.That(successfullyParsed, Is.True);
			Assert.That(dateTime, Is.EqualTo(new DateTime(2019, month, day)));
		}

		[Test]
		public void ExistingFileIsDeletedBeforeDownload()
		{
			var tariffsService = new DownLoadService();
			var tempFile = Path.GetTempFileName();
			File.Create(tempFile).Close();
			Assert.That(new FileInfo(tempFile), Does.Exist);
			try
			{
				tariffsService.DownloadFile("InvalidURL", tempFile);
			}
			catch
			{
			}
			Assert.That(new FileInfo(tempFile), Does.Not.Exist);
		}
	}
}
