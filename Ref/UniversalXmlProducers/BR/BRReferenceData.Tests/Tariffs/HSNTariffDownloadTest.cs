using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class HSNTariffDownloadTest
	{
		public void TestDownloadSubitemXMLWithoutMock()
		{
			using (var handler = new HttpClientHandler { UseCookies = false, CheckCertificateRevocationList = true })
			using (var client = new HttpClient(handler))
			{
				var bXml = new TariffNCMSubitemDownloader().Download(client);
				Assert.NotNull(bXml);

				new FileInfo(TestOutputFilePath).Directory.Create();
				File.WriteAllBytes(TestOutputFilePath + TestOutputFile, bXml);

				using (var xmlStream = new MemoryStream(bXml))
				{
					var xml = XDocument.Load(xmlStream);

					Assert.NotNull(xml?.Root?.Descendants("Ncm"));
				}
			}
		}

		[Test]
		public void TestDownloadSubitemXml()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_NCM_LVL6_SUBITEM"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TABELAS_ADUANEIRAS_SOURCES.NCM_LEVEL6_SUBITEM_GET.html"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_NCM_LVL6_SUBITEM"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.itemNcm.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var bXml = new TariffNCMSubitemDownloader().Download(client);
					Assert.NotNull(bXml);

					new FileInfo(TestOutputFilePath).Directory.Create();
					File.WriteAllBytes(TestOutputFilePath + TestOutputFile, bXml);

					using (var xmlStream = new MemoryStream(bXml))
					{
						var xml = XDocument.Load(xmlStream);
						var ncms = xml?.Root?.Descendants("Ncm");
						Assert.NotNull(ncms);
					}
				}
			}
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Tariffs\TestFiles\Output\");
		readonly string TestOutputFile = "Subitem_ncm.xml";
	}
}
