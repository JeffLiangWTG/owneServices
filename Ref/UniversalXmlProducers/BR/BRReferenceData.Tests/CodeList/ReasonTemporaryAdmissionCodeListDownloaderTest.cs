using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class ReasonTemporaryAdmissionCodeListDownloaderTest
	{
		[Test]
		public void TestCompareDownloadedFile()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MOTIVO_ADMISSAO_TEMPORARIA"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MOTIVO_ADMISSAO_TEMPORARIA_GET.html"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MOTIVO_ADMISSAO_TEMPORARIA"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MotivoAdmissaoTemporaria.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new ReasonTemporaryAdmissionCodeListDownloader();
					var bXml = downloader.DownloadXML(client);
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MotivoAdmissaoTemporaria.xml"))
					using (var outputStream = new MemoryStream(bXml))
					{
						var xml = XDocument.Load(outputStream);
						Assert.NotNull(xml, nameof(xml));
						Assert.AreEqual(expectedStream, outputStream);
					}
				}
			}
		}

		[Test]
		public void TestDownloadXml_OnFailure()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MOTIVO_ADMISSAO_TEMPORARIA"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MOTIVO_ADMISSAO_TEMPORARIA_GET.html"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MOTIVO_ADMISSAO_TEMPORARIA"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MOTIVO_ADMISSAO_TEMPORARIA_GET.html"));

				CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new ReasonTemporaryAdmissionCodeListDownloader();
					var bXml = downloader.DownloadXML(client);
					Assert.Null(bXml, nameof(bXml));
				}
			}
		}
	}
}
