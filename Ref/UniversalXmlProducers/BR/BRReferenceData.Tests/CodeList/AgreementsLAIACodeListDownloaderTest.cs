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
	class AgreementsLAIACodeListDownloaderTest
	{
		[Test]
		public void TestDownloadXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_ACORDO_ALADI"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.ACORDO_ALADI_GET.html"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_ACORDO_ALADI"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.AcordoAladi.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var bXml = AgreementsLAIACodeListDownloader.Download(client);

					Assert.NotNull(bXml, nameof(bXml));
					using (var expectedText = new StreamReader(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.AcordoAladi.xml"), Encoding.UTF8))
					using (var outputText = new StreamReader(new MemoryStream(bXml), Encoding.UTF8))
					{
						Assert.AreEqual(expectedText.ReadToEnd(), outputText.ReadToEnd());
					}
				}
			}
		}

		[Test]
		public void TestGetAgreement()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELA_ACORDOS_TARIFARIOS"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.TabelaAcordosTarifarios.html"));

				using (var client = mockHttp.ToHttpClient())
				{
					var agreementTableString = AgreementsLAIACodeListDownloader.DownloadAttributesHtml(client);

					Assert.NotNull(agreementTableString);
				}
			}
		}
	}
}
