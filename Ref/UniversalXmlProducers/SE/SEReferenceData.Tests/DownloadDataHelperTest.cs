using System.IO;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
{
	[TestFixture]
	sealed class DownloadDataHelperTest
	{
		const string Media_type_html = "text/html";
		const string GeographicalArea_filename = "GeographicalArea";
		const string Tulltaxan_Xml_tot_response = "CargoWise.RefDbRepo.SEReferenceData.Tests.Input.Tulltaxan_Xml_tot.html";

		[Test]
		public void TestFindModifiedDateFromFilename()
		{
			var filename = "https://distr.tullverket.se/tulltaxan/xml/tot/GeographicalArea_b84e4dd9-9403-4463-89fc-80c6fbc31452_231101.xml.gz.pgp";
			var fileDate = DownloadDataHelper.FindModifiedDateFromFilename(filename);

			Assert.That(fileDate, Is.EqualTo("231101"), "filedate");
		}

		[Test]
		public void TestFindLatestTotalFilename()
		{
			MockHttp.When(ApplicationConfig.CompleteMonthlyRepositoryUrl).Respond(Media_type_html, TestHelper.ReadManifestResourceContent(Tulltaxan_Xml_tot_response));
			var filename = DownloadDataHelper.FindLatestTotalFilename(MockHttp.ToHttpClient(), GeographicalArea_filename, ApplicationConfig.CompleteMonthlyRepositoryUrl);

			Assert.That(filename, Is.EqualTo("https://distr.tullverket.se/tulltaxan/xml/tot/GeographicalArea_b84e4dd9-9403-4463-89fc-80c6fbc31452_231101.xml.gz.pgp"), "newest file is chosen if more than one");
		}

		[SetUp]
		public void Setup()
		{
			MockHttp = new MockHttpMessageHandler();
		}
		MockHttpMessageHandler MockHttp;
	}
}
