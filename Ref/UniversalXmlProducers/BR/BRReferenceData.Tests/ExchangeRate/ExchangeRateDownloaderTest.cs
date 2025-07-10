using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRateDownloaderTest
	{
		[Test]
		public void TestDownload()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var oHttpClient = mockHttp.ToHttpClient();

				using (var csvStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Output.cotacaoTodasAsMoedas_10052021.csv"))
				using (var htmlStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Input.consultaBoletim.html"))
				{
					var today = DateTime.Today;
					var url = ConfigurationProvider.Configuration["BRExchangeRateUrl"] + $"&RadOpcao=2&ChkMoeda=61&DATAINI={today:dd}%2F{today:MM}%2F{today:yyyy}&DATAFIM=";

					mockHttp.When(HttpMethod.Get, url).Respond("application/html", htmlStream);
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["BRExchangeRateUrlCsv"] + "&id=61682").Respond("application/csv", csvStream);

					byte[] bXml = ExchangeRateDownloader.Download(oHttpClient, today);
					using (var downloadedFile = new MemoryStream(bXml))
					{
						Assert.AreEqual(csvStream, downloadedFile);
					}
				}
			}
		}
	}
}
