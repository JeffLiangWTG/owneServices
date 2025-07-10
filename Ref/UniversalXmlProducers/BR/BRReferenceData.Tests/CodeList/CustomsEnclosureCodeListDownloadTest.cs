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
	public class CustomsEnclosureCodeListDownloadTest
	{
		public void TestDownloadCustomsEnclosureXMLWithoutMock()
		{
			using (var client = new HttpClient())
			{
				var downloader = new CustomsEnclosureCodeListDownloader();
				var dto = downloader.Download(client);

				Assert.NotNull(dto, nameof(dto));
				Assert.NotNull(dto.data, nameof(dto.data));
				Assert.NotNull(dto.downloadedDate, nameof(dto.downloadedDate));

				using (var sXml = new MemoryStream(dto.data))
				{
					var xml = XDocument.Load(sXml);
					var items = xml.Root?.Descendants("RecintoAduaneiro");
				}
			}
		}

		[Test]
		public void TestDownloadCustomsEnclosureXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_RECINTO_ALFANDEGADO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.RECINTO_ALFANDEGADO_GET.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_RECINTO_ALFANDEGADO"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.RecintoAduaneiro.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new CustomsEnclosureCodeListDownloader();
					var dto = downloader.Download(client);

					Assert.NotNull(dto, nameof(dto));
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.RecintoAduaneiro.xml"))
					using (var outputStream = new MemoryStream(dto.data))
					{
						Assert.AreEqual(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
