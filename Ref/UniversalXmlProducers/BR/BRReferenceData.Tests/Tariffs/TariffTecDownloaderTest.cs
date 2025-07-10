using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class TariffTecDownloaderTest
	{
		[Test]
		public void TestTecXlsDownload()
		{
			var streamHomePage = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.Camex_Tec.camex_tec_home.txt");
			var streamXlsx = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.Camex_Tec.TEC_20210331.xlsx");
			var expectedStreamXlsx = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.Camex_Tec.TEC_20210331.xlsx");
			using (var mock = new MockHttpMessageHandler())
			{
				mock.When(ConfigurationProvider.Configuration["URL_TEC"]).Respond("Application/file", streamHomePage);
				mock.When(ConfigurationProvider.Configuration["BASE_URL_TEC"] + "/images/Excel/Listas/TEC_20210331.xlsx").Respond("Application/file", streamXlsx);

				using (MemoryStream ms = new MemoryStream())
				using (var client = mock.ToHttpClient())
				{
					var bFile = TariffTecDownloader.Download(client);

					Assert.IsNotEmpty(bFile);
					Assert.IsNotNull(bFile);

					expectedStreamXlsx?.CopyTo(ms);
					var expectedBytes = ms.ToArray();

					Assert.AreEqual(expectedBytes, bFile);
				}
			}
		}

		public void TestDownloadWithoutMock()
		{
			using (var client = new HttpClient())
			{
				if (!Directory.Exists(ConfigurationProvider.Configuration["OutputFolder"]))
				{
					Directory.CreateDirectory(ConfigurationProvider.Configuration["OutputFolder"]);
				}
				var file = Path.Combine(ConfigurationProvider.Configuration["OutputFolder"], "tec.xlsx");

				var bFile = TariffTecDownloader.Download(client);

				Assert.IsNotEmpty(bFile);
				Assert.IsNotNull(bFile);

				File.WriteAllBytes(file, bFile);
			}
		}
	}
}

