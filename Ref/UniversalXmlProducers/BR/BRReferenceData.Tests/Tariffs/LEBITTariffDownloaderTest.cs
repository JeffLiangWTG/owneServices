using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class LEBITTariffDownloaderTest
	{
		[Test]
		public void TestDownloadWithMock()
		{
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.LEBIT.anexo_vi_lebit_bk.xlsx"))
			using (var file = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.LEBIT.anexo_vi_lebit_bk.xlsx"))
			{
				using (var mock = new MockHttpMessageHandler())
				{
					mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_LEBIT_LIST"]).Respond("Application/file", file);

					using (var client = mock.ToHttpClient())
					{
						var bytesFile = LEBITTariffDownloader.Download(client);
						using (var exStream = new MemoryStream())
						{
							expectedStream?.CopyTo(exStream);
							Assert.IsTrue(CompareUtils.AreEquals(bytesFile, exStream.ToArray()));
						}
					}
				}
			}
		}
	}
}
