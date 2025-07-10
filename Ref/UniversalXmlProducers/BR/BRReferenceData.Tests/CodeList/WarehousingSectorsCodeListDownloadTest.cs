using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class WarehousingSectorsCodeListDownloadTest
	{
		[Test]
		public void DownloadXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_SETOR_LOTACAO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.SETOR_LOTACAO_GET.html"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_SETOR_LOTACAO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.SetorLotacao.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new WarehousingSectorsCodeListDownloader();
					var bXml = downloader.Download(client);

					Assert.NotNull(bXml, nameof(bXml));
					using (var expectedText = new StreamReader(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.SetorLotacao.xml"), Encoding.UTF8))
					using (var outputText = new StreamReader(new MemoryStream(bXml), Encoding.UTF8))
					{
						Assert.AreEqual(expectedText.ReadToEnd(), outputText.ReadToEnd());
					}
				}
			}
		}
	}
}
