using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class ExchangeHedgeMethodofPaymentCodeListDownloaderTest
	{
		[Test]
		public void DownloadCustomsMethodOfPaymentXMLWithMock()
		{
			CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MODALIDADE_PAGAMENTO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MODALIDADE_PAGAMENTO_GET.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MODALIDADE_PAGAMENTO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.ModalidadePagamento.xml"));

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new ExchangeHedgeMethodofPaymentCodeListDownloader();
					var bXMl = downloader.Download(client);

					Assert.NotNull(bXMl, nameof(bXMl));
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.ModalidadePagamento.xml"))
					using (var outputStream = new MemoryStream(bXMl))
					{
						Assert.AreEqual(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
