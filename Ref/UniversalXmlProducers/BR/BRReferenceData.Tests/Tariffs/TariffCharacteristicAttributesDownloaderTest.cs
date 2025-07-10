using System.Linq;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class TariffCharacteristicAttributesDownloaderTest
	{
		[Test]
		public void TestDownload()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_ATTRIBUTE"]).Respond("Application/file", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.ATRIBUTOS_2023_10_11.zip"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffCharacteristicAttributesDownloader(true);
					var attributes = downloader.Download(client);
					Assert.AreEqual(4909, attributes.Attributes.Count());
				}
			}
		}
	}
}
