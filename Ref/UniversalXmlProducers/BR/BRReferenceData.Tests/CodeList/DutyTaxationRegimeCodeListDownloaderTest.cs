using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class DutyTaxationRegimeCodeListDownloaderTest
	{
		public void DownloadCustomsTaxationRegimeDutyXMLWithoutMock()
		{
			using (var client = new HttpClient())
			{
				var downloader = new DutyTaxationRegimeCodeListDownloader();
				var bXMl = downloader.Download(client);

				Assert.NotNull(bXMl, nameof(bXMl));

				using (var sXml = new MemoryStream(bXMl))
				{
					var xml = XDocument.Load(sXml);
					var items = xml.Root?.Descendants(DutyTaxationRegimeCodeListConstants.TagTaxationRegime);
				}
			}
		}

		[Test]
		public void DownloadCustomsTaxationRegimeDutyXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_REGIME_TRIBUTACAO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.REGIME_TRIBUTACAO_GET.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_REGIME_TRIBUTACAO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.RegimeTributacao.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new DutyTaxationRegimeCodeListDownloader();
					var bXMl = downloader.Download(client);

					Assert.NotNull(bXMl, nameof(bXMl));
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.RegimeTributacao.xml"))
					using (var outputStream = new MemoryStream(bXMl))
					{
						Assert.AreEqual(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
