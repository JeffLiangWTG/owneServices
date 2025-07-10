using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class NaladiShDownloaderTest
	{
		[Test]
		public void DownloadCustomsNaladiShXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_NALADI_SH"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CustomsNaladi.TestFiles.Input.NALADI_SH_GET.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_NALADI_SH"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CustomsNaladi.TestFiles.Input.NaladiSh.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var bXMl = NaladiShDownloader.Download(client);

					Assert.NotNull(bXMl, nameof(bXMl));
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CustomsNaladi.TestFiles.Input.NaladiSh.xml"))
					using (var outputStream = new MemoryStream(bXMl))
					{
						StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
