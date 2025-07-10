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
	class ExportProcedureDownloadTest
	{
		public void TestDownloadWithoutMock()
		{
			using (var client = new HttpClient())
			{
				var downloader = new ExportProcedureDownloader();
				var cpcDTO = downloader.Download(client);

				Assert.NotNull(cpcDTO, nameof(cpcDTO));

				string result = CodePagesEncodingProvider.Instance.GetEncoding(1252).GetString(cpcDTO);

				Assert.IsNotEmpty(result, nameof(result));
			}
		}

		[Test]
		public void DownloadTest()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_ENQUADRAMENTO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.ENQUADRAMENTO_GET.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_ENQUADRAMENTO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.EnquadramentoOperacao.csv"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new ExportProcedureDownloader();
					var cpcDTO = downloader.Download(client);

					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.EnquadramentoOperacao.csv"))
					using (var outputStream = new MemoryStream(cpcDTO))
					{
						Assert.AreEqual(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
