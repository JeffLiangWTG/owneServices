using System.Net;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class TariffCharacteristicNCMVersionDownloaderTest
	{
		[Test]
		public void TestDownload_Ncm()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE_VERSION"]).Respond("Application/file", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.historico-versoes.html"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffCharacteristicNCMVersionDownloader(true);
					var version = downloader.GetLastVersion(client);
					Assert.AreEqual("versao37", version);
				}
			}
		}

		[Test]
		public void TestDownload_NcmTe()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE_VERSION_TEST"]).Respond("Application/file", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.historico-versoes.html"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffCharacteristicNCMVersionDownloader(false);
					var version = downloader.GetLastVersion(client);
					Assert.AreEqual("versao37", version);
				}
			}
		}
	}
}
