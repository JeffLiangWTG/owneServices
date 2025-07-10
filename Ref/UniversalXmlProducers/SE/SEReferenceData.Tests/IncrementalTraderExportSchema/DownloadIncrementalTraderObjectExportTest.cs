using System.IO;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
{
	[TestFixture]
	sealed class DownloadIncrementalTraderObjectExportTest
	{
		const string MEDIA_TYPE = "application/pgp-encrypted";
		const string INCREMENTAL_URL = "http://distr.tullverket.se/tulltaxan/xml/dif/IncrementalObjectTraderExport_230922.xml.gz.pgp";
		const string InputTestFilesPath = @"UniversalXmlProducers\SE\SEReferenceData.Tests\IncrementalTraderExportSchema\TestFiles\Input\";

		[Test]
		public void DownloadXml()
		{
			var filepath = Path.Combine(TestHelper.BaseSourcePath, InputTestFilesPath, "IncrementalObjectTraderExport_230922.xml.gz.pgp");
			using (var fileStream = File.OpenRead(filepath))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(INCREMENTAL_URL).Respond(MEDIA_TYPE, fileStream);

				var result = DownloadTraderObjectExport.Download<Services.IncrementalExport.export>(mockHttp.ToHttpClient(), INCREMENTAL_URL);
				Assert.That(result, Is.Not.Null);
				Assert.That(result.exportType, Is.EqualTo("IncrementalObjectTraderExport"));
				Assert.That(result.id, Is.EqualTo("46f97cb0-6e29-4314-b39b-829d043f84d2"));
				Assert.That(result.items, Has.Length.EqualTo(452));
			}
		}
	}
}
