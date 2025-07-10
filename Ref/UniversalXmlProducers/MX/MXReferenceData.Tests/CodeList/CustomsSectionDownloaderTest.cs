using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	public class CustomsSectionDownloaderTest
	{
		[Test]
		public void DownloadCustomsSectionWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var httpClient = mockHttp.ToHttpClient();

				using (var expectedJsonStream = TestUtils.GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.CodeList.TestFiles.Input.CustomsSectionAppendice1_result.txt"))
				using (var htmlStream = TestUtils.GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.CodeList.TestFiles.Input.Anexo22.html"))
				using (var reader = new StreamReader(expectedJsonStream))
				{
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration.GetSection("CUSTOMS_SECTION_URL").Value).Respond("application/html", htmlStream);

					var downloadList = CustomsSectionDownloader.Download(httpClient);
					var downloadJson = JsonConvert.SerializeObject(downloadList).Trim();

					var expectedJson = reader.ReadToEnd().Trim();
					
					Assert.AreEqual(expectedJson, downloadJson);
				}
			}
		}
	}
}
