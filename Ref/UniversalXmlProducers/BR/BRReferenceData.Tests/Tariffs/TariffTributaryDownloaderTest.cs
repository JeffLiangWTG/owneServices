using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class TariffTributaryDownloaderTest
	{
		[Test]
		public void DownloadCustomsTariffTributaryTest()
		{
			using (var zipStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ttce-importacao-dados-para-servico-duimp-atualizado-20210630.zip"))
			using (var expectedOutputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ttce-importacao-dados-para-servico-duimp-atualizado-20210630.json"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_PORTAL_UNICO_TRATAMENTOS_TRIBUTARIOS_DOWNLOAD"]).Respond("Application/file", zipStream);

					using (var client = mockHttp.ToHttpClient())
					{
						var tariffTributary = TariffTributaryDownloader.DownLoadZipAndExtract(client);

						Assert.IsTrue(tariffTributary != null);

						using (var reader = new StreamReader(expectedOutputStream))
						{
							var expectedJson = JsonConvert.DeserializeObject<TariffTributary>(reader.ReadToEnd());
							Assert.AreEqual(expectedJson.dataGeracao, tariffTributary.dataGeracao);
						}
					}
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			if (Directory.Exists(testOutputFilePath))
			{
				Directory.Delete(testOutputFilePath, true);
			}
		}

		static readonly string testOutputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationProvider.Configuration["OutputFolder"]);
		readonly string testOutputFilePath = Path.Combine(testOutputFolderPath, "Tariff Tributary");
	}
}
