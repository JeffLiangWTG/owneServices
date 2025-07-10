using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class DeclarationTypeTaxRegimeDownloaderTest
	{
		[Test]
		public void DownloadDeclarationTypeTaxRegimeXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_TIPO_DECLARACAO_REGIME_TRIBUTARIO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.DeclarationTypeTaxRegime.TIPO_DECLARACAO_REGIME_TRIBUTARIO_LISTAR.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_TIPO_DECLARACAO_REGIME_TRIBUTARIO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.DeclarationTypeTaxRegime.TipoDeclaracaoRegimeTributario.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new DeclarationTypeTaxRegimeDownloader();
					var bXMl = downloader.Download(client);

					Assert.NotNull(bXMl, nameof(bXMl));
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.DeclarationTypeTaxRegime.TipoDeclaracaoRegimeTributario.xml"))
					using (var outputStream = new MemoryStream(bXMl))
					{
						StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
