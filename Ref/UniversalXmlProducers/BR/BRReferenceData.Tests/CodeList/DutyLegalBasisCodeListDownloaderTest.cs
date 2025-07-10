using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class DutyLegalBasisCodeListDownloaderTest
	{
		public void TestDownloadCustomsDutyLegalBasisXMLWithoutMock()
		{
			using (var client = new HttpClient())
			{
				var downloader = new DutyLegalBasisCodeListDownloader();
				var bXml = downloader.Download(client);

				Assert.NotNull(bXml, nameof(bXml));

				using (var sXml = new MemoryStream(bXml))
				{
					var xml = XDocument.Load(sXml);
					var itens = xml.Root?.Descendants("FundamentoLegalRegimeTributacaoII");

					Assert.IsTrue(itens.Any());
				}
			}
		}

		[Test]
		public void TestDownloadCustomsDutyLegalBasisXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_FUNDAMENTO_LEGAL_II"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.LRTII.FUNDAMENTO_LEGAL_REGIME_II_LISTAR.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_FUNDAMENTO_LEGAL_II"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.LRTII.FundamentoLegalRegimeTributacaoII.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new DutyLegalBasisCodeListDownloader();
					var bXml = downloader.Download(client);

					Assert.NotNull(bXml, nameof(bXml));
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.LRTII.FundamentoLegalRegimeTributacaoII.xml"))
					using (var outputStream = new MemoryStream(bXml))
					{
						StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
