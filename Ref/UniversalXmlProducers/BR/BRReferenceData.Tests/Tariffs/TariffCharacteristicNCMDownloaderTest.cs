using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class TariffCharacteristicNCMDownloaderTest
	{
		[Test]
		public void TestDownload_Ncm()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE"]).Respond("Application/file", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffCharacteristicNCMDownloader(true);
					var ncms = downloader.Download(client, false);
					Assert.AreEqual(20, ncms.Ncms.Count());
					Assert.AreEqual(169, ncms.Ncms.Sum(ncm => ncm.listaAtributos.Count));
				}
			}
		}

		[Test]
		public void TestDownload_NcmTe()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE_TEST"]).Respond("Application/file", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffCharacteristicNCMDownloader(false);
					var ncms = downloader.Download(client, false);
					Assert.AreEqual(20, ncms.Ncms.Count());
					Assert.AreEqual(169, ncms.Ncms.Sum(ncm => ncm.listaAtributos.Count));
				}
			}
		}

		[Test]
		public void TestDownload_Ncm_ExpOnly()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE"]).Respond("Application/file", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffCharacteristicNCMDownloader(true);
					var ncms = downloader.Download(client, true);
					Assert.AreEqual(4, ncms.Ncms.Count());
					Assert.AreEqual(7, ncms.Ncms.Sum(ncm => ncm.listaAtributos.Count));
				}
			}
		}

		[Test]
		public void TestDownload_NcmTe_ExpOnly()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE_TEST"]).Respond("Application/file", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffCharacteristicNCMDownloader(false);
					var ncms = downloader.Download(client, true);
					Assert.AreEqual(4, ncms.Ncms.Count());
					Assert.AreEqual(7, ncms.Ncms.Sum(ncm => ncm.listaAtributos.Count));
				}
			}
		}
	}
}
